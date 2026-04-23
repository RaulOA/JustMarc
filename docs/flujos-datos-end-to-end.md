# Flujos de Datos End-to-End

## 1. Proposito
Trazar los flujos reales de datos desde UI hasta SQL Server y de vuelta, por rol funcional, con validaciones y errores por etapa.

## 2. Alcance
- Flujos implementados: crear boleta, listar mias, pendientes jefatura, detalle jefatura, resolver, consulta RRHH.
- Incluye mapeo endpoint -> service -> repository -> SQL -> tablas.

## 3. Fuente de verdad
- app.js
- Controllers en backend/src/IntegradorMarcas.Api/Controllers
- JustificacionService y JustificacionValidator
- JustificacionRepository y JustificacionesSql

## 4. Mapa tecnico global
| Flujo | Endpoint | Service | Repository | SQL principal | Tablas |
|---|---|---|---|---|---|
| Crear boleta | POST /api/justificaciones | CreateAsync | CreateAsync | InsertEncabezado + InsertDetalle | Justificaciones_Encabezado, Justificaciones_Detalle |
| Mis boletas | GET /api/justificaciones/mias | ListMineAsync | ListMineAsync | ListMine | Justificaciones_Encabezado, Justificaciones_Detalle, Estados |
| Pendientes jefatura | GET /api/jefatura/justificaciones/pendientes | ListPendientesJefaturaAsync | ListPendientesJefaturaAsync | ListPendientesJefatura | Justificaciones_Encabezado, Usuarios, Justificaciones_Detalle, Estados |
| Detalle jefatura | GET /api/jefatura/justificaciones/{id} | GetDetalleJefaturaAsync | GetResolverValidationAsync + GetDetalleJefaturaAsync | GetResolverValidation + GetDetalleJefaturaEncabezado + GetDetalleJefaturaLineas | Justificaciones_Encabezado, Usuarios, Justificaciones_Detalle, Cat_TiposJustificacion, Estados |
| Resolver boleta | PATCH /api/jefatura/justificaciones/{id}/resolver | ResolverAsync | GetResolverValidationAsync + ResolverAsync | GetResolverValidation + ResolverPendiente | Justificaciones_Encabezado, Usuarios |
| Consulta RRHH | GET /api/rrhh/justificaciones | ListRrhhAsync | ListRrhhAsync | ListRrhhGlobal | Justificaciones_Encabezado, Justificaciones_Detalle, Usuarios, Estados, Cat_TiposJustificacion |

## 5. Secuencias por rol

### 5.1 Funcionario: crear boleta
```mermaid
sequenceDiagram
    participant UI as Frontend
    participant API as JustificacionesController
    participant SVC as JustificacionService
    participant VAL as JustificacionValidator
    participant REP as JustificacionRepository
    participant DB as SQL Server

    UI->>API: POST /api/justificaciones + headers + payload
    API->>SVC: CreateAsync(user, dto)
    SVC->>VAL: ValidateCreate
    SVC->>REP: GetExistingTipoJustificacionIdsAsync
    REP->>DB: SELECT catalogo tipos
    DB-->>REP: IDs existentes
    SVC->>REP: CreateAsync (transaccion)
    REP->>DB: INSERT encabezado + detalle(s)
    DB-->>REP: justificacionId
    REP-->>SVC: justificacionId
    SVC-->>API: id
    API-->>UI: 201 Created
```

Errores clave:
- 400: motivo/lineas/tipo/fecha invalidos.
- 401: headers faltantes o invalidos.
- 403: rol distinto de Funcionario.

### 5.2 Jefatura: ver pendientes y detalle
```mermaid
sequenceDiagram
    participant UI as Frontend
    participant API as JefaturaController
    participant SVC as JustificacionService
    participant REP as JustificacionRepository
    participant DB as SQL Server

    UI->>API: GET pendientes
    API->>SVC: ListPendientesJefaturaAsync
    SVC->>REP: ListPendientesJefaturaAsync
    REP->>DB: SQL ListPendientesJefatura
    DB-->>UI: Lista resumen

    UI->>API: GET detalle/{id}
    API->>SVC: GetDetalleJefaturaAsync
    SVC->>REP: GetResolverValidationAsync
    REP->>DB: SQL validacion existencia + subordinacion
    SVC->>REP: GetDetalleJefaturaAsync
    REP->>DB: SQL encabezado + lineas
    API-->>UI: detalle completo
```

Errores clave:
- 403: rol no jefatura o no subordinado directo.
- 404: boleta no existe.

### 5.3 Jefatura: resolver boleta
```mermaid
sequenceDiagram
    participant UI as Frontend
    participant API as JefaturaController
    participant SVC as JustificacionService
    participant VAL as JustificacionValidator
    participant REP as JustificacionRepository
    participant DB as SQL Server

    UI->>API: PATCH resolver/{id}
    API->>SVC: ResolverAsync
    SVC->>VAL: ValidateAccion + NormalizeComentarioResolucion
    SVC->>REP: GetResolverValidationAsync
    REP->>DB: SQL validacion
    SVC->>REP: ResolverAsync
    REP->>DB: UPDATE ... WHERE EstadoID=Pendiente y subordinado
    DB-->>REP: filas afectadas
    API-->>UI: 204 No Content
```

Errores clave:
- 409 RN-04: boleta ya resuelta.
- 409 concurrencia: filas afectadas = 0 en UPDATE condicional.

### 5.4 RRHH: consulta global
```mermaid
sequenceDiagram
    participant UI as Frontend
    participant API as RrhhController
    participant SVC as JustificacionService
    participant VAL as JustificacionValidator
    participant REP as JustificacionRepository
    participant DB as SQL Server

    UI->>API: GET /api/rrhh/justificaciones?filtros
    API->>SVC: ListRrhhAsync
    SVC->>VAL: validar fechas/compania/texto
    SVC->>REP: ListRrhhAsync
    REP->>DB: SQL ListRrhhGlobal
    API-->>UI: lista global
```

Errores clave:
- 400 compania invalida o fechas inconsistentes.
- 403 si no es rol RRHH.

## 6. Puntos de validacion por etapa
- Frontend: valida requeridos basicos de formulario (motivo y lineas).
- API Security: valida headers y formato de identidad.
- Service/Validator: valida negocio y autorizacion por rol.
- SQL: aplica filtros y condicion de estado pendiente para concurrencia optimista.

## 7. Manejo de errores y trazabilidad
- AppException mapea a status definido.
- Middleware global retorna ProblemDetails.
- Handler principal agrega X-Correlation-Id y persiste en dbo.ApiErrorLog.
- Errores de red/timeout en frontend generan toast explicativo.

## 8. Checklist de validacion
- Cada flujo documentado llega a SQL real en JustificacionesSql.
- Reglas de autorizacion coinciden con JustificacionService.
- Errores coinciden con AppException + middleware.
- Endpoint/rol coincide con controladores y frontend.

## 9. Historial de cambios
- 2026-04-23: Documento creado con trazabilidad completa UI -> API -> SQL.
