using SGMG.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using SGMG.Data;
using Microsoft.Extensions.Logging;

namespace SGMG.Repository.RepositoryImpl
{
  public class PagoRepository : IPagoRepository
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<PagoRepository> _logger;

    public PagoRepository(ApplicationDbContext context, ILogger<PagoRepository> logger)
    {
      _context = context;
      _logger = logger;
    }

    public IEnumerable<Cita> GetCitasByFiltro(string tipoDocumento, string numeroDocumento, DateTime? fechaInicio, DateTime? fechaFin, string estadoPago)
    {
      _logger.LogInformation("=== INICIO GetCitasByFiltro ===");
      _logger.LogInformation($"Parámetros recibidos: TipoDoc={tipoDocumento}, NumDoc={numeroDocumento}, FechaInicio={fechaInicio}, FechaFin={fechaFin}, EstadoPago={estadoPago}");

      // Partir desde Citas para hacer el filtrado
      var queryCitas = _context.Citas
          .Include(c => c.Paciente)
          .Include(c => c.Medico)
          .Include(c => c.Pagos)
          .AsQueryable();

      if (!string.IsNullOrWhiteSpace(tipoDocumento))
        queryCitas = queryCitas.Where(c => c.Paciente != null && c.Paciente.TipoDocumento == tipoDocumento);

      if (!string.IsNullOrWhiteSpace(numeroDocumento))
        queryCitas = queryCitas.Where(c => c.Paciente != null && c.Paciente.NumeroDocumento == numeroDocumento);

      if (fechaInicio.HasValue)
        queryCitas = queryCitas.Where(c => c.FechaCita.Date >= fechaInicio.Value.Date);

      if (fechaFin.HasValue)
        queryCitas = queryCitas.Where(c => c.FechaCita.Date <= fechaFin.Value.Date);

      // Filtrar citas según estado de pagos si se especifica
      if (!string.IsNullOrWhiteSpace(estadoPago) && estadoPago != "Todos")
      {
        queryCitas = queryCitas.Where(c => c.Pagos.Any(p => p.EstadoPago == estadoPago));
      }

      var citasFiltradas = queryCitas
          .OrderByDescending(c => c.FechaCita)
          .ToList();

      _logger.LogInformation($"Total de citas encontradas después de filtrar: {citasFiltradas.Count}");
      return citasFiltradas;
    }


    public Pago? GetPagoById(int id)
    {
      _logger.LogInformation($"Buscando pago con ID: {id}");

      var pago = _context.Pagos
          .Include(p => p.Cita)
              .ThenInclude(c => c.Paciente)
                  .ThenInclude(pa => pa.HistoriasClinicas)
          .Include(p => p.Cita)
              .ThenInclude(c => c.Medico)
          .AsQueryable()
          .FirstOrDefault(p => p.IdPago == id);

      if (pago == null)
        _logger.LogWarning($"No se encontró pago con ID: {id}");
      else
        _logger.LogInformation($"Pago encontrado: ID={pago.IdPago}, Estado={pago.EstadoPago}");

      return pago;
    }
    public Cita? GetCitaById(int idCita)
    {
      _logger.LogInformation($"Buscando cita con ID: {idCita}");

      var cita = _context.Citas
          .Include(c => c.Paciente)
              .ThenInclude(p => p.HistoriasClinicas)
          .Include(c => c.Medico)
          .Include(c => c.Pagos)
          .FirstOrDefault(c => c.IdCita == idCita);

      if (cita == null)
        _logger.LogWarning($"No se encontró cita con ID: {idCita}");
      else
        _logger.LogInformation($"Cita encontrada: ID={cita.IdCita}, Fecha={cita.FechaCita}, Paciente={cita.Paciente?.NumeroDocumento}, CantidadPagos={cita.Pagos?.Count ?? 0}");

      return cita;
    }
    public bool UpdatePagoStatus(int id, string estado, DateTime? fechaPago = null)
    {
      _logger.LogInformation($"Actualizando pago ID={id}, NuevoEstado={estado}, NuevaFecha={fechaPago}");

      var pago = _context.Pagos.FirstOrDefault(p => p.IdPago == id);

      if (pago == null)
      {
        _logger.LogWarning($"No se encontró pago con ID: {id}");
        return false;
      }

      var estadoAnterior = pago.EstadoPago;
      var fechaAnterior = pago.FechaPago;

      pago.EstadoPago = estado ?? pago.EstadoPago;
      if (fechaPago.HasValue)
        pago.FechaPago = fechaPago.Value;

      _context.Pagos.Update(pago);
      _context.SaveChanges();

      _logger.LogInformation($"Pago actualizado: EstadoAnterior={estadoAnterior} -> EstadoNuevo={pago.EstadoPago}, FechaAnterior={fechaAnterior} -> FechaNueva={pago.FechaPago}");

      return true;
    }
  }
}