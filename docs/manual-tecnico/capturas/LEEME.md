# Capturas / Diagramas — Manual Técnico

Esta carpeta contiene los diagramas del manual técnico. **Ya están generados.** A diferencia de los manuales de usuario y administrador (capturas de pantalla de la app), aquí las imágenes son **diagramas** derivados del código y del esquema reales.

> **Herramienta usada:** cada diagrama está escrito en **Mermaid** (archivo fuente `.mmd` versionado junto al PNG) y se **renderizó a PNG** con Mermaid ejecutado en **Microsoft Edge headless dirigido por CDP** desde un script de **Node** nativo. Así, el PNG (compatible con cualquier visor y con exportación a PDF) y la fuente Mermaid editable se mantienen sincronizados.

## Diagramas generados

| Archivo | Fuente | Tipo | Contenido | Estado |
|---|---|---|---|---|
| `01-diagrama-despliegue.png` | `01-diagrama-despliegue.mmd` | Diagrama de despliegue | Navegador → frontend → API .NET 8 → SQL Server (INTEGRA_CNP) → WIZDOM/SIFCNP (solo lectura); IIS en producción | ✓ Generado |
| `02-modelo-datos.png` | `02-modelo-datos.mmd` | Diagrama ER | Entidades de los esquemas Configuracion, RecursosHumanos, Operacion y Auditoria y sus relaciones (PK/FK) | ✓ Generado |
| `03-secuencia-crear-resolver.png` | `03-secuencia-crear-resolver.mmd` | Diagrama de secuencia | Crear y resolver una boleta (controlador → servicio → repositorio → BD, con auditoría) | ✓ Generado |

Cada imagen ya está enlazada en `manual-tecnico.md` con su texto alternativo descriptivo (accesibilidad WCAG 2.2 §1.1.1) y su leyenda de figura.

## Cómo regenerarlos

- El diagrama de despliegue y el de secuencia derivan del código y de `manual-tecnico.md`; el diagrama ER deriva de `docs/db/02_EstructuraCompleta.sql`.
- Para regenerar los PNG: editar el `.mmd` correspondiente y volver a renderizar con Mermaid (CLI `mermaid` o un navegador headless con la librería Mermaid). Mantener el mismo nombre de archivo.
