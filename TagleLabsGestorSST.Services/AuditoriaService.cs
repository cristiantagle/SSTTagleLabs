using System;
using System.Threading.Tasks;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Services;

public interface IAuditoriaService
{
    Task RegistrarAccionAsync(string accion, string entidad, string entidadId, string detalle, string? usuario = null);
}

public class AuditoriaService : IAuditoriaService
{
    private readonly TagleLabsContext _db;
    private readonly ISessionService _sessionService;

    public AuditoriaService(TagleLabsContext db, ISessionService sessionService)
    {
        _db = db;
        _sessionService = sessionService;
    }

    public async Task RegistrarAccionAsync(string accion, string entidad, string entidadId, string detalle, string? usuario = null)
    {
        try
        {
            // Determinar usuario: argumento > sesión > "Sistema"
            // Determinar usuario: argumento > sesión > "Sistema"
            string usuarioFinal = usuario ?? _sessionService.UsuarioActual?.Username ?? "Sistema";

            System.Diagnostics.Debug.WriteLine($"[AUDITORIA] Intentando registrar: {accion} - {entidad} - {detalle}");
            
            var log = new LogAuditoria
            {
                Fecha = DateTime.Now,
                Accion = accion,
                Entidad = entidad,
                EntidadId = entidadId,
                Detalle = detalle,
                Usuario = usuarioFinal
            };

            _db.LogsAuditoria.Add(log);
            await _db.SaveChangesAsync();
            
            System.Diagnostics.Debug.WriteLine($"[AUDITORIA] Log guardado exitosamente. ID: {log.Id}");
        }
        catch (Exception ex)
        {
            // Fallback silencioso: si falla el log, no detener la operación principal, 
            // pero idealmente loguear a archivo de texto o consola.
            System.Diagnostics.Debug.WriteLine($"[AUDITORIA ERROR] Error registrando auditoría: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"[AUDITORIA ERROR] StackTrace: {ex.StackTrace}");
        }
    }
}
