# Bloque - Jerarquia/Admin Implementacion

## 1. Objetivo del bloque
Implementar la base tecnica para RF-03/RF-07/RF-08/RF-09/RF-10 sin romper contratos actuales de jefatura y con cambios minimos de API externa:
1. Migracion SQL fundacional para catalogos administrativos, jerarquia, delegaciones y auditoria funcional.
2. Incorporar `ROL_ADMIN` en constantes de dominio y guards de backend.
3. Implementar motor de alcance de aprobacion para jefatura (jerarquia activa o delegacion vigente).
4. Reusar el motor en flujos existentes de jefatura: pendientes, detalle y resolver.
5. Exponer endpoints admin minimos para gestionar jerarquias/delegaciones.
6. Persistir auditoria minima para acciones clave (crear, resolver, cambios admin).

## 2. Estado actual relevante
- El backend actual usa arquitectura por capas: Api/Application/Infrastructure/Domain.
- Autorizacion funcional se valida en servicios con `RolesSistema`.
- Flujo jefatura hoy depende de `Usuarios.JefaturaID = @JefaturaID` en SQL (`JustificacionesSql`).
- Ya existe persistencia de errores tecnicos en `dbo.ApiErrorLog`, pero no auditoria funcional persistente de negocio.
- Scripts SQL siguen convencion incremental en `docs/db/00X_*.sql`.

## 3. Estrategia de implementacion (resumen)
1. Crear migracion SQL incremental `009_*` con tablas/catalogos admin + seeds + indices + funcion de alcance.
2. Extender dominio/guards para `ROL_ADMIN`.
3. Crear componente de alcance de aprobacion en Infrastructure (query reusable sobre jerarquia/delegacion activa).
4. Reemplazar validaciones de subordinacion directa por validacion de alcance efectivo en service/repository.
5. Agregar controlador admin minimo (`/api/admin/aprobaciones/*`) con operaciones CRUD basicas para jerarquia/delegacion.
6. Registrar eventos en `Auditoria_Eventos` para create/resolver/admin.

## 4. Objetos SQL a agregar/ajustar

### 4.1 Nuevo script
- `docs/db/009_admin_hierarchy_delegation_audit_foundation.sql`

### 4.2 Tablas nuevas
1. `dbo.Cat_EstadosRegistro`
- `EstadoRegistroID int PK`
- `Descripcion varchar(50) not null`
- `Usr_Registro varchar(50) not null`
- `Fec_Registro datetime not null default(getdate())`
- Seed: `1=Activo`, `2=Inactivo`

2. `dbo.Cat_TiposEventoAuditoria`
- `TipoEventoAuditoriaID int PK`
- `Descripcion varchar(100) not null`
- `Usr_Registro varchar(50) not null`
- `Fec_Registro datetime not null default(getdate())`
- Seed minimo:
  - `1=CreacionJustificacion`
  - `2=ResolucionJustificacionAprobada`
  - `3=ResolucionJustificacionRechazada`
  - `4=AltaJerarquia`
  - `5=CambioEstadoJerarquia`
  - `6=AltaDelegacion`
  - `7=CambioEstadoDelegacion`

3. `dbo.Cat_ResultadosAuditoria`
- `ResultadoAuditoriaID int PK`
- `Descripcion varchar(50) not null`
- `Usr_Registro varchar(50) not null`
- `Fec_Registro datetime not null default(getdate())`
- Seed minimo: `1=Exito`, `2=Fallo`, `3=Denegado`

4. `dbo.Estructuras_Organizacionales`
- `EstructuraOrganizacionalID int identity PK`
- `Nombre varchar(150) not null`
- `CodigoOrigen varchar(50) null`
- `EstructuraPadreID int null FK -> dbo.Estructuras_Organizacionales`
- `EstadoRegistroID int not null FK -> dbo.Cat_EstadosRegistro`
- `VigenciaDesde datetime null`
- `VigenciaHasta datetime null`

