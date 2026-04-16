using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Models;

namespace SGMG.Repository
{
    public interface ITriajeRepository
    {
        Task<IEnumerable<Triaje>> GetAllTriajesAsync();
        Task<Triaje?> GetTriajeByIdAsync(int id);
        Task<Triaje?> GetTriajeByPacienteAsync(int idPaciente);
        Task AddTriajeAsync(Triaje triaje);
        Task UpdateTriajeAsync(Triaje triaje);
        Task DeleteTriajeAsync(int id);
        Task<IEnumerable<Triaje>> GetTriajesByPacienteAsync(int idPaciente);
    }
}