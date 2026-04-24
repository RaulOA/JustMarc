/* ============================================================
   INTEGRA_CNP - Base Inicial Consolidada
   
   Responsabilidad: crear la base funcional mínima completa e idempotente.
   Incluye: esquemas, tablas internas, catálogos, auditoría y datos semilla.
   
   Convención: todos los objetos siguen PascalCase, esquemas funcionales,
   primary keys con sufijo "Id", columnas de auditoría estándar.
   
   Fecha generación: 2026-04-23
   Basado en: spec sql_consolidacion_dos_archivos_spec.md
   ============================================================ */

IF DB_ID('INTEGRA_CNP') IS NULL
BEGIN
    CREATE DATABASE INTEGRA_CNP;
END;
GO

USE INTEGRA_CNP;
GO

SET XACT_ABORT ON;
GO

/* =========================
   Creación de esquemas funcionales
   ========================= */

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Configuracion')
BEGIN
    EXEC('CREATE SCHEMA Configuracion');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'RecursosHumanos')
BEGIN
    EXEC('CREATE SCHEMA RecursosHumanos');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Operacion')
BEGIN
    EXEC('CREATE SCHEMA Operacion');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Auditoria')
BEGIN
    EXEC('CREATE SCHEMA Auditoria');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integracion')
BEGIN
    EXEC('CREATE SCHEMA Integracion');
END;
GO

/* =========================
   Catálogos - Esquema Configuracion
   ========================= */

