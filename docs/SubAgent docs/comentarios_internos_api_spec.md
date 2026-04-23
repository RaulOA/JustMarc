# Especificacion: comentarios internos utiles en IntegradorMarcas.Api

Fecha: 2026-04-23
Scope analizado: backend/src/IntegradorMarcas.Api/Program.cs, Controllers, Contracts, Security

## Resumen ejecutivo

La recomendacion general es comentar poco y con alta intencion.

- Si conviene comentar en puntos donde hay reglas de negocio implicitas, decisiones de infraestructura, mapeos sensibles o manejo de errores no trivial.
- No conviene comentar en mapeos DTO mecanicos, constructores triviales y propiedades autoexplicativas.
- Tipo dominante recomendado:
  - XML docs en acciones publicas de controllers y en contratos con semantica ambigua.
  - Comentarios de bloque cortos en Program.cs para pipeline, excepciones y decisiones de despliegue.
  - Inline solo cuando una linea implementa una excepcion a la regla o comportamiento no intuitivo.

Hallazgo relevante: Program.cs contiene dos bloques UseExceptionHandler. Antes de comentar, conviene consolidar en un unico bloque para evitar que la documentacion interna quede inconsistente.

## 1) Donde si conviene comentar

### A. Program.cs

Archivo: backend/src/IntegradorMarcas.Api/Program.cs

Si conviene comentar en:

- Validacion de ConnectionStrings:IntegraCnp fuera de Development.
  - Justificacion: politica de entorno y fail-fast en arranque.
- Configuracion CORS LocalFrontend con SetIsOriginAllowed(_ => true).
  - Justificacion: trade-off de seguridad y alcance temporal.
- Bloque global de excepciones.
  - Justificacion: mapeo de excepciones a status code, correlationId y logging condicional de stacktrace.
- Endpoint /health.
  - Justificacion: contrato operativo para monitoreo (status + utc).

Tipo recomendado:

- Comentarios de bloque para secciones del pipeline y de excepciones.
- Inline puntual para decisiones de seguridad/operacion.

### B. Security/HeaderUserContext.cs

Archivo: backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs

Si conviene comentar en:

- Resolucion de nombres de headers desde configuracion con defaults.
- Validacion de X-User-Id y X-User-Role.
- Razon de devolver 401 via AppException cuando faltan headers.

Tipo recomendado:

- XML docs en clase y metodo GetCurrent.
- Inline corto en validaciones compuestas (TryGetValue + parse + rango).

### C. Controllers (acciones publicas)

Archivos:

- backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs
- backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs
- backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs
- backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs

Si conviene comentar en:

- XML docs de cada endpoint para dejar explicito:
  - rol esperado,
  - filtros relevantes,
  - efecto de negocio,
  - codigos de respuesta relevantes.
- Acciones con reglas no obvias:
  - Resolver en Jefatura (accion + comentario y cambio de estado).
  - Toggle de estado en jerarquias/delegaciones (semantica de habilitar/deshabilitar).
  - Listados con filtros compuestos en RRHH.

Tipo recomendado:

- XML docs en metodos de accion.
- Evitar bloque/inline dentro de mapeos Select salvo excepcion.

### D. Contracts con semantica ambigua

Requests donde si conviene documentar:

- backend/src/IntegradorMarcas.Api/Contracts/Requests/ResolverJustificacionRequest.cs
  - Accion: valores permitidos y semantica.
  - Comentario: cuando es obligatorio.
- backend/src/IntegradorMarcas.Api/Contracts/Requests/ToggleEstadoRegistroRequest.cs
  - EstadoRegistroID: catalogo esperado y significado.
- backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateDelegacionRequest.cs
  - JerarquiaAprobacionID nullable y reglas cuando es null.
  - VigenciaDesde/VigenciaHasta: convencion de inclusividad y zona horaria.

Responses donde si conviene documentar:

