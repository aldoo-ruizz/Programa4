using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SistemaUsuarios.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTiposUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "TiposUsuario",
                columns: table => new
                {
                    TipoUsuarioId = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    Nombre = table.Column<string>(type: "TEXT", maxLength: 60, nullable: false, collation: "NOCASE"),
                    Descripcion = table.Column<string>(type: "TEXT", maxLength: 250, nullable: true),
                    Activo = table.Column<bool>(type: "INTEGER", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposUsuario", x => x.TipoUsuarioId);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TiposUsuario_Nombre",
                table: "TiposUsuario",
                column: "Nombre",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "TiposUsuario");
        }
    }
}
