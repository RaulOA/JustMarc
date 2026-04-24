# Backend Alignment - Mapeo de Esquemas Nuevos
**Especificación para alineación de queries y repositorios con nueva estructura SQL**

**Fecha:** 23 de abril de 2026  
**Estado:** Investigación completada - Listo para implementación  
**Basado en:** Consolidación SQL completada en `docs/db/001_integra_marcas_base_inicial.sql`

---

## 1. Resumen Ejecutivo

El backend actualmente utiliza tablas con esquema `dbo` e identificadores con sufijo `ID` (mayúsculas). La consolidación SQL completa ha migrado estos objetos a esquemas funcionales (`RecursosHumanos`, `Operacion`, `Configuracion`, `Auditoria`) con convención PascalCase en identificadores (`Id`).

**Impacto:**
- 4 archivos de queries SQL (.cs) afectados
- 4 archivos de repositorios afectados
- ~35 referencias explícitas a tablas `dbo.*` detectadas
- Cambios en nombres de columnas en 5+ entidades
- Cambios de función SQL: `dbo.fn_AprobadoresVigentesPorSolicitante` → `Operacion.fn_AprobadoresVigentesPorSolicitante`

---

## 2. Mapeo Exacto: dbo.* → Esquema.Tabla

### 2.1 Tablas de Justificaciones (Operacion)

| Nombre Viejo | Nombre Nuevo | Cambio en Columnas | Nota |
|---|---|---|---|
| `dbo.Justificaciones_Encabezado` | `Operacion.Justificacion` | JustificacionID → JustificacionId | PK también usada en FKs |
| `dbo.Justificaciones_Detalle` | `Operacion.JustificacionDetalle` | DetalleID → JustificacionDetalleId; JustificacionID → JustificacionId | Nueva convención completa |

### 2.2 Tablas de Usuarios (RecursosHumanos)

| Nombre Viejo | Nombre Nuevo | Cambio en Columnas | Nota |
|---|---|---|---|
| `dbo.Usuarios` | `RecursosHumanos.Usuario` | UsuarioID → UsuarioId; JefaturaID → JefaturaId; UnidadID → UnidadId; Correo → CorreoElectronico; RolID → RolId (FK) | Cambio significativo en nombre y estructura |

### 2.3 Tablas de Catálogos (Configuracion)

| Nombre Viejo | Nombre Nuevo | Cambio en Columnas | Nota |
|---|---|---|---|
| `dbo.Estados` | `Configuracion.EstadoJustificacion` | EstadoID → EstadoJustificacionId; Descripcion permanece igual | Renombrada por dominio funcional |
| `dbo.Cat_TiposJustificacion` | `Configuracion.TipoJustificacion` | TipoJustificacionID → TipoJustificacionId; Descripcion permanece igual | Elimina prefijo `Cat_` |
| (N/A) | `Configuracion.EstadoRegistro` | EstadoRegistroId | Nueva tabla, usada en jerarquías/delegaciones |
| (N/A) | `Configuracion.Rol` | RolId | Nueva tabla, referenciada por Usuario.RolId |

### 2.4 Tablas de Jerarquías y Delegaciones (Operacion)

| Nombre Viejo | Nombre Nuevo | Cambio en Columnas | Nota |
|---|---|---|---|
| `dbo.Jerarquias_Aprobacion` | `Operacion.JerarquiaAprobacion` | JerarquiaAprobacionID → JerarquiaAprobacionId; AprobadorUsuarioID → AprobadorUsuarioId; EstructuraOrganizacionalID → EstructuraOrganizacionalId; EstadoRegistroID → EstadoRegistroId | Cambio de convención completo |
| `dbo.Delegaciones_Aprobacion` | `Operacion.DelegacionAprobacion` | DelegacionAprobacionID → DelegacionAprobacionId; DeleganteUsuarioID → DeleganteUsuarioId; DelegadoUsuarioID → DelegadoUsuarioId; JerarquiaAprobacionID → JerarquiaAprobacionId; EstadoRegistroID → EstadoRegistroId | Cambio de convención completo |

### 2.5 Tablas de Auditoría (Auditoria)

