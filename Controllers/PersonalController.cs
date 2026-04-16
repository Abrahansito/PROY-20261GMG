using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;
using SGMG.Services;

namespace SGMG.Controllers
{
  [ApiController]
  [Route("[controller]")]
  public class PersonalController : Controller
  {
    private readonly ILogger<PersonalController> _logger;
    private readonly IPersonalService _personalService;

    public PersonalController(ILogger<PersonalController> logger, IPersonalService personalService)
    {
      _logger = logger;
      _personalService = personalService;
    }

    [HttpGet]
    [Route("/personal/resumen")]
    public async Task<ActionResult<GenericResponse<ResumenPersonalResponse>>> GetResumenPersonal()
    {
      _logger.LogInformation("Iniciando obtención de resumen de personal");
      return await _personalService.GetResumenPersonalAsync();
    }

    [HttpPost]
    [Route("/personal/buscar")]
    public async Task<ActionResult<GenericResponse<IEnumerable<PersonalRegistradoResponse>>>> BuscarPersonal([FromBody] PersonalFiltroRequest filtro)

    {
      _logger.LogInformation("Iniciando búsqueda de personal con los siguientes filtros: {@filtro}", filtro);
      _logger.LogInformation("TipoPersonal: {TipoPersonal}, IdConsultorio: {IdConsultorio}", filtro.TipoPersonal, filtro.IdConsultorio);
      return await _personalService.BuscarPersonalAsync(filtro);
    }


  }
}