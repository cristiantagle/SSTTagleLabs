using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public interface IMatrizRiesgoService
{
    // Catálogo Maestro
    Task<IReadOnlyList<Rubro>> ObtenerRubrosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Actividad>> ObtenerActividadesPorRubroAsync(int rubroId, CancellationToken ct = default);
    Task<IReadOnlyList<Tarea>> ObtenerTareasPorActividadAsync(int actividadId, CancellationToken ct = default);
    Task<IReadOnlyList<Peligro>> ObtenerPeligrosAsync(CancellationToken ct = default);
    Task<IReadOnlyList<Control>> ObtenerControlesAsync(CancellationToken ct = default);

    // Matriz Empresa
    Task<MatrizRiesgoEmpresa?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<MatrizRiesgoEmpresa>> ObtenerMatrizPorCentroAsync(int centroId, CancellationToken ct = default);
    Task<MatrizRiesgoEmpresa> AgregarItemMatrizAsync(MatrizRiesgoEmpresa item, CancellationToken ct = default);
    Task ActualizarItemMatrizAsync(MatrizRiesgoEmpresa item, CancellationToken ct = default);
    Task EliminarItemMatrizAsync(int id, CancellationToken ct = default);
    Task<List<MatrizRiesgoEmpresa>> GenerarRiesgosSugeridosAsync(int actividadId, int centroId, CancellationToken ct = default);
    Task<List<MatrizRiesgoEmpresa>> ImportarRiesgosPorRubroAsync(int rubroId, int centroId, CancellationToken ct = default);
    Task<List<MatrizRiesgoEmpresa>> SugerirRiesgosPorRubroAsync(int rubroId, int centroId, CancellationToken ct = default);
    
    // Exportación
    Task<byte[]> ExportarMiperAExcelAsync(int centroId, CancellationToken ct = default);
}

public class MatrizRiesgoService : IMatrizRiesgoService
{
    private readonly TagleLabsContext _db;
    private readonly IAuditoriaService _auditoriaService;
    private readonly ILocalAiService _aiService;

    public MatrizRiesgoService(TagleLabsContext db, IAuditoriaService auditoriaService, ILocalAiService aiService)
    {
        _db = db;
        _auditoriaService = auditoriaService;
        _aiService = aiService;
    }

