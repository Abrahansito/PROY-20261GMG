using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGMG.Models
{
    public class Diagnostico
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdDiagnostico { get; set; }

        [ForeignKey(nameof(Paciente))]
        public int IdPaciente { get; set; }

        [ForeignKey(nameof(Medico))]
        public int IdMedico { get; set; }

        [ForeignKey(nameof(Cita))]
        public int IdCita { get; set; }

        public string DiagnosticoPrincipal { get; set; } = ""; // Ej: "Hipertensión arterial esencial"
        public string CodigoCie10 { get; set; } = ""; // Código CIE-10 (Ej: "I10")
        public string TratamientoEspecifico { get; set; } = "";
        public string ObservacionesMedicas { get; set; } = "";
        public DateTime FechaDiagnostico { get; set; }
        public TimeSpan HoraDiagnostico { get; set; }

        // Relaciones
        public Paciente Paciente { get; set; } = null!;
        public Medico Medico { get; set; } = null!;
        public Cita Cita { get; set; } = null!;
    }
}