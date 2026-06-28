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
- `/idea` — append de una línea con fecha a `backlog.md`, bajo **"Ideas propias"**. No cambia el foco.
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

## Hallazgos y backlog

- **Registro de hallazgos (`progress/findings.md`)**: el `reviewer` anota cada **detalle menor** que ve
  durante la revisión en una línea, con su **disposición** (`corregido-ahora` / `→ idea backlog` /
  `solo-anotado`). Es el registro escaneable de "qué se vio y qué se hizo con eso". El reviewer **captura
  y enruta**, no diseña la solución: una brecha de correctitud/requisito **bloquea**
  (`CHANGES_REQUESTED`); una mejora se anota como **idea derivada** en el backlog; lo demás queda solo
  anotado.
- **Backlog en dos secciones (`progress/backlog.md`)**: **"Ideas propias"** (las que capturás con
  `/idea`) e **"Ideas derivadas de hallazgos"** (las que enruta el `reviewer`, cada una con la feature
  relacionada — `id` existente o `nueva`).
- **Enfoques múltiples**: si un problema admite varios enfoques viables, **no se deciden en la
  revisión**. El `spec_author` los presenta en `design.md` y vos elegís en la **compuerta del spec**
  (puerta humana), antes de pasar a `in_progress`.
- **Cambios al mecanismo del arnés** (agentes, comandos, rituales): se registran en `CHANGELOG.md` (raíz),
  separado del historial del proyecto (`progress/history.md`).

## Retención / rotación

- `progress/reports.md`: cuando supera ~**800 líneas**, `/cerrar` lo **rota** a
  `archive/reports/reports-<fecha>.md` y deja un encabezado fresco.
- `progress/findings.md`: `/cerrar` **poda los hallazgos ya resueltos** (`corregido-ahora` /
  `→ idea backlog`) al pasar el umbral, conservando el encabezado.
- Specs de features `done` → se archivan en `archive/specs/<feature>/`.
- `init.ps1` (paso 5) avisa `[WARN]` si algún archivo de `progress/` supera el umbral.
- Lectura **dirigida**: los agentes leen la feature o las últimas entradas, no los archivos enteros.

## Ritual de cierre (`/cerrar`)

1. `init.ps1` en verde.
2. Destila lo terminado a **una línea** en `history.md` y **poda** la feature del tablero.
3. Archiva specs `done` → `archive/specs/`.
4. Rota `reports.md` → `archive/reports/` si excede el umbral.
5. Deja `current.md` en estado real. Reporta qué cerró.
