using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RegistrarCapacidadDiferente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "cat_capacidades_diferentes",
                columns: table => new
                {
                    idcapacidad = table.Column<int>(name: "id_capacidad", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    descripcion = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cat_capacidades_diferentes", x => x.idcapacidad);
                });

            migrationBuilder.InsertData(
                table: "cat_capacidades_diferentes",
                columns: new[] { "id_capacidad", "descripcion" },
                values: new object[,]
                {
                    { 1, "Ninguna" },
                    { 2, "Visual" },
                    { 3, "Auditiva" },
                    { 4, "Motriz" },
                    { 5, "Intelectual" }
                });

            migrationBuilder.AddColumn<int>(
                name: "id_capacidad",
                table: "usuarios",
                type: "int",
                nullable: true);

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

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_capacidad",
                table: "usuarios",
                column: "id_capacidad");

            migrationBuilder.CreateIndex(
                name: "IX_cat_capacidades_diferentes_descripcion",
                table: "cat_capacidades_diferentes",
                column: "descripcion",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_usuarios_capacidad_diferente",
                table: "usuarios",
                column: "id_capacidad",
                principalTable: "cat_capacidades_diferentes",
                principalColumn: "id_capacidad",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuarios_capacidad_diferente",
                table: "usuarios");

            migrationBuilder.DropTable(
                name: "cat_capacidades_diferentes");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_id_capacidad",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "id_capacidad",
                table: "usuarios");
        }
    }
}
