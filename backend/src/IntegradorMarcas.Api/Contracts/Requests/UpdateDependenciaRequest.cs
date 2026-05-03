namespace IntegradorMarcas.Api.Contracts.Requests;

public sealed class UpdateDependenciaRequest
{
    public string Nombre { get; set; } = string.Empty;
    public int? EstructuraPadreID { get; set; }
    public int EstadoRegistroID { get; set; }
}
