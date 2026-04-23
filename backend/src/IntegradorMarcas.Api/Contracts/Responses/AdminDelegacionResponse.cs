namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class AdminDelegacionResponse
{
    public int DelegacionAprobacionID { get; set; }
    public int DeleganteUsuarioID { get; set; }
    public int DelegadoUsuarioID { get; set; }
    public int? JerarquiaAprobacionID { get; set; }
    public string? Motivo { get; set; }
    public int EstadoRegistroID { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
