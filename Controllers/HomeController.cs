using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using SGMG.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using Microsoft.CodeAnalysis.Elfie.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGMG.Data;


namespace SGMG.Controllers;

public class HomeController : Controller
{
  private readonly ILogger<HomeController> _logger;
  private readonly ApplicationDbContext _context;

  public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
  {
    _logger = logger;
    _context = context;
  }

  public IActionResult Index()
  {
    return Redirect("/Identity/Account/Login");
  }

  public IActionResult Personal()
  {
    return View();
  }

  public IActionResult Receta(int? idPaciente, int? idCita, int? idMedico, int? idHistoriaClinica)
  {
    ViewBag.IdPaciente = idPaciente.HasValue && idPaciente.Value > 0 ? idPaciente : null;
    ViewBag.IdCita = idCita.HasValue && idCita.Value > 0 ? idCita : null;
    ViewBag.IdMedico = idMedico.HasValue && idMedico.Value > 0 ? idMedico : null;
    ViewBag.IdHistoriaClinica = idHistoriaClinica.HasValue && idHistoriaClinica.Value > 0 ? idHistoriaClinica : null;
    _logger.LogInformation("Accediendo a la vista de generación de receta médica.");
    return View();
  }


  [HttpGet("Home/HistorialRecetas/{idPaciente}")]
  public IActionResult HistorialRecetas(int idPaciente, int? idCita, int? idMedico)
  {
    if (idPaciente <= 0)
    {
      TempData["Error"] = "ID de paciente inválido";
      return RedirectToAction("Index");
    }

   
    ViewBag.IdPaciente = idPaciente;
    ViewBag.IdCita = idCita.HasValue && idCita.Value > 0 ? idCita : null;
    ViewBag.IdMedico = idMedico.HasValue && idMedico.Value > 0 ? idMedico : null;

    return View();
  }

  public IActionResult Paciente()
  {
    return View();
  }
  public IActionResult VisualCitas(int? idPaciente, int? idCita)
  {
    // Solo enviar a la vista si los parámetros son válidos
    ViewBag.IdPaciente = idPaciente.HasValue && idPaciente.Value > 0 ? idPaciente : null;
    ViewBag.IdCita = idCita.HasValue && idCita.Value > 0 ? idCita : null;

    return View();
  }

  public IActionResult Privacy()
  {
    return View();
  }
  public IActionResult Historial()
  {
    return View();
  }
  public IActionResult HorarioMedico(int? idMedico, int? idPaciente, int? idCita, int? semana)
  {
    if (idMedico == null || idMedico == 0)
    {
      return RedirectToAction("VisualCitas");
    }

    ViewBag.Semana = semana ?? 0;
    ViewBag.IdMedico = idMedico;
    ViewBag.IdPaciente = idPaciente.HasValue && idPaciente.Value > 0 ? idPaciente : null;
    ViewBag.IdCita = idCita.HasValue && idCita.Value > 0 ? idCita : null;
    ViewData["Title"] = $"Horario del Médico - ID: {idMedico}";

    return View();
  }


  [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
  public IActionResult Error(string? mensaje = null)
  {
    ViewBag.ErrorMessage = mensaje ?? "Ha ocurrido un error inesperado. Por favor, inténtalo nuevamente.";
    return View("ErrorCustom");
  }
  public IActionResult Error404()
  {
    Response.StatusCode = 404;
    return View();
  }

}
