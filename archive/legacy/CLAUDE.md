# CLAUDE.md — Justificacion de Marca (INTEGRA_CNP)

Sistema web para gestionar boletas de justificacion de marca (time-mark) de CNP/FANAL. Tres piezas: frontend estatico (HTML/CSS/JS vanilla en la raiz del repo), API REST .NET 8 en `backend/` (Clean Architecture), y base SQL Server `INTEGRA_CNP` en `docs/db/`.

La identidad NO usa JWT/cookies: el frontend envia headers `X-User-Id` / `X-User-Role` que el backend confia. Roles: `ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH`, `ROL_ADMIN`. Flujo de negocio: funcionario crea boleta, jefatura aprueba/rechaza, RRHH consulta global, admin gestiona jerarquias/delegaciones/organizacion.

Idioma de trabajo: ESPANOL (dominio, comentarios, mensajes de UI, specs). Estilo: directo, tecnico, conciso.

## Arquitectura

Backend Clean Architecture (`backend/src/`), regla de dependencia hacia adentro:

```
Domain          (entidades, constantes; SIN referencias)
   ^
Application      (DTOs, Interfaces, Services, Validation, Common) -> Domain
   ^
Infrastructure   (Data, Queries, Repositories)                    -> Domain + Application
   ^
Api              (Controllers, Contracts, Security)               -> Application + Infrastructure
```

- Las interfaces (`IJustificacionRepository`, `IUserContext`, `IErrorLogRepository`, `IAuditEventRepository`, services) se definen en `Application/Interfaces` y se implementan hacia afuera (Infrastructure o Api).
- Tests (`backend/tests/IntegradorMarcas.Tests`) referencian Application + Domain + Infrastructure.
- Frontend estatico: dos paginas (`index.html` login, `dashboard.html` app por roles) comparten un unico script global `app.js` (~2227 lineas) + `style.css`. Sin framework, sin bundler, sin paso de build. Cliente delgado sobre la API REST.
- BD: SQL Server 2022, base `INTEGRA_CNP`, esquemas funcionales `Configuracion`, `RecursosHumanos`, `Operacion`, `Auditoria`, `Integracion`. Bases externas `WIZDOM` / `SIFCNP` solo lectura via vistas de integracion.

No hay archivo `.sln`; todos los comandos `dotnet` referencian rutas `.csproj` explicitas (build/run/test). `restore`/`clean` apuntan a la carpeta `backend/`.

## Comandos esenciales

Todos tienen una VS Code task equivalente en `.vscode/tasks.json` (indicada entre parentesis).

Restore (task `restore`):
```
dotnet restore backend/
```

Build API (task `build-api`, default build task):
```
dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug
```

Run API (task `run-api`, background; requiere build previo por usar `--no-build`):
```
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --no-build
```

Test (task `test`, default test task):
```
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --verbosity=normal --logger "console;verbosity=detailed"
```

Un solo test / subconjunto xUnit (sin task dedicada):
```
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "FullyQualifiedName~NombreDelTest"
```

Solo unitarios, saltando el test de integracion contra BD real:
```
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "Category!=Integration"
```

Cobertura (task `test-coverage`, coverlet):
```
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj /p:CollectCoverage=true /p:CoverageFormat=opencover
```

Servir frontend desde la raiz del repo (task `serve-frontend`, background):
```
powershell -NoProfile -Command "if (Get-Command python -ErrorAction SilentlyContinue) { python -m http.server 8000 --directory . } else { py -m http.server 8000 --directory . }"
```

Liberar el puerto 5093 (task `stop-api-on-5093`):
```
powershell -NoProfile -Command "$conn = Get-NetTCPConnection -LocalPort 5093 -State Listen -ErrorAction SilentlyContinue | Select-Object -First 1; if ($conn) { Stop-Process -Id $conn.OwningProcess -Force; Write-Host 'Stopped process on port 5093.' } else { Write-Host 'Port 5093 already free.' }"
```

