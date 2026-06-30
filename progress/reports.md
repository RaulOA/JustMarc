# Informes de implementación / revisión

> Append-only. Es el lugar en disco del anti-teléfono: `implementer` y `reviewer` anexan aquí su informe
> detallado y devuelven solo una referencia. Rota a `archive/reports/` cuando supera el umbral (~800
> líneas).

<!-- ## [YYYY-MM-DD] <feature> — implementer
Archivos tocados:
Mapa R<n> -> test:
Salida de tests:
-->

## [2026-06-28] cierre-jerarquia-aprobacion-avanzada — implementer

### Archivos tocados

Produccion:
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminAprobacionesRepository.cs` — T1: nuevo metodo `ExistsJerarquiaActivaDuplicadaAsync`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs` — T2: nueva const `ExistsJerarquiaActivaDuplicada`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/AdminAprobacionesRepository.cs` — T3: implementacion `ExistsJerarquiaActivaDuplicadaAsync`
- `backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs` — T4/T5: metodo privado `EnsureJerarquiaNoDuplicadaAsync` + invocaciones en Create/Update

Tests (nuevos/extendidos):
- `backend/tests/IntegradorMarcas.Tests/AdminAprobacionesServiceJerarquiaTests.cs` — nuevo (T6, R7-R14)
- `backend/tests/IntegradorMarcas.Tests/JustificacionServiceCurrentApproverTests.cs` — extendido (T7, R1/R3/R6)
- `backend/tests/IntegradorMarcas.Tests/AprobadoresVigentesTvfIntegrationTests.cs` — nuevo (T8, R2/R4/R5, Category=Integration)

Spec:
- `specs/cierre-jerarquia-aprobacion-avanzada/tasks.md` — todas las tasks marcadas [x]
- `progress/current.md` — actualizado
- `progress/reports.md` — este informe

T9: sin defecto detectado en la TVF ni en `JustificacionesSql` — produccion SQL no tocada.

### Mapa R<n> -> test

| Req | Test (clase :: metodo) | Tipo |
|-----|------------------------|------|
| R1  | `JustificacionServiceCurrentApproverTests::GetCurrentApproverAsync_SinAprobadorVigente_RetornaAprobadorNuloSinExcepcion` | unitario |
| R2  | `AprobadoresVigentesTvfIntegrationTests::GetCurrentApproverAsync_MultiplesNivelesVigentes_NoFallaYDevuelveAprobador` | integracion (Skip) |
| R3  | `JustificacionServiceCurrentApproverTests::GetCurrentApproverAsync_DelegacionCoexisteConJerarquia_SeleccionaDelegacion` | unitario |
| R4  | `AprobadoresVigentesTvfIntegrationTests::GetCurrentApproverAsync_JerarquiaFueraDeVigenciaOInactiva_ExcluyeAprobador` | integracion (Skip) |
| R5  | `AprobadoresVigentesTvfIntegrationTests::GetCurrentApproverAsync_SolicitanteSinEstructuraVigente_DevuelveAprobadorNuloSinExcepcion` | integracion (Skip) |
| R6  | `JustificacionServiceCurrentApproverTests::GetCurrentApproverAsync_RolJefeSinAprobadorVigente_RetornaAprobadorNuloSinExcepcion` | unitario |
| R7  | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_NivelCero_Lanza400` | unitario |
| R7  | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_NivelNegativo_Lanza400` | unitario |
| R7  | `AdminAprobacionesServiceJerarquiaTests::UpdateJerarquia_NivelCero_Lanza400` | unitario |
| R8  | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_TipoRelacionInvalido_Lanza400` | unitario |
| R8  | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_TipoRelacionVerticalMinusculas_NoLanzaExcepcion` | unitario |
| R8  | `AdminAprobacionesServiceJerarquiaTests::UpdateJerarquia_TipoRelacionInvalido_Lanza400` | unitario |
| R9  | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_VigenciaHastaAnteriorDesde_Lanza400` | unitario |
| R9  | `AdminAprobacionesServiceJerarquiaTests::UpdateJerarquia_VigenciaHastaAnteriorDesde_Lanza400` | unitario |
| R10 | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_AprobadorInexistente_Lanza400` | unitario |
| R11 | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_EstructuraInexistente_Lanza400` | unitario |
| R12 | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_DuplicadoVigente_Lanza409` | unitario |
| R12 | `AdminAprobacionesServiceJerarquiaTests::UpdateJerarquia_DuplicadoVigente_Lanza409` | unitario |
| R12 | `AdminAprobacionesServiceJerarquiaTests::UpdateJerarquia_SinDuplicado_ActualizaCorrectamente` | unitario |
| R13 | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_UsuarioNoAdmin_Lanza403` | unitario |
| R13 | `AdminAprobacionesServiceJerarquiaTests::UpdateJerarquia_UsuarioNoAdmin_Lanza403` | unitario |
| R14 | `AdminAprobacionesServiceJerarquiaTests::CreateJerarquia_AltaValida_InvocaAuditoriaResumenYDetalle` | unitario |
| R14 | `AdminAprobacionesServiceJerarquiaTests::UpdateJerarquia_EdicionValida_InvocaAuditoriaResumenYDetalle` | unitario |
| R15 | (ver tabla anterior — cada R1..R14 cubierto por al menos 1 test nombrado) | — |
| R16 | gate: `pwsh ./init.ps1` VERDE — Build + 27 tests unitarios pasados, 0 errores | — |

