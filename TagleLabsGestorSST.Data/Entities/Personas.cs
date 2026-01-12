namespace TagleLabsGestorSST.Data.Entities;

public enum SensibilidadEspecial
{
    Ninguna = 0,
    Embarazo = 1,
    Discapacidad = 2,
    AdultoMayor = 3,
    EnfermedadCronica = 4,
    Otra = 9
}

public class Trabajador
{
    public int Id { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public DateTime FechaIngreso { get; set; }
    public string Cargo { get; set; } = string.Empty;
    public string? Email { get; set; }
    public bool Activo { get; set; } = true;
    public int DiasVacacionesUsados { get; set; } = 0;
    public string? FotoPath { get; set; }
    public DateTime FechaCreacion { get; set; } = DateTime.Now;
    public DateTime? FechaImportacion { get; set; }
    public string? Direccion { get; set; }
    public string? Comuna { get; set; }
    public string? EstadoCivil { get; set; }
    public DateTime? FechaNacimiento { get; set; }
    public string? AFP { get; set; }
    public string? SistemaSalud { get; set; }
    public int CentroTrabajoId { get; set; }
    public CentroTrabajo? CentroTrabajo { get; set; }
    public SensibilidadEspecial Sensibilidad { get; set; }
    public bool EsContratista { get; set; }
    public string? Observaciones { get; set; }
    public string? ProtocolosVigilancia { get; set; } // Comma-separated list: PREXOR, PLANESI, etc.
    public ICollection<Tarea> TareasAsignadas { get; set; } = new List<Tarea>();
    public ICollection<RolTrabajador> Roles { get; set; } = new List<RolTrabajador>();
    public ICollection<HistorialCambio> Historial { get; set; } = new List<HistorialCambio>();
}

public class Contratista
{
    public int Id { get; set; }
    public string NombreFantasia { get; set; } = string.Empty;
    public string Rut { get; set; } = string.Empty;
    public string Giro { get; set; } = string.Empty;
    public int? EmpresaPrincipalId { get; set; }
    public Empresa? EmpresaPrincipal { get; set; }
    public ICollection<Trabajador> Trabajadores { get; set; } = new List<Trabajador>();
}

public class RolInterno
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
}

public class RolTrabajador
{
    public int Id { get; set; }
    public int TrabajadorId { get; set; }
    public Trabajador Trabajador { get; set; } = null!;
    public int RolInternoId { get; set; }
    public RolInterno RolInterno { get; set; } = null!;
    public DateTime FechaAsignacion { get; set; } = DateTime.UtcNow;
}
