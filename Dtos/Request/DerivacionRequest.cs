using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROY_20252SGMG.Dtos.Request
{
  public class DerivacionRequest
  {
    public int IdCitaOrigen { get; set; }
    public string EspecialidadDestino { get; set; } = string.Empty;
    public string Prioridad { get; set; } = string.Empty;
    public string MotivoDerivacion { get; set; } = string.Empty;
    public int? IdMedicoDestino { get; set; } // Opcional
  }
}
