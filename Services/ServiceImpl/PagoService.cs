using SGMG.Models;
using SGMG.Repository;
using System;
using System.Collections.Generic;
using System.Linq;
using SGMG.Dtos.Response;

using Microsoft.Extensions.Logging;

namespace SGMG.Services.ServiceImpl
{
  public class PagoService : IPagoService
  {
    private readonly IPagoRepository _pagoRepository;
    private readonly ILogger<PagoService> _logger;

    public PagoService(IPagoRepository pagoRepository, ILogger<PagoService> logger)
    {
      _pagoRepository = pagoRepository;
      _logger = logger;
    }

    public IEnumerable<Cita> GetHistorialPagos(string tipoDocumento, string numeroDocumento, DateTime? fechaInicio, DateTime? fechaFin, string estado)
    {
      _logger.LogInformation("=== INICIO GetHistorialPagos (Service) ===");
      _logger.LogInformation($"Parámetros: TipoDoc={tipoDocumento}, NumDoc={numeroDocumento}, FechaInicio={fechaInicio}, FechaFin={fechaFin}, Estado={estado}");

      var citas = _pagoRepository.GetCitasByFiltro(tipoDocumento, numeroDocumento, fechaInicio, fechaFin, estado);

      _logger.LogInformation($"Citas obtenidas: {citas.Count()}");

      foreach (var cita in citas)
      {
        _logger.LogInformation($"  Cita ID={cita.IdCita}, Fecha={cita.FechaCita:dd/MM/yyyy}, Hora={cita.HoraCita}, Paciente={cita.Paciente?.NumeroDocumento}, CantidadPagos={cita.Pagos?.Count ?? 0}");
      }

      _logger.LogInformation("=== FIN GetHistorialPagos (Service) ===");

      return citas;
    }
    public IEnumerable<PagoResponseDTO> GetHistorialPagosDto(string tipoDocumento, string numeroDocumento, DateTime? fechaInicio, DateTime? fechaFin, string estado)
    {
      _logger.LogInformation("=== INICIO GetHistorialPagosDto (Service) ===");
      _logger.LogInformation($"Parámetros: TipoDoc={tipoDocumento}, NumDoc={numeroDocumento}, FechaInicio={fechaInicio}, FechaFin={fechaFin}, Estado={estado}");

      var citas = GetHistorialPagos(tipoDocumento, numeroDocumento, fechaInicio, fechaFin, estado);

      _logger.LogInformation($"Total de citas obtenidas: {citas.Count()}");

      // Mapear cada CITA (no cada pago) a un DTO
      var citasDto = citas
          .Select(cita =>
          {
            _logger.LogInformation($"Procesando Cita ID={cita.IdCita}, Fecha={cita.FechaCita:dd/MM/yyyy}, Hora={cita.HoraCita}, Paciente={cita.Paciente?.NumeroDocumento}");
            _logger.LogInformation($"  - Médico: {cita.Medico?.Nombre} {cita.Medico?.ApellidoPaterno}");
            _logger.LogInformation($"  - Especialidad: {cita.Especialidad}, Consultorio: {cita.Consultorio}");
            _logger.LogInformation($"  - Cantidad de pagos en esta cita: {cita.Pagos?.Count ?? 0}");

            // Obtener el primer pago (o valores por defecto si no hay pagos)
            var primerPago = cita.Pagos?.FirstOrDefault();

            if (primerPago == null)
            {
              _logger.LogWarning($"  ⚠️ La cita {cita.IdCita} NO tiene pagos asociados - Estado será Pendiente");
            }
            else
            {
              _logger.LogInformation($"    → Pago principal ID={primerPago.IdPago}, Estado={primerPago.EstadoPago}, Total={primerPago.Total}");
            }

            return new PagoResponseDTO
            {
              Id = cita.IdCita, // Usar ID de la CITA
              FechaPago = cita.FechaRegistro.ToString("dd/MM/yyyy"), // Fecha de registro de la CITA (solo fecha)
              FechaCita = cita.FechaCita.ToString("dd/MM/yyyy"),
              HoraCita = cita.HoraCita.ToString(@"hh\:mm"),

              // Datos del Paciente
              TipoDoc = cita.Paciente?.TipoDocumento ?? string.Empty,
              NumeroDoc = cita.Paciente?.NumeroDocumento ?? string.Empty,
              Nombres = cita.Paciente != null
                  ? string.Join(" ", new[] {
                        cita.Paciente.ApellidoPaterno,
                        cita.Paciente.ApellidoMaterno,
                        cita.Paciente.Nombre
                      }.Where(s => !string.IsNullOrWhiteSpace(s)))
                  : string.Empty,
              Edad = cita.Paciente?.Edad ?? 0,
              Telefono = string.Empty,
              HistoriaClinicaCodigo = cita.Paciente?.HistoriasClinicas?.FirstOrDefault()?.CodigoHistoria ?? string.Empty,
              TipoSeguro = cita.Paciente?.HistoriasClinicas?.FirstOrDefault()?.TipoSeguro ?? string.Empty,

              // Datos del Médico y Cita
              Medico = cita.Medico != null
                  ? string.Join(" ", new[] {
                        cita.Medico.ApellidoPaterno,
                        cita.Medico.ApellidoMaterno,
                        cita.Medico.Nombre
                      }.Where(s => !string.IsNullOrWhiteSpace(s)))
                  : string.Empty,
              Especialidad = cita.Especialidad ?? string.Empty,
              Consultorio = cita.Consultorio ?? string.Empty,
              Motivo = string.Empty,

              // Servicios del primer pago (si existe)
              Servicios = primerPago != null
                  ? new List<PagoServicioDTO>
                      {
                        new PagoServicioDTO
                        {
                            Codigo = primerPago.CodigoServicio,
                            Descripcion = primerPago.DescripcionServicio,
                            Cantidad = primerPago.Cantidad,
                            PrecioUnitario = primerPago.PrecioUnitario,
                            Total = primerPago.Cantidad * primerPago.PrecioUnitario
                        }
                      }
                  : new List<PagoServicioDTO>(),

              // Montos (con valores reales o por defecto)
              Subtotal = primerPago?.Subtotal ?? 50.0m,
              Igv = primerPago?.Igv ?? 9.0m,
              Total = primerPago?.Total ?? 59.0m,
              Estado = primerPago?.EstadoPago ?? "Pendiente"
            };
          })
          .OrderByDescending(dto => dto.FechaCita)
          .ThenByDescending(dto => dto.HoraCita)
          .ToList();

      _logger.LogInformation($"Total de DTOs de Citas generados: {citasDto.Count}");

      // Log resumen de cada DTO generado
      foreach (var dto in citasDto)
      {
        _logger.LogInformation($"DTO Cita → ID={dto.Id}, Paciente={dto.Nombres} ({dto.NumeroDoc}), FechaCita={dto.FechaCita}, FechaRegistro={dto.FechaPago}, Total={dto.Total}, Estado={dto.Estado}");
      }

      _logger.LogInformation("=== FIN GetHistorialPagosDto (Service) ===");

      return citasDto;
    }