- backend/src/IntegradorMarcas.Api/Contracts/Responses/RrhhJustificacionResumenResponse.cs
  - TipoPrincipal, JefaturaNombre y campos opcionales.
- backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleCompletaResponse.cs
  - Diferencia entre Solicitante y Aprobador nullable.

Tipo recomendado:

- XML docs por propiedad solo en campos ambiguos.
- No documentar cada propiedad obvia.

## 2) Donde no conviene comentar

No conviene comentar en:

- Constructores triviales de controllers y clases DTO.
- Bloques de mapeo 1:1 evidentes (x.Prop = y.Prop).
- Propiedades de contratos cuyo nombre ya explica completamente su uso.
- Declaraciones de DI autoexplicativas sin condicion especial.

Archivos con minimo valor de comentario interno adicional (salvo XML de contrato puntual):

- backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateJustificacionRequest.cs
- backend/src/IntegradorMarcas.Api/Contracts/Requests/JustificacionDetalleRequest.cs
- backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateJerarquiaRequest.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/CreateJustificacionResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionResumenResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleLineaResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminJerarquiaResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminDelegacionResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/UsuarioResumenResponse.cs

## 3) Tipo de comentario recomendado

### Regla base

- XML docs: para API publica (acciones de controllers, contratos ambiguos).
- Comentario de bloque: para decisiones de arquitectura/pipeline.
- Inline: solo para excepciones a la intuicion o notas de seguridad/operacion.

### Plantillas sugeridas

XML doc para endpoint:

/// <summary>
/// Lista justificaciones pendientes de aprobacion para la jefatura autenticada.
/// </summary>
/// <param name="desde">Fecha inicial opcional del rango de filtro.</param>
/// <param name="hasta">Fecha final opcional del rango de filtro.</param>
/// <remarks>
/// Requiere cabeceras de identidad y rol. Devuelve solo casos visibles para la jefatura actual.
/// </remarks>

Comentario de bloque para Program:

// Manejo global de errores: traduce excepciones de dominio a HTTP,
// agrega correlationId y persiste trazas de error para diagnostico.

Inline puntual:

// 499 indica cancelacion iniciada por el cliente (patron operativo, no RFC oficial).

## 4) Ejemplos concretos por archivo/simbolo

### Program.cs

Archivo: backend/src/IntegradorMarcas.Api/Program.cs

- Simbolo: validacion de IntegraCnp en entorno no Development.
  - Recomendado: bloque corto explicando fail-fast por dependencia critica.
- Simbolo: politica CORS LocalFrontend.
  - Recomendado: inline sobre naturaleza temporal/entorno controlado de AllowAnyOrigin equivalente.
- Simbolo: bloque UseExceptionHandler.
  - Recomendado: bloque con matriz de mapeo AppException/KeyNotFound/OperationCanceled/otros.
- Simbolo: respuesta Problem con extension correlationId.
  - Recomendado: inline sobre soporte y trazabilidad.

### Security/HeaderUserContext.cs

Archivo: backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs

- Simbolo: clase HeaderUserContext.
  - Recomendado: XML summary indicando estrategia de autenticacion basada en headers de gateway.
- Simbolo: metodo GetCurrent.
  - Recomendado: XML remarks indicando que falla con 401 al faltar identidad/rol.
- Simbolo: validacion userId <= 0.
  - Recomendado: inline sobre criterio de identidad valida.

### Controllers/JustificacionesController.cs

Archivo: backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs

- Simbolo: Create.
  - Recomendado: XML docs sobre estado inicial asignado (Pendiente Jefatura) y CreatedAtAction.
- Simbolo: ListMine.
  - Recomendado: XML docs sobre filtros opcionales y alcance del usuario autenticado.
- Simbolo: mapeos request->dto y dto->response.
  - Recomendado: no comentar salvo que aparezca transformacion no 1:1.

### Controllers/JefaturaController.cs

Archivo: backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs

