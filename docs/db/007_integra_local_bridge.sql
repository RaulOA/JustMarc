/* ============================================================
   INTEGRA_CNP - Local Bridge (WIZDOM + SIFCNP)
   Objetivo: concentrar datos externos en staging local y exponer
   vistas canonicas para consumo interno de Marcas.
   ============================================================ */

IF DB_ID('INTEGRA_CNP') IS NULL
BEGIN
    RAISERROR('La base de datos INTEGRA_CNP no existe. Ejecute primero 001_init_integra_cnp.sql', 16, 1);
    RETURN;
END;
GO

USE INTEGRA_CNP;
GO

SET XACT_ABORT ON;
GO

/* =========================
   Esquemas puente
   ========================= */
IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'stg')
BEGIN
    EXEC('CREATE SCHEMA stg');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'bridge')
BEGIN
    EXEC('CREATE SCHEMA bridge');
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'ext')
BEGIN
    EXEC('CREATE SCHEMA ext');
END;
GO

/* =========================
   Control de cargas
   ========================= */
IF OBJECT_ID('stg.BridgeCargaLote', 'U') IS NULL
BEGIN
    CREATE TABLE stg.BridgeCargaLote (
        LoteID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        SourceSystem VARCHAR(30) NOT NULL,
        SourceObject VARCHAR(128) NOT NULL,
        InicioCargaUtc DATETIME2(0) NOT NULL CONSTRAINT DF_BridgeCargaLote_InicioCargaUtc DEFAULT SYSUTCDATETIME(),
        FinCargaUtc DATETIME2(0) NULL,
        FilasLeidas INT NULL,
        FilasInsertadas INT NULL,
        FilasActualizadas INT NULL,
        EstadoCarga VARCHAR(20) NOT NULL CONSTRAINT DF_BridgeCargaLote_EstadoCarga DEFAULT 'INICIADO',
        DetalleError VARCHAR(1000) NULL,
        Usr_Registro VARCHAR(50) NOT NULL CONSTRAINT DF_BridgeCargaLote_Usr_Registro DEFAULT 'bridge',
        Fec_Registro DATETIME2(0) NOT NULL CONSTRAINT DF_BridgeCargaLote_Fec_Registro DEFAULT SYSUTCDATETIME()
    );
END;
GO

/* =========================
   Staging WIZDOM
   ========================= */
IF OBJECT_ID('stg.Wizdom_EmpleadoRaw', 'U') IS NULL
BEGIN
    CREATE TABLE stg.Wizdom_EmpleadoRaw (
        RawID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LoteID BIGINT NULL,
        SourceSystem VARCHAR(30) NOT NULL CONSTRAINT DF_WizdomEmpRaw_SourceSystem DEFAULT 'WIZDOM',
        SourceObject VARCHAR(128) NOT NULL CONSTRAINT DF_WizdomEmpRaw_SourceObject DEFAULT 'optec1empleado',
        SourceRowKey VARCHAR(200) NOT NULL,
        Compania VARCHAR(10) NULL,
        CodigoEmpleado VARCHAR(50) NULL,
        NumeroIdentificacion VARCHAR(50) NULL,
        Nombre VARCHAR(100) NULL,
        Apellido1 VARCHAR(100) NULL,
        Apellido2 VARCHAR(100) NULL,
        NombreCompleto VARCHAR(250) NULL,
        Correo VARCHAR(150) NULL,
        CodigoJefe VARCHAR(50) NULL,
        CodigoNodoOrganigrama VARCHAR(50) NULL,
        EstadoEmpleado VARCHAR(30) NULL,
        FechaIngreso DATE NULL,
        FechaEgreso DATE NULL,
        HashFila VARBINARY(32) NULL,
        FechaCargaUtc DATETIME2(0) NOT NULL CONSTRAINT DF_WizdomEmpRaw_FechaCargaUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_WizdomEmpRaw_Lote FOREIGN KEY (LoteID) REFERENCES stg.BridgeCargaLote(LoteID)
    );
END;
GO

IF OBJECT_ID('stg.Wizdom_RelacionEmpleadoRaw', 'U') IS NULL
BEGIN
    CREATE TABLE stg.Wizdom_RelacionEmpleadoRaw (
        RawID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LoteID BIGINT NULL,
        SourceSystem VARCHAR(30) NOT NULL CONSTRAINT DF_WizdomRelRaw_SourceSystem DEFAULT 'WIZDOM',
        SourceObject VARCHAR(128) NOT NULL CONSTRAINT DF_WizdomRelRaw_SourceObject DEFAULT 'relaciones_empleado',
        SourceRowKey VARCHAR(200) NOT NULL,
        CodigoEmpleado VARCHAR(50) NULL,
        CodigoJefe VARCHAR(50) NULL,
        CodigoNodoOrganigrama VARCHAR(50) NULL,
        NivelJerarquico INT NULL,
        EstadoRelacion VARCHAR(30) NULL,
        HashFila VARBINARY(32) NULL,
        FechaCargaUtc DATETIME2(0) NOT NULL CONSTRAINT DF_WizdomRelRaw_FechaCargaUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_WizdomRelRaw_Lote FOREIGN KEY (LoteID) REFERENCES stg.BridgeCargaLote(LoteID)
    );
