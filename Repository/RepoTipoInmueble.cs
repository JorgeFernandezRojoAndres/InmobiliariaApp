using MySql.Data.MySqlClient;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public class RepoTipoInmueble : IRepoTipoInmueble
    {
        private readonly string _connectionString;

        // 🔹 Constructor para inyección de dependencias
        public RepoTipoInmueble(string connectionString)
        {
            _connectionString = connectionString;
        }

        public List<TipoInmueble> ObtenerTodos()
        {
            var lista = new List<TipoInmueble>();
            using var conn = new MySqlConnection(_connectionString);
            var sql = "SELECT Id, Nombre FROM tipos_inmuebles";
            using var cmd = new MySqlCommand(sql, conn);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new TipoInmueble
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1)
                });
            }
            return lista;
        }

        public TipoInmueble? ObtenerPorId(int id)
        {
            TipoInmueble? tipo = null;
            using var conn = new MySqlConnection(_connectionString);
            var sql = "SELECT Id, Nombre FROM tipos_inmuebles WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            using var reader = cmd.ExecuteReader();
            if (reader.Read())
            {
                tipo = new TipoInmueble
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1)
                };
            }
            return tipo;
        }

        public int Alta(TipoInmueble tipo)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = "INSERT INTO tipos_inmuebles (Nombre) VALUES (@nombre)";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nombre", tipo.Nombre);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Modificacion(TipoInmueble tipo)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = "UPDATE tipos_inmuebles SET Nombre=@nombre WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", tipo.Id);
            cmd.Parameters.AddWithValue("@nombre", tipo.Nombre);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }

        public int Baja(int id)
        {
            using var conn = new MySqlConnection(_connectionString);
            var sql = "DELETE FROM tipos_inmuebles WHERE Id=@id";
            using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            conn.Open();
            return cmd.ExecuteNonQuery();
        }
    }
}
