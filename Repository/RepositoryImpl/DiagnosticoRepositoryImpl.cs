using Microsoft.EntityFrameworkCore;
using SGMG.Data;
using SGMG.Models;

namespace SGMG.Repository.RepositoryImpl
{
    public class DiagnosticoRepositoryImpl : IDiagnosticoRepository
    {
        private readonly ApplicationDbContext _context;

        public DiagnosticoRepositoryImpl(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Diagnostico>> GetDiagnosticosByPacienteAsync(int idPaciente)
        {
            var diagnosticos = await _context.Diagnosticos
                .Include(d => d.Medico)
                .Include(d => d.Cita)
                .Where(d => d.IdPaciente == idPaciente)
                .ToListAsync();

            // Ordenar en memoria (SQLite no soporta TimeSpan en OrderBy)
            return diagnosticos
                .OrderByDescending(d => d.FechaDiagnostico)
                .ThenByDescending(d => d.HoraDiagnostico)
                .ToList();
        }

        public async Task<Diagnostico?> GetDiagnosticoByIdAsync(int idDiagnostico)
        {
            return await _context.Diagnosticos
                .Include(d => d.Medico)
                .Include(d => d.Cita)
                .Include(d => d.Paciente)
                .FirstOrDefaultAsync(d => d.IdDiagnostico == idDiagnostico);
        }

        public async Task AddDiagnosticoAsync(Diagnostico diagnostico)
        {
            diagnostico.FechaDiagnostico = DateTime.UtcNow;
            diagnostico.HoraDiagnostico = DateTime.UtcNow.TimeOfDay;
            await _context.Diagnosticos.AddAsync(diagnostico);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDiagnosticoAsync(Diagnostico diagnostico)
        {
            _context.Diagnosticos.Update(diagnostico);
            await _context.SaveChangesAsync();
        }
    }
}