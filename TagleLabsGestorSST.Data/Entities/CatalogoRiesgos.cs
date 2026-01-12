namespace TagleLabsGestorSST.Data.Entities;

public class Rubro
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public ICollection<Actividad> Actividades { get; set; } = new List<Actividad>();
    
    // Relación N:N con Empresas
    public ICollection<Empresa> Empresas { get; set; } = new List<Empresa>();
}

public class Actividad
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int RubroId { get; set; }
    public Rubro Rubro { get; set; } = null!;
    public ICollection<Tarea> Tareas { get; set; } = new List<Tarea>();
}

public class Tarea
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? Descripcion { get; set; }
    public int ActividadId { get; set; }
    public Actividad Actividad { get; set; } = null!;
    public ICollection<TareaPeligroControl> Relaciones { get; set; } = new List<TareaPeligroControl>();
    public ICollection<Trabajador> Trabajadores { get; set; } = new List<Trabajador>();
    public ICollection<Equipo> Equipos { get; set; } = new List<Equipo>();
}

public class Peligro
{
    public int Id { get; set; }
    public string Categoria { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public string? CodigoSuseso { get; set; } // Nuevo campo para codificación oficial
    public ICollection<TareaPeligroControl> Relaciones { get; set; } = new List<TareaPeligroControl>();
}

public enum JerarquiaControl
{
    Eliminacion = 1,
    Sustitucion = 2,
    Ingenieria = 3,
    Administrativo = 4,
    EPP = 5
}

public class Control
{
    public int Id { get; set; }
    public string Tipo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public JerarquiaControl Jerarquia { get; set; } = JerarquiaControl.Administrativo; // Default
    public ICollection<TareaPeligroControl> Relaciones { get; set; } = new List<TareaPeligroControl>();
}

public class TareaPeligroControl
{
    public int Id { get; set; }
    public int TareaId { get; set; }
    public Tarea Tarea { get; set; } = null!;
    public int PeligroId { get; set; }
    public Peligro Peligro { get; set; } = null!;
    public int ControlId { get; set; }
    public Control Control { get; set; } = null!;
    public int Probabilidad { get; set; }
    public int Consecuencia { get; set; }
    public int NivelRiesgo { get; set; }
    public bool Aplicable { get; set; } = true;
    public string? Observaciones { get; set; }
}

public class Equipo
{
    public int Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string Tipo { get; set; } = string.Empty;
    public DateTime FechaAdquisicion { get; set; }
    public DateTime? FechaMantenimientoProgramado { get; set; }
    public string? InstruccionesSeguridad { get; set; }
    public int? CentroTrabajoId { get; set; }
    public CentroTrabajo? CentroTrabajo { get; set; }
    public ICollection<Tarea> TareasRelacionadas { get; set; } = new List<Tarea>();
}

public class MatrizRiesgoEmpresa
{
    public int Id { get; set; }
    public int CentroTrabajoId { get; set; }
    public CentroTrabajo CentroTrabajo { get; set; } = null!;
    
    public int? TareaId { get; set; }
    public Tarea? Tarea { get; set; }
    
    public int PeligroId { get; set; }
    public Peligro Peligro { get; set; } = null!;
    
    public int? ControlId { get; set; }
    public Control? Control { get; set; }
    
    public string MedidaControlEspecifica { get; set; } = string.Empty;

    public int Probabilidad { get; set; }
    public int Consecuencia { get; set; }
    // Propiedad calculada simple, no mapeada a BD si se prefiere, pero útil tenerla
    public int ValorRiesgo => Probabilidad * Consecuencia;
    public string NivelRiesgo { get; set; } = "Pendiente"; 
}
