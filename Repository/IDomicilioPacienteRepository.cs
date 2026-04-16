using System.Collections.Generic;
using System.Threading.Tasks;
using SGMG.Models;

namespace SGMG.Repository
{
    public interface IDomicilioPacienteRepository
    {
        Task<IEnumerable<DomicilioPaciente>> GetAllDomiciliosAsync();
        Task<DomicilioPaciente> GetDomicilioByIdAsync(int id);
        Task<DomicilioPaciente> GetDomicilioByPacienteIdAsync(int idPaciente);
        Task AddDomicilioAsync(DomicilioPaciente domicilio);
        Task UpdateDomicilioAsync(DomicilioPaciente domicilio);
        Task DeleteDomicilioAsync(int id);
    }
}