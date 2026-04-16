using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGMG.Dtos.Response
{
  public class MedicoDisponibilidadDTO
  {
    public int IdMedico { get; set; }
    public string NumeroDni { get; set; } = "";
    public string NombreCompleto { get; set; } = "";
    public string Consultorio { get; set; } = "";
    public string Turno { get; set; } = "";
    public int CitasActuales { get; set; }
    public int CitasMaximas { get; set; }
    public int CitasRestantes { get; set; }
  }
}