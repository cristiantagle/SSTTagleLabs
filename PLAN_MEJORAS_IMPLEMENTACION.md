# PLAN DE MEJORAS - Implementación Detallada

Este documento proporciona un plan de acción estructurado para implementar mejoras prioritarias en el proyecto SST TagleLabs. Está diseñado para ser ejecutado por agentes de IA o desarrolladores.

**Última actualización**: Diciembre 2025  
**Estado**: Activo - Listo para implementación

---

## 📋 Índice

1. [Quick Wins (Esta Semana)](#quick-wins-esta-semana)
2. [Mejoras Críticas (Próximas 2 Semanas)](#mejoras-críticas-próximas-2-semanas)
3. [Mejoras Importantes (Próximo Mes)](#mejoras-importantes-próximo-mes)
4. [Criterios de Aceptación](#criterios-de-aceptación)
5. [Testing Requirements](#testing-requirements)

---

## 🚀 QUICK WINS (Esta Semana)

### 1. Validación Automática de Documentos con IA

**Prioridad**: ALTA | **Esfuerzo**: 1 día | **Impacto**: ALTO

#### Contexto
El método `ValidarDocumentoConIAAsync()` ya existe en `LocalAiService.cs` pero no se está usando después de generar documentos.

#### Archivos a Modificar
- `TagleLabsGestorSST.Services/DocumentoService.cs`
- `TagleLabsGestorSST.UI/ViewModels/DocumentosViewModel.cs` (si aplica)

#### Pasos de Implementación

1. **Localizar el punto de generación de documentos**
   ```csharp
   // Buscar en DocumentoService.cs métodos como:
   // - GenerarRIOHSAsync()
   // - GenerarIRLAsync()
   // - GenerarReglamentoInternoAsync()
   ```

2. **Agregar validación post-generación**
   ```csharp
   // Después de generar el documento, agregar:
   var validacion = await _aiService.ValidarDocumentoConIAAsync(
       contenidoDocumento: contenidoGenerado,
       tipoDocumento: "RIOHS" // o el tipo correspondiente
   );
   
   // Agregar resultado de validación al objeto de retorno
   resultado.ValidacionIA = validacion;
   resultado.ScoreCumplimiento = validacion.Score;
   resultado.Aprobado = validacion.Aprobado;
   ```

3. **Mostrar resultados en UI**
   - Si `validacion.Aprobado == false`, mostrar advertencias
   - Mostrar score de cumplimiento (0-100)
   - Listar aspectos positivos y mejoras sugeridas

#### Criterios de Aceptación
- [ ] Todos los documentos generados son validados automáticamente
- [ ] El score de cumplimiento se muestra en la UI
- [ ] Las advertencias se muestran claramente si hay incumplimientos
- [ ] El usuario puede regenerar el documento si no está aprobado

#### Testing
```csharp
// Test unitario sugerido
[Fact]
public async Task GenerarRIOHS_DeberiaValidarConIA()
{
    // Arrange
    var documentoService = new DocumentoService(...);
    
    // Act
    var resultado = await documentoService.GenerarRIOHSAsync(...);
    
    // Assert
    Assert.NotNull(resultado.ValidacionIA);
    Assert.True(resultado.ScoreCumplimiento >= 0 && resultado.ScoreCumplimiento <= 100);
}
```

---

### 2. Agrupación MIPER por Categoría

**Prioridad**: ALTA | **Esfuerzo**: 1 día | **Impacto**: ALTO (UX)

#### Contexto
La matriz MIPER muestra una lista plana de peligros. Los usuarios necesitan agrupar por categoría (Ergonómico, Físico, Químico, etc.) para mejor navegación.

#### Archivos a Modificar
- `TagleLabsGestorSST.UI/ViewModels/MatrizRiesgosViewModel.cs`
- `TagleLabsGestorSST.UI/Views/MatrizRiesgosView.xaml`

#### Pasos de Implementación

1. **Modificar ViewModel para agrupar**
   ```csharp
   // En MatrizRiesgosViewModel.cs
   private ObservableCollection<MatrizRiesgoEmpresa> _matrizSource = new();
   private ICollectionView? _matrizView;
   
   public ICollectionView MatrizView
   {
       get
       {
           if (_matrizView == null)
           {
               _matrizView = CollectionViewSource.GetDefaultView(_matrizSource);
               _matrizView.GroupDescriptions.Add(
                   new PropertyGroupDescription("Peligro.Categoria")
               );
           }
           return _matrizView;
       }
   }
   
   // Al cargar la matriz:
   private async Task CargarMatriz()
   {
       var matriz = await _matrizService.ObtenerMatrizPorCentroAsync(...);
       _matrizSource.Clear();
       foreach (var item in matriz)
       {
           _matrizSource.Add(item);
       }
       // El CollectionView se actualiza automáticamente
   }
   ```

2. **Cambiar XAML de DataGrid a TreeView**
   ```xaml
   <!-- Reemplazar DataGrid con TreeView -->
   <TreeView ItemsSource="{Binding MatrizView.Groups}" 
             Background="Transparent"
             Margin="10">
       <TreeView.ItemTemplate>
           <HierarchicalDataTemplate ItemsSource="{Binding Items}">
               <!-- Encabezado de categoría -->
               <StackPanel Orientation="Horizontal" Margin="5">
                   <TextBlock FontWeight="Bold" 
                             Foreground="#8B5CF6" 
                             Text="{Binding Name}" 
                             FontSize="14"/>
                   <TextBlock Foreground="#94A3B8" 
                             Margin="10,0,0,0"
                             Text="{Binding ItemCount, StringFormat='({0} riesgos)'}"/>
               </StackPanel>
               
               <!-- Items dentro de cada categoría -->
               <HierarchicalDataTemplate.ItemTemplate>
                   <DataTemplate>
                       <Border Background="White" 
                              CornerRadius="4" 
                              Padding="10" 
                              Margin="5"
                              BorderBrush="#E2E8F0" 
                              BorderThickness="1">
                           <Grid>
                               <Grid.ColumnDefinitions>
                                   <ColumnDefinition Width="*"/>
                                   <ColumnDefinition Width="100"/>
                                   <ColumnDefinition Width="100"/>
                               </Grid.ColumnDefinitions>
                               
                               <TextBlock Grid.Column="0" 
                                         Text="{Binding Peligro.Descripcion}" 
                                         FontSize="12"/>
                               <TextBlock Grid.Column="1" 
                                         Text="{Binding NivelRiesgo}" 
                                         HorizontalAlignment="Center"/>
                               <TextBlock Grid.Column="2" 
                                         Text="{Binding Probabilidad, StringFormat='P:{0}'}" 
                                         HorizontalAlignment="Center"/>
                           </Grid>
                       </Border>
                   </DataTemplate>
               </HierarchicalDataTemplate.ItemTemplate>
           </HierarchicalDataTemplate>
       </TreeView.ItemTemplate>
   </TreeView>
   ```

#### Criterios de Aceptación
- [ ] Los riesgos se agrupan visualmente por categoría
- [ ] Cada categoría muestra el conteo de riesgos
- [ ] Las categorías son expandibles/colapsables
- [ ] Al hacer click en un riesgo, se muestran los detalles

#### Testing
- Abrir MIPER con datos de prueba
- Verificar que se agrupan correctamente por categoría
- Expandir/colapsar categorías
- Verificar conteo de riesgos por categoría

---

### 3. Top 5 Riesgos Críticos en Dashboard

**Prioridad**: ALTA | **Esfuerzo**: 0.5 días | **Impacto**: MEDIO (Visibilidad)

#### Contexto
El dashboard actual muestra KPIs básicos pero no resalta los riesgos más críticos que requieren atención inmediata.

#### Archivos a Modificar
- `TagleLabsGestorSST.UI/ViewModels/DashboardViewModel.cs`
- `TagleLabsGestorSST.UI/Views/DashboardView.xaml`

#### Pasos de Implementación

1. **Agregar propiedad en ViewModel**
   ```csharp
   // En DashboardViewModel.cs
   [ObservableProperty]
   private List<MatrizRiesgoEmpresa> _riesgosCriticos = new();
   
   private async Task CargarRiesgosCriticos()
   {
       if (CentroSeleccionado == null) return;
       
       var matriz = await _matrizService.ObtenerMatrizPorCentroAsync(
           CentroSeleccionado.Id
       );
       
       RiesgosCriticos = matriz
           .OrderByDescending(r => r.Probabilidad * r.Consecuencia)
           .Take(5)
           .ToList();
   }
   
   // Llamar en OnCentroSeleccionadoChanged o al cargar dashboard
   ```

2. **Agregar sección en XAML**
   ```xaml
   <!-- Agregar después de los KPIs existentes -->
   <Border Background="White" 
          CornerRadius="16" 
          Padding="20" 
          Margin="0,20,0,0">
       <StackPanel>
           <TextBlock Text="🔴 Riesgos Críticos (Top 5)" 
                     FontSize="16" 
                     FontWeight="Bold"
                     Margin="0,0,0,15"/>
           
           <ItemsControl ItemsSource="{Binding RiesgosCriticos}">
               <ItemsControl.ItemTemplate>
                   <DataTemplate>
                       <Border Background="#FEF2F2" 
                              CornerRadius="8" 
                              Padding="15" 
                              Margin="0,5">
                           <Grid>
                               <Grid.ColumnDefinitions>
                                   <ColumnDefinition Width="*"/>
                                   <ColumnDefinition Width="Auto"/>
                               </Grid.ColumnDefinitions>
                               
                               <StackPanel Grid.Column="0">
                                   <TextBlock Text="{Binding Peligro.Descripcion}" 
                                             FontWeight="Bold" 
                                             Foreground="#B91C1C"
                                             TextWrapping="Wrap"/>
                                   <TextBlock Text="{Binding NivelRiesgo}" 
                                             Foreground="#7F1D1D" 
                                             FontSize="11"
                                             Margin="0,5,0,0"/>
                               </StackPanel>
                               
                               <StackPanel Grid.Column="1" 
                                          HorizontalAlignment="Right">
                                   <TextBlock Text="{Binding Probabilidad, StringFormat='P:{0}'}" 
                                             Foreground="#B91C1C" 
                                             FontWeight="Bold"/>
                                   <TextBlock Text="{Binding Consecuencia, StringFormat='C:{0}'}" 
                                             Foreground="#B91C1C" 
                                             FontSize="11"/>
                               </StackPanel>
                           </Grid>
                       </Border>
                   </DataTemplate>
               </ItemsControl.ItemTemplate>
           </ItemsControl>
           
           <TextBlock Text="No hay riesgos críticos registrados" 
                     Visibility="{Binding RiesgosCriticos.Count, Converter={StaticResource CountToVisibilityConverter}}"
                     Foreground="#94A3B8"
                     Margin="0,20,0,0"
                     HorizontalAlignment="Center"/>
       </StackPanel>
   </Border>
   ```

#### Criterios de Aceptación
- [ ] Se muestran los 5 riesgos con mayor score (P × C)
- [ ] Los riesgos se ordenan de mayor a menor criticidad
- [ ] Se muestra claramente probabilidad y consecuencia
- [ ] Si no hay riesgos, se muestra mensaje apropiado

#### Testing
- Cargar dashboard con empresa que tenga riesgos
- Verificar que se muestran exactamente 5 riesgos
- Verificar ordenamiento correcto
- Probar con empresa sin riesgos

---

## 🔥 MEJORAS CRÍTICAS (Próximas 2 Semanas)

### 4. Búsqueda y Filtros Avanzados en MIPER

**Prioridad**: ALTA | **Esfuerzo**: 1.5 días | **Impacto**: ALTO

#### Contexto
Con 500+ peligros en BD, los usuarios necesitan buscar y filtrar eficientemente.

#### Archivos a Modificar
- `TagleLabsGestorSST.UI/ViewModels/MatrizRiesgosViewModel.cs`
- `TagleLabsGestorSST.UI/Views/MatrizRiesgosView.xaml`

#### Pasos de Implementación

1. **Agregar propiedades de filtro en ViewModel**
   ```csharp
   [ObservableProperty]
   private string _filtroTexto = string.Empty;
   
   [ObservableProperty]
   private string? _categoriaFiltro;
   
   [ObservableProperty]
   private string? _nivelRiesgoFiltro;
   
   [ObservableProperty]
   private ObservableCollection<string> _categoriasDisponibles = new();
   
   [ObservableProperty]
   private List<string> _nivelesRiesgo = new() 
   { 
       "Bajo", 
       "Medio", 
       "Alto", 
       "Crítico" 
   };
   
   // Métodos que se ejecutan cuando cambian los filtros
   partial void OnFiltroTextoChanged(string value) => AplicarFiltros();
   partial void OnCategoriaFiltroChanged(string? value) => AplicarFiltros();
   partial void OnNivelRiesgoFiltroChanged(string? value) => AplicarFiltros();
   
   private void AplicarFiltros()
   {
       if (_matrizSource == null || !_matrizSource.Any()) return;
       
       var filtered = _matrizSource
           .Where(m => 
               string.IsNullOrWhiteSpace(FiltroTexto) || 
               m.Peligro.Descripcion.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
               m.Peligro.Categoria.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase) ||
               m.MedidaControlEspecifica.Contains(FiltroTexto, StringComparison.OrdinalIgnoreCase))
           .Where(m => 
               string.IsNullOrWhiteSpace(CategoriaFiltro) || 
               m.Peligro.Categoria == CategoriaFiltro)
           .Where(m => 
               string.IsNullOrWhiteSpace(NivelRiesgoFiltro) || 
               m.NivelRiesgo == NivelRiesgoFiltro)
           .ToList();
       
       // Actualizar CollectionView con filtros aplicados
       var view = CollectionViewSource.GetDefaultView(
           new ObservableCollection<MatrizRiesgoEmpresa>(filtered)
       );
       view.GroupDescriptions.Add(new PropertyGroupDescription("Peligro.Categoria"));
       MatrizView = view;
   }
   
   // Cargar categorías disponibles al cargar matriz
   private void CargarCategoriasDisponibles()
   {
       CategoriasDisponibles = new ObservableCollection<string>(
           _matrizSource
               .Select(m => m.Peligro.Categoria)
               .Distinct()
               .OrderBy(c => c)
       );
   }
   ```

2. **Agregar controles de filtro en XAML**
   ```xaml
   <!-- Agregar antes del TreeView -->
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
               Style="{StaticResource MaterialDesignOutlinedTextBox}">
           <TextBox.InputBindings>
               <KeyBinding Key="Escape" Command="{Binding LimpiarFiltrosCommand}"/>
           </TextBox.InputBindings>
       </TextBox>
       
       <!-- Filtro por categoría -->
       <ComboBox Grid.Column="1" 
                ItemsSource="{Binding CategoriasDisponibles}" 
                SelectedItem="{Binding CategoriaFiltro}"
                materialDesign:HintAssist.Hint="Filtrar por categoría..."
                Margin="10,0">
           <ComboBox.ItemTemplate>
               <DataTemplate>
                   <TextBlock Text="{Binding}"/>
               </DataTemplate>
           </ComboBox.ItemTemplate>
       </ComboBox>
       
       <!-- Filtro por nivel riesgo -->
       <ComboBox Grid.Column="2" 
                ItemsSource="{Binding NivelesRiesgo}"
                SelectedItem="{Binding NivelRiesgoFiltro}"
                materialDesign:HintAssist.Hint="Filtro por nivel..."
                Margin="10,0"/>
   </Grid>
   
   <!-- Botón para limpiar filtros -->
   <Button Content="Limpiar Filtros" 
          Command="{Binding LimpiarFiltrosCommand}"
          Margin="0,10,0,0"
          HorizontalAlignment="Right"/>
   ```

3. **Agregar comando para limpiar filtros**
   ```csharp
   [RelayCommand]
   private void LimpiarFiltros()
   {
       FiltroTexto = string.Empty;
       CategoriaFiltro = null;
       NivelRiesgoFiltro = null;
       // AplicarFiltros() se ejecuta automáticamente por los setters
   }
   ```

#### Criterios de Aceptación
- [ ] Búsqueda funciona en tiempo real (UpdateSourceTrigger)
- [ ] Los filtros se pueden combinar (AND lógico)
- [ ] Los resultados se actualizan inmediatamente
- [ ] Se puede limpiar todos los filtros con un botón
- [ ] La búsqueda es case-insensitive

#### Testing
- Buscar por texto parcial
- Filtrar por categoría específica
- Filtrar por nivel de riesgo
- Combinar múltiples filtros
- Limpiar filtros y verificar que se muestran todos los riesgos

---

### 5. Dashboard Enriquecido Completo

**Prioridad**: ALTA | **Esfuerzo**: 1.5 días | **Impacto**: ALTO

#### Contexto
Expandir el dashboard con más información útil: documentos pendientes, trabajadores incompletos, gráficos de tendencia.

#### Archivos a Modificar
- `TagleLabsGestorSST.UI/ViewModels/DashboardViewModel.cs`
- `TagleLabsGestorSST.UI/Views/DashboardView.xaml`

#### Pasos de Implementación

1. **Agregar propiedades adicionales en ViewModel**
   ```csharp
   [ObservableProperty]
   private List<DocumentoPendiente> _documentosPendientes = new();
   
   [ObservableProperty]
   private int _trabajadoresSinDocs = 0;
   
   [ObservableProperty]
   private ObservableCollection<CumplimientoTendencia> _cumplimientoTendencia = new();
   
   public class DocumentoPendiente
   {
       public string Nombre { get; set; } = string.Empty;
       public DateTime FechaVencimiento { get; set; }
       public string Tipo { get; set; } = string.Empty;
   }
   
   public class CumplimientoTendencia
   {
       public DateTime Fecha { get; set; }
       public double Score { get; set; }
   }
   
   private async Task CargarDocumentosPendientes()
   {
       // Lógica para obtener documentos próximos a vencer
       var documentos = await _documentoService.ObtenerDocumentosPendientesAsync(
           EmpresaSeleccionada.Id
       );
       DocumentosPendientes = documentos
           .Where(d => d.FechaVencimiento <= DateTime.Now.AddDays(30))
           .OrderBy(d => d.FechaVencimiento)
           .Take(10)
           .ToList();
   }
   
   private async Task CargarTrabajadoresSinDocs()
   {
       var trabajadores = await _trabajadorService.ObtenerTrabajadoresAsync(
           EmpresaSeleccionada.Id
       );
       TrabajadoresSinDocs = trabajadores.Count(t => !t.DocumentacionCompleta);
   }
   
   private async Task CargarTendenciasCumplimiento()
   {
       var ultimos90Dias = await _auditService.ObtenerLogsUltimos90DiasAsync(
           EmpresaSeleccionada.Id
       );
       
       var tendencias = ultimos90Dias
           .GroupBy(l => l.Fecha.Date)
           .Select(g => new CumplimientoTendencia
           {
               Fecha = g.Key,
               Score = CalcularScoreCumplimiento(g)
           })
           .OrderBy(t => t.Fecha)
           .ToList();
       
       CumplimientoTendencia = new ObservableCollection<CumplimientoTendencia>(tendencias);
   }
   
   private double CalcularScoreCumplimiento(IGrouping<DateTime, LogAuditoria> logs)
   {
       // Lógica para calcular score de cumplimiento
       // Basado en documentos generados, actualizados, etc.
       return 85.5; // Ejemplo
   }
   ```

2. **Agregar secciones en XAML**
   ```xaml
   <!-- Documentos Pendientes -->
   <Border Background="White" CornerRadius="16" Padding="20" Margin="0,20,0,0">
       <StackPanel>
           <TextBlock Text="📄 Documentos Pendientes" 
                     FontSize="16" 
                     FontWeight="Bold"
                     Margin="0,0,0,15"/>
           
           <ItemsControl ItemsSource="{Binding DocumentosPendientes}">
               <ItemsControl.ItemTemplate>
                   <DataTemplate>
                       <Border Background="#FFFBEB" 
                              CornerRadius="4" 
                              Padding="10" 
                              Margin="0,5">
                           <Grid>
                               <Grid.ColumnDefinitions>
                                   <ColumnDefinition Width="*"/>
                                   <ColumnDefinition Width="Auto"/>
                               </Grid.ColumnDefinitions>
                               
                               <StackPanel Grid.Column="0">
                                   <TextBlock Text="{Binding Nombre}" 
                                             FontWeight="Bold"/>
                                   <TextBlock Text="{Binding FechaVencimiento, StringFormat='Vence: {0:dd/MM/yyyy}'}" 
                                             FontSize="11" 
                                             Foreground="#92400E"
                                             Margin="0,5,0,0"/>
                               </StackPanel>
                               
                               <Button Grid.Column="1" 
                                      Content="ACTUALIZAR" 
                                      Command="{Binding DataContext.GenerarDocumentoCommand, RelativeSource={RelativeSource AncestorType=Window}}"
                                      CommandParameter="{Binding}"
                                      HorizontalAlignment="Right"/>
                           </Grid>
                       </Border>
                   </DataTemplate>
               </ItemsControl.ItemTemplate>
           </ItemsControl>
           
           <TextBlock Text="No hay documentos pendientes" 
                     Visibility="{Binding DocumentosPendientes.Count, Converter={StaticResource CountToVisibilityConverter}}"
                     Foreground="#94A3B8"
                     Margin="0,20,0,0"
                     HorizontalAlignment="Center"/>
       </StackPanel>
   </Border>
   
   <!-- Trabajadores sin Documentación -->
   <Border Background="White" CornerRadius="16" Padding="20" Margin="0,20,0,0">
       <StackPanel>
           <TextBlock Text="👤 Trabajadores Incompletos" 
                     FontSize="16" 
                     FontWeight="Bold"
                     Margin="0,0,0,15"/>
           
           <TextBlock Text="{Binding TrabajadoresSinDocs, StringFormat='{0} trabajadores sin documentación completa'}" 
                     Foreground="#64748B" 
                     FontSize="14"
                     Margin="0,10,0,20"/>
           
           <Button Content="VER DETALLES" 
                  Command="{Binding VerTrabajadoresSinDocsCommand}"
                  HorizontalAlignment="Center"/>
       </StackPanel>
   </Border>
   ```

#### Criterios de Aceptación
- [ ] Se muestran documentos próximos a vencer (próximos 30 días)
- [ ] Se muestra conteo de trabajadores sin documentación completa
- [ ] Los documentos pendientes tienen botón para actualizar
- [ ] Se puede navegar a detalles de trabajadores incompletos

#### Testing
- Verificar que se cargan documentos pendientes correctamente
- Verificar conteo de trabajadores sin docs
- Probar navegación a detalles
- Probar con empresa sin documentos pendientes

---

### 6. Modo Offline en Kioscos (AsistenciaPro)

**Prioridad**: CRÍTICA | **Esfuerzo**: 3 días | **Impacto**: ALTO

#### Contexto
Los kioscos necesitan funcionar sin conexión a internet para registrar marcaciones que se sincronizarán cuando haya conexión.

#### Archivos a Modificar
- `AsistenciaPro/src/components/kiosk/KioskTerminal.tsx`
- `AsistenciaPro/src/lib/kiosk.ts` (nuevo archivo para lógica offline)
- `AsistenciaPro/public/sw.js` (service worker)

#### Pasos de Implementación

1. **Crear servicio de caché offline**
   ```typescript
   // AsistenciaPro/src/lib/kiosk-offline.ts
   const OFFLINE_STORAGE_KEY = 'kiosk_pending_marks';
   
   export interface PendingMark {
     id: string;
     employeeId: string;
     type: 'entry' | 'lunch_start' | 'lunch_end' | 'exit';
     timestamp: string;
     deviceId: string;
     retryCount: number;
   }
   
   export async function savePendingMark(mark: Omit<PendingMark, 'id' | 'retryCount'>): Promise<void> {
     const pending = await getPendingMarks();
     const newMark: PendingMark = {
       ...mark,
       id: crypto.randomUUID(),
       retryCount: 0,
     };
     pending.push(newMark);
     await localStorage.setItem(OFFLINE_STORAGE_KEY, JSON.stringify(pending));
   }
   
   export async function getPendingMarks(): Promise<PendingMark[]> {
     const stored = await localStorage.getItem(OFFLINE_STORAGE_KEY);
     return stored ? JSON.parse(stored) : [];
   }
   
   export async function removePendingMark(id: string): Promise<void> {
     const pending = await getPendingMarks();
     const filtered = pending.filter(m => m.id !== id);
     await localStorage.setItem(OFFLINE_STORAGE_KEY, JSON.stringify(filtered));
   }
   
   export async function syncPendingMarks(slug: string): Promise<void> {
     const pending = await getPendingMarks();
     const online = navigator.onLine;
     
     if (!online) return;
     
     for (const mark of pending) {
       try {
         const response = await fetch(`/api/kiosk/${slug}/mark`, {
           method: 'POST',
           headers: { 'Content-Type': 'application/json' },
           body: JSON.stringify({
             employeeId: mark.employeeId,
             type: mark.type,
             timestamp: mark.timestamp,
           }),
         });
         
         if (response.ok) {
           await removePendingMark(mark.id);
         } else {
           mark.retryCount++;
           if (mark.retryCount > 5) {
             await removePendingMark(mark.id); // Dar por perdido después de 5 intentos
           }
         }
       } catch (error) {
         console.error('Error syncing mark:', error);
         mark.retryCount++;
       }
     }
     
     // Actualizar storage con nuevos retryCount
     const updated = await getPendingMarks();
     await localStorage.setItem(OFFLINE_STORAGE_KEY, JSON.stringify(updated));
   }
   ```

2. **Modificar KioskTerminal para usar caché offline**
   ```typescript
   // En KioskTerminal.tsx
   import { savePendingMark, syncPendingMarks } from '@/lib/kiosk-offline';
   
   const handleMarkAttendance = async (type: MarkType) => {
     try {
       const online = navigator.onLine;
       
       if (!online) {
         // Guardar en caché local
         await savePendingMark({
           employeeId: selectedEmployee.id,
           type,
           timestamp: new Date().toISOString(),
           deviceId: deviceToken,
         });
         
         toast.success('Marcación guardada localmente. Se sincronizará cuando haya conexión.');
         return;
       }
       
       // Intentar enviar normalmente
       const response = await fetch(`/api/kiosk/${slug}/mark`, {
         method: 'POST',
         headers: { 'Content-Type': 'application/json' },
         body: JSON.stringify({
           employeeId: selectedEmployee.id,
           type,
         }),
       });
       
       if (!response.ok) {
         // Si falla, guardar en caché
         await savePendingMark({
           employeeId: selectedEmployee.id,
           type,
           timestamp: new Date().toISOString(),
           deviceId: deviceToken,
         });
         toast.warning('Marcación guardada localmente. Se sincronizará cuando haya conexión.');
       }
     } catch (error) {
       // En caso de error, guardar en caché
       await savePendingMark({
         employeeId: selectedEmployee.id,
         type,
         timestamp: new Date().toISOString(),
         deviceId: deviceToken,
       });
       toast.warning('Marcación guardada localmente.');
     }
   };
   
   // Sincronizar cuando se detecta conexión
   useEffect(() => {
     const handleOnline = () => {
       syncPendingMarks(slug);
     };
     
     window.addEventListener('online', handleOnline);
     return () => window.removeEventListener('online', handleOnline);
   }, [slug]);
   
   // Sincronizar periódicamente cada 30 segundos si hay conexión
   useEffect(() => {
     const interval = setInterval(() => {
       if (navigator.onLine) {
         syncPendingMarks(slug);
       }
     }, 30000);
     
     return () => clearInterval(interval);
   }, [slug]);
   ```

3. **Mostrar indicador de estado offline**
   ```typescript
   const [isOnline, setIsOnline] = useState(navigator.onLine);
   const [pendingCount, setPendingCount] = useState(0);
   
   useEffect(() => {
     const handleOnline = () => setIsOnline(true);
     const handleOffline = () => setIsOnline(false);
     
     window.addEventListener('online', handleOnline);
     window.addEventListener('offline', handleOffline);
     
     return () => {
       window.removeEventListener('online', handleOnline);
       window.removeEventListener('offline', handleOffline);
     };
   }, []);
   
   useEffect(() => {
     const updatePendingCount = async () => {
       const pending = await getPendingMarks();
       setPendingCount(pending.length);
     };
     updatePendingCount();
     const interval = setInterval(updatePendingCount, 5000);
     return () => clearInterval(interval);
   }, []);
   
   // En el JSX:
   {!isOnline && (
     <div className="fixed top-4 right-4 bg-yellow-500 text-white px-4 py-2 rounded">
       Modo Offline - {pendingCount} marcaciones pendientes
     </div>
   )}
   ```

#### Criterios de Aceptación
- [ ] Las marcaciones se guardan localmente cuando no hay conexión
- [ ] Se sincronizan automáticamente cuando se recupera la conexión
- [ ] Se muestra indicador visual del estado offline
- [ ] Se muestra conteo de marcaciones pendientes
- [ ] Las marcaciones se sincronizan periódicamente cuando hay conexión

#### Testing
- Desconectar internet y registrar marcación
- Verificar que se guarda en localStorage
- Reconectar internet y verificar sincronización
- Probar con múltiples marcaciones pendientes
- Probar límite de reintentos (5)

---

## 📊 MEJORAS IMPORTANTES (Próximo Mes)

### 7. Optimización RAG con Embeddings Vectoriales

**Prioridad**: MEDIA | **Esfuerzo**: 3 días | **Impacto**: ALTO

#### Contexto
El método `GetEmbeddingAsync()` ya existe pero los embeddings no se persisten. Necesitamos crear un índice vectorial para búsqueda semántica eficiente.

#### Archivos a Modificar
- `TagleLabsGestorSST.Data/TagleLabsContext.cs` (agregar tabla para embeddings)
- `TagleLabsGestorSST.Services/LocalAiService.cs` (mejorar ObtenerContextoRelevante)
- Crear migración SQL para nueva tabla

#### Pasos de Implementación

1. **Crear tabla para embeddings en SQLite**
   ```sql
   -- Migración: crear tabla KnowledgeEmbeddings
   CREATE TABLE IF NOT EXISTS KnowledgeEmbeddings (
       Id INTEGER PRIMARY KEY AUTOINCREMENT,
       KnowledgeChunkId INTEGER NOT NULL,
       Embedding TEXT NOT NULL, -- JSON array de floats
       CreatedAt DATETIME NOT NULL DEFAULT CURRENT_TIMESTAMP,
       FOREIGN KEY (KnowledgeChunkId) REFERENCES KnowledgeChunks(Id)
   );
   
   CREATE INDEX IF NOT EXISTS idx_knowledge_embeddings_chunk 
       ON KnowledgeEmbeddings(KnowledgeChunkId);
   ```

2. **Modificar ObtenerContextoRelevante para usar embeddings persistentes**
   ```csharp
   // En LocalAiService.cs
   private async Task<string> ObtenerContextoRelevante(string query)
   {
       try
       {
           // 1. Obtener embedding de la consulta
           var queryEmbedding = await GetEmbeddingAsync(query);
           if (queryEmbedding.Length == 0) 
               return "No se pudo generar embedding para la consulta.";

           using (var scope = _scopeFactory.CreateScope())
           {
               var db = scope.ServiceProvider.GetRequiredService<TagleLabsContext>();
               
               // 2. Obtener embeddings almacenados (no calcular en tiempo real)
               var storedEmbeddings = await db.KnowledgeEmbeddings
                   .Include(e => e.KnowledgeChunk)
                       .ThenInclude(c => c.KnowledgeItem)
                   .AsNoTracking()
                   .ToListAsync();

               if (!storedEmbeddings.Any()) 
                   return "No hay conocimiento base disponible.";

               // 3. Calcular similitud coseno
               var rankedChunks = storedEmbeddings
                   .Select(e =>
                   {
                       var chunkEmbedding = JsonConvert.DeserializeObject<float[]>(e.Embedding);
                       if (chunkEmbedding == null || chunkEmbedding.Length != queryEmbedding.Length) 
                           return new { Chunk = e.KnowledgeChunk, Score = 0.0 };
                           
                       return new 
                       { 
                           Chunk = e.KnowledgeChunk, 
                           Score = ComputeCosineSimilarity(queryEmbedding, chunkEmbedding) 
                       };
                   })
                   .Where(x => x.Score > 0.3) // Umbral mínimo
                   .OrderByDescending(x => x.Score)
                   .Take(5)
                   .ToList();

               if (rankedChunks.Any())
               {
                   var sb = new StringBuilder();
                   foreach (var item in rankedChunks)
                   {
                       sb.AppendLine($"--- FUENTE: {item.Chunk.KnowledgeItem.SourceFile} (Relevancia: {item.Score:P0}) ---");
                       sb.AppendLine(item.Chunk.Content);
                       sb.AppendLine();
                   }
                   return sb.ToString();
               }
           }
       }
       catch (Exception ex)
       {
           System.Diagnostics.Debug.WriteLine($"Error obteniendo contexto RAG: {ex.Message}");
       }

       return "No se encontró contexto relevante en la base de conocimientos.";
   }
   
   // Método para generar y almacenar embeddings al ingerir documentos
   public async Task GenerarEmbeddingsParaChunksAsync(List<int> chunkIds)
   {
       using (var scope = _scopeFactory.CreateScope())
       {
           var db = scope.ServiceProvider.GetRequiredService<TagleLabsContext>();
           
           var chunks = await db.KnowledgeChunks
               .Where(c => chunkIds.Contains(c.Id))
               .ToListAsync();
           
           foreach (var chunk in chunks)
           {
               // Verificar si ya existe embedding
               var exists = await db.KnowledgeEmbeddings
                   .AnyAsync(e => e.KnowledgeChunkId == chunk.Id);
               
               if (exists) continue;
               
               // Generar embedding
               var embedding = await GetEmbeddingAsync(chunk.Content);
               if (embedding.Length == 0) continue;
               
               // Guardar
               var knowledgeEmbedding = new KnowledgeEmbedding
               {
                   KnowledgeChunkId = chunk.Id,
                   Embedding = JsonConvert.SerializeObject(embedding),
                   CreatedAt = DateTime.Now
               };
               
               db.KnowledgeEmbeddings.Add(knowledgeEmbedding);
           }
           
           await db.SaveChangesAsync();
       }
   }
   ```

3. **Agregar entidad KnowledgeEmbedding**
   ```csharp
   // En TagleLabsGestorSST.Data/Entities/KnowledgeEmbedding.cs
   public class KnowledgeEmbedding
   {
       public int Id { get; set; }
       public int KnowledgeChunkId { get; set; }
       public string Embedding { get; set; } = string.Empty; // JSON array
       public DateTime CreatedAt { get; set; }
       
       public KnowledgeChunk KnowledgeChunk { get; set; } = null!;
   }
   ```

#### Criterios de Aceptación
- [ ] Los embeddings se generan y almacenan al ingerir documentos
- [ ] La búsqueda semántica usa embeddings almacenados (no calcula en tiempo real)
- [ ] La búsqueda es más rápida que la implementación anterior
- [ ] Los resultados son más relevantes semánticamente

#### Testing
- Ingresar nuevo documento y verificar que se generan embeddings
- Buscar con consulta semántica y verificar resultados relevantes
- Comparar tiempo de búsqueda antes/después
- Verificar que no se regeneran embeddings existentes

---

## ✅ CRITERIOS DE ACEPTACIÓN GENERALES

Todas las mejoras deben cumplir con:

1. **Compilación sin errores**
   - AsistenciaPro: `npm run build` exitoso
   - TagleLabsGestorSST: `dotnet build` sin errores ni advertencias críticas

2. **Tests pasando**
   - Agregar tests unitarios para nueva funcionalidad
   - Tests de integración para flujos completos

3. **Documentación actualizada**
   - Actualizar `AGENTS.md` si hay cambios en convenciones
   - Documentar nuevas APIs o endpoints

4. **UX consistente**
   - Seguir patrones de diseño existentes
   - Mantener coherencia visual con el resto de la aplicación

5. **Performance aceptable**
   - No degradar tiempos de carga existentes
   - Optimizar consultas a BD cuando sea necesario

---

## 🧪 TESTING REQUIREMENTS

### Para TagleLabsGestorSST

```csharp
// Estructura de test sugerida
[Fact]
public async Task NuevaFuncionalidad_DeberiaFuncionarCorrectamente()
{
    // Arrange
    var service = new MiServicio(...);
    
    // Act
    var resultado = await service.MiMetodoAsync(...);
    
    // Assert
    Assert.NotNull(resultado);
    Assert.True(resultado.CondicionEsperada);
}
```

### Para AsistenciaPro

```typescript
// Estructura de test sugerida
describe('NuevaFuncionalidad', () => {
  it('debería funcionar correctamente', async () => {
    // Arrange
    const service = new MiServicio();
    
    // Act
    const resultado = await service.miMetodo();
    
    // Assert
    expect(resultado).toBeDefined();
    expect(resultado.condicionEsperada).toBe(true);
  });
});
```

---

## 📝 NOTAS DE IMPLEMENTACIÓN

### Orden Recomendado de Implementación

1. **Semana 1**: Quick Wins (Items 1-3)
2. **Semana 2**: Búsqueda/Filtros MIPER (Item 4)
3. **Semana 3**: Dashboard completo (Item 5)
4. **Semana 4**: Modo offline kioscos (Item 6)
5. **Mes 2**: Optimización RAG (Item 7)

### Consideraciones Técnicas

- **Backward Compatibility**: Asegurar que cambios no rompan funcionalidad existente
- **Migration Path**: Si hay cambios en BD, crear migraciones SQL apropiadas
- **Error Handling**: Implementar manejo de errores robusto en todas las nuevas features
- **Logging**: Agregar logs apropiados para debugging

### Recursos Necesarios

- **Gemini API Key**: Para validación de documentos y RAG
- **Base de datos**: Asegurar espacio suficiente para embeddings
- **Testing Environment**: Ambiente de pruebas separado de producción

---

## 🔗 REFERENCIAS

- **AGENTS.md**: Convenciones y contexto del proyecto
- **HOJA_DE_RUTA_MEJORAS_FUTURAS.md**: Roadmap general
- **ANALISIS_OLLAMA_BRECHAS_Y_MEJORAS.md**: Análisis técnico previo

---

## 📞 CONTACTO Y SOPORTE

Para dudas sobre implementación:
- Revisar `AGENTS.md` para contexto del proyecto
- Consultar documentación técnica en `Docs/MASTER_COMPENDIO.md`
- Contacto: cristian.gonzalez.gt@gmail.com

---

**Última actualización**: Diciembre 2025  
**Estado**: Activo - Listo para implementación

