namespace IntegradorMarcas.Application.DTOs;

public sealed class CreateDelegacionDto
{
    public int DeleganteUsuarioId { get; set; }
    public int DelegadoUsuarioId { get; set; }
    public int? JerarquiaAprobacionId { get; set; }
    public string? Motivo { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
