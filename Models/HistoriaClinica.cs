using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class HistoriaClinica
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdHistoria { get; set; }
    [ForeignKey(nameof(Paciente))]
    public int IdPaciente { get; set; }
    public string CodigoHistoria { get; set; } = "";
    public string TipoSeguro { get; set; } = "";
    public DateTime FechaNacimiento { get; set; }
    public string EstadoCivil { get; set; } = "";
    public string TipoSangre { get; set; } = "";

    // Relaciones
    public Paciente Paciente { get; set; } = null!;
  }
}