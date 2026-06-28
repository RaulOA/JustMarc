# CHANGELOG — Arnés "Harness Engineering"

> Append-only. Registra cambios al **mecanismo del arnés** (agentes, comandos, rituales, documentos de
> `docs/`, archivos de `progress/`), **separado** del trabajo del proyecto. El historial de features
> terminadas vive en `progress/history.md`; **nada de features de la app acá**.
>
> Formato basado en [Keep a Changelog](https://keepachangelog.com/es-ES/1.1.0/).

## [No publicado]

## [2026-06-28] — Retrofit: control de hallazgos y backlog separado

> El arnés se instaló antes de existir este registro; esa historia previa queda en **git** (ver
> `f7e7dc3`, `792319f`, `42eaf1f`, …). Esta es la primera entrada del CHANGELOG.

### Agregado

- `progress/findings.md` — registro de hallazgos append-only: el `reviewer` anota cada detalle menor en
  una línea con su disposición (`corregido-ahora` / `→ idea backlog` / `solo-anotado`).
- `CHANGELOG.md` — este control de cambios del mecanismo del arnés.
- `progress/backlog.md` — nueva sección **"Ideas derivadas de hallazgos"** (con la feature relacionada),
  separada de las **"Ideas propias"** preexistentes.

### Cambiado

- `.claude/agents/reviewer.md` — nueva sección **Control de hallazgos**: registra y **enruta sin perder
  ninguno** cada detalle (brecha → bloquea `CHANGES_REQUESTED`; mejora → idea derivada en `backlog.md`;
  observación → `findings.md`). No diseña ni ejecuta la solución (eso es la compuerta del spec).
- `.claude/agents/spec_author.md` — en el diseño, si un problema admite **varios enfoques viables**, los
  presenta de forma comprensible para que el humano elija en la compuerta (además de la alternativa
  descartada que ya pedía).
- `.claude/commands/idea.md` — escribe la idea bajo la sección **"Ideas propias"** del backlog.
- `.claude/commands/cerrar.md` — incluye `findings.md` en la retención/rotación (poda lo resuelto al
  pasar el umbral, igual que `reports.md`).
- `docs/workflow.md` — documenta el registro de hallazgos, la separación del backlog y que los enfoques
  múltiples se aprueban en la compuerta del spec.
- `CLAUDE.md` — nueva sección **"Estilo de comunicación"**: las salidas de cara al usuario (respuestas,
  resúmenes, informes) usan tono pedagógico, corto y sin tecnicismos; no aplica a artefactos técnicos
  internos ni a la evidencia de jefatura.

### Verificación

- `init.ps1`: **VERDE** (2026-06-28). Archivos base 19/19, tablero coherente, Build 0 errores,
  27/27 tests unitarios, 0 omitidos. El paso [5/6] ya monitorea `findings.md` automáticamente. Este
  retrofit no toca build ni tests.
