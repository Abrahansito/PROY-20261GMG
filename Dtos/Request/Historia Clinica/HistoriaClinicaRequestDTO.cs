using System;
using System.ComponentModel.DataAnnotations;

namespace SGMG.Dtos.Request.HistoriaClinica
{
    public class HistoriaClinicaRequestDTO
    {
        public int IdHistoria { get; set; }
        
        public int IdPaciente { get; set; }

        public string? CodigoHistoria { get; set; }

        public string? TipoSeguro { get; set; }

        [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
        [DataType(DataType.Date)]
        public DateTime FechaNacimiento { get; set; }

        [Required(ErrorMessage = "El estado civil es obligatorio")]
        [StringLength(50, ErrorMessage = "El estado civil no puede exceder 50 caracteres")]
        public string? EstadoCivil { get; set; }

        [Required(ErrorMessage = "El tipo de sangre es obligatorio")]
        [StringLength(10, ErrorMessage = "El tipo de sangre no puede exceder 10 caracteres")]
        public string? TipoSangre { get; set; }
    }
}