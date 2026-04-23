# Plan de Correcciones por Validation Review

Fecha: 2026-04-23
Fuente: docs/SubAgent docs/validation-review.md

## Resumen Ejecutivo (priorizado)

1. Persistir comentario de resolucion en backend + BD para evitar perdida silenciosa de datos (F-01).
2. Validar existencia de TipoJustificacionID antes de insertar para devolver 400 semantico (F-02).
3. Unificar validacion de rango de fechas en ListMineAsync para consistencia entre listados (F-03).
4. Alinear validaciones declarativas de formularios frontend con reglas backend (F-06).
5. Consolidar funciones duplicadas/sombreadas parseApiError y showNotice sin romper toasts (F-05).

---

## 1) Persistir comentario de resolucion

### Archivos a modificar
- backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs
- backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs
- backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs
- backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs
- docs/db/001_init_integra_cnp.sql
- docs/db/008_add_comentario_resolucion.sql (nuevo)
- Opcional si se expone en respuestas: backend/src/IntegradorMarcas.Application/DTOs/JustificacionResumenDto.cs, backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionResumenResponse.cs

### Cambios de comportamiento exactos
- El endpoint PATCH /api/jefatura/justificaciones/{id}/resolver debe persistir request.Comentario en la boleta resuelta.
- Se agrega validacion semantica de comentario en backend:
  - Maximo 500 caracteres (coherente con MotivoGeneral).
  - Si viene null, persistir null.
  - Si viene whitespace, normalizar a null (trim + null).
- SQL ResolverPendiente debe actualizar columna ComentarioResolucion junto con EstadoID, AprobadorID y FechaAprobacion.
- Si se decide exponer en listados/detalle, agregar mapeo de salida; si no, mantenerlo solo para trazabilidad interna.

### Notas de compatibilidad
- Compatibilidad hacia atras del contrato request: Comentario ya existia, solo cambia de "aceptado y descartado" a "aceptado y persistido".
- No rompe clientes existentes que envian comentario vacio o null.
- Riesgo principal: esquema BD sin columna en ambientes existentes; mitigado con migracion incremental (script 008).

### Verificacion
- Backend unit/integration:
  - Resolver con comentario corto y verificar UPDATE en BD.
  - Resolver con comentario null y verificar valor null.
  - Resolver con comentario > 500 y esperar 400.
- API manual:
  - PATCH resolver + GET detalle/listado (si se expone) para confirmar persistencia.
- SQL:
  - SELECT ComentarioResolucion desde Justificaciones_Encabezado para boleta resuelta.

### SQL / migracion de datos
- Nuevo script incremental: docs/db/008_add_comentario_resolucion.sql
  - IF COL_LENGTH('dbo.Justificaciones_Encabezado','ComentarioResolucion') IS NULL
  - ALTER TABLE ... ADD ComentarioResolucion VARCHAR(500) NULL
- Actualizar script base 001 para instalaciones nuevas con la columna desde origen.
- No requiere backfill (campo nuevo nullable).

---

## 2) Validar existencia de TipoJustificacionID antes de insertar detalle

### Archivos a modificar
- backend/src/IntegradorMarcas.Application/Interfaces/IJustificacionRepository.cs
- backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs
- backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs
- backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs (mensaje/regla complementaria)

### Cambios de comportamiento exactos
- Antes de crear boleta, backend valida que todos los TipoJustificacionID solicitados existan en dbo.Cat_TiposJustificacion.
- Si existe al menos un ID invalido, responder 400 con mensaje de dominio (ejemplo: "Uno o mas TipoJustificacionID no existen en catalogo.").
- No se debe depender del FK como primer validador para este caso de negocio.

### Implementacion sugerida
- Agregar en repositorio metodo tipo:
  - Task<IReadOnlyCollection<int>> GetExistingTipoJustificacionIdsAsync(IEnumerable<int> ids, CancellationToken ct)
- En servicio CreateAsync:
  - extraer IDs distintos del request.
  - consultar existentes.
  - calcular faltantes y lanzar AppException(400) si faltan.
  - solo luego llamar CreateAsync.

### Notas de compatibilidad
- Comportamiento pasa de posible error tecnico (por FK) a error semantico 400.
- No rompe payloads validos.
- Mejora trazabilidad y UX de cliente.

### Verificacion
- Caso feliz: IDs existentes -> crea boleta normalmente.
- Caso invalido: incluir ID inexistente -> 400, mensaje claro, sin insercion parcial.
- Confirmar que la transaccion no inicia inserciones cuando falla validacion previa.

