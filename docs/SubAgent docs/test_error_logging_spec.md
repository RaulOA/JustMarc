# Spec: prueba de registro de errores en Auditoria.ErrorApi

## Seccion 1: Estado actual del proyecto de tests

### Framework y version
- Framework de tests: xUnit.
- Versiones detectadas en el proyecto de pruebas:
  - xunit: 2.5.3
  - xunit.runner.visualstudio: 2.5.3
  - Microsoft.NET.Test.Sdk: 17.8.0
  - coverlet.collector: 6.0.0
- Target framework: net8.0.

### Paquetes NuGet disponibles
- Actualmente NO hay Moq ni FluentAssertions en el proyecto de tests.
- Paquetes existentes solo cubren ejecucion y cobertura de pruebas (xUnit + Test SDK + coverlet).

### Estructura actual de tests
- Archivos encontrados en backend/tests/IntegradorMarcas.Tests:
  - IntegradorMarcas.Tests.csproj
  - UnitTest1.cs
- No hay helpers, fixtures, ni base classes de pruebas aun.
- El .csproj referencia solo:
  - IntegradorMarcas.Application
  - IntegradorMarcas.Domain
- Falta referencia a IntegradorMarcas.Infrastructure para poder instanciar ErrorLogRepository en pruebas.

### Connection string de BD para pruebas
- En appsettings.Development.json del API existe:
  - ConnectionStrings:IntegraCnp = Server=localhost\\SQLEXPRESS;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;
- No existe una BD de test separada configurada.
- Para este contexto, la estrategia mas realista es usar INTEGRA_CNP local (misma conexion de desarrollo), con limpieza explicita del dato insertado al finalizar la prueba.

## Seccion 2: Estrategia de prueba

### Opcion A: Test de integracion real (recomendada)
Descripcion:
- Conectar a SQL Server local (INTEGRA_CNP).
- Instanciar ErrorLogRepository con un ISqlConnectionFactory de test.
- Ejecutar LogAsync() con un CorrelationId unico.
- Consultar Auditoria.ErrorApi y validar que la fila fue insertada con los campos esperados.
- Hacer cleanup (DELETE por CorrelationId) en finally.

Pros:
- Valida comportamiento real de extremo a extremo (Dapper + SQL + esquema real).
- Detecta desalineaciones de columnas como las corregidas en 005_fix_errorapi_schema.sql.
- Aporta mas valor en un sistema donde la BD local ya esta disponible.

Contras:
- Depende de disponibilidad de SQL Server local y del estado de la BD.
- Es mas lenta y menos aislada que un unit test puro.

### Opcion B: Test unitario con mock
Descripcion:
- Mockear la conexion/ejecucion SQL y verificar que se intenta ejecutar INSERT con parametros esperados.

Pros:
- Rapido y sin dependencia de BD.

Contras:
- Poco valor para este caso: no valida nombres reales de columnas ni compatibilidad con esquema final.
- Requiere mas infraestructura (wrappers/mock de Dapper/IDbConnection) porque extension methods de Dapper no se mockean directo.

### Recomendacion final
- Recomendada: Opcion A (integracion real) para este codebase.
- Justificacion: existe conexion local INTEGRA_CNP y el objetivo principal es validar registro real en Auditoria.ErrorApi.

## Seccion 3: Codigo exacto de la prueba

Nombre de archivo propuesto:
- ErrorLogIntegrationTests.cs

Ubicacion sugerida:
- backend/tests/IntegradorMarcas.Tests/ErrorLogIntegrationTests.cs

Codigo C# listo para copiar:

```csharp
using System.Data;
using IntegradorMarcas.Application.Interfaces;
using IntegradorMarcas.Infrastructure.Data;
using IntegradorMarcas.Infrastructure.Repositories;
using Microsoft.Data.SqlClient;
using Xunit;

namespace IntegradorMarcas.Tests;

public sealed class ErrorLogIntegrationTests
{
    private const string TestConnectionString =
        "Server=localhost\\SQLEXPRESS;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;";

    [Fact]
    [Trait("Category", "Integration")]
    public async Task LogAsync_DebeInsertarRegistroEnAuditoriaErrorApi()
    {
        // Arrange
        var correlationId = Guid.NewGuid();
        var repository = new ErrorLogRepository(new TestSqlConnectionFactory(TestConnectionString));

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
            // Act
            await repository.LogAsync(entry);

            // Assert
            await using var conn = new SqlConnection(TestConnectionString);
            await conn.OpenAsync();

            const string selectSql = @"
SELECT TOP (1)
    CorrelationId,
    HttpMethod,
    Endpoint,
    StatusCode,
    TipoError,
    Mensaje,
    StackTrace,
    UsuarioID,
    RolUsuario,
    Entorno,
    Ip,
    UserAgent
FROM Auditoria.ErrorApi
WHERE CorrelationId = @CorrelationId
ORDER BY ErrorApiId DESC;";

            await using var cmd = new SqlCommand(selectSql, conn);
            cmd.Parameters.AddWithValue("@CorrelationId", correlationId);

            await using var reader = await cmd.ExecuteReaderAsync();

            Assert.True(await reader.ReadAsync(), "No se encontro registro en Auditoria.ErrorApi para el CorrelationId generado.");
            Assert.Equal("POST", reader["HttpMethod"]?.ToString());
            Assert.Equal("/api/justificaciones", reader["Endpoint"]?.ToString());
            Assert.Equal(500, Convert.ToInt32(reader["StatusCode"]));
            Assert.Equal("InvalidOperationException", reader["TipoError"]?.ToString());
            Assert.Equal("Error de prueba de integracion", reader["Mensaje"]?.ToString());
            Assert.Equal("stacktrace de prueba", reader["StackTrace"]?.ToString());
            Assert.Equal("123", reader["UsuarioID"]?.ToString());
            Assert.Equal("RRHH", reader["RolUsuario"]?.ToString());
            Assert.Equal("Development", reader["Entorno"]?.ToString());
            Assert.Equal("127.0.0.1", reader["Ip"]?.ToString());
            Assert.Equal("xunit-integration-test", reader["UserAgent"]?.ToString());
        }
        finally
        {
            // Cleanup
            await using var cleanupConn = new SqlConnection(TestConnectionString);
            await cleanupConn.OpenAsync();

            const string cleanupSql = "DELETE FROM Auditoria.ErrorApi WHERE CorrelationId = @CorrelationId;";
            await using var cleanupCmd = new SqlCommand(cleanupSql, cleanupConn);
            cleanupCmd.Parameters.AddWithValue("@CorrelationId", correlationId);
            await cleanupCmd.ExecuteNonQueryAsync();
        }
    }

    private sealed class TestSqlConnectionFactory : ISqlConnectionFactory
    {
        private readonly string _connectionString;

        public TestSqlConnectionFactory(string connectionString)
        {
            _connectionString = connectionString;
        }

        public IDbConnection CreateConnection()
        {
            return new SqlConnection(_connectionString);
        }
    }
}
```

Notas tecnicas del test:
- ErrorLogRepository.LogAsync encapsula excepciones (catch vacio), por eso la validacion debe ser contra la tabla real.
- El script 005_fix_errorapi_schema.sql confirma que las columnas esperadas por el repositorio son: HttpMethod, StatusCode, UsuarioID, Ip, RolUsuario, Entorno, UserAgent.
- Si la BD no esta disponible o la cadena no es valida, la prueba fallara por conectividad (esperado en prueba de integracion).

## Seccion 4: Cambios al .csproj si son necesarios

Archivo:
- backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj

Cambios recomendados minimos:

1) Agregar referencia a IntegradorMarcas.Infrastructure para acceder a ErrorLogRepository e ISqlConnectionFactory.

```xml
<ItemGroup>
  <ProjectReference Include="..\\..\\src\\IntegradorMarcas.Infrastructure\\IntegradorMarcas.Infrastructure.csproj" />
</ItemGroup>
```

2) Si hay errores de compilacion por SqlConnection en tests, agregar paquete explicito:

```xml
<ItemGroup>
  <PackageReference Include="Microsoft.Data.SqlClient" Version="5.2.2" />
</ItemGroup>
```

3) Opcional para enfoque de unit test con mocks (no requerido para integracion):

```xml
<ItemGroup>
  <PackageReference Include="Moq" Version="4.20.72" />
  <PackageReference Include="FluentAssertions" Version="6.12.0" />
</ItemGroup>
```

## Evidencia tecnica analizada

- ErrorLogRepository usa INSERT directo a Auditoria.ErrorApi con columnas finales del esquema corregido.
- No existe query de ErrorLog en carpeta Queries; el SQL de ErrorApi esta inline en ErrorLogRepository.
- Program.cs registra IErrorLogRepository -> ErrorLogRepository y usa log en middleware global de excepciones.
- AppException expone StatusCode y participa en el flujo de manejo global de errores.
- HeaderUserContext toma headers X-User-Id y X-User-Role (configurables), utiles para poblar UsuarioID/RolUsuario en errores.
- SqlConnectionFactory toma ConnectionStrings:IntegraCnp desde IConfiguration.
