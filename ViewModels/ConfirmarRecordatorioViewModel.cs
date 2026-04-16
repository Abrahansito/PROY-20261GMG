using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace PROY_20252SGMG.ViewsModels
{
  public class ConfirmarRecordatorioViewModel
  {
    // Información del Paciente
    public string NombreCompletoPaciente { get; set; } = "";
    public string Dni { get; set; } = "";
    public string Telefono { get; set; } = "";
    public int Edad { get; set; }

    // Detalles de la Cita
    public int IdCita { get; set; }
    public string Fecha { get; set; } = "";
    public string Hora { get; set; } = "";
    public string Medico { get; set; } = "";
    public string Consultorio { get; set; } = "";
    public string Especialidad { get; set; } = "";
    public string Estado { get; set; } = "";

    // Mensaje de WhatsApp
    public string MensajeWhatsApp { get; set; } = "";

    // Hospital Info
    public string NombreHospital { get; set; } = "Hospital Nacional Hipólito Unanue";
    public string TelefonoHospital { get; set; } = "(01) 362-0220";
  }
}