IF OBJECT_ID('Configuracion.Rol', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.Rol (
        RolId INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(50) NOT NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_Rol_FechaHoraCreacion DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID('Configuracion.EstadoJustificacion', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.EstadoJustificacion (
        EstadoJustificacionId INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(100) NOT NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_EstadoJustificacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID('Configuracion.TipoJustificacion', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.TipoJustificacion (
        TipoJustificacionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(100) NOT NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_TipoJustificacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID('Configuracion.EstadoRegistro', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.EstadoRegistro (
        EstadoRegistroId INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(50) NOT NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_EstadoRegistro_FechaHoraCreacion DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID('Configuracion.TipoEventoAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.TipoEventoAuditoria (
        TipoEventoAuditoriaId INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(100) NOT NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_TipoEventoAuditoria_FechaHoraCreacion DEFAULT SYSUTCDATETIME()
    );
END;
GO

IF OBJECT_ID('Configuracion.ResultadoAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.ResultadoAuditoria (
        ResultadoAuditoriaId INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(50) NOT NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_ResultadoAuditoria_FechaHoraCreacion DEFAULT SYSUTCDATETIME()
    );
END;
GO

/* =========================
   Recursos Humanos - Esquema RecursosHumanos
   ========================= */

IF OBJECT_ID('RecursosHumanos.Usuario', 'U') IS NULL
BEGIN
    CREATE TABLE RecursosHumanos.Usuario (
        UsuarioId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Cedula VARCHAR(64) NOT NULL,
        NombreCompleto VARCHAR(150) NOT NULL,
        CorreoElectronico VARCHAR(100) NOT NULL,
        JefaturaId INT NULL,
        UnidadId INT NOT NULL,
        RolId INT NOT NULL,
        Compania VARCHAR(10) NOT NULL,
        EsActivo BIT NOT NULL CONSTRAINT DF_Usuario_EsActivo DEFAULT 1,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_Usuario_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        ModificadoPor VARCHAR(100) NULL,
        FechaHoraModificacion DATETIME2 NULL,
        CONSTRAINT FK_Usuario_RolId FOREIGN KEY (RolId) REFERENCES Configuracion.Rol(RolId),
        CONSTRAINT FK_Usuario_JefaturaId FOREIGN KEY (JefaturaId) REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT CK_Usuario_Compania CHECK (Compania IN ('CNP', 'FANAL'))
    );
END;
GO

/* Compatibilidad: ampliar Cedula si es necesario */
IF OBJECT_ID('RecursosHumanos.Usuario', 'U') IS NOT NULL
   AND COL_LENGTH('RecursosHumanos.Usuario', 'Cedula') IS NOT NULL
   AND COL_LENGTH('RecursosHumanos.Usuario', 'Cedula') < 64
BEGIN
    ALTER TABLE RecursosHumanos.Usuario ALTER COLUMN Cedula VARCHAR(64) NOT NULL;
END;
GO

IF OBJECT_ID('RecursosHumanos.EstructuraOrganizacional', 'U') IS NULL
BEGIN
    CREATE TABLE RecursosHumanos.EstructuraOrganizacional (
        EstructuraOrganizacionalId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Nombre VARCHAR(150) NOT NULL,
        CodigoOrigen VARCHAR(50) NULL,
        EstructuraPadreId INT NULL,
        EstadoRegistroId INT NOT NULL,
        VigenciaDesde DATETIME2 NULL,
        VigenciaHasta DATETIME2 NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_EstructuraOrganizacional_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_EstructuraOrganizacional_Padre FOREIGN KEY (EstructuraPadreId) 
            REFERENCES RecursosHumanos.EstructuraOrganizacional(EstructuraOrganizacionalId),
        CONSTRAINT FK_EstructuraOrganizacional_EstadoRegistroId FOREIGN KEY (EstadoRegistroId) 
            REFERENCES Configuracion.EstadoRegistro(EstadoRegistroId)
    );
END;
GO

/* =========================
   Operación - Esquema Operacion
   ========================= */

IF OBJECT_ID('Operacion.Justificacion', 'U') IS NULL
BEGIN
    CREATE TABLE Operacion.Justificacion (
        JustificacionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UsuarioId INT NOT NULL,
        MotivoGeneral VARCHAR(500) NOT NULL,
        ComentarioResolucion VARCHAR(500) NULL,
        RolResolucion VARCHAR(20) NULL,
        EstadoJustificacionId INT NOT NULL,
        FechaCreacion DATETIME2 NOT NULL CONSTRAINT DF_Justificacion_FechaCreacion DEFAULT SYSUTCDATETIME(),
        AprobadorId INT NULL,
        FechaAprobacion DATETIME2 NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_Justificacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        ModificadoPor VARCHAR(100) NULL,
        FechaHoraModificacion DATETIME2 NULL,
        CONSTRAINT FK_Justificacion_UsuarioId FOREIGN KEY (UsuarioId) 
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_Justificacion_EstadoJustificacionId FOREIGN KEY (EstadoJustificacionId) 
            REFERENCES Configuracion.EstadoJustificacion(EstadoJustificacionId),
        CONSTRAINT FK_Justificacion_AprobadorId FOREIGN KEY (AprobadorId) 
            REFERENCES RecursosHumanos.Usuario(UsuarioId)
    );
END;
GO

IF OBJECT_ID('Operacion.JustificacionDetalle', 'U') IS NULL
BEGIN
    CREATE TABLE Operacion.JustificacionDetalle (
        JustificacionDetalleId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        JustificacionId INT NOT NULL,
        TipoJustificacionId INT NOT NULL,
        FechaMarca DATE NOT NULL,
        ObservacionDetalle VARCHAR(250) NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_JustificacionDetalle_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_JustificacionDetalle_JustificacionId FOREIGN KEY (JustificacionId) 
            REFERENCES Operacion.Justificacion(JustificacionId),
        CONSTRAINT FK_JustificacionDetalle_TipoJustificacionId FOREIGN KEY (TipoJustificacionId) 
            REFERENCES Configuracion.TipoJustificacion(TipoJustificacionId)
    );
END;
GO

IF OBJECT_ID('Operacion.JerarquiaAprobacion', 'U') IS NULL
BEGIN
    CREATE TABLE Operacion.JerarquiaAprobacion (
        JerarquiaAprobacionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AprobadorUsuarioId INT NOT NULL,
        EstructuraOrganizacionalId INT NOT NULL,
        NivelAprobacion INT NOT NULL,
        TipoRelacion VARCHAR(20) NOT NULL,
        EstadoRegistroId INT NOT NULL,
        VigenciaDesde DATETIME2 NOT NULL,
        VigenciaHasta DATETIME2 NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_JerarquiaAprobacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_JerarquiaAprobacion_AprobadorUsuarioId FOREIGN KEY (AprobadorUsuarioId) 
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_JerarquiaAprobacion_EstructuraOrganizacionalId FOREIGN KEY (EstructuraOrganizacionalId) 
            REFERENCES RecursosHumanos.EstructuraOrganizacional(EstructuraOrganizacionalId),
        CONSTRAINT FK_JerarquiaAprobacion_EstadoRegistroId FOREIGN KEY (EstadoRegistroId) 
            REFERENCES Configuracion.EstadoRegistro(EstadoRegistroId),
        CONSTRAINT CK_JerarquiaAprobacion_TipoRelacion CHECK (TipoRelacion IN ('Vertical', 'Horizontal'))
    );
END;
GO

IF OBJECT_ID('Operacion.DelegacionAprobacion', 'U') IS NULL
BEGIN
    CREATE TABLE Operacion.DelegacionAprobacion (
        DelegacionAprobacionId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DeleganteUsuarioId INT NOT NULL,
        DelegadoUsuarioId INT NOT NULL,
        JerarquiaAprobacionId INT NULL,
        Motivo VARCHAR(250) NULL,
        EstadoRegistroId INT NOT NULL,
        VigenciaDesde DATETIME2 NOT NULL,
        VigenciaHasta DATETIME2 NULL,
        CreadoPor VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2 NOT NULL CONSTRAINT DF_DelegacionAprobacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_DelegacionAprobacion_DeleganteUsuarioId FOREIGN KEY (DeleganteUsuarioId) 
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_DelegacionAprobacion_DelegadoUsuarioId FOREIGN KEY (DelegadoUsuarioId) 
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_DelegacionAprobacion_JerarquiaAprobacionId FOREIGN KEY (JerarquiaAprobacionId) 
            REFERENCES Operacion.JerarquiaAprobacion(JerarquiaAprobacionId),
        CONSTRAINT FK_DelegacionAprobacion_EstadoRegistroId FOREIGN KEY (EstadoRegistroId) 
            REFERENCES Configuracion.EstadoRegistro(EstadoRegistroId)
    );
END;
GO

/* =========================
   Auditoría - Esquema Auditoria
   ========================= */

IF OBJECT_ID('Auditoria.EventoAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE Auditoria.EventoAuditoria (
        EventoAuditoriaId BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FechaEvento DATETIME2 NOT NULL CONSTRAINT DF_EventoAuditoria_FechaEvento DEFAULT SYSUTCDATETIME(),
        UsuarioId INT NULL,
        NombreUsuario VARCHAR(150) NOT NULL,
        RolCodigo VARCHAR(20) NOT NULL,
        TipoEventoAuditoriaId INT NOT NULL,
        DescripcionEvento VARCHAR(500) NOT NULL,
        ResultadoAuditoriaId INT NOT NULL,
        ReferenciaFuncional VARCHAR(100) NULL,
        PayloadResumen VARCHAR(1000) NULL,
        CONSTRAINT FK_EventoAuditoria_UsuarioId FOREIGN KEY (UsuarioId) 
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_EventoAuditoria_TipoEventoAuditoriaId FOREIGN KEY (TipoEventoAuditoriaId) 
            REFERENCES Configuracion.TipoEventoAuditoria(TipoEventoAuditoriaId),
        CONSTRAINT FK_EventoAuditoria_ResultadoAuditoriaId FOREIGN KEY (ResultadoAuditoriaId) 
            REFERENCES Configuracion.ResultadoAuditoria(ResultadoAuditoriaId)
    );
END;
GO

IF OBJECT_ID('Auditoria.ErrorApi', 'U') IS NULL
BEGIN
    CREATE TABLE Auditoria.ErrorApi (
        ErrorApiId INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CorrelationId UNIQUEIDENTIFIER NOT NULL DEFAULT NEWID(),
        FechaUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME(),
        MetodoHttp VARCHAR(10) NOT NULL,
        Endpoint NVARCHAR(500) NOT NULL,
        CodigoEstado INT NOT NULL,
        TipoError VARCHAR(200) NOT NULL,
        Mensaje NVARCHAR(1000) NOT NULL,
        StackTrace NVARCHAR(MAX) NULL,
        UsuarioSolicitante VARCHAR(150) NULL,
        DireccionIP VARCHAR(45) NULL
    );
END;
GO

/* =========================
   Datos semilla - Catálogos
   ========================= */

MERGE INTO Configuracion.Rol AS tgt
USING (
    SELECT 1 AS RolId, 'Funcionario' AS Descripcion
    UNION ALL SELECT 2, 'Jefatura'
    UNION ALL SELECT 3, 'RRHH'
    UNION ALL SELECT 4, 'Administrador'
) AS src
ON tgt.RolId = src.RolId
WHEN NOT MATCHED THEN
    INSERT (RolId, Descripcion, CreadoPor)
    VALUES (src.RolId, src.Descripcion, 'seed');
GO

MERGE INTO Configuracion.EstadoJustificacion AS tgt
USING (
    SELECT 1 AS EstadoJustificacionId, 'Pendiente Jefatura' AS Descripcion
    UNION ALL SELECT 2, 'Aprobada'
    UNION ALL SELECT 3, 'Rechazada'
) AS src
ON tgt.EstadoJustificacionId = src.EstadoJustificacionId
WHEN NOT MATCHED THEN
    INSERT (EstadoJustificacionId, Descripcion, CreadoPor)
    VALUES (src.EstadoJustificacionId, src.Descripcion, 'seed');
GO

IF NOT EXISTS (SELECT 1 FROM Configuracion.TipoJustificacion)
BEGIN
    INSERT INTO Configuracion.TipoJustificacion (Descripcion, CreadoPor)
    VALUES
    ('Marca Tardía', 'seed'),
    ('Omisión Marca de Entrada', 'seed'),
    ('Omisión Marca de Salida', 'seed'),
    ('Marca antes Hora de Salida', 'seed'),
    ('Ausencia', 'seed');
END;
GO

MERGE INTO Configuracion.EstadoRegistro AS tgt
USING (
    SELECT 1 AS EstadoRegistroId, 'Activo' AS Descripcion
    UNION ALL SELECT 2, 'Inactivo'
) AS src
ON tgt.EstadoRegistroId = src.EstadoRegistroId
WHEN NOT MATCHED THEN
    INSERT (EstadoRegistroId, Descripcion, CreadoPor)
    VALUES (src.EstadoRegistroId, src.Descripcion, 'seed');
GO

MERGE INTO Configuracion.TipoEventoAuditoria AS tgt
USING (
    SELECT 1 AS TipoEventoAuditoriaId, 'CreacionJustificacion' AS Descripcion
    UNION ALL SELECT 2, 'ResolucionJustificacionAprobada'
    UNION ALL SELECT 3, 'ResolucionJustificacionRechazada'
    UNION ALL SELECT 4, 'AltaJerarquia'
    UNION ALL SELECT 5, 'CambioEstadoJerarquia'
    UNION ALL SELECT 6, 'AltaDelegacion'
    UNION ALL SELECT 7, 'CambioEstadoDelegacion'
) AS src
ON tgt.TipoEventoAuditoriaId = src.TipoEventoAuditoriaId
WHEN NOT MATCHED THEN
    INSERT (TipoEventoAuditoriaId, Descripcion, CreadoPor)
    VALUES (src.TipoEventoAuditoriaId, src.Descripcion, 'seed');
GO

MERGE INTO Configuracion.ResultadoAuditoria AS tgt
USING (
    SELECT 1 AS ResultadoAuditoriaId, 'Exito' AS Descripcion
    UNION ALL SELECT 2, 'Fallo'
    UNION ALL SELECT 3, 'Denegado'
) AS src
ON tgt.ResultadoAuditoriaId = src.ResultadoAuditoriaId
WHEN NOT MATCHED THEN
    INSERT (ResultadoAuditoriaId, Descripcion, CreadoPor)
    VALUES (src.ResultadoAuditoriaId, src.Descripcion, 'seed');
GO

/* =========================
   Índices de performance
   ========================= */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_Cedula' AND object_id = OBJECT_ID('RecursosHumanos.Usuario'))
BEGIN
    CREATE INDEX IX_Usuario_Cedula
        ON RecursosHumanos.Usuario (Cedula);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_RolId_EsActivo' AND object_id = OBJECT_ID('RecursosHumanos.Usuario'))
BEGIN
    CREATE INDEX IX_Usuario_RolId_EsActivo
        ON RecursosHumanos.Usuario (RolId, EsActivo);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EstructuraOrganizacional_CodigoOrigen_EstadoRegistroId' AND object_id = OBJECT_ID('RecursosHumanos.EstructuraOrganizacional'))
BEGIN
    CREATE INDEX IX_EstructuraOrganizacional_CodigoOrigen_EstadoRegistroId
        ON RecursosHumanos.EstructuraOrganizacional (CodigoOrigen, EstadoRegistroId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Justificacion_UsuarioId' AND object_id = OBJECT_ID('Operacion.Justificacion'))
BEGIN
    CREATE INDEX IX_Justificacion_UsuarioId
        ON Operacion.Justificacion (UsuarioId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Justificacion_EstadoJustificacionId' AND object_id = OBJECT_ID('Operacion.Justificacion'))
BEGIN
    CREATE INDEX IX_Justificacion_EstadoJustificacionId
        ON Operacion.Justificacion (EstadoJustificacionId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Justificacion_AprobadorId' AND object_id = OBJECT_ID('Operacion.Justificacion'))
BEGIN
    CREATE INDEX IX_Justificacion_AprobadorId
        ON Operacion.Justificacion (AprobadorId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JustificacionDetalle_JustificacionId' AND object_id = OBJECT_ID('Operacion.JustificacionDetalle'))
BEGIN
    CREATE INDEX IX_JustificacionDetalle_JustificacionId
        ON Operacion.JustificacionDetalle (JustificacionId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JustificacionDetalle_TipoJustificacionId' AND object_id = OBJECT_ID('Operacion.JustificacionDetalle'))
BEGIN
    CREATE INDEX IX_JustificacionDetalle_TipoJustificacionId
        ON Operacion.JustificacionDetalle (TipoJustificacionId);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JerarquiaAprobacion_AprobadorUsuarioId_EstructuraOrganizacionalId' AND object_id = OBJECT_ID('Operacion.JerarquiaAprobacion'))
BEGIN
    CREATE INDEX IX_JerarquiaAprobacion_AprobadorUsuarioId_EstructuraOrganizacionalId
        ON Operacion.JerarquiaAprobacion (AprobadorUsuarioId, EstructuraOrganizacionalId, EstadoRegistroId, VigenciaDesde, VigenciaHasta);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DelegacionAprobacion_DelegadoUsuarioId_Vigencia' AND object_id = OBJECT_ID('Operacion.DelegacionAprobacion'))
BEGIN
    CREATE INDEX IX_DelegacionAprobacion_DelegadoUsuarioId_Vigencia
        ON Operacion.DelegacionAprobacion (DelegadoUsuarioId, EstadoRegistroId, VigenciaDesde, VigenciaHasta);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventoAuditoria_FechaEvento' AND object_id = OBJECT_ID('Auditoria.EventoAuditoria'))
BEGIN
    CREATE INDEX IX_EventoAuditoria_FechaEvento
        ON Auditoria.EventoAuditoria (FechaEvento DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventoAuditoria_TipoEventoAuditoriaId_ResultadoAuditoriaId' AND object_id = OBJECT_ID('Auditoria.EventoAuditoria'))
BEGIN
    CREATE INDEX IX_EventoAuditoria_TipoEventoAuditoriaId_ResultadoAuditoriaId
        ON Auditoria.EventoAuditoria (TipoEventoAuditoriaId, ResultadoAuditoriaId, FechaEvento DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ErrorApi_FechaUtc' AND object_id = OBJECT_ID('Auditoria.ErrorApi'))
BEGIN
    CREATE INDEX IX_ErrorApi_FechaUtc
        ON Auditoria.ErrorApi (FechaUtc DESC);
END;
GO

/* =========================
   Fin script inicial
   ========================= */
PRINT 'Base inicial INTEGRA_CNP creada exitosamente.';
GO
