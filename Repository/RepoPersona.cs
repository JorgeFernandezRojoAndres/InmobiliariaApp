using MySqlConnector; 
using System.Collections.Generic;
using InmobiliariaApp.Models;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoPersona
    {
        private readonly string _connectionString;

        public RepoPersona(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public List<Persona> ObtenerTodos()
        {
            var lista = new List<Persona>();
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "SELECT ID, Nombre, Apellido, DNI, Email, Tipo FROM Personas";
            using var command = new MySqlCommand(sql, connection);
            connection.Open();
            using var reader = command.ExecuteReader();

            while (reader.Read())
            {
                lista.Add(new Persona
                {
                    Id = reader.GetInt32("ID"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Documento = reader.GetString("DNI"),
                    Email = reader.GetString("Email"),
                    Tipo = reader.GetString("Tipo")  // 🔹 nuevo
                });
            }
            return lista;
        }

        public Persona? ObtenerPorId(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "SELECT ID, Nombre, Apellido, DNI, Email, Tipo FROM Personas WHERE ID=@id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            using var reader = command.ExecuteReader();
            if (reader.Read())
            {
                return new Persona
                {
                    Id = reader.GetInt32("ID"),
                    Nombre = reader.GetString("Nombre"),
                    Apellido = reader.GetString("Apellido"),
                    Documento = reader.GetString("DNI"),
                    Email = reader.GetString("Email"),
                    Tipo = reader.GetString("Tipo")  // 🔹 nuevo
                };
            }
            return null;
        }

        public void Alta(Persona p)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"INSERT INTO Personas (Nombre, Apellido, DNI, Email, Tipo) 
                                 VALUES (@nombre, @apellido, @dni, @correo, @tipo)";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@nombre", p.Nombre);
            command.Parameters.AddWithValue("@apellido", p.Apellido);
            command.Parameters.AddWithValue("@dni", p.Documento);
            command.Parameters.AddWithValue("@correo", p.Email);
            command.Parameters.AddWithValue("@tipo", p.Tipo); // 🔹 nuevo
            connection.Open();
            command.ExecuteNonQuery();
            p.Id = (int)command.LastInsertedId;
        }

        public void Modificar(Persona p)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"UPDATE Personas SET 
                                    Nombre=@nombre, 
                                    Apellido=@apellido, 
                                    DNI=@dni, 
                                    Email=@correo,
                                    Tipo=@tipo
                                 WHERE ID=@id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@nombre", p.Nombre);
            command.Parameters.AddWithValue("@apellido", p.Apellido);
            command.Parameters.AddWithValue("@dni", p.Documento);
            command.Parameters.AddWithValue("@correo", p.Email);
            command.Parameters.AddWithValue("@tipo", p.Tipo); // 🔹 nuevo
            command.Parameters.AddWithValue("@id", p.Id);
            connection.Open();
            command.ExecuteNonQuery();
        }

        public void Eliminar(int id)
        {
            using var connection = new MySqlConnection(_connectionString);
            const string sql = "DELETE FROM Personas WHERE ID=@id";
            using var command = new MySqlCommand(sql, connection);
            command.Parameters.AddWithValue("@id", id);
            connection.Open();
            command.ExecuteNonQuery();
        }

        // 🔹 Métodos de ayuda para filtrar
        public List<Persona> ObtenerInquilinos()
        {
            return ObtenerTodos().Where(p => p.Tipo == "Inquilino").ToList();
        }

        public List<Persona> ObtenerPropietarios()
        {
            return ObtenerTodos().Where(p => p.Tipo == "Propietario").ToList();
        }
    }
}
