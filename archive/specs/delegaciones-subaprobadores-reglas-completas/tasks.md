# Tasks - delegaciones-subaprobadores-reglas-completas (F-004)

> Cada task cita sus `R<n>`. El `implementer` deja el mapa `R<n> → test` en `progress/reports.md`.
> Varias reglas ya están cubiertas por la base (TVF / SQL de F-003): para ésas la task es
> **blindar con test** y documentar, no reimplementar.

## Vigencia y estado (reglas 1 y 2 — base existente, blindar)

- [x] T1 (R1, R21): Verificar/asegurar validación de `VigenciaDesde` requerida y
  `VigenciaHasta >= VigenciaDesde` en `ValidateCreateDelegacion` / `ValidateUpdateDelegacion`; tests
  unitarios de los 400.
- [x] T2 (R2, R3, R5): Tests sobre `dbo.fn_AprobadoresVigentesPorSolicitante` (Integration) que
  prueben inclusión por fecha vigente, exclusión por `VigenciaHasta` pasada y exclusión por estado
  Inactivo.
- [x] T3 (R4): Test de `ToggleDelegacionEstadoAsync` a Inactivo (efecto inmediato) + auditoría.

## Anti-sub-delegación (regla 3)

- [x] T4 (R6): Agregar `ExistsDelegacionActivaComoDelegadoAsync` (repo + SQL) y guard en
  `EnsureReferencesForDelegacionAsync`; tests unitarios del 409 al crear y al actualizar.

## Asistencia de cualquier admin (regla 4)

- [x] T5 (R7): Tests que confirman que cualquier `ROL_ADMIN` puede crear/editar/togglear/borrar
  delegaciones y que roles no-admin reciben 403 (incluye el nuevo `DeleteDelegacionAsync`).

## Restricciones de aprobación del delegado (regla 5)

- [x] T6 (R8): Guard en `JustificacionService.ResolverAsync`: delegado no puede resolver justificación
  de su titular; test unitario 403 con `ScopeSource='Delegacion'` y solicitante = delegante.
- [x] T7 (R9): Test que confirma 403 al intentar auto-resolver (cubierto por
  `je.UsuarioID <> @AprobadorUsuarioID`).
- [x] T8 (R10): Test que confirma 403 cuando el solicitante está fuera del rango del titular (la fila
  `Delegacion` no aparece en la TVF → `IsInApprovalScope=false`).

## Bloqueo por expiración (regla 7)

- [x] T9 (R13): Test que confirma que, vencida `VigenciaHasta`, una justificación pendiente dirigida al
  delegado ya no puede ser resuelta por él (403).

## Visibilidad de la función del delegado (regla 6)

- [x] T10 (R11, R12): Crear `IDelegacionConsultaService` + `DelegacionConsultaService` +
  `DelegacionFuncionDto` + repo/SQL `MiFuncion`; endpoint `GET /api/delegaciones/mi-funcion`; tests de
  contenido (titular, vigencia, alcance de estructuras) y guard de rol.

## Registro de solo lectura del delegado (regla 9)

- [x] T11 (R16, R17, R18): `GetMiRegistroAsync` + `DelegacionRegistroDto` + repo/SQL `MiRegistro`;
  endpoint `GET /api/delegaciones/mi-registro` (solo lectura); tests de acotación por delegación y por
  período (según Decisión D4) y de ausencia de ruta de mutación.

## Soberanía del titular (regla 8)

- [x] T12 (R14): Test que confirma que el titular ve, en su histórico de aprobación, las
  justificaciones resueltas por su delegado durante la ausencia.
- [x] T13 (R15): Implementar la re-resolución del titular según Decisión D2 (endpoint dedicado
  recomendado); tests de éxito del titular y de rechazo a terceros, con auditoría.

## Alineación de borrado y UI (acceptance F-004)

- [x] T14 (R19, R20): `DeleteDelegacionAsync` (service + repo + SQL `DeleteDelegacion`) y endpoint
  `DELETE /api/admin/aprobaciones/delegaciones/{id}`; auditoría doble; tests unitarios (403 no-admin,
  404 inexistente, 200/204 ok con auditoría). Resolver según Decisión D1 (físico vs. lógico).
- [x] T15 (R7, UI): Alinear `app.js` (formulario admin con Motivo/Jerarquía; el `DELETE` ya esperado)
  y agregar las vistas del delegado (mi-funcion / mi-registro) en el dashboard de jefatura.

## Trazabilidad y auditoría transversal

- [x] T16 (R22): Verificar/asegurar auditoría resumen + detallada en create/update/toggle/delete de
  delegación; tests de invocación de auditoría (patrón `Fake*Repository` existente).
- [x] T17 (D5): Según decisión del humano, agregar `ModificadoPor`/`FechaHoraModificacion` a
  `Operacion.DelegacionAprobacion` (script idempotente) y poblarlas; o registrar la nota en
  `progress/backlog.md` si se descarta.

## Cierre

- [x] T18 (todos): `pwsh ./init.ps1` en verde (Build + Test `Category!=Integration`) y mapa
  `R<n> → test` completo en `progress/reports.md` para entrega al `reviewer`.
