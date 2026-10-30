using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace InmobiliariaApp.Helpers
{
    /// <summary>
    /// Resuelve la inmobiliaria (tenant) del usuario que está haciendo la petición.
    /// Se usa desde los repositorios para filtrar todas las consultas.
    ///
    /// Reglas:
    ///  - Sin sesión web (API móvil con JWT, seeder, etc.) => null => SIN filtro,
    ///    para no romper el flujo móvil que se autentica contra personas, no usuarios.
    ///  - Sesión de cookie con el claim "InmobiliariaId" => ese valor.
    ///  - Sesión de cookie sin el claim (cookies antiguas a la migración) =>
    ///    se resuelve desde la BD y, si el usuario no tiene inmobiliaria cargada,
    ///    se usa su propio Id (dueño de su propia inmobiliaria, sin datos ajenos).
    ///
    /// ⚠️ No depende de IRepoUsuario: RepoUsuario depende de este servicio y se
    ///    generaría una dependencia circular en el contenedor de DI.
    /// </summary>
    public interface IInmobiliariaContext
    {
        int? InmobiliariaId { get; }
    }

    public class InmobiliariaContext : IInmobiliariaContext
    {
        public const string ClaimType = "InmobiliariaId";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly string _connectionString;

        private bool _resuelto;
        private int? _valor;

        public InmobiliariaContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
        {
            _httpContextAccessor = httpContextAccessor;
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        // Se cachea por request (el servicio es scoped) para no repetir la consulta a la BD.
        public int? InmobiliariaId
        {
            get
            {
                if (!_resuelto)
                {
                    _valor = Resolver();
                    _resuelto = true;
                }
                return _valor;
            }
        }

        private int? Resolver()
        {
            var identity = _httpContextAccessor.HttpContext?.User?.Identity;
            if (identity == null || !identity.IsAuthenticated)
                return null;

            // Solo las sesiones web (cookie) están atadas a un Usuario de esta BD.
            if (identity.AuthenticationType != CookieAuthenticationDefaults.AuthenticationScheme)
                return null;

            var user = _httpContextAccessor.HttpContext!.User;

            var claim = user.FindFirst(ClaimType);
            if (int.TryParse(claim?.Value, out var inmobiliariaId))
                return inmobiliariaId;

            var idClaim = user.FindFirst(ClaimTypes.NameIdentifier);
            if (!int.TryParse(idClaim?.Value, out var usuarioId))
                return null;

            // Cookie emitida antes de la migración: buscar la inmobiliaria en la BD.
            return ConsultarInmobiliaria(usuarioId) ?? usuarioId;
        }

        private int? ConsultarInmobiliaria(int usuarioId)
        {
            using var connection = new MySqlConnection(_connectionString);
            connection.Open();

            using var cmd = new MySqlCommand(
                "SELECT InmobiliariaId FROM usuarios WHERE Id = @Id", connection);
            cmd.Parameters.AddWithValue("@Id", usuarioId);

            var resultado = cmd.ExecuteScalar();
            return resultado == null || resultado == DBNull.Value
                ? null
                : Convert.ToInt32(resultado);
        }
    }
}
