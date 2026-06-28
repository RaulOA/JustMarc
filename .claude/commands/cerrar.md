---
description: Ritual de cierre — init verde, destila a history, poda el tablero, archiva y rota.
---

Ritual de cierre:

1. Corré `pwsh ./init.ps1`; debe estar **verde**. Si no, no cierres.
2. Por cada feature `done`: destilala a **una línea** en `progress/history.md` (qué, cuándo,
   `approved_by`, resumen) y **podala** de `feature_list.json`.
3. Archivá specs de features `done` → `archive/specs/<feature>/`.
4. Si `progress/reports.md` supera ~800 líneas, **rotalo** a `archive/reports/reports-<fecha>.md` y dejá
   un encabezado fresco.
5. Dejá `progress/current.md` en su estado real. Reportá qué cerraste.
