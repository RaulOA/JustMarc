namespace IntegradorMarcas.Application.DTOs;

/// <summary>
/// F-004 R15: resultado de la consulta de validacion para re-resolucion del titular
/// sobre una justificacion que fue resuelta por su delegado.
/// </summary>
public sealed class RevisarTitularValidationDto
{
    public bool Exists { get; set; }
    public int EstadoId { get; set; }
    /// <summary>Indica que el actor tiene alcance de jerarquia directa (no delegacion) sobre el solicitante.</summary>
    public bool EsTitularPorJerarquia { get; set; }
    public int AprobadorAnteriorId { get; set; }
    public int SolicitanteUsuarioId { get; set; }
}
