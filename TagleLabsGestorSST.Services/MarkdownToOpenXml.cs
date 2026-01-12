using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace TagleLabsGestorSST.Services;

public static class MarkdownToOpenXml
{
    /// <summary>
    /// Parses a markdown string and appends corresponding OpenXML elements to the Body.
    /// Supports: Headings (#, ##, ###), Bold (**), Italic (*), Bullet Lists (-), and basic Paragraphs.
    /// Also detects CAPÍTULO, TÍTULO, Artículo keywords for styling.
    /// </summary>
    /// <summary>
    /// Parses markdown and inserts elements directly into the document body after a placeholder.
    /// This ensures styles are properly created in the actual document.
    /// </summary>
    public static void ParseAndInsertAfter(Body body, OpenXmlElement insertAfterElement, string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return;

        // CRITICAL: Ensure heading styles exist in the REAL document
        EnsureHeadingStylesExist(body);

        var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
        var insertPoint = insertAfterElement;
        
        // Table accumulator
        var tableLines = new List<string>();
        bool inTable = false;

        for (int i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            var trimmed = line.Trim();
            
            // Check if this is a table row (starts and ends with |)
            bool isTableRow = trimmed.StartsWith("|") && trimmed.EndsWith("|");
            // Skip separator rows like |---|---|---|
            bool isSeparator = isTableRow && Regex.IsMatch(trimmed, @"^\|[\s\-:]+\|$");
            
            if (isTableRow && !isSeparator)
            {
                inTable = true;
                tableLines.Add(trimmed);
                continue;
            }
            
            if (isSeparator && inTable)
            {
                // Skip separator row
                continue;
            }
            
            // If we were in a table and now hit non-table content, flush the table
            if (inTable && !isTableRow)
            {
                if (tableLines.Count > 0)
                {
                    var table = CreateWordTable(tableLines);
                    insertPoint.InsertAfterSelf(table);
                    insertPoint = table;
                    tableLines.Clear();
                }
                inTable = false;
            }
            
            Paragraph? p = null;

            // 1. Markdown Headings (# ## ###)
            if (trimmed.StartsWith("#"))
            {
                var level = trimmed.TakeWhile(c => c == '#').Count();
                var text = trimmed.Substring(level).Trim();
                string styleId = level switch { 1 => "Heading1", 2 => "Heading2", 3 => "Heading3", _ => "Heading4" };
                p = CreateStyledParagraph(text, styleId, level);
            }
            // 2. Detect CAPÍTULO / TÍTULO / Artículo without markdown
            else if (IsCapituloLine(trimmed))
            {
                p = CreateStyledParagraph(trimmed, "Heading1", 1);
            }
            else if (IsTituloLine(trimmed))
            {
                p = CreateStyledParagraph(trimmed, "Heading2", 2);
            }
            else if (IsArticuloLine(trimmed))
            {
                p = CreateStyledParagraph(trimmed, "Heading3", 3);
            }
            // 3. Lists (Bulleted)
            else if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
            {
                var text = trimmed.Substring(2).Trim();
                p = CreateParagraphWithFormatting(text);
                var pPr = p.GetFirstChild<ParagraphProperties>() ?? new ParagraphProperties();
                if (p.GetFirstChild<ParagraphProperties>() == null) p.PrependChild(pPr);
                pPr.Indentation = new Indentation() { Left = "720", Hanging = "360" };
            }
            // 4. Numbered lists
            else if (Regex.IsMatch(trimmed, @"^\d+\.\s"))
            {
                var text = Regex.Replace(trimmed, @"^\d+\.\s*", "");
                p = CreateParagraphWithFormatting(text);
                var pPr = p.GetFirstChild<ParagraphProperties>() ?? new ParagraphProperties();
                if (p.GetFirstChild<ParagraphProperties>() == null) p.PrependChild(pPr);
                pPr.Indentation = new Indentation() { Left = "720", Hanging = "360" };
            }
            // 5. Empty lines
            else if (string.IsNullOrWhiteSpace(trimmed))
            {
                p = new Paragraph(new Run(new Text("")));
            }
            // 6. Normal paragraph
            else
            {
                p = CreateParagraphWithFormatting(trimmed);
            }

            if (p != null)
            {
                insertPoint.InsertAfterSelf(p);
                insertPoint = p;
            }
        }
        
        // Flush any remaining table
        if (tableLines.Count > 0)
        {
            var table = CreateWordTable(tableLines);
            insertPoint.InsertAfterSelf(table);
        }
    }

