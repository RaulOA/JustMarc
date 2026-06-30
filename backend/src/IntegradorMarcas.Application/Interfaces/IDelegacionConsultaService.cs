using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

/// <summary>
/// F-004: servicio de vistas del delegado (regla 6 y regla 9).
/// </summary>
public interface IDelegacionConsultaService
{
    // R11/R12: funcion — que delegacion tiene activa, quien se la asigno y el alcance
    Task<IReadOnlyList<DelegacionFuncionDto>> GetMiFuncionAsync(UserContextInfo user, CancellationToken cancellationToken);

    // R16/R17: registro de solo lectura de lo tramitado como delegado dentro del periodo
    Task<IReadOnlyList<DelegacionRegistroDto>> GetMiRegistroAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken cancellationToken);
}
