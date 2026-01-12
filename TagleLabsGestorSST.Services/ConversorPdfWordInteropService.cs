using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.Versioning;
using System.Threading;
using System.Threading.Tasks;

namespace TagleLabsGestorSST.Services
{
    /// <summary>
    /// Servicio de conversión DOCX→PDF usando Word Interop de Microsoft Office.
    /// Prioridad máxima: si Office está instalado, esta es la mejor opción.
    /// Requiere: Microsoft Office 2016+ instalado en la máquina.
    /// </summary>
    public interface IConversorPdfWordInteropService
    {
        /// <summary>
        /// Detecta si Microsoft Office está instalado y accesible.
        /// </summary>
        bool EstaMicrosoftOfficeInstalado();

        /// <summary>
        /// Convierte un DOCX a PDF usando Word Interop.
        /// Proporciona 100% fidelidad ya que usa la API oficial de Microsoft.
        /// </summary>
        Task<string?> ConvertirDocxAPdfAsync(string rutaDocx, CancellationToken ct = default);

        /// <summary>
        /// Obtiene el mensaje de error del último intento fallido.
        /// </summary>
        string? ObtenerMensajeError();
    }

    [SupportedOSPlatform("windows")]
    public class ConversorPdfWordInteropService : IConversorPdfWordInteropService
    {
        private string? _mensajeError;
        private const int TIMEOUT_SEGUNDOS = 60;

        public bool EstaMicrosoftOfficeInstalado()
        {
            try
            {
                // Intenta crear un objeto Word.Application
                // Si falla, Office no está instalado
                var wordType = Type.GetTypeFromProgID("Word.Application");
                
                if (wordType == null)
                {
                    _mensajeError = "Microsoft Office no está instalado en esta máquina.";
                    Debug.WriteLine($"[ConversorPdfWordInterop] {_mensajeError}");
                    return false;
                }

                // Intenta instanciar brevemente para verificar que funciona
                var wordApp = Activator.CreateInstance(wordType);
                if (wordApp == null)
                {
                    _mensajeError = "No se pudo inicializar Microsoft Office.";
                    Debug.WriteLine($"[ConversorPdfWordInterop] {_mensajeError}");
                    return false;
                }

                // Limpia
                try { Marshal.ReleaseComObject(wordApp); } catch { }

                Debug.WriteLine("[ConversorPdfWordInterop] Microsoft Office detectado correctamente");
                return true;
            }
            catch (Exception ex)
            {
                _mensajeError = $"Error detectando Office: {ex.Message}";
                Debug.WriteLine($"[ConversorPdfWordInterop] {_mensajeError}");
                return false;
            }
        }

        public async Task<string?> ConvertirDocxAPdfAsync(string rutaDocx, CancellationToken ct = default)
        {
            if (!File.Exists(rutaDocx))
            {
                _mensajeError = $"Archivo DOCX no encontrado: {rutaDocx}";
                Debug.WriteLine($"[ConversorPdfWordInterop] {_mensajeError}");
                return null;
            }

            dynamic? wordApp = null;
            dynamic? doc = null;

            try
            {
                return await Task.Run(() =>
                {
                    // Crear instancia de Word
                    var wordType = Type.GetTypeFromProgID("Word.Application");
                    if (wordType == null)
                    {
                        _mensajeError = "Microsoft Office no está disponible.";
                        return null;
                    }

                    wordApp = Activator.CreateInstance(wordType);
                    if (wordApp == null)
                    {
                        _mensajeError = "No se pudo crear instancia de Word.Application.";
                        return null;
                    }

                    wordApp.Visible = false;
                    wordApp.ScreenUpdating = false;

                    // Abrir documento DOCX
                    var rutaAbsoluta = Path.GetFullPath(rutaDocx);
                    Debug.WriteLine($"[ConversorPdfWordInterop] Abriendo documento: {rutaAbsoluta}");

                    // FIX: Abrir en modo editable (no ReadOnly) para que Word ejecute todos los cálculos
                    doc = wordApp.Documents.Open(rutaAbsoluta, false, false, false);

                    // IMPORTANTE: Actualizar TODOS los campos incluyendo TOC (Tabla de Contenido)
                    try
                    {
                        // Actualizar tabla de contenido específicamente
                        foreach (dynamic toc in doc.TablesOfContents)
                        {
                            toc.Update();
                        }
                    }
                    catch { /* No hay TOC, continuar */ }

                    // Actualizar todos los demás campos
                    doc.Fields.Update();
                    
                    // Forzar repaginación completa
                    doc.Repaginate();
                    
                    // Esperar a que Office termine de procesar
                    System.Threading.Thread.Sleep(1000);

                    // Generar PDF con el mismo nombre pero extensión .pdf
                    var directorioDocx = Path.GetDirectoryName(rutaDocx) ?? Directory.GetCurrentDirectory();
                    var rutaPdf = Path.Combine(
                        directorioDocx,
                        Path.GetFileNameWithoutExtension(rutaDocx) + ".pdf"
                    );

                    // Constante de Word: 17 = wdFormatPDF
                    doc.SaveAs(rutaPdf, 17);

                    Debug.WriteLine($"[ConversorPdfWordInterop] PDF generado exitosamente: {rutaPdf}");

                    // Verificar que se creó
                    if (File.Exists(rutaPdf))
                    {
                        _mensajeError = null;
                        return rutaPdf;
                    }

                    _mensajeError = "PDF no se creó correctamente.";
                    return null;
                }, ct);
            }
            catch (OperationCanceledException)
            {
                _mensajeError = "Operación cancelada por el usuario.";
                Debug.WriteLine($"[ConversorPdfWordInterop] {_mensajeError}");
                return null;
            }
            catch (Exception ex)
            {
                _mensajeError = $"Error en conversión Word Interop: {ex.Message}";
                Debug.WriteLine($"[ConversorPdfWordInterop] {_mensajeError}");
                return null;
            }
            finally
            {
                // Limpieza: cerrar documento y Word
                try
                {
                    if (doc != null)
                    {
                        doc.Close(false); // false = no guardar cambios
                    }
                }
                catch { }

                try
                {
                    if (wordApp != null)
                    {
                        wordApp.Quit();
                    }
                }
                catch { }

                // Liberar referencias COM
                try
                {
                    if (doc != null) Marshal.ReleaseComObject(doc);
                    if (wordApp != null) Marshal.ReleaseComObject(wordApp);
                    GC.Collect();
                    GC.WaitForPendingFinalizers();
                }
                catch { }
            }
        }

        public string? ObtenerMensajeError()
        {
            return _mensajeError;
        }
    }
}
