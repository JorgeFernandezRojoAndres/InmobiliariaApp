using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using InmobiliariaApp.Models;

namespace InmobiliariaApp.Repository
{
    public class RepoContrato : IRepoContrato
    {
        private readonly string connectionString = "server=localhost;user=root;password=jorge007;database=mi_base_datos;";
        public IList<Contrato> ObtenerPorInmueble(int inmuebleId)
        {
            var lista = new List<Contrato>();

            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = @"SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                           i.ID as InmuebleID, i.Direccion, i.Tipo, i.Precio,
                           p.ID as InquilinoID, p.Nombre, p.Apellido, p.DNI
                    FROM contratos c
                    INNER JOIN inmuebles i ON c.InmuebleID = i.ID
                    INNER JOIN personas p ON c.InquilinoID = p.ID
                    WHERE i.ID = @inmuebleId";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inmuebleId", inmuebleId);
                    connection.Open();
                    var reader = command.ExecuteReader();
                    while (reader.Read())
                    {
                        lista.Add(new Contrato
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
                        });
                    }
                }
            }

            return lista;
        }

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
                           p.ID as InquilinoID, p.Nombre, p.Apellido, p.DNI,
                           c.CreadoPor, c.TerminadoPor,
                           u1.Id AS CreadorId, u1.Nombre AS CreadorNombre, u1.Apellido AS CreadorApellido,
                           u2.Id AS TerminadorId, u2.Nombre AS TerminadorNombre, u2.Apellido AS TerminadorApellido
                    FROM contratos c
                    INNER JOIN inmuebles i ON c.InmuebleID = i.ID
                    INNER JOIN personas p ON c.InquilinoID = p.ID
                    LEFT JOIN usuarios u1 ON c.CreadoPor = u1.Id
                    LEFT JOIN usuarios u2 ON c.TerminadoPor = u2.Id
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
                            },
                            CreadoPor = reader["CreadoPor"] != DBNull.Value ? Convert.ToInt32(reader["CreadoPor"]) : 0,
                            TerminadoPor = reader["TerminadoPor"] != DBNull.Value ? Convert.ToInt32(reader["TerminadoPor"]) : (int?)null,

                            UsuarioCreador = reader["CreadorId"] != DBNull.Value
                                ? new Usuario
                                {
                                    Id = Convert.ToInt32(reader["CreadorId"]),
                                    Nombre = reader["CreadorNombre"].ToString() ?? "",
                                    Apellido = reader["CreadorApellido"].ToString() ?? ""
                                }
                                : null,

                            UsuarioTerminador = reader["TerminadorId"] != DBNull.Value
                                ? new Usuario
                                {
                                    Id = Convert.ToInt32(reader["TerminadorId"]),
                                    Nombre = reader["TerminadorNombre"].ToString() ?? "",
                                    Apellido = reader["TerminadorApellido"].ToString() ?? ""
                                }
                                : null
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

        public IList<Contrato> ObtenerVigentesEntre(DateTime inicio, DateTime fin)
        {
            var lista = new List<Contrato>();

            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = @"
            SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                   i.ID as InmuebleID, i.Direccion, i.Tipo, i.Precio,
                   p.ID as InquilinoID, p.Nombre, p.Apellido, p.DNI
            FROM contratos c
            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
            INNER JOIN personas p ON c.InquilinoID = p.ID
            WHERE UPPER(c.Estado) = 'VIGENTE'
              AND (
                   (@inicio BETWEEN c.FechaInicio AND c.FechaFin)
                OR (@fin BETWEEN c.FechaInicio AND c.FechaFin)
                OR (c.FechaInicio BETWEEN @inicio AND @fin)
              );";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inicio", inicio);
                    command.Parameters.AddWithValue("@fin", fin);

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

                        lista.Add(contrato);
                    }
                }
            }

            return lista;
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

        public int Eliminar(int idContrato, int idUsuario)
        {
            // 🔹 Verificamos si ya está vencido
            using (var connection = new MySqlConnection(connectionString))
            {
                connection.Open();
                var checkSql = "SELECT FechaFin FROM contratos WHERE Id=@id";
                using (var checkCmd = new MySqlCommand(checkSql, connection))
                {
                    checkCmd.Parameters.AddWithValue("@id", idContrato);
                    var fechaFin = checkCmd.ExecuteScalar();

                    if (fechaFin != null && Convert.ToDateTime(fechaFin) < DateTime.Now)
                    {
                        return MarcarComoVencido(idContrato, idUsuario); // ✅ pasar también el usuario
                    }
                }
            }

            // 🔹 Si todavía está vigente, lo marcamos como Finalizado y registramos quién lo terminó
            int res;
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = @"UPDATE contratos 
                    SET Estado = 'Finalizado', TerminadoPor = @usuario 
                    WHERE Id = @idContrato";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@usuario", idUsuario);
                    command.Parameters.AddWithValue("@idContrato", idContrato);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }
        private int MarcarComoVencido(int idContrato, int idUsuario)
        {
            int res;
            using (var connection = new MySqlConnection(connectionString))
            {
                var sql = @"UPDATE contratos 
                    SET Estado = 'Vencido', TerminadoPor = @usuario 
                    WHERE Id = @idContrato";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@usuario", idUsuario);
                    command.Parameters.AddWithValue("@idContrato", idContrato);

                    connection.Open();
                    res = command.ExecuteNonQuery();
                }
            }
            return res;
        }   

    }
}
