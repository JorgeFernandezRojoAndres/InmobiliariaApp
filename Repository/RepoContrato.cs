using System;
using System.Collections.Generic;
using MySql.Data.MySqlClient;
using InmobiliariaApp.Models;
using Microsoft.Extensions.Configuration;

namespace InmobiliariaApp.Repository
{
    public class RepoContrato : IRepoContrato
    {
        private readonly string _connectionString;

        // 🔹 Ahora recibe IConfiguration en el constructor
        public RepoContrato(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontró la cadena de conexión 'DefaultConnection'.");
        }

        public IList<Contrato> ObtenerPorInmueble(int inmuebleId)
        {
            var lista = new List<Contrato>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                var sql = @"
            SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                   i.ID as InmuebleID, i.Direccion, i.Precio,
                   t.Nombre AS TipoNombre,
                   p.ID as InquilinoID, p.Nombre, p.Apellido, p.DNI
            FROM contratos c
            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
            INNER JOIN tipos_inmuebles t ON i.TipoId = t.Id
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
                                TipoNombre = reader.GetString("TipoNombre"), // ✅ ahora correcto
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
            using var connection = new MySqlConnection(_connectionString);
            const string sql = @"
        SELECT 
            c.Id, c.InquilinoId, c.InmuebleId, c.FechaInicio, c.FechaFin, 
            c.MontoMensual, c.Estado,
            i.Id AS InmuebleId, i.Direccion, i.PropietarioId, i.Precio,
            ti.Nombre AS TipoNombre,
            p.Nombre AS NombrePropietario, p.Apellido AS ApellidoPropietario,
            inq.Id AS IdInquilino, inq.Nombre AS NombreInquilino, inq.Apellido AS ApellidoInquilino
        FROM contratos c
        INNER JOIN inmuebles i ON c.InmuebleId = i.Id
        INNER JOIN tipos_inmuebles ti ON i.TipoId = ti.Id
        INNER JOIN personas p ON i.PropietarioId = p.Id
        INNER JOIN personas inq ON c.InquilinoId = inq.Id;";

            using var command = new MySqlCommand(sql, connection);
            connection.Open();

            using var reader = command.ExecuteReader();
            while (reader.Read())
            {
                var inmueble = new Inmueble
                {
                    Id = reader.GetInt32("InmuebleId"),
                    Direccion = reader.GetString("Direccion"),
                    PropietarioId = reader.GetInt32("PropietarioId"),
                    Precio = reader.GetDecimal("Precio"),
                    TipoNombre = reader.GetString("TipoNombre"),
                    NombrePropietario = $"{reader.GetString("NombrePropietario")} {reader.GetString("ApellidoPropietario")}"
                };

                var inquilino = new Inquilino
                {
                    Id = reader.GetInt32("IdInquilino"),
                    Nombre = reader.GetString("NombreInquilino"),
                    Apellido = reader.GetString("ApellidoInquilino")
                };

                var contrato = new Contrato
                {
                    Id = reader.GetInt32("Id"),
                    IdInquilino = reader.GetInt32("InquilinoId"),
                    IdInmueble = reader.GetInt32("InmuebleId"),
                    FechaInicio = reader.GetDateTime("FechaInicio"),
                    FechaFin = reader.GetDateTime("FechaFin"),
                    MontoMensual = reader.GetDecimal("MontoMensual"),
                    Estado = reader.GetString("Estado"),
                    Inmueble = inmueble,
                    Inquilino = inquilino
                };

                lista.Add(contrato);
            }

            return lista;
        }


