using SGMG.Data;
using SGMG.Dtos.Request;
using SGMG.Dtos.Response;
using SGMG.Models;
using SGMG.Repository;
using Microsoft.EntityFrameworkCore;

namespace SGMG.Services.ServiceImpl
{
    public class LaboratorioService : ILaboratorioService
    {
        private const string ObservacionesFinalesMarker = "--- Observaciones finales del laboratorio ---";

        private readonly IOrdenLaboratorioRepository _laboratorioRepository;
        private readonly ApplicationDbContext _context;

        public LaboratorioService(
            IOrdenLaboratorioRepository laboratorioRepository,
            ApplicationDbContext context)
        {
            _laboratorioRepository = laboratorioRepository;
            _context = context;
        }

        public async Task<GenericResponse<LaboratorioHistorialDTO>> GetHistorialLaboratorioAsync(int idPaciente)
        {
            try
            {
                // Obtener información del paciente
                var paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);

                if (paciente == null)
                    return new GenericResponse<LaboratorioHistorialDTO>(false, "Paciente no encontrado");

                // Obtener historia clínica
                var historiaClinica = await _context.HistoriasClinicas
                    .FirstOrDefaultAsync(h => h.IdPaciente == idPaciente);

                // Obtener órdenes
                var ordenes = await _laboratorioRepository.GetOrdenesByPacienteAsync(idPaciente);

                // Calcular edad
                int edad = paciente.Edad;
                if (historiaClinica?.FechaNacimiento != null)
                {
                    var today = DateTime.Today;
                    edad = today.Year - historiaClinica.FechaNacimiento.Year;
                    if (historiaClinica.FechaNacimiento.Date > today.AddYears(-edad)) edad--;
                }

                // Mapear a DTO
                var historialDTO = new LaboratorioHistorialDTO
                {
                    Paciente = new PacienteInfoDTO
                    {
                        Dni = paciente.NumeroDocumento,
                        NombreCompleto = $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}".Trim(),
                        Sexo = paciente.Sexo == "M" ? "Masculino" : "Femenino",
                        Edad = edad,
                        Seguro = historiaClinica?.TipoSeguro ?? "No registrado"
                    },
                    Ordenes = ordenes.Select(o => new OrdenLaboratorioResponseDTO
                    {
                        IdOrden = o.IdOrden,
                        NumeroOrden = o.NumeroOrden,
                        TipoExamen = o.TipoExamen,
                        NombreCompletoPaciente = $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}".Trim(),
                        FechaSolicitud = o.FechaSolicitud,
                        ObservacionesAdicionales = ObtenerObservacionesOrden(o.ObservacionesAdicionales),
                        ObservacionesFinales = ObtenerObservacionesFinales(o.ObservacionesAdicionales),
                        Resultados = o.Resultados,
                        Estado = o.Estado,
                        FechaResultado = o.FechaResultado
                    }).ToList()
                };

