# Exception Handling and Notification Spec

Fecha: 2026-04-23
Scope: backend API exception handling, frontend user feedback, DB error/audit persistence, visual design consistency.

## 1) Findings Summary (Current State)

### 1.1 Backend error handling

- `backend/src/IntegradorMarcas.Api/Program.cs`
  - Uses `app.UseExceptionHandler(...)` with inline handler.
  - Maps:
    - `AppException` -> `statusCode` + message
    - `KeyNotFoundException` -> 404
    - default -> 500 with generic title `Error interno del servidor`
  - Returns `Results.Problem(title: ..., statusCode: ...)`.
  - No structured error code, no trace id in payload, no logging side-effect, no environment-specific detail handling.

- `backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs`
  - Throws `AppException` (401) when auth headers are missing/invalid.

- Controllers (`backend/src/IntegradorMarcas.Api/Controllers/*.cs`)
  - `JustificacionesController`, `JefaturaController`, `RrhhController` do not use `try/catch`.
  - They rely fully on service exceptions + global middleware for error responses.

- Services (`backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`)
  - Domain/business validations throw `AppException` with explicit HTTP status (400/403/404/409).
  - This is currently the primary business error path.

- Validators (`backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs`)
  - Throws `AppException` for invalid payload/filters.

- Infrastructure (`backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs`)
  - Transactional `CreateAsync` uses `try/catch` only to rollback and rethrow.
  - No translation of SQL exceptions to app-level error codes.
  - No technical error log persistence.

### 1.2 Frontend user feedback and notifications

- `app.js`
  - Core notice helper: `showNotice(targetId, type, msg)`.
  - Uses inline panel notice placeholders (`f-notice`, `j-notice`, `rrhh-notice`) and simple success/error variants.
  - Auto-hide fixed at 5s; no queue/stack, no dedupe, no manual close, no action button.
  - Login feedback is direct DOM update of `#loginError` in `handleLogin()`.
  - API errors are normalized in `parseApiError()` + `apiFetch()` with timeout/network friendly messages.

- `dashboard.html`
  - Contains `div.alert` placeholders:
    - `#f-notice`
    - `#j-notice`
    - `#rrhh-notice`

- `index.html`
  - Contains login alert `#loginError` with class `alert alert-error`.

- `style.css`
  - Alert styles available:
    - `.alert`
    - `.alert-error`
    - `.alert-success`
  - No toast container/pattern yet.

### 1.3 Existing DB error/audit tables in SQL scripts

- `docs/db/001_init_integra_cnp.sql`
  - No dedicated error log or audit history table.
  - Transactional tables have standard insert metadata (`Usr_Registro`, `Fec_Registro`) but no error/audit timeline.

- `docs/db/007_integra_local_bridge.sql`
  - Has ETL/bridge control table `stg.BridgeCargaLote` with:
    - `EstadoCarga`
    - `DetalleError`
  - This is operational load-control logging for bridge/staging only, not API runtime exception logging.

- `docs/db/002_006` scripts
  - Extraction focused; no persistent API exception/audit table found.

### 1.4 Visual style references (for notification design)

From `style.css`:

- Typography:
  - `--font-main: 'IBM Plex Sans', sans-serif`
  - `--font-mono: 'IBM Plex Mono', monospace`

- Core palette:
  - Institutional blues (`--blue-900` ... `--blue-50`)
  - Neutral greys (`--grey-900` ... `--grey-50`)

- Semantic colors:
  - `--success`, `--success-bg`
  - `--danger`, `--danger-bg`
  - `--warning`, `--warning-bg`
  - `--info`, `--info-bg`

- UI tokens:
  - Radii: `--radius-sm/md/lg`
  - Shadows: `--shadow-sm/md/lg`
  - Transition: `--transition`

- Existing alert shape:
  - Left border accent + tinted background + icon/text row.

## 2) Gaps Identified

1. Backend returns minimal problem details; missing consistent machine-readable `code` and `traceId`.
2. No centralized structured logging contract for exceptions.
3. No DB-level API error log table.
4. Frontend notifications are tied to specific placeholders, not reusable global API.
5. No toast stack for cross-panel/global notifications.

