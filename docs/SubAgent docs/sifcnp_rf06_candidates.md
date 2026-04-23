# SIFCNP RF-06 - Candidate Objects Analysis

## Scope and sources reviewed
- Main source: docs/db/Resultados SIFCNP.rpt
- Related scripts: docs/db/003_extract_sifcnp_readonly.sql, docs/db/004_extract_integra_cnp_readonly.sql, docs/db/001_init_integra_cnp.sql
- Functional context: PRP_Justificacion_Marcas.md (RF-06 read-only historical query requirement)

## Key finding
SIFCNP already contains a coherent HR marks-justification domain with transactional tables plus a consolidated view. The strongest RF-06 candidates are in dbo and directly align to the new INTEGRA_CNP model semantics.

## Top candidates (recommended)

### 1) dbo.RH_JUSTIFICACIONES_ENC (USER_TABLE)
Why it is top candidate:
- Header-level entity for historical justifications.
- Has lifecycle fields needed by RF-06 historical query UI:
  - num_justificacion (business key)
  - cod_solicitante, cod_autoriza, cod_aprueba, cod_anula
  - fec_confeccion, fec_autorizacion, fec_aprobacion, fec_anulacion
  - cod_estado
  - des_justificacion, des_motivo, des_observaciones
- Cardinality in report is meaningful for history use (~17,679 rows), enough to support real historical lookup.

Potential mapping to INTEGRA_CNP:
- Justificaciones_Encabezado: JustificacionID, UsuarioID, MotivoGeneral, EstadoID, FechaCreacion, AprobadorID, FechaAprobacion

### 2) dbo.RH_JUSTIFICACIONES_DET (USER_TABLE)
Why it is top candidate:
- Detail lines associated to header (FK to RH_JUSTIFICACIONES_ENC by num_justificacion is present in report).
- Includes line-level date and concept fields:
  - num_justificacion, cod_linea
  - ind_concepto
  - fec_justificacion
  - ind_rebajar_planilla
- Cardinality is high and suitable for detail drilldown (~26,722 rows).

Potential mapping to INTEGRA_CNP:
- Justificaciones_Detalle: DetalleID, JustificacionID, TipoJustificacionID, FechaMarca

### 3) dbo.RH_TI_TipoMarca (USER_TABLE)
Why it is top candidate:
- Lookup catalog for concept/type used in detail lines.
- Columns:
  - IND_CONCEPTO
  - DES_TIPO_CONCEPTO
- Small and stable (report suggests ~5 rows), ideal for catalog mapping.

Potential mapping to INTEGRA_CNP:
- Cat_TiposJustificacion

### 4) dbo.RH_TI_EstadoMarcas (USER_TABLE)
Why it is top candidate:
- Status catalog for mark-justification workflow.
- Columns:
  - COD_ESTADO
  - DES_ESTADO
- Small and stable (report suggests ~2 rows in current snapshot).

Potential mapping to INTEGRA_CNP:
- Estados

### 5) dbo.V_TI_RH_JUST_MARCA (VIEW)
Why it is top candidate:
- Denormalized read-oriented projection for historical consultation (likely joins ENC + DET + lookup dimensions).
- Contains user-facing fields already shaped for queries:
  - FUNCIONARIO, CENTRO_COSTO, DES_ESTADO, CONCEPTO
  - NUM_JUSTIF, DIA_JUSTIFICA, fec_autorizacion
  - JUSTIFICACION, OBSERVACIONES
- Best option for quick RF-06 UI with minimal backend composition if performance is acceptable.

Recommended role in implementation:
- Use as first read model for RF-06 list/search.
- Keep table-based fallback path if view performance or permissions become an issue.

## Additional context from extraction scripts
- docs/db/003_extract_sifcnp_readonly.sql is metadata-first and read-only by design (READ UNCOMMITTED, lock timeout, optional sampling disabled by default).
- Candidate patterns in script 003 (%just%, %hist%, %marca%) are consistent with selected objects above.
- docs/db/004_extract_integra_cnp_readonly.sql and docs/db/001_init_integra_cnp.sql define the target canonical shape in INTEGRA_CNP (Encabezado/Detalle/Catalogos), which matches the SIFCNP RH objects semantically.

