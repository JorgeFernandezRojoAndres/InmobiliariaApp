using MySqlConnector;
using InmobiliariaApp.Models;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoInmueble
    {
        private readonly string _connectionString;

        // Recibe IConfiguration para leer la cadena desde appsettings.json
        public RepoInmueble(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public int Alta(Inmueble i)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"INSERT INTO Inmuebles 
                                 (Direccion, MetrosCuadrados, Precio, PropietarioID, Activo)
                                 VALUES (@dir, @m2, @precio, @prop, @activo);
                                 SELECT LAST_INSERT_ID();";

            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dir", i.Direccion);
            command.Parameters.AddWithValue("@m2", i.MetrosCuadrados);
            command.Parameters.AddWithValue("@precio", i.Precio);
            command.Parameters.AddWithValue("@prop", i.PropietarioId);
            command.Parameters.AddWithValue("@activo", i.Activo);

            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }
        public Inmueble? Obtener(int id)
{
    using var connection = new MySqlConnection(_connectionString);
    const string sql = @"
        SELECT i.ID, i.Direccion, i.MetrosCuadrados, i.Precio,
               i.PropietarioID, i.Activo,
               p.Nombre AS NombrePropietario
        FROM Inmuebles i
        JOIN Personas p ON p.ID = i.PropietarioID
        WHERE i.ID = @id";

    using var command = new MySqlCommand(sql, connection);
    command.Parameters.AddWithValue("@id", id);

    connection.Open();
    using var reader = command.ExecuteReader();

    if (reader.Read())
    {
        return new Inmueble
        {
            Id = reader.GetInt32("ID"),
            Direccion = reader.GetString("Direccion"),
            MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
            Precio = reader.GetDecimal("Precio"),
            PropietarioId = reader.GetInt32("PropietarioID"),
            NombrePropietario = reader.GetString("NombrePropietario"),
            Activo = reader.GetBoolean("Activo")
        };
    }
    return null;
}

        public List<Inmueble> Obtener(bool incluirInactivos = false)
        {
            var lista = new List<Inmueble>();
            using var connection = new MySqlConnection(_connectionString);
            string sql = @"
        SELECT i.ID, i.Direccion, i.MetrosCuadrados, i.Precio, 
               p.Nombre AS NombrePropietario, i.PropietarioID, i.Activo
        FROM Inmuebles i
        JOIN Personas p ON i.PropietarioID = p.ID";

            if (!incluirInactivos)
                sql += " WHERE i.Activo = 1";

            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Inmueble
                {
                    Id = reader.GetInt32("ID"),
                    Direccion = reader.GetString("Direccion"),
                    MetrosCuadrados = reader.GetInt32("MetrosCuadrados"),
                    Precio = reader.GetDecimal("Precio"),
                    PropietarioId = reader.GetInt32("PropietarioID"),
                    NombrePropietario = reader.GetString("NombrePropietario"),
                    Activo = reader.GetBoolean("Activo")
                });
            }
            return lista;
        }


        public void Modificar(Inmueble i)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"UPDATE Inmuebles 
                                 SET Direccion=@dir, MetrosCuadrados=@m2, Precio=@precio, 
                                     PropietarioID=@prop, Activo=@activo
                                 WHERE ID=@id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@dir", i.Direccion);
            command.Parameters.AddWithValue("@m2", i.MetrosCuadrados);
            command.Parameters.AddWithValue("@precio", i.Precio);
            command.Parameters.AddWithValue("@prop", i.PropietarioId);
            command.Parameters.AddWithValue("@activo", i.Activo);
            command.Parameters.AddWithValue("@id", i.Id);

            connection.Open();
            command.ExecuteNonQuery();
        }

        public void BajaLogica(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "UPDATE Inmuebles SET Activo = 0 WHERE ID = @id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);

            connection.Open();
            command.ExecuteNonQuery();
        }
    }
}
