using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace SGMG.Dtos.Response
{
  public class PersonalRegistradoResponse
  {
    public int Id { get; set; }
    public string? Dni { get; set; }
    public string? NombresApellidos { get; set; }
    public string? Cargo { get; set; }
    public string? Estado { get; set; }
    public string? Telefono { get; set; }
  }
}