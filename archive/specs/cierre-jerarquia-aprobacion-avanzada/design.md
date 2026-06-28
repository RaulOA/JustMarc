# Design - cierre-jerarquia-aprobacion-avanzada

## Enfoque

Cierre incremental sobre la base existente, respetando Clean Architecture y la regla de dependencia
(`docs/architecture.md`). Dos frentes:

1. **Casos edge de resolucion (R1-R6).** La logica vive en la TVF `dbo.fn_AprobadoresVigentesPorSolicitante`
   y en el SQL de `JustificacionesSql.GetCurrentApproverBySolicitante`. La TVF ya maneja vigencia,
   `EstadoRegistroId = 1`, multiples niveles (UNION ALL por jerarquia) y precedencia de delegacion (el
   `ORDER BY CASE WHEN Origen='Delegacion' THEN 0 ELSE 1 END` del OUTER APPLY). El cierre **confirma** ese
   comportamiento con tests de integracion contra BD real y, donde el contrato C# lo permite, con tests
   unitarios sobre `JustificacionService` usando un repositorio fake que simula los retornos edge (sin
   aprobador, delegacion, nulos). No se reescribe la TVF salvo que un test edge demuestre un defecto; si lo
   demuestra, se ajusta dentro de `docs/db/02_EstructuraCompleta.sql` (idempotente, `CREATE OR ALTER`).

2. **Validaciones avanzadas de alta/edicion (R7-R14).** Se cierran como guard clauses en
   `AdminAprobacionesService`. Las validaciones de forma (R7-R11) ya existen; el cierre agrega la
   **anti-duplicidad vigente (R12)** y asegura cobertura de test para todas. La unicidad se chequea via un
   nuevo metodo de repositorio que consulta jerarquias activas con la misma combinacion, manteniendo el SQL
   en `AdminAprobacionesSql` (nunca inline en controller/repository).

La autorizacion (R13) sigue siendo guard clause `EnsureAdmin` en Application (sin `[Authorize]`). La
auditoria (R14) reutiliza `IAuditEventRepository` + `IAdminActionAuditRepository` ya inyectados.

## Archivos y firmas

### Application (capa de validacion - cierra R7-R14)

- `backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs` (existente, editar):
  - En `CreateJerarquiaAsync` / `UpdateJerarquiaAsync`, tras `EnsureReferencesForJerarquiaAsync`, invocar
    una nueva verificacion de duplicidad vigente:
    `private async Task EnsureJerarquiaNoDuplicadaAsync(int aprobadorUsuarioId, int estructuraOrganizacionalId, int nivelAprobacion, int? jerarquiaAprobacionIdExcluida, CancellationToken cancellationToken)`
    que lanza `AppException("Ya existe una jerarquia activa para esa combinacion de aprobador, estructura y nivel.", 409)`
    cuando el repositorio reporta duplicado (R12). En `Update`, excluye el propio id.
  - Las guard clauses `ValidateCreateJerarquia` / `ValidateUpdateJerarquia` permanecen (R7, R8, R9) y
    `EnsureReferencesForJerarquiaAsync` (R10, R11). `EnsureAdmin` permanece (R13).

- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminAprobacionesRepository.cs` (existente, editar):
  - Agregar:
    `Task<bool> ExistsJerarquiaActivaDuplicadaAsync(int aprobadorUsuarioId, int estructuraOrganizacionalId, int nivelAprobacion, int? jerarquiaAprobacionIdExcluida, CancellationToken cancellationToken);`

### Infrastructure (SQL y resolucion - soporta R12 y confirma R1-R5)

- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs` (existente, editar):
  - Nueva `const string ExistsJerarquiaActivaDuplicada` (verbatim `@"..."` consistente con el archivo):
    `SELECT` `EXISTS` sobre `Operacion.JerarquiaAprobacion` con
    `EstadoRegistroId = 1 AND AprobadorUsuarioId = @AprobadorUsuarioID AND EstructuraOrganizacionalId = @EstructuraOrganizacionalID AND NivelAprobacion = @NivelAprobacion AND (@JerarquiaAprobacionIDExcluida IS NULL OR JerarquiaAprobacionId <> @JerarquiaAprobacionIDExcluida)`.

