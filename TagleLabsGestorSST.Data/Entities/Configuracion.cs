using System.ComponentModel.DataAnnotations;

namespace TagleLabsGestorSST.Data.Entities;

public class Configuracion
{
    public int Id { get; set; }
    
    [Required]
    public string Clave { get; set; } = string.Empty; // Ej: "MasterFolderPath"
    
    public string Valor { get; set; } = string.Empty; // Ej: "C:\SST_Master"
    
    public string? Descripcion { get; set; }
}
