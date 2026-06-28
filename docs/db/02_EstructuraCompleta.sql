/* ============================================================================
   INTEGRA_CNP — Script 2 de 3: ESTRUCTURA COMPLETA
   ----------------------------------------------------------------------------
   Sistema:  Justificacion de Marca (INTEGRA_CNP) — CNP / FANAL
   Fecha:    2026-06-24
   Motor:    SQL Server 2019+ (validado en 15.0.2135.5)
   Requiere: 01_CrearBaseDatos.sql ejecutado previamente.

   PROPOSITO
     Crear TODA la estructura interna de la base, alineada exactamente con lo que
     consume el backend .NET (Infrastructure/Queries, archivos .cs, y repositorios):
       - Tablas de catalogo, RRHH, operacion y auditoria.
       - Constraints (PK, FK, CHECK, DEFAULT) e indices con nombre explicito.
       - Funcion de alcance de aprobadores  dbo.fn_AprobadoresVigentesPorSolicitante.
       - Vistas de integracion externa (WIZDOM / SIFCNP) y vista legada SIFCNP.
     Nota: el SP de sincronizacion SIFCNP legado se RETIRA (asumia un esquema
     SIFCNP ficticio; ver Seccion G y el informe de observaciones).

   TRIGGERS
     El sistema NO define triggers. La logica de negocio vive en la capa
     Application del backend. (Ver seccion de observaciones del informe.)

   DECISIONES DE ALINEACION (ver informe de observaciones)
     - La funcion de aprobadores se crea en el esquema 'dbo' (no 'Operacion'),
       porque el backend la invoca como dbo.fn_AprobadoresVigentesPorSolicitante.
       Se elimina la version obsoleta Operacion.fn_... si existe.
     - Auditoria.ErrorApi usa los nombres de columna finales en ingles
       (HttpMethod, StatusCode, UsuarioID, Ip, RolUsuario, Entorno, UserAgent)
       que exige el contrato C#. Desviacion deliberada de "todo en espanol".
     - Los objetos que dependen de BD externas (WIZDOM/SIFCNP) y de tablas
       legadas dbo.RH_* se crean SOLO si esas dependencias existen (guardas
       DB_ID / OBJECT_ID), para que el script no aborte en entornos locales.

   IDEMPOTENCIA
     Re-ejecutable: tablas con IF OBJECT_ID, columnas con COL_LENGTH, indices con
     sys.indexes, rutinas con CREATE OR ALTER.

   CODIFICACION
     Ejecutar en UTF-8 (SSMS o sqlcmd -f 65001) para preservar acentos.
   ============================================================================ */

IF DB_ID('INTEGRA_CNP') IS NULL
BEGIN
    RAISERROR('La base INTEGRA_CNP no existe. Ejecute primero 01_CrearBaseDatos.sql', 16, 1);
    SET NOEXEC ON;
END;
GO

USE INTEGRA_CNP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ############################################################################
   SECCION A — CATALOGOS  (esquema Configuracion)
   Tablas de referencia de baja cardinalidad. Cumplen 3FN: cada atributo
   depende unicamente de su clave. Las que tienen IDs de negocio fijos usan
   PK manual; TipoJustificacion usa IDENTITY por ser un catalogo extensible.
   ############################################################################ */

