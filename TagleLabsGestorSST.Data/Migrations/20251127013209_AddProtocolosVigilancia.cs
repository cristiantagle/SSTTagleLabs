using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddProtocolosVigilancia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ArticulosDs44",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Numero = table.Column<string>(type: "TEXT", nullable: false),
                    Titulo = table.Column<string>(type: "TEXT", nullable: false),
                    Contenido = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ArticulosDs44", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Controles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Controles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Empresas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Rut = table.Column<string>(type: "TEXT", nullable: false),
                    RazonSocial = table.Column<string>(type: "TEXT", nullable: false),
                    Giro = table.Column<string>(type: "TEXT", nullable: false),
                    RubroPrincipal = table.Column<string>(type: "TEXT", nullable: true),
                    NumeroTrabajadores = table.Column<int>(type: "INTEGER", nullable: false),
                    Mutual = table.Column<string>(type: "TEXT", nullable: true),
                    LogoPath = table.Column<string>(type: "TEXT", nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", nullable: true),
                    Telefono = table.Column<string>(type: "TEXT", nullable: true),
                    EmailContacto = table.Column<string>(type: "TEXT", nullable: true),
                    RepresentanteLegal = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Empresas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "LogsAuditoria",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Accion = table.Column<string>(type: "TEXT", nullable: false),
                    Entidad = table.Column<string>(type: "TEXT", nullable: false),
                    EntidadId = table.Column<string>(type: "TEXT", nullable: false),
                    Detalle = table.Column<string>(type: "TEXT", nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LogsAuditoria", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Peligros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Categoria = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Peligros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Plantillas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    RutaBase = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Plantillas", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Roles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Roles", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Rubros",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rubros", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ObligacionesDs44",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    ArticuloDs44Id = table.Column<int>(type: "INTEGER", nullable: false),
                    AplicaMinTrab = table.Column<int>(type: "INTEGER", nullable: true),
                    AplicaMaxTrab = table.Column<int>(type: "INTEGER", nullable: true),
                    RubroObjetivo = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObligacionesDs44", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObligacionesDs44_ArticulosDs44_ArticuloDs44Id",
                        column: x => x.ArticuloDs44Id,
                        principalTable: "ArticulosDs44",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Alertas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    DiasAnticipacion = table.Column<int>(type: "INTEGER", nullable: false),
                    Activa = table.Column<bool>(type: "INTEGER", nullable: false),
                    TipoAlerta = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Alertas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Alertas_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CentrosTrabajo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Direccion = table.Column<string>(type: "TEXT", nullable: true),
                    Region = table.Column<string>(type: "TEXT", nullable: true),
                    Ciudad = table.Column<string>(type: "TEXT", nullable: true),
                    Sector = table.Column<string>(type: "TEXT", nullable: true),
                    PlanoRiesgosPath = table.Column<string>(type: "TEXT", nullable: true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CentrosTrabajo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CentrosTrabajo_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratistas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreFantasia = table.Column<string>(type: "TEXT", nullable: false),
                    Rut = table.Column<string>(type: "TEXT", nullable: false),
                    Giro = table.Column<string>(type: "TEXT", nullable: false),
                    EmpresaPrincipalId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratistas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratistas_Empresas_EmpresaPrincipalId",
                        column: x => x.EmpresaPrincipalId,
                        principalTable: "Empresas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Indicadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Formula = table.Column<string>(type: "TEXT", nullable: false),
                    Valor = table.Column<string>(type: "TEXT", nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Indicadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Indicadores_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantillaCampos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantillaDocumentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    NombreCampo = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Obligatorio = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillaCampos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantillaCampos_Plantillas_PlantillaDocumentoId",
                        column: x => x.PlantillaDocumentoId,
                        principalTable: "Plantillas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "VersionesDocumento",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantillaDocumentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Notas = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_VersionesDocumento", x => x.Id);
                    table.ForeignKey(
                        name: "FK_VersionesDocumento_Plantillas_PlantillaDocumentoId",
                        column: x => x.PlantillaDocumentoId,
                        principalTable: "Plantillas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Actividades",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    RubroId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Actividades", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Actividades_Rubros_RubroId",
                        column: x => x.RubroId,
                        principalTable: "Rubros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosGenerados",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    PlantillaDocumentoId = table.Column<int>(type: "INTEGER", nullable: false),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CentroTrabajoId = table.Column<int>(type: "INTEGER", nullable: true),
                    FechaGenerado = table.Column<DateTime>(type: "TEXT", nullable: false),
                    RutaArchivoEditable = table.Column<string>(type: "TEXT", nullable: false),
                    RutaArchivoPdf = table.Column<string>(type: "TEXT", nullable: false),
                    Version = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosGenerados", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosGenerados_CentrosTrabajo_CentroTrabajoId",
                        column: x => x.CentroTrabajoId,
                        principalTable: "CentrosTrabajo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_DocumentosGenerados_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentosGenerados_Plantillas_PlantillaDocumentoId",
                        column: x => x.PlantillaDocumentoId,
                        principalTable: "Plantillas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Equipo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Tipo = table.Column<string>(type: "TEXT", nullable: false),
                    FechaAdquisicion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaMantenimientoProgramado = table.Column<DateTime>(type: "TEXT", nullable: true),
                    InstruccionesSeguridad = table.Column<string>(type: "TEXT", nullable: true),
                    CentroTrabajoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Equipo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Equipo_CentrosTrabajo_CentroTrabajoId",
                        column: x => x.CentroTrabajoId,
                        principalTable: "CentrosTrabajo",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "ObligacionesEmpresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    CentroTrabajoId = table.Column<int>(type: "INTEGER", nullable: true),
                    ObligacionDs44Id = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaLimite = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaCumplimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ObligacionesEmpresa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ObligacionesEmpresa_CentrosTrabajo_CentroTrabajoId",
                        column: x => x.CentroTrabajoId,
                        principalTable: "CentrosTrabajo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_ObligacionesEmpresa_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ObligacionesEmpresa_ObligacionesDs44_ObligacionDs44Id",
                        column: x => x.ObligacionDs44Id,
                        principalTable: "ObligacionesDs44",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Trabajadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    NombreCompleto = table.Column<string>(type: "TEXT", nullable: false),
                    Rut = table.Column<string>(type: "TEXT", nullable: false),
                    FechaIngreso = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Cargo = table.Column<string>(type: "TEXT", nullable: false),
                    Email = table.Column<string>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    CentroTrabajoId = table.Column<int>(type: "INTEGER", nullable: true),
                    Sensibilidad = table.Column<int>(type: "INTEGER", nullable: false),
                    EsContratista = table.Column<bool>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true),
                    ProtocolosVigilancia = table.Column<string>(type: "TEXT", nullable: true),
                    ContratistaId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Trabajadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Trabajadores_CentrosTrabajo_CentroTrabajoId",
                        column: x => x.CentroTrabajoId,
                        principalTable: "CentrosTrabajo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Trabajadores_Contratistas_ContratistaId",
                        column: x => x.ContratistaId,
                        principalTable: "Contratistas",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Tareas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    ActividadId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Tareas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Tareas_Actividades_ActividadId",
                        column: x => x.ActividadId,
                        principalTable: "Actividades",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "HistorialCambios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Entidad = table.Column<string>(type: "TEXT", nullable: false),
                    TipoCambio = table.Column<string>(type: "TEXT", nullable: false),
                    Usuario = table.Column<string>(type: "TEXT", nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    ValorAnterior = table.Column<string>(type: "TEXT", nullable: true),
                    ValorNuevo = table.Column<string>(type: "TEXT", nullable: true),
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: true),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_HistorialCambios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_HistorialCambios_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_HistorialCambios_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "Incidentes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    Causas = table.Column<string>(type: "TEXT", nullable: true),
                    MedidasCorrectivas = table.Column<string>(type: "TEXT", nullable: true),
                    MedidasImplementadas = table.Column<bool>(type: "INTEGER", nullable: false),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: true),
                    CentroTrabajoId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Incidentes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Incidentes_CentrosTrabajo_CentroTrabajoId",
                        column: x => x.CentroTrabajoId,
                        principalTable: "CentrosTrabajo",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_Incidentes_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id");
                });

            migrationBuilder.CreateTable(
                name: "RolesTrabajador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    RolInternoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RolesTrabajador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RolesTrabajador_Roles_RolInternoId",
                        column: x => x.RolInternoId,
                        principalTable: "Roles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_RolesTrabajador_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MatrizRiesgosEmpresa",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CentroTrabajoId = table.Column<int>(type: "INTEGER", nullable: false),
                    TareaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeligroId = table.Column<int>(type: "INTEGER", nullable: false),
                    ControlId = table.Column<int>(type: "INTEGER", nullable: true),
                    MedidaControlEspecifica = table.Column<string>(type: "TEXT", nullable: false),
                    Probabilidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Consecuencia = table.Column<int>(type: "INTEGER", nullable: false),
                    NivelRiesgo = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MatrizRiesgosEmpresa", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MatrizRiesgosEmpresa_CentrosTrabajo_CentroTrabajoId",
                        column: x => x.CentroTrabajoId,
                        principalTable: "CentrosTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatrizRiesgosEmpresa_Controles_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controles",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MatrizRiesgosEmpresa_Peligros_PeligroId",
                        column: x => x.PeligroId,
                        principalTable: "Peligros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_MatrizRiesgosEmpresa_Tareas_TareaId",
                        column: x => x.TareaId,
                        principalTable: "Tareas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "TareaEquipo",
                columns: table => new
                {
                    EquiposId = table.Column<int>(type: "INTEGER", nullable: false),
                    TareasRelacionadasId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareaEquipo", x => new { x.EquiposId, x.TareasRelacionadasId });
                    table.ForeignKey(
                        name: "FK_TareaEquipo_Equipo_EquiposId",
                        column: x => x.EquiposId,
                        principalTable: "Equipo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TareaEquipo_Tareas_TareasRelacionadasId",
                        column: x => x.TareasRelacionadasId,
                        principalTable: "Tareas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TareaPeligroControles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TareaId = table.Column<int>(type: "INTEGER", nullable: false),
                    PeligroId = table.Column<int>(type: "INTEGER", nullable: false),
                    ControlId = table.Column<int>(type: "INTEGER", nullable: false),
                    Probabilidad = table.Column<int>(type: "INTEGER", nullable: false),
                    Consecuencia = table.Column<int>(type: "INTEGER", nullable: false),
                    NivelRiesgo = table.Column<int>(type: "INTEGER", nullable: false),
                    Aplicable = table.Column<bool>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TareaPeligroControles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_TareaPeligroControles_Controles_ControlId",
                        column: x => x.ControlId,
                        principalTable: "Controles",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TareaPeligroControles_Peligros_PeligroId",
                        column: x => x.PeligroId,
                        principalTable: "Peligros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TareaPeligroControles_Tareas_TareaId",
                        column: x => x.TareaId,
                        principalTable: "Tareas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "TrabajadorTarea",
                columns: table => new
                {
                    TareasAsignadasId = table.Column<int>(type: "INTEGER", nullable: false),
                    TrabajadoresId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TrabajadorTarea", x => new { x.TareasAsignadasId, x.TrabajadoresId });
                    table.ForeignKey(
                        name: "FK_TrabajadorTarea_Tareas_TareasAsignadasId",
                        column: x => x.TareasAsignadasId,
                        principalTable: "Tareas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_TrabajadorTarea_Trabajadores_TrabajadoresId",
                        column: x => x.TrabajadoresId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "RegistrosSiniestro",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IncidenteId = table.Column<int>(type: "INTEGER", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    Comentarios = table.Column<string>(type: "TEXT", nullable: true),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RegistrosSiniestro", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RegistrosSiniestro_Incidentes_IncidenteId",
                        column: x => x.IncidenteId,
                        principalTable: "Incidentes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "ArticulosDs44",
                columns: new[] { "Id", "Contenido", "Numero", "Titulo" },
                values: new object[,]
                {
                    { 1, "El empleador deberá garantizar condiciones de trabajo seguras y saludables.", "Art. 1", "Disposiciones Generales" },
                    { 7, "Implementación de vigilancia de la salud y protocolos MINSAL.", "Art. 7", "Salud Ocupacional" },
                    { 8, "Identificación de peligros y evaluación de riesgos (MIPER).", "Art. 8", "Gestión de Riesgos" },
                    { 18, "Constitución y funcionamiento de Comités Paritarios.", "Art. 18", "Participación" },
                    { 25, "Planes de emergencia y evacuación ante siniestros.", "Art. 25", "Emergencias" }
                });

            migrationBuilder.InsertData(
                table: "Controles",
                columns: new[] { "Id", "Descripcion", "Tipo" },
                values: new object[,]
                {
                    { 1, "Sistemas de entibación (Cajones, Tablestacas)", "Ingeniería" },
                    { 2, "Sistemas de extracción localizada de humos/polvo", "Ingeniería" },
                    { 3, "Protecciones y guardas en partes móviles", "Ingeniería" },
                    { 4, "Barreras físicas y delimitación de zonas", "Ingeniería" },
                    { 5, "Ayudas mecánicas para carga (Tecles, Grúas)", "Ingeniería" },
                    { 6, "Procedimiento de Trabajo Seguro (PTS)", "Administrativo" },
                    { 7, "Capacitación y Charla de 5 minutos", "Administrativo" },
                    { 8, "Señalización de seguridad", "Administrativo" },
                    { 9, "Permiso de Trabajo (Altura, Caliente, Espacio Confinado)", "Administrativo" },
                    { 10, "Rotación de puestos de trabajo", "Administrativo" },
                    { 11, "Bloqueo y Etiquetado (LOTO)", "Administrativo" },
                    { 12, "Programa de Vigilancia Médica (Protocolos MINSAL)", "Administrativo" },
                    { 13, "Casco de seguridad con barbiquejo", "EPP" },
                    { 14, "Lentes de seguridad (Claros/Oscuros)", "EPP" },
                    { 15, "Protección auditiva (Tapones/Fonos)", "EPP" },
                    { 16, "Respirador medio rostro con filtros (P100/Vapores)", "EPP" },
                    { 17, "Guantes de seguridad (Cabritilla, Hycron, Nitrilo)", "EPP" },
                    { 18, "Calzado de seguridad", "EPP" },
                    { 19, "Arnés de cuerpo completo con doble cabo de vida", "EPP" },
                    { 20, "Ropa con protección UV / Legionario", "EPP" },
                    { 21, "Traje Tyvek para químicos", "EPP" }
                });

            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Direccion", "EmailContacto", "Giro", "LogoPath", "Mutual", "NumeroTrabajadores", "RazonSocial", "RepresentanteLegal", "RubroPrincipal", "Rut", "Telefono" },
                values: new object[] { 1, "Pedro de Miranda #183, Lo Miranda", "contacto@jgvalenzuela.cl", "Servicios de mantención y reparación industrial", null, "ACHS", 15, "JUAN GONZALEZ VALENZUELA SPA", "Juan González Valenzuela", "Industria Manufacturera", "76.825.693-4", "+56 9 1234 5678" });

            migrationBuilder.InsertData(
                table: "LogsAuditoria",
                columns: new[] { "Id", "Accion", "Detalle", "Entidad", "EntidadId", "Fecha", "Usuario" },
                values: new object[] { 1, "Inicializar", "Base de datos poblada con investigación FINAL DS 44 (2023) y normativa complementaria", "Sistema", "0", new DateTime(2025, 11, 26, 22, 32, 9, 539, DateTimeKind.Local).AddTicks(4711), "Sistema" });

            migrationBuilder.InsertData(
                table: "Peligros",
                columns: new[] { "Id", "Categoria", "Descripcion" },
                values: new object[,]
                {
                    { 1, "Seguridad", "Caída a distinto nivel (> 1.8m)" },
                    { 2, "Seguridad", "Caída al mismo nivel (tropiezos, resbalones)" },
                    { 3, "Seguridad", "Atrapamiento por partes móviles" },
                    { 4, "Seguridad", "Golpeado por objeto en movimiento/caída" },
                    { 5, "Seguridad", "Contacto con energía eléctrica (Directo/Indirecto)" },
                    { 6, "Seguridad", "Cortes con herramientas cortopunzantes" },
                    { 7, "Seguridad", "Proyección de partículas incandescentes" },
                    { 8, "Seguridad", "Derrumbe de paredes/zanjas" },
                    { 9, "Seguridad", "Volcamiento de maquinaria" },
                    { 10, "Físico", "Exposición a Ruido (PREXOR)" },
                    { 11, "Físico", "Exposición a Sílice Libre (PLANESI)" },
                    { 12, "Físico", "Radiación UV Solar" },
                    { 13, "Físico", "Vibraciones (Mano-brazo / Cuerpo entero)" },
                    { 14, "Químico", "Humos metálicos de soldadura" },
                    { 15, "Químico", "Exposición a Plaguicidas/Fitosanitarios" },
                    { 16, "Químico", "Exposición a Solventes/Combustibles" },
                    { 17, "Biológico", "Exposición a virus/bacterias (Hanta, Covid, Tétanos)" },
                    { 18, "Ergonómico", "Manejo Manual de Cargas (MMC)" },
                    { 19, "Ergonómico", "Movimientos Repetitivos (TMERT)" },
                    { 20, "Ergonómico", "Posturas forzadas o estáticas" },
                    { 21, "Psicosocial", "Alta exigencia psicológica (Ritmo, Cantidad)" },
                    { 22, "Psicosocial", "Acoso laboral o sexual (Ley Karin)" },
                    { 23, "Psicosocial", "Doble presencia (Trabajo/Familia)" }
                });

            migrationBuilder.InsertData(
                table: "Plantillas",
                columns: new[] { "Id", "Codigo", "Descripcion", "Nombre", "RutaBase", "Tipo" },
                values: new object[,]
                {
                    { 1, "MIPER-STD", "Matriz de Riesgos con evaluación PxC", "Matriz IPER Estándar", "Plantillas/Matriz_IPER_V2.xlsx", "Excel" },
                    { 2, "RIOHS-KARIN", "Modelo RIOHS actualizado a Ley 21.643", "Reglamento Interno (Adaptado Ley Karin)", "Plantillas/RIOHS_2025_Karin.docx", "Word" },
                    { 3, "ODI-GEN", "Obligación de Informar Riesgos (DAS)", "Formato ODI Genérico", "Plantillas/ODI_Base.docx", "Word" },
                    { 4, "CHECK-DS594", "Auditoría condiciones sanitarias y ambientales", "Lista Chequeo DS 594", "Plantillas/Checklist_DS594.xlsx", "Excel" },
                    { 5, "PROTO-ACOS", "Protocolo obligatorio de prevención", "Protocolo Acoso y Violencia (Ley Karin)", "Plantillas/Protocolo_Karin.docx", "Word" },
                    { 6, "ACTA-CPHS", "Para empresas con más de 25 trabajadores", "Acta Constitución CPHS", "Plantillas/Acta_CPHS.docx", "Word" },
                    { 7, "REGL-CONTRAT", "Cumplimiento Ley 20.123", "Reglamento Especial Contratistas", "Plantillas/Reglamento_Contratistas.docx", "Word" },
                    { 8, "PROC-ACC-GRV", "Circular 3335 SUSESO", "Procedimiento Accidentes Graves", "Plantillas/Proc_Accidentes_Graves.docx", "Word" },
                    { 9, "POLITICA-SST", "Compromiso gerencial DS 44", "Política de Seguridad y Salud", "Plantillas/Politica_SST.docx", "Word" },
                    { 10, "PROG-PREV", "Planificación anual DS 44", "Programa de Prevención Anual", "Plantillas/Programa_Prevencion.xlsx", "Excel" },
                    { 11, "MAPA-RIESGOS", "Guía para elaborar mapa visual", "Mapa de Riesgos (Guía)", "Plantillas/Guia_Mapa_Riesgos.docx", "Word" },
                    { 12, "AUTO-SGSST", "Auditoría interna anual DS 44", "Autoevaluación SG-SST", "Plantillas/Autoevaluacion_SGSST.xlsx", "Excel" }
                });

            migrationBuilder.InsertData(
                table: "Rubros",
                columns: new[] { "Id", "Codigo", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, "CONST", "Obras de edificación y obras civiles", "Construcción" },
                    { 2, "IND", "Procesos de transformación industrial", "Industria Manufacturera" },
                    { 3, "MIN", "Extracción de minerales", "Minería" },
                    { 4, "TRANS", "Transporte de carga y pasajeros, almacenamiento", "Transporte y Logística" },
                    { 5, "AGRO", "Cultivos, cosecha y packing", "Agricultura" },
                    { 6, "SERV", "Oficinas, retail, educación, salud", "Servicios y Comercio" }
                });

            migrationBuilder.InsertData(
                table: "Actividades",
                columns: new[] { "Id", "Descripcion", "Nombre", "RubroId" },
                values: new object[,]
                {
                    { 1, "Uso de maquinaria pesada y excavación manual", "Excavación y Movimiento de Tierras", 1 },
                    { 2, "Montaje de estructuras, techumbres y fachadas", "Trabajos en Altura (Estructural)", 1 },
                    { 3, "Enfierradura, moldaje y hormigonado", "Obras de Hormigón", 1 },
                    { 4, "Unión de metales (Arco, TIG, MIG, Oxicorte)", "Soldadura y Corte", 2 },
                    { 5, "Reparación de maquinaria industrial", "Mantenimiento Mecánico", 2 },
                    { 6, "Transporte de carga en carretera y ciudad", "Conducción de Vehículos", 4 },
                    { 7, "Movimiento de carga en bodegas", "Operación de Grúa Horquilla", 4 },
                    { 8, "Uso de plaguicidas y pesticidas", "Aplicación de Fitosanitarios", 5 },
                    { 9, "Recolección de fruta y vegetales", "Cosecha Manual", 5 },
                    { 10, "Uso de pantallas y atención de público", "Trabajo Administrativo", 6 },
                    { 11, "Uso de productos químicos de limpieza", "Aseo y Limpieza", 6 }
                });

            migrationBuilder.InsertData(
                table: "CentrosTrabajo",
                columns: new[] { "Id", "Ciudad", "Direccion", "EmpresaId", "Nombre", "PlanoRiesgosPath", "Region", "Sector" },
                values: new object[] { 1, "Donihue", "Pedro de Miranda #183, Lo Miranda", 1, "Taller Principal", null, "O'Higgins", "Operaciones" });

            migrationBuilder.InsertData(
                table: "ObligacionesDs44",
                columns: new[] { "Id", "AplicaMaxTrab", "AplicaMinTrab", "ArticuloDs44Id", "Codigo", "Descripcion", "RubroObjetivo" },
                values: new object[,]
                {
                    { 1, null, 1, 1, "DS44-ART1", "Implementar Sistema de Gestión de Seguridad y Salud en el Trabajo (SGSST)", null },
                    { 2, null, 1, 8, "DS44-MIPER", "Matriz de Identificación de Peligros y Evaluación de Riesgos (MIPER) actualizada anualmente", null },
                    { 3, null, 25, 18, "DS44-CPHS", "Constitución y funcionamiento de Comité Paritario de Higiene y Seguridad (CPHS)", null },
                    { 4, null, 1, 25, "DS44-EMERG", "Plan de Emergencia y Evacuación con simulacros periódicos", null },
                    { 5, null, 1, 1, "DS594-AGUA", "Provisión de agua potable fresca y suficiente (Art. 12)", null },
                    { 6, null, 1, 1, "DS594-BANOS", "Servicios higiénicos independientes y separados por sexo (Art. 21)", null },
                    { 7, null, 1, 1, "DS594-LOCKER", "Guardarropas individuales para cambio de ropa (Art. 27)", null },
                    { 8, null, 1, 1, "DS594-COMEDOR", "Comedor habilitado separado de áreas de trabajo (Art. 28)", null },
                    { 9, null, 1, 1, "DS594-EXTINTOR", "Extintores de incendio adecuados y mantención vigente (Art. 45)", null },
                    { 10, null, 1, 1, "DS40-ODI", "Obligación de Informar (ODI) los riesgos laborales a cada trabajador (Art. 21)", null },
                    { 11, null, 10, 1, "DS40-RIOHS", "Reglamento Interno de Orden, Higiene y Seguridad (RIOHS) (Art. 14)", null },
                    { 12, null, 100, 1, "DS40-DEPTO", "Departamento de Prevención de Riesgos liderado por experto (Art. 8)", null },
                    { 13, null, 1, 1, "KARIN-PROTO", "Protocolo de Prevención del Acoso Sexual, Laboral y Violencia en el Trabajo", null },
                    { 14, null, 1, 1, "KARIN-DIFUSION", "Difusión semestral de canales de denuncia y medidas de resguardo", null },
                    { 15, null, 1, 7, "PROTO-PREXOR", "Implementación Protocolo de Exposición a Ruido (PREXOR)", null },
                    { 16, null, 1, 7, "PROTO-PLANESI", "Implementación Plan Nacional de Erradicación de Silicosis (PLANESI)", null },
                    { 17, null, 1, 7, "PROTO-TMERT", "Evaluación de Trastornos Musculoesqueléticos (TMERT-EESS)", null },
                    { 18, null, 1, 7, "PROTO-UV", "Programa de Protección contra Radiación UV de origen solar", null },
                    { 19, null, 1, 7, "PROTO-PSICO", "Evaluación de Riesgos Psicosociales (Cuestionario CEAL-SM / ISTAS21)", null },
                    { 20, null, 1, 7, "PROTO-MMC", "Gestión del riesgo por Manejo Manual de Cargas (Ley 20.949)", null },
                    { 21, null, 25, 18, "DS54-CONST", "Constitución de Comité Paritario (CPHS) con representantes titulares y suplentes", null },
                    { 22, null, 25, 18, "DS54-PROG", "Programa de Trabajo del CPHS y cronograma de reuniones mensuales", null },
                    { 23, null, 25, 18, "DS54-INVEST", "Investigación de todos los accidentes por parte del CPHS", null },
                    { 24, null, 1, 1, "LEY20123-REGL", "Reglamento Especial para Empresas Contratistas y Subcontratistas", null },
                    { 25, null, 1, 1, "LEY20123-REG", "Registro actualizado de antecedentes de trabajadores contratistas", null },
                    { 26, null, 1, 1, "DS43-AUTORIZ", "Autorización Sanitaria para almacenamiento de Sustancias Peligrosas", null },
                    { 27, null, 1, 1, "DS43-MATRIZ", "Matriz de Incompatibilidad Química y Hojas de Datos de Seguridad (HDS)", null },
                    { 28, null, 1, 25, "CIRC3335-NOTIF", "Procedimiento de Notificación Inmediata de Accidentes Graves y Fatales", null },
                    { 29, null, 1, 25, "CIRC3335-SUSP", "Procedimiento de Auto-suspensión de faenas ante riesgo inminente", null },
                    { 30, null, 1, 1, "DS18-CERT", "Uso exclusivo de Elementos de Protección Personal (EPP) certificados (ISP)", null },
                    { 31, null, 1, 1, "DS44-POLITICA", "Política de Seguridad y Salud en el Trabajo firmada y difundida", null },
                    { 32, null, 1, 1, "DS44-PROG-PREV", "Programa de Trabajo en Prevención de Riesgos (Anual)", null },
                    { 33, null, 1, 1, "DS44-MAPA", "Mapa de Riesgos del centro de trabajo (Visible)", null },
                    { 34, null, 1, 1, "DS44-AUTOEVAL", "Autoevaluación Anual del Sistema de Gestión (SG-SST)", null },
                    { 35, null, 1, 1, "DS44-REG-ACC", "Registro histórico de Accidentes y Enfermedades Profesionales", null }
                });

            migrationBuilder.InsertData(
                table: "Tareas",
                columns: new[] { "Id", "ActividadId", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 1, 1, "Manejo de maquinaria para zanjas", "Operación de Retroexcavadora" },
                    { 2, 1, "Uso de pala y picota al interior de zanja", "Perfilado manual de zanjas" },
                    { 3, 2, "Colocación de planchas de zinc sobre costaneras", "Instalación de techumbre" },
                    { 4, 2, "Montaje de cuerpos de andamio", "Armado de andamios" },
                    { 5, 4, "Unión de piezas con electrodo revestido", "Soldadura al arco manual" },
                    { 6, 4, "Uso de galletera angular", "Esmerilado de uniones" },
                    { 7, 7, "Operación de grúa horquilla en patio de carga", "Carga y descarga de camiones" },
                    { 8, 7, "Almacenamiento en altura", "Apilamiento en racks" },
                    { 9, 8, "Dosificación de plaguicidas", "Preparación de mezcla" },
                    { 10, 8, "Aplicación mecanizada en huerto", "Pulverización con tractor" },
                    { 11, 10, "Uso intensivo de teclado y mouse", "Digitación de datos" },
                    { 12, 10, "Interacción presencial con público", "Atención de clientes" }
                });

            migrationBuilder.InsertData(
                table: "MatrizRiesgosEmpresa",
                columns: new[] { "Id", "CentroTrabajoId", "Consecuencia", "ControlId", "MedidaControlEspecifica", "NivelRiesgo", "PeligroId", "Probabilidad", "TareaId" },
                values: new object[,]
                {
                    { 1, 1, 3, 2, "Brazo extractor articulado", "Medio", 14, 3, 5 },
                    { 2, 1, 3, 16, "Uso obligatorio respirador filtros P100", "Medio", 14, 3, 5 },
                    { 3, 1, 4, 4, "Biombos opacos incombustibles", "Medio", 12, 2, 5 },
                    { 4, 1, 4, 14, "Careta facial + Lentes seguridad", "Alto", 7, 3, 6 },
                    { 5, 1, 5, 19, "Uso permanente SPDC anclado a línea de vida", "Alto", 1, 3, 3 },
                    { 6, 1, 5, 1, "Entibación discontinua según mecánica de suelos", "Alto", 8, 2, 2 }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Actividades_RubroId",
                table: "Actividades",
                column: "RubroId");

            migrationBuilder.CreateIndex(
                name: "IX_Alertas_EmpresaId",
                table: "Alertas",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_CentrosTrabajo_EmpresaId",
                table: "CentrosTrabajo",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratistas_EmpresaPrincipalId",
                table: "Contratistas",
                column: "EmpresaPrincipalId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosGenerados_CentroTrabajoId",
                table: "DocumentosGenerados",
                column: "CentroTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosGenerados_EmpresaId",
                table: "DocumentosGenerados",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosGenerados_PlantillaDocumentoId",
                table: "DocumentosGenerados",
                column: "PlantillaDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_Rut",
                table: "Empresas",
                column: "Rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Equipo_CentroTrabajoId",
                table: "Equipo",
                column: "CentroTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCambios_EmpresaId",
                table: "HistorialCambios",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_HistorialCambios_TrabajadorId",
                table: "HistorialCambios",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_CentroTrabajoId",
                table: "Incidentes",
                column: "CentroTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_Incidentes_TrabajadorId",
                table: "Incidentes",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Indicadores_EmpresaId",
                table: "Indicadores",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_MatrizRiesgosEmpresa_CentroTrabajoId",
                table: "MatrizRiesgosEmpresa",
                column: "CentroTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_MatrizRiesgosEmpresa_ControlId",
                table: "MatrizRiesgosEmpresa",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_MatrizRiesgosEmpresa_PeligroId",
                table: "MatrizRiesgosEmpresa",
                column: "PeligroId");

            migrationBuilder.CreateIndex(
                name: "IX_MatrizRiesgosEmpresa_TareaId",
                table: "MatrizRiesgosEmpresa",
                column: "TareaId");

            migrationBuilder.CreateIndex(
                name: "IX_ObligacionesDs44_ArticuloDs44Id",
                table: "ObligacionesDs44",
                column: "ArticuloDs44Id");

            migrationBuilder.CreateIndex(
                name: "IX_ObligacionesEmpresa_CentroTrabajoId",
                table: "ObligacionesEmpresa",
                column: "CentroTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_ObligacionesEmpresa_EmpresaId",
                table: "ObligacionesEmpresa",
                column: "EmpresaId");

            migrationBuilder.CreateIndex(
                name: "IX_ObligacionesEmpresa_ObligacionDs44Id",
                table: "ObligacionesEmpresa",
                column: "ObligacionDs44Id");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillaCampos_PlantillaDocumentoId",
                table: "PlantillaCampos",
                column: "PlantillaDocumentoId");

            migrationBuilder.CreateIndex(
                name: "IX_RegistrosSiniestro_IncidenteId",
                table: "RegistrosSiniestro",
                column: "IncidenteId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesTrabajador_RolInternoId",
                table: "RolesTrabajador",
                column: "RolInternoId");

            migrationBuilder.CreateIndex(
                name: "IX_RolesTrabajador_TrabajadorId",
                table: "RolesTrabajador",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_TareaEquipo_TareasRelacionadasId",
                table: "TareaEquipo",
                column: "TareasRelacionadasId");

            migrationBuilder.CreateIndex(
                name: "IX_TareaPeligroControles_ControlId",
                table: "TareaPeligroControles",
                column: "ControlId");

            migrationBuilder.CreateIndex(
                name: "IX_TareaPeligroControles_PeligroId",
                table: "TareaPeligroControles",
                column: "PeligroId");

            migrationBuilder.CreateIndex(
                name: "IX_TareaPeligroControles_TareaId_PeligroId_ControlId",
                table: "TareaPeligroControles",
                columns: new[] { "TareaId", "PeligroId", "ControlId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Tareas_ActividadId",
                table: "Tareas",
                column: "ActividadId");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_CentroTrabajoId",
                table: "Trabajadores",
                column: "CentroTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_ContratistaId",
                table: "Trabajadores",
                column: "ContratistaId");

            migrationBuilder.CreateIndex(
                name: "IX_Trabajadores_Rut",
                table: "Trabajadores",
                column: "Rut",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_TrabajadorTarea_TrabajadoresId",
                table: "TrabajadorTarea",
                column: "TrabajadoresId");

            migrationBuilder.CreateIndex(
                name: "IX_VersionesDocumento_PlantillaDocumentoId",
                table: "VersionesDocumento",
                column: "PlantillaDocumentoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Alertas");

            migrationBuilder.DropTable(
                name: "DocumentosGenerados");

            migrationBuilder.DropTable(
                name: "HistorialCambios");

            migrationBuilder.DropTable(
                name: "Indicadores");

            migrationBuilder.DropTable(
                name: "LogsAuditoria");

            migrationBuilder.DropTable(
                name: "MatrizRiesgosEmpresa");

            migrationBuilder.DropTable(
                name: "ObligacionesEmpresa");

            migrationBuilder.DropTable(
                name: "PlantillaCampos");

            migrationBuilder.DropTable(
                name: "RegistrosSiniestro");

            migrationBuilder.DropTable(
                name: "RolesTrabajador");

            migrationBuilder.DropTable(
                name: "TareaEquipo");

            migrationBuilder.DropTable(
                name: "TareaPeligroControles");

            migrationBuilder.DropTable(
                name: "TrabajadorTarea");

            migrationBuilder.DropTable(
                name: "VersionesDocumento");

            migrationBuilder.DropTable(
                name: "ObligacionesDs44");

            migrationBuilder.DropTable(
                name: "Incidentes");

            migrationBuilder.DropTable(
                name: "Roles");

            migrationBuilder.DropTable(
                name: "Equipo");

            migrationBuilder.DropTable(
                name: "Controles");

            migrationBuilder.DropTable(
                name: "Peligros");

            migrationBuilder.DropTable(
                name: "Tareas");

            migrationBuilder.DropTable(
                name: "Plantillas");

            migrationBuilder.DropTable(
                name: "ArticulosDs44");

            migrationBuilder.DropTable(
                name: "Trabajadores");

            migrationBuilder.DropTable(
                name: "Actividades");

            migrationBuilder.DropTable(
                name: "CentrosTrabajo");

            migrationBuilder.DropTable(
                name: "Contratistas");

            migrationBuilder.DropTable(
                name: "Rubros");

            migrationBuilder.DropTable(
                name: "Empresas");
        }
    }
}
