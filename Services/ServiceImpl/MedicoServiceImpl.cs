using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.common.exception;
using SGMG.Dtos.Request.Medico;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Repository;
using SGMG.Services;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore; // ← AGREGAR ESTE
using SGMG.Data; // ← AGREGAR ESTE

namespace SGMG.Services.ServiceImpl
{
  public class MedicoServiceImpl : IMedicoService
  {
    private readonly IMedicoRepository _medicoRepository;
    private readonly IDisponibilidadSemanalRepository _disponibilidadRepository;
    private readonly ILogger<MedicoServiceImpl> _logger;
    private readonly ApplicationDbContext _context; // ← AGREGAR ESTE CAMPO

    public MedicoServiceImpl(
        IMedicoRepository medicoRepository,
        IDisponibilidadSemanalRepository disponibilidadRepository,
        ILogger<MedicoServiceImpl> logger,
        ApplicationDbContext context) // ← AGREGAR ESTE PARÁMETRO
    {
      _medicoRepository = medicoRepository;
      _disponibilidadRepository = disponibilidadRepository;
      _logger = logger;
      _context = context; // ← AGREGAR ESTA LÍNEA
    }


        // ← AGREGAR ESTE NUEVO MÉTODO AL FINAL DE LA CLASE
public async Task<GenericResponse<Medico>> GetMedicoByUserIdAsync(string userId)
{
    try
    {
        if (string.IsNullOrEmpty(userId))
        {
            return new GenericResponse<Medico>
            {
                Success = false,
                Message = "UserId no puede ser nulo o vacío"
            };
        }

        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null || string.IsNullOrEmpty(user.IdUsuario))
        {
            return new GenericResponse<Medico>
            {
                Success = false,
                Message = "Usuario no encontrado"
            };
        }

        if (int.TryParse(user.IdUsuario, out int idMedico))
        {
            var medico = await _medicoRepository.GetMedicoByIdAsync(idMedico);

            if (medico != null)
            {
                return new GenericResponse<Medico>
                {
                    Success = true,
                    Message = "Médico encontrado",
                    Data = medico
                };
            }
        }

        return new GenericResponse<Medico>
        {
            Success = false,
            Message = "Médico no encontrado para este usuario"
        };
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error al obtener médico por userId: {UserId}", userId);
        return new GenericResponse<Medico>
        {
            Success = false,
            Message = $"Error: {ex.Message}"
        };
    }
}

    public async Task<GenericResponse<IEnumerable<Medico>>> GetAllMedicosAsync()
    {
      try
      {
        var medicos = await _medicoRepository.GetAllMedicosAsync();
        if (medicos == null || !medicos.Any())
          return new GenericResponse<IEnumerable<Medico>> { Success = false, Message = "No se encontraron médicos.", Data = null };
        return new GenericResponse<IEnumerable<Medico>> { Success = true, Message = "Médicos obtenidos correctamente.", Data = medicos };
      }
      catch (Exception ex)
      {
        throw new GenericException("Error al obtener los médicos.", ex);
      }
    }

    public async Task<GenericResponse<Medico>> GetMedicoByIdAsync(int id)
    {
      try
      {
        _logger.LogInformation("Obteniendo médico con ID: {Id}", id);
        if (id <= 0)
          return new GenericResponse<Medico> { Success = false, Message = "El ID del médico no es válido.", Data = null };
        var medico = await _medicoRepository.GetMedicoByIdAsync(id);
        if (medico == null)
          return new GenericResponse<Medico> { Success = false, Message = "Médico no encontrado.", Data = null };
        return new GenericResponse<Medico> { Success = true, Message = "Médico obtenido correctamente.", Data = medico };
      }
      catch (Exception ex)
      {
        throw new GenericException("Error al obtener el médico con ID: " + id, ex);
      }
    }

