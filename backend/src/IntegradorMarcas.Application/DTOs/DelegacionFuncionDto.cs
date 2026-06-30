namespace IntegradorMarcas.Application.DTOs;

/// <summary>
/// F-004 R11/R12: funcion de delegacion recibida por un delegado (quién se la asigno, vigencia, alcance).
/// </summary>
public sealed class DelegacionFuncionDto
{
    public int DelegacionAprobacionId { get; set; }
    /// <summary>Id del titular (delegante) que asigno la delegacion.</summary>
    public int TitularUsuarioId { get; set; }
    public string TitularNombre { get; set; } = string.Empty;
    public DateTime VigenciaDesde { get; set; }
    public DateTime? VigenciaHasta { get; set; }
    public string? Motivo { get; set; }
    /// <summary>R12: estructuras organizacionales aprobables bajo esta delegacion (nombre separado por coma si son multiples).</summary>
    public string? AlcanceEstructuras { get; set; }
    public int? JerarquiaAprobacionId { get; set; }
}
