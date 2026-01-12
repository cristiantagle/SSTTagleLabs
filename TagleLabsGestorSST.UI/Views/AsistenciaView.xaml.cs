using System.IO;
using System.Windows.Controls;
using Microsoft.Web.WebView2.Core;

namespace TagleLabsGestorSST.UI.Views;

public partial class AsistenciaView : UserControl
{
    public AsistenciaView()
    {
        InitializeComponent();
        InitializeWebView();
    }

    private async void InitializeWebView()
    {
        try
        {
            // Configurar directorio de datos persistentes para guardar cookies, sesión y credenciales
            var userDataFolder = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "TagleLabsGestorSST",
                "WebView2Data"
            );
            
            // Crear el Environment con carpeta de datos persistente
            var environment = await CoreWebView2Environment.CreateAsync(
                userDataFolder: userDataFolder
            );
            
            // Inicializar WebView2 con el environment persistente
            await WebView.EnsureCoreWebView2Async(environment);
            
            // Habilitar guardado de contraseñas
            WebView.CoreWebView2.Settings.IsPasswordAutosaveEnabled = true;
            WebView.CoreWebView2.Settings.IsGeneralAutofillEnabled = true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[WebView2] Error al inicializar: {ex.Message}");
        }
    }
}

