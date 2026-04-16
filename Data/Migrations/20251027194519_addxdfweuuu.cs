using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMG.Data.Migrations
{
    /// <inheritdoc />
    public partial class addxdfweuuu : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Derivaciones",
                columns: table => new
                {
                    IdDerivacion = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdCitaOrigen = table.Column<int>(type: "INTEGER", nullable: false),
                    EspecialidadDestino = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    IdMedicoDestino = table.Column<int>(type: "INTEGER", nullable: true),
                    MotivoDerivacion = table.Column<string>(type: "TEXT", maxLength: 300, nullable: false),
                    FechaDerivacion = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EstadoDerivacion = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Derivaciones", x => x.IdDerivacion);
                    table.ForeignKey(
                        name: "FK_Derivaciones_Citas_IdCitaOrigen",
                        column: x => x.IdCitaOrigen,
                        principalTable: "Citas",
                        principalColumn: "IdCita",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Derivaciones_Medicos_IdMedicoDestino",
                        column: x => x.IdMedicoDestino,
                        principalTable: "Medicos",
                        principalColumn: "IdMedico");
                });

            migrationBuilder.CreateIndex(
                name: "IX_Derivaciones_IdCitaOrigen",
                table: "Derivaciones",
                column: "IdCitaOrigen");

            migrationBuilder.CreateIndex(
                name: "IX_Derivaciones_IdMedicoDestino",
                table: "Derivaciones",
                column: "IdMedicoDestino");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Derivaciones");
        }
    }
}
