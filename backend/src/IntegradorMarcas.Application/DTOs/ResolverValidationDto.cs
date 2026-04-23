namespace IntegradorMarcas.Application.DTOs;

public sealed class ResolverValidationDto
{
    public bool Exists { get; set; }
    public int EstadoId { get; set; }
    public bool IsSubordinado { get; set; }
}
