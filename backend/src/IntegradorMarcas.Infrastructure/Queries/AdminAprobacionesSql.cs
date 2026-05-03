namespace IntegradorMarcas.Infrastructure.Queries;

public static class AdminAprobacionesSql
{
    public const string ListJerarquias = @"
SELECT
    JerarquiaAprobacionId,
    AprobadorUsuarioId,
    EstructuraOrganizacionalId,
    NivelAprobacion,
    TipoRelacion,
    EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM Operacion.JerarquiaAprobacion
WHERE
    (@AprobadorUsuarioID IS NULL OR AprobadorUsuarioId = @AprobadorUsuarioID)
    AND (@EstadoRegistroID IS NULL OR EstadoRegistroId = @EstadoRegistroID)
ORDER BY JerarquiaAprobacionId DESC;";

    public const string CreateJerarquia = @"
INSERT INTO Operacion.JerarquiaAprobacion
(
    AprobadorUsuarioId,
    EstructuraOrganizacionalId,
    NivelAprobacion,
    TipoRelacion,
    EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta,
    CreadoPor
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
    @CreadoPor
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    public const string UpdateJerarquia = @"
UPDATE Operacion.JerarquiaAprobacion
SET
    AprobadorUsuarioId = @AprobadorUsuarioID,
    EstructuraOrganizacionalId = @EstructuraOrganizacionalID,
    NivelAprobacion = @NivelAprobacion,
    TipoRelacion = @TipoRelacion,
    EstadoRegistroId = @EstadoRegistroID,
    VigenciaDesde = @VigenciaDesde,
    VigenciaHasta = @VigenciaHasta
WHERE JerarquiaAprobacionId = @JerarquiaAprobacionID;";

    public const string GetJerarquiaById = @"
SELECT
    JerarquiaAprobacionId,
    AprobadorUsuarioId,
    EstructuraOrganizacionalId,
    NivelAprobacion,
    TipoRelacion,
    EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM Operacion.JerarquiaAprobacion
WHERE JerarquiaAprobacionId = @JerarquiaAprobacionID;";

    public const string ToggleJerarquiaEstado = @"
UPDATE Operacion.JerarquiaAprobacion
SET EstadoRegistroId = @EstadoRegistroID
WHERE JerarquiaAprobacionId = @JerarquiaAprobacionID;";

    public const string ListDelegaciones = @"
SELECT
    DelegacionAprobacionId,
    DeleganteUsuarioId,
    DelegadoUsuarioId,
    JerarquiaAprobacionId,
    Motivo,
    EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM Operacion.DelegacionAprobacion
WHERE
    (@DeleganteUsuarioID IS NULL OR DeleganteUsuarioId = @DeleganteUsuarioID)
    AND (@DelegadoUsuarioID IS NULL OR DelegadoUsuarioId = @DelegadoUsuarioID)
    AND (@EstadoRegistroID IS NULL OR EstadoRegistroId = @EstadoRegistroID)
    AND (
        @VigenteEnFecha IS NULL
        OR (
            VigenciaDesde <= @VigenteEnFecha
            AND (VigenciaHasta IS NULL OR VigenciaHasta >= @VigenteEnFecha)
        )
    )
ORDER BY DelegacionAprobacionId DESC;";

    public const string CreateDelegacion = @"
INSERT INTO Operacion.DelegacionAprobacion
(
    DeleganteUsuarioId,
    DelegadoUsuarioId,
    JerarquiaAprobacionId,
    Motivo,
    EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta,
    CreadoPor
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
    @CreadoPor
);
SELECT CAST(SCOPE_IDENTITY() AS INT);";

    public const string UpdateDelegacion = @"
UPDATE Operacion.DelegacionAprobacion
SET
    DeleganteUsuarioId = @DeleganteUsuarioID,
    DelegadoUsuarioId = @DelegadoUsuarioID,
    JerarquiaAprobacionId = @JerarquiaAprobacionID,
    Motivo = @Motivo,
    EstadoRegistroId = @EstadoRegistroID,
    VigenciaDesde = @VigenciaDesde,
    VigenciaHasta = @VigenciaHasta
WHERE DelegacionAprobacionId = @DelegacionAprobacionID;";

    public const string GetDelegacionById = @"
SELECT
    DelegacionAprobacionId,
    DeleganteUsuarioId,
    DelegadoUsuarioId,
    JerarquiaAprobacionId,
    Motivo,
    EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM Operacion.DelegacionAprobacion
WHERE DelegacionAprobacionId = @DelegacionAprobacionID;";

    public const string ToggleDelegacionEstado = @"
UPDATE Operacion.DelegacionAprobacion
SET EstadoRegistroId = @EstadoRegistroID
WHERE DelegacionAprobacionId = @DelegacionAprobacionID;";

    public const string ExistsUsuario = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1
    FROM RecursosHumanos.Usuario
    WHERE UsuarioId = @UsuarioID
) THEN 1 ELSE 0 END AS bit);";

    public const string ExistsEstructura = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1
    FROM RecursosHumanos.EstructuraOrganizacional
    WHERE EstructuraOrganizacionalId = @EstructuraOrganizacionalID
) THEN 1 ELSE 0 END AS bit);";

    public const string ExistsJerarquia = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1
    FROM Operacion.JerarquiaAprobacion
    WHERE JerarquiaAprobacionId = @JerarquiaAprobacionID
) THEN 1 ELSE 0 END AS bit);";
}
