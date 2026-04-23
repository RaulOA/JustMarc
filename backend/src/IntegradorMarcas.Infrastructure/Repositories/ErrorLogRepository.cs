using Dapper;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using Microsoft.Data.SqlClient;

namespace IntegradorMarcas.Infrastructure.Repositories;

public sealed class ErrorLogRepository : IErrorLogRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public ErrorLogRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task LogAsync(ErrorLogEntry entry)
    {
        try
        {
            await using var connection = (SqlConnection)_connectionFactory.CreateConnection();
            await connection.OpenAsync();

            const string sql = """
                INSERT INTO dbo.ApiErrorLog
                    (CorrelationID, HttpMethod, Endpoint, StatusCode, TipoError, Mensaje, StackTrace,
                     UsuarioID, RolUsuario, Entorno, Ip, UserAgent)
                VALUES
                    (@CorrelationID, @HttpMethod, @Endpoint, @StatusCode, @TipoError, @Mensaje, @StackTrace,
                     @UsuarioID, @RolUsuario, @Entorno, @Ip, @UserAgent)
                """;

            await connection.ExecuteAsync(sql, new
            {
                CorrelationID = entry.CorrelationId,
                entry.HttpMethod,
                entry.Endpoint,
                entry.StatusCode,
                entry.TipoError,
                entry.Mensaje,
                entry.StackTrace,
                UsuarioID  = entry.UsuarioId,
                entry.RolUsuario,
                entry.Entorno,
                entry.Ip,
                entry.UserAgent
            });
        }
        catch
        {
            // El log de errores nunca debe propagar excepciones hacia el usuario.
            // Si la BD no está disponible, la operación original ya falló; aquí solo se registra.
        }
    }
}