Liberar el puerto 8000 del frontend (no hay task dedicada):
```
Get-NetTCPConnection -LocalPort 8000 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
```

Flujo de arranque recomendado: task compuesta `start-full-stack` (`build-api` -> `run-api` -> `serve-frontend`, secuencial). Para apuntar la UI a otra API: `dashboard.html?api=http://localhost:5093`.

Publish para IIS (Release, en maquina de build/CI; el servidor de produccion no tiene SDK):
```
dotnet publish backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj -c Release -o .\artifacts\IntegradorMarcas.Api
```

## Puertos

| Servicio | Puerto | Notas |
|---|---|---|
| API .NET | 5093 (http), 7129 (https) | dev; perfil `http` en `launchSettings.json`; el frontend lo usa como `apiBaseUrl` por defecto |
| Frontend (python http.server) | 8000 | dev; sirve la raiz del repo con `--directory .` |
| IIS local | 8080 | validacion pre-produccion SOLAMENTE; NO forma parte de `start-full-stack` |
| SQL Server | 1433 | infra/prod (puede variar; dev usa instancia `localhost\SQLEXPRESS`) |

Health probe: `GET /health` -> `{status:'ok', utc}`.

## Autenticacion

Basada en headers, sin JWT/cookies. `IUserContext` -> `HeaderUserContext` (`backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs`) lee dos headers (configurables via `Security:HeaderUserId` / `Security:HeaderRole`, default `X-User-Id` / `X-User-Role`):

```
X-User-Id: 6
X-User-Role: ROL_RRHH
```

`GetCurrent()` lanza `AppException(401)` si no hay contexto HTTP, si `X-User-Id` falta / no es entero / `<= 0`, o si `X-User-Role` falta o esta vacio. Todos los endpoints de negocio devuelven **401** si faltan los headers (aplica tambien en Swagger y en los `.http`).

Roles (valores EXACTOS, `backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs`):

| Constante | Valor | Helper | Sinonimos aceptados |
|---|---|---|---|
| `RolFunc` | `ROL_FUNC` | `EsFuncionario` | `FUNCIONARIO`, `1` |
| `RolJefe` | `ROL_JEFE` | `EsJefatura` | `JEFATURA`, `2` |
| `RolRrhh` | `ROL_RRHH` | `EsRrhh` | `RRHH`, `3` |
| `RolAdmin` | `ROL_ADMIN` | `EsAdmin` | `ADMIN`, `4` |

Los helpers normalizan con `Trim().ToUpperInvariant()`.

La **autorizacion** NO usa atributos `[Authorize]` ni middleware de auth: cada metodo de servicio en Application es un guard clause que valida `RolesSistema.Es*(user.Role)` al inicio y lanza `AppException(403)` si no aplica. `ListHistorico` aplica scoping por rol (funcionario forzado a su propio `UserId`; jefatura al set de aprobadores excluyendo el propio; RRHH global).

Cualquiera que pueda fijar los headers ES ese usuario/rol. La flag `Security:UseMockIdentity` existe en config (true en Development) pero NO se consume en codigo: `HeaderUserContext` siempre lee headers reales.

## Acceso a datos

Sin EF Core. ADO.NET crudo (`Microsoft.Data.SqlClient` 7.0.0) + Dapper (2.1.72).

