using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace PROY_20252SGMG.Dtos.Request
{
  public class RecetaRequestDTO
  {
    [Required(ErrorMessage = "El ID de la cita es obligatorio")]
    public int IdCita { get; set; }

    [Required(ErrorMessage = "El ID del médico es obligatorio")]
    public int IdMedico { get; set; }

    [Required(ErrorMessage = "El ID del paciente es obligatorio")]
    public int IdPaciente { get; set; }

    public int? IdHistoriaClinica { get; set; }

    [StringLength(500)]
    public string ObservacionesGenerales { get; set; } = "";


    public List<DetalleRecetaRequestDTO> Detalles { get; set; } = new List<DetalleRecetaRequestDTO>();
  }
}