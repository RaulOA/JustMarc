namespace IntegradorMarcas.Application.DTOs;

public sealed class AdminJerarquiaDto
{
    public int JerarquiaAprobacionId { get; set; }
    public int AprobadorUsuarioId { get; set; }
    public int EstructuraOrganizacionalId { get; set; }
    public int NivelAprobacion { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
    public int EstadoRegistroId { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
