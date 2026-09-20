using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NoSqlU.Data.Migrations
{
    public partial class InitialCreate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "categorias",
                columns: table => new
                {
                    id_categoria = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: false),
                    descripcion = table.Column<string>(type: "varchar(255)", maxLength: 255, nullable: true),
                    estatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "activo"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_categorias", x => x.id_categoria);
                });

            migrationBuilder.CreateTable(
                name: "niveles",
                columns: table => new
                {
                    id_nivel = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(30)", maxLength: 30, nullable: false),
                    estatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "activo"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_niveles", x => x.id_nivel);
                });

            migrationBuilder.CreateTable(
                name: "instructores",
                columns: table => new
                {
                    id_instructor = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: false),
                    apellido = table.Column<string>(type: "varchar(100)", maxLength: 100, nullable: true, defaultValue: ""),
                    email = table.Column<string>(type: "varchar(150)", maxLength: 150, nullable: true, defaultValue: ""),
                    estatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "activo"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_instructores", x => x.id_instructor);
                });

            migrationBuilder.CreateTable(
                name: "cursos",
                columns: table => new
                {
                    id_curso = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    id_categoria = table.Column<int>(type: "int", nullable: false),
                    id_nivel = table.Column<int>(type: "int", nullable: false),
                    id_instructor = table.Column<int>(type: "int", nullable: false),
                    nombre = table.Column<string>(type: "varchar(200)", maxLength: 200, nullable: false),
                    descripcion = table.Column<string>(type: "text", nullable: false),
                    precio = table.Column<decimal>(type: "decimal(10,2)", nullable: false),
                    duracion_minutos = table.Column<int>(type: "int", nullable: false),
                    fecha_publicacion = table.Column<DateTime>(type: "date", nullable: false),
                    estatus = table.Column<string>(type: "varchar(20)", maxLength: 20, nullable: false, defaultValue: "activo"),
                    fecha_creacion = table.Column<DateTime>(type: "datetime2", nullable: false, defaultValueSql: "SYSUTCDATETIME()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cursos", x => x.id_curso);
                    table.ForeignKey(
                        name: "fk_cursos_categoria",
                        column: x => x.id_categoria,
                        principalTable: "categorias",
                        principalColumn: "id_categoria",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cursos_nivel",
                        column: x => x.id_nivel,
                        principalTable: "niveles",
                        principalColumn: "id_nivel",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cursos_instructor",
                        column: x => x.id_instructor,
                        principalTable: "instructores",
                        principalColumn: "id_instructor",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_categorias_nombre",
                table: "categorias",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_niveles_nombre",
                table: "niveles",
                column: "nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_instructores_nombre",
                table: "instructores",
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
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(name: "cursos");
            migrationBuilder.DropTable(name: "instructores");
            migrationBuilder.DropTable(name: "niveles");
            migrationBuilder.DropTable(name: "categorias");
        }
    }
}
