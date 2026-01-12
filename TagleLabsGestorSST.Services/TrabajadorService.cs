using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public interface ITrabajadorService
{
    Task<IReadOnlyList<Trabajador>> ObtenerTodosAsync(CancellationToken ct = default);
    Task<Trabajador?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task<Trabajador> CrearAsync(Trabajador trabajador, CancellationToken ct = default);
    Task ActualizarAsync(Trabajador trabajador, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<CentroTrabajo>> ObtenerCentrosTrabajoAsync(CancellationToken ct = default);
    Task<(double Ganados, int Usados, double Disponibles)> CalcularVacacionesAsync(int trabajadorId, CancellationToken ct = default);
}

public class TrabajadorService : ITrabajadorService
{
    private readonly TagleLabsContext _db;
    private readonly IAuditoriaService _auditoriaService;
    private readonly IAsistenciaProSyncService _syncService;

    public TrabajadorService(TagleLabsContext db, IAuditoriaService auditoriaService, IAsistenciaProSyncService syncService)
    {
        _db = db;
        _auditoriaService = auditoriaService;
        _syncService = syncService;
    }

    public async Task<IReadOnlyList<Trabajador>> ObtenerTodosAsync(CancellationToken ct = default)
    {
        return await _db.Trabajadores
            .Include(t => t.CentroTrabajo)
            .ThenInclude(c => c!.Empresa)
            .ToListAsync(ct);
    }

    public async Task<Trabajador?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Trabajadores
            .Include(t => t.CentroTrabajo)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
    }

    public async Task<Trabajador> CrearAsync(Trabajador trabajador, CancellationToken ct = default)
    {
        _db.Trabajadores.Add(trabajador);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Crear", "Trabajador", trabajador.Id.ToString(), $"Se creó el trabajador {trabajador.NombreCompleto}");
        
        // Sincronizar con AsistenciaPro
        var centro = await _db.CentrosTrabajo.Include(c => c.Empresa).FirstOrDefaultAsync(c => c.Id == trabajador.CentroTrabajoId, ct);
        if (centro?.Empresa != null && !string.IsNullOrEmpty(trabajador.Email))
        {
            _ = _syncService.SyncTrabajadorAsync(centro.Empresa.RazonSocial, trabajador.NombreCompleto, trabajador.Rut, trabajador.Email, null, ct);
        }
        
        return trabajador;
    }

    public async Task ActualizarAsync(Trabajador trabajador, CancellationToken ct = default)
    {
        _db.Trabajadores.Update(trabajador);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Editar", "Trabajador", trabajador.Id.ToString(), $"Se actualizó el trabajador {trabajador.NombreCompleto}");
        
        // Sincronizar con AsistenciaPro
        var centro = await _db.CentrosTrabajo.Include(c => c.Empresa).FirstOrDefaultAsync(c => c.Id == trabajador.CentroTrabajoId, ct);
        if (centro?.Empresa != null && !string.IsNullOrEmpty(trabajador.Email))
        {
            _ = _syncService.SyncTrabajadorAsync(centro.Empresa.RazonSocial, trabajador.NombreCompleto, trabajador.Rut, trabajador.Email, null, ct);
        }
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var trabajador = await _db.Trabajadores.FindAsync(new object[] { id }, ct);
        if (trabajador == null) return;
        
        var nombre = trabajador.NombreCompleto;
        _db.Trabajadores.Remove(trabajador);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Eliminar", "Trabajador", id.ToString(), $"Se eliminó el trabajador {nombre}");
    }

    public async Task<IReadOnlyList<CentroTrabajo>> ObtenerCentrosTrabajoAsync(CancellationToken ct = default)
    {
        return await _db.CentrosTrabajo
            .Include(c => c.Empresa)
            .ToListAsync(ct);
    }

    public async Task<(double Ganados, int Usados, double Disponibles)> CalcularVacacionesAsync(int trabajadorId, CancellationToken ct = default)
    {
        var trabajador = await _db.Trabajadores.FindAsync(new object[] { trabajadorId }, ct);
        if (trabajador == null) return (0, 0, 0);

        // Cálculo Básico: 15 días hábiles por año de servicio
        // Nota: No considera días progresivos ni feriados específicos, es una aproximación estándar.
        var antiguedad = DateTime.Now - trabajador.FechaIngreso;
        var aniosServicio = antiguedad.TotalDays / 365.25;
        
        // 1.25 días por mes = 15 días por año
        var diasGanados = Math.Round(aniosServicio * 15, 2);
        
        var diasUsados = trabajador.DiasVacacionesUsados;
        var diasDisponibles = diasGanados - diasUsados;

        return (diasGanados, diasUsados, diasDisponibles);
    }
}
