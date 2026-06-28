# CHANGELOG — Manual de Administrador (SIFCNP)

Todas las versiones notables de este manual se registran en este archivo.
Formato: versión, fecha y cambios.

## 1.0.0 — 2026-06-27

### Agregado
- Primera versión del Manual de Administrador de SIFCNP — Sistema de Justificación de Marcas.
- Estructura basada en ISO/IEC/IEEE 26514:2022 (cláusula 8) e IEC/IEEE 82079-1:2019 (cláusulas 5 y 7).
- Tareas del Panel de Administración: gestión de dependencias, usuarios y roles, jerarquías de aprobación, delegaciones y registros (auditoría).
- Secciones de configuración del sistema, respaldos y mantenimiento (a nivel administrativo/operativo).
- Tabla de solución de problemas con los mensajes reales del panel de administración.
- Sección de seguridad y limitaciones conocidas, incluido el encuadre de identidad Microsoft 365 / Entra (@cnp.go.cr).
- Glosario, tabla de trazabilidad con las normas y sección de fuentes.

### Completado — 2026-06-27
- Capturas de pantalla **reales** (10) generadas navegando el Panel de Administración con Microsoft Edge headless vía CDP (Node nativo), no mockups: ingreso del administrador, vista general de los cinco apartados, dependencias y su edición, usuarios y su edición de asignación, jerarquías y su creación, delegaciones y su creación, y registros de monitoreo/auditoría (con filtros). Insertadas con texto alternativo descriptivo (WCAG 2.2 §1.1.1) y leyenda de figura numerada.
- Organización emisora: **Unidad de Tecnologías de Información (UTI) — Consejo Nacional de Producción (CNP)**.
- Contacto de soporte técnico: mesa de ayuda institucional, correo uti@cnp.go.cr y canales internos de Microsoft Teams.
- Navegadores soportados derivados del propio código: Chrome/Edge 90+, Firefox 90+ y Safari 14+ (Internet Explorer y Edge legado no son compatibles).
- Gobernanza del rol Administrador: lo define la coordinación/jefatura de la UTI; no es de autoasignación.
- Acceso a la información de auditoría por instancias sin rol Administrador: se solicita a la UTI por los canales oficiales (no hay acceso directo al sistema).

### Pendiente (TODO)
- Política real de respaldos (frecuencia, herramienta, responsable, restauración).
- Procedimiento institucional de publicación de versiones y ventana de mantenimiento.
- Dirección de producción y estado de activación de Microsoft 365.
