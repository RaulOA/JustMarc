# Spec: Admin de Jerarquia y Asignaciones con Auditoria de Acciones

**Fecha:** 2026-05-03  
**Version:** 1.0  
**Objetivo:** disenar implementacion para pantalla/flujo administrativo que permita ver y editar jerarquia organizacional y asignaciones (dependencias, jefaturas, funcionarios, mapeos de aprobacion y delegaciones) con auditoria obligatoria por accion.

---

## 1. Estado Actual del Codigo

### 1.1 Roles en backend/frontend y existencia de ROL_ADMIN

Hallazgos:
- ROL_ADMIN existe en dominio: `backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs`.
- Guardas de admin existen en servicio: `backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs` via `RolesSistema.EsAdmin(...)`.
- Los endpoints `api/admin/aprobaciones/*` existen y ya requieren ROL_ADMIN por validacion de negocio.
- El contexto de usuario se toma solo de headers `X-User-Id` y `X-User-Role`: `backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs`.
- No hay auth middleware con claims/policies (no `[Authorize]`), todo se controla en servicios.
- Frontend actual no contempla ROL_ADMIN:
  - `app.js` solo muestra roles `ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH` en `configureRoleUI()`.
  - `dashboard.html` no tiene pestana/panel admin.
  - `MOCK_USER_DIRECTORY` no incluye usuario admin.

Conclusion:
- Backend ya tiene base para admin en modulo de aprobaciones.
- Frontend no expone capacidades admin.
- Seguridad por rol depende de header y validacion de servicio.

### 1.2 Endpoints/servicios/repositorios/tablas actuales para jerarquia, usuarios, reglas y delegaciones

Backend existente:
- Controller admin: `backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs`
  - GET `api/admin/aprobaciones/jerarquias`
  - POST `api/admin/aprobaciones/jerarquias`
  - PATCH `api/admin/aprobaciones/jerarquias/{id}/estado`
  - GET `api/admin/aprobaciones/delegaciones`
  - POST `api/admin/aprobaciones/delegaciones`
  - PATCH `api/admin/aprobaciones/delegaciones/{id}/estado`
- Service admin: `backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs`
- Repository admin: `backend/src/IntegradorMarcas.Infrastructure/Repositories/AdminAprobacionesRepository.cs`
- SQL admin: `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs`

Tablas funcionales actuales:
- `RecursosHumanos.Usuario`
- `RecursosHumanos.EstructuraOrganizacional`
- `Operacion.JerarquiaAprobacion`
- `Operacion.DelegacionAprobacion`
- Funcion de alcance: `dbo.fn_AprobadoresVigentesPorSolicitante` (consumida por queries de negocio)

Cobertura funcional actual:
- Hay lectura/alta/cambio de estado de jerarquias y delegaciones.
- No hay update completo ni delete logico explicito mas alla de estado.
- No hay endpoints admin para editar asignaciones de usuarios (jefatura/unidad/rol) ni para explorar estructura+usuarios con metadatos ricos para UI de administracion.

### 1.3 Infra de auditoria actual y mejor punto de extension

Infra actual:
- Auditoria de negocio: `Auditoria.EventoAuditoria`
  - Repositorio: `AuditEventRepository` + `AuditoriaSql.InsertEvento`
  - Campos: tipo evento, resultado, referencia, payload resumen (varchar 1000)
- Auditoria tecnica de errores: `Auditoria.ErrorApi`
  - Repositorio: `ErrorLogRepository`
  - Usada en `Program.cs` para excepciones globales.

Uso actual en admin:
- `AdminAprobacionesService` registra eventos tipo 4/5/6/7 para altas y cambios de estado.
- Registra actor y referencia, pero no persiste old/new estructurado.

