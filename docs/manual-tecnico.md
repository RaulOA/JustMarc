# Manual Tecnico del Sistema

## 1. Objetivo
Documentar la operacion tecnica del sistema Integrador Marcas para instalacion local, configuracion, ejecucion, validacion funcional y soporte operativo.

## 2. Alcance
Incluye lo actualmente implementado en el repositorio:
- Frontend estatico en HTML, CSS y JavaScript.
- Backend API en .NET 8 con arquitectura por capas.
- Persistencia en SQL Server (base INTEGRA_CNP).
- Flujos de Funcionario, Jefatura y RRHH.
- Manejo de errores con ProblemDetails y correlacion.

No incluye:
- Integracion activa en tiempo real con WIZDOM o SIFCNP.
- Autenticacion corporativa real (SSO/AD/OAuth).

## 3. Prerequisitos
### 3.1 Software
- .NET SDK 8.0.x.
- SQL Server 2019+ (local o remoto).
- Herramienta SQL (SSMS o equivalente).
- Navegador moderno para frontend.

### 3.2 Acceso
- Permisos para ejecutar scripts en INTEGRA_CNP.
- Permisos para ejecutar la API local.

## 4. Arquitectura
### 4.1 Arquitectura logica

```text
Frontend (index.html, dashboard.html, app.js)
        |
        | HTTP + headers X-User-Id / X-User-Role
        v
IntegradorMarcas.Api (controllers, middleware, health)
        |
        v
IntegradorMarcas.Application (servicios, validaciones, reglas)
        |
        v
IntegradorMarcas.Infrastructure (repositorios Dapper + SQL)
        |
        v
SQL Server / INTEGRA_CNP
```

### 4.2 Proyectos principales
- backend/src/IntegradorMarcas.Api: entrada HTTP, DI, excepciones globales, health.
- backend/src/IntegradorMarcas.Application: servicios de negocio y validadores.
- backend/src/IntegradorMarcas.Domain: constantes de roles y estados.
- backend/src/IntegradorMarcas.Infrastructure: acceso a datos y queries SQL.

### 4.3 Componentes tecnicos criticos
- HeaderUserContext: resuelve identidad desde headers obligatorios.
- JustificacionService: autoriza por rol y aplica reglas RN.
- JustificacionValidator: valida payloads, filtros y acciones.
- ErrorLogRepository: registra errores en dbo.ApiErrorLog.
- Middleware de excepciones: devuelve RFC7807 ProblemDetails y X-Correlation-Id.

## 5. Configuracion
### 5.1 Archivos
- backend/src/IntegradorMarcas.Api/appsettings.json: base comun.
- backend/src/IntegradorMarcas.Api/appsettings.Development.json: entorno local (Swagger habilitado, mock identity).
- backend/src/IntegradorMarcas.Api/appsettings.Production.json: flags seguros (Swagger deshabilitado, mock identity deshabilitado).

### 5.2 Claves relevantes
- ConnectionStrings:IntegraCnp (obligatoria fuera de Development).
- Security:HeaderUserId (default X-User-Id).
- Security:HeaderRole (default X-User-Role).
- Swagger:Enabled.

### 5.3 Variables recomendadas en produccion
- ASPNETCORE_ENVIRONMENT=Production
- ConnectionStrings__IntegraCnp=Server=<host>,1433;Database=INTEGRA_CNP;User Id=<user>;Password=<pwd>;TrustServerCertificate=True;

## 6. Scripts SQL y modelo de datos
### 6.1 Orden minimo de scripts para local
1. docs/db/001_init_integra_cnp.sql
2. docs/db/007_integra_local_bridge.sql
3. docs/db/008_add_comentario_resolucion.sql (seguro/idempotente en ambientes antiguos)

### 6.2 Proposito por script
- 001_init_integra_cnp.sql: crea esquema base, catalogos, tablas transaccionales, seeds e indices.
- 007_integra_local_bridge.sql: crea esquemas stg/bridge/ext y estructuras de staging/canonical views.
- 008_add_comentario_resolucion.sql: agrega ComentarioResolucion si no existe.
- 004_extract_integra_cnp_readonly.sql: extraccion solo lectura para analisis.

### 6.3 Tablas funcionales principales
- dbo.Usuarios
- dbo.Roles
- dbo.Estados
- dbo.Cat_TiposJustificacion
- dbo.Justificaciones_Encabezado
- dbo.Justificaciones_Detalle
- dbo.ApiErrorLog

## 7. Endpoints y contratos
### 7.1 Matriz endpoint-rol
| Endpoint | Metodo | Rol requerido | Resultado esperado |
|---|---|---|---|
| /api/justificaciones | POST | Funcionario | 201 Created |
| /api/justificaciones/mias | GET | Funcionario | 200 OK |
| /api/jefatura/justificaciones/pendientes | GET | Jefatura | 200 OK |
| /api/jefatura/justificaciones/{justificacionId} | GET | Jefatura | 200 OK |
| /api/jefatura/justificaciones/{justificacionId}/resolver | PATCH | Jefatura | 204 No Content |
| /api/rrhh/justificaciones | GET | RRHH | 200 OK |
| /health | GET | Tecnico | 200 OK |

### 7.2 Headers obligatorios (MVP)
- X-User-Id: entero > 0.
- X-User-Role: ROL_FUNC, ROL_JEFE, ROL_RRHH (tambien acepta alias FUNCIONARIO/JEFATURA/RRHH y 1/2/3).

