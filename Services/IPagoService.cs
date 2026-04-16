using SGMG.Models;
using System;
using System.Collections.Generic;


using SGMG.Dtos.Response;
using System;
using System.Collections.Generic;

namespace SGMG.Services
{
  public interface IPagoService
  {
    /// <summary>
    /// Obtiene el historial de citas filtradas por diferentes criterios
    /// </summary>
    IEnumerable<Cita> GetHistorialPagos(string tipoDocumento, string numeroDocumento, DateTime? fechaInicio, DateTime? fechaFin, string estado);
    bool Pagar(int idPago);

    /// <summary>
    /// Obtiene el historial de citas como DTOs listos para el frontend
    /// </summary>
    IEnumerable<PagoResponseDTO> GetHistorialPagosDto(string tipoDocumento, string numeroDocumento, DateTime? fechaInicio, DateTime? fechaFin, string estado);

    /// <summary>
    /// Obtiene los detalles de un pago específico por ID
    /// </summary>
    Pago? GetPagoById(int id);

    /// <summary>
    /// Obtiene los detalles de un pago como DTO listo para el frontend
    /// </summary>
    PagoResponseDTO? GetPagoByIdDto(int id);

    /// <summary>
    /// Marca un pago como pagado
    /// </summary>

    PagoResponseDTO? GetResumenByCitaId(int idCita);
    /// <summary>
    /// Actualiza el estado de un pago
    /// </summary>
    bool UpdatePagoStatus(int id, string estado, DateTime? fechaPago = null);
  }
}