- `ISqlConnectionFactory.CreateConnection()` -> nueva `SqlConnection` desde la connection string nombrada `IntegraCnp` (`backend/src/IntegradorMarcas.Infrastructure/Data/SqlConnectionFactory.cs`). Lanza `InvalidOperationException` si la cadena falta. Base = `INTEGRA_CNP`.
- Patron: `await using var connection = (SqlConnection)_connectionFactory.CreateConnection();` + Dapper `QueryAsync`/`QuerySingleAsync`/`ExecuteAsync`/`ExecuteScalarAsync` con `CommandDefinition`, params anonimos nombrados y `CancellationToken`. `OpenAsync` explicito solo para transacciones/multi-statement (`CreateAsync`, `ErrorLogRepository`, `SessionController.profile`).
- Un repositorio sealed por agregado (`JustificacionRepository`, `AdminAprobacionesRepository`, `AdminOrganizacionRepository`, `AuditEventRepository`, `AdminActionAuditRepository`, `ErrorLogRepository`), cada uno recibe `ISqlConnectionFactory`. Mapean filas a DTOs o a clases `*Row` privadas. Inserts usan `ExecuteScalarAsync<int>` con `SELECT CAST(SCOPE_IDENTITY() AS INT)`; multi-insert usa `BeginTransactionAsync`/Commit/Rollback explicito.
- **Todo el SQL** vive como `const string` en `backend/src/IntegradorMarcas.Infrastructure/Queries/*Sql.cs`: `JustificacionesSql`, `AdminAprobacionesSql`, `AdminOrganizacionSql`, `AuditoriaSql`, `AdminActionAuditSql`. Excepciones que llevan SQL inline en el controlador: `AdminMonitoringController` (UNION ALL grande sobre `Auditoria.ErrorApi` + `Auditoria.EventoAuditoria`) y `SessionController` (SELECT de `NombreCompleto`).
- El scope de aprobacion depende de la TVF `dbo.fn_AprobadoresVigentesPorSolicitante(usuarioId, GETDATE())` (devuelve `AprobadorUsuarioId`/`Origen`/`DeleganteUsuarioId`; `Origen='Delegacion'` tiene prioridad sobre jerarquia).

Errores: `AppException` (sealed, con `StatusCode`) es la unica excepcion de control de flujo (junto a `KeyNotFoundException`). `Program.cs UseExceptionHandler` mapea `AppException`->su status, `KeyNotFoundException`->404, `OperationCanceledException`->499, resto->500. Devuelve `ProblemDetails` con `extensions.correlationId` y header `X-Correlation-Id`. ModelState invalido se canaliza a `AppException(400)`.

Logging de errores (best-effort, fire-and-forget): `IErrorLogRepository.LogAsync` inserta en `Auditoria.ErrorApi`; `StackTrace` solo si `statusCode >= 500`; swallowea toda excepcion para no romper la respuesta. Auditoria de eventos: `IAuditEventRepository.LogEventAsync` -> `Auditoria.EventoAuditoria`; acciones admin -> `IAdminActionAuditRepository.LogActionAsync` con snapshots before/after JSON (`Auditoria.AdminAccionAuditoria`).

## Convenciones de codigo

### C#
- File-scoped namespaces (`namespace X;`). `Nullable` e `ImplicitUsings` habilitados en todos los proyectos. Todos target `net8.0`.
- Clases concretas `sealed` por defecto (controllers, services, repositories, entities, DTOs, exceptions). Unica excepcion: `SessionController`.
- Modelo de objetos en tres niveles: **Api/Contracts** (Requests/Responses, forma del wire) <-> **Application/DTOs** (transporte entre capas) <-> **Domain/Entities** (`JustificacionEncabezado`, `JustificacionDetalle`). Los controllers traducen Request->DTO y DTO->Response a mano con object initializers; sin AutoMapper.
- Naming: propiedades de DTOs/Entities en PascalCase con sufijo `Id` (`JustificacionId`, `UsuarioId`). Los **Contracts** usan deliberadamente sufijo `ID` (`JustificacionID`, `AprobadorID`); los controllers mapean `Id` <-> `ID` a mano. Params SQL en `@UsuarioID`.
- Todo I/O es `async Task` con `CancellationToken` hilado controller -> service -> repository -> Dapper. Colecciones como `IReadOnlyList`/`IReadOnlyCollection`.
- SQL nuevo usa raw string literals (`"""..."""`); el viejo usa verbatim (`@"..."`).
- Espanol en identificadores de dominio, comentarios y mensajes. Doc-comments `///` en acciones publicas de controllers.

