# Especificacion P1: comentarios internos en IntegradorMarcas.Api

Fecha: 2026-04-23
Scope: backend/src/IntegradorMarcas.Api/Program.cs, Security/HeaderUserContext.cs, Controllers/JefaturaController.cs, Controllers/AdminAprobacionesController.cs

## Objetivo
Definir comentarios internos de alta prioridad, precisos y alineados al comportamiento actual del codigo, evitando ruido y evitando documentar comportamientos no garantizados.

## Hallazgos relevantes del estado real

1. Program.cs tiene dos registros consecutivos de UseExceptionHandler.
2. El segundo bloque de UseExceptionHandler agrega correlationId, registra en IErrorLogRepository y maneja OperationCanceledException con 499.
3. HeaderUserContext obtiene nombres de headers desde configuracion con fallback a X-User-Id y X-User-Role, y lanza AppException 401 en validaciones fallidas.
4. JefaturaController y AdminAprobacionesController delegan reglas de negocio al servicio; el controller se enfoca en obtener contexto de usuario, invocar servicio y mapear DTOs.

## Regla de implementacion para esta P1

- Comentar solo decisiones no obvias, contratos publicos y semantica de seguridad/operacion.
- No comentar mapeos 1:1 ni constructores triviales.
- En Program.cs, no agregar comentarios funcionales al primer UseExceptionHandler mientras exista duplicidad, para no reforzar una configuracion confusa.

## Plan exacto de comentarios a agregar

## 1) Program.cs

Archivo: backend/src/IntegradorMarcas.Api/Program.cs

### 1.1 Validacion de connection string fuera de Development
- Tipo: bloque.
- Ubicacion: inmediatamente antes de `if (!builder.Environment.IsDevelopment())`.
- Texto sugerido:

```csharp
// En entornos no Development, la conexion principal es obligatoria para fallar en arranque
// si falta configuracion critica y evitar errores diferidos en runtime.
```

### 1.2 Politica CORS LocalFrontend
- Tipo: inline (una sola linea).
- Ubicacion: justo encima de `.SetIsOriginAllowed(_ => true)` dentro de AddPolicy("LocalFrontend").
- Texto sugerido:

```csharp
// Apertura total de origenes para entorno local/controlado; restringir en despliegues expuestos.
```

### 1.3 Segundo bloque global de excepciones (el que genera correlationId)
- Tipo: bloque.
- Ubicacion: inmediatamente antes del segundo `app.UseExceptionHandler(exceptionApp =>` (el bloque que contiene `var correlationId = Guid.NewGuid();`).
- Texto sugerido:

```csharp
// Manejo global de excepciones para respuestas ProblemDetails consistentes:
// - Traduce excepciones conocidas a codigos HTTP esperados.
// - Adjunta correlationId para trazabilidad en soporte.
// - Registra error tecnico en BD sin propagar fallos del logger.
```

### 1.4 Persistencia de error en bloque try/catch
- Tipo: inline.
- Ubicacion: sobre `catch { /* nunca propagar */ }`.
- Reemplazar comentario existente por:

```csharp
// El logging de error nunca debe romper la respuesta principal al cliente.
```

### 1.5 Header de correlacion en la respuesta
- Tipo: inline.
- Ubicacion: justo encima de `context.Response.Headers["X-Correlation-Id"] = correlationId.ToString();`.
- Texto sugerido:

```csharp
// Exponer correlationId para cruce entre respuesta del cliente y bitacora tecnica.
```

### 1.6 Endpoint de health check
- Tipo: bloque corto.
- Ubicacion: inmediatamente antes de `app.MapGet("/health", () => Results.Ok(new`.
- Texto sugerido:

```csharp
// Probe operativo minimo para disponibilidad del proceso y referencia temporal UTC.
```

### 1.7 No agregar comentario en primer UseExceptionHandler (por ahora)
- Tipo: decision explicita de no documentar.
- Motivo: existe duplicidad de middleware; comentar ese bloque puede fijar una semantica que deberia consolidarse primero.

## 2) Security/HeaderUserContext.cs

Archivo: backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs

### 2.1 Clase HeaderUserContext
- Tipo: XML summary.
- Ubicacion: inmediatamente sobre `public sealed class HeaderUserContext : IUserContext`.
- Texto sugerido:

```csharp
/// <summary>
/// Resuelve la identidad del usuario a partir de headers HTTP definidos por configuracion.
/// </summary>
```

### 2.2 Metodo GetCurrent
- Tipo: XML summary + remarks.
- Ubicacion: inmediatamente sobre `public UserContextInfo GetCurrent()`.
- Texto sugerido:

```csharp
/// <summary>
/// Obtiene UserId y Role desde el request actual.
/// </summary>
/// <remarks>
/// Lanza AppException (401) cuando no existe contexto HTTP o cuando los headers requeridos
/// no estan presentes o son invalidos.
/// </remarks>
```

### 2.3 Resolucion de nombres de header con fallback
- Tipo: inline.
- Ubicacion: sobre `var userHeader = _configuration["Security:HeaderUserId"] ?? "X-User-Id";`.
- Texto sugerido:

```csharp
// Permite cambiar nombres de cabecera por configuracion manteniendo defaults compatibles.
```

### 2.4 Validacion de userId numerico y positivo
- Tipo: inline.
- Ubicacion: sobre la condicion `if (!context.Request.Headers.TryGetValue(userHeader, out var userValues) ... userId <= 0)`.
- Texto sugerido:

