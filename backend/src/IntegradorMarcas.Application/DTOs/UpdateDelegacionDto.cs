namespace IntegradorMarcas.Application.DTOs;

public sealed class UpdateDelegacionDto
{
    public int DeleganteUsuarioId { get; set; }
    public int DelegadoUsuarioId { get; set; }
    public int? JerarquiaAprobacionId { get; set; }
    public string? Motivo { get; set; }
    public int EstadoRegistroId { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
