# Capturas / Diagramas — Manual Técnico

Esta carpeta contiene los diagramas e imágenes del manual técnico. En la versión 1.0.0 están **pendientes** (TODO). A diferencia de los manuales de usuario y administrador (capturas de pantalla con Playwright), aquí varias imágenes son **diagramas** que pueden generarse con una herramienta de diagramación (por ejemplo, Mermaid, draw.io o PlantUML).

## Imágenes planificadas

| Archivo | Tipo | Contenido |
|---|---|---|
| `01-diagrama-despliegue.png` | Diagrama | Vista de despliegue: navegador → frontend → API .NET → SQL Server → WIZDOM/SIFCNP |
| `02-modelo-datos.png` | Diagrama ER | Esquemas Configuracion, RecursosHumanos, Operacion, Auditoria, Integracion y sus relaciones |
| `03-secuencia-crear-resolver.png` | Diagrama de secuencia | Flujo de crear y resolver una boleta (controller → service → repositorio → BD) |

## Sugerencia de generación

- Los diagramas de despliegue y secuencia pueden escribirse en **Mermaid** dentro del propio `.md` o exportarse a PNG.
- El diagrama ER puede derivarse de `docs/db/02_EstructuraCompleta.sql`.
- Las capturas de pantalla de la interfaz (si se desea ilustrar Swagger o el panel admin) se generan con el MCP de Playwright, igual que en los otros manuales (frontend en `:8000`, API en `:5093`).