Mejor punto de extension:
- Mantener `Auditoria.EventoAuditoria` para trazabilidad resumida.
- Agregar tabla dedicada de detalle de acciones admin (old/new JSON + metadata) para cumplir requerimiento critico de auditoria completa.
- Implementar logging de accion admin en capa Application (service), con repositorio dedicado y transaccion junto al cambio de datos.

---

## 2. Riesgos y Brechas Detectadas (Previos al Cambio)

1. Desalineamiento de nombres de columnas de auditoria de creacion/modificacion:
- DDL base usa `CreadoPor/FechaHoraCreacion`.
- `AdminAprobacionesSql` inserta `Usr_Registro`.
- Riesgo: inserts admin pueden fallar segun esquema instalado.

2. Desalineamiento de objetos en queries legacy:
- `JustificacionesSql` usa `dbo.Estructuras_Organizacionales` en un query de detalle.
- El esquema base consolidado usa `RecursosHumanos.EstructuraOrganizacional`.

3. Auditoria admin incompleta para trazabilidad regulatoria:
- No hay persistencia estructurada de valor anterior y nuevo.
- `PayloadResumen` es texto y tamaño limitado.

4. Frontend sin acceso admin:
- No existe pantalla/tab para administrar jerarquia/delegaciones/asignaciones.

5. Falta de pruebas automatizadas del modulo admin:
- No hay tests para `AdminAprobacionesService`/controller y reglas de seguridad admin.

Estas brechas deben abordarse antes o durante la fase 1 del plan.

---

## 3. Arquitectura Minima Segura Propuesta

### 3.1 Frontend: nuevo panel admin (view + edit)

Objetivo UI:
- Nueva pestana `panel-admin` visible solo para `ROL_ADMIN`.
- Vista compuesta por 4 submodulos:
  1) Dependencias (estructura organizacional).
  2) Asignaciones de personal (jefaturas/funcionarios, unidad, jefatura directa).
  3) Reglas de aprobacion (jerarquias).
  4) Delegaciones.

Alcance minimo funcional:
- Listar con filtros y paginacion local/simple server-side.
- Crear y editar registros de jerarquia y delegacion.
- Editar asignaciones clave de usuario: `JefaturaId`, `UnidadId`, `RolId`, `EsActivo`.
- Confirmacion previa para acciones criticas y feedback con correlation id si aplica.

Archivos frontend a tocar:
- `dashboard.html` (nuevo tab + panel admin).
- `app.js` (guards de rol admin, llamadas API admin, render/acciones).
- `style.css` (estilos panel admin reutilizando sistema actual).

### 3.2 Backend: endpoints admin CRUD/updates con guarda de rol

Reusar y extender modulo existente `AdminAprobaciones*`:

Aprobaciones/delegaciones:
- Mantener endpoints actuales.
- Agregar updates completos:
  - PUT/PATCH `jerarquias/{id}` (nivel, relacion, vigencia, aprobador, estructura, estado).
  - PUT/PATCH `delegaciones/{id}` (delegante, delegado, jerarquia opcional, motivo, vigencia, estado).

Asignaciones organizacionales:
- Nuevo controller (sugerido): `AdminOrganizacionController` bajo `api/admin/organizacion`.
- Endpoints minimos:
  - GET `api/admin/organizacion/estructuras`
  - GET `api/admin/organizacion/usuarios`
  - PATCH `api/admin/organizacion/usuarios/{usuarioId}/asignacion`
  - PATCH `api/admin/organizacion/usuarios/{usuarioId}/estado`

Guardas:
- Todas las operaciones en service deben llamar `EnsureAdmin(user)`.
- Mantener bloqueo a no-admin con HTTP 403.

### 3.3 Auditoria obligatoria por cada accion admin

Regla:
- Cada accion admin de mutacion debe registrar:
  - Quien: usuario actor (id, rol, nombre/resuelto).
  - Cuando: fecha UTC.
  - Que accion: create/update/state-change.
  - Sobre que objetivo: entidad + id + claves naturales si aplica.
  - Valores anteriores y nuevos (JSON estructurado).
  - Resultado (exito/fallo/denegado).
  - Correlation/trace id de request.

