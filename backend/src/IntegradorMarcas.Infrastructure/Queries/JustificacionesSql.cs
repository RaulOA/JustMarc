namespace IntegradorMarcas.Infrastructure.Queries;

public static class JustificacionesSql
{
    public const string InsertEncabezado = @"
INSERT INTO dbo.Justificaciones_Encabezado
(
    UsuarioID,
    MotivoGeneral,
    EstadoID,
    Usr_Registro
)
VALUES
(
    @UsuarioID,
    @MotivoGeneral,
    @EstadoID,
    @UsrRegistro
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    public const string InsertDetalle = @"
INSERT INTO dbo.Justificaciones_Detalle
(
    JustificacionID,
    TipoJustificacionID,
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
    je.JustificacionID,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoID,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    COUNT(jd.DetalleID) AS CantidadDetalles,
    je.AprobadorID,
    je.FechaAprobacion
FROM dbo.Justificaciones_Encabezado je
INNER JOIN dbo.Estados e ON e.EstadoID = je.EstadoID
LEFT JOIN dbo.Justificaciones_Detalle jd ON jd.JustificacionID = je.JustificacionID
WHERE
    je.UsuarioID = @UsuarioID
    AND (@EstadoID IS NULL OR je.EstadoID = @EstadoID)
    AND (@Desde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@Desde AS DATE))
    AND (@Hasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@Hasta AS DATE))
GROUP BY
    je.JustificacionID,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoID,
    e.Descripcion,
    je.FechaCreacion,
    je.AprobadorID,
    je.FechaAprobacion
ORDER BY je.FechaCreacion DESC;";

    public const string ListPendientesJefatura = @"
SELECT
    je.JustificacionID,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoID,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    COUNT(jd.DetalleID) AS CantidadDetalles,
    je.AprobadorID,
    je.FechaAprobacion
FROM dbo.Justificaciones_Encabezado je
INNER JOIN dbo.Estados e ON e.EstadoID = je.EstadoID
LEFT JOIN dbo.Justificaciones_Detalle jd ON jd.JustificacionID = je.JustificacionID
WHERE
    je.EstadoID = @EstadoPendiente
    AND EXISTS (
        SELECT 1
        FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
        WHERE fa.AprobadorUsuarioID = @AprobadorUsuarioID
    )
    AND (@Desde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@Desde AS DATE))
    AND (@Hasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@Hasta AS DATE))
GROUP BY
    je.JustificacionID,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoID,
    e.Descripcion,
    je.FechaCreacion,
    je.AprobadorID,
    je.FechaAprobacion
ORDER BY je.FechaCreacion ASC;";

    public const string ListRrhhGlobal = @"
SELECT
    je.JustificacionID,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoID,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    COUNT(jd.DetalleID) AS CantidadDetalles,
    je.AprobadorID,
    je.FechaAprobacion,
    u.UsuarioID AS FuncionarioID,
    u.NombreCompleto AS FuncionarioNombre,
    u.Cedula AS FuncionarioCedula,
    u.Compania,
    u.JefaturaID,
    j.NombreCompleto AS JefaturaNombre,
    MIN(tj.Descripcion) AS TipoPrincipal
FROM dbo.Justificaciones_Encabezado je
INNER JOIN dbo.Estados e ON e.EstadoID = je.EstadoID
INNER JOIN dbo.Usuarios u ON u.UsuarioID = je.UsuarioID
LEFT JOIN dbo.Usuarios j ON j.UsuarioID = u.JefaturaID
LEFT JOIN dbo.Justificaciones_Detalle jd ON jd.JustificacionID = je.JustificacionID
LEFT JOIN dbo.Cat_TiposJustificacion tj ON tj.TipoJustificacionID = jd.TipoJustificacionID
WHERE
    (@EstadoID IS NULL OR je.EstadoID = @EstadoID)
    AND (@Compania IS NULL OR u.Compania = @Compania)
    AND (@FechaDesde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@FechaDesde AS DATE))
    AND (@FechaHasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@FechaHasta AS DATE))
    AND (
        @Funcionario IS NULL
        OR u.NombreCompleto LIKE CONCAT('%', @Funcionario, '%')
        OR u.Cedula LIKE CONCAT('%', @Funcionario, '%')
    )
GROUP BY
    je.JustificacionID,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoID,
    e.Descripcion,
    je.FechaCreacion,
    je.AprobadorID,
    je.FechaAprobacion,
    u.UsuarioID,
    u.NombreCompleto,
    u.Cedula,
    u.Compania,
    u.JefaturaID,
    j.NombreCompleto
ORDER BY je.FechaCreacion DESC, je.JustificacionID DESC;";

    public const string GetDetalleJefaturaEncabezado = @"
