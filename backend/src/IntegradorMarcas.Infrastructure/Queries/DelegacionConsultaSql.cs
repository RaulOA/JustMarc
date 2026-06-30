namespace IntegradorMarcas.Infrastructure.Queries;

/// <summary>
/// F-004: SQL para las vistas de consulta del delegado (R11/R12 mi-funcion; R16/R17 mi-registro).
/// D3 = A: repositorio dedicado, no se infla IAdminAprobacionesRepository.
/// </summary>
public static class DelegacionConsultaSql
{
    // R11/R12: funcion activa y vigente del delegado — titular, vigencia, alcance de estructuras
    public const string MiFuncion = """
SELECT
    da.DelegacionAprobacionId,
    da.DeleganteUsuarioId AS TitularUsuarioId,
    ut.NombreCompleto AS TitularNombre,
    da.VigenciaDesde,
    da.VigenciaHasta,
    da.Motivo,
    da.JerarquiaAprobacionId,
    -- R12: alcance de estructuras: la de la jerarquia referenciada, o todas las del titular si NULL
    ISNULL(
        eo_jerarquia.Nombre,
        (
            SELECT STRING_AGG(eo2.Nombre, ', ')
            FROM Operacion.JerarquiaAprobacion ja2
            INNER JOIN RecursosHumanos.EstructuraOrganizacional eo2
                ON eo2.EstructuraOrganizacionalId = ja2.EstructuraOrganizacionalId
            WHERE ja2.AprobadorUsuarioId = da.DeleganteUsuarioId
              AND ja2.EstadoRegistroId = 1
        )
    ) AS AlcanceEstructuras
FROM Operacion.DelegacionAprobacion da
INNER JOIN RecursosHumanos.Usuario ut ON ut.UsuarioID = da.DeleganteUsuarioId
LEFT JOIN Operacion.JerarquiaAprobacion ja ON ja.JerarquiaAprobacionId = da.JerarquiaAprobacionId
LEFT JOIN RecursosHumanos.EstructuraOrganizacional eo_jerarquia
    ON eo_jerarquia.EstructuraOrganizacionalId = ja.EstructuraOrganizacionalId
WHERE da.DelegadoUsuarioId = @DelegadoUsuarioID
  AND da.EstadoRegistroId = 1
  AND da.VigenciaDesde <= @FechaRef
  AND (da.VigenciaHasta IS NULL OR da.VigenciaHasta >= @FechaRef)
ORDER BY da.VigenciaDesde DESC;
""";

    // R16/R17: registro de solo lectura — justificaciones resueltas por el delegado
    // dentro del periodo de la delegacion (D4: filtro por FechaAprobacion dentro de [VigenciaDesde, VigenciaHasta])
    public const string MiRegistro = """
SELECT DISTINCT
    je.JustificacionId,
    je.MotivoGeneral,
    je.ComentarioResolucion,
    je.EstadoJustificacionId AS EstadoId,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    je.FechaAprobacion,
    je.UsuarioID AS SolicitanteUsuarioId,
    us.NombreCompleto AS SolicitanteNombre,
    da.DelegacionAprobacionId,
    da.DeleganteUsuarioId AS TitularUsuarioId,
    ut.NombreCompleto AS TitularNombre
FROM Operacion.Justificacion je
INNER JOIN Configuracion.EstadoJustificacion e ON e.EstadoJustificacionId = je.EstadoJustificacionId
INNER JOIN RecursosHumanos.Usuario us ON us.UsuarioID = je.UsuarioID
-- unir con la delegacion que dio alcance al delegado al momento de resolver
INNER JOIN Operacion.DelegacionAprobacion da
    ON da.DelegadoUsuarioId = @DelegadoUsuarioID
   AND da.EstadoRegistroId = 1
   AND je.FechaAprobacion >= da.VigenciaDesde
   AND (da.VigenciaHasta IS NULL OR je.FechaAprobacion <= da.VigenciaHasta)
INNER JOIN RecursosHumanos.Usuario ut ON ut.UsuarioID = da.DeleganteUsuarioId
WHERE je.AprobadorId = @DelegadoUsuarioID
  AND je.EstadoJustificacionId <> 1  -- excluir pendientes
  AND (@Desde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@Desde AS DATE))
  AND (@Hasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@Hasta AS DATE))
ORDER BY je.FechaAprobacion DESC;
""";
}
