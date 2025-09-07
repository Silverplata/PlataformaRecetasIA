using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PlataformaRecetasIA.Migrations
{
    /// <inheritdoc />
    public partial class AddCanUseAIToUsuario : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "CanUseAI",
                table: "Usuarios",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CanUseAI",
                table: "Usuarios");
        }
    }
}
