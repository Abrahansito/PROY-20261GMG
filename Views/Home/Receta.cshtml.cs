using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace PROY_20252SGMG.Views.RecetaMedica
{
  public class Receta : PageModel
  {
    private readonly ILogger<Receta> _logger;

    public Receta(ILogger<Receta> logger)
    {
      _logger = logger;
    }

    public void OnGet()
    {
    }
  }
}