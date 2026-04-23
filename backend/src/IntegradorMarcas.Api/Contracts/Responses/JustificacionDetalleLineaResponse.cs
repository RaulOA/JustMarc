namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class JustificacionDetalleLineaResponse
{
    public int DetalleID { get; set; }
    public int TipoJustificacionID { get; set; }
    public string TipoJustificacionDescripcion { get; set; } = string.Empty;
    public DateTime FechaMarca { get; set; }
    public string? ObservacionDetalle { get; set; }
}
