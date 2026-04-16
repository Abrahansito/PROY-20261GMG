using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SGMG.Data.Migrations
{
    /// <inheritdoc />
    public partial class AgregarTablaDiagnostico : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Diagnosticos",
                columns: table => new
                {
                    IdDiagnostico = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    IdPaciente = table.Column<int>(type: "INTEGER", nullable: false),
                    IdMedico = table.Column<int>(type: "INTEGER", nullable: false),
                    IdCita = table.Column<int>(type: "INTEGER", nullable: false),
                    DiagnosticoPrincipal = table.Column<string>(type: "TEXT", nullable: false),
                    CodigoCie10 = table.Column<string>(type: "TEXT", nullable: false),
                    TratamientoEspecifico = table.Column<string>(type: "TEXT", nullable: false),
                    ObservacionesMedicas = table.Column<string>(type: "TEXT", nullable: false),
                    FechaDiagnostico = table.Column<DateTime>(type: "TEXT", nullable: false),
                    HoraDiagnostico = table.Column<TimeSpan>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Diagnosticos", x => x.IdDiagnostico);
                    table.ForeignKey(
                        name: "FK_Diagnosticos_Citas_IdCita",
                        column: x => x.IdCita,
                        principalTable: "Citas",
                        principalColumn: "IdCita",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Diagnosticos_Medicos_IdMedico",
                        column: x => x.IdMedico,
                        principalTable: "Medicos",
                        principalColumn: "IdMedico",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_Diagnosticos_Pacientes_IdPaciente",
                        column: x => x.IdPaciente,
                        principalTable: "Pacientes",
                        principalColumn: "IdPaciente",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosticos_IdCita",
                table: "Diagnosticos",
                column: "IdCita");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosticos_IdMedico",
                table: "Diagnosticos",
                column: "IdMedico");

            migrationBuilder.CreateIndex(
                name: "IX_Diagnosticos_IdPaciente",
                table: "Diagnosticos",
                column: "IdPaciente");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Diagnosticos");
        }
    }
}
