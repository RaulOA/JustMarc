namespace IntegradorMarcas.Application.DTOs;

public sealed class FiltroJustificacionesDto
{
    public int? EstadoId { get; set; }
    public DateTime? Desde { get; set; }
    public DateTime? Hasta { get; set; }
}
