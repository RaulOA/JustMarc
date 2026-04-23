namespace IntegradorMarcas.Application.Interfaces;

public interface IErrorLogRepository
{
    Task LogAsync(ErrorLogEntry entry);
}

public sealed record ErrorLogEntry(
    Guid CorrelationId,
    string HttpMethod,
    string Endpoint,
    int StatusCode,
    string TipoError,
    string Mensaje,
    string? StackTrace,
    string? UsuarioId,
    string? RolUsuario,
    string Entorno,
    string? Ip,
    string? UserAgent
);