### SQL / migracion
- Sin cambios de esquema.
- FK existente se mantiene como red de seguridad secundaria.

---

## 3) Validar rango de fechas invertido en ListMineAsync

### Archivos a modificar
- backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs

### Cambios de comportamiento exactos
- En ListMineAsync, aplicar JustificacionValidator.ValidateRangoFechas(filtros.Desde, filtros.Hasta) antes de consultar repositorio.
- Si desde > hasta, retornar 400 con el mismo mensaje usado por Jefatura/RRHH.

### Notas de compatibilidad
- Cambio funcional esperado: escenarios que hoy devuelven lista vacia pasaran a 400 por input invalido.
- Se alinea contrato con otros endpoints de listado, reduciendo inconsistencias.

### Verificacion
- GET /api/justificaciones/mias?desde=2026-04-10&hasta=2026-04-01 -> 400.
- GET con rango valido o sin rango -> comportamiento previo intacto.

### SQL / migracion
- No aplica.

---

## 4) Alinear validacion declarativa frontend con reglas backend

### Archivos a modificar
- dashboard.html
- (Opcional refuerzo UX) app.js

### Campos y reglas objetivo (alineadas con backend actual)
- f-motivo (MotivoGeneral): required, maxlength=500
- f-d-tipo (Tipo de Justificacion): required
- f-d-fecha (FechaMarca): required
- f-d-observacion (ObservacionDetalle): maxlength=250
- rrhh-fn (texto de busqueda funcionario): maxlength=150
- rrhh-company: mantener opciones CNP/FANAL (ya alineado)

### Cambios de comportamiento exactos
- El navegador bloqueara envios incompletos o fuera de longitud maxima antes de llamar API.
- Se reduce volumen de requests invalidos y mejora feedback temprano al usuario.
- Mantener validaciones JS/backend existentes como segunda barrera (no confiar solo en HTML).

### Notas de compatibilidad
- No rompe APIs.
- Puede cambiar UX en casos donde antes permitia teclear mas de los limites y luego fallaba en backend.

### Verificacion
- Intentar registrar motivo > 500 y observar bloqueo UI.
- Intentar observacion > 250 y validar limite.
- En RRHH, intentar texto > 150 y validar limite.
- Confirmar que los mensajes de error backend siguen apareciendo cuando corresponda.

### SQL / migracion
- No aplica.

---

## 5) Consolidar duplicidad/sombreado parseApiError y showNotice en app.js

### Archivos a modificar
- app.js

### Cambios de comportamiento exactos
- Dejar una sola implementacion de parseApiError:
  - conservar extraccion de detail/title/message.
  - conservar correlationId desde payload o header X-Correlation-Id.
- Dejar una sola implementacion de showNotice como wrapper estable hacia toast(...).
- Eliminar implementaciones duplicadas/sombreadas para evitar ambiguedad de orden de declaracion.
- Mantener firmas actuales para no romper llamadas existentes:
  - parseApiError(response)
  - showNotice(targetId, type, msg)

### Notas de compatibilidad
- Compatibilidad binaria/funcional preservada para todos los call sites actuales.
- Cambia internamente la fuente de verdad, no el contrato.
- Riesgo bajo si se conserva el mismo mapping de type->toastType y mismos mensajes.

### Verificacion
- Flujo funcionario: errores de create muestran toast de error.
- Flujo jefatura: aprobar/rechazar muestran toasts success/error segun accion.
- Flujo RRHH: errores de carga muestran toast.
- Errores 5xx/timeout desde apiFetch muestran toast con correlationId cuando exista.
- Validar que no quedan definiciones duplicadas via busqueda textual de funciones.

### SQL / migracion
- No aplica.

---

## Orden propuesto de ejecucion tecnica

1. BD + backend para comentario de resolucion (item 1) para cerrar riesgo alto de trazabilidad.
2. Validacion semantica de TipoJustificacionID (item 2) para estandarizar 400 de negocio.
3. Validacion de rango en ListMineAsync (item 3) para consistencia de contrato.
4. Consolidacion app.js (item 5) para eliminar deuda tecnica y riesgo de regresion UI.
5. Ajustes declarativos en dashboard.html (item 4) para reforzar UX y prevenir input invalido.

## Criterios de aceptacion globales

- Ningun cambio rompe endpoints ni payloads validos existentes.
- Casos invalidos ahora fallan como 400 semantico cuando aplica (no 500 tecnico).
- Comentario de resolucion queda persistido y consultable en BD.
- Frontend conserva toasts actuales y elimina funciones duplicadas.
- Se documenta y ejecuta migracion SQL incremental en ambientes existentes.
