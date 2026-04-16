using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Repository;
using SGMG.common.exception;
using SGMG.Dtos.Request.Paciente;

using PROY_20252SGMG.Dtos.Response;


namespace SGMG.Services.ServiceImpl
{
  public class PacienteService : IPacienteService
  {
    private readonly IPacienteRepository _pacienteRepository;

    public PacienteService(IPacienteRepository pacienteRepository)
    {
      _pacienteRepository = pacienteRepository;
    }

    public async Task<GenericResponse<IEnumerable<Paciente>>> GetAllPacientesAsync()
    {
      try
      {
        var pacientes = await _pacienteRepository.GetAllPacientesAsync();

        if (pacientes == null || !pacientes.Any())
          return new GenericResponse<IEnumerable<Paciente>>(false, "No se encontraron pacientes.");

        return new GenericResponse<IEnumerable<Paciente>>(true, pacientes, "Pacientes obtenidos correctamente.");
      }
      catch (Exception ex)
      {
        throw new GenericException("Error al obtener pacientes.", ex);
      }
    }

    public async Task<GenericResponse<Paciente>> SearchPacienteByDocumentoAsync(string tipoDocumento, string numeroDocumento)
    {
      try
      {
        if (string.IsNullOrWhiteSpace(tipoDocumento) || string.IsNullOrWhiteSpace(numeroDocumento))
          return new GenericResponse<Paciente>(false, "Debe proporcionar tipo y número de documento.");

        var paciente = await _pacienteRepository.GetPacienteByDocumentoAsync(tipoDocumento, numeroDocumento);

        if (paciente == null)
          return new GenericResponse<Paciente>(false, "No se encontró ningún paciente con ese documento.");

        return new GenericResponse<Paciente>(true, paciente, "Paciente encontrado correctamente.");
      }
      catch (Exception ex)
      {
        throw new GenericException("Error al buscar paciente por documento.", ex);
      }
    }

    public async Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasPendientesByPacienteAsync(int idPaciente)
    {
      try
      {
        if (idPaciente <= 0)
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, "El ID del paciente no es válido.");

        var citas = await _pacienteRepository.GetCitasPendientesByPacienteAsync(idPaciente);

        if (citas == null || !citas.Any())
          return new GenericResponse<IEnumerable<CitaResponseDTO>>(false, "No hay citas pendientes para este paciente.");

        // Mapear a DTO para evitar ciclos de referencia
        var citasDTO = citas.Select(c => new CitaResponseDTO
        {
          IdCita = c.IdCita,
          IdPaciente = c.IdPaciente,
          IdMedico = c.IdMedico,
          Especialidad = c.Especialidad,
          FechaCita = c.FechaCita,
          HoraCita = c.HoraCita,
          Consultorio = c.Consultorio,
          EstadoCita = c.EstadoCita,
          TipoDocumento = c.Paciente?.TipoDocumento ?? "",
          NumeroDocumento = c.Paciente?.NumeroDocumento ?? "",
          NombreCompletoPaciente = $"{c.Paciente?.ApellidoPaterno ?? ""} {c.Paciente?.ApellidoMaterno ?? ""}, {c.Paciente?.Nombre ?? ""}",
          NombreCompletoMedico = $"{c.Medico?.Nombre ?? ""} {c.Medico?.ApellidoPaterno ?? ""}"
        }).ToList();

        return new GenericResponse<IEnumerable<CitaResponseDTO>>(true, citasDTO, "Citas pendientes obtenidas correctamente.");
      }
      catch (Exception ex)
      {
        throw new GenericException($"Error al obtener citas pendientes del paciente con ID: {idPaciente}", ex);
      }
    }
    public async Task<GenericResponse<Paciente>> GetPacienteByIdAsync(int id)
    {
      try
      {
        if (id <= 0)
          return new GenericResponse<Paciente>(false, "El ID del paciente no es válido.");

        var paciente = await _pacienteRepository.GetPacienteByIdAsync(id);
        if (paciente == null)
          return new GenericResponse<Paciente>(false, "Paciente no encontrado.");

        return new GenericResponse<Paciente>(true, paciente, "Paciente obtenido correctamente.");
      }
      catch (Exception ex)
      {
        throw new GenericException($"Error al obtener el paciente con ID: {id}", ex);
      }
    }

    public async Task<GenericResponse<Paciente>> AddPacienteAsync(PacienteRequestDTO pacienteRequestDTO)
    {
      var paciente = MapToPaciente(pacienteRequestDTO);
      try
      {
        if (pacienteRequestDTO == null)
          return new GenericResponse<Paciente>(false, "El paciente no puede ser nulo.");
        await _pacienteRepository.AddPacienteAsync(paciente);
        return new GenericResponse<Paciente>(true, paciente, "Paciente registrado exitosamente.");
      }
      catch (Exception ex)
      {
        return new GenericResponse<Paciente>(false, $"Error al registrar al paciente {paciente?.Nombre}: {ex.Message}");
      }
    }

