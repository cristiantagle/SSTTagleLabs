using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class LugaresTrabajo : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "LugaresTrabajo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    LogoPath = table.Column<string>(type: "TEXT", nullable: true),
                    Direccion = table.Column<string>(type: "TEXT", nullable: true),
                    ContactoNombre = table.Column<string>(type: "TEXT", nullable: true),
                    ContactoEmail = table.Column<string>(type: "TEXT", nullable: true),
                    ContactoTelefono = table.Column<string>(type: "TEXT", nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_LugaresTrabajo", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesTrabajador",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    LugarTrabajoId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaAsignacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesTrabajador", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesTrabajador_LugaresTrabajo_LugarTrabajoId",
                        column: x => x.LugarTrabajoId,
                        principalTable: "LugaresTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsignacionesTrabajador_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CharlasSST",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LugarTrabajoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Tema = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    Fecha = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Expositor = table.Column<string>(type: "TEXT", nullable: true),
                    DuracionMinutos = table.Column<int>(type: "INTEGER", nullable: false),
                    RutaActaFirmada = table.Column<string>(type: "TEXT", nullable: true),
                    RutaDocumentoGenerado = table.Column<string>(type: "TEXT", nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CharlasSST", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CharlasSST_LugaresTrabajo_LugarTrabajoId",
                        column: x => x.LugarTrabajoId,
                        principalTable: "LugaresTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "PlantillasLugarTrabajo",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    LugarTrabajoId = table.Column<int>(type: "INTEGER", nullable: false),
                    Codigo = table.Column<string>(type: "TEXT", nullable: false),
                    NombreDocumento = table.Column<string>(type: "TEXT", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: true),
                    RutaPlantilla = table.Column<string>(type: "TEXT", nullable: true),
                    TipoAplicacion = table.Column<int>(type: "INTEGER", nullable: false),
                    EsObligatorio = table.Column<bool>(type: "INTEGER", nullable: false),
                    DiasVigencia = table.Column<int>(type: "INTEGER", nullable: true),
                    Orden = table.Column<int>(type: "INTEGER", nullable: false),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlantillasLugarTrabajo", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlantillasLugarTrabajo_LugaresTrabajo_LugarTrabajoId",
                        column: x => x.LugarTrabajoId,
                        principalTable: "LugaresTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "AsistenciasCharla",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    CharlaId = table.Column<int>(type: "INTEGER", nullable: false),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Asistio = table.Column<bool>(type: "INTEGER", nullable: false),
                    FechaRegistro = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsistenciasCharla", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsistenciasCharla_CharlasSST_CharlaId",
                        column: x => x.CharlaId,
                        principalTable: "CharlasSST",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_AsistenciasCharla_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "DocumentosRequeridos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    PlantillaLugarTrabajoId = table.Column<int>(type: "INTEGER", nullable: false),
                    RutaDocumentoGenerado = table.Column<string>(type: "TEXT", nullable: true),
                    FechaGeneracion = table.Column<DateTime>(type: "TEXT", nullable: true),
                    FechaVencimiento = table.Column<DateTime>(type: "TEXT", nullable: true),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosRequeridos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosRequeridos_PlantillasLugarTrabajo_PlantillaLugarTrabajoId",
                        column: x => x.PlantillaLugarTrabajoId,
                        principalTable: "PlantillasLugarTrabajo",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_DocumentosRequeridos_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 30, 17, 10, 44, 947, DateTimeKind.Local).AddTicks(2663));

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesTrabajador_LugarTrabajoId",
                table: "AsignacionesTrabajador",
                column: "LugarTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesTrabajador_TrabajadorId_LugarTrabajoId",
                table: "AsignacionesTrabajador",
                columns: new[] { "TrabajadorId", "LugarTrabajoId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsistenciasCharla_CharlaId_TrabajadorId",
                table: "AsistenciasCharla",
                columns: new[] { "CharlaId", "TrabajadorId" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsistenciasCharla_TrabajadorId",
                table: "AsistenciasCharla",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_CharlasSST_LugarTrabajoId",
                table: "CharlasSST",
                column: "LugarTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosRequeridos_PlantillaLugarTrabajoId",
                table: "DocumentosRequeridos",
                column: "PlantillaLugarTrabajoId");

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosRequeridos_TrabajadorId",
                table: "DocumentosRequeridos",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_PlantillasLugarTrabajo_LugarTrabajoId",
                table: "PlantillasLugarTrabajo",
                column: "LugarTrabajoId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AsignacionesTrabajador");

            migrationBuilder.DropTable(
                name: "AsistenciasCharla");

            migrationBuilder.DropTable(
                name: "DocumentosRequeridos");

            migrationBuilder.DropTable(
                name: "CharlasSST");

            migrationBuilder.DropTable(
                name: "PlantillasLugarTrabajo");

            migrationBuilder.DropTable(
                name: "LugaresTrabajo");

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 12, 14, 22, 58, 186, DateTimeKind.Local).AddTicks(9164));
        }
    }
}
