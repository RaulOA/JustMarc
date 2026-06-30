namespace IntegradorMarcas.Api.Contracts.Responses;

/// <summary>F-004 R11/R12: respuesta de mi-funcion para el delegado.</summary>
public sealed class DelegacionFuncionResponse
{
    public int DelegacionAprobacionID { get; set; }
    public int TitularUsuarioID { get; set; }
    public string TitularNombre { get; set; } = string.Empty;
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public string? Motivo { get; set; }
    public string? AlcanceEstructuras { get; set; }
    public int? JerarquiaAprobacionID { get; set; }
}
