using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.IO;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using TagleLabsGestorSST.Data.Entities;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using UglyToad.PdfPig;

using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class ActualizacionDocumentosViewModel : ObservableObject
{
    private readonly ILocalAiService _aiService;
    private readonly INotificationService _notificationService;
    private readonly DocumentoService _documentoService;
    private readonly IEmpresaService _empresaService;
    private readonly bool _tieneGemini;
    private readonly string _modeloGemini;
    private CancellationTokenSource? _cts;

    [ObservableProperty]
    private string _textoDocumento = string.Empty;

    [ObservableProperty]
    private string _resultadoAnalisis = string.Empty;

    [ObservableProperty]
    private bool _isAnalizando;

    [ObservableProperty]
    private string _estadoAnalisis = "Listo para analizar.";

    [ObservableProperty]
    private bool _servicioDisponible;

    [ObservableProperty]
    private string _estadoServicio = "Verificando servicio de IA...";

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();

    [ObservableProperty]
    private Empresa? _selectedEmpresa;

    [ObservableProperty]
    private ObservableCollection<string> _progreso = new();

    [ObservableProperty]
    private int _totalBloques;

    [ObservableProperty]
    private int _bloquesProcesados;

    [ObservableProperty]
    private string _etaTexto = string.Empty;

    [ObservableProperty]
    private string _trazaIa = string.Empty;
    
    [ObservableProperty]
    private ObservableCollection<string> _modelosDisponibles = new();

    [ObservableProperty]
    private string _selectedModelo = string.Empty;

    [ObservableProperty]
    private bool _puedeCancel;

    public ActualizacionDocumentosViewModel(
        ILocalAiService aiService, 
        INotificationService notificationService,
        DocumentoService documentoService,
        IEmpresaService empresaService)
    {
        _aiService = aiService;
        _notificationService = notificationService;
        _documentoService = documentoService;
        _empresaService = empresaService;
        _tieneGemini = !string.IsNullOrWhiteSpace(GeminiApiKeyProvider.GetApiKey());
        _modeloGemini = Environment.GetEnvironmentVariable("GEMINI_MODEL") ?? "gemini-2.5-flash-lite";
        
        // Texto de ejemplo para demo
        TextoDocumento = "REGLAMENTO INTERNO DE ORDEN, HIGIENE Y SEGURIDAD\n\nARTÍCULO 1: El presente reglamento tiene por objeto establecer las normas generales...";
        
        Task.Run(VerificarServicio);
        Task.Run(CargarEmpresas);
        Task.Run(CargarModelos);
        RegistrarPaso("Listo para iniciar análisis IA.");
    }

    private async Task CargarModelos()
    {
        try
        {
            var modelos = await _aiService.ObtenerModelosDisponiblesAsync();
            var actual = _aiService.ObtenerModeloActual();
            
            App.Current.Dispatcher.Invoke(() =>
            {
                ModelosDisponibles = new ObservableCollection<string>(modelos);
                SelectedModelo = actual;
            });
        }
        catch { }
    }

    partial void OnSelectedModeloChanged(string value)
    {
        if (!string.IsNullOrEmpty(value))
        {
            _aiService.SetModeloAsync(value);
            RegistrarPaso($"Modelo de IA cambiado a: {value}");
        }
    }

    private async Task CargarEmpresas()
    {
        try
        {
            var empresas = await _empresaService.ObtenerEmpresasAsync();
            App.Current.Dispatcher.Invoke(() =>
            {
                Empresas = new ObservableCollection<Empresa>(empresas);
            });
        }
        catch { }
    }

    private async Task VerificarServicio()
    {
        // Solo verificamos claves remotas
        var disponible = await _aiService.VerificarDisponibilidadAsync();

        if (disponible)
        {
            ServicioDisponible = true;
            EstadoServicio = $"IA Externa Conectada ({_modeloGemini})";
        }
        else
        {
            ServicioDisponible = false;
            EstadoServicio = "IA no detectada (Verifique clave Gemini/Claude)";
        }
        RegistrarPaso(EstadoServicio);
    }

    [RelayCommand]
    private async Task AnalizarDocumento()
    {
        if (string.IsNullOrWhiteSpace(TextoDocumento))
        {
            _notificationService.ShowWarning("Por favor ingrese o cargue el texto del documento.");
            return;
        }

        IsAnalizando = true;
        EstadoAnalisis = "Analizando documento con IA Local...";
        ResultadoAnalisis = EstadoAnalisis;
        RegistrarPaso("Inicia análisis de cumplimiento y brechas.");

        try
        {
            var prompt = "Analiza este fragmento de reglamento y dime si cumple con las nuevas disposiciones de la Ley Karin y el DS44. Identifica brechas y sugiere mejoras de redacción.";
            var respuesta = await _aiService.AnalizarTextoAsync(prompt, TextoDocumento);
            ResultadoAnalisis = respuesta;
            RegistrarPaso("Análisis completado.");
            RegistrarPaso($"Motor IA utilizado: {_aiService.UltimoProveedor}");
        }
        catch (Exception ex)
        {
            ResultadoAnalisis = $"Error crítico: {ex.Message}";
            RegistrarPaso($"Error: {ex.Message}");
        }
        finally
        {
            IsAnalizando = false;
            EstadoAnalisis = "Listo";
        }
    }
    
    [RelayCommand]
    private async Task MejorarRedaccion()
    {
        if (string.IsNullOrWhiteSpace(TextoDocumento))
        {
            _notificationService.ShowWarning("Por favor ingrese o cargue el texto del documento.");
            return;
        }

        IsAnalizando = true;
        EstadoAnalisis = "Generando propuesta de redacción mejorada...";
        ResultadoAnalisis = EstadoAnalisis;
        RegistrarPaso("Inicia mejora de redacción.");

        try
        {
            var respuesta = await _aiService.MejorarRedaccionAsync(TextoDocumento);
            ResultadoAnalisis = respuesta;
            RegistrarPaso("Redacción mejorada lista.");
            RegistrarPaso($"Motor IA utilizado: {_aiService.UltimoProveedor}");
        }
        catch (Exception ex)
        {
            ResultadoAnalisis = $"Error crítico: {ex.Message}";
            RegistrarPaso($"Error: {ex.Message}");
        }
        finally
        {
            IsAnalizando = false;
            EstadoAnalisis = "Listo";
        }
    }

    [RelayCommand]
    private async Task GenerarProcedimiento()
    {
        if (string.IsNullOrWhiteSpace(TextoDocumento))
        {
            _notificationService.ShowWarning("Por favor ingrese una breve descripción de la tarea para generar el procedimiento.");
            return;
        }

        IsAnalizando = true;
        EstadoAnalisis = "Generando Procedimiento de Trabajo (IA)...";
        ResultadoAnalisis = EstadoAnalisis;
        RegistrarPaso("Inicia generación de procedimiento (IA).");

        try
        {
            // Enviar la descripción y (opcionalmente) contexto de la empresa
            var context = SelectedEmpresa != null ? $"Empresa: {SelectedEmpresa.RazonSocial}" : string.Empty;
            var respuesta = await _aiService.GenerarProcedimientoAsync(TextoDocumento, context);
            ResultadoAnalisis = respuesta;
            RegistrarPaso("Procedimiento (IA) generado.");
            RegistrarPaso($"Motor IA utilizado: {_aiService.UltimoProveedor}");
        }
        catch (Exception ex)
        {
            ResultadoAnalisis = $"Error generando procedimiento: {ex.Message}";
            RegistrarPaso($"Error: {ex.Message}");
        }
        finally
        {
            IsAnalizando = false;
            EstadoAnalisis = "Listo";
        }
    }

    [RelayCommand]
    private async Task GuardarWord()
    {
        await Task.CompletedTask; // Silence CS1998
        if (string.IsNullOrWhiteSpace(ResultadoAnalisis))
        {
            _notificationService.ShowWarning("No hay contenido para guardar. Analice o mejore un documento primero.");
            return;
        }

        var saveFileDialog = new Microsoft.Win32.SaveFileDialog
        {
            Filter = "Documento de Word|*.docx",
            Title = "Guardar Documento Mejorado",
            FileName = "Reglamento_Mejorado_IA.docx"
        };

        if (saveFileDialog.ShowDialog() == true)
        {
            try
            {
                CrearDocumentoWord(saveFileDialog.FileName, ResultadoAnalisis);
                _notificationService.ShowSuccess($"Documento guardado en: {saveFileDialog.FileName}");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error al guardar: {ex.Message}");
            }
        }
    }





    [RelayCommand]
    private void CancelarOperacion()
    {
        if (_cts != null && !_cts.IsCancellationRequested)
        {
            _cts.Cancel();
            RegistrarPaso("⛔ Cancelación solicitada por el usuario.");
            EstadoAnalisis = "Cancelando operación...";
            _notificationService.ShowWarning("Cancelando operación de IA...");
        }
    }

    [RelayCommand]
    private async Task MigrarReglamento()
    {
        if (string.IsNullOrWhiteSpace(TextoDocumento))
        {
            _notificationService.ShowWarning("Por favor cargue el Reglamento Antiguo (Source) primero.");
            return;
        }

        if (SelectedEmpresa == null)
        {
            _notificationService.ShowWarning("Por favor seleccione la Empresa de destino.");
            return;
        }

        // Crear nuevo token de cancelación
        _cts?.Dispose();
        _cts = new CancellationTokenSource();
        PuedeCancel = true;

        IsAnalizando = true;
        EstadoAnalisis = "Migración inteligente en curso...";
        ResultadoAnalisis = "Iniciando Migración Inteligente...\n1. Extrayendo datos del reglamento antiguo con IA...";
        RegistrarPaso("Migración: extracción de datos clave.");

        try
        {
            _cts.Token.ThrowIfCancellationRequested();
            // 1. Extracción de Datos con IA
            var datosExtraidos = await _aiService.ExtraerDatosClaveAsync(TextoDocumento);
            RegistrarPaso($"Motor IA utilizado (extracción): {_aiService.UltimoProveedor}");
            
            ResultadoAnalisis += "\nDatos extraídos:\n";
            if (datosExtraidos.Count == 0 || datosExtraidos.ContainsKey("ERROR"))
            {
                ResultadoAnalisis += "- ERROR: No se pudo extraer la estructura JSON. Se usarán valores por defecto.\n";
                datosExtraidos.Clear();
            }
            else
            {
                foreach(var kvp in datosExtraidos) ResultadoAnalisis += $"- {kvp.Key}: {kvp.Value.Substring(0, Math.Min(50, kvp.Value.Length))}...\n";
            }

            // 2. Reescritura completa del Reglamento
            RegistrarPaso("Migración: reescritura completa con IA y base normativa.");
            var promptReescritura = @$"ERES ABOGADO LABORALISTA + EXPERTO SST CHILE 2025.
TAREA: Reescribe COMPLETO el Reglamento Interno proporcionado, adaptándolo a Ley Karin (21.643), Ley 40 horas, DS44 y protocolos MINSAL.
EMPRESA: {SelectedEmpresa.RazonSocial} - RUT {SelectedEmpresa.Rut}.

ESTRUCTURA OBLIGATORIA (usa Markdown con # para títulos):

# PREÁMBULO
(Incluir referencia al DS44, objetivos del reglamento, ámbito de aplicación)

# CAPÍTULO I: NORMAS DE ORDEN
## TÍTULO I: DISPOSICIONES GENERALES
### Artículo 1°.- Definiciones...
## TÍTULO II: CONDICIONES DE INGRESO
## TÍTULO III: DEL CONTRATO INDIVIDUAL DE TRABAJO
... (continuar hasta TÍTULO XXVI)

# CAPÍTULO II: NORMAS DE HIGIENE Y SEGURIDAD
## TÍTULO I: DE LA POLÍTICA DE SEGURIDAD
## TÍTULO II: DEL COMITÉ PARITARIO DE HIGIENE Y SEGURIDAD
... (continuar hasta TÍTULO XIX)

# ANEXOS
(Protocolos MINSAL, matrices de riesgo si aplica)

REQUISITOS CRÍTICOS:
- USA MARKDOWN: '# ' para CAPÍTULO, '## ' para TÍTULO, '### ' para Artículos
- Mantén la extensión completa (mínimo 95% del original)
- Incorpora: Ley Karin (acoso sexual/laboral/violencia), DS44, TMERT, PREXOR, UV, CEAL-SM
- Incorpora: 44 horas semanales, Ley TEA (Art. 66 quinquies), SANNA, retención judicial alimentos
- Nombre empresa exacto: {SelectedEmpresa.RazonSocial}
- NO mezcles índices antiguos; genera estructura limpia desde cero

DEVUELVE: Reglamento completo en Markdown listo para conversión.";


            var reglamentoActualizado = await _aiService.AnalizarTextoAsync(promptReescritura, TextoDocumento, temperatura: 0.35);
            RegistrarPaso($"Motor IA utilizado (reescritura): {_aiService.UltimoProveedor}");

            // Si la IA devuelve poco texto, reintentar en bloques para evitar recortes
            if (reglamentoActualizado.Length < TextoDocumento.Length * 0.8)
            {
                RegistrarPaso("Respuesta corta detectada. Reintentando por bloques para preservar extensión.");
                reglamentoActualizado = await ReescribirEnBloques(TextoDocumento, SelectedEmpresa.RazonSocial, SelectedEmpresa.Rut);
            }

            ResultadoAnalisis += "\n2. Generando nuevo documento basado en Plantilla Maestra 2025...";
            RegistrarPaso("Migración: datos extraídos y validados para plantilla.");
            ResultadoAnalisis += "\nSección actualizada generada por IA lista para inyectar.";
            RegistrarPaso("Migración: texto completo generado.");

            // 3. Preparar Diccionario de Reemplazo (Mezcla Perfecta)
            var datosReemplazo = new Dictionary<string, string>
            {
                // Reemplazos de Identidad (Buscamos el texto literal de la plantilla antigua "CONST.CAMO")
                // Agregamos múltiples variaciones por si el formato en Word es distinto
                { "CONST.CAMO", SelectedEmpresa.RazonSocial },
                { "CONST. CAMO", SelectedEmpresa.RazonSocial }, 
                { "CONSTRUCTORA CAMO", SelectedEmpresa.RazonSocial },
                { "CAMO", SelectedEmpresa.RazonSocial }, // Agresivo, pero necesario si fallan los anteriores
                { "76.825.693-4", SelectedEmpresa.Rut }, 
                { "76825693-4", SelectedEmpresa.Rut },
                
                // Reemplazos de Variables Estándar
                { "RAZON_SOCIAL", SelectedEmpresa.RazonSocial },
                { "RUT_EMPRESA", SelectedEmpresa.Rut },
                { "FECHA_ACTUAL", DateTime.Now.ToString("dd 'de' MMMM 'de' yyyy") },

                // Inyección de Datos Extraídos por IA
                { "HORARIOS", datosExtraidos.GetValueOrDefault("HORARIOS", "Según contrato individual.") },
                { "PAGO", datosExtraidos.GetValueOrDefault("PAGO", "Según estipulación legal.") },
                { "BENEFICIOS", datosExtraidos.GetValueOrDefault("BENEFICIOS", "No estipulados.") },
                { "PROHIBICIONES_ESPECIALES", datosExtraidos.GetValueOrDefault("PROHIBICIONES_ESPECIALES", "No especificadas.") },

                // Texto completo del reglamento actualizado
                { "ACTUALIZACION_DS44", reglamentoActualizado },
                { "CONTENIDO_DINAMICO", reglamentoActualizado }
            };

            // 4. Generar Documento
            var docGenerado = await _documentoService.GenerarDocumentoAsync(SelectedEmpresa.Id, "CAMO_2025", datosReemplazo);

            if (docGenerado != null)
            {
                ResultadoAnalisis += $"\n\n¡MIGRACIÓN COMPLETADA!\n" +
                                     $"Archivo: {Path.GetFileName(docGenerado.RutaArchivoEditable)}\n" +
                                     $"Ubicación: {docGenerado.RutaArchivoEditable}\n";
                
                _notificationService.ShowSuccess("Migración completada exitosamente.");
                RegistrarPaso($"Documento generado: {Path.GetFileName(docGenerado.RutaArchivoEditable)}");
            }
            else
            {
                ResultadoAnalisis += "\nError: No se pudo generar el documento. Verifique la plantilla MASTER_2025.";
                _notificationService.ShowError("Error en la generación.");
                RegistrarPaso("Error al generar documento con plantilla MASTER_2025.");
            }
        }
        catch (OperationCanceledException)
        {
            ResultadoAnalisis += "\n\n⛔ OPERACIÓN CANCELADA por el usuario.";
            _notificationService.ShowWarning("Migración cancelada.");
            RegistrarPaso("⛔ Migración cancelada por el usuario.");
        }
        catch (Exception ex)
        {
            ResultadoAnalisis += $"\nError crítico: {ex.Message}";
            _notificationService.ShowError($"Error: {ex.Message}");
            RegistrarPaso($"Error crítico en migración: {ex.Message}");
        }
        finally
        {
            IsAnalizando = false;
            PuedeCancel = false;
            EstadoAnalisis = "Listo";
            _cts?.Dispose();
            _cts = null;
        }
    }

    private void CrearDocumentoWord(string path, string contenido)
    {
        using var wordDoc = WordprocessingDocument.Create(path, WordprocessingDocumentType.Document);
        var mainPart = wordDoc.AddMainDocumentPart();
        mainPart.Document = new Document(new Body());
        var body = mainPart.Document.Body!;

        // Use MarkdownToOpenXml for rich formatting
        TagleLabsGestorSST.Services.MarkdownToOpenXml.ParseMarkdown(body, contenido);
        
        mainPart.Document.Save();
    }

    [RelayCommand]
    private void CargarEjemplo()
    {
        TextoDocumento = @"TITULO PRELIMINAR
1. La empresa declara su compromiso con la seguridad.
2. Se prohíbe el acoso sexual.
3. Los EPP son obligatorios.

(Este texto es antiguo y no menciona explícitamente los protocolos de la Ley Karin sobre acoso laboral y violencia en el trabajo, ni los nuevos estándares del DS44 sobre gestión de riesgos psicosociales)";
    }



    [RelayCommand]
    private async Task CargarArchivo()
    {
        var openFileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Documentos|*.docx;*.pdf;*.txt|Todos los archivos|*.*",
            Title = "Seleccionar Documento o Reglamento"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                var extension = Path.GetExtension(openFileDialog.FileName).ToLower();
                string contenido = string.Empty;

                if (extension == ".docx")
                {
                    contenido = LeerDocx(openFileDialog.FileName);
                }
                else if (extension == ".pdf")
                {
                    contenido = LeerPdf(openFileDialog.FileName);
                }
                else if (extension == ".txt")
                {
                    contenido = await File.ReadAllTextAsync(openFileDialog.FileName);
                }
                else
                {
                    _notificationService.ShowWarning("Formato no soportado. Por favor use .docx, .pdf o .txt");
                    return;
                }

                TextoDocumento = contenido;
                _notificationService.ShowSuccess($"Archivo cargado: {Path.GetFileName(openFileDialog.FileName)}");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error al leer archivo: {ex.Message}");
            }
        }
    }

    private string LeerDocx(string path)
    {
        try
        {
            using (var wordDoc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(path, false))
            {
                var body = wordDoc.MainDocumentPart?.Document.Body;
                if (body == null) return string.Empty;

                // Use OpenXmlToMarkdown for structured text extraction
                return TagleLabsGestorSST.Services.OpenXmlToMarkdown.ParseDocx(body);
            }
        }
        catch
        {
            return "Error al leer el archivo Word. Asegúrese de que no esté abierto en otro programa.";
        }
    }

    private string LeerPdf(string path)
    {
        try
        {
            using var pdf = PdfDocument.Open(path);
            var sb = new System.Text.StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                sb.Append(page.Text);
                sb.Append(" ");
            }
            return sb.ToString();
        }
        catch
        {
            return "Error al leer el archivo PDF. Asegúrese de que no esté protegido o dañado.";
        }
    }

    private async Task<string> ReescribirEnBloques(string textoOriginal, string razonSocial, string rut)
    {
        // Divide el texto en bloques de ~4000 caracteres para evitar timeouts y recortes
        const int bloqueMax = 4000;
        var resultado = new System.Text.StringBuilder();
        int offset = 0;
        int bloque = 1;
        TotalBloques = (int)Math.Ceiling(textoOriginal.Length / (double)bloqueMax);
        BloquesProcesados = 0;
        EtaTexto = $"Bloques 0/{TotalBloques}";
        var tiemposSeg = new List<double>();

        while (offset < textoOriginal.Length)
        {
            var length = Math.Min(bloqueMax, textoOriginal.Length - offset);
            var segmento = textoOriginal.Substring(offset, length);
            offset += length;

            RegistrarPaso($"Reescritura por bloque {bloque}...");
            var reloj = System.Diagnostics.Stopwatch.StartNew();
            var prompt = @$"ERES ABOGADO LABORALISTA + EXPERTO SST CHILE 2025.
TAREA: Reescribe este BLOQUE del Reglamento Interno, adaptándolo a Ley Karin (21.643), Ley 40 horas, DS44 y protocolos MINSAL.
EMPRESA: {razonSocial} - RUT {rut}.
PLANTILLA: replica la estructura y los títulos del documento base 'RIOHS DS 44- CONST.CAMO- JULIO 2025'. No conserves portadas ni índices viejos; respeta un único índice siguiendo esa plantilla.
REQUISITOS:
- NO RESUMIR. Mantén estructura y extensión similar al bloque original.
- Si no hay cambios normativos, copia literal.
- Integra horarios/pago/beneficios/prohibiciones si aparecen.
- Texto plano, sin markdown. Usa exactamente el nombre de empresa indicado.

BLOQUE #{bloque} A PROCESAR:
""{segmento}""

DEVUELVE SOLO el bloque reescrito (sin encabezados extra ni índices duplicados).";

            var bloqueReescrito = await _aiService.AnalizarTextoAsync(prompt, segmento, temperatura: 0.3);
            reloj.Stop();
            tiemposSeg.Add(reloj.Elapsed.TotalSeconds);
            RegistrarPaso($"Motor IA utilizado (bloque {bloque}): {_aiService.UltimoProveedor}");

            // Si la IA falla o responde con error, usamos el bloque original para no insertar mensajes de error
            if (string.IsNullOrWhiteSpace(bloqueReescrito) 
                || bloqueReescrito.StartsWith("Error", StringComparison.OrdinalIgnoreCase) 
                || bloqueReescrito.Contains("Error de conexión", StringComparison.OrdinalIgnoreCase)
                || bloqueReescrito.Contains("[Gemini]", StringComparison.OrdinalIgnoreCase)
                || bloqueReescrito.Contains("Ollama", StringComparison.OrdinalIgnoreCase))
            {
                bloqueReescrito = segmento;
            }

            BloquesProcesados = bloque;
            if (tiemposSeg.Count >= 1)
            {
                var promedio = tiemposSeg.Average();
                var restantes = TotalBloques - BloquesProcesados;
                var etaSeg = promedio * restantes;
                var etaMin = Math.Max(1, (int)Math.Ceiling(etaSeg / 60));
                EtaTexto = $"Bloques {BloquesProcesados}/{TotalBloques} · ETA ~{etaMin} min";
            }
            else
            {
                EtaTexto = $"Bloques {BloquesProcesados}/{TotalBloques}";
            }

            resultado.AppendLine(bloqueReescrito);
            resultado.AppendLine(); // separación
            bloque++;
        }

        return resultado.ToString();
    }

    private void RegistrarPaso(string mensaje)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Progreso.Insert(0, $"{DateTime.Now:HH:mm:ss} - {mensaje}");
            if (Progreso.Count > 50)
            {
                Progreso.RemoveAt(Progreso.Count - 1);
            }
            if (!string.IsNullOrWhiteSpace(EtaTexto))
            {
                Progreso.Insert(0, $"ETA: {EtaTexto}");
            }

            if (!string.IsNullOrWhiteSpace(_aiService.UltimoProveedor) || !string.IsNullOrWhiteSpace(_aiService.UltimoDetalle))
            {
                var detalle = string.IsNullOrWhiteSpace(_aiService.UltimoDetalle) ? string.Empty : $" | {_aiService.UltimoDetalle}";
                TrazaIa = $"{DateTime.Now:HH:mm:ss} - {_aiService.UltimoProveedor}{detalle}";
            }
        });
    }
}