5. `dbo.Jerarquias_Aprobacion`
- `JerarquiaAprobacionID int identity PK`
- `AprobadorUsuarioID int not null FK -> dbo.Usuarios`
- `EstructuraOrganizacionalID int not null FK -> dbo.Estructuras_Organizacionales`
- `NivelAprobacion int not null`
- `TipoRelacion varchar(20) not null` (`Vertical` | `Horizontal`)
- `EstadoRegistroID int not null FK -> dbo.Cat_EstadosRegistro`
- `VigenciaDesde datetime not null`
- `VigenciaHasta datetime null`
- `Usr_Registro varchar(50) not null`
- `Fec_Registro datetime not null default(getdate())`

6. `dbo.Delegaciones_Aprobacion`
- `DelegacionAprobacionID int identity PK`
- `DeleganteUsuarioID int not null FK -> dbo.Usuarios`
- `DelegadoUsuarioID int not null FK -> dbo.Usuarios`
- `JerarquiaAprobacionID int null FK -> dbo.Jerarquias_Aprobacion`
- `Motivo varchar(250) null`
- `EstadoRegistroID int not null FK -> dbo.Cat_EstadosRegistro`
- `VigenciaDesde datetime not null`
- `VigenciaHasta datetime null`
- `Usr_Registro varchar(50) not null`
- `Fec_Registro datetime not null default(getdate())`

7. `dbo.Auditoria_Eventos`
- `AuditoriaEventoID bigint identity PK`
- `FechaEvento datetime not null default(getdate())`
- `UsuarioID int null FK -> dbo.Usuarios`
- `NombreUsuario varchar(150) not null`
- `RolCodigo varchar(20) not null`
- `TipoEventoAuditoriaID int not null FK -> dbo.Cat_TiposEventoAuditoria`
- `DescripcionEvento varchar(500) not null`
- `ResultadoAuditoriaID int not null FK -> dbo.Cat_ResultadosAuditoria`
- `ReferenciaFuncional varchar(100) null`
- `PayloadResumen varchar(1000) null`

### 4.3 Ajustes sobre tablas existentes
1. `dbo.Roles`
- MERGE seed para `RolID=4, NombreRol='Administrador'`.

2. `dbo.Justificaciones_Encabezado`
- Agregar columna si no existe:
  - `RolResolucion varchar(20) null`

### 4.4 Funcion SQL para motor de alcance
Agregar funcion inline table-valued:
- `dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioID int, @FechaRef datetime)`

Comportamiento esperado:
1. Obtiene estructura del solicitante (fase inicial: map por `Usuarios.UnidadID` => `Estructuras_Organizacionales.CodigoOrigen`).
2. Resuelve jerarquia activa (`Jerarquias_Aprobacion` + estado activo + vigencia).
3. Incluye delegaciones activas (`Delegaciones_Aprobacion`) como aprobadores efectivos.
4. Devuelve conjunto unico con:
- `AprobadorUsuarioID`
- `Origen` (`Jerarquia` o `Delegacion`)
- `DeleganteUsuarioID` (null para jerarquia directa)

### 4.5 Indices minimos
- `IX_Estructuras_CodigoOrigen_Activo` en `Estructuras_Organizacionales(CodigoOrigen, EstadoRegistroID)`
- `IX_Jerarquias_Aprobador_Estructura_Vigencia` en `Jerarquias_Aprobacion(AprobadorUsuarioID, EstructuraOrganizacionalID, EstadoRegistroID, VigenciaDesde, VigenciaHasta)`
- `IX_Delegaciones_Delegado_Vigencia` en `Delegaciones_Aprobacion(DelegadoUsuarioID, EstadoRegistroID, VigenciaDesde, VigenciaHasta)`
- `IX_Auditoria_FechaEvento` en `Auditoria_Eventos(FechaEvento DESC)`
- `IX_Auditoria_TipoResultadoFecha` en `Auditoria_Eventos(TipoEventoAuditoriaID, ResultadoAuditoriaID, FechaEvento DESC)`

