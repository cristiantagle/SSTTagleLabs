using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagleLabsGestorSST.Data.Entities;

public enum TipoMovimientoVacaciones
{
    AbonoMensual = 0, // Devengo automático (1.25)
    CargoSolicitud = 1, // Días tomados
    AjusteManual = 2, // Corrección de saldo
    AbonoProgresivo = 3 // Días progresivos por antigüedad
}

public class MovimientoVacaciones
{
    [Key]
    public int Id { get; set; }

    public int TrabajadorId { get; set; }
    [ForeignKey("TrabajadorId")]
    public Trabajador Trabajador { get; set; } = null!;

    public DateTime FechaMovimiento { get; set; } = DateTime.Now;
    
    public TipoMovimientoVacaciones Tipo { get; set; }
    
    // Cantidad de días (positivo para abonos, negativo para cargos)
    [Column(TypeName = "decimal(18,2)")]
    public decimal Dias { get; set; }
    
    public string Descripcion { get; set; } = string.Empty;

    // Relación opcional con una solicitud específica
    public int? SolicitudVacacionesId { get; set; }
    [ForeignKey("SolicitudVacacionesId")]
    public SolicitudVacaciones? Solicitud { get; set; }
}
