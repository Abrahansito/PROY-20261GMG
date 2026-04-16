using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGMG.Models
{
    public class OrdenLaboratorio
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int IdOrden { get; set; }

        [ForeignKey(nameof(Paciente))]
        public int IdPaciente { get; set; }

        [ForeignKey(nameof(Medico))]
        public int IdMedico { get; set; }

        public string NumeroOrden { get; set; } = ""; // Ej: #ORD001

        public string TipoExamen { get; set; } = ""; // Análisis de Sangre, Examen Microbiológico, etc.

        public string ObservacionesAdicionales { get; set; } = "";

        public string Resultados { get; set; } = ""; // Resultados del examen (cuando esté completo)

        public string Estado { get; set; } = "Pendiente"; // Pendiente, Realizado, Cancelado

        public DateTime FechaSolicitud { get; set; }

        public DateTime? FechaResultado { get; set; } // Cuando se completa el examen

        // Relaciones
        public Paciente Paciente { get; set; } = null!;
        public Medico Medico { get; set; } = null!;
    }
}