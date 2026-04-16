using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Response;
using SGMG.Models;


namespace SGMG.Repository
{
  public interface IPersonalTRepository
  {

    Task<PersonalTecnico?> GetPersonalTecnicoByIdAsync(int id);
    Task AddPersonalTecnicoAsync(PersonalTecnico personalTecnico);
    Task UpdatePersonalTecnicoAsync(PersonalTecnico personalTecnico);
    Task DeletePersonalTecnicoAsync(int id);
  }
}