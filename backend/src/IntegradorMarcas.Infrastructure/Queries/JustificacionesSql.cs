namespace IntegradorMarcas.Infrastructure.Queries;

public static class JustificacionesSql
{
    public const string InsertEncabezado = @"
INSERT INTO Operacion.Justificacion
(
    UsuarioID,
    MotivoGeneral,
    EstadoJustificacionId,
    Usr_Registro
)
VALUES
(
    @UsuarioID,
    @MotivoGeneral,
    @EstadoID,
    @UsrRegistro
);
SELECT CAST(SCOPE_IDENTITY() AS INT)";

    public const string InsertDetalle = @"
INSERT INTO Operacion.JustificacionDetalle
(
    JustificacionId,
    TipoJustificacionId,
    FechaMarca,
    ObservacionDetalle,
    Usr_Registro
)
VALUES
(
    @JustificacionID,
    @TipoJustificacionID,
    @FechaMarca,
    @ObservacionDetalle,
    @UsrRegistro
);";

    public const string ListMine = @"
SELECT
    je.JustificacionId,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoJustificacionId,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    COUNT(jd.JustificacionDetalleId) AS CantidadDetalles,
    je.AprobadorId,
    je.FechaAprobacion
FROM Operacion.Justificacion je
INNER JOIN Configuracion.EstadoJustificacion e ON e.EstadoJustificacionId = je.EstadoJustificacionId
LEFT JOIN Operacion.JustificacionDetalle jd ON jd.JustificacionId = je.JustificacionId
WHERE
    je.UsuarioID = @UsuarioID
    AND (@EstadoID IS NULL OR je.EstadoJustificacionId = @EstadoID)
    AND (@Desde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@Desde AS DATE))
    AND (@Hasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@Hasta AS DATE))
GROUP BY
    je.JustificacionId,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoJustificacionId,
    e.Descripcion,
    je.FechaCreacion,
    je.AprobadorId,
    je.FechaAprobacion
ORDER BY je.FechaCreacion DESC;";

    public const string ListPendientesJefatura = @"
SELECT
    je.JustificacionId,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoJustificacionId,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    COUNT(jd.JustificacionDetalleId) AS CantidadDetalles,
    je.AprobadorId,
    je.FechaAprobacion
FROM Operacion.Justificacion je
INNER JOIN Configuracion.EstadoJustificacion e ON e.EstadoJustificacionId = je.EstadoJustificacionId
LEFT JOIN Operacion.JustificacionDetalle jd ON jd.JustificacionId = je.JustificacionId
WHERE
    je.EstadoJustificacionId = @EstadoPendiente
    AND EXISTS (
        SELECT 1
        FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
        WHERE fa.AprobadorUsuarioId = @AprobadorUsuarioID
    )
    AND (@Desde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@Desde AS DATE))
    AND (@Hasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@Hasta AS DATE))
GROUP BY
    je.JustificacionId,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoJustificacionId,
    e.Descripcion,
    je.FechaCreacion,
    je.AprobadorId,
    je.FechaAprobacion
ORDER BY je.FechaCreacion ASC;";

    public const string ListRrhhGlobal = @"
SELECT
    je.JustificacionId,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoJustificacionId,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    COUNT(jd.JustificacionDetalleId) AS CantidadDetalles,
    je.AprobadorId,
    je.FechaAprobacion,
    u.UsuarioID AS FuncionarioID,
    u.NombreCompleto AS FuncionarioNombre,
    u.Cedula AS FuncionarioCedula,
    u.Compania,
    u.JefaturaId,
    j.NombreCompleto AS JefaturaNombre,
    MIN(tj.Descripcion) AS TipoPrincipal
FROM Operacion.Justificacion je
INNER JOIN Configuracion.EstadoJustificacion e ON e.EstadoJustificacionId = je.EstadoJustificacionId
INNER JOIN RecursosHumanos.Usuario u ON u.UsuarioID = je.UsuarioID
LEFT JOIN RecursosHumanos.Usuario j ON j.UsuarioID = u.JefaturaId
LEFT JOIN Operacion.JustificacionDetalle jd ON jd.JustificacionId = je.JustificacionId
LEFT JOIN Configuracion.TipoJustificacion tj ON tj.TipoJustificacionId = jd.TipoJustificacionId
WHERE
    (@EstadoID IS NULL OR je.EstadoJustificacionId = @EstadoID)
    AND (@Compania IS NULL OR u.Compania = @Compania)
    AND (@FechaDesde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@FechaDesde AS DATE))
    AND (@FechaHasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@FechaHasta AS DATE))
    AND (
        @Funcionario IS NULL
        OR u.NombreCompleto LIKE CONCAT('%', @Funcionario, '%')
        OR u.Cedula LIKE CONCAT('%', @Funcionario, '%')
    )
GROUP BY
    je.JustificacionId,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoJustificacionId,
    e.Descripcion,
    je.FechaCreacion,
    je.AprobadorId,
    je.FechaAprobacion,
    u.UsuarioID,
    u.NombreCompleto,
    u.Cedula,
    u.Compania,
    u.JefaturaId,
    j.NombreCompleto