## 5. Archivos exactos a editar/crear

### 5.1 Domain
Editar:
- `backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs`

Cambios:
- Agregar `public const string RolAdmin = "ROL_ADMIN";`
- Agregar `EsAdmin(string? rol)` con alias (`ADMIN`, `4`).

### 5.2 Application - DTOs
Crear:
- `backend/src/IntegradorMarcas.Application/DTOs/AprobacionScopeValidationDto.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/AdminJerarquiaDto.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/AdminDelegacionDto.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/CreateJerarquiaDto.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/CreateDelegacionDto.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/ToggleEstadoRegistroDto.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/AuditEventEntry.cs`

Editar:
- `backend/src/IntegradorMarcas.Application/DTOs/ResolverValidationDto.cs`

Cambios en `ResolverValidationDto`:
- Mantener `Exists` y `EstadoId`.
- Reemplazar o complementar `IsSubordinado` por `IsInApprovalScope`.
- Agregar metadato opcional `ScopeSource` (`Jerarquia`/`Delegacion`) para trazabilidad.

### 5.3 Application - Interfaces
Crear:
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminAprobacionesService.cs`
- `backend/src/IntegradorMarcas.Application/Interfaces/IAdminAprobacionesRepository.cs`
- `backend/src/IntegradorMarcas.Application/Interfaces/IAuditEventRepository.cs`

Editar:
- `backend/src/IntegradorMarcas.Application/Interfaces/IJustificacionRepository.cs`
- `backend/src/IntegradorMarcas.Application/Interfaces/IJustificacionService.cs`

Cambios clave:
- `IJustificacionRepository`: exponer metodo de validacion por alcance (`GetAprobacionScopeValidationAsync`).
- `IJustificacionService`: sin cambio de contratos HTTP; cambios internos de reglas.

### 5.4 Application - Services/Validation
Crear:
- `backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs`

Editar:
- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`
- `backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs`

Cambios en `JustificacionService`:
1. En `ListPendientesJefaturaAsync`, `GetDetalleJefaturaAsync`, `ResolverAsync`:
- Mantener guard de rol jefatura.
- Sustituir validacion de subordinado directo por validacion de alcance efectivo.
2. En `CreateAsync` y `ResolverAsync`:
- Emitir evento en `Auditoria_Eventos` (via `IAuditEventRepository`).

### 5.5 Infrastructure - Queries/Repositories
Crear:
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AuditoriaSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/AdminAprobacionesRepository.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/AuditEventRepository.cs`

Editar:
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs`

Cambios en `JustificacionesSql.cs`:
- `ListPendientesJefatura`: reemplazar filtro `u.JefaturaID = @JefaturaID` por `EXISTS` usando `fn_AprobadoresVigentesPorSolicitante`.
- `GetDetalleJefaturaEncabezado`: mismo cambio de filtro de alcance.
- `GetResolverValidation`: calcular `IsInApprovalScope` contra funcion de alcance.
- `ResolverPendiente`: aplicar alcance en `WHERE` con `EXISTS` para evitar resolver fuera de scope.
- Mantener contratos de salida existentes para no romper API.

Cambios en `JustificacionRepository.cs`:
- Ajustar parametros de queries a `AprobadorUsuarioID` (mantener nombre viejo si se quiere evitar tocar demasiados sitios).
- Implementar llamada a nueva validacion de alcance.
- Registrar evento de auditoria de creacion y resolucion exitosa.

### 5.6 API - Controllers/Contracts/Program
Crear:
- `backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateJerarquiaRequest.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateDelegacionRequest.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Requests/ToggleEstadoRegistroRequest.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminJerarquiaResponse.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminDelegacionResponse.cs`

