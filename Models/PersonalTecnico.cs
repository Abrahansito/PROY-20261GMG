using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class PersonalTecnico
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPersonal { get; set; }
    public string NumeroDni { get; set; } = "";
    public string Nombre { get; set; } = "";
    public string ApellidoPaterno { get; set; } = "";
    public string ApellidoMaterno { get; set; } = "";
    public DateTime FechaNacimiento { get; set; }
    public string Sexo { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Telefono { get; set; } = "";
    public string Email { get; set; } = "";
    public string EstadoLaboral { get; set; } = "";
    public DateTime FechaIngreso { get; set; }
    public string Turno { get; set; } = "";
    public string AreaServicio { get; set; } = "";
    public string Cargo { get; set; } = "";

  }
}