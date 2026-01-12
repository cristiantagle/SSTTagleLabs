using System;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;
using TagleLabsGestorSST.Data.Entities;

namespace TagleLabsGestorSST.Data;

public class TagleLabsContext : DbContext
{
    public DbSet<SolicitudVacaciones> SolicitudesVacaciones { get; set; }
    public DbSet<MovimientoVacaciones> MovimientosVacaciones { get; set; }
    public DbSet<DocumentoTrabajador> DocumentosTrabajadores { get; set; }

    public TagleLabsContext(DbContextOptions<TagleLabsContext> options) : base(options)
    {
    }

    public DbSet<Configuracion> Configuraciones { get; set; }
    public DbSet<Empresa> Empresas => Set<Empresa>();
    public DbSet<CentroTrabajo> CentrosTrabajo => Set<CentroTrabajo>();
    public DbSet<Trabajador> Trabajadores => Set<Trabajador>();
    public DbSet<Contratista> Contratistas => Set<Contratista>();
    public DbSet<RolInterno> Roles => Set<RolInterno>();
    public DbSet<RolTrabajador> RolesTrabajador => Set<RolTrabajador>();
    public DbSet<Rubro> Rubros => Set<Rubro>();
    public DbSet<Actividad> Actividades => Set<Actividad>();
    public DbSet<Tarea> Tareas => Set<Tarea>();
    public DbSet<Peligro> Peligros => Set<Peligro>();
    public DbSet<Control> Controles => Set<Control>();
    public DbSet<TareaPeligroControl> TareaPeligroControles => Set<TareaPeligroControl>();
    public DbSet<MatrizRiesgoEmpresa> MatrizRiesgosEmpresa => Set<MatrizRiesgoEmpresa>();
    public DbSet<ArticuloDs44> ArticulosDs44 => Set<ArticuloDs44>();
    public DbSet<ObligacionDs44> ObligacionesDs44 => Set<ObligacionDs44>();
    public DbSet<ObligacionEmpresa> ObligacionesEmpresa => Set<ObligacionEmpresa>();
    public DbSet<PlantillaDocumento> Plantillas => Set<PlantillaDocumento>();
    public DbSet<PlantillaCampo> PlantillaCampos => Set<PlantillaCampo>();
    public DbSet<DocumentoGenerado> DocumentosGenerados => Set<DocumentoGenerado>();
    public DbSet<VersionDocumento> VersionesDocumento => Set<VersionDocumento>();
    public DbSet<Incidente> Incidentes => Set<Incidente>();
    public DbSet<RegistroSiniestro> RegistrosSiniestro => Set<RegistroSiniestro>();
    public DbSet<IndicadorSst> Indicadores => Set<IndicadorSst>();
    public DbSet<HistorialCambio> HistorialCambios => Set<HistorialCambio>();
    public DbSet<AlertaConfiguracion> Alertas => Set<AlertaConfiguracion>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<KnowledgeItem> KnowledgeItems => Set<KnowledgeItem>();
    public DbSet<KnowledgeChunk> KnowledgeChunks => Set<KnowledgeChunk>();
    
    // Sistema de Documentos por Lugar de Trabajo
    public DbSet<LugarTrabajo> LugaresTrabajo => Set<LugarTrabajo>();
    public DbSet<PlantillaLugarTrabajo> PlantillasLugarTrabajo => Set<PlantillaLugarTrabajo>();
    public DbSet<AsignacionTrabajador> AsignacionesTrabajador => Set<AsignacionTrabajador>();
    public DbSet<DocumentoRequerido> DocumentosRequeridos => Set<DocumentoRequerido>();
    public DbSet<CharlaSST> CharlasSST => Set<CharlaSST>();
    public DbSet<AsistenciaCharla> AsistenciasCharla => Set<AsistenciaCharla>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Empresa>()
            .HasIndex(e => e.Rut)
            .IsUnique();

        // Relación 1:N con Rubro (legacy - mantener por compatibilidad)
        modelBuilder.Entity<Empresa>()
            .HasOne(e => e.Rubro)
            .WithMany()
            .HasForeignKey(e => e.RubroId)
            .OnDelete(DeleteBehavior.SetNull);

