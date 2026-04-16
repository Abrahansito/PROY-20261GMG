using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROY_20252SGMG.Dtos.Response
{
  public class DerivacionResponseDTO
  {
    public int IdDerivacion { get; set; }
    public int IdCitaOrigen { get; set; }
    public string TipoDocumento { get; set; } = string.Empty;
    public string NumeroDocumento { get; set; } = string.Empty;
    public string NombreCompletoPaciente { get; set; } = string.Empty;
    public string MedicoSolicitante { get; set; } = string.Empty;
    public string EspecialidadDestino { get; set; } = string.Empty;
    public string MotivoDerivacion { get; set; } = string.Empty;
    public DateTime FechaDerivacion { get; set; }
    public string EstadoDerivacion { get; set; } = string.Empty;

    // Datos adicionales para detalles
    public string ServicioOrigen { get; set; } = string.Empty;
    public string NumeroDocumentoMedico { get; set; } = string.Empty;
    public string EspecialidadSolicitante { get; set; } = string.Empty;
  }
}