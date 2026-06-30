# Control de hallazgos

> Append-only. Registro escaneable de **qué se vio y qué se hizo con eso**. Lo llena el `reviewer`:
> cada detalle menor que detecte durante la revisión va aquí en **una línea** con su **disposición**.
>
> Disposiciones posibles:
> - `corregido-ahora` — defecto menor arreglado en la misma revisión (la brecha de correctitud/requisito
>   bloquea con `CHANGES_REQUESTED`; no se "corrige al paso").
> - `→ idea backlog` — mejora capturada en `progress/backlog.md` como **idea derivada** (anotá la feature).
> - `solo-anotado` — observación que no amerita acción por ahora; queda registrada acá.
>
> El reviewer **no diseña ni ejecuta** la solución acá: esto es captura. Los enfoques se ven después, en
> la compuerta del spec. Se poda lo resuelto en `/cerrar` al pasar el umbral (igual que `reports.md`).

<!-- - [YYYY-MM-DD] <dónde> — <qué> · disposición: <corregido-ahora | → idea backlog (F-xxx/nueva) | solo-anotado> -->
- [2026-06-28] Edición de jerarquía (`AdminAprobacionesSql.UpdateJerarquia` → tabla `Operacion.JerarquiaAprobacion`) — se envía el parámetro `ModificadoPor` pero la tabla no tiene esa columna (solo `CreadoPor`/`FechaHoraCreacion`); Dapper lo ignora sin error. Herencia previa, no introducido por F-003. · disposición: → idea backlog (nueva)

- [2026-06-29] F-004 R1 — ValidateCreateDelegacion + CreateDelegacionRequest: R1 exige que VigenciaDesde sea provista al crear; la implementacion no valida VigenciaDesde != default(DateTime). El test del mapa para R1 (CreateDelegacion_VigenciaHastaAnteriorDesde_Lanza400) prueba R21, no R1. R1 carece de test propio que ejercite la condicion ausente. · disposicion: CHANGES_REQUESTED (brecha de requisito y trazabilidad)
