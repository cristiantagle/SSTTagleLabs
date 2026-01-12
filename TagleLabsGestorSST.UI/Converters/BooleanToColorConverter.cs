using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace TagleLabsGestorSST.UI.Converters;

public class BooleanToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is bool isAvailable && isAvailable)
        {
            return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#10B981")); // Verde (Emerald 500)
        }
        return new SolidColorBrush((Color)ColorConverter.ConvertFromString("#EF4444")); // Rojo (Red 500)
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
