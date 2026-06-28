# docs/verification.md — Verificación

> Niveles adaptados al stack. Comandos reales en `HARNESS-INSTALL.md` y `init.ps1`.

## Niveles

| Nivel | Obligatorio | Cómo |
|---|---|---|
| Unitarios | **Sí** | xUnit en `backend/tests/IntegradorMarcas.Tests/`. `dotnet test ... --filter "Category!=Integration"` |
| Integración / E2E UI | Sí **si toca interfaz** o BD real | Tests `[Trait Category=Integration]` (golpean BD real; fuera del gate por defecto). UI: validar paneles por rol en el frontend. |
| Smoke manual | Opcional | Levantar `start-full-stack`, probar login + flujo por rol. `GET /health`. |
| Trazabilidad `R<n>→test` | **Sí** para features `sdd` | Cada `R<n>` de `requirements.md` mapeado a un test nombrado. |

## Gate (`init.ps1`)

Corre Build + Test (unitarios, con `--filter "Category!=Integration"` → **sin BD ni red**). Verde =
listo para revisar/cerrar.

## Trazabilidad

- En `tasks.md`, cada `T<n>` cita sus `R<n>`.
- El `implementer` deja en `progress/reports.md` un mapa **`R<n> → test`** con la salida de los tests.
- El `reviewer` rechaza si algún `R<n>` no tiene test.

## Anti-patrones

- ❌ Tests que no asersan nada / siempre verdes.
- ❌ Depender del test de integración con cadena quemada (`ErrorLogIntegrationTests`) para el gate.
- ❌ Marcar `done` sin `init.ps1` verde.
- ❌ "Lo probé a mano" como única evidencia para una feature `sdd`.
