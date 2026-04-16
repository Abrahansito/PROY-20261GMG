using SGMG.Models;

namespace SGMG.Repository
{
  public interface ICitaRepository
  {
    Task<IEnumerable<Cita>> GetCitasPendientesAsync();
    Task<IEnumerable<Cita>> GetCitasFueraHorarioAsync();
    Task<IEnumerable<Cita>> GetAllCitasAsync();
    Task<Cita?> GetCitaByIdAsync(int id);


    Task UpdateCitaAsync(Cita cita);
  }
}