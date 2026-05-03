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

    public async Task<IReadOnlyCollection<int>> GetExistingTipoJustificacionIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
    {
        var distinctIds = ids.Distinct().ToArray();
        if (distinctIds.Length == 0)
        {
            return Array.Empty<int>();
        }

        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<int>(new CommandDefinition(
            JustificacionesSql.GetExistingTipoJustificacionIds,
            new
            {
                Ids = distinctIds
            },
            cancellationToken: cancellationToken));

        return data.ToArray();
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

    public async Task<CurrentApproverDto> GetCurrentApproverAsync(int solicitanteUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QuerySingleAsync<CurrentApproverRow>(new CommandDefinition(
            JustificacionesSql.GetCurrentApproverBySolicitante,
            new
            {
                SolicitanteUsuarioID = solicitanteUsuarioId
            },
            cancellationToken: cancellationToken));

        return new CurrentApproverDto
        {
            SolicitanteUsuarioId = data.SolicitanteUsuarioID,
            Origen = data.Origen,
            DeleganteUsuarioId = data.DeleganteUsuarioID,
            DeleganteNombre = data.DeleganteNombre,
            Aprobador = data.AprobadorUsuarioID.HasValue
                ? new UsuarioResumenDto
                {
                    UsuarioId = data.AprobadorUsuarioID.Value,
                    NombreCompleto = data.AprobadorNombreCompleto ?? string.Empty,
                    Cedula = data.AprobadorCedula ?? string.Empty,
                    Correo = data.AprobadorCorreo ?? string.Empty,
                    Compania = data.AprobadorCompania ?? string.Empty,
                    UnidadId = data.AprobadorUnidadID ?? 0,
                    UnidadNombre = data.AprobadorUnidadNombre ?? string.Empty,
                    JefaturaId = data.AprobadorJefaturaID
                }
                : null
        };
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

    public async Task<IReadOnlyList<JustificacionDetalleLineaDto>> ListMineLineasAsync(int usuarioId, int justificacionId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<JustificacionDetalleLineaDto>(new CommandDefinition(
            JustificacionesSql.ListMineLineas,
            new
            {
                UsuarioID = usuarioId,
                JustificacionID = justificacionId
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(int aprobadorUsuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<JustificacionResumenDto>(new CommandDefinition(
            JustificacionesSql.ListPendientesJefatura,
            new
            {
                AprobadorUsuarioID = aprobadorUsuarioId,
                EstadoPendiente = EstadoIds.PendienteJefatura,
                filtros.Desde,
                filtros.Hasta
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(FiltroRrhhJustificacionesDto filtros, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<RrhhJustificacionResumenDto>(new CommandDefinition(
            JustificacionesSql.ListRrhhGlobal,
            new
            {
                filtros.EstadoId,
                filtros.Compania,
                Funcionario = string.IsNullOrWhiteSpace(filtros.Funcionario) ? null : filtros.Funcionario.Trim(),
                filtros.FechaDesde,
                filtros.FechaHasta
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListHistoricoAsync(
        int? usuarioId,
        int? aprobadorUsuarioId,
        bool excluirPropiosEnScopeAprobador,
        FiltroRrhhJustificacionesDto filtros,
        CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QueryAsync<RrhhJustificacionResumenDto>(new CommandDefinition(
            JustificacionesSql.ListHistorico,
            new
            {
                UsuarioID = usuarioId,
                AprobadorUsuarioID = aprobadorUsuarioId,
                ExcluirPropiosEnScopeAprobador = excluirPropiosEnScopeAprobador,
                filtros.EstadoId,
                filtros.Compania,
                Funcionario = string.IsNullOrWhiteSpace(filtros.Funcionario) ? null : filtros.Funcionario.Trim(),
                filtros.FechaDesde,
                filtros.FechaHasta
            },
            cancellationToken: cancellationToken));

        return data.ToList();
    }

    public async Task<JustificacionCompletaDto?> GetDetalleJefaturaAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var encabezado = await connection.QuerySingleOrDefaultAsync<JustificacionDetalleJefaturaRow>(new CommandDefinition(
            JustificacionesSql.GetDetalleJefaturaEncabezado,
            new
            {
                JustificacionID = justificacionId,
                AprobadorUsuarioID = aprobadorUsuarioId
            },
            cancellationToken: cancellationToken));

        if (encabezado is null)
        {
            return null;
        }

        var lineas = await connection.QueryAsync<JustificacionDetalleLineaDto>(new CommandDefinition(
            JustificacionesSql.GetDetalleJefaturaLineas,
            new
            {
                JustificacionID = justificacionId
            },
            cancellationToken: cancellationToken));

        return new JustificacionCompletaDto
        {
            Encabezado = new JustificacionResumenDto
            {
                JustificacionId = encabezado.JustificacionID,
                MotivoGeneral = encabezado.MotivoGeneral,
                ComentarioResolucion = encabezado.ComentarioResolucion,
                EstadoId = encabezado.EstadoID,
                EstadoDescripcion = encabezado.EstadoDescripcion,
                FechaCreacion = encabezado.FechaCreacion,
                AprobadorId = encabezado.AprobadorID,
                FechaAprobacion = encabezado.FechaAprobacion
            },
            Solicitante = new UsuarioResumenDto
            {
                UsuarioId = encabezado.SolicitanteUsuarioID,
                NombreCompleto = encabezado.SolicitanteNombreCompleto,
                Cedula = encabezado.SolicitanteCedula,
                Correo = encabezado.SolicitanteCorreo,
                Compania = encabezado.SolicitanteCompania,
                UnidadId = encabezado.SolicitanteUnidadID,
                JefaturaId = encabezado.SolicitanteJefaturaID,
                UnidadNombre = encabezado.SolicitanteUnidadNombre ?? string.Empty
            },
            Aprobador = encabezado.AprobadorUsuarioID.HasValue
                ? new UsuarioResumenDto
                {
                    UsuarioId = encabezado.AprobadorUsuarioID.Value,
                    NombreCompleto = encabezado.AprobadorNombreCompleto ?? string.Empty,
                    Cedula = encabezado.AprobadorCedula ?? string.Empty,
                    Correo = encabezado.AprobadorCorreo ?? string.Empty,
                    Compania = encabezado.AprobadorCompania ?? string.Empty,
                    UnidadId = encabezado.AprobadorUnidadID ?? 0,
                    JefaturaId = encabezado.AprobadorJefaturaID
                }
                : null,
            Detalles = lineas.ToList()
        };
    }

    public async Task<AprobacionScopeValidationDto> GetAprobacionScopeValidationAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QuerySingleAsync<AprobacionScopeValidationDto>(new CommandDefinition(
            JustificacionesSql.GetAprobacionScopeValidation,
            new
            {
                JustificacionID = justificacionId,
                AprobadorUsuarioID = aprobadorUsuarioId
            },
            cancellationToken: cancellationToken));

        return data;
    }

    public async Task<ResolverValidationDto> GetResolverValidationAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        var data = await connection.QuerySingleAsync<ResolverValidationDto>(new CommandDefinition(
            JustificacionesSql.GetResolverValidation,
            new
            {
                JustificacionID = justificacionId,
                AprobadorUsuarioID = aprobadorUsuarioId
            },
            cancellationToken: cancellationToken));

        return data;
    }

    public async Task<int> ResolverAsync(int justificacionId, int aprobadorUsuarioId, int estadoId, string? comentario, string? rolResolucion, CancellationToken cancellationToken)
    {
        await using var connection = (SqlConnection)_connectionFactory.CreateConnection();

        return await connection.ExecuteAsync(new CommandDefinition(
            JustificacionesSql.ResolverPendiente,
            new
            {
                JustificacionID = justificacionId,
                AprobadorUsuarioID = aprobadorUsuarioId,
                EstadoID = estadoId,
                EstadoPendiente = EstadoIds.PendienteJefatura,
                Comentario = comentario,
                RolResolucion = string.IsNullOrWhiteSpace(rolResolucion) ? null : rolResolucion.Trim()
            },
            cancellationToken: cancellationToken));
    }

    private sealed class JustificacionDetalleJefaturaRow
    {
        public int JustificacionID { get; set; }
        public string MotivoGeneral { get; set; } = string.Empty;
        public string? ComentarioResolucion { get; set; }
        public int EstadoID { get; set; }
        public string EstadoDescripcion { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public int? AprobadorID { get; set; }
        public DateTime? FechaAprobacion { get; set; }
        public int SolicitanteUsuarioID { get; set; }
        public string SolicitanteNombreCompleto { get; set; } = string.Empty;
        public string SolicitanteCedula { get; set; } = string.Empty;
        public string SolicitanteCorreo { get; set; } = string.Empty;
        public string SolicitanteCompania { get; set; } = string.Empty;
        public int SolicitanteUnidadID { get; set; }
        public int? SolicitanteJefaturaID { get; set; }
        public string? SolicitanteUnidadNombre { get; set; }
        public int? AprobadorUsuarioID { get; set; }
        public string? AprobadorNombreCompleto { get; set; }
        public string? AprobadorCedula { get; set; }
        public string? AprobadorCorreo { get; set; }
        public string? AprobadorCompania { get; set; }
        public int? AprobadorUnidadID { get; set; }
        public int? AprobadorJefaturaID { get; set; }
    }

    private sealed class CurrentApproverRow
    {
        public int SolicitanteUsuarioID { get; set; }
        public string? Origen { get; set; }
        public int? DeleganteUsuarioID { get; set; }
        public string? DeleganteNombre { get; set; }
        public int? AprobadorUsuarioID { get; set; }
        public string? AprobadorNombreCompleto { get; set; }
        public string? AprobadorCedula { get; set; }
        public string? AprobadorCorreo { get; set; }
        public string? AprobadorCompania { get; set; }
        public int? AprobadorUnidadID { get; set; }
        public string? AprobadorUnidadNombre { get; set; }
        public int? AprobadorJefaturaID { get; set; }
    }
}