ORDER BY je.FechaCreacion DESC, je.JustificacionId DESC;";

    public const string GetDetalleJefaturaEncabezado = @"
SELECT
    je.JustificacionId,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoJustificacionId,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    je.AprobadorId,
    je.FechaAprobacion,
    u.UsuarioID AS SolicitanteUsuarioID,
    u.NombreCompleto AS SolicitanteNombreCompleto,
    u.Cedula AS SolicitanteCedula,
    u.CorreoElectronico AS SolicitanteCorreo,
    u.Compania AS SolicitanteCompania,
    u.UnidadId AS SolicitanteUnidadID,
    u.JefaturaId AS SolicitanteJefaturaID,
    eo.Nombre AS SolicitanteUnidadNombre,
    ua.UsuarioID AS AprobadorUsuarioID,
    ua.NombreCompleto AS AprobadorNombreCompleto,
    ua.Cedula AS AprobadorCedula,
    ua.CorreoElectronico AS AprobadorCorreo,
    ua.Compania AS AprobadorCompania,
    ua.UnidadId AS AprobadorUnidadID,
    ua.JefaturaId AS AprobadorJefaturaID
FROM Operacion.Justificacion je
INNER JOIN Configuracion.EstadoJustificacion e ON e.EstadoJustificacionId = je.EstadoJustificacionId
INNER JOIN RecursosHumanos.Usuario u ON u.UsuarioID = je.UsuarioID
LEFT JOIN RecursosHumanos.Usuario ua ON ua.UsuarioID = je.AprobadorId
LEFT JOIN dbo.Estructuras_Organizacionales eo ON eo.EstructuraOrganizacionalID = u.UnidadID
WHERE
    je.JustificacionId = @JustificacionID
    AND EXISTS (
        SELECT 1
        FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
        WHERE fa.AprobadorUsuarioId = @AprobadorUsuarioID
    );";

    public const string GetDetalleJefaturaLineas = @"
SELECT
    jd.JustificacionDetalleId,
    jd.TipoJustificacionId,
    tj.Descripcion AS TipoJustificacionDescripcion,
    jd.FechaMarca,
    jd.ObservacionDetalle
FROM Operacion.JustificacionDetalle jd
INNER JOIN Configuracion.TipoJustificacion tj ON tj.TipoJustificacionId = jd.TipoJustificacionId
WHERE jd.JustificacionId = @JustificacionID
ORDER BY jd.FechaMarca DESC, jd.JustificacionDetalleId DESC;";

    public const string GetResolverValidation = @"
SELECT
    CASE WHEN je.JustificacionId IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS [Exists],
    ISNULL(je.EstadoJustificacionId, 0) AS EstadoId,
    CASE WHEN scopeData.ScopeSource IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsInApprovalScope,
    scopeData.ScopeSource,
    scopeData.DeleganteUsuarioId
FROM (SELECT @JustificacionID AS JustificacionId) seed
LEFT JOIN Operacion.Justificacion je ON je.JustificacionId = seed.JustificacionId
OUTER APPLY (
    SELECT TOP 1
        fa.Origen AS ScopeSource,
        fa.DeleganteUsuarioId
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
    WHERE fa.AprobadorUsuarioId = @AprobadorUsuarioID
    ORDER BY CASE WHEN fa.Origen = 'Delegacion' THEN 0 ELSE 1 END
) scopeData;";

    public const string GetAprobacionScopeValidation = @"
SELECT
    CASE WHEN je.JustificacionId IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS [Exists],
    ISNULL(je.EstadoJustificacionId, 0) AS EstadoId,
    CASE WHEN scopeData.ScopeSource IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsInApprovalScope,
    scopeData.ScopeSource,
    scopeData.DeleganteUsuarioId
FROM (SELECT @JustificacionID AS JustificacionId) seed
LEFT JOIN Operacion.Justificacion je ON je.JustificacionId = seed.JustificacionId
OUTER APPLY (
    SELECT TOP 1
        fa.Origen AS ScopeSource,
        fa.DeleganteUsuarioId
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
    WHERE fa.AprobadorUsuarioId = @AprobadorUsuarioID
    ORDER BY CASE WHEN fa.Origen = 'Delegacion' THEN 0 ELSE 1 END
) scopeData;";

    public const string GetExistingTipoJustificacionIds = @"
SELECT DISTINCT
    TipoJustificacionId
FROM Configuracion.TipoJustificacion
WHERE TipoJustificacionId IN @Ids;";

    public const string ResolverPendiente = @"
UPDATE je
SET
    je.EstadoJustificacionId = @EstadoID,
    je.AprobadorId = @AprobadorUsuarioID,
    je.FechaAprobacion = GETDATE(),
    je.ComentarioResolucion = @Comentario,
    je.RolResolucion = @RolResolucion
FROM Operacion.Justificacion je
WHERE
    je.JustificacionId = @JustificacionID
    AND je.EstadoJustificacionId = @EstadoPendiente
    AND EXISTS (
        SELECT 1
        FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
        WHERE fa.AprobadorUsuarioId = @AprobadorUsuarioID
    );";
}
