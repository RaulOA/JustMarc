# Capturas — Manual de Administrador

Esta carpeta contiene las imágenes que acompañan al manual. En la versión 1.0.0 las capturas **están pendientes** (TODO): se generarán con el MCP de Playwright navegando la app en `http://localhost:8000` (frontend) con la API en `http://localhost:5093` y la base de datos poblada con los datos demo (Secciones B y C de `docs/db/03_DatosSemilla.sql`). Para estas pantallas se debe ingresar con un usuario administrador (por ejemplo `admin.sofia`).

## Capturas planificadas

| Archivo | Pantalla / momento | Leyenda sugerida |
|---|---|---|
| `01-ingreso-admin.png` | Ingreso | Acceso del administrador al sistema |
| `02-panel-admin-dependencias.png` | Panel Admin | Vista general con los cinco apartados |
| `03-editar-dependencia.png` | Dependencias | Edición de nombre y dependencia padre |
| `04-panel-usuarios.png` | Usuarios | Tabla de usuarios (ID, Nombre, Rol, Unidad, Activo) |
| `05-editar-usuario-asignacion.png` | Usuarios | Edición de rol, unidad y jefatura |
| `06-jerarquias.png` | Jerarquías | Lista de jerarquías de aprobación |
| `07-crear-jerarquia.png` | Jerarquías | Formulario de creación de jerarquía |
| `08-delegaciones.png` | Delegaciones | Lista de delegaciones |
| `09-crear-delegacion.png` | Delegaciones | Formulario de creación de delegación |
| `10-registros-auditoria.png` | Registros | Vista de monitoreo y auditoría con filtros |

## Cómo generarlas (cuando Playwright esté disponible)

1. Levantar el full-stack (tarea de VS Code `start-full-stack`): API en `:5093` y frontend en `:8000`.
2. Asegurar la base de datos `INTEGRA_CNP` con datos demo cargados.
3. Con el MCP de Playwright, ingresar con un usuario administrador (`admin.sofia`) y capturar cada apartado del Panel Admin.
4. Recordar que cada apartado requiere aplicar un filtro y hacer clic en **Buscar** para mostrar datos.
5. Guardar cada imagen con el nombre indicado; las leyendas y puntos de inserción ya están en `manual-administrador.md`.
