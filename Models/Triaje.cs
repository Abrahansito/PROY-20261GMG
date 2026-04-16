using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class Triaje
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTriage { get; set; }
    [ForeignKey(nameof(Paciente))]
    public int IdPaciente { get; set; }
    public decimal Temperatura { get; set; }
    public int PresionArterial { get; set; }
    public int Saturacion { get; set; }
    public int FrecuenciaCardiaca { get; set; }
    public int FrecuenciaRespiratoria { get; set; }
    public decimal Peso { get; set; }
    public decimal Talla { get; set; }
    public decimal PerimetroAbdominal { get; set; }
    public decimal SuperficieCorporal { get; set; }
    public decimal Imc { get; set; }
    public string ClasificacionImc { get; set; } = "";
    public string RiesgoEnfermedad { get; set; } = "";
    public DateTime FechaTriage { get; set; }
    public TimeSpan HoraTriage { get; set; }
    public string Observaciones { get; set; } = "";
    public string EstadoTriage { get; set; } = "";

    // Relaciones
    public Paciente Paciente { get; set; } = null!;
  }
}