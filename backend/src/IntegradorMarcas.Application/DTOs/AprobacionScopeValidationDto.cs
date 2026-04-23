namespace IntegradorMarcas.Application.DTOs;

public sealed class AprobacionScopeValidationDto
{
    public bool Exists { get; set; }
    public int EstadoId { get; set; }
    public bool IsInApprovalScope { get; set; }
    public string? ScopeSource { get; set; }
    public int? DeleganteUsuarioId { get; set; }
}
