namespace IntegradorMarcas.Application.DTOs;

public sealed class CreateJerarquiaDto
{
    public int AprobadorUsuarioId { get; set; }
    public int EstructuraOrganizacionalId { get; set; }
    public int NivelAprobacion { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
