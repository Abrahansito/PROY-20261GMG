using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROY_20252SGMG.Dtos.Response
{
  public class RecetaEstadisticas
  {
    public int TotalRecetas { get; set; }
    public int TotalMedicamentos { get; set; }
    public int RecetasImpresas { get; set; }
    public DateTime? UltimaReceta { get; set; }
  }
}