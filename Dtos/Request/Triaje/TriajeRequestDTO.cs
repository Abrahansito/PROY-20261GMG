using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGMG.Dtos.Request.Triaje
{
    public class TriajeRequestDTO
    {
        public int IdTriaje { get; set; }
        public int IdPaciente { get; set; }
        public decimal Temperatura { get; set; }
        public int PresionArterial { get; set; }
        public int Saturacion { get; set; }
        public int FrecuenciaCardiaca { get; set; }
        public int FrecuenciaRespiratoria { get; set; }
        public decimal Peso { get; set; }
        public decimal Talla { get; set; }
        public decimal PerimAbdominal { get; set; }
        public decimal SuperficieCorporal { get; set; }
        public decimal Imc { get; set; }
        public string ClasificacionImc { get; set; } = "";
        public string RiesgoEnfermedad { get; set; } = "";
        public string EstadoTriage { get; set; } = "";
        public string Observaciones { get; set; } = "";
    }
}