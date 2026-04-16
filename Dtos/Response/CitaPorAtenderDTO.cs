// Dtos/Response/CitaPorAtenderDTO.cs
namespace SGMG.Dtos.Response
{
    public class CitaPorAtenderDTO
    {
        public int IdCita { get; set; }
        public int IdPaciente { get; set; }
        public string NumeroDocumento { get; set; } = "";
        public string TipoDocumento { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Sexo { get; set; } = "";
        public int Edad { get; set; }
        public DateTime FechaCita { get; set; }
        public TimeSpan HoraCita { get; set; }
        public string Consultorio { get; set; } = "";
        public string EstadoCita { get; set; } = "";
        public DateTime FechaRegistro { get; set; }
        public string Especialidad { get; set; } = "";
    }
}