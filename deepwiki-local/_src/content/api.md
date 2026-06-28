## En breve

Esta es la referencia de la **API REST .NET 8** del sistema (proyecto `IntegradorMarcas.Api`). Documenta cada endpoint que el frontend consume: metodo, ruta, rol requerido, parametros, cuerpo y respuesta, todo derivado de los `[Http...]`/`[Route]` y de los Contracts reales del codigo.

> 📌 **Identidad por headers.** TODOS los endpoints de negocio exigen los headers `X-User-Id` y `X-User-Role`. Si faltan, estan vacios o `X-User-Id` no es un entero `> 0`, el backend responde **401**. No hay JWT ni cookies: el backend confia ciegamente en esos dos headers. El detalle del modelo de identidad y los roles esta en [Seguridad](seguridad.html).

La unica excepcion al 401 es `GET /health`, que es publico (no lee headers).

### Convenciones transversales

**Headers de identidad** (en cada request de negocio):

```http
X-User-Id: 6
X-User-Role: ROL_RRHH
```

Roles validos (valores exactos): `ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH`, `ROL_ADMIN`. Los helpers de [RolesSistema.cs](../backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs) aceptan sinonimos (`FUNCIONARIO`/`1`, `JEFATURA`/`2`, `RRHH`/`3`, `ADMIN`/`4`); ver [Seguridad](seguridad.html).

> ⚠️ **Sufijo `ID` en el wire.** Los Contracts (Requests/Responses) usan deliberadamente el sufijo `ID` en mayuscula (`JustificacionID`, `AprobadorID`, `EstructuraOrganizacionalID`). Los DTOs internos usan `Id`. El JSON que viaja por la red lleva la forma del Contract: `ID`.

**Errores: `ProblemDetails` + correlationId.** Cualquier excepcion se mapea en [Program.cs:80-139](../backend/src/IntegradorMarcas.Api/Program.cs) a una respuesta `application/problem+json` con esta forma, y agrega el header `X-Correlation-Id`:

```json
{
  "title": "Solo jefatura puede resolver boletas.",
  "status": 403,
  "correlationId": "0f7d3c4e-1a2b-4c5d-8e9f-001122334455"
}
```

El mismo `correlationId` (un GUID nuevo por error) viaja en el header `X-Correlation-Id` y se persiste en la bitacora tecnica (`Auditoria.ErrorApi`) para cruce en soporte.

**Codigos de estado transversales:**

| Codigo | Origen | Significado |
|---|---|---|
| `200` | Ok | Operacion exitosa con cuerpo |
| `201` | CreatedAtAction | Recurso creado (solo `POST` de boletas) |
| `204` | NoContent | Exito sin cuerpo (resolver / toggle estado) |
| `400` | `AppException(400)` o ModelState invalido | Datos invalidos; ModelState se canaliza a `AppException(400)` en [Program.cs:34-46](../backend/src/IntegradorMarcas.Api/Program.cs) |
| `401` | `IUserContext.GetCurrent()` | Faltan/invalidos los headers de identidad |
| `403` | guard clause de rol en Application | El rol autenticado no puede ejecutar la operacion |
| `404` | `AppException(404)` o `KeyNotFoundException` | Recurso inexistente; tambien rutas no mapeadas ([Program.cs:150-157](../backend/src/IntegradorMarcas.Api/Program.cs)) |
| `409` | `AppException(409)` | Conflicto de estado (p. ej. boleta ya resuelta, RN-04) |
| `499` | `OperationCanceledException` | El cliente canceló la solicitud |
| `500` | cualquier otra excepcion | Error interno; `StackTrace` solo se registra cuando `status >= 500` |

> 💡 La autorizacion NO usa atributos `[Authorize]` ni middleware: cada metodo de servicio en la capa Application valida el rol al inicio (guard clause) y lanza `AppException(403)`. Por eso un 401 siempre viene de los headers y un 403 siempre del rol.

---

## SessionController

