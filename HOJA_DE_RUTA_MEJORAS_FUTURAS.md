# 🎯 HOJA DE RUTA: Mejoras Pendientes y Próximos Pasos

**Fecha**: 1 de Diciembre 2025  
**Basado en**: Análisis profundo de Ollama, UI y Lógica de Negocio

---

## 📊 PRIORIZACIÓN DE MEJORAS

### TIER 1: CRÍTICAS (Esta semana)

#### 1.1 Agrupación de Peligros por Categoría en MIPER
**Prioridad**: ALTA | **Esfuerzo**: 1 día | **Impacto**: ALTO (usabilidad)

**Problema Actual**:
```
DataGrid plano muestra 100+ peligros sin clasificar
Usuario: "¿Cómo encuentro peligros ergonómicos?"
Respuesta actual: "Scroll y scroll hasta encontrar..."
```

**Solución Propuesta**:
```csharp
// En MatrizRiesgosViewModel.cs
partial void OnCentroSeleccionadoChanged(CentroTrabajo? value)
{
    if (value != null)
    {
        CargarMatrizCommand.Execute(null);
        // NUEVO: Agrupar automáticamente
        var view = System.Windows.Data.CollectionViewSource.GetDefaultView(_matrizSource);
        view.GroupDescriptions.Clear();
        view.GroupDescriptions.Add(new PropertyGroupDescription("Peligro.Categoria"));
        MatrizView = view;
    }
}
```

**XAML en TreeView**:
```xaml
<TreeView ItemsSource="{Binding MatrizView.Groups}" Background="Transparent">
    <TreeView.ItemTemplate>
        <HierarchicalDataTemplate ItemsSource="{Binding Items}">
            <!-- Encabezado: Categoría (ej: Ergonómico) -->
            <StackPanel Orientation="Horizontal" Margin="5">
                <TextBlock FontWeight="Bold" Foreground="#8B5CF6" 
                          Text="{Binding Name}" Width="200"/>
                <TextBlock Foreground="#94A3B8" 
                          Text="{Binding ItemCount, StringFormat='({0})'}"/>
            </StackPanel>
            
            <!-- Items dentro: Peligros específicos -->
            <HierarchicalDataTemplate.ItemTemplate>
                <DataTemplate>
                    <Border Background="White" CornerRadius="4" Padding="10" 
                            Margin="5" BorderBrush="#E2E8F0" BorderThickness="1">
                        <Grid>
                            <TextBlock Text="{Binding Peligro.Descripcion}" FontSize="12"/>
                        </Grid>
                    </Border>
                </DataTemplate>
            </HierarchicalDataTemplate.ItemTemplate>
        </HierarchicalDataTemplate>
    </TreeView.ItemTemplate>
</TreeView>
```

**Beneficios**:
- ✅ Búsqueda visual por categoría
- ✅ Detalles como contador de riesgos/categoría
- ✅ Menos scroll, más productividad
- ✅ Estructura lógica para usuarios

**Testing**:
```
1. Abrir MIPER
2. Verificar que se agrupa por categoría
3. Expandir "Ergonómico" → debe mostrar peligros ergonómicos
4. Verificar contador: "Ergonómico (5)" si hay 5 riesgos
5. Click en peligro → debe cargar detalles
```

---

#### 1.2 Búsqueda y Filtros Avanzados en MIPER
**Prioridad**: ALTA | **Esfuerzo**: 1.5 días | **Impacto**: ALTO (productividad)

**Problema Actual**:
```
500+ peligros en BD
Usuario quiere encontrar "ruido"
Solución actual: Scroll, scroll, scroll... ❌
Solución propuesta: Búsqueda y filtros ✅
```

