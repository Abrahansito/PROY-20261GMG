using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace SGMG.Models
{
  public class Pago
  {
    [Key]
    [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
    public int IdPago { get; set; }

    [ForeignKey(nameof(Cita))]
    public int IdCita { get; set; }
    public string CodigoServicio { get; set; } = "";
    public string DescripcionServicio { get; set; } = "";
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }
    public decimal Subtotal { get; set; }
    public decimal Igv { get; set; }
    public decimal Total { get; set; }
    public string EstadoPago { get; set; } = "";
    public DateTime FechaPago { get; set; }

    // Relaciones
    public Cita Cita { get; set; } = null!;
  }
}