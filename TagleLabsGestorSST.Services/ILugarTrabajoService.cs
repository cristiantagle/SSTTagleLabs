using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

/// <summary>
/// DTO para el estado de documentación de un trabajador en un lugar de trabajo
/// </summary>
public class EstadoDocumentacionDto
{
    public int TrabajadorId { get; set; }
    public string NombreTrabajador { get; set; } = string.Empty;
    public int TotalRequeridos { get; set; }
    public int TotalGenerados { get; set; }
    public int TotalVencidos { get; set; }
    public int TotalPendientes => TotalRequeridos - TotalGenerados;
    
    /// <summary>
    /// Estado general: Verde (100%), Amarillo (parcial), Rojo (0%)
    /// </summary>
    public string EstadoSemaforo => TotalRequeridos == 0 ? "Gris" :
        TotalGenerados == TotalRequeridos ? "Verde" :
        TotalGenerados > 0 ? "Amarillo" : "Rojo";
    
    public List<DocumentoRequeridoDto> Documentos { get; set; } = new();
}

public class DocumentoRequeridoDto
{
    public int Id { get; set; }
    public string CodigoPlantilla { get; set; } = string.Empty;
    public string NombreDocumento { get; set; } = string.Empty;
    public EstadoDocumentoRequerido Estado { get; set; }
    public DateTime? FechaGeneracion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public bool EsObligatorio { get; set; }
}

/// <summary>
/// Servicio para gestión de lugares de trabajo y documentos requeridos
/// </summary>
public interface ILugarTrabajoService
{
    // CRUD Lugares de Trabajo
    Task<List<LugarTrabajo>> GetAllAsync();
    Task<LugarTrabajo?> GetByIdAsync(int id);
    Task<LugarTrabajo> CreateAsync(LugarTrabajo lugar);
    Task UpdateAsync(LugarTrabajo lugar);
    Task DeleteAsync(int id);
    
    // Plantillas por Lugar
    Task<List<PlantillaLugarTrabajo>> GetPlantillasAsync(int lugarTrabajoId);
    Task<PlantillaLugarTrabajo> AddPlantillaAsync(PlantillaLugarTrabajo plantilla);
    Task UpdatePlantillaAsync(PlantillaLugarTrabajo plantilla);
    Task DeletePlantillaAsync(int plantillaId);
    
    // Asignaciones de Trabajadores
    Task AsignarTrabajadorAsync(int trabajadorId, int lugarTrabajoId);
    Task DesasignarTrabajadorAsync(int trabajadorId, int lugarTrabajoId);
    Task<List<Trabajador>> GetTrabajadoresAsignadosAsync(int lugarTrabajoId);
    Task<List<LugarTrabajo>> GetLugaresDeTrabajoAsync(int trabajadorId);
    
    // Estado de Documentación
    Task<EstadoDocumentacionDto> GetEstadoDocumentacionAsync(int trabajadorId, int lugarTrabajoId);
    Task<List<EstadoDocumentacionDto>> GetEstadoTodosLosTrabajadioresAsync(int lugarTrabajoId);
    
    // Generación de Documentos
    Task<DocumentoRequerido> GenerarDocumentoAsync(int trabajadorId, int plantillaId, string rutaDocumento);
    Task MarcarDocumentoVencidoAsync(int documentoRequeridoId);
    
    /// <summary>
    /// Genera documento real procesando placeholders en Word o Excel
    /// </summary>
    Task<string> GenerarDocumentoRealAsync(
        string rutaPlantilla, 
        Dictionary<string, string> datos,
        string? nombreBase = null,
        List<(string Nombre, string Cargo, string Rut)>? listaAsistentes = null);
    
    /// <summary>
    /// Detecta plantillas automáticamente desde carpetas Templates/{CodigoLugar}/
    /// </summary>
    Task<int> SincronizarPlantillasDesdeCarrpetaAsync(int lugarTrabajoId);
    
    // Charlas / Generación Masiva
    Task<CharlaSST> CrearCharlaAsync(CharlaSST charla, List<int> trabajadoresIds);
    Task<List<CharlaSST>> GetCharlasAsync(int lugarTrabajoId, DateTime? desde = null, DateTime? hasta = null);
    Task<string> GenerarActaCharlaAsync(int charlaId);
    Task RegistrarAsistenciaAsync(int charlaId, int trabajadorId, bool asistio);
}
