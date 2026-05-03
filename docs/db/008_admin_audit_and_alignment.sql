/* ============================================================
   INTEGRA_CNP - Auditoria Admin y Alineamientos

   Objetivo:
   - Crear auditoria detallada de acciones administrativas.
   - Alinear columnas de trazabilidad para inserciones admin.
   - Extender catalogo de tipo de evento de auditoria.

   Fecha: 2026-05-03
   ============================================================ */

IF DB_ID('INTEGRA_CNP') IS NULL
BEGIN
    RAISERROR('La base de datos INTEGRA_CNP no existe. Ejecute primero 001_integra_marcas_base_inicial.sql', 16, 1);
    RETURN;
END;
GO

USE INTEGRA_CNP;
GO

SET XACT_ABORT ON;
GO

/* =========================
   Alineamiento de columnas CreadoPor para objetos admin
   ========================= */

IF OBJECT_ID('Operacion.JerarquiaAprobacion', 'U') IS NOT NULL
   AND COL_LENGTH('Operacion.JerarquiaAprobacion', 'CreadoPor') IS NULL
BEGIN
    ALTER TABLE Operacion.JerarquiaAprobacion
        ADD CreadoPor VARCHAR(100) NULL;

    IF COL_LENGTH('Operacion.JerarquiaAprobacion', 'Usr_Registro') IS NOT NULL
    BEGIN
        EXEC('UPDATE Operacion.JerarquiaAprobacion SET CreadoPor = NULLIF(CAST(Usr_Registro AS VARCHAR(100)), '''') WHERE CreadoPor IS NULL;');
    END;

    UPDATE Operacion.JerarquiaAprobacion
        SET CreadoPor = COALESCE(CreadoPor, 'migracion_008')
    WHERE CreadoPor IS NULL;

    ALTER TABLE Operacion.JerarquiaAprobacion
        ALTER COLUMN CreadoPor VARCHAR(100) NOT NULL;
END;
GO

IF OBJECT_ID('Operacion.DelegacionAprobacion', 'U') IS NOT NULL
   AND COL_LENGTH('Operacion.DelegacionAprobacion', 'CreadoPor') IS NULL
BEGIN
    ALTER TABLE Operacion.DelegacionAprobacion
        ADD CreadoPor VARCHAR(100) NULL;

    IF COL_LENGTH('Operacion.DelegacionAprobacion', 'Usr_Registro') IS NOT NULL
    BEGIN
        EXEC('UPDATE Operacion.DelegacionAprobacion SET CreadoPor = NULLIF(CAST(Usr_Registro AS VARCHAR(100)), '''') WHERE CreadoPor IS NULL;');
    END;

    UPDATE Operacion.DelegacionAprobacion
        SET CreadoPor = COALESCE(CreadoPor, 'migracion_008')
    WHERE CreadoPor IS NULL;

    ALTER TABLE Operacion.DelegacionAprobacion
        ALTER COLUMN CreadoPor VARCHAR(100) NOT NULL;
END;
GO

/* =========================
   Tabla de auditoria detallada admin
   ========================= */

IF OBJECT_ID('Auditoria.AdminAccionAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE Auditoria.AdminAccionAuditoria
    (
        AdminAccionAuditoriaId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FechaEventoUtc DATETIME2(3) NOT NULL CONSTRAINT DF_AdminAccionAuditoria_FechaEventoUtc DEFAULT SYSUTCDATETIME(),
        CorrelationId NVARCHAR(100) NULL,
        UsuarioActorId INT NOT NULL,
        RolActorCodigo VARCHAR(20) NOT NULL,
        EntidadObjetivo VARCHAR(80) NOT NULL,
        EntidadObjetivoId VARCHAR(80) NOT NULL,
        Accion VARCHAR(40) NOT NULL,
        ResultadoAuditoriaId INT NOT NULL,
        Descripcion VARCHAR(500) NOT NULL,
        ValoresAnteriores NVARCHAR(MAX) NULL,
        ValoresNuevos NVARCHAR(MAX) NULL,
        Metadata NVARCHAR(MAX) NULL,
        CONSTRAINT FK_AdminAccionAuditoria_UsuarioActorId FOREIGN KEY (UsuarioActorId)
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_AdminAccionAuditoria_ResultadoAuditoriaId FOREIGN KEY (ResultadoAuditoriaId)
            REFERENCES Configuracion.ResultadoAuditoria(ResultadoAuditoriaId)
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AdminAccionAuditoria_FechaEventoUtc' AND object_id = OBJECT_ID('Auditoria.AdminAccionAuditoria'))
BEGIN
    CREATE INDEX IX_AdminAccionAuditoria_FechaEventoUtc
        ON Auditoria.AdminAccionAuditoria (FechaEventoUtc DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AdminAccionAuditoria_Entidad' AND object_id = OBJECT_ID('Auditoria.AdminAccionAuditoria'))
BEGIN
    CREATE INDEX IX_AdminAccionAuditoria_Entidad
        ON Auditoria.AdminAccionAuditoria (EntidadObjetivo, EntidadObjetivoId, FechaEventoUtc DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AdminAccionAuditoria_Actor' AND object_id = OBJECT_ID('Auditoria.AdminAccionAuditoria'))
BEGIN
    CREATE INDEX IX_AdminAccionAuditoria_Actor
        ON Auditoria.AdminAccionAuditoria (UsuarioActorId, FechaEventoUtc DESC);
END;
GO

/* =========================
   Catalogo tipo evento auditoria (nuevos tipos admin)
   ========================= */

MERGE INTO Configuracion.TipoEventoAuditoria AS tgt
USING (
    SELECT 8 AS TipoEventoAuditoriaId, 'ActualizacionJerarquia' AS Descripcion
    UNION ALL SELECT 9, 'ActualizacionDelegacion'
    UNION ALL SELECT 10, 'ActualizacionAsignacionUsuarioODependencia'
    UNION ALL SELECT 11, 'CambioEstadoUsuario'
) AS src
ON tgt.TipoEventoAuditoriaId = src.TipoEventoAuditoriaId
WHEN NOT MATCHED THEN
    INSERT (TipoEventoAuditoriaId, Descripcion, CreadoPor)
    VALUES (src.TipoEventoAuditoriaId, src.Descripcion, 'migracion_008');
GO

PRINT 'Script 008 ejecutado correctamente.';
GO
