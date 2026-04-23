using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IAdminAprobacionesRepository
{
    Task<IReadOnlyList<AdminJerarquiaDto>> ListJerarquiasAsync(int? aprobadorUsuarioId, int? estadoRegistroId, CancellationToken cancellationToken);
    Task<AdminJerarquiaDto> CreateJerarquiaAsync(CreateJerarquiaDto request, int actorUsuarioId, CancellationToken cancellationToken);
    Task<int> ToggleJerarquiaEstadoAsync(int jerarquiaAprobacionId, int estadoRegistroId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminDelegacionDto>> ListDelegacionesAsync(int? deleganteUsuarioId, int? delegadoUsuarioId, int? estadoRegistroId, DateTime? vigenteEnFecha, CancellationToken cancellationToken);
    Task<AdminDelegacionDto> CreateDelegacionAsync(CreateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken);
    Task<int> ToggleDelegacionEstadoAsync(int delegacionAprobacionId, int estadoRegistroId, CancellationToken cancellationToken);
}
