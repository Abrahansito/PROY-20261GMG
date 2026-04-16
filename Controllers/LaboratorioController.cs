using Microsoft.AspNetCore.Mvc;
using SGMG.Dtos.Request;
using SGMG.Services;

namespace SGMG.Controllers
{
  [Route("[controller]")]
  public class LaboratorioController : Controller
  {
    private readonly ILaboratorioService _laboratorioService;

    public LaboratorioController(ILaboratorioService laboratorioService)
    {
      _laboratorioService = laboratorioService;
    }

    // Vista de listado
    [HttpGet]
    public IActionResult Index()
    {
      return View();
    }

    // Vista de nueva orden
    [HttpGet]
    [Route("nueva-orden")]
    public IActionResult NuevaOrden()
    {
      try
      {

        return View();
      }
      catch (Exception ex)
      {
        // Si hay error al intentar preparar la vista
        ViewBag.ErrorMessage = "Error al cargar la el formulario. Por favor, regrese e intente nuevamente.";
        return View("Error2");
      }

    }

    // Vista de subir detalles
    [HttpGet]
    [Route("subir-detalles")]
    public IActionResult SubirDetalles()
    {
      try
      {

        return View();
      }
      catch (Exception ex)
      {
        // Si hay error al intentar preparar la vista
        ViewBag.ErrorMessage = "No se pudo cargar la página para subir resultados. Por favor, intente nuevamente.";
        return View("Error1");
      }
    }

    // API: Obtener historial
    [HttpGet]
    [Route("api/historial/{idPaciente}")]
    public async Task<IActionResult> GetHistorial(int idPaciente)
    {
      try
      {
        var response = await _laboratorioService.GetHistorialLaboratorioAsync(idPaciente);

        if (response.Success.GetValueOrDefault())
        {
          return Ok(new
          {
            success = true,
            paciente = response.Data?.Paciente,
            ordenes = response.Data?.Ordenes,
            message = response.Message
          });
        }
        else
        {
          return Ok(new { success = false, message = response.Message });
        }
      }
      catch (Exception ex)
      {
        // 🔹 Captura de errores, incluyendo fallas de conexión a la BD
        return StatusCode(500, new
        {
          success = false,
          message = "Error al cargar las órdenes de laboratorio. Por favor, intente de nuevo más tarde."
        });
      }
    }


    // API: Crear orden
    [HttpPost]
    [Route("api/crear")]
    public async Task<IActionResult> CrearOrden([FromBody] OrdenLaboratorioRequestDTO dto)
    {
      try
      {

        var response = await _laboratorioService.CrearOrdenAsync(dto);

        return Ok(new
        {
          success = response.Success,
          data = response.Data,
          message = response.Message ?? (response.Success == true
            ? "Orden creada exitosamente"
            : "Ocurrió un error al guardar la orden. Por favor, intente nuevamente.")
        });
      }
      catch (Exception ex)
      {


        return Ok(new
        {
          success = false,
          message = "Ocurrió un error al guardar la orden. Por favor, intente nuevamente."
        });
      }
    }

    // API: Actualizar orden
    [HttpPut]
    [Route("api/actualizar")]
    public async Task<IActionResult> ActualizarOrden([FromBody] OrdenLaboratorioRequestDTO dto)
    {
      var response = await _laboratorioService.ActualizarOrdenAsync(dto);
      return Ok(new
      {
        success = response.Success,
        data = response.Data,
        message = response.Message
      });
    }

    // API: Obtener orden por ID
    // API: Obtener orden por ID
    [HttpGet]
    [Route("api/orden/{idOrden}")]
    public async Task<IActionResult> GetOrden(int idOrden)
    {
      try
      {
        throw new Exception("Error simulado para pruebas");
      }
      catch (Exception ex)
      {
        // 🔹 Captura de errores (por ejemplo, falla de conexión con la BD)
        return StatusCode(500, new
        {
          success = false,
          message = "No se pudo cargar la página para subir resultados. Por favor, intente nuevamente."
        });
      }
    }


    //API: Cancelar orden
    [HttpPut]
    [Route("api/cancelar/{idOrden}")]
    public async Task<IActionResult> CancelarOrden(int idOrden)
    {
      try
      {
        var response = await _laboratorioService.CancelarOrdenAsync(idOrden);
        return Ok(new
        {
          success = response.Success,
          message = response.Message ?? "Orden cancelada exitosamente"
        });
      }
      catch (Exception ex)
      {
        return Ok(new
        {
          success = false,
          message = $"Error al cancelar la orden: {ex.Message}"
        });
      }
    }

    //API: Actualizar resultados de la orden
    [HttpPut]
    [Route("api/actualizar-resultados")]
    public async Task<IActionResult> ActualizarResultados([FromBody] ActualizarResultadosDTO dto)
    {
      try
      {

        var response = await _laboratorioService.ActualizarResultadosAsync(dto);
        return Ok(new
        {
          success = response.Success,
          message = response.Message ?? "Resultados actualizados exitosamente"
        });
      }
      catch (Exception ex)
      {
        return Ok(new
        {
          success = false,
          message = "Ocurrió un error al guardar los resultados. Por favor, intente nuevamente."
        });
      }
    }
  }
}

