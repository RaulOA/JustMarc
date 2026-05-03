# Spec: Logging Completo de Errores — IntegradorMarcas

**Fecha:** 2026-05-02  
**Versión:** 1.0  
**Alcance:** Cobertura total de logging en `Auditoria.ErrorApi` para todos los tipos de error HTTP.

---

## Sección 1: Tablas de Auditoría Disponibles

### 1.1 `Auditoria.ErrorApi`

**Propósito:** Registro de errores técnicos de la API (excepciones, fallos HTTP).  
**Script origen:** `001_integra_marcas_base_inicial.sql` + `005_fix_errorapi_schema.sql`

| Columna | Tipo | Nullable | Notas |
|---|---|---|---|
| `ErrorApiId` | INT IDENTITY PK | NO | Auto-generado |
| `CorrelationId` | UNIQUEIDENTIFIER | NO | DEFAULT NEWID() |
| `FechaUtc` | DATETIME2(3) | NO | DEFAULT SYSUTCDATETIME() |
| `HttpMethod` | VARCHAR(10) | NO | Renombrado de `MetodoHttp` (005) |
| `Endpoint` | NVARCHAR(500) | NO | Path de la request |
| `StatusCode` | INT | NO | Renombrado de `CodigoEstado` (005) |
| `TipoError` | VARCHAR(200) | NO | Nombre del tipo de excepción |
| `Mensaje` | NVARCHAR(1000) | NO | `exception.Message` |
| `StackTrace` | NVARCHAR(MAX) | SÍ | Solo se envía en errores ≥500 hoy |
| `UsuarioID` | VARCHAR(150) | SÍ | Renombrado de `UsuarioSolicitante` (005) |
| `Ip` | VARCHAR(45) | SÍ | Renombrado de `DireccionIP` (005) |
| `RolUsuario` | VARCHAR(50) | SÍ | Agregado en 005 |
| `Entorno` | VARCHAR(50) | SÍ | Agregado en 005 |
| `UserAgent` | NVARCHAR(500) | SÍ | Agregado en 005 |

---

### 1.2 `Auditoria.EventoAuditoria`

**Propósito:** Registro de eventos de negocio exitosos (auditoría funcional, no errores).  
**Nota:** No es el destino de errores; es complementaria.

| Columna | Tipo | Nullable |
|---|---|---|
| `EventoAuditoriaId` | BIGINT IDENTITY PK | NO |
| `FechaEvento` | DATETIME2 | NO |
| `UsuarioId` | INT | SÍ |
| `NombreUsuario` | VARCHAR(150) | NO |
| `RolCodigo` | VARCHAR(20) | NO |
| `TipoEventoAuditoriaId` | INT FK | NO |
| `DescripcionEvento` | VARCHAR(500) | NO |
| `ResultadoAuditoriaId` | INT FK | NO |
| `ReferenciaFuncional` | VARCHAR(100) | SÍ |
| `PayloadResumen` | VARCHAR(1000) | SÍ |

---

## Sección 2: Cobertura Actual de Logging

### 2.1 Flujo actual

El `UseExceptionHandler` en `Program.cs` actúa como middleware global. Captura **únicamente excepciones no manejadas** que atraviesan el pipeline. El registro ocurre dentro del handler con un fire-and-forget seguro (`catch {}`).

```
Request → [Middleware pipeline] → Controller → Service → Repository
                                                              ↓ (excepción lanzada)
                                         UseExceptionHandler ← (captura)
                                                              ↓
                                               Auditoria.ErrorApi (LogAsync)
```

### 2.2 Estado de cobertura por tipo de error

| Tipo de Error | HTTP | ¿Se Loggea? | Motivo |
|---|---|---|---|
| Excepción no manejada (bugs runtime) | 500 | ✅ SÍ | Capturada por `UseExceptionHandler`, rama `_` |
| `AppException` (negocio) | 400/403/404/401 | ✅ SÍ | Capturada por `UseExceptionHandler`, rama `AppException` |
| `AppException(401)` desde `HeaderUserContext` | 401 | ✅ SÍ | Igual; se lanza antes del controller, sigue siendo excepción |
| `KeyNotFoundException` | 404 | ✅ SÍ | Rama explícita en el switch |
| `OperationCanceledException` | 499 | ✅ SÍ | Rama explícita en el switch |
| `SqlException` en repositorios | 500 | ✅ SÍ | Sin try/catch en repos → propaga → handler global |
| **Validación automática de ModelState** (400) | **400** | ❌ **NO** | `[ApiController]` cortocircuita y devuelve 400 sin lanzar excepción |
| **JSON malformado** en body | **400** | ❌ **NO** | ASP.NET Core devuelve 400 directamente, sin excepción |
| **Ruta no encontrada** (404) | **404** | ❌ **NO** | No hay handler de endpoint → respuesta 404 sin excepción |
| `StackTrace` en errores 400/401/403/404 | — | ⚠️ PARCIAL | Se envía `null` para statusCode < 500; dificulta diagnóstico |

