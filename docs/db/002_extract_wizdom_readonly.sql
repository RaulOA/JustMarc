/* ============================================================================
   WIZDOM - Extraccion Read-Only (SQL Server)

   Run-order (compacto):
   1) Ajustar variables en "Parametros".
   2) Ejecutar secciones 1-4 para descubrir metadatos, candidatos, columnas y volumen.
   3) Ejecutar seccion 5 para extraer muestras TOP(N) de objetos candidatos.
   4) Exportar resultados a CSV UTF-8 por result set.

   Seguridad:
   - Script 100% solo lectura: usa SELECT y metadata de sistema.
   - No usa INSERT/UPDATE/DELETE/MERGE/CREATE/ALTER/DROP/TRUNCATE.
   ============================================================================ */

USE [WIZDOM];
GO

SET NOCOUNT ON;
SET DEADLOCK_PRIORITY LOW;
SET LOCK_TIMEOUT 5000;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

/* =========================
   Parametros
   ========================= */
DECLARE @TopN INT = 500;
DECLARE @MaxSampleObjects INT = 5;
DECLARE @EnableSampling BIT = 0;

DECLARE @ObjectPattern1 NVARCHAR(100) = N'%func%';
DECLARE @ObjectPattern2 NVARCHAR(100) = N'%emplead%';
DECLARE @ObjectPattern3 NVARCHAR(100) = N'%jef%';
DECLARE @ObjectPattern4 NVARCHAR(100) = N'%unidad%';
DECLARE @ObjectPattern5 NVARCHAR(100) = N'%compa%';

DECLARE @ColumnPattern1 NVARCHAR(100) = N'%ced%';
DECLARE @ColumnPattern2 NVARCHAR(100) = N'%correo%';
DECLARE @ColumnPattern3 NVARCHAR(100) = N'%mail%';
DECLARE @ColumnPattern4 NVARCHAR(100) = N'%jef%';
DECLARE @ColumnPattern5 NVARCHAR(100) = N'%compa%';

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
    i.name AS IndexName,
    i.type_desc AS IndexType,
    i.is_unique,
    i.is_primary_key
FROM sys.tables t
INNER JOIN sys.schemas sch ON sch.schema_id = t.schema_id
LEFT JOIN sys.indexes i ON i.object_id = t.object_id
ORDER BY sch.name, t.name, i.index_id;

/* =========================
   2) Candidate object identification
   ========================= */
;WITH ObjectMatches AS (
    SELECT
        o.object_id,
        s.name AS SchemaName,
        o.name AS ObjectName,
        o.type_desc AS ObjectType,
        CASE
            WHEN o.name LIKE @ObjectPattern1 OR o.name LIKE @ObjectPattern2
              OR o.name LIKE @ObjectPattern3 OR o.name LIKE @ObjectPattern4
              OR o.name LIKE @ObjectPattern5
            THEN 1 ELSE 0
        END AS NameMatch
    FROM sys.objects o
    INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
    WHERE o.type IN ('U', 'V')
),
ColumnMatches AS (
    SELECT DISTINCT
        c.object_id
    FROM sys.columns c
    WHERE c.name LIKE @ColumnPattern1 OR c.name LIKE @ColumnPattern2
       OR c.name LIKE @ColumnPattern3 OR c.name LIKE @ColumnPattern4
       OR c.name LIKE @ColumnPattern5
)
SELECT
    om.SchemaName,
    om.ObjectName,
    om.ObjectType,
    CASE WHEN cm.object_id IS NOT NULL THEN 1 ELSE 0 END AS HasRelevantColumns,
    om.NameMatch,
    CAST(om.NameMatch + CASE WHEN cm.object_id IS NOT NULL THEN 1 ELSE 0 END AS INT) AS CandidateScore
FROM ObjectMatches om
LEFT JOIN ColumnMatches cm ON cm.object_id = om.object_id
WHERE om.NameMatch = 1 OR cm.object_id IS NOT NULL
ORDER BY CandidateScore DESC, om.SchemaName, om.ObjectName;

/* =========================
   3) Schema/columns extraction
   ========================= */
;WITH CandidateObjects AS (
    SELECT
        o.object_id,
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
)
SELECT
    co.SchemaName,
    co.ObjectName,
    co.ObjectType,
    c.column_id AS OrdinalPosition,
    c.name AS ColumnName,
    ty.name AS DataType,
    c.max_length AS MaxLengthBytes,
    c.precision AS NumericPrecision,
    c.scale AS NumericScale,
    c.is_nullable,
    c.is_identity,
    c.is_computed
FROM CandidateObjects co
INNER JOIN sys.columns c ON c.object_id = co.object_id
INNER JOIN sys.types ty ON ty.user_type_id = c.user_type_id
ORDER BY co.SchemaName, co.ObjectName, c.column_id;

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
WHERE t.name LIKE @ObjectPattern1 OR t.name LIKE @ObjectPattern2
   OR t.name LIKE @ObjectPattern3 OR t.name LIKE @ObjectPattern4
   OR t.name LIKE @ObjectPattern5
GROUP BY s.name, t.name
ORDER BY ApproxRowCount DESC, s.name, t.name;

/* =========================
   5) Sample-data extraction (TOP N)
   ========================= */
DECLARE @SchemaName SYSNAME;
DECLARE @ObjectName SYSNAME;
DECLARE @Sql NVARCHAR(MAX);

IF @EnableSampling = 1
BEGIN
    DECLARE sample_cursor CURSOR LOCAL FAST_FORWARD FOR
    SELECT TOP (@MaxSampleObjects)
        s.name AS SchemaName,
        o.name AS ObjectName
    FROM sys.objects o
    INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
    WHERE o.type IN ('U', 'V')
      AND (
           o.name LIKE @ObjectPattern1 OR o.name LIKE @ObjectPattern2
        OR o.name LIKE @ObjectPattern3 OR o.name LIKE @ObjectPattern4
        OR o.name LIKE @ObjectPattern5
      )
    ORDER BY s.name, o.name;

    OPEN sample_cursor;
    FETCH NEXT FROM sample_cursor INTO @SchemaName, @ObjectName;

    WHILE @@FETCH_STATUS = 0
    BEGIN
        SELECT
            DB_NAME() AS DatabaseName,
            @SchemaName AS SampleSchema,
            @ObjectName AS SampleObject,
            @TopN AS TopNRequested;

        SET @Sql = N'SELECT TOP (@TopN) * FROM '
            + QUOTENAME(@SchemaName) + N'.' + QUOTENAME(@ObjectName)
            + N' ORDER BY (SELECT NULL);';

        EXEC sys.sp_executesql @Sql, N'@TopN INT', @TopN = @TopN;

        FETCH NEXT FROM sample_cursor INTO @SchemaName, @ObjectName;
    END;

    CLOSE sample_cursor;
    DEALLOCATE sample_cursor;
END;
ELSE
BEGIN
    SELECT
        DB_NAME() AS DatabaseName,
        CAST('Sampling deshabilitado por seguridad (set @EnableSampling = 1 para habilitar).' AS NVARCHAR(200)) AS Info;
END;
