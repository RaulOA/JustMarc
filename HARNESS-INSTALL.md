# HARNESS-INSTALL — Traspaso de instalación del arnés "Harness Engineering"

> Documento de estado entre etapas. Vive en el repo (versionado) para viajar entre máquinas vía git.
> Etapa actual: **0 / Investigación y plan**. En esta etapa **NO se creó el arnés**: solo se investigó
> y se propuso el plan. La construcción ocurre en etapas posteriores, tras la confirmación humana.

---

## 1) Stack, layout y comandos (🔎 derivado de este repo)

### Stack real

| Capa | Tecnología real (verificada en manifiestos/código) |
|---|---|
| **Backend** | .NET 8 (`net8.0`), C#, Clean Architecture en 4 proyectos. ADO.NET crudo (`Microsoft.Data.SqlClient` 7.0.0) + Dapper 2.1.72. **Sin EF Core.** |
| **Frontend** | HTML/CSS/JavaScript **vanilla** en la raíz del repo. Sin framework, sin bundler, sin paso de build, sin TypeScript, sin `package.json`. |
| **Base de datos** | SQL Server 2022, base `INTEGRA_CNP`. Scripts SQL consolidados en `docs/db/`. |
| **Tests** | xUnit (`backend/tests/IntegradorMarcas.Tests`), coverlet para cobertura. |
| **Gestor de paquetes** | NuGet (vía `dotnet restore backend/`). No hay npm/yarn/pip como toolchain del producto. |
| **Plataforma** | Windows 10/11 + PowerShell (todos los helpers de puertos son PowerShell-only). |
| **Build/SLN** | **No hay `.sln`**: todo comando `dotnet` referencia rutas `.csproj` explícitas. |
| **SDK** | Máquina con .NET SDK 10.0.301; proyectos `net8.0` por roll-forward (requiere runtime/targeting pack net8.0). |

### Dónde vive el código y los tests (rutas reales)

