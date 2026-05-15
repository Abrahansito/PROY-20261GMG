using SGMG.Data;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Repository;
using Microsoft.EntityFrameworkCore;

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
                var contextoValido = await CompletarContextoAtencionAsync(dto);
                if (!contextoValido)
                    return new GenericResponse<ConsultaResponseDTO>(false, "No se encontro una cita o medico activo para registrar la consulta");

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

                if (dto.IdMedico <= 0)
                {
                    dto.IdMedico = consulta.IdMedico;
                    dto.IdCita ??= consulta.IdCita;
                }

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
                IdCita = consulta.IdCita,
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

        private async Task<bool> CompletarContextoAtencionAsync(ConsultaRequestDTO dto)
        {
            if (dto.IdMedico > 0)
                return true;

            Cita? cita = null;

            if (dto.IdCita.HasValue && dto.IdCita.Value > 0)
            {
                cita = await _context.Citas
                    .AsNoTracking()
                    .FirstOrDefaultAsync(c => c.IdCita == dto.IdCita.Value);
            }

            if (cita == null)
            {
                var estadosAtencion = new[] { "Triada", "Pagado", "Pendiente", "Programada", "Reservada" };

                cita = await _context.Citas
                    .AsNoTracking()
                    .Where(c => c.IdPaciente == dto.IdPaciente && estadosAtencion.Contains(c.EstadoCita))
                    .OrderByDescending(c => c.FechaCita)
                    .ThenByDescending(c => c.HoraCita)
                    .FirstOrDefaultAsync();
            }

            if (cita == null || cita.IdMedico <= 0)
                return false;

            dto.IdMedico = cita.IdMedico;
            dto.IdCita ??= cita.IdCita;
            return true;
        }
    }
}
