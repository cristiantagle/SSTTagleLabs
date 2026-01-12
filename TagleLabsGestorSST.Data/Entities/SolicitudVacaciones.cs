using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagleLabsGestorSST.Data.Entities;

public enum EstadoSolicitud
{
    Pendiente = 0,
    Aprobada = 1,
    Rechazada = 2,
    Anulada = 3
}

public class SolicitudVacaciones
{
    [Key]
    public int Id { get; set; }

    public int TrabajadorId { get; set; }
    [ForeignKey("TrabajadorId")]
    public Trabajador Trabajador { get; set; } = null!;

    public DateTime FechaSolicitud { get; set; } = DateTime.Now;
    public DateTime FechaInicio { get; set; }
    public DateTime FechaFin { get; set; }
    
    // Días hábiles descontados del saldo
    public decimal DiasHabiles { get; set; }

    public EstadoSolicitud Estado { get; set; } = EstadoSolicitud.Pendiente;
    
    public string? Observaciones { get; set; }
    
    // Fecha en que el trabajador debe regresar
    public DateTime FechaRetorno { get; set; }
}
