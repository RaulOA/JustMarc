using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IJustificacionService
{
    Task<int> CreateAsync(UserContextInfo user, CreateJustificacionDto request, CancellationToken cancellationToken);
    Task<CurrentApproverDto> GetCurrentApproverAsync(UserContextInfo user, CancellationToken cancellationToken);
    Task<IReadOnlyList<JustificacionResumenDto>> ListMineAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(UserContextInfo user, FiltroRrhhJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListHistoricoAsync(UserContextInfo user, FiltroRrhhJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<JustificacionDetalleLineaDto>> ListMineLineasAsync(UserContextInfo user, int justificacionId, CancellationToken cancellationToken);
    Task<JustificacionCompletaDto> GetDetalleJefaturaAsync(UserContextInfo user, int justificacionId, CancellationToken cancellationToken);
    Task ResolverAsync(UserContextInfo user, int justificacionId, ResolverJustificacionDto request, CancellationToken cancellationToken);

    // F-004 T13 R15: re-resolucion del titular sobre lo resuelto por su delegado (D2 = B)
    Task RevisarTitularAsync(UserContextInfo user, int justificacionId, ResolverJustificacionDto request, CancellationToken cancellationToken);
}
