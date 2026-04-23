# Guia accionable de comentarios internos (IntegradorMarcas.Api)

Fecha: 2026-04-23
Base: especificacion en docs/SubAgent docs/comentarios_internos_api_spec.md

## Objetivo
Estandarizar comentarios internos utiles en backend/src/IntegradorMarcas.Api para mejorar mantenibilidad sin sobre-documentar.

## Regla de oro
Agregar comentario solo si cumple al menos una condicion:
- Explica una regla de negocio no evidente.
- Justifica una decision tecnica con trade-off.
- Evita un error probable de mantenimiento.
- Estabiliza un contrato publico consumido por otros equipos.

Si no cumple ninguna, no comentar.

## Prioridad y estilo por archivo

### Prioridad 1 (alto impacto)

1. backend/src/IntegradorMarcas.Api/Program.cs
- Estilo principal: comentario de bloque.
- Estilo secundario: inline puntual.
- Donde comentar:
  - Validacion de ConnectionStrings:IntegraCnp fuera de Development (fail-fast).
  - Politica CORS LocalFrontend con SetIsOriginAllowed(_ => true) (trade-off de seguridad en entorno controlado).
  - Manejo global de excepciones (mapeo excepcion -> HTTP + correlationId).
  - Endpoint /health (contrato operativo).
- Accion concreta:
  - Consolidar los bloques duplicados de UseExceptionHandler antes de documentar, para evitar comentarios contradictorios.

2. backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs
- Estilo principal: XML doc en clase y metodo publico.
- Estilo secundario: inline en validaciones no obvias.
- Donde comentar:
  - Estrategia de identidad por headers (origen gateway/proxy).
  - Criterios de rechazo 401 cuando faltan/invalidan headers.
  - Parse y validacion de UserId (> 0).

3. backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs
- Estilo principal: XML doc por endpoint.
- Estilo secundario: inline solo en reglas excepcionales.
- Donde comentar:
  - Alcance de ListPendientes (solo casos visibles para la jefatura autenticada).
  - Composicion de GetDetalle (encabezado, solicitante, aprobador, detalle).
  - Semantica de Resolver (accion permitida + efecto de estado).

4. backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs
- Estilo principal: XML doc por endpoint.
- Estilo secundario: inline para decisiones no intuitivas.
- Donde comentar:
  - ToggleJerarquiaEstado/ToggleDelegacionEstado (habilitar vs inhabilitar logico).
  - Reglas de vigencia y referencias opcionales en CreateJerarquia/CreateDelegacion.

### Prioridad 2 (impacto medio)

5. backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs
- Estilo: XML doc por endpoint.
- Donde comentar:
  - Create: estado inicial de negocio + CreatedAtAction.
  - ListMine: filtros opcionales y alcance por usuario autenticado.

6. backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs
- Estilo: XML doc por endpoint.
- Donde comentar:
  - Filtros combinables.
  - Semantica de rangos de fecha.
- No comentar:
  - Mapeos Select 1:1 salvo transformacion especial.

7. backend/src/IntegradorMarcas.Api/Contracts/Requests/ResolverJustificacionRequest.cs
- Estilo: XML doc por propiedad ambigua.
- Donde comentar:
  - Accion: valores permitidos.
  - Comentario: obligatoriedad condicional (si aplica).

8. backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateDelegacionRequest.cs
- Estilo: XML doc por propiedad ambigua.
- Donde comentar:
  - JerarquiaAprobacionID nullable (semantica cuando es null).
  - VigenciaDesde/VigenciaHasta (inclusividad y zona horaria).

9. backend/src/IntegradorMarcas.Api/Contracts/Requests/ToggleEstadoRegistroRequest.cs
- Estilo: XML doc por propiedad ambigua.
- Donde comentar:
  - EstadoRegistroID: catalogo esperado y significado funcional.

### Prioridad 3 (impacto puntual)

