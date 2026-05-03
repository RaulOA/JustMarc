using Dapper;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace IntegradorMarcas.Tests;

public sealed class ErrorLogIntegrationTests
{
    private const string TestConnectionString =
        "Server=WinDev2407Eval\\SQLEXPRESS;Database=INTEGRA_CNP;Integrated Security=True;TrustServerCertificate=True";

    [Fact]
    [Trait("Category", "Integration")]
    public async Task LogAsync_DebeInsertarRegistroEnAuditoriaErrorApi()
    {
        var correlationId = Guid.NewGuid();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:IntegraCnp"] = TestConnectionString
            })
            .Build();

        var connectionFactory = new SqlConnectionFactory(configuration);
        var repository = new ErrorLogRepository(connectionFactory);

        var entry = new ErrorLogEntry(
            CorrelationId: correlationId,
            HttpMethod: "POST",
            Endpoint: "/api/justificaciones",
            StatusCode: 500,
            TipoError: "InvalidOperationException",
            Mensaje: "Error de prueba de integracion",
            StackTrace: "stacktrace de prueba",
            UsuarioId: "123",
            RolUsuario: "RRHH",
            Entorno: "Development",
            Ip: "127.0.0.1",
            UserAgent: "xunit-integration-test"
        );

        try
        {
            await repository.LogAsync(entry);

            await using var connection = new SqlConnection(TestConnectionString);
            await connection.OpenAsync();

            const string selectSql = """
                SELECT TOP (1)
                    CorrelationId,
                    StatusCode,
                    Mensaje
                FROM Auditoria.ErrorApi
                WHERE CorrelationId = @CorrelationId
                ORDER BY ErrorApiId DESC;
                """;

            var row = await connection.QuerySingleOrDefaultAsync<ErrorApiRow>(selectSql, new
            {
                CorrelationId = correlationId
            });

            Assert.NotNull(row);
            Assert.Equal(correlationId, row!.CorrelationId);
            Assert.Equal(500, row.StatusCode);
            Assert.Equal("Error de prueba de integracion", row.Mensaje);
        }
        finally
        {
            await using var cleanupConnection = new SqlConnection(TestConnectionString);
            await cleanupConnection.OpenAsync();

            const string cleanupSql = "DELETE FROM Auditoria.ErrorApi WHERE CorrelationId = @CorrelationId;";
            await cleanupConnection.ExecuteAsync(cleanupSql, new
            {
                CorrelationId = correlationId
            });
        }
    }

    private sealed class ErrorApiRow
    {
        public Guid CorrelationId { get; init; }
        public int StatusCode { get; init; }
        public string Mensaje { get; init; } = string.Empty;
    }
}