| Nombre Viejo | Nombre Nuevo | Cambio en Columnas | Nota |
|---|---|---|---|
| `dbo.Auditoria_Eventos` | `Auditoria.EventoAuditoria` | Columnas probables: TipoEventoAuditoriaID → TipoEventoAuditoriaId; ResultadoAuditoriaID → ResultadoAuditoriaId | Detectada en queries |
| `dbo.ApiErrorLog` | `Auditoria.ErrorApi` | CorrelationID, UsuarioID cambios menores | Tabla de logs de errores API |

### 2.6 Funciones SQL

| Nombre Viejo | Nombre Nuevo | Cambio de Parámetros |
|---|---|---|
| `dbo.fn_AprobadoresVigentesPorSolicitante` | `Operacion.fn_AprobadoresVigentesPorSolicitante` | Mismo esquema de parámetros (@SolicitanteUsuarioId, @FechaRef); el resultado incluye AprobadorUsuarioId, Origen, DeleganteUsuarioId |

---

## 3. Archivos .cs que Necesitan Cambios

### 3.1 Infrastructure/Queries

#### Archivo: `JustificacionesSql.cs`
**Tipo:** Clase estática con constantes de queries SQL  
**Líneas afectadas:** ~200+ líneas distribuidas en 8+ constantes  

**Constantes que requieren cambios:**
1. `InsertEncabezado` - INSERT INTO
2. `InsertDetalle` - INSERT INTO
3. `ListMine` - SELECT with JOINs
4. `ListPendientesJefatura` - SELECT with JOINs + función
5. `ListRrhhGlobal` - SELECT with múltiples JOINs
6. `GetDetalleJefaturaEncabezado` - SELECT with JOINs
7. `GetDetalleJefaturaLineas` - SELECT with JOINs
8. `GetResolverValidation` - SELECT con función
9. `GetAprobacionScopeValidation` - SELECT con función
10. `GetExistingTipoJustificacionIds` - SELECT simple
11. `ResolverPendiente` - UPDATE

**Cambios específicos:**
- Reemplazar `dbo.Justificaciones_Encabezado` con `Operacion.Justificacion`
- Reemplazar `dbo.Justificaciones_Detalle` con `Operacion.JustificacionDetalle`
- Reemplazar `dbo.Estados` con `Configuracion.EstadoJustificacion`
- Reemplazar `dbo.Usuarios` con `RecursosHumanos.Usuario`
- Reemplazar `dbo.Cat_TiposJustificacion` con `Configuracion.TipoJustificacion`
- Reemplazar `dbo.fn_AprobadoresVigentesPorSolicitante` con `Operacion.fn_AprobadoresVigentesPorSolicitante`
- Actualizar alias de columnas: `JustificacionID` → `JustificacionId`, `DetalleID` → `JustificacionDetalleId`, etc.
- Actualizar joins: `e.EstadoID` → `e.EstadoJustificacionId`, `u.JefaturaID` → `u.JefaturaId`
- Cambiar `Correo` a `CorreoElectronico` en proyecciones

#### Archivo: `AdminAprobacionesSql.cs`
**Tipo:** Clase estática con constantes de queries SQL  
**Líneas afectadas:** ~100+ líneas distribuidas en 6+ constantes

**Constantes que requieren cambios:**
1. `ListJerarquias` - SELECT FROM dbo.Jerarquias_Aprobacion
2. `CreateJerarquia` - INSERT INTO dbo.Jerarquias_Aprobacion
3. `GetJerarquiaById` - SELECT FROM dbo.Jerarquias_Aprobacion
4. `ToggleJerarquiaEstado` - UPDATE dbo.Jerarquias_Aprobacion
5. `ListDelegaciones` - SELECT FROM dbo.Delegaciones_Aprobacion
6. `CreateDelegacion` - INSERT INTO dbo.Delegaciones_Aprobacion (se debe leer el resto del archivo)

