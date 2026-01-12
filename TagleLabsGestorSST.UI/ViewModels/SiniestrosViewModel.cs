using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class SiniestrosViewModel : ObservableObject
{
    private readonly ISiniestrosService _siniestrosService;
    private readonly IEmpresaService _empresaService;
    private readonly ITrabajadorService _trabajadorService;

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();
    [ObservableProperty]
    private Empresa? _empresaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<CentroTrabajo> _centrosTrabajo = new();

    [ObservableProperty]
    private ObservableCollection<Trabajador> _trabajadores = new();

    [ObservableProperty]
    private ObservableCollection<Incidente> _incidentes = new();
    [ObservableProperty]
    private Incidente? _incidenteSeleccionado;

    [ObservableProperty]
    private bool _esEdicion;

    public SiniestrosViewModel(ISiniestrosService siniestrosService, IEmpresaService empresaService, ITrabajadorService trabajadorService)
    {
        _siniestrosService = siniestrosService;
        _empresaService = empresaService;
        _trabajadorService = trabajadorService;
        CargarDatosInicialesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task CargarDatosIniciales()
    {
        var empresas = await _empresaService.ObtenerEmpresasAsync();
        Empresas = new ObservableCollection<Empresa>(empresas);
        if (Empresas.Any()) EmpresaSeleccionada = Empresas.First();
    }

    partial void OnEmpresaSeleccionadaChanged(Empresa? value)
    {
        if (value != null)
        {
            CentrosTrabajo = new ObservableCollection<CentroTrabajo>(value.CentrosTrabajo);
            CargarIncidentesCommand.Execute(null);
            CargarTrabajadoresCommand.Execute(null);
        }
        else
        {
            CentrosTrabajo.Clear();
            Incidentes.Clear();
            Trabajadores.Clear();
        }
    }

    [RelayCommand]
    private async Task CargarIncidentes()
    {
        if (EmpresaSeleccionada == null) return;
        var lista = await _siniestrosService.ObtenerPorEmpresaAsync(EmpresaSeleccionada.Id);
        Incidentes = new ObservableCollection<Incidente>(lista);
    }

    [RelayCommand]
    private async Task CargarTrabajadores()
    {
        // Idealmente filtraríamos trabajadores por empresa, pero el servicio actual trae todos.
        // Para simplificar, traemos todos y filtramos en memoria si es necesario, o usamos el servicio tal cual.
        var lista = await _trabajadorService.ObtenerTodosAsync();
        // Filtramos en memoria por los centros de la empresa seleccionada
        var filtrados = lista.Where(t => t.CentroTrabajo != null && t.CentroTrabajo.EmpresaId == EmpresaSeleccionada?.Id).ToList();
        Trabajadores = new ObservableCollection<Trabajador>(filtrados);
    }

    [RelayCommand]
    private void NuevoIncidente()
    {
        IncidenteSeleccionado = new Incidente
        {
            Fecha = DateTime.Now,
            Tipo = TipoSiniestro.Incidente
        };
        EsEdicion = true;
    }

    [RelayCommand]
    private async Task GuardarIncidente()
    {
        if (IncidenteSeleccionado == null) return;

        if (IncidenteSeleccionado.Id == 0)
        {
            await _siniestrosService.CrearAsync(IncidenteSeleccionado);
        }
        else
        {
            await _siniestrosService.ActualizarAsync(IncidenteSeleccionado);
        }

        await CargarIncidentes();
        EsEdicion = false;
        // Mantener selección o limpiar? Limpiemos para volver a la lista
        IncidenteSeleccionado = null;
    }

    [RelayCommand]
    private async Task EliminarIncidente()
    {
        if (IncidenteSeleccionado == null || IncidenteSeleccionado.Id == 0) return;
        await _siniestrosService.EliminarAsync(IncidenteSeleccionado.Id);
        await CargarIncidentes();
        IncidenteSeleccionado = null;
    }

    [RelayCommand]
    private void CancelarEdicion()
    {
        IncidenteSeleccionado = null;
        EsEdicion = false;
    }
}
