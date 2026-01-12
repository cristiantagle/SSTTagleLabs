namespace TagleLabsGestorSST.Data.Entities;

public enum TipoSiniestro
{
    Incidente = 0,
    Accidente = 1,
    Enfermedad = 2
}

public class Incidente
{
    public int Id { get; set; }
    public TipoSiniestro Tipo { get; set; }
    public DateTime Fecha { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? Causas { get; set; }
    public string? MedidasCorrectivas { get; set; }
    public bool MedidasImplementadas { get; set; }
    public int? TrabajadorId { get; set; }
    public Trabajador? Trabajador { get; set; }
    public int? CentroTrabajoId { get; set; }
    public CentroTrabajo? CentroTrabajo { get; set; }
    public ICollection<RegistroSiniestro> Registros { get; set; } = new List<RegistroSiniestro>();
}

public class RegistroSiniestro
{
    public int Id { get; set; }
    public int IncidenteId { get; set; }
    public Incidente Incidente { get; set; } = null!;
    public string Estado { get; set; } = string.Empty;
    public string? Comentarios { get; set; }
    public DateTime FechaRegistro { get; set; }
}

public class IndicadorSst
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string Formula { get; set; } = string.Empty;
    public string Valor { get; set; } = "0";
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public DateTime Fecha { get; set; }
}
