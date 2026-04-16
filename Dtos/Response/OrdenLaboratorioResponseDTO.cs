namespace SGMG.Dtos.Response
{
    public class OrdenLaboratorioResponseDTO
    {
        public int IdOrden { get; set; }
        public int IdPaciente { get; set; }
        public string NumeroOrden { get; set; } = "";
        public string TipoExamen { get; set; } = "";
        public string NombreCompletoPaciente { get; set; } = "";
        public string DniPaciente { get; set; } = "";
        public DateTime FechaSolicitud { get; set; }
        public string ObservacionesAdicionales { get; set; } = "";
        public string Resultados { get; set; } = "";
        public string Estado { get; set; } = "";
        public DateTime? FechaResultado { get; set; }
    }
}