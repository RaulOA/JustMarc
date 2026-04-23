namespace IntegradorMarcas.Domain.Entities;

public sealed class JustificacionEncabezado
{
    public int JustificacionId { get; set; }
    public int UsuarioId { get; set; }
    public string MotivoGeneral { get; set; } = string.Empty;
    public int EstadoId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public int? AprobadorId { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public string UsrRegistro { get; set; } = string.Empty;
    public DateTime FecRegistro { get; set; }
}
