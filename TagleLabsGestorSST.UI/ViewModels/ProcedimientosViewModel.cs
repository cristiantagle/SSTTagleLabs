using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

/// <summary>
/// ViewModel para gestión de Procedimientos de Trabajo Seguro (PTS)
/// </summary>
public partial class ProcedimientosViewModel : ObservableObject
{
    private readonly ILocalAiService _aiService;
    private readonly INotificationService _notificationService;
    private readonly DocumentoService _documentoService;
    private readonly IEmpresaService _empresaService;

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();

    [ObservableProperty]
    private Empresa? _empresaSeleccionada;

    [ObservableProperty]
    private string _descripcionTarea = string.Empty;

    [ObservableProperty]
    private string _responsable = string.Empty;

    [ObservableProperty]
    private string _aprobador = string.Empty;

    [ObservableProperty]
    private string _contenidoProcedimiento = string.Empty;

    [ObservableProperty]
    private bool _isLoading = false;

    [ObservableProperty]
    private string _estadoProceso = string.Empty;

    public ProcedimientosViewModel(
        ILocalAiService aiService,
        INotificationService notificationService,
        DocumentoService documentoService,
        IEmpresaService empresaService)
    {
        _aiService = aiService;
        _notificationService = notificationService;
        _documentoService = documentoService;
        _empresaService = empresaService;

        _ = InicializarAsync();
    }

    private async Task InicializarAsync()
    {
        try
        {
            var empresas = await _empresaService.ObtenerEmpresasAsync();
            Empresas = new ObservableCollection<Empresa>(empresas);
            if (Empresas.Any())
            {
                EmpresaSeleccionada = Empresas.First();
            }
        }
        catch (Exception ex)
        {
            _notificationService?.ShowError($"Error cargando empresas: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task GenerarConIA()
    {
        if (string.IsNullOrWhiteSpace(DescripcionTarea))
        {
            _notificationService?.ShowWarning("Describe la tarea o proceso para generar el procedimiento");
            return;
        }

        try
        {
            IsLoading = true;
            EstadoProceso = "Generando procedimiento con IA...";

            var contexto = EmpresaSeleccionada != null 
                ? $"Empresa: {EmpresaSeleccionada.RazonSocial}" 
                : string.Empty;

            ContenidoProcedimiento = await _aiService.GenerarProcedimientoAsync(DescripcionTarea, contexto);
            
            EstadoProceso = "Procedimiento generado";
            _notificationService?.ShowSuccess("Procedimiento generado exitosamente");
        }
        catch (Exception ex)
        {
            EstadoProceso = "Error generando";
            _notificationService?.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private async Task GuardarProcedimiento()
    {
        if (EmpresaSeleccionada == null)
        {
            _notificationService?.ShowWarning("Seleccione una empresa");
            return;
        }

        if (string.IsNullOrWhiteSpace(ContenidoProcedimiento))
        {
            _notificationService?.ShowWarning("No hay contenido para guardar");
            return;
        }

        try
        {
            IsLoading = true;
            EstadoProceso = "Guardando procedimiento...";

            var datos = new Dictionary<string, string>
            {
                { "TITULO", DescripcionTarea },
                { "CONTENIDO_DINAMICO", ContenidoProcedimiento },
                { "RESPONSABLE", Responsable },
                { "APROBADOR", Aprobador }
            };

            var resultado = await _documentoService.GenerarDocumentoAsync(
                EmpresaSeleccionada.Id, 
                "PTS-STD", 
                datos, 
                null, 
                null);

            if (resultado != null)
            {
                var nombreArchivo = System.IO.Path.GetFileName(resultado.RutaArchivoEditable);
                var carpeta = System.IO.Path.GetDirectoryName(resultado.RutaArchivoEditable);
                
                EstadoProceso = $"Guardado: {nombreArchivo}";
                _notificationService?.ShowSuccess($"Procedimiento guardado:\n📄 {nombreArchivo}\n📁 {carpeta}");
                
                // Preguntar si desea abrir el archivo
                var abrirArchivo = System.Windows.MessageBox.Show(
                    $"Procedimiento guardado exitosamente:\n\n📄 {nombreArchivo}\n📁 {carpeta}\n\n¿Desea abrir el documento?",
                    "Procedimiento Guardado",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Information);
                
                if (abrirArchivo == System.Windows.MessageBoxResult.Yes && System.IO.File.Exists(resultado.RutaArchivoEditable))
                {
                    System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = resultado.RutaArchivoEditable,
                        UseShellExecute = true
                    });
                }
            }
            else
            {
                EstadoProceso = "Error al guardar";
                _notificationService?.ShowError("No se pudo guardar el procedimiento");
            }
        }
        catch (Exception ex)
        {
            EstadoProceso = "Error";
            _notificationService?.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsLoading = false;
        }
    }

    [RelayCommand]
    private void LimpiarFormulario()
    {
        DescripcionTarea = string.Empty;
        Responsable = string.Empty;
        Aprobador = string.Empty;
        ContenidoProcedimiento = string.Empty;
        EstadoProceso = string.Empty;
    }
}
