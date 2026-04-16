namespace SGMG.Dtos.Request
{
    public class ActualizarResultadosDTO
    {
        public int IdOrden { get; set; }
        public string Resultados { get; set; } = string.Empty;
        public DateTime FechaResultado { get; set; }
        public string? ObservacionesFinales { get; set; }
        public string Estado { get; set; } = "Realizado";
    }
}