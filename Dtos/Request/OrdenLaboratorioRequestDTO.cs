namespace SGMG.Dtos.Request
{
    public class OrdenLaboratorioRequestDTO
    {
        public int IdOrden { get; set; }
        public int IdPaciente { get; set; }
        public int IdMedico { get; set; }
        public string TipoExamen { get; set; } = "";
        public string ObservacionesAdicionales { get; set; } = "";
        public string Resultados { get; set; } = "";
        public string Estado { get; set; } = "Pendiente";
    }
}