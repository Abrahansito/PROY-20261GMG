using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using SGMG.Models;
using SGMG.Dtos.Request.HistoriaClinica;
using SGMG.Dtos.Request.Paciente;
using SGMG.Dtos.Request.DomicilioPaciente;
using SGMG.Repository;
using SGMG.Services;
using SGMG.Dtos.Response;


namespace SGMG.Controllers
{
  public class HistoriaClinicaController : Controller
  {
    private readonly IPacienteRepository _pacienteRepository;
    private readonly IHistoriaClinicaRepository _historiaClinicaRepository;
    private readonly IDomicilioPacienteRepository _domicilioPacienteRepository;
    private readonly IHistorialClinicoService _historialService;

    public HistoriaClinicaController(
        IPacienteRepository pacienteRepository,
        IHistoriaClinicaRepository historiaClinicaRepository,
        IDomicilioPacienteRepository domicilioPacienteRepository,
        IHistorialClinicoService historialService)
    {
      _pacienteRepository = pacienteRepository;
      _historiaClinicaRepository = historiaClinicaRepository;
      _domicilioPacienteRepository = domicilioPacienteRepository;
      _historialService = historialService;
    }

    //NUEVOS ENDPOINTS

    [HttpGet]
    [Route("/historia-clinica/ver")]
    public IActionResult Ver()
    {
      return View();
    }

    [HttpGet]
    [Route("/api/historia-clinica/paciente/{idPaciente}")]
    public async Task<GenericResponse<HistorialClinicoDTO>> GetHistorialPaciente(int idPaciente)
    {
      return await _historialService.GetHistorialByPacienteAsync(idPaciente);
    }

    [HttpGet]
    [Route("/api/historia-clinica/diagnostico/{idDiagnostico}")]
    public async Task<GenericResponse<DiagnosticoResponseDTO>> GetDiagnosticoDetalle(int idDiagnostico)
    {
      return await _historialService.GetDiagnosticoDetalleAsync(idDiagnostico);
    }



