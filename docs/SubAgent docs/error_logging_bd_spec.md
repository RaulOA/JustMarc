# Spec: Error Logging y Auditoría en BD

**Fecha:** 2026-05-02  
**Estado:** Investigación completa — pendiente corrección de schema mismatch

---

## 1. Tablas de Log / Auditoría en BD (DDL fuente: `001_integra_marcas_base_inicial.sql`)

### 1.1 `Auditoria.EventoAuditoria`
Propósito: trazabilidad de eventos de negocio (creación de justificaciones, resoluciones, jerarquías, etc.)

| Columna                | Tipo              | Nulo | Notas                                          |
|------------------------|-------------------|------|------------------------------------------------|
| EventoAuditoriaId      | BIGINT IDENTITY   | NO   | PK                                             |
| FechaEvento            | DATETIME2         | NO   | DEFAULT SYSUTCDATETIME()                       |
| UsuarioId              | INT               | SÍ   | FK → RecursosHumanos.Usuario                   |
| NombreUsuario          | VARCHAR(150)      | NO   |                                                |
| RolCodigo              | VARCHAR(20)       | NO   |                                                |
| TipoEventoAuditoriaId  | INT               | NO   | FK → Configuracion.TipoEventoAuditoria         |
| DescripcionEvento      | VARCHAR(500)      | NO   |                                                |
| ResultadoAuditoriaId   | INT               | NO   | FK → Configuracion.ResultadoAuditoria          |
| ReferenciaFuncional    | VARCHAR(100)      | SÍ   |                                                |
| PayloadResumen         | VARCHAR(1000)     | SÍ   |                                                |

**Catálogos relacionados:**
- `Configuracion.TipoEventoAuditoria` — seed: CreacionJustificacion(1), ResolucionAprobada(2), ResolucionRechazada(3), AltaJerarquia(4), CambioEstadoJerarquia(5), AltaDelegacion(6), CambioEstadoDelegacion(7)
- `Configuracion.ResultadoAuditoria` — seed: Exito(1), Fallo(2), Denegado(3)

---

### 1.2 `Auditoria.ErrorApi` — ⚠️ SCHEMA MISMATCH DETECTADO
Propósito: registrar excepciones HTTP no controladas.

**DDL en `001_integra_marcas_base_inicial.sql` (estado actual de la BD):**

| Columna            | Tipo                  | Notas                         |
|--------------------|-----------------------|-------------------------------|
| ErrorApiId         | INT IDENTITY          | PK                            |
| CorrelationId      | UNIQUEIDENTIFIER      | DEFAULT NEWID()               |
| FechaUtc           | DATETIME2(3)          | DEFAULT SYSUTCDATETIME()      |
| MetodoHttp         | VARCHAR(10)           | NOT NULL                      |
| Endpoint           | NVARCHAR(500)         | NOT NULL                      |
| CodigoEstado       | INT                   | NOT NULL                      |
| TipoError          | VARCHAR(200)          | NOT NULL                      |
| Mensaje            | NVARCHAR(1000)        | NOT NULL                      |
| StackTrace         | NVARCHAR(MAX)         | NULL                          |
| UsuarioSolicitante | VARCHAR(150)          | NULL                          |
| DireccionIP        | VARCHAR(45)           | NULL                          |

**INSERT que genera `ErrorLogRepository.cs` (columnas referenciadas en código):**
```
CorrelationID, HttpMethod, Endpoint, StatusCode, TipoError, Mensaje, StackTrace,
UsuarioID, RolUsuario, Entorno, Ip, UserAgent
```

**Divergencias críticas (INSERT fallará en runtime):**

| Código C# (columna en INSERT) | DDL en BD             | Estado           |
|-------------------------------|-----------------------|------------------|
| `HttpMethod`                  | `MetodoHttp`          | ❌ nombre distinto |
| `StatusCode`                  | `CodigoEstado`        | ❌ nombre distinto |
| `UsuarioID`                   | `UsuarioSolicitante`  | ❌ nombre distinto |
| `Ip`                          | `DireccionIP`         | ❌ nombre distinto |
| `RolUsuario`                  | *(no existe)*         | ❌ columna faltante |
| `Entorno`                     | *(no existe)*         | ❌ columna faltante |
| `UserAgent`                   | *(no existe)*         | ❌ columna faltante |

