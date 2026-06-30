using IntegradorMarcas.Application.Common;
using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Application.Services;
using IntegradorMarcas.Domain.Constants;
using System.Reflection;

namespace IntegradorMarcas.Tests;

/// <summary>
/// Tests unitarios para F-004: delegaciones, sub-aprobadores y reglas completas.
/// T1 R1/R21; T3 R4; T4 R6; T5 R7; T6 R8; T7 R9; T8 R10; T9 R13;
/// T10 R11/R12; T11 R16/R17/R18; T12 R14; T13 R15; T14 R19/R20; T16 R22.
/// Todos los repositorios son fakes en memoria (gate: Category!=Integration).
/// </summary>
public sealed class F004DelegacionesTests
{
    // -----------------------------------------------------------------------
    // T1: R1 / R21 — VigenciaDesde requerida y VigenciaHasta >= VigenciaDesde
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateDelegacion_VigenciaDesdeAusente_Lanza400()
    {
        // R1: VigenciaDesde == default(DateTime) debe rechazarse con 400
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildAdminService(repo);

        var request = new CreateDelegacionDto
        {
            DeleganteUsuarioId = 10,
            DelegadoUsuarioId = 20
            // VigenciaDesde omitida -> DateTime.MinValue (default)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateDelegacionAsync(AdminUser(), request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
        Assert.Contains("VigenciaDesde", ex.Message);
    }

    [Fact]
    public async Task CreateDelegacion_VigenciaHastaAnteriorDesde_Lanza400()
    {
        // R21
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildAdminService(repo);

        var request = new CreateDelegacionDto
        {
            DeleganteUsuarioId = 10,
            DelegadoUsuarioId = 20,
            VigenciaDesde = new DateTime(2026, 6, 1),
            VigenciaHasta = new DateTime(2026, 5, 31)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateDelegacionAsync(AdminUser(), request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateDelegacion_VigenciaHastaAnteriorDesde_Lanza400()
    {
        // R21
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildAdminService(repo);

        var request = new UpdateDelegacionDto
        {
            DeleganteUsuarioId = 10,
            DelegadoUsuarioId = 20,
            EstadoRegistroId = 1,
            VigenciaDesde = new DateTime(2026, 6, 1),
            VigenciaHasta = new DateTime(2026, 5, 31)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.UpdateDelegacionAsync(AdminUser(), 1, request, null, CancellationToken.None));

        Assert.Equal(400, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // T3: R4 — Toggle a Inactivo invoca auditoria
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ToggleDelegacion_AInactivo_InvocaAuditoriaResumenYDetalle()
    {
        // R4: efecto inmediato al poner estado=2 (Inactivo)
        var repo = new FakeAdminAprobacionesRepository();
        repo.SetDelegacionById(5, BuildDelegacionDto(5, estadoRegistroId: 1));
        var auditSummary = new FakeAuditEventRepository();
        var auditDetail = new FakeAdminActionAuditRepository();
        var service = new AdminAprobacionesService(repo, auditSummary, auditDetail);

        await service.ToggleDelegacionEstadoAsync(
            AdminUser(), 5, new ToggleEstadoRegistroDto { EstadoRegistroId = 2 }, null, CancellationToken.None);

        Assert.Equal(1, auditSummary.LogCount);
        Assert.Equal(1, auditDetail.LogCount);
    }

    // -----------------------------------------------------------------------
    // T4: R6 — Anti-sub-delegacion (delegante es a su vez delegado)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateDelegacion_DeleganteEsDelegadoActivo_Lanza409()
    {
        // R6
        var repo = new FakeAdminAprobacionesRepository { DelegacionActivaComoDelegadoExiste = true };
        var service = BuildAdminService(repo);

        var request = new CreateDelegacionDto
        {
            DeleganteUsuarioId = 10,
            DelegadoUsuarioId = 20,
            VigenciaDesde = new DateTime(2026, 1, 1)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateDelegacionAsync(AdminUser(), request, null, CancellationToken.None));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task UpdateDelegacion_DeleganteEsDelegadoActivo_Lanza409()
    {
        // R6: al actualizar, el delegante propuesto es a su vez delegado
        var repo = new FakeAdminAprobacionesRepository { DelegacionActivaComoDelegadoExiste = true };
        repo.SetDelegacionById(3, BuildDelegacionDto(3));
        var service = BuildAdminService(repo);

        var request = new UpdateDelegacionDto
        {
            DeleganteUsuarioId = 10,
            DelegadoUsuarioId = 20,
            EstadoRegistroId = 1,
            VigenciaDesde = new DateTime(2026, 1, 1)
        };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.UpdateDelegacionAsync(AdminUser(), 3, request, null, CancellationToken.None));

        Assert.Equal(409, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // T5: R7 — Cualquier ROL_ADMIN puede operar, no-admin recibe 403
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateDelegacion_UsuarioNoAdmin_Lanza403()
    {
        // R7
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildAdminService(repo);
        var user = new UserContextInfo { UserId = 5, Role = RolesSistema.RolJefe };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.CreateDelegacionAsync(user, new CreateDelegacionDto
            {
                DeleganteUsuarioId = 10,
                DelegadoUsuarioId = 20,
                VigenciaDesde = new DateTime(2026, 1, 1)
            }, null, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task DeleteDelegacion_UsuarioNoAdmin_Lanza403()
    {
        // T5/R7: DeleteDelegacionAsync tambien requiere ROL_ADMIN
        var repo = new FakeAdminAprobacionesRepository();
        var service = BuildAdminService(repo);
        var user = new UserContextInfo { UserId = 5, Role = RolesSistema.RolFunc };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.DeleteDelegacionAsync(user, 1, null, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task DeleteDelegacion_AdminYExiste_InvocaAuditoriaYBorra()
    {
        // T14 R19/R20: borrado fisico con auditoria previa (D1)
        var repo = new FakeAdminAprobacionesRepository();
        repo.SetDelegacionById(7, BuildDelegacionDto(7));
        var auditSummary = new FakeAuditEventRepository();
        var auditDetail = new FakeAdminActionAuditRepository();
        var service = new AdminAprobacionesService(repo, auditSummary, auditDetail);

        await service.DeleteDelegacionAsync(AdminUser(), 7, "corr-del-001", CancellationToken.None);

        // R20: auditoria resumen Y detalle invocadas antes del borrado
        Assert.Equal(1, auditSummary.LogCount);
        Assert.Equal(1, auditDetail.LogCount);
        Assert.True(repo.DeleteFueInvocado);
    }

    [Fact]
    public async Task DeleteDelegacion_NoExiste_Lanza404()
    {
        // T14 R19: delegacion inexistente -> 404
        var repo = new FakeAdminAprobacionesRepository(); // sin ninguna delegacion
        var service = BuildAdminService(repo);

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.DeleteDelegacionAsync(AdminUser(), 999, null, CancellationToken.None));

        Assert.Equal(404, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // T6: R8 — Delegado no puede resolver justificacion de su titular
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ResolverAsync_DelegadoResuelveTitular_Lanza403()
    {
        // R8: ScopeSource=Delegacion, solicitante=delegante
        var repo = new FakeJustificacionRepository
        {
            ResolverValidationToReturn = new ResolverValidationDto
            {
                Exists = true,
                EstadoId = EstadoIds.PendienteJefatura,
                IsInApprovalScope = true,
                ScopeSource = "Delegacion",
                DeleganteUsuarioId = 100,
                SolicitanteUsuarioId = 100  // el solicitante ES el delegante
            }
        };
        var service = new JustificacionService(repo, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 50, Role = RolesSistema.RolJefe }; // delegado

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.ResolverAsync(user, 1, new ResolverJustificacionDto { Accion = "APROBAR" }, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // T7: R9 — Auto-resolucion rechazada (cubierta por TVF/SQL, test documentado)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ResolverAsync_FueraDeScopeDeAprobacion_Lanza403()
    {
        // R9/R10: IsInApprovalScope=false (la TVF no devuelve fila para el aprobador)
        var repo = new FakeJustificacionRepository
        {
            ResolverValidationToReturn = new ResolverValidationDto
            {
                Exists = true,
                EstadoId = EstadoIds.PendienteJefatura,
                IsInApprovalScope = false,
                ScopeSource = null,
                SolicitanteUsuarioId = 99
            }
        };
        var service = new JustificacionService(repo, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 99, Role = RolesSistema.RolJefe };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.ResolverAsync(user, 1, new ResolverJustificacionDto { Accion = "APROBAR" }, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // T8: R10 — Fuera del rango del titular (cubierta por TVF, test documentado)
    // El solicitante fuera del rango del titular: la TVF no devuelve fila Delegacion
    // -> IsInApprovalScope=false -> 403. Mismo mecanismo que R9.
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ResolverAsync_SolicitanteFueraRangoTitular_Lanza403()
    {
        // R10: documentado — la TVF garantiza que el delegado no puede aprobar fuera del rango
        // del titular. Si no hay fila para el aprobador, IsInApprovalScope=false.
        var repo = new FakeJustificacionRepository
        {
            ResolverValidationToReturn = new ResolverValidationDto
            {
                Exists = true,
                EstadoId = EstadoIds.PendienteJefatura,
                IsInApprovalScope = false,   // TVF no devuelve fila: solicitante fuera del rango
                ScopeSource = null,
                SolicitanteUsuarioId = 55
            }
        };
        var service = new JustificacionService(repo, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 30, Role = RolesSistema.RolJefe };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.ResolverAsync(user, 1, new ResolverJustificacionDto { Accion = "RECHAZAR" }, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // T9: R13 — Vencida VigenciaHasta, delegado no puede resolver (TVF excluye)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ResolverAsync_DelegacionExpirada_Lanza403()
    {
        // R13: la TVF filtra por VigenciaHasta. Si la delegacion expiro, no hay fila
        // -> IsInApprovalScope=false -> 403.
        var repo = new FakeJustificacionRepository
        {
            ResolverValidationToReturn = new ResolverValidationDto
            {
                Exists = true,
                EstadoId = EstadoIds.PendienteJefatura,
                IsInApprovalScope = false,  // TVF excluyo al delegado porque VigenciaHasta paso
                ScopeSource = null,
                SolicitanteUsuarioId = 70
            }
        };
        var service = new JustificacionService(repo, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 80, Role = RolesSistema.RolJefe };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.ResolverAsync(user, 5, new ResolverJustificacionDto { Accion = "APROBAR" }, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    // -----------------------------------------------------------------------
    // T10: R11/R12 — GetMiFuncion del delegado: guard rol y contenido
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetMiFuncion_RolNoJefe_Lanza403()
    {
        // R11 guard
        var repo = new FakeDelegacionConsultaRepository();
        var service = new DelegacionConsultaService(repo);
        var user = new UserContextInfo { UserId = 1, Role = RolesSistema.RolFunc };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.GetMiFuncionAsync(user, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task GetMiFuncion_RolJefe_DevuelveDelegacionConTitularYAlcance()
    {
        // R11/R12: el servicio devuelve la info del titular y el alcance
        var expectedDto = new DelegacionFuncionDto
        {
            DelegacionAprobacionId = 10,
            TitularUsuarioId = 5,
            TitularNombre = "Ana Titular",
            VigenciaDesde = new DateTime(2026, 1, 1),
            VigenciaHasta = new DateTime(2026, 12, 31),
            AlcanceEstructuras = "Operaciones, RRHH"
        };
        var repo = new FakeDelegacionConsultaRepository { FuncionToReturn = [expectedDto] };
        var service = new DelegacionConsultaService(repo);
        var user = new UserContextInfo { UserId = 20, Role = RolesSistema.RolJefe };

        var result = await service.GetMiFuncionAsync(user, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(5, result[0].TitularUsuarioId);
        Assert.Equal("Ana Titular", result[0].TitularNombre);
        Assert.Equal("Operaciones, RRHH", result[0].AlcanceEstructuras);
    }

    // -----------------------------------------------------------------------
    // T11: R16/R17/R18 — GetMiRegistro del delegado (solo lectura)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task GetMiRegistro_RolNoJefe_Lanza403()
    {
        // R16 guard — rol incorrecto
        var repo = new FakeDelegacionConsultaRepository();
        var service = new DelegacionConsultaService(repo);
        var user = new UserContextInfo { UserId = 1, Role = RolesSistema.RolFunc };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.GetMiRegistroAsync(user, new FiltroJustificacionesDto(), CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task GetMiRegistro_RolJefe_DevuelveRegistroAcotadoPorPeriodo()
    {
        // R16/R17: el repositorio filtra por FechaAprobacion dentro del periodo (D4)
        var expectedDto = new DelegacionRegistroDto
        {
            JustificacionId = 100,
            SolicitanteUsuarioId = 7,
            SolicitanteNombre = "Luis Funcionario",
            DelegacionAprobacionId = 10,
            TitularUsuarioId = 5,
            TitularNombre = "Ana Titular",
            FechaAprobacion = new DateTime(2026, 3, 15),
            EstadoId = EstadoIds.Aprobado,
            EstadoDescripcion = "Aprobado"
        };
        var repo = new FakeDelegacionConsultaRepository { RegistroToReturn = [expectedDto] };
        var service = new DelegacionConsultaService(repo);
        var user = new UserContextInfo { UserId = 20, Role = RolesSistema.RolJefe };

        var result = await service.GetMiRegistroAsync(user, new FiltroJustificacionesDto
        {
            Desde = new DateTime(2026, 1, 1),
            Hasta = new DateTime(2026, 12, 31)
        }, CancellationToken.None);

        Assert.Single(result);
        Assert.Equal(10, result[0].DelegacionAprobacionId);
        Assert.Equal(5, result[0].TitularUsuarioId);
    }

    [Fact]
    public void GetMiRegistro_NoExisteRutaDeMutacion_ConfirmadoPorAusenciaDeEndpoint()
    {
        // R18: el registro de delegado es de solo lectura.
        // Este test documenta que DelegacionConsultaService solo expone GetMiRegistroAsync (lectura).
        // No existe un metodo SetMiRegistroAsync ni PatchMiRegistroAsync.
        var serviceType = typeof(DelegacionConsultaService);
        var mutacionMetodos = serviceType
            .GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
            .Where(m => m.Name.StartsWith("Set", StringComparison.OrdinalIgnoreCase)
                     || m.Name.StartsWith("Patch", StringComparison.OrdinalIgnoreCase)
                     || m.Name.StartsWith("Delete", StringComparison.OrdinalIgnoreCase)
                     || m.Name.StartsWith("Update", StringComparison.OrdinalIgnoreCase))
            .ToList();

        Assert.Empty(mutacionMetodos);
    }

    // -----------------------------------------------------------------------
    // T12: R14 — Titular ve justificaciones resueltas por su delegado
    // (cubierta por ListHistoricoAsync con aprobadorUsuarioIdScope)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task ListHistoricoAsync_RolJefatura_IncluirResueltasPorDelegado()
    {
        // R14: el titular con rol jefatura consulta historico con aprobadorUsuarioIdScope=su id
        // -> el repositorio devuelve tambien las resueltas por su delegado
        var repo = new FakeJustificacionRepository
        {
            HistoricoToReturn =
            [
                new RrhhJustificacionResumenDto { JustificacionID = 201, AprobadorID = 88 }, // resuelta por delegado 88
                new RrhhJustificacionResumenDto { JustificacionID = 202, AprobadorID = 10 }  // resuelta por el titular
            ]
        };
        var service = new JustificacionService(repo, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 10, Role = RolesSistema.RolJefe };

        var result = await service.ListHistoricoAsync(user, new FiltroRrhhJustificacionesDto
        {
            FechaDesde = new DateTime(2026, 1, 1),
            FechaHasta = new DateTime(2026, 12, 31),
            Compania = "CNP"
        }, CancellationToken.None);

        Assert.Equal(2, result.Count);
        Assert.Contains(result, r => r.AprobadorID == 88); // la resuelta por el delegado sigue visible
    }

    // -----------------------------------------------------------------------
    // T13: R15 — Re-resolucion del titular (D2 = B endpoint dedicado)
    // -----------------------------------------------------------------------

    [Fact]
    public async Task RevisarTitularAsync_RolNoJefe_Lanza403()
    {
        // R15: guard de rol
        var repo = new FakeJustificacionRepository();
        var service = new JustificacionService(repo, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 1, Role = RolesSistema.RolFunc };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.RevisarTitularAsync(user, 1, new ResolverJustificacionDto { Accion = "APROBAR" }, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task RevisarTitularAsync_SinAlcanceJerarquia_Lanza403()
    {
        // R15: el titular no tiene alcance por jerarquia directa
        var repo = new FakeJustificacionRepository
        {
            RevisarTitularValidationToReturn = new RevisarTitularValidationDto
            {
                Exists = true,
                EstadoId = EstadoIds.Aprobado,
                EsTitularPorJerarquia = false,
                AprobadorAnteriorId = 88
            }
        };
        var service = new JustificacionService(repo, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 10, Role = RolesSistema.RolJefe };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.RevisarTitularAsync(user, 1, new ResolverJustificacionDto { Accion = "APROBAR" }, CancellationToken.None));

        Assert.Equal(403, ex.StatusCode);
    }

    [Fact]
    public async Task RevisarTitularAsync_JustificacionPendiente_Lanza409()
    {
        // R15: si la justificacion aun esta pendiente, no se usa este endpoint
        var repo = new FakeJustificacionRepository
        {
            RevisarTitularValidationToReturn = new RevisarTitularValidationDto
            {
                Exists = true,
                EstadoId = EstadoIds.PendienteJefatura, // aun pendiente
                EsTitularPorJerarquia = true,
                AprobadorAnteriorId = 88
            }
        };
        var service = new JustificacionService(repo, new FakeAuditEventRepository());
        var user = new UserContextInfo { UserId = 10, Role = RolesSistema.RolJefe };

        var ex = await Assert.ThrowsAsync<AppException>(() =>
            service.RevisarTitularAsync(user, 1, new ResolverJustificacionDto { Accion = "APROBAR" }, CancellationToken.None));

        Assert.Equal(409, ex.StatusCode);
    }

    [Fact]
    public async Task RevisarTitularAsync_TitularConAlcanceYDelegadoAnterior_ResuelvyAudita()
    {
        // R15: titular con jerarquia, justificacion resuelta por delegado -> re-resolucion exitosa
        var repo = new FakeJustificacionRepository
        {
            RevisarTitularValidationToReturn = new RevisarTitularValidationDto
            {
                Exists = true,
                EstadoId = EstadoIds.Aprobado,
                EsTitularPorJerarquia = true,
                AprobadorAnteriorId = 88  // delegado anterior (distinto del titular)
            },
            RevisarTitularAffected = 1
        };
        var auditRepo = new FakeAuditEventRepository();
        var service = new JustificacionService(repo, auditRepo);
        var user = new UserContextInfo { UserId = 10, Role = RolesSistema.RolJefe };

        await service.RevisarTitularAsync(user, 1, new ResolverJustificacionDto { Accion = "RECHAZAR" }, CancellationToken.None);

        Assert.Equal(1, auditRepo.LogCount);
    }

    // -----------------------------------------------------------------------
    // T16: R22 — Auditoria en create/update/toggle/delete de delegacion
    // -----------------------------------------------------------------------

    [Fact]
    public async Task CreateDelegacion_AltaValida_InvocaAuditoriaResumenYDetalle()
    {
        // R22
        var repo = new FakeAdminAprobacionesRepository();
        var auditSummary = new FakeAuditEventRepository();
        var auditDetail = new FakeAdminActionAuditRepository();
        var service = new AdminAprobacionesService(repo, auditSummary, auditDetail);

        await service.CreateDelegacionAsync(AdminUser(), ValidCreateDelegacion(), "corr-create", CancellationToken.None);

        Assert.Equal(1, auditSummary.LogCount);
        Assert.Equal(1, auditDetail.LogCount);
    }

    [Fact]
    public async Task UpdateDelegacion_EdicionValida_InvocaAuditoriaResumenYDetalle()
    {
        // R22
        var repo = new FakeAdminAprobacionesRepository();
        repo.SetDelegacionById(1, BuildDelegacionDto(1));
        var auditSummary = new FakeAuditEventRepository();
        var auditDetail = new FakeAdminActionAuditRepository();
        var service = new AdminAprobacionesService(repo, auditSummary, auditDetail);

        await service.UpdateDelegacionAsync(AdminUser(), 1, ValidUpdateDelegacion(), "corr-update", CancellationToken.None);

        Assert.Equal(1, auditSummary.LogCount);
        Assert.Equal(1, auditDetail.LogCount);
    }

    [Fact]
    public async Task ToggleDelegacion_CambioEstado_InvocaAuditoriaResumenYDetalle()
    {
        // R22
        var repo = new FakeAdminAprobacionesRepository();
        repo.SetDelegacionById(2, BuildDelegacionDto(2));
        var auditSummary = new FakeAuditEventRepository();
        var auditDetail = new FakeAdminActionAuditRepository();
        var service = new AdminAprobacionesService(repo, auditSummary, auditDetail);

        await service.ToggleDelegacionEstadoAsync(AdminUser(), 2, new ToggleEstadoRegistroDto { EstadoRegistroId = 2 }, null, CancellationToken.None);

        Assert.Equal(1, auditSummary.LogCount);
        Assert.Equal(1, auditDetail.LogCount);
    }

    // -----------------------------------------------------------------------
    // Helpers
    // -----------------------------------------------------------------------

    private static AdminAprobacionesService BuildAdminService(FakeAdminAprobacionesRepository repo)
        => new(repo, new FakeAuditEventRepository(), new FakeAdminActionAuditRepository());

    private static UserContextInfo AdminUser()
        => new() { UserId = 1, Role = RolesSistema.RolAdmin };

    private static CreateDelegacionDto ValidCreateDelegacion() => new()
    {
        DeleganteUsuarioId = 10,
        DelegadoUsuarioId = 20,
        VigenciaDesde = new DateTime(2026, 1, 1),
        VigenciaHasta = new DateTime(2026, 12, 31),
        Motivo = "Vacaciones"
    };

    private static UpdateDelegacionDto ValidUpdateDelegacion() => new()
    {
        DeleganteUsuarioId = 10,
        DelegadoUsuarioId = 20,
        EstadoRegistroId = 1,
        VigenciaDesde = new DateTime(2026, 1, 1),
        VigenciaHasta = new DateTime(2026, 12, 31),
        Motivo = "Vacaciones extendidas"
    };

    private static AdminDelegacionDto BuildDelegacionDto(int id, int estadoRegistroId = 1) => new()
    {
        DelegacionAprobacionId = id,
        DeleganteUsuarioId = 10,
        DelegadoUsuarioId = 20,
        EstadoRegistroId = estadoRegistroId,
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
        public bool DelegacionActivaComoDelegadoExiste { get; set; } = false;
        public bool DeleteFueInvocado { get; private set; }

        private readonly Dictionary<int, AdminJerarquiaDto> _jerarquias = [];
        private readonly Dictionary<int, AdminDelegacionDto> _delegaciones = [];

        public void SetDelegacionById(int id, AdminDelegacionDto dto) => _delegaciones[id] = dto;
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
        {
            _delegaciones.TryGetValue(delegacionAprobacionId, out var dto);
            return Task.FromResult(dto);
        }

        public Task<AdminDelegacionDto> CreateDelegacionAsync(CreateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken)
        {
            var dto = new AdminDelegacionDto
            {
                DelegacionAprobacionId = 88,
                DeleganteUsuarioId = request.DeleganteUsuarioId,
                DelegadoUsuarioId = request.DelegadoUsuarioId,
                EstadoRegistroId = 1,
                VigenciaDesde = request.VigenciaDesde,
                VigenciaHasta = request.VigenciaHasta,
                Motivo = request.Motivo
            };
            _delegaciones[dto.DelegacionAprobacionId] = dto;
            return Task.FromResult(dto);
        }

        public Task<int> UpdateDelegacionAsync(int delegacionAprobacionId, UpdateDelegacionDto request, int actorUsuarioId, CancellationToken cancellationToken)
            => Task.FromResult(1);

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

        public Task<bool> ExistsDelegacionActivaComoDelegadoAsync(int usuarioId, DateTime fechaRef, int? delegacionIdExcluida, CancellationToken cancellationToken)
            => Task.FromResult(DelegacionActivaComoDelegadoExiste);

        public Task<int> DeleteDelegacionAsync(int delegacionAprobacionId, CancellationToken cancellationToken)
        {
            DeleteFueInvocado = true;
            return Task.FromResult(1);
        }
    }

    private sealed class FakeJustificacionRepository : IJustificacionRepository
    {
        public ResolverValidationDto ResolverValidationToReturn { get; set; } = new()
        {
            Exists = false,
            EstadoId = 0,
            IsInApprovalScope = false,
            SolicitanteUsuarioId = 0
        };

        public RevisarTitularValidationDto RevisarTitularValidationToReturn { get; set; } = new()
        {
            Exists = false,
            EstadoId = 0,
            EsTitularPorJerarquia = false
        };

        public int RevisarTitularAffected { get; set; } = 0;

        public IReadOnlyList<RrhhJustificacionResumenDto> HistoricoToReturn { get; set; } = [];

        public Task<IReadOnlyCollection<int>> GetExistingTipoJustificacionIdsAsync(IEnumerable<int> ids, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<int> CreateAsync(int usuarioId, CreateJustificacionDto request, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<CurrentApproverDto> GetCurrentApproverAsync(int solicitanteUsuarioId, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<JustificacionResumenDto>> ListMineAsync(int usuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<JustificacionDetalleLineaDto>> ListMineLineasAsync(int usuarioId, int justificacionId, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<JustificacionResumenDto>> ListPendientesJefaturaAsync(int aprobadorUsuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(FiltroRrhhJustificacionesDto filtros, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListHistoricoAsync(
            int? usuarioId,
            int? aprobadorUsuarioId,
            bool excluirPropiosEnScopeAprobador,
            FiltroRrhhJustificacionesDto filtros,
            CancellationToken cancellationToken)
            => Task.FromResult(HistoricoToReturn);

        public Task<JustificacionCompletaDto?> GetDetalleJefaturaAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<AprobacionScopeValidationDto> GetAprobacionScopeValidationAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
            => throw new NotImplementedException();

        public Task<ResolverValidationDto> GetResolverValidationAsync(int justificacionId, int aprobadorUsuarioId, CancellationToken cancellationToken)
            => Task.FromResult(ResolverValidationToReturn);

        public Task<int> ResolverAsync(int justificacionId, int aprobadorUsuarioId, int estadoId, string? comentario, string? rolResolucion, CancellationToken cancellationToken)
            => Task.FromResult(1);

        public Task<RevisarTitularValidationDto> GetRevisarTitularValidationAsync(int justificacionId, int titularUsuarioId, CancellationToken cancellationToken)
            => Task.FromResult(RevisarTitularValidationToReturn);

        public Task<int> RevisarTitularAsync(int justificacionId, int titularUsuarioId, int estadoId, string? comentario, string? rolResolucion, CancellationToken cancellationToken)
            => Task.FromResult(RevisarTitularAffected);
    }

    private sealed class FakeDelegacionConsultaRepository : IDelegacionConsultaRepository
    {
        public IReadOnlyList<DelegacionFuncionDto> FuncionToReturn { get; set; } = [];
        public IReadOnlyList<DelegacionRegistroDto> RegistroToReturn { get; set; } = [];

        public Task<IReadOnlyList<DelegacionFuncionDto>> GetMiFuncionAsync(int delegadoUsuarioId, DateTime fechaRef, CancellationToken cancellationToken)
            => Task.FromResult(FuncionToReturn);

        public Task<IReadOnlyList<DelegacionRegistroDto>> GetMiRegistroAsync(int delegadoUsuarioId, FiltroJustificacionesDto filtros, CancellationToken cancellationToken)
            => Task.FromResult(RegistroToReturn);
    }
}
