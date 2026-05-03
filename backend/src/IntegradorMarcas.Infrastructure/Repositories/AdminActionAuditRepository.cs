using Dapper;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Queries;
using Microsoft.Data.SqlClient;

namespace IntegradorMarcas.Infrastructure.Repositories;

public sealed class AdminActionAuditRepository : IAdminActionAuditRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AdminActionAuditRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task LogActionAsync(AdminActionAuditEntry entry, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        await connection.ExecuteAsync(new CommandDefinition(
            AdminActionAuditSql.InsertAction,
            new
            {
                entry.FechaEventoUtc,
                entry.CorrelationId,
                entry.UsuarioActorId,
                entry.RolActorCodigo,
                entry.EntidadObjetivo,
                entry.EntidadObjetivoId,
                entry.Accion,
                entry.ResultadoAuditoriaId,
                entry.Descripcion,
                entry.ValoresAnteriores,
                entry.ValoresNuevos,
                entry.Metadata
            },
            cancellationToken: cancellationToken));
    }
}
