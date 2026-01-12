using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public interface ISiniestrosService
{
    Task<IReadOnlyList<Incidente>> ObtenerPorEmpresaAsync(int empresaId, CancellationToken ct = default);
    Task<Incidente> CrearAsync(Incidente incidente, CancellationToken ct = default);
    Task ActualizarAsync(Incidente incidente, CancellationToken ct = default);
    Task EliminarAsync(int id, CancellationToken ct = default);
}

public class SiniestrosService : ISiniestrosService
{
    private readonly TagleLabsContext _db;
    private readonly IAuditoriaService _auditoriaService;

    public SiniestrosService(TagleLabsContext db, IAuditoriaService auditoriaService)
    {
        _db = db;
        _auditoriaService = auditoriaService;
    }

    public async Task<IReadOnlyList<Incidente>> ObtenerPorEmpresaAsync(int empresaId, CancellationToken ct = default)
    {
        return await _db.Incidentes
            .Include(i => i.Trabajador)
            .Include(i => i.CentroTrabajo)
            .Where(i => i.CentroTrabajo != null && i.CentroTrabajo.EmpresaId == empresaId)
            .OrderByDescending(i => i.Fecha)
            .ToListAsync(ct);
    }

    public async Task<Incidente> CrearAsync(Incidente incidente, CancellationToken ct = default)
    {
        _db.Incidentes.Add(incidente);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Crear", "Siniestro", incidente.Id.ToString(), $"Se registró siniestro: {incidente.Descripcion}");
        
        return incidente;
    }

    public async Task ActualizarAsync(Incidente incidente, CancellationToken ct = default)
    {
        _db.Incidentes.Update(incidente);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Editar", "Siniestro", incidente.Id.ToString(), $"Se actualizó siniestro ID {incidente.Id}");
    }

    public async Task EliminarAsync(int id, CancellationToken ct = default)
    {
        var item = await _db.Incidentes.FindAsync(new object[] { id }, ct);
        if (item == null) return;
        _db.Incidentes.Remove(item);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Eliminar", "Siniestro", id.ToString(), $"Se eliminó siniestro ID {id}");
    }
}
