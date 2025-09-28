-- MySQL dump 10.13  Distrib 8.0.43, for Win64 (x86_64)
--
-- Host: localhost    Database: mi_base_datos
-- ------------------------------------------------------
-- Server version	8.0.43

/*!40101 SET @OLD_CHARACTER_SET_CLIENT=@@CHARACTER_SET_CLIENT */;
/*!40101 SET @OLD_CHARACTER_SET_RESULTS=@@CHARACTER_SET_RESULTS */;
/*!40101 SET @OLD_COLLATION_CONNECTION=@@COLLATION_CONNECTION */;
/*!50503 SET NAMES utf8 */;
/*!40103 SET @OLD_TIME_ZONE=@@TIME_ZONE */;
/*!40103 SET TIME_ZONE='+00:00' */;
/*!40014 SET @OLD_UNIQUE_CHECKS=@@UNIQUE_CHECKS, UNIQUE_CHECKS=0 */;
/*!40014 SET @OLD_FOREIGN_KEY_CHECKS=@@FOREIGN_KEY_CHECKS, FOREIGN_KEY_CHECKS=0 */;
/*!40101 SET @OLD_SQL_MODE=@@SQL_MODE, SQL_MODE='NO_AUTO_VALUE_ON_ZERO' */;
/*!40111 SET @OLD_SQL_NOTES=@@SQL_NOTES, SQL_NOTES=0 */;

--
-- Table structure for table `contratos`
--

