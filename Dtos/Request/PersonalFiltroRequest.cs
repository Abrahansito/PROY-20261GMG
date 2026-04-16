using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SGMG.Dtos.Request
{
  public class PersonalFiltroRequest
  {
    [StringLength(100, ErrorMessage = "El nombre no puede superar los 100 caracteres.")]
    [RegularExpression(@"^(?!\d+$).+", ErrorMessage = "El nombre no puede ser solo números.")]
    public string? Nombre { get; set; }

    [RegularExpression(@"^\d{8}$", ErrorMessage = "El DNI debe tener exactamente 8 dígitos.")]
    public string? Dni { get; set; }

    [RegularExpression("^(Activo|Inactivo)$", ErrorMessage = "El estado debe ser 'Activo' o 'Inactivo'.")]
    public string? Estado { get; set; }

    [RegularExpression("^(MEDICO GENERAL|ENFERMERIA|CAJERO|ADMINISTRADOR|ADMISION|TODOS)$", ErrorMessage = "El tipo de personal debe ser: MEDICO GENERAL, ENFERMERIA, CAJERO, ADMINISTRADOR,ADMISION o TODOS.")]
    public string? TipoPersonal { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "El IdConsultorio debe ser un número entero positivo.")]
    public int? IdConsultorio { get; set; }
  }
}