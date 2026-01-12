using System.Text;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

using UglyToad.PdfPig;

namespace TagleLabsGestorSST.Services;

public interface IKnowledgeIngestionService
{
    Task<int> IngestarDirectorioAsync(string directorioPath, CancellationToken ct = default);
    Task<int> ProcesarArchivoAsync(string filePath, CancellationToken ct = default);
}

public class KnowledgeIngestionService : IKnowledgeIngestionService
{
    private readonly TagleLabsContext _db;
    private readonly ILocalAiService _aiService;

    public KnowledgeIngestionService(TagleLabsContext db, ILocalAiService aiService)
    {
        _db = db;
        _aiService = aiService;
    }

    public async Task<int> IngestarDirectorioAsync(string directorioPath, CancellationToken ct = default)
    {
        if (!Directory.Exists(directorioPath)) return 0;

        var files = Directory.GetFiles(directorioPath, "*.*", SearchOption.AllDirectories)
            .Where(f => f.EndsWith(".txt") || f.EndsWith(".docx") || f.EndsWith(".md") || f.EndsWith(".pdf"))
            .ToList();

        int count = 0;
        foreach (var file in files)
        {
            if (ct.IsCancellationRequested) break;
            
            // Verificar si ya existe
            var relativePath = Path.GetRelativePath(directorioPath, file);
            if (await _db.KnowledgeItems.AnyAsync(k => k.SourceFile == relativePath, ct))
                continue;

            await ProcesarArchivoAsync(file, ct);
            count++;
        }

        return count;
    }

    public async Task<int> ProcesarArchivoAsync(string filePath, CancellationToken ct = default)
    {
        try
        {
            string content = "";
            string ext = Path.GetExtension(filePath).ToLower();

            if (ext == ".docx")
            {
                content = ExtractTextFromDocx(filePath);
            }
            else if (ext == ".pdf")
            {
                content = ExtractTextFromPdf(filePath);
            }
            else
            {
                content = await File.ReadAllTextAsync(filePath, Encoding.UTF8, ct);
            }

            if (string.IsNullOrWhiteSpace(content)) return 0;

            var item = new KnowledgeItem
            {
                SourceFile = Path.GetFileName(filePath),
                FileType = ext,
                Category = "General",
                Content = content,
                ProcessedDate = DateTime.Now
            };

            // Chunking
            var chunks = SplitTextIntoChunks(content, 1000, 200); // 1000 chars, 200 overlap
            int index = 0;
            foreach (var chunkText in chunks)
            {
                var embedding = await _aiService.GetEmbeddingAsync(chunkText);
                var embeddingJson = Newtonsoft.Json.JsonConvert.SerializeObject(embedding);

                item.Chunks.Add(new KnowledgeChunk
                {
                    Content = chunkText,
                    Embedding = embeddingJson,
                    ChunkIndex = index++
                });
            }

            _db.KnowledgeItems.Add(item);
            await _db.SaveChangesAsync(ct);
            return 1;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error ingestando {filePath}: {ex.Message}");
            return 0;
        }
    }

    private List<string> SplitTextIntoChunks(string text, int maxChunkSize, int overlap)
    {
        var chunks = new List<string>();
        if (string.IsNullOrWhiteSpace(text)) return chunks;

        // Limpieza básica
        text = System.Text.RegularExpressions.Regex.Replace(text, @"\s+", " ").Trim();

        for (int i = 0; i < text.Length; i += (maxChunkSize - overlap))
        {
            int length = Math.Min(maxChunkSize, text.Length - i);
            chunks.Add(text.Substring(i, length));
        }

        return chunks;
    }

    private string ExtractTextFromDocx(string filePath)
    {
        try
        {
            using var wordDoc = WordprocessingDocument.Open(filePath, false);
            var body = wordDoc.MainDocumentPart?.Document.Body;
            if (body == null) return "";

            var sb = new StringBuilder();
            foreach (var element in body.Elements())
            {
                if (element is Paragraph p)
                {
                    var text = p.InnerText.Trim();
                    if (string.IsNullOrEmpty(text)) continue;

                    // Detectar Estilos de Título
                    var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
                    if (!string.IsNullOrEmpty(styleId))
                    {
                        if (styleId.StartsWith("Heading1", StringComparison.OrdinalIgnoreCase) || styleId.StartsWith("Ttulo1", StringComparison.OrdinalIgnoreCase))
                            sb.AppendLine($"# {text}");
                        else if (styleId.StartsWith("Heading2", StringComparison.OrdinalIgnoreCase) || styleId.StartsWith("Ttulo2", StringComparison.OrdinalIgnoreCase))
                            sb.AppendLine($"## {text}");
                        else if (styleId.StartsWith("Heading3", StringComparison.OrdinalIgnoreCase) || styleId.StartsWith("Ttulo3", StringComparison.OrdinalIgnoreCase))
                            sb.AppendLine($"### {text}");
                        else
                            sb.AppendLine(text);
                    }
                    else
                    {
                        // Detectar Listas (Bullet points)
                        if (p.ParagraphProperties?.NumberingProperties != null)
                        {
                            sb.AppendLine($"- {text}");
                        }
                        else
                        {
                            sb.AppendLine(text);
                        }
                    }
                    
                    sb.AppendLine(); // Doble salto de línea para separar párrafos claramente
                }
                else if (element is Table t)
                {
                    // Extracción básica de tablas (fila por fila)
                    foreach (var row in t.Elements<TableRow>())
                    {
                        var cells = row.Elements<TableCell>().Select(c => c.InnerText.Trim());
                        sb.AppendLine("| " + string.Join(" | ", cells) + " |");
                    }
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error leyendo DOCX {filePath}: {ex.Message}");
            return "";
        }
    }

    private string ExtractTextFromPdf(string filePath)
    {
        try
        {
            using var pdf = PdfDocument.Open(filePath);
            var sb = new StringBuilder();
            foreach (var page in pdf.GetPages())
            {
                // PdfPig extrae texto plano, pero intentamos mantener separación de páginas
                var text = page.Text;
                if (!string.IsNullOrWhiteSpace(text))
                {
                    sb.AppendLine(text);
                    sb.AppendLine(); // Separador de página
                    sb.AppendLine("---"); // Separador visual de página
                    sb.AppendLine();
                }
            }
            return sb.ToString();
        }
        catch
        {
            return "";
        }
    }
}
