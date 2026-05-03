namespace IntegradorMarcas.Api.Contracts.Requests;

public sealed class UpdateDelegacionRequest
{
    public int DeleganteUsuarioID { get; set; }
    public int DelegadoUsuarioID { get; set; }
    public int? JerarquiaAprobacionID { get; set; }
    public string? Motivo { get; set; }
    public int EstadoRegistroID { get; set; }
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
}
