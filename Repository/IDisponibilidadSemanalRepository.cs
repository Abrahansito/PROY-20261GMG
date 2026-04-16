using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using SGMG.Models;

namespace SGMG.Repository
{
  public interface IDisponibilidadSemanalRepository
  {
    Task<IEnumerable<DisponibilidadSemanal>> GetDisponibilidadesPorSemanaAsync(
            DateTime fechaInicio,
            DateTime fechaFin);

    Task<DisponibilidadSemanal?> GetDisponibilidadByMedicoYSemanaAsync(
        int idMedico,
        DateTime fechaInicio,
        DateTime fechaFin);
  }
}