using MySql.Data.MySqlClient;
using InmobiliariaApp.Models;
using InmobiliariaApp.Helpers;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoPago : IRepoPago
    {
        private readonly string _connectionString;
        private readonly IInmobiliariaContext? _inmobiliaria;

        // 🔹 Ahora se inyecta IConfiguration para leer del appsettings.json
        public RepoPago(IConfiguration configuration, IInmobiliariaContext? inmobiliaria = null)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
            _inmobiliaria = inmobiliaria;
        }

        // 🔒 Filtro de aislamiento: null = sin filtro (API móvil / sin sesión web)
        private void AgregarFiltroInmobiliaria(MySqlCommand cmd)
        {
            cmd.Parameters.AddWithValue("@inmobiliariaId",
                (object?)_inmobiliaria?.InmobiliariaId ?? DBNull.Value);
        }

        public List<Pago> ObtenerTodos()
        {
            var lista = new List<Pago>();
            using var conn = new MySqlConnection(_connectionString);
            var sql = @"
                SELECT p.Id, p.ContratoId, p.FechaPago, p.Detalle, p.Importe,
                       CONCAT('Contrato #', c.Id, ' - Inmueble ', c.InmuebleID,
                              ' - ', DATE_FORMAT(c.FechaInicio,'%d/%m/%Y'),
                              ' a ', DATE_FORMAT(c.FechaFin,'%d/%m/%Y'),
                              ' ($', c.MontoMensual, ')') AS ContratoDescripcion
                FROM pagos p
                INNER JOIN contratos c ON p.ContratoId = c.Id
                WHERE @inmobiliariaId IS NULL OR p.InmobiliariaId = @inmobiliariaId";
            using var cmd = new MySqlCommand(sql, conn);
            AgregarFiltroInmobiliaria(cmd);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Pago
                {
                    Id = reader.GetInt32(0),
                    ContratoId = reader.GetInt32(1),
                    FechaPago = reader.GetDateTime(2),
                    Detalle = reader.IsDBNull(3) ? null : reader.GetString(3),
                    Importe = reader.GetDecimal(4),
                    ContratoDescripcion = reader.IsDBNull(5) ? null : reader.GetString(5)
                });
            }
            return lista;
        }
       public List<Pago> ObtenerPorContrato(int contratoId) 
{
    var lista = new List<Pago>();
    using var conn = new MySqlConnection(_connectionString); // ✅ ahora usa la cadena centralizada
    var sql = @"
        SELECT p.Id, p.ContratoId, p.FechaPago, p.Detalle, p.Importe,
               CONCAT('Contrato #', c.Id, ' - Inmueble ', c.InmuebleID,
                      ' - ', DATE_FORMAT(c.FechaInicio,'%d/%m/%Y'),
                      ' a ', DATE_FORMAT(c.FechaFin,'%d/%m/%Y'),
                      ' ($', c.MontoMensual, ')') AS ContratoDescripcion
        FROM pagos p
        INNER JOIN contratos c ON p.ContratoId = c.Id
        WHERE p.ContratoId=@contratoId
          AND (@inmobiliariaId IS NULL OR p.InmobiliariaId = @inmobiliariaId)";
    
    using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@contratoId", contratoId);
    AgregarFiltroInmobiliaria(cmd);
    conn.Open();
    using var reader = cmd.ExecuteReader();
    while (reader.Read())
    {
        lista.Add(new Pago
        {
            Id = reader.GetInt32(0),
            ContratoId = reader.GetInt32(1),
            FechaPago = reader.GetDateTime(2),
            Detalle = reader.IsDBNull(3) ? null : reader.GetString(3),
            Importe = reader.GetDecimal(4),
            ContratoDescripcion = reader.IsDBNull(5) ? null : reader.GetString(5)
        });
    }
    return lista;
}

       public Pago? ObtenerPorId(int id)
{
    Pago? pago = null;
    using var conn = new MySqlConnection(_connectionString); // ✅ centralizado
    var sql = @"
        SELECT p.Id, p.ContratoId, p.FechaPago, p.Detalle, p.Importe,
               CONCAT('Contrato #', c.Id, ' - Inmueble ', c.InmuebleID,
                      ' - ', DATE_FORMAT(c.FechaInicio,'%d/%m/%Y'),
                      ' a ', DATE_FORMAT(c.FechaFin,'%d/%m/%Y'),
                      ' ($', c.MontoMensual, ')') AS ContratoDescripcion,
               p.CreadoPor, u1.Id, u1.Nombre, u1.Apellido,
               p.AnuladoPor, u2.Id, u2.Nombre, u2.Apellido
        FROM pagos p
        INNER JOIN contratos c ON p.ContratoId = c.Id
        LEFT JOIN usuarios u1 ON p.CreadoPor = u1.Id
        LEFT JOIN usuarios u2 ON p.AnuladoPor = u2.Id
        WHERE p.Id=@id
          AND (@inmobiliariaId IS NULL OR p.InmobiliariaId = @inmobiliariaId)";
    
    using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@id", id);
    AgregarFiltroInmobiliaria(cmd);
    conn.Open();
    using var reader = cmd.ExecuteReader();
    if (reader.Read())
    {
        pago = new Pago
        {
            Id = reader.GetInt32(0),
            ContratoId = reader.GetInt32(1),
            FechaPago = reader.GetDateTime(2),
            Detalle = reader.IsDBNull(3) ? null : reader.GetString(3),
            Importe = reader.GetDecimal(4),
            ContratoDescripcion = reader.IsDBNull(5) ? null : reader.GetString(5),
            CreadoPor = reader.GetInt32(6),
            UsuarioCreador = reader.IsDBNull(7) ? null : new Usuario
            {
                Id = reader.GetInt32(7),
                Nombre = reader.IsDBNull(8) ? "" : reader.GetString(8),
                Apellido = reader.IsDBNull(9) ? "" : reader.GetString(9)
            },
            AnuladoPor = reader.IsDBNull(10) ? null : reader.GetInt32(10),
            UsuarioAnulador = reader.IsDBNull(11) ? null : new Usuario
            {
                Id = reader.GetInt32(11),
                Nombre = reader.IsDBNull(12) ? "" : reader.GetString(12),
                Apellido = reader.IsDBNull(13) ? "" : reader.GetString(13)
            }
        };
    }
    return pago;
}

        public int Alta(Pago pago)
{
    try
    {
        using var conn = new MySqlConnection(_connectionString);
        const string sql = @"
            INSERT INTO pagos (ContratoId, FechaPago, Detalle, Importe, CreadoPor, InmobiliariaId)
            VALUES (@contratoId, @fechaPago, @detalle, @importe, @creadoPor,
                    COALESCE(@inmobiliariaId, (SELECT InmobiliariaId FROM contratos WHERE Id = @contratoId)));
            SELECT LAST_INSERT_ID();";

        using var cmd = new MySqlCommand(sql, conn);
        cmd.Parameters.AddWithValue("@contratoId", pago.ContratoId);
        cmd.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
        cmd.Parameters.AddWithValue("@detalle", pago.Detalle ?? (object)DBNull.Value);
        cmd.Parameters.AddWithValue("@importe", pago.Importe);
        cmd.Parameters.AddWithValue("@creadoPor", pago.CreadoPor);
        AgregarFiltroInmobiliaria(cmd);

        conn.Open();
        var result = cmd.ExecuteScalar(); // ✅ devuelve el ID generado, no bloquea
        conn.Close();

        return Convert.ToInt32(result);
    }
    catch (MySqlException ex)
    {
        Console.WriteLine($"❌ Error MySQL al insertar pago: {ex.Message}");
        throw;
    }
    catch (Exception ex)
    {
        Console.WriteLine($"⚠️ Error general en Alta(Pago): {ex.Message}");
        throw;
    }
}

        public int Modificacion(Pago pago)
{
    using var conn = new MySqlConnection(_connectionString); // ✅ centralizado
    var sql = @"UPDATE pagos SET ContratoId=@contratoId, FechaPago=@fechaPago, 
                Detalle=@detalle, Importe=@importe 
                WHERE Id=@id AND (@inmobiliariaId IS NULL OR InmobiliariaId = @inmobiliariaId)";
    using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@id", pago.Id);
    cmd.Parameters.AddWithValue("@contratoId", pago.ContratoId);
    cmd.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
    cmd.Parameters.AddWithValue("@detalle", pago.Detalle ?? (object)DBNull.Value);
    cmd.Parameters.AddWithValue("@importe", pago.Importe);
    AgregarFiltroInmobiliaria(cmd);
    conn.Open();
    return cmd.ExecuteNonQuery();
}

public int Baja(int id, int anuladoPorId)
{
    using var conn = new MySqlConnection(_connectionString); // ✅ centralizado
    var sql = @"UPDATE pagos 
                SET AnuladoPorId=@anuladoPorId 
                WHERE Id=@id AND (@inmobiliariaId IS NULL OR InmobiliariaId = @inmobiliariaId)";
    using var cmd = new MySqlCommand(sql, conn);
    cmd.Parameters.AddWithValue("@id", id);
    cmd.Parameters.AddWithValue("@anuladoPorId", anuladoPorId);
    AgregarFiltroInmobiliaria(cmd);
    conn.Open();
    return cmd.ExecuteNonQuery();
}


    }
}
