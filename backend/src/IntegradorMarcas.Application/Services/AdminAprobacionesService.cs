using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Domain.Constants;
using System.Text.Json;

namespace IntegradorMarcas.Application.Services;

public sealed class AdminAprobacionesService : IAdminAprobacionesService
{
    private readonly IAdminAprobacionesRepository _repository;
    private readonly IAuditEventRepository _auditEventRepository;
    private readonly IAdminActionAuditRepository _adminActionAuditRepository;

    public AdminAprobacionesService(
        IAdminAprobacionesRepository repository,
        IAuditEventRepository auditEventRepository,
        IAdminActionAuditRepository adminActionAuditRepository)
    {
        _repository = repository;
        _auditEventRepository = auditEventRepository;
        _adminActionAuditRepository = adminActionAuditRepository;
    }

    public async Task<IReadOnlyList<AdminJerarquiaDto>> ListJerarquiasAsync(UserContextInfo user, int? aprobadorUsuarioId, int? estadoRegistroId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        return await _repository.ListJerarquiasAsync(aprobadorUsuarioId, estadoRegistroId, cancellationToken);
    }

    public async Task<AdminJerarquiaDto> CreateJerarquiaAsync(UserContextInfo user, CreateJerarquiaDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateCreateJerarquia(request);
        await EnsureReferencesForJerarquiaAsync(request.AprobadorUsuarioId, request.EstructuraOrganizacionalId, cancellationToken);
        await EnsureJerarquiaNoDuplicadaAsync(request.AprobadorUsuarioId, request.EstructuraOrganizacionalId, request.NivelAprobacion, null, cancellationToken);

        var created = await _repository.CreateJerarquiaAsync(request, user.UserId, cancellationToken);

        await LogSummaryEventAsync(new AuditEventEntry
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

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "JerarquiaAprobacion",
            entidadObjetivoId: created.JerarquiaAprobacionId.ToString(),
            accion: "Create",
            descripcion: "Alta de jerarquia de aprobacion.",
            valoresAnteriores: null,
            valoresNuevos: created,
            cancellationToken: cancellationToken);

        return created;
    }