## 3) Proposed Target Design

### 3.1 Backend global exception handler shape

Response format: RFC7807-compatible `ProblemDetails` with extension fields.

Example payload:

```json
{
  "type": "https://httpstatuses.com/409",
  "title": "RN-04: la boleta ya fue resuelta y no puede modificarse.",
  "status": 409,
  "detail": "RN-04: la boleta ya fue resuelta y no puede modificarse.",
  "instance": "/api/jefatura/justificaciones/123/resolver",
  "traceId": "00-7f0f...-...-01",
  "code": "BOLETA_ESTADO_INVALIDO",
  "timestampUtc": "2026-04-23T18:25:10Z"
}
```

Mapping proposal:

- `AppException` -> status from exception, code from `AppException.Code` (new), safe detail.
- `ValidationException` (if introduced later) -> 400.
- `SqlException` -> 503 for transient or 500 for unknown DB failures, generic public detail.
- Any other `Exception` -> 500 generic.

Logging proposal:

- `ILogger` with scope fields: `TraceId`, `Path`, `Method`, `UserId`, `Role`.
- Persist technical log row via repository for 5xx and optionally selected 4xx (configurable).

### 3.2 Frontend notification API shape

New global notifier API in `app.js`:

```js
notify.show({
  type: 'success' | 'error' | 'warning' | 'info',
  title: 'Operacion completada',
  message: 'Boleta JM-0007 aprobada.',
  durationMs: 5000,
  sticky: false,
  context: 'jefatura',
  actionLabel: 'Ver detalle',
  onAction: () => {}
});

notify.success(message, options)
notify.error(message, options)
notify.warning(message, options)
notify.info(message, options)
notify.dismiss(id)
notify.clear(context?)
```

Behavior:

- Toast stack fixed top-right on desktop, full-width top on mobile.
- Max visible toasts (e.g. 4), queue overflow strategy.
- Auto-dismiss except `sticky=true`.
- Accessible with `aria-live="polite"` and keyboard-close.
- Keep `showNotice(...)` as compatibility wrapper during migration.

### 3.3 Proposed DB table for error log

New script proposal: `docs/db/008_api_error_log.sql`

Table proposal: `dbo.ApiErrorLog`

Suggested columns:

- `ApiErrorLogID BIGINT IDENTITY PRIMARY KEY`
- `TraceId VARCHAR(100) NOT NULL`
- `OccurredUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()`
- `Environment VARCHAR(30) NOT NULL`
- `Method VARCHAR(10) NOT NULL`
- `Path VARCHAR(500) NOT NULL`
- `StatusCode INT NOT NULL`
- `ErrorCode VARCHAR(80) NULL`
- `MessagePublic VARCHAR(1000) NULL`
- `ExceptionType VARCHAR(200) NULL`
- `ExceptionMessage VARCHAR(2000) NULL`
- `StackTrace VARCHAR(MAX) NULL`
- `UserID INT NULL`
- `UserRole VARCHAR(30) NULL`
- `PayloadJson NVARCHAR(MAX) NULL`
- `Usr_Registro VARCHAR(50) NOT NULL DEFAULT 'api'`
- `Fec_Registro DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()`

Recommended indexes:

- `IX_ApiErrorLog_OccurredUtc` on `OccurredUtc DESC`
- `IX_ApiErrorLog_TraceId` on `TraceId`
- `IX_ApiErrorLog_StatusCode_OccurredUtc` on `(StatusCode, OccurredUtc DESC)`

Optional retention job:

- Purge rows older than N days (e.g. 90/180) via SQL Agent job.

## 4) Exact Files To Edit

### 4.1 Backend

Edit:

- `backend/src/IntegradorMarcas.Api/Program.cs`
  - Replace inline exception lambda with centralized handler registration.
  - Add logging + problem details enrichment pipeline.

- `backend/src/IntegradorMarcas.Application/Common/AppException.cs`
  - Add optional `Code` property (`string?`) and constructor overload.

- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`
  - Assign stable business error codes where rules already exist (RN-01, RN-03, RN-04, etc.).

- `backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs`
  - Add stable codes for validation failures.

Create:

- `backend/src/IntegradorMarcas.Api/Middleware/GlobalExceptionHandlerMiddleware.cs`
  - Centralized exception-to-problem-details mapping and structured logging.

- `backend/src/IntegradorMarcas.Api/Contracts/Responses/ApiProblemDetailsExtensions.cs`
  - Optional typed constants/helper for extension field names (`code`, `traceId`, `timestampUtc`).

- `backend/src/IntegradorMarcas.Application/Interfaces/IErrorLogRepository.cs`
  - Contract for persisting error log rows.

- `backend/src/IntegradorMarcas.Infrastructure/Repositories/ErrorLogRepository.cs`
  - Dapper insert implementation into `dbo.ApiErrorLog`.

- `backend/src/IntegradorMarcas.Application/DTOs/ApiErrorLogEntryDto.cs`
  - DTO for log persistence.

- `backend/src/IntegradorMarcas.Infrastructure/Queries/ErrorLogSql.cs`
  - SQL insert statement(s).

### 4.2 Frontend

Edit:

- `app.js`
  - Add `notify` module (toast API).
  - Keep/adapt `showNotice` to call `notify.show`.
  - Map API problem details fields (`code`, `traceId`) into user-visible and debug-friendly text.

- `dashboard.html`
  - Add global toast container root element near top-level layout.

- `index.html`
  - Add same toast container (or shared root) and route login errors through notify API.

- `style.css`
  - Add toast component styles aligned with existing tokens.
  - Add variants: success/error/warning/info.
  - Add enter/exit animation using existing transition cadence.

### 4.3 DB scripts/docs

Create:

- `docs/db/008_api_error_log.sql`
  - `dbo.ApiErrorLog` DDL + indexes + optional retention notes.

Optional docs update:

- `docs/Guia_Implementacion_Dev_Prod.md`
  - Add section for exception logging behavior and retention policy.

## 5) Notification UI Design (Consistent with Existing Style)

Design reference rules:

- Use existing `IBM Plex Sans` and token colors.
- Prefer `var(--white)` surface + left border accent pattern from `.alert-*`.
- Toast card:
  - Background `var(--white)`
  - Border `1px solid var(--grey-200)`
  - Left accent per type (`--success`, `--danger`, `--warning`, `--info`)
  - Shadow `var(--shadow-md)`
  - Radius `var(--radius-md)`
- Text hierarchy:
  - Title `.85rem/.9rem`, semibold
  - Body `.8rem/.85rem`, `var(--grey-700)`
  - Optional trace line in mono `var(--font-mono)`
- Motion:
  - Enter: slight translateY + fade (~180ms)
  - Exit: fade + collapse
- Placement:
  - Desktop: top-right with safe margin below topbar in dashboard
  - Mobile: top full-width stack with horizontal padding

## 6) Implementation Order (Recommended)

1. Add DB script `008_api_error_log.sql`.
2. Implement backend middleware + app exception code support.
3. Add error log repository and wire in DI.
4. Refactor frontend to new `notify` API while preserving current calls.
5. Migrate login/dashboard messages to toast style.
6. Validate with manual scenarios: 400/401/403/404/409/500 + network timeout.

## 7) Acceptance Criteria

- All API errors return uniform problem details with `traceId` and optional `code`.
- Business rule failures preserve current HTTP statuses and user-understandable message.
- Technical exceptions are logged via ILogger and persisted in `dbo.ApiErrorLog`.
- Frontend shows consistent non-blocking toasts in all panels and login.
- Visual design remains coherent with existing institutional palette/typography/tokens.

## 8) Risks and Notes

- Exposing internal exception details in production must be avoided; use environment gating.
- Logging full payloads may include sensitive data; mask or store selectively.
- Toast overuse can create noise; enforce dedupe and max concurrent toasts.
