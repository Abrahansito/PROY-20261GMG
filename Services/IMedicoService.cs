using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Request.Medico;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Repository;
using SGMG.common.exception;

namespace SGMG.Services
{
  public interface IMedicoService
  {
        Task<GenericResponse<IEnumerable<Medico>>> GetAllMedicosAsync();
        Task<GenericResponse<Medico>> GetMedicoByIdAsync(int id);
        Task<GenericResponse<Medico>> AddMedicoAsync(MedicoRequestDTO medicoRequestDTO);
        Task<GenericResponse<Medico>> UpdateMedicoAsync(MedicoRequestDTO medicoRequestDTO);
        Task<GenericResponse<Medico>> DeleteMedicoAsync(int id);
        Task<GenericResponse<IEnumerable<Medico>>> GetMedicosFilteredAsync(string? numeroDni, string? idConsultorio, string? estado, string? fechaInicio, string? fechaFin, string? turno);
        Task<GenericResponse<DisponibilidadSemanaResponse>> GetMedicosDisponiblesPorSemanaAsync(int semana, string? numeroDni, int? idConsultorio, string? estado, string? turno);
        
        // NUEVO MÉTODO para obtener médico por userId
        Task<GenericResponse<Medico>> GetMedicoByUserIdAsync(string userId);           
  }
}