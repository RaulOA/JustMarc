using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IJustificacionRepository
{
    Task<IReadOnlyCollection<int>> GetExistingTipoJustificacionIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken);
    Task<int> CreateAsync(int usuarioId, CreateJustificacionDto request, CancellationToken cancellationToken);
    Task<CurrentApproverDto> GetCurrentApproverAsync(int solicitanteUsuarioId, CancellationToken cancellationToken);
    Task<IReadOnlyList<JustificacionResumenDto>> ListMineAsync(int usuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<JustificacionDetalleLineaDto>> ListMineLineasAsync(int usuarioId, int justificacionId, CancellationToken cancellationToken);
    Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(int aprobadorUsuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(FiltroRrhhJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListHistoricoAsync(
        int? usuarioId,
        int? aprobadorUsuarioId,
        bool excluirPropiosEnScopeAprobador,
        FiltroRrhhJustificacionesDto filtros,
        CancellationToken cancellationToken);
    Task<JustificacionCompletaDto?> GetDetalleJefaturaAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken);
    Task<AprobacionScopeValidationDto> GetAprobacionScopeValidationAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken);
    Task<ResolverValidationDto> GetResolverValidationAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken);
    Task<int> ResolverAsync(int justificacionId, int aprobadorUsuarioId, int estadoId, string? comentario, string? rolResolucion, CancellationToken cancellationToken);

    // F-004 T13 R15: re-resolucion del titular sobre lo resuelto por delegado (D2 = B)
    Task<RevisarTitularValidationDto> GetRevisarTitularValidationAsync(int justificacionId, int titularUsuarioId, CancellationToken cancellationToken);
    Task<int> RevisarTitularAsync(int justificacionId, int titularUsuarioId, int estadoId, string? comentario, string? rolResolucion, CancellationToken cancellationToken);
}
