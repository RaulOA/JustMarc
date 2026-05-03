namespace IntegradorMarcas.Infrastructure.Queries;

public static class AdminActionAuditSql
{
    public const string InsertAction = @"
INSERT INTO Auditoria.AdminAccionAuditoria
(
    FechaEventoUtc,
    CorrelationId,
    UsuarioActorId,
    RolActorCodigo,
    EntidadObjetivo,
    EntidadObjetivoId,
    Accion,
    ResultadoAuditoriaId,
    Descripcion,
    ValoresAnteriores,
    ValoresNuevos,
    Metadata
)
VALUES
(
    @FechaEventoUtc,
    @CorrelationId,
    @UsuarioActorId,
    @RolActorCodigo,
    @EntidadObjetivo,
    @EntidadObjetivoId,
    @Accion,
    @ResultadoAuditoriaId,
    @Descripcion,
    @ValoresAnteriores,
    @ValoresNuevos,
    @Metadata
);";
}
