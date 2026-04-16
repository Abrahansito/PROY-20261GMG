namespace SGMG.Dtos.Response
{
    public class DiagnosticoResponseDTO
    {
        public int IdDiagnostico { get; set; }
        public DateTime FechaDiagnostico { get; set; }
        public string DiagnosticoPrincipal { get; set; } = "";
        public string CodigoCie10 { get; set; } = "";
        public string NombreCompletoMedico { get; set; } = "";
        public string Consultorio { get; set; } = "";
        public string ObservacionesMedicas { get; set; } = "";
        public string TratamientoEspecifico { get; set; } = "";
    }
}