**Solución Propuesta**:
```xaml
<!-- En MatrizRiesgosView.xaml: Agregar encima del TreeView -->
<Grid Margin="0,0,0,20">
    <Grid.ColumnDefinitions>
        <ColumnDefinition Width="*"/>
        <ColumnDefinition Width="200"/>
        <ColumnDefinition Width="200"/>
    </Grid.ColumnDefinitions>
    
    <!-- Búsqueda de texto -->
    <TextBox Grid.Column="0" 
            Text="{Binding FiltroTexto, UpdateSourceTrigger=PropertyChanged}"
            materialDesign:HintAssist.Hint="Buscar peligro, riesgo, control..."
            Style="{StaticResource MaterialDesignOutlinedTextBox}"/>
    
    <!-- Filtro por categoría -->
    <ComboBox Grid.Column="1" 
             ItemsSource="{Binding Categorias}" 
             SelectedItem="{Binding CategoriaFiltro}"
             DisplayMemberPath="Nombre"
             materialDesign:HintAssist.Hint="Filtrar por categoría..."
             Margin="10,0"/>
    
    <!-- Filtro por nivel riesgo -->
    <ComboBox Grid.Column="2" 
             ItemsSource="{Binding NivelesRiesgo}"
             SelectedItem="{Binding NivelRiesgoFiltro}"
             materialDesign:HintAssist.Hint="Filtro por nivel..."
             Margin="10,0"/>
</Grid>
```

**ViewModel Changes**:
```csharp
[ObservableProperty]
private string _filtroTexto = string.Empty;

[ObservableProperty]
private string? _categoriaFiltro;

[ObservableProperty]
private string? _nivelRiesgoFiltro;

[ObservableProperty]
private ObservableCollection<string> _categorias = new();

[ObservableProperty]
private List<string> _nivelesRiesgo = new() { "Bajo", "Medio", "Alto" };

// Método que se ejecuta cuando cambia filtro
partial void OnFiltroTextoChanged(string value) => AplicarFiltros();
partial void OnCategoriaFiltroChanged(string? value) => AplicarFiltros();
partial void OnNivelRiesgoFiltroChanged(string? value) => AplicarFiltros();

private void AplicarFiltros()
{
    if (!_matrizSource.Any()) return;
    
    var filtered = _matrizSource
        .Where(m => string.IsNullOrWhiteSpace(FiltroTexto) || 
                   m.Peligro.Descripcion.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
                   m.Peligro.Categoria.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
                   m.MedidaControlEspecifica.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase))
        .Where(m => string.IsNullOrWhiteSpace(CategoriaFiltro) || 
                   m.Peligro.Categoria == CategoriaFiltro)
        .Where(m => string.IsNullOrWhiteSpace(NivelRiesgoFiltro) || 
                   m.NivelRiesgo == NivelRiesgoFiltro)
        .ToList();
    
    var view = System.Windows.Data.CollectionViewSource.GetDefaultView(
        new ObservableCollection<MatrizRiesgoEmpresa>(filtered));
    view.GroupDescriptions.Add(new PropertyGroupDescription("Peligro.Categoria"));
    MatrizView = view;
}
```

**Beneficios**:
- ✅ Búsqueda full-text (descripción, categoría, medida)
- ✅ Filtro por categoría (multi-select posible)
- ✅ Filtro por nivel (Alto, Medio, Bajo)
- ✅ Combinación de filtros (AND lógico)
- ✅ Resultados en tiempo real (UpdateSourceTrigger)

---

#### 1.3 Dashboard Enriquecido con Información Crítica
**Prioridad**: ALTA | **Esfuerzo**: 2 días | **Impacto**: ALTO (información)

**Problema Actual**:
```
Dashboard muestra solo KPIs básicos
Usuario: "¿Cuál es el riesgo más crítico?"
Respuesta: "No sé, mira todo el sistema"
```

**Mejoras Propuestas**:

##### Sección 1: Top 5 Riesgos Críticos
```xaml
<Border Background="White" CornerRadius="16" Padding="20" Margin="0,0,20,20">
    <StackPanel>
        <TextBlock Text="🔴 Riesgos Críticos (Top 5)" FontSize="14" FontWeight="Bold"/>
        <ItemsControl ItemsSource="{Binding RiesgosCriticos}">
            <ItemsControl.ItemTemplate>
                <DataTemplate>
                    <Grid Margin="0,10,0,0" Height="60">
                        <Border Background="#FEF2F2" CornerRadius="8" Padding="15">
                            <Grid>
                                <StackPanel VerticalAlignment="Center">
                                    <TextBlock Text="{Binding Peligro.Descripcion}" 
                                              FontWeight="Bold" Foreground="#B91C1C"/>
                                    <TextBlock Text="{Binding NivelRiesgo}" 
                                              Foreground="#7F1D1D" FontSize="11"/>
                                </StackPanel>
                                <StackPanel HorizontalAlignment="Right" VerticalAlignment="Center">
                                    <TextBlock Text="{Binding Probabilidad, StringFormat='P:{0}'}" 
                                              Foreground="#B91C1C" FontWeight="Bold"/>
                                    <TextBlock Text="{Binding Consecuencia, StringFormat='C:{0}'}" 
                                              Foreground="#B91C1C" FontSize="11"/>
                                </StackPanel>
                            </Grid>
                        </Border>
                    </Grid>
                </DataTemplate>
            </ItemsControl.ItemTemplate>
        </ItemsControl>
    </StackPanel>
</Border>
```

