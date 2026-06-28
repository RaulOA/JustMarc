# DeepWiki local — SIFCNP / INTEGRA_CNP

Wiki estático y navegable que explica el proyecto completo, anclado al **código real**. Es una **herramienta de apoyo para quienes desarrollan**, NO un entregable del producto ni parte de la aplicación.

## Cómo abrirlo

**Hacé doble clic en `index.html`.** Se abre en el navegador como `file://`, sin servidor, sin compilar y sin internet. Eso es todo.

> 💡 Recomendado: Microsoft Edge, Google Chrome o Firefox recientes. Los diagramas (Mermaid) y la búsqueda funcionan offline porque todo el JS/CSS está vendorizado en `assets/`.

## Qué incluye

- `*.html` — una página por sección del wiki (Overview, Arquitectura, Seguridad, una por capa, Modelo de datos, Referencia de API, Flujos, Glosario, Ask).
- `assets/` — `styles.css`, `wiki.js` (tema claro/oscuro, búsqueda, menú móvil), `mermaid.min.js` (build UMD local) y `search-index.js` (índice de búsqueda embebido).
- `_src/` — fuentes para regenerar: el contenido en Markdown (`_src/content/*.md`) y el generador (`_src/build.js`).

## Cómo regenerarlo

El sitio se genera a partir del Markdown de `_src/content/`:

```
node _src/build.js
```

Eso reescribe los `*.html` y `assets/search-index.js`. **No** toca `assets/styles.css` ni `assets/wiki.js` (están escritos a mano), y conserva el `assets/mermaid.min.js` ya vendorizado. Para refrescar Mermaid: `npm install mermaid@10` y copiar `node_modules/mermaid/dist/mermaid.min.js` a `assets/`.

## Aislamiento (importante)

Esta carpeta vive dentro del repo pero **no se acopla al proyecto**: no se importa desde el código de la app, no está en el build, `package.json`, linters, tests ni rutas, y está en `.gitignore` (no se versiona). Borrarla no afecta a la aplicación en nada.

## Notas

- Los enlaces "al código" apuntan a los archivos reales del repo con rutas relativas (`../backend/...`, `../app.js`, `../docs/...`). Abren el archivo en el navegador (no saltan a la línea; el rango se indica en el texto del enlace).
- La página **Ask** (chat conversacional sobre el código) está **prevista pero inactiva**: requiere un modelo de fondo. Por ahora, "preguntale" al wiki con el **buscador** (arriba a la izquierda).
- Generado a partir del código del repositorio. Los `TODO` que aparecen marcan datos que el código no revela (decisiones futuras del equipo).
