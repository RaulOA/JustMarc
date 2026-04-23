using Dapper;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Domain.Constants;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Queries;
using Microsoft.Data.SqlClient;
using System.Data;

namespace IntegradorMarcas.Infrastructure.Repositories;

public sealed class JustificacionRepository : IJustificacionRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public JustificacionRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> CreateAsync(int usuarioId, CreateJustificacionDto request, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();
        await connection.OpenAsync(cancellationToken);
        await using var transaction = await connection.BeginTransactionAsync(cancellationToken);

        try
        {
            var justificacionId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
                JustificacionesSql.InsertEncabezado,
                new
                {
                    UsuarioID = usuarioId,
                    request.MotivoGeneral,
                    EstadoID = EstadoIds.PendienteJefatura,
                    UsrRegistro = usuarioId.ToString()
                },
                transaction,
                cancellationToken: cancellationToken));

            foreach (var detalle in request.Detalles)
            {
                await connection.ExecuteAsync(new CommandDefinition(
                    JustificacionesSql.InsertDetalle,
                    new
                    {
                        JustificacionID = justificacionId,
                        TipoJustificacionID = detalle.TipoJustificacionId,
                        FechaMarca = detalle.FechaMarca.Date,
                        detalle.ObservacionDetalle,
                        UsrRegistro = usuarioId.ToString()
                    },
                    transaction,
                    cancellationToken: cancellationToken));
            }

            await transaction.CommitAsync(cancellationToken);
            return justificacionId;
        }
        catch
        {
            await transaction.RollbackAsync(cancellationToken);
            throw;
        }
    }

    public async Task<IReadOnlyList<JustificacionResumenDto>> ListMineAsync(int usuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<JustificacionResumenDto>(new CommandDefinition(
            JustificacionesSql.ListMine,
            new
            {
                UsuarioID = usuarioId,
                filtros.EstadoId,
                filtros.Desde,
                filtros.Hasta
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(int jefaturaId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<JustificacionResumenDto>(new CommandDefinition(
            JustificacionesSql.ListPendientesJefatura,
            new
            {
                JefaturaID = jefaturaId,
                EstadoPendiente = EstadoIds.PendienteJefatura,
                filtros.Desde,
                filtros.Hasta
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<ResolverValidationDto> GetResolverValidationAsync(int justificacionId, int jefaturaId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QuerySingleAsync<ResolverValidationDto>(new CommandDefinition(
            JustificacionesSql.GetResolverValidation,
            new
            {
                JustificacionID = justificacionId,
                JefaturaID = jefaturaId
            },
            cancellationToken: cancellationToken));

        return data;
    }

    public async Task<int> ResolverAsync(int justificacionId, int jefaturaId, int estadoId, string? comentario, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            JustificacionesSql.ResolverPendiente,
            new
            {
                JustificacionID = justificacionId,
                JefaturaID = jefaturaId,
                EstadoID = estadoId,
                EstadoPendiente = EstadoIds.PendienteJefatura,
                comentario
            },
            cancellationToken: cancellationToken));
    }
}
