using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGMG.Dtos.Response
{
  public class DisponibilidadSemanaResponse
  {
    public int NumeroSemana { get; set; }
    public string FechaInicioSemana { get; set; } = "";
    public string FechaFinSemana { get; set; } = "";
    public string PeriodoSemana { get; set; } = "";
    public int TotalMedicosDisponibles { get; set; }
    public List<MedicoDisponibilidadDTO> MedicosDisponibles { get; set; } = new();
  }
}