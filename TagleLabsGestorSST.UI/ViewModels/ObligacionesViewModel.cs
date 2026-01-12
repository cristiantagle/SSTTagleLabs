using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class ObligacionesViewModel : ObservableObject
{
    private readonly IObligacionesService _obligacionesService;
    private readonly IEmpresaService _empresaService;

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();

    [ObservableProperty]
    private Empresa? _empresaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<ObligacionEmpresa> _obligaciones = new();

    [ObservableProperty]
    private ObligacionEmpresa? _obligacionSeleccionada;

    public ObligacionesViewModel(IObligacionesService obligacionesService, IEmpresaService empresaService)
    {
        _obligacionesService = obligacionesService;
        _empresaService = empresaService;
        CargarDatosInicialesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task CargarDatosIniciales()
    {
        var empresas = await _empresaService.ObtenerEmpresasAsync();
        Empresas = new ObservableCollection<Empresa>(empresas);

        if (Empresas.Any())
        {
            EmpresaSeleccionada = Empresas.First();
        }
    }

    partial void OnEmpresaSeleccionadaChanged(Empresa? value)
    {
        if (value != null)
        {
            CargarObligacionesCommand.Execute(null);
        }
        else
        {
            Obligaciones.Clear();
        }
    }

    [RelayCommand]
    private async Task CargarObligaciones()
    {
        if (EmpresaSeleccionada == null) return;
        
        // Asegurar que las obligaciones estén asignadas (si es nueva empresa)
        await _obligacionesService.AsignarObligacionesAsync(EmpresaSeleccionada.Id);

        var lista = await _obligacionesService.ObtenerTodasPorEmpresaAsync(EmpresaSeleccionada.Id);
        Obligaciones = new ObservableCollection<ObligacionEmpresa>(lista);
    }

    [RelayCommand]
    private async Task MarcarCumplida()
    {
        if (ObligacionSeleccionada == null) return;
        
        await _obligacionesService.ActualizarEstadoAsync(
            ObligacionSeleccionada.Id, 
            EstadoObligacion.Cumplida, 
            ObligacionSeleccionada.Observaciones);
        
        await CargarObligaciones();
    }

    [RelayCommand]
    private async Task MarcarEnProgreso()
    {
        if (ObligacionSeleccionada == null) return;

        await _obligacionesService.ActualizarEstadoAsync(
            ObligacionSeleccionada.Id, 
            EstadoObligacion.EnProgreso, 
            ObligacionSeleccionada.Observaciones);

        await CargarObligaciones();
    }

    [RelayCommand]
    private async Task GuardarObservacion()
    {
        if (ObligacionSeleccionada == null) return;

        await _obligacionesService.ActualizarEstadoAsync(
            ObligacionSeleccionada.Id,
            ObligacionSeleccionada.Estado,
            ObligacionSeleccionada.Observaciones);
            
        // No recargamos toda la lista para no perder el foco, solo guardamos
    }
}
