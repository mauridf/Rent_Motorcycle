using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rent_Motorcycle.Migrations
{
    /// <inheritdoc />
    public partial class UpdateIncludeValorLocacao : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "ValorLocacao",
                schema: "public",
                table: "Locacoes",
                type: "numeric",
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ValorLocacao",
                schema: "public",
                table: "Locacoes");
        }
    }
}
