using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Dtos.Request.PersonalTecnico;

namespace SGMG.Services
{
  public interface IPersonalTservice
  {

    Task<GenericResponse<PersonalTecnico>> GetPersonalTecnicoByIdAsync(int id);
    Task<GenericResponse<PersonalTecnico>> AddPersonalTecnicoAsync(PersonalTecnicoRequestDTO personalTecnicoRequestDTO);
    Task<GenericResponse<PersonalTecnico>> UpdatePersonalTecnicoAsync(PersonalTecnicoRequestDTO personalTecnicoRequestDTO);
    Task<GenericResponse<PersonalTecnico>> DeletePersonalTecnicoAsync(int id);
  }
}