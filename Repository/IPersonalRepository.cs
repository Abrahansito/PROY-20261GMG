using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;

namespace SGMG.Repository
{
  public interface IPersonalRepository
  {
    Task<ResumenPersonalResponse> GetResumenPersonalAsync();
    Task<IEnumerable<PersonalRegistradoResponse>> BuscarPersonalAsync(PersonalFiltroRequest filtro);
  }
}