---

## 2. Infraestructura C# existente

### 2.1 Interfaces (Application layer)

**`IErrorLogRepository`** — `backend/src/IntegradorMarcas.Application/Interfaces/IErrorLogRepository.cs`
```csharp
public interface IErrorLogRepository
{
    Task LogAsync(ErrorLogEntry entry);
}

public sealed record ErrorLogEntry(
    Guid CorrelationId, string HttpMethod, string Endpoint, int StatusCode,
    string TipoError, string Mensaje, string? StackTrace,
    string? UsuarioId, string? RolUsuario, string Entorno, string? Ip, string? UserAgent
);
```

**`IAuditEventRepository`** — interface existente con `LogEventAsync(AuditEventEntry, CancellationToken)`

### 2.2 Repositorios (Infrastructure layer)

**`ErrorLogRepository`** — `backend/src/IntegradorMarcas.Infrastructure/Repositories/ErrorLogRepository.cs`
- Implementa `IErrorLogRepository`
- Usa Dapper + `SqlConnection` directa
- Inserta en `Auditoria.ErrorApi` (con nombres de columna que NO coinciden con el DDL actual)
- Envuelve internamente en try/catch para nunca propagar excepción

**`AuditEventRepository`** — `backend/src/IntegradorMarcas.Infrastructure/Repositories/AuditEventRepository.cs`
- Implementa `IAuditEventRepository`
- Usa `AuditoriaSql.InsertEvento` (query en `Queries/AuditoriaSql.cs`)
- Inserta en `Auditoria.EventoAuditoria` — columnas coinciden con DDL ✅

### 2.3 DI (Program.cs)
Ambos repositorios están registrados:
```csharp
builder.Services.AddScoped<IAuditEventRepository, AuditEventRepository>();
builder.Services.AddScoped<IErrorLogRepository, ErrorLogRepository>();
```

### 2.4 Global Exception Handler (Program.cs) — ⚠️ DOBLE REGISTRO

`Program.cs` registra **dos** middlewares `UseExceptionHandler` consecutivos:

1. **Primer handler** (líneas ~56–67): responde con ProblemDetails pero **no llama a `errorRepo`**. No registra en BD.
2. **Segundo handler** (líneas ~70–120): llama a `errorRepo.LogAsync(...)` con `CorrelationId` y todos los campos. Expone `X-Correlation-Id` en header de respuesta.

En ASP.NET Core, cuando se registran dos `UseExceptionHandler`, **sólo el último tiene efecto**. El primer handler es código muerto. Actualmente el segundo (con logging a BD) es el activo — comportamiento correcto accidentalmente.

---

## 3. Qué falta para que el logging funcione end-to-end

| # | Problema                                           | Impacto                                                      |
|---|----------------------------------------------------|--------------------------------------------------------------|
| 1 | Schema de `Auditoria.ErrorApi` no coincide con código | Todo INSERT de error falla con `Invalid column name` en runtime |
| 2 | Primer `UseExceptionHandler` es código muerto      | Confusión de mantenimiento; riesgo si alguien reordena los middleware |
| 3 | `Auditoria.ErrorApi` no expone `RolUsuario`, `Entorno`, `UserAgent` | Datos de diagnóstico perdidos (no hay columna donde persistirlos) |

---

## 4. Plan de implementación

### 4.1 Migración SQL — Alinear `Auditoria.ErrorApi` al contrato del código

Crear script `docs/db/005_fix_errorapi_schema.sql`:

