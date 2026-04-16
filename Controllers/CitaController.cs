using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SGMG.Data;
using SGMG.Models;
using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.Extensions.Logging;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;
using SGMG.Services;


using System.Threading.Tasks;
using SGMG.Repository;
using PROY_20252SGMG.Dtos.Request;

namespace SGMG.Controllers
{
  public class CitaController : Controller
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CitaController> _logger;
    private readonly ICitaService _citaService;
    private readonly ICitaRepository _citaRepository;

    public CitaController(ApplicationDbContext context, ILogger<CitaController> logger, ICitaService citaService, ICitaRepository citaRepository)
    {
      _context = context;
      _logger = logger;
      _citaService = citaService;
      _citaRepository = citaRepository;
    }
// Indica que este controlador es de tipo API (validaciones automáticas, etc.)
    [HttpGet]
    public IActionResult Index()
    {
      return View();
    }


    [HttpGet]
    [Route("/citas/pendientes")]
    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasPendientes()
    {
      return await _citaService.GetCitasPendientesAsync();
    }

    //Obtener citas fuera de horario (sin filtros)
    [HttpGet]
    [Route("/citas/fuera-horario")]
    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasFueraHorario()
    {
      return await _citaService.GetCitasFueraHorarioAsync();
    }


    //Buscar citas pendientes con filtros   
    [HttpGet]
    [Route("/citas/buscar-pendientes")]
    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> BuscarCitasPendientes([FromQuery] string? tipoDoc, [FromQuery] string? numeroDoc)
    {
      return await _citaService.BuscarCitasPendientesAsync(tipoDoc, numeroDoc);
    }

    [HttpGet]
    [Route("/citas/buscar-fuera-horario")]
    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> BuscarCitasFueraHorario([FromQuery] string? tipoDoc, [FromQuery] string? numeroDoc)
    {
      return await _citaService.BuscarCitasFueraHorarioAsync(tipoDoc, numeroDoc);
    }

    [HttpGet]
    [Route("/citas/all")]
    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetAllCitas()
    {
      return await _citaService.GetAllCitasAsync();
    }


    [HttpGet]
    [Route("/citas/{id}")]
    public async Task<GenericResponse<CitaResponseDTO>> GetCitaById(int id)
    {
      return await _citaService.GetCitaByIdAsync(id);
    }

