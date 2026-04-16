using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.ComponentModel.DataAnnotations;

namespace SGMG.Dtos.Request.Enfermeria
{
  public class EnfermeriaRequestDTO
  {


    public int? IdEnfermeria { get; set; }

    public int? IdPersonal { get; set; }

    [StringLength(50, ErrorMessage = "El campo NumeroColegiaturaEnfermeria no puede exceder 50 caracteres")]
    public string? NumeroColegiaturaEnfermeria { get; set; }

    [StringLength(50, ErrorMessage = "El campo NivelProfesional no puede exceder 50 caracteres")]
    public string? NivelProfesional { get; set; }

    [Required(ErrorMessage = "El DNI es obligatorio")]
    [RegularExpression(@"^[0-9]{8}$", ErrorMessage = "El DNI debe contener solo 8 dígitos")]
    public string? NumeroDni { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "El nombre debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El nombre solo puede contener letras y espacios")]
    public string? Nombre { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido paterno debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido paterno solo puede contener letras y espacios")]
    public string? ApellidoPaterno { get; set; }

    [StringLength(100, MinimumLength = 2, ErrorMessage = "El apellido materno debe tener entre 2 y 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El apellido materno solo puede contener letras y espacios")]
    public string? ApellidoMaterno { get; set; }

    [Required(ErrorMessage = "La fecha de nacimiento es obligatoria")]
    [DataType(DataType.Date, ErrorMessage = "La fecha de nacimiento debe estar en formato yyyy-MM-dd")]
    public DateTime? FechaNacimiento { get; set; }


    [RegularExpression("^(M|F)$", ErrorMessage = "El sexo debe ser M (Masculino) o F (Femenino)")]
    public string? Sexo { get; set; }

    [StringLength(200, ErrorMessage = "La dirección no puede exceder 200 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9áéíóúÁÉÍÓÚñÑ\s.,#-]+$", ErrorMessage = "La dirección contiene caracteres no válidos")]
    public string? Direccion { get; set; }

    [Phone(ErrorMessage = "El formato del teléfono no es válido")]
    [StringLength(12, MinimumLength = 7, ErrorMessage = "El teléfono debe tener entre 7 y 12 caracteres")]
    [RegularExpression(@"^\+?[0-9]{7,12}$", ErrorMessage = "El teléfono solo puede contener números y opcionalmente el símbolo + al inicio")]
    public string? Telefono { get; set; }


    [StringLength(100, ErrorMessage = "El email no puede exceder 100 caracteres")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "El formato del email no es válido")]
    public string? Email { get; set; }

    [RegularExpression("^(Activo|Inactivo|Licencia|Vacaciones|Suspendido)$", ErrorMessage = "El estado laboral debe ser: Activo, Inactivo, Licencia, Vacaciones o Suspendido")]
    public string? EstadoLaboral { get; set; }

    [Required(ErrorMessage = "La fecha de ingreso es obligatoria")]
    [DataType(DataType.Date, ErrorMessage = "La fecha de ingreso debe estar en formato yyyy-MM-dd")]
    public DateTime? FechaIngreso { get; set; }

    [RegularExpression("^(Mañana|Tarde|Noche|Rotativo)$", ErrorMessage = "El turno debe ser: Mañana, Tarde, Noche o Rotativo")]
    public string? Turno { get; set; }

    [StringLength(100, ErrorMessage = "El área de servicio no puede exceder 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El área de servicio solo puede contener letras y espacios")]
    public string? AreaServicio { get; set; }

    [StringLength(100, ErrorMessage = "El cargo no puede exceder 100 caracteres")]
    [RegularExpression(@"^[a-zA-ZáéíóúÁÉÍÓÚñÑ\s]+$", ErrorMessage = "El cargo solo puede contener letras y espacios")]
    public string? Cargo { get; set; }

    [Required(ErrorMessage = "El ID del consultorio es obligatorio")]
    [Range(1, int.MaxValue, ErrorMessage = "El ID del consultorio debe ser un número positivo")]
    public int? IdConsultorio { get; set; }
  }
}