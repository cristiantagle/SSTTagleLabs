using System;
using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public interface IObligacionesService
{
    Task AsignarObligacionesAsync(int empresaId, CancellationToken ct = default);
    Task<IEnumerable<ObligacionEmpresa>> ObtenerPendientesAsync(int empresaId, CancellationToken ct = default);
    Task<IEnumerable<ObligacionEmpresa>> ObtenerTodasPorEmpresaAsync(int empresaId, CancellationToken ct = default);
    Task ActualizarEstadoAsync(int obligacionEmpresaId, EstadoObligacion estado, string? observaciones = null, CancellationToken ct = default);
}

public class ObligacionesService : IObligacionesService
{
    private readonly TagleLabsContext _db;
    private readonly IAuditoriaService _auditoriaService;

    public ObligacionesService(TagleLabsContext db, IAuditoriaService auditoriaService)
    {
        _db = db;
        _auditoriaService = auditoriaService;
    }

    public async Task AsignarObligacionesAsync(int empresaId, CancellationToken ct = default)
    {
        var empresa = await _db.Empresas.Include(e => e.CentrosTrabajo).FirstOrDefaultAsync(e => e.Id == empresaId, ct);
        if (empresa == null) return;

        var obligatorias = await _db.ObligacionesDs44
            .Where(o => (!o.AplicaMinTrab.HasValue || empresa.NumeroTrabajadores >= o.AplicaMinTrab)
                        && (!o.AplicaMaxTrab.HasValue || empresa.NumeroTrabajadores <= o.AplicaMaxTrab)
                        && (string.IsNullOrWhiteSpace(o.RubroObjetivo) || o.RubroObjetivo == empresa.RubroPrincipal))
            .ToListAsync(ct);

        foreach (var obligacion in obligatorias)
        {
            var yaExiste = await _db.ObligacionesEmpresa.AnyAsync(o => o.EmpresaId == empresa.Id && o.ObligacionDs44Id == obligacion.Id, ct);
            if (yaExiste) continue;

            _db.ObligacionesEmpresa.Add(new ObligacionEmpresa
            {
                EmpresaId = empresa.Id,
                ObligacionDs44Id = obligacion.Id,
                Estado = EstadoObligacion.Pendiente,
                FechaLimite = DateTime.UtcNow.AddDays(30)
            });
        }

        await _db.SaveChangesAsync(ct);
    }

    public async Task<IEnumerable<ObligacionEmpresa>> ObtenerPendientesAsync(int empresaId, CancellationToken ct = default)
    {
        return await _db.ObligacionesEmpresa
            .Include(o => o.Obligacion)
            .Where(o => o.EmpresaId == empresaId && o.Estado != EstadoObligacion.Cumplida)
            .ToListAsync(ct);
    }

    public async Task<IEnumerable<ObligacionEmpresa>> ObtenerTodasPorEmpresaAsync(int empresaId, CancellationToken ct = default)
    {
        return await _db.ObligacionesEmpresa
            .Include(o => o.Obligacion)
            .Where(o => o.EmpresaId == empresaId)
            .OrderBy(o => o.FechaLimite)
            .ToListAsync(ct);
    }

    public async Task ActualizarEstadoAsync(int obligacionEmpresaId, EstadoObligacion estado, string? observaciones = null, CancellationToken ct = default)
    {
        var registro = await _db.ObligacionesEmpresa.FirstOrDefaultAsync(o => o.Id == obligacionEmpresaId, ct);
        if (registro == null) return;
        registro.Estado = estado;
        registro.Observaciones = observaciones;
        if (estado == EstadoObligacion.Cumplida)
        {
            registro.FechaCumplimiento = DateTime.UtcNow;
        }
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Actualizar", "Obligación", obligacionEmpresaId.ToString(), $"Estado cambiado a {estado}");
    }
}
