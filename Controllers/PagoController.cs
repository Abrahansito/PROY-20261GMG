// ...existing code...
using Microsoft.AspNetCore.Mvc;
using SGMG.Services;
using SGMG.common.exception;
using System;
using System.Linq;
using System.Collections.Generic;
using SGMG.Dtos.Response; // <-- agregado
using SGMG.Models;
using SGMG.Repository;
using SGMG.Data;
// ...existing code...
namespace SGMG.Controllers
{
  public class PagoController : Controller
  {
    private readonly IPagoService _pagoService;
    private readonly IPagoRepository _pagoRepository;
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PagoController> _logger;
    private readonly ICitaRepository _citaRepository;

    public PagoController(IPagoService pagoService, IPagoRepository pagoRepository, ApplicationDbContext context, ILogger<PagoController> logger, ICitaRepository citaRepository)
    {
      _pagoService = pagoService;
      _pagoRepository = pagoRepository;
      _context = context;
      _logger = logger;
      _citaRepository = citaRepository;
    }

    [HttpGet]
    [NoValidation]
    [Route("/pagos/all")]
    public IActionResult Historial(string tipoBusqueda, string numeroDocumento, DateTime? inicio, DateTime? fin, string estado)
    {
      // El controlador delega la lógica al servicio
      var tipoDocumento = tipoBusqueda;
      var pagosDto = _pagoService.GetHistorialPagosDto(tipoDocumento, numeroDocumento, inicio, fin, estado);
      // La vista real está en Views/Home/Historial.cshtml, indicar la ruta completa para evitar búsquedas en /Views/Pago/
      var model = pagosDto ?? new List<PagoResponseDTO>();
      return View("~/Views/Home/Historial.cshtml", model);
    }

    [HttpGet]
    [Route("/pagos/search")]
    public ActionResult<GenericResponse<IEnumerable<PagoResponseDTO>>> Search(string tipoBusqueda, string numeroDocumento, DateTime? inicio, DateTime? fin, string estado)
    {
      var tipoDocumento = tipoBusqueda;
      var pagosDto = _pagoService.GetHistorialPagosDto(tipoDocumento, numeroDocumento, inicio, fin, estado) ?? Enumerable.Empty<PagoResponseDTO>();
      var resp = new GenericResponse<IEnumerable<PagoResponseDTO>>
      {
        Success = true,
        Message = "OK",
        Data = pagosDto
      };
      return Ok(resp);
    }

    [HttpGet]
    [Route("/pagos/resumen/{id}")]
    public IActionResult Resumen(int id)
    {
      _logger.LogInformation($"Solicitando resumen para Cita ID={id}");

      // Usar el nuevo método que busca por ID de Cita
      var pagoDto = _pagoService.GetResumenByCitaId(id);

      if (pagoDto == null)
      {
        _logger.LogWarning($"No se encontró información para la Cita ID={id}");
        return NotFound();
      }

      _logger.LogInformation($"Mostrando resumen de Cita ID={id}, Paciente={pagoDto.Nombres}");

      return View("~/Views/Home/Resumen.cshtml", pagoDto);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    [Route("/pagos/pagar/{id}")]
    public IActionResult Pagar(int id)
    {
      _logger.LogInformation($"=== INICIO Pagar (Cita ID={id}) ===");

      try
      {
        // Obtener la cita directamente desde el repositorio
        var cita = _pagoRepository.GetCitaById(id);

        if (cita == null)
        {
          _logger.LogWarning($"No se encontró la Cita ID={id}");
          return NotFound();
        }

        _logger.LogInformation($"Cita encontrada: ID={cita.IdCita}, Paciente={cita.Paciente?.NumeroDocumento}, Estado actual: {cita.EstadoCita}");
        _logger.LogInformation($"Cantidad de pagos asociados: {cita.Pagos?.Count ?? 0}");

        // Verificar si ya tiene un pago
        var pagoExistente = cita.Pagos?.FirstOrDefault();

        if (pagoExistente != null)
        {
          _logger.LogInformation($"La cita ya tiene un pago ID={pagoExistente.IdPago}, Estado={pagoExistente.EstadoPago}");

          // Si el pago existe pero está Pendiente, actualizarlo a Pagado
          if (pagoExistente.EstadoPago == "Pendiente")
          {
            _logger.LogInformation($"Actualizando pago existente ID={pagoExistente.IdPago} a estado Pagado");
            var actualizado = _pagoRepository.UpdatePagoStatus(pagoExistente.IdPago, "Pagado", DateTime.Now);

            if (actualizado)
            {
              // ACTUALIZAR EL ESTADO DE LA CITA A "Pagado"
              cita.EstadoCita = "Pagado";
              _context.Citas.Update(cita);
              _context.SaveChanges();

              _logger.LogInformation($"✅ Pago ID={pagoExistente.IdPago} actualizado a Pagado exitosamente");
              _logger.LogInformation($"✅ Estado de cita actualizado a: {cita.EstadoCita}");
              TempData["Success"] = "Pago procesado exitosamente";
            }
            else
            {
              _logger.LogError($"❌ Error al actualizar el pago ID={pagoExistente.IdPago}");
              TempData["Error"] = "Error al procesar el pago";
            }
          }
          else
          {
            _logger.LogInformation($"El pago ya está en estado: {pagoExistente.EstadoPago}");
            TempData["Info"] = $"El pago ya está {pagoExistente.EstadoPago}";
          }
        }
        else
        {
          _logger.LogWarning($"⚠️ La cita ID={id} NO tiene pago asociado - Creando nuevo pago");

          // Crear un nuevo registro de pago directamente con el contexto
          var nuevoPago = new Pago
          {
            IdCita = id,
            CodigoServicio = "SERV-001",
            DescripcionServicio = "Consulta Medicina General",
            Cantidad = 1,
            PrecioUnitario = 50.0m,
            Subtotal = 50.0m,
            Igv = 9.0m,
            Total = 59.0m,
            EstadoPago = "Pagado",
            FechaPago = DateTime.Now
          };

          _context.Pagos.Add(nuevoPago);

          // ACTUALIZAR EL ESTADO DE LA CITA A "Pagado"
          cita.EstadoCita = "Pagado";
          _context.Citas.Update(cita);

          var filasAfectadas = _context.SaveChanges();

          if (filasAfectadas > 0)
          {
            _logger.LogInformation($"✅ Nuevo pago creado exitosamente: ID={nuevoPago.IdPago}, Total={nuevoPago.Total}");
            _logger.LogInformation($"✅ Estado de cita actualizado a: {cita.EstadoCita}");
            TempData["Success"] = "Pago creado y procesado exitosamente";
          }
          else
          {
            _logger.LogError($"❌ Error al crear el pago para la Cita ID={id}");
            TempData["Error"] = "No se pudo crear el pago";
          }
        }

        _logger.LogInformation("=== FIN Pagar ===");
        return RedirectToAction("Resumen", new { id });
      }
      catch (Exception ex)
      {
        _logger.LogError($"❌ Excepción en Pagar: {ex.Message}");
        _logger.LogError($"StackTrace: {ex.StackTrace}");
        TempData["Error"] = "Ocurrió un error al procesar el pago";
        return RedirectToAction("Resumen", new { id });
      }
    }
  }
}