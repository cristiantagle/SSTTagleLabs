using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMultipleRubrosToEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empresas_Rubros_RubroId",
                table: "Empresas");

            migrationBuilder.CreateTable(
                name: "EmpresaRubro",
                columns: table => new
                {
                    EmpresaId = table.Column<int>(type: "INTEGER", nullable: false),
                    RubroId = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_EmpresaRubro", x => new { x.EmpresaId, x.RubroId });
                    table.ForeignKey(
                        name: "FK_EmpresaRubro_Empresas_EmpresaId",
                        column: x => x.EmpresaId,
                        principalTable: "Empresas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_EmpresaRubro_Rubros_RubroId",
                        column: x => x.RubroId,
                        principalTable: "Rubros",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 12, 14, 22, 58, 186, DateTimeKind.Local).AddTicks(9164));

            migrationBuilder.CreateIndex(
                name: "IX_EmpresaRubro_RubroId",
                table: "EmpresaRubro",
                column: "RubroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empresas_Rubros_RubroId",
                table: "Empresas",
                column: "RubroId",
                principalTable: "Rubros",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empresas_Rubros_RubroId",
                table: "Empresas");

            migrationBuilder.DropTable(
                name: "EmpresaRubro");

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 11, 11, 10, 33, 997, DateTimeKind.Local).AddTicks(6946));

            migrationBuilder.AddForeignKey(
                name: "FK_Empresas_Rubros_RubroId",
                table: "Empresas",
                column: "RubroId",
                principalTable: "Rubros",
                principalColumn: "Id");
        }
    }
}