### Frontend (`app.js`)
- Vanilla JS, un solo script global, sin modulos/imports. ~50 funciones globales cableadas via `onclick` inline + un unico `DOMContentLoaded`. Toda interaccion con backend pasa por `apiFetch` + `buildApiHeaders` (no hay `fetch()` directos en otros lados).
- UI en espanol. Identificadores mixtos ingles/espanol. Roles como constantes `ROL_*`.
- Claves de `sessionStorage` namespaced con prefijo `sjm_`; global de override `window.SJM_API_BASE_URL`.
- Todo valor interpolado en `innerHTML` pasa por `escapeHtml()`. Fechas via `formatDate`/`formatDateTime` (dd/mm/yyyy), `—` para vacios.

### Base de datos (OBLIGATORIO)
Convenciones formalizadas en `docs/db/Convenciones_Nomeclatura_BD.md`:
- PascalCase en TODO objeto (tablas, columnas, vistas, SP, params, variables). Espanol, descriptivo, sin abreviaciones.
- Tablas en singular, sin prefijos decorativos (`Usuario`, no `tbl_Usuarios`). Formato `Esquema.NombreTabla`.
- Esquema explicito por area funcional (`Configuracion`, `RecursosHumanos`, `Operacion`, `Auditoria`, `Integracion`). `dbo` reservado a sistema (y vistas legadas SIFCNP, ver gotchas).
- PK = `[NombreTabla]Id` (`JustificacionId`). FK = mismo nombre que la PK referenciada.
- Booleanos afirmativos `Es`/`Tiene` (`EsActivo`). Fechas con `Fecha`/`FechaHora`. Codigos externos con `Codigo`.
- Columnas de auditoria obligatorias en entidades de negocio: `CreadoPor`, `FechaHoraCreacion`, `ModificadoPor`, `FechaHoraModificacion`.
- Vistas `v_PascalCase`. Procedimientos `usp_EntidadAccion` (prohibido `sp_`). Params `@PascalCase` identicos a la columna.
- Constraints/indices con nombre explicito (nunca autogenerados). Scripts idempotentes (`IF OBJECT_ID/COL_LENGTH/EXISTS`, `MERGE` para seed, `SET XACT_ABORT ON`).

## Base de datos

Scripts consolidados en `docs/db/`, ejecutar en orden (sqlcmd o SSMS, codificacion UTF-8):

1. `01_CrearBaseDatos.sql` — `CREATE DATABASE INTEGRA_CNP` + los 5 esquemas funcionales.
2. `02_EstructuraCompleta.sql` — TODA la estructura: catalogos, tablas RRHH/Operacion/Auditoria, indices, funcion `dbo.fn_AprobadoresVigentesPorSolicitante`, SP de sincronizacion, vistas `Integracion.*`, vista legada `dbo.V_JUSTIFICACIONES_DETALLE` y shim `dbo.Estructuras_Organizacionales`. Los objetos que dependen de WIZDOM/SIFCNP y de `dbo.RH_*` se crean **con guardas** (`DB_ID`/`OBJECT_ID`): si faltan, se omiten sin abortar.
3. `03_DatosSemilla.sql` — A) catalogos (**obligatorio**); B) demo minimo unidad 120; C) jerarquia de 12 dependencias; D) remediacion mojibake (opcional). En produccion ejecutar **solo la Seccion A**.

Detalles de la consolidacion (2026-06-24) y analisis 3FN: `docs/db/Observaciones_Consolidacion_SQL.md`. Los 10 scripts previos (`001`–`008`, `fix_fn_aprobadores`, `historico_justificaciones`) estan archivados en `docs/db/_legacy/` (no ejecutar); `003` quedo obsoleto (esquema viejo `dbo.*`).

Notas clave: la funcion de aprobadores vive en **`dbo`** (no `Operacion`) porque asi la invoca el backend. `Auditoria.ErrorApi` usa nombres de columna en ingles por contrato C# (desviacion deliberada, NO corregir).

