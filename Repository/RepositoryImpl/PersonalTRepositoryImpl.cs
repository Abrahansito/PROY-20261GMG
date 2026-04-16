using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Data;

namespace SGMG.Repository.RepositoryImpl
{
  public class PersonalTRepositoryImpl : IPersonalTRepository
  {
    private readonly ApplicationDbContext _context;

    public PersonalTRepositoryImpl(ApplicationDbContext context)
    {
      _context = context;
    }



    public async Task<PersonalTecnico?> GetPersonalTecnicoByIdAsync(int id)
    {
      return await _context.PersonalTecnicos.FindAsync(id);
    }

    public async Task AddPersonalTecnicoAsync(PersonalTecnico personalTecnico)
    {
      await _context.PersonalTecnicos.AddAsync(personalTecnico);
      await _context.SaveChangesAsync();
    }

    public async Task UpdatePersonalTecnicoAsync(PersonalTecnico personalTecnico)
    {
      _context.PersonalTecnicos.Update(personalTecnico);
      await _context.SaveChangesAsync();
    }

    public async Task DeletePersonalTecnicoAsync(int id)
    {
      var personalTecnico = await _context.PersonalTecnicos.FindAsync(id);
      if (personalTecnico != null)
      {
        _context.PersonalTecnicos.Remove(personalTecnico);
        await _context.SaveChangesAsync();
      }
    }
  }
}