-- ============================================================
-- Migración: multi-inmobiliaria (aislamiento de datos por tenant)
-- ============================================================
-- Se corre UNA SOLA VEZ contra la base existente (mi_base_datos),
-- ANTES de levantar la versión nueva de la app, porque el código
-- nuevo ya hace SELECT sobre la columna InmobiliariaId.
--
-- Desde el server:
--   docker exec -i inmobiliaria_mysql mysql -uapp_user -papp_pass123 mi_base_datos < migrate-inmobiliaria.sql
--
-- ¿Por qué algunos usuarios quedan en la inmobiliaria 1?
--   Los Id <= 14 son los usuarios creados por init-db.sql (datos de
--   prueba) y todos los datos existentes son de la inmobiliaria demo.
--   Los usuarios Id > 14 se registraron desde la app: cada uno pasa a
--   ser dueño de su propia inmobiliaria, vacía, sin datos de nadie.
-- ============================================================

USE mi_base_datos;

-- 1) Columnas nuevas ------------------------------------------------
ALTER TABLE usuarios  ADD COLUMN InmobiliariaId INT NULL;
ALTER TABLE personas  ADD COLUMN InmobiliariaId INT NULL;
ALTER TABLE inmuebles ADD COLUMN InmobiliariaId INT NULL;
ALTER TABLE contratos ADD COLUMN InmobiliariaId INT NULL;
ALTER TABLE pagos     ADD COLUMN InmobiliariaId INT NULL;

-- 2) Índices para que los filtros no hagan full scan ---------------
CREATE INDEX usuarios_inmobiliaria_idx  ON usuarios (InmobiliariaId);
CREATE INDEX personas_inmobiliaria_idx  ON personas (InmobiliariaId);
CREATE INDEX inmuebles_inmobiliaria_idx ON inmuebles (InmobiliariaId);
CREATE INDEX contratos_inmobiliaria_idx ON contratos (InmobiliariaId);
CREATE INDEX pagos_inmobiliaria_idx     ON pagos (InmobiliariaId);

-- 3) Usuarios -------------------------------------------------------
--    Prueba (init-db.sql) => todos a la inmobiliaria 1, así los
--    usuarios viejos siguen viendo los datos de prueba.
UPDATE usuarios SET InmobiliariaId = 1 WHERE Id <= 14;

--    Registrados desde la app => cada uno es dueño de la suya.
UPDATE usuarios SET InmobiliariaId = Id WHERE InmobiliariaId IS NULL;

-- 4) Datos existentes => inmobiliaria 1 (la demo) -------------------
UPDATE personas  SET InmobiliariaId = 1 WHERE InmobiliariaId IS NULL;
UPDATE inmuebles SET InmobiliariaId = 1 WHERE InmobiliariaId IS NULL;
UPDATE contratos SET InmobiliariaId = 1 WHERE InmobiliariaId IS NULL;
UPDATE pagos     SET InmobiliariaId = 1 WHERE InmobiliariaId IS NULL;

-- 5) Verificación ---------------------------------------------------
SELECT 'usuarios' AS tabla, InmobiliariaId, COUNT(*) AS filas FROM usuarios GROUP BY InmobiliariaId
UNION ALL
SELECT 'personas', InmobiliariaId, COUNT(*) FROM personas GROUP BY InmobiliariaId
UNION ALL
SELECT 'inmuebles', InmobiliariaId, COUNT(*) FROM inmuebles GROUP BY InmobiliariaId
UNION ALL
SELECT 'contratos', InmobiliariaId, COUNT(*) FROM contratos GROUP BY InmobiliariaId
UNION ALL
SELECT 'pagos', InmobiliariaId, COUNT(*) FROM pagos GROUP BY InmobiliariaId;
