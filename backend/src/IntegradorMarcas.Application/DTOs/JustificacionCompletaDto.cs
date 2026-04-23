namespace IntegradorMarcas.Application.DTOs;

public sealed class JustificacionCompletaDto
{
    public JustificacionResumenDto Encabezado { get; set; } = new();
    public UsuarioResumenDto Solicitante { get; set; } = new();
    public UsuarioResumenDto? Aprobador { get; set; }
    public IReadOnlyList<JustificacionDetalleLineaDto> Detalles { get; set; } = Array.Empty<JustificacionDetalleLineaDto>();
}
