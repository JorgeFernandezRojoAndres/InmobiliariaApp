using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public abstract class BaseModel
    {
        protected readonly string ConnectionString;

        protected BaseModel(IConfiguration configuration)
        {
            ConnectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }
    }
}
