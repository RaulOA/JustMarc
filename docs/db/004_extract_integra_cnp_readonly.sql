/* ============================================================================
   INTEGRA_CNP - Extraccion Read-Only (SQL Server)

   Run-order (compacto):
   1) Ajustar variables en "Parametros".
   2) Ejecutar secciones 1-4 para metadata, estructura y dimensionamiento.
   3) Ejecutar seccion 5 para muestras TOP(N) por tabla clave.
   4) Ejecutar seccion 6 para dataset canonico operativo.

   Seguridad:
   - Script 100% solo lectura: usa SELECT y metadata de sistema.
   - No usa INSERT/UPDATE/DELETE/MERGE/CREATE/ALTER/DROP/TRUNCATE.
   ============================================================================ */

USE [INTEGRA_CNP];
GO

SET NOCOUNT ON;
SET DEADLOCK_PRIORITY LOW;
SET LOCK_TIMEOUT 5000;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

/* =========================
   Parametros
   ========================= */
DECLARE @TopN INT = 1000;
DECLARE @TopNCanonico INT = 1000;
DECLARE @FromDate DATE = NULL;
DECLARE @ToDate DATE = NULL;
DECLARE @EnableSampling BIT = 0;
DECLARE @EnableCanonicalExtract BIT = 0;

DECLARE @ObjectPattern1 NVARCHAR(100) = N'%usuario%';
DECLARE @ObjectPattern2 NVARCHAR(100) = N'%justificacion%';
DECLARE @ObjectPattern3 NVARCHAR(100) = N'%estado%';
DECLARE @ObjectPattern4 NVARCHAR(100) = N'%tipo%';
DECLARE @ObjectPattern5 NVARCHAR(100) = N'%rol%';

/* =========================
   1) Metadata discovery
   ========================= */
SELECT
    DB_NAME() AS DatabaseName,
    s.name AS SchemaName,
    o.name AS ObjectName,
    o.type_desc AS ObjectType
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE o.type IN ('U', 'V')
ORDER BY s.name, o.name;

SELECT
    c.TABLE_SCHEMA,
    c.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.NUMERIC_PRECISION,
    c.NUMERIC_SCALE,
    c.IS_NULLABLE,
    c.ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS c
ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION;

SELECT
    tc.TABLE_SCHEMA,
    tc.TABLE_NAME,
    tc.CONSTRAINT_NAME,
    tc.CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
ORDER BY tc.TABLE_SCHEMA, tc.TABLE_NAME, tc.CONSTRAINT_NAME;

SELECT
    sch.name AS SchemaName,
    t.name AS TableName,
    c.name AS ColumnName,
    ic.seed_value,
    ic.increment_value,
    ic.last_value
FROM sys.tables t
INNER JOIN sys.schemas sch ON sch.schema_id = t.schema_id
INNER JOIN sys.identity_columns ic ON ic.object_id = t.object_id
INNER JOIN sys.columns c ON c.object_id = t.object_id AND c.column_id = ic.column_id
ORDER BY sch.name, t.name;

/* =========================
   2) Candidate object identification
   ========================= */
SELECT
    s.name AS SchemaName,
    o.name AS ObjectName,
    o.type_desc AS ObjectType
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE o.type IN ('U', 'V')
  AND (
       o.name LIKE @ObjectPattern1 OR o.name LIKE @ObjectPattern2
    OR o.name LIKE @ObjectPattern3 OR o.name LIKE @ObjectPattern4
    OR o.name LIKE @ObjectPattern5
  )
ORDER BY s.name, o.name;

/* =========================
   3) Schema/columns extraction
   ========================= */
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    c.column_id AS OrdinalPosition,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLengthBytes,
    c.precision AS NumericPrecision,
    c.scale AS NumericScale,
    c.is_nullable,
    c.is_identity,
    c.is_computed
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.columns c ON c.object_id = t.object_id
INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
WHERE t.name IN (
    N'Roles',
    N'Estados',
    N'Cat_TiposJustificacion',
    N'Usuarios',
    N'Justificaciones_Encabezado',
    N'Justificaciones_Detalle'
)
ORDER BY s.name, t.name, c.column_id;

/* =========================
   4) Row-count sizing
   ========================= */
SELECT
    s.name AS SchemaName,
    t.name AS TableName,
    SUM(p.rows) AS ApproxRowCount
FROM sys.tables t
INNER JOIN sys.schemas s ON s.schema_id = t.schema_id
INNER JOIN sys.partitions p ON p.object_id = t.object_id AND p.index_id IN (0, 1)
WHERE t.name IN (
    N'Roles',
    N'Estados',
    N'Cat_TiposJustificacion',
    N'Usuarios',
    N'Justificaciones_Encabezado',
    N'Justificaciones_Detalle'
)
GROUP BY s.name, t.name
ORDER BY ApproxRowCount DESC, s.name, t.name;

/* =========================
   5) Sample-data extraction (TOP N)
   ========================= */
