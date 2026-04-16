using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Data; // Namespace del ApplicationDbContext
using PROY_20252SGMG.Models; // Namespace correcto de los modelos
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging; // <-- IMPORTANTE para ILogger
using SGMG.common.exception; // Namespace del NoValidationAttribute

namespace PROY_20252SGMG.Controllers
{

  public class CitasController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CitasController> _logger; // ✅ Logger

    // Inyectamos el logger en el constructor
    public CitasController(ApplicationDbContext context, ILogger<CitasController> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IActionResult> Index(int? idPaciente)
    {
      if (!idPaciente.HasValue)
      {
        TempData["ErrorMessage"] = "No se ha especificado un paciente válido.";
        return RedirectToAction("Index", "Home");
      }

      try
      {
        // 1️⃣ Datos del paciente
        var paciente = await _context.Pacientes
            .Where(p => p.IdPaciente == idPaciente.Value)
            .Select(p => new
            {
              p.IdPaciente,
              p.NumeroDocumento,
              p.TipoDocumento,
              p.Nombre,
              p.ApellidoPaterno,
              p.ApellidoMaterno,
              p.Sexo,
              p.Edad
            })
            .FirstOrDefaultAsync();

        if (paciente == null)
        {
          _logger.LogWarning("Paciente con ID {IdPaciente} no encontrado.", idPaciente);
          TempData["ErrorMessage"] = "El paciente no existe.";
          return RedirectToAction("Index", "Home");
        }

        // 2️⃣ Traer citas (sin ordenar en SQL por TimeSpan)
        var citas = await _context.Citas
            .Where(c => c.IdPaciente == idPaciente.Value)
            .Include(c => c.Medico)
            .ToListAsync();

        // 3️⃣ Ordenar en memoria
        citas = citas
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .ToList();

        // 4️⃣ Proyección
        var citasConPaciente = citas.Select(c => new
        {
          Id = c.IdCita, // ✅ Cambiado de IdCita a Id
          c.FechaCita,
          c.HoraCita,
          TipoDocumento = paciente.TipoDocumento,
          NumeroDocumento = paciente.NumeroDocumento,
          NombreCompleto = $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}",
          c.Especialidad,
          c.Consultorio,
          c.EstadoCita,
          NombreMedico = $"{c.Medico.Nombre} {c.Medico.ApellidoPaterno} {c.Medico.ApellidoMaterno}"
        }).ToList();

        // 5️⃣ Datos del paciente
        string seguro = "SIS - Sistema Integral de Salud";

        ViewBag.Paciente = new
        {
          Dni = paciente.NumeroDocumento,
          NombreCompleto = $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}",
          Sexo = paciente.Sexo,
          Edad = paciente.Edad,
          Seguro = seguro
        };

        ViewBag.Citas = citasConPaciente;
        ViewBag.TieneCitas = citasConPaciente.Any();

        _logger.LogInformation("Historial de citas cargado correctamente para el paciente {IdPaciente}.", idPaciente);

        return View();
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al cargar historial de citas para el paciente {IdPaciente}.", idPaciente);

        ViewBag.ErrorCargaHistorial = "Ocurrió un error al cargar el historial de citas. Por favor, intente nuevamente.";
        ViewBag.TieneCitas = false;

        return View();
      }
    }

    [HttpGet]
    [Route("Citas/ObtenerDetallesCita")]
    public async Task<IActionResult> ObtenerDetallesCita(int id) // ✅ Cambiado de idCita a id
    {
      try
      {
        _logger.LogWarning("Intentando cargar detalles de la cita con ID {Id}", id); // ✅ Actualizado log

        // 1️⃣ Traes la cita con el médico
        var cita = await _context.Citas
            .Include(c => c.Medico)
            .FirstOrDefaultAsync(c => c.IdCita == id); // ✅ Actualizado


        if (cita == null)
        {
          _logger.LogWarning("No se encontró gyjtyjtyj la cita con ID {Id}", id); // ✅ Actualizado log
          return Json(new { success = false, message = "No se pudieron cargar los detalles de esta cita. Intente de nuevo." });
        }
        _logger.LogInformation("Cita con ID {Id} encontrada.", id); // ✅ Actualizado log

        // 2️⃣ Formatear valores
        var detalles = new
        {
          FechaCita = cita.FechaCita.ToString("dd/MM/yyyy"),
          Hora = cita.HoraCita.ToString(@"hh\:mm"),
          Medico = $"{cita.Medico.Nombre} {cita.Medico.ApellidoPaterno} {cita.Medico.ApellidoMaterno}",
          Especialidad = cita.Especialidad,
          Consultorio = cita.Consultorio,
          Estado = cita.EstadoCita
        };

        _logger.LogInformation("Detalles de la cita {Id} cargados correctamente.", id); // ✅ Actualizado log

        return Json(new { success = true, data = detalles });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al cargar los detalles de la cita {Id}", id); // ✅ Actualizado log
        return Json(new { success = false, message = "No se pudieron cargar los detalles de esta cita. Intente de nuevo." });
      }
    }
  }
}