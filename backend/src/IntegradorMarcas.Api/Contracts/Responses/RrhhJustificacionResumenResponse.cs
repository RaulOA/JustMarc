namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class RrhhJustificacionResumenResponse
{
    public int JustificacionID { get; set; }
    public string MotivoGeneral { get; set; } = string.Empty;
    public string? ComentarioResolucion { get; set; }
    public int EstadoID { get; set; }
    public string EstadoDescripcion { get; set; } = string.Empty;
    public DateTime FechaCreacion { get; set; }
    public int CantidadDetalles { get; set; }
    public int? AprobadorID { get; set; }
    public DateTime? FechaAprobacion { get; set; }
    public int FuncionarioID { get; set; }
    public string FuncionarioNombre { get; set; } = string.Empty;
    public string FuncionarioCedula { get; set; } = string.Empty;
    public string Compania { get; set; } = string.Empty;
    public int? JefaturaID { get; set; }
    public string? JefaturaNombre { get; set; }
    public string? TipoPrincipal { get; set; }
}
