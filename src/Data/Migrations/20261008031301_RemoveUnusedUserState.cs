using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedUserState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'usuarios', N'EstadoId') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'FK_usuarios_cat_estados_EstadoId', N'F') IS NOT NULL
                        ALTER TABLE [usuarios] DROP CONSTRAINT [FK_usuarios_cat_estados_EstadoId];

                    IF EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE [name] = N'IX_usuarios_EstadoId'
                          AND [object_id] = OBJECT_ID(N'usuarios'))
                        DROP INDEX [IX_usuarios_EstadoId] ON [usuarios];

                    ALTER TABLE [usuarios] DROP COLUMN [EstadoId];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EstadoId",
                table: "usuarios",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_usuarios_EstadoId",
                table: "usuarios",
                column: "EstadoId");

            migrationBuilder.AddForeignKey(
                name: "FK_usuarios_cat_estados_EstadoId",
                table: "usuarios",
                column: "EstadoId",
                principalTable: "cat_estados",
                principalColumn: "id_estado");
        }
    }
}
