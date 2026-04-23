namespace IntegradorMarcas.Infrastructure.Queries;

public static class AdminAprobacionesSql
{
    public const string ListJerarquias = @"
SELECT
    JerarquiaAprobacionID AS JerarquiaAprobacionId,
    AprobadorUsuarioID AS AprobadorUsuarioId,
    EstructuraOrganizacionalID AS EstructuraOrganizacionalId,
    NivelAprobacion,
    TipoRelacion,
    EstadoRegistroID AS EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM dbo.Jerarquias_Aprobacion
WHERE
    (@AprobadorUsuarioID IS NULL OR AprobadorUsuarioID = @AprobadorUsuarioID)
    AND (@EstadoRegistroID IS NULL OR EstadoRegistroID = @EstadoRegistroID)
ORDER BY JerarquiaAprobacionID DESC;";

    public const string CreateJerarquia = @"
INSERT INTO dbo.Jerarquias_Aprobacion
(
    AprobadorUsuarioID,
    EstructuraOrganizacionalID,
    NivelAprobacion,
    TipoRelacion,
    EstadoRegistroID,
    VigenciaDesde,
    VigenciaHasta,
    Usr_Registro
)
VALUES
(
    @AprobadorUsuarioID,
    @EstructuraOrganizacionalID,
    @NivelAprobacion,
    @TipoRelacion,
    1,
    @VigenciaDesde,
    @VigenciaHasta,
    @UsrRegistro
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    public const string GetJerarquiaById = @"
SELECT
    JerarquiaAprobacionID AS JerarquiaAprobacionId,
    AprobadorUsuarioID AS AprobadorUsuarioId,
    EstructuraOrganizacionalID AS EstructuraOrganizacionalId,
    NivelAprobacion,
    TipoRelacion,
    EstadoRegistroID AS EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM dbo.Jerarquias_Aprobacion
WHERE JerarquiaAprobacionID = @JerarquiaAprobacionID;";

    public const string ToggleJerarquiaEstado = @"
UPDATE dbo.Jerarquias_Aprobacion
SET EstadoRegistroID = @EstadoRegistroID
WHERE JerarquiaAprobacionID = @JerarquiaAprobacionID;";

    public const string ListDelegaciones = @"
SELECT
    DelegacionAprobacionID AS DelegacionAprobacionId,
    DeleganteUsuarioID AS DeleganteUsuarioId,
    DelegadoUsuarioID AS DelegadoUsuarioId,
    JerarquiaAprobacionID AS JerarquiaAprobacionId,
    Motivo,
    EstadoRegistroID AS EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM dbo.Delegaciones_Aprobacion
WHERE
    (@DeleganteUsuarioID IS NULL OR DeleganteUsuarioID = @DeleganteUsuarioID)
    AND (@DelegadoUsuarioID IS NULL OR DelegadoUsuarioID = @DelegadoUsuarioID)
    AND (@EstadoRegistroID IS NULL OR EstadoRegistroID = @EstadoRegistroID)
    AND (
        @VigenteEnFecha IS NULL
        OR (
            VigenciaDesde <= @VigenteEnFecha
            AND (VigenciaHasta IS NULL OR VigenciaHasta >= @VigenteEnFecha)
        )
    )
ORDER BY DelegacionAprobacionID DESC;";

    public const string CreateDelegacion = @"
INSERT INTO dbo.Delegaciones_Aprobacion
(
    DeleganteUsuarioID,
    DelegadoUsuarioID,
    JerarquiaAprobacionID,
    Motivo,
    EstadoRegistroID,
    VigenciaDesde,
    VigenciaHasta,
    Usr_Registro
)
VALUES
(
    @DeleganteUsuarioID,
    @DelegadoUsuarioID,
    @JerarquiaAprobacionID,
    @Motivo,
    1,
    @VigenciaDesde,
    @VigenciaHasta,
    @UsrRegistro
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    public const string GetDelegacionById = @"
SELECT
    DelegacionAprobacionID AS DelegacionAprobacionId,
    DeleganteUsuarioID AS DeleganteUsuarioId,
    DelegadoUsuarioID AS DelegadoUsuarioId,
    JerarquiaAprobacionID AS JerarquiaAprobacionId,
    Motivo,
    EstadoRegistroID AS EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM dbo.Delegaciones_Aprobacion
WHERE DelegacionAprobacionID = @DelegacionAprobacionID;";

    public const string ToggleDelegacionEstado = @"
UPDATE dbo.Delegaciones_Aprobacion
SET EstadoRegistroID = @EstadoRegistroID
WHERE DelegacionAprobacionID = @DelegacionAprobacionID;";
}