10. backend/src/IntegradorMarcas.Api/Contracts/Responses/RrhhJustificacionResumenResponse.cs
- Estilo: XML doc solo en campos ambiguos.
- Donde comentar:
  - TipoPrincipal: criterio de seleccion.
  - JefaturaNombre: semantica de nulabilidad.

11. backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleCompletaResponse.cs
- Estilo: XML doc solo en campos ambiguos.
- Donde comentar:
  - Aprobador nullable: cuando no existe resolucion aun.

## Estilo de comentario por caso

- XML doc:
  - Usar en API publica (metodos de controllers) y contratos con semantica ambigua.
  - Incluir summary y, cuando aporte valor, remarks.
- Bloque:
  - Usar en decisiones de pipeline/infraestructura con impacto operativo.
  - Limitar a 2-4 lineas, orientado a motivo y efecto.
- Inline:
  - Usar solo para excepciones a la intuicion o notas de seguridad/operacion.
  - Maximo una linea cuando sea posible.

## Plantillas de ejemplo

### 1) XML doc para endpoint
```csharp
/// <summary>
/// Lista justificaciones pendientes de aprobacion para la jefatura autenticada.
/// </summary>
/// <param name="desde">Fecha inicial opcional del rango de filtro.</param>
/// <param name="hasta">Fecha final opcional del rango de filtro.</param>
/// <remarks>
/// Requiere cabeceras de identidad y rol. Devuelve solo casos visibles para la jefatura actual.
/// </remarks>
```

### 2) XML doc para propiedad de contrato
```csharp
/// <summary>
/// Accion solicitada sobre la justificacion.
/// Valores permitidos: APROBAR o RECHAZAR.
/// </summary>
public string Accion { get; init; } = string.Empty;
```

### 3) Comentario de bloque (Program.cs)
```csharp
// Manejo global de errores:
// - Traduce excepciones de dominio a codigos HTTP coherentes.
// - Adjunta correlationId para trazabilidad operativa.
// - Evita exponer detalles internos en produccion.
```

### 4) Inline puntual
```csharp
// 499 representa cancelacion iniciada por el cliente (uso operativo interno).
```

## Anti-patrones a evitar

- Comentar lo obvio:
  - "Asigna X a Y", "Obtiene lista", "Constructor por defecto".
- Duplicar codigo en comentarios:
  - Si el comentario repite literalmente la instruccion, eliminarlo.
- Comentarios largos sin accion:
  - Evitar parrafos historicos o de contexto no operativo.
- Comentarios desactualizables:
  - No fijar reglas de negocio que cambian rapido sin enlazar a fuente estable.
- Mezclar idioma/terminologia inconsistente:
  - Mantener termino de dominio estable (Jefatura, RRHH, Delegacion, etc.).
- Inline excesivo en LINQ/mapeos:
  - Documentar la regla en el contrato o XML del endpoint, no en cada linea del Select.

## Archivos con bajo retorno (evitar comentar salvo excepcion)

- backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateJustificacionRequest.cs
- backend/src/IntegradorMarcas.Api/Contracts/Requests/JustificacionDetalleRequest.cs
- backend/src/IntegradorMarcas.Api/Contracts/Requests/CreateJerarquiaRequest.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/CreateJustificacionResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionResumenResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/JustificacionDetalleLineaResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminJerarquiaResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/AdminDelegacionResponse.cs
- backend/src/IntegradorMarcas.Api/Contracts/Responses/UsuarioResumenResponse.cs

## Secuencia sugerida de ejecucion (sprint corto)

1. Program.cs y HeaderUserContext.cs.
2. JefaturaController.cs y AdminAprobacionesController.cs.
3. JustificacionesController.cs y RrhhController.cs.
4. Requests ambiguos (Resolver, Delegacion, ToggleEstado).
5. Responses ambiguos (RRHH resumen, detalle completo).

Criterio de cierre por archivo:
- Comentarios minimos, precisos y consistentes con el comportamiento actual.
- Sin comentarios redundantes en mapeos triviales.
- Sin contradicciones entre XML doc y validaciones reales.