DROP TABLE IF EXISTS `contratos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `contratos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `InquilinoID` int NOT NULL,
  `InmuebleID` int NOT NULL,
  `FechaInicio` date NOT NULL,
  `FechaFin` date NOT NULL,
  `MontoMensual` decimal(10,2) NOT NULL,
  `Estado` varchar(50) DEFAULT 'Vigente',
  `CreadoPor` int NOT NULL,
  `TerminadoPor` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `fk_contrato_inquilino` (`InquilinoID`),
  KEY `fk_contrato_inmueble` (`InmuebleID`),
  CONSTRAINT `fk_contrato_inmueble` FOREIGN KEY (`InmuebleID`) REFERENCES `inmuebles` (`ID`) ON DELETE CASCADE,
  CONSTRAINT `fk_contrato_inquilino` FOREIGN KEY (`InquilinoID`) REFERENCES `personas` (`ID`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=24 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `contratos`
--

LOCK TABLES `contratos` WRITE;
/*!40000 ALTER TABLE `contratos` DISABLE KEYS */;
INSERT INTO `contratos` VALUES (2,31,35,'2025-09-12','2027-09-12',260000.00,'Vigente',1,NULL),(7,34,40,'2025-09-12','2027-09-12',100000.00,'Vigente',2,NULL),(9,34,43,'2025-09-12','2027-09-12',100000.00,'Vigente',4,NULL),(10,39,42,'2025-09-13','2026-09-13',250000.00,'Vigente',7,NULL),(11,33,47,'2025-09-13','2028-09-20',200000.00,'Vigente',1,NULL),(13,39,44,'2026-09-13','2028-09-13',175000.00,'Vencido',2,NULL),(16,32,36,'2020-01-01','2021-01-01',50000.00,'Vencido',4,NULL),(19,32,37,'2024-01-01','2026-01-01',80000.00,'Vigente',1,NULL),(20,32,37,'2020-01-01','2021-01-01',50000.00,'Vencido',2,NULL),(21,32,36,'2021-01-02','2029-01-01',50000.00,'Finalizado',4,1),(23,28,41,'2024-01-01','2024-12-31',75000.00,'Finalizado',2,4);
/*!40000 ALTER TABLE `contratos` ENABLE KEYS */;
UNLOCK TABLES;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;
/*!50003 SET character_set_client  = utf8mb4 */ ;
/*!50003 SET character_set_results = utf8mb4 */ ;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;
DELIMITER ;;
/*!50003 CREATE*/ /*!50017 DEFINER=`root`@`localhost`*/ /*!50003 TRIGGER `trg_contrato_no_duplicado` BEFORE INSERT ON `contratos` FOR EACH ROW BEGIN
    DECLARE existe INT;

    SELECT COUNT(*) INTO existe
    FROM contratos
    WHERE InmuebleID = NEW.InmuebleID
      AND Estado = 'Vigente'
      AND (
            (NEW.FechaInicio BETWEEN FechaInicio AND FechaFin) OR
            (NEW.FechaFin BETWEEN FechaInicio AND FechaFin) OR
            (FechaInicio BETWEEN NEW.FechaInicio AND NEW.FechaFin)
          );

    IF existe > 0 THEN
        SIGNAL SQLSTATE '45000'
        SET MESSAGE_TEXT = '❌ Este inmueble ya tiene un contrato vigente en las fechas indicadas.';
    END IF;
END */;;
DELIMITER ;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;
/*!50003 SET character_set_client  = @saved_cs_client */ ;
/*!50003 SET character_set_results = @saved_cs_results */ ;
/*!50003 SET collation_connection  = @saved_col_connection */ ;

--
-- Table structure for table `inmuebles`
--

DROP TABLE IF EXISTS `inmuebles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `inmuebles` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Direccion` varchar(255) NOT NULL,
  `MetrosCuadrados` int NOT NULL,
  `Precio` decimal(10,2) NOT NULL,
  `PropietarioID` int NOT NULL,
  `Activo` tinyint(1) NOT NULL DEFAULT '1',
  `TipoId` int DEFAULT NULL,
  PRIMARY KEY (`ID`),
  KEY `inmuebles_ibfk_1` (`PropietarioID`),
  KEY `FK_Inmuebles_TipoId` (`TipoId`),
  CONSTRAINT `fk_inmuebles_tipo` FOREIGN KEY (`TipoId`) REFERENCES `tipos_inmuebles` (`Id`),
  CONSTRAINT `FK_Inmuebles_TipoId` FOREIGN KEY (`TipoId`) REFERENCES `tipos_inmuebles` (`Id`),
  CONSTRAINT `inmuebles_ibfk_1` FOREIGN KEY (`PropietarioID`) REFERENCES `personas` (`ID`) ON DELETE CASCADE
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `inmuebles`
--

LOCK TABLES `inmuebles` WRITE;
/*!40000 ALTER TABLE `inmuebles` DISABLE KEYS */;
INSERT INTO `inmuebles` VALUES (35,'Av. Rivadavia 1234',75,95000.00,21,0,5),(36,'Calle San Martín 456',120,150000.00,22,1,2),(37,'Boulevard Illia 789',60,80000.00,23,1,3),(38,'Ruta 8 km 35',200,220000.00,24,1,4),(39,'Mitre 234',40,60000.00,25,1,5),(40,'Belgrano 321',85,110000.00,26,1,1),(41,'9 de Julio 876',55,75000.00,27,1,1),(42,'Italia 555',180,250000.00,28,1,2),(43,'España 999',50,70000.00,29,1,3),(44,'Colón 222',140,175000.00,30,1,5),(45,'Sarmiento 777',95,120000.00,43,1,1),(47,'caricusta 2022 este',50,200000.00,25,1,1);
/*!40000 ALTER TABLE `inmuebles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `pagos`
--

DROP TABLE IF EXISTS `pagos`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `pagos` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `ContratoId` int NOT NULL,
  `FechaPago` date NOT NULL,
  `Detalle` varchar(255) DEFAULT NULL,
  `Importe` decimal(15,2) NOT NULL,
  `CreadoPor` int NOT NULL,
  `AnuladoPor` int DEFAULT NULL,
  PRIMARY KEY (`Id`),
  KEY `ContratoId` (`ContratoId`),
  KEY `fk_pagos_creado` (`CreadoPor`),
  KEY `fk_pagos_anulado` (`AnuladoPor`),
  CONSTRAINT `fk_pagos_anulado` FOREIGN KEY (`AnuladoPor`) REFERENCES `usuarios` (`Id`),
  CONSTRAINT `fk_pagos_creado` FOREIGN KEY (`CreadoPor`) REFERENCES `usuarios` (`Id`),
  CONSTRAINT `pagos_ibfk_1` FOREIGN KEY (`ContratoId`) REFERENCES `contratos` (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `pagos`
--

LOCK TABLES `pagos` WRITE;
/*!40000 ALTER TABLE `pagos` DISABLE KEYS */;
INSERT INTO `pagos` VALUES (2,2,'2025-09-12','Cuota mensual Septiembre 2025+deposito',520000.00,1,4);
/*!40000 ALTER TABLE `pagos` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `persona_roles`
--

DROP TABLE IF EXISTS `persona_roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `persona_roles` (
  `persona_id` int NOT NULL,
  `rol_id` int NOT NULL,
  PRIMARY KEY (`persona_id`,`rol_id`),
  KEY `rol_id` (`rol_id`),
  CONSTRAINT `persona_roles_ibfk_1` FOREIGN KEY (`persona_id`) REFERENCES `personas` (`ID`) ON DELETE CASCADE,
  CONSTRAINT `persona_roles_ibfk_2` FOREIGN KEY (`rol_id`) REFERENCES `roles` (`id`) ON DELETE CASCADE
) ENGINE=InnoDB DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `persona_roles`
--

LOCK TABLES `persona_roles` WRITE;
/*!40000 ALTER TABLE `persona_roles` DISABLE KEYS */;
INSERT INTO `persona_roles` VALUES (31,1),(32,1),(33,1),(34,1),(35,1),(36,1),(37,1),(38,1),(39,1),(40,1),(43,1),(21,2),(22,2),(23,2),(24,2),(25,2),(26,2),(27,2),(28,2),(29,2),(30,2),(43,2);
/*!40000 ALTER TABLE `persona_roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `personas`
--

DROP TABLE IF EXISTS `personas`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `personas` (
  `ID` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `DNI` varchar(20) NOT NULL,
  `Email` varchar(150) NOT NULL,
  PRIMARY KEY (`ID`),
  UNIQUE KEY `DNI` (`DNI`)
) ENGINE=InnoDB AUTO_INCREMENT=48 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `personas`
--

LOCK TABLES `personas` WRITE;
/*!40000 ALTER TABLE `personas` DISABLE KEYS */;
INSERT INTO `personas` VALUES (21,'María','Gómez','10000001','maria@mail.com'),(22,'Jorge','Fernández','10000002','jorge@mail.com'),(23,'Laura','Martínez','10000003','laura@mail.com'),(24,'Andrés','Pérez','10000004','andres@mail.com'),(25,'Carolina','Díaz','10000005','carolina@mail.com'),(26,'Pedro','Suárez','10000006','pedro@mail.com'),(27,'Valeria','López','10000007','valeria@mail.com'),(28,'Ricardo','Torres','10000008','ricardo@mail.com'),(29,'Sofía','Hernández','10000009','sofia@mail.com'),(30,'Martín','Ramírez','10000010','martin@mail.com'),(31,'Diego','Navarro','20000001','diego@mail.com'),(32,'Lucía','Silva','20000002','lucia@mail.com'),(33,'Federico','Mendoza','20000003','federico@mail.com'),(34,'Camila','Rojas','20000004','camila@mail.com'),(35,'Gabriel','Castro','20000005','gabriel@mail.com'),(36,'Paula','Vega','20000006','paula@mail.com'),(37,'Hernán','Morales','20000007','hernan@mail.com'),(38,'Florencia','Acosta','20000008','florencia@mail.com'),(39,'Sebastián','Ibarra','20000009','sebastian@mail.com'),(40,'Natalia','Ortega','20000010','natalia@mail.com'),(43,'Cintia ','Barroso','37296121','ASaaSasAS@HOTMAIL.COM'),(46,'Carlos','Propietario','40000000','carlos.prop@test.com'),(47,'Ana','Inquilina','30000000','ana.inq@test.com');
/*!40000 ALTER TABLE `personas` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `roles`
--

DROP TABLE IF EXISTS `roles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `roles` (
  `id` int NOT NULL AUTO_INCREMENT,
  `nombre` varchar(50) NOT NULL,
  PRIMARY KEY (`id`),
  UNIQUE KEY `nombre` (`nombre`)
) ENGINE=InnoDB AUTO_INCREMENT=3 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `roles`
--

LOCK TABLES `roles` WRITE;
/*!40000 ALTER TABLE `roles` DISABLE KEYS */;
INSERT INTO `roles` VALUES (1,'Inquilino'),(2,'Propietario');
/*!40000 ALTER TABLE `roles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `tipos_inmuebles`
--

DROP TABLE IF EXISTS `tipos_inmuebles`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `tipos_inmuebles` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Nombre` varchar(100) NOT NULL,
  PRIMARY KEY (`Id`)
) ENGINE=InnoDB AUTO_INCREMENT=13 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `tipos_inmuebles`
--

LOCK TABLES `tipos_inmuebles` WRITE;
/*!40000 ALTER TABLE `tipos_inmuebles` DISABLE KEYS */;
INSERT INTO `tipos_inmuebles` VALUES (1,'Departamento'),(2,'Casa'),(3,'Local Comercial'),(4,'Casa Quinta'),(5,'Oficina'),(8,'Casa'),(9,'Departamento'),(10,'PH'),(11,'Local'),(12,'Galpón');
/*!40000 ALTER TABLE `tipos_inmuebles` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Table structure for table `usuarios`
--

DROP TABLE IF EXISTS `usuarios`;
/*!40101 SET @saved_cs_client     = @@character_set_client */;
/*!50503 SET character_set_client = utf8mb4 */;
CREATE TABLE `usuarios` (
  `Id` int NOT NULL AUTO_INCREMENT,
  `Email` varchar(255) NOT NULL,
  `PasswordHash` varchar(255) NOT NULL,
  `Nombre` varchar(100) NOT NULL,
  `Apellido` varchar(100) NOT NULL,
  `Rol` varchar(50) NOT NULL,
  `AvatarUrl` varchar(255) DEFAULT NULL,
  PRIMARY KEY (`Id`),
  UNIQUE KEY `Email` (`Email`)
) ENGINE=InnoDB AUTO_INCREMENT=8 DEFAULT CHARSET=utf8mb4 COLLATE=utf8mb4_0900_ai_ci;
/*!40101 SET character_set_client = @saved_cs_client */;

--
-- Dumping data for table `usuarios`
--

LOCK TABLES `usuarios` WRITE;
/*!40000 ALTER TABLE `usuarios` DISABLE KEYS */;
INSERT INTO `usuarios` VALUES (1,'admin@inmobiliaria.com','$2a$11$OoG18IaDQueH2njOkep4MOVQO5FwERgdv8RC5Ap.XS8M0KLvqRyB.','Mario','Administrador','Administrador','/avatars/816948a3-9e8b-49c5-9794-7dbaf690ff74.jpg'),(2,'empleado@inmobiliaria.com','$2a$11$2f9DsIRcWkx7hQFZJOK9Z.nZ8yq4N2ttLZqPhsgrmrG7nmS8MNYDy','Juan','Palacios','Empleado','/avatars/e3c1a4c0-c0ee-41da-82f6-9a0cad0aa6ac.jpg'),(4,'ceciadmin@inmobiliaria.com','$2a$11$EolmeGtvjDt.YPOx6Fv0DuDMroBS34P2oh/1N7RyXofyBx7jp2djS','Cecilia','Barroso','Empleado','/avatars/ded447e0-bc73-4ad7-9614-bc047302164f.jpg'),(6,'jghhjfhgd@inmobiliaria.com','$2a$11$UlhjupaojI5V2X9hA8Xm1uzcdcozcFPnM4cv4/ViVklXSb9MhRK5a','Jorge','Fernandez','Empleado','/avatars/31c211f0-4590-4c05-872b-ba9a915d27e6.jpg'),(7,'asdasdasd@gmail.com','$2a$11$YvvJoGY7PuRj9af5.k5G8ebMASWm9Saz8KkfQVOFIeNLqWQDFUcFi','Cintia','Villega','Empleado','/avatars/d25952d6-1cec-4650-93aa-a55e72498ba3.jpg');
/*!40000 ALTER TABLE `usuarios` ENABLE KEYS */;
UNLOCK TABLES;

--
-- Dumping events for database 'mi_base_datos'
--
/*!50106 SET @save_time_zone= @@TIME_ZONE */ ;
/*!50106 DROP EVENT IF EXISTS `evt_marcar_contratos_vencidos` */;
DELIMITER ;;
/*!50003 SET @saved_cs_client      = @@character_set_client */ ;;
/*!50003 SET @saved_cs_results     = @@character_set_results */ ;;
/*!50003 SET @saved_col_connection = @@collation_connection */ ;;
/*!50003 SET character_set_client  = utf8mb4 */ ;;
/*!50003 SET character_set_results = utf8mb4 */ ;;
/*!50003 SET collation_connection  = utf8mb4_0900_ai_ci */ ;;
/*!50003 SET @saved_sql_mode       = @@sql_mode */ ;;
/*!50003 SET sql_mode              = 'ONLY_FULL_GROUP_BY,STRICT_TRANS_TABLES,NO_ZERO_IN_DATE,NO_ZERO_DATE,ERROR_FOR_DIVISION_BY_ZERO,NO_ENGINE_SUBSTITUTION' */ ;;
/*!50003 SET @saved_time_zone      = @@time_zone */ ;;
/*!50003 SET time_zone             = 'SYSTEM' */ ;;
/*!50106 CREATE*/ /*!50117 DEFINER=`root`@`localhost`*/ /*!50106 EVENT `evt_marcar_contratos_vencidos` ON SCHEDULE EVERY 1 DAY STARTS '2025-09-11 21:24:13' ON COMPLETION NOT PRESERVE ENABLE DO UPDATE contratos
  SET Estado = 'Vencido'
  WHERE FechaFin < CURDATE()
    AND Estado = 'Vigente' */ ;;
/*!50003 SET time_zone             = @saved_time_zone */ ;;
/*!50003 SET sql_mode              = @saved_sql_mode */ ;;
/*!50003 SET character_set_client  = @saved_cs_client */ ;;
/*!50003 SET character_set_results = @saved_cs_results */ ;;
/*!50003 SET collation_connection  = @saved_col_connection */ ;;
DELIMITER ;
/*!50106 SET TIME_ZONE= @save_time_zone */ ;

--
-- Dumping routines for database 'mi_base_datos'
--
/*!40103 SET TIME_ZONE=@OLD_TIME_ZONE */;

/*!40101 SET SQL_MODE=@OLD_SQL_MODE */;
/*!40014 SET FOREIGN_KEY_CHECKS=@OLD_FOREIGN_KEY_CHECKS */;
/*!40014 SET UNIQUE_CHECKS=@OLD_UNIQUE_CHECKS */;
/*!40101 SET CHARACTER_SET_CLIENT=@OLD_CHARACTER_SET_CLIENT */;
/*!40101 SET CHARACTER_SET_RESULTS=@OLD_CHARACTER_SET_RESULTS */;
/*!40101 SET COLLATION_CONNECTION=@OLD_COLLATION_CONNECTION */;
/*!40111 SET SQL_NOTES=@OLD_SQL_NOTES */;

-- Dump completed on 2025-09-28 14:40:59
