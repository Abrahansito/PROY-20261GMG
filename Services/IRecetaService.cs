using SGMG.Dtos.Response;
using PROY_20252SGMG.Dtos.Request;
using PROY_20252SGMG.Dtos.Response;

namespace PROY_20252SGMG.Services
{
  public interface IRecetaService
  {
    // ===== NUEVOS MÉTODOS PARA ESTADOS =====

    /// <summary>
    /// Agrega un medicamento individual con estado PENDIENTE
    /// </summary>
    Task<GenericResponse<DetalleRecetaResponseDTO>> AgregarMedicamentoPendienteAsync(AgregarMedicamentoRequestDTO request);

    /// <summary>
    /// Obtiene medicamentos PENDIENTES filtrados por cita, médico, paciente e HC
    /// </summary>
    Task<GenericResponse<IEnumerable<DetalleRecetaResponseDTO>>> GetMedicamentosPendientesAsync(
        int idCita, int idMedico, int idPaciente, int? idHistoriaClinica);

    /// <summary>
    /// Elimina un medicamento PENDIENTE
    /// </summary>
    Task<GenericResponse<bool>> EliminarMedicamentoPendienteAsync(int idDetalle);

    /// <summary>
    /// Completa la receta: Cambia medicamentos PENDIENTES a COMPLETADO
    /// </summary>
    Task<GenericResponse<RecetaResponseDTO>> CompletarRecetaAsync(RecetaRequestDTO request);

    // ===== MÉTODOS ORIGINALES =====

    Task<GenericResponse<RecetaResponseDTO>> CreateRecetaAsync(RecetaRequestDTO request);
    Task<GenericResponse<RecetaResponseDTO>> GetRecetaByIdAsync(int id);
    Task<GenericResponse<IEnumerable<RecetaResponseDTO>>> GetRecetasByPacienteAsync(int idPaciente);
    Task<GenericResponse<IEnumerable<RecetaResponseDTO>>> GetRecetasByPacienteAsyncCompleto(int idPaciente);
    Task<GenericResponse<IEnumerable<RecetaResponseDTO>>> GetRecetasByMedicoAsync(int idMedico);
    Task<GenericResponse<RecetaResponseDTO>> GetRecetaByCitaAsync(int idCita);
    Task<GenericResponse<bool>> ImprimirRecetaAsync(int id);


  }
}