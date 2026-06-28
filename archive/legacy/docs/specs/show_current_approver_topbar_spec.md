# Spec: Mostrar Aprobador Actual en Topbar

## Objetivo

Mostrar en el topbar del dashboard quien aprueba actualmente las justificaciones del usuario autenticado (jefatura directa o delegado vigente), sin romper endpoints existentes ni cambiar reglas de negocio.

## Hallazgos clave

## 1) Como se resuelve hoy el alcance de aprobacion vigente

La logica vigente de aprobadores (incluyendo delegacion) ya existe en BD y backend:

- Funcion SQL usada por backend:
  - `dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioID, @FechaRef)`
  - Devuelve: `AprobadorUsuarioId`, `Origen` (`Jerarquia` o `Delegacion`), `DeleganteUsuarioId`.
- Evidencia de uso actual en queries de negocio:
  - `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
  - Se usa en:
    - `ListPendientesJefatura`
    - `ListHistorico` (scope jefatura)
    - `GetDetalleJefaturaEncabezado`
    - `GetAprobacionScopeValidation`
    - `GetResolverValidation`
    - `ResolverPendiente`

Conclusion: no hay que inventar logica nueva de vigencia/delegacion; ya esta centralizada en la funcion SQL.

## 2) Endpoints/servicios existentes relacionados

- `GET /api/jefatura/justificaciones/pendientes`
  - Controller: `backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs`
  - Service: `JustificacionService.ListPendientesJefaturaAsync`
  - Query: `JustificacionesSql.ListPendientesJefatura`
  - Usa `fn_AprobadoresVigentesPorSolicitante` para decidir si la jefatura autenticada esta en alcance.

- `GET /api/justificaciones/historico`
  - Controller: `backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs`
  - Service: `JustificacionService.ListHistoricoAsync`
  - Query: `JustificacionesSql.ListHistorico`
  - Para rol jefatura, usa scope por `AprobadorUsuarioID` y excluye propios en scope aprobador.

- `GET /api/admin/aprobaciones/delegaciones`
  - Controller: `backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs`
  - Service/repo SQL: `AdminAprobacionesService/ListDelegacionesAsync` + `AdminAprobacionesSql.ListDelegaciones`
  - Permite ver delegaciones con filtro de vigencia, pero es flujo administrativo (no pensado para topbar del funcionario).

## 3) Como se llena el contexto de usuario en frontend dashboard

- Login y sesion:
  - `app.js` -> `handleLogin()` guarda sesion en `sessionStorage` (`sjm_session`) con `{ username, role, company, apiBaseUrl }`.
- Resolucion de identidad para API:
  - `app.js` -> `resolveUserIdentity(session)` y `buildApiHeaders(session)`.
  - Headers enviados a backend:
    - `X-User-Id`
    - `X-User-Role`
- Topbar actual:
  - `dashboard.html` muestra `current-user` y `current-role`.
  - `app.js` -> `configureRoleUI()` rellena esos campos.
- Carga inicial dashboard:
  - `app.js` -> `initDashboardPage()` llama `configureRoleUI()` y luego renders de paneles.

Conclusion: el punto natural para mostrar "aprobador actual" es `configureRoleUI()` (o justo despues, dentro de `initDashboardPage()`).

## 4) Gap actual

No existe endpoint dedicado para que un funcionario consulte "mi aprobador vigente actual" con detalle (nombre, origen, delegante).

Datos disponibles hoy:
- En resúmenes de boletas (`/api/justificaciones/mias`) solo llega `AprobadorID` historico de resolucion de cada boleta, no el aprobador vigente actual para nuevas boletas.

## Implementacion minima y segura propuesta

## A) Backend: nuevo endpoint de consulta puntual

Agregar endpoint read-only en flujo de justificaciones (no admin):

- `GET /api/justificaciones/aprobador-actual`

Ubicacion sugerida:
- Controller: `JustificacionesController`
- Service: `IJustificacionService` / `JustificacionService`
- Repository: `IJustificacionRepository` / `JustificacionRepository`
- SQL: `JustificacionesSql`

Permisos:
- Permitir `ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH` (mismo patron de consultas "mias/historico").
- Scope siempre por usuario autenticado (`X-User-Id` via `IUserContext`).

Comportamiento:
- Busca aprobador(es) vigentes para `user.UserId` a fecha actual con `fn_AprobadoresVigentesPorSolicitante`.
- Si hay mas de uno, priorizar `Delegacion` sobre `Jerarquia` (alineado a validaciones actuales que usan ese criterio).
- Si no hay aprobador vigente, retornar `200` con `aprobador: null` (sin error funcional).

Respuesta sugerida:

```json
{
  "solicitanteUsuarioID": 4,
  "aprobador": {
    "usuarioID": 3,
    "nombreCompleto": "Maria Jefe",
    "cedula": "...",
    "correo": "...",
    "compania": "CNP",
    "unidadID": 120,
    "jefaturaID": null
  },
  "origen": "Delegacion",
  "deleganteUsuarioID": 8,
  "deleganteNombre": "Carlos Delegante"
}
```

DTOs nuevos minimos:
- Application:
  - `CurrentApproverDto`
- API contracts:
  - `CurrentApproverResponse`

## B) SQL minimo (sin tocar funcion existente)

Agregar query nueva en `JustificacionesSql` (ejemplo de estrategia):

- `GetCurrentApproverBySolicitante`
- Base:
  - `OUTER APPLY` sobre `dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioID, GETDATE())`
  - `TOP 1` ordenado con prioridad delegacion:
    - `ORDER BY CASE WHEN fa.Origen = 'Delegacion' THEN 0 ELSE 1 END, fa.AprobadorUsuarioId`
- Join a `RecursosHumanos.Usuario` para datos del aprobador.
- Join opcional a usuario delegante cuando `DeleganteUsuarioId` no sea null.

Importante:
- Reusar semantica actual del sistema (`GETDATE()` y prioridad delegacion) para evitar divergencias.

## C) Frontend dashboard topbar

Cambios minimos en UI:

1. `dashboard.html`
- En bloque de `user-info`, agregar span para aprobador actual:
  - id sugerido: `current-approver`
  - texto inicial: `Aprobador actual: Cargando...`

2. `app.js`
- Crear funcion `renderCurrentApproverTopbar()`:
  - Lee sesion.
  - Llama `GET /api/justificaciones/aprobador-actual` con `buildApiHeaders(session)`.
  - Renderiza:
    - `Aprobador actual: <Nombre> (Delegacion de <Delegante>)`
    - o `Aprobador actual: <Nombre> (Jerarquia)`
    - o `Aprobador actual: No definido`
- Invocarla desde `initDashboardPage()` justo despues de `configureRoleUI()`.
- En error de red/backend, fallback no bloqueante:
  - `Aprobador actual: No disponible`
  - opcion: toast solo si error >=500 (consistente con `apiFetch`).

## D) Seguridad y riesgo

- No usar endpoint admin de delegaciones para topbar de funcionario.
  - Riesgo de sobreexponer informacion y mezclar permisos administrativos.
- No confiar en username de frontend para scope.
  - Scope debe basarse en `X-User-Id` ya resuelto por backend (`HeaderUserContext`).
- Mantener endpoint read-only y sin side effects.

## E) Pruebas minimas recomendadas

Backend:
- Test servicio: retorna aprobador por jerarquia cuando no hay delegacion.
- Test servicio: prioriza delegacion cuando coexiste jerarquia+delegacion.
- Test servicio: retorna null cuando no hay aprobador vigente.
- Test autorizacion: roles no permitidos => 403 (si aplica politica estricta).

Frontend:
- Smoke test dashboard:
  - carga topbar con nombre de aprobador.
  - fallback correcto cuando API falla.

## Criterio de aceptacion

1. Topbar muestra aprobador vigente para usuario autenticado.
2. Si el aprobador llega por delegacion, se muestra claramente el delegante.
3. Si no hay aprobador vigente, se informa sin romper el dashboard.
4. No hay regresiones en endpoints existentes de justificaciones/jefatura.

## Notas de consistencia tecnica

- El backend actual referencia `dbo.fn_AprobadoresVigentesPorSolicitante` en queries.
- En scripts historicos tambien aparece `Operacion.fn_AprobadoresVigentesPorSolicitante`.
- Antes de desplegar, validar en entorno real el schema definitivo de la funcion para evitar errores de objeto inexistente.
