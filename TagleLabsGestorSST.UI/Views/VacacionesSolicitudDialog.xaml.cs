using System.Windows;
using TagleLabsGestorSST.UI.ViewModels;

namespace TagleLabsGestorSST.UI.Views;

public partial class VacacionesSolicitudDialog : Window
{
    public VacacionesSolicitudViewModel ViewModel { get; }

    public VacacionesSolicitudDialog(VacacionesSolicitudViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = viewModel;
    }

    private void CancelarClick(object sender, RoutedEventArgs e)
    {
        DialogResult = false;
        Close();
    }

    public void AbrirYObtenerDatos()
    {
        ShowDialog();
    }
}
