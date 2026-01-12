using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TagleLabsGestorSST.Services;

public static class OpenXmlToMarkdown
{
    public static string ParseDocx(Body body)
    {
        if (body == null) return string.Empty;

        var sb = new StringBuilder();

        foreach (var element in body.Elements())
        {
            if (element is Paragraph p)
            {
                sb.AppendLine(ParseParagraph(p));
            }
            else if (element is Table t)
            {
                sb.AppendLine(ParseTable(t));
            }
        }

        return sb.ToString();
    }

    private static string ParseParagraph(Paragraph p)
    {
        var sb = new StringBuilder();
        
        // 1. Check for Headings
        var styleId = p.ParagraphProperties?.ParagraphStyleId?.Val?.Value;
        if (!string.IsNullOrEmpty(styleId) && styleId.StartsWith("Heading", StringComparison.OrdinalIgnoreCase))
        {
            if (int.TryParse(styleId.Substring(7), out int level))
            {
                sb.Append(new string('#', Math.Min(6, level)) + " ");
            }
            else
            {
                // Fallback for "Heading 1" etc if styleId is just "Heading"
                sb.Append("## ");
            }
        }
        else if (!string.IsNullOrEmpty(styleId) && styleId.StartsWith("Titulo", StringComparison.OrdinalIgnoreCase))
        {
             if (int.TryParse(styleId.Substring(6), out int level))
            {
                sb.Append(new string('#', Math.Min(6, level)) + " ");
            }
            else
            {
                sb.Append("## ");
            }
        }

        // 2. Check for Lists
        var numPr = p.ParagraphProperties?.NumberingProperties;
        if (numPr != null)
        {
            // Simple list detection - assume bullet or numbered
            // Ideally we'd check NumberingId and Level, but for now just use a dash
            sb.Append("- ");
        }

        // 3. Parse Runs (Bold/Italic)
        foreach (var run in p.Descendants<Run>())
        {
            var text = run.InnerText;
            if (string.IsNullOrEmpty(text)) continue;

            var rPr = run.RunProperties;
            bool isBold = rPr?.Bold != null;
            bool isItalic = rPr?.Italic != null;

            if (isBold && isItalic) sb.Append($"***{text}***");
            else if (isBold) sb.Append($"**{text}**");
            else if (isItalic) sb.Append($"*{text}*");
            else sb.Append(text);
        }

        return sb.ToString();
    }

    private static string ParseTable(Table t)
    {
        var sb = new StringBuilder();
        var rows = t.Elements<TableRow>().ToList();
        if (!rows.Any()) return string.Empty;

        // Determine max columns to normalize table
        int maxCols = rows.Max(r => r.Elements<TableCell>().Count());

        // Process Header (First Row)
        var headerRow = rows.First();
        sb.AppendLine(ParseTableRow(headerRow, maxCols));

        // Separator Row
        sb.Append("|");
        for (int i = 0; i < maxCols; i++) sb.Append("---|");
        sb.AppendLine();

        // Process Data Rows
        foreach (var row in rows.Skip(1))
        {
            sb.AppendLine(ParseTableRow(row, maxCols));
        }

        return sb.ToString();
    }

    private static string ParseTableRow(TableRow row, int targetCols)
    {
        var sb = new StringBuilder();
        sb.Append("|");
        
        var cells = row.Elements<TableCell>().ToList();
        int currentCols = 0;

        foreach (var cell in cells)
        {
            // Extract text from cell paragraphs
            var cellText = string.Join(" ", cell.Descendants<Paragraph>().Select(p => p.InnerText));
            // Escape pipes to avoid breaking markdown table
            cellText = cellText.Replace("|", "\\|"); 
            sb.Append($" {cellText} |");
            currentCols++;
        }

        // Fill missing columns if any
        while (currentCols < targetCols)
        {
            sb.Append(" |");
            currentCols++;
        }

        return sb.ToString();
    }
}
