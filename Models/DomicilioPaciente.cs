using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class DomicilioPaciente
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDomicilio { get; set; }
    [ForeignKey(nameof(Paciente))]
    public int IdPaciente { get; set; }
    public string Departamento { get; set; } = "";
    public string Provincia { get; set; } = "";
    public string Distrito { get; set; } = "";
    public string Direccion { get; set; } = "";
    public string Referencia { get; set; } = "";

    // Relaciones
    public Paciente Paciente { get; set; } = null!;
  }
}