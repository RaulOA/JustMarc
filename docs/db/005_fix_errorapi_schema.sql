USE INTEGRA_CNP;
GO

-- ============================================================
-- 005_fix_errorapi_schema.sql
-- Alinea Auditoria.ErrorApi con el contrato del repositorio C#.
-- Script idempotente: verifica existencia antes de cada ALTER.
-- ============================================================

-- ── Renombres de columnas (solo si el nombre antiguo aún existe) ──────────────

-- MetodoHttp → HttpMethod
IF COL_LENGTH('Auditoria.ErrorApi', 'MetodoHttp') IS NOT NULL
    AND COL_LENGTH('Auditoria.ErrorApi', 'HttpMethod') IS NULL
BEGIN
    EXEC sp_rename 'Auditoria.ErrorApi.MetodoHttp', 'HttpMethod', 'COLUMN';
    PRINT 'Columna renombrada: MetodoHttp → HttpMethod';
END
GO

-- CodigoEstado → StatusCode
IF COL_LENGTH('Auditoria.ErrorApi', 'CodigoEstado') IS NOT NULL
    AND COL_LENGTH('Auditoria.ErrorApi', 'StatusCode') IS NULL
BEGIN
    EXEC sp_rename 'Auditoria.ErrorApi.CodigoEstado', 'StatusCode', 'COLUMN';
    PRINT 'Columna renombrada: CodigoEstado → StatusCode';
END
GO

-- UsuarioSolicitante → UsuarioID
IF COL_LENGTH('Auditoria.ErrorApi', 'UsuarioSolicitante') IS NOT NULL
    AND COL_LENGTH('Auditoria.ErrorApi', 'UsuarioID') IS NULL
BEGIN
    EXEC sp_rename 'Auditoria.ErrorApi.UsuarioSolicitante', 'UsuarioID', 'COLUMN';
    PRINT 'Columna renombrada: UsuarioSolicitante → UsuarioID';
END
GO

-- DireccionIP → Ip
IF COL_LENGTH('Auditoria.ErrorApi', 'DireccionIP') IS NOT NULL
    AND COL_LENGTH('Auditoria.ErrorApi', 'Ip') IS NULL
BEGIN
    EXEC sp_rename 'Auditoria.ErrorApi.DireccionIP', 'Ip', 'COLUMN';
    PRINT 'Columna renombrada: DireccionIP → Ip';
END
GO

-- ── Columnas nuevas (solo si no existen) ─────────────────────────────────────

IF COL_LENGTH('Auditoria.ErrorApi', 'RolUsuario') IS NULL
BEGIN
    ALTER TABLE Auditoria.ErrorApi
        ADD RolUsuario VARCHAR(50) NULL;
    PRINT 'Columna agregada: RolUsuario';
END
GO

IF COL_LENGTH('Auditoria.ErrorApi', 'Entorno') IS NULL
BEGIN
    ALTER TABLE Auditoria.ErrorApi
        ADD Entorno VARCHAR(50) NULL;
    PRINT 'Columna agregada: Entorno';
END
GO

IF COL_LENGTH('Auditoria.ErrorApi', 'UserAgent') IS NULL
BEGIN
    ALTER TABLE Auditoria.ErrorApi
        ADD UserAgent NVARCHAR(500) NULL;
    PRINT 'Columna agregada: UserAgent';
END
GO

PRINT '005_fix_errorapi_schema.sql completado.';
GO
