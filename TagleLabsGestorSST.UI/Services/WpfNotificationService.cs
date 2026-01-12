using System.Windows;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.Services;

/// <summary>
/// Implementación de INotificationService para WPF usando MessageBox
/// </summary>
public class WpfNotificationService : INotificationService
{
    public void Show(string message)
    {
        MessageBox.Show(message, "Información", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowSuccess(string message)
    {
        MessageBox.Show(message, "Éxito", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public void ShowError(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public void ShowWarning(string message)
    {
        MessageBox.Show(message, "Advertencia", MessageBoxButton.OK, MessageBoxImage.Warning);
    }
}
