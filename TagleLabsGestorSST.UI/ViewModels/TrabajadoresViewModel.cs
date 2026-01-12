using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using ClosedXML.Excel;
using Microsoft.Win32;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class CheckableItem : ObservableObject
{
    [ObservableProperty]
    private string _name = string.Empty;

    [ObservableProperty]
    private string _description = string.Empty;

    [ObservableProperty]
    private bool _isChecked;
}

public partial class TrabajadoresViewModel : ObservableObject
{
    private readonly ITrabajadorService _trabajadorService;
    private readonly IVacacionesService _vacacionesService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private ObservableCollection<Trabajador> _trabajadores = new();

    [ObservableProperty]
    private ObservableCollection<CentroTrabajo> _centrosTrabajo = new();

    [ObservableProperty]
    private Trabajador? _trabajadorSeleccionado;

    [ObservableProperty]
    private bool _esEdicion;

    [ObservableProperty]
    private ObservableCollection<CheckableItem> _protocolosList = new();

    // --- RRHH: Vacaciones ---
    [ObservableProperty]
    private decimal _diasGanados;

    [ObservableProperty]
    private decimal _diasUsados;

    [ObservableProperty]
    private decimal _saldoVacaciones;

    [ObservableProperty]
    private ObservableCollection<SolicitudVacaciones> _historialVacaciones = new();

    [ObservableProperty]
    private SolicitudVacaciones _nuevaSolicitud = new();
    
    // --- RRHH: Carpeta Digital ---
    [ObservableProperty]
    private ObservableCollection<DocumentoTrabajador> _documentosTrabajador = new();

    // --- NUEVO: Reporte Cotizaciones ---
    [ObservableProperty]
    private int _mesReporte = DateTime.Now.Month;

    [ObservableProperty]
    private int _añoReporte = DateTime.Now.Year;

    public List<int> MesesDisponibles { get; } = Enumerable.Range(1, 12).ToList();
    public List<int> AñosDisponibles { get; } = Enumerable.Range(DateTime.Now.Year - 5, 6).ToList();

    public TrabajadoresViewModel(ITrabajadorService trabajadorService, IVacacionesService vacacionesService, INotificationService notificationService)
    {
        _trabajadorService = trabajadorService;
        _vacacionesService = vacacionesService;
        _notificationService = notificationService;
        
        InicializarProtocolos();
        InicializarProtocolos();
        // Fire and forget safe execution
        _ = CargarDatos();
    }

    private void InicializarProtocolos()
    {
        ProtocolosList = new ObservableCollection<CheckableItem>
        {
            new CheckableItem { Name = "PREXOR", Description = "Ruido Ocupacional" },
            new CheckableItem { Name = "PLANESI", Description = "Sílice (Erradicación Silicosis)" },
            new CheckableItem { Name = "TMERT-EESS", Description = "Trastornos Musculoesqueléticos" },
            new CheckableItem { Name = "MMC", Description = "Manejo Manual de Cargas (>25kg)" },
            new CheckableItem { Name = "UV", Description = "Radiación UV Solar" },
            new CheckableItem { Name = "Psicosocial", Description = "Riesgos Psicosociales (ISTAS21)" },
            new CheckableItem { Name = "Citostáticos", Description = "Exposición a Drogas Antineoplásicas" },
            new CheckableItem { Name = "Hipobaria", Description = "Gran Altitud Geográfica" }
        };
    }

    partial void OnTrabajadorSeleccionadoChanged(Trabajador? value)
    {
        EsEdicion = value != null;
        
        // Reset checks
        foreach (var item in ProtocolosList) item.IsChecked = false;

        if (value != null && !string.IsNullOrEmpty(value.ProtocolosVigilancia))
        {
            var selected = value.ProtocolosVigilancia.Split(',', StringSplitOptions.RemoveEmptyEntries)
                                                     .Select(s => s.Trim())
                                                     .ToHashSet();

            foreach (var item in ProtocolosList)
            {
                if (selected.Contains(item.Name))
                {
                    item.IsChecked = true;
                }
            }
        }

        if (value != null && value.Id > 0)
        {
            _ = CargarVacaciones();
        }
        else
        {
            // Resetear valores si es nuevo o nulo
            DiasGanados = 0;
            DiasUsados = 0;
            SaldoVacaciones = 0;
            HistorialVacaciones.Clear();
            NuevaSolicitud = new SolicitudVacaciones { FechaInicio = DateTime.Today, FechaFin = DateTime.Today };
        }
    }

