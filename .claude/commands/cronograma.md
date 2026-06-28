---
description: Ritual semanal del cronograma de jefatura — refleja el estado real en docs/cronograma.xlsx y genera el paquete de evidencia ejecutivo en evidencias/. Nunca marca "hecho" sin evidencia.
---

Ritual **semanal** del cronograma de jefatura. NO implementa features ni cierra nada: **refleja** lo ya
hecho y **reporta** a jefatura. `docs/cronograma.xlsx` es la columna vertebral del control de avance de
jefatura, aunque no siempre describa el trabajo técnico real (esa es la vista lineal que jefatura espera).

## Dos capas separadas — NO mezclar

- **Capa interna (verificación):** el tablero, `progress/`, `git log`, `init.ps1`. Sirve para SABER qué
  está realmente hecho. **Nunca** se filtra a la evidencia de jefatura.
- **Capa externa (evidencia jefatura):** `evidencias/` + `docs/cronograma.xlsx`. Ejecutiva, de negocio,
  **limpia**. PROHIBIDO mencionar: IA, agentes, arnés, specs, features `F-xxx`, tests/`init.ps1`,
  commits, subagentes o cualquier detalle del proceso de desarrollo. Solo **contenido que demuestre que
  la tarea quedó al 100%** (o la justificación de por qué no).

## Pasos

### 1 — Reunir el estado real (solo lectura, capa interna)
- `feature_list.json` — qué features están `done`/`in_progress`/`pending` y su campo `cronograma`
  (`T026`→F-003, `T027`→F-004, `T028`→F-005; `null` = sin tarea de cronograma).
- `progress/history.md` (features destiladas como terminadas) y `progress/reports.md` (informes).
- `git log` desde la última corrida:
  `git log -1 --format=%cI -- docs/cronograma.xlsx` da la fecha del último update; luego
  `git log --since="<esa fecha>" --oneline`.
- `pwsh ./init.ps1` debe estar **verde** antes de declarar cualquier tarea como concluida.

### 2 — Determinar el estado real de cada tarea
Mapeá tarea de cronograma → realidad:
- **Hecho (100%)** SOLO si hay evidencia verificable: la funcionalidad existe y opera, y para tareas de
  desarrollo además feature `done` + informe + `init.ps1` verde. Sin eso, NUNCA "Hecho".
- **En curso** — hay avance demostrable pero no cierre; preparар justificación.
- **Pendiente** — sin avance.
- Tareas de reporte/ops (UAT, capacitación, despliegue T029–T038): su estado se completa cuando termina
  la codificación; evidencia documental (plan, acta, manual), no tests.
- Caso especial: T022/T023/T025 se reportan **hechas a nivel teórico** (decisión de jefatura para ganar
  tiempo); su remanente técnico real vive en F-003/F-004/F-005. No reabrir en el cronograma.

### 3 — Actualizar `docs/cronograma.xlsx`
Editá con ImportExcel (`Open-ExcelPackage` / `Set-ExcelRange` / `Close-ExcelPackage`). Es artefacto de
jefatura (docs), no código de app.
- Columna **`SEGUIMIENTO JEFATURA`** (col 14): estado real corto + enlace lógico a la ficha de evidencia
  (p.ej. `Hecho — ver evidencias/2026-06-30_T026`).
- Ajustá **`HORAS PENDIENTES`** / **fechas** si el plan cambió.
- Regla dura: **jamás escribir "Hecho" sin la ficha de evidencia correspondiente en `evidencias/`.**

### 4 — Generar el paquete de evidencia ejecutivo (capa externa)
Por cada tarea **completada esta semana**, creá una ficha en `evidencias/` (estructura y plantilla en
`evidencias/LEEME.md`):

```
evidencias/<fecha-entrega>_<Txxx>_<slug>/
  informe.md          (ejecutivo, de negocio, LIMPIO — ver _PLANTILLA-informe.md)
  capturas/
    01-<desc>.png     (frontend real, vinculadas desde informe.md)
```

- **Capturas del frontend** con el método nativo ya usado en `docs/manual-*/capturas/`: **Edge headless
  vía CDP (Node, sin dependencias)** — manejar `msedge.exe --headless=new --remote-debugging-port=<p>
  --remote-allow-origins=*` y dirigirlo por CDP (`Page.captureScreenshot`). Levantar el stack antes
  (`build-api` → `run-api` → `serve-frontend`) e inyectar `sessionStorage.sjm_session` para saltar el
  login mock. (NO Playwright, salvo que el humano lo pida.)
- `informe.md`: lenguaje de negocio, qué entrega la tarea, capturas vinculadas, alcance cubierto, y
  **si no está al 100%** una justificación clara con fecha estimada. **Sin** una sola mención del proceso
  de desarrollo interno.
- Actualizá `evidencias/INDICE.md` (tabla: Tarea · Fecha entrega · Estado · enlace a la ficha).

### 5 — Cerrar el ritual (sin cerrar features)
- NO marqués features `done` ni podes el tablero — eso es de `/cerrar` + `reviewer` + puerta humana.
- Reportá: qué tareas cambiaron de estado en el xlsx y qué fichas de evidencia se generaron/actualizaron,
  listo para presentar a jefatura.
