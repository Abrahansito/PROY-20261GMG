using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using SGMG.Repository;
using SGMG.Data;
using SGMG.Dtos.Response;
using SGMG.Dtos.Request;


namespace SGMG.Repository.RepositoryImpl
{
  public class EnfermeriaRepositoryImpl : IEnfermeriaRepository
  {
    private readonly ApplicationDbContext _context;

    // Inyección de dependencias (DbContext viene de Program.cs)
    public EnfermeriaRepositoryImpl(ApplicationDbContext context)
    {
      _context = context;
    }


    public async Task<IEnumerable<EnfermeriaResponse>> GetAllEnfermeriasAsync()
    {
      return await _context.Enfermerias
          .Include(e => e.Personal)
          .Select(e => new EnfermeriaResponse
          {
            EstadoLaboral = e.Personal.EstadoLaboral,
            NumeroDni = e.Personal.NumeroDni,
            NombreCompleto = $"{e.Personal.Nombre} {e.Personal.ApellidoPaterno} {e.Personal.ApellidoMaterno}",
            Cargo = e.Personal.Cargo,
            Telefono = e.Personal.Telefono
          })
          .ToListAsync();
    }

    public async Task<Enfermeria?> GetEnfermeriaByIdAsync(int id)
    {
      return await _context.Enfermerias
          .Include(e => e.Personal)
          .FirstOrDefaultAsync(e => e.IdEnfermeria == id);
    }

    public async Task AddEnfermeriaAsync(Enfermeria enfermeria)
    {
      await _context.Enfermerias.AddAsync(enfermeria);
      await _context.SaveChangesAsync();
    }

    public async Task UpdateEnfermeriaAsync(Enfermeria enfermeria)
    {
      _context.Enfermerias.Update(enfermeria);
      await _context.SaveChangesAsync();
    }

    public async Task DeleteEnfermeriaAsync(int id)
    {
      var enfermeria = await _context.Enfermerias.FindAsync(id);
      if (enfermeria != null)
      {
        _context.Enfermerias.Remove(enfermeria);
        await _context.SaveChangesAsync();
      }
    }
  }
}