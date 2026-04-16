using System.ComponentModel.DataAnnotations;
using SGMG.Dtos.Request.Paciente;
using SGMG.Dtos.Request.HistoriaClinica;
using SGMG.Dtos.Request.DomicilioPaciente;

namespace SGMG.Dtos.Request.HistoriaClinica
{
    public class HistoriaClinicaCompletaRequestDTO
    {
        [Required]
        public PacienteRequestDTO Paciente { get; set; } = new PacienteRequestDTO();

        [Required]
        public HistoriaClinicaRequestDTO HistoriaClinica { get; set; } = new HistoriaClinicaRequestDTO();

        public DomicilioPacienteRequestDTO? Domicilio { get; set; } = new DomicilioPacienteRequestDTO();

        public bool EsEdicion => Paciente.IdPaciente > 0;
    }
}