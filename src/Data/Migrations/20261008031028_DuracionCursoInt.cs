using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class DuracionCursoInt : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_cursos_duracion",
                table: "cursos");

            migrationBuilder.AlterColumn<int>(
                name: "duracion_minutos",
                table: "cursos",
                type: "int",
                nullable: false,
                oldClrType: typeof(long),
                oldType: "bigint");

            migrationBuilder.AddCheckConstraint(
                name: "chk_cursos_duracion",
                table: "cursos",
                sql: "duracion_minutos > 0");

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 1,
                column: "duracion_minutos",
                value: 300);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 2,
                column: "duracion_minutos",
                value: 420);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 3,
                column: "duracion_minutos",
                value: 240);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 4,
                column: "duracion_minutos",
                value: 360);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 5,
                column: "duracion_minutos",
                value: 390);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 6,
                column: "duracion_minutos",
                value: 330);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "chk_cursos_duracion",
                table: "cursos");

            migrationBuilder.AlterColumn<long>(
                name: "duracion_minutos",
                table: "cursos",
                type: "bigint",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddCheckConstraint(
                name: "chk_cursos_duracion",
                table: "cursos",
                sql: "duracion_minutos > 0");

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 1,
                column: "duracion_minutos",
                value: 300L);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 2,
                column: "duracion_minutos",
                value: 420L);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 3,
                column: "duracion_minutos",
                value: 240L);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 4,
                column: "duracion_minutos",
                value: 360L);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 5,
                column: "duracion_minutos",
                value: 390L);

            migrationBuilder.UpdateData(
                table: "cursos",
                keyColumn: "id_curso",
                keyValue: 6,
                column: "duracion_minutos",
                value: 330L);
        }
    }
}