Bases externas `WIZDOM` / `SIFCNP` son **solo lectura**: nunca INSERT/UPDATE/DELETE. La escritura ocurre solo en `INTEGRA_CNP`.

Connection string: clave `IntegraCnp` (no `INTEGRA_CNP`). **NO se versiona en archivos**: se inyecta por la variable de entorno de usuario `ConnectionStrings__IntegraCnp` (.NET la mapea a `ConnectionStrings:IntegraCnp`; el `__` equivale a `:` y las env vars sobrescriben los `appsettings`). `appsettings.Development.json` ya no contiene la cadena. `Program.cs` valida en arranque: en no-Development **aborta** (fail-fast) si falta; en Development **advierte** y continua. Justificacion y fuentes: `docs/seguridad/gestion_credenciales_conexion_bd.md`.

## Flujo de trabajo con Claude (spec-driven)

Modo hibrido:

- **Tareas normales** (fixes acotados, cambios localizados, diagnosticos): Claude trabaja directo usando `Read`/`Edit`/`Grep`/`Glob`. No se delega ni se escribe spec.
- **Features grandes o requests ambiguos**: usar el comando `/spec` para generar primero un spec en `docs/specs/` y revisarlo antes de implementar. Todos los specs del proyecto (historicos y nuevos) viven en `docs/specs/`; seguir la convencion de nombres existente: snake_case minusculas con sufijo `_spec.md`.

> Nota: hasta esta migracion la carpeta se llamaba `docs/SubAgent docs/` (arnes Copilot). Se renombro a `docs/specs/`.

Plantilla de spec (consolidada de los specs existentes):
1. Encabezado `# Spec: <titulo>` + metadata (Fecha, Estado, Objetivo en una linea).
2. Objetivo / Alcance (1-3 vinetas).
3. Supuestos (cuando hay incertidumbre).
4. Hallazgos / Estado actual (citar archivos y lineas concretas; tablas `Funcion|Comportamiento` cuando aplique).
5. Causa exacta (para fixes).
6. Cambios propuestos por capa/archivo (backend Application/Infrastructure/Repository/API o frontend HTML/JS/CSS; firmas, SQL, snippets concretos).
7. Resumen de cambios por archivo (tabla `Archivo|Cambio|Tipo`).
8. Pruebas requeridas (nombres de tests).
9. Riesgos y consideraciones.
10. Criterios de aceptacion (lista verificable y numerada).
11. (features grandes) Plan incremental por fases.
12. Archivo de salida (ruta del propio spec).

Subagentes **opcionales**, solo cuando aportan paralelismo o aislamiento de contexto:
- `spec-researcher`: lee el codigo, analiza y produce el spec segun la plantilla.
- `implementer`: en contexto fresco, lee la ruta del spec e implementa.

No son obligatorios: a diferencia del arnes Copilot previo, el agente principal SI usa sus herramientas directamente.

## Gotchas

