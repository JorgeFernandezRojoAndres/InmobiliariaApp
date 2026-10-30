-- MySQL 8.0 - Inicialización de la base de datos InmobiliariaApp
-- Se ejecuta automáticamente al crear el contenedor por primera vez
-- Referencia: Archivos diagramas bas de datos/mi_base_datos.sql

CREATE DATABASE IF NOT EXISTS mi_base_datos
  DEFAULT CHARACTER SET utf8mb4
  DEFAULT COLLATE utf8mb4_unicode_ci;

USE mi_base_datos;

-- ============================================================
-- Tabla: roles
-- ============================================================
DROP TABLE IF EXISTS persona_roles;
DROP TABLE IF EXISTS pagos;
DROP TABLE IF EXISTS contratos;
DROP TABLE IF EXISTS inmuebles;
DROP TABLE IF EXISTS usuarios;
DROP TABLE IF EXISTS personas;
DROP TABLE IF EXISTS roles;
DROP TABLE IF EXISTS tipos_inmuebles;

CREATE TABLE roles (
  id INT NOT NULL AUTO_INCREMENT,
  nombre VARCHAR(50) NOT NULL,
  PRIMARY KEY (id),
  UNIQUE KEY nombre_UNIQUE (nombre)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO roles (id, nombre) VALUES
(1, 'Inquilino'),
(2, 'Propietario');

-- ============================================================
-- Tabla: tipos_inmuebles
-- ============================================================
CREATE TABLE tipos_inmuebles (
  Id INT NOT NULL AUTO_INCREMENT,
  Nombre VARCHAR(100) NOT NULL,
  PRIMARY KEY (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

INSERT INTO tipos_inmuebles (Id, Nombre) VALUES
(1, 'Departamento'),
(2, 'Casa'),
(3, 'Local Comercial'),
(4, 'Casa Quinta'),
(5, 'Oficina'),
(8, 'Casa'),
(9, 'Departamento'),
(10, 'PH'),
(11, 'Local'),
(12, 'Galpón');

-- ============================================================
-- Tabla: personas
-- ============================================================
CREATE TABLE personas (
  ID INT NOT NULL AUTO_INCREMENT,
  Nombre VARCHAR(100) NOT NULL,
  Apellido VARCHAR(100) NOT NULL,
  DNI VARCHAR(20) NOT NULL,
  Email VARCHAR(150) NOT NULL,
  Clave VARCHAR(255) DEFAULT NULL,
  Telefono VARCHAR(50) DEFAULT NULL,
  AvatarUrl VARCHAR(255) DEFAULT NULL,
  InmobiliariaId INT DEFAULT NULL,
  PRIMARY KEY (ID),
  UNIQUE KEY DNI_UNIQUE (DNI),
  KEY personas_inmobiliaria_idx (InmobiliariaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- Tabla: persona_roles (relación muchos a muchos)
-- ============================================================
CREATE TABLE persona_roles (
  persona_id INT NOT NULL,
  rol_id INT NOT NULL,
  PRIMARY KEY (persona_id, rol_id),
  KEY rol_id_idx (rol_id),
  CONSTRAINT persona_roles_ibfk_1
    FOREIGN KEY (persona_id) REFERENCES personas (ID)
    ON DELETE CASCADE,
  CONSTRAINT persona_roles_ibfk_2
    FOREIGN KEY (rol_id) REFERENCES roles (id)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- Tabla: inmuebles
-- ============================================================
CREATE TABLE inmuebles (
  ID INT NOT NULL AUTO_INCREMENT,
  Direccion VARCHAR(255) NOT NULL,
  MetrosCuadrados INT NOT NULL,
  Precio DECIMAL(12,2) DEFAULT NULL,
  PropietarioID INT NOT NULL,
  Activo TINYINT(1) NOT NULL DEFAULT '1',
  TipoId INT DEFAULT NULL,
  ImagenUrl VARCHAR(255) DEFAULT NULL,
  InmobiliariaId INT DEFAULT NULL,
  PRIMARY KEY (ID),
  KEY inmuebles_propietario_idx (PropietarioID),
  KEY inmuebles_inmobiliaria_idx (InmobiliariaId),
  KEY inmuebles_tipo_idx (TipoId),
  CONSTRAINT fk_inmuebles_propietario
    FOREIGN KEY (PropietarioID) REFERENCES personas (ID)
    ON DELETE CASCADE,
  CONSTRAINT fk_inmuebles_tipo
    FOREIGN KEY (TipoId) REFERENCES tipos_inmuebles (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- Tabla: contratos
-- ============================================================
CREATE TABLE contratos (
  Id INT NOT NULL AUTO_INCREMENT,
  InquilinoID INT NOT NULL,
  InmuebleID INT NOT NULL,
  FechaInicio DATE NOT NULL,
  FechaFin DATE NOT NULL,
  MontoMensual DECIMAL(10,2) NOT NULL,
  Estado VARCHAR(50) DEFAULT 'Vigente',
  CreadoPor INT NOT NULL,
  TerminadoPor INT DEFAULT NULL,
  InmobiliariaId INT DEFAULT NULL,
  PRIMARY KEY (Id),
  KEY contrato_inquilino_idx (InquilinoID),
  KEY contratos_inmobiliaria_idx (InmobiliariaId),
  KEY contrato_inmueble_idx (InmuebleID),
  CONSTRAINT fk_contrato_inmueble
    FOREIGN KEY (InmuebleID) REFERENCES inmuebles (ID)
    ON DELETE CASCADE,
  CONSTRAINT fk_contrato_inquilino
    FOREIGN KEY (InquilinoID) REFERENCES personas (ID)
    ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- Trigger de exclusión mutua: no se puede crear un contrato vigente
-- para un inmueble que ya tenga un contrato en esas fechas
DELIMITER ;;
CREATE TRIGGER trg_contrato_no_duplicado
BEFORE INSERT ON contratos
FOR EACH ROW
BEGIN
  DECLARE existe INT;
  SELECT COUNT(*) INTO existe
  FROM contratos
  WHERE InmuebleID = NEW.InmuebleID
    AND Estado = 'Vigente'
    AND (
          (NEW.FechaInicio BETWEEN FechaInicio AND FechaFin)
       OR (NEW.FechaFin BETWEEN FechaInicio AND FechaFin)
       OR (FechaInicio BETWEEN NEW.FechaInicio AND NEW.FechaFin)
         );
  IF existe > 0 THEN
    SIGNAL SQLSTATE '45000'
      SET MESSAGE_TEXT = 'Este inmueble ya tiene un contrato vigente en las fechas indicadas.';
  END IF;
END;;
DELIMITER ;

-- ============================================================
-- Tabla: usuarios
-- (tiene que ir ANTES de pagos: pagos tiene dos foreign key
--  hacia usuarios y MySQL falla con "Failed to open the
--  referenced table 'usuarios'" si la crea después)
-- ============================================================
CREATE TABLE usuarios (
  Id INT NOT NULL AUTO_INCREMENT,
  Email VARCHAR(255) NOT NULL,
  PasswordHash VARCHAR(255) NOT NULL,
  Nombre VARCHAR(100) NOT NULL,
  Apellido VARCHAR(100) NOT NULL,
  Rol VARCHAR(50) NOT NULL,
  AvatarUrl VARCHAR(255) DEFAULT NULL,
  InmobiliariaId INT DEFAULT NULL,
  PRIMARY KEY (Id),
  UNIQUE KEY Email_UNIQUE (Email),
  KEY usuarios_inmobiliaria_idx (InmobiliariaId)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- Tabla: pagos
-- ============================================================
CREATE TABLE pagos (
  Id INT NOT NULL AUTO_INCREMENT,
  ContratoId INT NOT NULL,
  FechaPago DATE NOT NULL,
  Detalle VARCHAR(255) DEFAULT NULL,
  Importe DECIMAL(15,2) NOT NULL,
  CreadoPor INT NOT NULL,
  AnuladoPor INT DEFAULT NULL,
  InmobiliariaId INT DEFAULT NULL,
  PRIMARY KEY (Id),
  KEY pagos_contrato_idx (ContratoId),
  KEY pagos_inmobiliaria_idx (InmobiliariaId),
  KEY pagos_creado_idx (CreadoPor),
  KEY pagos_anulado_idx (AnuladoPor),
  CONSTRAINT fk_pagos_creado
    FOREIGN KEY (CreadoPor) REFERENCES usuarios (Id),
  CONSTRAINT fk_pagos_anulado
    FOREIGN KEY (AnuladoPor) REFERENCES usuarios (Id),
  CONSTRAINT fk_pagos_contrato
    FOREIGN KEY (ContratoId) REFERENCES contratos (Id)
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_unicode_ci;

-- ============================================================
-- Datos de ejemplo (normalizados a la estructura de la app)
-- ============================================================

-- Personas (21 al 43 son inquilinos, 22-30 y 48 son propietarios)
INSERT INTO personas (ID, Nombre, Apellido, DNI, Email, Telefono, AvatarUrl) VALUES
(21, 'María luisa', 'Gómez', '10000001', 'maria23@mail.com', '2665262629', '/avatars/0977930c-c242-4edd-ad01-31e27019c3e7.jpg'),
(22, 'Jorge', 'Fernández', '10000002', 'jorge@mail.com', NULL, NULL),
(23, 'Laura', 'Martínez', '10000003', 'laura@mail.com', NULL, NULL),
(24, 'Andrés', 'Pérez', '10000004', 'andres@mail.com', NULL, NULL),
(25, 'Carolina', 'Díaz', '10000005', 'carolina@mail.com', '266332322', '/avatars/8694287a-cd0f-46ca-9004-bf042c2fbcb6.jpg'),
(26, 'Pedro', 'Suárez', '10000006', 'pedro@mail.com', NULL, NULL),
(27, 'Valeria', 'López', '10000007', 'valeria@mail.com', NULL, NULL),
(28, 'Ricardo', 'Torres', '10000008', 'ricardo@mail.com', NULL, NULL),
(29, 'Sofía', 'Hernández', '10000009', 'sofia@mail.com', NULL, NULL),
(30, 'Martín', 'Ramírez', '10000010', 'martin@mail.com', NULL, NULL),
(31, 'Diego', 'Navarro', '20000001', 'diego@mail.com', NULL, NULL),
(32, 'Lucía', 'Silva', '20000002', 'lucia@mail.com', NULL, NULL),
(33, 'Federico', 'Mendoza', '20000003', 'federico@mail.com', NULL, NULL),
(34, 'Camila', 'Rojas', '20000004', 'camila@mail.com', NULL, NULL),
(35, 'Gabriel', 'Castro', '20000005', 'gabriel@mail.com', NULL, NULL),
(36, 'Paula', 'Vega', '20000006', 'paula@mail.com', NULL, NULL),
(37, 'Hernán', 'Morales', '20000007', 'hernan@mail.com', NULL, NULL),
(38, 'Florencia', 'Acosta', '20000008', 'florencia@mail.com', NULL, NULL),
(39, 'Sebastián', 'Ibarra', '20000009', 'sebastian@mail.com', NULL, NULL),
(40, 'Natalia', 'Ortega', '20000010', 'natalia@mail.com', NULL, NULL),
(43, 'Cintia', 'Barroso', '37296121', 'ASaaSasAS@HOTMAIL.COM', NULL, NULL),
(48, 'Pablo', 'Luna', '38345858', 'pablorafaelluna@hotmail.com', NULL, NULL);

-- Rol persona_inquilino (1 = Inquilino)
INSERT INTO persona_roles (persona_id, rol_id) VALUES
(31,1),(32,1),(33,1),(34,1),(35,1),(36,1),(37,1),(38,1),(39,1),(40,1),(43,1),
(21,2),(22,2),(23,2),(24,2),(25,2),(26,2),(27,2),(28,2),(29,2),(30,2),(48,2);

-- Inmuebles
INSERT INTO inmuebles (ID, Direccion, MetrosCuadrados, Precio, PropietarioID, Activo, TipoId, ImagenUrl) VALUES
(35, 'Av. Rivadavia 1234', 230, 9000000.00, 21, 1, 5, '/uploads/propietarios_21/d8cabace-a76b-447c-98e8-1eeb781a904b.jpg'),
(36, 'Calle San Martín 456', 120, 150000.00, 22, 1, 4, NULL),
(37, 'Boulevard Illia 789', 60, 80000.00, 23, 1, 3, NULL),
(38, 'Ruta 8 km 35', 200, 220000.00, 24, 1, 4, NULL),
(39, 'juansabedra 544', 300, 500.00, 25, 1, 1, '/uploads/propietarios_25/ff6133f9-83ef-448a-9353-9bc7b0062fd1.jpg'),
(40, 'Belgrano 321', 85, 110000.00, 26, 1, 1, NULL),
(41, '9 de Julio 876', 55, 75000.00, 27, 1, 1, NULL),
(42, 'Italia 555', 180, 250000.00, 28, 1, 2, NULL),
(43, 'España 999', 50, 70000.00, 29, 1, 3, NULL),
(44, 'Colón 222', 140, 175000.00, 30, 1, 5, NULL),
(45, 'Sarmiento 777', 95, 120000.00, 43, 1, 1, NULL),
(47, 'caricusta 2022 este', 50, 50.00, 25, 1, 4, '/uploads/propietarios_25/8a100a00-7564-4ca1-8c79-411942e22384.jpg'),
(48, 'el porvenir 3443', 45, 200000.00, 22, 1, 9, NULL),
(49, 'elporvenir 654654', 120, 500000.00, 22, 1, 2, NULL),
(50, 'ayacucho 2025', 250, 800.00, 21, 1, 10, '/uploads/propietarios_21/7e119d35-73ea-42d8-a93f-dbeabc11b343.jpg'),
(51, 'san pedro  255', 200, 10.00, 21, 1, 3, '/uploads/propietarios_21/6876636c-cb18-4406-89c4-3d6186ba4b58.jpg'),
(54, 'calle juan alberto 2052', 250, 200000.00, 21, 1, 4, '/uploads/propietarios_21/75e32eff-3df6-4395-969f-0080d8aa3880.jpg'),
(55, 'san ingnacion 2025', 200, 800000.00, 21, 1, 9, '/uploads/propietarios_21/b9cc0e44-7b5c-44af-ac95-7f56a5378a03.jpg'),
(56, 'santa barroso 555', 100, 80000.00, 21, 1, 4, '/uploads/propietarios_21/1310f5a9-31b9-46ae-a7b0-ec1623a97a63.jpg'),
(59, 'olivos 3433', 90, 2000000.00, 25, 0, 1, '/uploads/propietarios_25/f0d0e948-00b7-4fa1-bcf5-25445d06bd22.jpg'),
(63, 'san ignacio 205', 200, 8500000.00, 25, 0, 4, '/uploads/propietarios_25/06547919-4ba0-40d8-af35-50e5fa54cdb5.jpg'),
(64, 'carlos 343', 200, 350000.00, 25, 0, 1, '/uploads/propietarios_25/839874ef-f22a-40c3-8b1a-66cd55e9a190.jpg'),
(65, 'gabriela mnistral 45', 200, 200000.00, 25, 0, 1, '/uploads/propietarios_25/54411348-9151-4f7c-b761-408f4fc14cb3.jpg'),
(66, 'gallo oro 20', 200, 200500.00, 25, 1, 1, '/uploads/propietarios_25/a85b622c-9de6-4e33-b50a-f5ba07970498.jpg'),
(67, 'san cristobal 43321', 200, 650000.00, 25, 1, 1, '/uploads/propietarios_25/758cd80c-359a-42d3-911d-fddda3ec1ab8.jpg'),
(68, 'pedroignacio 12', 200, 252222.00, 25, 1, 1, '/uploads/propietarios_25/8a12fdd3-3958-4d63-84ec-4a141b1686e3.jpg'),
(69, 'san pablo quiros 200', 200, 5.00, 25, 1, 1, '/uploads/propietarios_25/f36d5d07-81ec-4bc7-a9e5-30e30f2929ea.jpg'),
(70, 'bertiga 435', 200, 352.00, 25, 1, 1, '/uploads/propietarios_25/c0ae3ecb-23c4-4c09-9d22-ecab683967d0.jpg'),
(71, 'cartagena 343', 500, 2800000.00, 25, 0, 1, '/uploads/propietarios_25/a8078d82-b7de-4bf6-8e13-a86ea9ad53d4.jpg'),
(72, 'palacio rodrigues 3223', 250, 5500000.00, 25, 1, 1, '/uploads/propietarios_25/cf8b05fa-81c8-4aa9-bfbf-e4d63aa2bbb8.jpg'),
(73, 'juansabedra', 200, 2522.00, 25, 1, 1, '/uploads/propietarios_25/59de8c5c-d79a-46bc-ad7e-613eb71c5093.jpg'),
(74, 'carf 5543', 200, 500000.00, 25, 1, 1, '/uploads/propietarios_25/eade9051-b865-4bd4-8b4a-aa1382e88788.jpg'),
(75, 'cafallate 2021', 200, 500000.00, 25, 1, 1, '/uploads/propietarios_25/32d698f9-e5f0-4906-8fb7-481b57ea1c22.jpg'),
(76, 'mauricio 3423', 300, 200000.00, 21, 1, 1, '/uploads/propietarios_21/88cc1d98-e7bb-497a-93a2-4398b3dc969f.jpg'),
(77, 'nibarroso 202', 500, 250000.00, 25, 1, 1, '/uploads/propietarios_25/4e98cd82-c09b-4bbd-bb8a-f8d15f3cabea.jpg'),
(78, 'hostal 334', 500, 250000.00, 25, 0, 1, '/uploads/propietarios_25/dc180433-c4a7-4077-b8b2-528a646b1d42.jpg');

-- Contratos
INSERT INTO contratos (Id, InquilinoID, InmuebleID, FechaInicio, FechaFin, MontoMensual, Estado, CreadoPor, TerminadoPor) VALUES
(2, 31, 35, '2025-09-12', '2027-09-12', 260000.00, 'Vigente', 1, NULL),
(7, 34, 40, '2025-09-12', '2027-09-12', 100000.00, 'Vigente', 2, NULL),
(9, 34, 43, '2025-09-12', '2027-09-12', 100000.00, 'Vigente', 4, NULL),
(10, 39, 42, '2025-09-13', '2026-09-13', 250000.00, 'Vigente', 7, NULL),
(11, 33, 47, '2025-09-13', '2028-09-20', 200000.00, 'Vigente', 1, NULL),
(13, 39, 44, '2026-09-13', '2028-09-13', 175000.00, 'Vencido', 2, NULL),
(16, 32, 36, '2020-01-01', '2021-01-01', 50000.00, 'Vencido', 4, NULL),
(19, 32, 37, '2024-01-01', '2026-01-01', 80000.00, 'Vigente', 1, NULL),
(20, 32, 37, '2020-01-01', '2021-01-01', 50000.00, 'Vencido', 2, NULL),
(21, 32, 36, '2021-01-02', '2029-01-01', 50000.00, 'Finalizado', 4, 1),
(23, 28, 41, '2024-01-01', '2024-12-31', 75000.00, 'Finalizado', 2, 4),
(24, 48, 36, '2025-10-03', '2026-10-03', 150000.00, 'Vigente', 0, NULL);

-- Usuarios (administrador + empleados)
INSERT INTO usuarios (Id, Email, PasswordHash, Nombre, Apellido, Rol, AvatarUrl) VALUES
(1, 'admin@inmobiliaria.com', '$2a$11$OoG18IaDQueH2njOkep4MOVQO5FwERgdv8RC5Ap.XS8M0KLvqRyB.', 'Mario roberto', 'Administrador', 'Administrador', '/avatars/816948a3-9e8b-49c5-9794-7dbaf690ff74.jpg'),
(2, 'empleado@inmobiliaria.com', '$2a$11$2f9DsIRcWkx7hQFZJOK9Z.nZ8yq4N2ttLZqPhsgrmrG7nmS8MNYDy', 'Juan', 'Palacios', 'Empleado', '/avatars/e3c1a4c0-c0ee-41da-82f6-9a0cad0aa6ac.jpg'),
(4, 'ceciadmin@inmobiliaria.com', '$2a$11$EolmeGtvjDt.YPOx6Fv0DuDMroBS34P2oh/1N7RyXofyBx7jp2djS', 'Cecilia', 'Barroso', 'Empleado', '/avatars/ded447e0-bc73-4ad7-9614-bc047302164f.jpg'),
(6, 'jghhjfhgd@inmobiliaria.com', '$2a$11$UlhjupaojI5V2X9hA8Xm1uzcdcozcFPnM4cv4/ViVklXSb9MhRK5a', 'Jorge', 'Fernandez', 'Empleado', '/avatars/31c211f0-4590-4c05-872b-ba9a915d27e6.jpg'),
(7, 'asdasdasd@gmail.com', '$2a$11$YvvJoGY7PuRj9af5.k5G8ebMASWm9Saz8KkfQVOFIeNLqWQDFUcFi', 'Cintia', 'Villega', 'Empleado', '/avatars/d25952d6-1cec-4650-93aa-a55e72498ba3.jpg'),
(8, 'juanballestero@hotmail.com', '$2a$11$ABTJ88keVZK7CmWmyU1ROunzNstcawT/KkXo4CmyC9K29.aQdblmW', 'Juan', 'Ballestero', 'Empleado', '/avatars/f4338abf-247c-482a-b106-5b50e53bb1b6.png'),
(9, 'maribarroso@hotmail.com', '$2a$11$vTu7PwVJ3271EKxoatRj3eb0wnTuHFvPEN9vUKtbPFt2wq1gZC6sW', 'marisol', 'barroso', 'Empleado', '/avatars/default.png'),
(10, 'carloq@gmail.com', '$2a$11$DtsWeml/k83AWZ23OdYa/u79Zw0gKoIP9ALyOaQQGHIGjDzptXXSm', 'carlos', 'quiroga ', 'Empleado', '/avatars/default.png'),
(11, 'damilaOrosco@hotmial.com', '$2a$11$Thnxn0ducRnZV4KvMTUydedkY24gQBGidy66euNYCA3cG7aYNARWG', 'Damila ', 'Orozcp', 'Empleado', '/avatars/default.png'),
(12, 'kokoq@hotmail.com', '$2a$11$laYhDvsMmuS3Gs3pcS..eehfeUFmlyMfs4/h5gCWdXbJuwbJw/Hh.', 'koko', 'raul', 'Empleado', '/avatars/default.png'),
(13, 'lolo@hotmail.com', '$2a$11$pzyym8E06yPVy.EkB3rHO.HpKVXWNrV7GWXM3EvdYHmIEt2Ks5.iq', 'lolo', 'fernandez', 'Empleado', '/avatars/default.png'),
(14, 'bauti@gmail,com', '$2a$11$eh51qZdrQlgxWo0x6YDU4O.xp072B/10v6BtooleY4hYx0XDEABLu', 'simon', 'bauti', 'Empleado', '/avatars/default.png');

-- Pagos (va DESPUÉS de usuarios: CreadoPor y AnuladoPor son FK hacia usuarios,
-- si se inserta antes MySQL falla con ERROR 1452)
INSERT INTO pagos (Id, ContratoId, FechaPago, Detalle, Importe, CreadoPor, AnuladoPor) VALUES
(2, 2, '2025-09-12', 'Cuota mensual Septiembre 2025+deposito', 520000.00, 1, 4);

-- ============================================================
-- Multi-inmobiliaria: TODO lo de arriba (personas, inmuebles,
-- contratos, pagos y usuarios de prueba) pertenece a la
-- inmobiliaria 1, es decir, a la inmobiliaria demo.
-- Un usuario que se registre desde la app no ve ninguno de
-- estos datos: queda como dueño de una inmobiliaria vacía.
-- ============================================================
UPDATE personas SET InmobiliariaId = 1;
UPDATE inmuebles SET InmobiliariaId = 1;
UPDATE contratos SET InmobiliariaId = 1;
UPDATE pagos SET InmobiliariaId = 1;
UPDATE usuarios SET InmobiliariaId = 1;