    public Pago? GetPagoById(int id)
    {
      _logger.LogInformation($"Buscando pago con ID={id}");
      return _pagoRepository.GetPagoById(id);
    }

    public PagoResponseDTO? GetPagoByIdDto(int id)
    {
      var pago = GetPagoById(id);
      if (pago == null) return null;

      var cita = pago.Cita;

      return new PagoResponseDTO
      {
        Id = pago.IdPago,
        FechaPago = pago.FechaPago.ToString("dd/MM/yyyy"),
        FechaCita = cita?.FechaCita.ToString("dd/MM/yyyy") ?? string.Empty,
        HoraCita = cita?.HoraCita.ToString(@"hh\:mm") ?? string.Empty,
        TipoDoc = cita?.Paciente?.TipoDocumento ?? string.Empty,
        NumeroDoc = cita?.Paciente?.NumeroDocumento ?? string.Empty,
        Nombres = cita?.Paciente != null
              ? string.Join(" ", new[] { cita.Paciente.ApellidoPaterno, cita.Paciente.ApellidoMaterno, cita.Paciente.Nombre }.Where(s => !string.IsNullOrWhiteSpace(s)))
              : string.Empty,
        Medico = cita?.Medico != null
              ? string.Join(" ", new[] { cita.Medico.ApellidoPaterno, cita.Medico.ApellidoMaterno, cita.Medico.Nombre }.Where(s => !string.IsNullOrWhiteSpace(s)))
              : string.Empty,
        Especialidad = cita?.Especialidad ?? string.Empty,
        Consultorio = cita?.Consultorio ?? string.Empty,
        Total = pago.Total,
        Estado = pago.EstadoPago ?? "Pendiente"
      };
    }