        public Contrato? ObtenerPorId(int id)
        {
            Contrato? contrato = null;

            using (var connection = new MySqlConnection(_connectionString))
            {
                var sql = @"
            SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                   i.ID as InmuebleID, i.Direccion, i.Precio,
                   t.Nombre AS TipoNombre,
                   p.ID as InquilinoID, p.Nombre, p.Apellido, p.DNI,
                   c.CreadoPor, c.TerminadoPor,
                   u1.Id AS CreadorId, u1.Nombre AS CreadorNombre, u1.Apellido AS CreadorApellido,
                   u2.Id AS TerminadorId, u2.Nombre AS TerminadorNombre, u2.Apellido AS TerminadorApellido
            FROM contratos c
            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
            INNER JOIN tipos_inmuebles t ON i.TipoId = t.Id
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
                                TipoNombre = reader.GetString("TipoNombre"), // ✅ ahora correcto
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
                                    Nombre = reader["CreadorNombre"]?.ToString() ?? "",
                                    Apellido = reader["CreadorApellido"]?.ToString() ?? ""
                                }
                                : null,

                            UsuarioTerminador = reader["TerminadorId"] != DBNull.Value
                                ? new Usuario
                                {
                                    Id = Convert.ToInt32(reader["TerminadorId"]),
                                    Nombre = reader["TerminadorNombre"]?.ToString() ?? "",
                                    Apellido = reader["TerminadorApellido"]?.ToString() ?? ""
                                }
                                : null
                        };

                        // 🔹 Marcar como vencido si corresponde
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

            using (var connection = new MySqlConnection(_connectionString))
            {
                var sql = @"
            INSERT INTO contratos 
                (InquilinoID, InmuebleID, FechaInicio, FechaFin, MontoMensual, Estado, CreadoPor)
            VALUES 
                (@inquilino, @inmueble, @inicio, @fin, @monto, @estado, @creadoPor);
            SELECT LAST_INSERT_ID();";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@inquilino", contrato.IdInquilino);
                    command.Parameters.AddWithValue("@inmueble", contrato.IdInmueble);
                    command.Parameters.AddWithValue("@inicio", contrato.FechaInicio);
                    command.Parameters.AddWithValue("@fin", contrato.FechaFin);
                    command.Parameters.AddWithValue("@monto", contrato.MontoMensual);
                    command.Parameters.AddWithValue("@estado", contrato.Estado);
                    command.Parameters.AddWithValue("@creadoPor", contrato.CreadoPor);

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

            using (var connection = new MySqlConnection(_connectionString))
            {
                var sql = @"
            SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                   i.ID as InmuebleID, i.Direccion, i.Precio,
                   t.Nombre AS TipoNombre,
                   p.ID as InquilinoID, p.Nombre, p.Apellido, p.DNI
            FROM contratos c
            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
            INNER JOIN tipos_inmuebles t ON i.TipoId = t.Id
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
                                TipoNombre = reader.GetString("TipoNombre"), // ✅ ahora correcto
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
            using (var connection = new MySqlConnection(_connectionString))
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
            using (var connection = new MySqlConnection(_connectionString))
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
            using (var connection = new MySqlConnection(_connectionString))
            {
                connection.Open();
                var checkSql = "SELECT FechaFin FROM contratos WHERE Id=@id";
                using (var checkCmd = new MySqlCommand(checkSql, connection))
                {
                    checkCmd.Parameters.AddWithValue("@id", idContrato);
                    var fechaFin = checkCmd.ExecuteScalar();

                    if (fechaFin != null && Convert.ToDateTime(fechaFin) < DateTime.Now)
                    {
                        return MarcarComoVencido(idContrato, idUsuario);
                    }
                }
            }

            int res;
            using (var connection = new MySqlConnection(_connectionString))
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
        // 🔹 NUEVO MÉTODO para devolver los pagos asociados a un contrato (ajustado al modelo actual)
        public IList<Pago> ObtenerPagosPorContrato(int contratoId)
        {
            var lista = new List<Pago>();

            using (var connection = new MySqlConnection(_connectionString))
            {
                var sql = @"
            SELECT p.Id, p.ContratoId, p.FechaPago, p.Detalle, p.Importe
            FROM pagos p
            WHERE p.ContratoId = @contratoId
            ORDER BY p.FechaPago DESC;";

                using (var command = new MySqlCommand(sql, connection))
                {
                    command.Parameters.AddWithValue("@contratoId", contratoId);
                    connection.Open();

                    using (var reader = command.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            lista.Add(new Pago
                            {
                                Id = reader.GetInt32("Id"),
                                ContratoId = reader.GetInt32("ContratoId"), // ✅ nombre correcto según tu modelo
                                FechaPago = reader.GetDateTime("FechaPago"),
                                Detalle = reader["Detalle"] != DBNull.Value ? reader.GetString("Detalle") : null,
                                Importe = reader.GetDecimal("Importe")
                            });
                        }
                    }
                }
            }

            return lista;
        }

