namespace IntegradorMarcas.Application.DTOs;

/// <summary>
/// F-004 R16/R17: registro de solo lectura de justificaciones tramitadas por el delegado
/// dentro del periodo de la delegacion correspondiente (D4: filtro por FechaAprobacion).
/// </summary>
public sealed class DelegacionRegistroDto
{
    public int JustificacionId { get; set; }
    public string? MotivoGeneral { get; set; }
    public string? ComentarioResolucion { get; set; }
    public int EstadoId { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public int SolicitanteUsuarioId { get; set; }
    public string SolicitanteNombre { get; set; } = string.Empty;
    /// <summary>Id de la delegacion que dio el alcance para resolver esta justificacion.</summary>
    public int DelegacionAprobacionId { get; set; }
    public int TitularUsuarioId { get; set; }
    public string TitularNombre { get; set; } = string.Empty;
}
