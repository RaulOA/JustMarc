using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.Interfaces;

namespace IntegradorMarcas.Api.Security;

/// <summary>
/// Resuelve la identidad del usuario a partir de headers HTTP definidos por configuracion.
/// </summary>
public sealed class HeaderUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public HeaderUserContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    /// <summary>
    /// Obtiene UserId y Role desde el request actual.
    /// </summary>
    /// <remarks>
    /// Lanza AppException (401) cuando no existe contexto HTTP o cuando los headers requeridos
    /// no estan presentes o son invalidos.
    /// </remarks>
    public UserContextInfo GetCurrent()
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new AppException("No hay contexto HTTP disponible.", 401);

        // Permite cambiar nombres de cabecera por configuracion manteniendo defaults compatibles.
        var userHeader = _configuration["Security:HeaderUserId"] ?? "X-User-Id";
        var roleHeader = _configuration["Security:HeaderRole"] ?? "X-User-Role";

        // Se considera identidad valida solo cuando el UserId existe, parsea a entero y es mayor a cero.
        if (!context.Request.Headers.TryGetValue(userHeader, out var userValues)
            || !int.TryParse(userValues.FirstOrDefault(), out var userId)
            || userId <= 0)
        {
            throw new AppException($"Header requerido inválido: {userHeader}", 401);
        }

        // El rol es obligatorio para aplicar autorizacion de negocio en capas superiores.
        if (!context.Request.Headers.TryGetValue(roleHeader, out var roleValues)
            || string.IsNullOrWhiteSpace(roleValues.FirstOrDefault()))
        {
            throw new AppException($"Header requerido inválido: {roleHeader}", 401);
        }

        return new UserContextInfo
        {
            UserId = userId,
            Role = roleValues.FirstOrDefault() ?? string.Empty
        };
    }
}