    [HttpGet]
    public IActionResult Create()
    {
      var model = new HistoriaClinicaCompletaRequestDTO();
      return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(HistoriaClinicaCompletaRequestDTO model)
    {
      if (!ModelState.IsValid)
      {
        return View(model);
      }

      try
      {
        var paciente = new Paciente
        {
          NumeroDocumento = model.Paciente.NumeroDocumento ?? string.Empty,
          TipoDocumento = model.Paciente.TipoDocumento ?? string.Empty,
          Nombre = model.Paciente.Nombre ?? string.Empty,
          ApellidoPaterno = model.Paciente.ApellidoPaterno ?? string.Empty,
          ApellidoMaterno = model.Paciente.ApellidoMaterno ?? string.Empty,
          Sexo = model.Paciente.Sexo ?? string.Empty,
          FechaRegistro = DateTime.UtcNow
        };

        await _pacienteRepository.AddPacienteAsync(paciente);

        var historiaClinica = new HistoriaClinica
        {
          IdPaciente = paciente.IdPaciente,
          CodigoHistoria = await GenerarCodigoHistoria(),
          TipoSeguro = model.HistoriaClinica.TipoSeguro ?? string.Empty,
          FechaNacimiento = model.HistoriaClinica.FechaNacimiento,
          EstadoCivil = model.HistoriaClinica.EstadoCivil ?? string.Empty,
          TipoSangre = model.HistoriaClinica.TipoSangre ?? string.Empty
        };

        await _historiaClinicaRepository.AddHistoriaClinicaAsync(historiaClinica);

        if (model.Domicilio != null &&
            (!string.IsNullOrEmpty(model.Domicilio.Departamento) ||
             !string.IsNullOrEmpty(model.Domicilio.Direccion)))
        {
          var domicilio = new DomicilioPaciente
          {
            IdPaciente = paciente.IdPaciente,
            Departamento = model.Domicilio.Departamento ?? string.Empty,
            Provincia = model.Domicilio.Provincia ?? string.Empty,
            Distrito = model.Domicilio.Distrito ?? string.Empty,
            Direccion = model.Domicilio.Direccion ?? string.Empty,
            Referencia = model.Domicilio.Referencia ?? string.Empty
          };

          await _domicilioPacienteRepository.AddDomicilioAsync(domicilio);
        }

        TempData["Success"] = "Historia clínica registrada exitosamente.";
        return RedirectToAction("paciente", "Home");
      }
      catch (Exception ex)
      {
        ModelState.AddModelError("", $"Error al registrar la historia clínica: {ex.Message}");
        return View(model);
      }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
      try
      {
        var paciente = await _pacienteRepository.GetPacienteByIdAsync(id);
        if (paciente == null)
        {
          TempData["Error"] = "Paciente no encontrado.";
          return RedirectToAction("Index", "Paciente");
        }

        var historiaClinica = await _historiaClinicaRepository.GetHistoriaClinicaByPacienteIdAsync(id);
        var domicilio = await _domicilioPacienteRepository.GetDomicilioByPacienteIdAsync(id);

        var model = new HistoriaClinicaCompletaRequestDTO
        {
          Paciente = new PacienteRequestDTO
          {
            IdPaciente = paciente.IdPaciente,
            TipoDocumento = paciente.TipoDocumento,
            NumeroDocumento = paciente.NumeroDocumento,
            Sexo = paciente.Sexo,
            ApellidoPaterno = paciente.ApellidoPaterno,
            Nombre = paciente.Nombre,
            ApellidoMaterno = paciente.ApellidoMaterno
          },
          HistoriaClinica = new HistoriaClinicaRequestDTO
          {
            IdHistoria = historiaClinica?.IdHistoria ?? 0,
            IdPaciente = id,
            CodigoHistoria = historiaClinica?.CodigoHistoria ?? string.Empty,
            EstadoCivil = historiaClinica?.EstadoCivil ?? string.Empty,
            FechaNacimiento = historiaClinica?.FechaNacimiento ?? DateTime.Now,
            TipoSangre = historiaClinica?.TipoSangre ?? string.Empty,
            TipoSeguro = historiaClinica?.TipoSeguro ?? string.Empty
          },
          Domicilio = domicilio != null ? new DomicilioPacienteRequestDTO
          {
            IdDomicilio = domicilio.IdDomicilio,
            IdPaciente = id,
            Departamento = domicilio.Departamento,
            Provincia = domicilio.Provincia,
            Distrito = domicilio.Distrito,
            Direccion = domicilio.Direccion,
            Referencia = domicilio.Referencia
          } : new DomicilioPacienteRequestDTO()
        };

        return View("Create", model);
      }
      catch (Exception ex)
      {
        TempData["Error"] = $"Error al cargar la historia clínica: {ex.Message}";
        return RedirectToAction("Index", "Paciente");
      }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(HistoriaClinicaCompletaRequestDTO model)
    {
      if (!ModelState.IsValid)
      {
        return View("Create", model);
      }

      try
      {
        var paciente = await _pacienteRepository.GetPacienteByIdAsync(model.Paciente.IdPaciente);
        if (paciente == null)
        {
          TempData["Error"] = "Paciente no encontrado.";
          return RedirectToAction("Index", "Paciente");
        }

        paciente.NumeroDocumento = model.Paciente.NumeroDocumento ?? string.Empty;
        paciente.TipoDocumento = model.Paciente.TipoDocumento ?? string.Empty;
        paciente.Nombre = model.Paciente.Nombre ?? string.Empty;
        paciente.ApellidoPaterno = model.Paciente.ApellidoPaterno ?? string.Empty;
        paciente.ApellidoMaterno = model.Paciente.ApellidoMaterno ?? string.Empty;
        paciente.Sexo = model.Paciente.Sexo ?? string.Empty;

        await _pacienteRepository.UpdatePacienteAsync(paciente);

        if (model.HistoriaClinica.IdHistoria > 0)
        {
          var historiaClinica = await _historiaClinicaRepository.GetHistoriaClinicaByIdAsync(model.HistoriaClinica.IdHistoria);
          if (historiaClinica != null)
          {
            historiaClinica.TipoSeguro = model.HistoriaClinica.TipoSeguro ?? string.Empty;
            historiaClinica.FechaNacimiento = model.HistoriaClinica.FechaNacimiento;
            historiaClinica.EstadoCivil = model.HistoriaClinica.EstadoCivil ?? string.Empty;
            historiaClinica.TipoSangre = model.HistoriaClinica.TipoSangre ?? string.Empty;

            await _historiaClinicaRepository.UpdateHistoriaClinicaAsync(historiaClinica);
          }
        }
        else
        {
          var nuevaHistoria = new HistoriaClinica
          {
            IdPaciente = paciente.IdPaciente,
            CodigoHistoria = await GenerarCodigoHistoria(),
            TipoSeguro = model.HistoriaClinica.TipoSeguro ?? string.Empty,
            FechaNacimiento = model.HistoriaClinica.FechaNacimiento.Date,
            EstadoCivil = model.HistoriaClinica.EstadoCivil ?? string.Empty,
            TipoSangre = model.HistoriaClinica.TipoSangre ?? string.Empty
          };
          await _historiaClinicaRepository.AddHistoriaClinicaAsync(nuevaHistoria);
        }

        if (model.Domicilio != null)
        {
          if (model.Domicilio.IdDomicilio > 0)
          {
            var domicilio = await _domicilioPacienteRepository.GetDomicilioByIdAsync(model.Domicilio.IdDomicilio);
            if (domicilio != null)
            {
              domicilio.Departamento = model.Domicilio.Departamento ?? string.Empty;
              domicilio.Provincia = model.Domicilio.Provincia ?? string.Empty;
              domicilio.Distrito = model.Domicilio.Distrito ?? string.Empty;
              domicilio.Direccion = model.Domicilio.Direccion ?? string.Empty;
              domicilio.Referencia = model.Domicilio.Referencia ?? string.Empty;

              await _domicilioPacienteRepository.UpdateDomicilioAsync(domicilio);
            }
          }
          else if (!string.IsNullOrEmpty(model.Domicilio.Departamento) ||
                   !string.IsNullOrEmpty(model.Domicilio.Direccion))
          {
            var nuevoDomicilio = new DomicilioPaciente
            {
              IdPaciente = paciente.IdPaciente,
              Departamento = model.Domicilio.Departamento ?? string.Empty,
              Provincia = model.Domicilio.Provincia ?? string.Empty,
              Distrito = model.Domicilio.Distrito ?? string.Empty,
              Direccion = model.Domicilio.Direccion ?? string.Empty,
              Referencia = model.Domicilio.Referencia ?? string.Empty
            };

            await _domicilioPacienteRepository.AddDomicilioAsync(nuevoDomicilio);
          }
        }

        TempData["Success"] = "Historia clínica actualizada exitosamente.";
        return RedirectToAction("Index", "Paciente");
      }
      catch (Exception ex)
      {
        ModelState.AddModelError("", $"Error al actualizar la historia clínica: {ex.Message}");
        return View("Create", model);
      }
    }

    private async Task<string> GenerarCodigoHistoria()
    {
      var historias = await _historiaClinicaRepository.GetAllHistoriasClinicasAsync();
      var ultimoNumero = historias
          .Where(h => h.CodigoHistoria.StartsWith("HC-2025-"))
          .Select(h => int.TryParse(h.CodigoHistoria.Substring(8), out int num) ? num : 0)
          .DefaultIfEmpty(0)
          .Max();

      return $"HC-2025-{(ultimoNumero + 1):D4}";
    }
  }
}