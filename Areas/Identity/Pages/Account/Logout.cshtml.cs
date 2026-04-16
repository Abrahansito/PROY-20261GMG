// ============================================
// Archivo: Areas/Identity/Pages/Account/Logout.cshtml.cs
// ============================================

using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using SGMG.Models; // ✅ IMPORTANTE: Usar tu modelo ApplicationUser
using PROY_20252SGMG.Models; // Asegúrate de que este namespace es correcto

namespace SGMG.Areas.Identity.Pages.Account
{
  [AllowAnonymous]
  public class LogoutModel : PageModel
  {
    private readonly SignInManager<ApplicationUser> _signInManager; // ✅ ApplicationUser, NO IdentityUser
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(
        SignInManager<ApplicationUser> signInManager,
        ILogger<LogoutModel> logger)
    {
      _signInManager = signInManager;
      _logger = logger;
    }

    public async Task<IActionResult> OnPost(string returnUrl = null)
    {
      await _signInManager.SignOutAsync();
      _logger.LogInformation("Usuario cerró sesión exitosamente.");

      // ✅ Redirigir a /Home/Index después de cerrar sesión
      return RedirectToAction("Index", "Home");
    }

    public IActionResult OnGet()
    {
      // Si alguien intenta acceder por GET, cerrar sesión de todas formas
      return RedirectToAction("Index", "Home");
    }
  }
}