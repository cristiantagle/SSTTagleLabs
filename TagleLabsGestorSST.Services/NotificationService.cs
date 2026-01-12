using System;

namespace TagleLabsGestorSST.Services;

public interface INotificationService
{
    void Show(string message);
    void ShowSuccess(string message);
    void ShowError(string message);
    void ShowWarning(string message);
}

public class NotificationService : INotificationService
{
    // Implementación simple que solo registra en consola/debug
    // La UI puede inyectar una implementación real
    public void Show(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[INFO] {message}");
    }

    public void ShowSuccess(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[SUCCESS] {message}");
    }

    public void ShowError(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[ERROR] {message}");
    }

    public void ShowWarning(string message)
    {
        System.Diagnostics.Debug.WriteLine($"[WARNING] {message}");
    }
}
