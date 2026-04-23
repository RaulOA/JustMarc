namespace IntegradorMarcas.Application.DTOs;

public sealed class CreateJustificacionDto
{
    public string MotivoGeneral { get; set; } = string.Empty;
    public List<JustificacionDetalleDto> Detalles { get; set; } = new();
}
