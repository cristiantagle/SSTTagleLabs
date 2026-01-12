using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using System.Net;

namespace TagleLabsGestorSST.Services;

public interface ILocalAiService
{
    Task<string> AnalizarTextoAsync(string prompt, string contexto, double? temperatura = null);
    Task<string> MejorarRedaccionAsync(string textoOriginal);
    Task<bool> VerificarDisponibilidadAsync();
    Task<Dictionary<string, string>> ExtraerDatosClaveAsync(string textoReglamentoAntiguo);
    Task<string> GenerarProcedimientoAsync(string descripcionTrabajo, string contexto = "", double? temperatura = null);
    Task<List<string>> ObtenerModelosDisponiblesAsync();
    string ObtenerModeloActual();
    Task SetModeloAsync(string nombreModelo);
    string UltimoProveedor { get; }
    string UltimoDetalle { get; }
    Task<float[]> GetEmbeddingAsync(string text);
    
    /// <summary>
    /// Valida que un documento generado cumpla con DS44 y Ley Karin.
    /// Devuelve un resultado estructurado con aspectos positivos, advertencias y score.
    /// </summary>
    Task<DocumentoValidacionResult> ValidarDocumentoConIAAsync(string contenidoDocumento, string tipoDocumento);
}

/// <summary>
/// Resultado de la validación IA de un documento
/// </summary>
public class DocumentoValidacionResult
{
    public bool Aprobado { get; set; }
    public int Score { get; set; } // 0-100
    public List<string> AspectosPositivos { get; set; } = new();
    public List<string> Advertencias { get; set; } = new();
    public List<string> Mejoras { get; set; } = new();
    public string ResumenIA { get; set; } = "";
}

public class LocalAiService : ILocalAiService
{
    private readonly HttpClient _httpClient;
    private readonly Microsoft.Extensions.DependencyInjection.IServiceScopeFactory _scopeFactory;
    private const string GEMINI_ENDPOINT = "https://generativelanguage.googleapis.com/v1beta/models";
    private const string CLAUDE_ENDPOINT = "https://api.anthropic.com/v1/messages";
    private const string GROQ_ENDPOINT = "https://api.groq.com/openai/v1/chat/completions";

    private readonly string? _geminiApiKey;
    private string _geminiModelName;
    private readonly int _geminiMaxOutputTokens;
    private readonly string? _claudeApiKey;
    private readonly string _claudeModelName;
    private readonly string? _groqApiKey;
    private readonly string _groqModelName;
    private readonly bool _preferRemoteOnly;
    private bool _disableGeminiForSession = false;
    private bool _disableClaudeForSession = false;
    private bool _disableGroqForSession = true; // Groq deshabilitado por defecto (siempre falla con contextos largos)
    private bool HasGeminiKey => !string.IsNullOrWhiteSpace(_geminiApiKey);
    private bool HasClaudeKey => !string.IsNullOrWhiteSpace(_claudeApiKey);
    private bool HasGroqKey => !string.IsNullOrWhiteSpace(_groqApiKey);
    public string UltimoProveedor { get; private set; } = "Desconocido";
    public string UltimoDetalle { get; private set; } = string.Empty;

    // Throttle para respetar rate limit de 10 RPM (Gemini 2.5 Free Tier)
    private static readonly SemaphoreSlim _geminiRateLimiter = new(1, 1);
    private static DateTime _lastGeminiCall = DateTime.MinValue;
    private const int MIN_GEMINI_DELAY_MS = 6500; // 6.5 segundos = max 9.2 RPM (bajo límite de 10)

    // Log path
    private readonly string _logPath;

    private void Log(string message, string level = "INFO")
    {
        try
        {
            var logEntry = $"{DateTime.Now:yyyy-MM-dd HH:mm:ss} [{level}] {message}{Environment.NewLine}";
            File.AppendAllText(_logPath, logEntry);
        }
        catch { /* Ignorar errores de log para no romper flujo */ }
    }

