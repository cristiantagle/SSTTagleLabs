using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRRHHEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.CreateTable(
                name: "DocumentosTrabajadores",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    Nombre = table.Column<string>(type: "TEXT", nullable: false),
                    Categoria = table.Column<int>(type: "INTEGER", nullable: false),
                    RutaArchivo = table.Column<string>(type: "TEXT", nullable: false),
                    Extension = table.Column<string>(type: "TEXT", nullable: false),
                    FechaSubida = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DocumentosTrabajadores", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DocumentosTrabajadores_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "SolicitudesVacaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaInicio = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaFin = table.Column<DateTime>(type: "TEXT", nullable: false),
                    DiasHabiles = table.Column<decimal>(type: "TEXT", nullable: false),
                    Estado = table.Column<int>(type: "INTEGER", nullable: false),
                    Observaciones = table.Column<string>(type: "TEXT", nullable: true),
                    FechaRetorno = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SolicitudesVacaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SolicitudesVacaciones_Trabajadores_TrabajadorId",
                        column: x => x.TrabajadorId,
                        principalTable: "Trabajadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosVacaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TrabajadorId = table.Column<int>(type: "INTEGER", nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Tipo = table.Column<int>(type: "INTEGER", nullable: false),
                    Dias = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Descripcion = table.Column<string>(type: "TEXT", nullable: false),
                    SolicitudVacacionesId = table.Column<int>(type: "INTEGER", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosVacaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosVacaciones_SolicitudesVacaciones_SolicitudVacacionesId",
                        column: x => x.SolicitudVacacionesId,
                        principalTable: "SolicitudesVacaciones",
                        principalColumn: "Id");
                    table.ForeignKey(
                        name: "FK_MovimientosVacaciones_Trabajadores_TrabajadorId",
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
                value: new DateTime(2025, 11, 30, 10, 29, 52, 252, DateTimeKind.Local).AddTicks(4202));

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosTrabajadores_TrabajadorId",
                table: "DocumentosTrabajadores",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosVacaciones_SolicitudVacacionesId",
                table: "MovimientosVacaciones",
                column: "SolicitudVacacionesId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosVacaciones_TrabajadorId",
                table: "MovimientosVacaciones",
                column: "TrabajadorId");

            migrationBuilder.CreateIndex(
                name: "IX_SolicitudesVacaciones_TrabajadorId",
                table: "SolicitudesVacaciones",
                column: "TrabajadorId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {


            migrationBuilder.DropTable(
                name: "DocumentosTrabajadores");

            migrationBuilder.DropTable(
                name: "MovimientosVacaciones");

            migrationBuilder.DropTable(
                name: "SolicitudesVacaciones");

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 11, 29, 0, 15, 44, 50, DateTimeKind.Local).AddTicks(7824));
        }
    }
}
