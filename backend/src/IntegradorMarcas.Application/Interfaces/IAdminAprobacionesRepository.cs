using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IAdminAprobacionesRepository
{
    Task<IReadOnlyList<AdminJerarquiaDto>> ListJerarquiasAsync(int? aprobadorUsuarioId, int? estadoRegistroId, CancellationToken cancellationToken);
    Task<AdminJerarquiaDto?> GetJerarquiaByIdAsync(int jerarquiaAprobacionId, CancellationToken cancellationToken);
    Task<AdminJerarquiaDto> CreateJerarquiaAsync(CreateJerarquiaDto request, int actorUsuarioId, CancellationToken cancellationToken);
    Task<int> UpdateJerarquiaAsync(int jerarquiaAprobacionId, UpdateJerarquiaDto request, int actorUsuarioId, CancellationToken cancellationToken);
    Task<int> ToggleJerarquiaEstadoAsync(int jerarquiaAprobacionId, int estadoRegistroId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminDelegacionDto>> ListDelegacionesAsync(int? deleganteUsuarioId, int? delegadoUsuarioId, int? estadoRegistroId, DateTime? vigenteEnFecha, CancellationToken cancellationToken);
    Task<AdminDelegacionDto?> GetDelegacionByIdAsync(int delegacionAprobacionId, CancellationToken cancellationToken);
    Task<AdminDelegacionDto> CreateDelegacionAsync(CreateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken);
    Task<int> UpdateDelegacionAsync(int delegacionAprobacionId, UpdateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken);
    Task<int> ToggleDelegacionEstadoAsync(int delegacionAprobacionId, int estadoRegistroId, CancellationToken cancellationToken);

    Task<bool> ExistsUsuarioAsync(int usuarioId, CancellationToken cancellationToken);
    Task<bool> ExistsEstructuraAsync(int estructuraOrganizacionalId, CancellationToken cancellationToken);
    Task<bool> ExistsJerarquiaAsync(int jerarquiaAprobacionId, CancellationToken cancellationToken);
    Task<bool> ExistsJerarquiaActivaDuplicadaAsync(int aprobadorUsuarioId, int estructuraOrganizacionalId, int nivelAprobacion, int? jerarquiaAprobacionIdExcluida, CancellationToken cancellationToken);
}
