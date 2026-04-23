# Especificación de Manuales de Documentación (ES)

Fecha: 23-04-2026  
Proyecto: Integrador Marcas / Justificación de Marcas

## 1. Resumen Ejecutivo

- El sistema implementa tres roles de negocio reales en código: Funcionario, Jefatura y RRHH.
- La solución sigue una arquitectura por capas en backend (.NET 8): Api, Application, Domain e Infrastructure.
- La autenticación actual para MVP se basa en headers HTTP (`X-User-Id`, `X-User-Role`) y autorización por rol en servicio de aplicación.
- Los flujos funcionales principales cubren creación de boletas, consulta propia, bandeja y resolución de jefatura, y consulta global RRHH.
- Los endpoints clave están expuestos en tres controladores: `JustificacionesController`, `JefaturaController` y `RrhhController`.
- Las validaciones de negocio incluyen reglas de longitud, obligatoriedad, rangos de fecha, acciones permitidas y pertenencia a jefatura.
- El manejo de errores usa `UseExceptionHandler`, `AppException` y respuesta RFC7807 (`ProblemDetails`) con `X-Correlation-Id` y persistencia en `ApiErrorLog`.
- La operación de BD se centra en `INTEGRA_CNP`, con scripts idempotentes de inicialización, bridge local y extracción read-only para fuentes externas.
- La operación local está documentada con runbook Dev/Prod, orden de scripts, configuración por `appsettings` y comandos `dotnet`.
- Se definen 4 manuales destino: 1 técnico y 3 de usuario (uno por cada rol implementado).

## 2. Objetivo de esta Especificación

Definir la estructura y el contenido mínimo de la documentación formal del sistema, en español, para:

1. Manual técnico del sistema.
2. Manuales de usuario por rol real implementado.

## 3. Fuentes Analizadas (Repositorio)

### Backend y API

