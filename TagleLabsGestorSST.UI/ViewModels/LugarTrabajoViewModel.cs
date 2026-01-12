using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

/// <summary>
/// Item con checkbox para selección múltiple de trabajadores
/// </summary>
public partial class TrabajadorSeleccionable : ObservableObject
{
    public Trabajador Trabajador { get; set; } = null!;
    
    [ObservableProperty]
    private bool isSelected;
}

/// <summary>
/// ViewModel para gestión de documentos por lugar de trabajo
/// </summary>
public partial class LugarTrabajoViewModel : ObservableObject
{
    private readonly ILugarTrabajoService _lugarTrabajoService;
    private readonly ITrabajadorService _trabajadorService;
    private readonly DocumentoService _documentoService;
    private readonly INotificationService _notificationService;

    public LugarTrabajoViewModel(
        ILugarTrabajoService lugarTrabajoService,
        ITrabajadorService trabajadorService,
        DocumentoService documentoService,
        INotificationService notificationService)
    {
        _lugarTrabajoService = lugarTrabajoService;
        _trabajadorService = trabajadorService;
        _documentoService = documentoService;
        _notificationService = notificationService;
        
        _ = CargarDatosAsync();
    }

    // ==================== PROPIEDADES OBSERVABLES ====================
    
    [ObservableProperty]
    private ObservableCollection<LugarTrabajo> lugares = new();
    
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(TieneLugarSeleccionado))]
    private LugarTrabajo? lugarSeleccionado;
    
    [ObservableProperty]
    private ObservableCollection<PlantillaLugarTrabajo> plantillas = new();
    
    [ObservableProperty]
    private PlantillaLugarTrabajo? plantillaSeleccionada;
    
    [ObservableProperty]
    private ObservableCollection<EstadoDocumentacionDto> estadosTrabajadores = new();
    
    [ObservableProperty]
    private EstadoDocumentacionDto? trabajadorSeleccionado;
    
    [ObservableProperty]
    private ObservableCollection<TrabajadorSeleccionable> trabajadoresDisponibles = new();
    
    [ObservableProperty]
    private ObservableCollection<CharlaSST> charlas = new();
    
    // Para crear nuevo lugar
    [ObservableProperty]
    private string nuevoLugarNombre = string.Empty;
    
    [ObservableProperty]
    private string nuevoLugarCodigo = string.Empty;
    
    // Para crear nueva charla
    [ObservableProperty]
    private string nuevaCharlaTema = string.Empty;
    
    [ObservableProperty]
    private DateTime nuevaCharlaFecha = DateTime.Now;
    
    [ObservableProperty]
    private string nuevaCharlaExpositor = string.Empty;
    
    [ObservableProperty]
    private int nuevaCharlaDuracion = 30;
    
    [ObservableProperty]
    private string nuevaCharlaHora = "";
    
    // Checkboxes de Objetivo
    [ObservableProperty]
    private bool objCharla = true;
    
    [ObservableProperty]
    private bool objCapacitacion;
    
    [ObservableProperty]
    private bool objReinduccion;
    
    [ObservableProperty]
    private bool objInspeccion;
    
    [ObservableProperty]
    private bool objOtros;
    
    [ObservableProperty]
    private string nuevaCharlaObservaciones = "";
    
    [ObservableProperty]
    private string nuevaCharlaAcuerdos = "";
    
    // Estado UI
    [ObservableProperty]
    private bool isBusy;
    
    [ObservableProperty]
    private string statusMessage = string.Empty;
    
    // Propiedades calculadas
    public bool TieneLugarSeleccionado => LugarSeleccionado != null;
    public bool TienePlantillaSeleccionada => PlantillaSeleccionada != null;
    
    public int TrabajadoresSeleccionadosCount => 
        TrabajadoresDisponibles?.Count(t => t.IsSelected) ?? 0;
    
    // Lista de plantillas para dropdown (alias de Plantillas)
    public ObservableCollection<PlantillaLugarTrabajo> PlantillasDisponibles => Plantillas;
    
    [ObservableProperty]
    private bool seleccionarTodos;
    
    partial void OnSeleccionarTodosChanged(bool value)
    {
        foreach (var t in TrabajadoresDisponibles)
        {
            t.IsSelected = value;
        }
        OnPropertyChanged(nameof(TrabajadoresSeleccionadosCount));
    }
    
    partial void OnPlantillaSeleccionadaChanged(PlantillaLugarTrabajo? value)
    {
        OnPropertyChanged(nameof(TienePlantillaSeleccionada));
    }
    
    // ==================== CARGA DE DATOS ====================
    
    private async Task CargarDatosAsync()
    {
        try
        {
            IsBusy = true;
            var lugaresDb = await _lugarTrabajoService.GetAllAsync();
            Lugares = new ObservableCollection<LugarTrabajo>(lugaresDb);
            
            // Cargar trabajadores disponibles
            var trabajadores = await _trabajadorService.ObtenerTodosAsync();
            TrabajadoresDisponibles = new ObservableCollection<TrabajadorSeleccionable>(
                trabajadores.Select(t => new TrabajadorSeleccionable { Trabajador = t })
            );
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error al cargar datos: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    partial void OnLugarSeleccionadoChanged(LugarTrabajo? value)
    {
        if (value != null)
        {
            _ = CargarDatosLugarAsync(value.Id);
        }
        else
        {
            Plantillas.Clear();
            EstadosTrabajadores.Clear();
            Charlas.Clear();
        }
    }
    
    private async Task CargarDatosLugarAsync(int lugarId)
    {
        try
        {
            IsBusy = true;
            
            // Cargar plantillas
            var plantillasDb = await _lugarTrabajoService.GetPlantillasAsync(lugarId);
            Plantillas = new ObservableCollection<PlantillaLugarTrabajo>(plantillasDb);
            
            // Cargar estado de trabajadores (semáforo)
            var estados = await _lugarTrabajoService.GetEstadoTodosLosTrabajadioresAsync(lugarId);
            EstadosTrabajadores = new ObservableCollection<EstadoDocumentacionDto>(estados);
            
            // Cargar charlas recientes
            var charlasDb = await _lugarTrabajoService.GetCharlasAsync(lugarId);
            Charlas = new ObservableCollection<CharlaSST>(charlasDb);
            
            // Marcar trabajadores asignados
            var trabajadoresAsignados = await _lugarTrabajoService.GetTrabajadoresAsignadosAsync(lugarId);
            var idsAsignados = trabajadoresAsignados.Select(t => t.Id).ToHashSet();
            
            foreach (var ts in TrabajadoresDisponibles)
            {
                ts.IsSelected = idsAsignados.Contains(ts.Trabajador.Id);
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    // ==================== COMANDOS CRUD LUGAR ====================
    
    [RelayCommand]
    private async Task CrearLugarAsync()
    {
        if (string.IsNullOrWhiteSpace(NuevoLugarNombre) || string.IsNullOrWhiteSpace(NuevoLugarCodigo))
        {
            _notificationService.ShowWarning("Debe ingresar nombre y código del lugar de trabajo");
            return;
        }
        
        try
        {
            IsBusy = true;
            var nuevoLugar = new LugarTrabajo
            {
                Nombre = NuevoLugarNombre,
                Codigo = NuevoLugarCodigo.ToUpper()
            };
            
            await _lugarTrabajoService.CreateAsync(nuevoLugar);
            Lugares.Add(nuevoLugar);
            LugarSeleccionado = nuevoLugar;
            
            NuevoLugarNombre = string.Empty;
            NuevoLugarCodigo = string.Empty;
            
            _notificationService.ShowSuccess($"Lugar de trabajo '{nuevoLugar.Nombre}' creado");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task EliminarLugarAsync()
    {
        if (LugarSeleccionado == null) return;
        
        try
        {
            await _lugarTrabajoService.DeleteAsync(LugarSeleccionado.Id);
            Lugares.Remove(LugarSeleccionado);
            LugarSeleccionado = null;
            _notificationService.ShowSuccess("Lugar de trabajo eliminado");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error: {ex.Message}");
        }
    }
    
    /// <summary>
    /// Escanea la carpeta Templates/ para detectar plantillas automáticamente
    /// </summary>
    [RelayCommand]
    private async Task EscanearPlantillasAsync()
    {
        try
        {
            IsBusy = true;
            StatusMessage = "Escaneando carpeta de plantillas...";
            
            // Buscar plantillas en carpeta relativa al programa
            var baseDir = AppDomain.CurrentDomain.BaseDirectory;
            var documentsDir = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            
            // Rutas portables - NO hardcodeadas
            var rutasPosibles = new[]
            {
                // 1. Carpeta junto al ejecutable
                Path.Combine(baseDir, "Templates"),
                // 2. Carpeta Documents del usuario
                Path.Combine(documentsDir, "TagleLabsSST", "Templates"),
                // 3. Desarrollo: subiendo desde bin/Debug/net8.0
                Path.Combine(baseDir, "..", "..", "..", "..", "Templates", "LugaresTrabajo"),
                // 4. Directorio actual de trabajo
                Path.Combine(Directory.GetCurrentDirectory(), "Templates", "LugaresTrabajo")
            };
            
            var plantillasEncontradas = new List<PlantillaLugarTrabajo>();
            var rutaEncontrada = "";
            
            foreach (var ruta in rutasPosibles)
            {
                var rutaNormalizada = Path.GetFullPath(ruta);
                if (!Directory.Exists(rutaNormalizada)) continue;
                
                rutaEncontrada = rutaNormalizada;
                var archivos = Directory.GetFiles(rutaNormalizada, "*.*")
                    .Where(f => f.EndsWith(".docx", StringComparison.OrdinalIgnoreCase) || 
                                f.EndsWith(".xlsx", StringComparison.OrdinalIgnoreCase))
                    .ToList();
                
                foreach (var archivo in archivos)
                {
                    var nombreArchivo = Path.GetFileNameWithoutExtension(archivo);
                    var codigo = nombreArchivo.ToUpper().Replace(" ", "_");
                    
                    // Verificar si ya existe
                    if (Plantillas.Any(p => p.Codigo == codigo)) continue;
                    
                    var esCharla = nombreArchivo.ToLower().Contains("charla") || 
                                   nombreArchivo.ToLower().Contains("acta") ||
                                   nombreArchivo.ToLower().Contains("registro");
                    
                    var plantilla = new PlantillaLugarTrabajo
                    {
                        LugarTrabajoId = LugarSeleccionado?.Id ?? 0,
                        Codigo = codigo,
                        NombreDocumento = nombreArchivo.Replace("_", " "),
                        RutaPlantilla = archivo,
                        TipoAplicacion = esCharla ? TipoAplicacionDocumento.Masivo : TipoAplicacionDocumento.Individual,
                        EsObligatorio = !esCharla
                    };
                    
                    // Si hay lugar seleccionado, guardar en BD
                    if (LugarSeleccionado != null)
                    {
                        plantilla.LugarTrabajoId = LugarSeleccionado.Id;
                        await _lugarTrabajoService.AddPlantillaAsync(plantilla);
                    }
                    
                    plantillasEncontradas.Add(plantilla);
                }
            }
            
            if (plantillasEncontradas.Count > 0)
            {
                foreach (var p in plantillasEncontradas)
                {
                    Plantillas.Add(p);
                }
                OnPropertyChanged(nameof(PlantillasDisponibles));
                StatusMessage = $"✓ {plantillasEncontradas.Count} plantillas encontradas";
                _notificationService.ShowSuccess($"{plantillasEncontradas.Count} plantillas detectadas");
            }
            else if (!string.IsNullOrEmpty(rutaEncontrada))
            {
                // No hay nuevas, pero recargar las existentes desde la BD
                if (LugarSeleccionado != null)
                {
                    var plantillasDb = await _lugarTrabajoService.GetPlantillasAsync(LugarSeleccionado.Id);
                    if (plantillasDb.Count > 0)
                    {
                        Plantillas = new ObservableCollection<PlantillaLugarTrabajo>(plantillasDb);
                        OnPropertyChanged(nameof(PlantillasDisponibles));
                        StatusMessage = $"✓ {plantillasDb.Count} plantillas cargadas";
                    }
                    else
                    {
                        StatusMessage = $"No hay plantillas para este lugar";
                        _notificationService.ShowWarning($"No hay plantillas registradas para {LugarSeleccionado.Nombre}");
                    }
                }
                else
                {
                    StatusMessage = "Seleccione un lugar primero";
                    _notificationService.ShowWarning("Seleccione un lugar de trabajo primero");
                }
            }
            else
            {
                StatusMessage = "Carpeta Templates no encontrada";
                _notificationService.ShowWarning($"Cree la carpeta Templates junto al ejecutable");
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _notificationService.ShowError($"Error al escanear: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    // ==================== COMANDOS ASIGNACIÓN ====================
    
    [RelayCommand]
    private async Task AsignarTrabajadoresAsync()
    {
        if (LugarSeleccionado == null) return;
        
        try
        {
            IsBusy = true;
            var seleccionados = TrabajadoresDisponibles.Where(t => t.IsSelected).ToList();
            
            foreach (var ts in seleccionados)
            {
                await _lugarTrabajoService.AsignarTrabajadorAsync(ts.Trabajador.Id, LugarSeleccionado.Id);
            }
            
            // Recargar estados
            await CargarDatosLugarAsync(LugarSeleccionado.Id);
            
            _notificationService.ShowSuccess($"{seleccionados.Count} trabajador(es) asignado(s)");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    // ==================== COMANDOS PLANTILLAS ====================
    
    [RelayCommand]
    private async Task AgregarPlantillaAsync()
    {
        if (LugarSeleccionado == null) return;
        
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Documentos Word (*.docx)|*.docx|Hojas Excel (*.xlsx)|*.xlsx|Todos los archivos (*.*)|*.*",
            Title = "Seleccionar plantilla (Word o Excel)"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(dialog.FileName);
                var codigo = $"{LugarSeleccionado.Codigo}-{fileName}".ToUpper();
                
                var plantilla = new PlantillaLugarTrabajo
                {
                    LugarTrabajoId = LugarSeleccionado.Id,
                    Codigo = codigo,
                    NombreDocumento = fileName,
                    RutaPlantilla = dialog.FileName,
                    TipoAplicacion = TipoAplicacionDocumento.Individual,
                    EsObligatorio = true,
                    Orden = Plantillas.Count + 1
                };
                
                await _lugarTrabajoService.AddPlantillaAsync(plantilla);
                Plantillas.Add(plantilla);
                
                _notificationService.ShowSuccess($"Plantilla '{fileName}' agregada");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error: {ex.Message}");
            }
        }
    }
    
    [RelayCommand]
    private async Task AgregarPlantillaCharlaAsync()
    {
        if (LugarSeleccionado == null) return;
        
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Filter = "Hojas Excel (*.xlsx)|*.xlsx|Documentos Word (*.docx)|*.docx|Todos los archivos (*.*)|*.*",
            Title = "Seleccionar plantilla de charla (Excel recomendado)"
        };
        
        if (dialog.ShowDialog() == true)
        {
            try
            {
                var fileName = Path.GetFileNameWithoutExtension(dialog.FileName);
                
                var plantilla = new PlantillaLugarTrabajo
                {
                    LugarTrabajoId = LugarSeleccionado.Id,
                    Codigo = $"{LugarSeleccionado.Codigo}-CHARLA",
                    NombreDocumento = "Charla de Seguridad",
                    RutaPlantilla = dialog.FileName,
                    TipoAplicacion = TipoAplicacionDocumento.Masivo,
                    EsObligatorio = false,
                    Orden = 999
                };
                
                await _lugarTrabajoService.AddPlantillaAsync(plantilla);
                Plantillas.Add(plantilla);
                
                _notificationService.ShowSuccess("Plantilla de charla agregada");
            }
            catch (Exception ex)
            {
                _notificationService.ShowError($"Error: {ex.Message}");
            }
        }
    }
    
    [RelayCommand]
    private async Task EliminarPlantillaAsync()
    {
        if (PlantillaSeleccionada == null) return;
        
        await _lugarTrabajoService.DeletePlantillaAsync(PlantillaSeleccionada.Id);
        Plantillas.Remove(PlantillaSeleccionada);
        _notificationService.ShowSuccess("Plantilla eliminada");
    }
    
    // ==================== COMANDOS GENERACIÓN DOCUMENTOS ====================
    
    /// <summary>
    /// Genera N documentos individuales (uno por cada trabajador seleccionado)
    /// </summary>
    [RelayCommand]
    private async Task GenerarIndividualAsync()
    {
        if (PlantillaSeleccionada == null || LugarSeleccionado == null)
        {
            _notificationService.ShowWarning("Seleccione una plantilla");
            return;
        }
        
        var seleccionados = TrabajadoresDisponibles.Where(t => t.IsSelected).ToList();
        if (seleccionados.Count == 0)
        {
            _notificationService.ShowWarning("Seleccione al menos un trabajador");
            return;
        }
        
        try
        {
            IsBusy = true;
            int generados = 0;
            
            foreach (var ts in seleccionados)
            {
                StatusMessage = $"Generando documento {++generados}/{seleccionados.Count}...";
                
                var trabajador = ts.Trabajador;
                
                // Preparar datos para la plantilla
                var datos = new Dictionary<string, string>
                {
                    ["NOMBRE_TRABAJADOR"] = trabajador.NombreCompleto,
                    ["RUT_TRABAJADOR"] = trabajador.Rut,
                    ["CARGO"] = trabajador.Cargo ?? "",
                    ["FECHA_ACTUAL"] = DateTime.Now.ToString("dd/MM/yyyy"),
                    ["LUGAR_TRABAJO"] = LugarSeleccionado.Nombre,
                    ["EMPRESA"] = trabajador.CentroTrabajo?.Empresa?.RazonSocial ?? ""
                };
                
                // TODO: Usar DocumentoService para procesar plantilla y generar archivo
                // Por ahora marcamos como generado
                if (PlantillaSeleccionada.RutaPlantilla != null)
                {
                    await _lugarTrabajoService.GenerarDocumentoAsync(
                        trabajador.Id,
                        PlantillaSeleccionada.Id,
                        PlantillaSeleccionada.RutaPlantilla
                    );
                }
            }
            
            StatusMessage = $"✓ {generados} documentos generados";
            _notificationService.ShowSuccess($"{generados} documentos generados exitosamente");
            
            // Refrescar
            await CargarDatosLugarAsync(LugarSeleccionado.Id);
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _notificationService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    /// <summary>
    /// Genera 1 documento único con la lista de todos los trabajadores seleccionados
    /// (útil para actas de charla, listas de asistencia, etc.)
    /// </summary>
    [RelayCommand]
    private async Task GenerarDocumentoUnicoAsync()
    {
        if (PlantillaSeleccionada == null || LugarSeleccionado == null)
        {
            _notificationService.ShowWarning("Seleccione una plantilla");
            return;
        }
        
        var seleccionados = TrabajadoresDisponibles.Where(t => t.IsSelected).ToList();
        if (seleccionados.Count == 0)
        {
            _notificationService.ShowWarning("Seleccione al menos un trabajador");
            return;
        }
        
        try
        {
            IsBusy = true;
            StatusMessage = "Generando documento con lista de trabajadores...";
            
            // Construir lista de asistentes
            var listaAsistentes = string.Join("\n", seleccionados.Select((t, i) => 
                $"{i + 1}. {t.Trabajador.NombreCompleto} - {t.Trabajador.Rut}"));
            
            // Preparar datos para la plantilla
            var datos = new Dictionary<string, string>
            {
                ["LUGAR_TRABAJO"] = LugarSeleccionado.Nombre,
                ["EMPRESA"] = "Tagle Labs", // TODO: Obtener de configuración o empresa asociada
                ["AREA"] = LugarSeleccionado.Nombre, // El área es el lugar de trabajo
                ["FECHA_ACTUAL"] = DateTime.Now.ToString("dd-MM-yyyy"),
                ["FECHA_CHARLA"] = NuevaCharlaFecha.ToString("dd-MM-yyyy"),
                ["HORA_CHARLA"] = NuevaCharlaHora ?? "",
                ["TEMA_CHARLA"] = NuevaCharlaTema ?? PlantillaSeleccionada.NombreDocumento,
                ["EXPOSITOR"] = NuevaCharlaExpositor ?? "Sin especificar",
                ["DURACION"] = $"{NuevaCharlaDuracion} minutos",
                ["LISTA_ASISTENTES"] = listaAsistentes,
                ["TOTAL_ASISTENTES"] = seleccionados.Count.ToString(),
                
                // Checkboxes de Objetivo - X si está marcado, vacío si no
                ["OBJ_CHARLA"] = ObjCharla ? "X" : "",
                ["OBJ_CAPACITACION"] = ObjCapacitacion ? "X" : "",
                ["OBJ_REINDUCCION"] = ObjReinduccion ? "X" : "",
                ["OBJ_INSPECCION"] = ObjInspeccion ? "X" : "",
                ["OBJ_OTROS"] = ObjOtros ? "X" : "",
                
                // Campos de texto libre
                ["OBSERVACIONES"] = NuevaCharlaObservaciones ?? "",
                ["ACUERDOS"] = NuevaCharlaAcuerdos ?? ""
            };
            
            // Generar documento con placeholders reemplazados
            // Nombre, Cargo y RUT vienen automáticamente de la base de datos
            var listaParaExcel = seleccionados
                .Select(t => (t.Trabajador.NombreCompleto, t.Trabajador.Cargo ?? "", t.Trabajador.Rut))
                .ToList();
            
            var rutaGenerada = await _lugarTrabajoService.GenerarDocumentoRealAsync(
                PlantillaSeleccionada.RutaPlantilla!,
                datos,
                $"{LugarSeleccionado.Codigo}_{PlantillaSeleccionada.NombreDocumento}",
                listaParaExcel
            );
            
            StatusMessage = $"✓ Documento generado con {seleccionados.Count} trabajadores";
            _notificationService.ShowSuccess($"Documento único generado con {seleccionados.Count} trabajadores");
            
            // Abrir documento generado
            if (File.Exists(rutaGenerada))
            {
                Process.Start(new ProcessStartInfo(rutaGenerada) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            StatusMessage = $"Error: {ex.Message}";
            _notificationService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    // ==================== COMANDOS CHARLAS MASIVAS ====================
    
    [RelayCommand]
    private async Task CrearCharlaAsync()
    {
        if (LugarSeleccionado == null || string.IsNullOrWhiteSpace(NuevaCharlaTema))
        {
            _notificationService.ShowWarning("Seleccione un lugar y escriba el tema de la charla");
            return;
        }
        
        var trabajadoresSeleccionados = TrabajadoresDisponibles
            .Where(t => t.IsSelected)
            .Select(t => t.Trabajador.Id)
            .ToList();
        
        if (trabajadoresSeleccionados.Count == 0)
        {
            _notificationService.ShowWarning("Seleccione al menos un trabajador");
            return;
        }
        
        try
        {
            IsBusy = true;
            
            var charla = new CharlaSST
            {
                LugarTrabajoId = LugarSeleccionado.Id,
                Tema = NuevaCharlaTema,
                Fecha = NuevaCharlaFecha,
                Expositor = NuevaCharlaExpositor,
                DuracionMinutos = NuevaCharlaDuracion
            };
            
            await _lugarTrabajoService.CrearCharlaAsync(charla, trabajadoresSeleccionados);
            
            Charlas.Insert(0, charla);
            
            // Limpiar formulario
            NuevaCharlaTema = string.Empty;
            NuevaCharlaExpositor = string.Empty;
            NuevaCharlaDuracion = 30;
            
            _notificationService.ShowSuccess($"Charla creada para {trabajadoresSeleccionados.Count} trabajadores");
            
            // Generar acta
            var rutaActa = await _lugarTrabajoService.GenerarActaCharlaAsync(charla.Id);
            if (!string.IsNullOrEmpty(rutaActa) && File.Exists(rutaActa))
            {
                Process.Start(new ProcessStartInfo(rutaActa) { UseShellExecute = true });
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }
    
    [RelayCommand]
    private async Task GenerarActaCharlaAsync(CharlaSST charla)
    {
        if (charla == null) return;
        
        try
        {
            var rutaActa = await _lugarTrabajoService.GenerarActaCharlaAsync(charla.Id);
            if (!string.IsNullOrEmpty(rutaActa) && File.Exists(rutaActa))
            {
                Process.Start(new ProcessStartInfo(rutaActa) { UseShellExecute = true });
            }
            else
            {
                _notificationService.ShowWarning("No hay plantilla de charla configurada");
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error: {ex.Message}");
        }
    }
}