    public async Task<AdminJerarquiaDto> UpdateJerarquiaAsync(UserContextInfo user, int jerarquiaAprobacionId, UpdateJerarquiaDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateUpdateJerarquia(request);
        await EnsureReferencesForJerarquiaAsync(request.AprobadorUsuarioId, request.EstructuraOrganizacionalId, cancellationToken);
        await EnsureJerarquiaNoDuplicadaAsync(request.AprobadorUsuarioId, request.EstructuraOrganizacionalId, request.NivelAprobacion, jerarquiaAprobacionId, cancellationToken);

        var previous = await _repository.GetJerarquiaByIdAsync(jerarquiaAprobacionId, cancellationToken);
        if (previous is null)
        {
            throw new AppException("No existe la jerarquia indicada.", 404);
        }

        var affected = await _repository.UpdateJerarquiaAsync(jerarquiaAprobacionId, request, user.UserId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No se pudo actualizar la jerarquia indicada.", 404);
        }

        var updated = await _repository.GetJerarquiaByIdAsync(jerarquiaAprobacionId, cancellationToken)
            ?? throw new AppException("No se pudo recuperar la jerarquia actualizada.", 500);

        await LogSummaryEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 8,
            DescripcionEvento = "Actualizacion de jerarquia de aprobacion.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Jerarquia:{jerarquiaAprobacionId}",
            PayloadResumen = $"Aprobador={updated.AprobadorUsuarioId};Estructura={updated.EstructuraOrganizacionalId};Nivel={updated.NivelAprobacion};Estado={updated.EstadoRegistroId}"
        }, cancellationToken);

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "JerarquiaAprobacion",
            entidadObjetivoId: jerarquiaAprobacionId.ToString(),
            accion: "Update",
            descripcion: "Actualizacion de jerarquia de aprobacion.",
            valoresAnteriores: previous,
            valoresNuevos: updated,
            cancellationToken: cancellationToken);

        return updated;
    }

    public async Task ToggleJerarquiaEstadoAsync(UserContextInfo user, int jerarquiaAprobacionId, ToggleEstadoRegistroDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateEstadoRegistro(request.EstadoRegistroId);

        var previous = await _repository.GetJerarquiaByIdAsync(jerarquiaAprobacionId, cancellationToken);
        if (previous is null)
        {
            throw new AppException("No existe la jerarquia indicada.", 404);
        }

        var affected = await _repository.ToggleJerarquiaEstadoAsync(jerarquiaAprobacionId, request.EstadoRegistroId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No existe la jerarquia indicada.", 404);
        }

        var updated = await _repository.GetJerarquiaByIdAsync(jerarquiaAprobacionId, cancellationToken);

        await LogSummaryEventAsync(new AuditEventEntry
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

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "JerarquiaAprobacion",
            entidadObjetivoId: jerarquiaAprobacionId.ToString(),
            accion: "ChangeState",
            descripcion: "Cambio de estado de jerarquia de aprobacion.",
            valoresAnteriores: previous,
            valoresNuevos: updated,
            cancellationToken: cancellationToken);
    }

    public async Task<IReadOnlyList<AdminDelegacionDto>> ListDelegacionesAsync(UserContextInfo user, int? deleganteUsuarioId, int? delegadoUsuarioId, int? estadoRegistroId, DateTime? vigenteEnFecha, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        return await _repository.ListDelegacionesAsync(deleganteUsuarioId, delegadoUsuarioId, estadoRegistroId, vigenteEnFecha, cancellationToken);
    }

    public async Task<AdminDelegacionDto> CreateDelegacionAsync(UserContextInfo user, CreateDelegacionDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateCreateDelegacion(request);
        await EnsureReferencesForDelegacionAsync(request.DeleganteUsuarioId, request.DelegadoUsuarioId, request.JerarquiaAprobacionId, cancellationToken);

        var created = await _repository.CreateDelegacionAsync(request, user.UserId, cancellationToken);

        await LogSummaryEventAsync(new AuditEventEntry
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

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "DelegacionAprobacion",
            entidadObjetivoId: created.DelegacionAprobacionId.ToString(),
            accion: "Create",
            descripcion: "Alta de delegacion de aprobacion.",
            valoresAnteriores: null,
            valoresNuevos: created,
            cancellationToken: cancellationToken);

        return created;
    }

    public async Task<AdminDelegacionDto> UpdateDelegacionAsync(UserContextInfo user, int delegacionAprobacionId, UpdateDelegacionDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateUpdateDelegacion(request);
        await EnsureReferencesForDelegacionAsync(request.DeleganteUsuarioId, request.DelegadoUsuarioId, request.JerarquiaAprobacionId, cancellationToken, delegacionAprobacionId);

        var previous = await _repository.GetDelegacionByIdAsync(delegacionAprobacionId, cancellationToken);
        if (previous is null)
        {
            throw new AppException("No existe la delegacion indicada.", 404);
        }

        var affected = await _repository.UpdateDelegacionAsync(delegacionAprobacionId, request, user.UserId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No se pudo actualizar la delegacion indicada.", 404);
        }

        var updated = await _repository.GetDelegacionByIdAsync(delegacionAprobacionId, cancellationToken)
            ?? throw new AppException("No se pudo recuperar la delegacion actualizada.", 500);

        await LogSummaryEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 9,
            DescripcionEvento = "Actualizacion de delegacion de aprobacion.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Delegacion:{delegacionAprobacionId}",
            PayloadResumen = $"Delegante={updated.DeleganteUsuarioId};Delegado={updated.DelegadoUsuarioId};Estado={updated.EstadoRegistroId}"
        }, cancellationToken);

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "DelegacionAprobacion",
            entidadObjetivoId: delegacionAprobacionId.ToString(),
            accion: "Update",
            descripcion: "Actualizacion de delegacion de aprobacion.",
            valoresAnteriores: previous,
            valoresNuevos: updated,
            cancellationToken: cancellationToken);

        return updated;
    }

    public async Task ToggleDelegacionEstadoAsync(UserContextInfo user, int delegacionAprobacionId, ToggleEstadoRegistroDto request, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);
        ValidateEstadoRegistro(request.EstadoRegistroId);

        var previous = await _repository.GetDelegacionByIdAsync(delegacionAprobacionId, cancellationToken);
        if (previous is null)
        {
            throw new AppException("No existe la delegacion indicada.", 404);
        }

        var affected = await _repository.ToggleDelegacionEstadoAsync(delegacionAprobacionId, request.EstadoRegistroId, user.UserId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No existe la delegacion indicada.", 404);
        }

        var updated = await _repository.GetDelegacionByIdAsync(delegacionAprobacionId, cancellationToken);

        await LogSummaryEventAsync(new AuditEventEntry
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

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "DelegacionAprobacion",
            entidadObjetivoId: delegacionAprobacionId.ToString(),
            accion: "ChangeState",
            descripcion: "Cambio de estado de delegacion de aprobacion.",
            valoresAnteriores: previous,
            valoresNuevos: updated,
            cancellationToken: cancellationToken);
    }

    // F-004 T14 R19/R20: borrado fisico con auditoria previa (D1 = A)
    public async Task DeleteDelegacionAsync(UserContextInfo user, int delegacionAprobacionId, string? correlationId, CancellationToken cancellationToken)
    {
        EnsureAdmin(user);

        var previous = await _repository.GetDelegacionByIdAsync(delegacionAprobacionId, cancellationToken);
        if (previous is null)
        {
            throw new AppException("No existe la delegacion indicada.", 404);
        }

        // R20: auditoria previa al borrado (serializar valores anteriores antes del DELETE)
        await LogSummaryEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 10,
            DescripcionEvento = "Borrado de delegacion de aprobacion.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Delegacion:{delegacionAprobacionId}",
            PayloadResumen = $"Delegante={previous.DeleganteUsuarioId};Delegado={previous.DelegadoUsuarioId};Estado={previous.EstadoRegistroId}"
        }, cancellationToken);

        await LogDetailedAuditAsync(
            user,
            correlationId,
            entidadObjetivo: "DelegacionAprobacion",
            entidadObjetivoId: delegacionAprobacionId.ToString(),
            accion: "Delete",
            descripcion: "Borrado fisico de delegacion de aprobacion.",
            valoresAnteriores: previous,
            valoresNuevos: null,
            cancellationToken: cancellationToken);

        var affected = await _repository.DeleteDelegacionAsync(delegacionAprobacionId, cancellationToken);
        if (affected == 0)
        {
            throw new AppException("No se pudo borrar la delegacion indicada.", 404);
        }
    }

    private async Task EnsureJerarquiaNoDuplicadaAsync(int aprobadorUsuarioId, int estructuraOrganizacionalId, int nivelAprobacion, int? jerarquiaAprobacionIdExcluida, CancellationToken cancellationToken)
    {
        if (await _repository.ExistsJerarquiaActivaDuplicadaAsync(aprobadorUsuarioId, estructuraOrganizacionalId, nivelAprobacion, jerarquiaAprobacionIdExcluida, cancellationToken))
        {
            throw new AppException("Ya existe una jerarquia activa para esa combinacion de aprobador, estructura y nivel.", 409);
        }
    }

    private async Task EnsureReferencesForJerarquiaAsync(int aprobadorUsuarioId, int estructuraOrganizacionalId, CancellationToken cancellationToken)
    {
        if (!await _repository.ExistsUsuarioAsync(aprobadorUsuarioId, cancellationToken))
        {
            throw new AppException("El aprobador indicado no existe.", 400);
        }

        if (!await _repository.ExistsEstructuraAsync(estructuraOrganizacionalId, cancellationToken))
        {
            throw new AppException("La estructura organizacional indicada no existe.", 400);
        }
    }

    private async Task EnsureReferencesForDelegacionAsync(int deleganteUsuarioId, int delegadoUsuarioId, int? jerarquiaAprobacionId, CancellationToken cancellationToken, int? delegacionIdExcluida = null)
    {
        if (deleganteUsuarioId == delegadoUsuarioId)
        {
            throw new AppException("No se permite auto-delegacion.", 400);
        }

        if (!await _repository.ExistsUsuarioAsync(deleganteUsuarioId, cancellationToken))
        {
            throw new AppException("El delegante indicado no existe.", 400);
        }

        if (!await _repository.ExistsUsuarioAsync(delegadoUsuarioId, cancellationToken))
        {
            throw new AppException("El delegado indicado no existe.", 400);
        }

        if (jerarquiaAprobacionId.HasValue && !await _repository.ExistsJerarquiaAsync(jerarquiaAprobacionId.Value, cancellationToken))
        {
            throw new AppException("La jerarquia indicada no existe.", 400);
        }

        // F-004 R6: prohibicion de sub-delegacion — el delegante propuesto no puede ser a su vez delegado activo y vigente
        var fechaRef = DateTime.Today;
        if (await _repository.ExistsDelegacionActivaComoDelegadoAsync(deleganteUsuarioId, fechaRef, delegacionIdExcluida, cancellationToken))
        {
            throw new AppException("El delegante propuesto es a su vez delegado activo en otra delegacion. No se permite sub-delegacion.", 409);
        }
    }

    private Task LogSummaryEventAsync(AuditEventEntry entry, CancellationToken cancellationToken)
    {
        return _auditEventRepository.LogEventAsync(entry, cancellationToken);
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
                scope = "admin.aprobaciones"
            })
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

        if (request.DeleganteUsuarioId == request.DelegadoUsuarioId)
        {
            throw new AppException("No se permite auto-delegacion.", 400);
        }

        // R1: VigenciaDesde es obligatoria
        if (request.VigenciaDesde == default)
        {
            throw new AppException("VigenciaDesde es requerida.", 400);
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

    private static void ValidateUpdateJerarquia(UpdateJerarquiaDto request)
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

        ValidateEstadoRegistro(request.EstadoRegistroId);

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

    private static void ValidateUpdateDelegacion(UpdateDelegacionDto request)
    {
        if (request.DeleganteUsuarioId <= 0)
        {
            throw new AppException("DeleganteUsuarioId es requerido.", 400);
        }

        if (request.DelegadoUsuarioId <= 0)
        {
            throw new AppException("DelegadoUsuarioId es requerido.", 400);
        }

        if (request.DeleganteUsuarioId == request.DelegadoUsuarioId)
        {
            throw new AppException("No se permite auto-delegacion.", 400);
        }

        ValidateEstadoRegistro(request.EstadoRegistroId);

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
