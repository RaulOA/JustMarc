using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

public interface IAdminOrganizacionService
{
    Task<IReadOnlyList<AdminDependenciaDto>> ListDependenciasAsync(UserContextInfo user, int? estadoRegistroId, string? search, CancellationToken cancellationToken);
    Task<AdminDependenciaDto> UpdateDependenciaAsync(UserContextInfo user, int estructuraOrganizacionalId, UpdateDependenciaDto request, string? correlationId, CancellationToken cancellationToken);

    Task<IReadOnlyList<AdminUsuarioAsignacionDto>> ListUsuariosAsync(UserContextInfo user, int? rolId, int? unidadId, int? jefaturaId, bool? esActivo, string? search, CancellationToken cancellationToken);
    Task<AdminUsuarioAsignacionDto> UpdateUsuarioAsignacionAsync(UserContextInfo user, int usuarioId, UpdateUsuarioAsignacionDto request, string? correlationId, CancellationToken cancellationToken);
    Task<AdminUsuarioAsignacionDto> UpdateUsuarioEstadoAsync(UserContextInfo user, int usuarioId, UpdateUsuarioEstadoDto request, string? correlationId, CancellationToken cancellationToken);
}
