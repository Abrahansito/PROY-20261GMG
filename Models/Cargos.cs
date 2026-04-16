using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGMG.Models
{
  public class Cargos
  {

    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdCargo { get; set; }
    public string? NombreCargo { get; set; }
  }
}