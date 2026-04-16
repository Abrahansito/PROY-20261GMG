using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGMG.Models
{
  public class DisponibilidadSemanal
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDisponibilidad { get; set; }

    [ForeignKey(nameof(Medico))]
    public int IdMedico { get; set; }
    public Medico? Medico { get; set; }

    public DateTime FechaInicioSemana { get; set; }
    public DateTime FechaFinSemana { get; set; }
    public int CitasActuales { get; set; } = 0;
    public int CitasMaximas { get; set; } = 126;
  }
}