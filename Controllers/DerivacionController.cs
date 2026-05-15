using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using PROY_20252SGMG.Models;
using SGMG.Data;
using PROY_20252SGMG.Dtos.Request;

namespace SGMG.Controllers
{
  public class DerivacionController : Controller
  {
    private readonly ApplicationDbContext _context;

    public DerivacionController(ApplicationDbContext context)
    {
      _context = context;
    }

    [HttpGet]
    [Route("Derivacion/Create/{idCita}")]
    public async Task<IActionResult> Create(int idCita)
    {
  
      if (idCita <= 0)
      {
        TempData["Error"] = "El ID de la cita es inválido.";
        return RedirectToAction("Index", "Home");
      }

     
      var cita = await _context.Citas
          .Include(c => c.Paciente)
          .Include(c => c.Medico)
          .FirstOrDefaultAsync(c => c.IdCita == idCita);

      if (cita == null)
      {
        TempData["Error"] = "No se encontró la cita especificada.";
        return RedirectToAction("Index", "Home");
      }

      // Pasar datos a la vista
      var historiaClinica = await _context.HistoriasClinicas
          .Where(h => h.IdPaciente == cita.IdPaciente)
          .OrderByDescending(h => h.IdHistoria)
          .FirstOrDefaultAsync();

      var edadPaciente = historiaClinica != null
          ? CalcularEdad(historiaClinica.FechaNacimiento)
          : cita.Paciente.Edad;

      ViewBag.IdCita = idCita;
      ViewBag.Paciente = cita.Paciente;
      ViewBag.Medico = cita.Medico;
      ViewBag.EdadPaciente = edadPaciente;

      return View();
    }

    // POST: Derivacion/Create (con JSON)
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] DerivacionRequest request)
    {
      try
      {

        Console.WriteLine("=== INICIO POST CREATE ===");
        Console.WriteLine($"Request recibido: IdCita={request?.IdCitaOrigen}, Especialidad={request?.EspecialidadDestino}");

        // Validar datos recibidos
        if (request == null)
        {
          Console.WriteLine("ERROR: Request es null");
          return Json(new { success = false, message = "Datos inválidos." });
        }

        Console.WriteLine($"Buscando cita con ID: {request.IdCitaOrigen}");

        // Verificar que la cita existe
        var cita = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .FirstOrDefaultAsync(c => c.IdCita == request.IdCitaOrigen);

        if (cita == null)
        {
          Console.WriteLine("ERROR: Cita no encontrada");
          return Json(new { success = false, message = "La cita especificada no existe." });
        }

        Console.WriteLine($"Cita encontrada: {cita.IdCita}");
        Console.WriteLine("Creando derivación...");

        // Crear la derivación
        var derivacion = new Derivacion
        {
          IdCitaOrigen = request.IdCitaOrigen,
          EspecialidadDestino = request.EspecialidadDestino,
          IdMedicoDestino = request.IdMedicoDestino, // Agregar médico destino si viene
          MotivoDerivacion = request.MotivoDerivacion.Trim(),
          FechaDerivacion = DateTime.Now,
          EstadoDerivacion = "Pendiente"
        };

        Console.WriteLine("Agregando derivación al contexto...");

        // Agregar y guardar
        _context.Derivaciones.Add(derivacion);

        Console.WriteLine("Guardando cambios...");
        await _context.SaveChangesAsync();

        Console.WriteLine("Derivación guardada exitosamente!");
        Console.WriteLine("=== FIN POST CREATE ===");

        return Json(new
        {
          success = true,
          message = $"Derivación a {request.EspecialidadDestino} (Prioridad: {request.Prioridad}) creada exitosamente.",
          redirectUrl = Url.Action("Historial", "HistorialDerivacion", new { idCita = request.IdCitaOrigen })
        });
      }
      catch (Exception ex)
      {
        Console.WriteLine($"ERROR EXCEPTION: {ex.Message}");
        Console.WriteLine($"StackTrace: {ex.StackTrace}");
        if (ex.InnerException != null)
        {
          Console.WriteLine($"InnerException: {ex.InnerException.Message}");
        }

        return Json(new { success = false, message = "No se pudo registrar la derivación. Por favor, intente nuevamente." });
      }
    }

    private static int CalcularEdad(DateTime fechaNacimiento)
    {
      var hoy = DateTime.Today;
      var edad = hoy.Year - fechaNacimiento.Year;

      if (fechaNacimiento.Date > hoy.AddYears(-edad))
        edad--;

      return Math.Max(edad, 0);
    }

    // Clase para recibir los datos del POST
  }
}
