namespace IntegradorMarcas.Application.DTOs;

public sealed class JustificacionDetalleDto
{
    public int TipoJustificacionId { get; set; }
    public DateTime FechaMarca { get; set; }
    public string? ObservacionDetalle { get; set; }
}
