using SGMG.Dtos.Response;

namespace SGMG.Services
{
    public interface IHistorialClinicoService
    {
        Task<GenericResponse<HistorialClinicoDTO>> GetHistorialByPacienteAsync(int idPaciente);
        Task<GenericResponse<DiagnosticoResponseDTO>> GetDiagnosticoDetalleAsync(int idDiagnostico);
    }
}