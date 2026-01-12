using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using MaterialDesignThemes.Wpf;
using Microsoft.Win32;
using TagleLabsGestorSST.Services;
using TagleLabsGestorSST.UI.Services;

namespace TagleLabsGestorSST.UI.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly IServiceProvider _serviceProvider;
    private readonly INotificationService _notificationService;
    private readonly ISessionService _sessionService;
    private readonly ThemeService _themeService;
    private readonly BackupService _backupService;

    // ============================================
    // Snackbar para notificaciones Toast
    // ============================================
    public SnackbarMessageQueue SnackbarMessageQueue { get; } = new SnackbarMessageQueue(TimeSpan.FromSeconds(3));

    [ObservableProperty]
    private object _currentView;

    [ObservableProperty]
    private string _tituloVentana = "Tagle Labs Gestor SST";

    [ObservableProperty]
    private bool _isSidebarVisible = false;

    [ObservableProperty]
    private string _usuarioActualNombre = string.Empty;

    [ObservableProperty]
    private bool _isDarkMode = false;

    partial void OnIsDarkModeChanged(bool value)
    {
        _themeService.SetTheme(value);
    }

    public MainViewModel(IServiceProvider serviceProvider, INotificationService notificationService, ISessionService sessionService, ThemeService themeService, BackupService backupService)
    {
        _serviceProvider = serviceProvider;
        _notificationService = notificationService;
        _sessionService = sessionService;
        _themeService = themeService;
        _backupService = backupService;

        // Suscribirse a eventos de sesión
        _sessionService.OnLoginSuccess += OnLoginSuccess;
        _sessionService.OnLogout += OnLogout;

        // Iniciar con Login
        CurrentView = _serviceProvider.GetRequiredService<LoginViewModel>();
        TituloVentana = "Bienvenido - Iniciar Sesión";
        IsSidebarVisible = false;
    }

    private void OnLoginSuccess()
    {
        IsSidebarVisible = true;
        UsuarioActualNombre = _sessionService.UsuarioActual?.NombreCompleto ?? "Usuario";
        NavigateToDashboard();
    }

    private void OnLogout()
    {
        IsSidebarVisible = false;
        UsuarioActualNombre = string.Empty;
        CurrentView = _serviceProvider.GetRequiredService<LoginViewModel>();
        TituloVentana = "Bienvenido - Iniciar Sesión";
    }

    [RelayCommand]
    private void CerrarSesion()
    {
        _sessionService.CerrarSesion();
    }

    [RelayCommand]
    private void NavigateToDashboard()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<DashboardViewModel>(); TituloVentana = "Dashboard - Tagle Labs Gestor SST"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToEmpresas()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<EmpresasViewModel>(); TituloVentana = "Gestión de Empresas"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToTrabajadores()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<TrabajadoresViewModel>(); TituloVentana = "Trabajadores y Contratistas"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToMatriz()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<MatrizRiesgosViewModel>(); TituloVentana = "Matriz de Identificación de Peligros (MIPER)"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToObligaciones()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<ObligacionesViewModel>(); TituloVentana = "Obligaciones Normativas DS44"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToSiniestros()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<SiniestrosViewModel>(); TituloVentana = "Registro de Accidentes e Incidentes"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }
    
    [RelayCommand]
    private void NavigateToDocumentos()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<DocumentosViewModel>(); TituloVentana = "Generación de Documentos"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToProcedimientos()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<ProcedimientosViewModel>(); TituloVentana = "Procedimientos de Trabajo (PTS)"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToImportacion()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<ImportacionViewModel>(); TituloVentana = "Importación Masiva de Datos"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToAuditoria()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<AuditoriaViewModel>(); TituloVentana = "Auditoría del Sistema"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToActualizacion()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<ActualizacionDocumentosViewModel>(); TituloVentana = "Actualización Documental IA"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToBusquedaSemantica()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<BusquedaSemanticaViewModel>(); TituloVentana = "Búsqueda Semántica en Base de Conocimientos"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToAsistencia()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<AsistenciaViewModel>(); TituloVentana = "Control de Asistencia - AsistenciaPro"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    [RelayCommand]
    private void NavigateToLugaresTrabajo()
    {
        try { CurrentView = _serviceProvider.GetRequiredService<LugarTrabajoViewModel>(); TituloVentana = "Documentos por Lugar de Trabajo"; }
        catch (Exception ex) { System.Windows.MessageBox.Show(ex.ToString()); }
    }

    // ============================================
    // NUEVO: Comandos de Backup/Restauración
    // ============================================
    
    [RelayCommand]
    private async Task CrearBackupAsync()
    {
        try
        {
            var dialog = new SaveFileDialog
            {
                FileName = $"backup_{DateTime.Now:yyyyMMdd_HHmmss}.bak",
                DefaultExt = ".bak",
                Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*",
                Title = "Guardar Backup de Base de Datos"
            };

            if (dialog.ShowDialog() == true)
            {
                var carpeta = System.IO.Path.GetDirectoryName(dialog.FileName) ?? "";
                var archivo = await _backupService.CrearBackupAsync(carpeta);
                
                // Renombrar al nombre elegido por usuario
                if (System.IO.File.Exists(archivo) && archivo != dialog.FileName)
                {
                    System.IO.File.Move(archivo, dialog.FileName, overwrite: true);
                }
                
                SnackbarMessageQueue.Enqueue($"✅ Backup creado exitosamente: {System.IO.Path.GetFileName(dialog.FileName)}");
            }
        }
        catch (Exception ex)
        {
            SnackbarMessageQueue.Enqueue($"❌ Error creando backup: {ex.Message}");
        }
    }

    [RelayCommand]
    private async Task RestaurarBackupAsync()
    {
        try
        {
            var dialog = new OpenFileDialog
            {
                DefaultExt = ".bak",
                Filter = "Backup files (*.bak)|*.bak|All files (*.*)|*.*",
                Title = "Seleccionar Backup para Restaurar"
            };

            if (dialog.ShowDialog() == true)
            {
                var result = System.Windows.MessageBox.Show(
                    "⚠️ ADVERTENCIA: Esto reemplazará TODOS los datos actuales con los del backup.\n\n¿Estás seguro que deseas continuar?",
                    "Confirmar Restauración",
                    System.Windows.MessageBoxButton.YesNo,
                    System.Windows.MessageBoxImage.Warning);

                if (result == System.Windows.MessageBoxResult.Yes)
                {
                    await _backupService.RestaurarBackupAsync(dialog.FileName);
                    SnackbarMessageQueue.Enqueue("✅ Backup restaurado. Reinicia la aplicación para ver los cambios.");
                }
            }
        }
        catch (Exception ex)
        {
            SnackbarMessageQueue.Enqueue($"❌ Error restaurando backup: {ex.Message}");
        }
    }

    /// <summary>
    /// Muestra un mensaje toast en el snackbar
    /// </summary>
    public void ShowToast(string message)
    {
        SnackbarMessageQueue.Enqueue(message);
    }
}
