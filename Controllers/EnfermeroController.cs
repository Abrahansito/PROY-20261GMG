using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using SGMG.Models;
using SGMG.Services;
using SGMG.Dtos.Response;
using SGMG.Dtos.Request.Enfermeria;

namespace SGMG.Controllers
{

  [Route("api/[controller]")]
  public class EnfermeroController : Controller
  {
    private readonly ILogger<EnfermeroController> _logger;
    private readonly IEnfermeriaService _enfermeriaService;

    public EnfermeroController(ILogger<EnfermeroController> logger, IEnfermeriaService enfermeriaService)
    {
      _logger = logger;
      _enfermeriaService = enfermeriaService;
    }

    [HttpGet]
    [Route("/enfermerias/all")]
    public async Task<GenericResponse<IEnumerable<EnfermeriaResponse>>> GetAllEnfermerias()
    {
      return await _enfermeriaService.GetAllEnfermeriasAsync();
    }

    [HttpGet]
    [Route("/enfermerias/{id}")]
    public async Task<GenericResponse<Enfermeria>> GetEnfermeriaById(int id)
    {
      return await _enfermeriaService.GetEnfermeriaByIdAsync(id);
    }

    [HttpPost]
    [Route("/enfermerias/register")]
    public async Task<GenericResponse<Enfermeria>> CreateEnfermeria([FromBody] EnfermeriaRequestDTO enfermeriaRequestDTO)
    {
      return await _enfermeriaService.AddEnfermeriaAsync(enfermeriaRequestDTO);
    }


    [HttpPut]
    [Route("/enfermerias/update")]
    public async Task<GenericResponse<Enfermeria>> UpdateEnfermeria([FromBody] EnfermeriaRequestDTO enfermeriaRequestDTO)
    {
      return await _enfermeriaService.UpdateEnfermeriaAsync(enfermeriaRequestDTO);
    }

    [HttpDelete]
    [Route("/enfermerias/delete/{id}")]
    public async Task<GenericResponse<Enfermeria>> DeleteEnfermeria(int id)
    {
      return await _enfermeriaService.DeleteEnfermeriaAsync(id);
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }

    [HttpGet("/force-error")]
    public IActionResult ForceError()
    {
      int x = 0;
      int y = 5 / x; // Excepción
      return Ok();
    }
  }
}