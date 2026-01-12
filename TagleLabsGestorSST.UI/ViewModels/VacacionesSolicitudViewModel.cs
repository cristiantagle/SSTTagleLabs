using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class VacacionesSolicitudViewModel : ObservableObject
{
    [ObservableProperty]
    private Trabajador? _trabajador;

    [ObservableProperty]
    private double _diasAcumulados;

    [ObservableProperty]
    private int _diasUsadosPrevios;

    [ObservableProperty]
    private double _saldoDisponible;

    [ObservableProperty]
    private int _diasASolicitar;

    [ObservableProperty]
    private DateTime? _fechaInicio;

    [ObservableProperty]
    private DateTime? _fechaFin;

    private INotificationService _notificationService;

    public VacacionesSolicitudViewModel(INotificationService notificationService)
    {
        _notificationService = notificationService;
    }

    public void InicializarConTrabajador(Trabajador trabajador, double diasAcumulados, int diasUsadosPrevios)
    {
        Trabajador = trabajador;
        DiasAcumulados = diasAcumulados;
        DiasUsadosPrevios = diasUsadosPrevios;
        CalcularSaldo();
    }

    partial void OnDiasUsadosPreviosChanged(int value)
    {
        CalcularSaldo();
    }

    partial void OnDiasASolicitarChanged(int value)
    {
        CalcularSaldo();
    }

    private void CalcularSaldo()
    {
        var disponibles = DiasAcumulados - DiasUsadosPrevios;
        SaldoDisponible = disponibles;
    }

    [RelayCommand]
    private void Generar()
    {
        if (Trabajador == null)
        {
            _notificationService.ShowWarning("Debe seleccionar un trabajador");
            return;
        }

        if (!FechaInicio.HasValue || !FechaFin.HasValue)
        {
            _notificationService.ShowWarning("Debe indicar fechas de inicio y fin");
            return;
        }

        if (DiasASolicitar <= 0)
        {
            _notificationService.ShowWarning("Debe solicitar al menos 1 día");
            return;
        }

        if (DiasASolicitar > SaldoDisponible)
        {
            _notificationService.ShowError($"Saldo insuficiente. Disponible: {SaldoDisponible:F1} días");
            return;
        }

        // Si pasó todas las validaciones, completar exitosamente
    }
}