Patron tecnico propuesto:
- Nuevo repositorio `IAdminActionAuditRepository`.
- Llamado desde Application service dentro de la misma transaccion de negocio.
- Mantener ademas registro resumido en `Auditoria.EventoAuditoria` para continuidad operativa con lo ya existente.

---

## 4. Cambios de Base de Datos Propuestos

## 4.1 Tabla nueva de auditoria detallada (recomendada)

Nueva tabla: `Auditoria.AdminAccionAuditoria`

Columnas sugeridas:
- `AdminAccionAuditoriaId BIGINT IDENTITY PK`
- `FechaEventoUtc DATETIME2(3) NOT NULL DEFAULT SYSUTCDATETIME()`
- `CorrelationId UNIQUEIDENTIFIER NULL`
- `UsuarioActorId INT NOT NULL`
- `RolActorCodigo VARCHAR(20) NOT NULL`
- `EntidadObjetivo VARCHAR(80) NOT NULL` (JerarquiaAprobacion, DelegacionAprobacion, Usuario, EstructuraOrganizacional)
- `EntidadObjetivoId VARCHAR(80) NOT NULL` (permite ids compuestos)
- `Accion VARCHAR(40) NOT NULL` (Create, Update, ChangeState, Reassign, Activate, Deactivate)
- `ResultadoAuditoriaId INT NOT NULL` FK `Configuracion.ResultadoAuditoria`
- `Descripcion VARCHAR(500) NOT NULL`
- `ValoresAnteriores NVARCHAR(MAX) NULL` (JSON)
- `ValoresNuevos NVARCHAR(MAX) NULL` (JSON)
- `Metadata NVARCHAR(MAX) NULL` (JSON: ip, user-agent, endpoint)

Indices minimos:
- IX por `FechaEventoUtc DESC`
- IX por `EntidadObjetivo, EntidadObjetivoId`
- IX por `UsuarioActorId, FechaEventoUtc DESC`

## 4.2 Catalogo de tipos de evento (opcional pero recomendado)

Extender `Configuracion.TipoEventoAuditoria` con tipos admin de update:
- `8 = ActualizacionJerarquia`
- `9 = ActualizacionDelegacion`
- `10 = ActualizacionAsignacionUsuario`
- `11 = CambioEstadoUsuario`

## 4.3 Ajustes de consistencia previos

- Alinear columnas de auditoria de creacion/modificacion en SQL de infraestructura:
  - usar `CreadoPor` en lugar de `Usr_Registro`.
- Revisar query de detalle que referencia `dbo.Estructuras_Organizacionales` y mover a `RecursosHumanos.EstructuraOrganizacional`.

---

## 5. Contratos API Propuestos

### 5.1 Lectura organizacional

GET `api/admin/organizacion/estructuras?estadoRegistroId=&search=`
- Response item:
  - `estructuraOrganizacionalId`
  - `nombre`
  - `codigoOrigen`
  - `estructuraPadreId`
  - `estadoRegistroId`
  - `vigenciaDesde`
  - `vigenciaHasta`
  - `cantidadUsuarios`
  - `cantidadJefaturas`

GET `api/admin/organizacion/usuarios?rolId=&unidadId=&jefaturaId=&esActivo=&search=`
- Response item:
  - `usuarioId`
  - `cedula`
  - `nombreCompleto`
  - `correoElectronico`
  - `rolId`
  - `rolDescripcion`
  - `unidadId`
  - `unidadNombre`
  - `jefaturaId`
  - `jefaturaNombre`
  - `esActivo`

### 5.2 Mutaciones de asignacion

PATCH `api/admin/organizacion/usuarios/{usuarioId}/asignacion`
- Request:
  - `rolId` (opcional)
  - `unidadId` (opcional)
  - `jefaturaId` (opcional, null permitido)
