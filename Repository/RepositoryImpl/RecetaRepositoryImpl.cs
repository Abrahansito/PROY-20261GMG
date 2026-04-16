using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using SGMG.Data;
using SGMG.Models;
using PROY_20252SGMG.Repository;
using System.Collections.Generic;
using System.Threading.Tasks;
using System;
using PROY_20252SGMG.Dtos.Response;


namespace SGMG.Repository.RepositoryImpl
{
  public class RecetaRepositoryImpl : IRecetaRepository
  {
    private readonly ApplicationDbContext _context;
    private readonly ILogger<RecetaRepositoryImpl> _logger;

    public RecetaRepositoryImpl(ApplicationDbContext context, ILogger<RecetaRepositoryImpl> logger)
    {
      _context = context;
      _logger = logger;
    }

    // ===== NUEVOS MÉTODOS PARA ESTADOS =====

    public async Task<Receta?> GetRecetaPendienteByCitaAsync(int idCita, int idMedico, int idPaciente)
    {
      try
      {
        return await _context.Recetas
            .Include(r => r.Detalles)
            .Include(r => r.Paciente)
            .Include(r => r.Medico)
            .FirstOrDefaultAsync(r =>
                r.IdCita == idCita &&
                r.IdMedico == idMedico &&
                r.IdPaciente == idPaciente &&
                r.EstadoReceta == "PENDIENTE");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener receta pendiente: {ex.Message}");
        throw;
      }
    }

    public async Task<DetalleReceta> AgregarDetalleAsync(DetalleReceta detalle)
    {
      try
      {
        await _context.DetallesRecetas.AddAsync(detalle);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Detalle agregado con ID: {detalle.IdDetalle}");

        return detalle;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al agregar detalle: {ex.Message}");
        throw;
      }
    }

    public async Task<bool> EliminarDetalleAsync(int idDetalle)
    {
      try
      {
        var detalle = await _context.DetallesRecetas.FindAsync(idDetalle);
        if (detalle == null) return false;

        _context.DetallesRecetas.Remove(detalle);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Detalle {idDetalle} eliminado");

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al eliminar detalle: {ex.Message}");
        throw;
      }
    }

