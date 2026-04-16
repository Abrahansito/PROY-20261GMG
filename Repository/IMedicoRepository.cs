using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Models;

namespace SGMG.Repository
{
  public interface IMedicoRepository
  {
    Task<IEnumerable<Medico>> GetAllMedicosAsync();
    Task<Medico?> GetMedicoByIdAsync(int id);
    // Obtener médicos con filtros opcionales
    Task<IEnumerable<Medico>> GetMedicosFilteredAsync(string? numeroDni, int? idConsultorio, string? estado, DateTime? fechaInicio, DateTime? fechaFin, string? turno);
    Task AddMedicoAsync(Medico medico);
    Task UpdateMedicoAsync(Medico medico);
    Task DeleteMedicoAsync(int id);
  }

}