    /// <summary>
    /// Creates a Word table from Markdown table rows.
    /// </summary>
    private static Table CreateWordTable(List<string> tableLines)
    {
        var table = new Table();
        
        // Table properties - with borders and full width
        var tblPr = new TableProperties(
            new TableBorders(
                new TopBorder() { Val = BorderValues.Single, Size = 6, Color = "000000" },
                new BottomBorder() { Val = BorderValues.Single, Size = 6, Color = "000000" },
                new LeftBorder() { Val = BorderValues.Single, Size = 6, Color = "000000" },
                new RightBorder() { Val = BorderValues.Single, Size = 6, Color = "000000" },
                new InsideHorizontalBorder() { Val = BorderValues.Single, Size = 6, Color = "CCCCCC" },
                new InsideVerticalBorder() { Val = BorderValues.Single, Size = 6, Color = "CCCCCC" }
            ),
            new TableWidth() { Width = "5000", Type = TableWidthUnitValues.Pct } // 100% width
        );
        table.AppendChild(tblPr);
        
        bool isHeader = true;
        foreach (var line in tableLines)
        {
            // Parse cells: | cell1 | cell2 | cell3 |
            var cells = line.Split('|')
                .Where(c => !string.IsNullOrWhiteSpace(c))
                .Select(c => c.Trim())
                .ToList();
            
            var row = new TableRow();
            
            foreach (var cellText in cells)
            {
                var cell = new TableCell();
                
                // Cell properties
                var tcPr = new TableCellProperties();
                tcPr.Append(new TableCellVerticalAlignment() { Val = TableVerticalAlignmentValues.Center });
                
                if (isHeader)
                {
                    // Header cell styling - bold text with gray background
                    tcPr.Append(new Shading() { Fill = "E0E0E0", Val = ShadingPatternValues.Clear });
                }
                
                cell.Append(tcPr);
                
                // Cell content
                var para = new Paragraph();
                var run = new Run();
                
                if (isHeader)
                {
                    run.Append(new RunProperties(new Bold()));
                }
                
                // Remove markdown bold markers from text
                var cleanText = cellText.Replace("**", "");
                run.Append(new Text(cleanText) { Space = SpaceProcessingModeValues.Preserve });
                para.Append(run);
                cell.Append(para);
                
                row.Append(cell);
            }
            
            table.Append(row);
            isHeader = false; // Only first row is header
        }
        
        return table;
    }

    /// <summary>
    /// Post-processes all paragraphs in a body to ensure proper heading styles based on text content.
    /// Call this after inserting content to fix any missing styles.
    /// </summary>
    public static void PostProcessHeadingStyles(Body body)
    {
        EnsureHeadingStylesExist(body);

        foreach (var para in body.Descendants<Paragraph>().ToList())
        {
            var text = para.InnerText.Trim();
            if (string.IsNullOrEmpty(text)) continue;

            string? targetStyle = null;
            int level = 0;

            if (IsCapituloLine(text)) { targetStyle = "Heading1"; level = 1; }
            else if (IsTituloLine(text)) { targetStyle = "Heading2"; level = 2; }
            else if (IsArticuloLine(text)) { targetStyle = "Heading3"; level = 3; }

            if (targetStyle != null)
            {
                var pPr = para.GetFirstChild<ParagraphProperties>();
                if (pPr == null)
                {
                    pPr = new ParagraphProperties();
                    para.PrependChild(pPr);
                }

                // Set style
                pPr.ParagraphStyleId = new ParagraphStyleId() { Val = targetStyle };
                
                // Ensure KeepNext and KeepLines
                if (pPr.KeepNext == null) pPr.KeepNext = new KeepNext();
                if (pPr.KeepLines == null) pPr.KeepLines = new KeepLines();
                
                // Set outline level for TOC
                pPr.OutlineLevel = new OutlineLevel() { Val = level - 1 };
                
                // Add spacing if not present
                if (pPr.SpacingBetweenLines == null)
                {
                    pPr.SpacingBetweenLines = new SpacingBetweenLines() 
                    { 
                        Before = level == 1 ? "360" : "240", 
                        After = "120" 
                    };
                }
            }
        }
    }

    public static void ParseMarkdown(Body body, string markdown)
    {
        if (string.IsNullOrWhiteSpace(markdown)) return;

        // First, ensure heading styles exist in the document
        EnsureHeadingStylesExist(body);

        var lines = markdown.Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);

