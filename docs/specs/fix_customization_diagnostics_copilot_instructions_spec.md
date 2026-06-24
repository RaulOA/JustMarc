# Spec: Fix Customization Diagnostics for .github/copilot-instructions.md

Fecha: 2026-05-13
Archivo analizado: `.github/copilot-instructions.md`
Objetivo: documentar problemas de calidad de instrucciones (contradicciones, ambiguedades, conflictos de persona, carga cognitiva y vacios de cobertura) y proponer ediciones minimas.

## 1) Supuestos

- No se proporciono una lista explicita de diagnosticos (linea/codigo/mensaje) de la extension de evaluacion.
- El analisis se realizo por inspeccion manual del archivo y con criterios alineados al intent de "Fix Diagnostics".
- Las propuestas buscan cambios minimos, preservando estructura e intencion general.

## 2) Hallazgos

### Hallazgo A: Flujo obligatorio contradice escenarios de solo investigacion (cobertura gap + rigidez)

Seccion citada (lineas 19-30):

> "## 4) Mandatory Two-Subagent Workflow (No Exceptions)"
>
> "2. Spawn Subagent #1 (Research and Spec):"
>
> "4. Spawn Subagent #2 (Implementation, fresh context):"

Racional:

- El "No Exceptions" obliga implementacion incluso cuando la solicitud del usuario es solo analisis/spec.
- Esto genera sobre-ejecucion y puede producir acciones no solicitadas.
- Es una brecha de cobertura porque no contempla tareas de diagnostico sin cambios de codigo.

Edicion minima propuesta:

- Cambiar el titulo a: `Mandatory Two-Subagent Workflow (Default)`.
- Agregar una excepcion breve bajo la lista:
  - `Exception: For research-only or diagnostics-only requests, run only Subagent #1 and return findings/spec.`

### Hallazgo B: Dependencia absoluta de runSubagent sin ruta de contingencia (coverage gap)

Secciones citadas (lineas 32-45, 83-84):

> "## 5) runSubagent Invocation Contract"
>
> "runSubagent( ... )"
>
> "If error is `missing required property`: provide both `description` and `prompt`."

Racional:

- El documento asume disponibilidad total de `runSubagent`.
- El manejo de errores cubre solo dos casos y no contempla indisponibilidad de herramienta, permisos o timeout.
- Falta instruccion para fallback seguro cuando la capacidad no existe.

Edicion minima propuesta:

- Agregar en seccion 8 una linea:
  - `If runSubagent is unavailable in the current environment, return a blocked status with reason and request user guidance.`

### Hallazgo C: Redundancia alta entre secciones 1, 3 y 7 (cognitive load)

Secciones citadas:

- Lineas 5-8:

> "You do not read files directly."
> "You do not edit or create code directly."

- Lineas 12-13:

> "Never read files yourself; delegate to a subagent."
> "Never edit or create code yourself; delegate to a subagent."

- Lineas 76-77:

> "No direct file reading."
> "No direct edit or create operations."

Racional:

- El mismo mandato aparece tres veces con redaccion casi identica.
- Esto incrementa ruido y costo cognitivo sin agregar precision operativa.

Edicion minima propuesta:

- Mantener la version mas fuerte en seccion 3.
- En seccion 1 y/o 7 reemplazar duplicados por referencia:
  - `See Section 3 for non-negotiable execution constraints.`

### Hallazgo D: Numeracion inconsistente de encabezados (claridad estructural)

Seccion citada (lineas 3 y 10):

> "## 1) Purpose and Role"
>
> "## 3) Core Operating Constraints"

Racional:

- Falta el bloque "2", lo que sugiere documento incompleto o edicion parcial.
- Aunque menor, afecta navegabilidad y confianza en la especificacion.

Edicion minima propuesta:

- Renumerar `## 3)` a `## 2)` y ajustar correlativos siguientes, o usar encabezados sin numeracion.

### Hallazgo E: Conflicto de estilo interno (persona/interaction conflict)

Secciones citadas (lineas 89-91, 95):

> "Use direct, concise, technical, non-conversational style."
> "Do not add questions, recommendations, or alternatives unless required."
> "If ambiguous, ask for minimal clarification in one line."

Racional:

- Se prohibe formular preguntas salvo requisito, pero se exige preguntar ante ambiguedad.
- No define explicitamente cuando una ambiguedad "requires" pregunta.
- Puede provocar comportamiento inconsistente entre agentes.

Edicion minima propuesta:

- Ajustar una sola linea para criterio operativo:
  - `Ask one-line clarification only when ambiguity blocks safe execution.`

### Hallazgo F: Restriccion "Always use the default subagent" es ambigua para tareas especializadas

Seccion citada (linea 14):

> "Always use the default subagent."

Racional:

- No aclara que hacer si existen subagentes especializados o si default no soporta una tarea.
- Puede reducir calidad en escenarios multi-dominio.

Edicion minima propuesta:

- Reemplazar por:
  - `Use the default subagent unless the task explicitly requires a specialized subagent.`

## 3) Propuesta de parche minimo consolidado (texto sugerido)

1. `## 4) Mandatory Two-Subagent Workflow (Default)`
2. Agregar despues del paso 4:
   - `Exception: For research-only or diagnostics-only requests, execute only Subagent #1.`
3. Seccion 8, agregar:
   - `If runSubagent is unavailable, return blocked status with cause and ask for user direction.`
4. Reducir duplicados en secciones 1 y 7 con referencia a seccion 3.
5. Corregir numeracion de encabezados (1,2,3...).
6. Seccion 9, reemplazar ultima linea por:
   - `If ambiguity blocks safe execution, ask one minimal clarification line.`
7. Linea 14, ajustar a:
   - `Use the default subagent unless explicit task constraints require otherwise.`

## 4) Riesgo de no corregir

- Ejecucion de pasos no solicitados por el usuario.
- Bloqueo operacional cuando `runSubagent` no esta disponible.
- Variacion de estilo/respuestas por ambiguedades de criterio.
- Mayor friccion para mantener instrucciones a futuro por redundancia.
