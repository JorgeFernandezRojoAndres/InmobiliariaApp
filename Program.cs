using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using InmobiliariaApp.Data;
using InmobiliariaApp.Helpers;
using Microsoft.Extensions.FileProviders;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// ======================================================
// 🔹 Servicios principales
// ======================================================
builder.Services.AddControllersWithViews();
builder.Services.AddScoped<RepoInmueble>();
builder.Services.AddScoped<RepoPersona>();
builder.Services.AddScoped<IRepoContrato, RepoContrato>();
builder.Services.AddScoped<IRepoPago, RepoPago>();
builder.Services.AddScoped<IRepoTipoInmueble, RepoTipoInmueble>();
builder.Services.AddScoped<IRepoUsuario, RepoUsuario>();
builder.Services.AddScoped<JwtHelper>();

// ======================================================
// 🔹 AUTENTICACIÓN: Cookies (web) + JWT (API móvil)
// ======================================================
builder.Services.AddAuthentication(options =>
{
    options.DefaultScheme = "Smart";
    options.DefaultAuthenticateScheme = "Smart";
    options.DefaultChallengeScheme = "Smart";
})
.AddPolicyScheme("Smart", "Smart", options =>
{
    options.ForwardDefaultSelector = context =>
        context.Request.Headers.ContainsKey("Authorization") &&
        context.Request.Headers["Authorization"].ToString().StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
        ? JwtBearerDefaults.AuthenticationScheme
        : CookieAuthenticationDefaults.AuthenticationScheme;
})
// 🔸 JWT Bearer → usado en /api/*
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var key = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(key))
        throw new InvalidOperationException("❌ Faltan las claves JWT en appsettings.json (Jwt:Key).");

    options.RequireHttpsMetadata = false; // para pruebas locales
    options.SaveToken = true;

    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = builder.Configuration["Jwt:Issuer"],
        ValidAudience = builder.Configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key))
    };

    // 🔍 Log de errores JWT en consola
    options.Events = new JwtBearerEvents
    {
        OnAuthenticationFailed = ctx =>
        {
            Console.WriteLine($"❌ Error JWT: {ctx.Exception.Message}");
            return Task.CompletedTask;
        },
        OnChallenge = ctx =>
        {
            ctx.HandleResponse();
            ctx.Response.StatusCode = 401;
            return ctx.Response.WriteAsync("Unauthorized");
        }
    };
})
// 🔸 Cookies → sigue habilitado para el panel MVC
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// ======================================================
// 🔹 Autorización por políticas
// ======================================================
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));

    // Política JWT explícita para las APIs móviles
    options.AddPolicy("ApiJwt", policy =>
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
              .RequireAuthenticatedUser());
});

var app = builder.Build();

// ======================================================
// 🔹 Middlewares
// ======================================================
app.UseRouting();

// 👉 Archivos estáticos
app.UseStaticFiles();

// 👉 Carpeta avatars
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(
        Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars")),
    RequestPath = "/avatars"
});

// 👉 Autenticación y autorización (orden correcto)
app.UseAuthentication();
app.UseAuthorization();

// ======================================================
// 🔹 Mapear controladores y vistas
// ======================================================
app.MapControllers(); // ✅ APIs
app.MapStaticAssets();

// 🔹 Ruta por defecto (panel MVC)
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

// ======================================================
// 🔹 Hash de prueba y Seeder
// ======================================================
var hash = BCrypt.Net.BCrypt.HashPassword("1234");
Console.WriteLine("Hash generado para 1234 => " + hash);

using (var scope = app.Services.CreateScope())
{
    var repoUsuario = scope.ServiceProvider.GetRequiredService<IRepoUsuario>();
    DbSeeder.Seed(repoUsuario);
}

// ======================================================
// 🌐 Escuchar en toda la red local
// ======================================================
app.Urls.Add("http://0.0.0.0:5027");
Console.WriteLine("🌍 Servidor accesible en red local: http://0.0.0.0:5027");

// ======================================================
// 🔹 Swagger opcional
// ======================================================
// builder.Services.AddEndpointsApiExplorer();
// builder.Services.AddSwaggerGen();
// if (app.Environment.IsDevelopment())
// {
//     app.UseSwagger();
//     app.UseSwaggerUI();
// }

app.Run();