        // 🔹 NUEVO MÉTODO PARA API (Android)
       public IList<Contrato> ObtenerVigentesPorPropietario(int idPropietario)
{
    var lista = new List<Contrato>();

    using (var connection = new MySqlConnection(_connectionString))
    {
        var sql = @"
            SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                   i.ID AS InmuebleID, i.Direccion, i.Precio,
                   t.Nombre AS TipoNombre,
                   p.ID AS InquilinoID, p.Nombre, p.Apellido, p.DNI
            FROM contratos c
            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
            INNER JOIN tipos_inmuebles t ON i.TipoId = t.Id
            INNER JOIN personas p ON c.InquilinoID = p.ID
            WHERE UPPER(c.Estado) = 'VIGENTE'
              AND i.PropietarioID = @idPropietario
            ORDER BY c.FechaInicio DESC;";

        using (var command = new MySqlCommand(sql, connection))
        {
            command.Parameters.AddWithValue("@idPropietario", idPropietario);
            connection.Open();

            using (var reader = command.ExecuteReader())
            {
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
                            TipoNombre = reader.GetString("TipoNombre"),
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
    }

    // ✅ LIMPIEZA: si fecha fin ya pasó, no es vigente
    lista = lista.Where(c => c.FechaFin >= DateTime.Now).ToList();

    return lista;
}

        // ✅ Contratos finalizados por propietario (Android)
public IList<Contrato> ObtenerFinalizadosPorPropietario(int idPropietario)
{
    var lista = new List<Contrato>();

    using (var connection = new MySqlConnection(_connectionString))
    {
        var sql = @"
            SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                   i.ID AS InmuebleID, i.Direccion, i.Precio,
                   t.Nombre AS TipoNombre,
                   p.ID AS InquilinoID, p.Nombre, p.Apellido, p.DNI
            FROM contratos c
            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
            INNER JOIN tipos_inmuebles t ON i.TipoId = t.Id
            INNER JOIN personas p ON c.InquilinoID = p.ID
            WHERE i.PropietarioID = @idPropietario
              AND (
                      UPPER(c.Estado) IN ('FINALIZADO','RESCINDIDO')

                      OR c.FechaFin < CURDATE()
                  )
            ORDER BY c.FechaFin DESC;";

        using (var cmd = new MySqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@idPropietario", idPropietario);
            connection.Open();

            using (var reader = cmd.ExecuteReader())
            {
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
                            TipoNombre = reader.GetString("TipoNombre"),
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
    }

    // ✅ Asegurar coherencia por si el SQL devolvió algo raro
    lista = lista.Where(c =>
        c.FechaFin < DateTime.Now ||
        c.Estado.Equals("Finalizado", StringComparison.OrdinalIgnoreCase) ||
        c.Estado.Equals("Vencido", StringComparison.OrdinalIgnoreCase) ||
        c.Estado.Equals("Rescindido", StringComparison.OrdinalIgnoreCase) ||
        c.Estado.Equals("Renovado", StringComparison.OrdinalIgnoreCase)
    ).ToList();

    return lista;
}


// ✅ Todos los contratos por propietario (Android)
public IList<Contrato> ObtenerTodosPorPropietario(int idPropietario)
{
    var lista = new List<Contrato>();

    using (var connection = new MySqlConnection(_connectionString))
    {
        var sql = @"
            SELECT c.Id, c.FechaInicio, c.FechaFin, c.MontoMensual, c.Estado,
                   i.ID AS InmuebleID, i.Direccion, i.Precio,
                   t.Nombre AS TipoNombre,
                   p.ID AS InquilinoID, p.Nombre, p.Apellido, p.DNI
            FROM contratos c
            INNER JOIN inmuebles i ON c.InmuebleID = i.ID
            INNER JOIN tipos_inmuebles t ON i.TipoId = t.Id
            INNER JOIN personas p ON c.InquilinoID = p.ID
            WHERE i.PropietarioID = @idPropietario
            ORDER BY c.FechaInicio DESC;";

        using (var cmd = new MySqlCommand(sql, connection))
        {
            cmd.Parameters.AddWithValue("@idPropietario", idPropietario);
            connection.Open();

            using (var reader = cmd.ExecuteReader())
            {
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
                            TipoNombre = reader.GetString("TipoNombre"),
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
    }

    return lista;
}
public bool ExisteContratoVigenteParaInmueble(int idInmueble, int idContratoActual)
{
    using var connection = new MySqlConnection(_connectionString);
    var sql = @"
        SELECT COUNT(*) 
        FROM contratos 
        WHERE InmuebleId = @idInmueble
          AND Estado = 'Vigente'
          AND Id != @idContratoActual";

    using var cmd = new MySqlCommand(sql, connection);
    cmd.Parameters.AddWithValue("@idInmueble", idInmueble);
    cmd.Parameters.AddWithValue("@idContratoActual", idContratoActual);

    connection.Open();
    var count = Convert.ToInt32(cmd.ExecuteScalar());

    return count > 0;
}

        public int RenovarContrato(int idContratoOriginal, DateTime nuevaFechaInicio, DateTime nuevaFechaFin, decimal nuevoMonto, int idPropietario)
{
    // 1) Obtener contrato original
    var contratoOriginal = ObtenerPorId(idContratoOriginal);
    if (contratoOriginal == null)
        throw new Exception("Contrato original no encontrado.");

    // 2) Marcar el contrato original como Finalizado
    using (var connection1 = new MySqlConnection(_connectionString))
    {
        var sql1 = @"UPDATE contratos 
                     SET Estado = 'Finalizado', FechaFin = @hoy, TerminadoPor = @prop 
                     WHERE Id = @id";

        using (var cmd = new MySqlCommand(sql1, connection1))
        {
            cmd.Parameters.AddWithValue("@hoy", DateTime.Now);
            cmd.Parameters.AddWithValue("@prop", idPropietario);
            cmd.Parameters.AddWithValue("@id", idContratoOriginal);

            connection1.Open();
            cmd.ExecuteNonQuery();
        }
    }

    // 3) Crear nuevo contrato
    int nuevoId;
    using (var connection2 = new MySqlConnection(_connectionString))
    {
        var sql2 = @"
            INSERT INTO contratos 
                (InquilinoID, InmuebleID, FechaInicio, FechaFin, MontoMensual, Estado, CreadoPor, ContratoOriginalId)
            VALUES 
                (@inquilino, @inmueble, @inicio, @fin, @monto, 'Vigente', @propietario, @original);
            SELECT LAST_INSERT_ID();";

        using (var cmd = new MySqlCommand(sql2, connection2))
        {
            cmd.Parameters.AddWithValue("@inquilino", contratoOriginal.IdInquilino);
            cmd.Parameters.AddWithValue("@inmueble", contratoOriginal.IdInmueble);
            cmd.Parameters.AddWithValue("@inicio", nuevaFechaInicio);
            cmd.Parameters.AddWithValue("@fin", nuevaFechaFin);
            cmd.Parameters.AddWithValue("@monto", nuevoMonto);
            cmd.Parameters.AddWithValue("@propietario", idPropietario);
            cmd.Parameters.AddWithValue("@original", idContratoOriginal);

            connection2.Open();
            nuevoId = Convert.ToInt32(cmd.ExecuteScalar());
        }
    }

    return nuevoId;
}



        private int MarcarComoVencido(int idContrato, int idUsuario)
        {
            int res;
            using (var connection = new MySqlConnection(_connectionString))
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
