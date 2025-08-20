using MySqlConnector;
using System.Collections.Generic;
using InmobiliariaApp.Models;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoPersona
    {
        private readonly string _connectionString;

        // Recibe IConfiguration para obtener la cadena de conexión de appsettings.json
        public RepoPersona(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

     public List<Persona> ObtenerTodos()
{
    var lista = new List<Persona>();

    using var connection = new MySqlConnection(_connectionString);
    const string sql = "SELECT ID, Nombre, Apellido, DNI, Email FROM Personas";

    using var command = new MySqlCommand(sql, connection);
    connection.Open();

    using var reader = command.ExecuteReader();

    while (reader.Read())
    {
        lista.Add(new Persona
        {
            Id = reader.GetInt32(nameof(Persona.Id)),
            Nombre = reader.GetString(nameof(Persona.Nombre)),
            Apellido = reader.GetString(nameof(Persona.Apellido)),
            Documento = reader.GetString(reader.GetOrdinal("DNI")),

            Email = reader.GetString(nameof(Persona.Email))
        });
    }

    return lista;
}


    }
}
