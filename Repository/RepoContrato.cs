using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public class RepoContrato : IRepoContrato
    {
        private readonly string connectionString = "server=localhost;user=root;password=jorge007;database=mi_base_datos;";

        public IList<Contrato> ObtenerTodos()
        {
            var lista = new List<Contrato>();

            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = @"SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                                   i.ID as InmuebleID, i.Direccion, i.Tipo, i.Precio,
                                   p.ID as InquilinoID, p.Nombre, p.Apellido, p.DNI
                            FROM contratos c
                            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
                            INNER JOIN personas p ON c.InquilinoID = p.ID;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        var contrato = new Contrato
                        {
                            Id = reader.GetInt32("Id"),
                            FechaInicio = reader.GetDateTime("FechaInicio"),
                            FechaFin = reader.GetDateTime("FechaFin"),
                            MontoMensual = reader.GetDecimal("MontoMensual"),
                            Estado = reader.GetString("Estado"),
                            Inmueble = new Inmueble
                            {
                                Id = reader.GetInt32("InmuebleID"),
                                Direccion = reader.GetString("Direccion"),
                                Tipo = reader.GetString("Tipo"),
                                Precio = reader.GetDecimal("Precio")
                            },
                            Inquilino = new Inquilino
                            {
                                Id = reader.GetInt32("InquilinoID"),
                                Nombre = reader.GetString("Nombre"),
                                Apellido = reader.GetString("Apellido"),
                                Documento = reader.GetString("DNI")
                            }
                        };

                        // 🔹 Si ya venció, lo actualizamos automáticamente
                        if (contrato.FechaFin < DateTime.Now && contrato.Estado == "Vigente")
                        {
                            MarcarComoVencido(contrato.Id);
                            contrato.Estado = "Vencido";
                        }

                        lista.Add(contrato);
                    }
                }
            }

            return lista;
        }

        public Contrato? ObtenerPorId(int id)
        {
            Contrato? contrato = null;

            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = @"SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                                   i.ID as InmuebleID, i.Direccion, i.Tipo, i.Precio,
                                   p.ID as InquilinoID, p.Nombre, p.Apellido, p.DNI
                            FROM contratos c
                            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
                            INNER JOIN personas p ON c.InquilinoID = p.ID
                            WHERE c.Id = @id";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    if (reader.Read())
                    {
                        contrato = new Contrato
                        {
                            Id = reader.GetInt32("Id"),
                            FechaInicio = reader.GetDateTime("FechaInicio"),
                            FechaFin = reader.GetDateTime("FechaFin"),
                            MontoMensual = reader.GetDecimal("MontoMensual"),
                            Estado = reader.GetString("Estado"),
                            IdInquilino = reader.GetInt32("InquilinoID"),
                            IdInmueble = reader.GetInt32("InmuebleID"),
                            Inmueble = new Inmueble
                            {
                                Id = reader.GetInt32("InmuebleID"),
                                Direccion = reader.GetString("Direccion"),
                                Tipo = reader.GetString("Tipo"),
                                Precio = reader.GetDecimal("Precio")
                            },
                            Inquilino = new Inquilino
                            {
                                Id = reader.GetInt32("InquilinoID"),
                                Nombre = reader.GetString("Nombre"),
                                Apellido = reader.GetString("Apellido"),
                                Documento = reader.GetString("DNI")
                            }
                        };

                        // 🔹 Si venció, actualizar en BD
                        if (contrato.FechaFin < DateTime.Now && contrato.Estado == "Vigente")
                        {
                            MarcarComoVencido(contrato.Id);
                            contrato.Estado = "Vencido";
                        }
                    }
                }
            }

            return contrato;
        }

        public int Crear(Contrato contrato)
{
    if (contrato.IdInquilino == 0 || contrato.IdInmueble == 0)
        throw new ArgumentException("Debe seleccionar un inquilino y un inmueble válidos.");

    using (var connection = new MySqlConnection(connectionString))
    {
        var sql = @"INSERT INTO contratos (InquilinoID, InmuebleID, FechaInicio, FechaFin, MontoMensual, Estado)
                    VALUES (@inquilino, @inmueble, @inicio, @fin, @monto, @estado);
                    SELECT LAST_INSERT_ID();";

        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@inquilino", contrato.IdInquilino);
            command.Parameters.AddWithValue("@inmueble", contrato.IdInmueble);
            command.Parameters.AddWithValue("@inicio", contrato.FechaInicio);
            command.Parameters.AddWithValue("@fin", contrato.FechaFin);
            command.Parameters.AddWithValue("@monto", contrato.MontoMensual);
            command.Parameters.AddWithValue("@estado", contrato.Estado);

            connection.Open();
            var res = Convert.ToInt32(command.ExecuteScalar());
            contrato.Id = res;
            return res;
        }
    }
}


        public int MarcarComoVencido(int id)
        {
            int res = -1;
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = "UPDATE contratos SET Estado = 'Vencido' WHERE Id = @id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }

        public int Editar(Contrato contrato)
        {
            if (contrato.IdInquilino == 0 || contrato.IdInmueble == 0)
                throw new ArgumentException("Debe seleccionar un inquilino y un inmueble válidos.");

            int res;
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = @"UPDATE contratos 
                            SET InquilinoID=@inquilino, InmuebleID=@inmueble, FechaInicio=@inicio, FechaFin=@fin,
                                MontoMensual=@monto, Estado=@estado
                            WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inquilino", contrato.IdInquilino);
                    command.Parameters.AddWithValue("@inmueble", contrato.IdInmueble);
                    command.Parameters.AddWithValue("@inicio", contrato.FechaInicio);
                    command.Parameters.AddWithValue("@fin", contrato.FechaFin);
                    command.Parameters.AddWithValue("@monto", contrato.MontoMensual);
                    command.Parameters.AddWithValue("@estado", contrato.Estado);
                    command.Parameters.AddWithValue("@id", contrato.Id);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }

            return res;
        }

        public int Eliminar(int id)
        {
            // 🔹 Si está vencido lo dejamos en BD, si no venció, se elimina
            using (var contrato = new MySqlConnection(connectionString))
            {
                contrato.Open();
                var checkSql = "SELECT FechaFin FROM contratos WHERE Id=@id";
                using (var checkCmd = new MySqlCommand(checkSql, contrato))
                {
                    checkCmd.Parameters.AddWithValue("@id", id);
                    var fechaFin = checkCmd.ExecuteScalar();

                    if (fechaFin != null && Convert.ToDateTime(fechaFin) < DateTime.Now)
                    {
                        return MarcarComoVencido(id);
                    }
                }
            }

            int res;
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = "DELETE FROM contratos WHERE Id=@id";
                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@id", id);
                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }
    }
}
