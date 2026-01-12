using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using TagleLabsGestorSST.UI.ViewModels;

namespace TagleLabsGestorSST.UI.Views;

public partial class ImportacionView : UserControl
{
    public ImportacionView()
    {
        InitializeComponent();
    }

    private void BtnExaminar_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Archivos Excel (*.xlsx)|*.xlsx",
            Title = "Seleccionar archivo de importación"
        };

        if (dialog.ShowDialog() == true)
        {
            if (DataContext is ImportacionViewModel vm)
            {
                vm.RutaArchivo = dialog.FileName;
            }
        }
    }

    private void BtnExaminarEmpresas_Click(object sender, RoutedEventArgs e)
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Archivos Excel (*.xlsx)|*.xlsx",
            Title = "Seleccionar archivo de empresas"
        };

        if (dialog.ShowDialog() == true)
        {
            if (DataContext is ImportacionViewModel vm)
            {
                vm.RutaArchivoEmpresas = dialog.FileName;
            }
        }
    }
}
