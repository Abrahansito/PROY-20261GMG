namespace SGMG.Dtos.Request
{
    public class ConsultaRequestDTO
    {
        public int IdConsulta { get; set; }
        public int IdPaciente { get; set; }
        public int IdMedico { get; set; }
        public int? IdCita { get; set; }

        // Síntomas
        public string MotivoConsulta { get; set; } = "";
        public string SintomasPresentados { get; set; } = "";

        // Diagnóstico
        public string DiagnosticoPrincipal { get; set; } = "";
        public string CodigoCie10 { get; set; } = "";
        public string Observaciones { get; set; } = "";

        // Evolución
        public string DescripcionEvolucion { get; set; } = "";
        public string IndicacionesRecomendaciones { get; set; } = "";
    }
}