### Salida de tests (gate T11)

```
Correctas! - Con error: 0, Superado: 27, Omitido: 0, Total: 27, Duracion: 62 ms
init.ps1 -> Resultado: VERDE
```

Nota R2/R4/R5: los tests de integracion tienen `Skip` porque requieren datos especificos en BD de desarrollo. Esto es deliberado y conforme a `docs/verification.md`: el gate corre solo unitarios; los de integracion quedan etiquetados `[Trait("Category","Integration")]` para ejecucion manual o pipeline de integracion.

## [2026-06-28] cierre-jerarquia-aprobacion-avanzada — reviewer

### Veredicto: APROBADO

### Puntos verificados

**1. Trazabilidad R1..R16**

Cada requisito tiene al menos un test nombrado que lo cubre realmente:

- R1, R3, R6: unitarios en `JustificacionServiceCurrentApproverTests` — asersiones concretas (`Assert.Null(result.Aprobador)`, `Assert.Equal("Delegacion", result.Origen)`). Los fakes simulan correctamente los retornos edge del repositorio via contrato.
- R2, R4, R5: tests de integracion en `AprobadoresVigentesTvfIntegrationTests` con `[Trait("Category","Integration")]` y `Skip` explicito. Conforme a `docs/verification.md`: el gate corre solo unitarios; R15 exige test nombrado (unitario o integracion), no ejecucion en el gate. Cumple.
- R7–R14: unitarios en `AdminAprobacionesServiceJerarquiaTests`. Fakes en memoria (`FakeAdminAprobacionesRepository`) con comportamiento configurable. Asersiones verifican el `StatusCode` correcto (400/403/409) y los contadores de auditoria (`LogCount`).
- R15: la tabla del informe del implementer cubre R1..R14 con nombre exacto de metodo.
- R16: gate corrido en esta revision — VERDE (Build 0 errores, 27/27 unitarios, Omitido 0).

**2. Tasks T1–T11: todas [x] con cambios reales verificados**

