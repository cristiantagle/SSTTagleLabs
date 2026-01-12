using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.EntityFrameworkCore;
using System.Collections.ObjectModel;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;
using LiveChartsCore;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class DashboardViewModel : ObservableObject
{
    private readonly TagleLabsContext _db;
    private readonly DocumentoService _documentoService;
    private readonly IAsistenciaProSyncService? _syncService;

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EmpresaActual))]
    private Empresa? _empresaSeleccionada;

    // Cuando cambia la empresa seleccionada, recargar datos
    partial void OnEmpresaSeleccionadaChanged(Empresa? value)
    {
        if (value != null)
        {
            EmpresaActual = value;
            Task.Run(CargarDatosEmpresaAsync);
        }
    }

    [ObservableProperty]
    private Empresa? _empresaActual;

    [ObservableProperty]
    private ObservableCollection<ObligacionEmpresa> _obligaciones = new();

    [ObservableProperty]
    private ObservableCollection<DocumentoCard> _documentosRecientes = new();

    [ObservableProperty]
    private ObservableCollection<TagleLabsGestorSST.Data.Entities.MatrizRiesgoEmpresa> _riesgosCriticos = new();

    [ObservableProperty]
    private IndicadoresDashboard _indicadores = new();

    // ============================================
    // NUEVOS: Alertas y Vencimientos DS44
    // ============================================
    
    [ObservableProperty]
    private ObservableCollection<AlertaVencimiento> _alertasVencimiento = new();

    [ObservableProperty]
    private int _obligacionesVencidas;

    [ObservableProperty]
    private int _proximosVencimientos30Dias;

    [ObservableProperty]
    private double _porcentajeCumplimientoGlobal;

    // ============================================
    // NUEVO: Estado de AsistenciaPro
    // ============================================
    
    [ObservableProperty]
    private bool _asistenciaProEnabled;

    [ObservableProperty]
    private string _asistenciaProStatus = "No configurado";

    // ============================================
    // NUEVO: KPIs Enriquecidos
    // ============================================
    
    [ObservableProperty]
    private int _documentosEsteMes;

    [ObservableProperty]
    private int _trabajadoresSinDocumentacion;

    [ObservableProperty]
    private int _totalTrabajadores;

    // LiveCharts2 Series
    [ObservableProperty]
    private ISeries[] _cumplimientoSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private ISeries[] _siniestralidadSeries = Array.Empty<ISeries>();

    [ObservableProperty]
    private Axis[] _xAxes = Array.Empty<Axis>();

    public DashboardViewModel(TagleLabsContext db, DocumentoService documentoService, IAsistenciaProSyncService? syncService = null)
    {
        _db = db;
        _documentoService = documentoService;
        _syncService = syncService;
        
        // Estado de AsistenciaPro
        AsistenciaProEnabled = _syncService?.IsEnabled ?? false;
        AsistenciaProStatus = AsistenciaProEnabled ? "Conectado" : "No configurado";
        
        Task.Run(CargarDatosAsync);
    }

    private async Task CargarDatosAsync()
    {
        try
        {
            // 1. Cargar lista de empresas
            var empresasList = await _db.Empresas.ToListAsync();
            
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                Empresas = new ObservableCollection<Empresa>(empresasList);
                
                // Seleccionar primera empresa por defecto
                if (Empresas.Any())
                {
                    EmpresaSeleccionada = Empresas.First();
                }
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando empresas: {ex.Message}");
        }
    }

    private async Task CargarDatosEmpresaAsync()
    {
        if (EmpresaActual == null) return;
        
        try
        {
            var obligaciones = await _db.ObligacionesEmpresa
                .Include(o => o.Obligacion)
                .Where(o => o.EmpresaId == EmpresaActual.Id)
                .OrderByDescending(o => o.FechaLimite)
                .ToListAsync(); // Traemos todas para calcular estadísticas

            var documentos = await _db.DocumentosGenerados
                .OrderByDescending(d => d.FechaGenerado)
                .Take(5)
                .ToListAsync();

            // Siniestralidad últimos 6 meses
            var fechaInicio = DateTime.UtcNow.AddMonths(-6);
            var incidentes = await _db.Incidentes
                .Where(i => i.Fecha >= fechaInicio)
                .ToListAsync();

            var siniestrosCount = incidentes.Count;

            // Vigilancia Activa (Trabajadores con protocolos)
            var vigilanciaCount = await _db.Trabajadores
                .Where(t => t.ProtocolosVigilancia != null && t.ProtocolosVigilancia != "")
                .CountAsync();

            // ============================================
            // NUEVO: KPIs Enriquecidos
            // ============================================
            
            // Documentos generados este mes
            var primerDiaMes = new DateTime(DateTime.Now.Year, DateTime.Now.Month, 1);
            var documentosEsteMesCount = await _db.DocumentosGenerados
                .Where(d => d.FechaGenerado >= primerDiaMes && d.EmpresaId == EmpresaActual.Id)
                .CountAsync();

            // Trabajadores sin documentación completa (sin AFP, Salud, o FechaNacimiento)
            var trabajadoresTotal = await _db.Trabajadores
                .Where(t => t.CentroTrabajo != null && t.CentroTrabajo.EmpresaId == EmpresaActual.Id && t.Activo)
                .CountAsync();
            
            var trabajadoresSinDocs = await _db.Trabajadores
                .Where(t => t.CentroTrabajo != null && t.CentroTrabajo.EmpresaId == EmpresaActual.Id && t.Activo)
                .Where(t => t.AFP == null || t.AFP == "" || t.SistemaSalud == null || t.SistemaSalud == "" || t.Rut == null || t.Rut == "")
                .CountAsync();

            // Actualizar UI en el hilo principal
            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                // Lista de próximas (top 20)
                Obligaciones = new ObservableCollection<ObligacionEmpresa>(obligaciones.Take(20));
                
                DocumentosRecientes.Clear();
                foreach (var doc in documentos)
                {
                    var plantillaNombre = _db.Plantillas.Find(doc.PlantillaDocumentoId)?.Nombre ?? "Plantilla";
                    DocumentosRecientes.Add(new DocumentoCard
                    {
                        PlantillaNombre = plantillaNombre,
                        Fecha = doc.FechaGenerado,
                        Ruta = doc.RutaArchivoEditable
                    });
                }

                // Indicadores Texto
                var totalObligaciones = obligaciones.Count;
                var cumplidas = obligaciones.Count(o => o.Estado == EstadoObligacion.Cumplida);
                var pendientes = obligaciones.Count(o => o.Estado != EstadoObligacion.Cumplida);
                
                Indicadores = new IndicadoresDashboard
                {
                    ObligacionesPendientes = pendientes.ToString(),
                    PorcentajeCumplimiento = totalObligaciones > 0 ? $"{(int)((double)cumplidas / totalObligaciones * 100)}%" : "0%",
                    SiniestrosPeriodo = siniestrosCount.ToString(),
                    VigilanciaActiva = vigilanciaCount.ToString()
                };

                // ============================================
                // NUEVO: Asignar KPIs Enriquecidos
                // ============================================
                DocumentosEsteMes = documentosEsteMesCount;
                TrabajadoresSinDocumentacion = trabajadoresSinDocs;
                TotalTrabajadores = trabajadoresTotal;

                // ============================================
                // NUEVO: Calcular Vencimientos y Alertas DS44
                // ============================================
                var hoy = DateTime.Now.Date;
                var en30Dias = hoy.AddDays(30);

                // Obligaciones vencidas (tienen fecha límite pasada y no están cumplidas)
                ObligacionesVencidas = obligaciones.Count(o => 
                    o.FechaLimite.HasValue && 
                    o.FechaLimite.Value.Date < hoy && 
                    o.Estado != EstadoObligacion.Cumplida);

                // Próximos vencimientos (en los próximos 30 días)
                ProximosVencimientos30Dias = obligaciones.Count(o => 
                    o.FechaLimite.HasValue && 
                    o.FechaLimite.Value.Date >= hoy && 
                    o.FechaLimite.Value.Date <= en30Dias &&
                    o.Estado != EstadoObligacion.Cumplida);

                // Porcentaje de cumplimiento global
                PorcentajeCumplimientoGlobal = totalObligaciones > 0 
                    ? Math.Round((double)cumplidas / totalObligaciones * 100, 1) 
                    : 0;

                // Alertas de vencimiento (ordenadas por urgencia)
                var alertas = obligaciones
                    .Where(o => o.FechaLimite.HasValue && o.Estado != EstadoObligacion.Cumplida)
                    .OrderBy(o => o.FechaLimite)
                    .Take(10)
                    .Select(o => new AlertaVencimiento
                    {
                        ObligacionCodigo = o.Obligacion?.Codigo ?? "N/A",
                        Descripcion = o.Obligacion?.Descripcion ?? "Sin descripción",
                        FechaVencimiento = o.FechaLimite!.Value,
                        DiasRestantes = (o.FechaLimite!.Value.Date - hoy).Days,
                        EsVencida = o.FechaLimite!.Value.Date < hoy,
                        EsUrgente = (o.FechaLimite!.Value.Date - hoy).Days <= 7 && (o.FechaLimite!.Value.Date - hoy).Days >= 0
                    })
                    .ToList();

                AlertasVencimiento = new ObservableCollection<AlertaVencimiento>(alertas);

                // Gráfico Circular: Cumplimiento
                ConfigurarGraficoCumplimiento(cumplidas, pendientes);

                // Gráfico Barras: Siniestralidad Mensual
                ConfigurarGraficoSiniestralidad(incidentes);

                // Top 5 riesgos (global)
                var riesgosGlobales = _db.MatrizRiesgosEmpresa
                    .Include(r => r.Peligro)
                    .Include(r => r.Tarea)
                    .OrderByDescending(r => r.Probabilidad * r.Consecuencia)
                    .Take(5)
                    .ToList();

                RiesgosCriticos = new ObservableCollection<TagleLabsGestorSST.Data.Entities.MatrizRiesgoEmpresa>(riesgosGlobales);
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error cargando dashboard: {ex.Message}");
        }
    }

    private void ConfigurarGraficoCumplimiento(int cumplidas, int pendientes)
    {
        if (cumplidas == 0 && pendientes == 0)
        {
            CumplimientoSeries = new ISeries[]
            {
                new PieSeries<int>
                {
                    Values = new int[] { 1 },
                    Name = "Sin Datos",
                    Fill = new SolidColorPaint(new SKColor(226, 232, 240)), // Slate 200
                    InnerRadius = 50,
                    HoverPushout = 0,
                    //TooltipLabelFormatter = point => "Sin datos registrados" // Obsolete
                }
            };
            return;
        }

        CumplimientoSeries = new ISeries[]
        {
            new PieSeries<int>
            {
                Values = new int[] { cumplidas },
                Name = "Cumplidas",
                Fill = new SolidColorPaint(SKColors.Teal),
                InnerRadius = 50
            },
            new PieSeries<int>
            {
                Values = new int[] { pendientes },
                Name = "Pendientes",
                Fill = new SolidColorPaint(SKColors.OrangeRed),
                InnerRadius = 50
            }
        };
    }

    private void ConfigurarGraficoSiniestralidad(List<Incidente> incidentes)
    {
        var meses = new List<string>();
        var valores = new List<int>();

        for (int i = 5; i >= 0; i--)
        {
            var mes = DateTime.Now.AddMonths(-i);
            meses.Add(mes.ToString("MMM"));
            var count = incidentes.Count(inc => inc.Fecha.Month == mes.Month && inc.Fecha.Year == mes.Year);
            valores.Add(count);
        }

        SiniestralidadSeries = new ISeries[]
        {
            new ColumnSeries<int>
            {
                Values = valores.ToArray(),
                Name = "Incidentes",
                Fill = new SolidColorPaint(SKColors.IndianRed)
            }
        };

        XAxes = new Axis[]
        {
            new Axis
            {
                Labels = meses.ToArray(),
                LabelsRotation = 0,
                SeparatorsPaint = new SolidColorPaint(new SKColor(200, 200, 200)),
                SeparatorsAtCenter = false,
                TicksPaint = new SolidColorPaint(new SKColor(35, 35, 35)),
                TicksAtCenter = true
            }
        };
    }

    [RelayCommand]
    private void AbrirDocumento(string ruta)
    {
        if (string.IsNullOrEmpty(ruta) || !System.IO.File.Exists(ruta)) return;
        
        try
        {
            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
            {
                FileName = ruta,
                UseShellExecute = true
            });
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error abriendo documento: {ex.Message}");
        }
    }
}

public class DocumentoCard
{
    public string PlantillaNombre { get; set; } = string.Empty;
    public DateTime Fecha { get; set; }
    public string Ruta { get; set; } = string.Empty;
}

public class IndicadoresDashboard
{
    public string ObligacionesPendientes { get; set; } = "0";
    public string PorcentajeCumplimiento { get; set; } = "0%";
    public string SiniestrosPeriodo { get; set; } = "0";
    public string VigilanciaActiva { get; set; } = "0";
}

/// <summary>
/// Representa una alerta de vencimiento de obligación DS44
/// </summary>
public class AlertaVencimiento
{
    public string ObligacionCodigo { get; set; } = string.Empty;
    public string Descripcion { get; set; } = string.Empty;
    public DateTime FechaVencimiento { get; set; }
    public int DiasRestantes { get; set; }
    public bool EsVencida { get; set; }
    public bool EsUrgente { get; set; }
    
    public string EstadoTexto => EsVencida ? "VENCIDA" : (EsUrgente ? "URGENTE" : $"{DiasRestantes} días");
    public string ColorEstado => EsVencida ? "#DC2626" : (EsUrgente ? "#F59E0B" : "#10B981");
}
