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
