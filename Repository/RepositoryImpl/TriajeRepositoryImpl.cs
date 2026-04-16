using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using SGMG.Data;

namespace SGMG.Repository.RepositoryImpl
{
    public class TriajeRepositoryImpl : ITriajeRepository
    {
        private readonly ApplicationDbContext _context;

        public TriajeRepositoryImpl(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Triaje>> GetAllTriajesAsync()
        {
            try
            {
                // Traer TODOS los triajes (sin filtrar por estado)
                var triajes = await _context.Triages
                    .Include(t => t.Paciente)
                    .OrderByDescending(t => t.FechaTriage)
                    .ToListAsync();

                var triajesOrdenados = triajes
                    .OrderByDescending(t => t.FechaTriage)
                    .ThenByDescending(t => t.HoraTriage)
                    .ToList();

                Console.WriteLine($"Total triajes registrados: {triajesOrdenados.Count}");

                return triajesOrdenados;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                throw;
            }
        }
     

        public async Task<Triaje?> GetTriajeByIdAsync(int id)
        {
            return await _context.Triages.Include(t => t.Paciente).FirstOrDefaultAsync(t => t.IdTriage == id);
        }

        public async Task<Triaje?> GetTriajeByPacienteAsync(int idPaciente)
        {
            return await _context.Triages
                .Include(t => t.Paciente)
                .Where(t => t.IdPaciente == idPaciente)
                .OrderByDescending(t => t.FechaTriage)
                .FirstOrDefaultAsync();
        }

        public async Task AddTriajeAsync(Triaje triaje)
        {
            triaje.FechaTriage = DateTime.UtcNow;
            triaje.HoraTriage = DateTime.UtcNow.TimeOfDay;
            await _context.Triages.AddAsync(triaje);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateTriajeAsync(Triaje triaje)
        {
            _context.Triages.Update(triaje);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteTriajeAsync(int id)
        {
            var triaje = await _context.Triages.FindAsync(id);
            if (triaje != null)
            {
                _context.Triages.Remove(triaje);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Triaje>> GetTriajesByPacienteAsync(int idPaciente)
        {
            var triajes = await _context.Triages
                .Include(t => t.Paciente)
                .Where(t => t.IdPaciente == idPaciente)
                .ToListAsync();

            // Ordenar en memoria
            return triajes
                .OrderByDescending(t => t.FechaTriage)
                .ThenByDescending(t => t.HoraTriage)
                .ToList();
        }
    }
}