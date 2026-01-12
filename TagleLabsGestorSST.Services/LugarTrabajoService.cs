using Microsoft.EntityFrameworkCore;
using System.IO;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public class LugarTrabajoService : ILugarTrabajoService
{
    private readonly TagleLabsContext _db;
    private readonly DocumentoService _documentoService;

    public LugarTrabajoService(TagleLabsContext db, DocumentoService documentoService)
    {
        _db = db;
        _documentoService = documentoService;
    }

    #region CRUD Lugares de Trabajo

    public async Task<List<LugarTrabajo>> GetAllAsync()
    {
        return await _db.LugaresTrabajo
            .Where(l => l.Activo)
            .Include(l => l.Plantillas.Where(p => p.Activo))
            .OrderBy(l => l.Nombre)
            .ToListAsync();
    }

    public async Task<LugarTrabajo?> GetByIdAsync(int id)
    {
        return await _db.LugaresTrabajo
            .Include(l => l.Plantillas.Where(p => p.Activo))
            .Include(l => l.Asignaciones.Where(a => a.Activo))
                .ThenInclude(a => a.Trabajador)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<LugarTrabajo> CreateAsync(LugarTrabajo lugar)
    {
        _db.LugaresTrabajo.Add(lugar);
        await _db.SaveChangesAsync();
        return lugar;
    }

    public async Task UpdateAsync(LugarTrabajo lugar)
    {
        _db.LugaresTrabajo.Update(lugar);
        await _db.SaveChangesAsync();
    }

    public async Task DeleteAsync(int id)
    {
        var lugar = await _db.LugaresTrabajo.FindAsync(id);
        if (lugar != null)
        {
            lugar.Activo = false; // Soft delete
            await _db.SaveChangesAsync();
        }
    }

    #endregion

    #region Plantillas

    public async Task<List<PlantillaLugarTrabajo>> GetPlantillasAsync(int lugarTrabajoId)
    {
        return await _db.PlantillasLugarTrabajo
            .Where(p => p.LugarTrabajoId == lugarTrabajoId && p.Activo)
            .OrderBy(p => p.Orden)
            .ThenBy(p => p.NombreDocumento)
            .ToListAsync();
    }

    public async Task<PlantillaLugarTrabajo> AddPlantillaAsync(PlantillaLugarTrabajo plantilla)
    {
        _db.PlantillasLugarTrabajo.Add(plantilla);
        await _db.SaveChangesAsync();
        return plantilla;
    }

    public async Task UpdatePlantillaAsync(PlantillaLugarTrabajo plantilla)
    {
        _db.PlantillasLugarTrabajo.Update(plantilla);
        await _db.SaveChangesAsync();
    }

    public async Task DeletePlantillaAsync(int plantillaId)
    {
        var plantilla = await _db.PlantillasLugarTrabajo.FindAsync(plantillaId);
        if (plantilla != null)
        {
            plantilla.Activo = false;
            await _db.SaveChangesAsync();
        }
    }

    #endregion

    #region Asignaciones

    public async Task AsignarTrabajadorAsync(int trabajadorId, int lugarTrabajoId)
    {
        // Verificar si ya existe
        var existente = await _db.AsignacionesTrabajador
            .FirstOrDefaultAsync(a => a.TrabajadorId == trabajadorId && a.LugarTrabajoId == lugarTrabajoId);
        
        if (existente != null)
        {
            existente.Activo = true;
            existente.FechaAsignacion = DateTime.Now;
        }
        else
        {
            _db.AsignacionesTrabajador.Add(new AsignacionTrabajador
            {
                TrabajadorId = trabajadorId,
                LugarTrabajoId = lugarTrabajoId
            });
        }
        
        // Crear documentos requeridos pendientes para este trabajador
        await CrearDocumentosRequeridosAsync(trabajadorId, lugarTrabajoId);
        
        await _db.SaveChangesAsync();
    }

    public async Task DesasignarTrabajadorAsync(int trabajadorId, int lugarTrabajoId)
    {
        var asignacion = await _db.AsignacionesTrabajador
            .FirstOrDefaultAsync(a => a.TrabajadorId == trabajadorId && a.LugarTrabajoId == lugarTrabajoId);
        
        if (asignacion != null)
        {
            asignacion.Activo = false;
            await _db.SaveChangesAsync();
        }
    }

    public async Task<List<Trabajador>> GetTrabajadoresAsignadosAsync(int lugarTrabajoId)
    {
        return await _db.AsignacionesTrabajador
            .Where(a => a.LugarTrabajoId == lugarTrabajoId && a.Activo)
            .Include(a => a.Trabajador)
            .Select(a => a.Trabajador)
            .OrderBy(t => t.NombreCompleto)
            .ToListAsync();
    }

    public async Task<List<LugarTrabajo>> GetLugaresDeTrabajoAsync(int trabajadorId)
    {
        return await _db.AsignacionesTrabajador
            .Where(a => a.TrabajadorId == trabajadorId && a.Activo)
            .Include(a => a.LugarTrabajo)
            .Select(a => a.LugarTrabajo)
            .ToListAsync();
    }

    private async Task CrearDocumentosRequeridosAsync(int trabajadorId, int lugarTrabajoId)
    {
        // Obtener plantillas del lugar que son individuales
        var plantillas = await _db.PlantillasLugarTrabajo
            .Where(p => p.LugarTrabajoId == lugarTrabajoId && 
                       p.Activo && 
                       p.TipoAplicacion == TipoAplicacionDocumento.Individual)
            .ToListAsync();
        
        foreach (var plantilla in plantillas)
        {
            // Verificar si ya existe un documento requerido
            var existente = await _db.DocumentosRequeridos
                .FirstOrDefaultAsync(d => d.TrabajadorId == trabajadorId && 
                                          d.PlantillaLugarTrabajoId == plantilla.Id);
            
            if (existente == null)
            {
                _db.DocumentosRequeridos.Add(new DocumentoRequerido
                {
                    TrabajadorId = trabajadorId,
                    PlantillaLugarTrabajoId = plantilla.Id,
                    Estado = EstadoDocumentoRequerido.Pendiente
                });
            }
        }
    }

    #endregion

    #region Estado de Documentación

    public async Task<EstadoDocumentacionDto> GetEstadoDocumentacionAsync(int trabajadorId, int lugarTrabajoId)
    {
        var trabajador = await _db.Trabajadores.FindAsync(trabajadorId);
        if (trabajador == null) return new EstadoDocumentacionDto();
        
        // Obtener plantillas individuales del lugar
        var plantillasIndividuales = await _db.PlantillasLugarTrabajo
            .Where(p => p.LugarTrabajoId == lugarTrabajoId && 
                       p.Activo && 
                       p.TipoAplicacion == TipoAplicacionDocumento.Individual)
            .ToListAsync();
        
        // Obtener documentos requeridos del trabajador para este lugar
        var documentosRequeridos = await _db.DocumentosRequeridos
            .Where(d => d.TrabajadorId == trabajadorId && 
                       d.PlantillaLugarTrabajo.LugarTrabajoId == lugarTrabajoId)
            .Include(d => d.PlantillaLugarTrabajo)
            .ToListAsync();
        
        // Obtener charlas masivas donde asistió el trabajador
        var asistenciasCharlas = await _db.AsistenciasCharla
            .Where(a => a.TrabajadorId == trabajadorId && 
                       a.Charla.LugarTrabajoId == lugarTrabajoId && 
                       a.Asistio)
            .Include(a => a.Charla)
            .ToListAsync();
        
        var dto = new EstadoDocumentacionDto
        {
            TrabajadorId = trabajadorId,
            NombreTrabajador = trabajador.NombreCompleto,
            TotalRequeridos = plantillasIndividuales.Count,
            TotalGenerados = documentosRequeridos.Count(d => d.Estado == EstadoDocumentoRequerido.Generado),
            TotalVencidos = documentosRequeridos.Count(d => d.Estado == EstadoDocumentoRequerido.Vencido)
        };
        
        // Mapear documentos
        foreach (var plantilla in plantillasIndividuales)
        {
            var docReq = documentosRequeridos.FirstOrDefault(d => d.PlantillaLugarTrabajoId == plantilla.Id);
            dto.Documentos.Add(new DocumentoRequeridoDto
            {
                Id = docReq?.Id ?? 0,
                CodigoPlantilla = plantilla.Codigo,
                NombreDocumento = plantilla.NombreDocumento,
                Estado = docReq?.Estado ?? EstadoDocumentoRequerido.Pendiente,
                FechaGeneracion = docReq?.FechaGeneracion,
                FechaVencimiento = docReq?.FechaVencimiento,
                EsObligatorio = plantilla.EsObligatorio
            });
        }
        
        return dto;
    }

    public async Task<List<EstadoDocumentacionDto>> GetEstadoTodosLosTrabajadioresAsync(int lugarTrabajoId)
    {
        var trabajadores = await GetTrabajadoresAsignadosAsync(lugarTrabajoId);
        var estados = new List<EstadoDocumentacionDto>();
        
        foreach (var trabajador in trabajadores)
        {
            var estado = await GetEstadoDocumentacionAsync(trabajador.Id, lugarTrabajoId);
            estados.Add(estado);
        }
        
        return estados;
    }

    #endregion

    #region Generación de Documentos

    public async Task<DocumentoRequerido> GenerarDocumentoAsync(int trabajadorId, int plantillaId, string rutaDocumento)
    {
        var docRequerido = await _db.DocumentosRequeridos
            .FirstOrDefaultAsync(d => d.TrabajadorId == trabajadorId && d.PlantillaLugarTrabajoId == plantillaId);
        
        if (docRequerido == null)
        {
            // Crear si no existe
            docRequerido = new DocumentoRequerido
            {
                TrabajadorId = trabajadorId,
                PlantillaLugarTrabajoId = plantillaId
            };
            _db.DocumentosRequeridos.Add(docRequerido);
        }
        
        docRequerido.RutaDocumentoGenerado = rutaDocumento;
        docRequerido.FechaGeneracion = DateTime.Now;
        docRequerido.Estado = EstadoDocumentoRequerido.Generado;
        
        // Calcular vencimiento si aplica
        var plantilla = await _db.PlantillasLugarTrabajo.FindAsync(plantillaId);
        if (plantilla?.DiasVigencia.HasValue == true)
        {
            docRequerido.FechaVencimiento = DateTime.Now.AddDays(plantilla.DiasVigencia.Value);
        }
        
        await _db.SaveChangesAsync();
        return docRequerido;
    }

    /// <summary>
    /// Genera un documento real procesando placeholders en Word o Excel
    /// </summary>
    public async Task<string> GenerarDocumentoRealAsync(
        string rutaPlantilla, 
        Dictionary<string, string> datos,
        string? nombreBase = null,
        List<(string Nombre, string Cargo, string Rut)>? listaAsistentes = null)
    {
        if (string.IsNullOrEmpty(rutaPlantilla) || !File.Exists(rutaPlantilla))
            throw new FileNotFoundException($"Plantilla no encontrada: {rutaPlantilla}");
        
        var extension = Path.GetExtension(rutaPlantilla).ToLower();
        var nombreArchivo = nombreBase ?? Path.GetFileNameWithoutExtension(rutaPlantilla);
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        
        // Carpeta de salida
        var outputDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TagleLabsGestorSST", "Outputs", "LugaresTrabajo"
        );
        Directory.CreateDirectory(outputDir);
        
        var rutaSalida = Path.Combine(outputDir, $"{nombreArchivo}_{timestamp}{extension}");
        
        // Procesar en hilo de fondo para no bloquear UI
        await Task.Run(() =>
        {
            // Copiar plantilla
            File.Copy(rutaPlantilla, rutaSalida, overwrite: true);
            
            if (extension == ".xlsx" || extension == ".xls")
            {
                ProcesarExcel(rutaSalida, datos, listaAsistentes);
            }
            else if (extension == ".docx")
            {
                ProcesarWord(rutaSalida, datos);
            }
        });
        
        return rutaSalida;
    }
    
    private void ProcesarExcel(string rutaArchivo, Dictionary<string, string> datos, List<(string Nombre, string Cargo, string Rut)>? listaAsistentes)
    {
        using var workbook = new ClosedXML.Excel.XLWorkbook(rutaArchivo);
        
        // Si hay lista de asistentes, agregar placeholders por fila al diccionario
        if (listaAsistentes != null)
        {
            for (int i = 0; i < Math.Min(listaAsistentes.Count, 13); i++)
            {
                var num = i + 1;
                datos[$"NOMBRE_{num}"] = listaAsistentes[i].Nombre;
                datos[$"CARGO_{num}"] = listaAsistentes[i].Cargo;
                datos[$"RUT_{num}"] = listaAsistentes[i].Rut;
            }
            // Limpiar filas no usadas (del 4 al 13 si solo hay 3 asistentes)
            for (int i = listaAsistentes.Count; i < 13; i++)
            {
                var num = i + 1;
                datos[$"NOMBRE_{num}"] = "";
                datos[$"CARGO_{num}"] = "";
                datos[$"RUT_{num}"] = "";
            }
        }
        
        foreach (var worksheet in workbook.Worksheets)
        {
            // Reemplazar TODOS los placeholders {{KEY}} en todas las celdas
            foreach (var cell in worksheet.CellsUsed().ToList())
            {
                var valor = cell.Value.ToString();
                if (string.IsNullOrEmpty(valor)) continue;
                
                // Buscar cualquier placeholder {{...}}
                foreach (var kvp in datos)
                {
                    var placeholder = $"{{{{{kvp.Key}}}}}";
                    if (valor.Contains(placeholder))
                    {
                        valor = valor.Replace(placeholder, kvp.Value);
                    }
                }
                
                cell.Value = valor;
            }
        }
        
        workbook.Save();
    }
    
    private void ProcesarWord(string rutaArchivo, Dictionary<string, string> datos)
    {
        using var doc = DocumentFormat.OpenXml.Packaging.WordprocessingDocument.Open(rutaArchivo, true);
        var body = doc.MainDocumentPart?.Document?.Body;
        if (body == null) return;
        
        foreach (var text in body.Descendants<DocumentFormat.OpenXml.Wordprocessing.Text>())
        {
            foreach (var kvp in datos)
            {
                var placeholder = $"{{{{{kvp.Key}}}}}";
                if (text.Text.Contains(placeholder))
                {
                    text.Text = text.Text.Replace(placeholder, kvp.Value);
                }
            }
        }
        
        doc.MainDocumentPart?.Document?.Save();
    }

    /// <summary>
    /// Detecta plantillas automáticamente desde la carpeta Templates/{CodigoLugar}/
    /// </summary>
    public async Task<int> SincronizarPlantillasDesdeCarrpetaAsync(int lugarTrabajoId)
    {
        var lugar = await _db.LugaresTrabajo.FindAsync(lugarTrabajoId);
        if (lugar == null) return 0;
        
        // Buscar carpeta de plantillas
        var templatesDir = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "TagleLabsGestorSST", "Templates", lugar.Codigo
        );
        
        // También buscar en la carpeta del programa
        var appTemplatesDir = Path.Combine(
            AppContext.BaseDirectory, "Templates", lugar.Codigo
        );
        
        // Proyecto templates
        var projectDir = Path.Combine(
            Directory.GetCurrentDirectory(), "Templates", "LugaresTrabajo"
        );
        
        var plantillasAgregadas = 0;
        
        foreach (var dir in new[] { templatesDir, appTemplatesDir, projectDir })
        {
            if (!Directory.Exists(dir)) continue;
            
            var archivos = Directory.GetFiles(dir, "*.*")
                .Where(f => f.EndsWith(".docx") || f.EndsWith(".xlsx"))
                .ToList();
            
            foreach (var archivo in archivos)
            {
                var nombreArchivo = Path.GetFileNameWithoutExtension(archivo);
                var codigo = $"{lugar.Codigo}-{nombreArchivo}".ToUpper();
                
                // Verificar si ya existe
                var existente = await _db.PlantillasLugarTrabajo
                    .FirstOrDefaultAsync(p => p.LugarTrabajoId == lugarTrabajoId && p.Codigo == codigo);
                
                if (existente == null)
                {
                    var esCharla = nombreArchivo.ToLower().Contains("charla") || 
                                   nombreArchivo.ToLower().Contains("acta") ||
                                   nombreArchivo.ToLower().Contains("asistencia");
                    
                    _db.PlantillasLugarTrabajo.Add(new PlantillaLugarTrabajo
                    {
                        LugarTrabajoId = lugarTrabajoId,
                        Codigo = codigo,
                        NombreDocumento = nombreArchivo.Replace("_", " "),
                        RutaPlantilla = archivo,
                        TipoAplicacion = esCharla ? TipoAplicacionDocumento.Masivo : TipoAplicacionDocumento.Individual,
                        EsObligatorio = !esCharla,
                        Orden = plantillasAgregadas
                    });
                    plantillasAgregadas++;
                }
            }
        }
        
        if (plantillasAgregadas > 0)
            await _db.SaveChangesAsync();
        
        return plantillasAgregadas;
    }

    public async Task MarcarDocumentoVencidoAsync(int documentoRequeridoId)
    {
        var doc = await _db.DocumentosRequeridos.FindAsync(documentoRequeridoId);
        if (doc != null)
        {
            doc.Estado = EstadoDocumentoRequerido.Vencido;
            await _db.SaveChangesAsync();
        }
    }

    #endregion

    #region Charlas / Generación Masiva

    public async Task<CharlaSST> CrearCharlaAsync(CharlaSST charla, List<int> trabajadoresIds)
    {
        _db.CharlasSST.Add(charla);
        await _db.SaveChangesAsync();
        
        // Registrar asistencias
        foreach (var trabajadorId in trabajadoresIds)
        {
            _db.AsistenciasCharla.Add(new AsistenciaCharla
            {
                CharlaId = charla.Id,
                TrabajadorId = trabajadorId,
                Asistio = true
            });
        }
        
        await _db.SaveChangesAsync();
        return charla;
    }

    public async Task<List<CharlaSST>> GetCharlasAsync(int lugarTrabajoId, DateTime? desde = null, DateTime? hasta = null)
    {
        var query = _db.CharlasSST
            .Where(c => c.LugarTrabajoId == lugarTrabajoId)
            .Include(c => c.Asistencias)
                .ThenInclude(a => a.Trabajador)
            .AsQueryable();
        
        if (desde.HasValue)
            query = query.Where(c => c.Fecha >= desde.Value);
        
        if (hasta.HasValue)
            query = query.Where(c => c.Fecha <= hasta.Value);
        
        return await query.OrderByDescending(c => c.Fecha).ToListAsync();
    }

    public async Task<string> GenerarActaCharlaAsync(int charlaId)
    {
        var charla = await _db.CharlasSST
            .Include(c => c.LugarTrabajo)
            .Include(c => c.Asistencias)
                .ThenInclude(a => a.Trabajador)
            .FirstOrDefaultAsync(c => c.Id == charlaId);
        
        if (charla == null) return string.Empty;
        
        // Generar tabla de asistentes en formato para el documento
        var asistentes = charla.Asistencias
            .Where(a => a.Asistio)
            .Select(a => $"| {a.Trabajador.NombreCompleto} | {a.Trabajador.Rut} | ________________________ |")
            .ToList();
        
        var listaAsistentesMarkdown = string.Join("\n", asistentes);
        
        // Datos para la plantilla
        var datos = new Dictionary<string, string>
        {
            ["LUGAR_TRABAJO"] = charla.LugarTrabajo.Nombre,
            ["TEMA_CHARLA"] = charla.Tema,
            ["FECHA_CHARLA"] = charla.Fecha.ToString("dd/MM/yyyy"),
            ["HORA_CHARLA"] = charla.Fecha.ToString("HH:mm"),
            ["EXPOSITOR"] = charla.Expositor ?? "No especificado",
            ["DURACION"] = $"{charla.DuracionMinutos} minutos",
            ["DESCRIPCION"] = charla.Descripcion ?? "",
            ["LISTA_ASISTENTES"] = listaAsistentesMarkdown,
            ["TOTAL_ASISTENTES"] = charla.Asistencias.Count(a => a.Asistio).ToString(),
            ["FECHA_ACTUAL"] = DateTime.Now.ToString("dd/MM/yyyy")
        };
        
        // Buscar plantilla de charla del lugar
        var plantillaCharla = await _db.PlantillasLugarTrabajo
            .FirstOrDefaultAsync(p => p.LugarTrabajoId == charla.LugarTrabajoId && 
                                      p.TipoAplicacion == TipoAplicacionDocumento.Masivo &&
                                      p.Activo);
        
        if (plantillaCharla == null || string.IsNullOrEmpty(plantillaCharla.RutaPlantilla))
        {
            // Sin plantilla, retornar vacío (la UI debería manejar esto)
            return string.Empty;
        }
        
        // TODO: Integrar con DocumentoService para generar el acta
        // Por ahora retornamos la ruta de la plantilla
        return plantillaCharla.RutaPlantilla;
    }

    public async Task RegistrarAsistenciaAsync(int charlaId, int trabajadorId, bool asistio)
    {
        var asistencia = await _db.AsistenciasCharla
            .FirstOrDefaultAsync(a => a.CharlaId == charlaId && a.TrabajadorId == trabajadorId);
        
        if (asistencia != null)
        {
            asistencia.Asistio = asistio;
        }
        else
        {
            _db.AsistenciasCharla.Add(new AsistenciaCharla
            {
                CharlaId = charlaId,
                TrabajadorId = trabajadorId,
                Asistio = asistio
            });
        }
        
        await _db.SaveChangesAsync();
    }

    #endregion
}