```sql
USE INTEGRA_CNP;
GO

/* Renombrar columnas con nombre distinto al contrato del código C# */
-- MetodoHttp → HttpMethod
EXEC sp_rename 'Auditoria.ErrorApi.MetodoHttp',      'HttpMethod',     'COLUMN';
GO
-- CodigoEstado → StatusCode
EXEC sp_rename 'Auditoria.ErrorApi.CodigoEstado',    'StatusCode',     'COLUMN';
GO
-- UsuarioSolicitante → UsuarioID
EXEC sp_rename 'Auditoria.ErrorApi.UsuarioSolicitante', 'UsuarioID',   'COLUMN';
GO
-- DireccionIP → Ip
EXEC sp_rename 'Auditoria.ErrorApi.DireccionIP',     'Ip',             'COLUMN';
GO

/* Agregar columnas faltantes */
ALTER TABLE Auditoria.ErrorApi
    ADD RolUsuario  VARCHAR(50)     NULL,
        Entorno     VARCHAR(50)     NULL,
        UserAgent   NVARCHAR(500)   NULL;
GO
```

### 4.2 Program.cs — Eliminar el primer UseExceptionHandler duplicado

**Archivo:** `backend/src/IntegradorMarcas.Api/Program.cs`

Eliminar el primer bloque `UseExceptionHandler` (el que no registra en BD). Conservar únicamente el segundo, que ya incluye el logging. Añadir comentario de guarda:

```csharp
// ÚNICO handler de excepciones globales.
// No agregar otro UseExceptionHandler — solo el último tiene efecto en ASP.NET Core.
app.UseExceptionHandler(exceptionApp =>
{
    exceptionApp.Run(async context =>
    {
        // ... (bloque existente con correlationId y errorRepo.LogAsync)
    });
});
```

### 4.3 (Opcional) Renombrar columnas del DDL fuente para futura coherencia

Actualizar `docs/db/001_integra_marcas_base_inicial.sql` — sección `Auditoria.ErrorApi` — para que refleje el schema post-migración:

```sql
IF OBJECT_ID('Auditoria.ErrorApi', 'U') IS NULL
BEGIN
    CREATE TABLE Auditoria.ErrorApi (
        ErrorApiId    INT IDENTITY(1,1) NOT NULL PRIMARY KEY,
        CorrelationId UNIQUEIDENTIFIER  NOT NULL DEFAULT NEWID(),
        FechaUtc      DATETIME2(3)      NOT NULL DEFAULT SYSUTCDATETIME(),
        HttpMethod    VARCHAR(10)       NOT NULL,
        Endpoint      NVARCHAR(500)     NOT NULL,
        StatusCode    INT               NOT NULL,
        TipoError     VARCHAR(200)      NOT NULL,
        Mensaje       NVARCHAR(1000)    NOT NULL,
        StackTrace    NVARCHAR(MAX)     NULL,
        UsuarioID     VARCHAR(150)      NULL,
        RolUsuario    VARCHAR(50)       NULL,
        Entorno       VARCHAR(50)       NULL,
        Ip            VARCHAR(45)       NULL,
        UserAgent     NVARCHAR(500)     NULL
    );
END;
GO
```

---

## 5. Resumen de estado actual

| Componente                        | Estado  | Notas                                              |
|-----------------------------------|---------|----------------------------------------------------|
| `Auditoria.EventoAuditoria`       | ✅ OK   | DDL y repositorio coinciden, DI configurada         |
| `AuditEventRepository`            | ✅ OK   | Funcional                                           |
| `IAuditEventRepository`           | ✅ OK   | Interfaz correcta                                   |
| `Auditoria.ErrorApi` (DDL)        | ❌ ROTO | Nombres de columna no coinciden con el código       |
| `ErrorLogRepository`              | ⚠️ ROTO | Fallará en runtime hasta que se ejecute migración   |
| `IErrorLogRepository`             | ✅ OK   | Contrato correcto                                   |
| Global exception handler (2do)    | ✅ OK   | Lógica correcta, llama a errorRepo                  |
| Global exception handler (1ro)    | ❌ MUERTO | Código duplicado sin logging, debe eliminarse      |
| DI registration                   | ✅ OK   | Ambos repositorios registrados como Scoped          |

---

## 6. Orden de ejecución recomendado

1. Ejecutar `docs/db/005_fix_errorapi_schema.sql` en BD `INTEGRA_CNP`
2. Modificar `Program.cs` para eliminar el primer `UseExceptionHandler`
3. Actualizar DDL fuente `001_integra_marcas_base_inicial.sql` para coherencia futura
4. Verificar con una llamada a un endpoint inexistente → revisar fila en `Auditoria.ErrorApi`
