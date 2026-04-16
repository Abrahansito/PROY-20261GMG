using SGMG.Models;

namespace PROY_20252SGMG.Repository
{
  public interface IRecetaRepository
  {
    // ===== NUEVOS MÉTODOS PARA ESTADOS =====

    /// <summary>
    /// Obtiene receta PENDIENTE por cita, médico y paciente
    /// </summary>
    Task<Receta?> GetRecetaPendienteByCitaAsync(int idCita, int idMedico, int idPaciente);

    /// <summary>
    /// Agrega un detalle a una receta existente
    /// </summary>
    Task<DetalleReceta> AgregarDetalleAsync(DetalleReceta detalle);

    /// <summary>
    /// Elimina un detalle por ID
    /// </summary>
    Task<bool> EliminarDetalleAsync(int idDetalle);

    /// <summary>
    /// Completa una receta cambiando su estado a COMPLETADO
    /// </summary>
    Task<bool> CompletarRecetaAsync(Receta receta);

    // ===== MÉTODOS ORIGINALES =====

    Task<Receta> CreateRecetaAsync(Receta receta);
    Task<Receta?> GetRecetaByIdAsync(int id);
    Task<IEnumerable<Receta>> GetRecetasByPacienteAsync(int idPaciente);
    Task<IEnumerable<Receta>> GetRecetasByMedicoAsync(int idMedico);
    Task<Receta?> GetRecetaByCitaAsync(int idCita);
    Task<bool> UpdateEstadoRecetaAsync(int id, string estado);
    Task<IEnumerable<Receta>> GetRecetasByPacienteAsyncCompleto(int idPaciente);
  }
}