    public async Task<IReadOnlyList<Rubro>> ObtenerRubrosAsync(CancellationToken ct = default)
    {
        return await _db.Rubros.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Actividad>> ObtenerActividadesPorRubroAsync(int rubroId, CancellationToken ct = default)
    {
        return await _db.Actividades.Where(a => a.RubroId == rubroId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Tarea>> ObtenerTareasPorActividadAsync(int actividadId, CancellationToken ct = default)
    {
        return await _db.Tareas.Where(t => t.ActividadId == actividadId).ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Peligro>> ObtenerPeligrosAsync(CancellationToken ct = default)
    {
        return await _db.Peligros.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<Control>> ObtenerControlesAsync(CancellationToken ct = default)
    {
        return await _db.Controles.ToListAsync(ct);
    }

    public async Task<IReadOnlyList<MatrizRiesgoEmpresa>> ObtenerMatrizPorCentroAsync(int centroId, CancellationToken ct = default)
    {
        return await _db.MatrizRiesgosEmpresa
            .Include(m => m.Tarea)
            .Include(m => m.Peligro)
            .Include(m => m.Control)
            .Where(m => m.CentroTrabajoId == centroId)
            .ToListAsync(ct);
    }

    public async Task<MatrizRiesgoEmpresa?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.MatrizRiesgosEmpresa
            .Include(m => m.Tarea)
            .Include(m => m.Peligro)
            .Include(m => m.Control)
            .FirstOrDefaultAsync(m => m.Id == id, ct);
    }

    public async Task<MatrizRiesgoEmpresa> AgregarItemMatrizAsync(MatrizRiesgoEmpresa item, CancellationToken ct = default)
    {
        CalcularNivelRiesgo(item);
        _db.MatrizRiesgosEmpresa.Add(item);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Crear", "Matriz MIPER", item.Id.ToString(), $"Se agregó riesgo para tarea ID {item.TareaId}");
        
        return item;
    }

    public async Task ActualizarItemMatrizAsync(MatrizRiesgoEmpresa item, CancellationToken ct = default)
    {
        CalcularNivelRiesgo(item);
        _db.MatrizRiesgosEmpresa.Update(item);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Editar", "Matriz MIPER", item.Id.ToString(), $"Se actualizó riesgo ID {item.Id}");
    }

    public async Task EliminarItemMatrizAsync(int id, CancellationToken ct = default)
    {
        var item = await _db.MatrizRiesgosEmpresa.FindAsync(new object[] { id }, ct);
        if (item == null) return;
        _db.MatrizRiesgosEmpresa.Remove(item);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Eliminar", "Matriz MIPER", id.ToString(), $"Se eliminó riesgo ID {id}");
    }

    public async Task<List<MatrizRiesgoEmpresa>> GenerarRiesgosSugeridosAsync(int actividadId, int centroId, CancellationToken ct = default)
    {
        var actividad = await _db.Actividades
            .Include(a => a.Rubro)
            .FirstOrDefaultAsync(a => a.Id == actividadId, ct);

        if (actividad == null) return new List<MatrizRiesgoEmpresa>();

        // 1. Obtener Contexto de KnowledgeItems
        var knowledgeItems = await _db.KnowledgeItems
            .OrderByDescending(k => k.ProcessedDate)
            .Take(5) // Tomar los 5 más recientes por ahora
            .ToListAsync(ct);

        var contextoBuilder = new System.Text.StringBuilder();
        foreach (var item in knowledgeItems)
        {
            contextoBuilder.AppendLine($"--- FUENTE: {item.SourceFile} ---");
            contextoBuilder.AppendLine(item.Content.Length > 500 ? item.Content.Substring(0, 500) + "..." : item.Content);
        }

        // 2. Construir Prompt
        var prompt = $@"
ERES UN EXPERTO EN PREVENCIÓN DE RIESGOS (CHILE, DS 44).
ACTIVIDAD: {actividad.Nombre}
RUBRO: {actividad.Rubro.Nombre}

CONTEXTO ADICIONAL (Normativa/Protocolos):
{contextoBuilder}

TAREA: Genera una lista de 3 a 5 riesgos específicos y técnicos para esta actividad.
FORMATO JSON (Array de objetos):
[
  {{
    ""Peligro"": ""Nombre técnico del peligro (ej. Ruido, Sílice)"",
    ""Riesgo"": ""Consecuencia (ej. Hipoacusia, Silicosis)"",
    ""MedidaControl"": ""Medida de control específica (Ingeniería/Administrativa)"",
    ""Probabilidad"": 1 (Baja), 2 (Media) o 3 (Alta),
    ""Consecuencia"": 1 (Leve), 2 (Grave) o 3 (Fatal)
  }}
]
RESPONDE SOLO CON EL JSON.";

        // 3. Llamar a IA
        var jsonResponse = await _aiService.AnalizarTextoAsync(prompt, "Contexto Legal General");
        
        // 4. Parsear y Mapear
        var sugerencias = new List<MatrizRiesgoEmpresa>();
        try
        {
            jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse);

            if (items != null)
            {
                foreach (var item in items)
                {
                    // Buscar o Crear Peligro/Control (Simplificado: Usamos existentes o genéricos por ahora)
                    // En un sistema real, buscaríamos por nombre o crearíamos nuevos.
                    // Aquí asignaremos el PRIMER peligro/control que coincida o uno genérico "Otros".
                    
                    var nombrePeligro = (string)item.Peligro;
                    var peligro = await _db.Peligros.FirstOrDefaultAsync(p => p.Categoria.Contains(nombrePeligro) || p.Descripcion.Contains(nombrePeligro), ct)
                                  ?? await _db.Peligros.FirstOrDefaultAsync(ct); // Fallback

                    var control = await _db.Controles.FirstOrDefaultAsync(ct); // Fallback

                    if (peligro != null && control != null)
                    {
                        var matrizItem = new MatrizRiesgoEmpresa
                        {
                            CentroTrabajoId = centroId,
                            TareaId = null, // Sin tarea (generado por IA)
                            PeligroId = peligro.Id,
                            Peligro = peligro,
                            ControlId = control.Id,
                            Control = control,
                            MedidaControlEspecifica = (string)item.MedidaControl,
                            Probabilidad = (int)item.Probabilidad,
                            Consecuencia = (int)item.Consecuencia,
                            NivelRiesgo = "Pendiente"
                        };
                        CalcularNivelRiesgo(matrizItem);
                        sugerencias.Add(matrizItem);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parseando IA: {ex.Message}");
        }

        return sugerencias;
    }

    private void CalcularNivelRiesgo(MatrizRiesgoEmpresa item)
    {
        var valor = item.Probabilidad * item.Consecuencia;
        if (valor <= 4) item.NivelRiesgo = "Bajo";
        else if (valor <= 8) item.NivelRiesgo = "Medio";
        else item.NivelRiesgo = "Alto";
    }

    public async Task<List<MatrizRiesgoEmpresa>> ImportarRiesgosPorRubroAsync(int rubroId, int centroId, CancellationToken ct = default)
    {
        // 1. Obtener todas las Tareas del Rubro
        var tareasRubro = await _db.Tareas
            .Include(t => t.Actividad)
            .Where(t => t.Actividad.RubroId == rubroId)
            .ToListAsync(ct);

        var tareaIds = tareasRubro.Select(t => t.Id).ToList();

        // 2. Obtener las relaciones Tarea-Peligro-Control predefinidas (Standard)
        var estandares = await _db.TareaPeligroControles
            .Include(tpc => tpc.Peligro)
            .Include(tpc => tpc.Control)
            .Where(tpc => tareaIds.Contains(tpc.TareaId))
            .ToListAsync(ct);

        var nuevosItems = new List<MatrizRiesgoEmpresa>();

        foreach (var estandar in estandares)
        {
            // Verificar si ya existe para no duplicar
            var existe = await _db.MatrizRiesgosEmpresa
                .AnyAsync(m => m.CentroTrabajoId == centroId 
                            && m.TareaId == estandar.TareaId 
                            && m.PeligroId == estandar.PeligroId, ct);

            if (!existe)
            {
                var nuevo = new MatrizRiesgoEmpresa
                {
                    CentroTrabajoId = centroId,
                    TareaId = estandar.TareaId,
                    PeligroId = estandar.PeligroId,
                    ControlId = estandar.ControlId,
                    Probabilidad = estandar.Probabilidad,
                    Consecuencia = estandar.Consecuencia,
                    NivelRiesgo = "Pendiente", // Se recalcula
                    MedidaControlEspecifica = estandar.Control.Descripcion // Default
                };
                CalcularNivelRiesgo(nuevo);
                nuevosItems.Add(nuevo);
            }
        }

        if (nuevosItems.Any())
        {
            _db.MatrizRiesgosEmpresa.AddRange(nuevosItems);
            await _db.SaveChangesAsync(ct);
            await _auditoriaService.RegistrarAccionAsync("Importar", "Matriz MIPER", "Masivo", $"Se importaron {nuevosItems.Count} riesgos del rubro ID {rubroId}");
        }

        return nuevosItems;
    }

    public async Task<List<MatrizRiesgoEmpresa>> SugerirRiesgosPorRubroAsync(int rubroId, int centroId, CancellationToken ct = default)
    {
        // 1. Obtener Rubro y nombre descriptivo
        var rubro = await _db.Rubros.FindAsync(new object[] { rubroId }, cancellationToken: ct);
        if (rubro == null) return new List<MatrizRiesgoEmpresa>();

        // 2. Obtener contexto de protocolos MINSAL y normativa
        var knowledgeItems = await _db.KnowledgeItems
            .OrderByDescending(k => k.ProcessedDate)
            .Take(10)
            .ToListAsync(ct);

        var contextoBuilder = new System.Text.StringBuilder();
        foreach (var item in knowledgeItems.Take(3)) // Top 3 documentos
        {
            contextoBuilder.AppendLine($"[{item.SourceFile}]: {item.Content.Substring(0, Math.Min(300, item.Content.Length))}...");
        }

        // 3. Construir prompt comprehensivo para sugerencias por rubro
        var prompt = $@"
ERES UN EXPERTO SENIOR EN PREVENCION DE RIESGOS LABORALES (CHILE, DS 44, LEY 16.744, PROTOCOLOS MINSAL).
Necesito que generes una matriz MIPER COMPLETA para el siguiente RUBRO:

RUBRO: {rubro.Nombre}

NORMATIVA Y PROTOCOLOS RELEVANTES:
{contextoBuilder}

INSTRUCCIONES CRÍTICAS:
1. Genera entre 25-35 riesgos ESPECÍFICOS, TÉCNICOS Y REALISTAS para este rubro.
2. CUBRE OBLIGATORIAMENTE las siguientes categorías:
   - MECÁNICOS: atrapamiento, golpes, cortes, proyección de partículas, caídas de altura, caídas mismo nivel
   - ERGONÓMICOS: TMERT, MMC (manejo manual de cargas), posturas forzadas, movimientos repetitivos
   - QUÍMICOS: exposición a sustancias tóxicas, sílice, humos, vapores, aerosoles
   - FÍSICOS: ruido (PREXOR), vibraciones, radiación UV, temperaturas extremas, iluminación
   - ELÉCTRICOS: contacto directo/indirecto, arco eléctrico
   - PSICOSOCIALES: estrés laboral, violencia (Ley Karin), acoso, carga mental
   - BIOLÓGICOS: exposición a agentes patógenos, COVID, vectores
3. Para cada riesgo, sugiere medidas de control JERÁRQUICAS (priorizar eliminación/ingeniería sobre EPP).
4. Asigna probabilidad y consecuencia basándote en la realidad del rubro.

FORMATO JSON (Array de objetos):
[
  {{
    ""Peligro"": ""Descripción técnica específica del peligro"",
    ""Riesgo"": ""Consecuencia o daño potencial"",
    ""MedidaControl"": ""Medida de control específica y aplicable"",
    ""Probabilidad"": número entre 1-5,
    ""Consecuencia"": número entre 1-5,
    ""Categoria"": ""Mecánico|Ergonómico|Químico|Físico|Eléctrico|Psicosocial|Biológico""
  }}
]

REGLAS ADICIONALES:
- NO repitas peligros similares
- Incluye peligros específicos de actividades típicas del rubro (ej: para construcción incluir excavaciones, trabajos en altura, soldadura, hormigonado, etc.)
- Las medidas de control deben ser concretas y aplicables (no genéricas como ""usar EPP"")
- Probabilidad: 1=Raro, 2=Improbable, 3=Posible, 4=Probable, 5=Casi seguro
- Consecuencia: 1=Insignificante, 2=Menor, 3=Moderado, 4=Mayor, 5=Catastrófico

RESPONDE SOLO CON EL JSON, SIN TEXTO ADICIONAL.";

        // 4. Llamar IA con temperatura más alta para más creatividad
        var jsonResponse = await _aiService.AnalizarTextoAsync(prompt, "Protocolos MINSAL y DS44");

        // 5. Parsear y Mapear respuesta
        var sugerencias = new List<MatrizRiesgoEmpresa>();
        try
        {
            jsonResponse = jsonResponse.Replace("```json", "").Replace("```", "").Trim();
            var items = Newtonsoft.Json.JsonConvert.DeserializeObject<List<dynamic>>(jsonResponse);

            if (items != null)
            {
                foreach (var item in items)
                {
                    string nombrePeligro = (string)item.Peligro ?? "";
                    string categoria = (string)item.Categoria ?? "Otro";
                    string medidaControl = (string)item.MedidaControl ?? "";
                    
                    // Buscar peligro por categoría o descripción similar
                    var primeraPalabraPeligro = nombrePeligro.Split(' ').FirstOrDefault() ?? "";
                    var peligro = await _db.Peligros
                        .FirstOrDefaultAsync(p => p.Categoria == categoria 
                            || p.Descripcion.Contains(primeraPalabraPeligro), ct)
                        ?? await _db.Peligros.FirstOrDefaultAsync(ct);

                    // Buscar control apropiado según palabras clave en la medida sugerida
                    var medidaUpper = medidaControl.ToUpperInvariant();
                    Control? control;
                    
                    if (medidaUpper.Contains("EPP") || medidaUpper.Contains("CASCO") || medidaUpper.Contains("GUANTE") 
                        || medidaUpper.Contains("ZAPATO") || medidaUpper.Contains("PROTECTOR") || medidaUpper.Contains("LENTE"))
                    {
                        control = await _db.Controles.FirstOrDefaultAsync(c => c.Tipo.Contains("EPP") || c.Descripcion.Contains("EPP"), ct);
                    }
                    else if (medidaUpper.Contains("CAPACIT") || medidaUpper.Contains("CHARLA") || medidaUpper.Contains("INDUC"))
                    {
                        control = await _db.Controles.FirstOrDefaultAsync(c => c.Tipo.Contains("Capacitación") || c.Descripcion.Contains("Capacitación"), ct);
                    }
                    else if (medidaUpper.Contains("PROCEDIMIENTO") || medidaUpper.Contains("PROTOCOLO") || medidaUpper.Contains("INSTRUCTIVO"))
                    {
                        control = await _db.Controles.FirstOrDefaultAsync(c => c.Tipo.Contains("Procedimiento") || c.Descripcion.Contains("Procedimiento"), ct);
                    }
                    else if (medidaUpper.Contains("SEÑAL") || medidaUpper.Contains("DEMARCA"))
                    {
                        control = await _db.Controles.FirstOrDefaultAsync(c => c.Descripcion.Contains("Señalización"), ct);
                    }
                    else if (medidaUpper.Contains("INGENIERÍA") || medidaUpper.Contains("BARRERA") || medidaUpper.Contains("VENTILACIÓN") 
                        || medidaUpper.Contains("ASPIRACIÓN") || medidaUpper.Contains("AISLAMIENTO"))
                    {
                        control = await _db.Controles.FirstOrDefaultAsync(c => c.Jerarquia == JerarquiaControl.Ingenieria, ct);
                    }
                    else
                    {
                        // Buscar cualquier control que contenga alguna palabra de la medida
                        var palabras = medidaControl.Split(' ').Where(p => p.Length > 4).Take(3).ToList();
                        control = null;
                        foreach (var palabra in palabras)
                        {
                            control = await _db.Controles.FirstOrDefaultAsync(c => c.Descripcion.Contains(palabra), ct);
                            if (control != null) break;
                        }
                    }
                    
                    // Fallback si no encontró nada
                    control ??= await _db.Controles.FirstOrDefaultAsync(ct);
                    
                    // Prevenir duplicados por descripción de peligro
                    var peligroDescripcion = nombrePeligro.ToUpperInvariant().Trim();
                    if (sugerencias.Any(s => s.MedidaControlEspecifica?.ToUpperInvariant()?.Contains(peligroDescripcion) == true
                        || (s.Peligro?.Descripcion?.ToUpperInvariant() ?? "").Contains(peligroDescripcion)))
                    {
                        continue; // Skip duplicate
                    }

                    if (peligro != null && control != null)
                    {
                        var matrizItem = new MatrizRiesgoEmpresa
                        {
                            CentroTrabajoId = centroId,
                            TareaId = null, // Sin tarea asignada (generado por IA)
                            PeligroId = peligro.Id,
                            Peligro = peligro,
                            ControlId = control.Id,
                            Control = control,
                            MedidaControlEspecifica = medidaControl, // Siempre usar la medida específica de la IA
                            Probabilidad = (int)item.Probabilidad,
                            Consecuencia = (int)item.Consecuencia,
                            NivelRiesgo = "Pendiente"
                        };
                        CalcularNivelRiesgo(matrizItem);
                        sugerencias.Add(matrizItem);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error parseando sugerencias IA: {ex.Message}");
        }

        return sugerencias;
    }

    public async Task<byte[]> ExportarMiperAExcelAsync(int centroId, CancellationToken ct = default)
    {
        // Obtener datos de la matriz
        var matriz = await _db.MatrizRiesgosEmpresa
            .Include(m => m.Tarea)
            .Include(m => m.Peligro)
            .Include(m => m.Control)
            .Include(m => m.CentroTrabajo)
                .ThenInclude(c => c.Empresa)
            .Where(m => m.CentroTrabajoId == centroId)
            .OrderBy(m => m.NivelRiesgo)
            .ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var worksheet = workbook.Worksheets.Add("Matriz MIPER");

        // Encabezado del documento
        var centro = matriz.FirstOrDefault()?.CentroTrabajo;
        var empresa = centro?.Empresa;
        
        worksheet.Cell("A1").Value = "MATRIZ DE IDENTIFICACIÓN DE PELIGROS Y EVALUACIÓN DE RIESGOS (MIPER)";
        worksheet.Range("A1:I1").Merge().Style.Font.SetBold(true).Font.SetFontSize(14);
        worksheet.Cell("A2").Value = $"Empresa: {empresa?.RazonSocial ?? "N/A"}";
        worksheet.Cell("A3").Value = $"Centro: {centro?.Nombre ?? "N/A"}";
        worksheet.Cell("A4").Value = $"Fecha: {DateTime.Now:dd/MM/yyyy}";

        // Headers de la tabla
        var headerRow = 6;
        var headers = new[] { "ID", "Tarea", "Peligro", "Medida de Control", "Probabilidad", "Consecuencia", "Valor", "Nivel Riesgo", "Control Aplicado" };
        for (var i = 0; i < headers.Length; i++)
        {
            worksheet.Cell(headerRow, i + 1).Value = headers[i];
        }
        var headerRange = worksheet.Range(headerRow, 1, headerRow, headers.Length);
        headerRange.Style
            .Fill.SetBackgroundColor(XLColor.FromHtml("#0F172A"))
            .Font.SetFontColor(XLColor.White)
            .Font.SetBold(true)
            .Alignment.SetHorizontal(XLAlignmentHorizontalValues.Center);

        // Datos
        var dataRow = headerRow + 1;
        foreach (var item in matriz)
        {
            worksheet.Cell(dataRow, 1).Value = item.Id;
            worksheet.Cell(dataRow, 2).Value = item.Tarea?.Nombre ?? "General";
            worksheet.Cell(dataRow, 3).Value = item.Peligro?.Descripcion ?? "";
            worksheet.Cell(dataRow, 4).Value = item.MedidaControlEspecifica;
            worksheet.Cell(dataRow, 5).Value = item.Probabilidad;
            worksheet.Cell(dataRow, 6).Value = item.Consecuencia;
            worksheet.Cell(dataRow, 7).Value = item.ValorRiesgo;
            worksheet.Cell(dataRow, 8).Value = item.NivelRiesgo;
            worksheet.Cell(dataRow, 9).Value = item.Control?.Descripcion ?? "";

            // Color según nivel de riesgo
            var nivelColor = item.NivelRiesgo switch
            {
                "Alto" => XLColor.FromHtml("#FEE2E2"),    // Rojo claro
                "Medio" => XLColor.FromHtml("#FEF3C7"),   // Amarillo claro
                "Bajo" => XLColor.FromHtml("#D1FAE5"),    // Verde claro
                _ => XLColor.White
            };
            worksheet.Range(dataRow, 1, dataRow, headers.Length).Style.Fill.SetBackgroundColor(nivelColor);

            dataRow++;
        }

        // Ajustar ancho de columnas
        worksheet.Columns().AdjustToContents(5, 50);

        // Bordes
        var tableRange = worksheet.Range(headerRow, 1, dataRow - 1, headers.Length);
        tableRange.Style.Border.SetOutsideBorder(XLBorderStyleValues.Thin);
        tableRange.Style.Border.SetInsideBorder(XLBorderStyleValues.Thin);

        // Guardar a memoria
        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
