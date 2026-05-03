namespace IntegradorMarcas.Api.Contracts.Requests;

public sealed class UpdateUsuarioAsignacionRequest
{
    public int? RolID { get; set; }
    public int? UnidadID { get; set; }
    public int? JefaturaID { get; set; }
}
