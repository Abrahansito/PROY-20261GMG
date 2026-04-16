using System;
using System.Collections.Generic;

namespace SGMG.Dtos.Response
{
    public class PagoServicioDTO
    {
        public string Codigo { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Total { get; set; }
    }

    public class PagoResponseDTO
    {
        public int Id { get; set; }
        public string FechaPago { get; set; } = string.Empty;
        public string FechaCita { get; set; } = string.Empty;
        public string HoraCita { get; set; } = string.Empty;

        // Paciente
        public string TipoDoc { get; set; } = string.Empty;
        public string NumeroDoc { get; set; } = string.Empty;
        public string Nombres { get; set; } = string.Empty;
        public int Edad { get; set; }
        public string Telefono { get; set; } = string.Empty;
        public string HistoriaClinicaCodigo { get; set; } = string.Empty;
        public string TipoSeguro { get; set; } = string.Empty;

        // Cita / Médico
        public string Medico { get; set; } = string.Empty;
        public string Especialidad { get; set; } = string.Empty;
        public string Consultorio { get; set; } = string.Empty;
        public string Motivo { get; set; } = string.Empty;

        // Servicios y montos
        public List<PagoServicioDTO> Servicios { get; set; } = new List<PagoServicioDTO>();
        public decimal Subtotal { get; set; }
        public decimal Igv { get; set; }
        public decimal Total { get; set; }

        public string Estado { get; set; } = string.Empty;
    }
}
