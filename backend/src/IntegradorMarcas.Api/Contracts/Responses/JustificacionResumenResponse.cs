namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class JustificacionResumenResponse
{
    public int JustificacionID { get; set; }
    public string MotivoGeneral { get; set; } = string.Empty;
    public int EstadoID { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int CantidadDetalles { get; set; }
    public int? AprobadorID { get; set; }
    public DateTime? FechaAprobacion { get; set; }
}