                return new GenericResponse<LaboratorioHistorialDTO>(
                    true,
                    historialDTO,
                    "Historial obtenido correctamente"
                );
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new GenericResponse<LaboratorioHistorialDTO>(false, $"Error: {ex.Message}");
            }
        }

        public async Task<GenericResponse<OrdenLaboratorioResponseDTO>> CrearOrdenAsync(OrdenLaboratorioRequestDTO dto)
        {
            try
            {
                var contextoValido = await CompletarContextoAtencionAsync(dto);
                if (!contextoValido)
                    return new GenericResponse<OrdenLaboratorioResponseDTO>(false, "No se encontro el medico de la atencion actual");

                var numeroOrden = await _laboratorioRepository.GenerarNumeroOrdenAsync();

                var orden = new OrdenLaboratorio
                {
                    IdPaciente = dto.IdPaciente,
                    IdMedico = dto.IdMedico,
                    NumeroOrden = numeroOrden,
                    TipoExamen = dto.TipoExamen,
                    ObservacionesAdicionales = dto.ObservacionesAdicionales,
                    Estado = "Pendiente" //Estado por defecto
                };

                await _laboratorioRepository.AddOrdenAsync(orden);

                var paciente = await _context.Pacientes.FindAsync(dto.IdPaciente);

                var responseDTO = new OrdenLaboratorioResponseDTO
                {
                    IdOrden = orden.IdOrden,
                    NumeroOrden = orden.NumeroOrden,
                    TipoExamen = orden.TipoExamen,
                    NombreCompletoPaciente = paciente != null
                        ? $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}".Trim()
                        : "",
                    FechaSolicitud = orden.FechaSolicitud,
                    ObservacionesAdicionales = ObtenerObservacionesOrden(orden.ObservacionesAdicionales),
                    ObservacionesFinales = ObtenerObservacionesFinales(orden.ObservacionesAdicionales),
                    Estado = orden.Estado
                };

                return new GenericResponse<OrdenLaboratorioResponseDTO>(
                    true,
                    responseDTO,
                    "Orden creada exitosamente"
                );
            }
            catch (Exception ex)
            {
                return new GenericResponse<OrdenLaboratorioResponseDTO>(false, $"Error: {ex.Message}");
            }
        }

        public async Task<GenericResponse<OrdenLaboratorioResponseDTO>> ActualizarOrdenAsync(OrdenLaboratorioRequestDTO dto)
        {
            try
            {
                var orden = await _laboratorioRepository.GetOrdenByIdAsync(dto.IdOrden);
                if (orden == null)
                    return new GenericResponse<OrdenLaboratorioResponseDTO>(false, "Orden no encontrada");

                orden.TipoExamen = dto.TipoExamen;
                orden.ObservacionesAdicionales = dto.ObservacionesAdicionales;
                orden.Resultados = dto.Resultados;
                orden.Estado = dto.Estado;

                if (dto.Estado == "Realizado" && !orden.FechaResultado.HasValue)
                {
                    orden.FechaResultado = DateTime.UtcNow;
                }

                await _laboratorioRepository.UpdateOrdenAsync(orden);

                var responseDTO = new OrdenLaboratorioResponseDTO
                {
                    IdOrden = orden.IdOrden,
                    IdPaciente = orden.IdPaciente,
                    NumeroOrden = orden.NumeroOrden,
                    TipoExamen = orden.TipoExamen,
                    NombreCompletoPaciente = orden.Paciente != null
                        ? $"{orden.Paciente.Nombre} {orden.Paciente.ApellidoPaterno} {orden.Paciente.ApellidoMaterno}".Trim()
                        : "",
                    DniPaciente = orden.Paciente?.NumeroDocumento ?? "",
                    FechaSolicitud = orden.FechaSolicitud,
                    ObservacionesAdicionales = ObtenerObservacionesOrden(orden.ObservacionesAdicionales),
                    ObservacionesFinales = ObtenerObservacionesFinales(orden.ObservacionesAdicionales),
                    Resultados = orden.Resultados,
                    Estado = orden.Estado,
                    FechaResultado = orden.FechaResultado
                };

                return new GenericResponse<OrdenLaboratorioResponseDTO>(
                    true,
                    responseDTO,
                    "Orden actualizada exitosamente"
                );
            }
            catch (Exception ex)
            {
                return new GenericResponse<OrdenLaboratorioResponseDTO>(false, $"Error: {ex.Message}");
            }
        }

        public async Task<GenericResponse<OrdenLaboratorioResponseDTO>> GetOrdenByIdAsync(int idOrden)
        {
            try
            {
                // Usar el método correcto del repositorio
                var orden = await _laboratorioRepository.GetOrdenByIdAsync(idOrden);

                if (orden == null)
                {
                    return new GenericResponse<OrdenLaboratorioResponseDTO>(false, "Orden no encontrada");
                }

                var ordenDTO = new OrdenLaboratorioResponseDTO
                {
                    IdOrden = orden.IdOrden,
                    IdPaciente = orden.IdPaciente,
                    NumeroOrden = orden.NumeroOrden,
                    NombreCompletoPaciente = orden.Paciente != null
                        ? $"{orden.Paciente.Nombre} {orden.Paciente.ApellidoPaterno} {orden.Paciente.ApellidoMaterno}".Trim()
                        : "No disponible",
                    DniPaciente = orden.Paciente?.NumeroDocumento ?? "N/A",
                    TipoExamen = orden.TipoExamen,
                    FechaSolicitud = orden.FechaSolicitud,
                    FechaResultado = orden.FechaResultado,
                    Estado = orden.Estado,
                    Resultados = orden.Resultados,
                    ObservacionesAdicionales = ObtenerObservacionesOrden(orden.ObservacionesAdicionales),
                    ObservacionesFinales = ObtenerObservacionesFinales(orden.ObservacionesAdicionales)
                };

                return new GenericResponse<OrdenLaboratorioResponseDTO>(true, ordenDTO, "Orden obtenida exitosamente");
            }
            catch (Exception ex)
            {
                return new GenericResponse<OrdenLaboratorioResponseDTO>(false, $"Error al obtener la orden: {ex.Message}");
            }
        }

        //MÉTODO PARA CANCELAR ORDEN
        public async Task<GenericResponse<bool>> CancelarOrdenAsync(int idOrden)
        {
            try
            {
                var orden = await _laboratorioRepository.GetOrdenByIdAsync(idOrden);

                if (orden == null)
                {
                    return new GenericResponse<bool>(false, "Orden no encontrada");
                }

                if (orden.Estado != "Pendiente")
                {
                    return new GenericResponse<bool>(false, "Solo se pueden cancelar órdenes en estado Pendiente");
                }

                orden.Estado = "Cancelado";
                await _laboratorioRepository.UpdateOrdenAsync(orden);

                return new GenericResponse<bool>(true, true, "Orden cancelada exitosamente");
            }
            catch (Exception ex)
            {
                return new GenericResponse<bool>(false, $"Error al cancelar la orden: {ex.Message}");
            }
        }

        //MÉTODO PARA ACTUALIZAR RESULTADOS
        public async Task<GenericResponse<bool>> ActualizarResultadosAsync(ActualizarResultadosDTO request)
        {
            try
            {
                var orden = await _laboratorioRepository.GetOrdenByIdAsync(request.IdOrden);

                if (orden == null)
                {
                    return new GenericResponse<bool>(false, "Orden no encontrada");
                }

                if (orden.Estado != "Pendiente")
                {
                    return new GenericResponse<bool>(false, "Solo se pueden actualizar resultados de órdenes en estado Pendiente");
                }

                // Actualizar los resultados
                orden.Resultados = request.Resultados;
                orden.FechaResultado = request.FechaResultado;
                orden.Estado = "Realizado";

                var observacionesOrden = ObtenerObservacionesOrden(orden.ObservacionesAdicionales);
                orden.ObservacionesAdicionales = string.IsNullOrWhiteSpace(request.ObservacionesFinales)
                    ? observacionesOrden
                    : $"{observacionesOrden}\n\n{ObservacionesFinalesMarker}\n{request.ObservacionesFinales.Trim()}";

                await _laboratorioRepository.UpdateOrdenAsync(orden);

                return new GenericResponse<bool>(true, true, "Resultados actualizados exitosamente");
            }
            catch (Exception ex)
            {
                return new GenericResponse<bool>(false, $"Error al actualizar resultados: {ex.Message}");
            }
        }

        private async Task<bool> CompletarContextoAtencionAsync(OrdenLaboratorioRequestDTO dto)
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
                    .ThenByDescending(c => c.IdCita)
                    .FirstOrDefaultAsync();
            }

            if (cita == null || cita.IdMedico <= 0)
                return false;

            dto.IdMedico = cita.IdMedico;
            dto.IdCita ??= cita.IdCita;
            return true;
        }

        private static string ObtenerObservacionesOrden(string? observaciones)
        {
            if (string.IsNullOrWhiteSpace(observaciones))
                return "";

            var markerIndex = observaciones.IndexOf(
                ObservacionesFinalesMarker,
                StringComparison.OrdinalIgnoreCase);

            return markerIndex >= 0
                ? observaciones[..markerIndex].Trim()
                : observaciones.Trim();
        }

        private static string ObtenerObservacionesFinales(string? observaciones)
        {
            if (string.IsNullOrWhiteSpace(observaciones))
                return "";

            var markerIndex = observaciones.IndexOf(
                ObservacionesFinalesMarker,
                StringComparison.OrdinalIgnoreCase);

            if (markerIndex < 0)
                return "";

            return observaciones[(markerIndex + ObservacionesFinalesMarker.Length)..].Trim();
        }
    }
}


       