## 8. Validaciones y reglas de negocio
### 8.1 Matriz validacion-regla-codigo HTTP
| Validacion/regla | Mensaje principal | HTTP |
|---|---|---|
| MotivoGeneral requerido y <= 500 | MotivoGeneral es requerido y no puede exceder 500 caracteres. | 400 |
| Al menos una linea de detalle (RN-01) | RN-01: una boleta debe incluir al menos una linea de detalle. | 400 |
| TipoJustificacionID valido | TipoJustificacionID es requerido. / no existe en catalogo. | 400 |
| FechaMarca requerida | FechaMarca es requerida. | 400 |
| ObservacionDetalle <= 250 | ObservacionDetalle no puede exceder 250 caracteres. | 400 |
| Accion resolver APROBAR/RECHAZAR | Accion debe ser APROBAR o RECHAZAR. | 400 |
| Comentario <= 500 | Comentario no puede exceder 500 caracteres. | 400 |
| Rango fechas valido | Desde no puede ser mayor que Hasta. | 400 |
| Compania permitida CNP/FANAL | Compania invalida. | 400 |
| Texto funcionario <= 150 | texto de busqueda no puede exceder 150. | 400 |
| Solo rol autorizado por flujo | Solo Funcionario/Jefatura/RRHH... | 403 |
| Solo subordinado directo en Jefatura | boleta no pertenece a subordinado directo | 403 |
| Boleta inexistente | No existe la boleta indicada. | 404 |
| Boleta ya resuelta (RN-04) | RN-04: la boleta ya fue resuelta... | 409 |
| Resolucion concurrente | ya cambio de estado | 409 |

## 9. Seguridad
### 9.1 Identidad y autorizacion actual
- El sistema usa identidad por headers HTTP.
- HeaderUserContext valida presencia y formato.
- La autorizacion por rol se aplica en JustificacionService.

### 9.2 Consideraciones
- Security:UseMockIdentity no desactiva el requerimiento de headers en la implementacion actual.
- En produccion no exponer Swagger (Swagger:Enabled=false).
- No almacenar credenciales en repositorio; usar variables de entorno.

## 10. Manejo de errores y trazabilidad
### 10.1 Respuesta estandar
- Formato ProblemDetails (RFC7807).
- Codigos observados: 400, 401, 403, 404, 409, 499, 500.

### 10.2 Correlacion
- Se agrega header X-Correlation-Id en respuesta de error.
- El correlationId tambien viaja en extensions.correlationId del ProblemDetails.

### 10.3 Persistencia
- Errores se registran en dbo.ApiErrorLog con:
  - Metodo, endpoint, status, tipo, mensaje.
  - Usuario, rol, IP, user-agent, entorno.
  - StackTrace para errores 500.

## 11. Despliegue local paso a paso
1. Ejecutar scripts SQL en el orden indicado (001, 007 y opcional 008).
2. Configurar ConnectionStrings:IntegraCnp en appsettings.Development.json.
3. Restaurar dependencias:
   - dotnet restore backend/IntegradorMarcas.slnx
4. Compilar:
   - dotnet build backend/IntegradorMarcas.slnx
5. Ejecutar API:
   - dotnet run --project backend/src/IntegradorMarcas.Api
6. Verificar salud:
   - GET http://localhost:5093/health
7. Abrir frontend:
   - index.html y luego dashboard.html

## 12. Validacion operativa
### 12.1 Smoke test tecnico minimo
- Health responde 200.
- POST /api/justificaciones con rol Funcionario crea boleta.
- GET /api/jefatura/justificaciones/pendientes muestra boleta.
- PATCH resolver devuelve 204.
- GET /api/rrhh/justificaciones refleja estado final.

### 12.2 Verificacion de logs
- Provocar un error controlado (por ejemplo header invalido) y confirmar registro en dbo.ApiErrorLog.

## 13. Operacion diaria
- Monitorear /health y errores HTTP 5xx.
- Revisar ApiErrorLog por volumen, status y correlacion.
- Verificar tiempos de respuesta y conectividad SQL.
- Mantener scripts de extraccion en modo solo lectura cuando aplique.

## 14. Troubleshooting
### 14.1 Error 401 Header requerido invalido
Causa probable:
- Falta X-User-Id o X-User-Role, o valor no valido.
Accion:
- Enviar ambos headers, con UserId entero > 0 y rol valido.

### 14.2 Error 403 por rol
Causa probable:
- Endpoint consumido por rol no autorizado.
Accion:
- Usar endpoint correcto segun matriz endpoint-rol.

### 14.3 Error 409 al resolver
Causa probable:
- Boleta ya resuelta por otra operacion (concurrencia).
Accion:
- Refrescar bandeja y no reintentar sobre la misma boleta.

### 14.4 Error de conexion SQL
Causa probable:
- ConnectionStrings:IntegraCnp incorrecta o SQL no disponible.
Accion:
- Validar host, puerto, credenciales y acceso de red.

### 14.5 CORS o API no accesible desde frontend
Causa probable:
- API no iniciada o URL base incorrecta.
Accion:
- Verificar que la API este arriba y revisar base URL configurada en sesion.

## 15. Buenas practicas
- Mantener idempotencia en scripts y respetar orden de ejecucion.
- Registrar y compartir correlationId en incidencias.
- Evitar cambios de esquema manuales sin script versionado.
- Aplicar principio de minimo privilegio para cuentas SQL.
- Probar con los tres roles antes de pasar cambios a produccion.
