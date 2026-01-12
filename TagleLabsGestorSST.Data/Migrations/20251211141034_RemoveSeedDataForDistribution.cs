using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveSeedDataForDistribution : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "MatrizRiesgosEmpresa",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "MatrizRiesgosEmpresa",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "MatrizRiesgosEmpresa",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "MatrizRiesgosEmpresa",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "MatrizRiesgosEmpresa",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "MatrizRiesgosEmpresa",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "CentrosTrabajo",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 11, 11, 10, 33, 997, DateTimeKind.Local).AddTicks(6946));
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.InsertData(
                table: "Empresas",
                columns: new[] { "Id", "Direccion", "EmailContacto", "Giro", "LogoPath", "Mutual", "NumeroTrabajadores", "RazonSocial", "RepresentanteLegal", "RubroId", "RubroPrincipal", "Rut", "Telefono" },
                values: new object[] { 1, "Pedro de Miranda #183, Lo Miranda", "contacto@jgvalenzuela.cl", "Servicios de mantención y reparación industrial", null, "ACHS", 15, "JUAN GONZALEZ VALENZUELA SPA", "Juan González Valenzuela", null, "Industria Manufacturera", "76.825.693-4", "+56 9 1234 5678" });

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 12, 8, 11, 26, 46, 852, DateTimeKind.Local).AddTicks(3968));

            migrationBuilder.InsertData(
                table: "CentrosTrabajo",
                columns: new[] { "Id", "Ciudad", "Direccion", "EmpresaId", "Nombre", "PlanoRiesgosPath", "Region", "Sector" },
                values: new object[] { 1, "Donihue", "Pedro de Miranda #183, Lo Miranda", 1, "Taller Principal", null, "O'Higgins", "Operaciones" });

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
        }
    }
}
