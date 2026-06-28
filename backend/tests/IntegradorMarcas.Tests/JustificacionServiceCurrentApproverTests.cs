using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Application.Services;
using IntegradorMarcas.Domain.Constants;

namespace IntegradorMarcas.Tests;

public sealed class JustificacionServiceCurrentApproverTests
{
    [Fact]
    public async Task GetCurrentApproverAsync_RolFuncionario_RetornaDatoDesdeRepositorio()
    {
        var repository = new FakeJustificacionRepository
        {
            CurrentApproverToReturn = new CurrentApproverDto
            {
                SolicitanteUsuarioId = 4,
                Origen = "Jerarquia",
                Aprobador = new UsuarioResumenDto
                {
                    UsuarioId = 3,
                    NombreCompleto = "Maria Jefe"
                }
            }
        };

        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo
        {
            UserId = 4,
            Role = RolesSistema.RolFunc
        };

        var result = await service.GetCurrentApproverAsync(user, CancellationToken.None);

        Assert.Equal(4, repository.LastSolicitanteUsuarioId);
        Assert.Equal("Jerarquia", result.Origen);
        Assert.Equal(3, result.Aprobador?.UsuarioId);
    }

    [Fact]
    public async Task GetCurrentApproverAsync_RolJefatura_Aceptado()
    {
        var repository = new FakeJustificacionRepository
        {
            CurrentApproverToReturn = new CurrentApproverDto
            {
                SolicitanteUsuarioId = 3,
                Origen = "Delegacion",
                DeleganteUsuarioId = 8,
                DeleganteNombre = "Carlos Delegante",
                Aprobador = new UsuarioResumenDto
                {
                    UsuarioId = 10,
                    NombreCompleto = "Laura Delegada"
                }
            }
        };

        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo
        {
            UserId = 3,
            Role = RolesSistema.RolJefe
        };

        var result = await service.GetCurrentApproverAsync(user, CancellationToken.None);

        Assert.Equal(3, repository.LastSolicitanteUsuarioId);
        Assert.Equal("Delegacion", result.Origen);
        Assert.Equal(8, result.DeleganteUsuarioId);
        Assert.Equal("Carlos Delegante", result.DeleganteNombre);
    }

    [Fact]
    public async Task GetCurrentApproverAsync_RolNoPermitido_Lanza403()
    {
        var repository = new FakeJustificacionRepository();
        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo
        {
            UserId = 99,
            Role = "ROL_INVITADO"
        };

        var exception = await Assert.ThrowsAsync<AppException>(() => service.GetCurrentApproverAsync(user, CancellationToken.None));

        Assert.Equal(403, exception.StatusCode);
        Assert.Null(repository.LastSolicitanteUsuarioId);
    }

    /// <summary>
    /// R1: Solicitante sin jerarquia ni delegacion vigente -> aprobador nulo, sin excepcion.
    /// </summary>
    [Fact]
    public async Task GetCurrentApproverAsync_SinAprobadorVigente_RetornaAprobadorNuloSinExcepcion()
    {
        // R1
        var repository = new FakeJustificacionRepository
        {
            CurrentApproverToReturn = new CurrentApproverDto
            {
                SolicitanteUsuarioId = 5,
                Aprobador = null,
                Origen = null
            }
        };
        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 5, Role = RolesSistema.RolFunc };

        var result = await service.GetCurrentApproverAsync(user, CancellationToken.None);

        Assert.Null(result.Aprobador);
        Assert.Equal(5, result.SolicitanteUsuarioId);
    }

    /// <summary>
    /// R3: Cuando coexisten jerarquia y delegacion vigente, el resultado con Origen='Delegacion' es el efectivo.
    /// </summary>
    [Fact]
    public async Task GetCurrentApproverAsync_DelegacionCoexisteConJerarquia_SeleccionaDelegacion()
    {
        // R3: El repositorio (TVF via OUTER APPLY con ORDER BY Delegacion primero) ya devuelve la delegacion.
        // El servicio no filtra; confirma que el dato del repositorio se retorna sin alteracion.
        var repository = new FakeJustificacionRepository
        {
            CurrentApproverToReturn = new CurrentApproverDto
            {
                SolicitanteUsuarioId = 7,
                Origen = "Delegacion",
                DeleganteUsuarioId = 8,
                DeleganteNombre = "Ana Delegante",
                Aprobador = new UsuarioResumenDto { UsuarioId = 15, NombreCompleto = "Pedro Delegado" }
            }
        };
        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 7, Role = RolesSistema.RolFunc };

        var result = await service.GetCurrentApproverAsync(user, CancellationToken.None);

        Assert.Equal("Delegacion", result.Origen);
        Assert.Equal(15, result.Aprobador?.UsuarioId);
        Assert.Equal(8, result.DeleganteUsuarioId);
    }

    /// <summary>
    /// R6: ROL_JEFE sin aprobador vigente resuelto -> CurrentApproverDto con aprobador nulo, sin excepcion.
    /// </summary>
    [Fact]
    public async Task GetCurrentApproverAsync_RolJefeSinAprobadorVigente_RetornaAprobadorNuloSinExcepcion()
    {
        // R6
        var repository = new FakeJustificacionRepository
        {
            CurrentApproverToReturn = new CurrentApproverDto
            {
                SolicitanteUsuarioId = 3,
                Aprobador = null,
                Origen = null
            }
        };
        var service = new JustificacionService(repository, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 3, Role = RolesSistema.RolJefe };

        var result = await service.GetCurrentApproverAsync(user, CancellationToken.None);

        Assert.Null(result.Aprobador);
        Assert.Null(result.Origen);
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
        public int? LastSolicitanteUsuarioId { get; private set; }
        public CurrentApproverDto CurrentApproverToReturn { get; set; } = new();

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
            LastSolicitanteUsuarioId = solicitanteUsuarioId;
            return Task.FromResult(CurrentApproverToReturn);
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
            throw new NotImplementedException();
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
    }
}
