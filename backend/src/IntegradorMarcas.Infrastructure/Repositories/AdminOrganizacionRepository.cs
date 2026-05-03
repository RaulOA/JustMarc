using Dapper;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Queries;
using Microsoft.Data.SqlClient;

namespace IntegradorMarcas.Infrastructure.Repositories;

public sealed class AdminOrganizacionRepository : IAdminOrganizacionRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public AdminOrganizacionRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IReadOnlyList<AdminDependenciaDto>> ListDependenciasAsync(int? estadoRegistroId, string? search, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<AdminDependenciaDto>(new CommandDefinition(
            AdminOrganizacionSql.ListDependencias,
            new
            {
                EstadoRegistroID = estadoRegistroId,
                Search = search,
                SearchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%"
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<AdminDependenciaDto?> GetDependenciaByIdAsync(int estructuraOrganizacionalId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<AdminDependenciaDto>(new CommandDefinition(
            AdminOrganizacionSql.GetDependenciaById,
            new
            {
                EstructuraOrganizacionalID = estructuraOrganizacionalId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<int> UpdateDependenciaAsync(int estructuraOrganizacionalId, UpdateDependenciaDto request, int actorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminOrganizacionSql.UpdateDependencia,
            new
            {
                EstructuraOrganizacionalID = estructuraOrganizacionalId,
                Nombre = request.Nombre.Trim(),
                EstructuraPadreID = request.EstructuraPadreId,
                EstadoRegistroID = request.EstadoRegistroId,
                ModificadoPor = actorUsuarioId.ToString()
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsDependenciaAsync(int estructuraOrganizacionalId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminOrganizacionSql.ExistsDependencia,
            new
            {
                EstructuraOrganizacionalID = estructuraOrganizacionalId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> WouldCreateDependenciaCycleAsync(int estructuraOrganizacionalId, int? estructuraPadreId, CancellationToken cancellationToken)
    {
        if (!estructuraPadreId.HasValue)
        {
            return false;
        }

        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminOrganizacionSql.WouldCreateDependenciaCycle,
            new
            {
                EstructuraOrganizacionalID = estructuraOrganizacionalId,
                EstructuraPadreID = estructuraPadreId.Value
            },
            cancellationToken: cancellationToken));
    }

    public async Task<IReadOnlyList<AdminUsuarioAsignacionDto>> ListUsuariosAsync(int? rolId, int? unidadId, int? jefaturaId, bool? esActivo, string? search, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<AdminUsuarioAsignacionDto>(new CommandDefinition(
            AdminOrganizacionSql.ListUsuarios,
            new
            {
                RolID = rolId,
                UnidadID = unidadId,
                JefaturaID = jefaturaId,
                EsActivo = esActivo,
                Search = search,
                SearchLike = string.IsNullOrWhiteSpace(search) ? null : $"%{search.Trim()}%"
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<AdminUsuarioAsignacionDto?> GetUsuarioAsignacionByIdAsync(int usuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<AdminUsuarioAsignacionDto>(new CommandDefinition(
            AdminOrganizacionSql.GetUsuarioById,
            new
            {
                UsuarioID = usuarioId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<int> UpdateUsuarioAsignacionAsync(int usuarioId, UpdateUsuarioAsignacionDto request, int actorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminOrganizacionSql.UpdateUsuarioAsignacion,
            new
            {
                UsuarioID = usuarioId,
                RolID = request.RolId,
                UnidadID = request.UnidadId,
                JefaturaID = request.JefaturaId,
                ModificadoPor = actorUsuarioId.ToString()
            },
            cancellationToken: cancellationToken));
    }

    public async Task<int> UpdateUsuarioEstadoAsync(int usuarioId, bool esActivo, int actorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminOrganizacionSql.UpdateUsuarioEstado,
            new
            {
                UsuarioID = usuarioId,
                EsActivo = esActivo,
                ModificadoPor = actorUsuarioId.ToString()
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminOrganizacionSql.ExistsUsuario,
            new
            {
                UsuarioID = usuarioId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsRolAsync(int rolId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminOrganizacionSql.ExistsRol,
            new
            {
                RolID = rolId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsUnidadAsync(int unidadId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminOrganizacionSql.ExistsUnidad,
            new
            {
                UnidadID = unidadId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsJefaturaValidaAsync(int usuarioId, int jefaturaId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminOrganizacionSql.ExistsJefaturaValida,
            new
            {
                UsuarioID = usuarioId,
                JefaturaID = jefaturaId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsDirectJefaturaCycleAsync(int usuarioId, int jefaturaId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminOrganizacionSql.ExistsDirectJefaturaCycle,
            new
            {
                UsuarioID = usuarioId,
                JefaturaID = jefaturaId
            },
            cancellationToken: cancellationToken));
    }
}
