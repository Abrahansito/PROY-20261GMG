namespace SGMG.Dtos.Response
{
    public class TriajeResponseDTO
    {
        // Datos del triaje
        public int IdTriaje { get; set; }
        public decimal Temperatura { get; set; }
        public int PresionArterial { get; set; }
        public int Saturacion { get; set; }
        public int FrecuenciaCardiaca { get; set; }
        public int FrecuenciaRespiratoria { get; set; }
        public decimal Peso { get; set; }
        public decimal Talla { get; set; }
        public decimal PerimetroAbdominal { get; set; }
        public decimal SuperficieCorporal { get; set; }
        public decimal Imc { get; set; }
        public string ClasificacionImc { get; set; } = "";
        public string RiesgoEnfermedad { get; set; } = "";
        public string EstadoTriage { get; set; } = "";
        public DateTime FechaTriage { get; set; }
        public TimeSpan HoraTriage { get; set; }
        public string Observaciones { get; set; } = "";

        // Datos del paciente
        public int IdPaciente { get; set; }
        public string NumeroDocumento { get; set; } = "";
        public string TipoDocumento { get; set; } = "";
        public string NombreCompletoPaciente { get; set; } = ""; //Para listado

        // Datos de la cita (para listado)
        public string Consultorio { get; set; } = "";
        public string HoraCita { get; set; } = ""; 
        public DateTime? FechaCita { get; set; }
        public string NombreCompletoMedico { get; set; } = "";

        //Datos individuales del paciente para edición
        public string Nombre { get; set; } = "";
        public string ApellidoPaterno { get; set; } = "";
        public string ApellidoMaterno { get; set; } = "";
        public string Sexo { get; set; } = "";
        public int Edad { get; set; }
    }
}
