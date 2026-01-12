using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagleLabsGestorSST.Data.Entities;

public enum CategoriaDocumento
{
    Contrato = 0,
    Anexo = 1,
    Legal = 2, // Cédula, Finiquito
    SST = 3, // ODI, Entrega EPP
    Medico = 4, // Exámenes
    Capacitacion = 5,
    Otro = 9
}

public class DocumentoTrabajador
{
    [Key]
    public int Id { get; set; }

    public int TrabajadorId { get; set; }
    [ForeignKey("TrabajadorId")]
    public Trabajador Trabajador { get; set; } = null!;

    public string Nombre { get; set; } = string.Empty;
    public CategoriaDocumento Categoria { get; set; }
    
    public string RutaArchivo { get; set; } = string.Empty; // Ruta relativa o absoluta en el sistema de archivos
    public string Extension { get; set; } = string.Empty;
    
    public DateTime FechaSubida { get; set; } = DateTime.Now;
    
    public string? Observaciones { get; set; }
}
