using Microsoft.Extensions.Logging;
using SGMG.Dtos.Response;
using SGMG.Models;
using PROY_20252SGMG.Services;
using PROY_20252SGMG.Dtos.Request;
using PROY_20252SGMG.Dtos.Response;
using PROY_20252SGMG.Repository;

namespace SGMG.Services.ServiceImpl
{
  public class RecetaService : IRecetaService
  {
    private readonly IRecetaRepository _recetaRepository;
    private readonly ILogger<RecetaService> _logger;

    public RecetaService(IRecetaRepository recetaRepository, ILogger<RecetaService> logger)
    {
      _recetaRepository = recetaRepository;
      _logger = logger;
    }

    // ===== NUEVOS MÉTODOS PARA ESTADOS =====

    public async Task<GenericResponse<DetalleRecetaResponseDTO>> AgregarMedicamentoPendienteAsync(AgregarMedicamentoRequestDTO request)
    {
      try
      {
        // Buscar o crear receta en estado PENDIENTE
        var receta = await _recetaRepository.GetRecetaPendienteByCitaAsync(
            request.IdCita, request.IdMedico, request.IdPaciente);

        if (receta == null)
        {
          // Crear nueva receta en estado PENDIENTE
          receta = new Receta
          {
            IdCita = request.IdCita,
            IdMedico = request.IdMedico,
            IdPaciente = request.IdPaciente,
            IdHistoriaClinica = request.IdHistoriaClinica,
            FechaEmision = DateTime.Now,
            ObservacionesGenerales = "",
            EstadoReceta = "PENDIENTE", // ✅ Estado inicial
            Detalles = new List<DetalleReceta>()
          };

          receta = await _recetaRepository.CreateRecetaAsync(receta);
        }

        // Agregar nuevo detalle
        var nuevoDetalle = new DetalleReceta
        {
          IdReceta = receta.IdReceta,
          ProductoFarmaceutico = request.Detalle.ProductoFarmaceutico,
          Concentracion = request.Detalle.Concentracion,
          Frecuencia = request.Detalle.Frecuencia,
          Duracion = request.Detalle.Duracion,
          ViaAdministracion = request.Detalle.ViaAdministracion,
          Observaciones = request.Detalle.Observaciones
        };

        var detalleCreado = await _recetaRepository.AgregarDetalleAsync(nuevoDetalle);

        return new GenericResponse<DetalleRecetaResponseDTO>(
            true,
            MapDetalleToDTO(detalleCreado),
            "Medicamento agregado correctamente");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al agregar medicamento: {ex.Message}");
        return new GenericResponse<DetalleRecetaResponseDTO>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<IEnumerable<DetalleRecetaResponseDTO>>> GetMedicamentosPendientesAsync(
        int idCita, int idMedico, int idPaciente, int? idHistoriaClinica)
    {
      try
      {
        var receta = await _recetaRepository.GetRecetaPendienteByCitaAsync(idCita, idMedico, idPaciente);

        if (receta == null || receta.Detalles == null || !receta.Detalles.Any())
        {
          return new GenericResponse<IEnumerable<DetalleRecetaResponseDTO>>(
              true,
              new List<DetalleRecetaResponseDTO>(),
              "No hay medicamentos pendientes");
        }

        var detallesDTO = receta.Detalles.Select(MapDetalleToDTO).ToList();

        return new GenericResponse<IEnumerable<DetalleRecetaResponseDTO>>(
            true,
            detallesDTO,
            "Medicamentos pendientes obtenidos");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener medicamentos pendientes: {ex.Message}");
        return new GenericResponse<IEnumerable<DetalleRecetaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<bool>> EliminarMedicamentoPendienteAsync(int idDetalle)
    {
      try
      {
        var eliminado = await _recetaRepository.EliminarDetalleAsync(idDetalle);

        if (!eliminado)
        {
          return new GenericResponse<bool>(false, "No se pudo eliminar el medicamento");
        }

        return new GenericResponse<bool>(true, true, "Medicamento eliminado correctamente");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al eliminar medicamento: {ex.Message}");
        return new GenericResponse<bool>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<RecetaResponseDTO>> CompletarRecetaAsync(RecetaRequestDTO request)
    {
      try
      {
        // Buscar receta PENDIENTE
        var receta = await _recetaRepository.GetRecetaPendienteByCitaAsync(
            request.IdCita, request.IdMedico, request.IdPaciente);

        if (receta == null || receta.Detalles == null || !receta.Detalles.Any())
        {
          return new GenericResponse<RecetaResponseDTO>(
              false,
              "No hay medicamentos pendientes para completar");
        }

        // Actualizar observaciones generales
        receta.ObservacionesGenerales = request.ObservacionesGenerales;
        receta.EstadoReceta = "COMPLETADO"; // ✅ Cambiar estado

        // Guardar cambios
        var actualizado = await _recetaRepository.CompletarRecetaAsync(receta);

        if (!actualizado)
        {
          return new GenericResponse<RecetaResponseDTO>(false, "Error al completar la receta");
        }

        // Obtener receta actualizada
        var recetaCompleta = await _recetaRepository.GetRecetaByIdAsync(receta.IdReceta);
        var recetaDTO = MapToDTO(recetaCompleta);

        return new GenericResponse<RecetaResponseDTO>(
            true,
            recetaDTO,
            "Receta guardada correctamente");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al completar receta: {ex.Message}");
        return new GenericResponse<RecetaResponseDTO>(false, $"Error: {ex.Message}");
      }
    }

    // ===== MÉTODOS ORIGINALES =====

    public async Task<GenericResponse<RecetaResponseDTO>> CreateRecetaAsync(RecetaRequestDTO request)
    {
      try
      {
        if (request.Detalles == null || !request.Detalles.Any())
        {
          return new GenericResponse<RecetaResponseDTO>(false, "Debe agregar al menos un medicamento");
        }

        var receta = MapToModel(request);
        var recetaCreada = await _recetaRepository.CreateRecetaAsync(receta);
        var recetaCompleta = await _recetaRepository.GetRecetaByIdAsync(recetaCreada.IdReceta);

        if (recetaCompleta == null)
        {
          return new GenericResponse<RecetaResponseDTO>(false, "Error al recuperar la receta creada");
        }

        var recetaDTO = MapToDTO(recetaCompleta);
        return new GenericResponse<RecetaResponseDTO>(true, recetaDTO, "Receta creada correctamente");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al crear receta: {ex.Message}");
        return new GenericResponse<RecetaResponseDTO>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<RecetaResponseDTO>> GetRecetaByIdAsync(int id)
    {
      try
      {
        var receta = await _recetaRepository.GetRecetaByIdAsync(id);
        if (receta == null)
        {
          return new GenericResponse<RecetaResponseDTO>(false, "Receta no encontrada");
        }

        var recetaDTO = MapToDTO(receta);
        return new GenericResponse<RecetaResponseDTO>(true, recetaDTO, "Receta obtenida");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener receta: {ex.Message}");
        return new GenericResponse<RecetaResponseDTO>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<IEnumerable<RecetaResponseDTO>>> GetRecetasByPacienteAsync(int idPaciente)
    {
      try
      {
        var recetas = await _recetaRepository.GetRecetasByPacienteAsync(idPaciente);
        var recetasDTO = recetas.Select(MapToDTO).ToList();
        return new GenericResponse<IEnumerable<RecetaResponseDTO>>(true, recetasDTO, "Recetas obtenidas");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<RecetaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }
    public async Task<GenericResponse<IEnumerable<RecetaResponseDTO>>> GetRecetasByPacienteAsyncCompleto(int idPaciente)
    {
      try
      {
        var recetas = await _recetaRepository.GetRecetasByPacienteAsyncCompleto(idPaciente);
        var recetasDTO = recetas.Select(MapToDTO).ToList();
        return new GenericResponse<IEnumerable<RecetaResponseDTO>>(true, recetasDTO, "Recetas obtenidas");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<RecetaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<IEnumerable<RecetaResponseDTO>>> GetRecetasByMedicoAsync(int idMedico)
    {
      try
      {
        var recetas = await _recetaRepository.GetRecetasByMedicoAsync(idMedico);
        var recetasDTO = recetas.Select(MapToDTO).ToList();
        return new GenericResponse<IEnumerable<RecetaResponseDTO>>(true, recetasDTO, "Recetas obtenidas");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<IEnumerable<RecetaResponseDTO>>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<RecetaResponseDTO>> GetRecetaByCitaAsync(int idCita)
    {
      try
      {
        var receta = await _recetaRepository.GetRecetaByCitaAsync(idCita);
        if (receta == null)
        {
          return new GenericResponse<RecetaResponseDTO>(false, "No existe receta para esta cita");
        }

        var recetaDTO = MapToDTO(receta);
        return new GenericResponse<RecetaResponseDTO>(true, recetaDTO, "Receta obtenida");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<RecetaResponseDTO>(false, $"Error: {ex.Message}");
      }
    }

    public async Task<GenericResponse<bool>> ImprimirRecetaAsync(int id)
    {
      try
      {
        var actualizado = await _recetaRepository.UpdateEstadoRecetaAsync(id, "Impresa");
        if (!actualizado)
        {
          return new GenericResponse<bool>(false, "No se pudo actualizar el estado");
        }

        return new GenericResponse<bool>(true, true, "Receta marcada como impresa");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        return new GenericResponse<bool>(false, $"Error: {ex.Message}");
      }
    }

    // ===== MÉTODOS DE MAPEO =====

    private Receta MapToModel(RecetaRequestDTO dto)
    {
      return new Receta
      {
        IdCita = dto.IdCita,
        IdMedico = dto.IdMedico,
        IdPaciente = dto.IdPaciente,
        IdHistoriaClinica = dto.IdHistoriaClinica,
        FechaEmision = DateTime.Now,
        ObservacionesGenerales = dto.ObservacionesGenerales,
        EstadoReceta = "Emitida",
        Detalles = dto.Detalles.Select(d => new DetalleReceta
        {
          ProductoFarmaceutico = d.ProductoFarmaceutico,
          Concentracion = d.Concentracion,
          Frecuencia = d.Frecuencia,
          Duracion = d.Duracion,
          ViaAdministracion = d.ViaAdministracion,
          Observaciones = d.Observaciones
        }).ToList()
      };
    }

    private RecetaResponseDTO MapToDTO(Receta receta)
    {
      return new RecetaResponseDTO
      {
        IdReceta = receta.IdReceta,
        IdCita = receta.IdCita,
        IdMedico = receta.IdMedico,
        IdPaciente = receta.IdPaciente,
        IdHistoriaClinica = receta.IdHistoriaClinica,
        FechaEmision = receta.FechaEmision,
        ObservacionesGenerales = receta.ObservacionesGenerales,
        EstadoReceta = receta.EstadoReceta,
        NombreCompletoPaciente = $"{receta.Paciente?.ApellidoPaterno ?? ""} {receta.Paciente?.ApellidoMaterno ?? ""} {receta.Paciente?.Nombre ?? ""}".Trim(),
        NumeroDocumentoPaciente = receta.Paciente?.NumeroDocumento ?? "",
        NombreCompletoMedico = $"Dr(a). {receta.Medico?.Nombre ?? ""} {receta.Medico?.ApellidoPaterno ?? ""}".Trim(),
        EspecialidadMedico = receta.Medico?.AreaServicio ?? "",
        Detalles = receta.Detalles?.Select(MapDetalleToDTO).ToList() ?? new List<DetalleRecetaResponseDTO>()
      };
    }

    private DetalleRecetaResponseDTO MapDetalleToDTO(DetalleReceta detalle)
    {
      return new DetalleRecetaResponseDTO
      {
        IdDetalle = detalle.IdDetalle,
        ProductoFarmaceutico = detalle.ProductoFarmaceutico,
        Concentracion = detalle.Concentracion,
        Frecuencia = detalle.Frecuencia,
        Duracion = detalle.Duracion,
        ViaAdministracion = detalle.ViaAdministracion,
        Observaciones = detalle.Observaciones
      };
    }
  }
}