# Requirements - cierre-jerarquia-aprobacion-avanzada

Feature: F-003 (cronograma T026)
Origen (PRP): RF-08 (jerarquia de aprobacion configurable, vertical/horizontal, niveles),
RF-03 (resolucion por jerarquia/delegacion vigente), RN-03, RNF-05 (trazabilidad),
RNF-06 (continuidad operativa), RNF-07 (configurabilidad).

> Trabajo de **cierre/extension**. La base ya existe: resolucion vigente en la TVF
> `dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioId, @FechaRef)`, gestion admin en
> `AdminAprobacionesService` (Application) + `AdminAprobacionesRepository` (Infrastructure). Esta spec
> cierra **casos edge de resolucion** y **validaciones avanzadas de alta/edicion**, sin re-arquitecturar.

## Convenciones de la spec

- `Resolver el alcance` = ejecutar `dbo.fn_AprobadoresVigentesPorSolicitante` para un solicitante en una
  fecha de referencia y obtener el conjunto de aprobadores vigentes (`AprobadorUsuarioId`, `Origen`,
  `DeleganteUsuarioId`).
- `Aprobador vigente` = fila devuelta por esa TVF con `EstadoRegistroId = 1` y vigencia que cubre la fecha.
- `Validacion avanzada` = guard clause en `AdminAprobacionesService` que rechaza un alta/edicion
  inconsistente con `AppException(400)` antes de tocar el repositorio.

## Requisitos (EARS)

### Casos edge de resolucion de jerarquia

- R1: CUANDO se resuelve el alcance de un solicitante que no tiene ninguna jerarquia ni delegacion
  vigente, el sistema DEBE devolver un resultado sin aprobador (conjunto vacio / aprobador nulo) en lugar
  de fallar.
- R2: CUANDO se resuelve el alcance de un solicitante cubierto por varias jerarquias vigentes de
  distinto `NivelAprobacion`, el sistema DEBE incluir un aprobador vigente por cada jerarquia aplicable
  sin descartar niveles.
- R3: MIENTRAS coexisten un aprobador por jerarquia y un aprobador por delegacion vigente para el mismo
  solicitante, el sistema DEBE seleccionar el de `Origen = 'Delegacion'` como aprobador efectivo en la
  resolucion del aprobador actual.
- R4: SI una jerarquia o delegacion esta fuera de vigencia (`VigenciaHasta` anterior a la fecha de
  referencia) o inactiva (`EstadoRegistroId <> 1`) ENTONCES el sistema DEBE excluir su aprobador del
  conjunto resuelto.
- R5: SI el solicitante carece de estructura organizacional vigente asociada a su `UnidadId` (dato
  incompleto) ENTONCES el sistema DEBE devolver un resultado sin aprobador por jerarquia en lugar de
  fallar.
- R6: CUANDO un usuario `ROL_JEFE` consulta su aprobador actual sin aprobador vigente resuelto, el
  sistema DEBE responder un `CurrentApproverDto` con aprobador nulo sin lanzar excepcion.

### Validaciones avanzadas de alta/edicion de jerarquia

- R7: SI un alta o edicion de jerarquia llega con `NivelAprobacion` menor o igual a cero ENTONCES el
  sistema DEBE rechazarla con `AppException(400)`.
- R8: SI un alta o edicion de jerarquia llega con `TipoRelacion` distinto de `Vertical` u `Horizontal`
  (ignorando mayusculas/minusculas y espacios) ENTONCES el sistema DEBE rechazarla con `AppException(400)`.
- R9: SI un alta o edicion de jerarquia llega con `VigenciaHasta` anterior a `VigenciaDesde` ENTONCES el
  sistema DEBE rechazarla con `AppException(400)`.
- R10: SI un alta o edicion de jerarquia referencia un `AprobadorUsuarioId` inexistente ENTONCES el
  sistema DEBE rechazarla con `AppException(400)`.
- R11: SI un alta o edicion de jerarquia referencia una `EstructuraOrganizacionalId` inexistente
  ENTONCES el sistema DEBE rechazarla con `AppException(400)`.
- R12: SI un alta o edicion de jerarquia crea un duplicado vigente para la misma combinacion
  (`AprobadorUsuarioId`, `EstructuraOrganizacionalId`, `NivelAprobacion`) ya activa ENTONCES el sistema
  DEBE rechazarla con `AppException(409)`.
- R13: SI un alta o edicion de jerarquia es ejecutada por un usuario cuyo rol no es `ROL_ADMIN` ENTONCES
  el sistema DEBE rechazarla con `AppException(403)`.
- R14: CUANDO un alta o edicion de jerarquia valida supera las validaciones avanzadas, el sistema DEBE
  registrar el evento de auditoria correspondiente (resumen y detalle) antes de retornar.

### Trazabilidad y gate

- R15: El sistema DEBE mantener una trazabilidad `R<n> -> test` en la que cada requisito R1..R14 quede
  cubierto por al menos un test nombrado.
- R16: El sistema DEBE conservar `init.ps1` en verde (Build + Test unitarios con
  `--filter "Category!=Integration"`) tras la implementacion.

## Cobertura de aceptacion

- "La resolucion de jerarquia cubre casos edge: sin aprobador vigente, multiples niveles, datos
  incompletos." -> R1, R2, R3, R4, R5, R6
- "Validaciones avanzadas al crear/editar jerarquias de aprobacion." -> R7, R8, R9, R10, R11, R12, R13, R14
- "Trazabilidad R<n>->test con cobertura de los casos edge." -> R15
- "init.ps1 en verde." -> R16