    public async Task<GenericResponse<Medico>> AddMedicoAsync(MedicoRequestDTO medicoRequestDTO)
    {
      try
      {
        if (medicoRequestDTO == null)
          return new GenericResponse<Medico> { Success = false, Message = "El médico no puede ser nulo.", Data = null };

        var medico = MapToMedicoAdd(medicoRequestDTO);
        await _medicoRepository.AddMedicoAsync(medico);
        return new GenericResponse<Medico> { Success = true, Message = "Médico registrado exitosamente.", Data = medico };
      }
      catch (Exception ex)
      {
        return new GenericResponse<Medico> { Success = false, Message = "Error al registrar al medico " + medicoRequestDTO.Nombre + ": " + ex.Message };
      }
    }

    public async Task<GenericResponse<Medico>> UpdateMedicoAsync(MedicoRequestDTO medicoRequestDTO)
    {
      try
      {
        if (medicoRequestDTO == null || medicoRequestDTO.IdMedico <= 0)
          return new GenericResponse<Medico> { Success = false, Message = "El médico no es válido.", Data = null };
        var medicoExistente = await _medicoRepository.GetMedicoByIdAsync(medicoRequestDTO.IdMedico.Value);
        if (medicoExistente == null)
          return new GenericResponse<Medico> { Success = false, Message = "Médico no encontrado.", Data = null };

        MapToMedico(medicoRequestDTO, medicoExistente);
        await _medicoRepository.UpdateMedicoAsync(medicoExistente);
        return new GenericResponse<Medico> { Success = true, Message = "Médico actualizado correctamente.", Data = medicoExistente };
      }
      catch (Exception ex)
      {
        return new GenericResponse<Medico> { Success = false, Message = "Error al actualizar al medico " + medicoRequestDTO.Nombre + ": " + ex.Message };
      }
    }

    public async Task<GenericResponse<Medico>> DeleteMedicoAsync(int id)
    {
      try
      {
        if (id <= 0)
          return new GenericResponse<Medico> { Success = false, Message = "El ID no es válido.", Data = null };
        var medico = await _medicoRepository.GetMedicoByIdAsync(id);
        if (medico == null)
          return new GenericResponse<Medico> { Success = false, Message = "Médico no encontrado.", Data = null };
        await _medicoRepository.DeleteMedicoAsync(id);
        return new GenericResponse<Medico> { Success = true, Message = "Médico eliminado correctamente.", Data = medico };
      }
      catch (Exception ex)
      {
        return new GenericResponse<Medico> { Success = false, Message = "Error al eliminar al medico con ID: " + id + ": " + ex.Message };
      }
    }

    public async Task<GenericResponse<IEnumerable<Medico>>> GetMedicosFilteredAsync(string? numeroDni, string? idConsultorio, string? estado, string? fechaInicio, string? fechaFin, string? turno)
    {
      try
      {
        // Parsear idConsultorio
        int? idConsult = null;
        if (!string.IsNullOrWhiteSpace(idConsultorio) && int.TryParse(idConsultorio, out var idc))
          idConsult = idc;

        // Parsear fechas (aceptamos YYYY-MM-DD o con hora)
        DateTime? from = null;
        DateTime? to = null;
        if (!string.IsNullOrWhiteSpace(fechaInicio) && DateTime.TryParse(fechaInicio, out var f))
          from = f.Date;
        if (!string.IsNullOrWhiteSpace(fechaFin) && DateTime.TryParse(fechaFin, out var t))
          to = t.Date;

        var medicos = await _medicoRepository.GetMedicosFilteredAsync(numeroDni, idConsult, estado, from, to, turno);
        if (medicos == null || !medicos.Any())
          return new GenericResponse<IEnumerable<Medico>> { Success = false, Message = "No se encontraron médicos con los filtros proporcionados.", Data = null };
        return new GenericResponse<IEnumerable<Medico>> { Success = true, Message = "Médicos obtenidos correctamente.", Data = medicos };
      }
      catch (Exception ex)
      {
        throw new GenericException("Error al filtrar los médicos.", ex);
      }
    }