SELECT
    je.JustificacionID,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoID,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    je.AprobadorID,
    je.FechaAprobacion,
    u.UsuarioID AS SolicitanteUsuarioID,
    u.NombreCompleto AS SolicitanteNombreCompleto,
    u.Cedula AS SolicitanteCedula,
    u.Correo AS SolicitanteCorreo,
    u.Compania AS SolicitanteCompania,
    u.UnidadID AS SolicitanteUnidadID,
    u.JefaturaID AS SolicitanteJefaturaID,
    ua.UsuarioID AS AprobadorUsuarioID,
    ua.NombreCompleto AS AprobadorNombreCompleto,
    ua.Cedula AS AprobadorCedula,
    ua.Correo AS AprobadorCorreo,
    ua.Compania AS AprobadorCompania,
    ua.UnidadID AS AprobadorUnidadID,
    ua.JefaturaID AS AprobadorJefaturaID
FROM dbo.Justificaciones_Encabezado je
INNER JOIN dbo.Estados e ON e.EstadoID = je.EstadoID
INNER JOIN dbo.Usuarios u ON u.UsuarioID = je.UsuarioID
LEFT JOIN dbo.Usuarios ua ON ua.UsuarioID = je.AprobadorID
WHERE
    je.JustificacionID = @JustificacionID
    AND EXISTS (
        SELECT 1
        FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
        WHERE fa.AprobadorUsuarioID = @AprobadorUsuarioID
    );";

    public const string GetDetalleJefaturaLineas = @"
SELECT
    jd.DetalleID,
    jd.TipoJustificacionID,
    tj.Descripcion AS TipoJustificacionDescripcion,
    jd.FechaMarca,
    jd.ObservacionDetalle
FROM dbo.Justificaciones_Detalle jd
INNER JOIN dbo.Cat_TiposJustificacion tj ON tj.TipoJustificacionID = jd.TipoJustificacionID
WHERE jd.JustificacionID = @JustificacionID
ORDER BY jd.FechaMarca DESC, jd.DetalleID DESC;";

    public const string GetResolverValidation = @"
SELECT
    CASE WHEN je.JustificacionID IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS [Exists],
    ISNULL(je.EstadoID, 0) AS EstadoId,
    CASE WHEN scopeData.ScopeSource IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsInApprovalScope,
    scopeData.ScopeSource,
    scopeData.DeleganteUsuarioId
FROM (SELECT @JustificacionID AS JustificacionID) seed
LEFT JOIN dbo.Justificaciones_Encabezado je ON je.JustificacionID = seed.JustificacionID
OUTER APPLY (
    SELECT TOP 1
        fa.Origen AS ScopeSource,
        fa.DeleganteUsuarioID AS DeleganteUsuarioId
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
    WHERE fa.AprobadorUsuarioID = @AprobadorUsuarioID
    ORDER BY CASE WHEN fa.Origen = 'Delegacion' THEN 0 ELSE 1 END
) scopeData;";

    public const string GetAprobacionScopeValidation = @"
SELECT
    CASE WHEN je.JustificacionID IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS [Exists],
    ISNULL(je.EstadoID, 0) AS EstadoId,
    CASE WHEN scopeData.ScopeSource IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS IsInApprovalScope,
    scopeData.ScopeSource,
    scopeData.DeleganteUsuarioId
FROM (SELECT @JustificacionID AS JustificacionID) seed
LEFT JOIN dbo.Justificaciones_Encabezado je ON je.JustificacionID = seed.JustificacionID
OUTER APPLY (
    SELECT TOP 1
        fa.Origen AS ScopeSource,
        fa.DeleganteUsuarioID AS DeleganteUsuarioId
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
    WHERE fa.AprobadorUsuarioID = @AprobadorUsuarioID
    ORDER BY CASE WHEN fa.Origen = 'Delegacion' THEN 0 ELSE 1 END
) scopeData;";

    public const string GetExistingTipoJustificacionIds = @"
SELECT DISTINCT
    TipoJustificacionID
FROM dbo.Cat_TiposJustificacion
WHERE TipoJustificacionID IN @Ids;";

    public const string ResolverPendiente = @"
UPDATE je
SET
    je.EstadoID = @EstadoID,
    je.AprobadorID = @AprobadorUsuarioID,
    je.FechaAprobacion = GETDATE(),
    je.ComentarioResolucion = @Comentario,
    je.RolResolucion = @RolResolucion
FROM dbo.Justificaciones_Encabezado je
WHERE
    je.JustificacionID = @JustificacionID
    AND je.EstadoID = @EstadoPendiente
    AND EXISTS (
        SELECT 1
        FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa
        WHERE fa.AprobadorUsuarioID = @AprobadorUsuarioID
    );";
}