Editar:
- `backend/src/IntegradorMarcas.Api/Program.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs` (solo si se ocupa ajustar mensajes/codigos)

Cambios en `Program.cs`:
- Registrar DI para:
  - `IAdminAprobacionesService` -> `AdminAprobacionesService`
  - `IAdminAprobacionesRepository` -> `AdminAprobacionesRepository`
  - `IAuditEventRepository` -> `AuditEventRepository`

## 6. Endpoints admin minimos
Base route:
- `api/admin/aprobaciones`

Autorizacion funcional:
- Solo usuarios con `ROL_ADMIN` (guard en service).

### 6.1 Jerarquias
1. `GET /api/admin/aprobaciones/jerarquias`
- Query opcional: `aprobadorUsuarioId`, `estadoRegistroId`
- Retorna lista simple para administracion.

2. `POST /api/admin/aprobaciones/jerarquias`
- Crea regla de jerarquia.
- Body minimo:
  - `aprobadorUsuarioId`
  - `estructuraOrganizacionalId`
  - `nivelAprobacion`
  - `tipoRelacion`
  - `vigenciaDesde`
  - `vigenciaHasta` (opcional)

3. `PATCH /api/admin/aprobaciones/jerarquias/{jerarquiaAprobacionId:int}/estado`
- Activa/Inactiva regla (`estadoRegistroId`).

### 6.2 Delegaciones
1. `GET /api/admin/aprobaciones/delegaciones`
- Query opcional: `deleganteUsuarioId`, `delegadoUsuarioId`, `estadoRegistroId`, `vigenteEnFecha`.

2. `POST /api/admin/aprobaciones/delegaciones`
- Crea delegacion.
- Body minimo:
  - `deleganteUsuarioId`
  - `delegadoUsuarioId`
  - `jerarquiaAprobacionId` (opcional)
  - `motivo` (opcional)
  - `vigenciaDesde`
  - `vigenciaHasta` (opcional)

3. `PATCH /api/admin/aprobaciones/delegaciones/{delegacionAprobacionId:int}/estado`
- Activa/Inactiva delegacion (`estadoRegistroId`).

## 7. Cambios de reglas en flujos jefatura existentes
No se cambian rutas publicas ya existentes:
- `GET /api/jefatura/justificaciones/pendientes`
- `GET /api/jefatura/justificaciones/{justificacionId}`
- `PATCH /api/jefatura/justificaciones/{justificacionId}/resolver`

Cambio funcional interno:
- Antes: visible/resoluble solo si solicitante tiene `Usuarios.JefaturaID = usuarioAutenticado`.
- Nuevo: visible/resoluble si usuario autenticado esta en `fn_AprobadoresVigentesPorSolicitante(solicitante)` por jerarquia activa o delegacion vigente.

Mensaje de negocio esperado en denegacion:
- `403`: "La boleta no pertenece al alcance de aprobacion vigente del usuario autenticado."

## 8. Orden recomendado de migracion/implementacion

### Fase 1 - SQL base
1. Ejecutar `009_admin_hierarchy_delegation_audit_foundation.sql`.
2. Verificar seeds (`Roles`, catalogos admin, eventos, resultados).
3. Verificar creacion de funcion `fn_AprobadoresVigentesPorSolicitante`.

### Fase 2 - Dominio y DI
1. Agregar `ROL_ADMIN` y guard.
2. Registrar servicios/repos nuevos en `Program.cs`.

### Fase 3 - Motor de alcance y jefatura
1. Actualizar SQL de `JustificacionesSql` para usar alcance efectivo.
2. Ajustar `JustificacionRepository` y `JustificacionService`.
3. Probar pendientes/detalle/resolver con casos de jerarquia y delegacion.

### Fase 4 - Admin endpoints
1. Implementar repository/service/controller admin.
2. Probar CRUD minimo (listar/crear/cambiar estado) para jerarquia y delegaciones.