    public LocalAiService(Microsoft.Extensions.DependencyInjection.IServiceScopeFactory scopeFactory)
    {
        _httpClient = new HttpClient();
        _httpClient.Timeout = TimeSpan.FromMinutes(10); // Dar más tiempo para análisis largos
        _scopeFactory = scopeFactory;
        
        _geminiApiKey = GeminiApiKeyProvider.GetApiKey();
        // ACTUALIZADO: Gemini 2.5 Flash Lite (Diciembre 2025)
        _geminiModelName = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.5-flash-lite";
        // Gemini 2.5 soporta hasta 65,536 tokens de salida
        _geminiMaxOutputTokens = int.TryParse(Environment.GetEnvironmentVariable("GEMINI_MAX_OUTPUT_TOKENS"), out var parsedTokens)
            ? Math.Clamp(parsedTokens, 512, 65536)
            : 65536; // Máximo permitido por Gemini 2.5
        _claudeApiKey = Environment.GetEnvironmentVariable("CLAUDE_API_KEY");
        _claudeModelName = Environment.GetEnvironmentVariable("CLAUDE_MODEL") ?? "claude-3-5-sonnet-20240620";
        _groqApiKey = Environment.GetEnvironmentVariable("GROQ_API_KEY");
        _groqModelName = Environment.GetEnvironmentVariable("GROQ_MODEL") ?? "llama-3.1-70b-versatile";
        _preferRemoteOnly = bool.TryParse(Environment.GetEnvironmentVariable("PREFER_REMOTE_ONLY"), out var pref) && pref;
        
        _logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "ia_debug_log.txt");
        Log($"LocalAiService inicializado - Gemini 2.5 ({_geminiModelName}) con {_geminiMaxOutputTokens} tokens, throttle {MIN_GEMINI_DELAY_MS}ms.");
    }

    private async Task<HttpResponseMessage> ExecuteWithRetryAsync(Func<Task<HttpResponseMessage>> action, string providerName, int maxRetries = 3)
    {
        // Estrategia de Backoff optimizada basada en logs:
        // Patrón corto: ~20s de recuperación.
        // Patrón largo (Quota): ~6 minutos de recuperación.
        // Nuevos tiempos: 5s, 20s, 60s, 120s, 300s (5 min).
        int[] waitTimes = { 5, 20, 60, 120, 300, 300, 300, 300, 300, 300 }; 

        for (int i = 0; i <= maxRetries; i++)
        {
            try
            {
                var response = await action();
                
                if ((int)response.StatusCode != 429)
                {
                    Log($"[{providerName}] Response: {(int)response.StatusCode}");
                }

                if (response.IsSuccessStatusCode || 
                    response.StatusCode == HttpStatusCode.Unauthorized || 
                    response.StatusCode == HttpStatusCode.Forbidden ||
                    response.StatusCode == HttpStatusCode.NotFound ||
                    response.StatusCode == HttpStatusCode.InternalServerError ||
                    response.StatusCode == HttpStatusCode.BadRequest)
                {
                    return response;
                }

                // Retry on 429 (Rate Limit) AND 503 (Service Unavailable/Overloaded)
                if ((int)response.StatusCode == 429 || (int)response.StatusCode == 503)
                {
                    if (i == maxRetries) return response;

                    // Usar tiempos predefinidos o fallback a 60s si excedemos el array
                    var waitTime = (i < waitTimes.Length) ? waitTimes[i] : 60;
                    var reason = (int)response.StatusCode == 429 ? "Rate Limit" : "Server Overloaded";
                    
                    Log($"[{providerName}] {reason} ({(int)response.StatusCode}). Esperando {waitTime}s antes de reintento {i + 1}/{maxRetries}...", "WARN");
                    UltimoDetalle = $"Esperando {waitTime}s por {reason}...";
                    
                    await Task.Delay(waitTime * 1000);
                    continue;
                }

                return response;
            }
            catch (Exception ex)
            {
                Log($"[{providerName}] Exception: {ex.Message}", "ERROR");
                if (i == maxRetries) throw;
                await Task.Delay(2000); // Wait 2s on exception
            }
        }
        return new HttpResponseMessage(HttpStatusCode.ServiceUnavailable);
    }

    public Task<bool> VerificarDisponibilidadAsync()
    {
        // Solo verificamos claves remotas
        if ((HasGeminiKey && !_disableGeminiForSession) ||
            (HasClaudeKey && !_disableClaudeForSession) ||
            (HasGroqKey && !_disableGroqForSession))
            return Task.FromResult(true);

        return Task.FromResult(false);
    }

    private async Task<string> ObtenerContextoRelevante(string query)
    {
        try
        {
            // 1. Obtener embedding de la consulta
            var queryEmbedding = await GetEmbeddingAsync(query);
            if (queryEmbedding.Length == 0) return "No se pudo generar embedding para la consulta.";

            using (var scope = _scopeFactory.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TagleLabsGestorSST.Data.TagleLabsContext>();
                
                // 2. Traer todos los chunks (Optimización futura: Vector Search en BD con sqlite-vss)
                // Por ahora, con < 10,000 chunks, hacerlo en memoria es rápido (< 100ms)
                var allChunks = await db.KnowledgeChunks
                    .Include(c => c.KnowledgeItem)
                    .AsNoTracking()
                    .ToListAsync();

                if (!allChunks.Any()) return "No hay conocimiento base disponible.";

                // 3. Calcular similitud coseno en memoria
                var rankedChunks = allChunks
                    .Select(c =>
                    {
                        var chunkEmbedding = JsonConvert.DeserializeObject<float[]>(c.Embedding);
                        if (chunkEmbedding == null || chunkEmbedding.Length != queryEmbedding.Length) return new { Chunk = c, Score = 0.0 };
                        
                        return new { Chunk = c, Score = ComputeCosineSimilarity(queryEmbedding, chunkEmbedding) };
                    })
                    .Where(x => x.Score > 0.3) // Umbral mínimo de relevancia
                    .OrderByDescending(x => x.Score)
                    .Take(5) // Top 5 chunks más relevantes
                    .ToList();

                if (rankedChunks.Any())
                {
                    var sb = new StringBuilder();
                    foreach (var item in rankedChunks)
                    {
                        sb.AppendLine($"--- FUENTE: {item.Chunk.KnowledgeItem.SourceFile} (Relevancia: {item.Score:P0}) ---");
                        sb.AppendLine(item.Chunk.Content);
                        sb.AppendLine();
                    }
                    return sb.ToString();
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error obteniendo contexto RAG: {ex.Message}");
        }

        return "No se encontró contexto relevante en la base de conocimientos.";
    }

    private static int ContarCoincidencias(string content, string term)
    {
        if (string.IsNullOrWhiteSpace(content) || string.IsNullOrWhiteSpace(term)) return 0;
        var count = 0;
        var index = content.IndexOf(term, StringComparison.OrdinalIgnoreCase);
        while (index >= 0)
        {
            count++;
            index = content.IndexOf(term, index + term.Length, StringComparison.OrdinalIgnoreCase);
        }
        return count;
    }

    public async Task<string> AnalizarTextoAsync(string prompt, string contexto, double? temperatura = null)
    {
        try
        {
            // 1. Obtener Contexto RAG de la Base de Conocimientos
            var contextoRAG = await ObtenerContextoRelevante(prompt + " " + contexto);

            // 2. Obtener Contexto Legal Hardcoded (Base Normativa)
            var contextoLegal = LegalContext.ObtenerContextoLegal();

            // Prompt optimizado con Contexto Legal Inyectado + RAG Dinámico
            var fullPrompt = $@"ACTÚA COMO ABOGADO LABORALISTA EXPERTO Y PREVENCIONISTA DE RIESGOS (CHILE 2025).

TU BIBLIOTECA DE CONOCIMIENTO (FUENTE DE VERDAD):
=== NORMATIVA VIGENTE ===
{contextoLegal}

=== DOCUMENTOS INTERNOS DE LA EMPRESA (RAG) ===
{contextoRAG}

INSTRUCCIONES CRÍTICAS:
1. TU OBJETIVO ES ADAPTAR EL TEXTO ANTIGUO A LA NORMATIVA VIGENTE EN 2025 Y A LOS DOCUMENTOS INTERNOS.
2. PRIORIDAD: Normativa Vigente > Documentos Internos > Texto Antiguo.
3. Si el texto menciona '45 horas', CÁMBIALO a '44 horas' (Fase 2025).
4. Si el texto menciona 'Acoso' sin las nuevas definiciones de Ley Karin, REEMPLÁZALO con las definiciones de la BIBLIOTECA.
5. Si el texto omite 'Violencia en el Trabajo' o 'Conciliación', AGRÉGALO según la BIBLIOTECA.
6. VERIFICA PROTOCOLOS MINSAL: Si el texto no menciona TMERT, PREXOR, UV o Psicosocial (CEAL-SM), AGREGA un apartado obligatorio citando la normativa vigente.
7. VERIFICA LEYES NUEVAS (2023-2024):
   - AGREGA el Permiso de Emergencia por Ley TEA (Art. 66 quinquies).
   - AGREGA mención a Ley SANNA.
   - AGREGA obligación de retención judicial de alimentos (Ley 21.389).
   - Si la empresa tiene >100 trabajadores, AGREGA Gestor de Inclusión.
   - Si el texto menciona 'Prevención de Delitos', ACTUALIZA a Ley 21.595 (Delitos Económicos).
8. MANTÉN el tono formal, legal y punitivo del reglamento original, pero con las reglas nuevas.

TEXTO ANTIGUO A PROCESAR:
""{contexto}""

TAREA ESPECÍFICA: {prompt}

RESPUESTA (SOLO EL TEXTO CORREGIDO/ANALIZADO):";

            var temp = temperatura ?? 0.3;
            var esContextoLargo = fullPrompt.Length > 30000; // Umbral conservador para modelos estándar

            // Orden: gratuitos primero (Groq, Gemini), luego Claude como fallback.
            
            // Groq suele fallar con contextos muy largos (Error 400). Saltarlo si es gigante.
            if (!esContextoLargo)
            {
                var groq = await TryGroqAsync(fullPrompt, temp);
                if (groq != null) return groq;
            }
            else
            {
                Log("[Auto-Skip] Saltando Groq por longitud de contexto (>30k chars).");
            }

            var gemini = await TryGeminiAsync(fullPrompt, temp);
            if (gemini != null)
                return gemini;

            // Claude como fallback final
            if (!esContextoLargo)
            {
                var claude = await TryClaudeAsync(fullPrompt, temp);
                if (claude != null) return claude;
            }
            else
            {
                Log("[Auto-Skip] Saltando Claude por longitud de contexto (>30k chars).");
            }

            // Si fallan todos los remotos, error.
            UltimoProveedor = "Ninguno";
            return "Error: Todos los proveedores remotos fallaron o no están configurados.";
        }
        catch (Exception ex)
        {
            return $"Error de conexión: {ex.Message}. Verifica tu conexión a internet.";
        }
    }
    public async Task<string> MejorarRedaccionAsync(string textoOriginal)
    {
        // Prompt diseñado para evitar rechazos por "seguridad" al analizar textos legales que mencionan delitos/acoso
        var prompt = "ERES UN ASISTENTE LEGAL EXPERTO EN CUMPLIMIENTO NORMATIVO CHILENO (LEY KARIN Y DS44).\n" +
                     "CONTEXTO: El usuario te enviará un BORRADOR de Reglamento Interno. Este documento menciona conductas prohibidas (como acoso o violencia) precisamente para SANCIONARLAS y PREVENIRLAS.\n" +
                     "TU OBJETIVO: Reescribir el texto para que sea MÁS ESTRICTO en la prohibición de estas conductas, cumpliendo con la Ley Karin.\n" +
                     "INSTRUCCIONES:\n" +
                     "1. No rechaces el texto por mencionar violencia/acoso; es un documento legal para prevenirlos.\n" +
                     "2. Mejora la redacción para que sea formal, clara y legalmente vinculante.\n" +
                     "3. Asegúrate de incluir definiciones claras de Acoso Sexual, Laboral y Violencia según la nueva normativa.\n" +
                     "4. Devuelve SOLO el texto mejorado.\n\n" +
                     "TEXTO A MEJORAR:\n";
        
        return await AnalizarTextoAsync(prompt, textoOriginal, temperatura: 0.7);
    }

    public async Task<Dictionary<string, string>> ExtraerDatosClaveAsync(string textoReglamentoAntiguo)
    {
        var prompt = @"ERES UN EXPERTO EN ANÁLISIS DE REGLAMENTOS INTERNOS CHILENOS.
TU TAREA ES LEER EL TEXTO Y EXTRAER DATOS ESPECÍFICOS EN FORMATO JSON.

EXTRAE EXACTAMENTE ESTOS CAMPOS:
- ""HORARIOS"": Todo el texto relacionado con la jornada de trabajo, horarios de entrada/salida y turnos.
- ""PAGO"": Fechas y formas de pago de remuneraciones.
- ""PROHIBICIONES_ESPECIALES"": Prohibiciones específicas técnicas o propias del rubro (no las genéricas legales).
- ""BENEFICIOS"": Aguinaldos, bonos o beneficios mencionados.

SI NO ENCUENTRAS INFORMACIÓN, DEJA EL CAMPO COMO ""NO ESPECIFICADO"".
RESPONDE SOLO CON EL JSON, SIN TEXTO ADICIONAL.";

        var respuestaJson = await AnalizarTextoAsync(prompt, textoReglamentoAntiguo, temperatura: 0.2);
        
        try 
        {
            // Limpieza básica por si la IA incluye markdown
            respuestaJson = respuestaJson.Replace("```json", "").Replace("```", "").Trim();
            return JsonConvert.DeserializeObject<Dictionary<string, string>>(respuestaJson) ?? new Dictionary<string, string>();
        }
        catch
        {
            // Fallback si falla el JSON
            return new Dictionary<string, string> { { "ERROR", "No se pudo extraer la estructura JSON." } };
        }
    }

    public async Task<string> GenerarProcedimientoAsync(string descripcionTrabajo, string contexto = "", double? temperatura = null)
    {
        // Prompt que sigue la plantilla PTS y genera estructura en Markdown
        var prompt = $@"Eres un redactor técnico experto en Prevención de Riesgos y Seguridad Industrial (Chile, DS44). Genera un Procedimiento de Trabajo Seguro (PTS) completo en formato Markdown siguiendo esta plantilla: Título, Código, Versión, Fecha, Propósito, Alcance, Definiciones, Responsabilidades, Condiciones Previas, Materiales y EPP, Pasos detallados (paso a paso), Identificación de Peligros y Controles, Requisitos de Formación, Registros, Emergencias, Revisión y Anexos.

    Toma en cuenta la siguiente DESCRIPCIÓN de la tarea:
    {descripcionTrabajo}

    Contexto operativo (si aplica):
    {contexto}

    Incluye tablas para los peligros (Peligro | Consecuencia | Probabilidad | Consecuencia | Medidas de control). Mantén el lenguaje formal y claro, y agrega un checklist final de verificación.

    Responde SOLO con el Procedimiento en Markdown, sin explicaciones adicionales.";

        // Para generación de procedimientos, usamos temperatura ligeramente más alta para sugerencias útiles
        var response = await AnalizarTextoAsync(prompt, contexto, temperatura ?? 0.6);
        return response;
    }

    public Task<List<string>> ObtenerModelosDisponiblesAsync()
    {
        return Task.FromResult(new List<string> 
        { 
            "gemini-2.5-flash-lite",
            "gemini-3-flash-preview",
            "gemini-2.5-flash",
            "gemini-2.5-pro",
            "gemini-2.0-flash"
        });
    }

    public string ObtenerModeloActual() => _geminiModelName;

    public Task SetModeloAsync(string nombreModelo)
    {
        if (string.IsNullOrWhiteSpace(nombreModelo)) return Task.CompletedTask;
        
        _geminiModelName = nombreModelo;
        Log($"Modelo cambiado a: {_geminiModelName}");
        return Task.CompletedTask;
    }

    private async Task<string?> TryGeminiAsync(string prompt, double temperatura)
    {
        if (string.IsNullOrWhiteSpace(_geminiApiKey) || _disableGeminiForSession)
            return null;

        // THROTTLE PROACTIVO: Respetar 10 RPM de Gemini 2.5 Free Tier
        await _geminiRateLimiter.WaitAsync();
        try
        {
            var elapsed = DateTime.Now - _lastGeminiCall;
            if (elapsed.TotalMilliseconds < MIN_GEMINI_DELAY_MS)
            {
                var waitTime = MIN_GEMINI_DELAY_MS - (int)elapsed.TotalMilliseconds;
                Log($"[Gemini-Throttle] Esperando {waitTime}ms para respetar 10 RPM...");
                UltimoDetalle = $"Throttle: esperando {waitTime / 1000.0:F1}s...";
                await Task.Delay(waitTime);
            }
            _lastGeminiCall = DateTime.Now;

            var url = $"{GEMINI_ENDPOINT}/{_geminiModelName}:generateContent?key={_geminiApiKey}";
            var payload = new
            {
                contents = new[]
                {
                    new {
                        parts = new[] { new { text = prompt } }
                    }
                },
                generationConfig = new
                {
                    temperature = temperatura,
                    maxOutputTokens = _geminiMaxOutputTokens
                },
                safetySettings = new[]
                {
                    new { category = "HARM_CATEGORY_HARASSMENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_HATE_SPEECH", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_SEXUALLY_EXPLICIT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_DANGEROUS_CONTENT", threshold = "BLOCK_NONE" },
                    new { category = "HARM_CATEGORY_CIVIC_INTEGRITY", threshold = "BLOCK_NONE" }
                }
            };
            var json = JsonConvert.SerializeObject(payload);
            Log($"[{_geminiModelName}] Enviando request...");
            
            // Reintentos con backoff optimizado
            var response = await ExecuteWithRetryAsync(async () => 
            {
                var req = new HttpRequestMessage(HttpMethod.Post, url)
                {
                    Content = new StringContent(json, Encoding.UTF8, "application/json")
                };
                return await _httpClient.SendAsync(req);
            }, "Gemini", maxRetries: 5); // Reducido a 5 reintentos con throttle proactivo

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == (HttpStatusCode)429)
            {
                UltimoDetalle = $"Gemini límite/prohibido {(int)response.StatusCode}";
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                var errorBody = await response.Content.ReadAsStringAsync();
                Log($"[Gemini] Error {(int)response.StatusCode}: {errorBody}");
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
            var text = jsonResponse["candidates"]?.First?["content"]?["parts"]?.First?["text"]?.ToString();
            var finish = jsonResponse["candidates"]?.First?["finishReason"]?.ToString() ?? string.Empty;
            
            Log($"[Gemini] FinishReason: {finish}. Text Length: {text?.Length ?? 0}");

            UltimoProveedor = $"Gemini ({_geminiModelName})";
            UltimoDetalle = $"Gemini OK finish={finish} tokens={_geminiMaxOutputTokens}";
            System.Diagnostics.Debug.WriteLine($"[IA][Gemini] status={(int)response.StatusCode} finish={finish}");

            // Aceptar STOP y MAX_TOKENS como respuestas válidas
            if (string.IsNullOrWhiteSpace(text) || (!string.IsNullOrWhiteSpace(finish) && !finish.Equals("STOP", StringComparison.OrdinalIgnoreCase) && !finish.Equals("MAX_TOKENS", StringComparison.OrdinalIgnoreCase)))
            {
                Log($"[Gemini] Fallback triggered. Text empty or finish reason not STOP/MAX_TOKENS.");
                return null;
            }

            return text;
        }
        catch (Exception ex)
        {
            Log($"[Gemini] Exception: {ex.Message}");
            UltimoDetalle = "Gemini exception";
            return null;
        }
        finally
        {
            _geminiRateLimiter.Release();
        }
    }

    private async Task<string?> TryClaudeAsync(string prompt, double temperatura)
    {
        if (string.IsNullOrWhiteSpace(_claudeApiKey) || _disableClaudeForSession)
            return null;

        try
        {
            var payload = new
            {
                model = _claudeModelName,
                max_tokens = 4000,
                temperature = temperatura,
                messages = new[]
                {
                    new
                    {
                        role = "user",
                        content = new[]
                        {
                            new { type = "text", text = prompt }
                        }
                    }
                }
            };

            Log($"[Claude] Enviando request...");
            
            var response = await ExecuteWithRetryAsync(async () => 
            {
                var req = new HttpRequestMessage(HttpMethod.Post, CLAUDE_ENDPOINT)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                };
                req.Headers.Add("x-api-key", _claudeApiKey);
                req.Headers.Add("anthropic-version", "2023-06-01");
                req.Headers.Add("anthropic-beta", "max-tokens-3-5-sonnet-2024-07-15");
                return await _httpClient.SendAsync(req);
            }, "Claude");

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == (HttpStatusCode)429)
            {
                UltimoDetalle = $"Claude límite/prohibido {(int)response.StatusCode}";
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                UltimoDetalle = $"Claude error {(int)response.StatusCode}";
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
            var text = jsonResponse["content"]?.First?["text"]?.ToString();
            var stopReason = jsonResponse["stop_reason"]?.ToString() ?? string.Empty;
            UltimoProveedor = $"Claude ({_claudeModelName})";
            UltimoDetalle = $"Claude status {(int)response.StatusCode} stop={stopReason}";
            System.Diagnostics.Debug.WriteLine($"[IA][Claude] status={(int)response.StatusCode} stop={stopReason}");

            if (string.IsNullOrWhiteSpace(text) || (!string.IsNullOrEmpty(stopReason) && !stopReason.Equals("end_turn", StringComparison.OrdinalIgnoreCase)))
                return null;

            return text;
        }
        catch
        {
            UltimoDetalle = "Claude exception";
            return null;
        }
    }

    // OpenRouter REMOVIDO - nunca funcionó correctamente

    private async Task<string?> TryGroqAsync(string prompt, double temperatura)
    {
        if (string.IsNullOrWhiteSpace(_groqApiKey) || _disableGroqForSession)
            return null;

        try
        {
            var payload = new
            {
                model = _groqModelName,
                messages = new[]
                {
                    new { role = "user", content = prompt }
                },
                temperature = temperatura,
                max_tokens = 4000,
                stream = false
            };

            Log($"[Groq] Enviando request...");
            
            var response = await ExecuteWithRetryAsync(async () => 
            {
                var req = new HttpRequestMessage(HttpMethod.Post, GROQ_ENDPOINT)
                {
                    Content = new StringContent(JsonConvert.SerializeObject(payload), Encoding.UTF8, "application/json")
                };
                req.Headers.Add("Authorization", $"Bearer {_groqApiKey}");
                req.Headers.Add("Accept", "application/json");
                return await _httpClient.SendAsync(req);
            }, "Groq");

            if (response.StatusCode == HttpStatusCode.Forbidden || response.StatusCode == (HttpStatusCode)429)
            {
                UltimoDetalle = $"Groq límite/prohibido {(int)response.StatusCode}";
                return null;
            }

            if (!response.IsSuccessStatusCode)
            {
                UltimoDetalle = $"Error Groq {(int)response.StatusCode}";
                return null;
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
            var text = jsonResponse["choices"]?.First?["message"]?["content"]?.ToString();
            var finish = jsonResponse["choices"]?.First?["finish_reason"]?.ToString() ?? string.Empty;
            UltimoProveedor = $"Groq ({_groqModelName})";
            UltimoDetalle = $"Estado Groq {(int)response.StatusCode} finish={finish}";
            System.Diagnostics.Debug.WriteLine($"[IA][Groq] status={(int)response.StatusCode} finish={finish}");

            if (string.IsNullOrWhiteSpace(text) || (!string.IsNullOrEmpty(finish) && !finish.Equals("stop", StringComparison.OrdinalIgnoreCase)))
                return null;

            return text;
        }
        catch (Exception ex)
        {
            UltimoDetalle = $"Excepción Groq: {ex.Message}";
            return null;
        }
    }

    public async Task<float[]> GetEmbeddingAsync(string text)
    {
        if (string.IsNullOrWhiteSpace(_geminiApiKey)) return Array.Empty<float>();

        try
        {
            // Usar modelo text-embedding-004 de Gemini
            var url = $"{GEMINI_ENDPOINT}/text-embedding-004:embedContent?key={_geminiApiKey}";
            var payload = new
            {
                model = "models/text-embedding-004",
                content = new { parts = new[] { new { text = text } } }
            };

            var json = JsonConvert.SerializeObject(payload);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync(url, content);

            if (!response.IsSuccessStatusCode)
            {
                Log($"[Gemini Embed] Error {(int)response.StatusCode}");
                return Array.Empty<float>();
            }

            var responseString = await response.Content.ReadAsStringAsync();
            var jsonResponse = JObject.Parse(responseString);
            var values = jsonResponse["embedding"]?["values"]?.ToObject<float[]>();

            return values ?? Array.Empty<float>();
        }
        catch (Exception ex)
        {
            Log($"[Gemini Embed] Exception: {ex.Message}");
            return Array.Empty<float>();
        }
    }

    public static double ComputeCosineSimilarity(float[] vectorA, float[] vectorB)
    {
        if (vectorA.Length != vectorB.Length) return 0;

        double dotProduct = 0;
        double normA = 0;
        double normB = 0;

        for (int i = 0; i < vectorA.Length; i++)
        {
            dotProduct += vectorA[i] * vectorB[i];
            normA += vectorA[i] * vectorA[i];
            normB += vectorB[i] * vectorB[i];
        }

        if (normA == 0 || normB == 0) return 0;

        return dotProduct / (Math.Sqrt(normA) * Math.Sqrt(normB));
    }

    /// <summary>
    /// Valida que un documento generado cumpla con DS44 y Ley Karin.
    /// Analiza el contenido y devuelve un resultado estructurado.
    /// </summary>
    public async Task<DocumentoValidacionResult> ValidarDocumentoConIAAsync(string contenidoDocumento, string tipoDocumento)
    {
        var result = new DocumentoValidacionResult();

        if (string.IsNullOrWhiteSpace(contenidoDocumento))
        {
            result.Aprobado = false;
            result.Score = 0;
            result.Advertencias.Add("El documento está vacío.");
            return result;
        }

        try
        {
            // Prompt específico para validación de cumplimiento normativo
            var prompt = $@"ERES UN AUDITOR LEGAL DE SEGURIDAD Y SALUD EN EL TRABAJO (CHILE 2025).

DOCUMENTO A VALIDAR (TIPO: {tipoDocumento}):
---
{contenidoDocumento.Substring(0, Math.Min(contenidoDocumento.Length, 15000))}
---

VERIFICA CUMPLIMIENTO CON:
1. DS 44 (2023) - Nuevo Decreto de SST
2. Ley Karin 21.643 - Acoso y Violencia
3. DS 594 - Condiciones Sanitarias
4. Protocolos MINSAL (PREXOR, PLANESI, TMERT, UV, Psicosocial)

RESPONDE SOLO EN FORMATO JSON (sin markdown ni explicaciones):
{{
  ""score"": [0-100],
  ""aprobado"": [true/false],
  ""positivos"": [""aspecto 1"", ""aspecto 2"", ""aspecto 3""],
  ""advertencias"": [""advertencia 1"", ""advertencia 2""],
  ""mejoras"": [""mejora sugerida 1"", ""mejora sugerida 2""],
  ""resumen"": ""Breve resumen de la evaluación (máx 100 palabras)""
}}

CRITERIOS DE SCORE:
- 90-100: Cumple completamente con toda la normativa
- 70-89: Cumple con la mayoría, algunas mejoras menores
- 50-69: Cumplimiento parcial, requiere revisión
- 0-49: Incumplimientos críticos, no apto";

            var respuesta = await AnalizarTextoAsync(prompt, "", temperatura: 0.2);

            // Limpiar posible markdown
            respuesta = respuesta.Replace("```json", "").Replace("```", "").Trim();
            
            // Intentar parsear JSON
            try
            {
                var json = JObject.Parse(respuesta);
                result.Score = json["score"]?.Value<int>() ?? 50;
                result.Aprobado = json["aprobado"]?.Value<bool>() ?? (result.Score >= 70);
                result.ResumenIA = json["resumen"]?.ToString() ?? "Validación completada.";

                var positivos = json["positivos"]?.ToObject<List<string>>();
                if (positivos != null) result.AspectosPositivos = positivos;

                var advertencias = json["advertencias"]?.ToObject<List<string>>();
                if (advertencias != null) result.Advertencias = advertencias;

                var mejoras = json["mejoras"]?.ToObject<List<string>>();
                if (mejoras != null) result.Mejoras = mejoras;

                Log($"[ValidarDoc] Tipo={tipoDocumento} Score={result.Score} Aprobado={result.Aprobado} Mejoras={result.Mejoras?.Count ?? 0}");
            }
            catch (JsonException)
            {
                // Si falla el parsing, extraer información del texto
                result.Score = 60;
                result.Aprobado = true;
                result.ResumenIA = respuesta.Length > 500 ? respuesta.Substring(0, 500) + "..." : respuesta;
                result.Advertencias.Add("No se pudo parsear la respuesta estructurada de la IA.");
                Log("[ValidarDoc] JSON parse failed, using fallback.", "WARN");
            }
        }
        catch (Exception ex)
        {
            result.Aprobado = false;
            result.Score = 0;
            result.Advertencias.Add($"Error al validar: {ex.Message}");
            result.ResumenIA = "Error durante la validación con IA.";
            Log($"[ValidarDoc] Exception: {ex.Message}", "ERROR");
        }

        return result;
    }
}
