using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class Paciente
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPaciente { get; set; }
    public string NumeroDocumento { get; set; } = "";
    public string TipoDocumento { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string ApellidoPaterno { get; set; } = "";
    public string ApellidoMaterno { get; set; } = "";
    public string Sexo { get; set; } = "";
    public int Edad { get; set; }
    public DateTime FechaRegistro { get; set; }
    public string Telefono { get; set; } = "";
    // Relaciones
    public ICollection<HistoriaClinica> HistoriasClinicas { get; set; } = new List<HistoriaClinica>();
    public ICollection<Triaje> Triages { get; set; } = new List<Triaje>();
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    public ICollection<DomicilioPaciente> Domicilios { get; set; } = new List<DomicilioPaciente>();
    public ICollection<Diagnostico> Diagnosticos { get; set; } = new List<Diagnostico>(); //NUEVO
  }
}