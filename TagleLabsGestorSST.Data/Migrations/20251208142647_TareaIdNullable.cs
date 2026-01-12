using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class TareaIdNullable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TareaId",
                table: "MatrizRiesgosEmpresa",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 8, 11, 26, 46, 852, DateTimeKind.Local).AddTicks(3968));

            migrationBuilder.UpdateData(
                table: "ObligacionesDs44",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Codigo", "Descripcion" },
                values: new object[] { "DS44-RIOHS", "Reglamento Interno de Orden, Higiene y Seguridad (RIOHS) - Art. 153 Código del Trabajo" });

            migrationBuilder.UpdateData(
                table: "ObligacionesDs44",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Codigo", "Descripcion" },
                values: new object[] { "DS44-DEPTO", "Departamento de Prevención de Riesgos liderado por Experto (DS44)" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "TareaId",
                table: "MatrizRiesgosEmpresa",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 7, 11, 26, 15, 657, DateTimeKind.Local).AddTicks(8082));

            migrationBuilder.UpdateData(
                table: "ObligacionesDs44",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Codigo", "Descripcion" },
                values: new object[] { "DS40-RIOHS", "Reglamento Interno de Orden, Higiene y Seguridad (RIOHS) (Art. 14)" });

            migrationBuilder.UpdateData(
                table: "ObligacionesDs44",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "Codigo", "Descripcion" },
                values: new object[] { "DS40-DEPTO", "Departamento de Prevención de Riesgos liderado por experto (Art. 8)" });
        }
    }
}
