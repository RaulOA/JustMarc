namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class AdminJerarquiaResponse
{
    public int JerarquiaAprobacionID { get; set; }
    public int AprobadorUsuarioID { get; set; }
    public int EstructuraOrganizacionalID { get; set; }
    public int NivelAprobacion { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
    public int EstadoRegistroID { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
