using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Net.Http;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class AsistenciaViewModel : ObservableObject
{
    [ObservableProperty]
    private string _asistenciaUrl = "https://agenda-pro-peach.vercel.app";

    [ObservableProperty]
    private bool _isLoading = true;

    [ObservableProperty]
    private bool _isServerAvailable = false;

    [ObservableProperty]
    private string _statusMessage = "Conectando con AsistenciaPro...";

    public AsistenciaViewModel()
    {
        _ = CheckServerStatusAsync();
    }

    private async Task CheckServerStatusAsync()
    {
        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(5);
            var response = await client.GetAsync(AsistenciaUrl);
            
            IsServerAvailable = response.IsSuccessStatusCode;
            StatusMessage = IsServerAvailable 
                ? "Conectado a AsistenciaPro" 
                : "AsistenciaPro no disponible";
        }
        catch
        {
            IsServerAvailable = false;
            StatusMessage = "No se pudo conectar a AsistenciaPro. Asegúrese de que el servidor esté ejecutándose.";
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task Refresh()
    {
        IsLoading = true;
        await CheckServerStatusAsync();
    }

    [RelayCommand]
    private void OpenInBrowser()
    {
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = AsistenciaUrl,
                UseShellExecute = true
            });
        }
        catch { }
    }
}
