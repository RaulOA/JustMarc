# Spec: Seccion README para REST Client (.http)

## 1) Contexto revisado
Archivos REST Client relacionados detectados en el workspace:
- `backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.http`
- `.vscode/settings.json`
- `.vscode/extensions.json` (recomendacion `humao.rest-client`)

Estado actual del README:
- Ya existe `## 7) Uso practico de Swagger`.
- No existe una seccion dedicada a archivos `.http` ni al flujo con REST Client.
- En `## 13) Ejecutar desde VS Code` ya se recomienda la extension `humao.rest-client`.

## 2) Ubicacion propuesta en README
Insertar una nueva seccion inmediatamente despues de `## 7) Uso practico de Swagger` y antes de la seccion actual de pruebas (`## 8) Pruebas creadas`).

Motivo:
- Mantiene juntas las dos formas de probar la API (Swagger y REST Client).
- Evita duplicar conceptos dentro de la seccion 13 (VS Code), dejandola solo como referencia de tooling.

## 3) Titulo de seccion propuesto
`## 8) Uso de archivos .http con REST Client (VS Code)`

Nota de numeracion:
- Al insertar esta seccion, las secciones posteriores se renumeran +1.

## 4) Subsecciones exactas a incluir
1. `### Que es y para que usarlo`
2. `### Archivo base del proyecto`
3. `### Variables de entorno en VS Code (.vscode/settings.json)`
4. `### Ejecucion paso a paso en VS Code`
5. `### Ejemplos recomendados para validar`
6. `### Troubleshooting rapido`

## 5) Ejemplos concretos que deben mencionarse
Tomar ejemplos reales de `backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.http`:
- Variable host:
  - `@IntegradorMarcas.Api_HostAddress = {{apiBaseUrl}}`
- Health check:
  - `GET {{IntegradorMarcas.Api_HostAddress}}/health`
- Endpoint con identidad:
  - `GET {{IntegradorMarcas.Api_HostAddress}}/api/rrhh/justificaciones`
  - `X-User-Id: {{userId}}`
  - `X-User-Role: {{userRole}}`
- Separador de requests:
  - `###`

Tomar ejemplos reales de `.vscode/settings.json`:
- Clave de configuracion:
  - `rest-client.environmentVariables`
- Variables compartidas (`$shared`):
  - `apiBaseUrl: http://localhost:5093`
  - `userId: 6`
  - `userRole: ROL_RRHH`
- Entorno local (`local`) con override de `apiBaseUrl`.

Referencia complementaria a `.vscode/extensions.json`:
- Mencionar que la extension recomendada es `humao.rest-client`.

## 6) Lineamientos de redaccion (alineados al tono actual del README)
- Estilo practico, directo y orientado a ejecucion local en Windows.
- Frases cortas, verbos de accion y listas de pasos numerados cuando aplique.
- Mantener formato de comandos y snippets en bloques (como el resto del README).
- Evitar teoria extensa; priorizar "como correrlo" y "que validar".
- Reutilizar lenguaje existente del README: "copiar/pegar", "flujo rapido", "troubleshooting".
- Mantener consistencia con headers requeridos por la API (`X-User-Id`, `X-User-Role`) y roles validos.

## 7) Alcance de esta insercion
- Solo documentacion en README.
- Sin cambios funcionales en backend/frontend.
- Sin mover ni renombrar archivos `.http` o `.vscode`.
