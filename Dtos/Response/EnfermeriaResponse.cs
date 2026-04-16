using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGMG.Dtos.Response
{
  public class EnfermeriaResponse
  {
    public string? EstadoLaboral { get; set; }
    public string? NumeroDni { get; set; }
    public string? NombreCompleto { get; set; }
    public string? Cargo { get; set; }
    public string? Telefono { get; set; }
  }
}