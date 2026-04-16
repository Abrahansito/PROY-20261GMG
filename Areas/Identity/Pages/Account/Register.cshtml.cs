using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using SGMG.Models;
using PROY_20252SGMG.Models;
using SGMG.Data;

namespace SGMG.Areas.Identity.Pages.Account
{
  public class RegisterModel : PageModel
  {
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IUserStore<ApplicationUser> _userStore;
    private readonly IUserEmailStore<ApplicationUser> _emailStore;
    private readonly ILogger<RegisterModel> _logger;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ApplicationDbContext _context;

    // Lista de roles con GUID como Value
    public List<SelectListItem> RolesList { get; set; } = new List<SelectListItem>
        {
            new SelectListItem { Value = "f3a2c1d4-1234-5678-9abc-def012345678", Text = "ADMINISTRADOR" },
            new SelectListItem { Value = "b1c2d3e4-2345-6789-abcd-ef0123456789", Text = "MEDICO" },
            new SelectListItem { Value = "c1d2e3f4-3456-789a-bcde-f01234567890", Text = "ENFERMERIA" },
            new SelectListItem { Value = "d1e2f3a4-4567-89ab-cdef-012345678901", Text = "CAJERO" },
            new SelectListItem { Value = "d1e2f3a4-4567-89ab-cdef-012345671234", Text = "ADMISION" }
        };

    public RegisterModel(
        UserManager<ApplicationUser> userManager,
        IUserStore<ApplicationUser> userStore,
        SignInManager<ApplicationUser> signInManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<RegisterModel> logger,
        ApplicationDbContext context)
    {
      _userManager = userManager;
      _userStore = userStore;
      _emailStore = GetEmailStore();
      _signInManager = signInManager;
      _roleManager = roleManager;
      _logger = logger;
      _context = context;
    }

    [BindProperty]
    public InputModel Input { get; set; }

    public string ReturnUrl { get; set; }
    public IList<AuthenticationScheme> ExternalLogins { get; set; }

    public class InputModel
    {
      [Required(ErrorMessage = "El correo electrónico es requerido")]
      [EmailAddress(ErrorMessage = "Ingrese un correo electrónico válido")]
      [Display(Name = "Email")]
      public string Email { get; set; }

