using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using BCrypt.Net;
using InmobiliariaApp.Data;

var builder = WebApplication.CreateBuilder(args);

// 👉 Agregar soporte para controladores y vistas
builder.Services.AddControllersWithViews();

// 👉 Registrar repositorios para inyección de dependencias
builder.Services.AddScoped<RepoInmueble>();
builder.Services.AddScoped<RepoPersona>();

// ✅ Registrar RepoUsuario tanto con la interfaz como con la clase concreta
builder.Services.AddScoped<IRepoUsuario>(sp =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
    if (string.IsNullOrEmpty(connectionString))
        throw new InvalidOperationException("Connection string 'DefaultConnection' is missing or empty.");
    return new RepoUsuario(connectionString);
});

builder.Services.AddScoped<RepoUsuario>(sp =>
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

// 👉 Habilitar archivos estáticos (para avatares, fotos, etc.)
app.UseStaticFiles();   // 🔹 ESTA LÍNEA ES LA CLAVE

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
