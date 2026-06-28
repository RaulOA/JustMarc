# Prompt usado para generar la primera versión de los manuales — fecha: 2026-06-27

> **Prompt usado para generar la primera versión de los manuales — fecha: 2026-06-27**
>
> Valores resueltos para este repositorio (el texto original usaba placeholders):
>
> - **[APP]** = Justificación de Marca (INTEGRA_CNP)
> - **[URL_BASE]** = `http://localhost:8000` (frontend en desarrollo); la API REST corre en `http://localhost:5093`.
>
> Objetivo de preservar este prompt: que cualquier instancia futura sepa con qué criterios y normas se construyeron los manuales originales. El texto que sigue es el prompt original, preservado tal cual.

---

## Tarea: generar manuales de usuario y técnico aplicando las normas respectivas

Sos un documentador técnico experto en normas ISO/IEC/IEEE y en automatización con MCP Playwright. Generá la documentación de [APP] a partir del código de ESTE repositorio. Escribí todo en español de Costa Rica (voseo cuando aplique), con lenguaje claro y pedagógico: explicá como si la persona usara la app por primera vez, con frases cortas y ejemplos. No inventés funcionalidades: todo sale del código real.

La app corre en: [URL_BASE]   (ej. <http://localhost:3000>)

## REGLA RECTORA — la norma manda

La estructura, el orden y las secciones obligatorias los define SIEMPRE la norma correspondiente. Si hay diferencia entre lo que prescribe la norma y lo que vos propondrías, MANDA la norma. Tu criterio solo se usa para rellenar lo que la norma deja abierto, nunca para reorganizarla.

## Fase 0 — Preservar este prompt

Antes de empezar, guardá ESTE prompt completo, tal cual, en `/docs/PROMPT-GENERACION-MANUALES.md`. Agregá al inicio la nota: "Prompt usado para generar la primera versión de los manuales — fecha: [fecha de hoy]". Objetivo: que cualquier instancia futura sepa con qué criterios y normas se construyeron los manuales originales.

## Fase 1 — Investigar el código

Explorá el repo: stack y dependencias, estructura de carpetas, rutas/páginas, roles y permisos, flujos principales, endpoints de API, modelo de datos, variables de entorno y pasos de despliegue. No omitás componentes del sistema.

## Fase 2 — Investigar las normas (fuentes públicas)

El texto íntegro de las ISO es de pago; usá índices, resúmenes y guías públicas para extraer su estructura y secciones obligatorias:

- Usuario final + administrador: ISO/IEC/IEEE 26514:2022 (estructura del documento) e IEC/IEEE 82079-1:2019 (principios de instrucciones de uso).
- Técnico (desarrolladores): ISO/IEC/IEEE 15289:2019 (contenido de los ítems de información) + ISO/IEC/IEEE 12207 (procesos del ciclo de vida del software) + ISO/IEC/IEEE 42010 (arquitectura) + OpenAPI Specification (API).
- Ayuda dentro de la web app: WCAG 2.2.
Resumí en una tabla qué secciones exige cada norma; ese índice es el esqueleto de cada manual. Guardá las URLs consultadas para citarlas al final.

## Fase 3 — Consultar vacíos

Antes de redactar, hacé UNA lista consolidada de la información que el código no revela y que debe aportar el desarrollador principal (ej. credenciales de prueba, datos de ejemplo, decisiones de negocio, alcance). Presentá esa lista y esperá respuesta. Solo lo que quede sin respuesta se marca como TODO en el documento.

## Fase 4 — Capturas con Playwright (MCP)

Para cada manual, identificá los puntos donde una imagen ayuda a entender (ingreso, formularios, paneles, resultados, mensajes). Usá el MCP de Playwright para navegar [URL_BASE], tomar capturas REALES y guardarlas en la carpeta `capturas/` del manual correspondiente, con nombre descriptivo (ej. `01-pantalla-ingreso.png`). Luego enlazalas en el .md con su leyenda. Priorizá los manuales de usuario y administrador.

## Fase 5 — Generar los manuales

Creá en `/docs` UNA carpeta por manual, cada una con su `.md`, un `CHANGELOG.md` y una carpeta `capturas/`:

/docs/manual-usuario-final/   → manual-usuario-final.md + CHANGELOG.md + capturas/
/docs/manual-administrador/   → manual-administrador.md + CHANGELOG.md + capturas/
/docs/manual-tecnico/         → manual-tecnico.md + CHANGELOG.md + capturas/

Contenido (siguiendo el esqueleto de la Fase 2):

1. Usuario final — alcance, requisitos, ingreso, tareas paso a paso, manejo de errores, glosario. (26514 + 82079-1)
2. Administrador — gestión de usuarios y roles, configuración, respaldos, auditoría, mantenimiento. (26514)
3. Técnico — arquitectura, componentes, modelo de datos, referencia de API en formato OpenAPI, flujos internos, instalación, configuración, despliegue, mantenimiento y gestión de cambios. Debe cubrir el ciclo de vida completo del software, sin omitir componentes. (15289 + 12207 + 42010 + OpenAPI)

Reglas de lenguaje:

- Los tres: tono pedagógico, orientado a tareas (qué quiere lograr la persona + pasos numerados), frases cortas.
- Usuario final y administrador: SIN tecnicismos informáticos. Usá términos cotidianos; si un término técnico es inevitable, explicalo en una línea. (Evitá "endpoint", "instancia", "deploy"; usá "dirección del sistema", "copia en funcionamiento", "publicación".)
- Técnico: puede usar lenguaje técnico, pero igual claro y bien explicado.

Reglas comunes:

- Encabezado con título, versión, fecha y público objetivo.
- Insertá las capturas de la Fase 4 donde corresponda, con leyenda.
- Glosario y sección de solución de problemas.
- CHANGELOG.md con formato: versión, fecha y cambios (empezá en 1.0.0).
- Al final de cada manual: tabla de trazabilidad (sección del manual ↔ cláusula de la norma que cumple) y sección "Fuentes" con las normas y URLs consultadas.

## Fase 6 — Revisar

Verificá cada manual contra la tabla de la Fase 2, confirmá que la estructura respeta la norma (regla rectora), que las capturas existen y están bien enlazadas, y listá los TODO pendientes.