- **Windows/PowerShell**: todos los helpers de servir/liberar puertos son PowerShell-only (`Get-NetTCPConnection`, `Stop-Process`). El flujo asume Windows 10/11 + PowerShell.
- **Sin .sln**: `dotnet build`/`test` sin ruta de proyecto no encuentran nada. Pasar siempre el `.csproj` explicito.
- **SDK vs target**: la maquina tiene .NET SDK 10.0.301 pero todos los proyectos son `net8.0` (roll-forward). Debe existir el targeting/runtime pack de net8.0 o el build falla. No asumir runtime net10.
- **`run-api` usa `--no-build`**: corre binarios potencialmente viejos si no se hizo `build-api` antes. Los launch.json ejecutan la DLL prebuilt (`bin/Debug/net8.0/IntegradorMarcas.Api.dll`), no `dotnet run`.
- **Sin auth real**: `handleLogin()` (`app.js:521`) nunca envia el password; solo valida `length>=4` localmente. Cualquier password funciona; el rol se deriva del string del username. El backend confia en headers triviales de spoofear. Diseno de MVP/demo inseguro.
- **Credenciales quemadas**: `MOCK_USER_DIRECTORY` (`app.js:141`) y usernames de ejemplo impresos en `index.html` (funcionario.ana, jefe.maria, rrhh.carlos, admin.sofia, admin.demo). Usuarios desconocidos caen a `userId:10` (magic default que puede colisionar). Varios usernames comparten userId (jefe.maria/jefe.ricardo->3, rrhh.carlos/rrhh.sandra->6, admin.sofia/admin.demo->1).
- **CORS abierto**: la politica `LocalFrontend` permite CUALQUIER origin/header/method (`SetIsOriginAllowed(_ => true)`). El propio codigo marca que debe restringirse en despliegues expuestos.
- **Mojibake/encoding**: `normalizeMojibakeTemporaryForHistoryDetail()` (`app.js:446`) es un parche que reemplaza UTF-8-como-Latin1 (`Ã¡`->`á`, etc.) SOLO en el detalle de historial de funcionario, no en Jefatura/RRHH/SIFCNP. Bug de encoding del backend sin resolver; ordering fragil. Del lado BD, la remediacion idempotente esta en la Seccion D de `03_DatosSemilla.sql`.
- **BD externas**: el scope de aprobacion depende de la TVF `dbo.fn_AprobadoresVigentesPorSolicitante` y de tablas/vistas (`Operacion.*`, `RecursosHumanos.*`, `Configuracion.*`, `dbo.Estructuras_Organizacionales`) que viven en la BD. `dbo.Estructuras_Organizacionales` la usa UNA sola consulta (`JustificacionesSql.GetDetalleJefaturaEncabezado`) de forma inconsistente con el resto del codigo; `02_EstructuraCompleta.sql` provee un shim guardado. Recomendado migrar esa consulta a `RecursosHumanos.EstructuraOrganizacional` (ver `Observaciones_Consolidacion_SQL.md`).
- **Funcion de aprobadores en `dbo`**: el esquema VIGENTE es `dbo.fn_AprobadoresVigentesPorSolicitante` (lo que invoca el backend). La version `Operacion.fn_...` quedo obsoleta; `02_EstructuraCompleta.sql` crea la de `dbo` y dropea la de `Operacion` si existe.
- **Vista legada**: `dbo.V_JUSTIFICACIONES_DETALLE` (ahora en la Seccion I de `02_EstructuraCompleta.sql`) usa nomenclatura MAYUSCULAS_SNAKE en esquema `dbo` a proposito (compatibilidad SIFCNP). No aplicar PascalCase aqui.
- **Test de integracion con cadena quemada**: `ErrorLogIntegrationTests.cs` tiene un connection string hardcodeado (`Server=WinDev2407Eval\SQLEXPRESS`) que no coincide con otras maquinas; gateado por `[Trait Category=Integration]` y golpea una BD real. Saltarlo con `--filter "Category!=Integration"`.
- **Frontend expone el repo entero**: `serve-frontend` sirve desde `--directory .` (raiz), exponiendo backend, docs y scripts SQL sobre `:8000` en dev. Sin live reload ni SPA fallback (404 en rutas desconocidas).
- **`SessionController` y `AdminMonitoringController` rompen convenciones**: toman `ISqlConnectionFactory` directo y corren SQL inline; Session hace su propio parseo de headers en vez de usar `IUserContext`.
- **Convenciones bypass de auth**: no hay atributos `[Authorize]` ni middleware; la autorizacion solo se aplica dentro de los servicios de Application.
- **Skew de paquetes**: `Microsoft.Extensions.Configuration.Abstractions` v10.0.7 en Infrastructure mientras se target net8.0 (tests pin Configuration v8.0.0).
- **Leftovers de scaffolding**: `Class1.cs` (Application/Domain/Infrastructure) y `UnitTest1.cs` siguen en el arbol. Tambien una copia sin seguimiento `docs/PRP_Justificacion_Marcas - Copy.md` (el canonico es `docs/PRP_Justificacion_Marcas.md`).
