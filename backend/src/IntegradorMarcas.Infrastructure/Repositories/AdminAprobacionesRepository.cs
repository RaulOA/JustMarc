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

    public async Task<AdminJerarquiaDto?> GetJerarquiaByIdAsync(int jerarquiaAprobacionId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<AdminJerarquiaDto>(new CommandDefinition(
            AdminAprobacionesSql.GetJerarquiaById,
            new
            {
                JerarquiaAprobacionID = jerarquiaAprobacionId
            },
            cancellationToken: cancellationToken));
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
                CreadoPor = actorUsuarioId.ToString()
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

    public async Task<int> UpdateJerarquiaAsync(int jerarquiaAprobacionId, UpdateJerarquiaDto request, int actorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminAprobacionesSql.UpdateJerarquia,
            new
            {
                JerarquiaAprobacionID = jerarquiaAprobacionId,
                AprobadorUsuarioID = request.AprobadorUsuarioId,
                EstructuraOrganizacionalID = request.EstructuraOrganizacionalId,
                request.NivelAprobacion,
                TipoRelacion = request.TipoRelacion.Trim(),
                EstadoRegistroID = request.EstadoRegistroId,
                request.VigenciaDesde,
                request.VigenciaHasta,
                ModificadoPor = actorUsuarioId.ToString()
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

    public async Task<AdminDelegacionDto?> GetDelegacionByIdAsync(int delegacionAprobacionId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.QuerySingleOrDefaultAsync<AdminDelegacionDto>(new CommandDefinition(
            AdminAprobacionesSql.GetDelegacionById,
            new
            {
                DelegacionAprobacionID = delegacionAprobacionId
            },
            cancellationToken: cancellationToken));
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
                CreadoPor = actorUsuarioId.ToString()
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

    public async Task<int> UpdateDelegacionAsync(int delegacionAprobacionId, UpdateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminAprobacionesSql.UpdateDelegacion,
            new
            {
                DelegacionAprobacionID = delegacionAprobacionId,
                DeleganteUsuarioID = request.DeleganteUsuarioId,
                DelegadoUsuarioID = request.DelegadoUsuarioId,
                JerarquiaAprobacionID = request.JerarquiaAprobacionId,
                Motivo = string.IsNullOrWhiteSpace(request.Motivo) ? null : request.Motivo.Trim(),
                EstadoRegistroID = request.EstadoRegistroId,
                request.VigenciaDesde,
                request.VigenciaHasta,
                ModificadoPor = actorUsuarioId.ToString()
            },
            cancellationToken: cancellationToken));
    }

    public async Task<int> ToggleDelegacionEstadoAsync(int delegacionAprobacionId, int estadoRegistroId, int actorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminAprobacionesSql.ToggleDelegacionEstado,
            new
            {
                DelegacionAprobacionID = delegacionAprobacionId,
                EstadoRegistroID = estadoRegistroId,
                ModificadoPor = actorUsuarioId.ToString()
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminAprobacionesSql.ExistsUsuario,
            new
            {
                UsuarioID = usuarioId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsEstructuraAsync(int estructuraOrganizacionalId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminAprobacionesSql.ExistsEstructura,
            new
            {
                EstructuraOrganizacionalID = estructuraOrganizacionalId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsJerarquiaAsync(int jerarquiaAprobacionId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminAprobacionesSql.ExistsJerarquia,
            new
            {
                JerarquiaAprobacionID = jerarquiaAprobacionId
            },
            cancellationToken: cancellationToken));
    }

    public async Task<bool> ExistsJerarquiaActivaDuplicadaAsync(int aprobadorUsuarioId, int estructuraOrganizacionalId, int nivelAprobacion, int? jerarquiaAprobacionIdExcluida, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminAprobacionesSql.ExistsJerarquiaActivaDuplicada,
            new
            {
                AprobadorUsuarioID = aprobadorUsuarioId,
                EstructuraOrganizacionalID = estructuraOrganizacionalId,
                NivelAprobacion = nivelAprobacion,
                JerarquiaAprobacionIDExcluida = jerarquiaAprobacionIdExcluida
            },
            cancellationToken: cancellationToken));
    }

    // F-004 T4 R6: anti-sub-delegacion
    public async Task<bool> ExistsDelegacionActivaComoDelegadoAsync(int usuarioId, DateTime fechaRef, int? delegacionIdExcluida, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteScalarAsync<bool>(new CommandDefinition(
            AdminAprobacionesSql.ExistsDelegacionActivaComoDelegado,
            new
            {
                UsuarioID = usuarioId,
                FechaRef = fechaRef,
                DelegacionIDExcluida = delegacionIdExcluida
            },
            cancellationToken: cancellationToken));
    }

    // F-004 T14 R19: borrado fisico con auditoria previa en servicio (D1)
    public async Task<int> DeleteDelegacionAsync(int delegacionAprobacionId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            AdminAprobacionesSql.DeleteDelegacion,
            new
            {
                DelegacionAprobacionID = delegacionAprobacionId
            },
            cancellationToken: cancellationToken));
    }
}
