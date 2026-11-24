using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTelefonoEmergencia : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.usuarios', N'telefono_emergencia') IS NULL
                    ALTER TABLE dbo.usuarios
                    ADD telefono_emergencia VARCHAR(20) NULL;
                """);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql(
                """
                IF COL_LENGTH(N'dbo.usuarios', N'telefono_emergencia') IS NOT NULL
                    ALTER TABLE dbo.usuarios DROP COLUMN telefono_emergencia;
                """);
        }
    }
}
