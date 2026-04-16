using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class Medico
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdMedico { get; set; }
    public string NumeroDni { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string ApellidoPaterno { get; set; } = "";
    public string ApellidoMaterno { get; set; } = "";
    public string Sexo { get; set; } = "";
    public DateTime FechaNacimiento { get; set; }
    public string Direccion { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string CorreoElectronico { get; set; } = "";
    public string EstadoLaboral { get; set; } = "";
    public DateTime FechaIngreso { get; set; }
    public string Turno { get; set; } = "";
    public string AreaServicio { get; set; } = "";
    public string CargoMedico { get; set; } = "";
    public string NumeroColegiatura { get; set; } = "";
    public string TipoMedico { get; set; } = "";

    [ForeignKey(nameof(ConsultorioAsignado))]
    public int? IdConsultorio { get; set; }
    public Consultorio? ConsultorioAsignado { get; set; }

    // Relaciones
    public ICollection<Cita> Citas { get; set; } = new List<Cita>();
    public ICollection<DisponibilidadMedico> Disponibilidades { get; set; } = new List<DisponibilidadMedico>();
    public ICollection<Diagnostico> Diagnosticos { get; set; } = new List<Diagnostico>(); //NUEVO
  }
}