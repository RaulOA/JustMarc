using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Domain.Constants;

namespace IntegradorMarcas.Application.Services;

/// <summary>
/// F-004: implementacion del servicio de vistas del delegado.
/// Guard: solo ROL_JEFE puede consultar su funcion y registro de delegado.
/// </summary>
public sealed class DelegacionConsultaService : IDelegacionConsultaService
{
    private readonly IDelegacionConsultaRepository _repository;

    public DelegacionConsultaService(IDelegacionConsultaRepository repository)
    {
        _repository = repository;
    }

    // R11/R12
    public async Task<IReadOnlyList<DelegacionFuncionDto>> GetMiFuncionAsync(UserContextInfo user, CancellationToken cancellationToken)
    {
        EnsureJefatura(user);
        return await _repository.GetMiFuncionAsync(user.UserId, DateTime.Today, cancellationToken);
    }

    // R16/R17 (R18: sin ruta de mutacion — solo existe este GET)
    public async Task<IReadOnlyList<DelegacionRegistroDto>> GetMiRegistroAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        EnsureJefatura(user);
        return await _repository.GetMiRegistroAsync(user.UserId, filtros, cancellationToken);
    }

    private static void EnsureJefatura(UserContextInfo user)
    {
        if (!RolesSistema.EsJefatura(user.Role))
        {
            throw new AppException("Solo jefatura puede consultar sus delegaciones.", 403);
        }
    }
}
