using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IAdminAprobacionesService
{
    Task<IReadOnlyList<AdminJerarquiaDto>> ListJerarquiasAsync(UserContextInfo user, int? aprobadorUsuarioId, int? estadoRegistroId, CancellationToken cancellationToken);
    Task<AdminJerarquiaDto> CreateJerarquiaAsync(UserContextInfo user, CreateJerarquiaDto request, CancellationToken cancellationToken);
    Task ToggleJerarquiaEstadoAsync(UserContextInfo user, int jerarquiaAprobacionId, ToggleEstadoRegistroDto request, CancellationToken cancellationToken);
    Task<IReadOnlyList<AdminDelegacionDto>> ListDelegacionesAsync(UserContextInfo user, int? deleganteUsuarioId, int? delegadoUsuarioId, int? estadoRegistroId, DateTime? vigenteEnFecha, CancellationToken cancellationToken);
    Task<AdminDelegacionDto> CreateDelegacionAsync(UserContextInfo user, CreateDelegacionDto request, CancellationToken cancellationToken);
    Task ToggleDelegacionEstadoAsync(UserContextInfo user, int delegacionAprobacionId, ToggleEstadoRegistroDto request, CancellationToken cancellationToken);
}
