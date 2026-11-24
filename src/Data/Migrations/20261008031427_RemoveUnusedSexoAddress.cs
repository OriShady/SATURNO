using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class RemoveUnusedSexoAddress : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'direcciones_usuario', N'SexoId') IS NOT NULL
                BEGIN
                    IF OBJECT_ID(N'FK_direcciones_usuario_cat_sexo_SexoId', N'F') IS NOT NULL
                        ALTER TABLE [direcciones_usuario] DROP CONSTRAINT [FK_direcciones_usuario_cat_sexo_SexoId];

                    IF EXISTS (
                        SELECT 1
                        FROM sys.indexes
                        WHERE [name] = N'IX_direcciones_usuario_SexoId'
                          AND [object_id] = OBJECT_ID(N'direcciones_usuario'))
                        DROP INDEX [IX_direcciones_usuario_SexoId] ON [direcciones_usuario];

                    ALTER TABLE [direcciones_usuario] DROP COLUMN [SexoId];
                END
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SexoId",
                table: "direcciones_usuario",
                type: "char(1)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_direcciones_usuario_SexoId",
                table: "direcciones_usuario",
                column: "SexoId");

            migrationBuilder.AddForeignKey(
                name: "FK_direcciones_usuario_cat_sexo_SexoId",
                table: "direcciones_usuario",
                column: "SexoId",
                principalTable: "cat_sexo",
                principalColumn: "id_sexo");
        }
    }
}
