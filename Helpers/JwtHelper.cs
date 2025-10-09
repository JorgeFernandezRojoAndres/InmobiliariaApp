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

            // 🔹 Roles (si hay)
            foreach (var rol in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, rol));
            }

            // 🔹 Clave secreta
            var secretKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("Falta la clave JWT en appsettings.json.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        // ✅ NUEVA SOBRECARGA: ahora el controlador puede pasar directamente los claims personalizados
        public string GenerarToken(IEnumerable<Claim> claims)
        {
            if (claims == null)
                throw new ArgumentNullException(nameof(claims), "Los claims no pueden ser nulos.");

            var secretKey = _config["Jwt:Key"];
            if (string.IsNullOrEmpty(secretKey))
                throw new InvalidOperationException("Falta la clave JWT en appsettings.json.");

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _config["Jwt:Issuer"],
                audience: _config["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(3),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
