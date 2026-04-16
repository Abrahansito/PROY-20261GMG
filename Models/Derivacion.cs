using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PROY_20252SGMG.Models;

namespace SGMG.Models
{
  public class Derivacion
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDerivacion { get; set; }

    // 🔗 Relación con Cita (atención desde la cual se deriva)
    [Required]
    [ForeignKey("Cita")]
    public int IdCitaOrigen { get; set; }
    public Cita? Cita { get; set; }

    // 🩺 Especialidad de destino (sin tabla relacionada)
    [Required]
    [StringLength(100)]
    public string EspecialidadDestino { get; set; } = string.Empty;

    // (Opcional) Médico al que se deriva
    [ForeignKey("MedicoDestino")]
    public int? IdMedicoDestino { get; set; }
    public Medico? MedicoDestino { get; set; }

    // 📝 Motivo de la derivación
    [Required]
    [StringLength(300)]
    public string MotivoDerivacion { get; set; } = string.Empty;

    // 📅 Fecha de la derivación
    public DateTime FechaDerivacion { get; set; } = DateTime.Now;

    // 🔄 Estado: Pendiente, Atendida, Cancelada
    [Required]
    [StringLength(20)]
    [RegularExpression("^(Pendiente|Atendida|Cancelada)$",
        ErrorMessage = "El estado debe ser Pendiente, Atendida o Cancelada.")]
    public string EstadoDerivacion { get; set; } = "Pendiente";
  }
}