- Response: objeto usuario actualizado.

PATCH `api/admin/organizacion/usuarios/{usuarioId}/estado`
- Request:
  - `esActivo` (bool)
- Response: 204 o usuario actualizado.

### 5.3 Mutaciones de jerarquia/delegacion

PATCH `api/admin/aprobaciones/jerarquias/{id}`
- Request:
  - `aprobadorUsuarioId`
  - `estructuraOrganizacionalId`
  - `nivelAprobacion`
  - `tipoRelacion`
  - `vigenciaDesde`
  - `vigenciaHasta`
  - `estadoRegistroId`

PATCH `api/admin/aprobaciones/delegaciones/{id}`
- Request:
  - `deleganteUsuarioId`
  - `delegadoUsuarioId`
  - `jerarquiaAprobacionId`
  - `motivo`
  - `vigenciaDesde`
  - `vigenciaHasta`
  - `estadoRegistroId`

### 5.4 Contrato de auditoria admin

No exponer escritura de auditoria por API publica.
Se registra internamente en cada endpoint de mutacion.

Opcional para consulta admin futura:
GET `api/admin/auditoria/acciones?entidad=&entidadId=&desde=&hasta=&actorUsuarioId=&accion=&resultado=`

---

## 6. Reglas de Validacion y Seguridad

Validaciones de negocio obligatorias:
1. Actor debe ser `ROL_ADMIN`.
2. IDs > 0 para referencias obligatorias.
3. `vigenciaHasta >= vigenciaDesde` cuando exista.
4. Prohibir self-delegacion (`deleganteUsuarioId != delegadoUsuarioId`).
5. Solo delegar desde jerarquia activa y vigente.
6. Para asignaciones de usuario:
   - `rolId` debe existir en `Configuracion.Rol`.
   - `unidadId` debe existir en `RecursosHumanos.EstructuraOrganizacional` via `CodigoOrigen` o mapping definido.
   - `jefaturaId` debe existir y corresponder a usuario activo de rol jefatura/admin segun regla definida.
7. Evitar ciclos directos de jefatura (usuario no puede ser su propia jefatura).
8. Soft delete por estado en vez de hard delete para trazabilidad.

Controles de seguridad:
- No confiar en frontend para autorizacion.
- Sanitizar y limitar strings (`motivo`, search, descripcion).
- Paginacion en listados admin para evitar extraccion masiva.
- Registrar intentos denegados admin en auditoria (resultado denegado) cuando aplique.
- Incluir correlation id en respuesta de error para soporte.

---

## 7. Plan Incremental de Implementacion (con archivos y criterios)

### Fase 0: Saneamiento de base y compatibilidad

Cambios:
- Revisar y alinear SQL infra para columnas `CreadoPor`.
- Corregir referencias a objetos legacy (`dbo.Estructuras_Organizacionales` etc.).

Archivos:
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- Nuevo script `docs/db/008_admin_audit_and_alignment.sql`

Aceptacion:
- Endpoints actuales admin crean/actualizan sin error SQL.
- Smoke test de consultas jefatura sigue verde.

### Fase 1: Auditoria detallada de acciones admin

Cambios:
- Crear tabla `Auditoria.AdminAccionAuditoria`.
- Implementar `IAdminActionAuditRepository` + repo infra.
- Registrar old/new JSON en mutaciones admin existentes (create/toggle jerarquia/delegacion).

