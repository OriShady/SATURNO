using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class PermitirCapacidadNulaEnUsuariosExistentes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "id_capacidad",
                table: "usuarios",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.Sql(
                "UPDATE [usuarios] SET [id_capacidad] = NULL WHERE [id_capacidad] IS NOT NULL;");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                "UPDATE [usuarios] SET [id_capacidad] = 1 WHERE [id_capacidad] IS NULL;");

            migrationBuilder.AlterColumn<int>(
                name: "id_capacidad",
                table: "usuarios",
                type: "int",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);
        }
    }
}
