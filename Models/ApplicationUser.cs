using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Identity;

namespace PROY_20252SGMG.Models
{
  public class ApplicationUser : IdentityUser
  {
    public string IdUsuario { get; set; }
  }
}