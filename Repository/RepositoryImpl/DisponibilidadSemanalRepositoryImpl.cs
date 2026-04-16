using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGMG.Data;
using SGMG.Models;
using SGMG.Repository;

namespace SGMG.Repository.RepositoryImpl
{
  public class DisponibilidadSemanalRepositoryImpl : IDisponibilidadSemanalRepository
  {

    private readonly ApplicationDbContext _context;
    private readonly ILogger<DisponibilidadSemanalRepositoryImpl> _logger;

    public DisponibilidadSemanalRepositoryImpl(
        ApplicationDbContext context,
        ILogger<DisponibilidadSemanalRepositoryImpl> logger)
    {
      _context = context;
      _logger = logger;
    }

    public async Task<IEnumerable<DisponibilidadSemanal>> GetDisponibilidadesPorSemanaAsync(
        DateTime fechaInicio,
        DateTime fechaFin)
    {
      try
      {
        _logger.LogInformation(
            "Obteniendo disponibilidades para la semana del {FechaInicio} al {FechaFin}",
            fechaInicio.ToString("dd/MM/yyyy"),
            fechaFin.ToString("dd/MM/yyyy"));

        var disponibilidades = await _context.DisponibilidadesSemanales
            .Include(d => d.Medico)
            .ThenInclude(m => m.ConsultorioAsignado)
            .Where(d => d.FechaInicioSemana <= fechaFin && d.FechaFinSemana >= fechaInicio)
            .ToListAsync();

        _logger.LogInformation(
            "Se encontraron {Count} registros de disponibilidad",
            disponibilidades.Count);

        return disponibilidades;
      }
      catch (Exception ex)
      {
        _logger.LogError(
            ex,
            "Error al obtener disponibilidades para la semana del {FechaInicio} al {FechaFin}",
            fechaInicio,
            fechaFin);
        throw;
      }
    }

    public async Task<DisponibilidadSemanal?> GetDisponibilidadByMedicoYSemanaAsync(
        int idMedico,
        DateTime fechaInicio,
        DateTime fechaFin)
    {
      try
      {
        _logger.LogInformation(
            "Obteniendo disponibilidad del médico {IdMedico} para la semana del {FechaInicio} al {FechaFin}",
            idMedico,
            fechaInicio.ToString("dd/MM/yyyy"),
            fechaFin.ToString("dd/MM/yyyy"));

        var disponibilidad = await _context.DisponibilidadesSemanales
            .FirstOrDefaultAsync(d =>
                d.IdMedico == idMedico &&
                d.FechaInicioSemana == fechaInicio &&
                d.FechaFinSemana == fechaFin);

        if (disponibilidad != null)
        {
          _logger.LogInformation(
              "Disponibilidad encontrada: {CitasActuales}/{CitasMaximas} citas",
              disponibilidad.CitasActuales,
              disponibilidad.CitasMaximas);
        }
        else
        {
          _logger.LogInformation(
              "No se encontró registro de disponibilidad para el médico {IdMedico}",
              idMedico);
        }

        return disponibilidad;
      }
      catch (Exception ex)
      {
        _logger.LogError(
            ex,
            "Error al obtener disponibilidad del médico {IdMedico}",
            idMedico);
        throw;
      }
    }
  }
}