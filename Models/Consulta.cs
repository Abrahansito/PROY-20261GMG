using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGMG.Models
{
    public class Consulta
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdConsulta { get; set; }

        [ForeignKey(nameof(Paciente))]
        public int IdPaciente { get; set; }

        [ForeignKey(nameof(Medico))]
        public int IdMedico { get; set; }

        [ForeignKey(nameof(Cita))]
        public int? IdCita { get; set; }

        // Síntomas
        public string MotivoConsulta { get; set; } = "";
        public string SintomasPresentados { get; set; } = "";

        // Diagnóstico
        public string DiagnosticoPrincipal { get; set; } = "";
        public string CodigoCie10 { get; set; } = "";
        public string Observaciones { get; set; } = "";

        // Evolución
        public string DescripcionEvolucion { get; set; } = "";
        public string IndicacionesRecomendaciones { get; set; } = "";

        public DateTime FechaConsulta { get; set; }
        public TimeSpan HoraConsulta { get; set; }

        // Relaciones
        public Paciente Paciente { get; set; } = null!;
        public Medico Medico { get; set; } = null!;
        public Cita? Cita { get; set; }
    }
}