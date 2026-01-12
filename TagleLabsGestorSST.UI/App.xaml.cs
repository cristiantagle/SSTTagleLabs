using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Windows;
using System.Linq;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Data.Entities;
using TagleLabsGestorSST.Services;
using TagleLabsGestorSST.UI.ViewModels;
using TagleLabsGestorSST.UI.Views;
using System.IO;

namespace TagleLabsGestorSST.UI;

public partial class App : Application
{
    public static IHost? AppHost { get; private set; }

    public App()
    {
        try 
        {
            // Manejo global de excepciones UI
            this.DispatcherUnhandledException += App_DispatcherUnhandledException;

            AppHost = Host.CreateDefaultBuilder()
                .ConfigureServices((hostContext, services) =>
                {
                    // Base de datos SQLite - usar AppData\Local para evitar problemas de permisos en Program Files
                    var appDataFolder = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "TagleLabsGestorSST"
                    );
                    // Crear la carpeta si no existe
                    if (!Directory.Exists(appDataFolder))
                    {
                        Directory.CreateDirectory(appDataFolder);
                    }
                    var dbPath = Path.Combine(appDataFolder, "TagleLabs.db");
                    services.AddDbContext<TagleLabsContext>(options =>
                    {
                        options.UseSqlite($"Data Source={dbPath}");
                    }, ServiceLifetime.Transient, ServiceLifetime.Transient);
                    // Alternativa: registrar factory (usa la misma ruta)
                    services.AddSingleton<Func<TagleLabsContext>>(sp => 
                    {
                        var optionsBuilder = new DbContextOptionsBuilder<TagleLabsContext>();
                        optionsBuilder.UseSqlite($"Data Source={dbPath}");
                        return () => new TagleLabsContext(optionsBuilder.Options);
                    });

                    // Servicios de Negocio
                    services.AddTransient<IEmpresaService, EmpresaService>();
                    services.AddTransient<ITrabajadorService, TrabajadorService>();
                    services.AddTransient<IMatrizRiesgoService, MatrizRiesgoService>();
                    services.AddTransient<IObligacionesService, ObligacionesService>(); 
                    services.AddTransient<ISiniestrosService, SiniestrosService>();
                    services.AddTransient<DocumentoService>();
                    services.AddTransient<ImportacionService>();
                    services.AddTransient<IAuditoriaService, AuditoriaService>();
                    services.AddSingleton<ISessionService, SessionService>();
                    services.AddSingleton<INotificationService, TagleLabsGestorSST.UI.Services.WpfNotificationService>();
                    services.AddSingleton<ILocalAiService, LocalAiService>();
                    services.AddTransient<IKnowledgeIngestionService, KnowledgeIngestionService>();
                    services.AddTransient<IConfiguracionService, ConfiguracionService>();
                    services.AddTransient<IVacacionesService, VacacionesService>();
                    services.AddSingleton<IConversorPdfService, ConversorPdfService>();
                    services.AddSingleton<IConversorPdfWordInteropService, ConversorPdfWordInteropService>();
                    services.AddSingleton<IAsistenciaProSyncService, AsistenciaProSyncService>();
                    services.AddTransient<ILugarTrabajoService, LugarTrabajoService>(); // Sistema docs por lugar
                    
                    // Servicio de Backup
                    services.AddSingleton(sp => new BackupService(dbPath));

                    // ViewModels
                    services.AddSingleton<TagleLabsGestorSST.UI.Services.ThemeService>();
                    services.AddSingleton<MainViewModel>();
                    services.AddTransient<LoginViewModel>();
                    services.AddTransient<DashboardViewModel>();
                    services.AddTransient<EmpresasViewModel>();
                    services.AddTransient<TrabajadoresViewModel>();
                    services.AddTransient<MatrizRiesgosViewModel>();
                    services.AddTransient<ObligacionesViewModel>();
                    services.AddTransient<SiniestrosViewModel>();
                    services.AddTransient<DocumentosViewModel>();
                    services.AddTransient<ProcedimientosViewModel>();
                    services.AddTransient<ImportacionViewModel>();
                    services.AddTransient<AuditoriaViewModel>();
                    services.AddTransient<ActualizacionDocumentosViewModel>();
                    services.AddTransient<BusquedaSemanticaViewModel>(); // TIER 3: Búsqueda semántica
                    services.AddTransient<AsistenciaViewModel>(); // Módulo AsistenciaPro integrado
                    services.AddTransient<LugarTrabajoViewModel>(); // Sistema docs por lugar de trabajo
                    
                    // Ventana Principal
                    services.AddSingleton<MainWindow>();
                })
                .Build();
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error CRÍTICO al iniciar (Constructor): {ex.Message}\n{ex.InnerException?.Message}", "Error Fatal", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    private void App_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
    {
        MessageBox.Show($"Ocurrió un error inesperado:\n\n{e.Exception.Message}\n\n{e.Exception.StackTrace}", 
                        "Error de Aplicación", 
                        MessageBoxButton.OK, 
                        MessageBoxImage.Error);
        e.Handled = true; // Evita que la aplicación se cierre
    }

    protected override async void OnStartup(StartupEventArgs e)
    {
        try
        {
            await AppHost!.StartAsync();

            // Asegurar que la BD esté creada
            using (var scope = AppHost.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<TagleLabsContext>();
                
                // BACKUP AUTOMÁTICO: Copiar BD antes de cualquier migración
                try
                {
                    var dbPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "TagleLabsGestorSST",
                        "tagleLabsGestor.db"
                    );
                    
                    if (File.Exists(dbPath))
                    {
                        var backupPath = dbPath + ".bak";
                        File.Copy(dbPath, backupPath, overwrite: true);
                        System.Diagnostics.Debug.WriteLine($"[BACKUP] BD respaldada en: {backupPath}");
                    }
                }
                catch (Exception backupEx)
                {
                    System.Diagnostics.Debug.WriteLine($"[BACKUP] Error al crear backup: {backupEx.Message}");
                    // No fallar silenciosamente, continuar con la app
                }
                
                // Usar Migraciones en lugar de EnsureCreated para permitir evolución del esquema
                await db.Database.MigrateAsync();
                
                // VERIFICAR: Tabla LogsAuditoria
                System.Diagnostics.Debug.WriteLine($"[APP STARTUP] Verificando tabla LogsAuditoria...");
                var countLogs = await db.LogsAuditoria.CountAsync();
                System.Diagnostics.Debug.WriteLine($"[APP STARTUP] LogsAuditoria tiene {countLogs} registros");
                
                // MIGRACIÓN: Crear centros de trabajo para empresas que no los tienen
                var empresasSinCentros = await db.Empresas
                    .Where(e => !db.CentrosTrabajo.Any(c => c.EmpresaId == e.Id))
                    .ToListAsync();
                
                if (empresasSinCentros.Any())
                {
                    foreach (var empresa in empresasSinCentros)
                    {
                        var centroDefault = new CentroTrabajo
                        {
                            Nombre = "Sede Principal",
                            EmpresaId = empresa.Id,
                            Direccion = empresa.Direccion ?? "Sin dirección especificada",
                            Region = "Región Metropolitana",
                            Ciudad = "Santiago"
                        };
                        db.CentrosTrabajo.Add(centroDefault);
                    }
                    await db.SaveChangesAsync();
                }

                // INICIALIZAR PLANTILLAS DE DOCUMENTOS
                var docService = scope.ServiceProvider.GetRequiredService<DocumentoService>();
                await docService.InicializarPlantillasBase();
            }

            var mainWindow = AppHost.Services.GetRequiredService<MainWindow>();
            mainWindow.Show();

            base.OnStartup(e);
        }
        catch (Exception ex)
        {
            MessageBox.Show($"Error fatal al iniciar la aplicación:\n\n{ex.Message}\n\n{ex.StackTrace}", "Error de Inicio", MessageBoxButton.OK, MessageBoxImage.Error);
            Shutdown();
        }
    }

    protected override async void OnExit(ExitEventArgs e)
    {
        await AppHost!.StopAsync();
        base.OnExit(e);
    }
}