**Backend**:
```csharp
// En DashboardViewModel
[ObservableProperty]
private List<MatrizRiesgoEmpresa> _riesgosCriticos = new();

private async Task CargarRiesgosCriticos()
{
    var riesgos = await _matrizService.ObtenerMatrizPorCentroAsync(CentroSeleccionado.Id);
    RiesgosCriticos = riesgos
        .OrderByDescending(r => r.Probabilidad * r.Consecuencia)
        .Take(5)
        .ToList();
}
```

##### Sección 2: Documentos Pendientes de Actualización
```xaml
<Border Background="White" CornerRadius="16" Padding="20" Margin="0,0,20,20">
    <StackPanel>
        <TextBlock Text="📄 Documentos Pendientes" FontSize="14" FontWeight="Bold"/>
        <ListBox ItemsSource="{Binding DocumentosPendientes}" SelectionMode="Multiple">
            <ListBox.ItemTemplate>
                <DataTemplate>
                    <Border Background="#FFFBEB" CornerRadius="4" Padding="10" Margin="0,5">
                        <Grid>
                            <StackPanel>
                                <TextBlock Text="{Binding Nombre}" FontWeight="Bold"/>
                                <TextBlock Text="{Binding FechaVencimiento, StringFormat='Vencimiento: {0:dd/MM/yyyy}'}" 
                                          FontSize="11" Foreground="#92400E"/>
                            </StackPanel>
                            <Button HorizontalAlignment="Right" Content="ACTUALIZAR" 
                                   Command="{Binding GenerarDocumentoCommand}"/>
                        </Grid>
                    </Border>
                </DataTemplate>
            </ListBox.ItemTemplate>
        </ListBox>
    </StackPanel>
</Border>
```

##### Sección 3: Trabajadores sin Documentación
```xaml
<Border Background="White" CornerRadius="16" Padding="20" Margin="0,0,20,20">
    <StackPanel>
        <TextBlock Text="👤 Trabajadores Incompletos" FontSize="14" FontWeight="Bold"/>
        <TextBlock Text="{Binding TrabajadoresSinDocs.Count, StringFormat='{0} trabajadores sin documentación completa'}" 
                  Foreground="#64748B" Margin="0,10,0,20"/>
        <Button Content="VER DETALLES" Command="{Binding VerTrabajadoresSinDocsCommand}"/>
    </StackPanel>
</Border>
```

##### Sección 4: Gráfico de Cumplimiento Tendencia
```xaml
<!-- Requiere LiveCharts.Core -->
<lvc:CartesianChart Series="{Binding CumplimientoTendencia}" 
                    XAxes="{Binding XAxes}" YAxes="{Binding YAxes}"
                    Margin="0,20,0,0" Height="300"/>
```

**Backend para Gráfico**:
```csharp
[ObservableProperty]
private ObservableCollection<ISeries> _cumplimientoTendencia = new();

private async Task CargarTendenciasCumplimiento()
{
    var ultimos90Dias = await _db.LogsAuditoria
        .Where(l => l.Fecha >= DateTime.Now.AddDays(-90))
        .GroupBy(l => l.Fecha.Date)
        .Select(g => new { Fecha = g.Key, Score = CalcularScore(g) })
        .ToListAsync();
    
    var series = new LineSeries<double>
    {
        Values = ultimos90Dias.Select(x => (double)x.Score).ToList(),
        Name = "Cumplimiento (%)"
    };
    
    CumplimientoTendencia = new ObservableCollection<ISeries> { series };
}
```

---

### TIER 2: IMPORTANTES (Próximas 2 semanas)

#### 2.1 Análisis Automático de Documentos Generados
**Esfuerzo**: 1.5 días | **Impacto**: ALTO (validación)