- `backend/src/IntegradorMarcas.Infrastructure/Repositories/AdminAprobacionesRepository.cs` (existente,
  editar): implementar `ExistsJerarquiaActivaDuplicadaAsync` con `ExecuteScalarAsync<bool>` (patron
  identico a `ExistsJerarquiaAsync`).

- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs` (existente, **solo se toca si
  un test edge demuestra defecto**): el `OUTER APPLY` + `LEFT JOIN aprobador` ya devuelve fila con
  aprobador nulo cuando no hay scope (soporta R1, R6). No requiere cambio salvo defecto comprobado.

- `docs/db/02_EstructuraCompleta.sql` (existente, **solo se toca si un test edge demuestra defecto** en la
  TVF respecto a R2/R4/R5). Cambios via `CREATE OR ALTER`, idempotentes.

### Tests (cierra R1-R16; sin produccion)

- `backend/tests/IntegradorMarcas.Tests/AdminAprobacionesServiceJerarquiaTests.cs` (nuevo): unitarios con
  repositorio fake, cubren R7-R14 (validaciones, referencias inexistentes, duplicidad, rol no admin,
  auditoria invocada).
- `backend/tests/IntegradorMarcas.Tests/JustificacionServiceCurrentApproverTests.cs` (existente, extender):
  casos edge de resolucion expresables por contrato (R1, R3, R6) con fakes que devuelven aprobador nulo /
  delegacion.
- `backend/tests/IntegradorMarcas.Tests/AprobadoresVigentesTvfIntegrationTests.cs` (nuevo,
  `[Trait("Category","Integration")]`): R2, R4, R5 contra BD real (multiples niveles, fuera de vigencia /
  inactivo, solicitante sin estructura). **Fuera del gate** por defecto (`docs/verification.md`).

> Nota de trazabilidad: el gate (`init.ps1`) corre solo unitarios. R2, R4, R5 quedan cubiertos por tests
> de integracion etiquetados; su no-ejecucion en el gate es deliberada y conforme a `docs/verification.md`.
> R15 exige que todo R1..R14 tenga test nombrado (unitario o integracion), no que todos corran en el gate.

## Errores/excepciones

- `AppException(400)` — validaciones de forma y referencias inexistentes (R7-R11).
- `AppException(409)` — duplicado vigente de jerarquia (R12). Codigo nuevo respecto al 400 generico, para
  distinguir conflicto de estado de error de entrada.
- `AppException(403)` — actor no `ROL_ADMIN` (R13).
- Resolucion sin aprobador (R1, R5, R6) **no** es excepcion: retorna conjunto vacio / `CurrentApproverDto`
  con `Aprobador = null`. Mapeo a `ProblemDetails` via `UseExceptionHandler` solo aplica a las excepciones.

## Alternativas descartadas

- **Imponer unicidad con un UNIQUE INDEX filtrado en `Operacion.JerarquiaAprobacion`** (sobre
  `AprobadorUsuarioId, EstructuraOrganizacionalId, NivelAprobacion WHERE EstadoRegistroId = 1`) en lugar de
  validar en Application. Descartada: trasladaria la regla a un `SqlException` opaco que el handler mapearia
  a 500, perdiendo el mensaje de negocio y el `AppException(409)`; ademas la regla "duplicado **vigente**"
  depende de fechas de vigencia que un indice no modela bien, y el gate (unitarios sin BD) no podria
  verificarla. La validacion en Application es unit-testable y devuelve un error de dominio claro.

- **Reescribir la TVF para que un solicitante sin estructura lance error en vez de devolver vacio.**
  Descartada: contradice R1/R5/R6 (continuidad operativa, RNF-06) y romperia `GetCurrentApprover`, que hoy
  espera aprobador nulo como estado valido. La ausencia de aprobador es un caso de negocio legitimo, no un
  fallo.

- **Mover toda la validacion edge a tests de integracion contra BD.** Descartada: dejaria el gate
  (`init.ps1`, unitarios sin BD) sin cobertura de las validaciones de alta/edicion, violando el espiritu de
  `docs/verification.md`. Se prioriza unitario para R7-R14 e integracion solo para lo que es intrinseco a
  la TVF/SQL (R2, R4, R5).
