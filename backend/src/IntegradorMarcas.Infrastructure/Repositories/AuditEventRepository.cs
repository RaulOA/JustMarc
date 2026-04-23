using Dapper;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Queries;
using Microsoft.Data.SqlClient;

namespace IntegradorMarcas.Infrastructure.Repositories;

public sealed class AuditEventRepository : IAuditEventRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AuditEventRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task LogEventAsync(AuditEventEntry entry, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            AuditoriaSql.InsertEvento,
            new
            {
                UsuarioID = entry.UsuarioId,
                entry.NombreUsuario,
                entry.RolCodigo,
                TipoEventoAuditoriaID = entry.TipoEventoAuditoriaId,
                entry.DescripcionEvento,
                ResultadoAuditoriaID = entry.ResultadoAuditoriaId,
                entry.ReferenciaFuncional,
                entry.PayloadResumen
            },
            cancellationToken: cancellationToken));
    }
}
