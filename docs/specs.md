# docs/specs.md — Proceso SDD (Spec-Driven Development)

> Una feature `sdd` se especifica **antes** de implementarse. La spec es el contrato. Hay **puerta
> humana** obligatoria.

## Estructura

Cada feature `sdd` vive en `specs/<feature>/` con tres archivos:

- `requirements.md` — qué debe hacer (EARS estricto, ids `R<n>`).
- `design.md` — cómo: archivos, firmas, excepciones, **≥1 alternativa descartada**.
- `tasks.md` — checklist `[ ]` de `T<n>`, cada una citando sus `R<n>`.

## Estados y puerta humana

```text
pending --[spec_author]--> spec_ready --> (HUMANO aprueba) --> in_progress --[implementer->reviewer]--> done
```

| Estado | Significado | Quién lo escribe |
|---|---|---|
| `pending` | En el tablero, sin spec | humano / `/nueva` |
| `spec_ready` | Specs escritos, falta aprobación | `spec_author` |
| `in_progress` | Aprobada, en implementación | leader (tras `approved_by`/`approved_at` humanos) |
| `done` | Implementada, revisada, verde | tras `reviewer` + cierre humano |
| `blocked` | Frenada por un fallo | cualquiera, documentando |

**La puerta humana es inviolable:** `spec_ready → in_progress` requiere que el **humano** escriba
`approved_by` y `approved_at` en `feature_list.json`. El agente nunca se autoaprueba.

## EARS estricto

| Patrón | Plantilla |
|---|---|
| Ubicuo | `El sistema DEBE <acción>.` |
| Evento | `CUANDO <disparador>, el sistema DEBE <acción>.` |
| Estado | `MIENTRAS <estado>, el sistema DEBE <acción>.` |
| Opcional | `DONDE <feature opcional>, el sistema DEBE <acción>.` |
| No deseado | `SI <evento no deseado> ENTONCES el sistema DEBE <acción>.` |

Reglas:

- Ids estables `R1`, `R2`, … (no se renumeran).
- **Un solo `DEBE` por requisito.**
- Nada de verbos blandos ("debería", "podría", "soporta"): solo `DEBE` / `NO DEBE`.
- Cada `R<n>` debe ser **verificable por un test**.
- Trazabilidad `R<n> ↔ test` **obligatoria** (ver `docs/verification.md`).
- Fuente de requisitos del producto: los **RF/RNF del PRP** (`docs/PRP_Justificacion_Marcas.md`). Un
  `R<n>` de spec puede referenciar el RF/RNF que lo origina.

## Compuerta de complejidad

- `"sdd": true` **solo** si la feature es multi-archivo, riesgosa, o toca la interfaz/contrato.
- Cambio trivial o de un solo archivo → **sin SDD**: `implementer` directo, igual con test.

## Plantillas

`specs/<feature>/requirements.md`:

```text
# Requirements - <feature>
Origen: <RF/RNF del PRP si aplica>

## Requisitos (EARS)
- R1: El sistema DEBE ...
- R2: CUANDO ..., el sistema DEBE ...

## Cobertura de aceptacion
- <acceptance 1> -> R1, R2
```

`specs/<feature>/design.md`:

```text
# Design - <feature>
## Enfoque
## Archivos y firmas
## Errores/excepciones
## Alternativas descartadas
- <alternativa> - por que no
```

`specs/<feature>/tasks.md`:

```text
# Tasks - <feature>
- [ ] T1 (R1): ...
- [ ] T2 (R2, R3): ...
```
