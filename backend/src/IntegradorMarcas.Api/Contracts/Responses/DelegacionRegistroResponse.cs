namespace IntegradorMarcas.Api.Contracts.Responses;

/// <summary>F-004 R16/R17: respuesta de mi-registro (solo lectura) para el delegado.</summary>
public sealed class DelegacionRegistroResponse
{
    public int JustificacionID { get; set; }
    public string? MotivoGeneral { get; set; }
    public string? ComentarioResolucion { get; set; }
    public int EstadoID { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public int SolicitanteUsuarioID { get; set; }
    public string SolicitanteNombre { get; set; } = string.Empty;
    public int DelegacionAprobacionID { get; set; }
    public int TitularUsuarioID { get; set; }
    public string TitularNombre { get; set; } = string.Empty;
}