**Cambios específicos:**
- Reemplazar `dbo.Jerarquias_Aprobacion` con `Operacion.JerarquiaAprobacion`
- Reemplazar `dbo.Delegaciones_Aprobacion` con `Operacion.DelegacionAprobacion`
- Actualizar todos los identificadores: `ID` → `Id` (JerarquiaAprobacionID → JerarquiaAprobacionId, etc.)
- Cambiar `EstadoRegistroID` a `EstadoRegistroId`
- Cambiar proyecciones de columnas: `AprobadorUsuarioID AS AprobadorUsuarioId` permanece en estructura pero con nomenclatura uniforme

#### Archivo: `AuditoriaSql.cs`
**Tipo:** Clase estática con constantes de queries SQL  
**Líneas afectadas:** ~15 líneas en 1 constante

**Constantes que requieren cambios:**
1. `InsertEvento` - INSERT INTO dbo.Auditoria_Eventos

**Cambios específicos:**
- Reemplazar `dbo.Auditoria_Eventos` con `Auditoria.EventoAuditoria`
- Actualizar identificadores si aplica (TipoEventoAuditoriaID → TipoEventoAuditoriaId, ResultadoAuditoriaID → ResultadoAuditoriaId)

### 3.2 Infrastructure/Repositories

#### Archivo: `JustificacionRepository.cs`
**Tipo:** Clase sellada que implementa IJustificacionRepository  
**Líneas afectadas:** ~80 líneas

**Métodos/secciones:**
1. `GetExistingTipoJustificacionIdsAsync` - usa JustificacionesSql.GetExistingTipoJustificacionIds
2. `CreateAsync` - usa JustificacionesSql.InsertEncabezado y InsertDetalle

**Cambios específicos:**
- Sin cambios directos de código C#; los cambios en las queries SQL se propagan automáticamente
- Validar que los parámetros se mapeen correctamente (UsuarioID → UsuarioId si aplica en mapeo de DTOs)

#### Archivo: `ErrorLogRepository.cs`
**Tipo:** Clase sellada que implementa IErrorLogRepository  
**Líneas afectadas:** ~20 líneas

**Métodos:**
1. `LogAsync` - INSERT INTO dbo.ApiErrorLog (inline SQL)

**Cambios específicos:**
- Cambiar nombre de tabla: `dbo.ApiErrorLog` → `Auditoria.ErrorApi`
- Revisar si los nombres de parámetros necesitan cambios (probablemente CorrelationID permanece, UsuarioID → UsuarioId si aplica)

#### Archivo: `AuditEventRepository.cs`
**Tipo:** Clase sellada que implementa IAuditEventRepository  
**Líneas afectadas:** ~20 líneas

**Métodos:**
1. `LogEventAsync` - usa AuditoriaSql.InsertEvento

**Cambios específicos:**
- Sin cambios directos de código C#; los cambios en las queries SQL se propagan automáticamente
- Validar que los parámetros en AuditEventEntry mapeen con los nuevos nombres de columnas

#### Archivo: `AdminAprobacionesRepository.cs`
**Tipo:** Clase sellada que implementa IAdminAprobacionesRepository  
**Líneas afectadas:** ~100+ líneas

**Métodos (estimados):**
- `ListJerarquiasAsync`
- `CreateJerarquiaAsync`
- `GetJerarquiaByIdAsync`
- `ToggleJerarquiaEstadoAsync`
- `ListDelegacionesAsync`
- `CreateDelegacionAsync`
- `GetDelegacionByIdAsync`
- `ToggleDelegacionEstadoAsync`

**Cambios específicos:**
- Sin cambios directos de código C# en la lógica; la propagación viene de AdminAprobacionesSql
- Revisar mapeo de parámetros en constructores de objetos anónimos (anonymous types)

---

## 4. Cambios Concretos por Archivo

### 4.1 JustificacionesSql.cs

**Patrón de reemplazo general:**

```sql
-- ANTES (ejemplo de una query):
FROM dbo.Justificaciones_Encabezado je
INNER JOIN dbo.Estados e ON e.EstadoID = je.EstadoID
LEFT JOIN dbo.Justificaciones_Detalle jd ON jd.JustificacionID = je.JustificacionID
INNER JOIN dbo.Usuarios u ON u.UsuarioID = je.UsuarioID

-- DESPUÉS (con nuevos esquemas y convención):
FROM Operacion.Justificacion je
INNER JOIN Configuracion.EstadoJustificacion e ON e.EstadoJustificacionId = je.EstadoJustificacionId
LEFT JOIN Operacion.JustificacionDetalle jd ON jd.JustificacionId = je.JustificacionId
INNER JOIN RecursosHumanos.Usuario u ON u.UsuarioId = je.UsuarioId
```

