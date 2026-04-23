/* ============================================================
   INTEGRA_CNP - Script Inicial de Esquema
   Basado en PRP_Justificacion_Marcas.md
   ============================================================ */

IF DB_ID('INTEGRA_CNP') IS NULL
BEGIN
    CREATE DATABASE INTEGRA_CNP;
END;
GO

USE INTEGRA_CNP;
GO

/* =========================
   Catálogos
   ========================= */
IF OBJECT_ID('dbo.Roles', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Roles (
        RolID INT NOT NULL PRIMARY KEY,
        NombreRol VARCHAR(50) NOT NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_Roles_Fec_Registro DEFAULT GETDATE()
    );
END;
GO

IF OBJECT_ID('dbo.Estados', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Estados (
        EstadoID INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(100) NOT NULL,
        Proceso VARCHAR(50) NOT NULL
    );
END;
GO

IF OBJECT_ID('dbo.Cat_TiposJustificacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cat_TiposJustificacion (
        TipoJustificacionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(100) NOT NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_TipoJust_Fec_Registro DEFAULT GETDATE()
    );
END;
GO

/* =========================
   Usuarios
   ========================= */
IF OBJECT_ID('dbo.Usuarios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Usuarios (
        UsuarioID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Cedula VARCHAR(20) NOT NULL,
        NombreCompleto VARCHAR(150) NOT NULL,
        Correo VARCHAR(100) NOT NULL,
        JefaturaID INT NULL,
        UnidadID INT NOT NULL,
        RolID INT NOT NULL,
        Compania VARCHAR(10) NOT NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_Usuarios_Fec_Registro DEFAULT GETDATE(),
        Usr_Modifica VARCHAR(50) NULL,
        Fec_Modifica DATETIME NULL,
        CONSTRAINT FK_Usuarios_Roles FOREIGN KEY (RolID) REFERENCES dbo.Roles(RolID),
        CONSTRAINT FK_Usuarios_Jefatura FOREIGN KEY (JefaturaID) REFERENCES dbo.Usuarios(UsuarioID),
        CONSTRAINT CK_Usuarios_Compania CHECK (Compania IN ('CNP', 'FANAL'))
    );
END;
GO

/* =========================
   Tablas transaccionales
   ========================= */
IF OBJECT_ID('dbo.Justificaciones_Encabezado', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Justificaciones_Encabezado (
        JustificacionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        UsuarioID INT NOT NULL,
        MotivoGeneral VARCHAR(500) NOT NULL,
        ComentarioResolucion VARCHAR(500) NULL,
        EstadoID INT NOT NULL,
        FechaCreacion DATETIME NOT NULL CONSTRAINT DF_JustifEnc_FechaCreacion DEFAULT GETDATE(),
        AprobadorID INT NULL,
        FechaAprobacion DATETIME NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_JustifEnc_Fec_Registro DEFAULT GETDATE(),
        CONSTRAINT FK_JustifEnc_Usuario FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID),
        CONSTRAINT FK_JustifEnc_Estado FOREIGN KEY (EstadoID) REFERENCES dbo.Estados(EstadoID),
        CONSTRAINT FK_JustifEnc_Aprobador FOREIGN KEY (AprobadorID) REFERENCES dbo.Usuarios(UsuarioID)
    );
END;
GO

IF OBJECT_ID('dbo.Justificaciones_Detalle', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Justificaciones_Detalle (
        DetalleID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        JustificacionID INT NOT NULL,
        TipoJustificacionID INT NOT NULL,
        FechaMarca DATE NOT NULL,
        ObservacionDetalle VARCHAR(250) NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_JustifDet_Fec_Registro DEFAULT GETDATE(),
        CONSTRAINT FK_JustifDet_Encabezado FOREIGN KEY (JustificacionID) REFERENCES dbo.Justificaciones_Encabezado(JustificacionID),
        CONSTRAINT FK_JustifDet_Tipo FOREIGN KEY (TipoJustificacionID) REFERENCES dbo.Cat_TiposJustificacion(TipoJustificacionID)
    );
END;
GO

/* =========================
   Datos semilla
   ========================= */
MERGE dbo.Roles AS tgt
USING (
    SELECT 1 AS RolID, 'Funcionario' AS NombreRol
    UNION ALL SELECT 2, 'Jefatura'
    UNION ALL SELECT 3, 'RRHH'
) AS src
ON tgt.RolID = src.RolID
WHEN NOT MATCHED THEN
    INSERT (RolID, NombreRol, Usr_Registro)
    VALUES (src.RolID, src.NombreRol, 'seed');
GO

MERGE dbo.Estados AS tgt
USING (
    SELECT 1 AS EstadoID, 'Pendiente Jefatura' AS Descripcion, 'Marcas' AS Proceso
    UNION ALL SELECT 2, 'Aprobado', 'Marcas'
    UNION ALL SELECT 3, 'Rechazado', 'Marcas'
) AS src
ON tgt.EstadoID = src.EstadoID
WHEN NOT MATCHED THEN
    INSERT (EstadoID, Descripcion, Proceso)
    VALUES (src.EstadoID, src.Descripcion, src.Proceso);
GO

IF NOT EXISTS (SELECT 1 FROM dbo.Cat_TiposJustificacion)
BEGIN
    INSERT INTO dbo.Cat_TiposJustificacion (Descripcion, Usr_Registro)
    VALUES
    ('Marca Tardía', 'seed'),
    ('Omisión Marca de Entrada', 'seed'),
    ('Omisión Marca de Salida', 'seed'),
    ('Marca antes Hora de Salida', 'seed'),
    ('Ausencia', 'seed');
END;
GO

/* =========================
   Índices mínimos PRP
   ========================= */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JustifEnc_UsuarioID' AND object_id = OBJECT_ID('dbo.Justificaciones_Encabezado'))
BEGIN
    CREATE INDEX IX_JustifEnc_UsuarioID
        ON dbo.Justificaciones_Encabezado (UsuarioID);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JustifEnc_EstadoID' AND object_id = OBJECT_ID('dbo.Justificaciones_Encabezado'))
BEGIN
    CREATE INDEX IX_JustifEnc_EstadoID
        ON dbo.Justificaciones_Encabezado (EstadoID);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JustifDet_JustificacionID' AND object_id = OBJECT_ID('dbo.Justificaciones_Detalle'))
BEGIN
    CREATE INDEX IX_JustifDet_JustificacionID
        ON dbo.Justificaciones_Detalle (JustificacionID);
END;
GO

/* =========================
   Log de errores de API
   ========================= */
IF OBJECT_ID('dbo.ApiErrorLog', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ApiErrorLog (
        ErrorLogID      INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CorrelationID   UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWID(),
        FechaUtc        DATETIME2(3)      NOT NULL DEFAULT SYSUTCDATETIME(),
        HttpMethod      VARCHAR(10)       NOT NULL,
        Endpoint        NVARCHAR(500)     NOT NULL,
        StatusCode      INT               NOT NULL,
        TipoError       VARCHAR(200)      NOT NULL,
        Mensaje         NVARCHAR(1000)    NOT NULL,
        StackTrace      NVARCHAR(MAX)     NULL,
        UsuarioID       NVARCHAR(100)     NULL,
        RolUsuario      NVARCHAR(50)      NULL,
        Entorno         VARCHAR(20)       NOT NULL DEFAULT 'Unknown',
        Ip              VARCHAR(45)       NULL,
        UserAgent       NVARCHAR(500)     NULL
    );
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiErrorLog_FechaUtc' AND object_id = OBJECT_ID('dbo.ApiErrorLog'))
BEGIN
    CREATE INDEX IX_ApiErrorLog_FechaUtc ON dbo.ApiErrorLog (FechaUtc DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ApiErrorLog_StatusCode' AND object_id = OBJECT_ID('dbo.ApiErrorLog'))
BEGIN
    CREATE INDEX IX_ApiErrorLog_StatusCode ON dbo.ApiErrorLog (StatusCode, FechaUtc DESC);
END;
GO
