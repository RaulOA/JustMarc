using IntegradorMarcas.Application.DTOs;

namespace IntegradorMarcas.Application.Interfaces;

/// <summary>
/// F-004 D3 = A: repositorio dedicado para las vistas de consulta del delegado.
/// No infla IAdminAprobacionesRepository.
/// </summary>
public interface IDelegacionConsultaRepository
{
    // R11/R12: funcion de delegacion activa/vigente recibida por el delegado
    Task<IReadOnlyList<DelegacionFuncionDto>> GetMiFuncionAsync(int delegadoUsuarioId, DateTime fechaRef, CancellationToken cancellationToken);

    // R16/R17: registro de solo lectura — justificaciones tramitadas dentro del periodo de la delegacion (D4)
    Task<IReadOnlyList<DelegacionRegistroDto>> GetMiRegistroAsync(int delegadoUsuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken);
}
