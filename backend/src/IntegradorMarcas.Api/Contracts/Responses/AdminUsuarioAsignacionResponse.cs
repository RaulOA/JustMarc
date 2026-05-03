namespace IntegradorMarcas.Api.Contracts.Responses;

public sealed class AdminUsuarioAsignacionResponse
{
    public int UsuarioID { get; set; }
    public string Cedula { get; set; } = string.Empty;
    public string NombreCompleto { get; set; } = string.Empty;
    public string CorreoElectronico { get; set; } = string.Empty;
    public int? JefaturaID { get; set; }
    public string? JefaturaNombre { get; set; }
    public int UnidadID { get; set; }
    public int RolID { get; set; }
    public string RolDescripcion { get; set; } = string.Empty;
    public bool EsActivo { get; set; }
}
