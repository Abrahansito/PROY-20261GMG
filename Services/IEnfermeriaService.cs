using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Models;
using SGMG.Dtos.Response;
using SGMG.Dtos.Request.Enfermeria;


namespace SGMG.Services
{
  public interface IEnfermeriaService
  {
    Task<GenericResponse<IEnumerable<EnfermeriaResponse>>> GetAllEnfermeriasAsync();
    Task<GenericResponse<Enfermeria>> GetEnfermeriaByIdAsync(int id);
    Task<GenericResponse<Enfermeria>> AddEnfermeriaAsync(EnfermeriaRequestDTO enfermeriaRequestDTO);
    Task<GenericResponse<Enfermeria>> UpdateEnfermeriaAsync(EnfermeriaRequestDTO enfermeriaRequestDTO);
    Task<GenericResponse<Enfermeria>> DeleteEnfermeriaAsync(int id);
  }
}