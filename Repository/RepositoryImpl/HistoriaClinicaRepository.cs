using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using SGMG.Data;

namespace SGMG.Repository.RepositoryImpl
{
    public class HistoriaClinicaRepository : IHistoriaClinicaRepository
    {
        private readonly ApplicationDbContext _context;

        public HistoriaClinicaRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaByIdAsync(int id)
        {
            return await _context.HistoriasClinicas
                .Include(h => h.Paciente)
                .FirstOrDefaultAsync(h => h.IdHistoria == id);
        }

        public async Task<IEnumerable<HistoriaClinica>> GetAllHistoriasClinicasAsync()
        {
            return await _context.HistoriasClinicas
                .Include(h => h.Paciente)
                .ToListAsync();
        }

        public async Task<HistoriaClinica> GetHistoriaClinicaByPacienteIdAsync(int idPaciente)
        {
            return await _context.HistoriasClinicas
                .Include(h => h.Paciente)
                .FirstOrDefaultAsync(h => h.IdPaciente == idPaciente);
        }

        public async Task AddHistoriaClinicaAsync(HistoriaClinica historiaClinica)
        {
            await _context.HistoriasClinicas.AddAsync(historiaClinica);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateHistoriaClinicaAsync(HistoriaClinica historiaClinica)
        {
            _context.HistoriasClinicas.Update(historiaClinica);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteHistoriaClinicaAsync(int id)
        {
            var historiaClinica = await GetHistoriaClinicaByIdAsync(id);
            if (historiaClinica != null)
            {
                _context.HistoriasClinicas.Remove(historiaClinica);
                await _context.SaveChangesAsync();
            }
        }
    }
}