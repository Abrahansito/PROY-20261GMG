using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROY_20252SGMG.Dtos.Response
{
  public class RecetaResponseDTO
  {
    public int IdReceta { get; set; }
    public int IdCita { get; set; }
    public int IdMedico { get; set; }
    public int IdPaciente { get; set; }
    public int? IdHistoriaClinica { get; set; }
    public DateTime FechaEmision { get; set; }
    public string ObservacionesGenerales { get; set; } = "";
    public string EstadoReceta { get; set; } = "";

    // Datos del paciente
    public string NombreCompletoPaciente { get; set; } = "";
    public string NumeroDocumentoPaciente { get; set; } = "";

    // Datos del médico
    public string NombreCompletoMedico { get; set; } = "";
    public string EspecialidadMedico { get; set; } = "";

    // Detalles de medicamentos
    public List<DetalleRecetaResponseDTO> Detalles { get; set; } = new List<DetalleRecetaResponseDTO>();
  }
}