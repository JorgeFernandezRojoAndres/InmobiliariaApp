using InmobiliariaApp.Repository;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using BCrypt.Net;
using InmobiliariaApp.Data;
using InmobiliariaApp.Helpers;
using Microsoft.Extensions.FileProviders;
using Microsoft.OpenApi.Models;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// -------------------------
// 🧩 Servicios principales
// -------------------------
builder.Services.AddControllersWithViews();
builder.Services.AddControllers(); // ✅ Agregado: asegura que los controladores API se registren correctamente

// -------------------------
// 🧩 Inyección de dependencias
// -------------------------
builder.Services.AddScoped<RepoInmueble>();
builder.Services.AddScoped<RepoPersona>();
builder.Services.AddScoped<IRepoContrato, RepoContrato>();
builder.Services.AddScoped<IRepoPago, RepoPago>();
builder.Services.AddScoped<IRepoTipoInmueble, RepoTipoInmueble>(); // ✅ tu nuevo repo
builder.Services.AddScoped<IRepoUsuario, RepoUsuario>();
builder.Services.AddScoped<JwtHelper>();

// -------------------------
// 🧩 Swagger
// -------------------------
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inmobiliaria API",
        Version = "v1",
        Description = "Documentación de endpoints para app móvil y panel web"
    });

    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT en este formato: Bearer {token}"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// -------------------------
// 🔐 Autenticación
// -------------------------
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
.AddJwtBearer(JwtBearerDefaults.AuthenticationScheme, options =>
{
    var key = builder.Configuration["Jwt:Key"];
    if (string.IsNullOrEmpty(key))
        throw new InvalidOperationException("Faltan las claves JWT en appsettings.json (Jwt:Key).");

    options.RequireHttpsMetadata = false;
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
.AddCookie(CookieAuthenticationDefaults.AuthenticationScheme, options =>
{
    options.LoginPath = "/Auth/Login";
    options.AccessDeniedPath = "/Auth/AccessDenied";
});

// -------------------------
// 🔒 Autorización
// -------------------------
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("Administrador", policy => policy.RequireRole("Administrador"));
    options.AddPolicy("ApiJwt", policy =>
        policy.AddAuthenticationSchemes(JwtBearerDefaults.AuthenticationScheme)
              .RequireAuthenticatedUser());
});

// -------------------------
// 🚀 Construcción del app
// -------------------------
var app = builder.Build();

// -------------------------
// 🧭 Middleware
// -------------------------
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inmobiliaria API v1");
        c.RoutePrefix = "swagger";
    });
}

app.UseRouting();
app.UseStaticFiles();

// ✅ Archivos estáticos para avatares
var avatarsPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "avatars");
if (!Directory.Exists(avatarsPath))
{
    Directory.CreateDirectory(avatarsPath);
}
app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(avatarsPath),
    RequestPath = "/avatars"
});

app.UseAuthentication();
app.UseAuthorization();

// ✅ Importante: Mapea los controladores API REST
app.MapControllers(); // ← Esto asegura que /api/... funcione correctamente

// ✅ Rutas MVC tradicionales
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Auth}/{action=Login}/{id?}")
    .WithStaticAssets();

// -------------------------
// 🧱 Inicialización y servidor local
// -------------------------
var hash = BCrypt.Net.BCrypt.HashPassword("1234");
Console.WriteLine("Hash generado para 1234 => " + hash);

using (var scope = app.Services.CreateScope())
{
    var repoUsuario = scope.ServiceProvider.GetRequiredService<IRepoUsuario>();
    DbSeeder.Seed(repoUsuario);
}

// 🌍 Servidor accesible en red local
app.Urls.Add("http://0.0.0.0:5027");
Console.WriteLine("🌍 Servidor accesible en red local: http://0.0.0.0:5027");

app.Run();
