namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class JustificacionDetalleCompletaResponse
{
    public JustificacionResumenResponse Encabezado { get; set; } = new();
    public UsuarioResumenResponse Solicitante { get; set; } = new();
    public UsuarioResumenResponse? Aprobador { get; set; }
    public IReadOnlyList<JustificacionDetalleLineaResponse> Detalles { get; set; } = Array.Empty<JustificacionDetalleLineaResponse>();
}
