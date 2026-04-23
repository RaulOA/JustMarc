namespace IntegradorMarcas.Application.DTOs;

public sealed class JustificacionDetalleLineaDto
{
    public int DetalleID { get; set; }
    public int TipoJustificacionID { get; set; }
    public string TipoJustificacionDescripcion { get; set; } = string.Empty;
    public DateTime FechaMarca { get; set; }
    public string? ObservacionDetalle { get; set; }
}
