using SGMG.Data;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Repository;

namespace SGMG.Services.ServiceImpl
{
    public class ConsultaService : IConsultaService
    {
        private readonly IConsultaRepository _consultaRepository;
        private readonly ApplicationDbContext _context;

        public ConsultaService(IConsultaRepository consultaRepository, ApplicationDbContext context)
        {
            _consultaRepository = consultaRepository;
            _context = context;
        }

        public async Task<GenericResponse<ConsultaResponseDTO>> AddConsultaAsync(ConsultaRequestDTO dto)
        {
            try
            {
                var consulta = new Consulta
                {
                    IdPaciente = dto.IdPaciente,
                    IdMedico = dto.IdMedico,
                    IdCita = dto.IdCita,
                    MotivoConsulta = dto.MotivoConsulta,
                    SintomasPresentados = dto.SintomasPresentados,
                    DiagnosticoPrincipal = dto.DiagnosticoPrincipal,
                    CodigoCie10 = dto.CodigoCie10,
                    Observaciones = dto.Observaciones,
                    DescripcionEvolucion = dto.DescripcionEvolucion,
                    IndicacionesRecomendaciones = dto.IndicacionesRecomendaciones
                };

                await _consultaRepository.AddConsultaAsync(consulta);

                var consultaCreada = await _consultaRepository.GetConsultaByIdAsync(consulta.IdConsulta);

                var responseDTO = MapToDTO(consultaCreada!);

                return new GenericResponse<ConsultaResponseDTO>(true, responseDTO, "Consulta registrada exitosamente");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new GenericResponse<ConsultaResponseDTO>(false, $"Error: {ex.Message}");
            }
        }

        public async Task<GenericResponse<ConsultaResponseDTO>> UpdateConsultaAsync(ConsultaRequestDTO dto)
        {
            try
            {
                var consulta = await _consultaRepository.GetConsultaByIdAsync(dto.IdConsulta);
                if (consulta == null)
                    return new GenericResponse<ConsultaResponseDTO>(false, "Consulta no encontrada");

                consulta.MotivoConsulta = dto.MotivoConsulta;
                consulta.SintomasPresentados = dto.SintomasPresentados;
                consulta.DiagnosticoPrincipal = dto.DiagnosticoPrincipal;
                consulta.CodigoCie10 = dto.CodigoCie10;
                consulta.Observaciones = dto.Observaciones;
                consulta.DescripcionEvolucion = dto.DescripcionEvolucion;
                consulta.IndicacionesRecomendaciones = dto.IndicacionesRecomendaciones;

                await _consultaRepository.UpdateConsultaAsync(consulta);

                var responseDTO = MapToDTO(consulta);

                return new GenericResponse<ConsultaResponseDTO>(true, responseDTO, "Consulta actualizada exitosamente");
            }
            catch (Exception ex)
            {
                return new GenericResponse<ConsultaResponseDTO>(false, $"Error: {ex.Message}");
            }
        }

        public async Task<GenericResponse<ConsultaResponseDTO>> GetConsultaByIdAsync(int id)
        {
            try
            {
                var consulta = await _consultaRepository.GetConsultaByIdAsync(id);
                if (consulta == null)
                    return new GenericResponse<ConsultaResponseDTO>(false, "Consulta no encontrada");

                var responseDTO = MapToDTO(consulta);

                return new GenericResponse<ConsultaResponseDTO>(true, responseDTO, "Consulta obtenida correctamente");
            }
            catch (Exception ex)
            {
                return new GenericResponse<ConsultaResponseDTO>(false, $"Error: {ex.Message}");
            }
        }

        private ConsultaResponseDTO MapToDTO(Consulta consulta)
        {
            return new ConsultaResponseDTO
            {
                IdConsulta = consulta.IdConsulta,
                IdPaciente = consulta.IdPaciente,
                IdMedico = consulta.IdMedico,
                MotivoConsulta = consulta.MotivoConsulta,
                SintomasPresentados = consulta.SintomasPresentados,
                DiagnosticoPrincipal = consulta.DiagnosticoPrincipal,
                CodigoCie10 = consulta.CodigoCie10,
                Observaciones = consulta.Observaciones,
                DescripcionEvolucion = consulta.DescripcionEvolucion,
                IndicacionesRecomendaciones = consulta.IndicacionesRecomendaciones,
                FechaConsulta = consulta.FechaConsulta,
                HoraConsulta = consulta.HoraConsulta,
                NombreCompletoMedico = consulta.Medico != null
                    ? $"Dr. {consulta.Medico.Nombre} {consulta.Medico.ApellidoPaterno}".Trim()
                    : ""
            };
        }
    }
}