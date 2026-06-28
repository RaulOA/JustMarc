# Spec: Fix error al rechazar boleta en Panel Jefatura

## Objetivo
Corregir el comportamiento donde, al rechazar una boleta desde Panel Jefatura, la UI muestra un estado/mensaje de error aunque la resolución sea válida.

## Hallazgos

### 1) Ruta backend PATCH y payload
- Endpoint: `PATCH /api/jefatura/justificaciones/{justificacionId}/resolver`.
- Controlador: `JefaturaController.Resolver` mapea body a `ResolverJustificacionDto` con campos `Accion` y `Comentario`.
- Contrato request (`ResolverJustificacionRequest`):
  - `Accion: string`
  - `Comentario: string?`
- Frontend envía:
  - `accion: 'APROBAR' | 'RECHAZAR'`
  - `comentario: ''`
- Esto es compatible con model binding JSON de ASP.NET Core (case-insensitive por defecto).

Referencias:
- `backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs`
- `backend/src/IntegradorMarcas.Api/Contracts/Requests/ResolverJustificacionRequest.cs`
- `app.js`

### 2) Reglas de validación
- `ValidateAccion` acepta únicamente `APROBAR` o `RECHAZAR`.
- `NormalizeComentarioResolucion` permite `null`/vacío y solo valida longitud máxima (500).
- No existe regla que obligue comentario para rechazo.

Referencias:
- `backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs`
- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`

### 3) Causa raíz exacta en UI
En `approveRequest(boletaId, action)`, luego de un `PATCH` exitoso, la notificación se renderiza con severidad:
- `success` si `action === 'approve'`
- `error` para cualquier otro caso (incluye `reject`)

Código actual relevante:
- `showNotice('j-notice', action === 'approve' ? 'success' : 'error', ...)`

Resultado:
- Rechazar se comunica visualmente como error, aunque la operación haya sido correcta (204 NoContent esperado).

Referencia:
- `app.js` (función `approveRequest`)

## Fix mínimo y seguro propuesto

### Cambio puntual
En la llamada a `showNotice` de `approveRequest`, reemplazar la severidad de rechazo:
- Opción recomendada: usar `success` para ambos casos (aprobar/rechazar), manteniendo texto diferencial del mensaje.
- Alternativa: usar `warning` para rechazo si el componente soporta ese nivel semántico sin estilos inconsistentes.

Propuesta concreta (mínima):
- Cambiar:
  - `action === 'approve' ? 'success' : 'error'`
- Por:
  - `'success'`

Con esto:
- No se altera payload ni contrato API.
- No se toca lógica de dominio ni transiciones de estado.
- Se elimina falso positivo visual de error.

## Riesgo
- Muy bajo. Solo cambia severidad visual de notificación en frontend.

## Validación posterior al fix
1. En Panel Jefatura, rechazar boleta pendiente.
2. Verificar que:
   - El `PATCH` responde 204.
   - Mensaje mostrado sea de éxito (no error).
   - La boleta desaparezca de pendientes tras recarga de tabla.
3. Repetir para aprobar, confirmando que no hay regresión.

## Alcance no incluido
- No se introduce regla de comentario obligatorio al rechazar.
- No se modifican validaciones backend ni SQL de resolución.
