# Guía del arnés — cómo operar (Justificación de Marca / INTEGRA_CNP)

> Guía **para el humano**. Cómo arrancar y operar el arnés "Harness Engineering" día a día. El mapa para
> el **agente** está en `AGENTS.md`; las reglas duras en `CLAUDE.md` y `docs/constitution.md`.

## En una línea

El arnés hace que el agente trabaje como **leader** (orquesta, no codifica), una feature a la vez, con
specs aprobados por vos (puerta humana), verificación ejecutable (`init.ps1`) y todo el estado en
archivos del repo. **Viaja por git:** en otra máquina, `git pull` y listo — nada que instalar.

## Arranque rápido

1. `git pull`.
2. `pwsh ./init.ps1` (o pedí `/retomar`). Debe terminar **VERDE**.
3. Trabajás con los **comandos `/`** (abajo). No hace falta memorizar ceremonias.

## El loop diario (comandos `/`)

| Comando | Para qué |
|---|---|
| `/retomar` | Ponerse al día (lee `progress/` + tablero). No trabaja. |
| `/idea <texto>` | Capturar una idea al `backlog` sin perder el foco. |
| `/nueva <texto>` | Arrancar trabajo nuevo (aplica la compuerta SDD vs directo). |
| `/arreglar <texto>` | Corregir un problema (reciente o archivado) con evidencia. |
| `/planificar` | Ordenar el backlog contra el cronograma. No ejecuta. |
| `/cerrar` | Ritual de cierre: `init.ps1` verde, destila a `history`, archiva, rota. |
| `/estado` | Vistazo de una pantalla. Sin acción. |

Principio: **capturar ≠ ejecutar**. Las ideas se capturan; se ejecuta **una a la vez**. Detalle en
`docs/workflow.md`.

## Cómo arranco el flujo SDD (la ceremonia)

> Hay una feature `pending` con `"sdd": true`. Decime *"implementá la siguiente feature pendiente"*.

1. El leader lanza **`spec_author`**, que escribe los 3 specs en `specs/<feature>/` (`requirements.md`
   EARS, `design.md`, `tasks.md`) y **para** en `spec_ready`.
2. Vos leés `specs/<feature>/` y respondés **"aprobado"** (o pedís cambios).
3. Recién ahí el leader escribe `approved_by`/`approved_at` en `feature_list.json` (**puerta humana**),
   pasa a `in_progress` y corre **`implementer` → `reviewer`** hasta `done`.

La feature nunca pasa a `in_progress` sin tu aprobación escrita; el agente nunca se autoaprueba ni marca
`done` solo. Esto está blindado en `docs/constitution.md` y lo verifica `init.ps1`.

## Modo hands-off (auto)

Si querés que avance sin interrumpirte, corré en modo **`auto`**: el agente ejecuta el loop y solo te
frena en lo de impacto real (la puerta humana de aprobación de specs, operaciones destructivas sobre
bases reales — ver constitución principio 7). El resto lo resuelve y te lo informa.

## Verificación

- `pwsh ./init.ps1` es la verdad: toolchain → archivos del arnés → invariantes del tablero → build →
  tests → retención. Verde = sano.
- Hooks (`.claude/settings.json`): al cerrar la sesión (`Stop`) corre `init.ps1` y **bloquea** si está
  rojo; al editar un `.cs` recompila como chequeo barato.

## Stack y comandos (referencia)

- **Stack:** frontend HTML/CSS/JS vanilla (raíz: `index.html`, `dashboard.html`, `app.js`, `style.css`);
  API .NET 8 Clean Architecture (`backend/src/IntegradorMarcas.{Domain,Application,Infrastructure,Api}`);
  tests xUnit (`backend/tests/IntegradorMarcas.Tests/`); SQL Server `INTEGRA_CNP` (`docs/db/`).
- **Comandos** (fuente de verdad: bloque de configuración de `init.ps1` y `.vscode/tasks.json`):
  - TEST: `dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "Category!=Integration"`
  - BUILD: `dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug`
  - RUN: API `dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --no-build` (5093) + frontend `python -m http.server 8000 --directory .` (task `start-full-stack`)
  - LINT / TYPECHECK: no aplican en este repo (vacíos).

## Archivos clave

- `AGENTS.md` — mapa de navegación del agente.
- `CLAUDE.md` — rol leader + reglas duras.
- `docs/constitution.md` — principios inmutables (contrato).
- `docs/specs.md` / `docs/architecture.md` / `docs/conventions.md` / `docs/verification.md` / `docs/workflow.md`.
- `feature_list.json` — tablero. `progress/` — memoria. `archive/legacy/` — lo viejo (con `INDEX.md`).
