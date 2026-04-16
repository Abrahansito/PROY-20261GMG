using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class Cita
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCita { get; set; }
    [ForeignKey(nameof(Paciente))]
    public int IdPaciente { get; set; }
    [ForeignKey(nameof(Medico))]
    public int IdMedico { get; set; }
    [ForeignKey(nameof(Triage))]
    public int? IdTriage { get; set; }
    public string Especialidad { get; set; } = "";
    public DateTime FechaCita { get; set; }
    public TimeSpan HoraCita { get; set; }
    public string Consultorio { get; set; } = "";
    public string EstadoCita { get; set; } = "";
    public DateTime FechaRegistro { get; set; }

    // Relaciones
    public Paciente Paciente { get; set; } = null!;
    public Medico Medico { get; set; } = null!;
    public Triaje? Triage { get; set; } = null!;
    public ICollection<Pago> Pagos { get; set; } = new List<Pago>();
    public ICollection<Diagnostico> Diagnosticos { get; set; } = new List<Diagnostico>(); //NUEVO
  }
}