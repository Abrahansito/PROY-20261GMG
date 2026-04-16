using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROY_20252SGMG.Dtos.Request
{
  public class ReprogramarCitaRequest
  {
    public int IdCita { get; set; }
    public int IdMedico { get; set; }
    public int IdPaciente { get; set; }
    public string FechaCita { get; set; }
    public string HoraCita { get; set; }
    public int Semana { get; set; }
  }
}