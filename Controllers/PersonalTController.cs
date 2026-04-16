using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Services;
using SGMG.Dtos.Request.PersonalTecnico;
using SGMG.Services.ServiceImpl;


namespace SGMG.Controllers
{
  [Route("[controller]")]
  public class PersonalTController : Controller
  {
    private readonly ILogger<PersonalTController> _logger;
    private readonly IPersonalTservice _personalTecnicoService;

    public PersonalTController(ILogger<PersonalTController> logger, IPersonalTservice personalTecnicoService)
    {
      _logger = logger;
      _personalTecnicoService = personalTecnicoService;
    }

    [HttpGet]
    [Route("/personal-tecnico/{id}")]
    public async Task<GenericResponse<PersonalTecnico>> GetPersonalTecnicoById(int id)
    {
      return await _personalTecnicoService.GetPersonalTecnicoByIdAsync(id);
    }

    [HttpPost]
    [Route("/personal-tecnico/register")]
    public async Task<GenericResponse<PersonalTecnico>> CreatePersonalTecnico([FromBody] PersonalTecnicoRequestDTO personalTecnicoRequestDTO)
    {
      return await _personalTecnicoService.AddPersonalTecnicoAsync(personalTecnicoRequestDTO);
    }

    [HttpPut]
    [Route("/personal-tecnico/update")]
    public async Task<GenericResponse<PersonalTecnico>> UpdatePersonalTecnico([FromBody] PersonalTecnicoRequestDTO personalTecnicoRequestDTO)
    {
      return await _personalTecnicoService.UpdatePersonalTecnicoAsync(personalTecnicoRequestDTO);
    }

    [HttpDelete]
    [Route("/personal-tecnico/delete/{id}")]
    public async Task<GenericResponse<PersonalTecnico>> DeletePersonalTecnico(int id)
    {
      return await _personalTecnicoService.DeletePersonalTecnicoAsync(id);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}