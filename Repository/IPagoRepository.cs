using SGMG.Models;
using System;
using System.Collections.Generic;

namespace SGMG.Repository
{
  public interface IPagoRepository
  {
    IEnumerable<Cita> GetCitasByFiltro(string tipoDocumento, string numeroDocumento, DateTime? fechaInicio, DateTime? fechaFin, string estadoPago);

    Pago? GetPagoById(int id);
    bool UpdatePagoStatus(int id, string estado, DateTime? fechaPago = null);
    Cita? GetCitaById(int idCita);
  }

}