## Risks and notes
- Legacy types text in ENC and view (des_justificacion, des_motivo, des_observaciones) may need explicit handling/mapping in API DTOs.
- cod_estado is char(1), while INTEGRA_CNP Estados uses integer IDs; translation table/mapping rule is required.
- cod_solicitante/cod_autoriza/cod_aprueba/cod_anula appear as numeric identifiers to RH_FUNCIONARIOS; user resolution may require joining HR employee tables/views not yet validated in this pass.

## Minimal read-only validation queries (production-safe, low impact)
Run in short, controlled windows. Keep result sizes low (TOP) and avoid full data extracts.

```sql
USE [SIFCNP];
SET NOCOUNT ON;
SET LOCK_TIMEOUT 3000;
SET TRANSACTION ISOLATION LEVEL READ UNCOMMITTED;
```

### Q1) Object existence and quick table size via metadata only
```sql
SELECT
    s.name AS SchemaName,
    o.name AS ObjectName,
    o.type_desc AS ObjectType,
    COALESCE(SUM(ps.row_count), 0) AS ApproxRows
FROM sys.objects o
JOIN sys.schemas s ON s.schema_id = o.schema_id
LEFT JOIN sys.dm_db_partition_stats ps
    ON ps.object_id = o.object_id
   AND ps.index_id IN (0,1)
WHERE s.name = 'dbo'
  AND o.name IN (
      'RH_JUSTIFICACIONES_ENC',
      'RH_JUSTIFICACIONES_DET',
      'RH_TI_TipoMarca',
      'RH_TI_EstadoMarcas',
      'V_TI_RH_JUST_MARCA'
  )
GROUP BY s.name, o.name, o.type_desc
ORDER BY o.type_desc, o.name;
```

### Q2) Header sample (last records) with narrow projection
```sql
SELECT TOP (30)
    num_justificacion,
    cod_solicitante,
    cod_estado,
    fec_confeccion,
    fec_autorizacion,
    fec_aprobacion
FROM dbo.RH_JUSTIFICACIONES_ENC
ORDER BY num_justificacion DESC;
```

### Q3) Detail sample linked to recent headers
```sql
;WITH H AS (
    SELECT TOP (30) num_justificacion
    FROM dbo.RH_JUSTIFICACIONES_ENC
    ORDER BY num_justificacion DESC
)
SELECT TOP (100)
    d.num_justificacion,
    d.cod_linea,
    d.ind_concepto,
    d.fec_justificacion,
    d.ind_rebajar_planilla
FROM dbo.RH_JUSTIFICACIONES_DET d
JOIN H ON H.num_justificacion = d.num_justificacion
ORDER BY d.num_justificacion DESC, d.cod_linea;
```

### Q4) Catalog dictionaries (very low volume)
```sql
SELECT COD_ESTADO, DES_ESTADO
FROM dbo.RH_TI_EstadoMarcas
ORDER BY COD_ESTADO;

SELECT IND_CONCEPTO, DES_TIPO_CONCEPTO
FROM dbo.RH_TI_TipoMarca
ORDER BY IND_CONCEPTO;
```

### Q5) Consolidated view smoke test
```sql
SELECT TOP (30)
    NUM_JUSTIF,
    FUNCIONARIO,
    DES_ESTADO,
    CONCEPTO,
    DIA_JUSTIFICA,
    fec_autorizacion
FROM dbo.V_TI_RH_JUST_MARCA
ORDER BY NUM_JUSTIF DESC;
```

## Recommendation for RF-06 implementation path
1. Start with V_TI_RH_JUST_MARCA as read model for list/search API.
2. Keep RH_JUSTIFICACIONES_ENC + RH_JUSTIFICACIONES_DET as source-of-truth fallback for detail endpoint.
3. Use RH_TI_EstadoMarcas and RH_TI_TipoMarca to normalize filters/catalogs in UI.
4. Add explicit mapping rules for cod_estado (char) and text legacy fields before exposing API contracts.
