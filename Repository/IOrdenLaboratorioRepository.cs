using SGMG.Models;

namespace SGMG.Repository
{
    public interface IOrdenLaboratorioRepository
    {
        Task<IEnumerable<OrdenLaboratorio>> GetOrdenesByPacienteAsync(int idPaciente);
        Task<OrdenLaboratorio?> GetOrdenByIdAsync(int idOrden);
        Task AddOrdenAsync(OrdenLaboratorio orden);
        Task UpdateOrdenAsync(OrdenLaboratorio orden);
        Task<string> GenerarNumeroOrdenAsync();  
    }
}