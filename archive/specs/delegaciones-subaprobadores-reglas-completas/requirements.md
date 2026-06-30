# Requirements - delegaciones-subaprobadores-reglas-completas (F-004)

Origen: PRP RF-09 (Gestión de Delegados y Subaprobadores), RF-03 (Aprobación por jerarquía/delegación
vigente), RN-03, RN-06, RN-10, RNF-01 (seguridad/segregación de roles), RNF-05 (trazabilidad),
RNF-06 (continuidad operativa por delegaciones), RNF-07 (configurabilidad).
Insumo de negocio: `progress/backlog.md` → "Ideas propias" → `[2026-06-28]` (9 reglas de delegación).

> F-004 **extiende** la base ya existente: tabla `Operacion.DelegacionAprobacion`, función
> `dbo.fn_AprobadoresVigentesPorSolicitante` y la prioridad `Origen='Delegacion'` que cerró F-003.
> Estos requisitos NO duplican lo existente; agregan las reglas de negocio que faltan y alinean el
> borrado y la UI.

## Mapa regla de negocio (backlog) → requisitos

| Regla backlog | Resumen | Requisitos |
|---|---|---|
| 1 | Delegado temporal con fecha inicio/fin; la app habilita/deshabilita por esas fechas | R1, R2, R3 |
| 2 | El jefe (o admin) puede anular/detener el acceso en cualquier momento | R4, R5 |
| 3 | Los delegados NO pueden sub-delegar | R6 |
| 4 | Cualquier admin puede asistir a los titulares en el proceso | R7 |
| 5 | El delegado NO aprueba: al titular, a sí mismo, ni fuera del rango jerárquico del titular | R8, R9, R10 |
| 6 | El delegado ve su función, quién se la asignó y el alcance (dependencias) | R11, R12 |
| 7 | Aprobaciones dirigidas a un delegado no tramitadas a tiempo se bloquean al fin del permiso | R13 |
| 8 | El titular siempre ve y modifica las justificaciones creadas en su ausencia por niveles inferiores | R14, R15 |
| 9 | Los delegados tienen registro de **solo lectura** de lo delegado dentro del período | R16, R17, R18 |

## Requisitos (EARS)

### Vigencia temporal (regla 1)

- R1: El sistema DEBE exigir `VigenciaDesde` en toda delegación al crearla.
- R2: CUANDO se resuelve el alcance de aprobadores de un solicitante en una fecha de referencia, el
  sistema DEBE considerar una delegación solo si esa fecha de referencia es mayor o igual a su
  `VigenciaDesde`.
- R3: CUANDO se resuelve el alcance de aprobadores de un solicitante en una fecha de referencia, el
  sistema DEBE excluir toda delegación cuya `VigenciaHasta` no sea nula y sea anterior a esa fecha de
  referencia.

### Anulación/detención por el titular o admin (regla 2)

- R4: CUANDO un `ROL_ADMIN` cambia el estado de una delegación a Inactivo (2), el sistema DEBE
  registrar la delegación como no vigente con efecto inmediato.
- R5: CUANDO se resuelve el alcance de aprobadores de un solicitante, el sistema DEBE excluir toda
  delegación cuyo `EstadoRegistroId` sea distinto de Activo (1).

### Prohibición de sub-delegación (regla 3)

- R6: SI se intenta crear o actualizar una delegación cuyo `DeleganteUsuarioId` es delegado de una
  delegación activa y vigente de otra persona ENTONCES el sistema DEBE rechazar la operación con
  código 409.

### Asistencia de cualquier admin (regla 4)

- R7: El sistema DEBE permitir que cualquier usuario con rol `ROL_ADMIN` ejecute el alta, edición,
  cambio de estado y borrado de cualquier delegación, sin restringirlo a un admin en particular.

### Restricciones de aprobación del delegado (regla 5)

- R8: SI un delegado intenta resolver una justificación cuyo solicitante es su propio titular
  (delegante) ENTONCES el sistema DEBE rechazar la operación con código 403.
- R9: SI un usuario intenta resolver una justificación cuyo solicitante es él mismo ENTONCES el
  sistema DEBE rechazar la operación con código 403.
- R10: SI un delegado intenta resolver una justificación cuyo solicitante pertenece a una estructura
  organizacional fuera del rango jerárquico aprobable por su titular ENTONCES el sistema DEBE
  rechazar la operación con código 403.

### Visibilidad de la función del delegado (regla 6)

- R11: CUANDO un delegado consulta su función de delegación, el sistema DEBE devolver, por cada
  delegación activa y vigente recibida, quién se la asignó (titular) y la vigencia del permiso.
- R12: CUANDO un delegado consulta su función de delegación, el sistema DEBE devolver el alcance de
  dependencias (estructuras organizacionales) que puede aprobar bajo esa delegación.

### Bloqueo por expiración (regla 7)

- R13: MIENTRAS una justificación dirigida a un delegado permanezca pendiente, el sistema DEBE impedir
  que ese delegado la resuelva una vez superada la `VigenciaHasta` de la delegación que le daba el
  alcance.

### Soberanía del titular sobre lo hecho en su ausencia (regla 8)

- R14: CUANDO el titular consulta sus justificaciones de aprobación, el sistema DEBE incluir las
  justificaciones de su alcance que fueron resueltas por un delegado suyo durante la ausencia.
- R15: El sistema DEBE permitir que el titular vuelva a resolver una justificación de su alcance que
  haya sido resuelta por un delegado suyo, conservando trazabilidad de la modificación.

### Registro de solo lectura del delegado (regla 9)

- R16: CUANDO un delegado consulta su registro de justificaciones delegadas, el sistema DEBE devolver
  únicamente las justificaciones que recibió por delegación.
- R17: CUANDO un delegado consulta su registro de justificaciones delegadas, el sistema DEBE limitar
  el resultado a las justificaciones cuya fecha de resolución (o vigencia) cae dentro del período de
  la delegación correspondiente.
- R18: SI un delegado intenta modificar una justificación a través del endpoint de registro de
  delegado ENTONCES el sistema DEBE rechazar la operación (acceso de solo lectura).

### Alineación de borrado y UI (acceptance F-004)

- R19: El sistema DEBE exponer un endpoint de borrado de delegación (`DELETE`) que el frontend de
  administración invoca, ejecutado solo por `ROL_ADMIN` y dejando evidencia de auditoría.
- R20: CUANDO un `ROL_ADMIN` borra una delegación, el sistema DEBE registrar un evento de auditoría
  resumen y un registro de auditoría detallado de la acción.

### Integridad de vigencia y trazabilidad (transversal)

- R21: SI al crear o actualizar una delegación `VigenciaHasta` no es nula y es anterior a
  `VigenciaDesde` ENTONCES el sistema DEBE rechazar la operación con código 400.
- R22: CUANDO se crea, actualiza, cambia de estado o borra una delegación, el sistema DEBE registrar
  un evento de auditoría resumen y un registro de auditoría detallado con los valores anteriores y
  nuevos.

## Cobertura de aceptacion

- "La delegación prioriza sobre la jerarquía en todos los flujos (Origen='Delegacion')." -> R2, R3,
  R5, R8, R10, R13 (la prioridad ya existe; estos requisitos la mantienen y la acotan por las reglas).
- "Flujos end-to-end de delegación: crear, vigencia, expiración y borrado alineados." -> R1, R2, R3,
  R13, R19, R20, R21.
- "La UI admin de delegados es consistente con el backend." -> R7, R19, R11, R12.
- "Trazabilidad R<n>→test e init.ps1 en verde." -> todos los R1–R22 con test asociado (ver
  `tasks.md` y `progress/reports.md`).
