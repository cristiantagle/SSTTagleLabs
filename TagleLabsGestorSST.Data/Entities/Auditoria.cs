namespace TagleLabsGestorSST.Data.Entities;

public class HistorialCambio
{
    public int Id { get; set; }
    public string Entidad { get; set; } = string.Empty;
    public string TipoCambio { get; set; } = string.Empty;
    public string? Usuario { get; set; }
    public DateTime Fecha { get; set; }
    public string? ValorAnterior { get; set; }
    public string? ValorNuevo { get; set; }
    public int? EmpresaId { get; set; }
    public Empresa? Empresa { get; set; }
}

public class AlertaConfiguracion
{
    public int Id { get; set; }
    public int EmpresaId { get; set; }
    public Empresa Empresa { get; set; } = null!;
    public string Nombre { get; set; } = string.Empty;
    public int DiasAnticipacion { get; set; }
    public bool Activa { get; set; } = true;
    public string TipoAlerta { get; set; } = string.Empty;
}
