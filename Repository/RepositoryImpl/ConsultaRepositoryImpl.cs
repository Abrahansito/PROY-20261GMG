using Microsoft.EntityFrameworkCore;
using SGMG.Data;
using SGMG.Models;

namespace SGMG.Repository.RepositoryImpl
{
    public class ConsultaRepositoryImpl : IConsultaRepository
    {
        private readonly ApplicationDbContext _context;

        public ConsultaRepositoryImpl(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Consulta?> GetConsultaByIdAsync(int id)
        {
            return await _context.Consultas
                .Include(c => c.Paciente)
                .Include(c => c.Medico)
                .FirstOrDefaultAsync(c => c.IdConsulta == id);
        }

        public async Task<IEnumerable<Consulta>> GetConsultasByPacienteAsync(int idPaciente)
        {
            var consultas = await _context.Consultas
                .Include(c => c.Medico)
                .Where(c => c.IdPaciente == idPaciente)
                .ToListAsync();

            return consultas
                .OrderByDescending(c => c.FechaConsulta)
                .ThenByDescending(c => c.HoraConsulta)
                .ToList();
        }

        public async Task AddConsultaAsync(Consulta consulta)
        {
            consulta.FechaConsulta = DateTime.UtcNow;
            consulta.HoraConsulta = DateTime.UtcNow.TimeOfDay;
            await _context.Consultas.AddAsync(consulta);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateConsultaAsync(Consulta consulta)
        {
            _context.Consultas.Update(consulta);
            await _context.SaveChangesAsync();
        }
    }
}