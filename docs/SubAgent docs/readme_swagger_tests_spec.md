# Especificación para actualizar README: Swagger + Tests

Fecha: 2026-05-02
Workspace: Justificacion de Marca

## 1) Objetivo

Actualizar `README.md` con dos bloques de documentación prácticos y precisos:
1. Guía de uso de Swagger para esta API (URLs reales, headers obligatorios, ejemplos por rol).
2. Documentación de las pruebas unitarias/integración ya creadas (estado actual, comandos y filtros útiles).

## 2) Hallazgos verificados (fuente de verdad)

## 2.1 Swagger y arranque del API

- Swagger se registra en `Program.cs` con:
  - `builder.Services.AddEndpointsApiExplorer();`
  - `builder.Services.AddSwaggerGen();`
  - `app.UseSwagger();`
  - `app.UseSwaggerUI();`
- Swagger depende de `Swagger:Enabled`:
  - Development: `true` (`appsettings.Development.json`)
  - Base/Production: `false` (`appsettings.json`, `appsettings.Production.json`)
- Launch profiles (`launchSettings.json`):
  - HTTP: `http://localhost:5093`
  - HTTPS: `https://localhost:7129` (y también HTTP 5093 en perfil `https`)
  - `launchUrl`: `swagger`
- Rutas prácticas de Swagger UI/OpenAPI:
  - `http://localhost:5093/swagger`
  - `https://localhost:7129/swagger`
  - `http://localhost:5093/swagger/index.html`
  - `https://localhost:7129/swagger/index.html`
  - `http://localhost:5093/swagger/v1/swagger.json`
  - `https://localhost:7129/swagger/v1/swagger.json`

## 2.2 Requisitos de headers/identidad para probar endpoints en Swagger

- La identidad se resuelve en `HeaderUserContext`.
- Headers configurables (por default):
  - `X-User-Id`
  - `X-User-Role`
- Reglas de validación:
  - `X-User-Id` debe ser entero positivo.
  - `X-User-Role` debe existir y no ir vacío.
  - Si falla, responde `401` (`AppException`).
- Roles aceptados por negocio (`RolesSistema`):
  - `ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH`, `ROL_ADMIN`
  - También hay aliases: `FUNCIONARIO/1`, `JEFATURA/2`, `RRHH/3`, `ADMIN/4`.

## 2.3 Endpoints útiles para guía práctica en README

- Salud:
  - `GET /health` (sin headers de usuario)
- Funcionario:
  - `POST /api/justificaciones`
  - `GET /api/justificaciones/mias`
- Jefatura:
  - `GET /api/jefatura/justificaciones/pendientes`
  - `GET /api/jefatura/justificaciones/{justificacionId}`
  - `PATCH /api/jefatura/justificaciones/{justificacionId}/resolver`
- RRHH:
  - `GET /api/rrhh/justificaciones`
- Admin:
  - `GET /api/admin/aprobaciones/jerarquias`
  - `POST /api/admin/aprobaciones/jerarquias`
  - `PATCH /api/admin/aprobaciones/jerarquias/{jerarquiaAprobacionId}/estado`
  - `GET /api/admin/aprobaciones/delegaciones`
  - `POST /api/admin/aprobaciones/delegaciones`
  - `PATCH /api/admin/aprobaciones/delegaciones/{delegacionAprobacionId}/estado`

## 2.4 Proyecto de pruebas y estado actual

- Proyecto: `backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj`
- Framework: xUnit (`xunit`, `xunit.runner.visualstudio`, `Microsoft.NET.Test.Sdk`)
- Cobertura: `coverlet.collector`
- Clases/casos existentes:
  - `UnitTest1.Test1` (placeholder, sin aserciones)
  - `ErrorLogIntegrationTests.LogAsync_DebeInsertarRegistroEnAuditoriaErrorApi` con `Trait("Category", "Integration")`
- Estado actual verificado al ejecutar tests:
  - Falla de compilación `CS0103` en `ErrorLogIntegrationTests.cs` porque `cleanupSql` está comentado pero se usa.
  - Resultado: `dotnet test` termina con código de salida 1 y no llega a ejecutar casos.

## 2.5 Requisitos específicos de la prueba de integración

La prueba `ErrorLogIntegrationTests` usa conexión hardcodeada:
- `Server=WinDev2407Eval\\SQLEXPRESS;Database=INTEGRA_CNP;Integrated Security=True;TrustServerCertificate=True`

Implicaciones:
- No usa la conexión de `appsettings.Development.json`.
- Para ejecutarla exitosamente, ese servidor/instancia debe existir y contener la tabla `Auditoria.ErrorApi` con esquema compatible.

## 3) Especificación de cambios sugeridos en README.md

## 3.1 Sección nueva: "Uso práctico de Swagger"

