using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using SGMG.Data;

namespace SGMG.Repository.RepositoryImpl
{
  public class PacienteRepositoryImpl : IPacienteRepository
  {
    private readonly ApplicationDbContext _context;

    public PacienteRepositoryImpl(ApplicationDbContext context)
    {
      _context = context;
    }

    public async Task<IEnumerable<Paciente>> GetAllPacientesAsync()
    {
      return await _context.Pacientes.ToListAsync();
    }

    public async Task<Paciente?> GetPacienteByDocumentoAsync(string tipoDocumento, string numeroDocumento)
    {
      return await _context.Pacientes
        .FirstOrDefaultAsync(p => p.TipoDocumento == tipoDocumento && p.NumeroDocumento == numeroDocumento);
    }

    public async Task<IEnumerable<Cita>> GetCitasPendientesByPacienteAsync(int idPaciente)
    {
      return await _context.Citas
        .Include(c => c.Paciente)
        .Include(c => c.Medico)
        .Where(c => c.IdPaciente == idPaciente &&
               (c.EstadoCita == "Pendiente" || c.EstadoCita == "Pagado"))
        .OrderByDescending(c => c.FechaRegistro)
        .ToListAsync();
    }

    // NUEVO: Obtener derivaciones de un paciente
    public async Task<IEnumerable<Derivacion>> GetDerivacionesByPacienteAsync(int idPaciente)
    {
      return await _context.Derivaciones
        .Include(d => d.Cita)
          .ThenInclude(c => c.Paciente)
        .Include(d => d.Cita)
          .ThenInclude(c => c.Medico)
        .Include(d => d.MedicoDestino)
        .Where(d => d.Cita.IdPaciente == idPaciente)
        .OrderByDescending(d => d.FechaDerivacion)
        .ToListAsync();
    }

    public async Task<Paciente?> GetPacienteByIdAsync(int id)
    {
      return await _context.Pacientes.FindAsync(id);
    }

    public async Task AddPacienteAsync(Paciente paciente)
    {
      await _context.Pacientes.AddAsync(paciente);
      await _context.SaveChangesAsync();
    }

    public async Task UpdatePacienteAsync(Paciente paciente)
    {
      _context.Pacientes.Update(paciente);
      await _context.SaveChangesAsync();
    }

    public async Task DeletePacienteAsync(int id)
    {
      var paciente = await _context.Pacientes.FindAsync(id);
      if (paciente != null)
      {
        _context.Pacientes.Remove(paciente);
        await _context.SaveChangesAsync();
      }
    }
  }
}