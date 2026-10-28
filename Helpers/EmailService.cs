using System.Net;
using System.Net.Mail;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace InmobiliariaApp.Helpers
{
    public class EmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task EnviarCorreoAsync(string destino, string asunto, string cuerpoHtml)
        {
            // 🧩 Cargar configuración con valores por defecto
            var host = _config["Smtp:Host"] ?? "smtp.gmail.com";
            var user = _config["Smtp:User"] ?? "no-reply@inmobiliaria.com";
            var pass = _config["Smtp:Pass"] ?? string.Empty;
            var port = int.TryParse(_config["Smtp:Port"], out int p) ? p : 587;

            // ⚙️ Configurar cliente SMTP
            var smtp = new SmtpClient(host, port)
            {
                Credentials = new NetworkCredential(user, pass),
                EnableSsl = true
            };

            // ✉️ Crear el correo con valor seguro en el remitente
            var mail = new MailMessage
            {
                From = new MailAddress(user, "Inmobiliaria 2025"),
                Subject = asunto,
                Body = cuerpoHtml,
                IsBodyHtml = true
            };

            mail.To.Add(destino);

            // 🚀 Enviar el correo
            await smtp.SendMailAsync(mail);
        }
    }
}