    public async Task<GenericResponse<DisponibilidadSemanaResponse>> GetMedicosDisponiblesPorSemanaAsync(
           int semana,
           string? numeroDni,
           int? idConsultorio,
           string? estado,
           string? turno)
    {
      try
      {
        _logger.LogInformation(
            "Buscando médicos disponibles para semana {Semana} con filtros: DNI={DNI}, Consultorio={Consultorio}, Estado={Estado}, Turno={Turno}",
            semana, numeroDni, idConsultorio, estado, turno);

        // Validar que semana esté entre 0 y 4
        if (semana < 0 || semana > 4)
        {
          _logger.LogWarning("Parámetro 'semana' fuera de rango: {Semana}", semana);
          return new GenericResponse<DisponibilidadSemanaResponse>(
              false,
              "El parámetro 'semana' debe estar entre 0 (actual) y 4 (en 4 semanas)."
          );
        }

        // Calcular fecha objetivo sumando las semanas
        DateTime fechaObjetivo = DateTime.Now.AddDays(semana * 7);
        var (inicioSemana, finSemana) = ObtenerRangoSemana(fechaObjetivo);

        _logger.LogInformation(
            "Rango de semana calculado: {Inicio} - {Fin}",
            inicioSemana.ToString("dd/MM/yyyy"),
            finSemana.ToString("dd/MM/yyyy"));

        // Obtener médicos filtrados por criterios básicos
        var medicos = await _medicoRepository.GetMedicosFilteredAsync(
            numeroDni, idConsultorio, estado, null, null, turno);

        if (medicos == null || !medicos.Any())
        {
          _logger.LogWarning("No se encontraron médicos con los filtros proporcionados");
          return new GenericResponse<DisponibilidadSemanaResponse>(
              false,
              "No se encontraron médicos con los filtros proporcionados."
          );
        }

        _logger.LogInformation("Se encontraron {Count} médicos que cumplen los filtros básicos", medicos.Count());

        // Obtener disponibilidades SOLO de la semana objetivo
        var disponibilidades = await _disponibilidadRepository.GetDisponibilidadesPorSemanaAsync(
            inicioSemana, finSemana);

        var disponibilidadesDict = disponibilidades.ToDictionary(d => d.IdMedico);
        var medicosDisponibles = new List<MedicoDisponibilidadDTO>();

        foreach (var disp in disponibilidades)
        {
          var medico = medicos.FirstOrDefault(m => m.IdMedico == disp.IdMedico);
          if (medico == null)
            continue; // Por si hay una disponibilidad huérfana sin médico válido

          if (disp.CitasActuales < disp.CitasMaximas)
          {
            medicosDisponibles.Add(new MedicoDisponibilidadDTO
            {
              IdMedico = medico.IdMedico,
              NumeroDni = medico.NumeroDni,
              NombreCompleto = $"{medico.Nombre} {medico.ApellidoPaterno} {medico.ApellidoMaterno}".Trim(),
              Consultorio = medico.ConsultorioAsignado?.Nombre ?? "Sin consultorio",
              Turno = medico.Turno,
              CitasActuales = disp.CitasActuales,
              CitasMaximas = disp.CitasMaximas,
              CitasRestantes = disp.CitasMaximas - disp.CitasActuales
            });
          }
        }


        // Preparar respuesta
        var response = new DisponibilidadSemanaResponse
        {
          NumeroSemana = semana,
          FechaInicioSemana = inicioSemana.ToString("yyyy-MM-dd"),
          FechaFinSemana = finSemana.ToString("yyyy-MM-dd"),
          PeriodoSemana = $"{inicioSemana:dd/MM/yyyy} - {finSemana:dd/MM/yyyy}",
          TotalMedicosDisponibles = medicosDisponibles.Count,
          MedicosDisponibles = medicosDisponibles
        };

        if (!medicosDisponibles.Any())
        {
          _logger.LogWarning(
              "No hay médicos disponibles para la semana del {Inicio} al {Fin}",
              inicioSemana.ToString("dd/MM/yyyy"),
              finSemana.ToString("dd/MM/yyyy"));

          return new GenericResponse<DisponibilidadSemanaResponse>(
              false,
              response,
              $"No hay médicos disponibles para la semana del {inicioSemana:dd/MM/yyyy} al {finSemana:dd/MM/yyyy}. Todas las citas están completas."
          );
        }

        string nombreSemana = semana switch
        {
          0 => "semana actual",
          1 => "próxima semana",
          _ => $"semana en {semana} semanas"
        };

        _logger.LogInformation(
            "Se encontraron {Count} médico(s) disponible(s) para la {NombreSemana}",
            medicosDisponibles.Count,
            nombreSemana);

        return new GenericResponse<DisponibilidadSemanaResponse>(
            true,
            response,
            $"Se encontraron {medicosDisponibles.Count} médico(s) disponible(s) para la {nombreSemana} ({inicioSemana:dd/MM/yyyy} - {finSemana:dd/MM/yyyy})."
        );
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener médicos disponibles por semana {Semana}", semana);
        throw new GenericException("Error al obtener médicos disponibles por semana.", ex);
      }
    }

