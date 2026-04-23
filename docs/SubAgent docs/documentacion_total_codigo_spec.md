# Especificación: Documentación Total del Código (Frontend + Backend)

## 1. Objetivo
Definir una estrategia integral y accionable para documentar el código existente del proyecto Integrador Marcas, cubriendo arquitectura, módulos, API, flujo de datos, configuración, ejecución local, pruebas y convenciones de desarrollo.

## 2. Alcance
Incluye código y configuración actualmente presentes en el repositorio:
- Frontend estático: `index.html`, `dashboard.html`, `style.css`, `app.js`.
- Backend .NET 8: API, Application, Domain, Infrastructure, Tests.
- Scripts SQL y documentación técnica operativa ya existente en `docs/`.

No incluye (por ahora):
- Nuevas funcionalidades no implementadas.
- Reescritura del sistema de autenticación (actualmente headers mock).
- Integraciones productivas activas con fuentes externas fuera de lo ya modelado/documentado.

## 3. Hallazgos del Codebase (Resumen Técnico)

### 3.1 Arquitectura actual
- Arquitectura por capas en backend:
  - `IntegradorMarcas.Api`: entrypoint, controllers, middleware de errores, DI, CORS, Swagger, health check.
  - `IntegradorMarcas.Application`: servicios, DTOs, validaciones/reglas de negocio.
  - `IntegradorMarcas.Domain`: constantes de dominio (`RolesSistema`, `EstadoIds`).
  - `IntegradorMarcas.Infrastructure`: acceso a datos con Dapper + SQL literal centralizado en `JustificacionesSql`.
- Frontend en HTML/CSS/JS puro, con lógica de estado de sesión y consumo directo de API vía `fetch`.

### 3.2 Endpoints implementados
- `POST /api/justificaciones`
- `GET /api/justificaciones/mias`
- `GET /api/jefatura/justificaciones/pendientes`
- `GET /api/jefatura/justificaciones/{justificacionId}`
- `PATCH /api/jefatura/justificaciones/{justificacionId}/resolver`
- `GET /api/rrhh/justificaciones`
- `GET /health`

### 3.3 Seguridad y autorización
- Identidad HTTP basada en headers (`X-User-Id`, `X-User-Role`) mediante `HeaderUserContext`.
- Autorización de negocio en `JustificacionService` (rol Funcionario/Jefatura/RRHH).
- Manejo de errores central con `ProblemDetails` + `X-Correlation-Id` y persistencia opcional en `dbo.ApiErrorLog`.

### 3.4 Datos y persistencia
- SQL Server con base objetivo `INTEGRA_CNP`.
- Reglas clave en consultas SQL:
  - listado por usuario (funcionario)
  - pendientes por jefatura
  - listado global RRHH con filtros
  - resolución con control de estado pendiente (concurrencia optimista por condición SQL)

### 3.5 Configuración por entorno
- `appsettings.json` + `appsettings.Development.json` + `appsettings.Production.json`.
- Validación de `ConnectionStrings:IntegraCnp` obligatoria fuera de Development.
- Swagger habilitable por configuración (`Swagger:Enabled`).

### 3.6 Pruebas
- Proyecto de tests existe (`backend/tests/IntegradorMarcas.Tests`) con xUnit.
- Cobertura real muy baja: actualmente solo prueba placeholder (`UnitTest1.cs`).

### 3.7 Frontend y flujo funcional
- Login simulado local y sesión en `sessionStorage`.
- Dashboards por rol (Funcionario, Jefatura, RRHH).
- Integración API ya conectada a endpoints reales para crear, listar, resolver y consultar.
- Manejo de errores de red/servidor con toasts y correlación cuando aplica.

## 4. Brechas de Documentación Identificadas
1. Falta un mapa único y actualizado de arquitectura orientado al código actual (hay documentación parcial dispersa).
2. No existe un documento de referencia de módulos frontend (funciones, responsabilidades, eventos UI).
3. No hay un API reference técnico consolidado con contratos request/response por endpoint.
4. El flujo de datos end-to-end (UI -> API -> servicio -> SQL -> respuesta) no está formalizado con trazabilidad por rol.
5. Las convenciones de código y de documentación no están unificadas en una guía práctica.
6. Estrategia de pruebas insuficientemente documentada y sin backlog de cobertura por capas.