    public async Task<bool> CompletarRecetaAsync(Receta receta)
    {
      try
      {
        // ✅ Cambiar el estado de la receta
        receta.EstadoReceta = "COMPLETADO";
        receta.FechaEmision = DateTime.Now;

        // ✅ Cambiar el estado de cada detalle a COMPLETADO
        var detalles = await _context.DetallesRecetas
            .Where(d => d.IdReceta == receta.IdReceta)
            .ToListAsync();

        foreach (var detalle in detalles)
        {
          detalle.Estado = "COMPLETADO";
        }

        // Actualizamos todo
        _context.Recetas.Update(receta);
        _context.DetallesRecetas.UpdateRange(detalles);

        await _context.SaveChangesAsync();

        _logger.LogInformation($"Receta {receta.IdReceta} y sus detalles fueron completados correctamente");
        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al completar receta: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Obtener recetas por paciente con ordenamiento por fecha descendente
    /// NOTA: Ya filtras por EstadoReceta != "PENDIENTE"
    /// </summary>
    public async Task<IEnumerable<Receta>> GetRecetasByPacienteAsync(int idPaciente)
    {
      try
      {
        return await _context.Recetas
            .Include(r => r.Paciente)
            .Include(r => r.Medico)
            .Include(r => r.Detalles)  // ✅ Importante para el conteo
            .Include(r => r.Cita)      // ✅ Por si necesitas info de la cita
            .Where(r => r.IdPaciente == idPaciente && r.EstadoReceta != "PENDIENTE")
            .OrderByDescending(r => r.FechaEmision)
            .ToListAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener recetas por paciente: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Si necesitas obtener recetas con paginación (opcional)
    /// </summary>
    public async Task<(IEnumerable<Receta> Recetas, int Total)> GetRecetasByPacientePaginadoAsync(
        int idPaciente, int pagina = 1, int porPagina = 10)
    {
      try
      {
        var query = _context.Recetas
            .Include(r => r.Paciente)
            .Include(r => r.Medico)
            .Include(r => r.Detalles)
            .Where(r => r.IdPaciente == idPaciente && r.EstadoReceta != "PENDIENTE");

        var total = await query.CountAsync();

        var recetas = await query
            .OrderByDescending(r => r.FechaEmision)
            .Skip((pagina - 1) * porPagina)
            .Take(porPagina)
            .ToListAsync();

        return (recetas, total);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener recetas paginadas: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Verificar si ya existe una receta COMPLETADA para una cita específica
    /// </summary>
    public async Task<bool> ExistsRecetaCompletadaByCitaAsync(int idCita)
    {
      try
      {
        return await _context.Recetas
            .AnyAsync(r => r.IdCita == idCita && r.EstadoReceta == "COMPLETADO");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al verificar receta completada: {ex.Message}");
        throw;
      }
    }

    /// <summary>
    /// Obtener estadísticas de recetas de un paciente
    /// </summary>
    public async Task<RecetaEstadisticas> GetEstadisticasRecetasPacienteAsync(int idPaciente)
    {
      try
      {
        var recetas = await _context.Recetas
            .Include(r => r.Detalles)
            .Where(r => r.IdPaciente == idPaciente && r.EstadoReceta != "PENDIENTE")
            .ToListAsync();

        return new RecetaEstadisticas
        {
          TotalRecetas = recetas.Count,
          TotalMedicamentos = recetas.SelectMany(r => r.Detalles).Count(),
          RecetasImpresas = recetas.Count(r => r.EstadoReceta == "Impresa"),
          UltimaReceta = recetas.OrderByDescending(r => r.FechaEmision).FirstOrDefault()?.FechaEmision
        };
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener estadísticas: {ex.Message}");
        throw;
      }
    }
    // ===== MÉTODOS ORIGINALES =====

    public async Task<Receta> CreateRecetaAsync(Receta receta)
    {
      try
      {
        await _context.Recetas.AddAsync(receta);
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Receta creada con ID: {receta.IdReceta}");

        return receta;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al crear receta: {ex.Message}");
        throw;
      }
    }

    public async Task<Receta?> GetRecetaByIdAsync(int id)
    {
      try
      {
        return await _context.Recetas
            .Include(r => r.Paciente)
            .Include(r => r.Medico)
            .Include(r => r.Detalles)
            .Include(r => r.Cita)
            .FirstOrDefaultAsync(r => r.IdReceta == id);
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener receta por ID: {ex.Message}");
        throw;
      }
    }

    public async Task<IEnumerable<Receta>> GetRecetasByPacienteAsyncCompleto(int idPaciente)
    {
      try
      {
        return await _context.Recetas
            .Include(r => r.Paciente)
            .Include(r => r.Medico)
            .Include(r => r.Detalles)
            .Where(r => r.IdPaciente == idPaciente && r.EstadoReceta != "COMPLETADO")
            .OrderByDescending(r => r.FechaEmision)
            .ToListAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener recetas por paciente: {ex.Message}");
        throw;
      }
    }

    public async Task<IEnumerable<Receta>> GetRecetasByMedicoAsync(int idMedico)
    {
      try
      {
        return await _context.Recetas
            .Include(r => r.Paciente)
            .Include(r => r.Medico)
            .Include(r => r.Detalles)
            .Where(r => r.IdMedico == idMedico && r.EstadoReceta != "PENDIENTE")
            .OrderByDescending(r => r.FechaEmision)
            .ToListAsync();
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener recetas por médico: {ex.Message}");
        throw;
      }
    }

    public async Task<Receta?> GetRecetaByCitaAsync(int idCita)
    {
      try
      {
        return await _context.Recetas
            .Include(r => r.Paciente)
            .Include(r => r.Medico)
            .Include(r => r.Detalles)
            .FirstOrDefaultAsync(r => r.IdCita == idCita && r.EstadoReceta != "PENDIENTE");
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al obtener receta por cita: {ex.Message}");
        throw;
      }
    }

    public async Task<bool> UpdateEstadoRecetaAsync(int id, string estado)
    {
      try
      {
        var receta = await _context.Recetas.FindAsync(id);
        if (receta == null) return false;

        receta.EstadoReceta = estado;
        await _context.SaveChangesAsync();

        _logger.LogInformation($"Estado de receta {id} actualizado a: {estado}");

        return true;
      }
      catch (Exception ex)
      {
        _logger.LogError($"Error al actualizar estado de receta: {ex.Message}");
        throw;
      }
    }
  }
}