// Herramienta para generar plantillas de ejemplo
// Ejecutar: dotnet run
using ClosedXML.Excel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

Console.WriteLine("Generando plantillas de ejemplo para Lugares de Trabajo...");

var outputDir = Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "Templates", "LugaresTrabajo");
Directory.CreateDirectory(outputDir);

// ============================================
// 1. PLANTILLA WORD - ODI INDIVIDUAL
// ============================================
var wordPath = Path.Combine(outputDir, "ODI_LugarTrabajo_Plantilla.docx");
using (var doc = WordprocessingDocument.Create(wordPath, WordprocessingDocumentType.Document))
{
    var mainPart = doc.AddMainDocumentPart();
    mainPart.Document = new Document();
    var body = mainPart.Document.AppendChild(new Body());
    
    // Título
    body.AppendChild(CreateParagraph("OBLIGACIÓN DE INFORMAR (ODI)", true, "28"));
    body.AppendChild(CreateParagraph("Riesgos Laborales - DS 40 Art. 21", false, "14"));
    body.AppendChild(new Paragraph());
    
    // Datos lugar
    body.AppendChild(CreateParagraph("Lugar de Trabajo: {{LUGAR_TRABAJO}}", false, "12"));
    body.AppendChild(CreateParagraph("Fecha: {{FECHA_ACTUAL}}", false, "12"));
    body.AppendChild(new Paragraph());
    
    // Datos trabajador
    body.AppendChild(CreateParagraph("DATOS DEL TRABAJADOR", true, "14"));
    body.AppendChild(CreateParagraph("Nombre: {{NOMBRE_TRABAJADOR}}", false, "12"));
    body.AppendChild(CreateParagraph("RUT: {{RUT_TRABAJADOR}}", false, "12"));
    body.AppendChild(CreateParagraph("Cargo: {{CARGO}}", false, "12"));
    body.AppendChild(CreateParagraph("Empresa: {{EMPRESA}}", false, "12"));
    body.AppendChild(new Paragraph());
    
    // Contenido
    body.AppendChild(CreateParagraph("DECLARACIÓN", true, "14"));
    body.AppendChild(CreateParagraph(
        "Declaro haber sido informado(a) sobre los riesgos laborales a los que estaré expuesto(a) " +
        "en el lugar de trabajo {{LUGAR_TRABAJO}}, según lo establecido en el Decreto Supremo N° 40, " +
        "Artículo 21. He recibido instrucciones sobre las medidas preventivas y de control aplicables.",
        false, "11"));
    body.AppendChild(new Paragraph());
    body.AppendChild(new Paragraph());
    
    // Firmas
    body.AppendChild(CreateParagraph("_______________________          _______________________", false, "12"));
    body.AppendChild(CreateParagraph("     Firma Trabajador                    Firma Empleador", false, "10"));
    
    mainPart.Document.Save();
}
Console.WriteLine($"✓ Creada: {wordPath}");