**Objetivo**: Cuando se genera un documento (RIOHS, Vacaciones, etc), Ollama valida automáticamente:
- ✅ Cumple con DS44
- ✅ Incluye obligaciones específicas del rubro
- ✅ Datos son consistentes (PDF = DOCX)
- ✅ No tiene brechas normativas

**Implementación**:
```csharp
// En DocumentoService.cs
public async Task<GeneracionResultado> GenerarDocumentoAsync(...)
{
    // ... generación existente ...
    
    // NUEVO: Validar con IA
    var validacion = await _aiService.AnalizarTextoAsync(
        prompt: "Valida que este documento cumple con DS44 y Ley Karin. Lista 3 aspectos positivos y 2 mejoras.",
        contexto: contenidoDocumento
    );
    
    resultado.ValidacionIA = validacion;
    resultado.Apto = !validacion.Contains("crítico") && !validacion.Contains("incumple");
    
    if (!resultado.Apto)
    {
        resultado.Advertencias = ExtractWarnings(validacion);
        // Usuario puede ignorar advertencias o regenerar
    }
    
    return resultado;
}
```

---

#### 2.2 Generación Inteligente de Obligaciones
**Esfuerzo**: 2 días | **Impacto**: MEDIO (automatización)

**Objetivo**: Cuando se crea una nueva empresa, IA genera automáticamente obligaciones relevantes por rubro

```csharp
// Nuevo servicio
public interface IObligacionesIAService
{
    Task<List<Obligacion>> GenerarObligacionesPorRubroAsync(int rubroId, int empresaId);
}

// Uso
var obligaciones = await _obligacionesIA.GenerarObligacionesPorRubroAsync(
    empresa.RubroId, empresa.Id
);
```

---

### TIER 3: DESEABLES (Próximo mes)

#### 3.1 Vector Embeddings para Búsqueda Semántica Real
**Esfuerzo**: 3 días | **Impacto**: ALTO (escalabilidad RAG)

**Problema actual**: Búsqueda RAG es por palabras clave (frágil)  
**Solución**: Embeddings vectoriales + búsqueda semántica

```
1. Descargar modelo: ollama pull all-minilm-l6-v2
2. Al ingerir documento: Generar embedding
3. Al buscar: Usar embedding query + cosine similarity
4. Almacenar en pgvector o sqlite-vss
```

#### 3.2 Análisis de Riesgos Psicosociales Automático
**Esfuerzo**: 1.5 días | **Impacto**: ALTO (cumplimiento)

**Objetivo**: Integrar protocolo CEAL-SM (Comisión de Evaluación de Ambientes Laborales)

```csharp
var psicosocialAnalysis = await _aiService.AnalizarTextoAsync(
    prompt: "Analiza riesgos psicosociales según CEAL-SM",
    contexto: descripcionActividadTrabajo
);
```

---

## ✅ CHECKLIST DE VALIDACIÓN ACTUAL

```
COMPILACIÓN (HOY):
✅ Compilación exitosa sin errores
✅ Mejoras #1, #2, #3 implementadas

TESTING PENDIENTE:
⚠️  Ejecutar con Ollama corriendo
⚠️  Probar botón "IA: Riesgos por Rubro"
⚠️  Validar JSON parsing
⚠️  Validar performance con 500+ peligros

IMPLEMENTACIONES SUGERIDAS (ORDEN):
1️⃣  P1: Agrupación MIPER (1 día)
2️⃣  P2: Búsqueda + Filtros (1.5 días)
3️⃣  P3: Dashboard enriquecido (2 días)
4️⃣  P4: Análisis automático docs (1.5 días)
5️⃣  P5: Obligaciones IA (2 días)
6️⃣  P6: Vector embeddings (3 días)
7️⃣  P7: Psicosocial CEAL-SM (1.5 días)

TOTAL ESTIMADO: ~13 días de desarrollo
EQUIPO RECOMENDADO: 1-2 desarrolladores
```

---

## 📞 CONCLUSIÓN

La aplicación ahora tiene **Ollama integrado de forma inteligente** con:
- ✅ Sugerencias contextuales por rubro
- ✅ Temperatura configurable según caso de uso
- ✅ Soporte para múltiples modelos
- ✅ UI mejorada en MIPER

**Próximo paso recomendado**: Implementar Mejoras P1 y P2 (agrupación + búsqueda) para completar la renovación de MIPER.