- Simbolo: ListPendientes.
  - Recomendado: XML docs sobre origen del universo de datos (solo pendientes de la jefatura).
- Simbolo: GetDetalle.
  - Recomendado: XML docs sobre composicion Encabezado/Solicitante/Aprobador/Detalles.
- Simbolo: Resolver.
  - Recomendado: XML docs sobre acciones esperadas y efecto de estado.

### Controllers/RrhhController.cs

Archivo: backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs

- Simbolo: List.
  - Recomendado: XML docs de filtros combinables y semantica de rangos de fecha.
- Simbolo: campos de response RRHH.
  - Recomendado: no inline dentro del Select; documentar en contrato si hace falta.

### Controllers/AdminAprobacionesController.cs

Archivo: backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs

- Simbolo: ListJerarquias/ListDelegaciones.
  - Recomendado: XML docs para filtros administrativos.
- Simbolo: ToggleJerarquiaEstado/ToggleDelegacionEstado.
  - Recomendado: XML docs sobre estado logico (habilitado/inhabilitado) y alcance.
- Simbolo: CreateJerarquia/CreateDelegacion.
  - Recomendado: XML docs sobre validaciones de vigencia y referencias opcionales.

### Contracts Requests

Archivo: backend/src/IntegradorMarcas.Api/Contracts/Requests/ResolverJustificacionRequest.cs

- Simbolo: Accion.
  - Recomendado: XML doc con valores permitidos (por ejemplo APROBAR/RECHAZAR, segun dominio real).
- Simbolo: Comentario.
  - Recomendado: XML doc indicando obligatoriedad condicional (si aplica).

Archivo: backend/src/IntegradorMarcas.Api/Contracts/Requests/ToggleEstadoRegistroRequest.cs

- Simbolo: EstadoRegistroID.
  - Recomendado: XML doc enlazando catalogo de estados administrativos.

Archivo: backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateDelegacionRequest.cs

- Simbolo: JerarquiaAprobacionID.
  - Recomendado: XML doc sobre caso null (delegacion directa vs asociada a jerarquia).
- Simbolo: VigenciaDesde/VigenciaHasta.
  - Recomendado: XML doc sobre zona horaria y limites inclusivos.

### Contracts Responses

Archivo: backend/src/IntegradorMarcas.Api/Contracts/Responses/RrhhJustificacionResumenResponse.cs

- Simbolo: TipoPrincipal.
  - Recomendado: XML doc sobre criterio de seleccion del tipo principal.
- Simbolo: JefaturaNombre.
  - Recomendado: XML doc sobre nulabilidad por estructura organizacional.

Archivo: backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleCompletaResponse.cs

- Simbolo: Aprobador.
  - Recomendado: XML doc explicando null cuando aun no existe resolucion.

## Lista priorizada de archivos recomendados

Prioridad 1 (alto impacto, comentar primero):

1. backend/src/IntegradorMarcas.Api/Program.cs
2. backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs
3. backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs
4. backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs

Prioridad 2 (impacto medio, XML docs en endpoints):

1. backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs
2. backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs
3. backend/src/IntegradorMarcas.Api/Contracts/Requests/ResolverJustificacionRequest.cs
4. backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateDelegacionRequest.cs
5. backend/src/IntegradorMarcas.Api/Contracts/Requests/ToggleEstadoRegistroRequest.cs

Prioridad 3 (impacto puntual, solo campos ambiguos):

1. backend/src/IntegradorMarcas.Api/Contracts/Responses/RrhhJustificacionResumenResponse.cs
2. backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleCompletaResponse.cs

## Criterio operativo para aplicar comentarios

Agregar comentario solo si cumple al menos una condicion:

- Explica una regla de negocio no evidente.
- Justifica una decision tecnica con trade-off.
- Evita un error probable de mantenimiento.
- Estabiliza el contrato publico de la API para otros equipos.

Si no cumple ninguna, no agregar comentario.
