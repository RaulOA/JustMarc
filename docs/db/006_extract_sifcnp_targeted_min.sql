/* ============================================================================
   SIFCNP RF-06 - Validacion dirigida minima (Read-Only)

   Objetivo:
   - Validar existencia y acceso a objetos candidatos RF-06 en SIFCNP.
   - Obtener muestras TOP pequenas para validacion funcional sin carga alta.

   Seguridad:
   - Script 100% solo lectura: usa SELECT y metadata de sistema.
   - No usa INSERT/UPDATE/DELETE/MERGE/CREATE/ALTER/DROP/TRUNCATE.
   ============================================================================ */

USE [SIFCNP];
GO

SET NOCOUNT ON;
SET DEADLOCK_PRIORITY LOW;
SET LOCK_TIMEOUT 3000;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

/* =========================
   Parametros de muestra
   ========================= */
DECLARE @TopEnc INT = 30;
DECLARE @TopDet INT = 100;
DECLARE @TopLookup INT = 50;
DECLARE @TopView INT = 30;

/* =========================
   1) Existencia y tamano aproximado (metadata)
   ========================= */
SELECT
    s.name AS SchemaName,
    o.name AS ObjectName,
    o.type_desc AS ObjectType,
    COALESCE(SUM(ps.row_count), 0) AS ApproxRows
FROM sys.objects o
INNER JOIN sys.schemas s
    ON s.schema_id = o.schema_id
LEFT JOIN sys.dm_db_partition_stats ps
    ON ps.object_id = o.object_id
   AND ps.index_id IN (0, 1)
WHERE s.name = 'dbo'
  AND o.name IN (
      'RH_JUSTIFICACIONES_ENC',
      'RH_JUSTIFICACIONES_DET',
      'RH_TI_TipoMarca',
      'RH_TI_EstadoMarcas',
      'V_TI_RH_JUST_MARCA'
  )
GROUP BY s.name, o.name, o.type_desc
ORDER BY o.type_desc, o.name;

/* =========================
   2) Muestra ENC (ultimas justificaciones)
   ========================= */
IF OBJECT_ID('dbo.RH_JUSTIFICACIONES_ENC', 'U') IS NOT NULL
BEGIN
    SELECT TOP (@TopEnc)
        num_justificacion,
        cod_solicitante,
        cod_estado,
        fec_confeccion,
        fec_autorizacion,
        fec_aprobacion
    FROM dbo.RH_JUSTIFICACIONES_ENC
    ORDER BY num_justificacion DESC;
END
ELSE
BEGIN
    SELECT CAST('Objeto no encontrado: dbo.RH_JUSTIFICACIONES_ENC' AS NVARCHAR(200)) AS Warning;
END;

/* =========================
   3) Muestra DET ligada a ENC reciente
   ========================= */
IF OBJECT_ID('dbo.RH_JUSTIFICACIONES_ENC', 'U') IS NOT NULL
   AND OBJECT_ID('dbo.RH_JUSTIFICACIONES_DET', 'U') IS NOT NULL
BEGIN
    ;WITH H AS (
        SELECT TOP (@TopEnc)
            num_justificacion
        FROM dbo.RH_JUSTIFICACIONES_ENC
        ORDER BY num_justificacion DESC
    )
    SELECT TOP (@TopDet)
        d.num_justificacion,
        d.cod_linea,
        d.ind_concepto,
        d.fec_justificacion,
        d.ind_rebajar_planilla
    FROM dbo.RH_JUSTIFICACIONES_DET d
    INNER JOIN H
        ON H.num_justificacion = d.num_justificacion
    ORDER BY d.num_justificacion DESC, d.cod_linea;
END
ELSE
BEGIN
    SELECT CAST('Objeto no encontrado: dbo.RH_JUSTIFICACIONES_DET o dbo.RH_JUSTIFICACIONES_ENC' AS NVARCHAR(200)) AS Warning;
END;

/* =========================
   4) Catalogos (volumen bajo)
   ========================= */
IF OBJECT_ID('dbo.RH_TI_EstadoMarcas', 'U') IS NOT NULL
BEGIN
    SELECT TOP (@TopLookup)
        COD_ESTADO,
        DES_ESTADO
    FROM dbo.RH_TI_EstadoMarcas
    ORDER BY COD_ESTADO;
END
ELSE
BEGIN
    SELECT CAST('Objeto no encontrado: dbo.RH_TI_EstadoMarcas' AS NVARCHAR(200)) AS Warning;
END;

IF OBJECT_ID('dbo.RH_TI_TipoMarca', 'U') IS NOT NULL
BEGIN
    SELECT TOP (@TopLookup)
        IND_CONCEPTO,
        DES_TIPO_CONCEPTO
    FROM dbo.RH_TI_TipoMarca
    ORDER BY IND_CONCEPTO;
END
ELSE
BEGIN
    SELECT CAST('Objeto no encontrado: dbo.RH_TI_TipoMarca' AS NVARCHAR(200)) AS Warning;
END;

/* =========================
   5) Smoke test de vista consolidada
   ========================= */
IF OBJECT_ID('dbo.V_TI_RH_JUST_MARCA', 'V') IS NOT NULL
BEGIN
    SELECT TOP (@TopView)
        NUM_JUSTIF,
        FUNCIONARIO,
        DES_ESTADO,
        CONCEPTO,
        DIA_JUSTIFICA,
        fec_autorizacion
    FROM dbo.V_TI_RH_JUST_MARCA
    ORDER BY NUM_JUSTIF DESC;
END
ELSE
BEGIN
    SELECT CAST('Objeto no encontrado: dbo.V_TI_RH_JUST_MARCA' AS NVARCHAR(200)) AS Warning;
END;
