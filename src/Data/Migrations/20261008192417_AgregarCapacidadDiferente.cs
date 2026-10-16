using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarCapacidadDiferente : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "id_capacidad_diferente",
                table: "usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "cat_capacidad_diferente",
                columns: table => new
                {
                    idcapacidaddiferente = table.Column<int>(name: "id_capacidad_diferente", type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    nombre = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_cat_capacidad_diferente", x => x.idcapacidaddiferente);
                });

            migrationBuilder.InsertData(
                table: "cat_capacidad_diferente",
                columns: new[] { "id_capacidad_diferente", "nombre" },
                values: new object[,]
                {
                    { 1, "Ninguna" },
                    { 2, "Visual" },
                    { 3, "Auditiva" },
                    { 4, "Motriz" },
                    { 5, "Intelectual" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_id_capacidad_diferente",
                table: "usuarios",
                column: "id_capacidad_diferente");

            migrationBuilder.CreateIndex(
                name: "IX_cat_capacidad_diferente_nombre",
                table: "cat_capacidad_diferente",
                column: "nombre",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "fk_usuarios_capacidad_diferente",
                table: "usuarios",
                column: "id_capacidad_diferente",
                principalTable: "cat_capacidad_diferente",
                principalColumn: "id_capacidad_diferente",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "fk_usuarios_capacidad_diferente",
                table: "usuarios");

            migrationBuilder.DropTable(
                name: "cat_capacidad_diferente");

            migrationBuilder.DropIndex(
                name: "IX_usuarios_id_capacidad_diferente",
                table: "usuarios");

            migrationBuilder.DropColumn(
                name: "id_capacidad_diferente",
                table: "usuarios");
        }
    }
}
