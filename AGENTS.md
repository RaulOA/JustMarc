# AGENTS.md — Mapa de navegación del arnés

> Divulgación progresiva: no leas todo de una. Esta tabla te dice **qué leer y cuándo**.

## Al empezar (siempre)
1. `pwsh ./init.ps1` — verifica toolchain, tablero y código (fail-fast).
2. `progress/current.md` — en qué quedó la sesión activa.
3. `feature_list.json` — el tablero vivo (qué hay `pending`/`in_progress`).

## Qué es cada cosa y cuándo leerla
| Archivo | Qué es | Cuándo leerlo |
|---|---|---|
| `CLAUDE.md` | Tu rol (leader) y reglas duras | Siempre, al arrancar |
| `feature_list.json` | Tablero vivo de features | Al elegir/seguir trabajo |
| `docs/constitution.md` | Principios inmutables (contrato) | Antes de decidir o cerrar |
| `docs/specs.md` | Proceso SDD + EARS + puerta humana | Al crear/implementar una feature `sdd` |
| `docs/architecture.md` | Capas, rutas de código/tests, qué NO hacer | Antes de tocar código |
| `docs/conventions.md` | Estilo y nombres del proyecto | Al escribir código (subagentes) |
| `docs/verification.md` | Niveles de prueba, `R<n>→test` | Al verificar/cerrar |
| `docs/workflow.md` | Loop diario, comandos `/`, retención | Para el día a día |
| `progress/current.md` | Estado de la sesión activa | Al arrancar/retomar |
| `progress/history.md` | Bitácora compacta de lo terminado | `/retomar`, `/arreglar` (dirigido) |
| `progress/reports.md` | Informes detallados (anti-teléfono) | Al revisar un informe puntual |
| `progress/backlog.md` | Ideas sueltas con fecha | `/idea`, `/planificar` |
| `CHECKPOINTS.md` | Criterios de "estado final correcto" | Antes de cerrar / en review |
| `guia-del-arnes.md` | Cómo operar el arnés (humano): loop, comandos `/`, ceremonia SDD, hands-off | Para operar/arrancar |

## Reglas duras (resumen; manda la constitución)
- Una feature a la vez (`one_feature_at_a_time`).
- Sin spec aprobado por humano no hay código de feature `sdd`.
- Ninguna feature es `done` sin `init.ps1` verde y trazabilidad `R<n>→test`.
- Ante fallo de herramienta: parar, `blocked`, documentar. No improvisar.

## Flujo SDD (resumen)
`pending → [spec_author] → spec_ready → ⏸ HUMANO aprueba → in_progress → [implementer → reviewer] → done`
Detalle: `docs/specs.md`.

## Ritual de cierre
`/cerrar`: init.ps1 verde → destila a `history.md` → poda el tablero → archiva specs `done` → rota
`reports.md` si excede umbral. Detalle: `docs/workflow.md`.
