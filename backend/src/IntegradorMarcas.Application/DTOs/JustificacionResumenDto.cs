namespace IntegradorMarcas.Application.DTOs;

public sealed class JustificacionResumenDto
{
    public int JustificacionId { get; set; }
    public string MotivoGeneral { get; set; } = string.Empty;
    public string? ComentarioResolucion { get; set; }
    public int EstadoId { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int CantidadDetalles { get; set; }
    public int? AprobadorId { get; set; }
    public DateTime? FechaAprobacion { get; set; }
}
