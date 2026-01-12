using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddValidacionIADocumentos : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "AprobadoPorIA",
                table: "DocumentosGenerados",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "ResumenValidacionIA",
                table: "DocumentosGenerados",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ScoreCumplimiento",
                table: "DocumentosGenerados",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2026, 1, 5, 13, 3, 5, 884, DateTimeKind.Local).AddTicks(9271));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AprobadoPorIA",
                table: "DocumentosGenerados");

            migrationBuilder.DropColumn(
                name: "ResumenValidacionIA",
                table: "DocumentosGenerados");

            migrationBuilder.DropColumn(
                name: "ScoreCumplimiento",
                table: "DocumentosGenerados");

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 30, 17, 10, 44, 947, DateTimeKind.Local).AddTicks(2663));
        }
    }
}
