using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace TagleLabsGestorSST.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddRubroToEmpresa : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Trabajadores_CentrosTrabajo_CentroTrabajoId",
                table: "Trabajadores");

            migrationBuilder.AlterColumn<int>(
                name: "CentroTrabajoId",
                table: "Trabajadores",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "INTEGER",
                oldNullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AFP",
                table: "Trabajadores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Comuna",
                table: "Trabajadores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Direccion",
                table: "Trabajadores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EstadoCivil",
                table: "Trabajadores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaCreacion",
                table: "Trabajadores",
                type: "TEXT",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified));

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaImportacion",
                table: "Trabajadores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<DateTime>(
                name: "FechaNacimiento",
                table: "Trabajadores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FotoPath",
                table: "Trabajadores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SistemaSalud",
                table: "Trabajadores",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "RubroId",
                table: "Empresas",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "KnowledgeItems",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    SourceFile = table.Column<string>(type: "TEXT", maxLength: 500, nullable: false),
                    Category = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    FileType = table.Column<string>(type: "TEXT", maxLength: 50, nullable: false),
                    Content = table.Column<string>(type: "TEXT", nullable: false),
                    Tags = table.Column<string>(type: "TEXT", nullable: true),
                    ProcessedDate = table.Column<DateTime>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_KnowledgeItems", x => x.Id);
                });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Descripcion", "Nombre", "RubroId" },
                values: new object[] { "Extracción de material rocoso", "Perforación y Tronadura", 3 });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Descripcion", "Nombre", "RubroId" },
                values: new object[] { "Chancado y molienda", "Procesamiento de Mineral", 3 });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Descripcion", "Nombre", "RubroId" },
                values: new object[] { "Transporte de carga en carretera y ciudad", "Conducción de Vehículos", 4 });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Descripcion", "Nombre", "RubroId" },
                values: new object[] { "Almacenamiento y logística", "Operación de Bodega", 4 });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 10,
                column: "RubroId",
                value: 5);

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Descripcion", "RubroId" },
                values: new object[] { "Higiene de instalaciones", 5 });

            migrationBuilder.InsertData(
                table: "Actividades",
                columns: new[] { "Id", "Descripcion", "Nombre", "RubroId" },
                values: new object[] { 12, "Atención directa a pacientes", "Atención Clínica", 6 });

            migrationBuilder.UpdateData(
                table: "Empresas",
                keyColumn: "Id",
                keyValue: 1,
                column: "RubroId",
                value: null);

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 11, 29, 0, 15, 44, 50, DateTimeKind.Local).AddTicks(7824));

            migrationBuilder.UpdateData(
                table: "Rubros",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "SERV", "Oficinas, retail, educación, salud", "Servicios y Comercio" });

            migrationBuilder.UpdateData(
                table: "Rubros",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "SALUD", "Hospitales, clínicas y laboratorios", "Salud y Asistencia Social" });

            migrationBuilder.InsertData(
                table: "TareaPeligroControles",
                columns: new[] { "Id", "Aplicable", "Consecuencia", "ControlId", "NivelRiesgo", "Observaciones", "PeligroId", "Probabilidad", "TareaId" },
                values: new object[,]
                {
                    { 1, true, 2, 7, 4, null, 9, 2, 1 },
                    { 2, true, 2, 7, 4, null, 3, 2, 1 },
                    { 3, true, 2, 7, 4, null, 8, 2, 2 },
                    { 4, true, 2, 7, 4, null, 4, 2, 2 },
                    { 5, true, 2, 7, 4, null, 4, 2, 3 },
                    { 6, true, 2, 7, 4, null, 2, 2, 3 },
                    { 7, true, 2, 7, 4, null, 1, 2, 4 },
                    { 8, true, 2, 7, 4, null, 12, 2, 4 },
                    { 9, true, 2, 7, 4, null, 1, 2, 5 },
                    { 10, true, 2, 7, 4, null, 4, 2, 5 },
                    { 11, true, 2, 7, 4, null, 1, 2, 6 },
                    { 12, true, 2, 7, 4, null, 14, 2, 6 },
                    { 13, true, 2, 7, 4, null, 7, 2, 6 },
                    { 14, true, 2, 7, 4, null, 3, 2, 7 },
                    { 15, true, 2, 7, 4, null, 5, 2, 7 },
                    { 16, true, 2, 7, 4, null, 13, 2, 8 },
                    { 17, true, 2, 7, 4, null, 5, 2, 8 },
                    { 18, true, 2, 7, 4, null, 4, 2, 9 },
                    { 19, true, 2, 7, 4, null, 6, 2, 9 },
                    { 20, true, 2, 7, 4, null, 14, 2, 10 },
                    { 21, true, 2, 7, 4, null, 12, 2, 10 },
                    { 22, true, 2, 7, 4, null, 7, 2, 10 },
                    { 23, true, 2, 7, 4, null, 7, 2, 11 },
                    { 24, true, 2, 7, 4, null, 10, 2, 11 },
                    { 25, true, 2, 7, 4, null, 6, 2, 11 },
                    { 26, true, 2, 7, 4, null, 14, 2, 12 },
                    { 27, true, 2, 7, 4, null, 7, 2, 12 }
                });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 1, "Colocación de conos y letreros en vía pública", "Instalación de señalética vial" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Colocación de planchas de zinc sobre costaneras", "Instalación de techumbre" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 2, "Montaje de cuerpos de andamio", "Armado de andamios" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 2, "Unión de estructuras metálicas sobre nivel 1.8m", "Soldadura en altura" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 3, "Vaciado de hormigón mediante canoa o bomba", "Descarga de camión mixer" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 3, "Uso de sonda vibradora", "Vibrado de hormigón" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 3, "Retiro de moldajes metálicos o madera", "Descimbre de muros" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 4, "Unión de piezas con electrodo revestido", "Soldadura al arco manual" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 4, "Uso de galletera angular", "Esmerilado de uniones" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 4, "Uso de soplete con oxígeno y gas", "Corte con oxicorte" });

            migrationBuilder.InsertData(
                table: "Tareas",
                columns: new[] { "Id", "ActividadId", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 13, 5, "Aplicación de grasa en partes móviles", "Lubricación de rodamientos" },
                    { 14, 5, "Sustitución de bandas de goma", "Cambio de correas transportadoras" },
                    { 15, 5, "Aspirado y reapriete de conexiones", "Limpieza de tableros eléctricos" },
                    { 16, 6, "Manipulación de anfo y detonadores", "Carguío de explosivos" },
                    { 17, 6, "Perforación mecanizada en frente", "Operación de Jumbo" },
                    { 18, 7, "Control de alimentación de roca", "Operación de Chancador Primario" },
                    { 19, 7, "Cambio de revestimientos interiores", "Mantenimiento de Molino SAG" },
                    { 20, 8, "Transporte interurbano de carga", "Conducción de camión articulado" },
                    { 21, 8, "Aseguramiento de la carga con eslingas", "Estiba y desestiba de carga" },
                    { 22, 9, "Movimiento de pallets en altura", "Operación de Grúa Horquilla" },
                    { 23, 9, "Selección de productos en estanterías", "Picking manual" },
                    { 24, 10, "Uso intensivo de teclado y mouse", "Digitación de datos" },
                    { 25, 10, "Interacción en mesón de atención", "Atención de clientes presencial" },
                    { 26, 10, "Manejo manual de archivadores en altura", "Archivo de documentación" },
                    { 27, 11, "Uso de mopa y productos químicos", "Limpieza de pisos" },
                    { 28, 11, "Uso de andamios o escaleras", "Limpieza de vidrios en altura" }
                });

            migrationBuilder.InsertData(
                table: "TareaPeligroControles",
                columns: new[] { "Id", "Aplicable", "Consecuencia", "ControlId", "NivelRiesgo", "Observaciones", "PeligroId", "Probabilidad", "TareaId" },
                values: new object[,]
                {
                    { 28, true, 2, 7, 4, null, 3, 2, 13 },
                    { 29, true, 2, 7, 4, null, 3, 2, 14 },
                    { 30, true, 2, 7, 4, null, 18, 2, 14 },
                    { 31, true, 2, 7, 4, null, 5, 2, 15 },
                    { 32, true, 2, 7, 4, null, 11, 2, 16 },
                    { 33, true, 2, 7, 4, null, 10, 2, 16 },
                    { 34, true, 2, 7, 4, null, 10, 2, 17 },
                    { 35, true, 2, 7, 4, null, 13, 2, 17 },
                    { 36, true, 2, 7, 4, null, 11, 2, 18 },
                    { 37, true, 2, 7, 4, null, 10, 2, 18 },
                    { 38, true, 2, 7, 4, null, 1, 2, 19 },
                    { 39, true, 2, 7, 4, null, 3, 2, 19 },
                    { 40, true, 2, 7, 4, null, 1, 2, 21 },
                    { 41, true, 2, 7, 4, null, 4, 2, 21 },
                    { 42, true, 2, 7, 4, null, 9, 2, 22 },
                    { 43, true, 2, 7, 4, null, 18, 2, 23 },
                    { 44, true, 2, 7, 4, null, 2, 2, 23 },
                    { 45, true, 2, 7, 4, null, 20, 2, 24 },
                    { 46, true, 2, 7, 4, null, 19, 2, 24 },
                    { 47, true, 2, 7, 4, null, 21, 2, 25 },
                    { 48, true, 2, 7, 4, null, 1, 2, 26 },
                    { 49, true, 2, 7, 4, null, 18, 2, 26 },
                    { 50, true, 2, 7, 4, null, 2, 2, 27 },
                    { 51, true, 2, 7, 4, null, 1, 2, 28 }
                });

            migrationBuilder.InsertData(
                table: "Tareas",
                columns: new[] { "Id", "ActividadId", "Descripcion", "Nombre" },
                values: new object[,]
                {
                    { 29, 12, "Punción venosa", "Toma de muestras de sangre" },
                    { 30, 12, "Traslado de enfermos en cama o silla", "Movilización de pacientes" },
                    { 31, 12, "Retiro de desechos biológicos", "Manipulación de residuos REAS" }
                });

            migrationBuilder.InsertData(
                table: "TareaPeligroControles",
                columns: new[] { "Id", "Aplicable", "Consecuencia", "ControlId", "NivelRiesgo", "Observaciones", "PeligroId", "Probabilidad", "TareaId" },
                values: new object[] { 52, true, 2, 7, 4, null, 20, 2, 30 });

            migrationBuilder.CreateIndex(
                name: "IX_Empresas_RubroId",
                table: "Empresas",
                column: "RubroId");

            migrationBuilder.AddForeignKey(
                name: "FK_Empresas_Rubros_RubroId",
                table: "Empresas",
                column: "RubroId",
                principalTable: "Rubros",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Trabajadores_CentrosTrabajo_CentroTrabajoId",
                table: "Trabajadores",
                column: "CentroTrabajoId",
                principalTable: "CentrosTrabajo",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Empresas_Rubros_RubroId",
                table: "Empresas");

            migrationBuilder.DropForeignKey(
                name: "FK_Trabajadores_CentrosTrabajo_CentroTrabajoId",
                table: "Trabajadores");

            migrationBuilder.DropTable(
                name: "KnowledgeItems");

            migrationBuilder.DropIndex(
                name: "IX_Empresas_RubroId",
                table: "Empresas");

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 1);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 2);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 3);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 4);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 5);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 32);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 33);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 34);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 35);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 36);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 37);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 38);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 39);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 40);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 41);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 42);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 43);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 44);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 45);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 46);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 47);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 48);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 49);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 50);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 51);

            migrationBuilder.DeleteData(
                table: "TareaPeligroControles",
                keyColumn: "Id",
                keyValue: 52);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 29);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 31);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 23);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 24);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 25);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 26);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 27);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 28);

            migrationBuilder.DeleteData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 30);

            migrationBuilder.DeleteData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DropColumn(
                name: "AFP",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "Comuna",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "Direccion",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "EstadoCivil",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "FechaCreacion",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "FechaImportacion",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "FechaNacimiento",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "FotoPath",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "SistemaSalud",
                table: "Trabajadores");

            migrationBuilder.DropColumn(
                name: "RubroId",
                table: "Empresas");

            migrationBuilder.AlterColumn<int>(
                name: "CentroTrabajoId",
                table: "Trabajadores",
                type: "INTEGER",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "INTEGER");

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Descripcion", "Nombre", "RubroId" },
                values: new object[] { "Transporte de carga en carretera y ciudad", "Conducción de Vehículos", 4 });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "Descripcion", "Nombre", "RubroId" },
                values: new object[] { "Movimiento de carga en bodegas", "Operación de Grúa Horquilla", 4 });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "Descripcion", "Nombre", "RubroId" },
                values: new object[] { "Uso de plaguicidas y pesticidas", "Aplicación de Fitosanitarios", 5 });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "Descripcion", "Nombre", "RubroId" },
                values: new object[] { "Recolección de fruta y vegetales", "Cosecha Manual", 5 });

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 10,
                column: "RubroId",
                value: 6);

            migrationBuilder.UpdateData(
                table: "Actividades",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "Descripcion", "RubroId" },
                values: new object[] { "Uso de productos químicos de limpieza", 6 });

            migrationBuilder.UpdateData(
                table: "LogsAuditoria",
                keyColumn: "Id",
                keyValue: 1,
                column: "Fecha",
                value: new DateTime(2025, 11, 28, 9, 21, 32, 894, DateTimeKind.Local).AddTicks(152));

            migrationBuilder.UpdateData(
                table: "Rubros",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "AGRO", "Cultivos, cosecha y packing", "Agricultura" });

            migrationBuilder.UpdateData(
                table: "Rubros",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "Codigo", "Descripcion", "Nombre" },
                values: new object[] { "SERV", "Oficinas, retail, educación, salud", "Servicios y Comercio" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 3,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 2, "Colocación de planchas de zinc sobre costaneras", "Instalación de techumbre" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 4,
                columns: new[] { "Descripcion", "Nombre" },
                values: new object[] { "Montaje de cuerpos de andamio", "Armado de andamios" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 5,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 4, "Unión de piezas con electrodo revestido", "Soldadura al arco manual" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 6,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 4, "Uso de galletera angular", "Esmerilado de uniones" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 7,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 7, "Operación de grúa horquilla en patio de carga", "Carga y descarga de camiones" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 8,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 7, "Almacenamiento en altura", "Apilamiento en racks" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 9,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 8, "Dosificación de plaguicidas", "Preparación de mezcla" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 10,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 8, "Aplicación mecanizada en huerto", "Pulverización con tractor" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 11,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 10, "Uso intensivo de teclado y mouse", "Digitación de datos" });

            migrationBuilder.UpdateData(
                table: "Tareas",
                keyColumn: "Id",
                keyValue: 12,
                columns: new[] { "ActividadId", "Descripcion", "Nombre" },
                values: new object[] { 10, "Interacción presencial con público", "Atención de clientes" });

            migrationBuilder.AddForeignKey(
                name: "FK_Trabajadores_CentrosTrabajo_CentroTrabajoId",
                table: "Trabajadores",
                column: "CentroTrabajoId",
                principalTable: "CentrosTrabajo",
                principalColumn: "Id");
        }
    }
}
