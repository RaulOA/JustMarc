namespace IntegradorMarcas.Application.DTOs;

public sealed class UpdateDependenciaDto
{
    public string Nombre { get; set; } = string.Empty;
    public int? EstructuraPadreId { get; set; }
    public int EstadoRegistroId { get; set; }
}
