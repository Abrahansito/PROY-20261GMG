using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SGMG.Dtos.Request.Paciente
{
  public class PacienteRequestDTO

  {
    public int IdPaciente { get; set; }
    [Required(ErrorMessage = "El número de DNI es obligatorio")]
    [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe contener solo 8 dígitos")]
    public string? NumeroDocumento { get; set; }

    [Required(ErrorMessage = "El tipo de documento es obligatorio")]
    [RegularExpression("^(DNI|CE|Pasaporte)$", ErrorMessage = "El tipo de documento debe ser DNI, CE o Pasaporte")]
    public string? TipoDocumento { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
    public string? Nombre { get; set; }

    [Required(ErrorMessage = "El apellido paterno es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido paterno debe tener entre 2 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido paterno solo puede contener letras y espacios")]
    public string? ApellidoPaterno { get; set; }

    [Required(ErrorMessage = "El apellido materno es obligatorio")]
    [StringLength(50, MinimumLength = 2, ErrorMessage = "El apellido materno debe tener entre 2 y 50 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido materno solo puede contener letras y espacios")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "El sexo es obligatorio")]
    [RegularExpression("^(M|F)$", ErrorMessage = "El sexo debe ser M (Masculino) o F (Femenino)")]
    public string? Sexo { get; set; }

    [Required(ErrorMessage = "La edad es obligatoria")]
    [Range(0, 200, ErrorMessage = "La edad debe ser un numero entero")]
    public int Edad { get; set; }
  }
}