    public async Task<GenericResponse<Paciente>> UpdatePacienteAsync(PacienteRequestDTO pacienteRequestDTO)
    {
      try
      {
        if (pacienteRequestDTO == null || pacienteRequestDTO.IdPaciente <= 0)
          return new GenericResponse<Paciente>(false, "El paciente no es válido.");
        var paciente = MapToPaciente(pacienteRequestDTO);
        var pacienteExistente = await _pacienteRepository.GetPacienteByIdAsync(paciente.IdPaciente);
        if (pacienteExistente == null)
          return new GenericResponse<Paciente>(false, "Paciente no encontrado.");

        await _pacienteRepository.UpdatePacienteAsync(paciente);
        return new GenericResponse<Paciente>(true, paciente, "Paciente actualizado correctamente.");
      }
      catch (Exception ex)
      {
        return new GenericResponse<Paciente>(false, $"Error al actualizar al paciente {pacienteRequestDTO?.Nombre}: {ex.Message}");
      }
    }

    public async Task<GenericResponse<Paciente>> DeletePacienteAsync(int id)
    {
      try
      {
        if (id <= 0)
          return new GenericResponse<Paciente>(false, "El ID no es válido.");

        var paciente = await _pacienteRepository.GetPacienteByIdAsync(id);
        if (paciente == null)
          return new GenericResponse<Paciente>(false, "Paciente no encontrado.");

        await _pacienteRepository.DeletePacienteAsync(id);
        return new GenericResponse<Paciente>(true, paciente, "Paciente eliminado correctamente.");
      }
      catch (Exception ex)
      {
        return new GenericResponse<Paciente>(false, $"Error al eliminar al paciente con ID: {id}: {ex.Message}");
      }
    }
    public async Task<GenericResponse<IEnumerable<DerivacionResponseDTO>>> GetDerivacionesByPacienteAsync(int idPaciente)
    {
      try
      {
        if (idPaciente <= 0)
          return new GenericResponse<IEnumerable<DerivacionResponseDTO>>(false, "El ID del paciente no es válido.");

        var derivaciones = await _pacienteRepository.GetDerivacionesByPacienteAsync(idPaciente);

        if (derivaciones == null || !derivaciones.Any())
          return new GenericResponse<IEnumerable<DerivacionResponseDTO>>(false, "No hay derivaciones para este paciente.");

        // Mapear a DTO
        var derivacionesDTO = derivaciones.Select(d => new DerivacionResponseDTO
        {
          IdDerivacion = d.IdDerivacion,
          IdCitaOrigen = d.IdCitaOrigen,
          TipoDocumento = d.Cita?.Paciente?.TipoDocumento ?? "",
          NumeroDocumento = d.Cita?.Paciente?.NumeroDocumento ?? "",
          NombreCompletoPaciente = $"{d.Cita?.Paciente?.ApellidoPaterno ?? ""} {d.Cita?.Paciente?.ApellidoMaterno ?? ""}, {d.Cita?.Paciente?.Nombre ?? ""}",
          MedicoSolicitante = $"Dr. {d.Cita?.Medico?.Nombre ?? ""} {d.Cita?.Medico?.ApellidoPaterno ?? ""}",
          EspecialidadDestino = d.EspecialidadDestino,
          MotivoDerivacion = d.MotivoDerivacion,
          FechaDerivacion = d.FechaDerivacion,
          EstadoDerivacion = d.EstadoDerivacion,
          ServicioOrigen = d.Cita?.Especialidad ?? "",
          NumeroDocumentoMedico = d.Cita?.Medico?.NumeroDni ?? "",
          EspecialidadSolicitante = d.Cita?.Medico?.CargoMedico ?? ""
        }).ToList();

        return new GenericResponse<IEnumerable<DerivacionResponseDTO>>(true, derivacionesDTO, "Derivaciones obtenidas correctamente.");
      }
      catch (Exception ex)
      {
        throw new GenericException($"Error al obtener derivaciones del paciente con ID: {idPaciente}", ex);
      }
    }

    private Paciente MapToPaciente(PacienteRequestDTO dto)
    {
      return new Paciente
      {
        NumeroDocumento = dto.NumeroDocumento ?? string.Empty,
        TipoDocumento = dto.TipoDocumento ?? string.Empty,
        Nombre = dto.Nombre ?? string.Empty,
        ApellidoPaterno = dto.ApellidoPaterno ?? string.Empty,
        ApellidoMaterno = dto.ApellidoMaterno ?? string.Empty,
        Sexo = dto.Sexo ?? string.Empty,
        FechaRegistro = DateTime.UtcNow // al crear, se asigna la fecha actual
      };
    }
    private PacienteRequestDTO MapToPacienteRequestDTO(Paciente paciente)
    {
      return new PacienteRequestDTO
      {
        NumeroDocumento = string.IsNullOrEmpty(paciente.NumeroDocumento) ? null : paciente.NumeroDocumento,
        TipoDocumento = string.IsNullOrEmpty(paciente.TipoDocumento) ? null : paciente.TipoDocumento,
        Nombre = string.IsNullOrEmpty(paciente.Nombre) ? null : paciente.Nombre,
        ApellidoPaterno = string.IsNullOrEmpty(paciente.ApellidoPaterno) ? null : paciente.ApellidoPaterno,
        ApellidoMaterno = string.IsNullOrEmpty(paciente.ApellidoMaterno) ? null : paciente.ApellidoMaterno,
        Sexo = string.IsNullOrEmpty(paciente.Sexo) ? null : paciente.Sexo
      };
    }
  }
}