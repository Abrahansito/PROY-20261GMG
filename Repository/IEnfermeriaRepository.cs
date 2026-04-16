using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Models;
using SGMG.Dtos.Response;

namespace SGMG.Repository
{
  public interface IEnfermeriaRepository
  {
    Task<IEnumerable<EnfermeriaResponse>> GetAllEnfermeriasAsync();
    Task<Enfermeria?> GetEnfermeriaByIdAsync(int id);
    Task AddEnfermeriaAsync(Enfermeria enfermeria);
    Task UpdateEnfermeriaAsync(Enfermeria enfermeria);
    Task DeleteEnfermeriaAsync(int id);
  }
}