---

## Sección 3: Brechas Identificadas

### Brecha 1 (CRÍTICA): Validación automática de ModelState no se loggea

**Contexto:** `[ApiController]` habilita la validación automática de modelo. Si un request llega con datos faltantes o inválidos según `DataAnnotations`, ASP.NET Core genera una respuesta `ValidationProblemDetails` (HTTP 400) **sin lanzar excepción**. El `UseExceptionHandler` nunca se activa.

**Impacto:** Todos los errores 400 por binding/validación de request body son invisibles en BD.  
**Archivos afectados:** `Program.cs` (configuración `AddControllers`).

---

### Brecha 2 (MEDIA): JSON malformado no se loggea

**Contexto:** Si el cliente envía JSON inválido (ej. `{"motivo": `), el middleware de deserialización de ASP.NET Core devuelve 400 directamente sin lanzar excepción visible.

**Impacto:** Errores de cliente por payload malformado son invisibles.  
**Archivos afectados:** `Program.cs`.

---

### Brecha 3 (BAJA): Rutas no encontradas (404) no se loggean

**Contexto:** Requests a endpoints inexistentes (`GET /api/no-existe`) producen 404 sin excepción.

**Impacto:** Se pierde visibilidad de errores de integración (clientes con URLs incorrectas).  
**Archivos afectados:** `Program.cs`.

---

### Brecha 4 (BAJA): StackTrace omitido para errores < 500

**Contexto:** La línea `StackTrace: statusCode >= 500 ? exception.StackTrace : null` descarta el stack para AppException (400/401/403/404). En producción, esto puede dificultar identificar el origen exacto de una AppException inesperada.

**Impacto:** Menor. La mayoría de AppException son intencionales, pero puede ser útil en debugging.  
**Archivos afectados:** `Program.cs` línea 91.

---

## Sección 4: Plan de Implementación

### Cambio 1: Interceptar validación de ModelState y JSON malformado (Brechas 1 y 2)

**Estrategia:** Configurar `InvalidModelStateResponseFactory` para que transforme los errores de validación en `AppException`, así llegan al `UseExceptionHandler` existente.

**Archivo:** `backend/src/IntegradorMarcas.Api/Program.cs`  
**Insertar después de:** `builder.Services.AddControllers();`

```csharp
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>
    {
        options.InvalidModelStateResponseFactory = context =>
        {
            // Serializa errores de ModelState como mensaje legible
            var errors = context.ModelState
                .Where(e => e.Value?.Errors.Count > 0)
                .SelectMany(e => e.Value!.Errors.Select(er =>
                    string.IsNullOrWhiteSpace(er.ErrorMessage)
                        ? er.Exception?.Message ?? "Error de validación"
                        : er.ErrorMessage))
                .ToList();

            var mensaje = errors.Count > 0
                ? string.Join("; ", errors)
                : "Datos de entrada inválidos.";

            // Al lanzar excepción, el UseExceptionHandler la captura y loggea en BD
            throw new AppException(mensaje, StatusCodes.Status400BadRequest);
        };
    });
```

**Nota:** Esto convierte la respuesta automática 400 en una excepción que pasa por el handler global. El resultado HTTP final sigue siendo 400.

---

### Cambio 2: Interceptar rutas no encontradas y métodos no permitidos (Brecha 3)

**Estrategia:** Usar `UseStatusCodePages` con un delegado que loggee en BD cuando la respuesta tiene status code de error pero no hay excepción pendiente (ej. 404 por ruta, 405 por método).

**Archivo:** `backend/src/IntegradorMarcas.Api/Program.cs`  
**Insertar antes de:** `app.MapControllers();`

