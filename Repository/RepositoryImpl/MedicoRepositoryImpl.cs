using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using SGMG.Repository;
using SGMG.Data;

namespace SGMG.Repository.RepositoryImpl
{
    public class MedicoRepositoryImpl : IMedicoRepository
    {
        private readonly ApplicationDbContext _context;

        public MedicoRepositoryImpl(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Medico>> GetAllMedicosAsync()
        {
            return await _context.Medicos
                .Include(m => m.ConsultorioAsignado)
                .ToListAsync();
        }

        public async Task<Medico?> GetMedicoByIdAsync(int id)
        {
            return await _context.Medicos
                .Include(m => m.ConsultorioAsignado)
                .FirstOrDefaultAsync(m => m.IdMedico == id);
        }

        public async Task<IEnumerable<Medico>> GetMedicosFilteredAsync(string? numeroDni, int? idConsultorio, string? estado, DateTime? fechaInicio, DateTime? fechaFin, string? turno)
        {
            var query = _context.Medicos
                .Include(m => m.ConsultorioAsignado)
                .AsQueryable();

            if (!string.IsNullOrEmpty(numeroDni))
            {
                query = query.Where(m => m.NumeroDni.Contains(numeroDni));
            }

            if (idConsultorio.HasValue)
            {
                query = query.Where(m => m.IdConsultorio == idConsultorio.Value);
            }

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(m => m.EstadoLaboral == estado);
            }

            if (fechaInicio.HasValue)
            {
                query = query.Where(m => m.FechaIngreso >= fechaInicio.Value);
            }

            if (fechaFin.HasValue)
            {
                query = query.Where(m => m.FechaIngreso <= fechaFin.Value);
            }

            if (!string.IsNullOrEmpty(turno))
            {
                query = query.Where(m => m.Turno == turno);
            }

            return await query.ToListAsync();
        }

        public async Task AddMedicoAsync(Medico medico)
        {
            await _context.Medicos.AddAsync(medico);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateMedicoAsync(Medico medico)
        {
            _context.Medicos.Update(medico);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteMedicoAsync(int id)
        {
            var medico = await _context.Medicos.FindAsync(id);
            if (medico != null)
            {
                _context.Medicos.Remove(medico);
                await _context.SaveChangesAsync();
            }
        }
    }

}