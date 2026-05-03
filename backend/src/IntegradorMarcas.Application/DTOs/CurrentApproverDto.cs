namespace IntegradorMarcas.Application.DTOs;

public sealed class CurrentApproverDto
{
    public int SolicitanteUsuarioId { get; set; }
    public UsuarioResumenDto? Aprobador { get; set; }
    public string? Origen { get; set; }
    public int? DeleganteUsuarioId { get; set; }
    public string? DeleganteNombre { get; set; }
}
