using Microsoft.AspNetCore.Mvc;
using SGMG.Models;
using SGMG.Services;
using SGMG.Dtos.Response;
using SGMG.Dtos.Request.Triaje;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGMG.Data;
using PROY_20252SGMG.Models;
using Microsoft.AspNetCore.Authorization;

namespace SGMG.Controllers
{
  [Route("[controller]")]
  public class TriajeController : Controller
  {
    private readonly ILogger<TriajeController> _logger;
    private readonly ITriajeService _triajeService;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _context;

    public TriajeController(
        ILogger<TriajeController> logger,
        ITriajeService triajeService,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext context)
    {
      _logger = logger;
      _triajeService = triajeService;
      _userManager = userManager;
      _context = context;
    }


    [HttpGet]
    public async Task<IActionResult> Index()
    {
      //await CargarDatosEnfermera();
      return View();
    }

    [HttpGet]
    [Route("/api/triaje/historial-paciente/{idPaciente}")]
    public async Task<IActionResult> GetHistorialPaciente(int idPaciente)
    {
      try
      {

        var response = await _triajeService.GetHistorialTriajePacienteAsync(idPaciente);

        if (response.Success ?? false)
        {
          return Ok(new
          {
            success = true,
            paciente = response.Data?.Paciente,
            triajes = response.Data?.Triajes,
            message = response.Message
          });
        }
        else
        {
          return Ok(new
          {
            success = false,
            message = response.Message
          });
        }
      }
      catch (Exception ex)
      {
  
        return StatusCode(500, new
        {
          success = false,
          message = "Error al cargar el historial de triajes. Por favor, intente de nuevo más tarde."
        });
      }
    }

  }
}