Ruta base: `api/Session` ([SessionController.cs](../backend/src/IntegradorMarcas.Api/Controllers/SessionController.cs)). Valida estado de sesion y expone el perfil. **No** delega en la capa Application: parsea los headers a mano y, en `profile`, consulta la BD por `ISqlConnectionFactory` directo (ver [Desviaciones](#desviaciones-arquitectonicas)).

### GET /api/Session/status

Valida que los headers de identidad esten presentes y bien formados.

- **Rol:** cualquiera (solo exige headers validos).
- **Respuesta 200:** `{ isValid, userId, role, serverTime }` (`serverTime` en UTC).
- **Codigos:** `200`, `401` (faltan headers, vacios, o `X-User-Id` no entero).

```json
{ "isValid": true, "userId": 4, "role": "ROL_FUNC", "serverTime": "2026-06-27T14:30:00Z" }
```

### GET /api/Session/profile

Devuelve el perfil con el `nombreCompleto` resuelto desde `RecursosHumanos.Usuario` (best-effort: si la BD falla, retorna `null` sin romper).

- **Rol:** cualquiera (solo exige headers validos).
- **Respuesta 200:** `{ userId, role, nombreCompleto }` (`nombreCompleto` puede ser `null`).
- **Codigos:** `200`, `401`.

### POST /api/Session/logout

Endpoint preparado para invalidacion futura (hoy no hace nada del lado servidor; el cliente limpia su sesion local).

- **Rol:** cualquiera; los headers aqui son opcionales.
- **Respuesta 200:** `{ message, loggedOutAt }` (`loggedOutAt` en UTC).
- **Codigos:** `200`.

---

## JustificacionesController

Ruta base: `api/Justificaciones` ([JustificacionesController.cs](../backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs)). Endpoints del funcionario (y, segun el caso, jefatura/RRHH) sobre sus propias boletas.

### POST /api/Justificaciones

Crea una boleta de justificacion con sus lineas de detalle.

- **Rol:** `ROL_FUNC` o `ROL_JEFE` ([JustificacionService.cs:22-24](../backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs)).
- **Cuerpo** ([CreateJustificacionRequest.cs](../backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateJustificacionRequest.cs) + [JustificacionDetalleRequest.cs](../backend/src/IntegradorMarcas.Api/Contracts/Requests/JustificacionDetalleRequest.cs)):

  | Campo | Tipo | Notas |
  |---|---|---|
  | `MotivoGeneral` | `string` | requerido |
  | `Detalles[]` | array | lista de lineas |
  | `Detalles[].TipoJustificacionID` | `int` | debe existir en catalogo (si no, **400**) |
  | `Detalles[].FechaMarca` | `DateTime` | fecha de la marca |
  | `Detalles[].ObservacionDetalle` | `string?` | opcional |

- **Respuesta 201** ([CreateJustificacionResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/CreateJustificacionResponse.cs)): `{ JustificacionID, EstadoID, EstadoDescripcion }`. Se crea en estado `1` ("Pendiente Jefatura"). El `Location` apunta a `mias`.
- **Codigos:** `201`, `400` (tipo inexistente / ModelState), `401`, `403`.

```json
{ "JustificacionID": 57, "EstadoID": 1, "EstadoDescripcion": "Pendiente Jefatura" }
```

### GET /api/Justificaciones/mias

Lista las boletas del propio usuario con filtros opcionales.

- **Rol:** `ROL_FUNC`, `ROL_JEFE` o `ROL_RRHH`.
- **Query:** `estadoId` (`int?`), `desde` (`DateTime?`), `hasta` (`DateTime?`).
- **Respuesta 200:** array de [JustificacionResumenResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionResumenResponse.cs): `JustificacionID`, `MotivoGeneral`, `ObservacionDetalle`, `ComentarioResolucion`, `EstadoID`, `EstadoDescripcion`, `FechaCreacion`, `CantidadDetalles`, `AprobadorID`, `FechaAprobacion`.
- **Codigos:** `200`, `401`, `403`.

### GET /api/Justificaciones/aprobador-actual

Resuelve quien es el aprobador vigente del solicitante (jerarquia o delegacion).

- **Rol:** `ROL_FUNC`, `ROL_JEFE` o `ROL_RRHH`.
- **Respuesta 200** ([CurrentApproverResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/CurrentApproverResponse.cs)): `SolicitanteUsuarioID`, `Origen` (`Jerarquia`/`Delegacion`), `DeleganteUsuarioID`, `DeleganteNombre`, y `Aprobador` (objeto `UsuarioResumenResponse` o `null`).
- **Codigos:** `200`, `401`, `403`.

### GET /api/Justificaciones/{justificacionId}/lineas

Devuelve las lineas de detalle de una boleta propia.

- **Rol:** `ROL_FUNC`, `ROL_JEFE` o `ROL_RRHH`.
- **Ruta:** `justificacionId` (`int`, debe ser `> 0`, si no **400**).
- **Respuesta 200:** array de [JustificacionDetalleLineaResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleLineaResponse.cs): `DetalleID`, `TipoJustificacionID`, `TipoJustificacionDescripcion`, `FechaMarca`, `ObservacionDetalle`.
- **Codigos:** `200`, `400`, `401`, `403`.

### GET /api/Justificaciones/historico

Historico con scoping por rol (funcionario: sus boletas; jefatura: su set de aprobacion; RRHH: global).

- **Rol:** `ROL_FUNC`, `ROL_JEFE` o `ROL_RRHH`.
- **Query:** `funcionario` (`string?`), `estadoId` (`int?`), `compania` (`string?`), `fechaDesde` (`DateTime?`), `fechaHasta` (`DateTime?`).
- **Respuesta 200:** array de [RrhhJustificacionResumenResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/RrhhJustificacionResumenResponse.cs) (ver campos en [RrhhController](#rrhhcontroller)).
- **Codigos:** `200`, `401`, `403`.

---

## JefaturaController

Ruta base: `api/jefatura/justificaciones` ([JefaturaController.cs](../backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs)). Bandeja y resolucion de la jefatura. Todos los endpoints exigen **`ROL_JEFE`** y validan que la boleta este dentro del alcance de aprobacion vigente del usuario.

### GET /api/jefatura/justificaciones/pendientes

Boletas pendientes visibles para la jefatura autenticada.

- **Rol:** `ROL_JEFE` ([JustificacionService.cs:80-82](../backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs)).
- **Query:** `desde` (`DateTime?`), `hasta` (`DateTime?`).
- **Respuesta 200:** array de `JustificacionResumenResponse`.
- **Codigos:** `200`, `401`, `403`.

### GET /api/jefatura/justificaciones/{justificacionId}

Detalle completo de una boleta accesible por la jefatura actual.

- **Rol:** `ROL_JEFE`.
- **Ruta:** `justificacionId` (`int`).
- **Respuesta 200** ([JustificacionDetalleCompletaResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleCompletaResponse.cs)): `{ Encabezado, Solicitante, Aprobador?, Detalles[] }`, donde `Encabezado` es `JustificacionResumenResponse`, `Solicitante`/`Aprobador` son `UsuarioResumenResponse` y `Detalles[]` son `JustificacionDetalleLineaResponse`.
- **Codigos:** `200`, `401`, `403` (fuera de alcance), `404` (no existe).

### PATCH /api/jefatura/justificaciones/{justificacionId}/resolver

Aprueba o rechaza la boleta.

- **Rol:** `ROL_JEFE` (RN-03, [JustificacionService.cs:182-184](../backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs)).
- **Ruta:** `justificacionId` (`int`).
- **Cuerpo** ([ResolverJustificacionRequest.cs](../backend/src/IntegradorMarcas.Api/Contracts/Requests/ResolverJustificacionRequest.cs)):

  | Campo | Tipo | Notas |
  |---|---|---|
  | `Accion` | `string` | accion de resolucion (aprobar/rechazar; validada en Application) |
  | `Comentario` | `string?` | opcional |

- **Respuesta:** `204 No Content` (sin cuerpo).
- **Codigos:** `204`, `400`, `401`, `403` (fuera de alcance), `404`, `409` (boleta ya resuelta, RN-04).

---

## RrhhController

Ruta base: `api/rrhh/justificaciones` ([RrhhController.cs](../backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs)). Consulta global de RRHH.

### GET /api/rrhh/justificaciones

Lista global de boletas con filtros.

- **Rol:** `ROL_RRHH` ([JustificacionService.cs:107-109](../backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs)).
- **Query:** `funcionario` (`string?`), `estadoId` (`int?`), `compania` (`string?`), `fechaDesde` (`DateTime?`), `fechaHasta` (`DateTime?`).
- **Respuesta 200:** array de [RrhhJustificacionResumenResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/RrhhJustificacionResumenResponse.cs):

  | Campo | Tipo |
  |---|---|
  | `JustificacionID` | `int` |
  | `MotivoGeneral` | `string` |
  | `ComentarioResolucion` | `string?` |
  | `EstadoID` / `EstadoDescripcion` | `int` / `string` |
  | `FechaCreacion` | `DateTime` |
  | `CantidadDetalles` | `int` |
  | `AprobadorID` / `FechaAprobacion` | `int?` / `DateTime?` |
  | `FuncionarioID` / `FuncionarioNombre` / `FuncionarioCedula` | `int` / `string` / `string` |
  | `Compania` | `string` |
  | `JefaturaID` / `JefaturaNombre` | `int?` / `string?` |
  | `TipoPrincipal` | `string?` |

- **Codigos:** `200`, `400` (rango de fechas/compania invalidos), `401`, `403`.

---

## AdminOrganizacionController

Ruta base: `api/admin/organizacion` ([AdminOrganizacionController.cs](../backend/src/IntegradorMarcas.Api/Controllers/AdminOrganizacionController.cs)). Gestion de dependencias y asignacion de usuarios. **Todos exigen `ROL_ADMIN`** ([AdminOrganizacionService.cs:214-216](../backend/src/IntegradorMarcas.Application/Services/AdminOrganizacionService.cs)).

### GET /api/admin/organizacion/dependencias

- **Query:** `estadoRegistroId` (`int?`), `search` (`string?`).
- **Respuesta 200:** array de [AdminDependenciaResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminDependenciaResponse.cs): `EstructuraOrganizacionalID`, `Nombre`, `CodigoOrigen`, `EstructuraPadreID`, `EstadoRegistroID`, `VigenciaDesde`, `VigenciaHasta`.
- **Codigos:** `200`, `401`, `403`.

### PATCH /api/admin/organizacion/dependencias/{estructuraOrganizacionalId}

- **Ruta:** `estructuraOrganizacionalId` (`int`).
- **Cuerpo** ([UpdateDependenciaRequest.cs](../backend/src/IntegradorMarcas.Api/Contracts/Requests/UpdateDependenciaRequest.cs)): `Nombre` (`string`), `EstructuraPadreID` (`int?`), `EstadoRegistroID` (`int`).
- **Respuesta 200:** `AdminDependenciaResponse` actualizado.
- **Codigos:** `200`, `400`, `401`, `403`, `404`.

### GET /api/admin/organizacion/usuarios

- **Query:** `rolId` (`int?`), `unidadId` (`int?`), `jefaturaId` (`int?`), `esActivo` (`bool?`), `search` (`string?`).
- **Respuesta 200:** array de [AdminUsuarioAsignacionResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminUsuarioAsignacionResponse.cs): `UsuarioID`, `Cedula`, `NombreCompleto`, `CorreoElectronico`, `JefaturaID`, `JefaturaNombre`, `UnidadID`, `RolID`, `RolDescripcion`, `EsActivo`.
- **Codigos:** `200`, `401`, `403`.

### PATCH /api/admin/organizacion/usuarios/{usuarioId}/asignacion

- **Ruta:** `usuarioId` (`int`).
- **Cuerpo** ([UpdateUsuarioAsignacionRequest.cs](../backend/src/IntegradorMarcas.Api/Contracts/Requests/UpdateUsuarioAsignacionRequest.cs)): `RolID` (`int?`), `UnidadID` (`int?`), `JefaturaID` (`int?`).
- **Respuesta 200:** `AdminUsuarioAsignacionResponse` actualizado.
- **Codigos:** `200`, `400`, `401`, `403`, `404`.

### PATCH /api/admin/organizacion/usuarios/{usuarioId}/estado

- **Ruta:** `usuarioId` (`int`).
- **Cuerpo** ([UpdateUsuarioEstadoRequest.cs](../backend/src/IntegradorMarcas.Api/Contracts/Requests/UpdateUsuarioEstadoRequest.cs)): `EsActivo` (`bool`).
- **Respuesta 200:** `AdminUsuarioAsignacionResponse` actualizado.
- **Codigos:** `200`, `400`, `401`, `403`, `404`.

---

## AdminAprobacionesController

Ruta base: `api/admin/aprobaciones` ([AdminAprobacionesController.cs](../backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs)). CRUD de jerarquias y delegaciones de aprobacion. **Todos exigen `ROL_ADMIN`** ([AdminAprobacionesService.cs:358-360](../backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs)).

### Jerarquias

| Metodo + ruta | Cuerpo | Respuesta | Codigos |
|---|---|---|---|
| `GET /jerarquias` | query: `aprobadorUsuarioId` (`int?`), `estadoRegistroId` (`int?`) | array `AdminJerarquiaResponse` | `200`, `401`, `403` |
| `POST /jerarquias` | [CreateJerarquiaRequest](../backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateJerarquiaRequest.cs) | `AdminJerarquiaResponse` | `200`, `400`, `401`, `403`, `409` |
| `PATCH /jerarquias/{jerarquiaAprobacionId}` | [UpdateJerarquiaRequest](../backend/src/IntegradorMarcas.Api/Contracts/Requests/UpdateJerarquiaRequest.cs) | `AdminJerarquiaResponse` | `200`, `400`, `401`, `403`, `404`, `409` |
| `PATCH /jerarquias/{jerarquiaAprobacionId}/estado` | [ToggleEstadoRegistroRequest](../backend/src/IntegradorMarcas.Api/Contracts/Requests/ToggleEstadoRegistroRequest.cs) | `204 No Content` | `204`, `400`, `401`, `403`, `404` |

**`CreateJerarquiaRequest`:** `AprobadorUsuarioID` (`int`), `EstructuraOrganizacionalID` (`int`), `NivelAprobacion` (`int`), `TipoRelacion` (`string`), `VigenciaDesde` (`DateTime`), `VigenciaHasta` (`DateTime?`). **`UpdateJerarquiaRequest`** agrega `EstadoRegistroID` (`int`).

**`ToggleEstadoRegistroRequest`:** `EstadoRegistroID` (`int`).

**Respuesta** ([AdminJerarquiaResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminJerarquiaResponse.cs)): `JerarquiaAprobacionID`, `AprobadorUsuarioID`, `EstructuraOrganizacionalID`, `NivelAprobacion`, `TipoRelacion`, `EstadoRegistroID`, `VigenciaDesde`, `VigenciaHasta`.

> Las creaciones/updates de jerarquias y delegaciones devuelven **`200 Ok`** (no `201`), a diferencia del `POST` de boletas que devuelve `201 CreatedAtAction`.

### Delegaciones

| Metodo + ruta | Cuerpo | Respuesta | Codigos |
|---|---|---|---|
| `GET /delegaciones` | query: `deleganteUsuarioId` (`int?`), `delegadoUsuarioId` (`int?`), `estadoRegistroId` (`int?`), `vigenteEnFecha` (`DateTime?`) | array `AdminDelegacionResponse` | `200`, `401`, `403` |
| `POST /delegaciones` | [CreateDelegacionRequest](../backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateDelegacionRequest.cs) | `AdminDelegacionResponse` | `200`, `400`, `401`, `403`, `409` |
| `PATCH /delegaciones/{delegacionAprobacionId}` | [UpdateDelegacionRequest](../backend/src/IntegradorMarcas.Api/Contracts/Requests/UpdateDelegacionRequest.cs) | `AdminDelegacionResponse` | `200`, `400`, `401`, `403`, `404`, `409` |
| `PATCH /delegaciones/{delegacionAprobacionId}/estado` | [ToggleEstadoRegistroRequest](../backend/src/IntegradorMarcas.Api/Contracts/Requests/ToggleEstadoRegistroRequest.cs) | `204 No Content` | `204`, `400`, `401`, `403`, `404` |

**`CreateDelegacionRequest`:** `DeleganteUsuarioID` (`int`), `DelegadoUsuarioID` (`int`), `JerarquiaAprobacionID` (`int?`), `Motivo` (`string?`), `VigenciaDesde` (`DateTime`), `VigenciaHasta` (`DateTime?`). **`UpdateDelegacionRequest`** agrega `EstadoRegistroID` (`int`).

**Respuesta** ([AdminDelegacionResponse.cs](../backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminDelegacionResponse.cs)): `DelegacionAprobacionID`, `DeleganteUsuarioID`, `DelegadoUsuarioID`, `JerarquiaAprobacionID`, `Motivo`, `EstadoRegistroID`, `VigenciaDesde`, `VigenciaHasta`.

---

## AdminMonitoringController

Ruta base: `api/admin/monitoring` ([AdminMonitoringController.cs](../backend/src/IntegradorMarcas.Api/Controllers/AdminMonitoringController.cs)). Bitacora unificada de errores y eventos de auditoria. **Exige `ROL_ADMIN`** (validado en el propio controller, no en Application — ver [Desviaciones](#desviaciones-arquitectonicas)).

### GET /api/admin/monitoring/registros

Une `Auditoria.ErrorApi` + `Auditoria.EventoAuditoria` con filtros y ordenamiento.

- **Rol:** `ROL_ADMIN` ([AdminMonitoringController.cs:34-37](../backend/src/IntegradorMarcas.Api/Controllers/AdminMonitoringController.cs)).
- **Query:**

  | Param | Tipo | Valores validos |
  |---|---|---|
  | `tipo` | `string?` | vacio, `ERROR` o `EVENTO` (otro -> **400**) |
  | `search` | `string?` | texto libre (LIKE sobre mensaje/usuario/categoria/referencia/origen) |
  | `desde` / `hasta` | `DateTime?` | rango de fecha |
  | `sortBy` | `string?` | `fecha` (default), `tipo`, `mensaje`, `usuario`, `estado` (otro -> **400**) |
  | `sortDir` | `string?` | `asc` o `desc` (default `desc`; otro -> **400**) |

- **Respuesta 200:** array de `AdminMonitoringRecordResponse` (clase anidada del controller): `Fecha`, `Tipo`, `Categoria`, `Mensaje`, `Usuario`, `Estado`, `Referencia`, `Origen`, `Detalle`.
- **Codigos:** `200`, `400` (tipo/sortBy/sortDir invalidos), `401`, `403`.

---

## GET /health

Probe operativo definido en [Program.cs:161-165](../backend/src/IntegradorMarcas.Api/Program.cs) con `app.MapGet`.

- **Rol:** publico (no lee headers de identidad).
- **Respuesta 200:** `{ status: "ok", utc }` (`utc` = `DateTime.UtcNow`).

```http
GET /health
```

```json
{ "status": "ok", "utc": "2026-06-27T14:30:00Z" }
```

---

## Desviaciones arquitectonicas

La mayoria de controllers son clientes delgados: traducen Request -> DTO, llaman a un servicio de [Application](modulo-application.html) (que valida el rol) y mapean DTO -> Response. **Dos controllers rompen ese patron:**

> ⚠️ **`SessionController` y `AdminMonitoringController` toman `ISqlConnectionFactory` directo y corren SQL inline**, sin pasar por la capa Application.
>
> - **`SessionController`** ([SessionController.cs:112-117](../backend/src/IntegradorMarcas.Api/Controllers/SessionController.cs)): en `profile` consulta `RecursosHumanos.Usuario` con Dapper. Ademas hace su propio parseo de los headers (`X-User-Id`/`X-User-Role`) en vez de usar `IUserContext`, y es el unico controller **no `sealed`**.
> - **`AdminMonitoringController`** ([AdminMonitoringController.cs:61-143](../backend/src/IntegradorMarcas.Api/Controllers/AdminMonitoringController.cs)): valida el rol `ROL_ADMIN` y corre un `UNION ALL` grande inline; el `AdminMonitoringRecordResponse` es una clase anidada, no un Contract reutilizable.

> Otra nota: la politica CORS `LocalFrontend` ([Program.cs:50-60](../backend/src/IntegradorMarcas.Api/Program.cs)) permite **cualquier** origin/header/method (`SetIsOriginAllowed(_ => true)`); el codigo marca que debe restringirse en despliegues expuestos. Ver [Seguridad](seguridad.html).

---

## Referencias en el codigo

**Controllers:**

- [SessionController.cs](../backend/src/IntegradorMarcas.Api/Controllers/SessionController.cs)
- [JustificacionesController.cs](../backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs)
- [JefaturaController.cs](../backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs)
- [RrhhController.cs](../backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs)
- [AdminOrganizacionController.cs](../backend/src/IntegradorMarcas.Api/Controllers/AdminOrganizacionController.cs)
- [AdminAprobacionesController.cs](../backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs)
- [AdminMonitoringController.cs](../backend/src/IntegradorMarcas.Api/Controllers/AdminMonitoringController.cs)

**Arranque y manejo de errores:**

- [Program.cs](../backend/src/IntegradorMarcas.Api/Program.cs) — `GET /health`, `UseExceptionHandler` (ProblemDetails + `X-Correlation-Id`), CORS, ModelState -> `AppException(400)`.

**Guards de rol (Application):**

- [JustificacionService.cs](../backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs)
- [AdminAprobacionesService.cs](../backend/src/IntegradorMarcas.Application/Services/AdminAprobacionesService.cs)
- [AdminOrganizacionService.cs](../backend/src/IntegradorMarcas.Application/Services/AdminOrganizacionService.cs)
- [RolesSistema.cs](../backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs) · [EstadoIds.cs](../backend/src/IntegradorMarcas.Domain/Constants/EstadoIds.cs)

**Contracts (forma del wire, sufijo `ID`):** carpetas [Contracts/Requests](../backend/src/IntegradorMarcas.Api/Contracts/Requests) y [Contracts/Responses](../backend/src/IntegradorMarcas.Api/Contracts/Responses).

Para el detalle de identidad y autorizacion ver [Seguridad](seguridad.html); para el flujo de negocio extremo a extremo ver [Flujos](flujos.html).
