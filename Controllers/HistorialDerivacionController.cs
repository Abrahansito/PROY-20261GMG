using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using PROY_20252SGMG.Models;
using SGMG.Data;

namespace SGMG.Controllers
{
  public class HistorialDerivacionController : Controller
  {
    private readonly ApplicationDbContext _context;

    public HistorialDerivacionController(ApplicationDbContext context)
    {
      _context = context;
    }

  
    [HttpGet]
    [Route("HistorialDerivacion/Historial")]
    public async Task<IActionResult> Historial(int idCita)
    {
      if (idCita <= 0)
      {
        TempData["Error"] = "El ID de la cita es inválido.";
        return RedirectToAction("Index", "Home");
      }

    
      var cita = await _context.Citas
          .Include(c => c.Paciente)
          .FirstOrDefaultAsync(c => c.IdCita == idCita);

      if (cita == null)
      {
        TempData["Error"] = "No se encontró la cita especificada.";
        return RedirectToAction("Index", "Home");
      }

      var paciente = cita.Paciente;
      var idPaciente = cita.IdPaciente;
      var historiaClinica = await _context.HistoriasClinicas
          .Where(h => h.IdPaciente == idPaciente)
          .OrderByDescending(h => h.IdHistoria)
          .FirstOrDefaultAsync();

      var edadPaciente = historiaClinica != null
          ? CalcularEdad(historiaClinica.FechaNacimiento)
          : paciente.Edad;

      var seguroPaciente = !string.IsNullOrWhiteSpace(historiaClinica?.TipoSeguro)
          ? $"{historiaClinica.TipoSeguro} - Sistema Integral de Salud"
          : "SIS - Sistema Integral de Salud";

      // Obtener el historial de derivaciones ordenado por fecha más reciente
      var historialDerivaciones = await _context.Derivaciones
          .Include(d => d.Cita)
              .ThenInclude(c => c.Medico)
          .Include(d => d.MedicoDestino)
          .Where(d => d.Cita.IdPaciente == idPaciente)
          .OrderByDescending(d => d.FechaDerivacion)
          .Select(d => new
          {
            d.IdDerivacion,
            TipoDocumento = "DNI",
            NumeroDocumento = paciente.NumeroDocumento,
            MedicoSolicitante = d.Cita.Medico.Nombre + " " + d.Cita.Medico.ApellidoPaterno,
            d.EspecialidadDestino,
            d.FechaDerivacion,
            d.MotivoDerivacion,
            d.EstadoDerivacion,
            ServicioOrigen = "Medicina General",
            IdCita = d.IdCitaOrigen
          })
          .ToListAsync();

      // Pasar datos a la vista
      ViewBag.Paciente = paciente;
      ViewBag.EdadPaciente = edadPaciente;
      ViewBag.SeguroPaciente = seguroPaciente;
      ViewBag.HistorialDerivaciones = historialDerivaciones;
      ViewBag.IdCitaActual = idCita; // Pasar el ID de la cita actual

      return View();
    }

    // GET: HistorialDerivacion/ObtenerDetalle/5 (idDerivacion) - AJAX
    [HttpGet]
    public async Task<IActionResult> ObtenerDetalle(int idDerivacion)
    {
      if (idDerivacion <= 0)
      {
        return Json(new { success = false, message = "ID de derivación inválido." });
      }

      var derivacion = await _context.Derivaciones
          .Include(d => d.Cita)
              .ThenInclude(c => c.Paciente)
          .Include(d => d.Cita.Medico)
          .Include(d => d.MedicoDestino)
          .FirstOrDefaultAsync(d => d.IdDerivacion == idDerivacion);

      if (derivacion == null)
      {
        return Json(new { success = false, message = "Derivación no encontrada." });
      }

      var detalle = new
      {
        success = true,
        idCita = derivacion.IdCitaOrigen,
        paciente = derivacion.Cita.Paciente.Nombre + " " +
                   derivacion.Cita.Paciente.ApellidoPaterno + " " +
                   derivacion.Cita.Paciente.ApellidoMaterno,
        medicoSolicitante = "Dr. " + derivacion.Cita.Medico.Nombre + " " +
                           derivacion.Cita.Medico.ApellidoPaterno,
        tipoDocumento = "DNI",
        numeroDocumento = derivacion.Cita.Paciente.NumeroDocumento,
        especialidadSolicitada = derivacion.EspecialidadDestino,
        fechaSolicitud = derivacion.FechaDerivacion.ToString("dd/MM/yyyy"),
        horaSolicitud = derivacion.FechaDerivacion.ToString("HH:mm") + " AM",
        seguro = "SIS - Sistema Integral de Salud",
        servicioOrigen = "Medicina General",
        motivoDerivacion = derivacion.MotivoDerivacion,
        prioridad = derivacion.EstadoDerivacion == "Pendiente" ? "Normal" :
                   derivacion.EstadoDerivacion == "Atendida" ? "Normal" : "Baja"
      };

      return Json(detalle);
    }

    private static int CalcularEdad(DateTime fechaNacimiento)
    {
      var hoy = DateTime.Today;
      var edad = hoy.Year - fechaNacimiento.Year;

      if (fechaNacimiento.Date > hoy.AddYears(-edad))
        edad--;

      return Math.Max(edad, 0);
    }
  }
}
