using System;
using System.ComponentModel.DataAnnotations;

namespace TagleLabsGestorSST.Data.Entities;

public class KnowledgeItem
{
    public int Id { get; set; }

    [Required]
    [MaxLength(500)]
    public string SourceFile { get; set; } = string.Empty; // Ruta relativa

    [MaxLength(100)]
    public string Category { get; set; } = string.Empty; // Ej: MIPER, PROTOCOLO, CHARLA

    [MaxLength(50)]
    public string FileType { get; set; } = string.Empty; // .pdf, .docx, .xlsx

    public string Content { get; set; } = string.Empty; // Texto extraído

    public string? Tags { get; set; } // Etiquetas autogeneradas

    public DateTime ProcessedDate { get; set; } = DateTime.Now;

    public ICollection<KnowledgeChunk> Chunks { get; set; } = new List<KnowledgeChunk>();
}
