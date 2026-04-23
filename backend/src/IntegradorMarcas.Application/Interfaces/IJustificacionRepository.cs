using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IJustificacionRepository
{
    Task<int> CreateAsync(int usuarioId, CreateJustificacionDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<JustificacionResumenDto>> ListMineAsync(int usuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(int jefaturaId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(FiltroRrhhJustificacionesDto filtros, CancellationToken cancellationToken);
    Task<JustificacionCompletaDto?> GetDetalleJefaturaAsync(int justificacionId, int jefaturaId, CancellationToken cancellationToken);
    Task<ResolverValidationDto> GetResolverValidationAsync(int justificacionId, int jefaturaId, CancellationToken cancellationToken);
    Task<int> ResolverAsync(int justificacionId, int jefaturaId, int estadoId, string? comentario, CancellationToken cancellationToken);
}
