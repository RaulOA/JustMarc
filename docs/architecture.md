# docs/architecture.md — Qué es "buen trabajo" en este proyecto

> Leído del código real. Los subagentes referencian este archivo en vez de rutas fijas.

## Piezas

- **Frontend estático** (raíz del repo): `index.html` (login), `dashboard.html` (app por roles),
  `app.js` (script global único, vanilla JS, sin framework/bundler), `style.css`.
- **API .NET 8** (`backend/src/`), Clean Architecture en 4 proyectos.
- **BD SQL Server** `INTEGRA_CNP` (scripts en `docs/db/`).

## Capas backend y regla de dependencia (hacia adentro)

```text
Domain          (entidades, constantes; SIN referencias)
   ^
Application      (DTOs, Interfaces, Services, Validation, Common) -> Domain
   ^
Infrastructure   (Data, Queries, Repositories)                    -> Domain + Application
   ^
Api              (Controllers, Contracts, Security)               -> Application + Infrastructure
```

- Las interfaces se definen en `Application/Interfaces` y se implementan **hacia afuera**
  (Infrastructure o Api).
- Modelo de objetos en 3 niveles: **Api/Contracts** (wire) ↔ **Application/DTOs** (transporte) ↔
  **Domain/Entities**. Mapeo a mano (sin AutoMapper).

## Rutas de código y tests

- Frontend: `index.html`, `dashboard.html`, `app.js`, `style.css` (raíz).
- Backend: `backend/src/IntegradorMarcas.{Domain,Application,Infrastructure,Api}`.
- Tests: `backend/tests/IntegradorMarcas.Tests/` (xUnit).
- BD: `docs/db/` (`01_CrearBaseDatos.sql`, `02_EstructuraCompleta.sql`, `03_DatosSemilla.sql`,
  `_legacy/`).
- Comandos reales: `HARNESS-INSTALL.md`.

## Acceso a datos

- **Sin EF Core.** ADO.NET (`Microsoft.Data.SqlClient`) + Dapper.
- `ISqlConnectionFactory.CreateConnection()` → `SqlConnection` desde la cadena `IntegraCnp`.
- Un repositorio `sealed` por agregado, recibe `ISqlConnectionFactory`.
- **Todo el SQL** vive como `const string` en `Infrastructure/Queries/*Sql.cs`. (Excepciones inline
  históricas: `AdminMonitoringController`, `SessionController`.)
- Inserts: `ExecuteScalarAsync<int>` + `SELECT CAST(SCOPE_IDENTITY() AS INT)`; multi-insert con
  transacción explícita.

## Autenticación / autorización

- Sin JWT/cookies: headers `X-User-Id` / `X-User-Role` (los lee `HeaderUserContext` vía `IUserContext`).
- Roles: `ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH`, `ROL_ADMIN` (constantes en
  `Domain/Constants/RolesSistema.cs`).
- La autorización es **guard clause** dentro de cada servicio de Application (no hay `[Authorize]` ni
  middleware): valida `RolesSistema.Es*` y lanza `AppException(403)`.

## Manejo de errores

- `AppException` (sealed, con `StatusCode`) es la única excepción de control de flujo (+
  `KeyNotFoundException`).
- `Program.cs UseExceptionHandler` mapea a `ProblemDetails` con `correlationId`.
- Logging best-effort: `IErrorLogRepository.LogAsync` → `Auditoria.ErrorApi`; auditoría de eventos →
  `Auditoria.EventoAuditoria`.

## Qué NO hacer

- ❌ Meter SQL en controllers (va en `Queries/*Sql.cs` + Repository).
- ❌ Romper la regla de dependencia (p.ej. Domain referenciando Infrastructure).
- ❌ Escribir en `WIZDOM`/`SIFCNP` (son **solo lectura**; toda escritura va a `INTEGRA_CNP`).
- ❌ Versionar la cadena de conexión (se inyecta por env var `ConnectionStrings__IntegraCnp`).
- ❌ Asumir runtime net10: los proyectos son `net8.0`.
- ❌ Aplicar PascalCase a la vista legada `dbo.V_JUSTIFICACIONES_DETALLE` (compatibilidad SIFCNP).
