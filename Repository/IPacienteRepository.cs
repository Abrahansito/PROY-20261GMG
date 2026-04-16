using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Dtos.Request.Paciente;
using SGMG.Models;

namespace SGMG.Repository
{
  public interface IPacienteRepository
  {
    Task<IEnumerable<Paciente>> GetAllPacientesAsync();
    Task<Paciente?> GetPacienteByDocumentoAsync(string tipoDocumento, string numeroDocumento);
    Task<IEnumerable<Cita>> GetCitasPendientesByPacienteAsync(int idPaciente);
    Task<Paciente?> GetPacienteByIdAsync(int id);
    Task AddPacienteAsync(Paciente paciente);
    Task UpdatePacienteAsync(Paciente paciente);
    Task DeletePacienteAsync(int id);
    Task<IEnumerable<Derivacion>> GetDerivacionesByPacienteAsync(int idPaciente);
  }
}