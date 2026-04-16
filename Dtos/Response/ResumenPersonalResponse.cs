using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Models;

namespace SGMG.Dtos.Response
{
  public class ResumenPersonalResponse
  {
    public int MedicosActivos { get; set; }
    public int TecnicosActivos { get; set; }
    public int Consultorios { get; set; }
    public int PersonalCaja { get; set; }

    public List<Cargos>? Cargos { get; set; }
    public List<Consultorio>? ConsultoriosList { get; set; }
  }
}