        foreach (var line in lines)
        {
            var trimmed = line.Trim();

            // 1. Markdown Headings (# ## ###)
            if (trimmed.StartsWith("#"))
            {
                var level = trimmed.TakeWhile(c => c == '#').Count();
                var text = trimmed.Substring(level).Trim();
                
                // Use standard Word Heading styles
                string styleId = level switch
                {
                    1 => "Heading1",
                    2 => "Heading2",
                    3 => "Heading3",
                    _ => "Heading4"
                };

                var p = CreateStyledParagraph(text, styleId, level);
                body.Append(p);
                continue;
            }

            // 2. Detect CAPÍTULO / TÍTULO / Artículo without markdown
            if (IsCapituloLine(trimmed))
            {
                var p = CreateStyledParagraph(trimmed, "Heading1", 1);
                body.Append(p);
                continue;
            }

            if (IsTituloLine(trimmed))
            {
                var p = CreateStyledParagraph(trimmed, "Heading2", 2);
                body.Append(p);
                continue;
            }

            if (IsArticuloLine(trimmed))
            {
                var p = CreateStyledParagraph(trimmed, "Heading3", 3);
                body.Append(p);
                continue;
            }

            // 3. Lists (Bulleted)
            if (trimmed.StartsWith("- ") || trimmed.StartsWith("* "))
            {
                var text = trimmed.Substring(2).Trim();
                var p = CreateParagraphWithFormatting(text);
                
                // Add bullet formatting
                var pPr = p.GetFirstChild<ParagraphProperties>() ?? new ParagraphProperties();
                if (p.GetFirstChild<ParagraphProperties>() == null)
                    p.PrependChild(pPr);

                pPr.NumberingProperties = new NumberingProperties(
                    new NumberingLevelReference() { Val = 0 },
                    new NumberingId() { Val = 1 }
                );
                pPr.Indentation = new Indentation() { Left = "720", Hanging = "360" };

                body.Append(p);
                continue;
            }

            // 4. Numbered lists (1. 2. 3.)
            if (Regex.IsMatch(trimmed, @"^\d+\.\s"))
            {
                var text = Regex.Replace(trimmed, @"^\d+\.\s*", "");
                var p = CreateParagraphWithFormatting(text);
                
                var pPr = p.GetFirstChild<ParagraphProperties>() ?? new ParagraphProperties();
                if (p.GetFirstChild<ParagraphProperties>() == null)
                    p.PrependChild(pPr);

                pPr.Indentation = new Indentation() { Left = "720", Hanging = "360" };
                body.Append(p);
                continue;
            }

            // 5. Empty lines = paragraph break
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                body.Append(new Paragraph(new Run(new Text(""))));
                continue;
            }

