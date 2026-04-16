using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PROY_20252SGMG.Dtos.Request
{
  public class DetalleRecetaRequestDTO
  {
    [Required(ErrorMessage = "El producto farmacéutico es obligatorio")]
    [StringLength(150)]
    public string ProductoFarmaceutico { get; set; } = "";

    [Required(ErrorMessage = "La concentración es obligatoria")]
    [StringLength(100)]
    public string Concentracion { get; set; } = "";

    [Required(ErrorMessage = "La frecuencia es obligatoria")]
    [StringLength(100)]
    public string Frecuencia { get; set; } = "";

    [Required(ErrorMessage = "La duración es obligatoria")]
    [StringLength(100)]
    public string Duracion { get; set; } = "";

    [Required(ErrorMessage = "La vía de administración es obligatoria")]
    [StringLength(100)]
    public string ViaAdministracion { get; set; } = "";

    [StringLength(500)]
    public string Observaciones { get; set; } = "";
  }
}