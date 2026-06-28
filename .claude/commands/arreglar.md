---
description: Corrige un problema — ubica reciente vs archivado, reproduce, halla causa, muestra evidencia.
argument-hint: <descripción del problema>
---

Corrección: `$ARGUMENTS`.

1. Averiguá si es reciente o de una feature ya archivada. Si está archivada, traé su nota desde
   `archive/` (el índice apunta dónde) y sus specs en `archive/specs/`.
2. Reproducí el problema y hallá la **causa raíz** (no el síntoma).
3. Aplicá la compuerta de complejidad (`docs/specs.md`): trivial → `implementer` directo; con riesgo →
   SDD.
4. Mostrá **evidencia** de que quedó arreglado (salida de tests / `init.ps1` verde).