Archivos:
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminActionAuditRepository.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/AdminActionAuditRepository.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminActionAuditSql.cs`
- `backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs`
- `backend/src/IntegradorMarcas.Api/Program.cs` (DI)
- `docs/db/008_admin_audit_and_alignment.sql`

Aceptacion:
- Cada POST/PATCH admin genera 1 registro en `EventoAuditoria` + 1 en `AdminAccionAuditoria`.
- Registro contiene actor, accion, target, old/new y resultado.

### Fase 2: CRUD de actualizacion admin (jerarquia/delegacion)

Cambios:
- Agregar DTOs/contracts de update.
- Agregar metodos service/repository para PATCH completo.
- Validaciones extra (no solapamientos invalidos, delegacion consistente).

Archivos:
- `backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Requests/*Update*.cs`
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminAprobacionesService.cs`
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminAprobacionesRepository.cs`
- `backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/AdminAprobacionesRepository.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs`

Aceptacion:
- Admin puede editar jerarquia/delegacion sin recrear registro.
- Auditoria registra old/new completos por update.

### Fase 3: API de organizacion y asignaciones de usuarios

Cambios:
- Nuevo modulo `AdminOrganizacion` para listar estructuras/usuarios y editar asignaciones.

Archivos:
- `backend/src/IntegradorMarcas.Api/Controllers/AdminOrganizacionController.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Requests/AdminUsuarioAsignacionPatchRequest.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminUsuarioResponse.cs`
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminOrganizacionService.cs`
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminOrganizacionRepository.cs`
- `backend/src/IntegradorMarcas.Application/Services/AdminOrganizacionService.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/AdminOrganizacionRepository.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminOrganizacionSql.cs`
- `backend/src/IntegradorMarcas.Api/Program.cs` (DI)

Aceptacion:
- Admin lista dependencias/jefaturas/funcionarios con filtros.
- Admin cambia asignaciones claves y cambios impactan consultas de alcance.
- Toda mutacion queda auditada.

### Fase 4: Frontend Admin Dashboard

Cambios:
- Agregar tab/panel admin y formularios de gestion.
- Integrar llamadas API de fases 2 y 3.
- Guardas UI por rol admin.

Archivos:
- `dashboard.html`
- `app.js`
- `style.css`

Aceptacion:
- Usuario `ROL_ADMIN` ve panel admin.
- Puede listar/editar jerarquias, delegaciones y asignaciones.
- Usuario no admin no ve panel y recibe 403 si intenta endpoint admin.

### Fase 5: Pruebas y endurecimiento

Cambios:
- Tests unitarios de servicios admin.
- Tests de integracion para auditoria admin.
- Casos de seguridad/validacion negativa.

Archivos:
- `backend/tests/IntegradorMarcas.Tests/AdminAprobacionesServiceTests.cs` (nuevo)
- `backend/tests/IntegradorMarcas.Tests/AdminOrganizacionServiceTests.cs` (nuevo)
- `backend/tests/IntegradorMarcas.Tests/AdminAuditIntegrationTests.cs` (nuevo)

Aceptacion:
- Cobertura de reglas criticas admin y auditoria.
- Evidencia de registro old/new en BD.

---

## 8. Criterios de Aceptacion Globales

1. Existe panel admin funcional en frontend, visible solo para `ROL_ADMIN`.
2. Existen endpoints admin para ver y editar:
- estructura/dependencias,
- asignaciones de usuarios (jefatura/unidad/rol/estado),
- jerarquias,
- delegaciones.
3. Toda accion admin de mutacion registra auditoria detallada con old/new values.
4. Auditoria incluye actor, fecha, accion, target y resultado.
5. Operaciones admin no rompen flujos actuales de funcionario/jefatura/rrhh.
6. Pruebas automticas cubren permisos, validaciones y auditoria.

---

## 9. Recomendacion de Implementacion Minima (MVP seguro)

Si se requiere salida rapida sin sobrecargar alcance:
- Priorizar Fase 0 + Fase 1 + Fase 2 + subset de Fase 4 (solo jerarquias/delegaciones primero).
- Dejar Fase 3 (asignaciones de usuario) como segunda entrega, pero con auditoria ya lista.

Esto habilita administracion inmediata de reglas de aprobacion/delegacion con trazabilidad robusta y reduce riesgo de cambios amplios en RH desde el primer corte.