END;
GO

/* =========================
   Staging SIFCNP (historico RF-06)
   ========================= */
IF OBJECT_ID('stg.Sifcnp_JustificacionesEncRaw', 'U') IS NULL
BEGIN
    CREATE TABLE stg.Sifcnp_JustificacionesEncRaw (
        RawID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LoteID BIGINT NULL,
        SourceSystem VARCHAR(30) NOT NULL CONSTRAINT DF_SifEncRaw_SourceSystem DEFAULT 'SIFCNP',
        SourceObject VARCHAR(128) NOT NULL CONSTRAINT DF_SifEncRaw_SourceObject DEFAULT 'RH_JUSTIFICACIONES_ENC',
        SourceRowKey VARCHAR(200) NOT NULL,
        NumJustificacion VARCHAR(30) NULL,
        CodSolicitante VARCHAR(50) NULL,
        CodEstado VARCHAR(20) NULL,
        FecConfeccion DATETIME2(0) NULL,
        FecAutorizacion DATETIME2(0) NULL,
        FecAprobacion DATETIME2(0) NULL,
        DesJustificacion VARCHAR(1000) NULL,
        DesMotivo VARCHAR(1000) NULL,
        DesObservaciones VARCHAR(1000) NULL,
        HashFila VARBINARY(32) NULL,
        FechaCargaUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SifEncRaw_FechaCargaUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SifEncRaw_Lote FOREIGN KEY (LoteID) REFERENCES stg.BridgeCargaLote(LoteID)
    );
END;
GO

IF OBJECT_ID('stg.Sifcnp_JustificacionesDetRaw', 'U') IS NULL
BEGIN
    CREATE TABLE stg.Sifcnp_JustificacionesDetRaw (
        RawID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LoteID BIGINT NULL,
        SourceSystem VARCHAR(30) NOT NULL CONSTRAINT DF_SifDetRaw_SourceSystem DEFAULT 'SIFCNP',
        SourceObject VARCHAR(128) NOT NULL CONSTRAINT DF_SifDetRaw_SourceObject DEFAULT 'RH_JUSTIFICACIONES_DET',
        SourceRowKey VARCHAR(200) NOT NULL,
        NumJustificacion VARCHAR(30) NULL,
        CodLinea VARCHAR(20) NULL,
        IndConcepto VARCHAR(20) NULL,
        FecJustificacion DATE NULL,
        IndRebajarPlanilla VARCHAR(5) NULL,
        HashFila VARBINARY(32) NULL,
        FechaCargaUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SifDetRaw_FechaCargaUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SifDetRaw_Lote FOREIGN KEY (LoteID) REFERENCES stg.BridgeCargaLote(LoteID)
    );
END;
GO

IF OBJECT_ID('stg.Sifcnp_TipoMarcaRaw', 'U') IS NULL
BEGIN
    CREATE TABLE stg.Sifcnp_TipoMarcaRaw (
        RawID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LoteID BIGINT NULL,
        SourceSystem VARCHAR(30) NOT NULL CONSTRAINT DF_SifTipoRaw_SourceSystem DEFAULT 'SIFCNP',
        SourceObject VARCHAR(128) NOT NULL CONSTRAINT DF_SifTipoRaw_SourceObject DEFAULT 'RH_TI_TipoMarca',
        SourceRowKey VARCHAR(200) NOT NULL,
        IndConcepto VARCHAR(20) NULL,
        DesTipoConcepto VARCHAR(200) NULL,
        HashFila VARBINARY(32) NULL,
        FechaCargaUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SifTipoRaw_FechaCargaUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SifTipoRaw_Lote FOREIGN KEY (LoteID) REFERENCES stg.BridgeCargaLote(LoteID)
    );
END;
GO

