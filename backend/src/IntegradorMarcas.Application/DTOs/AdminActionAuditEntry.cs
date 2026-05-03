namespace IntegradorMarcas.Application.DTOs;

public sealed class AdminActionAuditEntry
{
    public DateTime FechaEventoUtc { get; set; } = DateTime.UtcNow;
    public string? CorrelationId { get; set; }
    public int UsuarioActorId { get; set; }
    public string RolActorCodigo { get; set; } = string.Empty;
    public string EntidadObjetivo { get; set; } = string.Empty;
    public string EntidadObjetivoId { get; set; } = string.Empty;
    public string Accion { get; set; } = string.Empty;
    public int ResultadoAuditoriaId { get; set; }
    public string Descripcion { get; set; } = string.Empty;
    public string? ValoresAnteriores { get; set; }
    public string? ValoresNuevos { get; set; }
    public string? Metadata { get; set; }
}
