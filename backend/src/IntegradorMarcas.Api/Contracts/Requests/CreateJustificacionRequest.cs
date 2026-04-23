namespace IntegradorMarcas.Api.Contracts.Requests;

public sealed class CreateJustificacionRequest
{
    public string MotivoGeneral { get; set; } = string.Empty;
    public List<JustificacionDetalleRequest> Detalles { get; set; } = new();
}
