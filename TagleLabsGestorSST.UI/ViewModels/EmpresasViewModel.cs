using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.IO;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class EmpresasViewModel : ObservableObject
{
    private readonly IEmpresaService _empresaService;
    private readonly INotificationService _notificationService;
    private readonly IMatrizRiesgoService _matrizService;

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();

    [ObservableProperty]
    private ObservableCollection<Rubro> _rubros = new();

    [ObservableProperty]
    private Empresa? _empresaSeleccionada;

    [ObservableProperty]
    private bool _esEdicion;

    public EmpresasViewModel(IEmpresaService empresaService, INotificationService notificationService, IMatrizRiesgoService matrizService)
    {
        _empresaService = empresaService;
        _notificationService = notificationService;
        _matrizService = matrizService;
        CargarEmpresasCommand.Execute(null);
    }

    partial void OnEmpresaSeleccionadaChanged(Empresa? value)
    {
        EsEdicion = value != null;
        if (value == null)
        {
            // Limpiar o preparar para nuevo
        }
    }

    [RelayCommand]
    private async Task CargarEmpresas()
    {
        var lista = await _empresaService.ObtenerEmpresasAsync();
        Empresas = new ObservableCollection<Empresa>(lista);

        var rubros = await _matrizService.ObtenerRubrosAsync();
        Rubros = new ObservableCollection<Rubro>(rubros);
    }

    [RelayCommand]
    private void NuevaEmpresa()
    {
        EmpresaSeleccionada = new Empresa { Rut = "", RazonSocial = "Nueva Empresa" };
        EsEdicion = true;
    }

    [RelayCommand]
    private async Task GuardarEmpresa()
    {
        if (EmpresaSeleccionada == null) return;

        if (string.IsNullOrWhiteSpace(EmpresaSeleccionada.RazonSocial))
        {
            _notificationService.ShowWarning("Debe ingresar la Razón Social.");
            return;
        }

        if (string.IsNullOrWhiteSpace(EmpresaSeleccionada.Rut))
        {
            _notificationService.ShowWarning("Debe ingresar el RUT.");
            return;
        }

        var esNuevaEmpresa = EmpresaSeleccionada.Id == 0;
        
        if (esNuevaEmpresa)
        {
            // NOTA: CrearEmpresaAsync ya genera obligaciones automáticamente via _obligacionesService.AsignarObligacionesAsync()
            await _empresaService.CrearEmpresaAsync(EmpresaSeleccionada);
        }
        else
        {
            await _empresaService.ActualizarEmpresaAsync(EmpresaSeleccionada);
        }

        await CargarEmpresas();
        
        if (esNuevaEmpresa)
        {
            _notificationService.ShowSuccess("Empresa creada. Se han asignado las obligaciones DS44 aplicables automáticamente.");
        }
        else
        {
            _notificationService.ShowSuccess("Empresa actualizada correctamente.");
        }
    }

    [RelayCommand]
    private async Task EliminarEmpresa()
    {
        if (EmpresaSeleccionada == null || EmpresaSeleccionada.Id == 0) return;

        var result = System.Windows.MessageBox.Show(
            $"¿Está seguro de que desea eliminar la empresa '{EmpresaSeleccionada.RazonSocial}'?\n\nEsta acción eliminará también todos sus trabajadores, documentos y registros asociados.",
            "Confirmar Eliminación",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            await _empresaService.EliminarEmpresaAsync(EmpresaSeleccionada.Id);
            EmpresaSeleccionada = null;
            await CargarEmpresas();
        }
    }

    [RelayCommand]
    private void SubirLogo()
    {
        if (EmpresaSeleccionada == null) return;

        var openFileDialog = new OpenFileDialog
        {
            Filter = "Imágenes|*.png;*.jpg;*.jpeg;*.bmp"
        };

        if (openFileDialog.ShowDialog() == true)
        {
            try
            {
                // Crear directorio de datos local si no existe
                var appDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "Logos");
                Directory.CreateDirectory(appDataPath);

                var fileInfo = new FileInfo(openFileDialog.FileName);
                var newFileName = $"{Guid.NewGuid()}{fileInfo.Extension}";
                var destinationPath = Path.Combine(appDataPath, newFileName);

                File.Copy(openFileDialog.FileName, destinationPath, true);

                // Guardar ruta relativa o absoluta local
                EmpresaSeleccionada.LogoPath = destinationPath;
                OnPropertyChanged(nameof(EmpresaSeleccionada));
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error al guardar la imagen: {ex.Message}");
            }
        }
    }
}
