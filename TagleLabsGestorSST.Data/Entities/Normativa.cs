namespace TagleLabsGestorSST.Data.Entities;

public class ArticuloDs44
{
    public int Id { get; set; }
    public string Numero { get; set; } = string.Empty;
    public string Titulo { get; set; } = string.Empty;
    public string Contenido { get; set; } = string.Empty;
}

public class ObligacionDs44
{
    public int Id { get; set; }
    public string Codigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public int ArticuloDs44Id { get; set; }
    public ArticuloDs44 Articulo { get; set; } = null!;
    public int? AplicaMinTrab { get; set; }
    public int? AplicaMaxTrab { get; set; }
    public string? RubroObjetivo { get; set; }
}

public enum EstadoObligacion
{
    Pendiente = 0,
    EnProgreso = 1,
    Cumplida = 2,
    Vencida = 3
}

public class ObligacionEmpresa
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public int? CentroTrabajoId { get; set; }
    public CentroTrabajo? CentroTrabajo { get; set; }
    public int ObligacionDs44Id { get; set; }
    public ObligacionDs44 Obligacion { get; set; } = null!;
    public EstadoObligacion Estado { get; set; } = EstadoObligacion.Pendiente;
    public DateTime? FechaLimite { get; set; }
    public DateTime? FechaCumplimiento { get; set; }
    public string? Observaciones { get; set; }
}
