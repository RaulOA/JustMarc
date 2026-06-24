ALTER FUNCTION dbo.fn_AprobadoresVigentesPorSolicitante
(
    @SolicitanteUsuarioID INT,
    @FechaRef DATETIME
)
RETURNS TABLE
AS
RETURN
(
    WITH SolicitanteEstructura AS (
        SELECT DISTINCT eo.EstructuraOrganizacionalId
        FROM RecursosHumanos.Usuario u
        INNER JOIN RecursosHumanos.EstructuraOrganizacional eo
            ON eo.CodigoOrigen = CAST(u.UnidadId AS VARCHAR(50))
           AND eo.EstadoRegistroId = 1
           AND (eo.VigenciaDesde IS NULL OR eo.VigenciaDesde <= @FechaRef)
           AND (eo.VigenciaHasta IS NULL OR eo.VigenciaHasta >= @FechaRef)
        WHERE u.UsuarioId = @SolicitanteUsuarioID
    ),
    JerarquiasActivas AS (
        SELECT DISTINCT ja.JerarquiaAprobacionId, ja.AprobadorUsuarioId
        FROM Operacion.JerarquiaAprobacion ja
        INNER JOIN SolicitanteEstructura se
            ON se.EstructuraOrganizacionalId = ja.EstructuraOrganizacionalId
        WHERE
            ja.EstadoRegistroId = 1
            AND (ja.VigenciaDesde IS NULL OR ja.VigenciaDesde <= @FechaRef)
            AND (ja.VigenciaHasta IS NULL OR ja.VigenciaHasta >= @FechaRef)
    ),
    DelegacionesActivas AS (
        SELECT DISTINCT
            da.DelegacionAprobacionId,
            da.DelegadoUsuarioId AS AprobadorUsuarioId,
            da.DeleganteUsuarioId
        FROM Operacion.DelegacionAprobacion da
        WHERE
            da.EstadoRegistroId = 1
            AND (da.VigenciaDesde IS NULL OR da.VigenciaDesde <= @FechaRef)
            AND (da.VigenciaHasta IS NULL OR da.VigenciaHasta >= @FechaRef)
            AND EXISTS (SELECT 1 FROM JerarquiasActivas ja WHERE ja.AprobadorUsuarioId = da.DeleganteUsuarioId)
    )
    SELECT ja.AprobadorUsuarioId, CAST('Jerarquia' AS varchar(20)) AS Origen, NULL AS DeleganteUsuarioId
    FROM JerarquiasActivas ja
    UNION ALL
    SELECT da.AprobadorUsuarioId, CAST('Delegacion' AS varchar(20)) AS Origen, da.DeleganteUsuarioId
    FROM DelegacionesActivas da
);
