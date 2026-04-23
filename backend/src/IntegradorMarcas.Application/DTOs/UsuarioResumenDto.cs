namespace IntegradorMarcas.Application.DTOs;

public sealed class UsuarioResumenDto
{
    public int UsuarioId { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Compania { get; set; } = string.Empty;
    public int UnidadId { get; set; }
    public int? JefaturaId { get; set; }
}