        // Relación N:N con múltiples Rubros para MIPER
        modelBuilder.Entity<Empresa>()
            .HasMany(e => e.Rubros)
            .WithMany(r => r.Empresas)
            .UsingEntity<Dictionary<string, object>>(
                "EmpresaRubro",
                j => j.HasOne<Rubro>().WithMany().HasForeignKey("RubroId"),
                j => j.HasOne<Empresa>().WithMany().HasForeignKey("EmpresaId")
            );

        modelBuilder.Entity<Trabajador>()
            .HasIndex(t => t.Rut)
            .IsUnique();

        modelBuilder.Entity<Trabajador>()
            .HasMany(t => t.TareasAsignadas)
            .WithMany(t => t.Trabajadores)
            .UsingEntity(j => j.ToTable("TrabajadorTarea"));

        modelBuilder.Entity<Tarea>()
            .HasMany(t => t.Equipos)
            .WithMany(e => e.TareasRelacionadas)
            .UsingEntity(j => j.ToTable("TareaEquipo"));

        modelBuilder.Entity<TareaPeligroControl>()
            .HasIndex(r => new { r.TareaId, r.PeligroId, r.ControlId })
            .IsUnique();

        modelBuilder.Entity<MatrizRiesgoEmpresa>()
            .HasOne(m => m.CentroTrabajo)
            .WithMany(c => c.MatrizRiesgos)
            .HasForeignKey(m => m.CentroTrabajoId)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<MatrizRiesgoEmpresa>()
            .HasOne(m => m.Tarea)
            .WithMany()
            .HasForeignKey(m => m.TareaId)
            .OnDelete(DeleteBehavior.Restrict); // No borrar tarea si se usa en matrices

        // =================================================================================
        // CONFIGURACIÓN SISTEMA DE DOCUMENTOS POR LUGAR DE TRABAJO
        // =================================================================================
        
