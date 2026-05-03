namespace IntegradorMarcas.Application.DTOs;

public sealed class AdminDependenciaDto
{
    public int EstructuraOrganizacionalId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? CodigoOrigen { get; set; }
    public int? EstructuraPadreId { get; set; }
    public int EstadoRegistroId { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
