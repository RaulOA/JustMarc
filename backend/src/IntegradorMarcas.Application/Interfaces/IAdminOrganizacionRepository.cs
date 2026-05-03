using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IAdminOrganizacionRepository
{
    Task<IReadOnlyList<AdminDependenciaDto>> ListDependenciasAsync(int? estadoRegistroId, string? search, CancellationToken cancellationToken);
    Task<AdminDependenciaDto?> GetDependenciaByIdAsync(int estructuraOrganizacionalId, CancellationToken cancellationToken);
    Task<int> UpdateDependenciaAsync(int estructuraOrganizacionalId, UpdateDependenciaDto request, int actorUsuarioId, CancellationToken cancellationToken);
    Task<bool> ExistsDependenciaAsync(int estructuraOrganizacionalId, CancellationToken cancellationToken);
    Task<bool> WouldCreateDependenciaCycleAsync(int estructuraOrganizacionalId, int? estructuraPadreId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminUsuarioAsignacionDto>> ListUsuariosAsync(int? rolId, int? unidadId, int? jefaturaId, bool? esActivo, string? search, CancellationToken cancellationToken);
    Task<AdminUsuarioAsignacionDto?> GetUsuarioAsignacionByIdAsync(int usuarioId, CancellationToken cancellationToken);
    Task<int> UpdateUsuarioAsignacionAsync(int usuarioId, UpdateUsuarioAsignacionDto request, int actorUsuarioId, CancellationToken cancellationToken);
    Task<int> UpdateUsuarioEstadoAsync(int usuarioId, bool esActivo, int actorUsuarioId, CancellationToken cancellationToken);

    Task<bool> ExistsUsuarioAsync(int usuarioId, CancellationToken cancellationToken);
    Task<bool> ExistsRolAsync(int rolId, CancellationToken cancellationToken);
    Task<bool> ExistsUnidadAsync(int unidadId, CancellationToken cancellationToken);
    Task<bool> ExistsJefaturaValidaAsync(int usuarioId, int jefaturaId, CancellationToken cancellationToken);
    Task<bool> ExistsDirectJefaturaCycleAsync(int usuarioId, int jefaturaId, CancellationToken cancellationToken);
}
