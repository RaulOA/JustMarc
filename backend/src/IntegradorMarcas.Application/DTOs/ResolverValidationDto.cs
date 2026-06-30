namespace IntegradorMarcas.Application.DTOs;

public sealed class ResolverValidationDto
{
    public bool Exists { get; set; }
    public int EstadoId { get; set; }
    public bool IsInApprovalScope { get; set; }
    public string? ScopeSource { get; set; }
    public int? DeleganteUsuarioId { get; set; }
    // F-004 R8: id del solicitante para verificar que el delegado no resuelve al titular
    public int SolicitanteUsuarioId { get; set; }
}
