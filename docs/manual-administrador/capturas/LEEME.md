# Capturas — Manual de Administrador

Esta carpeta contiene las imágenes que acompañan al manual. Las capturas **ya están generadas** (imágenes **reales** de la aplicación, no mockups): se tomaron navegando el **Panel de Administración** en `http://localhost:8000` (frontend) con la API en `http://localhost:5093` y la base de datos `INTEGRA_CNP` poblada con los datos demo (Secciones A+B+C de `docs/db/03_DatosSemilla.sql`). Se ingresó con un usuario administrador (`admin.sofia`).

> **Herramienta usada:** el MCP de Playwright no estaba disponible en el entorno, así que las capturas se tomaron con **Microsoft Edge en modo headless dirigido por CDP** (Chrome DevTools Protocol) desde un script de **Node** nativo (`fetch` + `WebSocket`, sin dependencias externas). El resultado es equivalente: capturas reales de la app corriendo.

## Capturas generadas

| Archivo | Pantalla / momento | Leyenda | Estado |
|---|---|---|---|
| `01-ingreso-admin.png` | Ingreso | Acceso del administrador al sistema | ✓ Generada |
| `02-panel-admin-dependencias.png` | Panel Admin | Vista general con los cinco apartados | ✓ Generada |
| `03-editar-dependencia.png` | Dependencias | Edición de nombre y dependencia padre | ✓ Generada |
| `04-panel-usuarios.png` | Usuarios | Tabla de usuarios (ID, Nombre, Rol, Unidad, Activo) | ✓ Generada |
| `05-editar-usuario-asignacion.png` | Usuarios | Edición de rol, unidad y jefatura | ✓ Generada |
| `06-jerarquias.png` | Jerarquías | Lista de jerarquías de aprobación | ✓ Generada |
| `07-crear-jerarquia.png` | Jerarquías | Formulario de creación de jerarquía | ✓ Generada |
| `08-delegaciones.png` | Delegaciones | Lista de delegaciones | ✓ Generada |
| `09-crear-delegacion.png` | Delegaciones | Formulario de creación de delegación | ✓ Generada |
| `10-registros-auditoria.png` | Registros | Vista de monitoreo y auditoría con filtros | ✓ Generada |

Cada imagen ya está enlazada en `manual-administrador.md` con su texto alternativo descriptivo (accesibilidad WCAG 2.2 §1.1.1) y su leyenda de figura numerada.

## Cómo regenerarlas

1. Levantar el full-stack (tarea de VS Code `start-full-stack`): API en `:5093` y frontend en `:8000`.
2. Asegurar la base de datos `INTEGRA_CNP` con los datos demo cargados (Secciones A+B+C de `03_DatosSemilla.sql`); para las listas de delegaciones y registros hace falta que existan delegaciones y eventos de auditoría.
3. Con un navegador headless dirigido por CDP (o el MCP de Playwright si está disponible), ingresar con un usuario administrador (`admin.sofia`) y capturar cada apartado del Panel Admin. **Cada apartado requiere aplicar un filtro y hacer clic en Buscar para mostrar datos.**
4. Guardar cada imagen con el nombre indicado en esta carpeta.
