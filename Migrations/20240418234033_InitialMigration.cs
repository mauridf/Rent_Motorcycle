using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace Rent_Motorcycle.Migrations
{
    /// <inheritdoc />
    public partial class InitialMigration : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "public");

            migrationBuilder.CreateTable(
                name: "Entregadores",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    CNPJ = table.Column<string>(type: "character varying(14)", maxLength: 14, nullable: false),
                    DataNascimento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CNH = table.Column<string>(type: "character varying(20)", maxLength: 20, nullable: false),
                    TipoCNH = table.Column<string>(type: "text", nullable: false),
                    ImagemCNH = table.Column<byte[]>(type: "bytea", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Entregadores", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Motos",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Ano = table.Column<int>(type: "integer", nullable: false),
                    Modelo = table.Column<string>(type: "text", nullable: false),
                    Placa = table.Column<string>(type: "character varying(7)", maxLength: 7, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Motos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "TiposPlanos",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nome = table.Column<string>(type: "text", nullable: false),
                    Dias = table.Column<int>(type: "integer", nullable: false),
                    Custo = table.Column<decimal>(type: "numeric", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TiposPlanos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Locacoes",
                schema: "public",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DataInicio = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataTermino = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    DataPrevistaTermino = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    TipoPlanoId = table.Column<int>(type: "integer", nullable: false),
                    MotoId = table.Column<int>(type: "integer", nullable: false),
                    EntregadorId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Locacoes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Locacoes_Entregadores_EntregadorId",
                        column: x => x.EntregadorId,
                        principalSchema: "public",
                        principalTable: "Entregadores",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Locacoes_Motos_MotoId",
                        column: x => x.MotoId,
                        principalSchema: "public",
                        principalTable: "Motos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Locacoes_TiposPlanos_TipoPlanoId",
                        column: x => x.TipoPlanoId,
                        principalSchema: "public",
                        principalTable: "TiposPlanos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.InsertData(
                schema: "public",
                table: "TiposPlanos",
                columns: new[] { "Id", "Custo", "Dias", "Nome" },
                values: new object[,]
                {
                    { 1, 30m, 7, "Plano 7 Dias" },
                    { 2, 28m, 15, "Plano 15 Dias" },
                    { 3, 22m, 30, "Plano 30 Dias" },
                    { 4, 20m, 45, "Plano 45 Dias" },
                    { 5, 18m, 50, "Plano 50 Dias" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Entregadores_CNH",
                schema: "public",
                table: "Entregadores",
                column: "CNH",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Entregadores_CNPJ",
                schema: "public",
                table: "Entregadores",
                column: "CNPJ",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Locacoes_EntregadorId",
                schema: "public",
                table: "Locacoes",
                column: "EntregadorId");

            migrationBuilder.CreateIndex(
                name: "IX_Locacoes_MotoId",
                schema: "public",
                table: "Locacoes",
                column: "MotoId");

            migrationBuilder.CreateIndex(
                name: "IX_Locacoes_TipoPlanoId",
                schema: "public",
                table: "Locacoes",
                column: "TipoPlanoId");

            migrationBuilder.CreateIndex(
                name: "IX_Motos_Placa",
                schema: "public",
                table: "Motos",
                column: "Placa",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Locacoes",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Entregadores",
                schema: "public");

            migrationBuilder.DropTable(
                name: "Motos",
                schema: "public");

            migrationBuilder.DropTable(
                name: "TiposPlanos",
                schema: "public");
        }
    }
}