    public bool UpdatePagoStatus(int id, string estado, DateTime? fechaPago = null)
    {
      return _pagoRepository.UpdatePagoStatus(id, estado, fechaPago);
    }
    public bool Pagar(int idPago)
    {
      // Lógica de pago, por ejemplo marcar como Pagado
      return UpdatePagoStatus(idPago, "Pagado", DateTime.Now);
    }
    public PagoResponseDTO? GetResumenByCitaId(int idCita)
    {
      _logger.LogInformation($"=== INICIO GetResumenByCitaId (ID Cita={idCita}) ===");

      var cita = _pagoRepository.GetCitaById(idCita);

      if (cita == null)
      {
        _logger.LogWarning($"No se encontró cita con ID: {idCita}");
        return null;
      }

      _logger.LogInformation($"Cita encontrada: ID={cita.IdCita}, Fecha={cita.FechaCita:dd/MM/yyyy}, Paciente={cita.Paciente?.NumeroDocumento}");
      _logger.LogInformation($"  - Cantidad de pagos en la cita: {cita.Pagos?.Count ?? 0}");

      // Log detallado de todos los pagos
      if (cita.Pagos != null && cita.Pagos.Any())
      {
        foreach (var p in cita.Pagos)
        {
          _logger.LogInformation($"    Pago ID={p.IdPago}, Codigo={p.CodigoServicio ?? "NULL"}, Desc={p.DescripcionServicio ?? "NULL"}, Estado={p.EstadoPago}, Total={p.Total}");
        }
      }

      var primerPago = cita.Pagos?.FirstOrDefault();

      if (primerPago == null)
      {
        _logger.LogWarning($"⚠️ La cita {idCita} NO tiene pagos asociados - Se mostrarán valores por defecto");
      }
      else
      {
        _logger.LogInformation($"✅ Pago encontrado: ID={primerPago.IdPago}, Estado={primerPago.EstadoPago}, Total={primerPago.Total}");
        _logger.LogInformation($"   - CodigoServicio: '{primerPago.CodigoServicio ?? "NULL"}'");
        _logger.LogInformation($"   - DescripcionServicio: '{primerPago.DescripcionServicio ?? "NULL"}'");
        _logger.LogInformation($"   - Cantidad: {primerPago.Cantidad}");
        _logger.LogInformation($"   - PrecioUnitario: {primerPago.PrecioUnitario}");
      }

      var dto = new PagoResponseDTO
      {
        Id = cita.IdCita,
        FechaPago = cita.FechaRegistro.ToString("dd/MM/yyyy"),
        FechaCita = cita.FechaCita.ToString("dd/MM/yyyy"),
        HoraCita = cita.HoraCita.ToString(@"hh\:mm"),

        // Datos del Paciente
        TipoDoc = cita.Paciente?.TipoDocumento ?? string.Empty,
        NumeroDoc = cita.Paciente?.NumeroDocumento ?? string.Empty,
        Nombres = cita.Paciente != null
              ? string.Join(" ", new[] {
                cita.Paciente.ApellidoPaterno,
                cita.Paciente.ApellidoMaterno,
                cita.Paciente.Nombre
              }.Where(s => !string.IsNullOrWhiteSpace(s)))
              : string.Empty,
        Edad = cita.Paciente?.Edad ?? 0,
        Telefono = string.Empty,
        HistoriaClinicaCodigo = cita.Paciente?.HistoriasClinicas?.FirstOrDefault()?.CodigoHistoria ?? string.Empty,
        TipoSeguro = cita.Paciente?.HistoriasClinicas?.FirstOrDefault()?.TipoSeguro ?? string.Empty,

        // Datos del Médico y Cita
        Medico = cita.Medico != null
              ? string.Join(" ", new[] {
                cita.Medico.ApellidoPaterno,
                cita.Medico.ApellidoMaterno,
                cita.Medico.Nombre
              }.Where(s => !string.IsNullOrWhiteSpace(s)))
              : string.Empty,
        Especialidad = cita.Especialidad ?? string.Empty,
        Consultorio = cita.Consultorio ?? string.Empty,
        Motivo = string.Empty,

        // SIEMPRE mostrar servicios (con datos reales o por defecto)
        Servicios = new List<PagoServicioDTO>
        {
            new PagoServicioDTO
            {
                Codigo = primerPago?.CodigoServicio ?? "SERV-001",
                Descripcion = primerPago?.DescripcionServicio ?? "Consulta Medicina General",
                Cantidad = primerPago?.Cantidad ?? 1,
                PrecioUnitario = primerPago?.PrecioUnitario ?? 50.0m,
                Total = primerPago != null
                    ? (primerPago.Cantidad * primerPago.PrecioUnitario)
                    : 50.0m
            }
        },

        // Montos (con valores reales o por defecto)
        Subtotal = primerPago?.Subtotal ?? 50.0m,
        Igv = primerPago?.Igv ?? 9.0m,
        Total = primerPago?.Total ?? 59.0m,
        Estado = primerPago?.EstadoPago ?? "Pendiente"
      };

      _logger.LogInformation($"DTO generado: Paciente={dto.Nombres}, Servicios={dto.Servicios.Count}, Total={dto.Total}, Estado={dto.Estado}");

      // Log de servicios generados
      foreach (var servicio in dto.Servicios)
      {
        _logger.LogInformation($"  Servicio: Codigo={servicio.Codigo}, Desc={servicio.Descripcion}, Cant={servicio.Cantidad}, PrecioUnit={servicio.PrecioUnitario}, Total={servicio.Total}");
      }

      _logger.LogInformation("=== FIN GetResumenByCitaId ===");

      return dto;
    }

  }
}