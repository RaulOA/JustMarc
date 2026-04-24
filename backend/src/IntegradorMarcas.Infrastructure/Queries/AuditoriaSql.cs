namespace IntegradorMarcas.Infrastructure.Queries;

public static class AuditoriaSql
{
    public const string InsertEvento = @"
INSERT INTO Auditoria.EventoAuditoria
(
    UsuarioID,
    NombreUsuario,
    RolCodigo,
    TipoEventoAuditoriaId,
    DescripcionEvento,
    ResultadoAuditoriaId,
    ReferenciaFuncional,
    PayloadResumen
)
VALUES
(
    @UsuarioID,
    @NombreUsuario,
    @RolCodigo,
    @TipoEventoAuditoriaID,
    @DescripcionEvento,
    @ResultadoAuditoriaID,
    @ReferenciaFuncional,
    @PayloadResumen
);";
}
