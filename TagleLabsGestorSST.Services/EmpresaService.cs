using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public interface IEmpresaService
{
    Task<Empresa> CrearEmpresaAsync(Empresa empresa, CancellationToken ct = default);
    Task<IReadOnlyList<Empresa>> ObtenerEmpresasAsync(CancellationToken ct = default);
    Task<Empresa?> ObtenerPorIdAsync(int id, CancellationToken ct = default);
    Task ActualizarEmpresaAsync(Empresa empresa, CancellationToken ct = default);
    Task EliminarEmpresaAsync(int id, CancellationToken ct = default);
}

public class EmpresaService : IEmpresaService
{
    private readonly TagleLabsContext _db;
    private readonly IObligacionesService _obligacionesService;
    private readonly IAuditoriaService _auditoriaService;
    private readonly IAsistenciaProSyncService _syncService;

    public EmpresaService(TagleLabsContext db, IObligacionesService obligacionesService, IAuditoriaService auditoriaService, IAsistenciaProSyncService syncService)
    {
        _db = db;
        _obligacionesService = obligacionesService;
        _auditoriaService = auditoriaService;
        _syncService = syncService;
    }

    public async Task<Empresa> CrearEmpresaAsync(Empresa empresa, CancellationToken ct = default)
    {
        _db.Empresas.Add(empresa);
        await _db.SaveChangesAsync(ct);
        
        // Crear un Centro de Trabajo por defecto
        var centroDefault = new CentroTrabajo
        {
            Nombre = "Sede Principal",
            EmpresaId = empresa.Id,
            Direccion = empresa.Direccion ?? "Sin dirección especificada",
            Region = "Región Metropolitana",
            Ciudad = "Santiago"
        };
        _db.CentrosTrabajo.Add(centroDefault);
        await _db.SaveChangesAsync(ct);
        
        await _obligacionesService.AsignarObligacionesAsync(empresa.Id, ct);
        
        await _auditoriaService.RegistrarAccionAsync("Crear", "Empresa", empresa.Id.ToString(), $"Se creó la empresa {empresa.RazonSocial}");
        
        // Sincronizar con AsistenciaPro
        _ = _syncService.SyncEmpresaAsync(empresa.RazonSocial, empresa.Rut, empresa.EmailContacto, empresa.Telefono, ct);
        
        return empresa;
    }

    public async Task<IReadOnlyList<Empresa>> ObtenerEmpresasAsync(CancellationToken ct = default)
    {
        return await _db.Empresas
            .Include(e => e.CentrosTrabajo)
            .Include(e => e.Rubro)
            .Include(e => e.Rubros)
            .ToListAsync(ct);
    }

    public async Task<Empresa?> ObtenerPorIdAsync(int id, CancellationToken ct = default)
    {
        return await _db.Empresas
            .Include(e => e.CentrosTrabajo)
            .Include(e => e.Obligaciones)
            .FirstOrDefaultAsync(e => e.Id == id, ct);
    }

    public async Task ActualizarEmpresaAsync(Empresa empresa, CancellationToken ct = default)
    {
        // Cargar empresa existente con sus rubros
        var empresaExistente = await _db.Empresas
            .Include(e => e.Rubros)
            .FirstOrDefaultAsync(e => e.Id == empresa.Id, ct);
        
        if (empresaExistente == null) return;

        // Actualizar campos escalares
        empresaExistente.Rut = empresa.Rut;
        empresaExistente.RazonSocial = empresa.RazonSocial;
        empresaExistente.Giro = empresa.Giro;
        empresaExistente.RubroPrincipal = empresa.RubroPrincipal;
        empresaExistente.RubroId = empresa.RubroId;
        empresaExistente.NumeroTrabajadores = empresa.NumeroTrabajadores;
        empresaExistente.Mutual = empresa.Mutual;
        empresaExistente.LogoPath = empresa.LogoPath;
        empresaExistente.Direccion = empresa.Direccion;
        empresaExistente.Telefono = empresa.Telefono;
        empresaExistente.EmailContacto = empresa.EmailContacto;
        empresaExistente.RepresentanteLegal = empresa.RepresentanteLegal;

        // Sincronizar Rubros (relación N:N)
        var rubrosIdsNuevos = empresa.Rubros.Select(r => r.Id).ToList();
        var rubrosExistentesDb = await _db.Set<Rubro>()
            .Where(r => rubrosIdsNuevos.Contains(r.Id))
            .ToListAsync(ct);
        
        empresaExistente.Rubros.Clear();
        foreach (var rubro in rubrosExistentesDb)
        {
            empresaExistente.Rubros.Add(rubro);
        }

        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Editar", "Empresa", empresa.Id.ToString(), $"Se actualizó la empresa {empresa.RazonSocial}");
        
        // Sincronizar con AsistenciaPro
        _ = _syncService.SyncEmpresaAsync(empresa.RazonSocial, empresa.Rut, empresa.EmailContacto, empresa.Telefono, ct);
    }

    public async Task EliminarEmpresaAsync(int id, CancellationToken ct = default)
    {
        var empresa = await _db.Empresas.FindAsync(new object[] { id }, ct);
        if (empresa == null) return;
        
        var nombreEmpresa = empresa.RazonSocial;
        _db.Empresas.Remove(empresa);
        await _db.SaveChangesAsync(ct);
        
        await _auditoriaService.RegistrarAccionAsync("Eliminar", "Empresa", id.ToString(), $"Se eliminó la empresa {nombreEmpresa}");
    }
}
