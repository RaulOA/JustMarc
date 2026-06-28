# CHANGELOG — Manual Técnico (SIFCNP / INTEGRA_CNP)

Todas las versiones notables de este manual se registran en este archivo.
Formato: versión, fecha y cambios.

## 1.0.0 — 2026-06-27

### Agregado
- Primera versión del Manual Técnico de SIFCNP — Sistema de Justificación de Marcas.
- Estructura basada en ISO/IEC/IEEE 15289:2019 (contenido del ítem de información), ISO/IEC/IEEE 12207:2017 (procesos del ciclo de vida), ISO/IEC/IEEE 42010:2022 (descripción de arquitectura) y OpenAPI 3.x (referencia de API).
- Visión general del sistema (frontend estático, API .NET 8, base de datos INTEGRA_CNP) y stack/dependencias.
- Descripción de arquitectura (42010): stakeholders, concerns, vistas (lógica, despliegue, datos, comportamiento, seguridad), 7 decisiones de arquitectura (ADR) con justificación, y reglas de correspondencia.
- Modelo de datos completo: 5 esquemas, tablas con PK/FK, función dbo.fn_AprobadoresVigentesPorSolicitante, vistas e integración WIZDOM/SIFCNP.
- Referencia de API en formato OpenAPI 3.0.3 cubriendo las 27 operaciones, con esquemas de request/response, esquemas de seguridad (headers) y respuestas de error.
- Flujos internos (crear/resolver boleta, alcance de aprobación, manejo de errores y auditoría).
- Mapeo del ciclo de vida (12207), instalación, configuración (cadena de conexión, fail-fast), despliegue IIS, pruebas y verificación.
- Mantenimiento y gestión de cambios con el roadmap técnico (incl. migración a Microsoft 365 / Entra).
- Solución de problemas técnica, glosario, tabla de trazabilidad y fuentes.

### Completado — 2026-06-27
- Diagramas **reales** (3) generados a partir del código y del esquema y renderizados a PNG con Mermaid (Microsoft Edge headless vía CDP), con su fuente `.mmd` versionada: despliegue (navegador → frontend → API .NET 8 → SQL Server → WIZDOM/SIFCNP de solo lectura), modelo entidad-relación de INTEGRA_CNP y secuencia de crear/resolver boleta (controlador → servicio → repositorio → BD, con auditoría). Insertados con texto alternativo descriptivo (WCAG 2.2 §1.1.1) y leyenda de figura.
- Organización emisora: **Unidad de Tecnologías de Información (UTI) — Consejo Nacional de Producción (CNP)**.
- Acceso a la información de auditoría: consulta restringida al rol Administrador (`/api/admin/monitoring/registros`); las instancias sin ese rol la solicitan a la UTI por canales oficiales.

### Pendiente (TODO)
- Instancias y credenciales de BD por entorno (dev/prod).
- Dirección de producción (IIS) en el bloque `servers` del OpenAPI.
- Estado de la integración WIZDOM/SIFCNP en el entorno destino.
- Migración del documento OpenAPI de 3.0.3 a 3.1 (opcional).
