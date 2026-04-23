# Arquitectura del Codigo Actual

## 1. Proposito
Documentar la arquitectura implementada actualmente en el repositorio Integrador Marcas, con foco en responsabilidades por capa, dependencias reales y decisiones tecnicas vigentes.

## 2. Alcance
Incluye:
- Frontend estatico: index.html, dashboard.html, style.css, app.js.
- Backend .NET 8: Api, Application, Domain, Infrastructure.
- Integracion con SQL Server mediante Dapper.

No incluye:
- Funcionalidades futuras no implementadas (administracion de jerarquias, delegaciones, auditoria funcional avanzada).
- Mecanismos de autenticacion corporativa (SSO/AD/OAuth).

## 3. Fuente de verdad
- backend/src/IntegradorMarcas.Api/Program.cs
- backend/src/IntegradorMarcas.Api/Controllers/*.cs
- backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs
- backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs
- backend/src/IntegradorMarcas.Infrastructure/Repositories/*.cs
- backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs
- app.js, index.html, dashboard.html

## 4. Arquitectura logica

```text
Frontend (HTML/CSS/JS)
  index.html + dashboard.html + app.js
          |
          | fetch + headers X-User-Id / X-User-Role
          v
IntegradorMarcas.Api (.NET 8)
  Controllers + DI + CORS + Swagger + health + exception handling
          |
          v
IntegradorMarcas.Application
  JustificacionService + JustificacionValidator + DTOs + interfaces
          |
          v
IntegradorMarcas.Infrastructure
  Dapper repositories + SQL centralizado en JustificacionesSql
          |
          v
SQL Server (INTEGRA_CNP)
  dbo.Justificaciones_Encabezado, dbo.Justificaciones_Detalle,
  dbo.Cat_TiposJustificacion, dbo.Usuarios, dbo.ApiErrorLog, etc.
```

## 5. Responsabilidades por modulo

### 5.1 Frontend
- app.js:
  - Gestion de sesion en sessionStorage.
  - Resolucion de identidad mock por usuario.
  - Cliente API con timeout y parseo de errores.
  - Vistas por rol (Funcionario, Jefatura, RRHH).
  - Toasts y mensajes de error, incluyendo correlationId cuando existe.
- dashboard.html:
  - Estructura de paneles y eventos de UI consumidos por app.js.
- index.html:
  - Login simulado local para ambiente MVP.

### 5.2 API
- Program.cs:
  - Registro de dependencias.
  - CORS abierto para frontend local.
  - Health check en /health.
  - Excepcion global con ProblemDetails.
- Controllers:
  - JustificacionesController: creacion y consulta del funcionario.
  - JefaturaController: pendientes, detalle y resolucion.
  - RrhhController: consulta global con filtros.
- Security/HeaderUserContext:
  - Lee identidad desde headers configurables.

### 5.3 Application
- JustificacionService:
  - Reglas de autorizacion por rol.
  - Orquestacion de validaciones de entrada.
  - Reglas de negocio RN-01 y RN-04.
- JustificacionValidator:
  - Validaciones de campos, accion, rango de fechas, compania y texto de busqueda.

### 5.4 Domain
- RolesSistema:
  - Canonicaliza roles y acepta alias (FUNCIONARIO/JEFATURA/RRHH y 1/2/3).
- EstadoIds:
  - PendienteJefatura=1, Aprobado=2, Rechazado=3.

### 5.5 Infrastructure
- JustificacionRepository:
  - Acceso transaccional para crear boletas.
  - Consultas de bandejas y detalle.
  - Resolucion con condicion SQL para concurrencia optimista.
- ErrorLogRepository:
  - Persistencia de errores en dbo.ApiErrorLog.
- JustificacionesSql:
  - SQL literal centralizado por operacion.

## 6. Dependencias entre proyectos

```text
IntegradorMarcas.Api
  -> IntegradorMarcas.Application
  -> IntegradorMarcas.Infrastructure

IntegradorMarcas.Application
  -> IntegradorMarcas.Domain

IntegradorMarcas.Infrastructure
  -> IntegradorMarcas.Application
  -> IntegradorMarcas.Domain
  -> Dapper
  -> Microsoft.Data.SqlClient

IntegradorMarcas.Tests
  -> IntegradorMarcas.Application
  -> IntegradorMarcas.Domain
```

## 7. Decisiones tecnicas vigentes
- Dapper + SQL literal (sin ORM completo ni migrations automáticas).
- Identidad por headers HTTP para MVP.
- Autorizacion por rol en capa Application.
- Manejo de errores con ProblemDetails y correlationId.
- Frontend sin framework (vanilla JS), consumo directo de API via fetch.

## 8. Limites y deuda tecnica observada
- En Program.cs hay dos registros consecutivos de UseExceptionHandler; el segundo concentra correlationId y logging, mientras el primero agrega complejidad innecesaria.
- Security:UseMockIdentity existe en configuracion, pero HeaderUserContext sigue exigiendo headers obligatorios.
- CORS local permite cualquier origen/metodo/header.
- Cobertura de pruebas automatizadas casi nula.

## 9. Checklist de validacion
- Arquitectura y dependencias corresponden a proyectos existentes.
- Endpoints descritos coinciden con controladores.
- Reglas de rol y estado alineadas con Domain/Application.
- Persistencia y SQL alineadas con Infrastructure.

## 10. Historial de cambios
- 2026-04-23: Documento creado y validado contra el codigo actual del repositorio.
