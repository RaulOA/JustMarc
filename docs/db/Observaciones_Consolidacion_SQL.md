# Observaciones — Consolidación y validación de scripts SQL (INTEGRA_CNP)

**Fecha:** 2026-06-24
**Alcance:** validación, depuración, reorganización y análisis de normalización de los
scripts SQL de `docs/db/`.
**Entregables:** `01_CrearBaseDatos.sql`, `02_EstructuraCompleta.sql`, `03_DatosSemilla.sql`
+ este informe.

> **Nota de validación:** los scripts **01 y 02 se ejecutaron con éxito** contra la
> instancia de desarrollo `CNPOCSRBD-V02-3\DESARROLLO` (SQL Server 2019, 15.0.2135.5)
> el 2026-06-24, usando la conexión de `ConnectionStrings__IntegraCnp`. Esa corrida
> reveló hallazgos adicionales (la BD estaba vacía; el esquema real de SIFCNP/WIZDOM
> difiere del que asumían los scripts legados) — ver **§9**. El script 03 (datos
> semilla) **no se ejecutó** por decisión del equipo (alcance "solo estructura").

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
| 2 | **Alta** | El SP `usp_SincronizarJustificacionesDesdeHistorico` (002) usaba `LIMIT 1` (inválido en T-SQL) y además referenciaba un **esquema SIFCNP ficticio** (ver #11). Inservible y no usado por el backend. | `_legacy/002_...sql` líneas 230–232 | **Retirado** de la estructura (Sección G de 02 documenta el esquema real y el re-mapeo pendiente). |
| 3 | **Alta** | `003_integra_marcas_seed_demo.sql` operaba sobre un esquema **inexistente** (`dbo.Usuarios`, `dbo.Justificaciones_Encabezado/_Detalle`). No corresponde al modelo actual (`RecursosHumanos.*`, `Operacion.*`). | `_legacy/003_...sql` | **Eliminado** del flujo. El demo vigente es la Sección B de 03 (ex-004). |
| 4 | Media | `Auditoria.ErrorApi` nacía en español (001) y se renombraba aparte (005). Una BD nueva quedaba desalineada hasta correr 005. | 001 vs 005 vs `ErrorLogRepository.cs` | En 02 la tabla se **crea ya con los nombres finales** (`HttpMethod`, `StatusCode`, `UsuarioID`, `Ip`, `RolUsuario`, `Entorno`, `UserAgent`) + bloque idempotente que **alinea BD legadas**. |
| 5 | Media | Inconsistencia **dentro del backend**: `GetDetalleJefaturaEncabezado` une `dbo.Estructuras_Organizacionales` por `EstructuraOrganizacionalID = UnidadID`, mientras el resto del código usa `RecursosHumanos.EstructuraOrganizacional` por `CodigoOrigen = CAST(UnidadId)`. La tabla `dbo.Estructuras_Organizacionales` no está definida en ningún script. | `JustificacionesSql.cs` línea 266 vs 356 | Documentado (ver §5). Se crea un **shim de compatibilidad** (vista guardada por `OBJECT_ID IS NULL`) en 02 (Sección J) para que la consulta no falle en entornos sin la tabla legada. **Recomendación: corregir el backend.** |
| 6 | Media | Discrepancia de esquema de la función entre 002 (`Operacion`), 007 y repos (`dbo`) y `fix_fn_aprobadores.sql` (`dbo`). | CLAUDE.md gotcha | Unificado en **`dbo`** (la forma que consume el sistema). |
| 7 | Baja | Catálogo `TipoEventoAuditoria` partido entre 001 (1–7) y 008 (8–11). | 001 / 008 | Unificado en un solo `MERGE` (1–11) en 03 (Sección A). |
| 8 | Baja | Objetos dependientes de WIZDOM/SIFCNP en 002 abortaban el script en entornos sin esas BD (las vistas validan columnas al crearse). | 002 | En 02 se crean con **guardas** `DB_ID` / `OBJECT_ID` **y TRY/CATCH** (best-effort): si faltan o el esquema difiere, se omiten con `PRINT` sin abortar. |
| 9 | Baja | Nombres de constraints/índices reales (`FK_`, `IX_`, `DF_`, `CK_`) **no siguen** el formato de `Convenciones_Nomeclatura_BD.md` (`Tabla_Columna`, `TablaOrigen_TablaDestino`, `_Default`). | 001 vs convenciones §8 | **No se renombran** (romperían la BD existente). Se preservan los nombres reales y se documenta la desviación (ver §6). |
| 10 | Baja | `fix_fn_aprobadores.sql` y `historico_justificaciones.sql` eran "fixes" sueltos fuera de la numeración. | — | Integrados en 02 (Secciones F e I). |
| 11 | **Alta** | Las vistas `Integracion.v_*Sifcnp` y el SP de sync (002) asumían columnas SIFCNP **ficticias** (`id_justificacion_enc`, `cedula_funcionario`, `motivo_general`, `estado_justificacion`…) que **no existen**. El SIFCNP real usa `num_justificacion`, `cod_solicitante`, `des_justificacion`, etc. Confirmado al ejecutar (Msg 207). | Ejecución 2026-06-24 vs `SIFCNP.sys.columns` | Vistas `v_*Sifcnp` **realineadas** al esquema real; SP **retirado** (modelo distinto, requiere mapeo). |
| 12 | Media | La vista `Integracion.v_OrganigramaWizdom` (002) usaba columnas ficticias (`codigo_nodo`, `tipo_nodo`, `nivel_jerarquia`, `estado_nodo`). El WIZDOM real usa `codigo_nodo_organigrama`, `codigo_tipo_nodo`, `nivel`, `estado`. (`v_EmpleadoWizdom` sí coincidía.) | Ejecución vs `WIZDOM.sys.columns` | `v_OrganigramaWizdom` **realineada** al esquema real; `v_EmpleadoWizdom` conservada. |

---

## 3. Reorganización (estructura final de 3 scripts)

| Script | Responsabilidad | Contenido |
|---|---|---|
| **`01_CrearBaseDatos.sql`** | Creación + configuración inicial | `CREATE DATABASE INTEGRA_CNP`; 5 esquemas (`Configuracion`, `RecursosHumanos`, `Operacion`, `Auditoria`, `Integracion`). |
| **`02_EstructuraCompleta.sql`** | Estructura completa | 6 catálogos, 2 tablas RRHH, 4 tablas Operación, 3 tablas Auditoría (15 tablas), 16 índices, función `dbo.fn_AprobadoresVigentesPorSolicitante`, 4 vistas `Integracion.*` (realineadas al esquema real), vista legada `dbo.V_JUSTIFICACIONES_DETALLE`, shim `dbo.Estructuras_Organizacionales`. **No hay triggers ni SP** en el sistema (el SP legado se retiró, ver §9). |
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

- **WIZDOM / SIFCNP** (solo lectura): las 4 vistas `Integracion.*` dependen de
  `[WIZDOM].[dbo].*` / `[SIFCNP].[dbo].*`. En 02 se crean **best-effort** (guarda `DB_ID` +
  TRY/CATCH) y fueron **realineadas al esquema real** (ver §9). El SP de sincronización legado
  se **retiró** (asumía un esquema SIFCNP ficticio).
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

1. **Realineación de las vistas de integración** al esquema REAL de WIZDOM/SIFCNP y **retiro del
   SP legado** (basado en columnas inexistentes) — validado ejecutando contra la BD real (§9).
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

---

## 9. Validación en vivo (2026-06-24)

Se ejecutaron **01 y 02** contra `CNPOCSRBD-V02-3\DESARROLLO` (SQL Server 2019, 15.0.2135.5)
usando `Invoke-Sqlcmd -ConnectionString` con la cadena de `ConnectionStrings__IntegraCnp`,
leyendo cada `.sql` como UTF-8 explícito. Procedimiento reutilizable: `docs/db/Ejecutar_Scripts.ps1`
y `docs/db/Runbook_Ejecucion_BD.md`.

**Estado inicial de la BD:** `INTEGRA_CNP` existía pero **sin esquema de aplicación** (solo
contenía la vista `dbo.VW_RH_JUSTIFICACIONES`). Es decir, la corrida creó todo desde cero.

**Hallazgos de la ejecución (no detectables sin BD real):**

1. **SIFCNP real ≠ esquema asumido.** `SIFCNP.dbo.RH_JUSTIFICACIONES_ENC` tiene
   `num_justificacion, cod_solicitante, cod_autoriza, fec_confeccion, des_justificacion,
   cod_centro, cod_estado, fec_autorizacion, cod_aprueba, fec_aprobacion, fec_anulacion,
   cod_anula, des_motivo, des_observaciones`; `_DET` tiene `num_justificacion, cod_linea,
   ind_concepto, fec_justificacion, ind_rebajar_planilla`. Nada que ver con el esquema
   ficticio del SP/vistas legados. → vistas `v_*Sifcnp` **realineadas**, SP **retirado**.
2. **WIZDOM `empleado`** coincide con la vista; **`organigrama`** usaba nombres ficticios →
   `v_OrganigramaWizdom` **realineada** (`codigo_nodo_organigrama`, `codigo_tipo_nodo`, `nivel`,
   `estado`, …).
3. **Bug de comentario:** el texto `Infrastructure/Queries/*.cs` en un comentario abría un
   **comentario anidado** (`/*`), desbalanceando el bloque y abortando el batch. Corregido.

**Resultado verificado tras 01+02:**

| Esquema | Objetos |
|---|---|
| Configuracion | 6 tablas |
| RecursosHumanos | 2 tablas |
| Operacion | 4 tablas |
| Auditoria | 3 tablas |
| dbo | función `fn_AprobadoresVigentesPorSolicitante` + shim `Estructuras_Organizacionales` (vista) |
| Integracion | 4 vistas (creadas OK contra WIZDOM/SIFCNP reales) |

- `Auditoria.ErrorApi` con las 14 columnas del contrato C# (incluye `HttpMethod`, `StatusCode`,
  `UsuarioID`, `Ip`, `RolUsuario`, `Entorno`, `UserAgent`).
- `dbo.fn_AprobadoresVigentesPorSolicitante` ejecutable; `Operacion.fn_...` obsoleta ausente.
- Vista legada `dbo.V_JUSTIFICACIONES_DETALLE` omitida (no existen tablas `dbo.RH_*` locales).

> **Pendiente para que la app funcione:** los **catálogos están vacíos** (no se corrió 03). El
> backend requiere al menos la **Sección A de `03_DatosSemilla.sql`** (Rol, EstadoJustificacion,
> TipoJustificacion, EstadoRegistro, TipoEventoAuditoria, ResultadoAuditoria) para operar.
