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

    // ============== RUTAS PARA VISTAS ==============

    [HttpGet]
    public async Task<IActionResult> Index()
    {
      await CargarDatosEnfermera();
      return View();
    }

    [HttpGet]
    [Route("/triaje/listado")]
    public async Task<IActionResult> Listado()
    {
      await CargarDatosEnfermera();
      return View();
    }

    [HttpGet]
    [Route("/triaje/registrar")]
    public async Task<IActionResult> Registrar()
    {
      try
      {

        await CargarDatosEnfermera();
        return View();
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error en Triaje/Registrar: {ex.Message}");
        ViewBag.ErrorMessage = "Error del sistema. No se pudo iniciar el triaje. Por favor, contacte a soporte técnico.";
        return View("Error3");
      }
    }

    [Authorize(Roles = "ENFERMERIA")]
    [HttpGet]
    [Route("/triaje/editar/{id}")]
    public async Task<IActionResult> Editar(int id)
    {

      try
      {
        //throw new Exception("Error de prueba en Editar Triaje");
        await CargarDatosEnfermera();

        var response = await _triajeService.GetTriajeByIdAsync(id);

        if (!(response.Success ?? false) || response.Data == null)
        {
          TempData["ErrorMessage"] = "Triaje no encontrado";
          return RedirectToAction("Listado");
        }

        var triajeData = response.Data;

        // Mapear a RequestDTO para el formulario
        var triajeRequest = new TriajeRequestDTO
        {
          IdTriaje = triajeData.IdTriaje,
          IdPaciente = triajeData.IdPaciente,
          Temperatura = triajeData.Temperatura,
          PresionArterial = triajeData.PresionArterial,
          Saturacion = triajeData.Saturacion,
          FrecuenciaCardiaca = triajeData.FrecuenciaCardiaca,
          FrecuenciaRespiratoria = triajeData.FrecuenciaRespiratoria,
          Peso = triajeData.Peso,
          Talla = triajeData.Talla,
          PerimAbdominal = triajeData.PerimetroAbdominal,
          SuperficieCorporal = triajeData.SuperficieCorporal,
          Imc = triajeData.Imc,
          ClasificacionImc = triajeData.ClasificacionImc,
          RiesgoEnfermedad = triajeData.RiesgoEnfermedad,
          EstadoTriage = triajeData.EstadoTriage,
          Observaciones = triajeData.Observaciones
        };

        // Pasar datos del paciente a la vista
        ViewBag.PacienteInfo = new
        {
          NombreCompleto = triajeData.NombreCompletoPaciente ?? "",
          Documento = $"{triajeData.TipoDocumento ?? ""}: {triajeData.NumeroDocumento ?? ""}",
          Sexo = triajeData.Sexo ?? "",
          Edad = triajeData.Edad,
          IdPaciente = triajeData.IdPaciente
        };

        return View("EditarTriaje", triajeRequest);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error al cargar triaje {id} para edición");
        TempData["ErrorMessage"] = "No se pudieron cargar los datos del triaje. Por favor, intente nuevamente";
        return RedirectToAction("Listado");
      }

    }


    [Authorize(Roles = "ENFERMERIA")]
    [HttpPost]
    [Route("/triaje/editar/{id}")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, TriajeRequestDTO triajeRequest)
    {
      try
      {
        // ⚠️ DESCOMENTAR PARA PROBAR EXCEPCIÓN
        throw new Exception("");

        await CargarDatosEnfermera();

        if (id != triajeRequest.IdTriaje)
        {
          TempData["ErrorMessage"] = "ID del triaje no coincide";
          return RedirectToAction("Listado");
        }

        if (!ModelState.IsValid)
        {
          // Recargar datos del paciente si hay error de validación
          var pacienteResponse = await _triajeService.GetTriajeByIdAsync(id);
          if ((pacienteResponse.Success ?? false) && pacienteResponse.Data != null)
          {
            ViewBag.PacienteInfo = new
            {
              NombreCompleto = pacienteResponse.Data.NombreCompletoPaciente ?? "",
              Documento = $"{pacienteResponse.Data.TipoDocumento ?? ""}: {pacienteResponse.Data.NumeroDocumento ?? ""}",
              Sexo = pacienteResponse.Data.Sexo ?? "",
              Edad = pacienteResponse.Data.Edad,
              IdPaciente = pacienteResponse.Data.IdPaciente
            };
          }
          return View("EditarTriaje", triajeRequest);
        }

        var response = await _triajeService.UpdateTriajeAsync(triajeRequest);

        if (response.Success ?? false)
        {
          TempData["SuccessMessage"] = "Triaje actualizado correctamente";
          return RedirectToAction("Listado");
        }

        ModelState.AddModelError("", response.Message ?? "Error al actualizar el triaje");

        // Recargar datos del paciente en caso de error
        var pacienteData = await _triajeService.GetTriajeByIdAsync(id);
        if ((pacienteData.Success ?? false) && pacienteData.Data != null)
        {
          ViewBag.PacienteInfo = new
          {
            NombreCompleto = pacienteData.Data.NombreCompletoPaciente ?? "",
            Documento = $"{pacienteData.Data.TipoDocumento ?? ""}: {pacienteData.Data.NumeroDocumento ?? ""}",
            Sexo = pacienteData.Data.Sexo ?? "",
            Edad = pacienteData.Data.Edad,
            IdPaciente = pacienteData.Data.IdPaciente
          };
        }

        return View("EditarTriaje", triajeRequest);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error al guardar cambios del triaje {id}");
        TempData["ErrorMessage"] = "Ocurrió un error al guardar los cambios. Por favor, intente nuevamente";
        return RedirectToAction("Listado");
      }
    }

    [Authorize(Roles = "ENFERMERIA")]
    [HttpGet]
    [Route("/triaje/detalles/{id}")]
    public async Task<IActionResult> Detalles(int id)
    {
      await CargarDatosEnfermera();

      var response = await _triajeService.GetTriajeByIdAsync(id);

      if (!(response.Success ?? false) || response.Data == null)
      {
        TempData["ErrorMessage"] = "Triaje no encontrado";
        return RedirectToAction("Listado");
      }

      return View(response.Data);
    }

    [HttpGet]
    [Route("/triaje/historial")]
    public IActionResult Historial()
    {
      return View();
    }

    // ============== MÉTODO PRIVADO PARA CARGAR DATOS ==============

    private async Task CargarDatosEnfermera()
    {
      var user = await _userManager.GetUserAsync(User);
      if (user != null)
      {
        var enfermera = await _context.Enfermerias.Where(e => e.IdEnfermeria.ToString() == user.IdUsuario)
                        .Include(e => e.Personal)
                        .FirstOrDefaultAsync();

        if (enfermera != null)
        {
          var nombreCompleto = $"{enfermera.Personal?.Nombre ?? ""} {enfermera.Personal?.ApellidoPaterno ?? ""} {enfermera.Personal?.ApellidoMaterno ?? ""}";
          ViewData["NombreEnfermera"] = nombreCompleto;
          ViewData["NumeroColegiaturaEnfermeria"] = enfermera.NumeroColegiaturaEnfermeria ?? "";
          ViewData["NivelProfesional"] = enfermera.NivelProfesional ?? "";
          ViewData["IdEnfermeria"] = enfermera.IdEnfermeria;
          ViewData["IdPersonal"] = enfermera.IdConsultorio;

          ViewData["ConsultorioInfo"] = $"Consultorio {enfermera.IdConsultorio}";
        }
      }
    }

    // ============== ENDPOINTS API (métodos existentes) ==============

    [HttpGet]
    [Route("/triaje/buscar")]
    public async Task<GenericResponse<IEnumerable<TriajeResponseDTO>>> BuscarTriajes(
        [FromQuery] string? tipoDoc,
        [FromQuery] string? numeroDoc,
        [FromQuery] DateTime? fechaInicio,
        [FromQuery] DateTime? fechaFin)
    {
      return await _triajeService.BuscarTriajesAsync(tipoDoc, numeroDoc, fechaInicio, fechaFin);
    }

    [HttpPost]
    [Route("/triaje/register")]
    public async Task<GenericResponse<Triaje>> CreateTriaje([FromBody] TriajeRequestDTO triajeRequestDTO)
    {
      return await _triajeService.AddTriajeAsync(triajeRequestDTO);
    }

    [HttpGet]
    [Route("/triaje/{id}")]
    public async Task<GenericResponse<TriajeResponseDTO>> GetTriajeById(int id)
    {
      return await _triajeService.GetTriajeByIdAsync(id);
    }

    [HttpPut]
    [Route("/triaje/update")]
    public async Task<GenericResponse<TriajeResponseDTO>> UpdateTriaje([FromBody] TriajeRequestDTO triajeRequestDTO)
    {
      return await _triajeService.UpdateTriajeAsync(triajeRequestDTO);
    }

    [HttpGet]
    [Route("/triaje/all")]
    public async Task<GenericResponse<IEnumerable<TriajeResponseDTO>>> GetAllTriajes()
    {

      return await _triajeService.GetAllTriajesAsync();
    }

    [HttpGet]
    [Route("/citas/pendientesportriage")]
    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasPagadasPorUsuarioLogueado()
    {
      try
      {
        // Obtener el ID del usuario logueado
        var user = await _userManager.GetUserAsync(User);
        if (user == null)
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, "Usuario no autenticado");
        }

        // Buscar la enfermera por el ID del usuario
        var enfermera = await _context.Enfermerias
            .FirstOrDefaultAsync(e => e.IdEnfermeria.ToString() == user.IdUsuario);

        if (enfermera == null)
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, "Enfermera no encontrada");
        }

        // Llamar al servicio con el ID de la enfermera
        return await _triajeService.GetCitasPagadasPorConsultorioEnfermeraAsync(enfermera.IdEnfermeria);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error en GetCitasPagadasPorUsuarioLogueado: {ex.Message}");
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }

    [HttpGet]
    [Route("/api/triaje/historial-paciente/{idPaciente}")]
    public async Task<IActionResult> GetHistorialPaciente(int idPaciente)
    {
      try
      {

        var response = await _triajeService.GetHistorialTriajePacienteAsync(idPaciente);

        // ✅ CORREGIDO: Usar ?? false para valores nullable
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
        // 🔹 Captura de errores (por ejemplo, falla de conexión a la BD)
        return StatusCode(500, new
        {
          success = false,
          message = "Error al cargar el historial de triajes. Por favor, intente de nuevo más tarde."
        });
      }
    }

  }
}