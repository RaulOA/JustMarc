namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class UsuarioResumenResponse
{
    public int UsuarioID { get; set; }
    public string NombreCompleto { get; set; } = string.Empty;
    public string Cedula { get; set; } = string.Empty;
    public string Correo { get; set; } = string.Empty;
    public string Compania { get; set; } = string.Empty;
    public int UnidadID { get; set; }
    public int? JefaturaID { get; set; }
}
