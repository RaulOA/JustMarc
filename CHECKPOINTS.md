# CHECKPOINTS.md — Estado final correcto

> Criterios objetivos para considerar el trabajo "bien cerrado". Sin nada atado a un lenguaje. El
> `reviewer` recorre esta lista; `init.ps1` automatiza varios.

## Arnés
- [ ] Todos los archivos base del arnés existen (ver `init.ps1`, paso 2).
- [ ] `init.ps1` corre en **VERDE** (toolchain, tablero, build, test).

## Estado / tablero
- [ ] A lo sumo **una** feature `in_progress`.
- [ ] Toda feature `in_progress` tiene `approved_by` y `approved_at` (puerta humana).
- [ ] Todos los estados son válidos (`pending|spec_ready|in_progress|done|blocked`).
- [ ] Las features `done` se movieron a `progress/history.md` y se podaron del JSON.

## Código
- [ ] Respeta la arquitectura (`docs/architecture.md`): regla de dependencia, SQL fuera de controllers,
  capas correctas.
- [ ] Respeta `docs/conventions.md` (nombres, estilo, español/UTF-8).

## Verificación real
- [ ] Pruebas por módulo verdes (ver `docs/verification.md`).
- [ ] (features `sdd`) Trazabilidad: **cada `R<n>` tiene al menos un test**.

## Sesión
- [ ] `progress/current.md` refleja el estado real.
- [ ] Informes en `progress/reports.md`; lo terminado destilado a `history.md`.

## Bloque SDD (solo features con `"sdd": true` y estado ≠ `pending`)
- [ ] Existen los 3 specs: `specs/<feature>/{requirements,design,tasks}.md`.
- [ ] `requirements.md` en **EARS estricto** (ids `R1..Rn`, un solo `DEBE` por requisito).
- [ ] `tasks.md` con todas las tasks en `[x]` y cada una citando sus `R<n>`.
- [ ] Cada `R<n>` mapeado a un test concreto.

## Constitución
- [ ] `docs/constitution.md` existe y el cambio **no viola** ningún principio inmutable.
