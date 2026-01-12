using System.Windows;
using TagleLabsGestorSST.UI.ViewModels;

namespace TagleLabsGestorSST.UI;

public partial class MainWindow : Window
{
    public MainWindow(MainViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
