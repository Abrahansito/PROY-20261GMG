using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Repository;
using Microsoft.EntityFrameworkCore;
using SGMG.Data;


namespace SGMG.Services.ServiceImpl
{
  public class CitaService : ICitaService
  {
    private readonly ICitaRepository _citaRepository;
    private readonly ILogger<CitaService> _logger;
    private readonly ApplicationDbContext _context;


    public CitaService(ICitaRepository citaRepository, ILogger<CitaService> logger, ApplicationDbContext context)
    {
      _citaRepository = citaRepository;
      _logger = logger;
      _context = context;
    }

    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasPendientesAsync()
    {
      try
      {
        var citas = await _citaRepository.GetCitasPendientesAsync();

        if (citas == null || !citas.Any())
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, new List<CitaResponseDTO>(), "No hay citas pendientes.");
        }

        var citasDTO = citas.Select(MapToDTO).ToList();
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, citasDTO, "Citas pendientes obtenidas correctamente.");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, $"Error al obtener citas pendientes: {ex.Message}");
      }
    }

    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasFueraHorarioAsync()
    {
      try
      {
        var citas = await _citaRepository.GetCitasFueraHorarioAsync();

        if (citas == null || !citas.Any())
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, new List<CitaResponseDTO>(), "No hay citas fuera de horario.");
        }

        var citasDTO = citas.Select(MapToDTO).ToList();
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, citasDTO, "Citas fuera de horario obtenidas correctamente.");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetAllCitasAsync()
    {
      try
      {
        var citas = await _citaRepository.GetAllCitasAsync();

        if (citas == null || !citas.Any())
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, new List<CitaResponseDTO>(), "No se encontraron citas.");
        }

        var citasDTO = citas.Select(MapToDTO).ToList();
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, citasDTO, "Citas obtenidas correctamente.");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<CitaResponseDTO>> GetCitaByIdAsync(int id)
    {
      try
      {
        if (id <= 0)
          return new GenericResponse<CitaResponseDTO>(false, "El ID de la cita no es válido.xdddddddddddddd");

        var cita = await _citaRepository.GetCitaByIdAsync(id);

        if (cita == null)
          return new GenericResponse<CitaResponseDTO>(false, "Cita no encontrada.");

        var citaDTO = MapToDTO(cita);
        return new GenericResponse<CitaResponseDTO>(true, citaDTO, "Cita obtenida correctamente.");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<CitaResponseDTO>(false, $"Error: {ex.Message}");
      }
    }

    private CitaResponseDTO MapToDTO(Cita cita)
    {
      return new CitaResponseDTO
      {
        IdCita = cita.IdCita,
        Especialidad = cita.Especialidad,
        FechaCita = cita.FechaCita,
        HoraCita = cita.HoraCita,
        Consultorio = cita.Consultorio,
        EstadoCita = cita.EstadoCita,
        IdPaciente = cita.IdPaciente,
        NumeroDocumento = cita.Paciente?.NumeroDocumento ?? "",
        NombreCompletoPaciente = $"{cita.Paciente?.ApellidoPaterno ?? ""} {cita.Paciente?.ApellidoMaterno ?? ""} {cita.Paciente?.Nombre ?? ""}".Trim(),
        IdMedico = cita.IdMedico,
        NombreCompletoMedico = $"{cita.Medico?.Nombre ?? ""} {cita.Medico?.ApellidoPaterno ?? ""}".Trim()
      };
    }


    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> BuscarCitasPendientesAsync(string? tipoDoc, string? numeroDoc)
    {
      try
      {
        // Si no hay filtros, cargar TODAS las citas con estado "Pagado"
        if (string.IsNullOrWhiteSpace(tipoDoc) && string.IsNullOrWhiteSpace(numeroDoc))
        {
          var todasLasCitas = await _context.Citas
              .Include(c => c.Paciente)
              .Include(c => c.Medico)
              .Where(c => c.EstadoCita == "Pagado") // Solo citas pagadas
              .OrderBy(c => c.FechaCita)
              .ThenBy(c => c.HoraCita)
              .ToListAsync();

          var citasDTOAll = todasLasCitas.Select(c => new CitaResponseDTO
          {
            IdPaciente = c.IdPaciente,
            NumeroDocumento = c.Paciente?.NumeroDocumento ?? "",
            NombreCompletoPaciente = c.Paciente != null
                  ? $"{c.Paciente.ApellidoPaterno} {c.Paciente.ApellidoMaterno} {c.Paciente.Nombre}".Trim()
                  : "",
            Consultorio = c.Consultorio,
            HoraCita = c.HoraCita,
            FechaCita = c.FechaCita,
            NombreCompletoMedico = c.Medico != null
                  ? $"{c.Medico.ApellidoPaterno} {c.Medico.ApellidoMaterno} {c.Medico.Nombre}".Trim()
                  : ""
          }).ToList();

          return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, citasDTOAll, "Citas pagadas cargadas");
        }

        // Validar que si se especifica un filtro, estén ambos campos completos
        if (string.IsNullOrWhiteSpace(tipoDoc) || string.IsNullOrWhiteSpace(numeroDoc))
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, "Debe especificar tipo y número de documento");
        }

        // Búsqueda con filtros (código existente)
        var citas = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Where(c => c.EstadoCita == "Pagado" &&
                       c.Paciente.NumeroDocumento == numeroDoc)
            .ToListAsync();

        // Ordenar en memoria
        citas = citas
            .OrderBy(c => c.FechaCita)
            .ThenBy(c => c.HoraCita)
            .ToList();

        var citasDTO = citas.Select(c => new CitaResponseDTO
        {
          IdPaciente = c.IdPaciente,
          NumeroDocumento = c.Paciente?.NumeroDocumento ?? "",
          NombreCompletoPaciente = c.Paciente != null
                ? $"{c.Paciente.ApellidoPaterno} {c.Paciente.ApellidoMaterno} {c.Paciente.Nombre}".Trim()
                : "",
          Consultorio = c.Consultorio,
          HoraCita = c.HoraCita,
          FechaCita = c.FechaCita,
          NombreCompletoMedico = c.Medico != null
                ? $"{c.Medico.ApellidoPaterno} {c.Medico.ApellidoMaterno} {c.Medico.Nombre}".Trim()
                : ""
        }).ToList();

        return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, citasDTO, "Búsqueda completada");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> BuscarCitasFueraHorarioAsync(string? tipoDoc, string? numeroDoc)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(tipoDoc) || string.IsNullOrWhiteSpace(numeroDoc))
        {
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, "Debe especificar tipo y número de documento");
        }

        var ahora = DateTime.UtcNow.Date;

        var citas = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Where(c => c.EstadoCita == "Pagado" &&
                       c.FechaCita < ahora &&
                       c.Paciente.NumeroDocumento == numeroDoc)
            .ToListAsync();

        // Ordenar en memoria
        citas = citas
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .ToList();

        var citasDTO = citas.Select(c => new CitaResponseDTO
        {
          IdPaciente = c.IdPaciente,
          NumeroDocumento = c.Paciente?.NumeroDocumento ?? "",
          NombreCompletoPaciente = c.Paciente != null
                ? $"{c.Paciente.ApellidoPaterno} {c.Paciente.ApellidoMaterno} {c.Paciente.Nombre}".Trim()
                : "",
          Consultorio = c.Consultorio,
          HoraCita = c.HoraCita,
          FechaCita = c.FechaCita,
          NombreCompletoMedico = c.Medico != null
                ? $"{c.Medico.ApellidoPaterno} {c.Medico.ApellidoMaterno} {c.Medico.Nombre}".Trim()
                : ""
        }).ToList();

        return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, citasDTO, "Búsqueda completada");
      }
      catch (Exception ex)
      {
        Console.WriteLine($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }

    // NUEVOS MÉTODOS para médicos
    public async Task<GenericResponse<IEnumerable<CitaPorAtenderDTO>>> GetCitasPorAtenderAsync(int idMedico, DateTime fecha, string consultorio)
    {
      try
      {
        // Obtener las citas sin ordenar por TimeSpan
        var citas = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Where(c => c.IdMedico == idMedico &&
                       c.FechaCita.Date == fecha.Date &&
                       c.Consultorio == consultorio &&
                       (c.EstadoCita == "Programada" || c.EstadoCita == "Pagado" || c.EstadoCita == "Reservada"))
            .ToListAsync(); // Primero obtener los datos

        // CORREGIDO: Ordenar en memoria por HoraCita
        var citasOrdenadas = citas
            .OrderBy(c => c.HoraCita)
            .ToList();

        var citasDTO = citasOrdenadas.Select(c => new CitaPorAtenderDTO
        {
          IdCita = c.IdCita,
          IdPaciente = c.IdPaciente,
          NumeroDocumento = c.Paciente?.NumeroDocumento ?? "",
          TipoDocumento = c.Paciente?.TipoDocumento ?? "",
          NombreCompleto = c.Paciente != null ?
                $"{c.Paciente.Nombre} {c.Paciente.ApellidoPaterno} {c.Paciente.ApellidoMaterno}".Trim() : "",
          Sexo = c.Paciente?.Sexo ?? "",
          Edad = c.Paciente?.Edad ?? 0,
          FechaCita = c.FechaCita,
          HoraCita = c.HoraCita,
          Consultorio = c.Consultorio,
          EstadoCita = c.EstadoCita,
          FechaRegistro = c.FechaRegistro,
          Especialidad = c.Especialidad
        }).ToList();

        return new GenericResponse<IEnumerable<CitaPorAtenderDTO>>
        {
          Success = true,
          Message = $"Se encontraron {citasDTO.Count} citas por atender",
          Data = citasDTO
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener citas por atender para médico {MedicoId}", idMedico);
        return new GenericResponse<IEnumerable<CitaPorAtenderDTO>>
        {
          Success = false,
          Message = $"Error al obtener citas: {ex.Message}"
        };
      }
    }

    public async Task<GenericResponse<IEnumerable<CitaPorAtenderDTO>>> GetCitasPorAtenderFiltradasAsync(int idMedico, DateTime fecha, string consultorio, string filtro)
    {
      try
      {
        var citasResponse = await GetCitasPorAtenderAsync(idMedico, fecha, consultorio);

        // CORREGIDO: Usar GetValueOrDefault() para bool?
        if (!citasResponse.Success.GetValueOrDefault())
          return citasResponse;

        var citas = citasResponse.Data;

        if (!string.IsNullOrEmpty(filtro))
        {
          citas = citas.Where(c =>
              c.NumeroDocumento.Contains(filtro) ||
              c.NombreCompleto.ToLower().Contains(filtro.ToLower()) ||
              c.TipoDocumento.ToLower().Contains(filtro.ToLower())
          ).ToList();
        }

        return new GenericResponse<IEnumerable<CitaPorAtenderDTO>>
        {
          Success = true,
          Message = $"Se encontraron {citas.Count()} citas filtradas",
          Data = citas
        };
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al filtrar citas por atender");
        return new GenericResponse<IEnumerable<CitaPorAtenderDTO>>
        {
          Success = false,
          Message = $"Error al filtrar citas: {ex.Message}"
        };
      }
    }

  }
}