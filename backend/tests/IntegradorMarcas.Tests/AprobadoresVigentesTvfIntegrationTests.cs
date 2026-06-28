using IntegradorMarcas.Application.DTOs;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Repositories;
using Microsoft.Extensions.Configuration;

namespace IntegradorMarcas.Tests;

/// <summary>
/// Tests de integracion contra BD real para la TVF dbo.fn_AprobadoresVigentesPorSolicitante.
/// Cubren R2 (multiples niveles incluidos), R4 (fuera de vigencia / inactivo excluido),
/// R5 (solicitante sin estructura vigente devuelve sin aprobador por jerarquia).
///
/// Requieren conexion a INTEGRA_CNP (DESARROLLO).
/// Se excluyen del gate por: [Trait("Category", "Integration")]
/// Gate corre: dotnet test --filter "Category!=Integration"
/// </summary>
[Trait("Category", "Integration")]
public sealed class AprobadoresVigentesTvfIntegrationTests
{
    private const string DefaultConnectionString =
        "Server=localhost;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;";

    private IJustificacionRepository BuildRepository()
    {
        var connStr = Environment.GetEnvironmentVariable("ConnectionStrings__IntegraCnp")
            ?? DefaultConnectionString;

        var config = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:IntegraCnp"] = connStr
            })
            .Build();

        var factory = new SqlConnectionFactory(config);
        return new JustificacionRepository(factory);
    }

    /// <summary>
    /// R2: Solicitante cubierto por varias jerarquias vigentes de distinto NivelAprobacion ->
    /// la TVF incluye un aprobador vigente por cada jerarquia aplicable sin descartar niveles.
    /// El OUTER APPLY selecciona el aprobador de mayor precedencia (Delegacion primero).
    /// Este test confirma que la consulta no falla ante multiples niveles vigentes.
    /// </summary>
    [Fact(Skip = "Requiere BD real con datos de multiples jerarquias vigentes para el solicitante de prueba.")]
    public async Task GetCurrentApproverAsync_MultiplesNivelesVigentes_NoFallaYDevuelveAprobador()
    {
        // R2
        // Ajustar solicitanteUsuarioId a un usuario con 2+ jerarquias vigentes en BD de desarrollo.
        const int solicitanteUsuarioId = 1;
        var repo = BuildRepository();

        var result = await repo.GetCurrentApproverAsync(solicitanteUsuarioId, CancellationToken.None);

        Assert.NotNull(result);
    }

    /// <summary>
    /// R4: Jerarquia o delegacion fuera de vigencia o inactiva -> excluida del conjunto resuelto.
    /// El repositorio no lanza excepcion; devuelve aprobador nulo si no hay vigente.
    /// </summary>
    [Fact(Skip = "Requiere BD real con solicitante cuya unica jerarquia este fuera de vigencia o inactiva.")]
    public async Task GetCurrentApproverAsync_JerarquiaFueraDeVigenciaOInactiva_ExcluyeAprobador()
    {
        // R4
        const int solicitanteConJerarquiaVencida = 2;
        var repo = BuildRepository();

        var result = await repo.GetCurrentApproverAsync(solicitanteConJerarquiaVencida, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Aprobador);
    }

    /// <summary>
    /// R5: Solicitante sin estructura organizacional vigente asociada a su UnidadId ->
    /// resultado sin aprobador por jerarquia, sin lanzar excepcion.
    /// </summary>
    [Fact(Skip = "Requiere BD real con solicitante cuya UnidadId no tiene estructura organizacional vigente.")]
    public async Task GetCurrentApproverAsync_SolicitanteSinEstructuraVigente_DevuelveAprobadorNuloSinExcepcion()
    {
        // R5
        const int solicitanteSinEstructura = 3;
        var repo = BuildRepository();

        var result = await repo.GetCurrentApproverAsync(solicitanteSinEstructura, CancellationToken.None);

        Assert.NotNull(result);
        Assert.Null(result.Aprobador);
    }
}
