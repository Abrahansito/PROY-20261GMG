using SGMG.Models;

namespace SGMG.Repository
{
    public interface IDiagnosticoRepository
    {
        Task<IEnumerable<Diagnostico>> GetDiagnosticosByPacienteAsync(int idPaciente);
        Task<Diagnostico?> GetDiagnosticoByIdAsync(int idDiagnostico);
        Task AddDiagnosticoAsync(Diagnostico diagnostico);
        Task UpdateDiagnosticoAsync(Diagnostico diagnostico);
    }
}