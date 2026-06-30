using Dapper;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Queries;
using Microsoft.Data.SqlClient;

namespace IntegradorMarcas.Infrastructure.Repositories;

/// <summary>
/// F-004 D3: repositorio dedicado para las vistas de consulta del delegado.
/// </summary>
public sealed class DelegacionConsultaRepository : IDelegacionConsultaRepository
{
    private readonly ISqlConnectionFactory _connectionFactory;

    public DelegacionConsultaRepository(ISqlConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    // R11/R12
    public async Task<IReadOnlyList<DelegacionFuncionDto>> GetMiFuncionAsync(int delegadoUsuarioId, DateTime fechaRef, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<DelegacionFuncionDto>(new CommandDefinition(
            DelegacionConsultaSql.MiFuncion,
            new
            {
                DelegadoUsuarioID = delegadoUsuarioId,
                FechaRef = fechaRef
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    // R16/R17
    public async Task<IReadOnlyList<DelegacionRegistroDto>> GetMiRegistroAsync(int delegadoUsuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<DelegacionRegistroDto>(new CommandDefinition(
            DelegacionConsultaSql.MiRegistro,
            new
            {
                DelegadoUsuarioID = delegadoUsuarioId,
                filtros.Desde,
                filtros.Hasta
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }
}
