using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Domain.Constants;
using System.Text.Json;

namespace IntegradorMarcas.Application.Services;

public sealed class AdminOrganizacionService : IAdminOrganizacionService
{
    private readonly IAdminOrganizacionRepository _repository;
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly IAdminActionAuditRepository _adminActionAuditRepository;

    public AdminOrganizacionService(
        IAdminOrganizacionRepository repository,
        IAuditEventRepository auditEventRepository,
        IAdminActionAuditRepository adminActionAuditRepository)
    {
        _repository = repository;
        _auditEventRepository = auditEventRepository;
        _adminActionAuditRepository = adminActionAuditRepository;
    }

    public async Task<IReadOnlyList<AdminDependenciaDto>> ListDependenciasAsync(UserContextInfo user, int? estadoRegistroId, string? search, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        return await _repository.ListDependenciasAsync(estadoRegistroId, search, cancellationToken);
    }

    public async Task<AdminDependenciaDto> UpdateDependenciaAsync(UserContextInfo user, int estructuraOrganizacionalId, UpdateDependenciaDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateDependencia(estructuraOrganizacionalId, request);

        if (!await _repository.ExistsDependenciaAsync(estructuraOrganizacionalId, cancellationToken))
        {
            throw new AppException("No existe la dependencia indicada.", 404);
        }

        if (request.EstructuraPadreId.HasValue && !await _repository.ExistsDependenciaAsync(request.EstructuraPadreId.Value, cancellationToken))
        {
            throw new AppException("La dependencia padre indicada no existe.", 400);
        }

        if (await _repository.WouldCreateDependenciaCycleAsync(estructuraOrganizacionalId, request.EstructuraPadreId, cancellationToken))
        {
            throw new AppException("La dependencia padre genera un ciclo jerarquico no permitido.", 400);
        }

        var previous = await _repository.GetDependenciaByIdAsync(estructuraOrganizacionalId, cancellationToken)
            ?? throw new AppException("No existe la dependencia indicada.", 404);

        var affected = await _repository.UpdateDependenciaAsync(estructuraOrganizacionalId, request, user.UserId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No se pudo actualizar la dependencia indicada.", 404);
        }

        var updated = await _repository.GetDependenciaByIdAsync(estructuraOrganizacionalId, cancellationToken)
            ?? throw new AppException("No se pudo recuperar la dependencia actualizada.", 500);

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 10,
            DescripcionEvento = "Actualizacion de dependencia organizacional.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Dependencia:{estructuraOrganizacionalId}",
            PayloadResumen = $"Nombre={updated.Nombre};Padre={updated.EstructuraPadreId};Estado={updated.EstadoRegistroId}"
        }, cancellationToken);

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "EstructuraOrganizacional",
            entidadObjetivoId: estructuraOrganizacionalId.ToString(),
            accion: "Update",
            descripcion: "Actualizacion de dependencia organizacional.",
            valoresAnteriores: previous,
            valoresNuevos: updated,
            cancellationToken: cancellationToken);

        return updated;
    }

    public async Task<IReadOnlyList<AdminUsuarioAsignacionDto>> ListUsuariosAsync(UserContextInfo user, int? rolId, int? unidadId, int? jefaturaId, bool? esActivo, string? search, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        return await _repository.ListUsuariosAsync(rolId, unidadId, jefaturaId, esActivo, search, cancellationToken);
    }

    public async Task<AdminUsuarioAsignacionDto> UpdateUsuarioAsignacionAsync(UserContextInfo user, int usuarioId, UpdateUsuarioAsignacionDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);

        if (!await _repository.ExistsUsuarioAsync(usuarioId, cancellationToken))
        {
            throw new AppException("No existe el usuario indicado.", 404);
        }

        if (request.RolId.HasValue && !await _repository.ExistsRolAsync(request.RolId.Value, cancellationToken))
        {
            throw new AppException("El rol indicado no existe.", 400);
        }

        if (request.UnidadId.HasValue && !await _repository.ExistsUnidadAsync(request.UnidadId.Value, cancellationToken))
        {
            throw new AppException("La unidad indicada no existe.", 400);
        }

        if (request.JefaturaId.HasValue)
        {
            if (request.JefaturaId.Value == usuarioId)
            {
                throw new AppException("Un usuario no puede ser su propia jefatura.", 400);
            }

            if (!await _repository.ExistsJefaturaValidaAsync(usuarioId, request.JefaturaId.Value, cancellationToken))
            {
                throw new AppException("La jefatura indicada no es valida o no esta activa.", 400);
            }

            if (await _repository.ExistsDirectJefaturaCycleAsync(usuarioId, request.JefaturaId.Value, cancellationToken))
            {
                throw new AppException("La asignacion de jefatura genera un ciclo directo no permitido.", 400);
            }
        }

        var previous = await _repository.GetUsuarioAsignacionByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("No existe el usuario indicado.", 404);

        var affected = await _repository.UpdateUsuarioAsignacionAsync(usuarioId, request, user.UserId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No se pudo actualizar la asignacion del usuario.", 404);
        }

        var updated = await _repository.GetUsuarioAsignacionByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("No se pudo recuperar el usuario actualizado.", 500);

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 10,
            DescripcionEvento = "Actualizacion de asignacion de usuario.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Usuario:{usuarioId}",
            PayloadResumen = $"Rol={updated.RolId};Unidad={updated.UnidadId};Jefatura={updated.JefaturaId}"
        }, cancellationToken);

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "Usuario",
            entidadObjetivoId: usuarioId.ToString(),
            accion: "Reassign",
            descripcion: "Actualizacion de asignacion de usuario.",
            valoresAnteriores: previous,
            valoresNuevos: updated,
            cancellationToken: cancellationToken);

        return updated;
    }

    public async Task<AdminUsuarioAsignacionDto> UpdateUsuarioEstadoAsync(UserContextInfo user, int usuarioId, UpdateUsuarioEstadoDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);

        var previous = await _repository.GetUsuarioAsignacionByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("No existe el usuario indicado.", 404);

        var affected = await _repository.UpdateUsuarioEstadoAsync(usuarioId, request.EsActivo, user.UserId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No se pudo actualizar el estado del usuario.", 404);
        }

        var updated = await _repository.GetUsuarioAsignacionByIdAsync(usuarioId, cancellationToken)
            ?? throw new AppException("No se pudo recuperar el usuario actualizado.", 500);

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 11,
            DescripcionEvento = "Cambio de estado de usuario.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Usuario:{usuarioId}",
            PayloadResumen = $"EsActivo={updated.EsActivo}"
        }, cancellationToken);

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "Usuario",
            entidadObjetivoId: usuarioId.ToString(),
            accion: request.EsActivo ? "Activate" : "Deactivate",
            descripcion: "Cambio de estado de usuario.",
            valoresAnteriores: previous,
            valoresNuevos: updated,
            cancellationToken: cancellationToken);

        return updated;
    }

    private static void EnsureAdmin(UserContextInfo user)
    {
        if (!RolesSistema.EsAdmin(user.Role))
        {
            throw new AppException("Solo ROL_ADMIN puede gestionar organizacion y asignaciones.", 403);
        }
    }

    private static void ValidateDependencia(int estructuraOrganizacionalId, UpdateDependenciaDto request)
    {
        if (estructuraOrganizacionalId <= 0)
        {
            throw new AppException("EstructuraOrganizacionalId es requerido.", 400);
        }

        if (string.IsNullOrWhiteSpace(request.Nombre))
        {
            throw new AppException("Nombre de dependencia es requerido.", 400);
        }

        if (request.Nombre.Trim().Length > 150)
        {
            throw new AppException("Nombre de dependencia no puede exceder 150 caracteres.", 400);
        }

        if (request.EstructuraPadreId.HasValue && request.EstructuraPadreId.Value == estructuraOrganizacionalId)
        {
            throw new AppException("Una dependencia no puede ser su propia dependencia padre.", 400);
        }

        if (request.EstadoRegistroId is not 1 and not 2)
        {
            throw new AppException("EstadoRegistroId debe ser 1 (Activo) o 2 (Inactivo).", 400);
        }
    }

    private Task LogDetailedAuditAsync(
        UserContextInfo user,
        string? correlationId,
        string entidadObjetivo,
        string entidadObjetivoId,
        string accion,
        string descripcion,
        object? valoresAnteriores,
        object? valoresNuevos,
        CancellationToken cancellationToken)
    {
        return _adminActionAuditRepository.LogActionAsync(new AdminActionAuditEntry
        {
            CorrelationId = correlationId,
            UsuarioActorId = user.UserId,
            RolActorCodigo = user.Role,
            EntidadObjetivo = entidadObjetivo,
            EntidadObjetivoId = entidadObjetivoId,
            Accion = accion,
            ResultadoAuditoriaId = 1,
            Descripcion = descripcion,
            ValoresAnteriores = valoresAnteriores is null ? null : JsonSerializer.Serialize(valoresAnteriores),
            ValoresNuevos = valoresNuevos is null ? null : JsonSerializer.Serialize(valoresNuevos),
            Metadata = JsonSerializer.Serialize(new
            {
                scope = "admin.organizacion"
            })
        }, cancellationToken);
    }
}
