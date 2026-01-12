using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.UI.Converters;

public class EstadoToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is EstadoObligacion estado)
        {
            return estado switch
            {
                EstadoObligacion.Pendiente => new SolidColorBrush(Colors.OrangeRed),
                EstadoObligacion.EnProgreso => new SolidColorBrush(Colors.Orange),
                EstadoObligacion.Cumplida => new SolidColorBrush(Colors.SeaGreen),
                _ => new SolidColorBrush(Colors.Gray)
            };
        }
        return new SolidColorBrush(Colors.Gray);
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