- **Frontend:** raíz del repo → `index.html` (login), `dashboard.html` (app por roles), `app.js` (~2227 líneas, script global único), `style.css`.
- **Backend:** `backend/src/` en 4 proyectos (regla de dependencia hacia adentro):
  - `backend/src/IntegradorMarcas.Domain` (entidades, constantes)
  - `backend/src/IntegradorMarcas.Application` (DTOs, Interfaces, Services, Validation)
  - `backend/src/IntegradorMarcas.Infrastructure` (Data, Queries/*Sql.cs, Repositories)
  - `backend/src/IntegradorMarcas.Api` (Controllers, Contracts, Security)
- **Tests:** `backend/tests/IntegradorMarcas.Tests/` (xUnit). Archivos: `JustificacionServiceCurrentApproverTests.cs`, `JustificacionServiceHistoricoTests.cs`, `ErrorLogIntegrationTests.cs` (gateado `[Trait Category=Integration]`, golpea BD real), `UnitTest1.cs` (scaffolding leftover).
- **BD:** `docs/db/` (`01_CrearBaseDatos.sql`, `02_EstructuraCompleta.sql`, `03_DatosSemilla.sql`, + `_legacy/`).

### Los 5 comandos (fuente de verdad: `.vscode/tasks.json` + `CLAUDE.md`)

| Comando | Valor real |
|---|---|
| **TEST** | `dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --verbosity=normal --logger "console;verbosity=detailed"` <br>(solo unitarios: añadir `--filter "Category!=Integration"`) |
| **LINT** | *(vacío)* — no hay linter configurado (sin ESLint/Prettier/StyleCop/.editorconfig/.globalconfig). |
| **TYPECHECK** | *(vacío)* — .NET no tiene typecheck separado (lo cubre el compilador en BUILD); frontend es JS vanilla sin TS. |
| **BUILD** | `dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug` |
| **RUN** | API: `dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --no-build` (puerto 5093). <br>Frontend: `python -m http.server 8000 --directory .` (puerto 8000). <br>Stack completo: task `start-full-stack` (build-api → run-api → serve-frontend). |

> Nota: `run-api` usa `--no-build` → requiere `build-api` previo. Health probe: `GET /health`.

---

## 2) Inventario de context-engineering previo, por intención (🔎)

Este repo **ya pasó por una migración de arnés**: tuvo un arnés Copilot (`.github/copilot-instructions.md`)
que fue migrado a un arnés Claude (`.claude/` + `docs/specs/`). `.github/` quedó **vacío** (solo el directorio).
Ahora se estandariza sobre el arnés "Harness Engineering".

### Clasificación

#### A. REUTILIZABLE — alimenta el arnés estándar

| Artefacto | Intención | Componente destino del arnés (provisional, se fija en Etapa 1+) |
|---|---|---|
| `CLAUDE.md` (raíz, 20 KB) | Instruir a la IA + arquitectura + comandos + convenciones + gotchas. **Es el artefacto central.** | Contexto de proyecto / "constitución" + convenciones + memoria de gotchas |
| `.claude/commands/spec.md` | Flujo spec-driven con puerta humana (SDD) | Definición del workflow SDD del arnés |
| `.claude/agents/spec-researcher.md` | Rol de agente: investiga y escribe spec | Rol **spec_author** (renombrar archivo a `.claude/agents/spec_author.md`) |
| `.claude/agents/implementer.md` | Rol de agente: implementa desde spec | Rol **implementer** (se conserva) |
| `docs/specs/` (28 specs + plantilla) | Specs históricos siguiendo plantilla SDD | **Migrar a `specs/<feature>/`** (top-level); la plantilla se promueve a convención del arnés |
| `docs/db/Convenciones_Nomeclatura_BD.md` | Convenciones/estándares de BD | Estándares del arnés (BD) |
| `docs/PRP_Justificacion_Marcas.md` | Product Requirements Plan/Prompt: requisitos (RF/RNF) + arquitectura + checklist de progreso | Ancla de requisitos para trazabilidad `R<n> → test` |
| `README.md` | Onboarding + comandos (humano) | Fuente de contexto de proyecto |
| `docs/PROMPT-GENERACION-MANUALES.md` | Prompt/persona de "documentador técnico" (recipe reproducible) | Rol/recipe auxiliar (documentación) |
| `.claude/settings.json` + `settings.local.json` | Permisos/config del harness | Config del arnés (se fusiona) |
| `.vscode/tasks.json` | Definición canónica de comandos (TEST/BUILD/RUN) | Fuente de los 5 comandos |
| `.mcp.json` | Config de servidores MCP (tooling) | Config de tooling del arnés |
| `deepwiki-local/` (32 archivos tracked) | Wiki estático anclado al código real (preservación de contexto/onboarding) | Recurso de referencia; herramienta autónoma, se mantiene en su sitio |

#### B. HISTÓRICO — preservar verbatim en `archive/legacy/`, no reutilizar

| Artefacto | Por qué es histórico |
|---|---|
| `Inicial.md` | Prompt original del prototipo visual (Sonnet); superado por el estado actual del producto |
| `docs/db/_legacy/` (10 scripts SQL + README) | Ya es el archivo legado propio del proyecto (scripts 001–008 + fixes), reemplazados por los consolidados 01–03 |
| `docs/specs/fix_customization_diagnostics_copilot_instructions_spec.md` | Documenta el arnés Copilot **predecesor** (ya migrado/removido) |

#### C. AUSENCIAS / ESTADO (no son artefactos, pero importan)

- **`.github/` vacío:** el arnés Copilot previo (`copilot-instructions.md`) fue removido. Confirma una migración previa.
- **Sin** `AGENTS.md`, `.cursorrules`, `.windsurfrules`, `GEMINI.md`, `.aider*`, `.continue`, `tsconfig`, `package.json`, `.editorconfig`.
- **`.playwright-mcp/`** está git-ignored (no versionado).
- **Deliverables que NO son context-engineering** (se quedan como docs del producto, ni reutilizar ni archivar): `docs/manual-administrador/`, `docs/manual-tecnico/`, `docs/manual-usuario-final/`, `docs/seguridad/`, demás `docs/db/*`.

---

## 3) Decisiones (RESUELTAS por el usuario)

**1–3 — Esqueleto estándar (invariante en todos los proyectos del usuario):**

- Estructura del arnés en **`.claude/` + `docs/` + `progress/` + `archive/`**.
- Roles: **crear `leader` y `reviewer`**; **renombrar `spec-researcher → spec_author`**; conservar `implementer`. Patrón completo: leader / spec_author / implementer / reviewer.
- Specs migran a **`specs/<feature>/`** (top-level, una carpeta por feature); **migrar las 28 de `docs/specs/`**.

**4 — Trazabilidad `R<n> → test`:** usar los **RF/RNF del PRP** (`docs/PRP_Justificacion_Marcas.md`) como fuente de los `R<n>`.

**5 — Alcance:** **vehículos queda FUERA por ahora** (`docs/vehiculos/PRP_Vehiculos.md` es un módulo futuro confirmado; se desarrollará más adelante). El arnés cubre solo "Justificación de Marca".

**6 — deepwiki:** **se conserva versionado** (sigue tracked). Pendiente menor: corregir el texto de `deepwiki-local/LEEME.md` que afirma estar gitignored.

### Esqueleto estándar acordado (estructura objetivo)

```text
.claude/
  agents/        leader.md, spec_author.md, implementer.md, reviewer.md
  commands/      spec.md (workflow SDD con puerta humana)
  settings.json, settings.local.json
docs/            convenciones, PRP (ancla R<n>), README, manuales, db/, seguridad/ ...
specs/
  <feature>/     specs migradas de docs/specs/ (una carpeta por feature)
progress/        memoria/estado entre sesiones (fuera del chat, versionado)
archive/
  legacy/        Inicial.md, docs/db/_legacy/, spec del arnés Copilot previo
```

> Las decisiones 1–6 quedan grabadas aquí (estado versionado). Construcción del esqueleto y migración: **Etapa 1+**.

---

## Etapa 1 — Esqueleto creado (archivos)

Raíz: `CLAUDE.md` (reescrito: leader-forcing; el contenido de producto del viejo se destiló a
`docs/architecture.md`/`docs/conventions.md` y sigue en git), `AGENTS.md`, `feature_list.json`,
`CHECKPOINTS.md`, `init.ps1`.
`docs/`: `specs.md`, `architecture.md`, `conventions.md`, `verification.md`, `constitution.md`,
`workflow.md`.
`.claude/agents/`: `leader.md`, `spec_author.md`, `implementer.md` (reescrito), `reviewer.md`.
`.claude/`: `settings.json` (hooks añadidos, permisos viejos preservados), `hooks/post_edit.ps1`.
`.claude/commands/`: `retomar.md`, `idea.md`, `nueva.md`, `arreglar.md`, `planificar.md`, `cerrar.md`,
`estado.md`.
`progress/`: `current.md`, `history.md`, `reports.md`, `backlog.md`.

Notas de estructura (resueltas por estándar, informadas):

- Hook barato: el repo no tiene lint/typecheck, así que `post_edit.ps1` recompila el backend solo en
  ediciones `.cs` (`.claude/hooks/post_edit.ps1`); el Stop hook corre `init.ps1` y bloquea con exit 2.
- `init.ps1` referencia **solo** los comandos del bloque de configuración (TEST/BUILD del proyecto;
  LINT/TYPECHECK vacíos → omitidos). TEST usa `--filter "Category!=Integration"` (sin BD ni red).
- Nada viejo migrado/borrado aún (Etapa 2): siguen `docs/specs/*`, `Inicial.md`, `docs/db/_legacy/`,
  `.claude/agents/spec-researcher.md`, `.claude/commands/spec.md`.

## Progreso de etapas

- [x] Etapa 0 — Investigación y plan
- [x] Etapa 1 — Esqueleto del arnés
- [ ] Etapa 2
- [ ] Etapa 3
