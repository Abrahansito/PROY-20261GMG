using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using SGMG.Models;

namespace PROY_20252SGMG.Models
{
  public class Turno
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdTurno { get; set; }

    [Required]
    public string Nombre { get; set; } = "";

    [Required]
    public TimeSpan HoraInicio { get; set; }

    [Required]
    public TimeSpan HoraFin { get; set; }

    // Relaciones
    public ICollection<Medico> Medicos { get; set; } = new List<Medico>();
  }
}