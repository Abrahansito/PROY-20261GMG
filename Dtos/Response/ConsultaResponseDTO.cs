namespace SGMG.Dtos.Response
{
    public class ConsultaResponseDTO
    {
        public int IdConsulta { get; set; }
        public int IdPaciente { get; set; }
        public int IdMedico { get; set; }

        public string MotivoConsulta { get; set; } = "";
        public string SintomasPresentados { get; set; } = "";
        public string DiagnosticoPrincipal { get; set; } = "";
        public string CodigoCie10 { get; set; } = "";
        public string Observaciones { get; set; } = "";
        public string DescripcionEvolucion { get; set; } = "";
        public string IndicacionesRecomendaciones { get; set; } = "";

        public DateTime FechaConsulta { get; set; }
        public TimeSpan HoraConsulta { get; set; }

        public string NombreCompletoMedico { get; set; } = "";
    }
}