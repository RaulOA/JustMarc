# Tasks - cierre-jerarquia-aprobacion-avanzada

> Cada T cita sus R. Orden sugerido: contrato -> SQL -> Infraestructura -> Application -> tests -> gate.

- [x] T1 (R12): Agregar `ExistsJerarquiaActivaDuplicadaAsync(aprobadorUsuarioId, estructuraOrganizacionalId, nivelAprobacion, jerarquiaAprobacionIdExcluida, ct)` a `IAdminAprobacionesRepository`.
- [x] T2 (R12): Agregar la `const string ExistsJerarquiaActivaDuplicada` en `AdminAprobacionesSql` (EXISTS sobre `Operacion.JerarquiaAprobacion` con `EstadoRegistroId = 1`, la combinacion aprobador/estructura/nivel y exclusion del id propio).
- [x] T3 (R12): Implementar `ExistsJerarquiaActivaDuplicadaAsync` en `AdminAprobacionesRepository` con `ExecuteScalarAsync<bool>` (patron de `ExistsJerarquiaAsync`).
- [x] T4 (R12): Agregar `EnsureJerarquiaNoDuplicadaAsync` en `AdminAprobacionesService` e invocarla en `CreateJerarquiaAsync` (sin exclusion) y `UpdateJerarquiaAsync` (excluyendo el id editado); lanzar `AppException(..., 409)` ante duplicado.
- [x] T5 (R7, R8, R9, R10, R11, R13): Verificar/consolidar las guard clauses existentes (`ValidateCreateJerarquia`, `ValidateUpdateJerarquia`, `EnsureReferencesForJerarquiaAsync`, `EnsureAdmin`) para que cubran nivel<=0, TipoRelacion invalido, vigencia invertida, aprobador/estructura inexistentes y rol no admin.
- [x] T6 (R7, R8, R9, R10, R11, R12, R13, R14): Crear `AdminAprobacionesServiceJerarquiaTests` (unitarios, repositorio fake): un test por requisito de validacion + un test que asersa que el alta valida invoca el log de auditoria resumen y detallado.
- [x] T7 (R1, R3, R6): Extender `JustificacionServiceCurrentApproverTests` con casos edge expresables por contrato: aprobador nulo (sin scope), precedencia de delegacion sobre jerarquia, y `ROL_JEFE` sin aprobador vigente sin excepcion.
- [x] T8 (R2, R4, R5): Crear `AprobadoresVigentesTvfIntegrationTests` con `[Trait("Category","Integration")]` contra BD real: multiples niveles incluidos, jerarquia/delegacion fuera de vigencia o inactiva excluida, y solicitante sin estructura vigente -> sin aprobador por jerarquia.
- [x] T9 (R1, R2, R4, R5): Si algun test edge de T7/T8 demuestra defecto, ajustar la TVF en `docs/db/02_EstructuraCompleta.sql` (`CREATE OR ALTER`, idempotente) y/o `JustificacionesSql.GetCurrentApproverBySolicitante`; en caso contrario, no tocar produccion SQL.
- [x] T10 (R15): Documentar en `progress/reports.md` el mapa `R1..R16 -> test` con el nombre exacto de cada test (unitario o integracion).
- [x] T11 (R16): Correr `pwsh ./init.ps1` y confirmar Build + Test unitarios (`--filter "Category!=Integration"`) en verde.
