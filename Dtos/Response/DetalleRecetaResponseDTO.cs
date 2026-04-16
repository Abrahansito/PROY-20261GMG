using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROY_20252SGMG.Dtos.Response
{
  public class DetalleRecetaResponseDTO
  {
    public int IdDetalle { get; set; }
    public string ProductoFarmaceutico { get; set; } = "";
    public string Concentracion { get; set; } = "";
    public string Frecuencia { get; set; } = "";
    public string Duracion { get; set; } = "";
    public string ViaAdministracion { get; set; } = "";
    public string Observaciones { get; set; } = "";
  }
}