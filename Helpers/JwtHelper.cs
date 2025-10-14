using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace InmobiliariaApp.Helpers
{
    public class JwtHelper
    {
        private readonly IConfiguration _config;

        public JwtHelper(IConfiguration config)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
        }

        // 🔹 MÉTODO ORIGINAL (no borrar)
        public string GenerarToken(string email, List<string> roles)
        {
            if (string.IsNullOrEmpty(email))
                throw new ArgumentException("El email no puede ser nulo o vacío.", nameof(email));

            roles ??= new List<string>();

            // 🔹 Claims base
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(JwtRegisteredClaimNames.Sub, email),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            foreach (var rol in roles)
                claims.Add(new Claim(ClaimTypes.Role, rol));

            var secretKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("Falta la clave JWT en appsettings.json.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // ✅ Duración extendida según entorno
            var expiracion = EsEntornoDesarrollo()
                ? DateTime.UtcNow.AddDays(7)   // 7 días en desarrollo
                : DateTime.UtcNow.AddHours(3); // 3 horas en producción

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ NUEVA SOBRECARGA: permite pasar claims personalizados
        public string GenerarToken(IEnumerable<Claim> claims)
        {
            if (claims == null)
                throw new ArgumentNullException(nameof(claims), "Los claims no pueden ser nulos.");

            var secretKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("Falta la clave JWT en appsettings.json.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expiracion = EsEntornoDesarrollo()
                ? DateTime.UtcNow.AddDays(7)
                : DateTime.UtcNow.AddHours(3);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: expiracion,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // 🔧 Helper: detecta si estamos en entorno de desarrollo
        private bool EsEntornoDesarrollo()
        {
            var env = _config["ASPNETCORE_ENVIRONMENT"];
            return !string.IsNullOrEmpty(env) &&
                   env.Equals("Development", StringComparison.OrdinalIgnoreCase);
        }
    }
}
