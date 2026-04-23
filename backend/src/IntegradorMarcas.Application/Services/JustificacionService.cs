using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Application.Validation;
using IntegradorMarcas.Domain.Constants;

namespace IntegradorMarcas.Application.Services;

public sealed class JustificacionService : IJustificacionService
{
    private readonly IJustificacionRepository _repository;

    public JustificacionService(IJustificacionRepository repository)
    {
        _repository = repository;
    }

    public async Task<int> CreateAsync(UserContextInfo user, CreateJustificacionDto request, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsFuncionario(user.Role))
        {
            throw new AppException("Solo un funcionario puede crear boletas.", 403);
        }

        JustificacionValidator.ValidateCreate(request);

        var justificacionId = await _repository.CreateAsync(user.UserId, request, cancellationToken);
        return justificacionId;
    }

    public async Task<IReadOnlyList<JustificacionResumenDto>> ListMineAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsFuncionario(user.Role))
        {
            throw new AppException("Solo un funcionario puede consultar sus boletas.", 403);
        }

        return await _repository.ListMineAsync(user.UserId, filtros, cancellationToken);
    }

    public async Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        if (!RolesSistema.EsJefatura(user.Role))
        {
            throw new AppException("Solo jefatura puede ver pendientes.", 403);
        }

        return await _repository.ListPendientesJefaturaAsync(user.UserId, filtros, cancellationToken);
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

        if (!validation.IsSubordinado)
        {
            throw new AppException("La boleta no pertenece a un subordinado directo de la jefatura autenticada.", 403);
        }

        if (validation.EstadoId != EstadoIds.PendienteJefatura)
        {
            throw new AppException("RN-04: la boleta ya fue resuelta y no puede modificarse.", 409);
        }

        var targetEstado = accion == "APROBAR" ? EstadoIds.Aprobado : EstadoIds.Rechazado;
        var affected = await _repository.ResolverAsync(justificacionId, user.UserId, targetEstado, request.Comentario, cancellationToken);

        if (affected == 0)
        {
            throw new AppException("No fue posible resolver la boleta porque ya cambió de estado.", 409);
        }
    }
}
