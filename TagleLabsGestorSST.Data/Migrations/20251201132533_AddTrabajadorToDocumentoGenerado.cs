using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddTrabajadorToDocumentoGenerado : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "TrabajadorId",
                table: "DocumentosGenerados",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 1, 10, 25, 32, 905, DateTimeKind.Local).AddTicks(1618));

            migrationBuilder.CreateIndex(
                name: "IX_DocumentosGenerados_TrabajadorId",
                table: "DocumentosGenerados",
                column: "TrabajadorId");

            migrationBuilder.AddForeignKey(
                name: "FK_DocumentosGenerados_Trabajadores_TrabajadorId",
                table: "DocumentosGenerados",
                column: "TrabajadorId",
                principalTable: "Trabajadores",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_DocumentosGenerados_Trabajadores_TrabajadorId",
                table: "DocumentosGenerados");

            migrationBuilder.DropIndex(
                name: "IX_DocumentosGenerados_TrabajadorId",
                table: "DocumentosGenerados");

            migrationBuilder.DropColumn(
                name: "TrabajadorId",
                table: "DocumentosGenerados");

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 11, 30, 11, 3, 14, 160, DateTimeKind.Local).AddTicks(5426));
        }
    }
}
