using System.Windows;
using System.Windows.Controls;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.Views;

public partial class TrabajadoresView : UserControl
{
    public TrabajadoresView()
    {
        InitializeComponent();
    }

    private void TxtRutTrabajador_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox txt)
        {
            ValidarRut(txt.Text);
        }
    }

    private void ValidarRut(string? rut)
    {
        if (string.IsNullOrWhiteSpace(rut))
        {
            IconRutTrabajadorValid.Visibility = Visibility.Collapsed;
            IconRutTrabajadorInvalid.Visibility = Visibility.Collapsed;
            TxtRutTrabajadorError.Visibility = Visibility.Collapsed;
            return;
        }

        var error = RutValidator.ObtenerMensajeError(rut);
        
        if (error == null)
        {
            // RUT válido
            IconRutTrabajadorValid.Visibility = Visibility.Visible;
            IconRutTrabajadorInvalid.Visibility = Visibility.Collapsed;
            TxtRutTrabajadorError.Visibility = Visibility.Collapsed;
        }
        else
        {
            // RUT inválido
            IconRutTrabajadorValid.Visibility = Visibility.Collapsed;
            IconRutTrabajadorInvalid.Visibility = Visibility.Visible;
            TxtRutTrabajadorError.Text = error;
            TxtRutTrabajadorError.Visibility = Visibility.Visible;
        }
    }
}

