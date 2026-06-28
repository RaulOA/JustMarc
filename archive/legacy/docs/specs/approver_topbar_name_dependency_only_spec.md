# Spec: Topbar Aprobador Solo Nombre y Dependencia

## Objetivo

Ajustar la visualizacion de `aprobador actual` en el topbar para mostrar solo:

- nombre del aprobador
- dependencia/unidad a la que pertenece

Sin cambiar la logica de negocio que resuelve el aprobador vigente y con el menor cambio posible en backend y frontend.

## Hallazgos

### 1. Endpoint existente y shape actual

Ya existe el endpoint `GET /api/justificaciones/aprobador-actual`.

Flujo actual:

- Controller: `backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs`
- Service: `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`
- Repository: `backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs`
- SQL: `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`

Respuesta actual:

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

Observacion: la respuesta incluye `UnidadID`, pero no incluye `UnidadNombre` para el aprobador.

### 2. DTOs y contracts

`UsuarioResumenDto` y `UsuarioResumenResponse` ya tienen el campo `UnidadNombre`.

Eso significa que el modelo compartido ya soporta exponer el nombre de unidad/dependencia y no hace falta crear DTOs nuevos.

Problema actual:

- En `GetCurrentApproverAsync`, el repository llena `UnidadId`, pero no `UnidadNombre`.
- En `JustificacionesController.GetCurrentApprover`, el mapping de `Aprobador` tampoco incluye `UnidadNombre`.

Conclusion: el gap no es de modelo sino de llenado y exposicion del campo.

### 3. Fuente SQL actual

La query `GetCurrentApproverBySolicitante` resuelve el aprobador vigente usando:

- `dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioID, GETDATE())`
- prioridad de `Delegacion` sobre `Jerarquia`
- join a `RecursosHumanos.Usuario` para datos del aprobador

Hoy selecciona:

- `AprobadorUsuarioID`
- `AprobadorNombreCompleto`
- `AprobadorCedula`
- `AprobadorCorreo`
- `AprobadorCompania`
- `AprobadorUnidadID`
- `AprobadorJefaturaID`

No hace join a `dbo.Estructuras_Organizacionales` para traer el nombre de la unidad del aprobador.

Referencia util: otra query del mismo repositorio ya usa ese patron para solicitante:

- `LEFT JOIN dbo.Estructuras_Organizacionales eo ON eo.EstructuraOrganizacionalID = u.UnidadID`
- `eo.Nombre AS SolicitanteUnidadNombre`

Conclusion: la dependencia/unidad legible no esta disponible hoy en la respuesta del endpoint. Backend debe exponerla.

### 4. Render frontend actual

El topbar ya tiene punto de montaje en `dashboard.html` con `id="current-approver"`.

La funcion `renderCurrentApproverTopbar()` en `app.js` ya consume el endpoint y hoy renderiza:

- `Aprobador actual: <Nombre> (Delegación de <Delegante>)`
- `Aprobador actual: <Nombre> (Jerarquía)`
- o solo `Aprobador actual: <Nombre>`

El frontend no tiene ninguna fuente local para resolver `UnidadID -> UnidadNombre`.

Conclusion: no es viable resolver la dependencia solo en frontend con el estado actual.

## Cambio minimo necesario

### Backend

Extender el endpoint existente `GET /api/justificaciones/aprobador-actual` para incluir `Aprobador.UnidadNombre`.

Cambios minimos:

1. SQL
   - En `GetCurrentApproverBySolicitante`, agregar join a `dbo.Estructuras_Organizacionales` usando `aprobador.UnidadID`.
   - Seleccionar `eo.Nombre AS AprobadorUnidadNombre`.

2. Repository
   - Agregar `AprobadorUnidadNombre` a `CurrentApproverRow`.
   - Mapear ese valor a `UsuarioResumenDto.UnidadNombre`.

3. API Controller
   - Incluir `UnidadNombre = result.Aprobador.UnidadNombre` en `CurrentApproverResponse`.

No se requiere:

- cambiar el endpoint
- cambiar reglas de autorizacion
- cambiar la logica de `fn_AprobadoresVigentesPorSolicitante`
- crear DTOs nuevos

### Frontend

Modificar `renderCurrentApproverTopbar()` para ignorar `origen` y `deleganteNombre` al mostrar texto y usar solo:

```text
Aprobador actual: <Nombre> - <Dependencia>
```

Fallback sugerido:

- si llega nombre pero no unidad: `Aprobador actual: <Nombre>`
- si no llega aprobador: `Aprobador actual: No definido`
- si falla la llamada: `Aprobador actual: No disponible`

## Razonamiento de minimo impacto

- La resolucion del aprobador vigente ya existe y funciona.
- El contrato ya tiene un campo para `UnidadNombre`.
- El frontend ya consume el endpoint correcto.
- Solo falta transportar el nombre de dependencia desde SQL hasta el response y cambiar una linea de render.

Este enfoque evita introducir un endpoint nuevo o duplicar logica de negocio.

## Archivos a tocar en una implementacion posterior

- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs`
- `backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs`
- `app.js`

## Validacion recomendada

1. Llamar `GET /api/justificaciones/aprobador-actual` y confirmar que `aprobador.unidadNombre` llegue poblado.
2. Verificar que el topbar muestre solo nombre y dependencia.
3. Verificar que delegacion y jerarquia sigan resolviendose igual a nivel de backend, aunque ya no se muestren en el texto.

## Conclusion

La dependencia/unidad legible no esta disponible hoy en el topbar porque el backend solo expone `UnidadID` para el aprobador actual. El cambio minimo correcto es exponer `Aprobador.UnidadNombre` desde el endpoint existente y simplificar el render frontend para mostrar solo nombre + dependencia.