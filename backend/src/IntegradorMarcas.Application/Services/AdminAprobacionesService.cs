using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Domain.Constants;

namespace IntegradorMarcas.Application.Services;

public sealed class AdminAprobacionesService : IAdminAprobacionesService
{
    private readonly IAdminAprobacionesRepository _repository;
    private readonly IAuditEventRepository _auditEventRepository;

    public AdminAprobacionesService(IAdminAprobacionesRepository repository, IAuditEventRepository auditEventRepository)
    {
        _repository = repository;
        _auditEventRepository = auditEventRepository;
    }

    public async Task<IReadOnlyList<AdminJerarquiaDto>> ListJerarquiasAsync(UserContextInfo user, int? aprobadorUsuarioId, int? estadoRegistroId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        return await _repository.ListJerarquiasAsync(aprobadorUsuarioId, estadoRegistroId, cancellationToken);
    }

    public async Task<AdminJerarquiaDto> CreateJerarquiaAsync(UserContextInfo user, CreateJerarquiaDto request, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateCreateJerarquia(request);

        var created = await _repository.CreateJerarquiaAsync(request, user.UserId, cancellationToken);

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 4,
            DescripcionEvento = "Alta de jerarquia de aprobacion.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Jerarquia:{created.JerarquiaAprobacionId}",
            PayloadResumen = $"Aprobador={created.AprobadorUsuarioId};Estructura={created.EstructuraOrganizacionalId};Nivel={created.NivelAprobacion};Estado={created.EstadoRegistroId}"
        }, cancellationToken);

        return created;
    }

    public async Task ToggleJerarquiaEstadoAsync(UserContextInfo user, int jerarquiaAprobacionId, ToggleEstadoRegistroDto request, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateEstadoRegistro(request.EstadoRegistroId);

        var affected = await _repository.ToggleJerarquiaEstadoAsync(jerarquiaAprobacionId, request.EstadoRegistroId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No existe la jerarquia indicada.", 404);
        }

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 5,
            DescripcionEvento = "Cambio de estado de jerarquia de aprobacion.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Jerarquia:{jerarquiaAprobacionId}",
            PayloadResumen = $"EstadoRegistroID={request.EstadoRegistroId}"
        }, cancellationToken);
    }

    public async Task<IReadOnlyList<AdminDelegacionDto>> ListDelegacionesAsync(UserContextInfo user, int? deleganteUsuarioId, int? delegadoUsuarioId, int? estadoRegistroId, DateTime? vigenteEnFecha, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        return await _repository.ListDelegacionesAsync(deleganteUsuarioId, delegadoUsuarioId, estadoRegistroId, vigenteEnFecha, cancellationToken);
    }

    public async Task<AdminDelegacionDto> CreateDelegacionAsync(UserContextInfo user, CreateDelegacionDto request, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateCreateDelegacion(request);

        var created = await _repository.CreateDelegacionAsync(request, user.UserId, cancellationToken);

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 6,
            DescripcionEvento = "Alta de delegacion de aprobacion.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Delegacion:{created.DelegacionAprobacionId}",
            PayloadResumen = $"Delegante={created.DeleganteUsuarioId};Delegado={created.DelegadoUsuarioId};Estado={created.EstadoRegistroId}"
        }, cancellationToken);

        return created;
    }

    public async Task ToggleDelegacionEstadoAsync(UserContextInfo user, int delegacionAprobacionId, ToggleEstadoRegistroDto request, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateEstadoRegistro(request.EstadoRegistroId);

        var affected = await _repository.ToggleDelegacionEstadoAsync(delegacionAprobacionId, request.EstadoRegistroId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No existe la delegacion indicada.", 404);
        }

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 7,
            DescripcionEvento = "Cambio de estado de delegacion de aprobacion.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Delegacion:{delegacionAprobacionId}",
            PayloadResumen = $"EstadoRegistroID={request.EstadoRegistroId}"
        }, cancellationToken);
    }

    private static void EnsureAdmin(UserContextInfo user)
    {
        if (!RolesSistema.EsAdmin(user.Role))
        {
            throw new AppException("Solo ROL_ADMIN puede gestionar jerarquias y delegaciones.", 403);
        }
    }

    private static void ValidateCreateJerarquia(CreateJerarquiaDto request)
    {
        if (request.AprobadorUsuarioId <= 0)
        {
            throw new AppException("AprobadorUsuarioId es requerido.", 400);
        }

        if (request.EstructuraOrganizacionalId <= 0)
        {
            throw new AppException("EstructuraOrganizacionalId es requerido.", 400);
        }

        if (request.NivelAprobacion <= 0)
        {
            throw new AppException("NivelAprobacion debe ser mayor a cero.", 400);
        }

        var tipoRelacion = request.TipoRelacion.Trim();
        if (!string.Equals(tipoRelacion, "Vertical", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(tipoRelacion, "Horizontal", StringComparison.OrdinalIgnoreCase))
        {
            throw new AppException("TipoRelacion debe ser Vertical u Horizontal.", 400);
        }

        if (request.VigenciaHasta.HasValue && request.VigenciaHasta.Value < request.VigenciaDesde)
        {
            throw new AppException("VigenciaHasta no puede ser menor a VigenciaDesde.", 400);
        }
    }

    private static void ValidateCreateDelegacion(CreateDelegacionDto request)
    {
        if (request.DeleganteUsuarioId <= 0)
        {
            throw new AppException("DeleganteUsuarioId es requerido.", 400);
        }

        if (request.DelegadoUsuarioId <= 0)
        {
            throw new AppException("DelegadoUsuarioId es requerido.", 400);
        }

        if (request.VigenciaHasta.HasValue && request.VigenciaHasta.Value < request.VigenciaDesde)
        {
            throw new AppException("VigenciaHasta no puede ser menor a VigenciaDesde.", 400);
        }

        if (!string.IsNullOrWhiteSpace(request.Motivo) && request.Motivo.Trim().Length > 250)
        {
            throw new AppException("Motivo no puede exceder 250 caracteres.", 400);
        }
    }

    private static void ValidateEstadoRegistro(int estadoRegistroId)
    {
        if (estadoRegistroId is not 1 and not 2)
        {
            throw new AppException("EstadoRegistroId debe ser 1 (Activo) o 2 (Inactivo).", 400);
        }
    }
}
