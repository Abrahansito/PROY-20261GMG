using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SGMG.Models
{
  public class DetalleReceta
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdDetalle { get; set; }

    [ForeignKey(nameof(Receta))]
    public int IdReceta { get; set; }

    [Required(ErrorMessage = "El producto farmacéutico es obligatorio")]
    [StringLength(150)]
    public string ProductoFarmaceutico { get; set; } = "";

    [Required(ErrorMessage = "La concentración es obligatoria")]
    [StringLength(100)]
    public string Concentracion { get; set; } = "";

    [Required(ErrorMessage = "La frecuencia es obligatoria")]
    [StringLength(100)]
    public string Frecuencia { get; set; } = "";

    [Required(ErrorMessage = "La duración es obligatoria")]
    [StringLength(100)]
    public string Duracion { get; set; } = "";

    [Required(ErrorMessage = "La vía de administración es obligatoria")]
    [StringLength(100)]
    public string ViaAdministracion { get; set; } = "";

    public string Observaciones { get; set; } = "";

    public string Estado { get; set; } = "PENDIENTE";

    // Relaciones
    public Receta Receta { get; set; } = null!;
  }
}