## 5. Estrategia de Documentación Total (Accionable)

## Fase 1 - Baseline técnico verificable
Objetivo: producir documentación mínima completa y coherente con código actual.

Acciones:
1. Crear inventario de módulos frontend y backend con responsables funcionales.
2. Consolidar arquitectura lógica + dependencias entre capas.
3. Generar catálogo de endpoints con contratos y reglas de autorización.
4. Documentar configuración por entorno y runbook local.
5. Publicar estado actual de pruebas y plan incremental de cobertura.

Entregables de fase:
- Arquitectura general actualizada.
- API reference base.
- Guía de ejecución local validada.
- Documento de convenciones inicial.

## Fase 2 - Profundización técnica por flujo
Objetivo: documentar el comportamiento por caso de uso (RF-02/RF-03/RF-04/RF-05/RF-06).

Acciones:
1. Para cada flujo, describir precondiciones, entrada, validaciones, errores y salida.
2. Incorporar diagramas de secuencia ligeros (Mermaid) por rol.
3. Trazar SQL utilizado por endpoint y tablas impactadas.
4. Definir tabla de errores estándar (401/403/404/409/500/499) con causa y mitigación.

Entregables de fase:
- Flujos de datos por rol.
- Manual de troubleshooting alineado a correlationId y ApiErrorLog.

## Fase 3 - Gobierno y mantenimiento continuo
Objetivo: evitar desalineación entre código y docs.

Acciones:
1. Definir checklist de actualización documental por PR.
2. Establecer dueños por documento (Backend, Frontend, QA, DevOps).
3. Incluir verificación documental en Definition of Done.
4. Programar revisión mensual de docs críticas.

Entregables de fase:
- Proceso de mantenimiento documental operativo.
- Matriz de trazabilidad documento -> código -> owner.

## 6. Estructura Recomendada de Documentación

## 6.1 Índice maestro
Crear un índice central en `docs/README-manuales.md` con:
- qué documento existe
- a quién va dirigido
- frecuencia de actualización
- owner técnico

## 6.2 Taxonomía sugerida
- Arquitectura
- API
- Frontend
- Backend internals
- Configuración y despliegue
- Pruebas y calidad
- Convenciones
- Operación y soporte

## 7. Lista de Archivos de Documentación a Crear/Actualizar

## Crear
1. `docs/arquitectura-codigo-actual.md`
Contenido esperado:
- Diagrama de capas y responsabilidades reales.
- Dependencias entre proyectos (`.Api` -> `.Application` -> `.Infrastructure`/`.Domain`).
- Decisiones técnicas vigentes (Dapper, SQL inline, headers mock).

2. `docs/api-endpoints-reference.md`
Contenido esperado:
- Tabla endpoint/método/rol.
- Request y response por contrato (campos, tipos, requeridos).
- Códigos HTTP esperados y errores comunes.
- Ejemplos `curl`/HTTP con headers obligatorios.

3. `docs/frontend-modulos-y-flujos.md`
Contenido esperado:
- Módulos JS principales (sesión, API client, panel funcionario, jefatura, rrhh, utilidades).
- Eventos de UI y funciones clave (`registerJustification`, `renderJefaturaRequests`, `approveRequest`, etc.).
- Flujo de navegación por tabs/roles.

4. `docs/flujos-datos-end-to-end.md`
Contenido esperado:
- Secuencias por rol desde UI hasta BD y vuelta.
- Mapa endpoint -> service -> repository -> SQL -> tablas.
- Puntos de validación y errores por etapa.

5. `docs/convenciones-codigo-y-documentacion.md`
Contenido esperado:
- Convenciones de naming (roles, DTOs, endpoints, variables).
- Estándar de errores y mensajes.
- Estilo de documentación técnica (plantillas, formato de ejemplos, versión).

6. `docs/pruebas-estrategia-y-cobertura.md`
Contenido esperado:
- Estado actual de pruebas.
- Pirámide objetivo (unitarias servicio/validador, integración repositorio, API smoke).
- Backlog de casos prioritarios y criterios de aceptación.

