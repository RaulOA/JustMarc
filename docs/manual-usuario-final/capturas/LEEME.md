# Capturas — Manual de Usuario Final

Esta carpeta contiene las imágenes que acompañan al manual. Las capturas **ya están generadas** (imágenes **reales** de la aplicación, no mockups): se tomaron navegando la app en `http://localhost:8000` (frontend) con la API en `http://localhost:5093` y la base de datos `INTEGRA_CNP` poblada con los datos demo (Secciones A+B+C de `docs/db/03_DatosSemilla.sql`) más una alineación mínima de personas demo para que cada panel muestre datos coherentes.

> **Herramienta usada:** el MCP de Playwright no estaba disponible en el entorno, así que las capturas se tomaron con **Microsoft Edge en modo headless dirigido por CDP** (Chrome DevTools Protocol) desde un script de **Node** nativo (`fetch` + `WebSocket`, sin dependencias externas). El resultado es equivalente: capturas reales de la app corriendo.

## Capturas generadas

| Archivo | Pantalla / momento | Leyenda | Estado |
|---|---|---|---|
| `01-pantalla-ingreso.png` | Pantalla de ingreso | Campos Usuario y Contraseña, botón Ingresar | ✓ Generada |
| `02-panel-funcionario-formulario.png` | Panel Funcionario | Formulario "Nueva Justificación" con Motivo General | ✓ Generada |
| `03-agregar-linea-detalle.png` | Panel Funcionario | Tabla de líneas de detalle agregadas | ✓ Generada |
| `04-mi-historial.png` | Panel Funcionario | Tabla "Mi Historial de Justificaciones" | ✓ Generada |
| `05-panel-jefatura-pendientes.png` | Panel Jefatura | Tabla "Solicitudes Pendientes" con el contador | ✓ Generada |
| `06-aprobar-rechazar.png` | Panel Jefatura | Botones Aprobar y Rechazar en una fila | ✓ Generada |
| `07-detalle-boleta.png` | Panel Jefatura | Detalle desplegado de una boleta | ✓ Generada |
| `08-panel-rrhh-filtros.png` | Panel RRHH | Barra de filtros y tabla global | ✓ Generada |
| `09-consulta-historica-sifcnp.png` | Consulta Histórica (SIFCNP) | Filtros de fecha y resultados | ✓ Generada |
| `10-aviso-sesion.png` | Cualquier pantalla | Ventana "Sesión por Expirar" | ✓ Generada |

Cada imagen ya está enlazada en `manual-usuario-final.md` con su texto alternativo descriptivo (accesibilidad WCAG 2.2 §1.1.1) y su leyenda de figura numerada.

## Cómo regenerarlas

1. Levantar el full-stack (tarea de VS Code `start-full-stack`): API en `:5093` y frontend en `:8000`.
2. Asegurar la base de datos `INTEGRA_CNP` con los datos demo cargados (Secciones A+B+C de `03_DatosSemilla.sql`).
3. Con un navegador headless dirigido por CDP (o el MCP de Playwright si está disponible), ingresar con cada usuario demo (`funcionario.ana`, `jefe.maria`, `rrhh.carlos`) y capturar cada pantalla de la tabla.
4. Guardar cada imagen con el nombre indicado en esta carpeta.