    private async Task CargarVacaciones()
    {
        if (TrabajadorSeleccionado == null) return;

        try
        {
            DiasGanados = await _vacacionesService.CalcularDiasGanadosAsync(TrabajadorSeleccionado.Id);
            DiasUsados = await _vacacionesService.CalcularDiasUsadosAsync(TrabajadorSeleccionado.Id);
            SaldoVacaciones = await _vacacionesService.ObtenerSaldoAsync(TrabajadorSeleccionado.Id);
            
            var historial = await _vacacionesService.ObtenerSolicitudesAsync(TrabajadorSeleccionado.Id);
            HistorialVacaciones = new ObservableCollection<SolicitudVacaciones>(historial);
            
            // Preparar nueva solicitud
            NuevaSolicitud = new SolicitudVacaciones 
            { 
                TrabajadorId = TrabajadorSeleccionado.Id,
                FechaInicio = DateTime.Today, 
                FechaFin = DateTime.Today 
            };
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error al cargar vacaciones: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RegistrarSolicitud()
    {
        if (TrabajadorSeleccionado == null || NuevaSolicitud == null) return;

        if (NuevaSolicitud.FechaFin < NuevaSolicitud.FechaInicio)
        {
            _notificationService.ShowWarning("La fecha de fin no puede ser anterior a la de inicio.");
            return;
        }

        if (NuevaSolicitud.DiasHabiles <= 0)
        {
             _notificationService.ShowWarning("Debe indicar la cantidad de días hábiles.");
             return;
        }

        try
        {
            NuevaSolicitud.TrabajadorId = TrabajadorSeleccionado.Id;
            NuevaSolicitud.FechaSolicitud = DateTime.Now;
            NuevaSolicitud.Estado = EstadoSolicitud.Aprobada; // Auto-aprobar por ahora o dejar pendiente

            await _vacacionesService.RegistrarSolicitudAsync(NuevaSolicitud);
            
            _notificationService.ShowSuccess("Solicitud registrada correctamente.");
            await CargarVacaciones(); // Recargar saldo e historial
        }
        catch (Exception ex)
        {
             _notificationService.ShowError($"Error al registrar solicitud: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task CargarDatos()
    {
        try
        {
            var trabajadores = await _trabajadorService.ObtenerTodosAsync();
            Trabajadores = new ObservableCollection<Trabajador>(trabajadores);

            var centros = await _trabajadorService.ObtenerCentrosTrabajoAsync();
            CentrosTrabajo = new ObservableCollection<CentroTrabajo>(centros);
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error al cargar datos: {ex.Message}");
        }
    }

    [RelayCommand]
    private void NuevoTrabajador()
    {
        TrabajadorSeleccionado = new Trabajador 
        { 
            NombreCompleto = "Nuevo Trabajador", 
            FechaIngreso = DateTime.Today,
            Sensibilidad = SensibilidadEspecial.Ninguna
        };
        EsEdicion = true;
        // Protocolos se resetean en OnTrabajadorSeleccionadoChanged
    }

    [RelayCommand]
    private async Task GuardarTrabajador()
    {
        if (TrabajadorSeleccionado == null) return;

        // Validaciones básicas
        if (string.IsNullOrWhiteSpace(TrabajadorSeleccionado.Rut))
        {
            _notificationService.ShowWarning("Debe ingresar el RUT del trabajador.");
            return;
        }

        if (string.IsNullOrWhiteSpace(TrabajadorSeleccionado.NombreCompleto))
        {
            _notificationService.ShowWarning("Debe ingresar el nombre completo.");
            return;
        }

        if (TrabajadorSeleccionado.CentroTrabajoId == 0)
        {
            _notificationService.ShowWarning("Debe asignar un Centro de Trabajo.");
            return;
        }

        // Serializar Protocolos
        var selectedProtocols = ProtocolosList.Where(p => p.IsChecked).Select(p => p.Name);
        TrabajadorSeleccionado.ProtocolosVigilancia = string.Join(",", selectedProtocols);

        try
        {
            if (TrabajadorSeleccionado.Id == 0)
            {
                await _trabajadorService.CrearAsync(TrabajadorSeleccionado);
            }
            else
            {
                await _trabajadorService.ActualizarAsync(TrabajadorSeleccionado);
            }

            await CargarDatos();
            TrabajadorSeleccionado = null; // Limpiar selección tras guardar
            _notificationService.ShowSuccess("Trabajador guardado correctamente.");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error al guardar: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task EliminarTrabajador()
    {
        if (TrabajadorSeleccionado == null || TrabajadorSeleccionado.Id == 0) return;

        var result = System.Windows.MessageBox.Show(
            $"¿Está seguro de que desea eliminar al trabajador '{TrabajadorSeleccionado.NombreCompleto}'?",
            "Confirmar Eliminación",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);

        if (result == System.Windows.MessageBoxResult.Yes)
        {
            try
            {
                await _trabajadorService.EliminarAsync(TrabajadorSeleccionado.Id);
                TrabajadorSeleccionado = null;
                await CargarDatos();
                _notificationService.ShowSuccess("Trabajador eliminado correctamente.");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error al eliminar: {ex.Message}");
            }
        }
    }
}
