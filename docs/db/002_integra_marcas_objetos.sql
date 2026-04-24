/* ============================================================
   INTEGRA_CNP - Objetos y Vistas de Integración
   
   Responsabilidad: crear objetos dependientes, lógica de aprobación
   e integración externa de solo lectura.
   
   Incluye:
   - Función de alcance de aprobadores
   - 4 vistas de solo lectura sobre tablas externas (WIZDOM, SIFCNP)
   - Procedimientos opcionales de sincronización
   
   Convención: COLLATE SQL_Latin1_General_CP1_CI_AS en joins cross-database
   
   Fecha generación: 2026-04-23
   Basado en: spec sql_consolidacion_dos_archivos_spec.md
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
   Función de alcance de aprobadores vigentes
   ========================= */

IF OBJECT_ID('Operacion.fn_AprobadoresVigentesPorSolicitante', 'FN') IS NOT NULL
BEGIN
    DROP FUNCTION Operacion.fn_AprobadoresVigentesPorSolicitante;
END;
GO

CREATE FUNCTION Operacion.fn_AprobadoresVigentesPorSolicitante
(
    @SolicitanteUsuarioId INT,
    @FechaRef DATETIME2
)
RETURNS TABLE
AS
RETURN
(
    WITH SolicitanteEstructura AS (
        /* Obtiene la estructura organizacional del solicitante */
        SELECT DISTINCT eo.EstructuraOrganizacionalId
        FROM RecursosHumanos.Usuario u
        INNER JOIN RecursosHumanos.EstructuraOrganizacional eo
            ON eo.CodigoOrigen = CAST(u.UnidadId AS VARCHAR(50)) COLLATE SQL_Latin1_General_CP1_CI_AS
           AND eo.EstadoRegistroId = 1
           AND (eo.VigenciaDesde IS NULL OR eo.VigenciaDesde <= @FechaRef)
           AND (eo.VigenciaHasta IS NULL OR eo.VigenciaHasta >= @FechaRef)
        WHERE u.UsuarioId = @SolicitanteUsuarioId
    ),
    JerarquiasActivas AS (
        /* Jerarquías de aprobación vigentes para la estructura del solicitante */
        SELECT DISTINCT ja.JerarquiaAprobacionId, ja.AprobadorUsuarioId
        FROM Operacion.JerarquiaAprobacion ja
        INNER JOIN SolicitanteEstructura se
            ON se.EstructuraOrganizacionalId = ja.EstructuraOrganizacionalId
        WHERE
            ja.EstadoRegistroId = 1
            AND ja.VigenciaDesde <= @FechaRef
            AND (ja.VigenciaHasta IS NULL OR ja.VigenciaHasta >= @FechaRef)
    ),
    AprobadoresJerarquia AS (
        /* Aprobadores provenientes de jerarquía */
        SELECT
            ja.AprobadorUsuarioId,
            CAST('Jerarquia' AS VARCHAR(20)) AS Origen,
            CAST(NULL AS INT) AS DeleganteUsuarioId
        FROM JerarquiasActivas ja
    ),
    AprobadoresDelegacion AS (
        /* Aprobadores provenientes de delegación */
        SELECT
            da.DelegadoUsuarioId AS AprobadorUsuarioId,
            CAST('Delegacion' AS VARCHAR(20)) AS Origen,
            da.DeleganteUsuarioId
        FROM Operacion.DelegacionAprobacion da
        INNER JOIN JerarquiasActivas ja
            ON ja.AprobadorUsuarioId = da.DeleganteUsuarioId
        WHERE
            da.EstadoRegistroId = 1
            AND da.VigenciaDesde <= @FechaRef
            AND (da.VigenciaHasta IS NULL OR da.VigenciaHasta >= @FechaRef)
            AND (da.JerarquiaAprobacionId IS NULL OR da.JerarquiaAprobacionId = ja.JerarquiaAprobacionId)
    )
    SELECT DISTINCT
        x.AprobadorUsuarioId,
        x.Origen,
        x.DeleganteUsuarioId
    FROM (
        SELECT * FROM AprobadoresJerarquia
        UNION ALL
        SELECT * FROM AprobadoresDelegacion
    ) x
);
GO

/* =========================
   Vistas de integración externa - Esquema Integracion
   ========================= */

/* Vista 1: Empleados desde WIZDOM */
IF OBJECT_ID('Integracion.v_EmpleadoWizdom', 'V') IS NOT NULL
BEGIN
    DROP VIEW Integracion.v_EmpleadoWizdom;
END;
GO

