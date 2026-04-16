using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using SGMG.Data;

namespace SGMG.Repository.RepositoryImpl
{
    public class DomicilioPacienteRepository : IDomicilioPacienteRepository
    {
        private readonly ApplicationDbContext _context;

        public DomicilioPacienteRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<DomicilioPaciente> GetDomicilioByIdAsync(int id)
        {
            return await _context.DomiciliosPacientes
                .Include(d => d.Paciente)
                .FirstOrDefaultAsync(d => d.IdDomicilio == id);
        }

        public async Task<IEnumerable<DomicilioPaciente>> GetAllDomiciliosAsync()
        {
            return await _context.DomiciliosPacientes
                .Include(d => d.Paciente)
                .ToListAsync();
        }

        public async Task<DomicilioPaciente> GetDomicilioByPacienteIdAsync(int idPaciente)
        {
            return await _context.DomiciliosPacientes
                .Include(d => d.Paciente)
                .FirstOrDefaultAsync(d => d.IdPaciente == idPaciente);
        }

        public async Task AddDomicilioAsync(DomicilioPaciente domicilio)
        {
            await _context.DomiciliosPacientes.AddAsync(domicilio);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDomicilioAsync(DomicilioPaciente domicilio)
        {
            _context.DomiciliosPacientes.Update(domicilio);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteDomicilioAsync(int id)
        {
            var domicilio = await GetDomicilioByIdAsync(id);
            if (domicilio != null)
            {
                _context.DomiciliosPacientes.Remove(domicilio);
                await _context.SaveChangesAsync();
            }
        }
    }
}