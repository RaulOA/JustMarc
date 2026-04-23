/* ============================================================================
   WIZDOM - Extraccion Targeted Minima (SQL Server)

   Objetivo:
   - Obtener SOLO los datos clave para sincronizacion de usuarios/jefaturas (RF-01).
   - Evitar barridos amplios en produccion.

   Run-order:
   1) Ejecutar tal cual (modo seguro).
   2) Exportar cada result set a CSV UTF-8.
   3) Solo si se necesita volumen, activar @EnableCount.

   Seguridad:
   - 100% lectura (solo SELECT).
   - Sin DML/DDL.
   ============================================================================ */

USE [WIZDOM];
GO

SET NOCOUNT ON;
SET DEADLOCK_PRIORITY LOW;
SET LOCK_TIMEOUT 5000;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;

DECLARE @TopN INT = 500;
DECLARE @EnableCount BIT = 0;

/* --------------------------------------------------------------------------
   1) Validacion rapida de objetos esperados
   -------------------------------------------------------------------------- */
SELECT
    DB_NAME() AS DatabaseName,
    s.name AS SchemaName,
    o.name AS ObjectName,
    o.type_desc AS ObjectType
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE o.type = 'V'
  AND o.name IN (N'optec1empleado', N'RH_FUNCIONARIOS', N'VW_EMPLEADO_RH_TMP', N'relaciones_empleado')
ORDER BY o.name;

/* --------------------------------------------------------------------------
   2) Estructura de columnas de vistas candidatas
   -------------------------------------------------------------------------- */
SELECT
    c.TABLE_SCHEMA,
    c.TABLE_NAME,
    c.ORDINAL_POSITION,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.NUMERIC_PRECISION,
    c.NUMERIC_SCALE,
    c.IS_NULLABLE
FROM INFORMATION_SCHEMA.COLUMNS c
WHERE c.TABLE_NAME IN (N'optec1empleado', N'RH_FUNCIONARIOS', N'VW_EMPLEADO_RH_TMP', N'relaciones_empleado')
ORDER BY c.TABLE_NAME, c.ORDINAL_POSITION;

/* --------------------------------------------------------------------------
   3) Muestra funcional principal - optec1empleado
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.optec1empleado', N'V') IS NOT NULL
BEGIN
    SELECT TOP (@TopN)
        compania,
        codigo_empleado,
        numero_identificacion,
        primer_apellido,
        segundo_apellido,
        nombre,
        correo_electronico_principal,
        codigo_jefe,
        codigo_empleado_jefe_funcional,
        codigo_nodo_organigrama,
        codigo_puesto,
        estado_empleado,
        condicion_laboral,
        fecha_ingreso,
        fecha_egreso
    FROM dbo.optec1empleado
    ORDER BY codigo_empleado;
END;

/* --------------------------------------------------------------------------
   4) Muestra funcional alternativa - RH_FUNCIONARIOS
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.RH_FUNCIONARIOS', N'V') IS NOT NULL
BEGIN
    SELECT TOP (@TopN)
        COD_FUNCIONARIO,
        DES_CEDULA,
        DES_APELLIDO1,
        DES_APELLIDO2,
        DES_NOMBRE,
        DES_EMAIL,
        COD_RESPONSABLE,
        COD_CENTRO,
        COD_PUESTO,
        IND_ESTADO,
        FEC_INGRESO,
        FEC_SALIDA
    FROM dbo.RH_FUNCIONARIOS
    ORDER BY COD_FUNCIONARIO;
END;

/* --------------------------------------------------------------------------
   5) Relacion empleado-jefatura (si existe vista de relaciones)
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.relaciones_empleado', N'V') IS NOT NULL
BEGIN
    SELECT TOP (@TopN)
        compania,
        codigo_empleado,
        codigo_empleado_relacion,
        tipo_relacion
    FROM dbo.relaciones_empleado
    ORDER BY codigo_empleado;
END;

/* --------------------------------------------------------------------------
   6) Perfil de companias detectadas
   -------------------------------------------------------------------------- */
IF OBJECT_ID(N'dbo.optec1empleado', N'V') IS NOT NULL
BEGIN
    SELECT
        compania,
        COUNT_BIG(*) AS Funcionarios
    FROM dbo.optec1empleado
    GROUP BY compania
    ORDER BY compania;
END;

/* --------------------------------------------------------------------------
   7) Conteos opcionales (desactivado por defecto)
   -------------------------------------------------------------------------- */
IF @EnableCount = 1
BEGIN
    IF OBJECT_ID(N'dbo.optec1empleado', N'V') IS NOT NULL
        SELECT COUNT_BIG(*) AS TotalRows_optec1empleado FROM dbo.optec1empleado;

    IF OBJECT_ID(N'dbo.RH_FUNCIONARIOS', N'V') IS NOT NULL
        SELECT COUNT_BIG(*) AS TotalRows_RH_FUNCIONARIOS FROM dbo.RH_FUNCIONARIOS;

    IF OBJECT_ID(N'dbo.relaciones_empleado', N'V') IS NOT NULL
        SELECT COUNT_BIG(*) AS TotalRows_relaciones_empleado FROM dbo.relaciones_empleado;
END;
ELSE
BEGIN
    SELECT CAST('Conteos globales deshabilitados por seguridad (@EnableCount = 0).' AS NVARCHAR(150)) AS Info;
END;
