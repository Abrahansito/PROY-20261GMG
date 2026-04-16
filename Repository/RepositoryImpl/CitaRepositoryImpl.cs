using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using SGMG.Data;

namespace SGMG.Repository.RepositoryImpl
{
  public class CitaRepositoryImpl : ICitaRepository
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<CitaRepositoryImpl> _logger;

    public CitaRepositoryImpl(ApplicationDbContext context, ILogger<CitaRepositoryImpl> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task UpdateCitaAsync(Cita cita)
    {
      try
      {
        _context.Citas.Update(cita);
        await _context.SaveChangesAsync();
        _logger.LogInformation($"Cita actualizada con ID: {cita.IdCita}");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al actualizar cita: {ex.Message}");
        throw;
      }
    }

    public async Task<IEnumerable<Cita>> GetCitasPendientesAsync()
    {
      try
      {
        // Citas Confirmadas (Osea que faltan por triar)
        var citas = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Where(c => c.EstadoCita == "Confirmada")
            .ToListAsync();

        var citasOrdenadas = citas
            .OrderBy(c => c.FechaCita)
            .ThenBy(c => c.HoraCita)
            .ToList();

        _logger.LogInformation($"Citas por triar: {citasOrdenadas.Count}");

        return citasOrdenadas;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        throw;
      }
    }

    public async Task<IEnumerable<Cita>> GetCitasFueraHorarioAsync()
    {
      try
      {
        var ahora = DateTime.Now;

        // Citas Confirmadas de fechas pasadas
        var citas = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .Where(c => (c.EstadoCita == "Pagado" || c.EstadoCita == "Confirmada")
                 && c.FechaCita.Date < ahora.Date)
            .ToListAsync();

        var citasOrdenadas = citas
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .ToList();

        _logger.LogInformation($"Citas fuera de horario: {citasOrdenadas.Count}");

        return citasOrdenadas;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error: {ex.Message}");
        throw;
      }
    }

    public async Task<IEnumerable<Cita>> GetAllCitasAsync()
    {
      try
      {
        // Traer todas las citas
        var citas = await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .OrderByDescending(c => c.FechaCita)
            .ToListAsync();

        // Ordenar por HoraCita en memoria
        var citasOrdenadas = citas
            .OrderByDescending(c => c.FechaCita)
            .ThenByDescending(c => c.HoraCita)
            .ToList();

        _logger.LogInformation($"Total citas: {citasOrdenadas.Count}");

        return citasOrdenadas;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error en GetAllCitasAsync: {ex.Message}");
        throw;
      }
    }

    public async Task<Cita?> GetCitaByIdAsync(int id)
    {
      try
      {
        return await _context.Citas
            .Include(c => c.Paciente)
            .Include(c => c.Medico)
            .FirstOrDefaultAsync(c => c.IdCita == id);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error en GetCitaByIdAsync: {ex.Message}");
        throw;
      }
    }
  }
}
