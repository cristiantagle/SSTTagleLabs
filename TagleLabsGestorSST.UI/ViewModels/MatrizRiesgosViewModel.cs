using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class MatrizRiesgosViewModel : ObservableObject
{
    private readonly IMatrizRiesgoService _matrizService;
    private readonly IEmpresaService _empresaService;
    private readonly ILocalAiService _aiService;
    private readonly INotificationService _notificationService;
    private readonly DocumentoService _documentoService;

    [ObservableProperty]
    private ObservableCollection<Empresa> _empresas = new();

    [ObservableProperty]
    private Empresa? _empresaSeleccionada;

    [ObservableProperty]
    private ObservableCollection<CentroTrabajo> _centrosTrabajo = new();

    [ObservableProperty]
    private CentroTrabajo? _centroSeleccionado;



    [ObservableProperty]
    private MatrizRiesgoEmpresa? _itemSeleccionado;

    // Catálogos para los ComboBox
    [ObservableProperty]
    private ObservableCollection<Rubro> _rubros = new();
    [ObservableProperty]
    private ObservableCollection<Actividad> _actividades = new();
    [ObservableProperty]
    private ObservableCollection<Tarea> _tareas = new();
    [ObservableProperty]
    private ObservableCollection<Peligro> _peligros = new();
    [ObservableProperty]
    private ObservableCollection<Control> _controles = new();

    // Selecciones del formulario
    [ObservableProperty]
    private Rubro? _rubroForm;
    [ObservableProperty]
    private Actividad? _actividadForm;

    [ObservableProperty]
    private bool _esEdicion;

    // UI: search / filters / grouping
    [ObservableProperty]
    private string _filtroTexto = string.Empty;

    [ObservableProperty]
    private ObservableCollection<string> _agrupamientos = new();

    [ObservableProperty]
    private string _agrupamientoSeleccionado = "Tarea";

    public MatrizRiesgosViewModel(IMatrizRiesgoService matrizService, IEmpresaService empresaService, ILocalAiService aiService, INotificationService notificationService, DocumentoService documentoService)
    {
        _matrizService = matrizService;
        _empresaService = empresaService;
        _aiService = aiService;
        _notificationService = notificationService;
        _documentoService = documentoService;
        _matrizService = matrizService;
        _empresaService = empresaService;
        // Setup grouping options
        Agrupamientos = new ObservableCollection<string>(new[] { "Tarea", "Categoría", "Nivel", "Probabilidad" });

        // Fire and forget safe execution
        _ = CargarDatosIniciales();
    }

    [RelayCommand]
    private async Task CargarDatosIniciales()
    {
        try
        {
            var empresas = await _empresaService.ObtenerEmpresasAsync();
            Empresas = new ObservableCollection<Empresa>(empresas);
            
            if (Empresas.Any())
            {
                EmpresaSeleccionada = Empresas.First();
            }

            var rubros = await _matrizService.ObtenerRubrosAsync();
            Rubros = new ObservableCollection<Rubro>(rubros);

            var peligros = await _matrizService.ObtenerPeligrosAsync();
            Peligros = new ObservableCollection<Peligro>(peligros);

            var controles = await _matrizService.ObtenerControlesAsync();
            Controles = new ObservableCollection<Control>(controles);
        }
        catch (Exception ex)
        {
            // Use explicit notification service call if available or MessageBox as fallback
            System.Windows.MessageBox.Show($"Error al cargar datos iniciales: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }

    partial void OnEmpresaSeleccionadaChanged(Empresa? value)
    {
        if (value != null)
        {
            CentrosTrabajo = new ObservableCollection<CentroTrabajo>(value.CentrosTrabajo);
            if (CentrosTrabajo.Any())
            {
                CentroSeleccionado = CentrosTrabajo.First();
            }
            else
            {
                CentroSeleccionado = null;
                CentroSeleccionado = null;
                MatrizView = null;
            }
        }
    }

    partial void OnCentroSeleccionadoChanged(CentroTrabajo? value)
    {
        if (value != null)
        {
            CargarMatrizCommand.Execute(null);
        }
        else
        {
            MatrizView = null;
        }
    }

    partial void OnRubroFormChanged(Rubro? value)
    {
        if (value != null)
        {
            CargarActividades(value.Id);
        }
        else
        {
            Actividades.Clear();
        }
    }

    partial void OnActividadFormChanged(Actividad? value)
    {
        if (value != null)
        {
            CargarTareas(value.Id);
        }
        else
        {
            Tareas.Clear();
        }
    }

    partial void OnItemSeleccionadoChanged(MatrizRiesgoEmpresa? value)
    {
        EsEdicion = value != null;
        // Aquí podríamos cargar los combos en cascada si es edición, 
        // pero por simplicidad asumiremos creación nueva principalmente.
    }

    [ObservableProperty]
    private string _filtroNivelRiesgo = "Todos";

    [ObservableProperty]
    private ObservableCollection<string> _nivelesRiesgo = new(new[] { "Todos", "Bajo", "Medio", "Alto" });

    partial void OnFiltroTextoChanged(string value) => ActualizarFiltro();
    partial void OnFiltroNivelRiesgoChanged(string value) => ActualizarFiltro();

    private void ActualizarFiltro()
    {
        if (MatrizView != null)
        {
            MatrizView.Filter = o =>
            {
                if (o is not MatrizRiesgoEmpresa item) return true;
                
                // Filtro por Nivel
                if (FiltroNivelRiesgo != "Todos" && item.NivelRiesgo != FiltroNivelRiesgo) return false;

                // Filtro por Texto
                if (string.IsNullOrWhiteSpace(FiltroTexto)) return true;

                var text = FiltroTexto.Trim();
                return (item.Peligro?.Descripcion?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (item.MedidaControlEspecifica?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (item.Tarea?.Nombre?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (item.NivelRiesgo?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false)
                    || (item.Peligro?.Categoria?.Contains(text, StringComparison.OrdinalIgnoreCase) ?? false);
            };
            MatrizView.Refresh();
        }
    }

    partial void OnAgrupamientoSeleccionadoChanged(string value)
    {
        if (MatrizView == null) return;

        try
        {
            MatrizView.GroupDescriptions.Clear();
            switch (value)
            {
                case "Tarea":
                    MatrizView.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("Tarea.Nombre"));
                    break;
                case "Categoría":
                    MatrizView.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("Peligro.Categoria"));
                    break;
                case "Nivel":
                    MatrizView.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("NivelRiesgo"));
                    break;
                case "Probabilidad":
                    MatrizView.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("Probabilidad"));
                    break;
                default:
                    MatrizView.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("Tarea.Nombre"));
                    break;
            }
            MatrizView.Refresh();
        }
        catch { }
    }

    private async void CargarActividades(int rubroId)
    {
        try
        {
            var actividades = await _matrizService.ObtenerActividadesPorRubroAsync(rubroId);
            Actividades = new ObservableCollection<Actividad>(actividades);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al cargar actividades: {ex.Message}");
        }
    }

    private async void CargarTareas(int actividadId)
    {
        try
        {
            var tareas = await _matrizService.ObtenerTareasPorActividadAsync(actividadId);
            Tareas = new ObservableCollection<Tarea>(tareas);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al cargar tareas: {ex.Message}");
        }
    }

    [ObservableProperty]
    private System.ComponentModel.ICollectionView? _matrizView;

    [ObservableProperty]
    private string _procedimientoGenerado = string.Empty;


    // Mantener la colección original para operaciones CRUD si es necesario, 
    // pero la vista se enlaza a MatrizView
    private ObservableCollection<MatrizRiesgoEmpresa> _matrizSource = new();
    // Locks to prevent multiple windows editing the same item concurrently
    private readonly HashSet<int> _lockedItems = new();

    [RelayCommand]
    private async Task CargarMatriz()
    {
        if (CentroSeleccionado == null) return;
        try
        {
            var lista = await _matrizService.ObtenerMatrizPorCentroAsync(CentroSeleccionado.Id);
            _matrizSource = new ObservableCollection<MatrizRiesgoEmpresa>(lista);
            
            // Configurar la vista con agrupación
            var view = System.Windows.Data.CollectionViewSource.GetDefaultView(_matrizSource);
            view.GroupDescriptions.Add(new System.Windows.Data.PropertyGroupDescription("Tarea.Nombre"));
            MatrizView = view;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al cargar la matriz: {ex.Message}");
        }
    }

    /// <summary>
    /// Try acquire lock for editing an item. Returns true if lock acquired or item is new (Id==0).
    /// </summary>
    public bool TryLockItem(int id)
    {
        if (id == 0) return true; // new items can be edited freely
        lock (_lockedItems)
        {
            if (_lockedItems.Contains(id)) return false;
            _lockedItems.Add(id);
            return true;
        }
    }

    public void UnlockItem(int id)
    {
        if (id == 0) return;
        lock (_lockedItems)
        {
            if (_lockedItems.Contains(id)) _lockedItems.Remove(id);
        }
    }

    /// <summary>
    /// Confirm and save an item opened in child window (handles collision detection).
    /// </summary>
    public async Task<bool> ConfirmAndSaveFromChildAsync(MatrizRiesgoEmpresa item, string originalSnapshotJson)
    {
        if (item == null) return false;

        try
        {
            // If existing item, check the source of truth in DB for changes
            if (item.Id != 0)
            {
                var latest = await _matrizService.ObtenerPorIdAsync(item.Id);
                if (latest != null)
                {
                    // Compare serialized JSON of latest with the original snapshot
                    var latestJson = Newtonsoft.Json.JsonConvert.SerializeObject(latest);
                    if (!string.Equals(latestJson, originalSnapshotJson, StringComparison.Ordinal))
                    {
                        var result = System.Windows.MessageBox.Show("El registro fue modificado por otro usuario/ventana desde que abriste la ventana de detalle. ¿Deseas sobrescribir los cambios?", "Conflicto detectado", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Warning);
                        if (result != System.Windows.MessageBoxResult.Yes)
                        {
                            return false; // user cancelled
                        }
                    }
                }
            }

            // Delegate to existing GuardarItem implementation
            if (item.Id == 0)
            {
                await _matrizService.AgregarItemMatrizAsync(item);
            }
            else
            {
                await _matrizService.ActualizarItemMatrizAsync(item);
            }

            // refresh
            await CargarMatriz();
            return true;
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error guardando registro: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return false;
        }
        finally
        {
            // Always unlock the item
            try { UnlockItem(item.Id); } catch { }
        }
    }

    [RelayCommand]
    private void NuevoItem()
    {
        if (CentroSeleccionado == null) return;
        ItemSeleccionado = new MatrizRiesgoEmpresa 
        { 
            CentroTrabajoId = CentroSeleccionado.Id,
            Probabilidad = 1,
            Consecuencia = 1
        };
        EsEdicion = true;
    }

    [RelayCommand]
    private async Task GuardarItem()
    {
        if (ItemSeleccionado == null) return;
        
        // Validaciones mínimas
        if (ItemSeleccionado.Tarea == null && ItemSeleccionado.TareaId == 0) return;
        if (ItemSeleccionado.Peligro == null && ItemSeleccionado.PeligroId == 0) return;

        // Ajustar IDs si vienen de objetos seleccionados en combos
        // (WPF Binding a veces asigna el objeto pero no el ID si no se configura bien, o viceversa)
        // Aquí asumimos que el Binding SelectedItem actualiza el objeto, y EF Core se encarga del ID al guardar,
        // o que usamos SelectedValuePath.

        if (ItemSeleccionado.Id == 0)
        {
            await _matrizService.AgregarItemMatrizAsync(ItemSeleccionado);
        }
        else
        {
            await _matrizService.ActualizarItemMatrizAsync(ItemSeleccionado);
        }

        await CargarMatriz();
        ItemSeleccionado = null;
    }

    [ObservableProperty]
    private bool _isBusy;

    [RelayCommand]
    private async Task GenerarRiesgosSugeridos()
    {
        if (CentroSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Debe seleccionar un Centro de Trabajo.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (ActividadForm == null)
        {
            System.Windows.MessageBox.Show("Debe seleccionar una Actividad en el formulario para generar sugerencias.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var sugerencias = await _matrizService.GenerarRiesgosSugeridosAsync(ActividadForm.Id, CentroSeleccionado.Id);
            
            if (sugerencias.Any())
            {
                foreach(var item in sugerencias)
                {
                    await _matrizService.AgregarItemMatrizAsync(item);
                }
                await CargarMatriz();
                System.Windows.MessageBox.Show($"Se han generado y guardado {sugerencias.Count} riesgos sugeridos por IA.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("La IA no generó sugerencias para esta actividad o no hay suficiente contexto.", "Información", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar riesgos: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ImportarRiesgosRubro()
    {
        if (CentroSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Debe seleccionar un Centro de Trabajo.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (EmpresaSeleccionada == null || EmpresaSeleccionada.RubroId == null)
        {
            System.Windows.MessageBox.Show("La empresa no tiene un Rubro asignado. Vaya a 'Empresas' y asigne un Rubro.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var importados = await _matrizService.ImportarRiesgosPorRubroAsync(EmpresaSeleccionada.RubroId.Value, CentroSeleccionado.Id);
            
            if (importados.Any())
            {
                await CargarMatriz();
                System.Windows.MessageBox.Show($"Se han importado {importados.Count} riesgos típicos del rubro.", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("No se encontraron nuevos riesgos típicos para importar o ya existen en la matriz.", "Información", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al importar riesgos: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task SugerirRiesgosPorRubroIA()
    {
        if (CentroSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Debe seleccionar un Centro de Trabajo.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        // Usar colección de Rubros (multi-rubro) o fallback al RubroId legacy
        var rubrosIds = EmpresaSeleccionada?.Rubros?.Select(r => r.Id).ToList() ?? new List<int>();
        if (!rubrosIds.Any() && EmpresaSeleccionada?.RubroId != null)
        {
            rubrosIds.Add(EmpresaSeleccionada.RubroId.Value);
        }

        if (!rubrosIds.Any())
        {
            System.Windows.MessageBox.Show("La empresa no tiene Rubros asignados. Vaya a 'Empresas' y asigne rubros primero.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var totalSugerencias = 0;
            var rubrosNombres = EmpresaSeleccionada?.Rubros?.Select(r => r.Nombre).ToList() ?? new List<string>();

            // Generar sugerencias para CADA rubro seleccionado
            foreach (var rubroId in rubrosIds)
            {
                var sugerencias = await _matrizService.SugerirRiesgosPorRubroAsync(rubroId, CentroSeleccionado.Id);
                
                foreach (var item in sugerencias)
                {
                    await _matrizService.AgregarItemMatrizAsync(item);
                }
                totalSugerencias += sugerencias.Count;
            }

            await CargarMatriz();

            if (totalSugerencias > 0)
            {
                System.Windows.MessageBox.Show($"Se han generado {totalSugerencias} riesgos para {rubrosIds.Count} rubro(s):\n{string.Join(", ", rubrosNombres)}", "Éxito", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
            else
            {
                System.Windows.MessageBox.Show("La IA no generó sugerencias para los rubros o no hay suficiente contexto.", "Información", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar sugerencias: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GenerarIRL()
    {
        if (EmpresaSeleccionada == null)
        {
            System.Windows.MessageBox.Show("Debe seleccionar una empresa.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (_matrizSource == null || !_matrizSource.Any())
        {
            System.Windows.MessageBox.Show("No hay riesgos en la matriz para generar el IRL.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            // Construir tabla de riesgos en formato Markdown para conversión a Word
            var tablaRiesgos = new System.Text.StringBuilder();
            
            // Encabezado de la tabla
            tablaRiesgos.AppendLine("| N° | Tarea | Peligro | P | C | Nivel | Medida de Control |");
            tablaRiesgos.AppendLine("|---|---|---|---|---|---|---|");
            
            int contador = 1;
            foreach (var riesgo in _matrizSource)
            {
                var tarea = riesgo.Tarea?.Nombre?.Replace("|", "-") ?? "N/A";
                var peligro = riesgo.Peligro?.Descripcion?.Replace("|", "-") ?? "N/A";
                var control = riesgo.MedidaControlEspecifica?.Replace("|", "-") ?? "N/A";
                
                // Truncar textos largos para mejor presentación
                if (tarea.Length > 30) tarea = tarea.Substring(0, 27) + "...";
                if (peligro.Length > 40) peligro = peligro.Substring(0, 37) + "...";
                if (control.Length > 50) control = control.Substring(0, 47) + "...";
                
                tablaRiesgos.AppendLine($"| {contador} | {tarea} | {peligro} | {riesgo.Probabilidad} | {riesgo.Consecuencia} | **{riesgo.NivelRiesgo}** | {control} |");
                contador++;
            }
            
            // Agregar resumen
            tablaRiesgos.AppendLine();
            tablaRiesgos.AppendLine($"**Total de riesgos identificados:** {_matrizSource.Count}");
            tablaRiesgos.AppendLine();
            tablaRiesgos.AppendLine($"**Centro de Trabajo:** {CentroSeleccionado?.Nombre ?? "General"}");
            tablaRiesgos.AppendLine();
            tablaRiesgos.AppendLine($"**Fecha de elaboración:** {DateTime.Now:dd/MM/yyyy}");
            
            // Clasificación de riesgos
            var altos = _matrizSource.Count(r => r.NivelRiesgo == "Alto");
            var medios = _matrizSource.Count(r => r.NivelRiesgo == "Medio");
            var bajos = _matrizSource.Count(r => r.NivelRiesgo == "Bajo");
            
            tablaRiesgos.AppendLine();
            tablaRiesgos.AppendLine("### Resumen por Nivel de Riesgo");
            tablaRiesgos.AppendLine($"- 🔴 **Altos:** {altos}");
            tablaRiesgos.AppendLine($"- 🟡 **Medios:** {medios}");
            tablaRiesgos.AppendLine($"- 🟢 **Bajos:** {bajos}");

            var datos = new Dictionary<string, string>
            {
                { "RAZON_SOCIAL", EmpresaSeleccionada.RazonSocial },
                { "RUT", EmpresaSeleccionada.Rut },
                { "FECHA", DateTime.Now.ToString("dd/MM/yyyy") },
                { "CENTRO", CentroSeleccionado?.Nombre ?? "General" },
                { "TABLA_RIESGOS", tablaRiesgos.ToString() },
                { "TOTAL_RIESGOS", _matrizSource.Count.ToString() },
                { "RIESGOS_ALTOS", altos.ToString() },
                { "RIESGOS_MEDIOS", medios.ToString() },
                { "RIESGOS_BAJOS", bajos.ToString() }
            };

            var doc = await _documentoService.GenerarDocumentoAsync(EmpresaSeleccionada.Id, "IRL001", datos);
            
            if (doc != null)
            {
                _notificationService.ShowSuccess($"IRL generado exitosamente con {_matrizSource.Count} riesgos. Archivo: {System.IO.Path.GetFileName(doc.RutaArchivoEditable)}");
            }
            else
            {
                _notificationService.ShowError("No se pudo generar el IRL. Verifique la plantilla IRL001.");
            }
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show($"Error al generar IRL: {ex.Message}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task EliminarItem()
    {
        if (ItemSeleccionado == null || ItemSeleccionado.Id == 0) return;
        
        // Confirmación antes de eliminar
        var resultado = System.Windows.MessageBox.Show(
            $"¿Estás seguro de que deseas eliminar este riesgo?\n\nTarea: {ItemSeleccionado.Tarea?.Nombre ?? "N/A"}\nPeligro: {ItemSeleccionado.Peligro?.Descripcion ?? "N/A"}\n\nEsta acción no se puede deshacer.",
            "⚠️ Confirmar eliminación",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Warning);
        
        if (resultado != System.Windows.MessageBoxResult.Yes) return;
        
        await _matrizService.EliminarItemMatrizAsync(ItemSeleccionado.Id);
        _notificationService.ShowSuccess("Riesgo eliminado correctamente.");
        ItemSeleccionado = null;
        await CargarMatriz();
    }

    [RelayCommand]
    private async Task GenerarProcedimientoParaItem()
    {
        if (ItemSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Seleccione un elemento de la matriz antes de generar un procedimiento.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        IsBusy = true;
        try
        {
            var descripcion = $"Tarea: {ItemSeleccionado.Tarea?.Nombre ?? "(no disponible)"}\nPeligro: {ItemSeleccionado.Peligro?.Descripcion ?? "(no disponible)"}\nMedida: {ItemSeleccionado.MedidaControlEspecifica ?? "(no disponible)"}\nProbabilidad: {ItemSeleccionado.Probabilidad} Consecuencia: {ItemSeleccionado.Consecuencia} Nivel: {ItemSeleccionado.NivelRiesgo}";
            var contexto = EmpresaSeleccionada != null ? $"Empresa: {EmpresaSeleccionada.RazonSocial} - Centro: {CentroSeleccionado?.Nombre}" : string.Empty;

            var resultado = await _aiService.GenerarProcedimientoAsync(descripcion, contexto);
            ProcedimientoGenerado = resultado ?? string.Empty;

            try { System.Windows.Clipboard.SetText(ProcedimientoGenerado); } catch { /* ignore clipboard failures */ }

            _notificationService.ShowSuccess("Procedimiento generado (copiado al portapapeles). Revise el texto en el panel.");
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error generando procedimiento: {ex.Message}");
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task GuardarProcedimientoGenerado()
    {
        if (ItemSeleccionado == null)
        {
            System.Windows.MessageBox.Show("Seleccione un elemento antes de guardar el procedimiento.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (string.IsNullOrWhiteSpace(ProcedimientoGenerado))
        {
            System.Windows.MessageBox.Show("No hay procedimiento generado para guardar.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        if (EmpresaSeleccionada == null)
        {
            System.Windows.MessageBox.Show("Seleccione la empresa antes de guardar el procedimiento.", "Aviso", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            return;
        }

        try
        {
            IsBusy = true;
            var titulo = ItemSeleccionado.Tarea?.Nombre ?? "Procedimiento Generado"
                ;
            var datos = new Dictionary<string, string>
            {
                { "TITULO", titulo },
                { "OBJETIVO", "" },
                { "ALCANCE", "" },
                { "CONTENIDO_DINAMICO", ProcedimientoGenerado }
            };

            var doc = await _documentoService.GenerarDocumentoAsync(EmpresaSeleccionada.Id, "PTS-STD", datos);
            if (doc != null)
            {
                _notificationService.ShowSuccess("Procedimiento guardado en Documentos.");
            }
            else
            {
                _notificationService.ShowError("No se pudo guardar el procedimiento. Verifique la plantilla PTS-STD.");
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error al guardar procedimiento: {ex.Message}");
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task ExportarExcel()
    {
        if (CentroSeleccionado == null)
        {
            _notificationService.ShowWarning("Seleccione un centro de trabajo primero.");
            return;
        }

        try
        {
            IsBusy = true;
            var excelBytes = await _matrizService.ExportarMiperAExcelAsync(CentroSeleccionado.Id);
            
            // Guardar archivo
            var dialog = new Microsoft.Win32.SaveFileDialog
            {
                Filter = "Excel (*.xlsx)|*.xlsx",
                FileName = $"MIPER_{CentroSeleccionado.Nombre.Replace(" ", "_")}_{DateTime.Now:yyyyMMdd}.xlsx",
                Title = "Guardar Matriz MIPER"
            };

            if (dialog.ShowDialog() == true)
            {
                await System.IO.File.WriteAllBytesAsync(dialog.FileName, excelBytes);
                _notificationService.ShowSuccess($"Matriz exportada exitosamente.");
                
                // Abrir el archivo
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = dialog.FileName,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            _notificationService.ShowError($"Error al exportar: {ex.Message}");
        }
        finally { IsBusy = false; }
    }
}
