using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using Microsoft.Extensions.Configuration;

namespace TagleLabsGestorSST.Services;

/// <summary>
/// Registro de asistencia sincronizado desde AsistenciaPro
/// </summary>
public class AttendanceRecordDto
{
    public string Id { get; set; } = string.Empty;
    public string EmployeeId { get; set; } = string.Empty;
    public string EmployeeName { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string TipoJornada { get; set; } = string.Empty;
    public string? HoraEntrada { get; set; }
    public string? HoraSalida { get; set; }
    public bool EsManual { get; set; }
    public string? Notas { get; set; }
}

public class AttendanceSyncResponse
{
    public List<AttendanceRecordDto> Records { get; set; } = new();
}

public interface IAsistenciaProSyncService
{
    Task<bool> SyncEmpresaAsync(string razonSocial, string? rut, string? email, string? telefono, CancellationToken ct = default);
    Task<bool> SyncTrabajadorAsync(string empresaNombre, string nombreCompleto, string? rut, string email, decimal? sueldoMensual, CancellationToken ct = default);
    Task<List<AttendanceRecordDto>> GetAttendanceAsync(string empresaNombre, DateTime startDate, DateTime endDate, CancellationToken ct = default);
    bool IsEnabled { get; }
}

public class AsistenciaProSyncService : IAsistenciaProSyncService
{
    private readonly HttpClient _http;
    private readonly string? _apiKey;
    private readonly string? _baseUrl;
    private readonly bool _isEnabled;

    public bool IsEnabled => _isEnabled;

    public AsistenciaProSyncService(IConfiguration configuration)
    {
        _http = new HttpClient();
        
        // Leer configuración de appsettings.json
        var section = configuration.GetSection("AsistenciaPro");
        _baseUrl = section["BaseUrl"]; // ej: "https://asistenciapro.vercel.app"
        _apiKey = section["ApiKey"];   // ej: "mi-api-key-secreta"
        
        _isEnabled = !string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(_apiKey);
        
        if (_isEnabled)
        {
            _http.DefaultRequestHeaders.Add("x-api-key", _apiKey);
        }
    }

    public async Task<bool> SyncEmpresaAsync(string razonSocial, string? rut, string? email, string? telefono, CancellationToken ct = default)
    {
        if (!_isEnabled) return false;

        try
        {
            var payload = new
            {
                externalId = Guid.NewGuid().ToString(),
                name = razonSocial,
                rut,
                emailContacto = email,
                telefonoContacto = telefono
            };

            var response = await _http.PostAsJsonAsync($"{_baseUrl}/api/sync/company", payload, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync(ct);
                System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Empresa sincronizada: {result}");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Error al sincronizar empresa: {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Excepción: {ex.Message}");
            return false;
        }
    }

    public async Task<bool> SyncTrabajadorAsync(string empresaNombre, string nombreCompleto, string? rut, string email, decimal? sueldoMensual, CancellationToken ct = default)
    {
        if (!_isEnabled) return false;

        try
        {
            var payload = new
            {
                companyName = empresaNombre,
                nombreCompleto,
                rut,
                email,
                sueldoMensual = sueldoMensual.HasValue ? (double?)Convert.ToDouble(sueldoMensual.Value) : null
            };

            var response = await _http.PostAsJsonAsync($"{_baseUrl}/api/sync/worker", payload, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadAsStringAsync(ct);
                System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Trabajador sincronizado: {result}");
                return true;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Error al sincronizar trabajador: {error}");
                return false;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Excepción: {ex.Message}");
            return false;
        }
    }

    public async Task<List<AttendanceRecordDto>> GetAttendanceAsync(string empresaNombre, DateTime startDate, DateTime endDate, CancellationToken ct = default)
    {
        if (!_isEnabled) return new List<AttendanceRecordDto>();

        try
        {
            var startStr = startDate.ToString("yyyy-MM-dd");
            var endStr = endDate.ToString("yyyy-MM-dd");
            var url = $"{_baseUrl}/api/sync/attendance?companyName={Uri.EscapeDataString(empresaNombre)}&startDate={startStr}&endDate={endStr}";

            var response = await _http.GetAsync(url, ct);
            
            if (response.IsSuccessStatusCode)
            {
                var json = await response.Content.ReadAsStringAsync(ct);
                var data = JsonSerializer.Deserialize<AttendanceSyncResponse>(json, new JsonSerializerOptions 
                { 
                    PropertyNameCaseInsensitive = true 
                });
                
                System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Asistencia obtenida: {data?.Records.Count ?? 0} registros");
                return data?.Records ?? new List<AttendanceRecordDto>();
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync(ct);
                System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Error al obtener asistencia: {error}");
                return new List<AttendanceRecordDto>();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"[AsistenciaPro Sync] Excepción: {ex.Message}");
            return new List<AttendanceRecordDto>();
        }
    }
}
