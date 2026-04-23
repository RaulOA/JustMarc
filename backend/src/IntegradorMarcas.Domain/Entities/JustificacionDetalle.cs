namespace IntegradorMarcas.Domain.Entities;

public sealed class JustificacionDetalle
{
    public int DetalleId { get; set; }
    public int JustificacionId { get; set; }
    public int TipoJustificacionId { get; set; }
    public DateTime FechaMarca { get; set; }
    public string? ObservacionDetalle { get; set; }
    public string UsrRegistro { get; set; } = string.Empty;
    public DateTime FecRegistro { get; set; }
}
