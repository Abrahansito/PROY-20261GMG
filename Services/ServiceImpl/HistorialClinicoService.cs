using SGMG.Data;
using SGMG.Dtos.Response;
using SGMG.Repository;
using Microsoft.EntityFrameworkCore;

namespace SGMG.Services.ServiceImpl
{
    public class HistorialClinicoService : IHistorialClinicoService
    {
        private readonly IDiagnosticoRepository _diagnosticoRepository;
        private readonly IHistoriaClinicaRepository _historiaClinicaRepository;
        private readonly ApplicationDbContext _context;

        public HistorialClinicoService(
            IDiagnosticoRepository diagnosticoRepository,
            IHistoriaClinicaRepository historiaClinicaRepository,
            ApplicationDbContext context)
        {
            _diagnosticoRepository = diagnosticoRepository;
            _historiaClinicaRepository = historiaClinicaRepository;
            _context = context;
        }

        public async Task<GenericResponse<HistorialClinicoDTO>> GetHistorialByPacienteAsync(int idPaciente)
        {
            try
            {
                // 1. Obtener paciente
                var paciente = await _context.Pacientes
                    .FirstOrDefaultAsync(p => p.IdPaciente == idPaciente);

                if (paciente == null)
                    return new GenericResponse<HistorialClinicoDTO>(false, "Paciente no encontrado.");

                // 2. Obtener historia clínica (para el seguro)
                var historiaClinica = await _historiaClinicaRepository.GetHistoriaClinicaByPacienteIdAsync(idPaciente);

                // 3. Obtener diagnósticos
                var diagnosticos = await _diagnosticoRepository.GetDiagnosticosByPacienteAsync(idPaciente);

                // 4. Calcular edad
                int edad = paciente.Edad;
                if (historiaClinica?.FechaNacimiento != null)
                {
                    var today = DateTime.Today;
                    edad = today.Year - historiaClinica.FechaNacimiento.Year;
                    if (historiaClinica.FechaNacimiento.Date > today.AddYears(-edad)) edad--;
                }

                // 5. Mapear a DTO
                var historial = new HistorialClinicoDTO
                {
                    IdPaciente = paciente.IdPaciente,
                    NumeroDocumento = paciente.NumeroDocumento,
                    TipoDocumento = paciente.TipoDocumento,
                    NombreCompleto = $"{paciente.Nombre} {paciente.ApellidoPaterno} {paciente.ApellidoMaterno}".Trim(),
                    Sexo = paciente.Sexo == "M" ? "Masculino" : "Femenino",
                    Edad = edad,
                    Seguro = historiaClinica?.TipoSeguro ?? "No registrado",

                    Diagnosticos = diagnosticos.Select(d => new DiagnosticoResponseDTO
                    {
                        IdDiagnostico = d.IdDiagnostico,
                        FechaDiagnostico = d.FechaDiagnostico,
                        DiagnosticoPrincipal = $"{d.DiagnosticoPrincipal} ({d.CodigoCie10})",
                        CodigoCie10 = d.CodigoCie10,
                        NombreCompletoMedico = d.Medico != null
                            ? $"Dr. {d.Medico.Nombre} {d.Medico.ApellidoPaterno}".Trim()
                            : "N/A",
                        Consultorio = d.Cita?.Consultorio ?? "N/A",
                        ObservacionesMedicas = d.ObservacionesMedicas,
                        TratamientoEspecifico = d.TratamientoEspecifico
                    }).ToList()
                };

                return new GenericResponse<HistorialClinicoDTO>(true, historial, "Historial obtenido correctamente.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return new GenericResponse<HistorialClinicoDTO>(false, $"Error: {ex.Message}");
            }
        }

        public async Task<GenericResponse<DiagnosticoResponseDTO>> GetDiagnosticoDetalleAsync(int idDiagnostico)
        {
            try
            {
                var diagnostico = await _diagnosticoRepository.GetDiagnosticoByIdAsync(idDiagnostico);

                if (diagnostico == null)
                    return new GenericResponse<DiagnosticoResponseDTO>(false, "Diagnóstico no encontrado.");

                var dto = new DiagnosticoResponseDTO
                {
                    IdDiagnostico = diagnostico.IdDiagnostico,
                    FechaDiagnostico = diagnostico.FechaDiagnostico,
                    DiagnosticoPrincipal = $"{diagnostico.DiagnosticoPrincipal} ({diagnostico.CodigoCie10})",
                    CodigoCie10 = diagnostico.CodigoCie10,
                    NombreCompletoMedico = diagnostico.Medico != null
                        ? $"Dr. {diagnostico.Medico.Nombre} {diagnostico.Medico.ApellidoPaterno}".Trim()
                        : "N/A",
                    Consultorio = diagnostico.Cita?.Consultorio ?? "N/A",
                    ObservacionesMedicas = diagnostico.ObservacionesMedicas,
                    TratamientoEspecifico = diagnostico.TratamientoEspecifico
                };

                return new GenericResponse<DiagnosticoResponseDTO>(true, dto, "Diagnóstico obtenido correctamente.");
            }
            catch (Exception ex)
            {
                return new GenericResponse<DiagnosticoResponseDTO>(false, $"Error: {ex.Message}");
            }
        }
    }
}