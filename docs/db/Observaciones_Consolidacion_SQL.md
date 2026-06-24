# Observaciones — Consolidación y validación de scripts SQL (INTEGRA_CNP)

**Fecha:** 2026-06-24
**Alcance:** validación, depuración, reorganización y análisis de normalización de los
scripts SQL de `docs/db/`.
**Entregables:** `01_CrearBaseDatos.sql`, `02_EstructuraCompleta.sql`, `03_DatosSemilla.sql`
+ este informe.

> **Nota de validación:** los tres scripts fueron revisados estáticamente y diseñados
> para ser idempotentes. **No se ejecutaron contra una instancia real** (no se dispone
> de la cadena de conexión, que se inyecta por variable de entorno). Se recomienda una
> corrida de verificación en un entorno dev antes de promover.

---

## 1. Método de validación

Se contrastó cada script contra la **fuente de verdad real del sistema**: el SQL que el
backend .NET ejecuta. Se inventariaron todas las referencias a objetos en:

- `backend/.../Infrastructure/Queries/*.cs` (Justificaciones, AdminAprobaciones, AdminOrganizacion, Auditoria, AdminActionAudit)
- `backend/.../Infrastructure/Repositories/ErrorLogRepository.cs`
- `backend/.../Api/Controllers/{AdminMonitoringController, SessionController}.cs` (SQL inline)

Esto permitió detectar dónde los scripts **no representaban** el estado que el código exige.

---

## 2. Inconsistencias detectadas

| # | Severidad | Hallazgo | Evidencia | Resolución en los 3 scripts |
|---|---|---|---|---|
| 1 | **Alta** | La función de aprobadores se creaba en `Operacion.fn_...` (002) pero el backend la invoca **7 veces** como `dbo.fn_...`. En esa forma, en una BD recién creada con 002, las consultas de jefatura fallarían. | `JustificacionesSql.cs` líneas 105, 202, 272, 301, 320, 352, 374 | Función creada en **`dbo`** (Sección F de 02). Se hace `DROP` de la versión obsoleta `Operacion.fn_...` si existe. |
| 2 | **Alta** | El SP `usp_SincronizarJustificacionesDesdeHistorico` (002) usaba `LIMIT 1`, sintaxis **inválida en T-SQL**. El `CREATE PROCEDURE` falla al parsear, dejando el objeto sin crear. | `_legacy/002_...sql` líneas 230–232 | Reescrito con subconsulta `(SELECT TOP 1 ...)` (Sección G de 02). |
| 3 | **Alta** | `003_integra_marcas_seed_demo.sql` operaba sobre un esquema **inexistente** (`dbo.Usuarios`, `dbo.Justificaciones_Encabezado/_Detalle`). No corresponde al modelo actual (`RecursosHumanos.*`, `Operacion.*`). | `_legacy/003_...sql` | **Eliminado** del flujo. El demo vigente es la Sección B de 03 (ex-004). |
| 4 | Media | `Auditoria.ErrorApi` nacía en español (001) y se renombraba aparte (005). Una BD nueva quedaba desalineada hasta correr 005. | 001 vs 005 vs `ErrorLogRepository.cs` | En 02 la tabla se **crea ya con los nombres finales** (`HttpMethod`, `StatusCode`, `UsuarioID`, `Ip`, `RolUsuario`, `Entorno`, `UserAgent`) + bloque idempotente que **alinea BD legadas**. |
| 5 | Media | Inconsistencia **dentro del backend**: `GetDetalleJefaturaEncabezado` une `dbo.Estructuras_Organizacionales` por `EstructuraOrganizacionalID = UnidadID`, mientras el resto del código usa `RecursosHumanos.EstructuraOrganizacional` por `CodigoOrigen = CAST(UnidadId)`. La tabla `dbo.Estructuras_Organizacionales` no está definida en ningún script. | `JustificacionesSql.cs` línea 266 vs 356 | Documentado (ver §5). Se crea un **shim de compatibilidad** (vista guardada por `OBJECT_ID IS NULL`) en 02 (Sección J) para que la consulta no falle en entornos sin la tabla legada. **Recomendación: corregir el backend.** |
| 6 | Media | Discrepancia de esquema de la función entre 002 (`Operacion`), 007 y repos (`dbo`) y `fix_fn_aprobadores.sql` (`dbo`). | CLAUDE.md gotcha | Unificado en **`dbo`** (la forma que consume el sistema). |
| 7 | Baja | Catálogo `TipoEventoAuditoria` partido entre 001 (1–7) y 008 (8–11). | 001 / 008 | Unificado en un solo `MERGE` (1–11) en 03 (Sección A). |
| 8 | Baja | Objetos dependientes de WIZDOM/SIFCNP en 002 abortaban el script en entornos sin esas BD (las vistas validan columnas al crearse). | 002 | En 02 se crean con **guardas** `DB_ID('WIZDOM')` / `DB_ID('SIFCNP')` / `OBJECT_ID('dbo.RH_*')`; si faltan, se omiten con `PRINT`. |
| 9 | Baja | Nombres de constraints/índices reales (`FK_`, `IX_`, `DF_`, `CK_`) **no siguen** el formato de `Convenciones_Nomeclatura_BD.md` (`Tabla_Columna`, `TablaOrigen_TablaDestino`, `_Default`). | 001 vs convenciones §8 | **No se renombran** (romperían la BD existente). Se preservan los nombres reales y se documenta la desviación (ver §6). |
| 10 | Baja | `fix_fn_aprobadores.sql` y `historico_justificaciones.sql` eran "fixes" sueltos fuera de la numeración. | — | Integrados en 02 (Secciones F e I). |

