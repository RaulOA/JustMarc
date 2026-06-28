# docs/workflow.md — Modelo de trabajo diario

## Principio: capturar ≠ ejecutar

Una idea nueva **se captura** (a `progress/backlog.md`) sin abandonar lo que está en curso. Ejecutar es
otra cosa, y es **una a la vez**.

## El loop

```text
/retomar    -> ponerse al dia (no trabaja)
/idea       -> capturar ideas sueltas (sigue lo actual)
/nueva      -> elegir UNA cosa nueva y arrancar el flujo correcto
( SDD: spec_author -> HUMANO aprueba -> implementer -> reviewer )
/cerrar     -> ritual de cierre (verde, destila, archiva, rota)
/planificar -> ordenar el backlog contra el cronograma
/cronograma -> ritual SEMANAL: refleja estado real en el xlsx + evidencia para jefatura
```

> `docs/cronograma.xlsx` es el cronograma de jefatura y la **columna vertebral** que `/planificar` usa
> para ordenar el trabajo; `/cronograma` es el **ritual semanal** que actualiza el estado real de cada
> tarea (nunca "hecho" sin evidencia) y arma el paquete de evidencia ejecutivo en `evidencias/`.

## Propósito de cada comando `/`

- `/retomar` — re-orienta desde `current.md` + tablero + últimas de `history.md`. No trabaja.
- `/idea` — append de una línea con fecha a `backlog.md`. No cambia el foco.
- `/nueva` — aplica la compuerta de complejidad (multi-archivo/riesgoso/UI → SDD; trivial → directo),
  agrega al tablero, arranca.
- `/arreglar` — corrección: ubica reciente vs. archivada (índice en `archive/`), reproduce, halla causa,
  arregla, muestra evidencia.
- `/planificar` — procesa `backlog.md`: agrupa, promueve a `pending`, deja "algún día" o descarta. No
  ejecuta.
- `/cronograma` — ritual **semanal**: lee tablero + `progress/` + `git log`, refleja el estado real en
  `docs/cronograma.xlsx` y genera evidencia ejecutiva (limpia, sin proceso interno) en `evidencias/`. No
  cierra features ni implementa.
- `/cerrar` — ritual de cierre (abajo).
- `/estado` — vistazo de una pantalla. Sin acción.

## Retención / rotación

- `progress/reports.md`: cuando supera ~**800 líneas**, `/cerrar` lo **rota** a
  `archive/reports/reports-<fecha>.md` y deja un encabezado fresco.
- Specs de features `done` → se archivan en `archive/specs/<feature>/`.
- `init.ps1` (paso 5) avisa `[WARN]` si algún archivo de `progress/` supera el umbral.
- Lectura **dirigida**: los agentes leen la feature o las últimas entradas, no los archivos enteros.

## Ritual de cierre (`/cerrar`)

1. `init.ps1` en verde.
2. Destila lo terminado a **una línea** en `history.md` y **poda** la feature del tablero.
3. Archiva specs `done` → `archive/specs/`.
4. Rota `reports.md` → `archive/reports/` si excede el umbral.
5. Deja `current.md` en estado real. Reporta qué cerró.
