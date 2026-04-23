# Bloque 4 - RRHH Global + Detalle de Boleta (Jefatura)

## 1. Objetivo
Definir el siguiente bloque de desarrollo despues de RF-02/RF-03, agregando:
1. Endpoint global RRHH para consulta de boletas con filtros.
2. Endpoint de detalle de boleta para panel expandible de jefatura (encabezado + detalles + info del solicitante).
3. Integracion frontend para consumir ambos endpoints con el estado real del backend actual.

## 2. Estado Actual (Hallazgos)

### 2.1 Backend implementado hoy
Endpoints activos:
- `POST /api/justificaciones`
- `GET /api/justificaciones/mias`
- `GET /api/jefatura/justificaciones/pendientes`
- `PATCH /api/jefatura/justificaciones/{justificacionId}/resolver`

Observaciones:
- RF-02 y RF-03 estan implementados en `JustificacionService` + `JustificacionRepository`.
- Seguridad actual por headers mock en `HeaderUserContext`:
  - `X-User-Id` (int > 0)
  - `X-User-Role` (string)
- Roles validados en dominio:
  - Funcionario: `ROL_FUNC`
  - Jefatura: `ROL_JEFE`
  - RRHH (`ROL_RRHH`) aun no tiene reglas ni endpoints en servicios.

### 2.2 Frontend implementado hoy
- `app.js` ya consume API para funcionario/jefatura.
- Panel RRHH aun no tiene endpoint dedicado:
  - para `ROL_RRHH` muestra mensaje de "sin endpoint RRHH".
  - filtros RRHH se aplican solo en DOM (cliente), no en backend.
- Panel jefatura tiene fila expandible, pero usa placeholders (no consulta detalle real).

## 3. Alcance Funcional del Bloque 4

### 3.1 RRHH Global Query
Agregar endpoint para que RRHH consulte boletas de todo el sistema con filtros server-side por:
- funcionario (nombre parcial o cedula parcial)
- compania (`CNP` | `FANAL`)
- estado
- rango de fechas
- jefatura

### 3.2 Detalle de Boleta para Jefatura
Agregar endpoint para cargar detalle completo de una boleta en el expandible:
- encabezado
- solicitante (funcionario)
- aprobador (si existe)
- lineas de detalle

### 3.3 Frontend
- Panel RRHH: consumir endpoint global y abandonar fallback a endpoints de funcionario/jefatura.
- Panel Jefatura: cargar detalle real on-demand al abrir expandible.

## 4. Contratos API Propuestos (Exactos)

## 4.1 GET RRHH global
- Metodo y ruta:
  - `GET /api/rrhh/justificaciones`
- Controller propuesto:
  - `RrhhController`
- Autorizacion funcional:
  - solo `ROL_RRHH`

### Query params
Todos opcionales:
- `estadoId` (int)
- `compania` (string: `CNP` | `FANAL`)
- `funcionario` (string, busca por nombre o cedula)
- `jefaturaId` (int)
- `desde` (date/datetime)
- `hasta` (date/datetime)

Ejemplo:
`/api/rrhh/justificaciones?compania=CNP&estadoId=1&funcionario=maria&desde=2026-01-01&hasta=2026-12-31`

### Response 200
```json
[
  {
    "justificacionID": 120,
    "motivoGeneral": "Consulta medica",
    "estadoID": 1,
    "estadoDescripcion": "Pendiente Jefatura",
    "fechaCreacion": "2026-04-20T08:25:00",
    "cantidadDetalles": 2,
    "aprobadorID": null,
    "fechaAprobacion": null,
    "funcionarioID": 10,
    "funcionarioNombre": "Ana Perez",
    "funcionarioCedula": "1-1111-1111",
    "compania": "CNP",
    "jefaturaID": 20,
    "jefaturaNombre": "Maria Solano",
    "tipoPrincipal": "Marca Tardia"
  }
]
```

### Errores esperados
- `400`: filtros invalidos (fechas invertidas, compania invalida, texto demasiado largo)
- `401`: headers de identidad invalidos
- `403`: rol no autorizado
- `500`: error interno

## 4.2 GET detalle boleta para jefatura
- Metodo y ruta:
  - `GET /api/jefatura/justificaciones/{justificacionId:int}`
- Controller actual a extender:
  - `JefaturaController`
- Autorizacion funcional:
  - solo `ROL_JEFE`
  - solo boletas de subordinados directos

