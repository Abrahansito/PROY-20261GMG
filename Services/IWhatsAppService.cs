using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using SGMG.Dtos.Response;

namespace PROY_20252SGMG.Services
{
  public interface IWhatsAppService
  {
    Task<GenericResponse<string>> EnviarRecordatorioCitaAsync(int idCita);
  }
}