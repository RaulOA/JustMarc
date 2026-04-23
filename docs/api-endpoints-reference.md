# API Endpoints Reference

## 1. Proposito
Centralizar los contratos HTTP reales de la API implementada, incluyendo roles, headers, request/response y errores frecuentes.

## 2. Alcance
Incluye todos los endpoints expuestos por los controladores actuales y /health.

## 3. Fuente de verdad
- backend/src/IntegradorMarcas.Api/Controllers/*.cs
- backend/src/IntegradorMarcas.Api/Contracts/Requests/*.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/*.cs
- backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs
- backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs
- backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs

## 4. Headers obligatorios (MVP)
- X-User-Id: entero > 0.
- X-User-Role: ROL_FUNC, ROL_JEFE, ROL_RRHH.

Alias aceptados por la logica de roles:
- Funcionario: FUNCIONARIO o 1.
- Jefatura: JEFATURA o 2.
- RRHH: RRHH o 3.

## 5. Matriz endpoint-metodo-rol
| Endpoint | Metodo | Rol requerido | Exito |
|---|---|---|---|
| /api/justificaciones | POST | Funcionario | 201 |
| /api/justificaciones/mias | GET | Funcionario | 200 |
| /api/jefatura/justificaciones/pendientes | GET | Jefatura | 200 |
| /api/jefatura/justificaciones/{justificacionId} | GET | Jefatura | 200 |
| /api/jefatura/justificaciones/{justificacionId}/resolver | PATCH | Jefatura | 204 |
| /api/rrhh/justificaciones | GET | RRHH | 200 |
| /health | GET | Tecnico | 200 |

## 6. Contratos por endpoint

### 6.1 POST /api/justificaciones
Rol: Funcionario.

Request JSON:
```json
{
  "motivoGeneral": "Justificacion general",
  "detalles": [
    {
      "tipoJustificacionID": 1,
      "fechaMarca": "2026-04-23T00:00:00",
      "observacionDetalle": "Opcional"
    }
  ]
}
```

Response 201:
```json
{
  "justificacionID": 101,
  "estadoID": 1,
  "estadoDescripcion": "Pendiente Jefatura"
}
```

### 6.2 GET /api/justificaciones/mias
Rol: Funcionario.

Query params opcionales:
- estadoId (int)
- desde (date/datetime)
- hasta (date/datetime)

Response 200:
```json
[
  {
    "justificacionID": 101,
    "motivoGeneral": "Justificacion general",
    "comentarioResolucion": null,
    "estadoID": 1,
    "estadoDescripcion": "Pendiente Jefatura",
    "fechaCreacion": "2026-04-23T15:10:00",
    "cantidadDetalles": 1,
    "aprobadorID": null,
    "fechaAprobacion": null
  }
]
```

### 6.3 GET /api/jefatura/justificaciones/pendientes
Rol: Jefatura.

Query params opcionales:
- desde (date/datetime)
- hasta (date/datetime)

Response 200: mismo shape de JustificacionResumenResponse.

### 6.4 GET /api/jefatura/justificaciones/{justificacionId}
Rol: Jefatura.

Response 200:
```json
{
  "encabezado": {
    "justificacionID": 101,
    "motivoGeneral": "Justificacion general",
    "comentarioResolucion": null,
    "estadoID": 1,
    "estadoDescripcion": "Pendiente Jefatura",
    "fechaCreacion": "2026-04-23T15:10:00",
    "cantidadDetalles": 2,
    "aprobadorID": null,
    "fechaAprobacion": null
  },
  "solicitante": {
    "usuarioID": 10,
    "nombreCompleto": "Funcionario Demo",
    "cedula": "...",
    "correo": "...",
    "compania": "CNP",
    "unidadID": 120,
    "jefaturaID": 20
  },
  "aprobador": null,
  "detalles": [
    {
      "detalleID": 1,
      "tipoJustificacionID": 1,
      "tipoJustificacionDescripcion": "Marca Tardia",
      "fechaMarca": "2026-04-22T00:00:00",
      "observacionDetalle": "..."
    }
  ]
}
```

### 6.5 PATCH /api/jefatura/justificaciones/{justificacionId}/resolver
Rol: Jefatura.

Request JSON:
```json
{
  "accion": "APROBAR",
  "comentario": "Opcional"
}
```

Response 204 sin cuerpo.

### 6.6 GET /api/rrhh/justificaciones
Rol: RRHH.

Query params opcionales:
- funcionario (string)
- estadoId (int)
- compania (CNP/FANAL)
- fechaDesde (date/datetime)
- fechaHasta (date/datetime)

Response 200:
```json
[
  {
    "justificacionID": 101,
    "motivoGeneral": "Justificacion general",
    "comentarioResolucion": null,
    "estadoID": 1,
    "estadoDescripcion": "Pendiente Jefatura",
    "fechaCreacion": "2026-04-23T15:10:00",
    "cantidadDetalles": 1,
    "aprobadorID": null,
    "fechaAprobacion": null,
    "funcionarioID": 10,
    "funcionarioNombre": "Funcionario Demo",
    "funcionarioCedula": "...",
    "compania": "CNP",
    "jefaturaID": 20,
    "jefaturaNombre": "Jefatura Demo",
    "tipoPrincipal": "Marca Tardia"
  }
]
```

### 6.7 GET /health
Response 200:
```json
{
  "status": "ok",
  "utc": "2026-04-23T15:20:00.0000000Z"
}
```

## 7. Errores y codigos esperados
Formato: ProblemDetails (RFC7807).

Codigos frecuentes:
- 400: validaciones de payload/filtros.
- 401: headers de identidad faltantes o invalidos.
- 403: rol no autorizado o boleta fuera de alcance de jefatura.
- 404: boleta inexistente.
- 409: boleta ya resuelta o concurrencia en resolucion.
- 499: solicitud cancelada por el cliente.
- 500: error no controlado.

En errores gestionados por el segundo handler global se agrega:
- Header X-Correlation-Id.
- extension correlationId en ProblemDetails.

## 8. Ejemplos curl

Crear boleta:
```bash
curl -X POST "http://localhost:5093/api/justificaciones" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 10" \
  -H "X-User-Role: ROL_FUNC" \
  -d '{
    "motivoGeneral": "Atraso por incidente vial",
    "detalles": [
      {
        "tipoJustificacionID": 1,
        "fechaMarca": "2026-04-23T00:00:00",
        "observacionDetalle": "Ingreso 08:22"
      }
    ]
  }'
```

Pendientes jefatura:
```bash
curl "http://localhost:5093/api/jefatura/justificaciones/pendientes" \
  -H "X-User-Id: 20" \
  -H "X-User-Role: ROL_JEFE"
```

Resolver boleta:
```bash
curl -X PATCH "http://localhost:5093/api/jefatura/justificaciones/101/resolver" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 20" \
  -H "X-User-Role: ROL_JEFE" \
  -d '{"accion":"RECHAZAR","comentario":"No coincide con evidencia"}'
```

Consulta RRHH:
```bash
curl "http://localhost:5093/api/rrhh/justificaciones?compania=CNP&estadoId=1" \
  -H "X-User-Id: 30" \
  -H "X-User-Role: ROL_RRHH"
```

## 9. Checklist de validacion
- Endpoints y rutas coinciden con controladores reales.
- Campos request/response coinciden con Contracts.
- Reglas de rol y errores coinciden con Service/Validator.
- Ejemplos usan headers requeridos.

## 10. Historial de cambios
- 2026-04-23: Documento creado y validado con el codigo actual.