/* Roles del sistema: Funcionario, Jefatura, RRHH, Administrador. */
IF OBJECT_ID('Configuracion.Rol', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.Rol (
        RolId             INT          NOT NULL,                 -- ID fijo de negocio (1..4)
        Descripcion       VARCHAR(50)  NOT NULL,
        CreadoPor         VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2    NOT NULL
            CONSTRAINT DF_Rol_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_Rol PRIMARY KEY (RolId)
    );
END;
GO

/* Estados del ciclo de una justificacion: Pendiente Jefatura / Aprobada / Rechazada. */
IF OBJECT_ID('Configuracion.EstadoJustificacion', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.EstadoJustificacion (
        EstadoJustificacionId INT          NOT NULL,
        Descripcion           VARCHAR(100) NOT NULL,
        CreadoPor             VARCHAR(100) NOT NULL,
        FechaHoraCreacion     DATETIME2    NOT NULL
            CONSTRAINT DF_EstadoJustificacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EstadoJustificacion PRIMARY KEY (EstadoJustificacionId)
    );
END;
GO

/* Catalogo extensible de tipos de marca (Marca Tardia, Omision Entrada, etc.). */
IF OBJECT_ID('Configuracion.TipoJustificacion', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.TipoJustificacion (
        TipoJustificacionId INT          IDENTITY(1,1) NOT NULL,
        Descripcion         VARCHAR(100) NOT NULL,
        CreadoPor           VARCHAR(100) NOT NULL,
        FechaHoraCreacion   DATETIME2    NOT NULL
            CONSTRAINT DF_TipoJustificacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TipoJustificacion PRIMARY KEY (TipoJustificacionId)
    );
END;
GO

/* Estado de vigencia de registros maestros (Activo / Inactivo) para baja logica. */
IF OBJECT_ID('Configuracion.EstadoRegistro', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.EstadoRegistro (
        EstadoRegistroId  INT          NOT NULL,
        Descripcion       VARCHAR(50)  NOT NULL,
        CreadoPor         VARCHAR(100) NOT NULL,
        FechaHoraCreacion DATETIME2    NOT NULL
            CONSTRAINT DF_EstadoRegistro_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EstadoRegistro PRIMARY KEY (EstadoRegistroId)
    );
END;
GO

/* Tipos de evento de auditoria (creacion, resolucion, altas/cambios admin...). */
IF OBJECT_ID('Configuracion.TipoEventoAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.TipoEventoAuditoria (
        TipoEventoAuditoriaId INT          NOT NULL,
        Descripcion           VARCHAR(100) NOT NULL,
        CreadoPor             VARCHAR(100) NOT NULL,
        FechaHoraCreacion     DATETIME2    NOT NULL
            CONSTRAINT DF_TipoEventoAuditoria_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_TipoEventoAuditoria PRIMARY KEY (TipoEventoAuditoriaId)
    );
END;
GO

/* Resultado de un evento auditado: Exito / Fallo / Denegado. */
IF OBJECT_ID('Configuracion.ResultadoAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE Configuracion.ResultadoAuditoria (
        ResultadoAuditoriaId INT         NOT NULL,
        Descripcion          VARCHAR(50) NOT NULL,
        CreadoPor            VARCHAR(100) NOT NULL,
        FechaHoraCreacion    DATETIME2   NOT NULL
            CONSTRAINT DF_ResultadoAuditoria_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_ResultadoAuditoria PRIMARY KEY (ResultadoAuditoriaId)
    );
END;
GO

/* ############################################################################
   SECCION B — RECURSOS HUMANOS  (esquema RecursosHumanos)
   ############################################################################ */

/* Personal del sistema. JefaturaId es auto-referencia (jefe directo).
   UnidadId es el codigo de unidad organizacional (se relaciona de forma logica
   con EstructuraOrganizacional.CodigoOrigen; ver observaciones 3FN). */
IF OBJECT_ID('RecursosHumanos.Usuario', 'U') IS NULL
BEGIN
    CREATE TABLE RecursosHumanos.Usuario (
        UsuarioId             INT          IDENTITY(1,1) NOT NULL,
        Cedula                VARCHAR(64)  NOT NULL,           -- identificador externo (cedula / codigo)
        NombreCompleto        VARCHAR(150) NOT NULL,
        CorreoElectronico     VARCHAR(100) NOT NULL,
        JefaturaId            INT          NULL,               -- jefe directo (auto-FK)
        UnidadId              INT          NOT NULL,           -- codigo de unidad (= EstructuraOrganizacional.CodigoOrigen)
        RolId                 INT          NOT NULL,
        Compania              VARCHAR(10)  NOT NULL,
        EsActivo              BIT          NOT NULL
            CONSTRAINT DF_Usuario_EsActivo DEFAULT 1,
        CreadoPor             VARCHAR(100) NOT NULL,
        FechaHoraCreacion     DATETIME2    NOT NULL
            CONSTRAINT DF_Usuario_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        ModificadoPor         VARCHAR(100) NULL,
        FechaHoraModificacion DATETIME2    NULL,
        CONSTRAINT PK_Usuario PRIMARY KEY (UsuarioId),
        CONSTRAINT FK_Usuario_RolId      FOREIGN KEY (RolId)
            REFERENCES Configuracion.Rol(RolId),
        CONSTRAINT FK_Usuario_JefaturaId FOREIGN KEY (JefaturaId)
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT CK_Usuario_Compania   CHECK (Compania IN ('CNP', 'FANAL'))
    );
END;
GO

/* Estructura organizacional jerarquica (auto-referencia por EstructuraPadreId).
   CodigoOrigen = codigo de la unidad en el sistema origen; se enlaza con
   Usuario.UnidadId. VigenciaDesde/Hasta dan versionado temporal del nodo. */
IF OBJECT_ID('RecursosHumanos.EstructuraOrganizacional', 'U') IS NULL
BEGIN
    CREATE TABLE RecursosHumanos.EstructuraOrganizacional (
        EstructuraOrganizacionalId INT          IDENTITY(1,1) NOT NULL,
        Nombre                     VARCHAR(150) NOT NULL,
        CodigoOrigen               VARCHAR(50)  NULL,         -- codigo de unidad (origen)
        EstructuraPadreId          INT          NULL,         -- nodo padre (auto-FK)
        EstadoRegistroId           INT          NOT NULL,
        VigenciaDesde              DATETIME2    NULL,
        VigenciaHasta              DATETIME2    NULL,
        CreadoPor                  VARCHAR(100) NOT NULL,
        FechaHoraCreacion          DATETIME2    NOT NULL
            CONSTRAINT DF_EstructuraOrganizacional_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_EstructuraOrganizacional PRIMARY KEY (EstructuraOrganizacionalId),
        CONSTRAINT FK_EstructuraOrganizacional_Padre FOREIGN KEY (EstructuraPadreId)
            REFERENCES RecursosHumanos.EstructuraOrganizacional(EstructuraOrganizacionalId),
        CONSTRAINT FK_EstructuraOrganizacional_EstadoRegistroId FOREIGN KEY (EstadoRegistroId)
            REFERENCES Configuracion.EstadoRegistro(EstadoRegistroId)
    );
END;
GO

/* ############################################################################
   SECCION C — OPERACION  (esquema Operacion)
   Modelo encabezado/detalle de la boleta + jerarquia y delegacion de aprobacion.
   ############################################################################ */

/* Encabezado de la boleta de justificacion (1 por solicitud).
   Datos de resolucion (Aprobador, Fecha, Comentario, RolResolucion) son un
   snapshot del momento de resolver; dependen de la PK -> cumple 3FN. */
IF OBJECT_ID('Operacion.Justificacion', 'U') IS NULL
BEGIN
    CREATE TABLE Operacion.Justificacion (
        JustificacionId       INT          IDENTITY(1,1) NOT NULL,
        UsuarioId             INT          NOT NULL,          -- solicitante
        MotivoGeneral         VARCHAR(500) NOT NULL,
        ComentarioResolucion  VARCHAR(500) NULL,
        RolResolucion         VARCHAR(20)  NULL,              -- rol del aprobador al resolver (snapshot)
        EstadoJustificacionId INT          NOT NULL,
        FechaCreacion         DATETIME2    NOT NULL
            CONSTRAINT DF_Justificacion_FechaCreacion DEFAULT SYSUTCDATETIME(),
        AprobadorId           INT          NULL,
        FechaAprobacion       DATETIME2    NULL,
        CreadoPor             VARCHAR(100) NOT NULL,
        FechaHoraCreacion     DATETIME2    NOT NULL
            CONSTRAINT DF_Justificacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        ModificadoPor         VARCHAR(100) NULL,
        FechaHoraModificacion DATETIME2    NULL,
        CONSTRAINT PK_Justificacion PRIMARY KEY (JustificacionId),
        CONSTRAINT FK_Justificacion_UsuarioId FOREIGN KEY (UsuarioId)
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_Justificacion_EstadoJustificacionId FOREIGN KEY (EstadoJustificacionId)
            REFERENCES Configuracion.EstadoJustificacion(EstadoJustificacionId),
        CONSTRAINT FK_Justificacion_AprobadorId FOREIGN KEY (AprobadorId)
            REFERENCES RecursosHumanos.Usuario(UsuarioId)
    );
END;
GO

/* Detalle (lineas) de la boleta: una marca/fecha por fila. */
IF OBJECT_ID('Operacion.JustificacionDetalle', 'U') IS NULL
BEGIN
    CREATE TABLE Operacion.JustificacionDetalle (
        JustificacionDetalleId INT          IDENTITY(1,1) NOT NULL,
        JustificacionId        INT          NOT NULL,
        TipoJustificacionId    INT          NOT NULL,
        FechaMarca             DATE         NOT NULL,
        ObservacionDetalle     VARCHAR(250) NULL,
        CreadoPor              VARCHAR(100) NOT NULL,
        FechaHoraCreacion      DATETIME2    NOT NULL
            CONSTRAINT DF_JustificacionDetalle_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_JustificacionDetalle PRIMARY KEY (JustificacionDetalleId),
        CONSTRAINT FK_JustificacionDetalle_JustificacionId FOREIGN KEY (JustificacionId)
            REFERENCES Operacion.Justificacion(JustificacionId),
        CONSTRAINT FK_JustificacionDetalle_TipoJustificacionId FOREIGN KEY (TipoJustificacionId)
            REFERENCES Configuracion.TipoJustificacion(TipoJustificacionId)
    );
END;
GO

/* Jerarquia de aprobacion: que aprobador resuelve para que estructura.
   TipoRelacion Vertical/Horizontal; vigencia temporal por VigenciaDesde/Hasta. */
IF OBJECT_ID('Operacion.JerarquiaAprobacion', 'U') IS NULL
BEGIN
    CREATE TABLE Operacion.JerarquiaAprobacion (
        JerarquiaAprobacionId      INT          IDENTITY(1,1) NOT NULL,
        AprobadorUsuarioId         INT          NOT NULL,
        EstructuraOrganizacionalId INT          NOT NULL,
        NivelAprobacion            INT          NOT NULL,
        TipoRelacion               VARCHAR(20)  NOT NULL,
        EstadoRegistroId           INT          NOT NULL,
        VigenciaDesde              DATETIME2    NOT NULL,
        VigenciaHasta              DATETIME2    NULL,
        CreadoPor                  VARCHAR(100) NOT NULL,
        FechaHoraCreacion          DATETIME2    NOT NULL
            CONSTRAINT DF_JerarquiaAprobacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_JerarquiaAprobacion PRIMARY KEY (JerarquiaAprobacionId),
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

/* Delegacion de aprobacion: un delegante cede su scope a un delegado por un
   periodo. JerarquiaAprobacionId NULL = aplica a todas las jerarquias del delegante. */
IF OBJECT_ID('Operacion.DelegacionAprobacion', 'U') IS NULL
BEGIN
    CREATE TABLE Operacion.DelegacionAprobacion (
        DelegacionAprobacionId INT          IDENTITY(1,1) NOT NULL,
        DeleganteUsuarioId     INT          NOT NULL,
        DelegadoUsuarioId      INT          NOT NULL,
        JerarquiaAprobacionId  INT          NULL,
        Motivo                 VARCHAR(250) NULL,
        EstadoRegistroId       INT          NOT NULL,
        VigenciaDesde          DATETIME2    NOT NULL,
        VigenciaHasta          DATETIME2    NULL,
        CreadoPor              VARCHAR(100) NOT NULL,
        FechaHoraCreacion      DATETIME2    NOT NULL
            CONSTRAINT DF_DelegacionAprobacion_FechaHoraCreacion DEFAULT SYSUTCDATETIME(),
        CONSTRAINT PK_DelegacionAprobacion PRIMARY KEY (DelegacionAprobacionId),
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

/* ############################################################################
   SECCION D — AUDITORIA  (esquema Auditoria)
   Tablas de bitacora. Campos como NombreUsuario / RolCodigo son snapshots
   inmutables del evento (no se normalizan a FK a proposito, para preservar el
   valor historico aun si el usuario cambia). Ver observaciones.
   ############################################################################ */

/* Bitacora de eventos funcionales de negocio. */
IF OBJECT_ID('Auditoria.EventoAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE Auditoria.EventoAuditoria (
        EventoAuditoriaId     BIGINT        IDENTITY(1,1) NOT NULL,
        FechaEvento           DATETIME2     NOT NULL
            CONSTRAINT DF_EventoAuditoria_FechaEvento DEFAULT SYSUTCDATETIME(),
        UsuarioId             INT           NULL,
        NombreUsuario         VARCHAR(150)  NOT NULL,        -- snapshot
        RolCodigo             VARCHAR(20)   NOT NULL,        -- snapshot
        TipoEventoAuditoriaId INT           NOT NULL,
        DescripcionEvento     VARCHAR(500)  NOT NULL,
        ResultadoAuditoriaId  INT           NOT NULL,
        ReferenciaFuncional   VARCHAR(100)  NULL,
        PayloadResumen        VARCHAR(1000) NULL,
        CONSTRAINT PK_EventoAuditoria PRIMARY KEY (EventoAuditoriaId),
        CONSTRAINT FK_EventoAuditoria_UsuarioId FOREIGN KEY (UsuarioId)
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_EventoAuditoria_TipoEventoAuditoriaId FOREIGN KEY (TipoEventoAuditoriaId)
            REFERENCES Configuracion.TipoEventoAuditoria(TipoEventoAuditoriaId),
        CONSTRAINT FK_EventoAuditoria_ResultadoAuditoriaId FOREIGN KEY (ResultadoAuditoriaId)
            REFERENCES Configuracion.ResultadoAuditoria(ResultadoAuditoriaId)
    );
END;
GO

/* Log de errores de la API. NOMBRES DE COLUMNA EN INGLES por contrato C#
   (IErrorLogRepository / ErrorLogRepository). Desviacion deliberada. */
IF OBJECT_ID('Auditoria.ErrorApi', 'U') IS NULL
BEGIN
    CREATE TABLE Auditoria.ErrorApi (
        ErrorApiId    INT              IDENTITY(1,1) NOT NULL,
        CorrelationId UNIQUEIDENTIFIER NOT NULL
            CONSTRAINT DF_ErrorApi_CorrelationId DEFAULT NEWID(),
        FechaUtc      DATETIME2(3)     NOT NULL
            CONSTRAINT DF_ErrorApi_FechaUtc DEFAULT SYSUTCDATETIME(),
        HttpMethod    VARCHAR(10)      NOT NULL,
        Endpoint      NVARCHAR(500)    NOT NULL,
        StatusCode    INT              NOT NULL,
        TipoError     VARCHAR(200)     NOT NULL,
        Mensaje       NVARCHAR(1000)   NOT NULL,
        StackTrace    NVARCHAR(MAX)    NULL,                 -- solo cuando StatusCode >= 500
        UsuarioID     VARCHAR(150)     NULL,
        RolUsuario    VARCHAR(50)      NULL,
        Entorno       VARCHAR(50)      NULL,
        Ip            VARCHAR(45)      NULL,
        UserAgent     NVARCHAR(500)    NULL,
        CONSTRAINT PK_ErrorApi PRIMARY KEY (ErrorApiId)
    );
END;
GO

/* Alineacion idempotente de Auditoria.ErrorApi cuando se actualiza una BD
   legada creada con los nombres antiguos en espanol (renombra + agrega faltantes). */
IF OBJECT_ID('Auditoria.ErrorApi', 'U') IS NOT NULL
BEGIN
    IF COL_LENGTH('Auditoria.ErrorApi', 'MetodoHttp') IS NOT NULL
       AND COL_LENGTH('Auditoria.ErrorApi', 'HttpMethod') IS NULL
        EXEC sp_rename 'Auditoria.ErrorApi.MetodoHttp', 'HttpMethod', 'COLUMN';

    IF COL_LENGTH('Auditoria.ErrorApi', 'CodigoEstado') IS NOT NULL
       AND COL_LENGTH('Auditoria.ErrorApi', 'StatusCode') IS NULL
        EXEC sp_rename 'Auditoria.ErrorApi.CodigoEstado', 'StatusCode', 'COLUMN';

    IF COL_LENGTH('Auditoria.ErrorApi', 'UsuarioSolicitante') IS NOT NULL
       AND COL_LENGTH('Auditoria.ErrorApi', 'UsuarioID') IS NULL
        EXEC sp_rename 'Auditoria.ErrorApi.UsuarioSolicitante', 'UsuarioID', 'COLUMN';

    IF COL_LENGTH('Auditoria.ErrorApi', 'DireccionIP') IS NOT NULL
       AND COL_LENGTH('Auditoria.ErrorApi', 'Ip') IS NULL
        EXEC sp_rename 'Auditoria.ErrorApi.DireccionIP', 'Ip', 'COLUMN';

    IF COL_LENGTH('Auditoria.ErrorApi', 'RolUsuario') IS NULL
        ALTER TABLE Auditoria.ErrorApi ADD RolUsuario VARCHAR(50) NULL;

    IF COL_LENGTH('Auditoria.ErrorApi', 'Entorno') IS NULL
        ALTER TABLE Auditoria.ErrorApi ADD Entorno VARCHAR(50) NULL;

    IF COL_LENGTH('Auditoria.ErrorApi', 'UserAgent') IS NULL
        ALTER TABLE Auditoria.ErrorApi ADD UserAgent NVARCHAR(500) NULL;
END;
GO

/* Auditoria detallada de acciones administrativas, con snapshots JSON
   antes/despues (ValoresAnteriores / ValoresNuevos). */
IF OBJECT_ID('Auditoria.AdminAccionAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE Auditoria.AdminAccionAuditoria (
        AdminAccionAuditoriaId BIGINT        IDENTITY(1,1) NOT NULL,
        FechaEventoUtc         DATETIME2(3)  NOT NULL
            CONSTRAINT DF_AdminAccionAuditoria_FechaEventoUtc DEFAULT SYSUTCDATETIME(),
        CorrelationId          NVARCHAR(100) NULL,
        UsuarioActorId         INT           NOT NULL,
        RolActorCodigo         VARCHAR(20)   NOT NULL,
        EntidadObjetivo        VARCHAR(80)   NOT NULL,
        EntidadObjetivoId      VARCHAR(80)   NOT NULL,
        Accion                 VARCHAR(40)   NOT NULL,
        ResultadoAuditoriaId   INT           NOT NULL,
        Descripcion            VARCHAR(500)  NOT NULL,
        ValoresAnteriores      NVARCHAR(MAX) NULL,           -- snapshot JSON previo
        ValoresNuevos          NVARCHAR(MAX) NULL,           -- snapshot JSON posterior
        Metadata               NVARCHAR(MAX) NULL,
        CONSTRAINT PK_AdminAccionAuditoria PRIMARY KEY (AdminAccionAuditoriaId),
        CONSTRAINT FK_AdminAccionAuditoria_UsuarioActorId FOREIGN KEY (UsuarioActorId)
            REFERENCES RecursosHumanos.Usuario(UsuarioId),
        CONSTRAINT FK_AdminAccionAuditoria_ResultadoAuditoriaId FOREIGN KEY (ResultadoAuditoriaId)
            REFERENCES Configuracion.ResultadoAuditoria(ResultadoAuditoriaId)
    );
END;
GO

/* ############################################################################
   SECCION E — INDICES SECUNDARIOS
   Soportan los filtros/joins reales del backend (por usuario, estado, aprobador,
   vigencia y fechas de auditoria).
   ############################################################################ */

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_Cedula' AND object_id = OBJECT_ID('RecursosHumanos.Usuario'))
    CREATE INDEX IX_Usuario_Cedula ON RecursosHumanos.Usuario (Cedula);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Usuario_RolId_EsActivo' AND object_id = OBJECT_ID('RecursosHumanos.Usuario'))
    CREATE INDEX IX_Usuario_RolId_EsActivo ON RecursosHumanos.Usuario (RolId, EsActivo);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EstructuraOrganizacional_CodigoOrigen_EstadoRegistroId' AND object_id = OBJECT_ID('RecursosHumanos.EstructuraOrganizacional'))
    CREATE INDEX IX_EstructuraOrganizacional_CodigoOrigen_EstadoRegistroId ON RecursosHumanos.EstructuraOrganizacional (CodigoOrigen, EstadoRegistroId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Justificacion_UsuarioId' AND object_id = OBJECT_ID('Operacion.Justificacion'))
    CREATE INDEX IX_Justificacion_UsuarioId ON Operacion.Justificacion (UsuarioId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Justificacion_EstadoJustificacionId' AND object_id = OBJECT_ID('Operacion.Justificacion'))
    CREATE INDEX IX_Justificacion_EstadoJustificacionId ON Operacion.Justificacion (EstadoJustificacionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Justificacion_AprobadorId' AND object_id = OBJECT_ID('Operacion.Justificacion'))
    CREATE INDEX IX_Justificacion_AprobadorId ON Operacion.Justificacion (AprobadorId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JustificacionDetalle_JustificacionId' AND object_id = OBJECT_ID('Operacion.JustificacionDetalle'))
    CREATE INDEX IX_JustificacionDetalle_JustificacionId ON Operacion.JustificacionDetalle (JustificacionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JustificacionDetalle_TipoJustificacionId' AND object_id = OBJECT_ID('Operacion.JustificacionDetalle'))
    CREATE INDEX IX_JustificacionDetalle_TipoJustificacionId ON Operacion.JustificacionDetalle (TipoJustificacionId);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_JerarquiaAprobacion_AprobadorUsuarioId_EstructuraOrganizacionalId' AND object_id = OBJECT_ID('Operacion.JerarquiaAprobacion'))
    CREATE INDEX IX_JerarquiaAprobacion_AprobadorUsuarioId_EstructuraOrganizacionalId
        ON Operacion.JerarquiaAprobacion (AprobadorUsuarioId, EstructuraOrganizacionalId, EstadoRegistroId, VigenciaDesde, VigenciaHasta);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_DelegacionAprobacion_DelegadoUsuarioId_Vigencia' AND object_id = OBJECT_ID('Operacion.DelegacionAprobacion'))
    CREATE INDEX IX_DelegacionAprobacion_DelegadoUsuarioId_Vigencia
        ON Operacion.DelegacionAprobacion (DelegadoUsuarioId, EstadoRegistroId, VigenciaDesde, VigenciaHasta);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventoAuditoria_FechaEvento' AND object_id = OBJECT_ID('Auditoria.EventoAuditoria'))
    CREATE INDEX IX_EventoAuditoria_FechaEvento ON Auditoria.EventoAuditoria (FechaEvento DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_EventoAuditoria_TipoEventoAuditoriaId_ResultadoAuditoriaId' AND object_id = OBJECT_ID('Auditoria.EventoAuditoria'))
    CREATE INDEX IX_EventoAuditoria_TipoEventoAuditoriaId_ResultadoAuditoriaId
        ON Auditoria.EventoAuditoria (TipoEventoAuditoriaId, ResultadoAuditoriaId, FechaEvento DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ErrorApi_FechaUtc' AND object_id = OBJECT_ID('Auditoria.ErrorApi'))
    CREATE INDEX IX_ErrorApi_FechaUtc ON Auditoria.ErrorApi (FechaUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AdminAccionAuditoria_FechaEventoUtc' AND object_id = OBJECT_ID('Auditoria.AdminAccionAuditoria'))
    CREATE INDEX IX_AdminAccionAuditoria_FechaEventoUtc ON Auditoria.AdminAccionAuditoria (FechaEventoUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AdminAccionAuditoria_Entidad' AND object_id = OBJECT_ID('Auditoria.AdminAccionAuditoria'))
    CREATE INDEX IX_AdminAccionAuditoria_Entidad ON Auditoria.AdminAccionAuditoria (EntidadObjetivo, EntidadObjetivoId, FechaEventoUtc DESC);
GO
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_AdminAccionAuditoria_Actor' AND object_id = OBJECT_ID('Auditoria.AdminAccionAuditoria'))
    CREATE INDEX IX_AdminAccionAuditoria_Actor ON Auditoria.AdminAccionAuditoria (UsuarioActorId, FechaEventoUtc DESC);
GO

/* ############################################################################
   SECCION F — FUNCION DE ALCANCE DE APROBADORES
   ----------------------------------------------------------------------------
   dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioId, @FechaRef)
   Devuelve los aprobadores vigentes para un solicitante en una fecha dada,
   combinando jerarquia (Origen='Jerarquia') y delegacion (Origen='Delegacion').
   El backend la consume con nombre 'dbo.fn_...'; por eso vive en el esquema dbo.
   Se elimina la version obsoleta Operacion.fn_... si existiera.
   ############################################################################ */

IF OBJECT_ID('Operacion.fn_AprobadoresVigentesPorSolicitante', 'IF') IS NOT NULL
   OR OBJECT_ID('Operacion.fn_AprobadoresVigentesPorSolicitante', 'TF') IS NOT NULL
   OR OBJECT_ID('Operacion.fn_AprobadoresVigentesPorSolicitante', 'FN') IS NOT NULL
BEGIN
    DROP FUNCTION Operacion.fn_AprobadoresVigentesPorSolicitante;
    PRINT 'Eliminada funcion obsoleta Operacion.fn_AprobadoresVigentesPorSolicitante.';
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_AprobadoresVigentesPorSolicitante
(
    @SolicitanteUsuarioId INT,
    @FechaRef             DATETIME2
)
RETURNS TABLE
AS
RETURN
(
    WITH SolicitanteEstructura AS (
        /* Estructura(s) organizacional(es) vigentes del solicitante por su UnidadId. */
        SELECT DISTINCT eo.EstructuraOrganizacionalId
        FROM RecursosHumanos.Usuario u
        INNER JOIN RecursosHumanos.EstructuraOrganizacional eo
            ON eo.CodigoOrigen = CAST(u.UnidadId AS VARCHAR(50))
           AND eo.EstadoRegistroId = 1
           AND (eo.VigenciaDesde IS NULL OR eo.VigenciaDesde <= @FechaRef)
           AND (eo.VigenciaHasta IS NULL OR eo.VigenciaHasta >= @FechaRef)
        WHERE u.UsuarioId = @SolicitanteUsuarioId
    ),
    JerarquiasActivas AS (
        /* Jerarquias de aprobacion vigentes para esas estructuras. */
        SELECT DISTINCT ja.JerarquiaAprobacionId, ja.AprobadorUsuarioId
        FROM Operacion.JerarquiaAprobacion ja
        INNER JOIN SolicitanteEstructura se
            ON se.EstructuraOrganizacionalId = ja.EstructuraOrganizacionalId
        WHERE ja.EstadoRegistroId = 1
          AND (ja.VigenciaDesde IS NULL OR ja.VigenciaDesde <= @FechaRef)
          AND (ja.VigenciaHasta IS NULL OR ja.VigenciaHasta >= @FechaRef)
    ),
    DelegacionesActivas AS (
        /* Delegaciones vigentes cuyo delegante es un aprobador por jerarquia. */
        SELECT DISTINCT
            da.DelegadoUsuarioId  AS AprobadorUsuarioId,
            da.DeleganteUsuarioId
        FROM Operacion.DelegacionAprobacion da
        WHERE da.EstadoRegistroId = 1
          AND (da.VigenciaDesde IS NULL OR da.VigenciaDesde <= @FechaRef)
          AND (da.VigenciaHasta IS NULL OR da.VigenciaHasta >= @FechaRef)
          AND EXISTS (SELECT 1 FROM JerarquiasActivas ja WHERE ja.AprobadorUsuarioId = da.DeleganteUsuarioId)
    )
    SELECT ja.AprobadorUsuarioId,
           CAST('Jerarquia'  AS VARCHAR(20)) AS Origen,
           CAST(NULL AS INT)                 AS DeleganteUsuarioId
    FROM JerarquiasActivas ja
    UNION ALL
    SELECT da.AprobadorUsuarioId,
           CAST('Delegacion' AS VARCHAR(20)) AS Origen,
           da.DeleganteUsuarioId
    FROM DelegacionesActivas da
);
GO

/* ############################################################################
   SECCION G — SINCRONIZACION SIFCNP  (NO IMPLEMENTADA — PENDIENTE DE RE-MAPEO)
   ----------------------------------------------------------------------------
   El SP legado (002) usp_SincronizarJustificacionesDesdeHistorico asumia un
   esquema SIFCNP FICTICIO (id_justificacion_enc, cedula_funcionario,
   motivo_general, estado_justificacion, comentario_resolucion, fecha_creacion)
   que NO existe en la base SIFCNP real. Su CREATE falla con Msg 207.

   Esquema REAL (verificado 2026-06-24):
     SIFCNP.dbo.RH_JUSTIFICACIONES_ENC:
       num_justificacion, cod_solicitante, cod_autoriza, fec_confeccion,
       des_justificacion, cod_centro, cod_estado, fec_autorizacion, cod_aprueba,
       fec_aprobacion, fec_anulacion, cod_anula, des_motivo, des_observaciones
     SIFCNP.dbo.RH_JUSTIFICACIONES_DET:
       num_justificacion, cod_linea, ind_concepto, fec_justificacion,
       ind_rebajar_planilla

   Como el modelo real es distinto (cod_solicitante es un codigo de funcionario,
   no una cedula; no hay 'motivo_general' ni estados textuales) y el backend NO
   invoca este SP, se RETIRA de la estructura. Reintroducir requiere un spec de
   mapeo real. Ver docs/db/Observaciones_Consolidacion_SQL.md.
   ############################################################################ */
GO

/* ############################################################################
   SECCION H — VISTAS DE INTEGRACION EXTERNA  (esquema Integracion)
   ----------------------------------------------------------------------------
   Solo lectura sobre WIZDOM/SIFCNP. CREATE VIEW valida columnas en tiempo de
   creacion, por lo que se crean SOLO si la BD externa existe (guarda DB_ID).
   Usan COLLATE SQL_Latin1_General_CP1_CI_AS en cadenas para joins cross-database.
   ############################################################################ */

IF DB_ID('WIZDOM') IS NOT NULL
BEGIN
    /* v_EmpleadoWizdom: columnas verificadas contra WIZDOM.dbo.empleado real. */
    BEGIN TRY
        EXEC('
        CREATE OR ALTER VIEW Integracion.v_EmpleadoWizdom AS
        SELECT
            CAST(e.compania AS VARCHAR(10))                        COLLATE SQL_Latin1_General_CP1_CI_AS AS Compania,
            CAST(e.codigo_empleado AS VARCHAR(50))                COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoEmpleado,
            CAST(e.numero_identificacion AS VARCHAR(64))          COLLATE SQL_Latin1_General_CP1_CI_AS AS NumeroIdentificacion,
            CAST(e.nombre AS VARCHAR(100))                        COLLATE SQL_Latin1_General_CP1_CI_AS AS Nombre,
            CAST(e.primer_apellido AS VARCHAR(100))               COLLATE SQL_Latin1_General_CP1_CI_AS AS PrimerApellido,
            CAST(e.segundo_apellido AS VARCHAR(100))              COLLATE SQL_Latin1_General_CP1_CI_AS AS SegundoApellido,
            CAST(e.correo_electronico_principal AS VARCHAR(150))  COLLATE SQL_Latin1_General_CP1_CI_AS AS CorreoElectronicoPrincipal,
            CAST(e.correo_electronico_alternativo AS VARCHAR(150)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CorreoElectronicoAlternativo,
            CAST(e.codigo_jefe AS VARCHAR(50))                    COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoJefe,
            CAST(e.codigo_nodo_organigrama AS VARCHAR(50))        COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoNodoOrganigrama,
            CAST(e.estado_empleado AS VARCHAR(30))                COLLATE SQL_Latin1_General_CP1_CI_AS AS EstadoEmpleado,
            CAST(e.fecha_ingreso AS DATE)                                                               AS FechaIngreso,
            CAST(e.fecha_egreso AS DATE)                                                                AS FechaEgreso,
            CAST(e.tstamp AS DATETIME2)                                                                 AS FechaActualizacion
        FROM [WIZDOM].[dbo].[empleado] e;');
        PRINT 'Integracion.v_EmpleadoWizdom creada/actualizada.';
    END TRY
    BEGIN CATCH
        PRINT 'Integracion.v_EmpleadoWizdom OMITIDA: ' + ERROR_MESSAGE();
    END CATCH;

    /* v_OrganigramaWizdom: REALINEADA a WIZDOM.dbo.organigrama real
       (codigo_nodo_organigrama, nombre_nodo_organigrama, *_padre, nivel, estado,
       codigo_tipo_nodo). La version legada usaba nombres ficticios. */
    BEGIN TRY
        EXEC('
        CREATE OR ALTER VIEW Integracion.v_OrganigramaWizdom AS
        SELECT
            CAST(o.compania AS VARCHAR(10))                       COLLATE SQL_Latin1_General_CP1_CI_AS AS Compania,
            CAST(o.codigo_nodo_organigrama AS VARCHAR(50))        COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoNodo,
            CAST(o.nombre_nodo_organigrama AS VARCHAR(150))       COLLATE SQL_Latin1_General_CP1_CI_AS AS NombreNodo,
            CAST(o.codigo_nodo_organigrama_padre AS VARCHAR(50))  COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoNodoPadre,
            CAST(o.codigo_tipo_nodo AS VARCHAR(50))               COLLATE SQL_Latin1_General_CP1_CI_AS AS TipoNodo,
            CAST(o.nivel AS INT)                                                                        AS NivelJerarquia,
            CAST(o.estado AS VARCHAR(30))                         COLLATE SQL_Latin1_General_CP1_CI_AS AS EstadoNodo,
            CAST(o.tstamp AS DATETIME2)                                                                 AS FechaActualizacion
        FROM [WIZDOM].[dbo].[organigrama] o;');
        PRINT 'Integracion.v_OrganigramaWizdom creada/actualizada.';
    END TRY
    BEGIN CATCH
        PRINT 'Integracion.v_OrganigramaWizdom OMITIDA: ' + ERROR_MESSAGE();
    END CATCH;
END
ELSE
    PRINT 'WIZDOM no disponible: se omiten v_EmpleadoWizdom y v_OrganigramaWizdom.';
GO

IF DB_ID('SIFCNP') IS NOT NULL
BEGIN
    /* REALINEADAS al esquema REAL de SIFCNP (verificado 2026-06-24). Exponen las
       columnas reales con alias PascalCase. El backend NO las consume; son una
       ventana de solo lectura sobre el historico SIFCNP. */
    BEGIN TRY
        EXEC('
        CREATE OR ALTER VIEW Integracion.v_JustificacionEncabezadoSifcnp AS
        SELECT
            j.num_justificacion   AS NumJustificacion,
            j.cod_solicitante     AS CodSolicitante,
            j.cod_autoriza        AS CodAutoriza,
            j.cod_aprueba         AS CodAprueba,
            j.cod_centro          AS CodCentro,
            j.cod_estado          AS CodEstado,
            j.cod_anula           AS CodAnula,
            j.fec_confeccion      AS FechaConfeccion,
            j.fec_autorizacion    AS FechaAutorizacion,
            j.fec_aprobacion      AS FechaAprobacion,
            j.fec_anulacion       AS FechaAnulacion,
            j.des_justificacion   AS DescripcionJustificacion,
            j.des_motivo          AS DescripcionMotivo,
            j.des_observaciones   AS DescripcionObservaciones
        FROM [SIFCNP].[dbo].[RH_JUSTIFICACIONES_ENC] j;');
        PRINT 'Integracion.v_JustificacionEncabezadoSifcnp creada/actualizada.';
    END TRY
    BEGIN CATCH
        PRINT 'Integracion.v_JustificacionEncabezadoSifcnp OMITIDA: ' + ERROR_MESSAGE();
    END CATCH;

    BEGIN TRY
        EXEC('
        CREATE OR ALTER VIEW Integracion.v_JustificacionDetalleSifcnp AS
        SELECT
            j.num_justificacion     AS NumJustificacion,
            j.cod_linea             AS CodLinea,
            j.ind_concepto          AS IndConcepto,
            j.fec_justificacion     AS FechaJustificacion,
            j.ind_rebajar_planilla  AS IndRebajarPlanilla
        FROM [SIFCNP].[dbo].[RH_JUSTIFICACIONES_DET] j;');
        PRINT 'Integracion.v_JustificacionDetalleSifcnp creada/actualizada.';
    END TRY
    BEGIN CATCH
        PRINT 'Integracion.v_JustificacionDetalleSifcnp OMITIDA: ' + ERROR_MESSAGE();
    END CATCH;
END
ELSE
    PRINT 'SIFCNP no disponible: se omiten las vistas v_*Sifcnp.';
GO

/* ############################################################################
   SECCION I — VISTA LEGADA SIFCNP  (esquema dbo, MAYUSCULAS_SNAKE a proposito)
   ----------------------------------------------------------------------------
   dbo.V_JUSTIFICACIONES_DETALLE: compatibilidad con consumidores SIFCNP. Usa
   nomenclatura legada DELIBERADAMENTE (no aplicar PascalCase). Depende de tablas
   locales dbo.RH_*; se crea solo si las cuatro existen.
   ############################################################################ */

IF  OBJECT_ID('dbo.RH_JUSTIFICACIONES_ENC', 'U') IS NOT NULL
AND OBJECT_ID('dbo.RH_JUSTIFICACIONES_DET', 'U') IS NOT NULL
AND OBJECT_ID('dbo.RH_FUNCIONARIOS', 'U')        IS NOT NULL
AND OBJECT_ID('dbo.RH_CENTROS_COSTO', 'U')       IS NOT NULL
BEGIN
    EXEC('
    CREATE OR ALTER VIEW dbo.V_JUSTIFICACIONES_DETALLE AS
    SELECT
        ENC.NUM_JUSTIFICACION AS NUM_JUSTIFICACION,
        DET.COD_LINEA         AS COD_LINEA,
        DET.FEC_JUSTIFICACION AS FEC_JUSTIFICACION,
        LTRIM(RTRIM(ISNULL(USR_SOL.DES_NOMBRE, '''') + '' '' + ISNULL(USR_SOL.DES_APELLIDO1, '''') + '' '' + ISNULL(USR_SOL.DES_APELLIDO2, ''''))) AS SOLICITANTE,
        LTRIM(RTRIM(ISNULL(USR_AUT.DES_NOMBRE, '''') + '' '' + ISNULL(USR_AUT.DES_APELLIDO1, '''') + '' '' + ISNULL(USR_AUT.DES_APELLIDO2, ''''))) AS AUTORIZA,
        LTRIM(RTRIM(ISNULL(USR_APR.DES_NOMBRE, '''') + '' '' + ISNULL(USR_APR.DES_APELLIDO1, '''') + '' '' + ISNULL(USR_APR.DES_APELLIDO2, ''''))) AS APRUEBA,
        ENC.FEC_CONFECCION    AS FEC_CONFECCION,
        ENC.DES_JUSTIFICACION AS DES_JUSTIFICACION,
        ENC.DES_OBSERVACIONES AS DES_OBSERVACIONES,
        CEN.DES_CENTRO        AS CENTRO,
        ENC.COD_ESTADO        AS COD_ESTADO,
        ENC.FEC_AUTORIZACION  AS FEC_AUTORIZACION,
        ENC.FEC_APROBACION    AS FEC_APROBACION,
        ENC.FEC_ANULACION     AS FEC_ANULACION,
        ENC.COD_ANULA         AS COD_ANULA,
        ENC.DES_MOTIVO        AS DES_MOTIVO,
        DET.IND_CONCEPTO      AS IND_CONCEPTO
    FROM dbo.RH_JUSTIFICACIONES_ENC AS ENC
        INNER JOIN dbo.RH_JUSTIFICACIONES_DET AS DET ON DET.NUM_JUSTIFICACION = ENC.NUM_JUSTIFICACION
        LEFT  JOIN dbo.RH_FUNCIONARIOS AS USR_SOL ON USR_SOL.COD_FUNCIONARIO = ENC.COD_SOLICITANTE
        LEFT  JOIN dbo.RH_FUNCIONARIOS AS USR_AUT ON USR_AUT.COD_FUNCIONARIO = ENC.COD_AUTORIZA
        LEFT  JOIN dbo.RH_FUNCIONARIOS AS USR_APR ON USR_APR.COD_FUNCIONARIO = ENC.COD_APRUEBA
        LEFT  JOIN dbo.RH_CENTROS_COSTO AS CEN    ON CEN.COD_CENTRO = ENC.COD_CENTRO;');

    PRINT 'Vista legada dbo.V_JUSTIFICACIONES_DETALLE creada/actualizada.';
END
ELSE
    PRINT 'Tablas dbo.RH_* ausentes: se omite la vista legada dbo.V_JUSTIFICACIONES_DETALLE.';
GO

/* ############################################################################
   SECCION J — COMPATIBILIDAD: dbo.Estructuras_Organizacionales
   ----------------------------------------------------------------------------
   El backend referencia dbo.Estructuras_Organizacionales en UNA sola consulta
   (JustificacionesSql.GetDetalleJefaturaEncabezado), uniendo
       eo.EstructuraOrganizacionalID = u.UnidadID
   mientras el resto del codigo usa RecursosHumanos.EstructuraOrganizacional
   con  eo.CodigoOrigen = CAST(u.UnidadId AS VARCHAR(50)). Es una inconsistencia
   del backend (ver informe de observaciones).

   Para que esa consulta no falle en entornos sin la tabla legada, se crea un
   SHIM (vista) SOLO si el objeto no existe. Expone EstructuraOrganizacionalID
   como el codigo numerico de unidad (= CodigoOrigen) para que el join calce con
   UnidadId. En produccion, donde exista la tabla real, la guarda lo respeta y
   no la toca.
   ############################################################################ */

IF OBJECT_ID('dbo.Estructuras_Organizacionales') IS NULL
BEGIN
    EXEC('
    CREATE VIEW dbo.Estructuras_Organizacionales AS
    SELECT
        TRY_CAST(eo.CodigoOrigen AS INT) AS EstructuraOrganizacionalID,
        eo.Nombre                        AS Nombre,
        eo.CodigoOrigen                  AS CodigoOrigen,
        eo.EstadoRegistroId              AS EstadoRegistroId
    FROM RecursosHumanos.EstructuraOrganizacional eo
    WHERE TRY_CAST(eo.CodigoOrigen AS INT) IS NOT NULL;');

    PRINT 'Shim de compatibilidad dbo.Estructuras_Organizacionales creado (vista).';
END
ELSE
    PRINT 'dbo.Estructuras_Organizacionales ya existe: no se modifica.';
GO

SET NOEXEC OFF;
GO
PRINT 'Script 02 completado: estructura completa lista.';
GO
