using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data;
using TagleLabsGestorSST.Services;
using System.IO;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// CORS para permitir peticiones desde Tauri/React
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowTauri", policy =>
    {
        policy.WithOrigins("http://localhost:5173", "tauri://localhost")
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

// Configuración de Base de Datos (Misma ruta que WPF)
var appDataFolder = Path.Combine(
    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
    "TagleLabsGestorSST"
);

if (!Directory.Exists(appDataFolder))
{
    Directory.CreateDirectory(appDataFolder);
}

var dbPath = Path.Combine(appDataFolder, "TagleLabs.db"); // Misma BD

builder.Services.AddDbContext<TagleLabsContext>(options =>
{
    options.UseSqlite($"Data Source={dbPath}");
}, ServiceLifetime.Transient, ServiceLifetime.Transient);

// Registrar Factory para DbContext
builder.Services.AddSingleton<Func<TagleLabsContext>>(sp => 
{
    var optionsBuilder = new DbContextOptionsBuilder<TagleLabsContext>();
    optionsBuilder.UseSqlite($"Data Source={dbPath}");
    return () => new TagleLabsContext(optionsBuilder.Options);
});

// --- Servicios de Negocio (Copiados de App.xaml.cs) ---
builder.Services.AddTransient<IEmpresaService, EmpresaService>();
builder.Services.AddTransient<ITrabajadorService, TrabajadorService>();
builder.Services.AddTransient<IMatrizRiesgoService, MatrizRiesgoService>();
builder.Services.AddTransient<IObligacionesService, ObligacionesService>(); 
builder.Services.AddTransient<ISiniestrosService, SiniestrosService>();
builder.Services.AddTransient<DocumentoService>();
builder.Services.AddTransient<ImportacionService>();
builder.Services.AddTransient<IAuditoriaService, AuditoriaService>();
builder.Services.AddSingleton<ISessionService, SessionService>();
// Nota: NotificationService original era WPF, aquí usaremos una implementación dummy o null por ahora
// builder.Services.AddSingleton<INotificationService, ...>(); 
builder.Services.AddSingleton<ILocalAiService, LocalAiService>();
builder.Services.AddTransient<IKnowledgeIngestionService, KnowledgeIngestionService>();
builder.Services.AddTransient<IConfiguracionService, ConfiguracionService>();
builder.Services.AddTransient<IVacacionesService, VacacionesService>();
builder.Services.AddSingleton<IConversorPdfService, ConversorPdfService>();
builder.Services.AddSingleton<IConversorPdfWordInteropService, ConversorPdfWordInteropService>();
builder.Services.AddSingleton<IAsistenciaProSyncService, AsistenciaProSyncService>();
builder.Services.AddTransient<ILugarTrabajoService, LugarTrabajoService>();
builder.Services.AddSingleton(sp => new BackupService(dbPath));

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseCors("AllowTauri");

app.UseAuthorization(); // Aunque no tengamos auth aún configurada

app.MapControllers();

// Endpoint de prueba rápido
app.MapGet("/api/status", () => 
{
    return Results.Ok(new { Status = "Online", System = "SST Tagle Labs API", Database = dbPath });
});

app.Run();
