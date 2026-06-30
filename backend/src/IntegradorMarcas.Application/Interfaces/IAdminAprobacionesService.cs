using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IAdminAprobacionesService
{
    Task<IReadOnlyList<AdminJerarquiaDto>> ListJerarquiasAsync(UserContextInfo user, int? aprobadorUsuarioId, int? estadoRegistroId, CancellationToken cancellationToken);
    Task<AdminJerarquiaDto> CreateJerarquiaAsync(UserContextInfo user, CreateJerarquiaDto request, string? correlationId, CancellationToken cancellationToken);
    Task<AdminJerarquiaDto> UpdateJerarquiaAsync(UserContextInfo user, int jerarquiaAprobacionId, UpdateJerarquiaDto request, string? correlationId, CancellationToken cancellationToken);
    Task ToggleJerarquiaEstadoAsync(UserContextInfo user, int jerarquiaAprobacionId, ToggleEstadoRegistroDto request, string? correlationId, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminDelegacionDto>> ListDelegacionesAsync(UserContextInfo user, int? deleganteUsuarioId, int? delegadoUsuarioId, int? estadoRegistroId, DateTime? vigenteEnFecha, CancellationToken cancellationToken);
    Task<AdminDelegacionDto> CreateDelegacionAsync(UserContextInfo user, CreateDelegacionDto request, string? correlationId, CancellationToken cancellationToken);
    Task<AdminDelegacionDto> UpdateDelegacionAsync(UserContextInfo user, int delegacionAprobacionId, UpdateDelegacionDto request, string? correlationId, CancellationToken cancellationToken);
    Task ToggleDelegacionEstadoAsync(UserContextInfo user, int delegacionAprobacionId, ToggleEstadoRegistroDto request, string? correlationId, CancellationToken cancellationToken);

    // F-004: borrado físico con auditoría (R19, R20, D1)
    Task DeleteDelegacionAsync(UserContextInfo user, int delegacionAprobacionId, string? correlationId, CancellationToken cancellationToken);
}
