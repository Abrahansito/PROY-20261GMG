using System.Collections.Generic;
using System.Threading.Tasks;
using SGMG.Models;

namespace SGMG.Repository
{
    public interface IHistoriaClinicaRepository
    {
        Task<IEnumerable<HistoriaClinica>> GetAllHistoriasClinicasAsync();
        Task<HistoriaClinica> GetHistoriaClinicaByIdAsync(int id);
        Task<HistoriaClinica> GetHistoriaClinicaByPacienteIdAsync(int idPaciente);
        Task AddHistoriaClinicaAsync(HistoriaClinica historiaClinica);
        Task UpdateHistoriaClinicaAsync(HistoriaClinica historiaClinica);
        Task DeleteHistoriaClinicaAsync(int id);
    }
}