namespace IntegradorMarcas.Api.Contracts.Requests;

public sealed class UpdateJerarquiaRequest
{
    public int AprobadorUsuarioID { get; set; }
    public int EstructuraOrganizacionalID { get; set; }
    public int NivelAprobacion { get; set; }
    public string TipoRelacion { get; set; } = string.Empty;
    public int EstadoRegistroID { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