```csharp
// Se considera identidad valida solo cuando el UserId existe, parsea a entero y es mayor a cero.
```

### 2.5 Validacion de rol requerido
- Tipo: inline.
- Ubicacion: sobre la condicion `if (!context.Request.Headers.TryGetValue(roleHeader, out var roleValues) ... )`.
- Texto sugerido:

```csharp
// El rol es obligatorio para aplicar autorizacion de negocio en capas superiores.
```

## 3) Controllers/JefaturaController.cs

Archivo: backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs

### 3.1 Endpoint ListPendientes
- Tipo: XML summary + param.
- Ubicacion: sobre `[HttpGet("pendientes")]` o directamente sobre la firma del metodo ListPendientes.
- Texto sugerido:

```csharp
/// <summary>
/// Lista justificaciones pendientes visibles para la jefatura autenticada.
/// </summary>
/// <param name="desde">Fecha inicial opcional para filtrar por rango de creacion.</param>
/// <param name="hasta">Fecha final opcional para filtrar por rango de creacion.</param>
```

### 3.2 Endpoint GetDetalle
- Tipo: XML summary.
- Ubicacion: sobre `[HttpGet("{justificacionId:int}")]` o la firma GetDetalle.
- Texto sugerido:

```csharp
/// <summary>
/// Obtiene el detalle completo de una justificacion accesible por la jefatura actual.
/// </summary>
```

### 3.3 Endpoint Resolver
- Tipo: XML summary + remarks.
- Ubicacion: sobre `[HttpPatch("{justificacionId:int}/resolver")]` o la firma Resolver.
- Texto sugerido:

```csharp
/// <summary>
/// Resuelve una justificacion mediante la accion indicada por la jefatura.
/// </summary>
/// <remarks>
/// La validacion de accion/comentario y las transiciones de estado se aplican en la capa de servicio.
/// </remarks>
```

### 3.4 No comentar bloques de mapeo response
- Tipo: decision explicita de no documentar.
- Motivo: mapeos son directos 1:1 y el comentario agregaria ruido.

## 4) Controllers/AdminAprobacionesController.cs

Archivo: backend/src/IntegradorMarcas.Api/Controllers/AdminAprobacionesController.cs

### 4.1 Endpoint ListJerarquias
- Tipo: XML summary.
- Ubicacion: sobre `[HttpGet("jerarquias")]` o firma ListJerarquias.
- Texto sugerido:

```csharp
/// <summary>
/// Lista jerarquias de aprobacion con filtros administrativos opcionales.
/// </summary>
```

### 4.2 Endpoint CreateJerarquia
- Tipo: XML summary + remarks.
- Ubicacion: sobre `[HttpPost("jerarquias")]` o firma CreateJerarquia.
- Texto sugerido:

```csharp
/// <summary>
/// Crea una jerarquia de aprobacion.
/// </summary>
/// <remarks>
/// Las reglas de consistencia (vigencia, duplicidad y permisos) se validan en la capa de servicio.
/// </remarks>
```

### 4.3 Endpoint ToggleJerarquiaEstado
- Tipo: XML summary.
- Ubicacion: sobre `[HttpPatch("jerarquias/{jerarquiaAprobacionId:int}/estado")]` o firma ToggleJerarquiaEstado.
- Texto sugerido:

```csharp
/// <summary>
/// Cambia el estado de registro de una jerarquia de aprobacion.
/// </summary>
```

### 4.4 Endpoint ListDelegaciones
- Tipo: XML summary.
- Ubicacion: sobre `[HttpGet("delegaciones")]` o firma ListDelegaciones.
- Texto sugerido:

```csharp
/// <summary>
/// Lista delegaciones de aprobacion con filtros por delegante, delegado, estado y fecha de vigencia.
/// </summary>
```

### 4.5 Endpoint CreateDelegacion
- Tipo: XML summary + remarks.
- Ubicacion: sobre `[HttpPost("delegaciones")]` o firma CreateDelegacion.
- Texto sugerido:

```csharp
/// <summary>
/// Crea una delegacion de aprobacion.
/// </summary>
/// <remarks>
/// Las validaciones de vigencia y consistencia referencial se ejecutan en la capa de servicio.
/// </remarks>
```

### 4.6 Endpoint ToggleDelegacionEstado
- Tipo: XML summary.
- Ubicacion: sobre `[HttpPatch("delegaciones/{delegacionAprobacionId:int}/estado")]` o firma ToggleDelegacionEstado.
- Texto sugerido:

```csharp
/// <summary>
/// Cambia el estado de registro de una delegacion de aprobacion.
/// </summary>
```

### 4.7 No comentar mapeos de DTOs
- Tipo: decision explicita de no documentar.
- Motivo: transformaciones directas y evidentes.

## Checklist de calidad para implementacion

1. No afirmar reglas de dominio no visibles en estos archivos.
2. No inventar codigos de respuesta adicionales a los ya manejados.
3. Mantener comentarios en espanol tecnico y consistentes con nombres del dominio actual.
4. Limitar inline a puntos no obvios (seguridad, operacion, trazabilidad).
5. Evitar comentarios en cada propiedad o linea de mapeo.

## Entregable
Este documento define los comentarios P1 a implementar sin ruido ni contradicciones con el estado actual del codigo.
