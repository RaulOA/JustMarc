---
description: Arranca trabajo nuevo aplicando la compuerta de complejidad y el flujo correcto.
argument-hint: <descripción del trabajo>
---

Trabajo nuevo: `$ARGUMENTS`.

1. Si ya hay una feature `in_progress`, respetá una-cosa-a-la-vez: preguntá si encolar o cambiar de foco.
2. Aplicá la **compuerta de complejidad** (`docs/specs.md`): multi-archivo / riesgoso / toca UI o
   contrato → `"sdd": true`; trivial o un solo archivo → directo.
3. Agregá la feature a `feature_list.json` (`status: pending`, con `sdd` si corresponde).
4. Arrancá el flujo: si es `sdd`, lanzá `spec_author` y **pará** en la puerta humana; si es directo,
   lanzá `implementer`.