```csharp
// Captura respuestas de error sin excepción (404 ruta, 405 método no permitido, etc.)
app.UseStatusCodePages(async statusCodeContext =>
{
    var context    = statusCodeContext.HttpContext;
    var statusCode = context.Response.StatusCode;

    // Solo loggear errores de cliente/servidor que no vienen de una excepción
    // (las excepciones ya fueron loggeadas por UseExceptionHandler)
    if (statusCode is >= 400 and not 499)
    {
        try
        {
            var errorRepo = context.RequestServices.GetService<IErrorLogRepository>();
            if (errorRepo is not null)
            {
                var req      = context.Request;
                var userId   = req.Headers.TryGetValue("X-User-Id",   out var uid)  ? uid.ToString()  : null;
                var userRole = req.Headers.TryGetValue("X-User-Role", out var role) ? role.ToString() : null;
                var ip       = context.Connection.RemoteIpAddress?.ToString();
                var ua       = req.Headers.UserAgent.ToString();
                var env      = app.Environment.EnvironmentName;

                var tipoError = statusCode switch
                {
                    404 => "RouteNotFound",
                    405 => "MethodNotAllowed",
                    _   => $"HttpError{statusCode}"
                };

                await errorRepo.LogAsync(new ErrorLogEntry(
                    CorrelationId: Guid.NewGuid(),
                    HttpMethod:    req.Method,
                    Endpoint:      req.Path.Value ?? "/",
                    StatusCode:    statusCode,
                    TipoError:     tipoError,
                    Mensaje:       $"Respuesta HTTP {statusCode} sin excepción.",
                    StackTrace:    null,
                    UsuarioId:     userId,
                    RolUsuario:    userRole,
                    Entorno:       env,
                    Ip:            ip,
                    UserAgent:     ua
                ));
            }
        }
        catch { }
    }
});
```

**Importante:** `UseStatusCodePages` debe colocarse **después** de `UseExceptionHandler` y **antes** de `MapControllers`. Cuando `UseExceptionHandler` ya manejó la excepción y escribió la respuesta, `UseStatusCodePages` no vuelve a activarse para ese request (el response body ya fue escrito). Esto es seguro: no habrá doble logging.

---

### Cambio 3: Incluir StackTrace para todos los errores (Brecha 4) — Opcional

**Archivo:** `backend/src/IntegradorMarcas.Api/Program.cs` — línea 91

**Estado actual:**
```csharp
StackTrace:    statusCode >= 500 ? exception.StackTrace : null,
```

**Cambio propuesto:**
```csharp
StackTrace:    exception.StackTrace,
```

**Consideración:** Esto aumenta el volumen de datos en BD para errores 400/401/403/404. Se recomienda solo si se quiere trazabilidad completa en desarrollo/staging. Para producción, puede mantenerse la restricción a ≥500 para reducir ruido.

---

## Resumen de Brechas y Prioridades

| # | Brecha | Impacto | Prioridad | Cambio requerido |
|---|---|---|---|---|
| 1 | ModelState 400 no loggeado | Alto | P1 | `InvalidModelStateResponseFactory` en `AddControllers()` |
| 2 | JSON malformado 400 no loggeado | Alto | P1 | Cubierto por el mismo cambio del punto 1 |
| 3 | Rutas 404 no loggeadas | Medio | P2 | `UseStatusCodePages` antes de `MapControllers` |
| 4 | StackTrace omitido para <500 | Bajo | P3 | Cambio de una línea en `Program.cs` (opcional) |

**Estado actual confirmado como correcto:**
- AppException (400/401/403/404) lanzada en servicios → ✅ loggeada
- AppException(401) de `HeaderUserContext` → ✅ loggeada  
- Excepciones no manejadas (500) → ✅ loggeada con StackTrace
- `SqlException` en repositorios → ✅ loggeada como 500
- `KeyNotFoundException` → ✅ loggeada como 404

---

## Orden de Aplicación de Cambios en `Program.cs`

El orden final del pipeline debe ser:

```csharp
app.UseExceptionHandler(...)   // ya existe — primero
app.UseStatusCodePages(...)    // NUEVO — segundo
app.UseSwagger() / UseSwaggerUI()
app.UseHttpsRedirection()
app.UseCors("LocalFrontend")
app.MapControllers()
```

Y en el builder:

```csharp
builder.Services.AddControllers()
    .ConfigureApiBehaviorOptions(options =>    // NUEVO
    {
        options.InvalidModelStateResponseFactory = ...;
    });
```
