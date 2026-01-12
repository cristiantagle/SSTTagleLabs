namespace TagleLabsGestorSST.Data.Entities;

public class Empresa
{
    public int Id { get; set; }
    public string Rut { get; set; } = string.Empty;
    public string RazonSocial { get; set; } = string.Empty;
    public string Giro { get; set; } = string.Empty;
    public string? RubroPrincipal { get; set; } // Legacy string, keep for now or migrate
    public int? RubroId { get; set; }
    public Rubro? Rubro { get; set; }
    
    // Múltiples rubros para MIPER (relación N:N)
    public ICollection<Rubro> Rubros { get; set; } = new List<Rubro>();
    public int NumeroTrabajadores { get; set; }
    public string? Mutual { get; set; }
    public string? LogoPath { get; set; }
    public string? Direccion { get; set; }
    public string? Telefono { get; set; }
    public string? EmailContacto { get; set; }
    public string? RepresentanteLegal { get; set; }
    public ICollection<CentroTrabajo> CentrosTrabajo { get; set; } = new List<CentroTrabajo>();
    public ICollection<ObligacionEmpresa> Obligaciones { get; set; } = new List<ObligacionEmpresa>();
    public ICollection<DocumentoGenerado> DocumentosGenerados { get; set; } = new List<DocumentoGenerado>();
    public ICollection<HistorialCambio> Historial { get; set; } = new List<HistorialCambio>();
    public ICollection<AlertaConfiguracion> Alertas { get; set; } = new List<AlertaConfiguracion>();
    public ICollection<IndicadorSst> Indicadores { get; set; } = new List<IndicadorSst>();
}

public class CentroTrabajo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Direccion { get; set; }
    public string? Region { get; set; }
    public string? Ciudad { get; set; }
    public string? Sector { get; set; }
    public string? PlanoRiesgosPath { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public ICollection<Trabajador> Trabajadores { get; set; } = new List<Trabajador>();
    public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
    public ICollection<DocumentoGenerado> DocumentosGenerados { get; set; } = new List<DocumentoGenerado>();
    public ICollection<Incidente> Incidentes { get; set; } = new List<Incidente>();
    public ICollection<MatrizRiesgoEmpresa> MatrizRiesgos { get; set; } = new List<MatrizRiesgoEmpresa>();
}
