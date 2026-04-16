using System;

namespace SGMG.Dtos.Response
{
    public class CitaResponseDTO
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public int IdMedico { get; set; }
        public string Especialidad { get; set; } = "";
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraCita { get; set; }
        public string Consultorio { get; set; } = "";
        public string EstadoCita { get; set; } = "";
        
        // Datos del paciente (solo lo necesario)
        public string TipoDocumento { get; set; } = "";
        public string NumeroDocumento { get; set; } = "";
        public string NombreCompletoPaciente { get; set; } = "";
        
        // Datos del médico (solo lo necesario)
        public string NombreCompletoMedico { get; set; } = "";
    }
}