### Response 200
```json
{
  "encabezado": {
    "justificacionID": 120,
    "motivoGeneral": "Consulta medica",
    "estadoID": 1,
    "estadoDescripcion": "Pendiente Jefatura",
    "fechaCreacion": "2026-04-20T08:25:00",
    "aprobadorID": null,
    "fechaAprobacion": null
  },
  "solicitante": {
    "usuarioID": 10,
    "nombreCompleto": "Ana Perez",
    "cedula": "1-1111-1111",
    "correo": "ana@cnp.go.cr",
    "compania": "CNP",
    "unidadID": 4,
    "jefaturaID": 20
  },
  "aprobador": null,
  "detalles": [
    {
      "detalleID": 501,
      "tipoJustificacionID": 1,
      "tipoJustificacionDescripcion": "Marca Tardia",
      "fechaMarca": "2026-04-19",
      "observacionDetalle": "Atraso por cita medica"
    },
    {
      "detalleID": 502,
      "tipoJustificacionID": 2,
      "tipoJustificacionDescripcion": "Omision Marca de Entrada",
      "fechaMarca": "2026-04-18",
      "observacionDetalle": null
    }
  ]
}
```

### Errores esperados
- `401`: headers invalidos
- `403`: rol no autorizado o boleta fuera de subordinacion
- `404`: boleta no existe
- `500`: error interno

## 5. Cambios Requeridos por Capa

## 5.1 API (Controllers + Contracts)
Nuevos archivos sugeridos:
- `IntegradorMarcas.Api/Controllers/RrhhController.cs`
- `IntegradorMarcas.Api/Contracts/Responses/RrhhJustificacionResumenResponse.cs`
- `IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleCompletaResponse.cs`
- `IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleLineaResponse.cs`
- `IntegradorMarcas.Api/Contracts/Responses/UsuarioResumenResponse.cs`

Cambios en existentes:
- `JefaturaController.cs`:
  - agregar `GET {justificacionId:int}` para detalle.

## 5.2 Application (DTO + Service + Validation)
Agregar DTOs:
- `FiltroRrhhJustificacionesDto`
- `RrhhJustificacionResumenDto`
- `JustificacionCompletaDto`
- `JustificacionDetalleLineaDto`
- `UsuarioResumenDto`

Extender interfaz y servicio:
- `IJustificacionService`
  - `Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(UserContextInfo user, FiltroRrhhJustificacionesDto filtros, CancellationToken ct)`
  - `Task<JustificacionCompletaDto> GetDetalleJefaturaAsync(UserContextInfo user, int justificacionId, CancellationToken ct)`

Reglas de servicio:
- `ListRrhhAsync`: valida rol RRHH y valida filtros.
- `GetDetalleJefaturaAsync`: valida rol jefatura + subordinacion.

Validation nueva sugerida:
- clase `ConsultaJustificacionValidator` con:
  - `ValidateRangoFechas(desde, hasta)` -> `desde <= hasta`
  - `ValidateCompania(compania)` -> solo `CNP` o `FANAL`
  - `ValidateTextoBusqueda(funcionario)` -> max 150

## 5.3 Infrastructure (Repository + SQL)
Extender interfaz:
- `IJustificacionRepository`
  - `Task<IReadOnlyList<RrhhJustificacionResumenDto>> ListRrhhAsync(FiltroRrhhJustificacionesDto filtros, CancellationToken ct)`
  - `Task<JustificacionCompletaDto?> GetDetalleJefaturaAsync(int justificacionId, int jefaturaId, CancellationToken ct)`

Implementacion en `JustificacionRepository`:
- Query RRHH global con joins a `Usuarios`, `Estados`, `Justificaciones_Detalle`, `Cat_TiposJustificacion`.
- Query detalle por boleta con doble lectura:
  - lectura 1: encabezado + solicitante + aprobador
  - lectura 2: lista de detalles

## 6. Estrategia SQL Detallada

## 6.1 RRHH global con filtros
Tabla base: `dbo.Justificaciones_Encabezado je`

Joins:
- `dbo.Estados e` para descripcion estado
- `dbo.Usuarios u` (solicitante)
- `dbo.Usuarios j` (jefatura)
- `dbo.Justificaciones_Detalle jd`
- `dbo.Cat_TiposJustificacion tj`

Patron de filtros:
```sql
WHERE
    (@EstadoID IS NULL OR je.EstadoID = @EstadoID)
    AND (@Compania IS NULL OR u.Compania = @Compania)
    AND (@JefaturaID IS NULL OR u.JefaturaID = @JefaturaID)
    AND (@Desde IS NULL OR CAST(je.FechaCreacion AS DATE) >= CAST(@Desde AS DATE))
    AND (@Hasta IS NULL OR CAST(je.FechaCreacion AS DATE) <= CAST(@Hasta AS DATE))
    AND (
        @Funcionario IS NULL
        OR u.NombreCompleto LIKE CONCAT('%', @Funcionario, '%')
        OR u.Cedula LIKE CONCAT('%', @Funcionario, '%')
    )
```

Salida resumida recomendada:
- `COUNT(jd.DetalleID) AS CantidadDetalles`
- `MIN(tj.Descripcion) AS TipoPrincipal` (estable para grilla sin traer todo el detalle)

Order sugerido:
- `ORDER BY je.FechaCreacion DESC, je.JustificacionID DESC`

