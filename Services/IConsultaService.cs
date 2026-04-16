using SGMG.Dtos.Request;
using SGMG.Dtos.Response;

namespace SGMG.Services
{
    public interface IConsultaService
    {
        Task<GenericResponse<ConsultaResponseDTO>> AddConsultaAsync(ConsultaRequestDTO dto);
        Task<GenericResponse<ConsultaResponseDTO>> UpdateConsultaAsync(ConsultaRequestDTO dto);
        Task<GenericResponse<ConsultaResponseDTO>> GetConsultaByIdAsync(int id);
    }
}