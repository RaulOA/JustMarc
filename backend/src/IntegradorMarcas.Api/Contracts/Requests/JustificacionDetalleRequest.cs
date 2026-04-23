namespace IntegradorMarcas.Api.Contracts.Requests;

public sealed class JustificacionDetalleRequest
{
    public int TipoJustificacionID { get; set; }
    public DateTime FechaMarca { get; set; }
    public string? ObservacionDetalle { get; set; }
}