IF OBJECT_ID('stg.Sifcnp_EstadoMarcasRaw', 'U') IS NULL
BEGIN
    CREATE TABLE stg.Sifcnp_EstadoMarcasRaw (
        RawID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        LoteID BIGINT NULL,
        SourceSystem VARCHAR(30) NOT NULL CONSTRAINT DF_SifEstRaw_SourceSystem DEFAULT 'SIFCNP',
        SourceObject VARCHAR(128) NOT NULL CONSTRAINT DF_SifEstRaw_SourceObject DEFAULT 'RH_TI_EstadoMarcas',
        SourceRowKey VARCHAR(200) NOT NULL,
        CodEstado VARCHAR(20) NULL,
        DesEstado VARCHAR(200) NULL,
        HashFila VARBINARY(32) NULL,
        FechaCargaUtc DATETIME2(0) NOT NULL CONSTRAINT DF_SifEstRaw_FechaCargaUtc DEFAULT SYSUTCDATETIME(),
        CONSTRAINT FK_SifEstRaw_Lote FOREIGN KEY (LoteID) REFERENCES stg.BridgeCargaLote(LoteID)
    );
END;
GO

/* =========================
   Indices minimos
   ========================= */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Wizdom_EmpleadoRaw_CodigoEmpleado' AND object_id = OBJECT_ID('stg.Wizdom_EmpleadoRaw'))