---

## 3. Reorganización (estructura final de 3 scripts)

| Script | Responsabilidad | Contenido |
|---|---|---|
| **`01_CrearBaseDatos.sql`** | Creación + configuración inicial | `CREATE DATABASE INTEGRA_CNP`; 5 esquemas (`Configuracion`, `RecursosHumanos`, `Operacion`, `Auditoria`, `Integracion`). |
| **`02_EstructuraCompleta.sql`** | Estructura completa | 6 catálogos, 2 tablas RRHH, 4 tablas Operación, 3 tablas Auditoría, 16 índices, función `dbo.fn_AprobadoresVigentesPorSolicitante`, SP de sincronización, 4 vistas `Integracion.*`, vista legada `dbo.V_JUSTIFICACIONES_DETALLE`, shim `dbo.Estructuras_Organizacionales`. **No hay triggers** en el sistema. |
| **`03_DatosSemilla.sql`** | Datos semilla | A) catálogos (obligatorio); B) demo mínimo unidad 120; C) jerarquía de 12 dependencias; D) remediación de mojibake (opcional). |

**Orden de ejecución:** 01 → 02 → 03. En producción ejecutar de 03 **solo la Sección A**.

---

## 4. Limpieza realizada

- **Archivados** en `docs/db/_legacy/` (con `git mv`, historial preservado): los 10 scripts
  originales (`001`–`008`, `fix_fn_aprobadores.sql`, `historico_justificaciones.sql`).
- **Eliminado del flujo** (obsoleto): `003_integra_marcas_seed_demo.sql` (esquema viejo `dbo.*`).
- **Definiciones redundantes unificadas:** la función de aprobadores (002 vs fix) → una sola
  en `dbo`; el catálogo `TipoEventoAuditoria` (001 + 008) → un solo `MERGE`; los renombres de
  `ErrorApi` (005) → incorporados al `CREATE TABLE`.
- **Conservados sin cambios:** `Convenciones_Nomeclatura_BD.md`, `wizdom_empleado_canonical_mapping.md`
  y los archivos de muestra WIZDOM (`*.csv`, `*.txt`).

---

## 5. Dependencias externas / legadas (documentadas, incluidas con guardas)

- **WIZDOM / SIFCNP** (solo lectura): las 4 vistas `Integracion.*` y el SP de sincronización
  dependen de `[WIZDOM].[dbo].*` / `[SIFCNP].[dbo].*`. En 02 se crean **solo si la BD existe**.
- **`dbo.RH_*` (legado SIFCNP):** la vista `dbo.V_JUSTIFICACIONES_DETALLE` usa mayúsculas
  `MAYUSCULAS_SNAKE` **a propósito** (compatibilidad). Se crea solo si existen las 4 tablas.
- **`dbo.Estructuras_Organizacionales`:** tabla legada referenciada por **una sola** consulta
  del backend, no definida en ningún script. Se provee un **shim (vista)** que mapea a
  `RecursosHumanos.EstructuraOrganizacional` (exponiendo `CodigoOrigen` numérico como
  `EstructuraOrganizacionalID`), creado solo si el objeto no existe.
  **Acción recomendada (backend):** migrar `JustificacionesSql.GetDetalleJefaturaEncabezado`
  para que use `RecursosHumanos.EstructuraOrganizacional` con el join por `CodigoOrigen`,
  igual que `GetCurrentApproverBySolicitante`. Eliminado ese uso, el shim deja de ser necesario.

