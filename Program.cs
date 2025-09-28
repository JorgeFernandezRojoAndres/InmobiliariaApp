using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using BCrypt.Net;
using InmobiliariaApp.Data;
using Microsoft.Extensions.FileProviders; // 🔹 necesario para PhysicalFileProvider

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<RepoInmueble>();
builder.Services.AddScoped<RepoPersona>();
builder.Services.AddScoped<IRepoContrato, RepoContrato>();
builder.Services.AddScoped<IRepoPago, RepoPago>();

// 🔹 Registrar el repo de tipos de inmueble
builder.Services.AddScoped<IRepoTipoInmueble>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
    return new RepoTipoInmueble(connectionString); // 👈 tu implementación concreta
});

builder.Services.AddScoped<IRepoUsuario>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
    return new RepoUsuario(connectionString);
});

// 👉 Configuración de autenticación con cookies
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        // Si el usuario no está autenticado, lo manda al login
        options.LoginPath = "/Auth/Login";

        // Si el usuario está autenticado pero no tiene permisos, lo manda acá
        options.AccessDeniedPath = "/Auth/AccessDenied";
    });

// 👉 Configuración de autorización con políticas
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy =>
        policy.RequireRole("Administrador"));
});

var app = builder.Build();

app.UseRouting();

// 👉 Habilitar archivos estáticos generales
app.UseStaticFiles();

// 👉 Habilitar carpeta avatars aunque se creen en runtime
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars")),
    RequestPath = "/avatars"
});

// 👉 Activar autenticación y autorización en el orden correcto
app.UseAuthentication();
app.UseAuthorization();

// 👉 Mapear controladores y vistas
app.MapStaticAssets();

// 🔹 Cambiar ruta por defecto: al iniciar -> Auth/Login
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

// 🔹 Generar hash de prueba (solo para debug, lo sacás después)
var hash = BCrypt.Net.BCrypt.HashPassword("1234");
Console.WriteLine("Hash generado para 1234 => " + hash);

// 🔹 Ejecutar seeder al iniciar
using (var scope = app.Services.CreateScope())
{
    var repoUsuario = scope.ServiceProvider.GetRequiredService<IRepoUsuario>();
    DbSeeder.Seed(repoUsuario);
}

app.Run();
