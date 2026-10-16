using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RegistroCursosCertificados : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "id_curso",
                table: "cursos_usuarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_inicio",
                table: "cursos_usuarios",
                type: "date",
                nullable: true,
                oldClrType: typeof(DateTime),
                oldType: "date");

            migrationBuilder.AddColumn<string>(
                name: "institucion",
                table: "cursos_usuarios",
                type: "nvarchar(150)",
                maxLength: 150,
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "nombre_curso",
                table: "cursos_usuarios",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                defaultValue: "");

            migrationBuilder.Sql(
                """
                UPDATE cu
                SET cu.nombre_curso = c.nombre
                FROM cursos_usuarios AS cu
                INNER JOIN cursos AS c ON c.id_curso = cu.id_curso
                """);

            migrationBuilder.AddColumn<string>(
                name: "estatus",
                table: "cat_estados",
                type: "nvarchar(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 1,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 2,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 3,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 4,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 5,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 6,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 7,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 8,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 9,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 10,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 11,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 12,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 13,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 14,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 15,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 16,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 17,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 18,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 19,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 20,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 21,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 22,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 23,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 24,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 25,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 26,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 27,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 28,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 29,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 30,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 31,
                column: "estatus",
                value: "ACTIVO");

            migrationBuilder.UpdateData(
                table: "cat_estados",
                keyColumn: "id_estado",
                keyValue: 32,
                column: "estatus",
                value: "ACTIVO");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF EXISTS (
                    SELECT 1
                    FROM cursos_usuarios
                    WHERE id_curso IS NULL OR institucion IS NOT NULL
                )
                    THROW 51000, 'No se puede revertir la captura de certificados sin perder datos.', 1;
                """);

            migrationBuilder.DropColumn(
                name: "institucion",
                table: "cursos_usuarios");

            migrationBuilder.DropColumn(
                name: "nombre_curso",
                table: "cursos_usuarios");

            migrationBuilder.DropColumn(
                name: "estatus",
                table: "cat_estados");

            migrationBuilder.AlterColumn<int>(
                name: "id_curso",
                table: "cursos_usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AlterColumn<DateTime>(
                name: "fecha_inicio",
                table: "cursos_usuarios",
                type: "date",
                nullable: false,
                defaultValue: new DateTime(1, 1, 1, 0, 0, 0, 0, DateTimeKind.Unspecified),
                oldClrType: typeof(DateTime),
                oldType: "date",
                oldNullable: true);
        }
    }
}
