using System;
using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace TagleLabsGestorSST.Services;

/// <summary>
/// Servicio para convertir documentos DOCX a PDF usando LibreOffice Portable.
/// Implementa fallback a generación simple si LibreOffice no está disponible.
/// </summary>
public interface IConversorPdfService
{
    /// <summary>
    /// Detecta si LibreOffice Portable está disponible en la carpeta local.
    /// </summary>
    bool EstaLibreOfficeDisponible();

    /// <summary>
    /// Obtiene la ruta al ejecutable de LibreOffice Portable.
    /// </summary>
    string? ObtenerRutaSoffice();

    /// <summary>
    /// Convierte un archivo DOCX a PDF usando LibreOffice Portable.
    /// Si LibreOffice no está disponible, devuelve null.
    /// </summary>
    Task<string?> ConvertirDocxAPdfAsync(string rutaDocx, CancellationToken ct = default);

    /// <summary>
    /// Descarga LibreOffice Portable desde sourceforge en background.
    /// Callback para reportar progreso: bytes descargados, total de bytes.
    /// </summary>
    Task<bool> DescargarLibreOfficeAsync(Action<long, long>? progressCallback = null, CancellationToken ct = default);

    /// <summary>
    /// Obtiene el tamaño del archivo LibreOffice Portable a descargar.
    /// </summary>
    Task<long> ObtenerTamanoDescargarAsync();
}

public class ConversorPdfService : IConversorPdfService
{
    private readonly string _libreOfficePath;
    private readonly string _sofficeExe;
    private readonly string _zipTemporalPath;
    
    // URL de descarga desde SourceForge (mirror confiable)
    private const string URL_LIBREOFFICE_PORTABLE = 
        "https://sourceforge.net/projects/portableapps/files/LibreOfficePortable/24.2.6.1/LibreOfficePortable_24.2.6.1_en-US.paf.exe/download";

    public ConversorPdfService()
    {
        _libreOfficePath = Path.Combine(Directory.GetCurrentDirectory(), "lib", "LibreOffice");
        _sofficeExe = Path.Combine(_libreOfficePath, "program", "soffice.exe");
        _zipTemporalPath = Path.Combine(Directory.GetCurrentDirectory(), "lib", "temp");
    }

    public bool EstaLibreOfficeDisponible()
    {
        return File.Exists(_sofficeExe);
    }

    public string? ObtenerRutaSoffice()
    {
        return EstaLibreOfficeDisponible() ? _sofficeExe : null;
    }

