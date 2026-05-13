using Dapper;
using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Domain.Constants;
using IntegradorMarcas.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

namespace IntegradorMarcas.Api.Controllers;

[ApiController]
[Route("api/admin/monitoring")]
public sealed class AdminMonitoringController : ControllerBase
{
    private readonly ISqlConnectionFactory _connectionFactory;
    private readonly IUserContext _userContext;

    public AdminMonitoringController(ISqlConnectionFactory connectionFactory, IUserContext userContext)
    {
        _connectionFactory = connectionFactory;
        _userContext = userContext;
    }

    [HttpGet("registros")]
    public async Task<ActionResult<IReadOnlyList<AdminMonitoringRecordResponse>>> ListRegistros(
        [FromQuery] string? tipo,
        [FromQuery] string? search,
        [FromQuery] DateTime? desde,
        [FromQuery] DateTime? hasta,
        [FromQuery] string? sortBy,
        [FromQuery] string? sortDir,
        CancellationToken cancellationToken)
    {
        var user = _userContext.GetCurrent();
        if (!RolesSistema.EsAdmin(user.Role))
        {
            throw new AppException("No tiene permisos para consultar registros de monitoreo.", 403);
        }

        var tipoNormalized = (tipo ?? string.Empty).Trim().ToUpperInvariant();
        if (tipoNormalized is not "" and not "ERROR" and not "EVENTO")
        {
            throw new AppException("El parámetro 'tipo' debe ser ERROR o EVENTO.", 400);
        }

        var sortByNormalized = (sortBy ?? "fecha").Trim().ToLowerInvariant();
        var sortDirNormalized = (sortDir ?? "desc").Trim().ToLowerInvariant();
        if (sortDirNormalized is not "asc" and not "desc")
        {
            throw new AppException("El parámetro 'sortDir' debe ser asc o desc.", 400);
        }

        var allowedSortFields = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            "fecha", "tipo", "mensaje", "usuario", "estado"
        };
        if (!allowedSortFields.Contains(sortByNormalized))
        {
            throw new AppException("El parámetro 'sortBy' no es válido.", 400);
        }

        const string sql = """
WITH Registros AS (
    SELECT
        CAST(e.FechaUtc AS datetime2) AS Fecha,
        CAST('ERROR' AS nvarchar(20)) AS Tipo,
        CAST(e.TipoError AS nvarchar(200)) AS Categoria,
        CAST(e.Mensaje AS nvarchar(max)) AS Mensaje,
        CAST(COALESCE(CAST(e.UsuarioID AS nvarchar(50)), '') AS nvarchar(200)) AS Usuario,
        CAST(CAST(e.StatusCode AS nvarchar(20)) AS nvarchar(50)) AS Estado,
        CAST(COALESCE(e.CorrelationId, '') AS nvarchar(120)) AS Referencia,
        CAST(COALESCE(e.Endpoint, '') AS nvarchar(300)) AS Origen,
        CAST(COALESCE(e.StackTrace, '') AS nvarchar(max)) AS Detalle
    FROM Auditoria.ErrorApi e
    WHERE (@Tipo = '' OR @Tipo = 'ERROR')
      AND (@Desde IS NULL OR e.FechaUtc >= @Desde)
      AND (@Hasta IS NULL OR e.FechaUtc < DATEADD(DAY, 1, @Hasta))

    UNION ALL

    SELECT
        CAST(a.FechaEvento AS datetime2) AS Fecha,
        CAST('EVENTO' AS nvarchar(20)) AS Tipo,
        CAST(COALESCE(CAST(a.TipoEventoAuditoriaId AS nvarchar(20)), '') AS nvarchar(200)) AS Categoria,
        CAST(COALESCE(a.DescripcionEvento, '') AS nvarchar(max)) AS Mensaje,
        CAST(COALESCE(a.NombreUsuario, CAST(a.UsuarioId AS nvarchar(50)), '') AS nvarchar(200)) AS Usuario,
        CAST(COALESCE(CAST(a.ResultadoAuditoriaId AS nvarchar(20)), '') AS nvarchar(50)) AS Estado,
        CAST(COALESCE(a.ReferenciaFuncional, '') AS nvarchar(120)) AS Referencia,
        CAST('EventoAuditoria' AS nvarchar(300)) AS Origen,
        CAST(COALESCE(a.PayloadResumen, '') AS nvarchar(max)) AS Detalle
    FROM Auditoria.EventoAuditoria a
    WHERE (@Tipo = '' OR @Tipo = 'EVENTO')
      AND (@Desde IS NULL OR a.FechaEvento >= @Desde)
      AND (@Hasta IS NULL OR a.FechaEvento < DATEADD(DAY, 1, @Hasta))
)
SELECT
    Fecha,
    Tipo,
    Categoria,
    Mensaje,
    Usuario,
    Estado,
    Referencia,
    Origen,
    Detalle
FROM Registros
WHERE (
    @Search = '' OR
    Mensaje LIKE '%' + @Search + '%' OR
    Usuario LIKE '%' + @Search + '%' OR
    Categoria LIKE '%' + @Search + '%' OR
    Referencia LIKE '%' + @Search + '%' OR
    Origen LIKE '%' + @Search + '%'
)
ORDER BY
    CASE WHEN @SortBy = 'fecha' AND @SortDir = 'asc'  THEN Fecha END ASC,
    CASE WHEN @SortBy = 'fecha' AND @SortDir = 'desc' THEN Fecha END DESC,
    CASE WHEN @SortBy = 'tipo' AND @SortDir = 'asc'  THEN Tipo END ASC,
    CASE WHEN @SortBy = 'tipo' AND @SortDir = 'desc' THEN Tipo END DESC,
    CASE WHEN @SortBy = 'mensaje' AND @SortDir = 'asc'  THEN Mensaje END ASC,
    CASE WHEN @SortBy = 'mensaje' AND @SortDir = 'desc' THEN Mensaje END DESC,
    CASE WHEN @SortBy = 'usuario' AND @SortDir = 'asc'  THEN Usuario END ASC,
    CASE WHEN @SortBy = 'usuario' AND @SortDir = 'desc' THEN Usuario END DESC,
    CASE WHEN @SortBy = 'estado' AND @SortDir = 'asc'  THEN Estado END ASC,
    CASE WHEN @SortBy = 'estado' AND @SortDir = 'desc' THEN Estado END DESC,
    Fecha DESC;
""";

        using var connection = _connectionFactory.CreateConnection();
        var data = await connection.QueryAsync<AdminMonitoringRecordResponse>(
            new CommandDefinition(
                sql,
                new
                {
                    Tipo = tipoNormalized,
                    Search = (search ?? string.Empty).Trim(),
                    Desde = desde,
                    Hasta = hasta,
                    SortBy = sortByNormalized,
                    SortDir = sortDirNormalized
                },
                cancellationToken: cancellationToken));

        return Ok(data.ToList());
    }

    public sealed class AdminMonitoringRecordResponse
    {
        public DateTime Fecha { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public string Categoria { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string Usuario { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Referencia { get; set; } = string.Empty;
        public string Origen { get; set; } = string.Empty;
        public string Detalle { get; set; } = string.Empty;
    }
}