- `backend/src/IntegradorMarcas.Api/Program.cs`
- `backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs`
- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`
- `backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs`
- `backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs`
- `backend/src/IntegradorMarcas.Api/appsettings.Development.json`
- `backend/src/IntegradorMarcas.Api/appsettings.Production.json`

### Frontend y UX

- `index.html`
- `dashboard.html`
- `app.js`

### Documentación y SQL

- `README.md`
- `docs/Guia_Implementacion_Dev_Prod.md`
- `docs/db/001_init_integra_cnp.sql`
- `docs/db/002_extract_wizdom_readonly.sql`
- `docs/db/003_extract_sifcnp_readonly.sql`
- `docs/db/004_extract_integra_cnp_readonly.sql`
- `docs/db/005_extract_wizdom_targeted_min.sql`
- `docs/db/006_extract_sifcnp_targeted_min.sql`
- `docs/db/007_integra_local_bridge.sql`
- `docs/db/008_add_comentario_resolucion.sql`

## 4. Roles Reales Detectados

Roles implementados en código (`RolesSistema.cs` y validaciones de `JustificacionService.cs`):

1. **Funcionario**
   - Identificadores: `ROL_FUNC`, `FUNCIONARIO`, `1`.
   - Capacidades: crear boletas, consultar boletas propias.

2. **Jefatura**
   - Identificadores: `ROL_JEFE`, `JEFATURA`, `2`.
   - Capacidades: listar pendientes de subordinados, ver detalle, resolver (aprobar/rechazar).

3. **RRHH**
   - Identificadores: `ROL_RRHH`, `RRHH`, `3`.
   - Capacidades: consulta global de boletas con filtros transversales.

No se detectan otros roles de negocio con permisos efectivos en endpoints actuales.

## 5. Arquitectura y Componentes a Documentar

## 5.1 Arquitectura lógica

- **Frontend estático:** `index.html`, `dashboard.html`, `style.css`, `app.js`.
- **Backend API .NET 8:** capa de entrada HTTP y configuración.
- **Application:** reglas de negocio, DTOs, validación y autorización por rol.
- **Domain:** constantes y entidades del dominio.
- **Infrastructure:** acceso a SQL Server (repositorios, queries, logging de error).
- **Base de datos principal:** `INTEGRA_CNP`.
- **Fuentes externas (read-only para extracción):** `WIZDOM`, `SIFCNP`.

## 5.2 Componentes técnicos críticos

- Resolución de identidad por headers (`HeaderUserContext`).
- Middleware global de excepciones con `ProblemDetails`.
- Persistencia de errores en `dbo.ApiErrorLog`.
- Repositorio de justificaciones con reglas de concurrencia/estado.
- Catálogos de tipos/estados y relación jerárquica usuario-jefatura.
- Bridge local de staging/canonical views (`stg`, `bridge`, `ext`).

## 6. Flujos Funcionales a Incluir en Manuales

## 6.1 Funcionario

1. Ingreso al sistema (mock identity en ambiente local).
2. Registro de justificación con líneas de detalle.
3. Consulta de historial propio con filtros.

## 6.2 Jefatura

1. Consulta de boletas pendientes de subordinados.
2. Consulta de detalle completo por boleta.
3. Resolución de boleta (aprobar/rechazar + comentario opcional).

## 6.3 RRHH

1. Consulta global de boletas.
2. Filtrado por funcionario, estado, compañía y rango de fechas.

## 7. Endpoints y Contratos (mínimo documental)

Sección obligatoria en manual técnico y resumida por rol en manuales de usuario.

1. `POST /api/justificaciones`
   - Rol: Funcionario.
   - Propósito: crear boleta.

2. `GET /api/justificaciones/mias`
   - Rol: Funcionario.
   - Propósito: consultar boletas propias.

3. `GET /api/jefatura/justificaciones/pendientes`
   - Rol: Jefatura.
   - Propósito: listar pendientes.

4. `GET /api/jefatura/justificaciones/{justificacionId}`
   - Rol: Jefatura.
   - Propósito: ver detalle completo.

5. `PATCH /api/jefatura/justificaciones/{justificacionId}/resolver`
   - Rol: Jefatura.
   - Propósito: aprobar/rechazar boleta.

6. `GET /api/rrhh/justificaciones`
   - Rol: RRHH.
   - Propósito: consulta global con filtros.

7. `GET /health`
   - Rol: técnico/operación.
   - Propósito: verificación de salud del servicio.

Headers obligatorios en MVP:

- `X-User-Id`
- `X-User-Role`

## 8. Validaciones Clave y Reglas de Negocio

## 8.1 Validaciones de entrada

- `MotivoGeneral` obligatorio, máximo 500.
- Al menos una línea de detalle (RN-01).
- `TipoJustificacionID` mayor a 0.
- `FechaMarca` obligatoria.
- `ObservacionDetalle` máximo 250.
- Acción de resolución solo `APROBAR` o `RECHAZAR`.
- Comentario de resolución máximo 500.
- Rango de fechas válido (`Desde <= Hasta`).
- Compañía permitida: `CNP` o `FANAL`.
- Texto de búsqueda de funcionario máximo 150.

## 8.2 Validaciones de autorización y estado

- Creación y consulta propia: solo Funcionario.
- Pendientes/detalle/resolución: solo Jefatura.
- Consulta global: solo RRHH.
- Jefatura solo sobre subordinados directos.
- Resolución solo si estado es Pendiente Jefatura (RN-04).
- Si hubo resolución concurrente: conflicto (409).

## 9. Manejo de Errores y Observabilidad

Elementos mínimos a documentar:

1. Tipos de error (`AppException`, `KeyNotFoundException`, no controlados).
2. Mapeo de códigos HTTP (400, 401, 403, 404, 409, 499, 500).
3. Estructura `ProblemDetails` devuelta por API.
4. Header de correlación `X-Correlation-Id`.
5. Registro de errores en `dbo.ApiErrorLog`.
6. Datos operativos guardados: endpoint, método, status, tipo, mensaje, usuario, rol, entorno, IP, user-agent.

## 10. Scripts SQL y Operación Local

## 10.1 Clasificación de scripts

1. Inicialización de esquema y semilla:
   - `docs/db/001_init_integra_cnp.sql`
2. Bridge/local staging para integración externa:
   - `docs/db/007_integra_local_bridge.sql`
3. Migración incremental:
   - `docs/db/008_add_comentario_resolucion.sql`
4. Extracción read-only y exploración:
   - `docs/db/002_extract_wizdom_readonly.sql`
   - `docs/db/003_extract_sifcnp_readonly.sql`
   - `docs/db/004_extract_integra_cnp_readonly.sql`
5. Extracción dirigida mínima (targeted):
   - `docs/db/005_extract_wizdom_targeted_min.sql`
   - `docs/db/006_extract_sifcnp_targeted_min.sql`

## 10.2 Operación local mínima

1. Ejecutar scripts BD en orden (al menos `001` y `007`).
2. Configurar `ConnectionStrings:IntegraCnp` en `appsettings.Development.json`.
3. Restaurar/compilar: `dotnet restore` y `dotnet build` sobre `backend/IntegradorMarcas.slnx`.
4. Ejecutar API: `dotnet run --project backend/src/IntegradorMarcas.Api`.
5. Verificar `GET /health`.
6. Consumir API con headers `X-User-Id` y `X-User-Role`.

## 11. Estructura Detallada de Manuales (Índice + Contenido Mínimo)

## 11.1 Manual Técnico del Sistema

Nombre propuesto: **Manual Técnico - Integrador Marcas**

Índice propuesto:

1. Introducción
2. Alcance funcional y no funcional
3. Arquitectura de solución
4. Arquitectura de despliegue (Dev/Prod)
5. Componentes del backend
6. Componentes del frontend
7. Modelo de datos y diccionario resumido
8. Seguridad, identidad y autorización por roles
9. Endpoints y contratos API
10. Reglas de negocio y validaciones
11. Manejo de errores, trazabilidad y correlación
12. Scripts SQL y estrategia de datos (core + bridge + extraction)
13. Operación local paso a paso
14. Configuración por entorno (appsettings + variables)
15. Monitoreo, troubleshooting y rollback básico
16. Anexos (ejemplos de request/response, códigos de error)

Contenido mínimo obligatorio:

- Diagrama de capas (texto/ASCII o imagen).
- Matriz endpoint-rol.
- Matriz validación-regla-código HTTP.
- Orden exacto de scripts y propósito.
- Runbook local validado con comandos.
- Sección de soporte con uso de `correlationId`.

## 11.2 Manual de Usuario - Funcionario

Nombre propuesto: **Manual de Usuario - Rol Funcionario**

Índice propuesto:

1. Objetivo del rol
2. Acceso al sistema
3. Navegación del panel Funcionario
4. Crear una justificación
5. Gestionar líneas de detalle
6. Consultar historial propio
7. Estados de boletas y significado
8. Mensajes de validación frecuentes
9. Errores comunes y qué hacer
10. Buenas prácticas de registro

Contenido mínimo obligatorio:

- Paso a paso con capturas/pantallas referenciales.
- Campos obligatorios y límites.
- Ejemplos de errores de validación y corrección.

## 11.3 Manual de Usuario - Jefatura

Nombre propuesto: **Manual de Usuario - Rol Jefatura**

Índice propuesto:

1. Objetivo del rol
2. Acceso y vista general de pendientes
3. Filtros por fecha
4. Revisión de detalle de boleta
5. Aprobar una boleta
6. Rechazar una boleta
7. Uso de comentario de resolución
8. Restricciones de subordinación
9. Mensajes frecuentes (403/404/409)
10. Recomendaciones operativas de aprobación

Contenido mínimo obligatorio:

- Flujo completo desde bandeja hasta resolución.
- Explicación de estados y bloqueo por boleta ya resuelta.
- Trazabilidad mínima de decisiones.

## 11.4 Manual de Usuario - RRHH

Nombre propuesto: **Manual de Usuario - Rol RRHH**

Índice propuesto:

1. Objetivo del rol
2. Acceso al panel RRHH
3. Consulta global de justificaciones
4. Uso de filtros (funcionario, estado, compañía, fechas)
5. Interpretación de resultados
6. Casos de consulta frecuentes
7. Mensajes de validación frecuentes
8. Buenas prácticas para auditoría operativa
9. Resolución de incidencias comunes

Contenido mínimo obligatorio:

- Definición operativa de cada filtro.
- Ejemplos de búsquedas típicas.
- Diferencias entre vista RRHH y vista Jefatura.

## 12. Rutas Finales de Archivos de Manuales en docs/

Se establece la siguiente estructura objetivo:

1. `docs/manual-tecnico-sistema.md`
2. `docs/manual-usuario-funcionario.md`
3. `docs/manual-usuario-jefatura.md`
4. `docs/manual-usuario-rrhh.md`

## 13. Criterios de Aceptación de la Documentación

1. Todos los roles manualizados corresponden a roles reales implementados en código.
2. El manual técnico cubre arquitectura, endpoints, validaciones, errores, SQL y operación local.
3. Cada manual de usuario es específico por rol y sin mezclar permisos de otros roles.
4. Las rutas finales de archivos quedan bajo `docs/` y con nomenclatura homogénea.
5. El contenido está en español, con lenguaje operativo y mantenible por el equipo.

## 14. Entregable de esta tarea

Documento generado en:

- `docs/SubAgent docs/manuales-especificacion.md`