## 6.2 Detalle boleta jefatura
Validacion de pertenencia incluida en SQL:
```sql
WHERE
    je.JustificacionID = @JustificacionID
    AND u.JefaturaID = @JefaturaID
```

Consulta encabezado:
- `je` + `e` + `u` + `aprobador` (`ua` left join)

Consulta detalle:
- `jd` + `tj`
- `ORDER BY jd.FechaMarca DESC, jd.DetalleID DESC`

### Indices recomendados (aditivos)
Manteniendo indices actuales, agregar:
- `IX_Usuarios_JefaturaID` en `Usuarios(JefaturaID)`
- `IX_Usuarios_Compania` en `Usuarios(Compania)`
- `IX_JustifEnc_FechaCreacion` en `Justificaciones_Encabezado(FechaCreacion)`
- indice compuesto para consulta RRHH:
  - `IX_JustifEnc_Estado_Fecha` en `Justificaciones_Encabezado(EstadoID, FechaCreacion)`

## 7. Seguridad y Reglas por Rol

## 7.1 ROL_FUNC
Sin cambios en este bloque:
- crea boletas
- consulta propias

## 7.2 ROL_JEFE
- mantiene acceso a pendientes y resolver
- nuevo acceso a detalle por id, restringido a subordinados directos

## 7.3 ROL_RRHH
- nuevo acceso exclusivo a consulta global `GET /api/rrhh/justificaciones`
- no aprueba/rechaza boletas en este bloque

## 7.4 Reglas transversales
- headers `X-User-Id` y `X-User-Role` siguen siendo obligatorios.
- respuestas de negocio con `AppException`:
  - 400, 401, 403, 404, 409 segun caso.
- no exponer datos fuera de alcance jerarquico en endpoint de jefatura.

## 8. Mapeo Frontend (app.js + dashboard)

## 8.1 Panel Jefatura (expandible real)
Cambios propuestos en `renderJefaturaRequests` y `toggleDetail`:
- al abrir fila, llamar una sola vez a:
  - `GET /api/jefatura/justificaciones/{id}`
- cache local por `justificacionID` para evitar llamadas repetidas.
- poblar campos reales:
  - funcionarioNombre <- `solicitante.nombreCompleto`
  - observacionDetalle <- resumen concatenado de lineas
  - tipoPrincipal <- primer item en `detalles`

## 8.2 Panel RRHH
Cambios propuestos en `renderRRHHTable`:
- cuando role sea `ROL_RRHH`, consumir siempre:
  - `GET /api/rrhh/justificaciones` con query params desde filtros UI.
- eliminar mensaje de "sin endpoint RRHH".
- para roles no RRHH mantener bloqueo de vista como hoy.

Mapeo columnas RRHH:
- Funcionario <- `funcionarioNombre`
- Compania <- `compania`
- Motivo <- `motivoGeneral`
- Tipo <- `tipoPrincipal`
- Fecha <- `fechaCreacion`
- Estado <- `estadoID/estadoDescripcion`
- Resolucion <- `aprobadorID + fechaAprobacion`

## 8.3 Filtros RRHH
`applyRRHHFilter` deja de filtrar DOM y pasa a:
- construir query string
- invocar `renderRRHHTable` (server-side filtering)

`resetRRHHFilter`:
- limpia inputs
- recarga `GET /api/rrhh/justificaciones` sin filtros

## 9. Compatibilidad y Riesgos
- Riesgo de N+1 en detalle jefatura si se hace eager de todos los detalles en lista:
  - mitigacion: carga lazy por fila expandida.
- Riesgo de respuesta grande en RRHH:
  - recomendacion siguiente bloque: paginacion (`page`, `pageSize`).
- Riesgo de discrepancia de nombres JSON:
  - mantener convencion actual de propiedades .NET en PascalCase para que frontend reciba camelCase esperado (`justificacionID`, etc).

## 10. Pruebas Minimas de Aceptacion
1. RRHH autenticado (`ROL_RRHH`) consulta global sin filtros y recibe registros.
2. RRHH filtra por compania, estado y rango de fechas, resultados consistentes.
3. Usuario no RRHH en endpoint RRHH recibe `403`.
4. Jefatura abre detalle de boleta subordinada y obtiene encabezado + detalles.
5. Jefatura abre boleta fuera de subordinacion y recibe `403`.
6. Jefatura sobre boleta inexistente recibe `404`.
7. Frontend jefatura muestra expandible con datos reales.
8. Frontend RRHH llena tabla con endpoint global y filtros server-side.

## 11. Orden de Implementacion Recomendado
1. DTOs + interfaces (Application).
2. SQL constants + Repository implementation.
3. Service rules por rol y validaciones.
4. Controllers y contracts API.
5. Integracion frontend (RRHH + detalle jefatura).
6. Pruebas unitarias/integracion basicas.
