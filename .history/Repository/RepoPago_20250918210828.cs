using MySql.Data.MySqlClient;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public class RepoPago : IRepoPago
    {
        public class RepoContrato : IRepoContrato
{
    private readonly string connectionString = "server=localhost;user=root;password=jorge007;database=mi_base_datos;";

        public List<Pago> ObtenerTodos()
        {
            var lista = new List<Pago>();
            using var conn = new MySqlConnection(connectionString);
            var sql = @"
                SELECT p.Id, p.ContratoId, p.FechaPago, p.Detalle, p.Importe,
                       CONCAT('Contrato #', c.Id, ' - Inmueble ', c.InmuebleID,
                              ' - ', DATE_FORMAT(c.FechaInicio,'%d/%m/%Y'),
                              ' a ', DATE_FORMAT(c.FechaFin,'%d/%m/%Y'),
                              ' ($', c.MontoMensual, ')') AS ContratoDescripcion
                FROM pagos p
                INNER JOIN contratos c ON p.ContratoId = c.Id";
            using var cmd = new MySqlCommand(sql, conn);
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
            using var conn = new MySqlConnection(connectionString);
            var sql = @"
                SELECT p.Id, p.ContratoId, p.FechaPago, p.Detalle, p.Importe,
                       CONCAT('Contrato #', c.Id, ' - Inmueble ', c.InmuebleID,
                              ' - ', DATE_FORMAT(c.FechaInicio,'%d/%m/%Y'),
                              ' a ', DATE_FORMAT(c.FechaFin,'%d/%m/%Y'),
                              ' ($', c.MontoMensual, ')') AS ContratoDescripcion
                FROM pagos p
                INNER JOIN contratos c ON p.ContratoId = c.Id
                WHERE p.ContratoId=@contratoId";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@contratoId", contratoId);
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
            using var conn = new MySqlConnection(connectionString);
            var sql = @"
                SELECT p.Id, p.ContratoId, p.FechaPago, p.Detalle, p.Importe,
                       CONCAT('Contrato #', c.Id, ' - Inmueble ', c.InmuebleID,
                              ' - ', DATE_FORMAT(c.FechaInicio,'%d/%m/%Y'),
                              ' a ', DATE_FORMAT(c.FechaFin,'%d/%m/%Y'),
                              ' ($', c.MontoMensual, ')') AS ContratoDescripcion
                FROM pagos p
                INNER JOIN contratos c ON p.ContratoId = c.Id
                WHERE p.Id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
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
                    ContratoDescripcion = reader.IsDBNull(5) ? null : reader.GetString(5)
                };
            }
            return pago;
        }

        public int Alta(Pago pago)
        {
            using var conn = new MySqlConnection(connectionString);
            var sql = @"INSERT INTO pagos (ContratoId, FechaPago, Detalle, Importe, CreadoPor) 
            VALUES (@contratoId, @fechaPago, @detalle, @importe, @creadoPor)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@contratoId", pago.ContratoId);
            cmd.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
            cmd.Parameters.AddWithValue("@detalle", pago.Detalle ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@importe", pago.Importe);
            cmd.Parameters.AddWithValue("@creadoPor", pago.CreadoPor);

            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Modificacion(Pago pago)
        {
            using var conn = new MySqlConnection(connectionString);
            var sql = @"UPDATE pagos SET ContratoId=@contratoId, FechaPago=@fechaPago, 
                        Detalle=@detalle, Importe=@importe WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", pago.Id);
            cmd.Parameters.AddWithValue("@contratoId", pago.ContratoId);
            cmd.Parameters.AddWithValue("@fechaPago", pago.FechaPago);
            cmd.Parameters.AddWithValue("@detalle", pago.Detalle ?? (object)DBNull.Value);
            cmd.Parameters.AddWithValue("@importe", pago.Importe);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Baja(int id)
        {
            using var conn = new MySqlConnection(connectionString);
            var sql = "DELETE FROM pagos WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}