## Actualizar
1. `README.md`
Contenido esperado:
- Actualizar resumen real de frontend + backend integrados.
- Enlaces al índice maestro de documentación.
- Quickstart corto (BD + API + frontend).

2. `docs/manual-tecnico.md`
Contenido esperado:
- Reducir duplicidad con nuevos docs especializados.
- Convertir a guía operacional de alto nivel + referencias.

3. `docs/Guia_Implementacion_Dev_Prod.md`
Contenido esperado:
- Mantener runbooks Dev/Prod como fuente principal de despliegue.
- Enlazar API reference y convenciones para evitar contenido duplicado.

4. `docs/PRP.md`
Contenido esperado:
- Alinear afirmaciones de estado del backend (ya no "pendiente" en lo básico).
- Mantenerlo como documento de producto, no como fuente de detalles de implementación.

5. `docs/README-manuales.md`
Contenido esperado:
- Convertir en portal documental principal con rutas, audiencia y owners.

## 8. Esqueleto Mínimo por Documento (Plantilla)
Cada documento técnico nuevo debe incluir:
1. Propósito
2. Alcance
3. Fuente de verdad (archivos/capas asociadas)
4. Contenido principal (tablas/diagramas)
5. Casos de error o límites
6. Checklist de validación
7. Historial de cambios

## 9. Mapa de Cobertura Requerida

## 9.1 Arquitectura y módulos
- Frontend: login, sesión, toasts, paneles por rol, consumo API.
- Backend: controllers, services, validators, repositories, SQL, middleware de errores.

## 9.2 API
- Contratos de request/response en `Contracts`.
- Reglas de autorización y validación por endpoint.
- Trazabilidad de errores y correlationId.

## 9.3 Datos
- Tablas funcionales involucradas por caso de uso.
- Scripts de inicialización/migración en `docs/db/`.

## 9.4 Operación
- Configuración por entorno.
- Ejecución local y validación health.
- Estrategia de logging de errores.

## 9.5 Calidad
- Estado actual de pruebas.
- Plan de crecimiento de cobertura.

## 10. Plan de Ejecución Sugerido (2 semanas)

Semana 1:
1. Arquitectura + API reference + frontend módulos.
2. Revisión técnica cruzada (backend + frontend).
3. Publicación del índice maestro.

Semana 2:
1. Flujos end-to-end + convenciones + pruebas.
2. Refactor de manual técnico para evitar redundancia.
3. Cierre con checklist de cobertura documental.

## 11. Roles y Responsables Recomendados
- Owner Backend Docs: responsable de API, validaciones, repositorios, SQL.
- Owner Frontend Docs: responsable de módulos UI, estado y flujo de interacción.
- Owner DevOps/Operación: responsable de ejecución y configuración por entorno.
- Owner QA: responsable de estrategia de pruebas, trazabilidad y matriz de casos.

## 12. Definición de Hecho (Documentation Done)
Una iteración documental se considera terminada si:
1. Cada endpoint implementado tiene contrato y ejemplo documentado.
2. Cada flujo por rol tiene diagrama o secuencia textual verificable.
3. Configuración Dev/Prod está validada contra `appsettings*` y comandos reales.
4. Existe al menos un documento de convenciones vigente y referenciado.
5. El índice maestro enlaza todos los documentos y owners.
6. Se registró historial de cambios en cada documento modificado.

## 13. Riesgos y Mitigaciones
- Riesgo: divergencia entre código y docs.
  - Mitigación: checklist en PR + owner por documento.
- Riesgo: duplicación de contenido entre manuales.
  - Mitigación: índice maestro con única fuente por tema.
- Riesgo: cobertura de pruebas insuficiente para sostener documentación funcional.
  - Mitigación: backlog de pruebas por prioridad y ejecución incremental.

## 14. Próximos Pasos Inmediatos
1. Crear índice documental maestro en `docs/README-manuales.md`.
2. Crear `docs/api-endpoints-reference.md` y `docs/arquitectura-codigo-actual.md` como base.
3. Actualizar `README.md` para enlazar la nueva estructura.
4. Abrir backlog de pruebas con al menos 10 casos iniciales (validadores + servicio).
