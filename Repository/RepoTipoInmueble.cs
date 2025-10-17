using MySql.Data.MySqlClient;
using InmobiliariaApp.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace InmobiliariaApp.Repository
{
    public class RepoTipoInmueble : IRepoTipoInmueble
    {
        private readonly string _connectionString;

        public RepoTipoInmueble(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        // ===============================
        // 🔹 Métodos síncronos (para MVC)
        // ===============================
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

        // ===============================
        // 🔹 Métodos asíncronos (para API)
        // ===============================
        public async Task<List<TipoInmueble>> ObtenerTodosAsync()
        {
            var lista = new List<TipoInmueble>();
            await using var conn = new MySqlConnection(_connectionString);
            var sql = "SELECT Id, Nombre FROM tipos_inmuebles";
            await using var cmd = new MySqlCommand(sql, conn);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                lista.Add(new TipoInmueble
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1)
                });
            }
            return lista;
        }

        public async Task<TipoInmueble?> ObtenerPorIdAsync(int id)
        {
            TipoInmueble? tipo = null;
            await using var conn = new MySqlConnection(_connectionString);
            var sql = "SELECT Id, Nombre FROM tipos_inmuebles WHERE Id=@id";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            await using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                tipo = new TipoInmueble
                {
                    Id = reader.GetInt32(0),
                    Nombre = reader.GetString(1)
                };
            }
            return tipo;
        }

        public async Task<int> AltaAsync(TipoInmueble tipo)
        {
            await using var conn = new MySqlConnection(_connectionString);
            var sql = "INSERT INTO tipos_inmuebles (Nombre) VALUES (@nombre)";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@nombre", tipo.Nombre);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> ModificacionAsync(TipoInmueble tipo)
        {
            await using var conn = new MySqlConnection(_connectionString);
            var sql = "UPDATE tipos_inmuebles SET Nombre=@nombre WHERE Id=@id";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", tipo.Id);
            cmd.Parameters.AddWithValue("@nombre", tipo.Nombre);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }

        public async Task<int> BajaAsync(int id)
        {
            await using var conn = new MySqlConnection(_connectionString);
            var sql = "DELETE FROM tipos_inmuebles WHERE Id=@id";
            await using var cmd = new MySqlCommand(sql, conn);
            cmd.Parameters.AddWithValue("@id", id);
            await conn.OpenAsync();
            return await cmd.ExecuteNonQueryAsync();
        }
    }
}
