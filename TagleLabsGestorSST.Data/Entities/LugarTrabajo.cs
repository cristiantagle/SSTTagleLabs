namespace TagleLabsGestorSST.Data.Entities;

/// <summary>
/// Lugar de trabajo externo (cliente) como Agrosuper, Elecmetal, etc.
/// Cada lugar tiene sus propios requisitos de documentación.
/// </summary>
public class LugarTrabajo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty; // "Agrosuper", "Elecmetal"
    public string Codigo { get; set; } = string.Empty; // "AGS", "ELM"
    public string? LogoPath { get; set; }
    public string? Direccion { get; set; }
    public string? ContactoNombre { get; set; }
    public string? ContactoEmail { get; set; }
    public string? ContactoTelefono { get; set; }
    public bool Activo { get; set; } = true;
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // Navegación
    public ICollection<PlantillaLugarTrabajo> Plantillas { get; set; } = new List<PlantillaLugarTrabajo>();
    public ICollection<AsignacionTrabajador> Asignaciones { get; set; } = new List<AsignacionTrabajador>();
    public ICollection<CharlaSST> Charlas { get; set; } = new List<CharlaSST>();
}

/// <summary>
/// Tipo de aplicación del documento
/// </summary>
public enum TipoAplicacionDocumento
{
    Individual = 0,  // Se genera por cada trabajador
    Masivo = 1       // Se genera para múltiples trabajadores (ej: charlas)
}

/// <summary>
/// Estado de un documento requerido
/// </summary>
public enum EstadoDocumentoRequerido
{
    Pendiente = 0,
    Generado = 1,
    Vencido = 2
}

/// <summary>
/// Plantilla de documento requerida por un lugar de trabajo.
/// Define qué documentos debe tener cada trabajador asignado a ese lugar.
/// </summary>
public class PlantillaLugarTrabajo
{
    public int Id { get; set; }
    public int LugarTrabajoId { get; set; }
    public LugarTrabajo LugarTrabajo { get; set; } = null!;
    
    public string Codigo { get; set; } = string.Empty;        // "AGS-ODI", "AGS-CHARLA"
    public string NombreDocumento { get; set; } = string.Empty; // "ODI Agrosuper", "Charla Mensual"
    public string? Descripcion { get; set; }
    public string? RutaPlantilla { get; set; }                 // path/to/template.docx
    public TipoAplicacionDocumento TipoAplicacion { get; set; } = TipoAplicacionDocumento.Individual;
    public bool EsObligatorio { get; set; } = true;
    public int? DiasVigencia { get; set; }                     // null = no vence, 365 = anual, etc.
    public int Orden { get; set; } = 0;                        // Para ordenar en UI
    public bool Activo { get; set; } = true;
    
    // Navegación
    public ICollection<DocumentoRequerido> DocumentosGenerados { get; set; } = new List<DocumentoRequerido>();
}

/// <summary>
/// Asignación de un trabajador a un lugar de trabajo.
/// Un trabajador puede estar asignado a múltiples lugares.
/// </summary>
public class AsignacionTrabajador
{
    public int Id { get; set; }
    public int TrabajadorId { get; set; }
    public Trabajador Trabajador { get; set; } = null!;
    public int LugarTrabajoId { get; set; }
    public LugarTrabajo LugarTrabajo { get; set; } = null!;
    
    public DateTime FechaAsignacion { get; set; } = DateTime.Now;
    public bool Activo { get; set; } = true;
}

/// <summary>
/// Registro de un documento requerido para un trabajador específico.
/// Vincula al trabajador con una plantilla del lugar de trabajo.
/// </summary>
public class DocumentoRequerido
{
    public int Id { get; set; }
    public int TrabajadorId { get; set; }
    public Trabajador Trabajador { get; set; } = null!;
    public int PlantillaLugarTrabajoId { get; set; }
    public PlantillaLugarTrabajo PlantillaLugarTrabajo { get; set; } = null!;
    
    public string? RutaDocumentoGenerado { get; set; }
    public DateTime? FechaGeneracion { get; set; }
    public DateTime? FechaVencimiento { get; set; }
    public EstadoDocumentoRequerido Estado { get; set; } = EstadoDocumentoRequerido.Pendiente;
    public string? Observaciones { get; set; }
}

/// <summary>
/// Charla de seguridad que aplica a múltiples trabajadores.
/// </summary>
public class CharlaSST
{
    public int Id { get; set; }
    public int LugarTrabajoId { get; set; }
    public LugarTrabajo LugarTrabajo { get; set; } = null!;
    
    public string Tema { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public DateTime Fecha { get; set; }
    public string? Expositor { get; set; }
    public int DuracionMinutos { get; set; } = 30;
    public string? RutaActaFirmada { get; set; }              // Acta con firmas escaneada
    public string? RutaDocumentoGenerado { get; set; }        // Acta generada por el sistema
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    
    // Navegación
    public ICollection<AsistenciaCharla> Asistencias { get; set; } = new List<AsistenciaCharla>();
}

/// <summary>
/// Registro de asistencia de un trabajador a una charla.
/// </summary>
public class AsistenciaCharla
{
    public int Id { get; set; }
    public int CharlaId { get; set; }
    public CharlaSST Charla { get; set; } = null!;
    public int TrabajadorId { get; set; }
    public Trabajador Trabajador { get; set; } = null!;
    
    public bool Asistio { get; set; } = true;
    public DateTime FechaRegistro { get; set; } = DateTime.Now;
    public string? Observaciones { get; set; }
}
