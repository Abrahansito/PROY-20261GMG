using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;


namespace SGMG.Dtos
{
  public class FiltroDisponibilidadRequest
  {
    public int Semana { get; set; }

    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos.")]
    public string? NumeroDni { get; set; }
    public int? IdConsultorio { get; set; }
    public string? Estado { get; set; }
    public string? Turno { get; set; }
  }
}