namespace IntegradorMarcas.Application.DTOs;

public sealed class AdminUsuarioAsignacionDto
{
    public int UsuarioId { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public int? JefaturaId { get; set; }
    public string? JefaturaNombre { get; set; }
    public int UnidadId { get; set; }
    public int RolId { get; set; }
    public string RolDescripcion { get; set; } = string.Empty;
    public bool EsActivo { get; set; }
}
