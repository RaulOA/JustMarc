using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Application.Services;
using IntegradorMarcas.Domain.Constants;

namespace IntegradorMarcas.Tests;

/// <summary>
/// Tests unitarios para AdminAprobacionesService — validaciones avanzadas de jerarquia (R7-R14).
/// Todos los repositorios son fakes en memoria.
/// </summary>
public sealed class AdminAprobacionesServiceJerarquiaTests
{
    // -----------------------------------------------------------------------
    // R13: EnsureAdmin — solo ROL_ADMIN puede operar
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateJerarquia_UsuarioNoAdmin_Lanza403()
    {
        // R13
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var user = new UserContextInfo { UserId = 5, Role = RolesSistema.RolJefe };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateJerarquiaAsync(user, ValidCreateJerarquia(), null, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateJerarquia_UsuarioNoAdmin_Lanza403()
    {
        // R13
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var user = new UserContextInfo { UserId = 5, Role = RolesSistema.RolFunc };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.UpdateJerarquiaAsync(user, 1, ValidUpdateJerarquia(), null, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // R7: NivelAprobacion <= 0
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateJerarquia_NivelCero_Lanza400()
    {
        // R7
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var request = new CreateJerarquiaDto
        {
            AprobadorUsuarioId = 10,
            EstructuraOrganizacionalId = 20,
            NivelAprobacion = 0,
            TipoRelacion = "Vertical",
            VigenciaDesde = new DateTime(2026, 1, 1)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateJerarquiaAsync(AdminUser(), request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task CreateJerarquia_NivelNegativo_Lanza400()
    {
        // R7
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var request = new CreateJerarquiaDto
        {
            AprobadorUsuarioId = 10,
            EstructuraOrganizacionalId = 20,
            NivelAprobacion = -3,
            TipoRelacion = "Vertical",
            VigenciaDesde = new DateTime(2026, 1, 1)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateJerarquiaAsync(AdminUser(), request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateJerarquia_NivelCero_Lanza400()
    {
        // R7
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var request = new UpdateJerarquiaDto
        {
            AprobadorUsuarioId = 10,
            EstructuraOrganizacionalId = 20,
            NivelAprobacion = 0,
            TipoRelacion = "Vertical",
            EstadoRegistroId = 1,
            VigenciaDesde = new DateTime(2026, 1, 1)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.UpdateJerarquiaAsync(AdminUser(), 1, request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // R8: TipoRelacion invalido
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateJerarquia_TipoRelacionInvalido_Lanza400()
    {
        // R8
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var request = new CreateJerarquiaDto
        {
            AprobadorUsuarioId = 10,
            EstructuraOrganizacionalId = 20,
            NivelAprobacion = 1,
            TipoRelacion = "Diagonal",
            VigenciaDesde = new DateTime(2026, 1, 1)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateJerarquiaAsync(AdminUser(), request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task CreateJerarquia_TipoRelacionVerticalMinusculas_NoLanzaExcepcion()
    {
        // R8 — "vertical" es valido (case insensitive)
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var request = new CreateJerarquiaDto
        {
            AprobadorUsuarioId = 10,
            EstructuraOrganizacionalId = 20,
            NivelAprobacion = 1,
            TipoRelacion = "vertical",
            VigenciaDesde = new DateTime(2026, 1, 1)
        };

        var result = await service.CreateJerarquiaAsync(AdminUser(), request, null, CancellationToken.None);

        Assert.NotNull(result);
    }

    [Fact]
    public async Task UpdateJerarquia_TipoRelacionInvalido_Lanza400()
    {
        // R8
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var request = new UpdateJerarquiaDto
        {
            AprobadorUsuarioId = 10,
            EstructuraOrganizacionalId = 20,
            NivelAprobacion = 1,
            TipoRelacion = "Oblicua",
            EstadoRegistroId = 1,
            VigenciaDesde = new DateTime(2026, 1, 1)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.UpdateJerarquiaAsync(AdminUser(), 1, request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // R9: VigenciaHasta < VigenciaDesde
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateJerarquia_VigenciaHastaAnteriorDesde_Lanza400()
    {
        // R9
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var request = new CreateJerarquiaDto
        {
            AprobadorUsuarioId = 10,
            EstructuraOrganizacionalId = 20,
            NivelAprobacion = 1,
            TipoRelacion = "Vertical",
            VigenciaDesde = new DateTime(2026, 6, 1),
            VigenciaHasta = new DateTime(2026, 5, 31)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateJerarquiaAsync(AdminUser(), request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateJerarquia_VigenciaHastaAnteriorDesde_Lanza400()
    {
        // R9
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildService(repo);
        var request = new UpdateJerarquiaDto
        {
            AprobadorUsuarioId = 10,
            EstructuraOrganizacionalId = 20,
            NivelAprobacion = 1,
            TipoRelacion = "Vertical",
            EstadoRegistroId = 1,
            VigenciaDesde = new DateTime(2026, 6, 1),
            VigenciaHasta = new DateTime(2026, 5, 31)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.UpdateJerarquiaAsync(AdminUser(), 1, request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // R10: AprobadorUsuarioId inexistente
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateJerarquia_AprobadorInexistente_Lanza400()
    {
        // R10
        var repo = new FakeAdminAprobacionesRepository { UsuarioExiste = false };
        var service = BuildService(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateJerarquiaAsync(AdminUser(), ValidCreateJerarquia(), null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // R11: EstructuraOrganizacionalId inexistente
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateJerarquia_EstructuraInexistente_Lanza400()
    {
        // R11
        var repo = new FakeAdminAprobacionesRepository { EstructuraExiste = false };
        var service = BuildService(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateJerarquiaAsync(AdminUser(), ValidCreateJerarquia(), null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // R12: Duplicado vigente
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateJerarquia_DuplicadoVigente_Lanza409()
    {
        // R12
        var repo = new FakeAdminAprobacionesRepository { JerarquiaActivaDuplicadaExiste = true };
        var service = BuildService(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateJerarquiaAsync(AdminUser(), ValidCreateJerarquia(), null, CancellationToken.None));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateJerarquia_DuplicadoVigente_Lanza409()
    {
        // R12
        var repo = new FakeAdminAprobacionesRepository { JerarquiaActivaDuplicadaExiste = true };
        var service = BuildService(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.UpdateJerarquiaAsync(AdminUser(), 1, ValidUpdateJerarquia(), null, CancellationToken.None));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateJerarquia_SinDuplicado_ActualizaCorrectamente()
    {
        // R12: el repositorio dice que NO hay duplicado (el propio id queda excluido).
        var repo = new FakeAdminAprobacionesRepository { JerarquiaActivaDuplicadaExiste = false };
        repo.SetJerarquiaById(1, BuildJerarquiaDto(1));
        var service = BuildService(repo);

        var result = await service.UpdateJerarquiaAsync(AdminUser(), 1, ValidUpdateJerarquia(), null, CancellationToken.None);

        Assert.NotNull(result);
    }

    // -----------------------------------------------------------------------
    // R14: auditoria invocada en alta valida
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateJerarquia_AltaValida_InvocaAuditoriaResumenYDetalle()
    {
        // R14
        var repo = new FakeAdminAprobacionesRepository();
        var auditSummary = new FakeAuditEventRepository();
        var auditDetail = new FakeAdminActionAuditRepository();
        var service = new AdminAprobacionesService(repo, auditSummary, auditDetail);

        await service.CreateJerarquiaAsync(AdminUser(), ValidCreateJerarquia(), "corr-123", CancellationToken.None);

        Assert.Equal(1, auditSummary.LogCount);
        Assert.Equal(1, auditDetail.LogCount);
    }

    [Fact]
    public async Task UpdateJerarquia_EdicionValida_InvocaAuditoriaResumenYDetalle()
    {
        // R14
        var repo = new FakeAdminAprobacionesRepository();
        repo.SetJerarquiaById(1, BuildJerarquiaDto(1));
        var auditSummary = new FakeAuditEventRepository();
        var auditDetail = new FakeAdminActionAuditRepository();
        var service = new AdminAprobacionesService(repo, auditSummary, auditDetail);

        await service.UpdateJerarquiaAsync(AdminUser(), 1, ValidUpdateJerarquia(), "corr-456", CancellationToken.None);

        Assert.Equal(1, auditSummary.LogCount);
        Assert.Equal(1, auditDetail.LogCount);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static AdminAprobacionesService BuildService(FakeAdminAprobacionesRepository repo)
        => new(repo, new FakeAuditEventRepository(), new FakeAdminActionAuditRepository());

    private static UserContextInfo AdminUser()
        => new() { UserId = 1, Role = RolesSistema.RolAdmin };

    private static CreateJerarquiaDto ValidCreateJerarquia() => new()
    {
        AprobadorUsuarioId = 10,
        EstructuraOrganizacionalId = 20,
        NivelAprobacion = 1,
        TipoRelacion = "Vertical",
        VigenciaDesde = new DateTime(2026, 1, 1),
        VigenciaHasta = new DateTime(2026, 12, 31)
    };

    private static UpdateJerarquiaDto ValidUpdateJerarquia() => new()
    {
        AprobadorUsuarioId = 10,
        EstructuraOrganizacionalId = 20,
        NivelAprobacion = 1,
        TipoRelacion = "Horizontal",
        EstadoRegistroId = 1,
        VigenciaDesde = new DateTime(2026, 1, 1),
        VigenciaHasta = new DateTime(2026, 12, 31)
    };

    private static AdminJerarquiaDto BuildJerarquiaDto(int id) => new()
    {
        JerarquiaAprobacionId = id,
        AprobadorUsuarioId = 10,
        EstructuraOrganizacionalId = 20,
        NivelAprobacion = 1,
        TipoRelacion = "Vertical",
        EstadoRegistroId = 1,
        VigenciaDesde = new DateTime(2026, 1, 1),
        VigenciaHasta = new DateTime(2026, 12, 31)
    };

    // -----------------------------------------------------------------------
    // Fakes
    // -----------------------------------------------------------------------

    private sealed class FakeAuditEventRepository : IAuditEventRepository
    {
        public int LogCount { get; private set; }

        public Task LogEventAsync(AuditEventEntry entry, CancellationToken cancellationToken)
        {
            LogCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAdminActionAuditRepository : IAdminActionAuditRepository
    {
        public int LogCount { get; private set; }

        public Task LogActionAsync(AdminActionAuditEntry entry, CancellationToken cancellationToken)
        {
            LogCount++;
            return Task.CompletedTask;
        }
    }

    private sealed class FakeAdminAprobacionesRepository : IAdminAprobacionesRepository
    {
        public bool UsuarioExiste { get; set; } = true;
        public bool EstructuraExiste { get; set; } = true;
        public bool JerarquiaActivaDuplicadaExiste { get; set; } = false;

        private readonly Dictionary<int, AdminJerarquiaDto> _jerarquias = [];

        public void SetJerarquiaById(int id, AdminJerarquiaDto dto) => _jerarquias[id] = dto;

        public Task<IReadOnlyList<AdminJerarquiaDto>> ListJerarquiasAsync(int? aprobadorUsuarioId, int? estadoRegistroId, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<AdminJerarquiaDto>>([]);

        public Task<AdminJerarquiaDto?> GetJerarquiaByIdAsync(int jerarquiaAprobacionId, CancellationToken cancellationToken)
        {
            _jerarquias.TryGetValue(jerarquiaAprobacionId, out var dto);
            return Task.FromResult(dto);
        }

        public Task<AdminJerarquiaDto> CreateJerarquiaAsync(CreateJerarquiaDto request, int actorUsuarioId, CancellationToken cancellationToken)
        {
            var dto = new AdminJerarquiaDto
            {
                JerarquiaAprobacionId = 99,
                AprobadorUsuarioId = request.AprobadorUsuarioId,
                EstructuraOrganizacionalId = request.EstructuraOrganizacionalId,
                NivelAprobacion = request.NivelAprobacion,
                TipoRelacion = request.TipoRelacion,
                EstadoRegistroId = 1,
                VigenciaDesde = request.VigenciaDesde,
                VigenciaHasta = request.VigenciaHasta
            };
            _jerarquias[dto.JerarquiaAprobacionId] = dto;
            return Task.FromResult(dto);
        }

        public Task<int> UpdateJerarquiaAsync(int jerarquiaAprobacionId, UpdateJerarquiaDto request, int actorUsuarioId, CancellationToken cancellationToken)
            => Task.FromResult(1);

        public Task<int> ToggleJerarquiaEstadoAsync(int jerarquiaAprobacionId, int estadoRegistroId, CancellationToken cancellationToken)
            => Task.FromResult(1);

        public Task<IReadOnlyList<AdminDelegacionDto>> ListDelegacionesAsync(int? deleganteUsuarioId, int? delegadoUsuarioId, int? estadoRegistroId, DateTime? vigenteEnFecha, CancellationToken cancellationToken)
            => Task.FromResult<IReadOnlyList<AdminDelegacionDto>>([]);

        public Task<AdminDelegacionDto?> GetDelegacionByIdAsync(int delegacionAprobacionId, CancellationToken cancellationToken)
            => Task.FromResult<AdminDelegacionDto?>(null);

        public Task<AdminDelegacionDto> CreateDelegacionAsync(CreateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<int> UpdateDelegacionAsync(int delegacionAprobacionId, UpdateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<int> ToggleDelegacionEstadoAsync(int delegacionAprobacionId, int estadoRegistroId, int actorUsuarioId, CancellationToken cancellationToken)
            => Task.FromResult(1);

        public Task<bool> ExistsUsuarioAsync(int usuarioId, CancellationToken cancellationToken)
            => Task.FromResult(UsuarioExiste);

        public Task<bool> ExistsEstructuraAsync(int estructuraOrganizacionalId, CancellationToken cancellationToken)
            => Task.FromResult(EstructuraExiste);

        public Task<bool> ExistsJerarquiaAsync(int jerarquiaAprobacionId, CancellationToken cancellationToken)
            => Task.FromResult(_jerarquias.ContainsKey(jerarquiaAprobacionId));

        public Task<bool> ExistsJerarquiaActivaDuplicadaAsync(int aprobadorUsuarioId, int estructuraOrganizacionalId, int nivelAprobacion, int? jerarquiaAprobacionIdExcluida, CancellationToken cancellationToken)
            => Task.FromResult(JerarquiaActivaDuplicadaExiste);

        // F-004: nuevos metodos de la interfaz
        public bool DelegacionActivaComoDelegadoExiste { get; set; } = false;

        public Task<bool> ExistsDelegacionActivaComoDelegadoAsync(int usuarioId, DateTime fechaRef, int? delegacionIdExcluida, CancellationToken cancellationToken)
            => Task.FromResult(DelegacionActivaComoDelegadoExiste);

        public Task<int> DeleteDelegacionAsync(int delegacionAprobacionId, CancellationToken cancellationToken)
            => Task.FromResult(1);
    }
}
