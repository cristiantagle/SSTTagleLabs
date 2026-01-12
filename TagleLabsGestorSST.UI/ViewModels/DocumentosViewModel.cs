using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;
using Microsoft.EntityFrameworkCore;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class DocumentosViewModel : ObservableObject
{
    private readonly DocumentoService _documentoService;
    private readonly IEmpresaService _empresaService;
    private readonly ITrabajadorService _trabajadorService;
    private readonly TagleLabsContext _db;

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();
    [ObservableProperty]
    private Empresa? _empresaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<PlantillaDocumento> _plantillas = new();
    [ObservableProperty]
    private PlantillaDocumento? _plantillaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<Trabajador> _trabajadores = new();
    [ObservableProperty]
    private Trabajador? _trabajadorSeleccionado;

    [ObservableProperty]
    private ObservableCollection<DocumentoGenerado> _historial = new();
    [ObservableProperty]
    private DocumentoGenerado? _documentoSeleccionado;

    // Propiedades para Vacaciones
    [ObservableProperty]
    private double _diasGanados;
    [ObservableProperty]
    private int _diasUsados;
    [ObservableProperty]
    private double _diasDisponibles;
    [ObservableProperty]
    private int _diasSolicitados;

    // Propiedades para Validación IA
    [ObservableProperty]
    private int? _ultimoScoreIA;
    [ObservableProperty]
    private bool? _ultimoAprobadoIA;
    [ObservableProperty]
    private string? _ultimoResumenIA;
    [ObservableProperty]
    private ObservableCollection<string> _mejorasIA = new();
    [ObservableProperty]
    private bool _mostrarValidacionIA;

    // Propiedades para Modal de Carga
    [ObservableProperty]
    private bool _isGenerando;
    [ObservableProperty]
    private string _progresoTexto = "";
    [ObservableProperty]
    private int _progresoValor;

    private readonly IConfiguracionService _configService;

    public DocumentosViewModel(DocumentoService documentoService, IEmpresaService empresaService, ITrabajadorService trabajadorService, TagleLabsContext db, IConfiguracionService configService)
    {
        _documentoService = documentoService;
        _empresaService = empresaService;
        _trabajadorService = trabajadorService;
        _db = db;
        _configService = configService;
        CargarDatosInicialesCommand.Execute(null);
    }

    [RelayCommand]
    private async Task CargarDatosIniciales()
    {
        // Inicializar plantillas base si no existen
        await _documentoService.InicializarPlantillasBase();

        var empresas = await _empresaService.ObtenerEmpresasAsync();
        Empresas = new ObservableCollection<Empresa>(empresas);
        if (Empresas.Any()) EmpresaSeleccionada = Empresas.First();

        var plantillas = await _db.Plantillas.ToListAsync();
        Plantillas = new ObservableCollection<PlantillaDocumento>(plantillas);
    }

    partial void OnEmpresaSeleccionadaChanged(Empresa? value)
    {
        if (value != null)
        {
            CargarTrabajadoresCommand.Execute(null);
            CargarHistorialCommand.Execute(null);
        }
    }

    async partial void OnTrabajadorSeleccionadoChanged(Trabajador? value)
    {
        if (value != null)
        {
            var (ganados, usados, disponibles) = await _trabajadorService.CalcularVacacionesAsync(value.Id);
            DiasGanados = ganados;
            DiasUsados = usados;
            DiasDisponibles = disponibles;
            CalcularDiasSolicitados();
        }
    }

    partial void OnDocumentoSeleccionadoChanged(DocumentoGenerado? value)
    {
        if (value != null && value.ScoreCumplimiento.HasValue)
        {
            UltimoScoreIA = value.ScoreCumplimiento;
            UltimoAprobadoIA = value.AprobadoPorIA;
            UltimoResumenIA = value.ResumenValidacionIA;
            MostrarValidacionIA = true;
            
            // Parsear mejoras del JSON
            MejorasIA.Clear();
            if (!string.IsNullOrWhiteSpace(value.MejorasSugeridas))
            {
                try
                {
                    var mejoras = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(value.MejorasSugeridas);
                    if (mejoras != null)
                    {
                        foreach (var m in mejoras) MejorasIA.Add(m);
                    }
                }
                catch { /* Ignorar errores de parsing */ }
            }
        }
        else if (value != null)
        {
            // Documento sin validación IA (anterior a la migración)
            MostrarValidacionIA = false;
            MejorasIA.Clear();
        }
    }

    partial void OnFechaInicioChanged(DateTime? value) => CalcularDiasSolicitados();
    partial void OnFechaFinChanged(DateTime? value) => CalcularDiasSolicitados();

    private void CalcularDiasSolicitados()
    {
        if (FechaInicio.HasValue && FechaFin.HasValue && FechaFin > FechaInicio)
        {
            // Cálculo simple de días corridos por ahora.
            // Idealmente debería excluir fines de semana y feriados.
            DiasSolicitados = (FechaFin.Value - FechaInicio.Value).Days;
        }
        else
        {
            DiasSolicitados = 0;
        }
    }

    [RelayCommand]
    private async Task CargarTrabajadores()
    {
        if (EmpresaSeleccionada == null) return;
        var todos = await _trabajadorService.ObtenerTodosAsync();
        // Filtrar por empresa
        var filtrados = todos.Where(t => t.CentroTrabajo != null && t.CentroTrabajo.EmpresaId == EmpresaSeleccionada.Id).ToList();
        Trabajadores = new ObservableCollection<Trabajador>(filtrados);
    }

    [RelayCommand]
    private async Task CargarHistorial()
    {
        if (EmpresaSeleccionada == null) return;
        var docs = await _db.DocumentosGenerados
            .Include(d => d.Plantilla)
            .Include(d => d.Empresa)
            .Include(d => d.Trabajador)
            .Where(d => d.EmpresaId == EmpresaSeleccionada.Id)
            .OrderByDescending(d => d.FechaGenerado)
            .ToListAsync();
        Historial = new ObservableCollection<DocumentoGenerado>(docs);
    }

    [ObservableProperty]
    private DateTime? _fechaInicio = DateTime.Today;
    [ObservableProperty]
    private DateTime? _fechaFin = DateTime.Today.AddDays(1);

    [RelayCommand]
    private async Task GenerarDocumento()
    {
        if (EmpresaSeleccionada == null || PlantillaSeleccionada == null) return;

        // Verificar Carpeta Maestra
        var masterPath = await _configService.GetValorAsync("MasterFolderPath");
        if (string.IsNullOrEmpty(masterPath))
        {
            // Prompt user to select folder
            var dialog = new Microsoft.Win32.OpenFolderDialog
            {
                Title = "Seleccione la CARPETA MAESTRA donde se guardarán todos los documentos",
                Multiselect = false
            };

            if (dialog.ShowDialog() == true)
            {
                masterPath = dialog.FolderName;
                await _configService.SetValorAsync("MasterFolderPath", masterPath);
            }
            else
            {
                return; // Cancelado por usuario
            }
        }

        // Validación de Vacaciones (si la plantilla es de vacaciones)
        if (PlantillaSeleccionada.Codigo.Contains("VACAC", StringComparison.OrdinalIgnoreCase) && TrabajadorSeleccionado != null)
        {
            if (DiasSolicitados > DiasDisponibles)
            {
                System.Windows.MessageBox.Show(
                    $"No se puede generar la solicitud de vacaciones.\n\n" +
                    $"Días solicitados: {DiasSolicitados}\n" +
                    $"Días disponibles: {DiasDisponibles:F1}\n\n" +
                    $"El trabajador no tiene suficientes días disponibles.",
                    "Saldo Insuficiente",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                return;
            }
        }

        // Preparar datos
        var datos = new Dictionary<string, string>
        {
            { "RAZON_SOCIAL", EmpresaSeleccionada.RazonSocial },
            { "RUT_EMPRESA", EmpresaSeleccionada.Rut },
            { "DIRECCION_EMPRESA", EmpresaSeleccionada.Direccion ?? "Sin dirección" },
            { "FECHA_ACTUAL", DateTime.Now.ToString("dd/MM/yyyy") }
        };

        if (FechaInicio.HasValue) datos.Add("FECHA_INICIO", FechaInicio.Value.ToString("dd/MM/yyyy"));
        if (FechaFin.HasValue) datos.Add("FECHA_FIN", FechaFin.Value.ToString("dd/MM/yyyy"));

        if (TrabajadorSeleccionado != null)
        {
            datos.Add("NOMBRE_TRABAJADOR", TrabajadorSeleccionado.NombreCompleto);
            datos.Add("RUT_TRABAJADOR", TrabajadorSeleccionado.Rut);
            datos.Add("CARGO_TRABAJADOR", TrabajadorSeleccionado.Cargo ?? "Sin cargo");
            
            // Datos de Vacaciones
            datos.Add("DIAS_GANADOS", DiasGanados.ToString("F1"));
            datos.Add("DIAS_USADOS_HISTORICOS", DiasUsados.ToString());
            datos.Add("DIAS_DISPONIBLES", DiasDisponibles.ToString("F1"));
            datos.Add("DIAS_SOLICITADOS", DiasSolicitados.ToString());
            datos.Add("SALDO_POSTERIOR", (DiasDisponibles - DiasSolicitados).ToString("F1"));
        }
        else
        {
            datos.Add("NOMBRE_TRABAJADOR", "____________________");
            datos.Add("RUT_TRABAJADOR", "____________________");
            datos.Add("CARGO_TRABAJADOR", "____________________");
        }

        // === INICIO: Modal de Carga ===
        IsGenerando = true;
        ProgresoTexto = "📄 Preparando documento...";
        ProgresoValor = 10;

        try
        {
            ProgresoTexto = "⚙️ Generando documento y PDF...";
            ProgresoValor = 30;
            
            // Generar
            var doc = await _documentoService.GenerarDocumentoAsync(EmpresaSeleccionada.Id, PlantillaSeleccionada.Codigo, datos, TrabajadorSeleccionado?.Id);
            
            ProgresoTexto = "🤖 Validación IA completada";
            ProgresoValor = 100;
            
            // Mostrar resultado de validación IA
            if (doc != null)
            {
                UltimoScoreIA = doc.ScoreCumplimiento;
                UltimoAprobadoIA = doc.AprobadoPorIA;
                UltimoResumenIA = doc.ResumenValidacionIA;
                MostrarValidacionIA = doc.ScoreCumplimiento.HasValue;
                
                // Parsear mejoras del JSON
                MejorasIA.Clear();
                if (!string.IsNullOrWhiteSpace(doc.MejorasSugeridas))
                {
                    try
                    {
                        var mejoras = Newtonsoft.Json.JsonConvert.DeserializeObject<List<string>>(doc.MejorasSugeridas);
                        if (mejoras != null)
                        {
                            foreach (var m in mejoras) MejorasIA.Add(m);
                        }
                    }
                    catch { /* Ignorar errores de parsing */ }
                }
            }
            else
            {
                MostrarValidacionIA = false;
                MejorasIA.Clear();
        }
        
        // Recargar historial
        await CargarHistorial();
        }
        finally
        {
            // === FIN: Modal de Carga ===
            IsGenerando = false;
            ProgresoTexto = "";
            ProgresoValor = 0;
        }
    }

    [RelayCommand]
    private void AbrirDocumento()
    {
        if (DocumentoSeleccionado == null) return;
        AbrirArchivo(DocumentoSeleccionado.RutaArchivoEditable);
    }

    [RelayCommand]
    private void AbrirPdf()
    {
        if (DocumentoSeleccionado == null) return;
        AbrirArchivo(DocumentoSeleccionado.RutaArchivoPdf);
    }

    private void AbrirArchivo(string path)
    {
        if (File.Exists(path))
        {
            try
            {
                Process.Start(new ProcessStartInfo(path) { UseShellExecute = true });
            }
            catch { /* Manejar error */ }
        }
    }
}
