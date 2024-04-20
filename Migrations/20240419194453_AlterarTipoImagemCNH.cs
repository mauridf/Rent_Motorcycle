using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Rent_Motorcycle.Migrations
{
    /// <inheritdoc />
    public partial class AlterarTipoImagemCNH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemCNH",
                schema: "public",
                table: "Entregadores");

            migrationBuilder.AddColumn<string>(
                name: "ImagemCNHUrl",
                schema: "public",
                table: "Entregadores",
                type: "text",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagemCNHUrl",
                schema: "public",
                table: "Entregadores");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImagemCNH",
                schema: "public",
                table: "Entregadores",
                type: "bytea",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