    public async Task<long> ObtenerTamanoDescargarAsync()
    {
        try
        {
            using (var client = new HttpClient())
            {
                // Usar GET con ResponseHeadersRead para obtener solo headers sin descargar el cuerpo
                using (var response = await client.GetAsync(URL_LIBREOFFICE_PORTABLE, HttpCompletionOption.ResponseHeadersRead))
                {
                    if (response.Content.Headers.ContentLength.HasValue)
                    {
                        return response.Content.Headers.ContentLength.Value;
                    }
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ConversorPdf] Error obteniendo tamaño: {ex.Message}");
        }
        return 0;
    }

    public async Task<bool> DescargarLibreOfficeAsync(Action<long, long>? progressCallback = null, CancellationToken ct = default)
    {
        try
        {
            // Crear directorio si no existe
            Directory.CreateDirectory(_zipTemporalPath);

            string rutaDescarga = Path.Combine(_zipTemporalPath, "LibreOfficePortable.exe");
            
            Debug.WriteLine($"[ConversorPdf] Iniciando descarga de LibreOffice Portable...");
            Debug.WriteLine($"[ConversorPdf] URL: {URL_LIBREOFFICE_PORTABLE}");
            Debug.WriteLine($"[ConversorPdf] Destino: {rutaDescarga}");

            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromMinutes(30); // 30 minutos para descargar

                using (var response = await client.GetAsync(URL_LIBREOFFICE_PORTABLE, HttpCompletionOption.ResponseHeadersRead, ct))
                {
                    response.EnsureSuccessStatusCode();

                    var totalBytes = response.Content.Headers.ContentLength ?? -1L;
                    var canReportProgress = totalBytes != -1 && progressCallback != null;

                    using (var contentStream = await response.Content.ReadAsStreamAsync())
                    using (var fileStream = new FileStream(rutaDescarga, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true))
                    {
                        var totalRead = 0L;
                        var buffer = new byte[8192];
                        int bytesRead;

                        while ((bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length, ct)) != 0)
                        {
                            await fileStream.WriteAsync(buffer, 0, bytesRead, ct);
                            totalRead += bytesRead;

                            if (canReportProgress)
                            {
                                progressCallback?.Invoke(totalRead, totalBytes);
                            }
                        }
                    }
                }
            }

            // El instalador .exe de SourceForge ya es autoejecutable
            // LibreOffice está contenido en el ejecutable
            // Se debe ejecutar el instalador para extraer LibreOffice
            Debug.WriteLine($"[ConversorPdf] Descarga completada. Ejecutando instalador...");

            var processInfo = new ProcessStartInfo
            {
                FileName = rutaDescarga,
                Arguments = "/S", // Silent installation
                UseShellExecute = true,
                CreateNoWindow = false,
                WorkingDirectory = _zipTemporalPath
            };

            using (var process = Process.Start(processInfo))
            {
                if (process == null)
                {
                    Debug.WriteLine($"[ConversorPdf] No se pudo iniciar instalador");
                    return false;
                }

                bool completado = await Task.Run(() => process.WaitForExit(300000), ct); // 5 minutos para instalar
                
                if (!completado)
                {
                    process.Kill();
                    Debug.WriteLine($"[ConversorPdf] Timeout instalando LibreOffice");
                    return false;
                }

                Debug.WriteLine($"[ConversorPdf] Instalador completado con código: {process.ExitCode}");
            }

            // Verificar que soffice.exe existe después de instalar
            if (File.Exists(_sofficeExe))
            {
                Debug.WriteLine($"[ConversorPdf] LibreOffice Portable instalado exitosamente");
                
                // Limpiar archivo temporal
                try { File.Delete(rutaDescarga); } catch { }
                
                return true;
            }
            else
            {
                Debug.WriteLine($"[ConversorPdf] soffice.exe no encontrado después de instalar");
                return false;
            }
        }
        catch (OperationCanceledException)
        {
            Debug.WriteLine($"[ConversorPdf] Descarga cancelada por usuario");
            return false;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ConversorPdf] Error descargando: {ex.Message}\n{ex.StackTrace}");
            return false;
        }
    }

    public async Task<string?> ConvertirDocxAPdfAsync(string rutaDocx, CancellationToken ct = default)
    {
        // Validar que el archivo DOCX existe
        if (!File.Exists(rutaDocx))
        {
            Debug.WriteLine($"[ConversorPdf] Archivo no encontrado: {rutaDocx}");
            return null;
        }

        // Detectar si LibreOffice está disponible
        if (!EstaLibreOfficeDisponible())
        {
            Debug.WriteLine($"[ConversorPdf] LibreOffice Portable no encontrado en: {_sofficeExe}");
            return null;
        }

        try
        {
            string carpetaSalida = Path.GetDirectoryName(rutaDocx) ?? Directory.GetCurrentDirectory();
            string nombreSinExtension = Path.GetFileNameWithoutExtension(rutaDocx);
            string rutaPdfEsperada = Path.Combine(carpetaSalida, $"{nombreSinExtension}.pdf");

            // Eliminar PDF anterior si existe (para evitar cachés)
            if (File.Exists(rutaPdfEsperada))
            {
                File.Delete(rutaPdfEsperada);
            }

            // Configurar proceso de LibreOffice
            var processInfo = new ProcessStartInfo
            {
                FileName = _sofficeExe,
                Arguments = $"--headless --convert-to pdf \"{rutaDocx}\" --outdir \"{carpetaSalida}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true,
                WorkingDirectory = _libreOfficePath
            };

            Debug.WriteLine($"[ConversorPdf] Ejecutando: {processInfo.FileName}");
            Debug.WriteLine($"[ConversorPdf] Argumentos: {processInfo.Arguments}");

            using (var process = Process.Start(processInfo))
            {
                if (process == null)
                {
                    Debug.WriteLine($"[ConversorPdf] No se pudo iniciar proceso");
                    return null;
                }

                // Esperar con timeout de 30 segundos
                bool completado = await Task.Run(() => process.WaitForExit(30000), ct);
                
                if (!completado)
                {
                    process.Kill();
                    Debug.WriteLine($"[ConversorPdf] Timeout esperando conversión (30s)");
                    return null;
                }

                // Capturar salida para debug
                string output = process.StandardOutput.ReadToEnd();
                string error = process.StandardError.ReadToEnd();
                
                Debug.WriteLine($"[ConversorPdf] Código salida: {process.ExitCode}");
                if (!string.IsNullOrEmpty(output))
                    Debug.WriteLine($"[ConversorPdf] Output: {output}");
                if (!string.IsNullOrEmpty(error))
                    Debug.WriteLine($"[ConversorPdf] Error: {error}");

                // Verificar que se creó el PDF
                if (File.Exists(rutaPdfEsperada))
                {
                    Debug.WriteLine($"[ConversorPdf] PDF creado exitosamente: {rutaPdfEsperada}");
                    return rutaPdfEsperada;
                }
                else
                {
                    Debug.WriteLine($"[ConversorPdf] PDF no se creó en: {rutaPdfEsperada}");
                    return null;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"[ConversorPdf] Excepción: {ex.Message}\n{ex.StackTrace}");
            return null;
        }
    }
}