### Fase 5 - Auditoria funcional
1. Implementar `IAuditEventRepository`.
2. Insertar eventos en create/resolver/admin cambios.
3. Verificar persistencia y payload resumido.

## 9. Criterios de aceptacion
1. Existe script SQL incremental en `docs/db` con nomenclatura de proyecto y crea todos los objetos fundacionales requeridos.
2. `ROL_ADMIN` existe en dominio (`RolesSistema`) y se puede validar con guard en servicios admin.
3. Un `ROL_JEFE` puede ver/resolver boletas por jerarquia activa aun cuando no sea jefatura directa en `Usuarios.JefaturaID`.
4. Un delegado activo puede ver/resolver boletas del delegante durante la vigencia.
5. Fuera de alcance vigente, detalle/resolucion retornan `403`.
6. Endpoints admin minimos (`GET/POST/PATCH estado`) para jerarquias y delegaciones responden y persisten cambios.
7. Se insertan eventos en `Auditoria_Eventos` para:
- creacion de boleta,
- resolucion (aprobar/rechazar),
- alta/cambio estado de jerarquia,
- alta/cambio estado de delegacion.
8. No se rompen rutas existentes de funcionario/jefatura/RRHH.

## 10. Validacion tecnica (build + behavior checks)

### 10.1 Build
Desde `backend/`:
- `dotnet build IntegradorMarcas.slnx`

Resultado esperado:
- Compila sin errores.

### 10.2 Checks SQL minimos
1. Verificar objetos:
- `SELECT OBJECT_ID('dbo.Cat_EstadosRegistro','U')`
- `SELECT OBJECT_ID('dbo.Jerarquias_Aprobacion','U')`
- `SELECT OBJECT_ID('dbo.Delegaciones_Aprobacion','U')`
- `SELECT OBJECT_ID('dbo.Auditoria_Eventos','U')`
- `SELECT OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante','IF')`

2. Verificar seed admin:
- `SELECT * FROM dbo.Roles WHERE RolID = 4`

### 10.3 Checks funcionales API (minimos)
1. Jefatura por jerarquia (no jefatura directa):
- Seed de jerarquia activa para aprobador X y solicitante Y.
- `GET /api/jefatura/justificaciones/pendientes` con headers de X debe incluir boletas de Y.

2. Jefatura por delegacion:
- Seed delegacion activa de X hacia Z.
- `PATCH /api/jefatura/justificaciones/{id}/resolver` con headers de Z debe retornar `204`.

3. Fuera de alcance:
- Usuario jefatura sin regla/delegacion activa para boleta objetivo debe recibir `403` en detalle/resolver.

4. Admin endpoints:
- `POST /api/admin/aprobaciones/jerarquias` con `ROL_ADMIN` crea registro.
- `PATCH /api/admin/aprobaciones/jerarquias/{id}/estado` actualiza estado.
- `POST /api/admin/aprobaciones/delegaciones` crea registro.
- `PATCH /api/admin/aprobaciones/delegaciones/{id}/estado` actualiza estado.

5. Auditoria persistente:
- Tras create/resolver/admin change, existe fila en `dbo.Auditoria_Eventos` con `TipoEventoAuditoriaID` y `ResultadoAuditoriaID` correctos.

## 11. Riesgos y decisiones tecnicas
1. Mapeo solicitante->estructura: fase inicial se resuelve por `Usuarios.UnidadID` contra `Estructuras_Organizacionales.CodigoOrigen`; si luego se requiere precision adicional, se agrega tabla puente sin romper API.
2. Vigencias: usar comparacion inclusiva (`VigenciaDesde <= @FechaRef` y `VigenciaHasta is null or VigenciaHasta >= @FechaRef`).
3. Concurrencia en resolver: mantener validacion de estado pendiente en `UPDATE` para evitar doble resolucion.
4. Minimizar breaking changes: conservar rutas y payloads actuales de jefatura; cambios son internos de autorizacion y trazabilidad.
