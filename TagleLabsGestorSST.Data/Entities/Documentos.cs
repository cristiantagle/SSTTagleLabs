namespace TagleLabsGestorSST.Data.Entities;

public class PlantillaDocumento
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty; // Word o Excel
    public string RutaBase { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public ICollection<PlantillaCampo> Campos { get; set; } = new List<PlantillaCampo>();
    public ICollection<VersionDocumento> Versiones { get; set; } = new List<VersionDocumento>();
}

public class PlantillaCampo
{
    public int Id { get; set; }
    public int PlantillaDocumentoId { get; set; }
    public PlantillaDocumento PlantillaDocumento { get; set; } = null!;
    public string NombreCampo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public bool Obligatorio { get; set; }
}

public class DocumentoGenerado
{
    public int Id { get; set; }
    public int PlantillaDocumentoId { get; set; }
    public PlantillaDocumento Plantilla { get; set; } = null!;
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public int? TrabajadorId { get; set; }
    public Trabajador? Trabajador { get; set; }
    public int? CentroTrabajoId { get; set; }
    public CentroTrabajo? CentroTrabajo { get; set; }
    public DateTime FechaGenerado { get; set; }
    public string RutaArchivoEditable { get; set; } = string.Empty;
    public string RutaArchivoPdf { get; set; } = string.Empty;
    public string Version { get; set; } = "1.0";
    
    // Validación IA
    public int? ScoreCumplimiento { get; set; }
    public bool? AprobadoPorIA { get; set; }
    public string? ResumenValidacionIA { get; set; }
    public string? MejorasSugeridas { get; set; } // JSON array de mejoras
}

public class VersionDocumento
{
    public int Id { get; set; }
    public int PlantillaDocumentoId { get; set; }
    public PlantillaDocumento PlantillaDocumento { get; set; } = null!;
    public string Version { get; set; } = "1.0";
    public DateTime Fecha { get; set; }
    public string? Notas { get; set; }
}
