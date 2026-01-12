using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSusesoAndJerarquia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CodigoSuseso",
                table: "Peligros",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Jerarquia",
                table: "Controles",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 1,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 2,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 3,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 4,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 5,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 6,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 7,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 8,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 9,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 10,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 11,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 12,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 13,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 14,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 15,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 16,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 17,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 18,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 19,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 20,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "Controles",
                keyColumn: "Id",
                keyValue: 21,
                column: "Jerarquia",
                value: 4);

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 11, 26, 22, 37, 59, 726, DateTimeKind.Local).AddTicks(1499));

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 1,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 2,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 3,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 4,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 5,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 6,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 7,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 8,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 9,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 10,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 11,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 12,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 13,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 14,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 15,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 16,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 17,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 18,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 19,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 20,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 21,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 22,
                column: "CodigoSuseso",
                value: null);

            migrationBuilder.UpdateData(
                table: "Peligros",
                keyColumn: "Id",
                keyValue: 23,
                column: "CodigoSuseso",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CodigoSuseso",
                table: "Peligros");

            migrationBuilder.DropColumn(
                name: "Jerarquia",
                table: "Controles");

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 11, 26, 22, 32, 9, 539, DateTimeKind.Local).AddTicks(4711));
        }
    }
}
