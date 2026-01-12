using System.Windows;
using TagleLabsGestorSST.UI.ViewModels;

namespace TagleLabsGestorSST.UI.Views;

/// <summary>
/// Ventana independiente para MIPER - permite trabajar con más espacio
/// </summary>
public partial class MatrizRiesgosWindow : Window
{
    public MatrizRiesgosWindow(MatrizRiesgosViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }

    private void CloseButton_Click(object sender, RoutedEventArgs e)
    {
        this.Close();
    }
}