      [Required(ErrorMessage = "La contraseña es requerida")]
      [StringLength(100, MinimumLength = 6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
      [DataType(DataType.Password)]
      [Display(Name = "Password")]
      public string Password { get; set; }

      [DataType(DataType.Password)]
      [Display(Name = "Confirm password")]
      [Compare("Password", ErrorMessage = "Las contraseñas no coinciden")]
      public string ConfirmPassword { get; set; }

      [Required(ErrorMessage = "Por favor seleccione un rol")]
      [Display(Name = "Role")]
      public string RoleId { get; set; }

      [Required(ErrorMessage = "Por favor seleccione un personal")]
      [Display(Name = "Personal")]
      public string PersonalId { get; set; }
    }

    public async Task OnGetAsync(string returnUrl = null)
    {
      ReturnUrl = returnUrl;
      ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();
    }

    /// <summary>
    /// API para obtener personal por rol
    /// Busca directamente en las tablas de Medico, PersonalTecnico y Enfermeria
    /// NO busca en AspNetUsers
    /// </summary>
    public async Task<IActionResult> OnGetPersonalByRoleAsync(string roleId)
    {
      try
      {
        var usuariosRegistrados = await _context.Users
            .Select(u => u.IdUsuario)
            .ToListAsync();
        var roleName = RolesList.FirstOrDefault(r => r.Value == roleId)?.Text;


        _logger.LogInformation("Obteniendo personal para el rol: {RoleName}", roleName);
        _logger.LogInformation("Usuarios ya registrados: {Ids}", string.Join(", ", usuariosRegistrados));


        if (string.IsNullOrEmpty(roleName))
        {
          return new JsonResult(new List<object>());
        }

        List<object> personalList = new List<object>();

        switch (roleName.ToUpper())
        {
          case "MEDICO":
            // 🔹 Buscar en la tabla MEDICO directamente
            var medicos = await _context.Medicos
                .Where(m => m.EstadoLaboral.ToUpper() == "ACTIVO" && !usuariosRegistrados.Contains(m.IdMedico.ToString()))
                .OrderBy(m => (m.Nombre + " " + m.ApellidoPaterno + " " + m.ApellidoMaterno).Trim())
                .Select(m => new
                {
                  id = m.IdMedico.ToString(),
                  nombre = (m.Nombre + " " + m.ApellidoPaterno + " " + m.ApellidoMaterno).Trim(),
                  numeroDni = m.NumeroDni,
                  cargoMedico = m.CargoMedico,
                  areaServicio = m.AreaServicio,
                  numeroColegiatura = m.NumeroColegiatura,
                  tipoMedico = m.TipoMedico,
                  turno = m.Turno
                })
                .ToListAsync();

            personalList.AddRange(medicos);
            _logger.LogInformation($"Se encontraron {medicos.Count} médicos activos");
            break;

          case "ENFERMERIA":
            // 🔹 Buscar PERSONAL TÉCNICO con cargo ENFERMERIA
            var personalEnfermeria = await _context.PersonalTecnicos
                .Where(p => p.EstadoLaboral.ToUpper() == "ACTIVO" &&
                           p.Cargo.ToUpper() == "ENFERMERIA" && !usuariosRegistrados.Contains(p.IdPersonal.ToString()))
                .OrderBy(p => (p.Nombre + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno).Trim())
                .ToListAsync();

            foreach (var personal in personalEnfermeria)
            {
              var enfermeria = await _context.Enfermerias
                  .FirstOrDefaultAsync(e => e.IdPersonal == personal.IdPersonal);

              if (enfermeria != null)
              {
                personalList.Add(new
                {
                  id = enfermeria.IdEnfermeria.ToString(),
                  nombre = (personal.Nombre + " " + personal.ApellidoPaterno + " " + personal.ApellidoMaterno).Trim(),
                  numeroDni = personal.NumeroDni,
                  cargo = personal.Cargo,
                  areaServicio = personal.AreaServicio,
                  numeroColegiaturaEnfermeria = enfermeria.NumeroColegiaturaEnfermeria,
                  nivelProfesional = enfermeria.NivelProfesional,
                  turno = personal.Turno
                });
              }
            }

            _logger.LogInformation($"Se encontraron {personalList.Count} enfermeras activas");
            break;

          case "CAJERO":
            // 🔹 Buscar PERSONAL TÉCNICO con cargo CAJERO
            var cajeros = await _context.PersonalTecnicos
                .Where(p => p.EstadoLaboral.ToUpper() == "ACTIVO" &&
                           p.Cargo.ToUpper() == "CAJERO" && !usuariosRegistrados.Contains(p.IdPersonal.ToString()))
                .OrderBy(p => (p.Nombre + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno).Trim())
                .Select(p => new
                {
                  id = p.IdPersonal.ToString(),
                  nombre = (p.Nombre + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno).Trim(),
                  numeroDni = p.NumeroDni,
                  cargo = p.Cargo,
                  areaServicio = p.AreaServicio,
                  turno = p.Turno,
                  fechaIngreso = p.FechaIngreso.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            personalList.AddRange(cajeros);
            _logger.LogInformation($"Se encontraron {cajeros.Count} cajeros activos");
            break;

          case "ADMINISTRADOR":
            // 🔹 Buscar PERSONAL TÉCNICO con cargo ADMINISTRADOR
            var administradores = await _context.PersonalTecnicos
                .Where(p => p.EstadoLaboral.ToUpper() == "ACTIVO" &&
                           p.Cargo.ToUpper() == "ADMINISTRADOR" && !usuariosRegistrados.Contains(p.IdPersonal.ToString()))
                .OrderBy(p => (p.Nombre + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno).Trim())
                .Select(p => new
                {
                  id = p.IdPersonal.ToString(),
                  nombre = (p.Nombre + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno).Trim(),
                  numeroDni = p.NumeroDni,
                  cargo = p.Cargo,
                  areaServicio = p.AreaServicio,
                  turno = p.Turno,
                  fechaIngreso = p.FechaIngreso.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            personalList.AddRange(administradores);
            _logger.LogInformation($"Se encontraron {administradores.Count} administradores activos");
            break;

          case "ADMISION":
            // 🔹 Buscar PERSONAL TÉCNICO con cargo ADMISION
            var admisionistas = await _context.PersonalTecnicos
                .Where(p => p.EstadoLaboral.ToUpper() == "ACTIVO" &&
                           p.Cargo.ToUpper() == "ADMISION" && !usuariosRegistrados.Contains(p.IdPersonal.ToString()))
                .OrderBy(p => (p.Nombre + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno).Trim())
                .Select(p => new
                {
                  id = p.IdPersonal.ToString(),
                  nombre = (p.Nombre + " " + p.ApellidoPaterno + " " + p.ApellidoMaterno).Trim(),
                  numeroDni = p.NumeroDni,
                  cargo = p.Cargo,
                  areaServicio = p.AreaServicio,
                  turno = p.Turno,
                  fechaIngreso = p.FechaIngreso.ToString("dd/MM/yyyy")
                })
                .ToListAsync();

            personalList.AddRange(admisionistas);
            _logger.LogInformation($"Se encontraron {admisionistas.Count} personal de admisión activo");
            break;

          default:
            _logger.LogWarning($"Rol no reconocido: {roleName}");
            break;
        }

        return new JsonResult(personalList);
      }
      catch (Exception ex)
      {
        _logger.LogError(ex, "Error al obtener personal por rol: {RoleId}", roleId);
        return new JsonResult(new { error = ex.Message });
      }
    }


    public async Task<IActionResult> OnPostAsync(string returnUrl = null)
    {
      returnUrl ??= Url.Content("~/");
      ExternalLogins = (await _signInManager.GetExternalAuthenticationSchemesAsync()).ToList();

      if (ModelState.IsValid)
      {
        try
        {
          // Verificar que el email no esté registrado
          var existingUser = await _userManager.FindByEmailAsync(Input.Email);
          if (existingUser != null)
          {
            ModelState.AddModelError(string.Empty, "Este correo electrónico ya está registrado.");
            return Page();
          }

          var user = CreateUser();

          // 🔹 Asignar el IdUsuario con el ID del personal seleccionado
          // Este ID ya viene correcto del JavaScript:
          // - Para MEDICO: IdMedico
          // - Para ENFERMERIA: IdEnfermeria (de la tabla Enfermeria)
          // - Para CAJERO: IdPersonal
          // - Para ADMINISTRADOR: IdPersonal
          // - Para ADMISION: IdPersonal
          user.IdUsuario = Input.PersonalId;

          await _userStore.SetUserNameAsync(user, Input.Email, CancellationToken.None);
          await _emailStore.SetEmailAsync(user, Input.Email, CancellationToken.None);

          // Activar LockoutEnabled y EmailConfirmed
          user.LockoutEnabled = true;
          user.EmailConfirmed = true;

          var result = await _userManager.CreateAsync(user, Input.Password);

          if (result.Succeeded)
          {
            _logger.LogInformation("Usuario creado exitosamente: {Email} con IdUsuario: {IdUsuario}",
                user.Email, user.IdUsuario);

            var roleName = RolesList.FirstOrDefault(r => r.Value == Input.RoleId)?.Text;

            if (!string.IsNullOrEmpty(roleName))
            {
              if (!await _roleManager.RoleExistsAsync(roleName))
              {
                await _roleManager.CreateAsync(new IdentityRole(roleName));
              }

              await _userManager.AddToRoleAsync(user, roleName);
            }

            // ✅ Mensaje de éxito persistente
            TempData["SuccessMessage"] = "Usuario registrado exitosamente.";

            // Redirigir según el rol asignado
            return roleName switch
            {
              "ADMINISTRADOR" => RedirectToPage("/Account/Register", new { area = "Identity" }),
              "MEDICO" => RedirectToPage("/Account/Register", new { area = "Identity" }),
              "ENFERMERIA" => RedirectToPage("/Account/Register", new { area = "Identity" }),
              "CAJERO" => RedirectToPage("/Account/Register", new { area = "Identity" }),
              "ADMISION" => RedirectToPage("/Account/Register", new { area = "Identity" }),
              _ => LocalRedirect(returnUrl)
            };
          }


          // Mostrar errores si falla la creación
          foreach (var error in result.Errors)
          {
            ModelState.AddModelError(string.Empty, error.Description);
          }
        }
        catch (Exception ex)
        {
          _logger.LogError(ex, "Error al crear el usuario");
          ModelState.AddModelError(string.Empty, "Ocurrió un error al crear el usuario. Por favor, intente nuevamente.");
        }
      }

      // Si algo falla, volver a mostrar la página con errores
      return Page();
    }

    private ApplicationUser CreateUser()
    {
      try
      {
        return Activator.CreateInstance<ApplicationUser>();
      }
      catch
      {
        throw new InvalidOperationException($"No se puede crear una instancia de '{nameof(ApplicationUser)}'.");
      }
    }

    private IUserEmailStore<ApplicationUser> GetEmailStore()
    {
      if (!_userManager.SupportsUserEmail)
      {
        throw new NotSupportedException("La interfaz predeterminada requiere un almacén de usuarios con soporte de correo electrónico.");
      }
      return (IUserEmailStore<ApplicationUser>)_userStore;
    }
  }
}