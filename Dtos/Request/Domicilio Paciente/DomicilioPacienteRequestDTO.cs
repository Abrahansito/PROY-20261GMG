using System.ComponentModel.DataAnnotations;

namespace SGMG.Dtos.Request.DomicilioPaciente
{
    public class DomicilioPacienteRequestDTO
    {
        public int IdDomicilio { get; set; }
        
        public int IdPaciente { get; set; }

        [StringLength(100, ErrorMessage = "El departamento no puede exceder 100 caracteres")]
        public string? Departamento { get; set; }

        [StringLength(100, ErrorMessage = "La provincia no puede exceder 100 caracteres")]
        public string? Provincia { get; set; }

        [StringLength(100, ErrorMessage = "El distrito no puede exceder 100 caracteres")]
        public string? Distrito { get; set; }

        [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
        public string? Direccion { get; set; }

        [StringLength(200, ErrorMessage = "La referencia no puede exceder 200 caracteres")]
        public string? Referencia { get; set; }
    }
}