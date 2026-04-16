using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using SGMG.Data;
using SGMG.Repository;
using SGMG.Repository.RepositoryImpl;
using SGMG.Services;
using SGMG.Services.ServiceImpl;
using SGMG.common.exception;
using SGMG.common.middleware;
using PROY_20252SGMG.Repository;
using PROY_20252SGMG.Services;
using PROY_20252SGMG.Models;
using NuGet.Packaging;
using QuestPDF.Infrastructure;

// QuestPDF.Settings.License = LicenseType.Community;
// The installed QuestPDF package version does not contain LicenseType.Community.
// If your QuestPDF version requires a license, set the appropriate value provided by that package
// or register a license according to the QuestPDF documentation.
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));
builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options =>
{
  options.SignIn.RequireConfirmedAccount = true;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddControllersWithViews(options =>
{

  options.Filters.Add<GlobalExceptionFilter>();
  options.Filters.Add<ValidationFilter>();

})
.ConfigureApiBehaviorOptions(options =>
{
  // Desactivar la respuesta automática de validación de ASP.NET
  // Para usar nuestro filtro personalizado
  options.SuppressModelStateInvalidFilter = true;
})
.AddJsonOptions(options =>
{
  // ESTO ES LO IMPORTANTE: No lanzar excepciones en errores de tipo
  options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
  // Esta opción hace que los errores de tipo se agreguen al ModelState
  // en lugar de lanzar JsonException
});
builder.Services.ConfigureApplicationCookie(options =>
{
  options.LoginPath = "/Identity/Account/Login";
  options.LogoutPath = "/Identity/Account/Logout";
  options.AccessDeniedPath = "/Identity/Account/AccessDenied";

  // ✅ Agregar esta línea para redirigir después del logout
  options.Events.OnSigningOut = async context =>
  {
    context.Response.Redirect("/Home/Index");
  };
});
builder.Services.AddRazorPages();
builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
      // Opciones adicionales para mejor control de serialización
      options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    });
// Registro de repositorios (Inyección de dependencias)
builder.Services.AddScoped<IPacienteRepository, PacienteRepositoryImpl>();
builder.Services.AddScoped<IMedicoRepository, MedicoRepositoryImpl>();
builder.Services.AddScoped<IEnfermeriaRepository, EnfermeriaRepositoryImpl>();
builder.Services.AddScoped<IPersonalRepository, PersonalRepositoryImpl>();
builder.Services.AddScoped<IPersonalTRepository, PersonalTRepositoryImpl>();
builder.Services.AddScoped<IHistoriaClinicaRepository, HistoriaClinicaRepository>();
builder.Services.AddScoped<IDomicilioPacienteRepository, DomicilioPacienteRepository>();
builder.Services.AddScoped<IPagoRepository, PagoRepository>();
builder.Services.AddScoped<IDisponibilidadSemanalRepository, DisponibilidadSemanalRepositoryImpl>();
builder.Services.AddScoped<ITriajeRepository, TriajeRepositoryImpl>();
builder.Services.AddScoped<ICitaRepository, CitaRepositoryImpl>();
builder.Services.AddScoped<IConsultaRepository, ConsultaRepositoryImpl>();
builder.Services.AddScoped<IOrdenLaboratorioRepository, OrdenLaboratorioRepositoryImpl>();






builder.Services.AddScoped<IDiagnosticoRepository, DiagnosticoRepositoryImpl>();


builder.Services.AddScoped<IRecetaRepository, RecetaRepositoryImpl>();
// Registro de servicios (Inyección de dependencias)
builder.Services.AddScoped<IPacienteService, PacienteService>();
builder.Services.AddScoped<IMedicoService, MedicoServiceImpl>();
builder.Services.AddScoped<IEnfermeriaService, EnfermeriaServiceImpl>();
builder.Services.AddScoped<IPersonalService, PersonalServiceImpl>();
builder.Services.AddScoped<IPersonalTservice, PersonalTServiceImpl>();
builder.Services.AddScoped<IPagoService, PagoService>();
builder.Services.AddScoped<ITriajeService, TriajeService>();
builder.Services.AddScoped<ICitaService, CitaService>();
builder.Services.AddScoped<IHistorialClinicoService, HistorialClinicoService>();
builder.Services.AddScoped<GlobalExceptionFilter>();
builder.Services.AddScoped<ValidationFilter>();
builder.Services.AddScoped<IRecetaService, RecetaService>();
builder.Services.AddScoped<IConsultaService, ConsultaService>();
builder.Services.AddScoped<ILaboratorioService, LaboratorioService>();
var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
  app.UseMigrationsEndPoint();
}
else
{
  app.UseExceptionHandler("/Home/Error");
  app.UseHsts();
}

// 👉 Mueve esto fuera del if para que funcione siempre
app.UseStatusCodePagesWithReExecute("/Home/Error404");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<JsonValidationMiddleware>();
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages();

app.Run();
