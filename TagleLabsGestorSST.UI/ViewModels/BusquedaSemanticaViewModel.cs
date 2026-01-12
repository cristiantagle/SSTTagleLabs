using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

/// <summary>
/// TIER 3: ViewModel para búsqueda semántica en la base de conocimientos
/// Usa embeddings de Gemini para encontrar documentos relevantes
/// </summary>
public partial class BusquedaSemanticaViewModel : ObservableObject
{
    private readonly ILocalAiService _aiService;
    private readonly INotificationService _notificationService;

    [ObservableProperty]
    private string _consulta = string.Empty;

    [ObservableProperty]
    private string _resultados = string.Empty;

    [ObservableProperty]
    private bool _buscando;

    [ObservableProperty]
    private string _estadoBusqueda = "Listo para buscar";

    [ObservableProperty]
    private ObservableCollection<ResultadoBusqueda> _resultadosLista = new();

    [ObservableProperty]
    private int _totalResultados;

    [ObservableProperty]
    private string _tiempoBusqueda = string.Empty;

    public BusquedaSemanticaViewModel(ILocalAiService aiService, INotificationService notificationService)
    {
        _aiService = aiService;
        _notificationService = notificationService;
    }

    [RelayCommand]
    private async Task Buscar()
    {
        if (string.IsNullOrWhiteSpace(Consulta))
        {
            _notificationService.ShowWarning("Ingrese una consulta de búsqueda.");
            return;
        }

        if (Consulta.Length < 3)
        {
            _notificationService.ShowWarning("La consulta debe tener al menos 3 caracteres.");
            return;
        }

        Buscando = true;
        EstadoBusqueda = "Generando embedding de consulta...";
        ResultadosLista.Clear();
        TotalResultados = 0;
        TiempoBusqueda = "";

        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        try
        {
            // 1. Verificar disponibilidad
            var disponible = await _aiService.VerificarDisponibilidadAsync();
            if (!disponible)
            {
                _notificationService.ShowError("Servicio de IA no disponible. Verifique la configuración de API.");
                return;
            }

            EstadoBusqueda = "Buscando en base de conocimientos...";

            // 2. Usar el método de análisis con contexto RAG (internamente usa embeddings)
            var respuesta = await _aiService.AnalizarTextoAsync(
                $"Busca información relacionada con: {Consulta}. " +
                "Lista los documentos y secciones más relevantes encontrados. " +
                "Incluye citas textuales breves de cada fuente encontrada. " +
                "Responde en formato estructurado con viñetas.",
                "",
                temperatura: 0.2
            );

            stopwatch.Stop();

            // 3. Mostrar resultados
            Resultados = respuesta;
            TiempoBusqueda = $"Búsqueda completada en {stopwatch.ElapsedMilliseconds}ms";
            EstadoBusqueda = "Búsqueda completada";

            // Parsear resultados (simplificado - la respuesta es texto)
            var lineas = respuesta.Split('\n', StringSplitOptions.RemoveEmptyEntries);
            int contador = 0;
            foreach (var linea in lineas.Take(20))
            {
                if (linea.Trim().StartsWith("-") || linea.Trim().StartsWith("•") || linea.Trim().StartsWith("*"))
                {
                    contador++;
                    ResultadosLista.Add(new ResultadoBusqueda
                    {
                        Numero = contador,
                        Contenido = linea.TrimStart('-', '•', '*', ' '),
                        Relevancia = Math.Max(0.5, 1.0 - (contador * 0.1))
                    });
                }
            }

            TotalResultados = ResultadosLista.Count;

            if (TotalResultados == 0)
            {
                EstadoBusqueda = "No se encontraron resultados estructurados, pero hay una respuesta.";
            }

            _notificationService.ShowSuccess($"Búsqueda completada: {TotalResultados} resultados");
        }
        catch (Exception ex)
        {
            EstadoBusqueda = "Error en la búsqueda";
            Resultados = $"Error: {ex.Message}";
            _notificationService.ShowError($"Error en búsqueda: {ex.Message}");
        }
        finally
        {
            Buscando = false;
        }
    }

    [RelayCommand]
    private void LimpiarBusqueda()
    {
        Consulta = string.Empty;
        Resultados = string.Empty;
        ResultadosLista.Clear();
        TotalResultados = 0;
        TiempoBusqueda = string.Empty;
        EstadoBusqueda = "Listo para buscar";
    }

    [RelayCommand]
    private async Task PreguntarSobreResultado(ResultadoBusqueda? resultado)
    {
        if (resultado == null) return;

        Buscando = true;
        EstadoBusqueda = "Ampliando información...";

        try
        {
            var respuesta = await _aiService.AnalizarTextoAsync(
                $"Amplía la siguiente información con más detalle, citando normativa chilena aplicable: {resultado.Contenido}",
                "",
                temperatura: 0.4
            );

            Resultados = $"### Detalle ampliado:\n\n{respuesta}";
            EstadoBusqueda = "Información ampliada";
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            Buscando = false;
        }
    }
}

/// <summary>
/// Modelo para un resultado de búsqueda semántica
/// </summary>
public class ResultadoBusqueda
{
    public int Numero { get; set; }
    public string Contenido { get; set; } = string.Empty;
    public double Relevancia { get; set; }
    public string Fuente { get; set; } = "Base de Conocimientos";
}