**Cambios específicos por constante:**

1. **InsertEncabezado**
   - `INSERT INTO dbo.Justificaciones_Encabezado` → `INSERT INTO Operacion.Justificacion`
   - Validar que columnas existan (MotivoGeneral, EstadoJustificacionId [era EstadoID], etc.)

2. **InsertDetalle**
   - `INSERT INTO dbo.Justificaciones_Detalle` → `INSERT INTO Operacion.JustificacionDetalle`
   - `JustificacionID` → `JustificacionId`
   - `TipoJustificacionID` → `TipoJustificacionId`
   - Columna `DetalleID` no existe en la nueva tabla (se reemplaza con JustificacionDetalleId que es auto-generada)

3. **ListMine**
   - Reemplazar todas las referencias de tabla según el patrón anterior
   - `je.EstadoID` → `je.EstadoJustificacionId`
   - `e.EstadoID` → `e.EstadoJustificacionId`

4. **ListPendientesJefatura**
   - Mismos cambios que ListMine
   - Función: `dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE())` → `Operacion.fn_AprobadoresVigentesPorSolicitante(je.UsuarioId, GETDATE())`

5. **ListRrhhGlobal**
   - Reemplazar esquemas de todas las tablas
   - `u.Correo` → `u.CorreoElectronico`
   - `u.JefaturaID` → `u.JefaturaId`
   - `u.UnidadID` → `u.UnidadId`
   - Join adicional posible: si `dbo.Usuarios j` refiere a jefatura, aplicar cambios análogos

6. **GetDetalleJefaturaEncabezado**
   - Cambios análogos a ListMine
   - Agregar referencias con alias correcto para todas las FK

7. **GetDetalleJefaturaLineas**
   - `dbo.Justificaciones_Detalle jd` → `Operacion.JustificacionDetalle jd`
   - `dbo.Cat_TiposJustificacion tj` → `Configuracion.TipoJustificacion tj`
   - `jd.TipoJustificacionID` → `jd.TipoJustificacionId`

8. **GetResolverValidation**
   - `dbo.Justificaciones_Encabezado` → `Operacion.Justificacion`
   - Función: `dbo.fn_AprobadoresVigentesPorSolicitante` → `Operacion.fn_AprobadoresVigentesPorSolicitante`
   - Campos del resultado de función: verificar que `AprobadorUsuarioID`, `Origen`, `DeleganteUsuarioID` existan

9. **GetAprobacionScopeValidation**
   - Idéntico a GetResolverValidation

10. **GetExistingTipoJustificacionIds**
    - `dbo.Cat_TiposJustificacion` → `Configuracion.TipoJustificacion`
    - `TipoJustificacionID` → `TipoJustificacionId` en columnas

11. **ResolverPendiente**
    - `dbo.Justificaciones_Encabezado` → `Operacion.Justificacion`
    - Función: `dbo.fn_AprobadoresVigentesPorSolicitante` → `Operacion.fn_AprobadoresVigentesPorSolicitante`
    - Actualizar todas las referencias de columna

### 4.2 AdminAprobacionesSql.cs

**Cambios por constante:**

1. **ListJerarquias**
   - `dbo.Jerarquias_Aprobacion` → `Operacion.JerarquiaAprobacion`
   - `JerarquiaAprobacionID AS JerarquiaAprobacionId` (ya usa alias correcto, pero cambiar tabla)
   - `AprobadorUsuarioID AS AprobadorUsuarioId`
   - `EstructuraOrganizacionalID AS EstructuraOrganizacionalId`
   - `EstadoRegistroID AS EstadoRegistroId`

2. **CreateJerarquia**
   - `INSERT INTO dbo.Jerarquias_Aprobacion` → `INSERT INTO Operacion.JerarquiaAprobacion`
   - Actualizar nombres de parámetros en INSERT

3. **GetJerarquiaById**
   - Similar a ListJerarquias

