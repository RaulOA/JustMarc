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
    je.EstadoID,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    COUNT(jd.DetalleID) AS CantidadDetalles,
    je.AprobadorID,
    je.FechaAprobacion
FROM dbo.Justificaciones_Encabezado je
INNER JOIN dbo.Estados e ON e.EstadoID = je.EstadoID
INNER JOIN dbo.Usuarios u ON u.UsuarioID = je.UsuarioID
LEFT JOIN dbo.Justificaciones_Detalle jd ON jd.JustificacionID = je.JustificacionID
WHERE
    je.EstadoID = @EstadoPendiente
    AND u.JefaturaID = @JefaturaID
    AND (@Desde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@Desde AS DATE))
    AND (@Hasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@Hasta AS DATE))
GROUP BY
    je.JustificacionID,
    je.MotivoGeneral,
    je.EstadoID,
    e.Descripcion,
    je.FechaCreacion,
    je.AprobadorID,
    je.FechaAprobacion
ORDER BY je.FechaCreacion ASC;";

    public const string GetResolverValidation = @"
SELECT
    CASE WHEN je.JustificacionID IS NULL THEN CAST(0 AS bit) ELSE CAST(1 AS bit) END AS [Exists],
    ISNULL(je.EstadoID, 0) AS EstadoId,
    CASE WHEN u.JefaturaID = @JefaturaID THEN CAST(1 AS bit) ELSE CAST(0 AS bit) END AS IsSubordinado
FROM (SELECT @JustificacionID AS JustificacionID) seed
LEFT JOIN dbo.Justificaciones_Encabezado je ON je.JustificacionID = seed.JustificacionID
LEFT JOIN dbo.Usuarios u ON u.UsuarioID = je.UsuarioID;";

    public const string ResolverPendiente = @"
UPDATE je
SET
    je.EstadoID = @EstadoID,
    je.AprobadorID = @JefaturaID,
    je.FechaAprobacion = GETDATE()
FROM dbo.Justificaciones_Encabezado je
INNER JOIN dbo.Usuarios u ON u.UsuarioID = je.UsuarioID
WHERE
    je.JustificacionID = @JustificacionID
    AND je.EstadoID = @EstadoPendiente
    AND u.JefaturaID = @JefaturaID;";
}