// ============================================
// 2. PLANTILLA EXCEL - CHARLA DE SEGURIDAD
// ============================================
var excelPath = Path.Combine(outputDir, "Charla_SST_Plantilla.xlsx");
using (var workbook = new XLWorkbook())
{
    var ws = workbook.Worksheets.Add("Charla SST");
    
    // Encabezado
    ws.Cell("A1").Value = "REGISTRO DE CHARLA DE SEGURIDAD";
    ws.Cell("A1").Style.Font.Bold = true;
    ws.Cell("A1").Style.Font.FontSize = 16;
    ws.Range("A1:D1").Merge();
    
    ws.Cell("A3").Value = "Lugar de Trabajo:";
    ws.Cell("B3").Value = "{{LUGAR_TRABAJO}}";
    ws.Cell("B3").Style.Fill.BackgroundColor = XLColor.LightYellow;
    
    ws.Cell("A4").Value = "Tema de la Charla:";
    ws.Cell("B4").Value = "{{TEMA_CHARLA}}";
    ws.Cell("B4").Style.Fill.BackgroundColor = XLColor.LightYellow;
    
    ws.Cell("A5").Value = "Fecha:";
    ws.Cell("B5").Value = "{{FECHA_CHARLA}}";
    ws.Cell("B5").Style.Fill.BackgroundColor = XLColor.LightYellow;
    
    ws.Cell("C4").Value = "Expositor:";
    ws.Cell("D4").Value = "{{EXPOSITOR}}";
    ws.Cell("D4").Style.Fill.BackgroundColor = XLColor.LightYellow;
    
    ws.Cell("C5").Value = "Duración:";
    ws.Cell("D5").Value = "{{DURACION}}";
    ws.Cell("D5").Style.Fill.BackgroundColor = XLColor.LightYellow;
    
    // Tabla de asistentes
    ws.Cell("A7").Value = "REGISTRO DE ASISTENCIA";
    ws.Cell("A7").Style.Font.Bold = true;
    ws.Range("A7:D7").Merge();
    
    // Encabezados tabla
    ws.Cell("A8").Value = "N°";
    ws.Cell("B8").Value = "Nombre Completo";
    ws.Cell("C8").Value = "RUT";
    ws.Cell("D8").Value = "Firma";
    ws.Range("A8:D8").Style.Font.Bold = true;
    ws.Range("A8:D8").Style.Fill.BackgroundColor = XLColor.LightGray;
    ws.Range("A8:D8").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    
    // Filas para asistentes (20 filas de ejemplo)
    for (int i = 1; i <= 20; i++)
    {
        ws.Cell($"A{8+i}").Value = i;
        ws.Cell($"B{8+i}").Value = "";
        ws.Cell($"C{8+i}").Value = "";
        ws.Cell($"D{8+i}").Value = "";
        ws.Range($"A{8+i}:D{8+i}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }
    
    // Placeholder para lista automática (alternativa)
    ws.Cell("A30").Value = "LISTA AUTOMÁTICA (opcional):";
    ws.Cell("A31").Value = "{{LISTA_ASISTENTES}}";
    ws.Cell("A31").Style.Fill.BackgroundColor = XLColor.LightBlue;
    
    // Observaciones
    ws.Cell("A33").Value = "Observaciones:";
    ws.Range("A34:D36").Merge();
    ws.Range("A34:D36").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    
    // Total
    ws.Cell("A38").Value = "Total Asistentes:";
    ws.Cell("B38").Value = "{{TOTAL_ASISTENTES}}";
    ws.Cell("B38").Style.Fill.BackgroundColor = XLColor.LightYellow;
    
    // Ajustar anchos
    ws.Column("A").Width = 5;
    ws.Column("B").Width = 35;
    ws.Column("C").Width = 15;
    ws.Column("D").Width = 20;
    
    workbook.SaveAs(excelPath);
}
Console.WriteLine($"✓ Creada: {excelPath}");

// ============================================
// 3. PLANTILLA EXCEL - EPP (Entrega EPP)
// ============================================
var eppPath = Path.Combine(outputDir, "Entrega_EPP_Plantilla.xlsx");
using (var workbook = new XLWorkbook())
{
    var ws = workbook.Worksheets.Add("Entrega EPP");
    
    ws.Cell("A1").Value = "REGISTRO DE ENTREGA DE EPP";
    ws.Cell("A1").Style.Font.Bold = true;
    ws.Cell("A1").Style.Font.FontSize = 14;
    ws.Range("A1:E1").Merge();
    
    ws.Cell("A3").Value = "Lugar:";
    ws.Cell("B3").Value = "{{LUGAR_TRABAJO}}";
    
    ws.Cell("A4").Value = "Trabajador:";
    ws.Cell("B4").Value = "{{NOMBRE_TRABAJADOR}}";
    
    ws.Cell("C4").Value = "RUT:";
    ws.Cell("D4").Value = "{{RUT_TRABAJADOR}}";
    
    ws.Cell("A5").Value = "Cargo:";
    ws.Cell("B5").Value = "{{CARGO}}";
    
    ws.Cell("A6").Value = "Fecha:";
    ws.Cell("B6").Value = "{{FECHA_ACTUAL}}";
    
    // Tabla EPP
    ws.Cell("A8").Value = "N°";
    ws.Cell("B8").Value = "Elemento de Protección";
    ws.Cell("C8").Value = "Cantidad";
    ws.Cell("D8").Value = "Estado";
    ws.Cell("E8").Value = "Firma Recepción";
    ws.Range("A8:E8").Style.Font.Bold = true;
    ws.Range("A8:E8").Style.Fill.BackgroundColor = XLColor.LightGray;
    
    // EPP comunes
    var epps = new[] { "Casco de seguridad", "Lentes de seguridad", "Guantes", "Zapatos de seguridad", "Chaleco reflectante", "Protector auditivo", "Mascarilla", "Arnés de seguridad" };
    for (int i = 0; i < epps.Length; i++)
    {
        ws.Cell($"A{9+i}").Value = i + 1;
        ws.Cell($"B{9+i}").Value = epps[i];
        ws.Cell($"C{9+i}").Value = "";
        ws.Cell($"D{9+i}").Value = "Nuevo";
        ws.Cell($"E{9+i}").Value = "";
        ws.Range($"A{9+i}:E{9+i}").Style.Border.OutsideBorder = XLBorderStyleValues.Thin;
    }
    
    ws.Column("B").Width = 30;
    ws.Column("E").Width = 20;
    
    workbook.SaveAs(eppPath);
}
Console.WriteLine($"✓ Creada: {eppPath}");

Console.WriteLine("\n✓ Plantillas creadas exitosamente en: " + outputDir);
Console.WriteLine("\nPlaceholders disponibles:");
Console.WriteLine("  {{NOMBRE_TRABAJADOR}} - Nombre completo del trabajador");
Console.WriteLine("  {{RUT_TRABAJADOR}}    - RUT del trabajador");
Console.WriteLine("  {{CARGO}}             - Cargo del trabajador");
Console.WriteLine("  {{FECHA_ACTUAL}}      - Fecha de generación");
Console.WriteLine("  {{LUGAR_TRABAJO}}     - Nombre del lugar de trabajo");
Console.WriteLine("  {{EMPRESA}}           - Razón social de la empresa");
Console.WriteLine("  {{TEMA_CHARLA}}       - Tema de la charla (solo charlas)");
Console.WriteLine("  {{FECHA_CHARLA}}      - Fecha de la charla");
Console.WriteLine("  {{EXPOSITOR}}         - Nombre del expositor");
Console.WriteLine("  {{DURACION}}          - Duración en minutos");
Console.WriteLine("  {{LISTA_ASISTENTES}}  - Lista de asistentes (tabla)");
Console.WriteLine("  {{TOTAL_ASISTENTES}}  - Número total de asistentes");

// Helper
static Paragraph CreateParagraph(string text, bool bold, string fontSize)
{
    var run = new Run(new Text(text));
    var props = new RunProperties();
    if (bold) props.Bold = new Bold();
    props.FontSize = new FontSize { Val = fontSize };
    run.PrependChild(props);
    return new Paragraph(run);
}
