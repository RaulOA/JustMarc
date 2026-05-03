namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class AdminDependenciaResponse
{
    public int EstructuraOrganizacionalID { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public string? CodigoOrigen { get; set; }
    public int? EstructuraPadreID { get; set; }
    public int EstadoRegistroID { get; set; }
    public DateTime? VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
