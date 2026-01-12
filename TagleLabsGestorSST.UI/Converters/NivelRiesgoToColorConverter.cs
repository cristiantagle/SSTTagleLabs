using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TagleLabsGestorSST.UI.Converters;

/// <summary>
/// Convierte el nivel de riesgo (Alto/Medio/Bajo) a un color de fondo
/// </summary>
public class NivelRiesgoToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string nivel && !string.IsNullOrWhiteSpace(nivel))
        {
            var nivelNorm = nivel.Trim().ToUpperInvariant();
            
            // Alto / Crítico / Muy Alto
            if (nivelNorm.Contains("ALTO") || nivelNorm.Contains("CRITICO") || nivelNorm.Contains("CRÍTICO"))
                return new SolidColorBrush(Color.FromRgb(220, 38, 38));   // Red-600
            
            // Medio / Moderado
            if (nivelNorm.Contains("MEDIO") || nivelNorm.Contains("MODERADO"))
                return new SolidColorBrush(Color.FromRgb(245, 158, 11)); // Amber-500
            
            // Bajo / Leve / Aceptable
            if (nivelNorm.Contains("BAJO") || nivelNorm.Contains("LEVE") || nivelNorm.Contains("ACEPTABLE"))
                return new SolidColorBrush(Color.FromRgb(16, 185, 129));  // Emerald-500
            
            // Pendiente
            if (nivelNorm.Contains("PENDIENTE"))
                return new SolidColorBrush(Color.FromRgb(100, 116, 139)); // Slate-500
        }
        return new SolidColorBrush(Color.FromRgb(100, 116, 139)); // Default gray
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