4. **ToggleJerarquiaEstado**
   - `UPDATE dbo.Jerarquias_Aprobacion` → `UPDATE Operacion.JerarquiaAprobacion`
   - `JerarquiaAprobacionID` → `JerarquiaAprobacionId`
   - `EstadoRegistroID` → `EstadoRegistroId`

5. **ListDelegaciones**
   - `dbo.Delegaciones_Aprobacion` → `Operacion.DelegacionAprobacion`
   - Actualizar todos los identificadores análogos

6. **CreateDelegacion** (presumiblemente similar a CreateJerarquia)
   - `INSERT INTO dbo.Delegaciones_Aprobacion` → `INSERT INTO Operacion.DelegacionAprobacion`

### 4.3 AuditoriaSql.cs

**Cambios por constante:**

1. **InsertEvento**
   - `INSERT INTO dbo.Auditoria_Eventos` → `INSERT INTO Auditoria.EventoAuditoria`
   - Validar nombres de columnas exactos en la nueva tabla

### 4.4 ErrorLogRepository.cs

**Método LogAsync:**
```csharp
// ANTES:
const string sql = """
    INSERT INTO dbo.ApiErrorLog
        (CorrelationID, HttpMethod, Endpoint, StatusCode, TipoError, Mensaje, StackTrace,
         UsuarioID, RolUsuario, Entorno, Ip, UserAgent)
    VALUES
        (@CorrelationID, @HttpMethod, @Endpoint, @StatusCode, @TipoError, @Mensaje, @StackTrace,
         @UsuarioID, @RolUsuario, @Entorno, @Ip, @UserAgent)
    """;

// DESPUÉS:
const string sql = """
    INSERT INTO Auditoria.ErrorApi
        (CorrelationID, HttpMethod, Endpoint, StatusCode, TipoError, Mensaje, StackTrace,
         UsuarioID, RolUsuario, Entorno, Ip, UserAgent)
    VALUES
        (@CorrelationID, @HttpMethod, @Endpoint, @StatusCode, @TipoError, @Mensaje, @StackTrace,
         @UsuarioID, @RolUsuario, @Entorno, @Ip, @UserAgent)
    """;
```

---

## 5. Validaciones de Compatibilidad

### 5.1 Validación de Queries SQL

- [ ] Todas las referencias a `dbo.*` reemplazadas con esquema + tabla correcta
- [ ] Todos los identificadores (columnas) actualizados a convención PascalCase
- [ ] Joins internos verificados (claves primarias y foráneas coinciden)
- [ ] Función `Operacion.fn_AprobadoresVigentesPorSolicitante` existe y retorna los campos esperados
- [ ] Parámetros @-prefixed mantienen coherencia de tipo (INT donde se espera INT, etc.)
- [ ] ORDER BY, GROUP BY, WHERE usan nombres nuevos correctamente

### 5.2 Validación de Mapeo Dapper/ORM

- [ ] Parámetros anónimos en repositorios coinciden con nuevos nombres de columna
- [ ] Tipos de datos en ExecuteScalarAsync, QueryAsync, ExecuteAsync son correctos
- [ ] Proyecciones en SELECT mapean correctamente a DTOs

### 5.3 Validación de Entity Framework (si aplica)

- [ ] DbContext mappings actualizados (si existen)
- [ ] Nombres de tablas en modelBuilder coinciden con nuevos esquemas
- [ ] Relaciones FK configuradas correctamente

### 5.4 Validación de Auditoría

- [ ] Tabla `Auditoria.EventoAuditoria` existe con las columnas correctas
- [ ] Tabla `Auditoria.ErrorApi` existe con las columnas correctas
- [ ] Parámetros en AuditEventEntry.cs mapean con columnas nuevas

### 5.5 Validación de Datos Existentes

- [ ] Datos migrados correctamente desde tablas antiguas (si existen scripts de migración)
- [ ] Identidades (IDENTITY/SEED) preservadas correctamente
- [ ] Integridad referencial verificada en ambiente de integración

---

## 6. Prioridad de Cambios

