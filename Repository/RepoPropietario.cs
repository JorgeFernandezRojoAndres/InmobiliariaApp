using MySqlConnector;
using System.Collections.Generic;
using InmobiliariaApp.Models;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoPropietario
    {
        private readonly string _connectionString;

        public RepoPropietario(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("Cadena de conexión no encontrada.");
        }

        public List<Propietario> ObtenerTodos()
        {
            var lista = new List<Propietario>();
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "SELECT Id, Nombre, Apellido, DNI, Email, Telefono FROM Propietarios";
            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                lista.Add(new Propietario
                {
                    Id = reader.GetInt32("Id"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Documento = reader.GetString("DNI"),
                    Email = reader.GetString("Email"),
                    Telefono = reader.GetString("Telefono")
                });
            }
            return lista;
        }

        // Alta, Modificar, Eliminar y ObtenerPorId → igual que en RepoPersona
    }
}