    // Métodos auxiliares privados
    private (DateTime inicioSemana, DateTime finSemana) ObtenerRangoSemana(DateTime fecha)
    {
      int diasHastaLunes = ((int)fecha.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
      DateTime inicioSemana = fecha.Date.AddDays(-diasHastaLunes);
      DateTime finSemana = inicioSemana.AddDays(6);
      return (inicioSemana, finSemana);
    }


    private void MapToMedico(MedicoRequestDTO dto, Medico medico)
    {
      // Solo actualizamos los campos si el DTO trae valores válidos

      if (!string.IsNullOrWhiteSpace(dto.NumeroDni))
        medico.NumeroDni = dto.NumeroDni;

      if (!string.IsNullOrWhiteSpace(dto.Nombre))
        medico.Nombre = dto.Nombre;

      if (!string.IsNullOrWhiteSpace(dto.ApellidoPaterno))
        medico.ApellidoPaterno = dto.ApellidoPaterno;

      if (!string.IsNullOrWhiteSpace(dto.ApellidoMaterno))
        medico.ApellidoMaterno = dto.ApellidoMaterno;

      if (!string.IsNullOrWhiteSpace(dto.Sexo))
        medico.Sexo = dto.Sexo;

      if (dto.FechaNacimiento != default)
        medico.FechaNacimiento = dto.FechaNacimiento;

      if (!string.IsNullOrWhiteSpace(dto.CorreoElectronico))
        medico.CorreoElectronico = dto.CorreoElectronico;

      if (!string.IsNullOrWhiteSpace(dto.EstadoLaboral))
        medico.EstadoLaboral = dto.EstadoLaboral;

      if (dto.FechaIngreso != default)
        medico.FechaIngreso = dto.FechaIngreso;

      if (!string.IsNullOrWhiteSpace(dto.Turno))
        medico.Turno = dto.Turno;

      if (!string.IsNullOrWhiteSpace(dto.AreaServicio))
        medico.AreaServicio = dto.AreaServicio;

      if (!string.IsNullOrWhiteSpace(dto.CargoMedico))
        medico.CargoMedico = dto.CargoMedico;

      if (!string.IsNullOrWhiteSpace(dto.NumeroColegiatura))
        medico.NumeroColegiatura = dto.NumeroColegiatura;

      if (!string.IsNullOrWhiteSpace(dto.TipoMedico))
        medico.TipoMedico = dto.TipoMedico;

      if (!string.IsNullOrWhiteSpace(dto.Direccion))
        medico.Direccion = dto.Direccion;

      if (!string.IsNullOrWhiteSpace(dto.Telefono))
        medico.Telefono = dto.Telefono;

      if (dto.IdConsultorio.HasValue && dto.IdConsultorio.Value > 0)
        medico.IdConsultorio = dto.IdConsultorio.Value;
    }


    private MedicoRequestDTO MapToMedicoRequestDTO(Medico medico)
    {
      return new MedicoRequestDTO
      {
        NumeroDni = medico.NumeroDni,
        Nombre = medico.Nombre,
        ApellidoPaterno = medico.ApellidoPaterno,
        ApellidoMaterno = string.IsNullOrEmpty(medico.ApellidoMaterno) ? null : medico.ApellidoMaterno,
        Sexo = medico.Sexo,
        FechaNacimiento = medico.FechaNacimiento,
        Direccion = string.IsNullOrEmpty(medico.Direccion) ? null : medico.Direccion,
        Telefono = string.IsNullOrEmpty(medico.Telefono) ? null : medico.Telefono,
        CorreoElectronico = medico.CorreoElectronico,
        EstadoLaboral = medico.EstadoLaboral,
        FechaIngreso = medico.FechaIngreso,
        Turno = medico.Turno,
        AreaServicio = medico.AreaServicio,
        CargoMedico = medico.CargoMedico,
        NumeroColegiatura = medico.NumeroColegiatura,
        TipoMedico = medico.TipoMedico,
        IdConsultorio = medico.IdConsultorio
      };
    }



    private Medico MapToMedicoAdd(MedicoRequestDTO dto)
    {
      var medico = new Medico();

      // Evaluación para cadenas obligatorias
      if (!string.IsNullOrWhiteSpace(dto.NumeroDni))
        medico.NumeroDni = dto.NumeroDni;

      if (!string.IsNullOrWhiteSpace(dto.Nombre))
        medico.Nombre = dto.Nombre;

      if (!string.IsNullOrWhiteSpace(dto.ApellidoPaterno))
        medico.ApellidoPaterno = dto.ApellidoPaterno;

      if (!string.IsNullOrWhiteSpace(dto.ApellidoMaterno))
        medico.ApellidoMaterno = dto.ApellidoMaterno;

      if (!string.IsNullOrWhiteSpace(dto.Sexo))
        medico.Sexo = dto.Sexo;

      if (dto.FechaNacimiento != default)
        medico.FechaNacimiento = dto.FechaNacimiento;

      if (!string.IsNullOrWhiteSpace(dto.CorreoElectronico))
        medico.CorreoElectronico = dto.CorreoElectronico;

      if (!string.IsNullOrWhiteSpace(dto.EstadoLaboral))
        medico.EstadoLaboral = dto.EstadoLaboral;

      if (dto.FechaIngreso != default)
        medico.FechaIngreso = dto.FechaIngreso;

      if (!string.IsNullOrWhiteSpace(dto.Turno))
        medico.Turno = dto.Turno;

      if (!string.IsNullOrWhiteSpace(dto.AreaServicio))
        medico.AreaServicio = dto.AreaServicio;

      if (!string.IsNullOrWhiteSpace(dto.CargoMedico))
        medico.CargoMedico = dto.CargoMedico;

      if (!string.IsNullOrWhiteSpace(dto.NumeroColegiatura))
        medico.NumeroColegiatura = dto.NumeroColegiatura;

      if (!string.IsNullOrWhiteSpace(dto.TipoMedico))
        medico.TipoMedico = dto.TipoMedico;

      // Campos opcionales
      if (!string.IsNullOrWhiteSpace(dto.Direccion))
        medico.Direccion = dto.Direccion;

      if (!string.IsNullOrWhiteSpace(dto.Telefono))
        medico.Telefono = dto.Telefono;

      if (dto.IdConsultorio.HasValue && dto.IdConsultorio.Value > 0)
        medico.IdConsultorio = dto.IdConsultorio.Value;

      return medico;
    }

  }
}