using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class Enfermeria
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdEnfermeria { get; set; }

    [ForeignKey(nameof(Personal))]
    public int IdPersonal { get; set; }
    public string NumeroColegiaturaEnfermeria { get; set; } = "";
    public string NivelProfesional { get; set; } = "";

    [ForeignKey(nameof(ConsultorioAsignado))]
    public int? IdConsultorio { get; set; }
    public Consultorio? ConsultorioAsignado { get; set; }
    // Relaciones
    public PersonalTecnico Personal { get; set; } = null!;
  }
}