        // LugarTrabajo -> PlantillaLugarTrabajo (1:N)
        modelBuilder.Entity<PlantillaLugarTrabajo>()
            .HasOne(p => p.LugarTrabajo)
            .WithMany(l => l.Plantillas)
            .HasForeignKey(p => p.LugarTrabajoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // LugarTrabajo -> AsignacionTrabajador (1:N)
        modelBuilder.Entity<AsignacionTrabajador>()
            .HasOne(a => a.LugarTrabajo)
            .WithMany(l => l.Asignaciones)
            .HasForeignKey(a => a.LugarTrabajoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Trabajador -> AsignacionTrabajador (1:N)
        modelBuilder.Entity<AsignacionTrabajador>()
            .HasOne(a => a.Trabajador)
            .WithMany()
            .HasForeignKey(a => a.TrabajadorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índice único para evitar duplicados de asignación
        modelBuilder.Entity<AsignacionTrabajador>()
            .HasIndex(a => new { a.TrabajadorId, a.LugarTrabajoId })
            .IsUnique();
        
        // PlantillaLugarTrabajo -> DocumentoRequerido (1:N)
        modelBuilder.Entity<DocumentoRequerido>()
            .HasOne(d => d.PlantillaLugarTrabajo)
            .WithMany(p => p.DocumentosGenerados)
            .HasForeignKey(d => d.PlantillaLugarTrabajoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Trabajador -> DocumentoRequerido (1:N)
        modelBuilder.Entity<DocumentoRequerido>()
            .HasOne(d => d.Trabajador)
            .WithMany()
            .HasForeignKey(d => d.TrabajadorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // LugarTrabajo -> CharlaSST (1:N)
        modelBuilder.Entity<CharlaSST>()
            .HasOne(c => c.LugarTrabajo)
            .WithMany(l => l.Charlas)
            .HasForeignKey(c => c.LugarTrabajoId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // CharlaSST -> AsistenciaCharla (1:N)
        modelBuilder.Entity<AsistenciaCharla>()
            .HasOne(a => a.Charla)
            .WithMany(c => c.Asistencias)
            .HasForeignKey(a => a.CharlaId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Trabajador -> AsistenciaCharla (1:N)
        modelBuilder.Entity<AsistenciaCharla>()
            .HasOne(a => a.Trabajador)
            .WithMany()
            .HasForeignKey(a => a.TrabajadorId)
            .OnDelete(DeleteBehavior.Cascade);
        
        // Índice único para evitar duplicados de asistencia
        modelBuilder.Entity<AsistenciaCharla>()
            .HasIndex(a => new { a.CharlaId, a.TrabajadorId })
            .IsUnique();

        // =================================================================================
        // SEED DATA MASIVO - INVESTIGACIÓN SST CHILE (SUSESO, MINSAL, ACHS)
        // =================================================================================

        // 4. PELIGROS (Codificación SUSESO / ISO 45001)
        var peligros = new List<Peligro>
        {
            // Seguridad
            new Peligro { Id = 1, Categoria = "Seguridad", Descripcion = "Caída a distinto nivel (> 1.8m)" },
            new Peligro { Id = 2, Categoria = "Seguridad", Descripcion = "Caída al mismo nivel (tropiezos, resbalones)" },
            new Peligro { Id = 3, Categoria = "Seguridad", Descripcion = "Atrapamiento por partes móviles" },
            new Peligro { Id = 4, Categoria = "Seguridad", Descripcion = "Golpeado por objeto en movimiento/caída" },
            new Peligro { Id = 5, Categoria = "Seguridad", Descripcion = "Contacto con energía eléctrica (Directo/Indirecto)" },
            new Peligro { Id = 6, Categoria = "Seguridad", Descripcion = "Cortes con herramientas cortopunzantes" },
            new Peligro { Id = 7, Categoria = "Seguridad", Descripcion = "Proyección de partículas incandescentes" },
            new Peligro { Id = 8, Categoria = "Seguridad", Descripcion = "Derrumbe de paredes/zanjas" },
            new Peligro { Id = 9, Categoria = "Seguridad", Descripcion = "Volcamiento de maquinaria" },

            // Higiene (Físicos, Químicos, Biológicos)
            new Peligro { Id = 10, Categoria = "Físico", Descripcion = "Exposición a Ruido (PREXOR)" },
            new Peligro { Id = 11, Categoria = "Físico", Descripcion = "Exposición a Sílice Libre (PLANESI)" },
            new Peligro { Id = 12, Categoria = "Físico", Descripcion = "Radiación UV Solar" },
            new Peligro { Id = 13, Categoria = "Físico", Descripcion = "Vibraciones (Mano-brazo / Cuerpo entero)" },
            new Peligro { Id = 14, Categoria = "Químico", Descripcion = "Humos metálicos de soldadura" },
            new Peligro { Id = 15, Categoria = "Químico", Descripcion = "Exposición a Plaguicidas/Fitosanitarios" },
            new Peligro { Id = 16, Categoria = "Químico", Descripcion = "Exposición a Solventes/Combustibles" },
            new Peligro { Id = 17, Categoria = "Biológico", Descripcion = "Exposición a virus/bacterias (Hanta, Covid, Tétanos)" },

            // Ergonómicos
            new Peligro { Id = 18, Categoria = "Ergonómico", Descripcion = "Manejo Manual de Cargas (MMC)" },
            new Peligro { Id = 19, Categoria = "Ergonómico", Descripcion = "Movimientos Repetitivos (TMERT)" },
            new Peligro { Id = 20, Categoria = "Ergonómico", Descripcion = "Posturas forzadas o estáticas" },

            // Psicosociales
            new Peligro { Id = 21, Categoria = "Psicosocial", Descripcion = "Alta exigencia psicológica (Ritmo, Cantidad)" },
            new Peligro { Id = 22, Categoria = "Psicosocial", Descripcion = "Acoso laboral o sexual (Ley Karin)" },
            new Peligro { Id = 23, Categoria = "Psicosocial", Descripcion = "Doble presencia (Trabajo/Familia)" }
        };
        modelBuilder.Entity<Peligro>().HasData(peligros);

        // 5. CONTROLES (Jerarquía)
        var controles = new List<Control>
        {
            // Ingeniería
            new Control { Id = 1, Tipo = "Ingeniería", Descripcion = "Sistemas de entibación (Cajones, Tablestacas)" },
            new Control { Id = 2, Tipo = "Ingeniería", Descripcion = "Sistemas de extracción localizada de humos/polvo" },
            new Control { Id = 3, Tipo = "Ingeniería", Descripcion = "Protecciones y guardas en partes móviles" },
            new Control { Id = 4, Tipo = "Ingeniería", Descripcion = "Barreras físicas y delimitación de zonas" },
            new Control { Id = 5, Tipo = "Ingeniería", Descripcion = "Ayudas mecánicas para carga (Tecles, Grúas)" },

            // Administrativos
            new Control { Id = 6, Tipo = "Administrativo", Descripcion = "Procedimiento de Trabajo Seguro (PTS)" },
            new Control { Id = 7, Tipo = "Administrativo", Descripcion = "Capacitación y Charla de 5 minutos" },
            new Control { Id = 8, Tipo = "Administrativo", Descripcion = "Señalización de seguridad" },
            new Control { Id = 9, Tipo = "Administrativo", Descripcion = "Permiso de Trabajo (Altura, Caliente, Espacio Confinado)" },
            new Control { Id = 10, Tipo = "Administrativo", Descripcion = "Rotación de puestos de trabajo" },
            new Control { Id = 11, Tipo = "Administrativo", Descripcion = "Bloqueo y Etiquetado (LOTO)" },
            new Control { Id = 12, Tipo = "Administrativo", Descripcion = "Programa de Vigilancia Médica (Protocolos MINSAL)" },

            // EPP
            new Control { Id = 13, Tipo = "EPP", Descripcion = "Casco de seguridad con barbiquejo" },
            new Control { Id = 14, Tipo = "EPP", Descripcion = "Lentes de seguridad (Claros/Oscuros)" },
            new Control { Id = 15, Tipo = "EPP", Descripcion = "Protección auditiva (Tapones/Fonos)" },
            new Control { Id = 16, Tipo = "EPP", Descripcion = "Respirador medio rostro con filtros (P100/Vapores)" },
            new Control { Id = 17, Tipo = "EPP", Descripcion = "Guantes de seguridad (Cabritilla, Hycron, Nitrilo)" },
            new Control { Id = 18, Tipo = "EPP", Descripcion = "Calzado de seguridad" },
            new Control { Id = 19, Tipo = "EPP", Descripcion = "Arnés de cuerpo completo con doble cabo de vida" },
            new Control { Id = 20, Tipo = "EPP", Descripcion = "Ropa con protección UV / Legionario" },
            new Control { Id = 21, Tipo = "EPP", Descripcion = "Traje Tyvek para químicos" }
        };
        modelBuilder.Entity<Control>().HasData(controles);

        // 1. RUBROS, ACTIVIDADES Y TAREAS (Desde JSON)
        var seedDataPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Data", "SeedData_MIPER.json");
        // Fallback path for development
        if (!File.Exists(seedDataPath))
        {
             seedDataPath = @"c:\SST TagleLabs\TagleLabsGestorSST.Data\SeedData_MIPER.json";
        }

        if (File.Exists(seedDataPath))
        {
            try 
            {
                var json = File.ReadAllText(seedDataPath);
                var rubrosJson = System.Text.Json.JsonSerializer.Deserialize<List<RubroSeedDto>>(json);

                if (rubrosJson != null)
                {
                    var rubrosList = new List<Rubro>();
                    var actividadesList = new List<Actividad>();
                    var tareasList = new List<Tarea>();
                    var tareaPeligroList = new List<TareaPeligroControl>();

                    int rubroId = 1;
                    int actId = 1;
                    int tareaId = 1;
                    int relacionId = 1;

                    foreach (var r in rubrosJson)
                    {
                        rubrosList.Add(new Rubro { Id = rubroId, Codigo = r.Codigo, Nombre = r.Nombre, Descripcion = r.Descripcion });
                        
                        if (r.Actividades != null)
                        {
                            foreach (var a in r.Actividades)
                            {
                                actividadesList.Add(new Actividad { Id = actId, RubroId = rubroId, Nombre = a.Nombre, Descripcion = a.Descripcion });
                                
                                if (a.Tareas != null)
                                {
                                    foreach (var t in a.Tareas)
                                    {
                                        tareasList.Add(new Tarea { Id = tareaId, ActividadId = actId, Nombre = t.Nombre, Descripcion = t.Descripcion });
                                        
                                        // Procesar Riesgos Típicos
                                        if (t.RiesgosTipicos != null)
                                        {
                                            foreach (var riesgoNombre in t.RiesgosTipicos)
                                            {
                                                // Buscar ID del peligro
                                                var peligro = peligros.FirstOrDefault(p => p.Descripcion.Contains(riesgoNombre, StringComparison.OrdinalIgnoreCase));
                                                if (peligro != null)
                                                {
                                                    // Asignar un control por defecto (e.g., Administrativo - Charla)
                                                    var control = controles.FirstOrDefault(c => c.Id == 7); // Charla 5 min default

                                                    tareaPeligroList.Add(new TareaPeligroControl
                                                    {
                                                        Id = relacionId,
                                                        TareaId = tareaId,
                                                        PeligroId = peligro.Id,
                                                        ControlId = control?.Id ?? 1,
                                                        Probabilidad = 2,
                                                        Consecuencia = 2,
                                                        NivelRiesgo = 4,
                                                        Aplicable = true
                                                    });
                                                    relacionId++;
                                                }
                                            }
                                        }
                                        tareaId++;
                                    }
                                }
                                actId++;
                            }
                        }
                        rubroId++;
                    }

                    modelBuilder.Entity<Rubro>().HasData(rubrosList);
                    modelBuilder.Entity<Actividad>().HasData(actividadesList);
                    modelBuilder.Entity<Tarea>().HasData(tareasList);
                    modelBuilder.Entity<TareaPeligroControl>().HasData(tareaPeligroList);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error seeding JSON: {ex.Message}");
            }
        }


        // 4. PELIGROS (Codificación SUSESO / ISO 45001)



        // NOTA: Se comentó el seed data de empresas de prueba para distribución.
        // Los usuarios deben crear sus propias empresas.
        // El seed data de Peligros, Controles, Obligaciones y Plantillas se mantiene
        // ya que son datos de referencia necesarios para el funcionamiento del sistema.

        // 7.5 ARTICULOS DS44 (Dependencia de Obligaciones)
        var articulos = new List<ArticuloDs44>
        {
            new ArticuloDs44 { Id = 1, Numero = "Art. 1", Titulo = "Disposiciones Generales", Contenido = "El empleador deberá garantizar condiciones de trabajo seguras y saludables." },
            new ArticuloDs44 { Id = 7, Numero = "Art. 7", Titulo = "Salud Ocupacional", Contenido = "Implementación de vigilancia de la salud y protocolos MINSAL." },
            new ArticuloDs44 { Id = 8, Numero = "Art. 8", Titulo = "Gestión de Riesgos", Contenido = "Identificación de peligros y evaluación de riesgos (MIPER)." },
            new ArticuloDs44 { Id = 18, Numero = "Art. 18", Titulo = "Participación", Contenido = "Constitución y funcionamiento de Comités Paritarios." },
            new ArticuloDs44 { Id = 25, Numero = "Art. 25", Titulo = "Emergencias", Contenido = "Planes de emergencia y evacuación ante siniestros." }
        };
        modelBuilder.Entity<ArticuloDs44>().HasData(articulos);

        // 8. OBLIGACIONES NORMATIVAS (DS44, DS594, DS40, Ley Karin)
        var obligaciones = new List<ObligacionDs44>
        {
            // DS 44 (Gestión SST)
            new ObligacionDs44 { Id = 1, Codigo = "DS44-ART1", Descripcion = "Implementar Sistema de Gestión de Seguridad y Salud en el Trabajo (SGSST)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 2, Codigo = "DS44-MIPER", Descripcion = "Matriz de Identificación de Peligros y Evaluación de Riesgos (MIPER) actualizada anualmente", ArticuloDs44Id = 8, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 3, Codigo = "DS44-CPHS", Descripcion = "Constitución y funcionamiento de Comité Paritario de Higiene y Seguridad (CPHS)", ArticuloDs44Id = 18, AplicaMinTrab = 25 },
            new ObligacionDs44 { Id = 4, Codigo = "DS44-EMERG", Descripcion = "Plan de Emergencia y Evacuación con simulacros periódicos", ArticuloDs44Id = 25, AplicaMinTrab = 1 },

            // DS 594 (Condiciones Sanitarias y Ambientales)
            new ObligacionDs44 { Id = 5, Codigo = "DS594-AGUA", Descripcion = "Provisión de agua potable fresca y suficiente (Art. 12)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 6, Codigo = "DS594-BANOS", Descripcion = "Servicios higiénicos independientes y separados por sexo (Art. 21)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 7, Codigo = "DS594-LOCKER", Descripcion = "Guardarropas individuales para cambio de ropa (Art. 27)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 8, Codigo = "DS594-COMEDOR", Descripcion = "Comedor habilitado separado de áreas de trabajo (Art. 28)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 9, Codigo = "DS594-EXTINTOR", Descripcion = "Extintores de incendio adecuados y mantención vigente (Art. 45)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },

            // DS 44 - Art. 15 (Información de Riesgos Laborales)
            new ObligacionDs44 { Id = 10, Codigo = "DS44-IRL", Descripcion = "Información de Riesgos Laborales (IRL) específica para cada puesto de trabajo (Art. 15 DS44)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 11, Codigo = "DS44-RIOHS", Descripcion = "Reglamento Interno de Orden, Higiene y Seguridad (RIOHS) - Art. 153 Código del Trabajo", ArticuloDs44Id = 1, AplicaMinTrab = 10 },
            new ObligacionDs44 { Id = 12, Codigo = "DS44-DEPTO", Descripcion = "Departamento de Prevención de Riesgos liderado por Experto (DS44)", ArticuloDs44Id = 1, AplicaMinTrab = 100 },

            // Ley 21.643 (Ley Karin)
            new ObligacionDs44 { Id = 13, Codigo = "KARIN-PROTO", Descripcion = "Protocolo de Prevención del Acoso Sexual, Laboral y Violencia en el Trabajo", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 14, Codigo = "KARIN-DIFUSION", Descripcion = "Difusión semestral de canales de denuncia y medidas de resguardo", ArticuloDs44Id = 1, AplicaMinTrab = 1 },

            // Protocolos MINSAL
            new ObligacionDs44 { Id = 15, Codigo = "PROTO-PREXOR", Descripcion = "Implementación Protocolo de Exposición a Ruido (PREXOR)", ArticuloDs44Id = 7, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 16, Codigo = "PROTO-PLANESI", Descripcion = "Implementación Plan Nacional de Erradicación de Silicosis (PLANESI)", ArticuloDs44Id = 7, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 17, Codigo = "PROTO-TMERT", Descripcion = "Evaluación de Trastornos Musculoesqueléticos (TMERT-EESS)", ArticuloDs44Id = 7, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 18, Codigo = "PROTO-UV", Descripcion = "Programa de Protección contra Radiación UV de origen solar", ArticuloDs44Id = 7, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 19, Codigo = "PROTO-PSICO", Descripcion = "Evaluación de Riesgos Psicosociales (Cuestionario CEAL-SM / ISTAS21)", ArticuloDs44Id = 7, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 20, Codigo = "PROTO-MMC", Descripcion = "Gestión del riesgo por Manejo Manual de Cargas (Ley 20.949)", ArticuloDs44Id = 7, AplicaMinTrab = 1 },

            // DS 54 (Comités Paritarios)
            new ObligacionDs44 { Id = 21, Codigo = "DS54-CONST", Descripcion = "Constitución de Comité Paritario (CPHS) con representantes titulares y suplentes", ArticuloDs44Id = 18, AplicaMinTrab = 25 },
            new ObligacionDs44 { Id = 22, Codigo = "DS54-PROG", Descripcion = "Programa de Trabajo del CPHS y cronograma de reuniones mensuales", ArticuloDs44Id = 18, AplicaMinTrab = 25 },
            new ObligacionDs44 { Id = 23, Codigo = "DS54-INVEST", Descripcion = "Investigación de todos los accidentes por parte del CPHS", ArticuloDs44Id = 18, AplicaMinTrab = 25 },

            // Ley 20.123 (Subcontratación)
            new ObligacionDs44 { Id = 24, Codigo = "LEY20123-REGL", Descripcion = "Reglamento Especial para Empresas Contratistas y Subcontratistas", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 25, Codigo = "LEY20123-REG", Descripcion = "Registro actualizado de antecedentes de trabajadores contratistas", ArticuloDs44Id = 1, AplicaMinTrab = 1 },

            // DS 43 (Sustancias Peligrosas)
            new ObligacionDs44 { Id = 26, Codigo = "DS43-AUTORIZ", Descripcion = "Autorización Sanitaria para almacenamiento de Sustancias Peligrosas", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 27, Codigo = "DS43-MATRIZ", Descripcion = "Matriz de Incompatibilidad Química y Hojas de Datos de Seguridad (HDS)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },

            // Circular 3335 (Accidentes Graves)
            new ObligacionDs44 { Id = 28, Codigo = "CIRC3335-NOTIF", Descripcion = "Procedimiento de Notificación Inmediata de Accidentes Graves y Fatales", ArticuloDs44Id = 25, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 29, Codigo = "CIRC3335-SUSP", Descripcion = "Procedimiento de Auto-suspensión de faenas ante riesgo inminente", ArticuloDs44Id = 25, AplicaMinTrab = 1 },

            // DS 18 (EPP)
            new ObligacionDs44 { Id = 30, Codigo = "DS18-CERT", Descripcion = "Uso exclusivo de Elementos de Protección Personal (EPP) certificados (ISP)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },

            // NUEVO DS 44 (2023) - VIGENCIA 2025 (Instrumentos de Gestión)
            new ObligacionDs44 { Id = 31, Codigo = "DS44-POLITICA", Descripcion = "Política de Seguridad y Salud en el Trabajo firmada y difundida", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 32, Codigo = "DS44-PROG-PREV", Descripcion = "Programa de Trabajo en Prevención de Riesgos (Anual)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 33, Codigo = "DS44-MAPA", Descripcion = "Mapa de Riesgos del centro de trabajo (Visible)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 34, Codigo = "DS44-AUTOEVAL", Descripcion = "Autoevaluación Anual del Sistema de Gestión (SG-SST)", ArticuloDs44Id = 1, AplicaMinTrab = 1 },
            new ObligacionDs44 { Id = 35, Codigo = "DS44-REG-ACC", Descripcion = "Registro histórico de Accidentes y Enfermedades Profesionales", ArticuloDs44Id = 1, AplicaMinTrab = 1 }
        };
        modelBuilder.Entity<ObligacionDs44>().HasData(obligaciones);

        // 9. PLANTILLAS DOCUMENTOS (Investigación Final DS 44 2023)
        var plantillas = new[]
        {
            new PlantillaDocumento { Id = 1, Codigo = "MIPER-STD", Nombre = "Matriz IPER Estándar", Tipo = "Excel", RutaBase = "Plantillas/Matriz_IPER_V2.xlsx", Descripcion = "Matriz de Riesgos con evaluación PxC" },
            new PlantillaDocumento { Id = 2, Codigo = "RIOHS-KARIN", Nombre = "Reglamento Interno (Adaptado Ley Karin)", Tipo = "Word", RutaBase = "Plantillas/RIOHS_2025_Karin.docx", Descripcion = "Modelo RIOHS actualizado a Ley 21.643" },
            new PlantillaDocumento { Id = 3, Codigo = "IRL-GEN", Nombre = "Formato IRL Genérico (DS44)", Tipo = "Word", RutaBase = "Plantillas/IRL_Base.docx", Descripcion = "Información de Riesgos Laborales - Art. 15 DS44" },
            new PlantillaDocumento { Id = 4, Codigo = "CHECK-DS594", Nombre = "Lista Chequeo DS 594", Tipo = "Excel", RutaBase = "Plantillas/Checklist_DS594.xlsx", Descripcion = "Auditoría condiciones sanitarias y ambientales" },
            new PlantillaDocumento { Id = 5, Codigo = "PROTO-ACOS", Nombre = "Protocolo Acoso y Violencia (Ley Karin)", Tipo = "Word", RutaBase = "Plantillas/Protocolo_Karin.docx", Descripcion = "Protocolo obligatorio de prevención" },
            new PlantillaDocumento { Id = 6, Codigo = "ACTA-CPHS", Nombre = "Acta Constitución CPHS", Tipo = "Word", RutaBase = "Plantillas/Acta_CPHS.docx", Descripcion = "Para empresas con más de 25 trabajadores" },
            new PlantillaDocumento { Id = 7, Codigo = "REGL-CONTRAT", Nombre = "Reglamento Especial Contratistas", Tipo = "Word", RutaBase = "Plantillas/Reglamento_Contratistas.docx", Descripcion = "Cumplimiento Ley 20.123" },
            new PlantillaDocumento { Id = 8, Codigo = "PROC-ACC-GRV", Nombre = "Procedimiento Accidentes Graves", Tipo = "Word", RutaBase = "Plantillas/Proc_Accidentes_Graves.docx", Descripcion = "Circular 3335 SUSESO" },
            new PlantillaDocumento { Id = 13, Codigo = "PTS-STD", Nombre = "Procedimiento Trabajo Seguro (PTS)", Tipo = "Word", RutaBase = "Plantillas/Procedimiento_PTS_Base.docx", Descripcion = "Plantilla base para PTS generada por IA" },
            
            // Nuevas Plantillas DS 44 (2023)
            new PlantillaDocumento { Id = 9, Codigo = "POLITICA-SST", Nombre = "Política de Seguridad y Salud", Tipo = "Word", RutaBase = "Plantillas/Politica_SST.docx", Descripcion = "Compromiso gerencial DS 44" },
            new PlantillaDocumento { Id = 10, Codigo = "PROG-PREV", Nombre = "Programa de Prevención Anual", Tipo = "Excel", RutaBase = "Plantillas/Programa_Prevencion.xlsx", Descripcion = "Planificación anual DS 44" },
            new PlantillaDocumento { Id = 11, Codigo = "MAPA-RIESGOS", Nombre = "Mapa de Riesgos (Guía)", Tipo = "Word", RutaBase = "Plantillas/Guia_Mapa_Riesgos.docx", Descripcion = "Guía para elaborar mapa visual" },
            new PlantillaDocumento { Id = 12, Codigo = "AUTO-SGSST", Nombre = "Autoevaluación SG-SST", Tipo = "Excel", RutaBase = "Plantillas/Autoevaluacion_SGSST.xlsx", Descripcion = "Auditoría interna anual DS 44" }
        };
        modelBuilder.Entity<PlantillaDocumento>().HasData(plantillas);

        // 10. LOGS AUDITORIA
        var logsAuditoria = new[]
        {
            new LogAuditoria { Id = 1, Fecha = DateTime.Now, Accion = "Inicializar", Entidad = "Sistema", EntidadId = "0", Detalle = "Base de datos poblada con investigación FINAL DS 44 (2023) y normativa complementaria", Usuario = "Sistema" }
        };
        modelBuilder.Entity<LogAuditoria>().HasData(logsAuditoria);
    }
}

// DTOs para Seed Data
public class RubroSeedDto
{
    public string Codigo { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public List<ActividadSeedDto>? Actividades { get; set; }
}

public class ActividadSeedDto
{
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public List<TareaSeedDto>? Tareas { get; set; }
}

public class TareaSeedDto
{
    public string Nombre { get; set; } = "";
    public string Descripcion { get; set; } = "";
    public List<string>? RiesgosTipicos { get; set; }
}
