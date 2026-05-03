namespace IntegradorMarcas.Infrastructure.Queries;

public static class AdminOrganizacionSql
{
    public const string ListDependencias = @"
SELECT
    eo.EstructuraOrganizacionalId,
    eo.Nombre,
    eo.CodigoOrigen,
    eo.EstructuraPadreId,
    eo.EstadoRegistroId,
    eo.VigenciaDesde,
    eo.VigenciaHasta
FROM RecursosHumanos.EstructuraOrganizacional eo
WHERE
    (@EstadoRegistroID IS NULL OR eo.EstadoRegistroId = @EstadoRegistroID)
    AND (
        @Search IS NULL
        OR eo.Nombre LIKE @SearchLike
        OR eo.CodigoOrigen LIKE @SearchLike
    )
ORDER BY eo.Nombre;";

    public const string GetDependenciaById = @"
SELECT
    EstructuraOrganizacionalId,
    Nombre,
    CodigoOrigen,
    EstructuraPadreId,
    EstadoRegistroId,
    VigenciaDesde,
    VigenciaHasta
FROM RecursosHumanos.EstructuraOrganizacional
WHERE EstructuraOrganizacionalId = @EstructuraOrganizacionalID;";

    public const string UpdateDependencia = @"
UPDATE RecursosHumanos.EstructuraOrganizacional
SET
    Nombre = @Nombre,
    EstructuraPadreId = @EstructuraPadreID,
    EstadoRegistroId = @EstadoRegistroID
WHERE EstructuraOrganizacionalId = @EstructuraOrganizacionalID;";

    public const string ExistsDependencia = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1
    FROM RecursosHumanos.EstructuraOrganizacional
    WHERE EstructuraOrganizacionalId = @EstructuraOrganizacionalID
) THEN 1 ELSE 0 END AS bit);";

    public const string WouldCreateDependenciaCycle = @"
;WITH Ancestros AS (
    SELECT EstructuraOrganizacionalId, EstructuraPadreId
    FROM RecursosHumanos.EstructuraOrganizacional
    WHERE EstructuraOrganizacionalId = @EstructuraPadreID

    UNION ALL

    SELECT e.EstructuraOrganizacionalId, e.EstructuraPadreId
    FROM RecursosHumanos.EstructuraOrganizacional e
    INNER JOIN Ancestros a
        ON e.EstructuraOrganizacionalId = a.EstructuraPadreId
)
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1
    FROM Ancestros
    WHERE EstructuraOrganizacionalId = @EstructuraOrganizacionalID
) THEN 1 ELSE 0 END AS bit);";

    public const string ListUsuarios = @"
SELECT
    u.UsuarioId,
    u.Cedula,
    u.NombreCompleto,
    u.CorreoElectronico,
    u.JefaturaId,
    j.NombreCompleto AS JefaturaNombre,
    u.UnidadId,
    u.RolId,
    r.Descripcion AS RolDescripcion,
    u.EsActivo
FROM RecursosHumanos.Usuario u
INNER JOIN Configuracion.Rol r
    ON r.RolId = u.RolId
LEFT JOIN RecursosHumanos.Usuario j
    ON j.UsuarioId = u.JefaturaId
WHERE
    (@RolID IS NULL OR u.RolId = @RolID)
    AND (@UnidadID IS NULL OR u.UnidadId = @UnidadID)
    AND (@JefaturaID IS NULL OR u.JefaturaId = @JefaturaID)
    AND (@EsActivo IS NULL OR u.EsActivo = @EsActivo)
    AND (
        @Search IS NULL
        OR u.NombreCompleto LIKE @SearchLike
        OR u.Cedula LIKE @SearchLike
        OR u.CorreoElectronico LIKE @SearchLike
    )
ORDER BY u.NombreCompleto;";

    public const string GetUsuarioById = @"
SELECT
    u.UsuarioId,
    u.Cedula,
    u.NombreCompleto,
    u.CorreoElectronico,
    u.JefaturaId,
    j.NombreCompleto AS JefaturaNombre,
    u.UnidadId,
    u.RolId,
    r.Descripcion AS RolDescripcion,
    u.EsActivo
FROM RecursosHumanos.Usuario u
INNER JOIN Configuracion.Rol r
    ON r.RolId = u.RolId
LEFT JOIN RecursosHumanos.Usuario j
    ON j.UsuarioId = u.JefaturaId
WHERE u.UsuarioId = @UsuarioID;";

    public const string UpdateUsuarioAsignacion = @"
UPDATE RecursosHumanos.Usuario
SET
    RolId = COALESCE(@RolID, RolId),
    UnidadId = COALESCE(@UnidadID, UnidadId),
    JefaturaId = @JefaturaID,
    ModificadoPor = @ModificadoPor,
    FechaHoraModificacion = SYSUTCDATETIME()
WHERE UsuarioId = @UsuarioID;";

    public const string UpdateUsuarioEstado = @"
UPDATE RecursosHumanos.Usuario
SET
    EsActivo = @EsActivo,
    ModificadoPor = @ModificadoPor,
    FechaHoraModificacion = SYSUTCDATETIME()
WHERE UsuarioId = @UsuarioID;";

    public const string ExistsUsuario = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1 FROM RecursosHumanos.Usuario WHERE UsuarioId = @UsuarioID
) THEN 1 ELSE 0 END AS bit);";

    public const string ExistsRol = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1 FROM Configuracion.Rol WHERE RolId = @RolID
) THEN 1 ELSE 0 END AS bit);";

    public const string ExistsUnidad = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1 FROM RecursosHumanos.EstructuraOrganizacional WHERE EstructuraOrganizacionalId = @UnidadID
) THEN 1 ELSE 0 END AS bit);";

    public const string ExistsJefaturaValida = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1
    FROM RecursosHumanos.Usuario
    WHERE UsuarioId = @JefaturaID
      AND EsActivo = 1
      AND RolId IN (2, 4)
      AND UsuarioId <> @UsuarioID
) THEN 1 ELSE 0 END AS bit);";

    public const string ExistsDirectJefaturaCycle = @"
SELECT CAST(CASE WHEN EXISTS (
    SELECT 1
    FROM RecursosHumanos.Usuario
    WHERE UsuarioId = @JefaturaID
      AND JefaturaId = @UsuarioID
) THEN 1 ELSE 0 END AS bit);";
}
