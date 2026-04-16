using SGMG.Dtos.Request;
using SGMG.Dtos.Response;

namespace SGMG.Services
{
    public interface ILaboratorioService
    {
        Task<GenericResponse<LaboratorioHistorialDTO>> GetHistorialLaboratorioAsync(int idPaciente);
        Task<GenericResponse<OrdenLaboratorioResponseDTO>> CrearOrdenAsync(OrdenLaboratorioRequestDTO dto);
        Task<GenericResponse<OrdenLaboratorioResponseDTO>> ActualizarOrdenAsync(OrdenLaboratorioRequestDTO dto);
        Task<GenericResponse<OrdenLaboratorioResponseDTO>> GetOrdenByIdAsync(int idOrden);
        Task<GenericResponse<bool>> CancelarOrdenAsync(int idOrden); 
        Task<GenericResponse<bool>> ActualizarResultadosAsync(ActualizarResultadosDTO request);
    }
}