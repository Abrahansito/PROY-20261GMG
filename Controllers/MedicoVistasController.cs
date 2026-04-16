// Controllers/MedicoVistasController.cs - VERSIÓN CORREGIDA
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using SGMG.Services;
using SGMG.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;

namespace SGMG.Controllers
{
    [Authorize(Roles = "MEDICO")]
    public class MedicoVistasController : Controller
    {
        private readonly ICitaService _citaService;
        private readonly IMedicoService _medicoService;
        private readonly ILogger<MedicoVistasController> _logger;

        public MedicoVistasController(
            ICitaService citaService, 
            IMedicoService medicoService,
            ILogger<MedicoVistasController> logger)
        {
            _citaService = citaService;
            _medicoService = medicoService;
            _logger = logger;
        }

        [HttpGet]
        [Route("/medico/pacientes-por-atender")]
        public async Task<IActionResult> PacientesPorAtender(string filtro = "")
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "No se pudo identificar al usuario";
                    return RedirectToAction("Index", "Home");
                }

                var medicoResponse = await _medicoService.GetMedicoByUserIdAsync(userId);
                
                if (medicoResponse == null || !medicoResponse.Success.GetValueOrDefault() || medicoResponse.Data == null)
                {
                    TempData["Error"] = "No se pudo encontrar la información del médico";
                    return RedirectToAction("Index", "Home");
                }

                var medico = medicoResponse.Data;
                var fechaHoy = DateTime.Today;
                var consultorio = medico.ConsultorioAsignado?.Nombre ?? medico.AreaServicio;

                GenericResponse<IEnumerable<CitaPorAtenderDTO>> citasResponse;

                if (string.IsNullOrEmpty(filtro))
                {
                    citasResponse = await _citaService.GetCitasPorAtenderAsync(medico.IdMedico, fechaHoy, consultorio);
                }
                else
                {
                    citasResponse = await _citaService.GetCitasPorAtenderFiltradasAsync(medico.IdMedico, fechaHoy, consultorio, filtro);
                }

                var citas = (citasResponse != null && citasResponse.Success.GetValueOrDefault()) 
                    ? citasResponse.Data ?? new List<CitaPorAtenderDTO>() 
                    : new List<CitaPorAtenderDTO>();

                // CORREGIDO: Usar .Count() para IEnumerable<T>
                int totalCitas = citas.Count();

                ViewBag.MedicoNombre = $"{medico.Nombre} {medico.ApellidoPaterno}";
                ViewBag.Consultorio = consultorio;
                ViewBag.FechaHoy = fechaHoy.ToString("dd/MM/yyyy");
                ViewBag.Filtro = filtro;
                ViewBag.TotalCitas = totalCitas;

                return View("~/Views/Medico/PacientesPorAtender.cshtml", citas);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar pacientes por atender");
                TempData["Error"] = "Error al cargar los pacientes";
                return RedirectToAction("Index", "Home");
            }
        }

        [HttpPost]
        [Route("/medico/pacientes-por-atender/buscar")]
        public IActionResult BuscarPacientesPorAtender(string filtro)
        {
            return RedirectToAction("PacientesPorAtender", new { filtro });
        }

        [HttpGet]
        [Route("/medico/dashboard")]
        public async Task<IActionResult> Dashboard()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["Error"] = "No se pudo identificar al usuario";
                    return RedirectToAction("Index", "Home");
                }

                var medicoResponse = await _medicoService.GetMedicoByUserIdAsync(userId);
                
                if (medicoResponse == null || !medicoResponse.Success.GetValueOrDefault() || medicoResponse.Data == null)
                {
                    TempData["Error"] = "No se pudo encontrar la información del médico";
                    return RedirectToAction("Index", "Home");
                }

                var medico = medicoResponse.Data;
                var fechaHoy = DateTime.Today;
                var consultorio = medico.ConsultorioAsignado?.Nombre ?? medico.AreaServicio;

                var citasResponse = await _citaService.GetCitasPorAtenderAsync(medico.IdMedico, fechaHoy, consultorio);
                
                var citasHoy = (citasResponse != null && citasResponse.Success.GetValueOrDefault()) 
                    ? citasResponse.Data ?? new List<CitaPorAtenderDTO>() 
                    : new List<CitaPorAtenderDTO>();

                // CORREGIDO: Usar .Count() para IEnumerable<T>
                int totalCitasHoy = citasHoy.Count();
                int citasAtendidas = citasHoy.Count(c => c.EstadoCita == "Atendida");
                int citasPendientes = citasHoy.Count(c => 
                    c.EstadoCita == "Programada" || 
                    c.EstadoCita == "Confirmada" || 
                    c.EstadoCita == "Reservada");

                ViewBag.MedicoNombre = $"{medico.Nombre} {medico.ApellidoPaterno}";
                ViewBag.Consultorio = consultorio;
                ViewBag.TotalCitasHoy = totalCitasHoy;
                ViewBag.CitasAtendidas = citasAtendidas;
                ViewBag.CitasPendientes = citasPendientes;

                return View("~/Views/Medico/Dashboard.cshtml");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cargar dashboard del médico");
                TempData["Error"] = "Error al cargar el dashboard";
                return RedirectToAction("Index", "Home");
            }
        }
    }
}