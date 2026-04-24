using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Application.Validation;
using IntegradorMarcas.Domain.Constants;

namespace IntegradorMarcas.Application.Services;

public sealed class JustificacionService : IJustificacionService
{
    private readonly IJustificacionRepository _repository;
    private readonly IAuditEventRepository _auditEventRepository;

    public JustificacionService(IJustificacionRepository repository, IAuditEventRepository auditEventRepository)
    {
        _repository = repository;
        _auditEventRepository = auditEventRepository;
    }

    public async Task<int> CreateAsync(UserContextInfo user, CreateJustificacionDto request, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsFuncionario(user.Role) && !RolesSistema.EsJefatura(user.Role))
        {
            throw new AppException("Solo funcionario o jefatura pueden crear boletas.", 403);
        }

        JustificacionValidator.ValidateCreate(request);

        var tipoIds = request.Detalles
            .Select(x => x.TipoJustificacionId)
            .Distinct()
            .ToArray();

        var existingTipoIds = await _repository.GetExistingTipoJustificacionIdsAsync(tipoIds, cancellationToken);
        if (existingTipoIds.Count != tipoIds.Length)
        {
            throw new AppException("Uno o mas TipoJustificacionID no existen en catalogo.", 400);
        }

        var justificacionId = await _repository.CreateAsync(user.UserId, request, cancellationToken);

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = 1,
            DescripcionEvento = "Creacion de boleta de justificacion.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Justificacion:{justificacionId}",
            PayloadResumen = $"Detalles={request.Detalles.Count};Estado={EstadoIds.PendienteJefatura}"
        }, cancellationToken);

        return justificacionId;
    }

    public async Task<IReadOnlyList<JustificacionResumenDto>> ListMineAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsFuncionario(user.Role))
        {
            throw new AppException("Solo un funcionario puede consultar sus boletas.", 403);
        }

        JustificacionValidator.ValidateRangoFechas(filtros.Desde, filtros.Hasta);
        return await _repository.ListMineAsync(user.UserId, filtros, cancellationToken);
    }

    public async Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsJefatura(user.Role))
        {
            throw new AppException("Solo jefatura puede ver pendientes.", 403);
        }

        JustificacionValidator.ValidateRangoFechas(filtros.Desde, filtros.Hasta);
        return await _repository.ListPendientesJefaturaAsync(user.UserId, filtros, cancellationToken);
    }

    public async Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(UserContextInfo user, FiltroRrhhJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsRrhh(user.Role))
        {
            throw new AppException("Solo RRHH puede consultar boletas globales.", 403);
        }

        JustificacionValidator.ValidateRangoFechas(filtros.FechaDesde, filtros.FechaHasta);
        JustificacionValidator.ValidateCompania(filtros.Compania);
        JustificacionValidator.ValidateTextoBusqueda(filtros.Funcionario);

        return await _repository.ListRrhhAsync(filtros, cancellationToken);
    }

    public async Task<JustificacionCompletaDto> GetDetalleJefaturaAsync(UserContextInfo user, int justificacionId, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsJefatura(user.Role))
        {
            throw new AppException("Solo jefatura puede ver el detalle de boletas.", 403);
        }

        var validation = await _repository.GetAprobacionScopeValidationAsync(justificacionId, user.UserId, cancellationToken);
        if (!validation.Exists)
        {
            throw new AppException("No existe la boleta indicada.", 404);
        }

        if (!validation.IsInApprovalScope)
        {
            throw new AppException("La boleta no pertenece al alcance de aprobacion vigente del usuario autenticado.", 403);
        }

        var detalle = await _repository.GetDetalleJefaturaAsync(justificacionId, user.UserId, cancellationToken);
        if (detalle is null)
        {
            throw new AppException("No existe la boleta indicada.", 404);
        }

        return detalle;
    }

    public async Task ResolverAsync(UserContextInfo user, int justificacionId, ResolverJustificacionDto request, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsJefatura(user.Role))
        {
            throw new AppException("RN-03: solo jefatura puede resolver boletas.", 403);
        }

        var accion = JustificacionValidator.ValidateAccion(request.Accion);
        var validation = await _repository.GetResolverValidationAsync(justificacionId, user.UserId, cancellationToken);

        if (!validation.Exists)
        {
            throw new AppException("No existe la boleta indicada.", 404);
        }

        if (!validation.IsInApprovalScope)
        {
            throw new AppException("La boleta no pertenece al alcance de aprobacion vigente del usuario autenticado.", 403);
        }

        if (validation.EstadoId != EstadoIds.PendienteJefatura)
        {
            throw new AppException("RN-04: la boleta ya fue resuelta y no puede modificarse.", 409);
        }

        var comentarioNormalizado = JustificacionValidator.NormalizeComentarioResolucion(request.Comentario);
        var targetEstado = accion == "APROBAR" ? EstadoIds.Aprobado : EstadoIds.Rechazado;
        var affected = await _repository.ResolverAsync(justificacionId, user.UserId, targetEstado, comentarioNormalizado, user.Role, cancellationToken);

        if (affected == 0)
        {
            throw new AppException("No fue posible resolver la boleta porque ya cambió de estado.", 409);
        }

        await _auditEventRepository.LogEventAsync(new AuditEventEntry
        {
            UsuarioId = user.UserId,
            NombreUsuario = $"Usuario {user.UserId}",
            RolCodigo = user.Role,
            TipoEventoAuditoriaId = targetEstado == EstadoIds.Aprobado ? 2 : 3,
            DescripcionEvento = targetEstado == EstadoIds.Aprobado
                ? "Resolucion de boleta aprobada."
                : "Resolucion de boleta rechazada.",
            ResultadoAuditoriaId = 1,
            ReferenciaFuncional = $"Justificacion:{justificacionId}",
            PayloadResumen = $"EstadoDestino={targetEstado};Scope={validation.ScopeSource ?? "N/A"};Delegante={validation.DeleganteUsuarioId?.ToString() ?? "N/A"}"
        }, cancellationToken);
    }
}
