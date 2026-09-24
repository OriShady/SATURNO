using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class ReconstruccionLimpia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    idcategoria = table.Column<int>(name: "id_categoria", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: true),
                    estatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fechacreacion = table.Column<DateTime>(name: "fecha_creacion", type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.idcategoria);
                });

            migrationBuilder.CreateTable(
                name: "instructores",
                columns: table => new
                {
                    idinstructor = table.Column<int>(name: "id_instructor", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    apellido = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true, defaultValue: ""),
                    email = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true, defaultValue: ""),
                    estatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fechacreacion = table.Column<DateTime>(name: "fecha_creacion", type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instructores", x => x.idinstructor);
                });

            migrationBuilder.CreateTable(
                name: "niveles",
                columns: table => new
                {
                    idnivel = table.Column<int>(name: "id_nivel", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    estatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fechacreacion = table.Column<DateTime>(name: "fecha_creacion", type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_niveles", x => x.idnivel);
                });

            migrationBuilder.CreateTable(
                name: "cursos",
                columns: table => new
                {
                    idcurso = table.Column<int>(name: "id_curso", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idcategoria = table.Column<int>(name: "id_categoria", type: "int", nullable: false),
                    idnivel = table.Column<int>(name: "id_nivel", type: "int", nullable: false),
                    idinstructor = table.Column<int>(name: "id_instructor", type: "int", nullable: false),
                    nombre = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    duracionminutos = table.Column<long>(name: "duracion_minutos", type: "bigint", nullable: false),
                    fechapublicacion = table.Column<DateTime>(name: "fecha_publicacion", type: "datetime2", nullable: false),
                    estatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    fechacreacion = table.Column<DateTime>(name: "fecha_creacion", type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cursos", x => x.idcurso);
                    table.CheckConstraint("chk_cursos_duracion", "duracion_minutos > 0");
                    table.CheckConstraint("chk_cursos_precio", "precio > 0");
                    table.ForeignKey(
                        name: "fk_cursos_categoria",
                        column: x => x.idcategoria,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cursos_instructor",
                        column: x => x.idinstructor,
                        principalTable: "instructores",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cursos_nivel",
                        column: x => x.idnivel,
                        principalTable: "niveles",
                        principalColumn: "id_nivel",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.InsertData(
                table: "categorias",
                columns: new[] { "id_categoria", "descripcion", "estatus", "fecha_creacion", "nombre" },
                values: new object[,]
                {
                    { 1, "Cursos relacionados con frontend, backend y aplicaciones web.", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Desarrollo Web" },
                    { 2, "Cursos sobre SQL, NoSQL, modelado y administración de datos.", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Bases de Datos" },
                    { 3, "Cursos de diseño gráfico, UX/UI y herramientas creativas.", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Diseño Digital" },
                    { 4, "Cursos sobre emprendimiento, administración y marketing.", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Negocios" },
                    { 5, "Cursos de seguridad informática, redes y protección de sistemas.", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Ciberseguridad" }
                });

            migrationBuilder.InsertData(
                table: "instructores",
                columns: new[] { "id_instructor", "email", "estatus", "fecha_creacion", "nombre" },
                values: new object[,]
                {
                    { 1, "laura.mendoza@email.com", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Laura Mendoza" },
                    { 2, "carlos.ramirez@email.com", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Carlos Ramírez" },
                    { 3, "andrea.torres@email.com", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Andrea Torres" },
                    { 4, "miguel.hernandez@email.com", "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Miguel Hernández" }
                });

            migrationBuilder.InsertData(
                table: "niveles",
                columns: new[] { "id_nivel", "estatus", "fecha_creacion", "nombre" },
                values: new object[,]
                {
                    { 1, "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Básico" },
                    { 2, "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Intermedio" },
                    { 3, "activo", new DateTime(2026, 5, 1, 0, 0, 0, 0, DateTimeKind.Unspecified), "Avanzado" }
                });

            migrationBuilder.InsertData(
                table: "cursos",
                columns: new[] { "id_curso", "id_categoria", "descripcion", "duracion_minutos", "estatus", "fecha_creacion", "fecha_publicacion", "id_instructor", "id_nivel", "nombre", "precio" },
                values: new object[,]
                {
                    { 1, 1, "Curso introductorio para crear páginas web modernas.", 300L, "activo", new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 10, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 1, "HTML, CSS y JavaScript desde cero", 499.00m },
                    { 2, 1, "Aprende a crear sistemas web usando PHP y bases de datos MySQL.", 420L, "activo", new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 11, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 2, "PHP y MySQL para aplicaciones web", 799.00m },
                    { 3, 2, "Curso básico para aprender consultas, tablas, relaciones y filtros en SQL.", 240L, "activo", new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 12, 0, 0, 0, 0, DateTimeKind.Unspecified), 3, 1, "Fundamentos de SQL", 399.00m },
                    { 4, 2, "Aprende documentos, colecciones, filtros y modelado NoSQL.", 360L, "activo", new DateTime(2026, 5, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 13, 0, 0, 0, 0, DateTimeKind.Unspecified), 4, 2, "MongoDB para principiantes", 599.00m },
                    { 5, 3, "Diseña interfaces atractivas y fáciles de usar.", 390L, "activo", new DateTime(2026, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 14, 0, 0, 0, 0, DateTimeKind.Unspecified), 1, 2, "UX/UI para interfaces modernas", 699.00m },
                    { 6, 5, "Introducción a amenazas, contraseñas, redes y buenas prácticas de seguridad.", 330L, "inactivo", new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), new DateTime(2026, 5, 15, 0, 0, 0, 0, DateTimeKind.Unspecified), 2, 1, "Ciberseguridad básica", 549.00m }
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                table: "categorias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_cursos_id_categoria",
                table: "cursos",
                column: "id_categoria");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_id_instructor",
                table: "cursos",
                column: "id_instructor");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_id_nivel",
                table: "cursos",
                column: "id_nivel");

            migrationBuilder.CreateIndex(
                name: "IX_instructores_nombre",
                table: "instructores",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_niveles_nombre",
                table: "niveles",
                column: "nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cursos");

            migrationBuilder.DropTable(
                name: "categorias");

            migrationBuilder.DropTable(
                name: "instructores");

            migrationBuilder.DropTable(
                name: "niveles");
        }
    }
}
