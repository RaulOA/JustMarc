namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class CurrentApproverResponse
{
    public int SolicitanteUsuarioID { get; set; }
    public UsuarioResumenResponse? Aprobador { get; set; }
    public string? Origen { get; set; }
    public int? DeleganteUsuarioID { get; set; }
    public string? DeleganteNombre { get; set; }
}
