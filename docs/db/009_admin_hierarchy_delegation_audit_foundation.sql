/* ============================================================
   INTEGRA_CNP - Migracion incremental
   Base de catalogos admin, jerarquias, delegaciones y auditoria funcional
   ============================================================ */

USE INTEGRA_CNP;
GO

IF OBJECT_ID('dbo.Cat_EstadosRegistro', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cat_EstadosRegistro (
        EstadoRegistroID INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(50) NOT NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_CatEstadosRegistro_Fec_Registro DEFAULT GETDATE()
    );
END;
GO

MERGE dbo.Cat_EstadosRegistro AS tgt
USING (
    SELECT 1 AS EstadoRegistroID, 'Activo' AS Descripcion
    UNION ALL SELECT 2, 'Inactivo'
) AS src
ON tgt.EstadoRegistroID = src.EstadoRegistroID
WHEN NOT MATCHED THEN
    INSERT (EstadoRegistroID, Descripcion, Usr_Registro)
    VALUES (src.EstadoRegistroID, src.Descripcion, 'seed');
GO

IF OBJECT_ID('dbo.Cat_TiposEventoAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cat_TiposEventoAuditoria (
        TipoEventoAuditoriaID INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(100) NOT NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_CatTiposEventoAuditoria_Fec_Registro DEFAULT GETDATE()
    );
END;
GO

MERGE dbo.Cat_TiposEventoAuditoria AS tgt
USING (
    SELECT 1 AS TipoEventoAuditoriaID, 'CreacionJustificacion' AS Descripcion
    UNION ALL SELECT 2, 'ResolucionJustificacionAprobada'
    UNION ALL SELECT 3, 'ResolucionJustificacionRechazada'
    UNION ALL SELECT 4, 'AltaJerarquia'
    UNION ALL SELECT 5, 'CambioEstadoJerarquia'
    UNION ALL SELECT 6, 'AltaDelegacion'
    UNION ALL SELECT 7, 'CambioEstadoDelegacion'
) AS src
ON tgt.TipoEventoAuditoriaID = src.TipoEventoAuditoriaID
WHEN NOT MATCHED THEN
    INSERT (TipoEventoAuditoriaID, Descripcion, Usr_Registro)
    VALUES (src.TipoEventoAuditoriaID, src.Descripcion, 'seed');
GO

IF OBJECT_ID('dbo.Cat_ResultadosAuditoria', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Cat_ResultadosAuditoria (
        ResultadoAuditoriaID INT NOT NULL PRIMARY KEY,
        Descripcion VARCHAR(50) NOT NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_CatResultadosAuditoria_Fec_Registro DEFAULT GETDATE()
    );
END;
GO

MERGE dbo.Cat_ResultadosAuditoria AS tgt
USING (
    SELECT 1 AS ResultadoAuditoriaID, 'Exito' AS Descripcion
    UNION ALL SELECT 2, 'Fallo'
    UNION ALL SELECT 3, 'Denegado'
) AS src
ON tgt.ResultadoAuditoriaID = src.ResultadoAuditoriaID
WHEN NOT MATCHED THEN
    INSERT (ResultadoAuditoriaID, Descripcion, Usr_Registro)
    VALUES (src.ResultadoAuditoriaID, src.Descripcion, 'seed');
GO

IF OBJECT_ID('dbo.Estructuras_Organizacionales', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Estructuras_Organizacionales (
        EstructuraOrganizacionalID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        Nombre VARCHAR(150) NOT NULL,
        CodigoOrigen VARCHAR(50) NULL,
        EstructuraPadreID INT NULL,
        EstadoRegistroID INT NOT NULL,
        VigenciaDesde DATETIME NULL,
        VigenciaHasta DATETIME NULL,
        CONSTRAINT FK_Estructuras_Padre
            FOREIGN KEY (EstructuraPadreID) REFERENCES dbo.Estructuras_Organizacionales(EstructuraOrganizacionalID),
        CONSTRAINT FK_Estructuras_EstadoRegistro
            FOREIGN KEY (EstadoRegistroID) REFERENCES dbo.Cat_EstadosRegistro(EstadoRegistroID)
    );
END;
GO

IF OBJECT_ID('dbo.Jerarquias_Aprobacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Jerarquias_Aprobacion (
        JerarquiaAprobacionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        AprobadorUsuarioID INT NOT NULL,
        EstructuraOrganizacionalID INT NOT NULL,
        NivelAprobacion INT NOT NULL,
        TipoRelacion VARCHAR(20) NOT NULL,
        EstadoRegistroID INT NOT NULL,
        VigenciaDesde DATETIME NOT NULL,
        VigenciaHasta DATETIME NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_JerarquiasAprobacion_Fec_Registro DEFAULT GETDATE(),
        CONSTRAINT FK_JerarquiasAprobacion_Aprobador
            FOREIGN KEY (AprobadorUsuarioID) REFERENCES dbo.Usuarios(UsuarioID),
        CONSTRAINT FK_JerarquiasAprobacion_Estructura
            FOREIGN KEY (EstructuraOrganizacionalID) REFERENCES dbo.Estructuras_Organizacionales(EstructuraOrganizacionalID),
        CONSTRAINT FK_JerarquiasAprobacion_EstadoRegistro
            FOREIGN KEY (EstadoRegistroID) REFERENCES dbo.Cat_EstadosRegistro(EstadoRegistroID),
        CONSTRAINT CK_JerarquiasAprobacion_TipoRelacion
            CHECK (TipoRelacion IN ('Vertical', 'Horizontal'))
    );
END;
GO

IF OBJECT_ID('dbo.Delegaciones_Aprobacion', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Delegaciones_Aprobacion (
        DelegacionAprobacionID INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        DeleganteUsuarioID INT NOT NULL,
        DelegadoUsuarioID INT NOT NULL,
        JerarquiaAprobacionID INT NULL,
        Motivo VARCHAR(250) NULL,
        EstadoRegistroID INT NOT NULL,
        VigenciaDesde DATETIME NOT NULL,
        VigenciaHasta DATETIME NULL,
        Usr_Registro VARCHAR(50) NOT NULL,
        Fec_Registro DATETIME NOT NULL CONSTRAINT DF_DelegacionesAprobacion_Fec_Registro DEFAULT GETDATE(),
        CONSTRAINT FK_DelegacionesAprobacion_Delegante
            FOREIGN KEY (DeleganteUsuarioID) REFERENCES dbo.Usuarios(UsuarioID),
        CONSTRAINT FK_DelegacionesAprobacion_Delegado
            FOREIGN KEY (DelegadoUsuarioID) REFERENCES dbo.Usuarios(UsuarioID),
        CONSTRAINT FK_DelegacionesAprobacion_Jerarquia
            FOREIGN KEY (JerarquiaAprobacionID) REFERENCES dbo.Jerarquias_Aprobacion(JerarquiaAprobacionID),
        CONSTRAINT FK_DelegacionesAprobacion_EstadoRegistro
            FOREIGN KEY (EstadoRegistroID) REFERENCES dbo.Cat_EstadosRegistro(EstadoRegistroID)
    );
END;
GO

IF OBJECT_ID('dbo.Auditoria_Eventos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Auditoria_Eventos (
        AuditoriaEventoID BIGINT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        FechaEvento DATETIME NOT NULL CONSTRAINT DF_AuditoriaEventos_FechaEvento DEFAULT GETDATE(),
        UsuarioID INT NULL,
        NombreUsuario VARCHAR(150) NOT NULL,
        RolCodigo VARCHAR(20) NOT NULL,
        TipoEventoAuditoriaID INT NOT NULL,
        DescripcionEvento VARCHAR(500) NOT NULL,
        ResultadoAuditoriaID INT NOT NULL,
        ReferenciaFuncional VARCHAR(100) NULL,
        PayloadResumen VARCHAR(1000) NULL,
        CONSTRAINT FK_AuditoriaEventos_Usuario
            FOREIGN KEY (UsuarioID) REFERENCES dbo.Usuarios(UsuarioID),
        CONSTRAINT FK_AuditoriaEventos_TipoEvento
            FOREIGN KEY (TipoEventoAuditoriaID) REFERENCES dbo.Cat_TiposEventoAuditoria(TipoEventoAuditoriaID),
        CONSTRAINT FK_AuditoriaEventos_Resultado
            FOREIGN KEY (ResultadoAuditoriaID) REFERENCES dbo.Cat_ResultadosAuditoria(ResultadoAuditoriaID)
    );
END;
GO

MERGE dbo.Roles AS tgt
USING (
    SELECT 4 AS RolID, 'Administrador' AS NombreRol
) AS src
ON tgt.RolID = src.RolID
WHEN NOT MATCHED THEN
    INSERT (RolID, NombreRol, Usr_Registro)
    VALUES (src.RolID, src.NombreRol, 'seed');
GO

IF COL_LENGTH('dbo.Justificaciones_Encabezado', 'RolResolucion') IS NULL
BEGIN
    ALTER TABLE dbo.Justificaciones_Encabezado
        ADD RolResolucion VARCHAR(20) NULL;
END;
GO

CREATE OR ALTER FUNCTION dbo.fn_AprobadoresVigentesPorSolicitante
(
    @SolicitanteUsuarioID INT,
    @FechaRef DATETIME
)
RETURNS TABLE
AS
RETURN
(
    WITH SolicitanteEstructura AS (
        SELECT DISTINCT eo.EstructuraOrganizacionalID
        FROM dbo.Usuarios u
        INNER JOIN dbo.Estructuras_Organizacionales eo
            ON eo.CodigoOrigen = CAST(u.UnidadID AS VARCHAR(50))
           AND eo.EstadoRegistroID = 1
           AND (eo.VigenciaDesde IS NULL OR eo.VigenciaDesde <= @FechaRef)
           AND (eo.VigenciaHasta IS NULL OR eo.VigenciaHasta >= @FechaRef)
        WHERE u.UsuarioID = @SolicitanteUsuarioID
    ),
    JerarquiasActivas AS (
        SELECT DISTINCT ja.JerarquiaAprobacionID, ja.AprobadorUsuarioID
        FROM dbo.Jerarquias_Aprobacion ja
        INNER JOIN SolicitanteEstructura se
            ON se.EstructuraOrganizacionalID = ja.EstructuraOrganizacionalID
        WHERE
            ja.EstadoRegistroID = 1
            AND ja.VigenciaDesde <= @FechaRef
            AND (ja.VigenciaHasta IS NULL OR ja.VigenciaHasta >= @FechaRef)
    ),
    AprobadoresJerarquia AS (
        SELECT
            ja.AprobadorUsuarioID,
            CAST('Jerarquia' AS VARCHAR(20)) AS Origen,
            CAST(NULL AS INT) AS DeleganteUsuarioID
        FROM JerarquiasActivas ja
    ),
    AprobadoresDelegacion AS (
        SELECT
            da.DelegadoUsuarioID AS AprobadorUsuarioID,
            CAST('Delegacion' AS VARCHAR(20)) AS Origen,
            da.DeleganteUsuarioID
        FROM dbo.Delegaciones_Aprobacion da
        INNER JOIN JerarquiasActivas ja
            ON ja.AprobadorUsuarioID = da.DeleganteUsuarioID
        WHERE
            da.EstadoRegistroID = 1
            AND da.VigenciaDesde <= @FechaRef
            AND (da.VigenciaHasta IS NULL OR da.VigenciaHasta >= @FechaRef)
            AND (da.JerarquiaAprobacionID IS NULL OR da.JerarquiaAprobacionID = ja.JerarquiaAprobacionID)
    )
    SELECT DISTINCT
        x.AprobadorUsuarioID,
        x.Origen,
        x.DeleganteUsuarioID
    FROM (
        SELECT * FROM AprobadoresJerarquia
        UNION ALL
        SELECT * FROM AprobadoresDelegacion
    ) x
);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Estructuras_CodigoOrigen_Activo' AND object_id = OBJECT_ID('dbo.Estructuras_Organizacionales'))
BEGIN
    CREATE INDEX IX_Estructuras_CodigoOrigen_Activo
        ON dbo.Estructuras_Organizacionales (CodigoOrigen, EstadoRegistroID);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Jerarquias_Aprobador_Estructura_Vigencia' AND object_id = OBJECT_ID('dbo.Jerarquias_Aprobacion'))
BEGIN
    CREATE INDEX IX_Jerarquias_Aprobador_Estructura_Vigencia
        ON dbo.Jerarquias_Aprobacion (AprobadorUsuarioID, EstructuraOrganizacionalID, EstadoRegistroID, VigenciaDesde, VigenciaHasta);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Delegaciones_Delegado_Vigencia' AND object_id = OBJECT_ID('dbo.Delegaciones_Aprobacion'))
BEGIN
    CREATE INDEX IX_Delegaciones_Delegado_Vigencia
        ON dbo.Delegaciones_Aprobacion (DelegadoUsuarioID, EstadoRegistroID, VigenciaDesde, VigenciaHasta);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auditoria_FechaEvento' AND object_id = OBJECT_ID('dbo.Auditoria_Eventos'))
BEGIN
    CREATE INDEX IX_Auditoria_FechaEvento
        ON dbo.Auditoria_Eventos (FechaEvento DESC);
END;
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_Auditoria_TipoResultadoFecha' AND object_id = OBJECT_ID('dbo.Auditoria_Eventos'))
BEGIN
    CREATE INDEX IX_Auditoria_TipoResultadoFecha
        ON dbo.Auditoria_Eventos (TipoEventoAuditoriaID, ResultadoAuditoriaID, FechaEvento DESC);
END;
GO
