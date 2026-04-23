using Dapper;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Queries;
using Microsoft.Data.SqlClient;

namespace IntegradorMarcas.Infrastructure.Repositories;

public sealed class AdminAprobacionesRepository : IAdminAprobacionesRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AdminAprobacionesRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AdminJerarquiaDto>> ListJerarquiasAsync(int? aprobadorUsuarioId, int? estadoRegistroId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<AdminJerarquiaDto>(new CommandDefinition(
            AdminAprobacionesSql.ListJerarquias,
            new
            {
                AprobadorUsuarioID = aprobadorUsuarioId,
                EstadoRegistroID = estadoRegistroId
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<AdminJerarquiaDto> CreateJerarquiaAsync(CreateJerarquiaDto request, int actorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var jerarquiaAprobacionId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            AdminAprobacionesSql.CreateJerarquia,
            new
            {
                AprobadorUsuarioID = request.AprobadorUsuarioId,
                EstructuraOrganizacionalID = request.EstructuraOrganizacionalId,
                request.NivelAprobacion,
                TipoRelacion = request.TipoRelacion.Trim(),
                request.VigenciaDesde,
                request.VigenciaHasta,
                UsrRegistro = actorUsuarioId.ToString()
            },
            cancellationToken: cancellationToken));

        return await connection.QuerySingleAsync<AdminJerarquiaDto>(new CommandDefinition(
            AdminAprobacionesSql.GetJerarquiaById,
            new
            {
                JerarquiaAprobacionID = jerarquiaAprobacionId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<int> ToggleJerarquiaEstadoAsync(int jerarquiaAprobacionId, int estadoRegistroId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminAprobacionesSql.ToggleJerarquiaEstado,
            new
            {
                JerarquiaAprobacionID = jerarquiaAprobacionId,
                EstadoRegistroID = estadoRegistroId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AdminDelegacionDto>> ListDelegacionesAsync(int? deleganteUsuarioId, int? delegadoUsuarioId, int? estadoRegistroId, DateTime? vigenteEnFecha, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<AdminDelegacionDto>(new CommandDefinition(
            AdminAprobacionesSql.ListDelegaciones,
            new
            {
                DeleganteUsuarioID = deleganteUsuarioId,
                DelegadoUsuarioID = delegadoUsuarioId,
                EstadoRegistroID = estadoRegistroId,
                VigenteEnFecha = vigenteEnFecha
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<AdminDelegacionDto> CreateDelegacionAsync(CreateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var delegacionAprobacionId = await connection.ExecuteScalarAsync<int>(new CommandDefinition(
            AdminAprobacionesSql.CreateDelegacion,
            new
            {
                DeleganteUsuarioID = request.DeleganteUsuarioId,
                DelegadoUsuarioID = request.DelegadoUsuarioId,
                JerarquiaAprobacionID = request.JerarquiaAprobacionId,
                Motivo = string.IsNullOrWhiteSpace(request.Motivo) ? null : request.Motivo.Trim(),
                request.VigenciaDesde,
                request.VigenciaHasta,
                UsrRegistro = actorUsuarioId.ToString()
            },
            cancellationToken: cancellationToken));

        return await connection.QuerySingleAsync<AdminDelegacionDto>(new CommandDefinition(
            AdminAprobacionesSql.GetDelegacionById,
            new
            {
                DelegacionAprobacionID = delegacionAprobacionId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<int> ToggleDelegacionEstadoAsync(int delegacionAprobacionId, int estadoRegistroId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminAprobacionesSql.ToggleDelegacionEstado,
            new
            {
                DelegacionAprobacionID = delegacionAprobacionId,
                EstadoRegistroID = estadoRegistroId
            },
            cancellationToken: cancellationToken));
    }
}
