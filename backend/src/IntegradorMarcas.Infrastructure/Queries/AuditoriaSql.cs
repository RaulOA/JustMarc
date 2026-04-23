namespace IntegradorMarcas.Infrastructure.Queries;

public static class AuditoriaSql
{
    public const string InsertEvento = @"
INSERT INTO dbo.Auditoria_Eventos
(
    UsuarioID,
    NombreUsuario,
    RolCodigo,
    TipoEventoAuditoriaID,
    DescripcionEvento,
    ResultadoAuditoriaID,
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
