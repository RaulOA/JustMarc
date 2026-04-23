using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.Interfaces;

namespace IntegradorMarcas.Api.Security;

public sealed class HeaderUserContext : IUserContext
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IConfiguration _configuration;

    public HeaderUserContext(IHttpContextAccessor httpContextAccessor, IConfiguration configuration)
    {
        _httpContextAccessor = httpContextAccessor;
        _configuration = configuration;
    }

    public UserContextInfo GetCurrent()
    {
        var context = _httpContextAccessor.HttpContext
            ?? throw new AppException("No hay contexto HTTP disponible.", 401);

        var userHeader = _configuration["Security:HeaderUserId"] ?? "X-User-Id";
        var roleHeader = _configuration["Security:HeaderRole"] ?? "X-User-Role";

        if (!context.Request.Headers.TryGetValue(userHeader, out var userValues)
            || !int.TryParse(userValues.FirstOrDefault(), out var userId)
            || userId <= 0)
        {
            throw new AppException($"Header requerido inválido: {userHeader}", 401);
        }

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
