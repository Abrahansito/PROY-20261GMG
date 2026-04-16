namespace SGMG.Dtos.Response
{
    public class LaboratorioHistorialDTO
    {
        public PacienteInfoDTO Paciente { get; set; } = new();
        public List<OrdenLaboratorioResponseDTO> Ordenes { get; set; } = new();
    }
}