IF @EnableSampling = 1
BEGIN
    IF OBJECT_ID(N'dbo.Roles', N'U') IS NOT NULL
    BEGIN
        SELECT TOP (@TopN)
            RolID,
            NombreRol
        FROM dbo.Roles
        ORDER BY RolID;
    END;

    IF OBJECT_ID(N'dbo.Estados', N'U') IS NOT NULL
    BEGIN
        SELECT TOP (@TopN)
            EstadoID,
            Descripcion,
            Proceso
        FROM dbo.Estados
        ORDER BY EstadoID;
    END;

    IF OBJECT_ID(N'dbo.Cat_TiposJustificacion', N'U') IS NOT NULL
    BEGIN
        SELECT TOP (@TopN)
            TipoJustificacionID,
            Descripcion
        FROM dbo.Cat_TiposJustificacion
        ORDER BY TipoJustificacionID;
    END;

    IF OBJECT_ID(N'dbo.Usuarios', N'U') IS NOT NULL
    BEGIN
        SELECT TOP (@TopN)
            UsuarioID,
            Cedula,
            NombreCompleto,
            Correo,
            JefaturaID,
            UnidadID,
            RolID,
            Compania
        FROM dbo.Usuarios
        ORDER BY UsuarioID DESC;
    END;

    IF OBJECT_ID(N'dbo.Justificaciones_Encabezado', N'U') IS NOT NULL
    BEGIN
        SELECT TOP (@TopN)
            JustificacionID,
            UsuarioID,
            MotivoGeneral,
            EstadoID,
            FechaCreacion,
            AprobadorID,
            FechaAprobacion
        FROM dbo.Justificaciones_Encabezado
        WHERE (@FromDate IS NULL OR FechaCreacion >= @FromDate)
          AND (@ToDate IS NULL OR FechaCreacion < DATEADD(DAY, 1, @ToDate))
        ORDER BY FechaCreacion DESC, JustificacionID DESC;
    END;

    IF OBJECT_ID(N'dbo.Justificaciones_Detalle', N'U') IS NOT NULL
    BEGIN
        SELECT TOP (@TopN)
            DetalleID,
            JustificacionID,
            TipoJustificacionID,
            FechaMarca,
            ObservacionDetalle
        FROM dbo.Justificaciones_Detalle
        WHERE (@FromDate IS NULL OR FechaMarca >= @FromDate)
          AND (@ToDate IS NULL OR FechaMarca < DATEADD(DAY, 1, @ToDate))
        ORDER BY FechaMarca DESC, DetalleID DESC;
    END;
END;
ELSE
BEGIN
    SELECT
        DB_NAME() AS DatabaseName,
        CAST('Sampling deshabilitado por seguridad (set @EnableSampling = 1 para habilitar).' AS NVARCHAR(200)) AS Info;
END;

/* =========================
   6) Sample-data extraction (TOP N) - Dataset canonico operativo
   ========================= */
IF @EnableCanonicalExtract = 1
   AND OBJECT_ID(N'dbo.Justificaciones_Encabezado', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Justificaciones_Detalle', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Estados', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Usuarios', N'U') IS NOT NULL
   AND OBJECT_ID(N'dbo.Cat_TiposJustificacion', N'U') IS NOT NULL
BEGIN
    SELECT TOP (@TopNCanonico)
        je.JustificacionID,
        je.MotivoGeneral,
        je.EstadoID,
        e.Descripcion AS EstadoDescripcion,
        je.FechaCreacion,
        COUNT(jd.DetalleID) AS CantidadDetalles,
        je.AprobadorID,
        je.FechaAprobacion,
        u.UsuarioID AS FuncionarioID,
        u.NombreCompleto AS FuncionarioNombre,
        u.Cedula AS FuncionarioCedula,
        u.Compania,
        u.JefaturaID,
        j.NombreCompleto AS JefaturaNombre,
        MIN(tj.Descripcion) AS TipoPrincipal
    FROM dbo.Justificaciones_Encabezado je
    INNER JOIN dbo.Estados e ON e.EstadoID = je.EstadoID
    INNER JOIN dbo.Usuarios u ON u.UsuarioID = je.UsuarioID
    LEFT JOIN dbo.Usuarios j ON j.UsuarioID = u.JefaturaID
    LEFT JOIN dbo.Justificaciones_Detalle jd ON jd.JustificacionID = je.JustificacionID
    LEFT JOIN dbo.Cat_TiposJustificacion tj ON tj.TipoJustificacionID = jd.TipoJustificacionID
    WHERE (@FromDate IS NULL OR je.FechaCreacion >= @FromDate)
      AND (@ToDate IS NULL OR je.FechaCreacion < DATEADD(DAY, 1, @ToDate))
    GROUP BY
        je.JustificacionID,
        je.MotivoGeneral,
        je.EstadoID,
        e.Descripcion,
        je.FechaCreacion,
        je.AprobadorID,
        je.FechaAprobacion,
        u.UsuarioID,
        u.NombreCompleto,
        u.Cedula,
        u.Compania,
        u.JefaturaID,
        j.NombreCompleto
    ORDER BY je.FechaCreacion DESC, je.JustificacionID DESC;
END;
ELSE
BEGIN
    SELECT
        DB_NAME() AS DatabaseName,
        CAST('Extraccion canonica deshabilitada por seguridad (set @EnableCanonicalExtract = 1 para habilitar).' AS NVARCHAR(220)) AS Info;
END;
