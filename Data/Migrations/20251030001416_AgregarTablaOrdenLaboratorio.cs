using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMG.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaOrdenLaboratorio : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "OrdenesLaboratorio",
                columns: table => new
                {
                    IdOrden = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdPaciente = table.Column<int>(type: "INTEGER", nullable: false),
                    IdMedico = table.Column<int>(type: "INTEGER", nullable: false),
                    NumeroOrden = table.Column<string>(type: "TEXT", nullable: false),
                    TipoExamen = table.Column<string>(type: "TEXT", nullable: false),
                    ObservacionesAdicionales = table.Column<string>(type: "TEXT", nullable: false),
                    Resultados = table.Column<string>(type: "TEXT", nullable: false),
                    Estado = table.Column<string>(type: "TEXT", nullable: false),
                    FechaSolicitud = table.Column<DateTime>(type: "TEXT", nullable: false),
                    FechaResultado = table.Column<DateTime>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OrdenesLaboratorio", x => x.IdOrden);
                    table.ForeignKey(
                        name: "FK_OrdenesLaboratorio_Medicos_IdMedico",
                        column: x => x.IdMedico,
                        principalTable: "Medicos",
                        principalColumn: "IdMedico",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_OrdenesLaboratorio_Pacientes_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Pacientes",
                        principalColumn: "IdPaciente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesLaboratorio_IdMedico",
                table: "OrdenesLaboratorio",
                column: "IdMedico");

            migrationBuilder.CreateIndex(
                name: "IX_OrdenesLaboratorio_IdPaciente",
                table: "OrdenesLaboratorio",
                column: "IdPaciente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "OrdenesLaboratorio");
        }
    }
}
