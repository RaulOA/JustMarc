namespace IntegradorMarcas.Application.DTOs;

public sealed class FiltroRrhhJustificacionesDto
{
    public string? Funcionario { get; set; }
    public int? EstadoId { get; set; }
    public string? Compania { get; set; }
    public DateTime? FechaDesde { get; set; }
    public DateTime? FechaHasta { get; set; }
}
