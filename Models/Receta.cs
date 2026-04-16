
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGMG.Models
{
  public class Receta
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdReceta { get; set; }

    [ForeignKey(nameof(Cita))]
    public int IdCita { get; set; }

    [ForeignKey(nameof(Medico))]
    public int IdMedico { get; set; }

    [ForeignKey(nameof(Paciente))]
    public int IdPaciente { get; set; }

    [ForeignKey(nameof(HistoriaClinica))]
    public int? IdHistoriaClinica { get; set; }

    public DateTime FechaEmision { get; set; } = DateTime.Now;

    public string ObservacionesGenerales { get; set; } = "";

    public string EstadoReceta { get; set; } = "Emitida"; // Ej: Emitida, Impresa, Anulada

    // Relaciones
    public Cita Cita { get; set; } = null!;
    public Medico Medico { get; set; } = null!;
    public Paciente Paciente { get; set; } = null!;
    public HistoriaClinica? HistoriaClinica { get; set; }

    public ICollection<DetalleReceta> Detalles { get; set; } = new List<DetalleReceta>();
  }
}
