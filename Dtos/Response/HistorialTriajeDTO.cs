namespace SGMG.Dtos.Response
{
    public class HistorialTriajeDTO
    {
        public PacienteInfoDTO Paciente { get; set; } = new();
        public List<TriajeDetalladoDTO> Triajes { get; set; } = new();
    }

    public class PacienteInfoDTO
    {
        public string Dni { get; set; } = "";
        public string NombreCompleto { get; set; } = "";
        public string Sexo { get; set; } = "";
        public int Edad { get; set; }
        public string Seguro { get; set; } = "";
    }

    public class TriajeDetalladoDTO
    {
        public string CodigoTriaje { get; set; } = "";
        public string FechaHora { get; set; } = "";
        public string PresionArterial { get; set; } = "";
        public string Temperatura { get; set; } = "";
        public SignosVitalesDTO Signos { get; set; } = new();
        public MedidasAntropometricasDTO Medidas { get; set; } = new();
        public InformacionTriajeDTO Informacion { get; set; } = new();
        public string Observaciones { get; set; } = "";
    }

    public class SignosVitalesDTO
    {
        public decimal Temperatura { get; set; }
        public int PresionArterial { get; set; }
        public int FrecuenciaCardiaca { get; set; }
        public int Saturacion { get; set; }
    }

    public class MedidasAntropometricasDTO
    {
        public decimal Peso { get; set; }
        public decimal Talla { get; set; }
        public decimal Imc { get; set; }
        public string ImcClasificacion { get; set; } = "";
    }

    public class InformacionTriajeDTO
    {
        public string Fecha { get; set; } = "";
        public string Hora { get; set; } = "";
        public string RealizadoPor { get; set; } = "";
    }
}