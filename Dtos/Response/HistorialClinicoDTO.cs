namespace SGMG.Dtos.Response
{
    public class HistorialClinicoDTO
    {
        // Información del paciente
        public int IdPaciente { get; set; }
        public string NumeroDocumento { get; set; } = "";
        public string TipoDocumento { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Sexo { get; set; } = "";
        public int Edad { get; set; }
        public string Seguro { get; set; } = "";

        // Historial de diagnósticos
        public List<DiagnosticoResponseDTO> Diagnosticos { get; set; } = new();
    }
}