using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Application.Services;
using IntegradorMarcas.Domain.Constants;

namespace IntegradorMarcas.Tests;

public sealed class JustificacionServiceHistoricoTests
{
    [Fact]
    public async Task ListHistoricoAsync_RolFuncionario_FuerzaScopePropioEIgnoraFiltroFuncionario()
    {
        var repository = new FakeJustificacionRepository();
        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo
        {
            UserId = 4,
            Role = RolesSistema.RolFunc
        };

        await service.ListHistoricoAsync(user, new FiltroRrhhJustificacionesDto
        {
            Funcionario = "Carlos",
            FechaDesde = new DateTime(2026, 1, 1),
            FechaHasta = new DateTime(2026, 1, 31)
        }, CancellationToken.None);

        Assert.Equal(4, repository.LastHistoricoUsuarioId);
        Assert.Null(repository.LastHistoricoAprobadorUsuarioId);
        Assert.False(repository.LastHistoricoExcluirPropiosEnScopeAprobador);
        Assert.NotNull(repository.LastHistoricoFiltros);
        Assert.Null(repository.LastHistoricoFiltros!.Funcionario);
    }

    [Fact]
    public async Task ListHistoricoAsync_RolRrhh_MantieneFiltroFuncionarioSinScopeUsuario()
    {
        var repository = new FakeJustificacionRepository();
        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo
        {
            UserId = 6,
            Role = RolesSistema.RolRrhh
        };

        await service.ListHistoricoAsync(user, new FiltroRrhhJustificacionesDto
        {
            Funcionario = "Ana",
            Compania = "CNP"
        }, CancellationToken.None);

        Assert.Null(repository.LastHistoricoUsuarioId);
        Assert.Null(repository.LastHistoricoAprobadorUsuarioId);
        Assert.False(repository.LastHistoricoExcluirPropiosEnScopeAprobador);
        Assert.NotNull(repository.LastHistoricoFiltros);
        Assert.Equal("Ana", repository.LastHistoricoFiltros!.Funcionario);
    }

    [Fact]
    public async Task ListHistoricoAsync_RolJefatura_AplicaScopeAprobadorYExcluyePropios()
    {
        var repository = new FakeJustificacionRepository();
        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo
        {
            UserId = 3,
            Role = RolesSistema.RolJefe
        };

        await service.ListHistoricoAsync(user, new FiltroRrhhJustificacionesDto
        {
            Funcionario = "Luis",
            Compania = "CNP"
        }, CancellationToken.None);

        Assert.Null(repository.LastHistoricoUsuarioId);
        Assert.Equal(3, repository.LastHistoricoAprobadorUsuarioId);
        Assert.True(repository.LastHistoricoExcluirPropiosEnScopeAprobador);
        Assert.NotNull(repository.LastHistoricoFiltros);
        Assert.Equal("Luis", repository.LastHistoricoFiltros!.Funcionario);
    }

    private sealed class FakeAuditEventRepository : IAuditEventRepository
    {
        public Task LogEventAsync(AuditEventEntry entry, CancellationToken cancellationToken)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class FakeJustificacionRepository : IJustificacionRepository
    {
        public int? LastHistoricoUsuarioId { get; private set; }
        public int? LastHistoricoAprobadorUsuarioId { get; private set; }
        public bool LastHistoricoExcluirPropiosEnScopeAprobador { get; private set; }
        public FiltroRrhhJustificacionesDto? LastHistoricoFiltros { get; private set; }

        public Task<IReadOnlyCollection<int>> GetExistingTipoJustificacionIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> CreateAsync(int usuarioId, CreateJustificacionDto request, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<CurrentApproverDto> GetCurrentApproverAsync(int solicitanteUsuarioId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<JustificacionResumenDto>> ListMineAsync(int usuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<JustificacionDetalleLineaDto>> ListMineLineasAsync(int usuarioId, int justificacionId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(int aprobadorUsuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(FiltroRrhhJustificacionesDto filtros, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListHistoricoAsync(
            int? usuarioId,
            int? aprobadorUsuarioId,
            bool excluirPropiosEnScopeAprobador,
            FiltroRrhhJustificacionesDto filtros,
            CancellationToken cancellationToken)
        {
            LastHistoricoUsuarioId = usuarioId;
            LastHistoricoAprobadorUsuarioId = aprobadorUsuarioId;
            LastHistoricoExcluirPropiosEnScopeAprobador = excluirPropiosEnScopeAprobador;
            LastHistoricoFiltros = filtros;
            return Task.FromResult<IReadOnlyList<RrhhJustificacionResumenDto>>(Array.Empty<RrhhJustificacionResumenDto>());
        }

        public Task<JustificacionCompletaDto?> GetDetalleJefaturaAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<AprobacionScopeValidationDto> GetAprobacionScopeValidationAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<ResolverValidationDto> GetResolverValidationAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> ResolverAsync(int justificacionId, int aprobadorUsuarioId, int estadoId, string? comentario, string? rolResolucion, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        // F-004 T13 R15
        public Task<RevisarTitularValidationDto> GetRevisarTitularValidationAsync(int justificacionId, int titularUsuarioId, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public Task<int> RevisarTitularAsync(int justificacionId, int titularUsuarioId, int estadoId, string? comentario, string? rolResolucion, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }
    }
}