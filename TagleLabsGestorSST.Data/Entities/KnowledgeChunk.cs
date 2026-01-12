using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TagleLabsGestorSST.Data.Entities;

public class KnowledgeChunk
{
    public int Id { get; set; }

    public int KnowledgeItemId { get; set; }
    
    [ForeignKey("KnowledgeItemId")]
    public KnowledgeItem KnowledgeItem { get; set; } = null!;

    public string Content { get; set; } = string.Empty; // El fragmento de texto

    public string Embedding { get; set; } = string.Empty; // JSON array de floats [0.1, 0.2, ...]

    public int ChunkIndex { get; set; }
}