CREATE VIEW Integracion.v_EmpleadoWizdom
AS
SELECT
    CAST(e.compania AS VARCHAR(10)) COLLATE SQL_Latin1_General_CP1_CI_AS AS Compania,
    CAST(e.codigo_empleado AS VARCHAR(50)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoEmpleado,
    CAST(e.numero_identificacion AS VARCHAR(64)) COLLATE SQL_Latin1_General_CP1_CI_AS AS NumeroIdentificacion,
    CAST(e.nombre AS VARCHAR(100)) COLLATE SQL_Latin1_General_CP1_CI_AS AS Nombre,
    CAST(e.primer_apellido AS VARCHAR(100)) COLLATE SQL_Latin1_General_CP1_CI_AS AS PrimerApellido,
    CAST(e.segundo_apellido AS VARCHAR(100)) COLLATE SQL_Latin1_General_CP1_CI_AS AS SegundoApellido,
    CAST(e.correo_electronico_principal AS VARCHAR(150)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CorreoElectronicoPrincipal,
    CAST(e.correo_electronico_alternativo AS VARCHAR(150)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CorreoElectronicoAlternativo,
    CAST(e.codigo_jefe AS VARCHAR(50)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoJefe,
    CAST(e.codigo_nodo_organigrama AS VARCHAR(50)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoNodoOrganigrama,
    CAST(e.estado_empleado AS VARCHAR(30)) COLLATE SQL_Latin1_General_CP1_CI_AS AS EstadoEmpleado,
    CAST(e.fecha_ingreso AS DATE) AS FechaIngreso,
    CAST(e.fecha_egreso AS DATE) AS FechaEgreso,
    CAST(e.tstamp AS DATETIME2) AS FechaActualizacion
FROM [WIZDOM].[dbo].[empleado] e;
GO

/* Vista 2: Organigrama desde WIZDOM */
IF OBJECT_ID('Integracion.v_OrganigramaWizdom', 'V') IS NOT NULL
BEGIN
    DROP VIEW Integracion.v_OrganigramaWizdom;
END;
GO

CREATE VIEW Integracion.v_OrganigramaWizdom
AS
SELECT
    CAST(o.codigo_nodo AS VARCHAR(50)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoNodo,
    CAST(o.nombre_nodo AS VARCHAR(150)) COLLATE SQL_Latin1_General_CP1_CI_AS AS NombreNodo,
    CAST(o.codigo_nodo_padre AS VARCHAR(50)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CodigoNodoPadre,
    CAST(o.tipo_nodo AS VARCHAR(50)) COLLATE SQL_Latin1_General_CP1_CI_AS AS TipoNodo,
    CAST(o.nivel_jerarquia AS INT) AS NivelJerarquia,
    CAST(o.estado_nodo AS VARCHAR(30)) COLLATE SQL_Latin1_General_CP1_CI_AS AS EstadoNodo,
    CAST(o.tstamp AS DATETIME2) AS FechaActualizacion
FROM [WIZDOM].[dbo].[organigrama] o;
GO

/* Vista 3: Encabezado de justificaciones desde SIFCNP */
IF OBJECT_ID('Integracion.v_JustificacionEncabezadoSifcnp', 'V') IS NOT NULL
BEGIN
    DROP VIEW Integracion.v_JustificacionEncabezadoSifcnp;
END;
GO

CREATE VIEW Integracion.v_JustificacionEncabezadoSifcnp
AS
SELECT
    CAST(j.id_justificacion_enc AS BIGINT) AS JustificacionEncabezadoId,
    CAST(j.cedula_funcionario AS VARCHAR(64)) COLLATE SQL_Latin1_General_CP1_CI_AS AS CedulaFuncionario,
    CAST(j.motivo_general AS VARCHAR(500)) COLLATE SQL_Latin1_General_CP1_CI_AS AS MotivoGeneral,
    CAST(j.fecha_creacion AS DATETIME2) AS FechaCreacion,
    CAST(j.estado_justificacion AS VARCHAR(50)) COLLATE SQL_Latin1_General_CP1_CI_AS AS EstadoJustificacion,
    CAST(j.fecha_resolucion AS DATETIME2) AS FechaResolucion,
    CAST(j.comentario_resolucion AS VARCHAR(500)) COLLATE SQL_Latin1_General_CP1_CI_AS AS ComentarioResolucion,
    CAST(j.tstamp AS DATETIME2) AS FechaActualizacion
FROM [SIFCNP].[dbo].[RH_JUSTIFICACIONES_ENC] j;
GO

/* Vista 4: Detalle de justificaciones desde SIFCNP */
IF OBJECT_ID('Integracion.v_JustificacionDetalleSifcnp', 'V') IS NOT NULL
BEGIN
    DROP VIEW Integracion.v_JustificacionDetalleSifcnp;
END;
GO

CREATE VIEW Integracion.v_JustificacionDetalleSifcnp
AS
SELECT
    CAST(j.id_justificacion_det AS BIGINT) AS JustificacionDetalleId,
    CAST(j.id_justificacion_enc AS BIGINT) AS JustificacionEncabezadoId,
    CAST(j.tipo_justificacion AS VARCHAR(100)) COLLATE SQL_Latin1_General_CP1_CI_AS AS TipoJustificacion,
    CAST(j.fecha_marca AS DATE) AS FechaMarca,
    CAST(j.observacion_detalle AS VARCHAR(250)) COLLATE SQL_Latin1_General_CP1_CI_AS AS ObservacionDetalle,
    CAST(j.tstamp AS DATETIME2) AS FechaActualizacion
FROM [SIFCNP].[dbo].[RH_JUSTIFICACIONES_DET] j;
GO

/* =========================
   Procedimiento de sincronización opcional
   
   Nota: Este procedimiento es opcional y puede utilizarse si se requiere
   cargar datos históricos desde SIFCNP hacia tablas internas de Operacion.
   La carga se ejecuta de forma controlada y auditada.
   ========================= */

IF OBJECT_ID('Operacion.usp_SincronizarJustificacionesDesdeHistorico', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE Operacion.usp_SincronizarJustificacionesDesdeHistorico;
END;
GO

CREATE PROCEDURE Operacion.usp_SincronizarJustificacionesDesdeHistorico
    @UsuarioEjecucion VARCHAR(100),
    @FechaInicio DATETIME2 = NULL,
    @FechaFin DATETIME2 = NULL,
    @MaximoRegistros INT = 1000
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    DECLARE @FilasInsertadas INT = 0;
    DECLARE @FilasActualizadas INT = 0;

    BEGIN TRY
        /* Sincronizar encabezados de justificaciones desde SIFCNP */
        WITH SifcnpEncabezados AS (
            SELECT
                CAST(j.id_justificacion_enc AS INT) AS JustificacionId,
                (SELECT UsuarioId FROM RecursosHumanos.Usuario 
                 WHERE Cedula COLLATE SQL_Latin1_General_CP1_CI_AS = j.cedula_funcionario COLLATE SQL_Latin1_General_CP1_CI_AS
                 LIMIT 1) AS UsuarioId,
                j.motivo_general AS MotivoGeneral,
                CASE 
                    WHEN j.estado_justificacion COLLATE SQL_Latin1_General_CP1_CI_AS = 'Pendiente' THEN 1
                    WHEN j.estado_justificacion COLLATE SQL_Latin1_General_CP1_CI_AS = 'Aprobada' THEN 2
                    WHEN j.estado_justificacion COLLATE SQL_Latin1_General_CP1_CI_AS = 'Rechazada' THEN 3
                    ELSE 1
                END AS EstadoJustificacionId,
                j.comentario_resolucion AS ComentarioResolucion,
                j.fecha_creacion AS FechaCreacion,
                j.tstamp AS FechaActualizacion
            FROM [SIFCNP].[dbo].[RH_JUSTIFICACIONES_ENC] j
            WHERE (@FechaInicio IS NULL OR j.fecha_creacion >= @FechaInicio)
              AND (@FechaFin IS NULL OR j.fecha_creacion <= @FechaFin)
        )
        MERGE INTO Operacion.Justificacion AS tgt
        USING SifcnpEncabezados AS src
        ON tgt.JustificacionId = src.JustificacionId
        WHEN NOT MATCHED AND src.UsuarioId IS NOT NULL THEN
            INSERT (UsuarioId, MotivoGeneral, ComentarioResolucion, EstadoJustificacionId, 
                    FechaCreacion, CreadoPor, FechaHoraCreacion)
            VALUES (src.UsuarioId, src.MotivoGeneral, src.ComentarioResolucion, src.EstadoJustificacionId,
                    src.FechaCreacion, @UsuarioEjecucion, SYSUTCDATETIME())
        WHEN MATCHED THEN
            UPDATE SET
                MotivoGeneral = src.MotivoGeneral,
                ComentarioResolucion = src.ComentarioResolucion,
                EstadoJustificacionId = src.EstadoJustificacionId,
                FechaCreacion = src.FechaCreacion,
                ModificadoPor = @UsuarioEjecucion,
                FechaHoraModificacion = SYSUTCDATETIME();

        SET @FilasInsertadas = @@ROWCOUNT;

        /* Registrar en auditoría */
        INSERT INTO Auditoria.EventoAuditoria 
            (NombreUsuario, RolCodigo, TipoEventoAuditoriaId, DescripcionEvento, ResultadoAuditoriaId, PayloadResumen)
        VALUES 
            (@UsuarioEjecucion, 'SISTEMA', 4, 
             'Sincronización de justificaciones desde histórico SIFCNP', 1,
             CONCAT('Registros procesados: ', @FilasInsertadas));

        PRINT CONCAT('Sincronización completada. Registros procesados: ', @FilasInsertadas);

    END TRY
    BEGIN CATCH
        DECLARE @ErrorMessage NVARCHAR(MAX) = ERROR_MESSAGE();
        DECLARE @ErrorNumber INT = ERROR_NUMBER();

        RAISERROR(@ErrorMessage, 16, 1);

        INSERT INTO Auditoria.EventoAuditoria 
            (NombreUsuario, RolCodigo, TipoEventoAuditoriaId, DescripcionEvento, ResultadoAuditoriaId, PayloadResumen)
        VALUES 
            (@UsuarioEjecucion, 'SISTEMA', 4, 
             'Sincronización fallida de justificaciones desde histórico SIFCNP', 2,
             CONCAT('Error: ', @ErrorMessage));
    END CATCH;
END;
GO

/* =========================
   Fin script de objetos
   ========================= */
PRINT 'Objetos y vistas de integración creados exitosamente.';
GO