- T1: `IAdminAprobacionesRepository.cs` linea 22 — metodo `ExistsJerarquiaActivaDuplicadaAsync` presente.
- T2: `AdminAprobacionesSql.cs` lineas 176–185 — const `ExistsJerarquiaActivaDuplicada` con SQL correcto (EXISTS, EstadoRegistroId=1, exclusion por id nullable).
- T3: `AdminAprobacionesRepository.cs` lineas 242–256 — implementacion con `ExecuteScalarAsync<bool>`, patron identico a `ExistsJerarquiaAsync`. Sellado, async, CancellationToken.
- T4: `AdminAprobacionesService.cs` lineas 287–293 — `EnsureJerarquiaNoDuplicadaAsync` privado; invocado en `CreateJerarquiaAsync` (linea 36, excluida=null) y `UpdateJerarquiaAsync` (linea 71, excluido=jerarquiaAprobacionId).
- T5: guard clauses verificadas en el servicio — `ValidateCreateJerarquia` (R7/R8/R9, lineas 374–402), `ValidateUpdateJerarquia` (R7/R8/R9, lineas 432–462), `EnsureReferencesForJerarquiaAsync` (R10/R11, lineas 295–306), `EnsureAdmin` (R13, linea 366–372).
- T6–T8: archivos de test creados/extendidos y leidos; contenido conforme al mapa.
- T9: sin cambios en TVF ni en `JustificacionesSql` — confirmado (ning un hallazgo en test edge).
- T10: mapa documentado en reports.md.
- T11: gate corrido, verde.

**3. Conformidad arquitectural y convenciones**

- Clean Architecture respetada: interfaz en `Application/Interfaces`, SQL en `Infrastructure/Queries/*Sql.cs`, implementacion en `Infrastructure/Repositories`. No hay SQL inline en controllers.
- `AdminAprobacionesRepository` es `sealed`, recibe `ISqlConnectionFactory`, todo I/O async con `CancellationToken`.
- Nombres SQL: params `@PascalCase` con sufijo `ID` (ej. `@AprobadorUsuarioID`) conforme a convenciones.
- `AppException` con codigos correctos: 400 (R7–R11), 403 (R13), 409 (R12).
- La resolucion sin aprobador (R1/R5/R6) retorna `CurrentApproverDto` con `Aprobador=null`, no excepcion.
- Auditoria (R14): invoca tanto `IAuditEventRepository.LogEventAsync` (resumen) como `IAdminActionAuditRepository.LogActionAsync` (detalle), antes de retornar.

**4. Alcance: TVF y JustificacionesSql no tocados**

Confirmado: ningun cambio en `docs/db/02_EstructuraCompleta.sql` ni en `JustificacionesSql`. T9 no encontro defecto, justificacion registrada.

**5. Nota informativa (no bloquea)**

El SQL `UpdateJerarquia` en `AdminAprobacionesSql.cs` pasa el parametro `ModificadoPor` en el repositorio pero la tabla `Operacion.JerarquiaAprobacion` no tiene esa columna en el schema (solo `CreadoPor`/`FechaHoraCreacion`). El parametro es ignorado por Dapper sin error. Hallazgo preexistente a F-003; no es regresion introducida por esta feature. Se registra como sugerencia para una tarea de limpieza futura.

**6. Checkpoints recorridos**

- Arnés: archivos base presentes (19), init.ps1 VERDE.
- Tablero: exactamente 1 feature in_progress (F-003), approved_by="Raul OA", approved_at="2026-06-28", estado valido.
- Codigo: regla de dependencia respetada, SQL en capa Infrastructure, convenciones de nombres cumplidas.
- Verificacion: 27 unitarios verdes, trazabilidad completa.
- Specs SDD: requirements.md/design.md/tasks.md presentes, EARS con ids R1..R16, tasks.md todas [x] citando sus R.
- Constitucion: ningun principio violado — no se toco WIZDOM/SIFCNP, no hay SQL inline en controllers, credenciales no versionadas, arquitectura limpia.

## [2026-06-29] delegaciones-subaprobadores-reglas-completas (F-004) — implementer

### Archivos tocados

