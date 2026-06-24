USE INTEGRA_CNP;
GO

-- ============================================================
-- 006_fix_mojibake_historial_textos.sql
-- Correcciones idempotentes para textos con mojibake en historial.
-- Incluye guia opcional para migrar VARCHAR -> NVARCHAR.
-- ============================================================

-- 1) Diagnostico rapido (ajustar/expandir segun hallazgos reales)
SELECT COUNT(1) AS TipoJustificacionMojibake
FROM Configuracion.TipoJustificacion
WHERE Descripcion LIKE '%Ã%' OR Descripcion LIKE '%Â%' OR Descripcion LIKE '%�%';

SELECT COUNT(1) AS MotivoGeneralMojibake
FROM Operacion.Justificacion
WHERE MotivoGeneral LIKE '%Ã%' OR MotivoGeneral LIKE '%Â%' OR MotivoGeneral LIKE '%�%';

SELECT COUNT(1) AS ComentarioResolucionMojibake
FROM Operacion.Justificacion
WHERE ComentarioResolucion LIKE '%Ã%' OR ComentarioResolucion LIKE '%Â%' OR ComentarioResolucion LIKE '%�%';

SELECT COUNT(1) AS ObservacionDetalleMojibake
FROM Operacion.JustificacionDetalle
WHERE ObservacionDetalle LIKE '%Ã%' OR ObservacionDetalle LIKE '%Â%' OR ObservacionDetalle LIKE '%�%';
GO

-- 2) Correcciones puntuales conocidas (catalogo)
UPDATE Configuracion.TipoJustificacion
SET Descripcion = 'Omisión'
WHERE Descripcion = 'OmisiÃ³n';

UPDATE Configuracion.TipoJustificacion
SET Descripcion = 'Comisión'
WHERE Descripcion = 'ComisiÃ³n';

UPDATE Configuracion.TipoJustificacion
SET Descripcion = 'Reunión'
WHERE Descripcion = 'ReuniÃ³n';
GO

-- 3) Normalizacion controlada en textos operativos (idempotente)
-- Nota: estas sustituciones son seguras de re-ejecutar; tras la primera corrida
-- ya no deberian existir los patrones de entrada.
UPDATE Operacion.Justificacion
SET MotivoGeneral = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(MotivoGeneral,
  'Ã¡', 'á'), 'Ã©', 'é'), 'Ã­', 'í'), 'Ã³', 'ó'), 'Ãº', 'ú'),
  'Ã', 'Á'), 'Ã‰', 'É'), 'Ã', 'Í'), 'Ã“', 'Ó'), 'Ãš', 'Ú'),
  'Ã±', 'ñ'), 'Ã‘', 'Ñ'), 'Ã¼', 'ü'), 'Ãœ', 'Ü'),
  'Â¿', '¿'), 'Â¡', '¡'), 'Â°', '°'), 'Â', ''), '�', ''),
  CHAR(194), ''))
WHERE MotivoGeneral LIKE '%Ã%' OR MotivoGeneral LIKE '%Â%' OR MotivoGeneral LIKE '%�%';

UPDATE Operacion.Justificacion
SET ComentarioResolucion = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ComentarioResolucion,
  'Ã¡', 'á'), 'Ã©', 'é'), 'Ã­', 'í'), 'Ã³', 'ó'), 'Ãº', 'ú'),
  'Ã', 'Á'), 'Ã‰', 'É'), 'Ã', 'Í'), 'Ã“', 'Ó'), 'Ãš', 'Ú'),
  'Ã±', 'ñ'), 'Ã‘', 'Ñ'), 'Ã¼', 'ü'), 'Ãœ', 'Ü'),
  'Â¿', '¿'), 'Â¡', '¡'), 'Â°', '°'), 'Â', ''), '�', ''),
  CHAR(194), ''))
WHERE ComentarioResolucion LIKE '%Ã%' OR ComentarioResolucion LIKE '%Â%' OR ComentarioResolucion LIKE '%�%';

UPDATE Operacion.JustificacionDetalle
SET ObservacionDetalle = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ObservacionDetalle,
  'Ã¡', 'á'), 'Ã©', 'é'), 'Ã­', 'í'), 'Ã³', 'ó'), 'Ãº', 'ú'),
  'Ã', 'Á'), 'Ã‰', 'É'), 'Ã', 'Í'), 'Ã“', 'Ó'), 'Ãš', 'Ú'),
  'Ã±', 'ñ'), 'Ã‘', 'Ñ'), 'Ã¼', 'ü'), 'Ãœ', 'Ü'),
  'Â¿', '¿'), 'Â¡', '¡'), 'Â°', '°'), 'Â', ''), '�', ''),
  CHAR(194), ''))
WHERE ObservacionDetalle LIKE '%Ã%' OR ObservacionDetalle LIKE '%Â%' OR ObservacionDetalle LIKE '%�%';
GO

-- 4) Guia opcional de hardening (ejecutar por ventana de cambio)
-- Recomendado para evitar recurrencia con acentos/unicode.
-- Validar primero longitudes, indices y dependencias.

/*
IF COL_LENGTH('Configuracion.TipoJustificacion', 'Descripcion') IS NOT NULL
BEGIN
  ALTER TABLE Configuracion.TipoJustificacion
    ALTER COLUMN Descripcion NVARCHAR(100) NOT NULL;
END
GO

IF COL_LENGTH('Operacion.Justificacion', 'MotivoGeneral') IS NOT NULL
BEGIN
  ALTER TABLE Operacion.Justificacion
    ALTER COLUMN MotivoGeneral NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH('Operacion.Justificacion', 'ComentarioResolucion') IS NOT NULL
BEGIN
  ALTER TABLE Operacion.Justificacion
    ALTER COLUMN ComentarioResolucion NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH('Operacion.JustificacionDetalle', 'ObservacionDetalle') IS NOT NULL
BEGIN
  ALTER TABLE Operacion.JustificacionDetalle
    ALTER COLUMN ObservacionDetalle NVARCHAR(250) NULL;
END
GO
*/

PRINT '006_fix_mojibake_historial_textos.sql completado.';
GO
