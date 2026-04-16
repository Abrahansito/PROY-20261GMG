using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;

namespace SGMG.Services
{
  public interface IPersonalService
  {
    Task<GenericResponse<ResumenPersonalResponse>> GetResumenPersonalAsync();
    Task<GenericResponse<IEnumerable<PersonalRegistradoResponse>>> BuscarPersonalAsync(PersonalFiltroRequest filtro);
  }
}