# archive/legacy/ — Índice de artefactos preservados

> Todo lo viejo del **arnés previo** se preserva aquí **verbatim** (mismo contenido, misma ruta relativa
> original). Doble respaldo: vive en `archive/legacy/` **y** en el historial de git. Nada se perdió.
>
> Contexto del repo: hubo un arnés Copilot → migrado a Claude en el commit **`30f1c5a`** (estado
> pre-arnés "Harness Engineering", con los originales ricos). El commit **`42eaf1f`** ("[HARNESS_ETAPA_1]")
> es la **Etapa 1 de ESTE arnés ya commiteada** (CLAUDE.md/settings.json/implementer.md fueron
> reescritos ahí). Por eso los tres originales se recuperaron de **`30f1c5a`**, no de `42eaf1f`.

| Ruta original | Clasificación | Procedencia | Contenido integrado a (arnés nuevo) |
|---|---|---|---|
| `CLAUDE.md` (20622 B, rico) | reusable | recuperado de `30f1c5a` (el activo fue reescrito en `42eaf1f`) | `docs/architecture.md` + `docs/conventions.md` (arquitectura, auth, acceso a datos, errores, convenciones C#/frontend/BD, gotchas); comandos → `HARNESS-INSTALL.md` / `init.ps1` |
| `.claude/settings.json` (562 B) | reusable | recuperado de `30f1c5a` | `.claude/settings.json` (lista `allow` preservada + hooks nuevos) |
| `.claude/agents/implementer.md` (2142 B) | reusable | recuperado de `30f1c5a` | `.claude/agents/implementer.md` (rol estándar) |
| `.claude/agents/spec-researcher.md` (2564 B) | reusable | movido del working tree | `.claude/agents/spec_author.md` (rol renombrado) |
| `.claude/commands/spec.md` | reusable | movido del working tree | `docs/specs.md` (flujo SDD) + `.claude/commands/*` (comandos `/` nuevos) |
| `Inicial.md` | historical | movido del working tree | — (prompt del prototipo visual original; superado por el estado actual) |
| `docs/specs/` (29 archivos) | reusable (plantilla) + historical (instancias) | movido del working tree | plantilla/convención → `docs/specs.md`; las 29 specs son **registro histórico** de trabajo ya `done` (no se reconstruyen al formato 3-archivos; los specs nuevos van a `specs/<feature>/`) |
| └ `docs/specs/fix_customization_diagnostics_copilot_instructions_spec.md` | historical | (incluido en `docs/specs/`) | documenta el arnés **Copilot** predecesor (ya migrado/removido) |

## Notas

- Los tres "recuperados de `30f1c5a`" tenían su copia activa **reescrita** por la Etapa 1; el original
  rico se preserva aquí verbatim (cualquier detalle no destilado a `docs/` sigue recuperable desde aquí
  o desde git `30f1c5a`).
- **NO archivado (queda en su lugar, reportado):** `docs/db/_legacy/` — son scripts SQL de dominio
  (historial de BD que el equipo mantiene junto a `docs/db/`), **no** artefactos del arnés/
  context-engineering. Se deja donde está para no separar el SQL de su área de trabajo. (Si preferís
  archivarlo también, decímelo.)
- **No migrados (docs vivos del producto, referenciados por el arnés, permanecen activos):** `README.md`,
  `docs/PRP_Justificacion_Marcas.md`, `docs/db/Convenciones_Nomeclatura_BD.md`, `docs/manual-*`,
  `docs/PROMPT-GENERACION-MANUALES.md`, `deepwiki-local/`, `.mcp.json`, `.vscode/tasks.json`.
