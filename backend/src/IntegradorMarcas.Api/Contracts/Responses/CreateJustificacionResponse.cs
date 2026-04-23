namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class CreateJustificacionResponse
{
    public int JustificacionID { get; set; }
    public int EstadoID { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
}
