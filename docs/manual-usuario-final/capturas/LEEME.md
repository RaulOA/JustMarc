# Capturas — Manual de Usuario Final

Esta carpeta contiene las imágenes que acompañan al manual. En la versión 1.0.0 las capturas **están pendientes** (TODO): se generarán con el MCP de Playwright navegando la app en `http://localhost:8000` (frontend) con la API en `http://localhost:5093` y la base de datos poblada con los datos demo (Secciones B y C de `docs/db/03_DatosSemilla.sql`).

## Capturas planificadas

| Archivo | Pantalla / momento | Leyenda sugerida |
|---|---|---|
| `01-pantalla-ingreso.png` | Pantalla de ingreso | Campos Usuario y Contraseña, botón Ingresar |
| `02-panel-funcionario-formulario.png` | Panel Funcionario | Formulario "Nueva Justificación" con Motivo General |
| `03-agregar-linea-detalle.png` | Panel Funcionario | Tabla de líneas de detalle agregadas |
| `04-mi-historial.png` | Panel Funcionario | Tabla "Mi Historial de Justificaciones" |
| `05-panel-jefatura-pendientes.png` | Panel Jefatura | Tabla "Solicitudes Pendientes" con el contador |
| `06-aprobar-rechazar.png` | Panel Jefatura | Botones Aprobar y Rechazar en una fila |
| `07-detalle-boleta.png` | Panel Jefatura | Detalle desplegado de una boleta |
| `08-panel-rrhh-filtros.png` | Panel RRHH | Barra de filtros y tabla global |
| `09-consulta-historica-sifcnp.png` | Consulta Histórica (SIFCNP) | Filtros de fecha y resultados |
| `10-aviso-sesion.png` | Cualquier pantalla | Ventana "Sesión por Expirar" |

## Cómo generarlas (cuando Playwright esté disponible)

1. Levantar el full-stack (tarea de VS Code `start-full-stack`): API en `:5093` y frontend en `:8000`.
2. Asegurar la base de datos `INTEGRA_CNP` con datos demo cargados.
3. Con el MCP de Playwright, ingresar con cada usuario demo (`funcionario.ana`, `jefe.maria`, `rrhh.carlos`) y capturar cada pantalla de la tabla.
4. Guardar cada imagen con el nombre indicado en esta carpeta.
5. Las leyendas y los puntos de inserción ya están en `manual-usuario-final.md` (marcadores "📷 Captura pendiente").
