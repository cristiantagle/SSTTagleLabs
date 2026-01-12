using System.IO;
using System.Text;

namespace TagleLabsGestorSST.Services;

public static class LegalContext
{
    public static string ObtenerContextoLegal()
    {
        var sb = new StringBuilder();
        sb.AppendLine("--- INICIO CONTEXTO LEGAL OBLIGATORIO (CHILE 2024-2025) ---");
        sb.AppendLine();

        // Intentar cargar archivos completos
        var legalDocsPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "LegalDocs");
        
        // Si estamos en desarrollo, buscar en la ruta del proyecto (hack temporal para depuración)
        if (!Directory.Exists(legalDocsPath))
        {
            legalDocsPath = @"c:\SST TagleLabs\LegalDocs";
        }

        if (Directory.Exists(legalDocsPath))
        {
            var karinPath = Path.Combine(legalDocsPath, "LeyKarin_Full.txt");
            if (File.Exists(karinPath))
            {
                sb.AppendLine("=== LEY KARIN (LEY 21.643) ===");
                sb.AppendLine(File.ReadAllText(karinPath));
                sb.AppendLine();
            }

            var ds44Path = Path.Combine(legalDocsPath, "DS44_Full.txt");
            if (File.Exists(ds44Path))
            {
                sb.AppendLine("=== DECRETO SUPREMO 44 (DS 44) ===");
                sb.AppendLine(File.ReadAllText(ds44Path));
                sb.AppendLine();
            }

            var ley40Path = Path.Combine(legalDocsPath, "Ley40Horas_2025.txt");
            if (File.Exists(ley40Path))
            {
                sb.AppendLine("=== LEY 40 HORAS (IMPLEMENTACIÓN 2025) ===");
                sb.AppendLine(File.ReadAllText(ley40Path));
                sb.AppendLine();
            }

            var leyConciliacionPath = Path.Combine(legalDocsPath, "LeyConciliacion_21645.txt");
            if (File.Exists(leyConciliacionPath))
            {
                sb.AppendLine("=== LEY CONCILIACIÓN (21.645) ===");
                sb.AppendLine(File.ReadAllText(leyConciliacionPath));
                sb.AppendLine();
            }

            var minsalPath = Path.Combine(legalDocsPath, "Protocolos_MINSAL.txt");
            if (File.Exists(minsalPath))
            {
                sb.AppendLine("=== PROTOCOLOS MINSAL OBLIGATORIOS (UV, RUIDO, PSICOSOCIAL, TMERT) ===");
                sb.AppendLine(File.ReadAllText(minsalPath));
                sb.AppendLine();
            }

            var teletrabajoPath = Path.Combine(legalDocsPath, "LeyTeletrabajo_21220.txt");
            if (File.Exists(teletrabajoPath))
            {
                sb.AppendLine("=== LEY TELETRABAJO Y DESCONEXIÓN (21.220) ===");
                sb.AppendLine(File.ReadAllText(teletrabajoPath));
                sb.AppendLine();
            }

            var inclusionPath = Path.Combine(legalDocsPath, "Inclusion_Ley21015.txt");
            if (File.Exists(inclusionPath))
            {
                sb.AppendLine("=== LEY INCLUSIÓN LABORAL (21.015) ===");
                sb.AppendLine(File.ReadAllText(inclusionPath));
                sb.AppendLine();
            }

            var sannaTeaPath = Path.Combine(legalDocsPath, "LeySanna_TEA.txt");
            if (File.Exists(sannaTeaPath))
            {
                sb.AppendLine("=== LEY SANNA Y LEY TEA (PROTECCIÓN FAMILIAR) ===");
                sb.AppendLine(File.ReadAllText(sannaTeaPath));
                sb.AppendLine();
            }

            var delitosPath = Path.Combine(legalDocsPath, "LeyDelitos_Alimentos.txt");
            if (File.Exists(delitosPath))
            {
                sb.AppendLine("=== LEY DELITOS ECONÓMICOS Y ALIMENTOS (CUMPLIMIENTO) ===");
                sb.AppendLine(File.ReadAllText(delitosPath));
                sb.AppendLine();
            }
        }
        else
        {
            sb.AppendLine("ADVERTENCIA: No se encontró la carpeta LegalDocs. Usando resumen de emergencia.");
            // Fallback content...
        }

        sb.AppendLine("--- FIN CONTEXTO LEGAL ---");
        return sb.ToString();
    }
}
