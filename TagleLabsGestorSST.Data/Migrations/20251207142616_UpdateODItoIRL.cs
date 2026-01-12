using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class UpdateODItoIRL : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 7, 11, 26, 15, 657, DateTimeKind.Local).AddTicks(8082));

            migrationBuilder.UpdateData(
                table: "ObligacionesDs44",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Codigo", "Descripcion" },
                values: new object[] { "DS44-IRL", "Información de Riesgos Laborales (IRL) específica para cada puesto de trabajo (Art. 15 DS44)" });

            migrationBuilder.UpdateData(
                table: "Plantillas",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Codigo", "Descripcion", "Nombre", "RutaBase" },
                values: new object[] { "IRL-GEN", "Información de Riesgos Laborales - Art. 15 DS44", "Formato IRL Genérico (DS44)", "Plantillas/IRL_Base.docx" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 3, 14, 36, 29, 530, DateTimeKind.Local).AddTicks(3829));

            migrationBuilder.UpdateData(
                table: "ObligacionesDs44",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "Codigo", "Descripcion" },
                values: new object[] { "DS40-ODI", "Obligación de Informar (ODI) los riesgos laborales a cada trabajador (Art. 21)" });

            migrationBuilder.UpdateData(
                table: "Plantillas",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "Codigo", "Descripcion", "Nombre", "RutaBase" },
                values: new object[] { "ODI-GEN", "Obligación de Informar Riesgos (DAS)", "Formato ODI Genérico", "Plantillas/ODI_Base.docx" });
        }
    }
}