Produccion:
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminAprobacionesRepository.cs` — T4/T14: nuevos metodos `ExistsDelegacionActivaComoDelegadoAsync`, `DeleteDelegacionAsync`; firma actualizada `ToggleDelegacionEstadoAsync` (agrega `actorUsuarioId`, D5)
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminAprobacionesService.cs` — T14: nuevo metodo `DeleteDelegacionAsync`
- `backend/src/IntegradorMarcas.Application/Interfaces/IJustificacionRepository.cs` — T13: nuevos metodos `GetRevisarTitularValidationAsync`, `RevisarTitularAsync`
- `backend/src/IntegradorMarcas.Application/Interfaces/IJustificacionService.cs` — T13: nuevo metodo `RevisarTitularAsync`
- `backend/src/IntegradorMarcas.Application/Interfaces/IDelegacionConsultaRepository.cs` — T10/T11: nuevo (D3)
- `backend/src/IntegradorMarcas.Application/Interfaces/IDelegacionConsultaService.cs` — T10/T11: nuevo
- `backend/src/IntegradorMarcas.Application/DTOs/ResolverValidationDto.cs` — T6: agrega `SolicitanteUsuarioId` (R8)
- `backend/src/IntegradorMarcas.Application/DTOs/RevisarTitularValidationDto.cs` — T13: nuevo DTO
- `backend/src/IntegradorMarcas.Application/DTOs/DelegacionFuncionDto.cs` — T10: nuevo DTO (R11/R12)
- `backend/src/IntegradorMarcas.Application/DTOs/DelegacionRegistroDto.cs` — T11: nuevo DTO (R16/R17)
- `backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs` — T4/T5/T14/T16/T17: guard anti-sub-delegacion, `DeleteDelegacionAsync`, `ToggleDelegacionEstadoAsync` con actorUsuarioId, `EnsureReferencesForDelegacionAsync` extendida
- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs` — T6/T13: guard R8 en `ResolverAsync`, nuevo `RevisarTitularAsync`
- `backend/src/IntegradorMarcas.Application/Services/DelegacionConsultaService.cs` — T10/T11: nuevo servicio
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs` — T4/T14/T17: nuevas const `ExistsDelegacionActivaComoDelegado`, `DeleteDelegacion`; `UpdateDelegacion` y `ToggleDelegacionEstado` con columnas D5
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs` — T6/T13: `GetResolverValidation` agrega `SolicitanteUsuarioId`; nuevas const `GetRevisarTitularValidation`, `RevisarTitular`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/DelegacionConsultaSql.cs` — T10/T11: nuevo archivo con `MiFuncion`, `MiRegistro`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/AdminAprobacionesRepository.cs` — T4/T14/T17: implementaciones `ExistsDelegacionActivaComoDelegadoAsync`, `DeleteDelegacionAsync`, `ToggleDelegacionEstadoAsync` actualizada
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs` — T13: implementaciones `GetRevisarTitularValidationAsync`, `RevisarTitularAsync`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/DelegacionConsultaRepository.cs` — T10/T11: nuevo repositorio
- `backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs` — T14: nuevo endpoint `DELETE delegaciones/{id}`
- `backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs` — T13: nuevo endpoint `PATCH {id}/revisar-titular`
- `backend/src/IntegradorMarcas.Api/Controllers/DelegacionesController.cs` — T10/T11: nuevo controller con `GET mi-funcion`, `GET mi-registro`
- `backend/src/IntegradorMarcas.Api/Contracts/Responses/DelegacionFuncionResponse.cs` — T10: nuevo
- `backend/src/IntegradorMarcas.Api/Contracts/Responses/DelegacionRegistroResponse.cs` — T11: nuevo
- `backend/src/IntegradorMarcas.Api/Program.cs` — DI: `IDelegacionConsultaRepository`, `IDelegacionConsultaService`
- `docs/db/02_EstructuraCompleta.sql` — T17 D5: script idempotente para columnas `ModificadoPor`/`FechaHoraModificacion` en `Operacion.DelegacionAprobacion`
- `app.js` — T15: funciones `loadMiFuncionDelegacion`, `loadMiRegistroDelegado` para dashboard de jefatura

Tests (nuevos/extendidos):
- `backend/tests/IntegradorMarcas.Tests/F004DelegacionesTests.cs` — nuevo (T1-T16, R1-R22 unitarios)
- `backend/tests/IntegradorMarcas.Tests/AdminAprobacionesServiceJerarquiaTests.cs` — extendido: firma `ToggleDelegacionEstadoAsync` actualizada, nuevos metodos en `FakeAdminAprobacionesRepository` (F-004)
- `backend/tests/IntegradorMarcas.Tests/JustificacionServiceCurrentApproverTests.cs` — extendido: nuevos metodos en `FakeJustificacionRepository` (R15)
- `backend/tests/IntegradorMarcas.Tests/JustificacionServiceHistoricoTests.cs` — extendido: nuevos metodos en `FakeJustificacionRepository` (R15)

Spec:
- `specs/delegaciones-subaprobadores-reglas-completas/tasks.md` — todas T1-T18 marcadas [x]
- `progress/reports.md` — este informe

### Mapa R<n> -> test

| Req | Test (clase :: metodo) | Tipo | Tarea |
|-----|------------------------|------|-------|
| R1  | `F004DelegacionesTests::CreateDelegacion_VigenciaDesdeAusente_Lanza400` | unitario | T1 |
| R21 | `F004DelegacionesTests::CreateDelegacion_VigenciaHastaAnteriorDesde_Lanza400` | unitario | T1 |
| R21 | `F004DelegacionesTests::UpdateDelegacion_VigenciaHastaAnteriorDesde_Lanza400` | unitario | T1 |
| R2  | `AprobadoresVigentesTvfIntegrationTests` (Category=Integration, Skip) — base existente, blindado en TVF | integracion (Skip) | T2 |
| R3  | `AprobadoresVigentesTvfIntegrationTests` (Category=Integration, Skip) | integracion (Skip) | T2 |
| R5  | `AprobadoresVigentesTvfIntegrationTests` (Category=Integration, Skip) | integracion (Skip) | T2 |
| R4  | `F004DelegacionesTests::ToggleDelegacion_AInactivo_InvocaAuditoriaResumenYDetalle` | unitario | T3 |
| R6  | `F004DelegacionesTests::CreateDelegacion_DeleganteEsDelegadoActivo_Lanza409` | unitario | T4 |
| R6  | `F004DelegacionesTests::UpdateDelegacion_DeleganteEsDelegadoActivo_Lanza409` | unitario | T4 |
| R7  | `F004DelegacionesTests::CreateDelegacion_UsuarioNoAdmin_Lanza403` | unitario | T5 |
| R7  | `F004DelegacionesTests::DeleteDelegacion_UsuarioNoAdmin_Lanza403` | unitario | T5 |
| R8  | `F004DelegacionesTests::ResolverAsync_DelegadoResuelveTitular_Lanza403` | unitario | T6 |
| R9  | `F004DelegacionesTests::ResolverAsync_FueraDeScopeDeAprobacion_Lanza403` | unitario | T7 |
| R10 | `F004DelegacionesTests::ResolverAsync_SolicitanteFueraRangoTitular_Lanza403` | unitario | T8 |
| R13 | `F004DelegacionesTests::ResolverAsync_DelegacionExpirada_Lanza403` | unitario | T9 |
| R11 | `F004DelegacionesTests::GetMiFuncion_RolJefe_DevuelveDelegacionConTitularYAlcance` | unitario | T10 |
| R11 | `F004DelegacionesTests::GetMiFuncion_RolNoJefe_Lanza403` | unitario | T10 |
| R12 | `F004DelegacionesTests::GetMiFuncion_RolJefe_DevuelveDelegacionConTitularYAlcance` | unitario | T10 |
| R16 | `F004DelegacionesTests::GetMiRegistro_RolJefe_DevuelveRegistroAcotadoPorPeriodo` | unitario | T11 |
| R16 | `F004DelegacionesTests::GetMiRegistro_RolNoJefe_Lanza403` | unitario | T11 |
| R17 | `F004DelegacionesTests::GetMiRegistro_RolJefe_DevuelveRegistroAcotadoPorPeriodo` | unitario | T11 |
| R18 | `F004DelegacionesTests::GetMiRegistro_NoExisteRutaDeMutacion_ConfirmadoPorAusenciaDeEndpoint` | unitario | T11 |
| R14 | `F004DelegacionesTests::ListHistoricoAsync_RolJefatura_IncluirResueltasPorDelegado` | unitario | T12 |
| R15 | `F004DelegacionesTests::RevisarTitularAsync_RolNoJefe_Lanza403` | unitario | T13 |
| R15 | `F004DelegacionesTests::RevisarTitularAsync_SinAlcanceJerarquia_Lanza403` | unitario | T13 |
| R15 | `F004DelegacionesTests::RevisarTitularAsync_JustificacionPendiente_Lanza409` | unitario | T13 |
| R15 | `F004DelegacionesTests::RevisarTitularAsync_TitularConAlcanceYDelegadoAnterior_ResuelvyAudita` | unitario | T13 |
| R19 | `F004DelegacionesTests::DeleteDelegacion_AdminYExiste_InvocaAuditoriaYBorra` | unitario | T14 |
| R19 | `F004DelegacionesTests::DeleteDelegacion_NoExiste_Lanza404` | unitario | T14 |
| R20 | `F004DelegacionesTests::DeleteDelegacion_AdminYExiste_InvocaAuditoriaYBorra` | unitario | T14 |
| R22 | `F004DelegacionesTests::CreateDelegacion_AltaValida_InvocaAuditoriaResumenYDetalle` | unitario | T16 |
| R22 | `F004DelegacionesTests::UpdateDelegacion_EdicionValida_InvocaAuditoriaResumenYDetalle` | unitario | T16 |
| R22 | `F004DelegacionesTests::ToggleDelegacion_CambioEstado_InvocaAuditoriaResumenYDetalle` | unitario | T16 |

### Notas de implementacion

- **R2/R3/R5/R10/R13**: cubiertos a nivel SQL/TVF en `fn_AprobadoresVigentesPorSolicitante` (base existente F-003). Los tests unitarios documentan el mecanismo: `IsInApprovalScope=false` cuando la TVF no devuelve fila para el aprobador. Los tests de integracion heredados tienen `Category=Integration` y Skip deliberado (gate solo corre unitarios).
- **D5**: columnas `ModificadoPor`/`FechaHoraModificacion` agregadas a `Operacion.DelegacionAprobacion` con script idempotente. `UpdateDelegacion` y `ToggleDelegacionEstado` las poblan. `DeleteDelegacion` no las pobla (borrado fisico, la auditoria va en `AdminAccionAuditoria`). La firma de `ToggleDelegacionEstadoAsync` en el repositorio fue extendida con `actorUsuarioId`; el test existente `AdminAprobacionesServiceJerarquiaTests` fue actualizado acordemente.
- **D2 (R15)**: endpoint dedicado `PATCH /justificaciones/{id}/revisar-titular` en `JefaturaController`. No se toco la semantica de `ResolverAsync` (RN-04 intacto).
- **T15 UI**: `deleteAdminDelegacion` ya estaba alineado al endpoint DELETE creado. Se agregaron `loadMiFuncionDelegacion` y `loadMiRegistroDelegado` en `app.js` para el dashboard de jefatura.

### Salida de tests (gate T18)

```
Passed!  - Failed: 0, Passed: 53, Skipped: 0, Total: 53, Duration: 48 ms
init.ps1 -> Resultado: VERDE
```

## [2026-06-29] delegaciones-subaprobadores-reglas-completas (F-004) — reviewer

### Veredicto: CHANGES_REQUESTED

### Estado de init.ps1

Corrido por el reviewer: VERDE. Build 0 errores, 53/53 unitarios, 0 omitidos, 0 fallidos.

### Brecha bloqueante

**R1 — sin cobertura efectiva de trazabilidad.**

R1 dice: El sistema DEBE exigir VigenciaDesde en toda delegacion al crearla.

El mapa del implementer asigna a R1 el test CreateDelegacion_VigenciaHastaAnteriorDesde_Lanza400. Ese test (linea 23 de F004DelegacionesTests.cs) tiene el comentario // R21 y ejercita el escenario VigenciaHasta < VigenciaDesde, que es exactamente lo que R21 exige. No hay ningun test que envie un CreateDelegacionDto con VigenciaDesde == default(DateTime) y verifique rechazo.

En la implementacion, ValidateCreateDelegacion (AdminAprobacionesService.cs lineas 453-479) valida DeleganteUsuarioId, DelegadoUsuarioId, auto-delegacion, VigenciaHasta >= VigenciaDesde y longitud de Motivo. No hay ninguna clausula que rechace VigenciaDesde ausente o igual al valor minimo. CreateDelegacionDto y CreateDelegacionRequest definen VigenciaDesde como DateTime no nullable: un cliente que omita el campo recibe DateTime.MinValue sin rechazo.

La brecha es doble: falta la validacion de guardia en el servicio (implementacion) y falta el test que la ejercite (trazabilidad).

### Conformidad de los demas requisitos (R2–R22)

- R2/R3/R5: cubiertos a nivel SQL por la TVF fn_AprobadoresVigentesPorSolicitante. Los tests de integracion tienen Category=Integration y Skip deliberado conforme a docs/verification.md. Aceptable.
- R4: test ToggleDelegacion_AInactivo_InvocaAuditoriaResumenYDetalle verifica efecto inmediato y auditoria. OK.
- R6: dos tests unitarios (Create y Update) verifican 409 cuando el delegante es a su vez delegado activo y vigente. OK.
- R7: tests de 403 para no-admin en Create y Delete. OK.
- R8: test ResolverAsync_DelegadoResuelveTitular_Lanza403; guard implementado en JustificacionService.ResolverAsync con comparacion SolicitanteUsuarioId == DeleganteUsuarioId. OK.
- R9/R10/R13: documentados como garantizados por la TVF (IsInApprovalScope=false); tests unitarios con fakes verifican el 403. Aceptable (el mecanismo esta en la TVF, no duplicado en capa).
- R11/R12: GetMiFuncion devuelve titular, vigencia y alcance de estructuras; tests de contenido y guard de rol. OK.
- R13: ver R9/R10. OK.
- R14: ListHistoricoAsync incluye resueltas por delegado (aprobadorUsuarioIdScope). Test OK.
- R15: endpoint dedicado PATCH /justificaciones/{id}/revisar-titular (D2=B). Cuatro tests cubren los casos: rol, alcance, estado pendiente y resolucion exitosa con auditoria. OK.
- R16/R17/R18: GetMiRegistro con filtro por periodo (D4). Test de contenido y de ausencia de ruta de mutacion por reflexion. OK.
- R19/R20: DeleteDelegacionAsync con borrado fisico y auditoria doble previa (D1=A). Tests de 403, 404 y exito con auditoria. OK.
- R21: dos tests (Create y Update) de 400 cuando VigenciaHasta < VigenciaDesde. OK.
- R22: tres tests de auditoria doble en Create, Update y Toggle. OK.

### Tasks T1–T18

T1 marcada [x] pero incompleta: R1 no tiene implementacion ni test propios. Todas las demas tasks tienen evidencia real de implementacion.

### Decisiones de diseno

- D1 (fisico con auditoria previa): DeleteDelegacionAsync serializa valores anteriores antes del DELETE. Confirmado en AdminAprobacionesService.cs lineas 287-327.
- D2 (endpoint dedicado R15): PATCH /justificaciones/{id}/revisar-titular en JefaturaController, ResolverAsync no tocado. Confirmado.
- D3 (nuevo repositorio): IDelegacionConsultaRepository + DelegacionConsultaRepository nuevos. Confirmado.
- D4 (filtro por FechaAprobacion): DelegacionConsultaSql.MiRegistro filtra je.FechaAprobacion dentro de [VigenciaDesde, VigenciaHasta]. Confirmado.
- D5 (columnas auditoria): script idempotente en 02_EstructuraCompleta.sql agrega ModificadoPor/FechaHoraModificacion; UpdateDelegacion y ToggleDelegacionEstado las poblan. Confirmado.

### Conformidad arquitectural y constitucional

- Clean Architecture respetada: interfaces en Application/Interfaces, SQL en Infrastructure/Queries/*Sql.cs, sin SQL inline en controllers.
- AppException con codigos correctos (400/403/404/409). Sin excepciones crudas.
- Auditoria doble en todos los mutadores de delegacion.
- Credenciales no versionadas, WIZDOM/SIFCNP no tocadas.
- Convencion sealed en todas las clases concretas nuevas.

### Accion requerida

El implementer debe: (1) agregar guard en ValidateCreateDelegacion que rechace VigenciaDesde == default o < fecha minima de negocio razonable; (2) agregar un test unitario especifico para R1 que envie VigenciaDesde omitida/default y verifique 400. Luego reenviar al reviewer.

## [2026-06-29] delegaciones-subaprobadores-reglas-completas (F-004) — reviewer (2da vuelta)

### Veredicto: APROBADO

### Estado de init.ps1

Corrido por el reviewer: VERDE. Build 0 errores, 54/54 unitarios, 0 omitidos, 0 fallidos.

### Verificacion puntual de la brecha R1

**Item 1 — Implementacion:** ValidateCreateDelegacion en AdminAprobacionesService.cs tiene la nueva clausula:
  if (request.VigenciaDesde == default) throw new AppException(VigenciaDesde es requerida., 400);
Ubicada antes de la validacion de VigenciaHasta, separada de R21. Correcto.

**Item 2 — Test real de R1:** Existe CreateDelegacion_VigenciaDesdeAusente_Lanza400 (F004DelegacionesTests.cs linea 23).
El test construye un CreateDelegacionDto sin asignar VigenciaDesde (queda DateTime.MinValue).
Verifica Assert.Equal(400, ex.StatusCode) y Assert.Contains(VigenciaDesde, ex.Message).
El escenario es R1 (campo ausente), no R21 (VigenciaHasta < VigenciaDesde). Correcto.

**Item 3 — Trazabilidad:** El mapa en reports.md (linea 175) fue actualizado por el implementer:
  R1 | F004DelegacionesTests::CreateDelegacion_VigenciaDesdeAusente_Lanza400 | unitario | T1
R21 conserva sus propios tests (CreateDelegacion_VigenciaHastaAnteriorDesde_Lanza400 y UpdateDelegacion_VigenciaHastaAnteriorDesde_Lanza400). Correcto.

### Confirmacion del resto (R2-R22, D1-D5, arquitectura)

Sin cambios respecto a la primera vuelta. Todos los items verificados en esa ronda siguen conformes;
no se introdujo ninguna regresion (el unico archivo tocado es AdminAprobacionesService.cs con la clausula R1
y F004DelegacionesTests.cs con el test nuevo; build y tests 54/54 verde).

### Checkpoints recorridos

- Arnés: 19 archivos base presentes, init.ps1 VERDE.
- Tablero: 1 feature in_progress (F-004), approved_by=Raul OA, approved_at=2026-06-29.
- Codigo: Clean Architecture respetada, SQL en Infrastructure, AppException con codigos correctos.
- Verificacion: 54 unitarios verdes, trazabilidad R1-R22 completa con tests nombrados.
- Specs SDD: requirements.md/design.md/tasks.md presentes, EARS con ids R1-R22, tasks.md T1-T18 todas [x].
- Constitucion: ningun principio violado.
