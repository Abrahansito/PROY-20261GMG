using SGMG.Models;

namespace SGMG.Repository
{
    public interface IConsultaRepository
    {
        Task<Consulta?> GetConsultaByIdAsync(int id);
        Task<IEnumerable<Consulta>> GetConsultasByPacienteAsync(int idPaciente);
        Task AddConsultaAsync(Consulta consulta);
        Task UpdateConsultaAsync(Consulta consulta);
    }
}