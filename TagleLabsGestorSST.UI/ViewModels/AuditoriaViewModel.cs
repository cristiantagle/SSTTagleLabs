using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;
using System.Windows;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class AuditoriaViewModel : ObservableObject
{
    private readonly TagleLabsContext _db;

    [ObservableProperty]
    private ObservableCollection<LogAuditoria> _logs = new();

    [ObservableProperty]
    private string _mensaje = "Cargando logs...";

    public AuditoriaViewModel(TagleLabsContext db)
    {
        _db = db;
        
        // Cargar de forma asíncrona después de la inicialización
        _ = CargarLogs();
    }

    [RelayCommand]
    private async Task CargarLogs()
    {
        try
        {
            Mensaje = "Cargando logs...";
            
            // Verificar si la tabla existe
            var canConnect = await _db.Database.CanConnectAsync();
            
            var lista = await _db.LogsAuditoria
                .OrderByDescending(l => l.Fecha)
                .Take(100) // Mostrar últimos 100 por rendimiento
                .ToListAsync();
            
            Logs = new ObservableCollection<LogAuditoria>(lista);
            
            if (lista.Count == 0)
            {
                Mensaje = "No hay registros de auditoría. Crea una empresa o trabajador para generar logs, o usa el botón de prueba.";
            }
            else
            {
                Mensaje = $"Mostrando {lista.Count} registro(s) de auditoría";
            }
        }
        catch (Exception ex)
        {
            Mensaje = $"Error al cargar logs: {ex.Message}";
        }
    }
}
