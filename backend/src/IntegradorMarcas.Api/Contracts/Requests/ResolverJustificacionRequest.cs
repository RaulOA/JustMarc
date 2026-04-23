namespace IntegradorMarcas.Api.Contracts.Requests;

public sealed class ResolverJustificacionRequest
{
    public string Accion { get; set; } = string.Empty;
    public string? Comentario { get; set; }
}
