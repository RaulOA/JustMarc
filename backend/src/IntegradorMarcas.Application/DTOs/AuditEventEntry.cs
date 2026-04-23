namespace IntegradorMarcas.Application.DTOs;

public sealed class AuditEventEntry
{
    public int? UsuarioId { get; set; }
    public string NombreUsuario { get; set; } = string.Empty;
    public string RolCodigo { get; set; } = string.Empty;
    public int TipoEventoAuditoriaId { get; set; }
    public string DescripcionEvento { get; set; } = string.Empty;
    public int ResultadoAuditoriaId { get; set; }
    public string? ReferenciaFuncional { get; set; }
    public string? PayloadResumen { get; set; }
}