---

## 6. Normalización (análisis 3FN)

Veredicto: **el modelo cumple la Tercera Forma Normal.** Se verificó tabla por tabla que
todo atributo no-clave depende de la clave completa y solo de la clave.

| Tabla | 1FN | 2FN | 3FN | Observación |
|---|---|---|---|---|
| Catálogos `Configuracion.*` | ✔ | ✔ | ✔ | Atómicos, PK simple. |
| `RecursosHumanos.Usuario` | ✔ | ✔ | ✔ | Ver nota A (Compania) y B (UnidadId). |
| `RecursosHumanos.EstructuraOrganizacional` | ✔ | ✔ | ✔ | Jerarquía por auto-FK; vigencia temporal correcta. |
| `Operacion.Justificacion` | ✔ | ✔ | ✔ | Ver nota C (datos de resolución como snapshot). |
| `Operacion.JustificacionDetalle` | ✔ | ✔ | ✔ | Encabezado/detalle correcto. |
| `Operacion.JerarquiaAprobacion` / `DelegacionAprobacion` | ✔ | ✔ | ✔ | Vigencia y estado normalizados. |
| `Auditoria.*` | ✔ | ✔ | ✔* | Ver nota D (snapshots de auditoría — desnormalización intencional y correcta). |

**Hallazgos y recomendaciones (no aplicados — serían cambios rompedores; ver decisión del equipo):**

- **Nota A — `Usuario.Compania`:** hoy es `VARCHAR(10)` con `CHECK IN ('CNP','FANAL')`. Para
  máxima pureza se podría modelar como catálogo `Configuracion.Compania` con FK. No es una
  violación de 3FN (el dominio es cerrado y estable); el `CHECK` es adecuado. *Mejora opcional.*
- **Nota B — `Usuario.UnidadId` ↔ `EstructuraOrganizacional.CodigoOrigen`:** la relación es
  **lógica, no declarada** (INT vs VARCHAR, unidos por `CAST`). No hay FK que garantice
  integridad referencial. Es el punto más débil del diseño. *Recomendación:* unificar el tipo
  y declarar FK real, o documentar formalmente la relación. Requiere cambios coordinados en
  backend + datos; **no se aplica aquí** por ser rompedor.
- **Nota C — Resolución en `Justificacion`** (`AprobadorId`, `FechaAprobacion`,
  `ComentarioResolucion`, `RolResolucion`): son atributos del propio encabezado que dependen de
  su PK → **cumplen 3FN**. `RolResolucion` es un snapshot del rol al resolver (correcto para
  trazabilidad histórica).
- **Nota D — Auditoría (`EventoAuditoria.NombreUsuario`/`RolCodigo`, `AdminAccionAuditoria.*`):**
  guardan copias del nombre/rol y JSON de valores. Es **desnormalización intencional y
  recomendada** en tablas de bitácora: preservan el valor histórico aunque el maestro cambie.
  No constituyen violación de 3FN en el contexto de auditoría.

---

## 7. Mejoras aplicadas

1. **Corrección de bug** del SP (`LIMIT`→`TOP 1`) — antes el objeto no se podía crear.
2. **Unificación del esquema de la función** en `dbo`, alineado con el backend.
3. **`Auditoria.ErrorApi` correcta desde el `CREATE`** + bloque de migración idempotente.
4. **Guardas de dependencias externas** para que 02 nunca aborte en dev (WIZDOM/SIFCNP ausentes).
5. **Constraints y defaults con nombre explícito** y comentarios técnicos por tabla/columna/SP.
6. **Idempotencia integral** (`IF OBJECT_ID`/`COL_LENGTH`/`sys.indexes`/`MERGE`/`CREATE OR ALTER`).
7. **Separación dev vs prod** clara en el script de semilla (Sección A obligatoria; B/C/D solo dev).
8. **Nota de codificación** (UTF-8 / `sqlcmd -f 65001`) para evitar reintroducir mojibake.

---

## 8. Acciones recomendadas de seguimiento (fuera de alcance de estos scripts)

- [ ] Corregir `GetDetalleJefaturaEncabezado` en el backend (eliminar dependencia de
      `dbo.Estructuras_Organizacionales`).
- [ ] Evaluar Nota B (tipo/FK de `UnidadId`) en una ventana de cambio coordinada.
- [ ] Decidir si se migran las columnas de texto a `NVARCHAR` (prevención definitiva de mojibake;
      guía comentada que estaba en el ex-006).
- [ ] Restringir CORS y endurecer autenticación antes de exponer en producción (ver CLAUDE.md).