Añadir una guía breve y operativa con:
- Cómo habilitar Swagger por ambiente:
  - Development: viene activo.
  - Si se ejecuta fuera de Development, indicar `Swagger:Enabled=true` temporalmente para pruebas locales.
- URL exacta para abrir Swagger UI y URL del JSON OpenAPI.
- Flujo recomendado de uso:
  1. Levantar API.
  2. Abrir `/swagger`.
  3. Probar primero `GET /health`.
  4. Probar endpoints de negocio enviando headers `X-User-Id` y `X-User-Role`.
- Ejemplos de headers por rol:
  - Funcionario: `X-User-Id: 100`, `X-User-Role: ROL_FUNC`
  - Jefatura: `X-User-Id: 200`, `X-User-Role: ROL_JEFE`
  - RRHH: `X-User-Id: 300`, `X-User-Role: ROL_RRHH`
  - Admin: `X-User-Id: 400`, `X-User-Role: ROL_ADMIN`
- Nota de errores comunes:
  - `401` cuando faltan/son inválidos headers.
  - `404` cuando ruta no existe (middleware transforma 404 en `AppException`).

## 3.2 Sección nueva o reforzada: "Pruebas existentes"

Documentar explícitamente:
- Qué pruebas existen hoy (unidad placeholder + integración de logging).
- Qué valida la integración (`ErrorLogRepository.LogAsync` inserta en `Auditoria.ErrorApi`).
- Estado actual (bloqueo por compilación en `cleanupSql`).

Incluir comandos prácticos:

```powershell
# Ejecutar todo
 dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj -v minimal

# Solo pruebas de integración (Trait)
 dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "Category=Integration"

# Solo prueba concreta por nombre totalmente calificado
 dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "FullyQualifiedName~ErrorLogIntegrationTests"

# Cobertura
 dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --collect:"XPlat Code Coverage"
```

Agregar advertencia operativa:
- Mientras exista el error `cleanupSql` no definido, cualquier comando `dotnet test` fallará en compilación.

## 4) Texto sugerido (listo para pegar en README)

## 4.1 Bloque Swagger (propuesta)

~~~md
## Uso práctico de Swagger

En desarrollo local, Swagger está habilitado por defecto (`Swagger:Enabled=true`).

1. Levanta la API:

~~~powershell
 dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj
~~~

2. Abre Swagger UI:
- http://localhost:5093/swagger
- https://localhost:7129/swagger

3. Verifica conectividad con `GET /health`.

4. Para endpoints de negocio, envía headers de identidad:
- X-User-Id (entero positivo)
- X-User-Role (ROL_FUNC | ROL_JEFE | ROL_RRHH | ROL_ADMIN)

Ejemplos rápidos por rol:
- Funcionario: X-User-Id=100, X-User-Role=ROL_FUNC
- Jefatura: X-User-Id=200, X-User-Role=ROL_JEFE
- RRHH: X-User-Id=300, X-User-Role=ROL_RRHH
- Admin: X-User-Id=400, X-User-Role=ROL_ADMIN

OpenAPI JSON:
- http://localhost:5093/swagger/v1/swagger.json
- https://localhost:7129/swagger/v1/swagger.json

Si Swagger no aparece, revisa `Swagger:Enabled` en configuración activa.
~~~

## 4.2 Bloque Tests (propuesta)

~~~md
## Pruebas existentes

Proyecto de pruebas:
- backend/tests/IntegradorMarcas.Tests

Casos actuales:
- UnitTest1.Test1 (placeholder)
- ErrorLogIntegrationTests.LogAsync_DebeInsertarRegistroEnAuditoriaErrorApi (integración)

Comandos:

~~~powershell
# Todas las pruebas
 dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj -v minimal

# Solo integración
 dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "Category=Integration"

# Caso específico
 dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "FullyQualifiedName~ErrorLogIntegrationTests"

# Cobertura
 dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --collect:"XPlat Code Coverage"
~~~

Estado actual conocido:
- Falla de compilación en ErrorLogIntegrationTests.cs (`cleanupSql` no definido).
- Hasta corregirlo, `dotnet test` no ejecutará los casos.
~~~

## 5) Archivos analizados

- `backend/src/IntegradorMarcas.Api/Program.cs`
- `backend/src/IntegradorMarcas.Api/Properties/launchSettings.json`
- `backend/src/IntegradorMarcas.Api/appsettings.Development.json`
- `backend/src/IntegradorMarcas.Api/appsettings.json`
- `backend/src/IntegradorMarcas.Api/appsettings.Production.json`
- `backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs`
- `backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj`
- `backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs`
- `backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj`
- `backend/tests/IntegradorMarcas.Tests/UnitTest1.cs`
- `backend/tests/IntegradorMarcas.Tests/ErrorLogIntegrationTests.cs`
