using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Dtos.Request.Triaje;

namespace SGMG.Services
{
  public interface ITriajeService
  {
    Task<GenericResponse<Triaje>> AddTriajeAsync(TriajeRequestDTO triajeRequestDTO);
    Task<GenericResponse<TriajeResponseDTO>> GetTriajeByIdAsync(int id);
    Task<GenericResponse<TriajeResponseDTO>> UpdateTriajeAsync(TriajeRequestDTO triajeRequestDTO);
    Task<GenericResponse<IEnumerable<TriajeResponseDTO>>> GetAllTriajesAsync(); // Método para obtener todos los triajes
    Task<GenericResponse<IEnumerable<TriajeResponseDTO>>> BuscarTriajesAsync(string? tipoDoc, string? numeroDoc, DateTime? fechaInicio, DateTime? fechaFin);
    Task<GenericResponse<IEnumerable<CitaResponseDTO>>> GetCitasPagadasPorConsultorioEnfermeraAsync(int idEnfermera);
    Task<GenericResponse<HistorialTriajeDTO>> GetHistorialTriajePacienteAsync(int idPaciente);

    

  }
}