            // 6. Default: Normal Paragraph with bold/italic parsing
            body.Append(CreateParagraphWithFormatting(trimmed));
        }
    }

    private static bool IsCapituloLine(string text)
    {
        var upper = text.ToUpperInvariant();
        return upper.StartsWith("CAPÍTULO") || upper.StartsWith("CAPITULO") || 
               upper.StartsWith("PREÁMBULO") || upper.StartsWith("PREAMBULO") ||
               upper.StartsWith("ANEXO");
    }

    private static bool IsTituloLine(string text)
    {
        var upper = text.ToUpperInvariant();
        return upper.StartsWith("TÍTULO") || upper.StartsWith("TITULO");
    }

    private static bool IsArticuloLine(string text)
    {
        return text.StartsWith("Artículo") || text.StartsWith("ARTÍCULO") || 
               text.StartsWith("Articulo") || text.StartsWith("ARTICULO") ||
               Regex.IsMatch(text, @"^Art\.\s*\d+");
    }

    /// <summary>
    /// Ensures that Heading1, Heading2, Heading3 styles exist in the document.
    /// </summary>
    private static void EnsureHeadingStylesExist(Body body)
    {
        var doc = body.Ancestors<Document>().FirstOrDefault();
        if (doc == null) return;

        var mainPart = doc.MainDocumentPart;
        if (mainPart == null) return;

        var stylesPart = mainPart.StyleDefinitionsPart;
        if (stylesPart == null)
        {
            stylesPart = mainPart.AddNewPart<StyleDefinitionsPart>();
            stylesPart.Styles = new Styles();
        }

        var styles = stylesPart.Styles;
        if (styles == null) return;

        // Create Heading1 if not exists
        if (styles.Elements<Style>().All(s => s.StyleId?.Value != "Heading1"))
        {
            styles.Append(CreateHeadingStyle("Heading1", "Heading 1", "1", 28, true));
        }

        // Create Heading2 if not exists
        if (styles.Elements<Style>().All(s => s.StyleId?.Value != "Heading2"))
        {
            styles.Append(CreateHeadingStyle("Heading2", "Heading 2", "2", 24, true));
        }

        // Create Heading3 if not exists
        if (styles.Elements<Style>().All(s => s.StyleId?.Value != "Heading3"))
        {
            styles.Append(CreateHeadingStyle("Heading3", "Heading 3", "3", 22, false));
        }

        styles.Save();
    }

    private static Style CreateHeadingStyle(string styleId, string styleName, string outlineLevel, int fontSize, bool bold)
    {
        var style = new Style()
        {
            Type = StyleValues.Paragraph,
            StyleId = styleId,
            CustomStyle = true
        };

        style.Append(new StyleName() { Val = styleName });
        style.Append(new BasedOn() { Val = "Normal" });
        style.Append(new NextParagraphStyle() { Val = "Normal" });

        var pPr = new StyleParagraphProperties();
        pPr.Append(new OutlineLevel() { Val = int.Parse(outlineLevel) - 1 }); // 0-based for TOC
        pPr.Append(new SpacingBetweenLines() { Before = "240", After = "120" });
        pPr.Append(new KeepNext());
        style.Append(pPr);

        var rPr = new StyleRunProperties();
        rPr.Append(new FontSize() { Val = (fontSize * 2).ToString() }); // Half-points
        rPr.Append(new FontSizeComplexScript() { Val = (fontSize * 2).ToString() });
        if (bold)
        {
            rPr.Append(new Bold());
            rPr.Append(new BoldComplexScript());
        }
        style.Append(rPr);

        return style;
    }

    /// <summary>
    /// Creates a styled paragraph for headings with proper formatting.
    /// Includes KeepNext and KeepLines to prevent orphan headings at page breaks.
    /// </summary>
    private static Paragraph CreateStyledParagraph(string text, string styleId, int level)
    {
        var p = new Paragraph();
        var pPr = new ParagraphProperties();
        
        pPr.ParagraphStyleId = new ParagraphStyleId() { Val = styleId };
        
        // Add outline level for TOC generation
        pPr.OutlineLevel = new OutlineLevel() { Val = level - 1 }; // 0-based
        
        // CRITICAL: Prevent orphan headings - keep heading with next paragraph
        pPr.KeepNext = new KeepNext();
        pPr.KeepLines = new KeepLines(); // Keep all lines of the heading together
        
        // Add spacing before headings for better visual separation
        pPr.SpacingBetweenLines = new SpacingBetweenLines() 
        { 
            Before = level == 1 ? "360" : "240", // More space before H1
            After = "120" 
        };
        
        p.Append(pPr);

        // Parse bold/italic in heading text
        AppendFormattedRuns(p, text);

        return p;
    }

    /// <summary>
    /// Creates a paragraph with rich text parsing (Bold/Italic).
    /// </summary>
    private static Paragraph CreateParagraphWithFormatting(string text)
    {
        var p = new Paragraph();
        var pPr = new ParagraphProperties();
        pPr.ParagraphStyleId = new ParagraphStyleId() { Val = "Normal" };
        p.Append(pPr);

        AppendFormattedRuns(p, text);

        return p;
    }

    /// <summary>
    /// Parses text for **bold** and *italic* and appends runs to paragraph.
    /// </summary>
    private static void AppendFormattedRuns(Paragraph p, string text)
    {
        // Pattern matches **bold** or *italic*
        var pattern = @"(\*\*.*?\*\*)|(\*.*?\*)";
        var parts = Regex.Split(text, pattern);

        foreach (var part in parts)
        {
            if (string.IsNullOrEmpty(part)) continue;

            var run = new Run();
            var runProps = new RunProperties();
            string content = part;

            if (part.StartsWith("**") && part.EndsWith("**") && part.Length > 4)
            {
                runProps.Bold = new Bold();
                content = part.Substring(2, part.Length - 4);
            }
            else if (part.StartsWith("*") && part.EndsWith("*") && part.Length > 2)
            {
                runProps.Italic = new Italic();
                content = part.Substring(1, part.Length - 2);
            }

            run.Append(runProps);
            run.Append(new Text(content) { Space = SpaceProcessingModeValues.Preserve });
            p.Append(run);
        }
    }
}