BEGIN
    CREATE INDEX IX_Wizdom_EmpleadoRaw_CodigoEmpleado
        ON stg.Wizdom_EmpleadoRaw (CodigoEmpleado, FechaCargaUtc DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Wizdom_EmpleadoRaw_NumeroIdentificacion' AND object_id = OBJECT_ID('stg.Wizdom_EmpleadoRaw'))
BEGIN
    CREATE INDEX IX_Wizdom_EmpleadoRaw_NumeroIdentificacion
        ON stg.Wizdom_EmpleadoRaw (NumeroIdentificacion, FechaCargaUtc DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Wizdom_RelacionEmpleadoRaw_CodigoEmpleado' AND object_id = OBJECT_ID('stg.Wizdom_RelacionEmpleadoRaw'))
BEGIN
    CREATE INDEX IX_Wizdom_RelacionEmpleadoRaw_CodigoEmpleado
        ON stg.Wizdom_RelacionEmpleadoRaw (CodigoEmpleado, FechaCargaUtc DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sifcnp_EncRaw_NumJustificacion' AND object_id = OBJECT_ID('stg.Sifcnp_JustificacionesEncRaw'))
BEGIN
    CREATE INDEX IX_Sifcnp_EncRaw_NumJustificacion
        ON stg.Sifcnp_JustificacionesEncRaw (NumJustificacion, FechaCargaUtc DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Sifcnp_DetRaw_NumJustificacion' AND object_id = OBJECT_ID('stg.Sifcnp_JustificacionesDetRaw'))
BEGIN
    CREATE INDEX IX_Sifcnp_DetRaw_NumJustificacion
        ON stg.Sifcnp_JustificacionesDetRaw (NumJustificacion, CodLinea, FechaCargaUtc DESC);
END;
GO

/* =========================
   Vistas canonicas de bridge
   ========================= */
CREATE OR ALTER VIEW bridge.vw_UsuariosCanonico
AS
SELECT
    wr.CodigoEmpleado AS CodigoEmpleado,
    wr.NumeroIdentificacion AS Cedula,
    COALESCE(NULLIF(wr.NombreCompleto, ''), CONCAT(COALESCE(wr.Nombre, ''), ' ', COALESCE(wr.Apellido1, ''), ' ', COALESCE(wr.Apellido2, ''))) AS NombreCompleto,
    wr.Correo,
    wr.CodigoJefe,
    wr.CodigoNodoOrganigrama,
    wr.Compania,
    wr.EstadoEmpleado,
    wr.FechaIngreso,
    wr.FechaEgreso,
    wr.FechaCargaUtc,
    wr.SourceRowKey
FROM stg.Wizdom_EmpleadoRaw wr;
GO

CREATE OR ALTER VIEW bridge.vw_SifcnpEstadosCanonico
AS
SELECT
    er.CodEstado,
    er.DesEstado,
    er.FechaCargaUtc,
    er.SourceRowKey
FROM stg.Sifcnp_EstadoMarcasRaw er;
GO

CREATE OR ALTER VIEW bridge.vw_SifcnpTiposCanonico
AS
SELECT
    tr.IndConcepto,
    tr.DesTipoConcepto,
    tr.FechaCargaUtc,
    tr.SourceRowKey
FROM stg.Sifcnp_TipoMarcaRaw tr;
GO

CREATE OR ALTER VIEW bridge.vw_SifcnpHistoricoEncabezadoCanonico
AS
SELECT
    er.NumJustificacion,
    er.CodSolicitante,
    er.CodEstado,
    er.FecConfeccion,
    er.FecAutorizacion,
    er.FecAprobacion,
    er.DesJustificacion,
    er.DesMotivo,
    er.DesObservaciones,
    es.DesEstado AS EstadoDescripcion,
    er.FechaCargaUtc,
    er.SourceRowKey
FROM stg.Sifcnp_JustificacionesEncRaw er
LEFT JOIN bridge.vw_SifcnpEstadosCanonico es
    ON es.CodEstado = er.CodEstado;
GO

CREATE OR ALTER VIEW bridge.vw_SifcnpHistoricoDetalleCanonico
AS
SELECT
    dr.NumJustificacion,
    dr.CodLinea,
    dr.IndConcepto,
    tp.DesTipoConcepto AS TipoConceptoDescripcion,
    dr.FecJustificacion,
    dr.IndRebajarPlanilla,
    dr.FechaCargaUtc,
    dr.SourceRowKey
FROM stg.Sifcnp_JustificacionesDetRaw dr
LEFT JOIN bridge.vw_SifcnpTiposCanonico tp
    ON tp.IndConcepto = dr.IndConcepto;
GO

CREATE OR ALTER VIEW bridge.vw_SifcnpHistoricoCompletoCanonico
AS
SELECT
    e.NumJustificacion,
    e.CodSolicitante,
    e.CodEstado,
    e.EstadoDescripcion,
    e.FecConfeccion,
    e.FecAutorizacion,
    e.FecAprobacion,
    e.DesJustificacion,
    e.DesMotivo,
    e.DesObservaciones,
    d.CodLinea,
    d.IndConcepto,
    d.TipoConceptoDescripcion,
    d.FecJustificacion,
    d.IndRebajarPlanilla,
    CASE
        WHEN e.FechaCargaUtc >= d.FechaCargaUtc THEN e.FechaCargaUtc
        ELSE d.FechaCargaUtc
    END AS FechaCargaUtc,
    e.SourceRowKey AS SourceRowKeyEnc,
    d.SourceRowKey AS SourceRowKeyDet
FROM bridge.vw_SifcnpHistoricoEncabezadoCanonico e
INNER JOIN bridge.vw_SifcnpHistoricoDetalleCanonico d
    ON d.NumJustificacion = e.NumJustificacion;
GO

/* =========================
   Placeholders para contratos ext (sin dependencia externa)
   Nota: estos bloques estan deshabilitados por defecto.
   ========================= */
/*
IF DB_ID('WIZDOM') IS NOT NULL AND OBJECT_ID('ext.Src_WIZDOM_optec1empleado', 'SN') IS NULL
    EXEC('CREATE SYNONYM ext.Src_WIZDOM_optec1empleado FOR [WIZDOM].[dbo].[optec1empleado]');

IF DB_ID('WIZDOM') IS NOT NULL AND OBJECT_ID('ext.Src_WIZDOM_relaciones_empleado', 'SN') IS NULL
    EXEC('CREATE SYNONYM ext.Src_WIZDOM_relaciones_empleado FOR [WIZDOM].[dbo].[relaciones_empleado]');

IF DB_ID('SIFCNP') IS NOT NULL AND OBJECT_ID('ext.Src_SIFCNP_RH_JUSTIFICACIONES_ENC', 'SN') IS NULL
    EXEC('CREATE SYNONYM ext.Src_SIFCNP_RH_JUSTIFICACIONES_ENC FOR [SIFCNP].[dbo].[RH_JUSTIFICACIONES_ENC]');

IF DB_ID('SIFCNP') IS NOT NULL AND OBJECT_ID('ext.Src_SIFCNP_RH_JUSTIFICACIONES_DET', 'SN') IS NULL
    EXEC('CREATE SYNONYM ext.Src_SIFCNP_RH_JUSTIFICACIONES_DET FOR [SIFCNP].[dbo].[RH_JUSTIFICACIONES_DET]');

IF DB_ID('SIFCNP') IS NOT NULL AND OBJECT_ID('ext.Src_SIFCNP_RH_TI_TipoMarca', 'SN') IS NULL
    EXEC('CREATE SYNONYM ext.Src_SIFCNP_RH_TI_TipoMarca FOR [SIFCNP].[dbo].[RH_TI_TipoMarca]');

IF DB_ID('SIFCNP') IS NOT NULL AND OBJECT_ID('ext.Src_SIFCNP_RH_TI_EstadoMarcas', 'SN') IS NULL
    EXEC('CREATE SYNONYM ext.Src_SIFCNP_RH_TI_EstadoMarcas FOR [SIFCNP].[dbo].[RH_TI_EstadoMarcas]');

IF DB_ID('SIFCNP') IS NOT NULL AND OBJECT_ID('ext.Src_SIFCNP_V_TI_RH_JUST_MARCA', 'SN') IS NULL
    EXEC('CREATE SYNONYM ext.Src_SIFCNP_V_TI_RH_JUST_MARCA FOR [SIFCNP].[dbo].[V_TI_RH_JUST_MARCA]');
*/
GO
