using System.Windows;
using System.Windows.Controls;
using TagleLabsGestorSST.UI.ViewModels;

namespace TagleLabsGestorSST.UI.Views;

public partial class MatrizRiesgosView : UserControl
{
    public MatrizRiesgosView()
    {
        InitializeComponent();
    }

    /// <summary>
    /// Abre MIPER en una ventana independiente maximizada para más espacio de trabajo
    /// </summary>
    private void ExpandirVentana_Click(object sender, RoutedEventArgs e)
    {
        if (DataContext is MatrizRiesgosViewModel viewModel)
        {
            var window = new MatrizRiesgosWindow(viewModel);
            window.ShowDialog();
        }
    }
}
