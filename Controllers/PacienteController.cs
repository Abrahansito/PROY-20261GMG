using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using SGMG.Services;
using SGMG.Dtos.Response;
using SGMG.Dtos.Request.Paciente;
using PROY_20252SGMG.Dtos.Response;
using SGMG.Data;
using PROY_20252SGMG.ViewsModels;
using Twilio;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace SGMG.Controllers
{
  [Route("[controller]")]
  public class PacienteController : Controller
  {
    private readonly ILogger<PacienteController> _logger;
    private readonly IPacienteService _pacienteService;
    private readonly ApplicationDbContext _context;
    private readonly IConfiguration _configuration;

    public PacienteController(
        ILogger<PacienteController> logger,
        IPacienteService pacienteService,
        ApplicationDbContext context,
        IConfiguration configuration)
    {
      _logger = logger;
      _pacienteService = pacienteService;
      _context = context;
      _configuration = configuration;
    }

    [HttpGet]
    [Route("/pacientes/all")]
    public async Task<GenericResponse<IEnumerable<Paciente>>> GetAllPacientes()
    {
      return await _pacienteService.GetAllPacientesAsync();
    }

    [HttpGet]
    [Route("/pacientes/search")]
    public async Task<GenericResponse<Paciente>> SearchPaciente([FromQuery] string tipoDocumento, [FromQuery] string numeroDocumento)
    {
      return await _pacienteService.SearchPacienteByDocumentoAsync(tipoDocumento, numeroDocumento);
    }

    [HttpGet]
    [Route("/pacientes/{id}/citas-pendientes")]
    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasPendientesByPaciente(int id)
    {
      return await _pacienteService.GetCitasPendientesByPacienteAsync(id);
    }

    [HttpGet]
    [Route("/pacientes/{id}")]
    public async Task<GenericResponse<Paciente>> GetPacienteById(int id)
    {
      return await _pacienteService.GetPacienteByIdAsync(id);
    }

    [HttpPost]
    [Route("/pacientes/register")]
    public async Task<GenericResponse<Paciente>> CreatePaciente([FromBody] PacienteRequestDTO pacienteRequestDTO)
    {
      return await _pacienteService.AddPacienteAsync(pacienteRequestDTO);
    }

    [HttpPut]
    [Route("/pacientes/update")]
    public async Task<GenericResponse<Paciente>> UpdatePaciente([FromBody] PacienteRequestDTO pacienteRequestDTO)
    {
      return await _pacienteService.UpdatePacienteAsync(pacienteRequestDTO);
    }

    [HttpDelete]
    [Route("/pacientes/delete/{id}")]
    public async Task<GenericResponse<Paciente>> DeletePaciente(int id)
    {
      return await _pacienteService.DeletePacienteAsync(id);
    }

    [HttpGet]
    [Route("/pacientes/{id}/derivaciones")]
    public async Task<GenericResponse<IEnumerable<DerivacionResponseDTO>>> GetDerivacionesByPaciente(int id)
    {
      return await _pacienteService.GetDerivacionesByPacienteAsync(id);
    }

    /// <summary>
    /// PRUEBA DE ACEPTACIÓN 2: Visualizar resumen de recordatorio
    /// Genera la vista previa del recordatorio antes de enviarlo
    /// </summary>
    [HttpGet]
    [Route("/pacientes/confirmar-recordatorio/{idCita}")]
    public async Task<IActionResult> ConfirmarRecordatorio(int idCita)
    {
      try
      {
        //throw new Exception("Error simulado para pruebas");
        _logger.LogInformation($"=== GENERANDO VISTA PREVIA DE RECORDATORIO - Cita ID: {idCita} ===");

        // Consultar la cita con sus relaciones
        var cita = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Where(c => c.IdCita == idCita)
            .FirstOrDefaultAsync();

        // PRUEBA 2.2: Error al generar la vista previa
        if (cita == null)
        {
          TempData["ErrorMessage"] = "Ocurrió un error al generar la vista previa del recordatorio. Por favor, intente nuevamente.";
          return RedirectToAction("VisualPaciente", "Home");
        }

        // PRUEBA 2.1: Paciente sin número de teléfono
        if (string.IsNullOrWhiteSpace(cita.Paciente?.Telefono))
        {
          TempData["ErrorMessage"] = "No se puede enviar el recordatorio porque el paciente no tiene un número de teléfono registrado.";
          return RedirectToAction("VisualPaciente", "Home");
        }

        // Construir el nombre completo del paciente
        string nombrePaciente = $"{cita.Paciente.ApellidoPaterno} {cita.Paciente.ApellidoMaterno}, {cita.Paciente.Nombre}".Trim();

        // Construir el nombre completo del médico
        string nombreMedico = $"Dr. {cita.Medico.Nombre} {cita.Medico.ApellidoPaterno}".Trim();

        // Formatear fecha y hora
        string fechaCita = cita.FechaCita.ToString("dd/MM/yyyy");
        string horaCita = cita.HoraCita.ToString(@"hh\:mm");

        // Construir el mensaje según el formato solicitado
        string mensaje = $@"Estimado(a) {nombrePaciente}

Le recordamos su cita médica programada:

🗓️ Fecha: {fechaCita}
⏰ Hora: {horaCita}
👨‍⚕️ Médico: {nombreMedico}
🏥 Consultorio: {cita.Consultorio}
🩺 Servicio: MEDICINA GENERAL

Por favor, llegue 15 minutos antes de su cita.

Hospital Nacional Hipólito Unanue
Consultas: (01) 362-0220";

        // Crear el ViewModel para la vista
        var viewModel = new ConfirmarRecordatorioViewModel
        {
          IdCita = cita.IdCita,
          NombreCompletoPaciente = nombrePaciente,
          Dni = cita.Paciente.NumeroDocumento,
          Telefono = cita.Paciente.Telefono,
          Edad = cita.Paciente.Edad,
          Fecha = fechaCita,
          Hora = horaCita,
          Medico = nombreMedico,
          Consultorio = cita.Consultorio,
          Especialidad = "MEDICINA GENERAL",
          Estado = cita.EstadoCita,
          MensajeWhatsApp = mensaje,
          NombreHospital = "Hospital Nacional Hipólito Unanue",
          TelefonoHospital = "(01) 362-0220"
        };

        _logger.LogInformation($"Vista previa generada exitosamente para cita {idCita}");

        // PRUEBA 2: Se muestra el resumen del recordatorio
        return View("ConfirmarRecordatorio", viewModel);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, $"Error al generar vista previa del recordatorio para cita {idCita}");
        // PRUEBA 2.2: Error al generar la vista previa
        TempData["ErrorMessage"] = "Ocurrió un error al generar la vista previa del recordatorio. Por favor, intente nuevamente.";
        return RedirectToAction("Paciente", "Home");
      }
    }

    /// <summary>
    /// PRUEBA DE ACEPTACIÓN 3: Enviar recordatorio por WhatsApp
    /// Envía el mensaje de WhatsApp usando Twilio
    /// </summary>
    [HttpPost]
    [Route("/pacientes/enviar-recordatorio-whatsapp/{idCita}")]
    public async Task<IActionResult> EnviarRecordatorioWhatsApp(int idCita)
    {
      try
      {
        _logger.LogInformation($"=== INICIANDO ENVÍO DE WHATSAPP - Cita ID: {idCita} ===");

        // Consultar la cita
        var cita = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Where(c => c.IdCita == idCita)
            .FirstOrDefaultAsync();

        if (cita == null)
        {
          return Ok(new GenericResponse<string>
          {
            Success = false,
            Message = "No se encontró la cita especificada."
          });
        }

        if (string.IsNullOrWhiteSpace(cita.Paciente?.Telefono))
        {
          return Ok(new GenericResponse<string>
          {
            Success = false,
            Message = "No se puede enviar el recordatorio porque el paciente no tiene un número de teléfono registrado."
          });
        }

        // Construir datos
        string nombrePaciente = $"{cita.Paciente.ApellidoPaterno} {cita.Paciente.ApellidoMaterno}, {cita.Paciente.Nombre}".Trim();
        string nombreMedico = $"Dr. {cita.Medico.Nombre} {cita.Medico.ApellidoPaterno}".Trim();
        string fechaCita = cita.FechaCita.ToString("dd/MM/yyyy");
        string horaCita = cita.HoraCita.ToString(@"hh\:mm");

        // Construir mensaje según formato solicitado
        string mensaje = $@"Estimado(a) {nombrePaciente}

Le recordamos su cita médica programada:

🗓️ Fecha: {fechaCita}
⏰ Hora: {horaCita}
👨‍⚕️ Médico: {nombreMedico}
🏥 Consultorio: {cita.Consultorio}
🩺 Servicio: MEDICINA GENERAL

Por favor, llegue 15 minutos antes de su cita.

Hospital Nacional Hipólito Unanue
Consultas: (01) 362-0220";

        // Formatear teléfono
        string telefonoFormateado = FormatearNumeroParaWhatsApp(cita.Paciente.Telefono);

        // Obtener credenciales de Twilio
        var accountSid = _configuration["Twilio:AccountSid"];
        var authToken = _configuration["Twilio:AuthToken"];
        var whatsappFrom = _configuration["Twilio:WhatsAppFrom"];

        if (string.IsNullOrWhiteSpace(accountSid) || string.IsNullOrWhiteSpace(authToken))
        {
          // PRUEBA 3.1: Error de conexión con WhatsApp
          return Ok(new GenericResponse<string>
          {
            Success = false,
            Message = "No se pudo conectar con WhatsApp. Por favor, verifique su conexión a internet e inténtelo de nuevo."
          });
        }

        // Inicializar Twilio
        TwilioClient.Init(accountSid, authToken);

        // Enviar mensaje
        var messageResource = await MessageResource.CreateAsync(
            body: mensaje,
            from: new PhoneNumber(whatsappFrom),
            to: new PhoneNumber($"whatsapp:{telefonoFormateado}")
        );

        _logger.LogInformation($"Mensaje enviado - SID: {messageResource.Sid}");

        if (messageResource.ErrorCode.HasValue)
        {
          // PRUEBA 3.1: Error de conexión con WhatsApp
          return Ok(new GenericResponse<string>
          {
            Success = false,
            Message = "No se pudo conectar con WhatsApp. Por favor, verifique su conexión a internet e inténtelo de nuevo."
          });
        }

        // PRUEBA 3: Envío exitoso
        return Ok(new GenericResponse<string>
        {
          Success = true,
          Message = $"Recordatorio enviado exitosamente a {nombrePaciente} por WhatsApp.",
          Data = messageResource.Sid
        });
      }
      catch (Twilio.Exceptions.ApiException twilioEx)
      {
        _logger.LogError(twilioEx, "Error de Twilio API");
        // PRUEBA 3.1: Error de conexión con WhatsApp
        return Ok(new GenericResponse<string>
        {
          Success = false,
          Message = "No se pudo conectar con WhatsApp. Por favor, verifique su conexión a internet e inténtelo de nuevo."
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error general al enviar WhatsApp");
        // PRUEBA 3.1: Error de conexión con WhatsApp
        return Ok(new GenericResponse<string>
        {
          Success = false,
          Message = "No se pudo conectar con WhatsApp. Por favor, verifique su conexión a internet e inténtelo de nuevo."
        });
      }
    }

    private string FormatearNumeroParaWhatsApp(string telefono)
    {
      if (string.IsNullOrWhiteSpace(telefono))
      {
        throw new ArgumentException("El número de teléfono no puede estar vacío");
      }

      string numeroLimpio = new string(telefono.Where(char.IsDigit).ToArray());

      if (numeroLimpio.Length == 9)
      {
        return $"+51{numeroLimpio}";
      }

      if (numeroLimpio.StartsWith("51") && numeroLimpio.Length == 11)
      {
        return $"+{numeroLimpio}";
      }

      if (numeroLimpio.StartsWith("51") || telefono.StartsWith("+51"))
      {
        return telefono.StartsWith("+") ? telefono : $"+{numeroLimpio}";
      }

      return $"+51{numeroLimpio}";
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
      return View("Error!");
    }
  }
}