using Microsoft.AspNetCore.Mvc;

namespace IntegradorMarcas.Api.Controllers;

/// <summary>
/// Controlador para gestión de sesiones de usuario.
/// Proporciona endpoints para validar estado de sesión y cerrar sesiones.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class SessionController : ControllerBase
{
    /// <summary>
    /// Valida el estado actual de la sesión del usuario.
    /// </summary>
    /// <remarks>
    /// Verifica que los headers de autenticación (X-User-Id, X-User-Role) sean válidos.
    /// Responde con 401 si la sesión es inválida o está expirada.
    /// </remarks>
    /// <returns>
    /// 200 OK con información de sesión válida:
    /// {
    ///   "isValid": true,
    ///   "userId": 4,
    ///   "role": "ROL_FUNC",
    ///   "serverTime": "2026-05-03T14:30:00Z"
    /// }
    /// </returns>
    [HttpGet("status")]
    [Produces("application/json")]
    public IActionResult GetSessionStatus()
    {
        // Verificar que los headers de identidad estén presentes
        if (!Request.Headers.TryGetValue("X-User-Id", out var userIdHeader) ||
            !Request.Headers.TryGetValue("X-User-Role", out var userRoleHeader))
        {
            return Unauthorized(new
            {
                detail = "Headers de autenticación (X-User-Id, X-User-Role) requeridos",
                statusCode = 401
            });
        }

        var userIdStr = userIdHeader.ToString().Trim();
        var userRole = userRoleHeader.ToString().Trim();

        // Validar que los headers no estén vacíos
        if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(userRole))
        {
            return Unauthorized(new
            {
                detail = "Headers de autenticación inválidos o vacíos",
                statusCode = 401
            });
        }

        // Validar que userId sea un número válido
        if (!int.TryParse(userIdStr, out var userId))
        {
            return Unauthorized(new
            {
                detail = "X-User-Id debe ser un número entero válido",
                statusCode = 401
            });
        }

        // Respuesta exitosa con información de sesión
        return Ok(new
        {
            isValid = true,
            userId = userId,
            role = userRole,
            serverTime = DateTime.UtcNow
        });
    }

    /// <summary>
    /// Cierra la sesión actual del usuario.
    /// </summary>
    /// <remarks>
    /// Endpoint preparado para futuras extensiones que puedan requerir invalidación
    /// servidor-lado de tokens (ej: JWT blacklisting en caso de migración futura).
    /// Actualmente responde con 200 OK indicando que el cliente puede limpiar su sesión local.
    /// </remarks>
    /// <returns>200 OK indicando logout exitoso</returns>
    [HttpPost("logout")]
    [Produces("application/json")]
    public IActionResult Logout()
    {
        // Headers opcionales para logging o auditoría futura
        Request.Headers.TryGetValue("X-User-Id", out var userIdHeader);
        Request.Headers.TryGetValue("X-User-Role", out var userRoleHeader);

        // Respuesta exitosa indicando al cliente que puede limpiar su sesión local
        return Ok(new
        {
            message = "Sesión cerrada exitosamente",
            loggedOutAt = DateTime.UtcNow
        });
    }
}
