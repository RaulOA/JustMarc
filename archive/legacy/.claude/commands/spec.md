---
description: Flujo spec-driven — investiga, genera un spec en docs/specs/ y (opcional) implementa
argument-hint: <descripcion de la tarea o feature>
---

Ejecuta el flujo spec-driven del proyecto para la siguiente tarea:

**Tarea:** $ARGUMENTS

Pasos:

1. **Investigacion y spec.** Lanza el subagente `spec-researcher` con la tarea. Producira un documento en `docs/specs/<slug>_spec.md` y devolvera un resumen de hallazgos + la ruta del spec.
2. **Revision.** Resume al usuario los hallazgos clave y la ruta del spec. Si la tarea es grande, ambigua o riesgosa, **pausa y pide confirmacion** antes de implementar. Para cambios pequenos y claros, continua.
3. **Implementacion.** Lanza el subagente `implementer` pasandole la ruta del spec. Aplicara los cambios, compilara y correra los tests.
4. **Cierre.** Reporta: archivos modificados, resultado de build/tests, y desviaciones respecto al spec.

Notas:
- Para solicitudes de **solo analisis/diagnostico**, ejecuta unicamente el paso 1 (spec-researcher) y entrega el spec, sin implementar.
- Mantén los specs en `docs/specs/` (convencion del proyecto, antes `docs/SubAgent docs/`).
- Respuesta directa, tecnica y en espanol.