    //MEDICO Y PACIENTE - VISTA HORARIO Y RESERVA CITA
    public IActionResult HorarioMedico(int? idMedico, int? idPaciente, int? semana)
    {
      if (idMedico == null || idMedico == 0)
      {
        _logger.LogWarning("Intento de acceder a HorarioMedico sin idMedico válido");
        return RedirectToAction("VisualCitas");
      }

      _logger.LogInformation($"Acceso a HorarioMedico - IdMedico: {idMedico}, IdPaciente: {idPaciente}, Semana: {semana}");

      ViewBag.Semana = semana ?? 0;
      ViewBag.IdMedico = idMedico;
      ViewBag.IdPaciente = idPaciente;
      ViewData["Title"] = $"Horario del Médico - ID: {idMedico}";

      return View();
    }
    [HttpPut]
    [Route("/ReprogramarCita")]
    public async Task<IActionResult> ReprogramarCita([FromBody] ReprogramarCitaRequest request)
    {
      try
      {
        //throw new Exception("Error de prueba para logging");

        if (request.IdCita <= 0)
          return Json(new { success = false, mensaje = "ID de cita inválido" });

        if (request.IdMedico <= 0 || request.IdPaciente <= 0)
          return Json(new { success = false, mensaje = "Datos incompletos" });

        if (string.IsNullOrEmpty(request.FechaCita) || string.IsNullOrEmpty(request.HoraCita))
          return Json(new { success = false, mensaje = "Fecha u hora inválidas" });

        // Buscar la cita existente
        var cita = await _citaRepository.GetCitaByIdAsync(request.IdCita);
        if (cita == null)
          return Json(new { success = false, mensaje = "Cita no encontrada" });

        // Verificar que la cita pertenece al paciente
        if (cita.IdPaciente != request.IdPaciente)
          return Json(new { success = false, mensaje = "La cita no pertenece al paciente especificado" });

        // GUARDAR DATOS ANTERIORES PARA DEVOLVERLOS
        var fechaAnterior = cita.FechaCita.ToString("yyyy-MM-dd");
        var horaAnterior = cita.HoraCita.ToString(@"hh\:mm");
        var medicoAnterior = cita.IdMedico;

        // Verificar que el nuevo horario esté disponible
        var fechaCita = DateTime.Parse(request.FechaCita);
        var horaCita = TimeSpan.Parse(request.HoraCita);

        // Verificar si ya existe otra cita en el nuevo horario
        var citaExistente = await _context.Citas
            .Where(c => c.IdMedico == request.IdMedico &&
                       c.FechaCita == fechaCita &&
                       c.HoraCita == horaCita &&
                       c.IdCita != request.IdCita)
            .FirstOrDefaultAsync();

        if (citaExistente != null)
          return Json(new { success = false, mensaje = "El horario seleccionado ya está ocupado" });

        // ACTUALIZAR DISPONIBILIDAD DEL MÉDICO ANTERIOR (si cambió de médico)
        if (medicoAnterior != request.IdMedico)
        {
          var hoy = DateTime.Today;
          var diasDesdeInicio = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
          if (diasDesdeInicio < 0) diasDesdeInicio += 7;

          var inicioSemanaBase = hoy.AddDays(-diasDesdeInicio);

          // Calcular semana de la cita anterior
          var diasDiferencia = (cita.FechaCita.Date - inicioSemanaBase.Date).Days;
          var semanaAnterior = diasDiferencia / 7;
          var inicioSemanaAnterior = inicioSemanaBase.AddDays(semanaAnterior * 7).Date;

          var disponibilidadAnterior = await _context.DisponibilidadesSemanales
              .FirstOrDefaultAsync(d => d.IdMedico == medicoAnterior &&
                                       d.FechaInicioSemana.Date == inicioSemanaAnterior.Date);

          if (disponibilidadAnterior != null && disponibilidadAnterior.CitasActuales > 0)
          {
            disponibilidadAnterior.CitasActuales--;
            _logger.LogInformation($"✅ Disponibilidad del médico anterior actualizada: {disponibilidadAnterior.CitasActuales}/{disponibilidadAnterior.CitasMaximas}");
          }
        }

        // Actualizar la cita
        cita.IdMedico = request.IdMedico;
        cita.FechaCita = fechaCita;
        cita.HoraCita = horaCita;
        cita.EstadoCita = "Pendiente";

        await _citaRepository.UpdateCitaAsync(cita);

        // ACTUALIZAR DISPONIBILIDAD DEL NUEVO MÉDICO (si cambió de médico)
        if (medicoAnterior != request.IdMedico)
        {
          var hoy = DateTime.Today;
          var diasDesdeInicio = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
          if (diasDesdeInicio < 0) diasDesdeInicio += 7;

          var inicioSemanaBase = hoy.AddDays(-diasDesdeInicio);
          var inicioSemana = inicioSemanaBase.AddDays(request.Semana * 7).Date;

          var disponibilidad = await _context.DisponibilidadesSemanales
              .FirstOrDefaultAsync(d => d.IdMedico == request.IdMedico &&
                                       d.FechaInicioSemana.Date == inicioSemana.Date);

          if (disponibilidad != null)
          {
            disponibilidad.CitasActuales++;
            _logger.LogInformation($"✅ Disponibilidad del nuevo médico actualizada: {disponibilidad.CitasActuales}/{disponibilidad.CitasMaximas}");
          }
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation($"✅ Cita reprogramada exitosamente: #{request.IdCita}");
        _logger.LogInformation($"   Anterior: {fechaAnterior} {horaAnterior}");
        _logger.LogInformation($"   Nueva: {cita.FechaCita:yyyy-MM-dd} {cita.HoraCita:hh\\:mm}");

        return Json(new
        {
          success = true,
          mensaje = "Cita reprogramada exitosamente",
          data = new
          {
            idCita = cita.IdCita,
            // DATOS NUEVOS
            fechaCita = cita.FechaCita.ToString("yyyy-MM-dd"),
            horaCita = cita.HoraCita.ToString(@"hh\:mm"),
            // DATOS ANTERIORES PARA DESBLOQUEAR EN EL CALENDARIO
            fechaAnterior = fechaAnterior,
            horaAnterior = horaAnterior,
            cambioMedico = medicoAnterior != request.IdMedico
          }
        });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al reprogramar cita");

        // Retornar JSON con el mensaje de error
        return Json(new
        {
          success = false,
          mensaje = "No se pudo reprogramar la cita en estos momentos. Por favor, intente nuevamente."
        });
      }
    }

    [HttpGet]
    public IActionResult ObtenerDatosCalendario(int idMedico, int semana)
    {
      try
      {
        _logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║           INICIO ObtenerDatosCalendario                        ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");
        _logger.LogInformation($"📋 Parámetros recibidos:");
        _logger.LogInformation($"   → IdMedico: {idMedico}");
        _logger.LogInformation($"   → Semana: {semana}");

        // Buscar médico
        _logger.LogInformation($"🔍 Buscando médico con ID {idMedico}...");
        var medico = _context.Medicos
            .Include(m => m.ConsultorioAsignado)
            .FirstOrDefault(m => m.IdMedico == idMedico);

        if (medico == null)
        {
          _logger.LogWarning($"❌ Médico con ID {idMedico} NO ENCONTRADO");
          return Json(new { error = true, mensaje = "Médico no encontrado" });
        }

        _logger.LogInformation($"✅ Médico encontrado:");
        _logger.LogInformation($"   → Nombre: {medico.Nombre} {medico.ApellidoPaterno} {medico.ApellidoMaterno}");
        _logger.LogInformation($"   → Turno: {medico.Turno}");
        _logger.LogInformation($"   → Consultorio: {medico.ConsultorioAsignado?.Nombre ?? "No asignado"}");

        // Calcular rango de fechas de la semana
        _logger.LogInformation($"📅 Calculando rango de fechas...");
        var hoy = DateTime.Today;
        _logger.LogInformation($"   → Fecha de hoy: {hoy:yyyy-MM-dd} ({hoy:dddd})");

        var diasDesdeInicio = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
        if (diasDesdeInicio < 0) diasDesdeInicio += 7;

        _logger.LogInformation($"   → Días desde el lunes: {diasDesdeInicio}");

        var inicioSemanaBase = hoy.AddDays(-diasDesdeInicio);
        _logger.LogInformation($"   → Inicio semana base (lunes actual): {inicioSemanaBase:yyyy-MM-dd}");

        var inicioSemana = inicioSemanaBase.AddDays(semana * 7).Date;
        var finSemana = inicioSemana.AddDays(6);

        _logger.LogInformation($"   → Rango buscado:");
        _logger.LogInformation($"      • Inicio: {inicioSemana:yyyy-MM-dd dddd}");
        _logger.LogInformation($"      • Fin:    {finSemana:yyyy-MM-dd dddd}");

        // Obtener TODAS las disponibilidades del médico
        _logger.LogInformation($"🔍 Buscando disponibilidades del médico en BD...");
        var todasDisponibilidades = _context.DisponibilidadesSemanales
            .Where(d => d.IdMedico == idMedico)
            .OrderBy(d => d.FechaInicioSemana)
            .ToList();

        _logger.LogInformation($"📊 Total de disponibilidades en BD: {todasDisponibilidades.Count}");

        if (todasDisponibilidades.Count == 0)
        {
          _logger.LogWarning($"⚠️ El médico NO tiene ninguna disponibilidad registrada");
        }
        else
        {
          _logger.LogInformation($"📋 Listado de disponibilidades:");
          int contador = 1;
          foreach (var disp in todasDisponibilidades)
          {
            _logger.LogInformation($"   [{contador}] ID: {disp.IdDisponibilidad}");
            _logger.LogInformation($"       → Inicio: {disp.FechaInicioSemana:yyyy-MM-dd}");
            _logger.LogInformation($"       → Fin:    {disp.FechaFinSemana:yyyy-MM-dd}");
            _logger.LogInformation($"       → Citas:  {disp.CitasActuales}/{disp.CitasMaximas}");
            contador++;
          }
        }

        // Comparar solo las fechas sin hora
        _logger.LogInformation($"🔍 Buscando disponibilidad para fecha inicio: {inicioSemana:yyyy-MM-dd}");
        var disponibilidad = todasDisponibilidades
            .FirstOrDefault(d => d.FechaInicioSemana.Date == inicioSemana.Date);

        if (disponibilidad == null)
        {
          _logger.LogWarning($"❌ NO SE ENCONTRÓ disponibilidad para la semana solicitada");
          _logger.LogWarning($"   → Fecha buscada: {inicioSemana:yyyy-MM-dd}");
          _logger.LogWarning($"   → Solución: Registrar disponibilidad para esta semana en la tabla DisponibilidadesSemanales");

          return Json(new
          {
            error = true,
            mensaje = "Semana aún no establecida para el médico"
          });
        }

        _logger.LogInformation($"✅ Disponibilidad ENCONTRADA:");
        _logger.LogInformation($"   → ID Disponibilidad: {disponibilidad.IdDisponibilidad}");
        _logger.LogInformation($"   → Citas Actuales: {disponibilidad.CitasActuales}");
        _logger.LogInformation($"   → Citas Máximas: {disponibilidad.CitasMaximas}");
        _logger.LogInformation($"   → Disponibles: {disponibilidad.CitasMaximas - disponibilidad.CitasActuales}");

        // IMPORTANTE: Obtener TODAS las citas (sin importar el estado)
        // Esto asegura que los horarios ocupados se muestren correctamente
        _logger.LogInformation($"🔍 Buscando citas ocupadas en el rango de fechas...");
        var citasOcupadas = _context.Citas
            .Where(c => c.IdMedico == idMedico &&
                       c.FechaCita >= inicioSemana &&
                       c.FechaCita <= finSemana)
            .Select(c => new
            {
              fecha = c.FechaCita.ToString("yyyy-MM-dd"),
              hora = c.HoraCita.ToString(@"hh\:mm"),
              estado = c.EstadoCita // Incluir estado para debugging
            })
            .ToList();

        _logger.LogInformation($"📊 Total de citas ocupadas: {citasOcupadas.Count}");

        if (citasOcupadas.Count > 0)
        {
          _logger.LogInformation($"📋 Listado de horarios ocupados:");
          int contador = 1;
          foreach (var cita in citasOcupadas)
          {
            _logger.LogInformation($"   [{contador}] Fecha: {cita.fecha}, Hora: {cita.hora}, Estado: {cita.estado}");
            contador++;
          }
        }
        else
        {
          _logger.LogInformation($"✅ No hay citas ocupadas en esta semana (todos los horarios disponibles)");
        }

        // Generar fechas de la semana
        var fechasSemana = new List<string>();
        for (int i = 0; i < 7; i++)
        {
          fechasSemana.Add(inicioSemana.AddDays(i).ToString("yyyy-MM-dd"));
        }

        var nombreCompleto = $"{medico.Nombre} {medico.ApellidoPaterno} {medico.ApellidoMaterno}";

        _logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║           FIN ObtenerDatosCalendario - ÉXITO ✅                ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");

        // Enviar solo fecha y hora (sin estado) al frontend
        return Json(new
        {
          medicoNombre = nombreCompleto,
          turno = medico.Turno,
          fechasSemana = fechasSemana,
          citasOcupadas = citasOcupadas.Select(c => new { c.fecha, c.hora }).ToList(),
          inicioSemana = inicioSemana.ToString("dd/MM/yyyy"),
          finSemana = finSemana.ToString("dd/MM/yyyy")
        });
      }
      catch (Exception ex)
      {
        _logger.LogError("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogError("║                   ERROR CRÍTICO ❌                              ║");
        _logger.LogError("╚════════════════════════════════════════════════════════════════╝");
        _logger.LogError($"💥 Mensaje: {ex.Message}");
        _logger.LogError($"📍 StackTrace: {ex.StackTrace}");

        return Json(new { error = true, mensaje = "Error al cargar calendario: " + ex.Message });
      }
    }

    [HttpGet]
    public IActionResult ObtenerDatosModalCita(int idMedico, int idPaciente)
    {
      try
      {
        _logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║           INICIO ObtenerDatosModalCita                         ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");
        _logger.LogInformation($"📋 Parámetros:");
        _logger.LogInformation($"   → IdMedico: {idMedico}");
        _logger.LogInformation($"   → IdPaciente: {idPaciente}");

        _logger.LogInformation($"🔍 Buscando médico...");
        var medico = _context.Medicos
            .Include(m => m.ConsultorioAsignado)
            .FirstOrDefault(m => m.IdMedico == idMedico);

        _logger.LogInformation($"🔍 Buscando paciente...");
        var paciente = _context.Pacientes
            .FirstOrDefault(p => p.IdPaciente == idPaciente);

        if (medico == null)
        {
          _logger.LogWarning($"❌ Médico con ID {idMedico} NO ENCONTRADO");
          return Json(new { error = true, mensaje = "Médico no encontrado" });
        }

        if (paciente == null)
        {
          _logger.LogWarning($"❌ Paciente con ID {idPaciente} NO ENCONTRADO");
          return Json(new { error = true, mensaje = "Paciente no encontrado" });
        }

        var nombreMedico = $"Dr. {medico.Nombre} {medico.ApellidoPaterno} {medico.ApellidoMaterno}";
        var nombrePaciente = $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}";
        var consultorio = medico.ConsultorioAsignado?.Nombre ?? "Consultorio A";

        _logger.LogInformation($"✅ Datos encontrados:");
        _logger.LogInformation($"👨‍⚕️ Médico:");
        _logger.LogInformation($"   → Nombre: {nombreMedico}");
        _logger.LogInformation($"   → Consultorio: {consultorio}");
        _logger.LogInformation($"👤 Paciente:");
        _logger.LogInformation($"   → Nombre: {nombrePaciente}");
        _logger.LogInformation($"   → DNI: {paciente.NumeroDocumento}");
        _logger.LogInformation($"   → Edad: {paciente.Edad} años");

        _logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║           FIN ObtenerDatosModalCita - ÉXITO ✅                 ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");

        return Json(new
        {
          medicoNombre = nombreMedico,
          consultorio = consultorio,
          paciente = new
          {
            dni = paciente.NumeroDocumento,
            historiaClinica = $"HC-{DateTime.Now.Year}-{paciente.IdPaciente.ToString().PadLeft(6, '0')}",
            nombreCompleto = nombrePaciente,
            edad = paciente.Edad,
            telefono = "902315786",
            correo = "No hay correo registrado"
          }
        });
      }
      catch (Exception ex)
      {
        _logger.LogError("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogError("║                   ERROR CRÍTICO ❌                              ║");
        _logger.LogError("╚════════════════════════════════════════════════════════════════╝");
        _logger.LogError($"💥 Mensaje: {ex.Message}");
        _logger.LogError($"📍 StackTrace: {ex.StackTrace}");

        return Json(new { error = true, mensaje = "Error: " + ex.Message });
      }
    }

    [HttpPost]
    public IActionResult RegistrarCita([FromBody] CitaRegistroDto datos)
    {
      try
      {
        _logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║                INICIO RegistrarCita                            ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");
        _logger.LogInformation($"📋 Datos recibidos:");
        _logger.LogInformation($"   → IdMedico: {datos.IdMedico}");
        _logger.LogInformation($"   → IdPaciente: {datos.IdPaciente}");
        _logger.LogInformation($"   → FechaCita: {datos.FechaCita}");
        _logger.LogInformation($"   → HoraCita: {datos.HoraCita}");
        _logger.LogInformation($"   → Semana: {datos.Semana}");

        _logger.LogInformation($"🔍 Buscando médico...");
        var medico = _context.Medicos
            .Include(m => m.ConsultorioAsignado)
            .FirstOrDefault(m => m.IdMedico == datos.IdMedico);

        if (medico == null)
        {
          _logger.LogWarning($"❌ Médico con ID {datos.IdMedico} NO ENCONTRADO");
          return Json(new { success = false, mensaje = "Médico no encontrado" });
        }

        _logger.LogInformation($"✅ Médico encontrado: {medico.Nombre} {medico.ApellidoPaterno}");

        // Parsear hora
        _logger.LogInformation($"🕐 Parseando hora {datos.HoraCita}...");
        var horaPartes = datos.HoraCita.Split(':');
        var horaCita = new TimeSpan(int.Parse(horaPartes[0]), int.Parse(horaPartes[1]), 0);
        _logger.LogInformation($"✅ Hora parseada correctamente: {horaCita}");

        // Crear cita
        _logger.LogInformation($"📝 Creando registro de cita...");
        var nuevaCita = new Cita
        {
          IdPaciente = datos.IdPaciente,
          IdMedico = datos.IdMedico,
          Especialidad = "Medicina General",
          FechaCita = DateTime.Parse(datos.FechaCita).Date,
          HoraCita = horaCita,
          Consultorio = medico.ConsultorioAsignado?.Nombre ?? "Consultorio A",
          EstadoCita = "Pendiente",
          FechaRegistro = DateTime.Now.Date


        };
        _logger.LogInformation($"✅ Registro de cita creado en memoria");
        _logger.LogInformation($"   → Fecha: {nuevaCita.FechaCita:yyyy-MM-dd}");
        _logger.LogInformation($"   → Hora: {nuevaCita.HoraCita}");
        _logger.LogInformation($"   → Consultorio: {nuevaCita.Consultorio}");
        _logger.LogInformation($"   → Estado: {nuevaCita.EstadoCita}");
        _logger.LogInformation($"   → FechaRegistro: {nuevaCita.FechaRegistro}");
        _logger.LogInformation($"   → Especialidad: {nuevaCita.Especialidad}");
        _logger.LogInformation($"   → IdPaciente: {nuevaCita.IdPaciente}");
        _logger.LogInformation($"   → IdMedico: {nuevaCita.IdMedico}");
        _logger.LogInformation($"   → Médico: Dr. {medico.Nombre} {medico.ApellidoPaterno}");
        _logger.LogInformation($"   → Paciente ID: {nuevaCita.IdPaciente}");
        _logger.LogInformation($"   → Paciente: (ID {nuevaCita.IdPaciente})");

        _context.Citas.Add(nuevaCita);
        _logger.LogInformation($"✅ Cita agregada al contexto de EF Core");

        // Actualizar disponibilidad semanal
        _logger.LogInformation($"📅 Calculando semana para actualizar disponibilidad...");
        var hoy = DateTime.Today;
        var diasDesdeInicio = (int)hoy.DayOfWeek - (int)DayOfWeek.Monday;
        if (diasDesdeInicio < 0) diasDesdeInicio += 7;

        var inicioSemanaBase = hoy.AddDays(-diasDesdeInicio);
        var inicioSemana = inicioSemanaBase.AddDays(datos.Semana * 7).Date;

        _logger.LogInformation($"🔍 Buscando disponibilidad semanal:");
        _logger.LogInformation($"   → Fecha inicio semana: {inicioSemana:yyyy-MM-dd}");

        var disponibilidad = _context.DisponibilidadesSemanales
            .FirstOrDefault(d => d.IdMedico == datos.IdMedico &&
                               d.FechaInicioSemana.Date == inicioSemana.Date);

        if (disponibilidad != null)
        {
          _logger.LogInformation($"✅ Disponibilidad encontrada:");
          _logger.LogInformation($"   → ID: {disponibilidad.IdDisponibilidad}");
          _logger.LogInformation($"   → Citas actuales ANTES: {disponibilidad.CitasActuales}");

          disponibilidad.CitasActuales++;

          _logger.LogInformation($"   → Citas actuales DESPUÉS: {disponibilidad.CitasActuales}");
          _logger.LogInformation($"   → Citas disponibles: {disponibilidad.CitasMaximas - disponibilidad.CitasActuales}");
        }
        else
        {
          _logger.LogWarning($"⚠️ No se encontró disponibilidad semanal para actualizar");
          _logger.LogWarning($"   → La cita se registrará pero no se actualizará el contador");
        }

        _logger.LogInformation($"💾 Guardando cambios en la base de datos...");
        var registrosAfectados = _context.SaveChanges();
        _logger.LogInformation($"✅ Guardado exitoso. Registros afectados: {registrosAfectados}");

        _logger.LogInformation("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogInformation("║              FIN RegistrarCita - ÉXITO ✅                       ║");
        _logger.LogInformation("╚════════════════════════════════════════════════════════════════╝");

        return Json(new { success = true, mensaje = "Cita registrada exitosamente" });
      }
      catch (Exception ex)
      {
        _logger.LogError("╔════════════════════════════════════════════════════════════════╗");
        _logger.LogError("║                   ERROR CRÍTICO ❌                              ║");
        _logger.LogError("╚════════════════════════════════════════════════════════════════╝");
        _logger.LogError($"💥 Mensaje: {ex.Message}");
        _logger.LogError($"📍 StackTrace: {ex.StackTrace}");

        if (ex.InnerException != null)
        {
          _logger.LogError($"🔍 Inner Exception: {ex.InnerException.Message}");
        }

        return Json(new { success = false, mensaje = "Error al registrar cita: " + ex.Message });
      }
    }

    // Cancelar una cita (solo si NO tiene triaje) y actualizar la disponibilidad semanal
    [HttpDelete]
    [Route("/citas/cancelar/{id}")]
    public async Task<IActionResult> CancelarCita(int id)
    {
      try
      {
        if (id <= 0)
          return Json(new { success = false, message = "ID de cita inválido", mensaje = "ID de cita inválido" });

        var cita = await _context.Citas
            .Include(c => c.Triage)
            .FirstOrDefaultAsync(c => c.IdCita == id);

        if (cita == null)
          return Json(new { success = false, message = "Cita no encontrada", mensaje = "Cita no encontrada" });

        // No permitir cancelar si ya existe un triaje asociado
        if (cita.IdTriage.HasValue && cita.IdTriage.Value > 0)
        {
          return Json(new { success = false, message = "No se puede cancelar la cita porque tiene triaje asociado.", mensaje = "No se puede cancelar la cita porque tiene triaje asociado." });
        }

        // Actualizar disponibilidad semanal: buscar la semana correspondiente a la fecha de la cita
        var fechaCita = cita.FechaCita.Date;
        var diasDesdeInicio = (int)fechaCita.DayOfWeek - (int)DayOfWeek.Monday;
        if (diasDesdeInicio < 0) diasDesdeInicio += 7;
        var inicioSemana = fechaCita.AddDays(-diasDesdeInicio).Date;

        var disponibilidad = _context.DisponibilidadesSemanales
            .FirstOrDefault(d => d.IdMedico == cita.IdMedico && d.FechaInicioSemana.Date == inicioSemana.Date);

        if (disponibilidad != null)
        {
          if (disponibilidad.CitasActuales > 0)
          {
            disponibilidad.CitasActuales--;
            _logger.LogInformation($"Disponibilidad actualizada tras cancelar cita: {disponibilidad.CitasActuales}/{disponibilidad.CitasMaximas}");
          }
          else
          {
            _logger.LogWarning($"Disponibilidad encontrada pero CitasActuales ya es 0 (IdDisponibilidad: {disponibilidad.IdDisponibilidad})");
          }
        }

        _context.Citas.Remove(cita);
        await _context.SaveChangesAsync();

        return Json(new { success = true, message = "Cita cancelada exitosamente", mensaje = "Cita cancelada exitosamente" });
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al cancelar cita");
        return Json(new { success = false, message = "Error al cancelar la cita: " + ex.Message, mensaje = "Error al cancelar la cita: " + ex.Message });
      }
    }
  }
}