### Fase 1 (Crítica - Blocking)
- [ ] JustificacionesSql.cs — Afecta todas las operaciones de justificaciones (RRHH, Jefatura)
- [ ] JustificacionRepository.cs — Implementación de queries críticas
- [ ] AuditoriaSql.cs — Auditoría del sistema

### Fase 2 (Alta - Importante)
- [ ] AdminAprobacionesSql.cs — Flujo de aprobaciones
- [ ] AdminAprobacionesRepository.cs — Gestión de jerarquías y delegaciones

### Fase 3 (Media - Complementaria)
- [ ] ErrorLogRepository.cs — Logging de errores
- [ ] AuditEventRepository.cs — Auditoría de eventos (dependiente de AuditoriaSql)

---

## 7. Archivos Relacionados a Revisar Post-Cambios

### Application Layer (Interfaces)
- `IntegradorMarcas.Application/Interfaces/IJustificacionRepository.cs` — Validar que métodos son compatibles
- `IntegradorMarcas.Application/Interfaces/IAdminAprobacionesService.cs` — Validar DTOs

### DTOs y Mappings
- `IntegradorMarcas.Application/DTOs/JustificacionResumenDto.cs` — Propiedades esperadas
- `IntegradorMarcas.Application/DTOs/JustificacionDetalleDto.cs` — Mapeo de columnas
- Otros DTOs relacionados con Usuario, Jerarquía, Delegación

### Services
- `IntegradorMarcas.Application/Services/JustificacionService.cs` — Lógica de negocio
- `IntegradorMarcas.Application/Services/AdminAprobacionesService.cs` — Lógica de aprobaciones

### API Layer
- `IntegradorMarcas.Api/Controllers/*` — Endpoints que usan repositorios (verificar errores de runtime)

---

## 8. Casos de Prueba Recomendados

### Unit Tests
- [ ] JustificacionRepository.GetExistingTipoJustificacionIdsAsync - inputs válidos e inválidos
- [ ] JustificacionRepository.CreateAsync - inserción completa con detalles
- [ ] AdminAprobacionesRepository.ListJerarquiasAsync - filtros activos/por usuario
- [ ] ErrorLogRepository.LogAsync - inserción exitosa en nueva tabla

### Integration Tests
- [ ] Crear justificación → Verificar en Operacion.Justificacion + Operacion.JustificacionDetalle
- [ ] Aprobar justificación → Verificar actualización en Operacion.Justificacion
- [ ] Listar justificaciones como jefatura → Verificar JOIN con Operacion.fn_AprobadoresVigentesPorSolicitante
- [ ] Crear jerarquía → Verificar en Operacion.JerarquiaAprobacion
- [ ] Log error API → Verificar en Auditoria.ErrorApi

### System Tests
- [ ] Flujo completo: Usuario crea justificación → Jefatura aprueba → RRHH resuelve
- [ ] Validación de datos en Auditoria.EventoAuditoria
- [ ] Performance: queries con nuevos esquemas mantienen índices y tiempo de respuesta

---

## 9. Dependencias Externas

### Bases de Datos
- INTEGRA_CNP debe estar creada con scripts 001_integra_marcas_base_inicial.sql + 002_integra_marcas_objetos.sql
- Función `Operacion.fn_AprobadoresVigentesPorSolicitante` debe estar disponible

### NuGet Packages
- Dapper (usado actualmente) — Compatible con nuevos esquemas
- Microsoft.Data.SqlClient — Sin cambios requeridos

### Configuration
- Connection strings — Sin cambios (misma base INTEGRA_CNP)
- Environment variables — Sin cambios requeridos

---

## 10. Notas de Implementación

1. **Orden de reemplazo:** Iniciar con queries SQL (.cs en Queries/), luego migrar repositorios para validar que referencias se actualizaron.

2. **Testing incremental:** Después de cambiar cada archivo de query, ejecutar pruebas unitarias del repositorio correspondiente.

3. **Rollback strategy:** Si hay problemas, mantener backup de constantes SQL antiguas en rama temporaria.

4. **Comunicación:** Coordinar con equipo de BBDD para verificar que tablas y funciones están activas en ambiente de desarrollo/integración.

5. **Documentation:** Actualizar manual técnico (docs/manual-tecnico.md) con nuevos nombres de tablas y esquemas.

