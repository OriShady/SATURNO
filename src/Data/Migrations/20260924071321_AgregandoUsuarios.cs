using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregandoUsuarios : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cat_estados",
                columns: table => new
                {
                    idestado = table.Column<int>(name: "id_estado", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombreestado = table.Column<string>(name: "nombre_estado", type: "nvarchar(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cat_estados", x => x.idestado);
                });

            migrationBuilder.CreateTable(
                name: "cat_sexo",
                columns: table => new
                {
                    idsexo = table.Column<string>(name: "id_sexo", type: "char(1)", nullable: false),
                    descripcion = table.Column<string>(type: "nvarchar(30)", maxLength: 30, nullable: false),
                    estatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVO")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cat_sexo", x => x.idsexo);
                });

            migrationBuilder.CreateTable(
                name: "usuarios",
                columns: table => new
                {
                    idusuario = table.Column<int>(name: "id_usuario", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombres = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    apellidopaterno = table.Column<string>(name: "apellido_paterno", type: "nvarchar(50)", maxLength: 50, nullable: true),
                    apellidomaterno = table.Column<string>(name: "apellido_materno", type: "nvarchar(50)", maxLength: 50, nullable: true),
                    fechanacimiento = table.Column<DateTime>(name: "fecha_nacimiento", type: "date", nullable: false),
                    idsexo = table.Column<string>(name: "id_sexo", type: "char(1)", nullable: false),
                    correoempresarial = table.Column<string>(name: "correo_empresarial", type: "nvarchar(150)", maxLength: 150, nullable: false),
                    telefono = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: true),
                    fechainscripcion = table.Column<DateTime>(name: "fecha_inscripcion", type: "date", nullable: false),
                    fecharegistro = table.Column<DateTime>(name: "fecha_registro", type: "datetime2", nullable: false, defaultValueSql: "GETDATE()"),
                    estatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "ACTIVO"),
                    EstadoId = table.Column<int>(type: "int", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuarios", x => x.idusuario);
                    table.CheckConstraint("chk_apellidos", "apellido_paterno IS NOT NULL OR apellido_materno IS NOT NULL");
                    table.ForeignKey(
                        name: "FK_usuarios_cat_estados_EstadoId",
                        column: x => x.EstadoId,
                        principalTable: "cat_estados",
                        principalColumn: "id_estado");
                    table.ForeignKey(
                        name: "fk_usuarios_sexo",
                        column: x => x.idsexo,
                        principalTable: "cat_sexo",
                        principalColumn: "id_sexo",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "cursos_usuarios",
                columns: table => new
                {
                    idusuariocurso = table.Column<int>(name: "id_usuario_curso", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idusuario = table.Column<int>(name: "id_usuario", type: "int", nullable: false),
                    idcurso = table.Column<int>(name: "id_curso", type: "int", nullable: false),
                    fechainicio = table.Column<DateTime>(name: "fecha_inicio", type: "date", nullable: false),
                    fechafinalizacion = table.Column<DateTime>(name: "fecha_finalizacion", type: "date", nullable: true),
                    estatus = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false, defaultValue: "EN CURSO")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cursos_usuarios", x => x.idusuariocurso);
                    table.ForeignKey(
                        name: "fk_cursos_usuarios_curso",
                        column: x => x.idcurso,
                        principalTable: "cursos",
                        principalColumn: "id_curso",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_cursos_usuarios_usuario",
                        column: x => x.idusuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "direcciones_usuario",
                columns: table => new
                {
                    iddireccion = table.Column<int>(name: "id_direccion", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    idusuario = table.Column<int>(name: "id_usuario", type: "int", nullable: false),
                    idestado = table.Column<int>(name: "id_estado", type: "int", nullable: false),
                    calle = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    numeroexterior = table.Column<string>(name: "numero_exterior", type: "nvarchar(20)", maxLength: 20, nullable: false),
                    numerointerior = table.Column<string>(name: "numero_interior", type: "nvarchar(20)", maxLength: 20, nullable: true),
                    codigopostal = table.Column<string>(name: "codigo_postal", type: "nvarchar(10)", maxLength: 10, nullable: false),
                    colonia = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    municipio = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    SexoId = table.Column<string>(type: "char(1)", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_direcciones_usuario", x => x.iddireccion);
                    table.ForeignKey(
                        name: "FK_direcciones_usuario_cat_sexo_SexoId",
                        column: x => x.SexoId,
                        principalTable: "cat_sexo",
                        principalColumn: "id_sexo");
                    table.ForeignKey(
                        name: "fk_direccion_estado",
                        column: x => x.idestado,
                        principalTable: "cat_estados",
                        principalColumn: "id_estado",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "fk_direccion_usuario",
                        column: x => x.idusuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "usuario_contrasena",
                columns: table => new
                {
                    idusuario = table.Column<int>(name: "id_usuario", type: "int", nullable: false),
                    usuario = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    passwordhash = table.Column<string>(name: "password_hash", type: "nvarchar(255)", maxLength: 255, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_usuario_contrasena", x => x.idusuario);
                    table.ForeignKey(
                        name: "fk_usuario_contrasena",
                        column: x => x.idusuario,
                        principalTable: "usuarios",
                        principalColumn: "id_usuario",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                table: "cat_estados",
                columns: new[] { "id_estado", "nombre_estado" },
                values: new object[,]
                {
                    { 1, "Aguascalientes" },
                    { 2, "Baja California" },
                    { 3, "Baja California Sur" },
                    { 4, "Campeche" },
                    { 5, "Coahuila de Zaragoza" },
                    { 6, "Colima" },
                    { 7, "Chiapas" },
                    { 8, "Chihuahua" },
                    { 9, "Ciudad de México" },
                    { 10, "Durango" },
                    { 11, "Guanajuato" },
                    { 12, "Guerrero" },
                    { 13, "Hidalgo" },
                    { 14, "Jalisco" },
                    { 15, "Estado de México" },
                    { 16, "Michoacán de Ocampo" },
                    { 17, "Morelos" },
                    { 18, "Nayarit" },
                    { 19, "Nuevo León" },
                    { 20, "Oaxaca" },
                    { 21, "Puebla" },
                    { 22, "Querétaro" },
                    { 23, "Quintana Roo" },
                    { 24, "San Luis Potosí" },
                    { 25, "Sinaloa" },
                    { 26, "Sonora" },
                    { 27, "Tabasco" },
                    { 28, "Tamaulipas" },
                    { 29, "Tlaxcala" },
                    { 30, "Veracruz" },
                    { 31, "Yucatán" },
                    { 32, "Zacatecas" }
                });

            migrationBuilder.InsertData(
                table: "cat_sexo",
                columns: new[] { "id_sexo", "descripcion", "estatus" },
                values: new object[,]
                {
                    { "F", "Femenino", "ACTIVO" },
                    { "M", "Masculino", "ACTIVO" },
                    { "X", "No binario / Otro", "ACTIVO" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_cursos_usuarios_id_curso",
                table: "cursos_usuarios",
                column: "id_curso");

            migrationBuilder.CreateIndex(
                name: "IX_cursos_usuarios_id_usuario",
                table: "cursos_usuarios",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_direcciones_usuario_id_estado",
                table: "direcciones_usuario",
                column: "id_estado");

            migrationBuilder.CreateIndex(
                name: "IX_direcciones_usuario_id_usuario",
                table: "direcciones_usuario",
                column: "id_usuario");

            migrationBuilder.CreateIndex(
                name: "IX_direcciones_usuario_SexoId",
                table: "direcciones_usuario",
                column: "SexoId");

            migrationBuilder.CreateIndex(
                name: "IX_usuario_contrasena_usuario",
                table: "usuario_contrasena",
                column: "usuario",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_correo_empresarial",
                table: "usuarios",
                column: "correo_empresarial",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_EstadoId",
                table: "usuarios",
                column: "EstadoId");

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_sexo",
                table: "usuarios",
                column: "id_sexo");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "cursos_usuarios");

            migrationBuilder.DropTable(
                name: "direcciones_usuario");

            migrationBuilder.DropTable(
                name: "usuario_contrasena");

            migrationBuilder.DropTable(
                name: "usuarios");

            migrationBuilder.DropTable(
                name: "cat_estados");

            migrationBuilder.DropTable(
                name: "cat_sexo");
        }
    }
}
