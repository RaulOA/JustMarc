# Archivos SQL Obsoletos - Consolidación 2026-04-23

## Estado de Consolidación

A partir de 2026-04-23, la estructura de setup SQL se ha consolidado en exactamente **dos archivos canónicos**:

1. **001_integra_marcas_base_inicial.sql** — Base inicial completa e idempotente
2. **002_integra_marcas_objetos.sql** — Objetos dependientes y vistas de integración

## Archivos Obsoletos (SUPERSEDED)

Los siguientes archivos quedan **obsoletos y superseded** por los dos archivos canónicos. Se conservan únicamente con propósito histórico/referencia:

### Exploración y Extracción (NO forman parte del setup final)
- **002_extract_wizdom_readonly.sql** — Exploración WIZDOM
- **003_extract_sifcnp_readonly.sql** — Exploración SIFCNP
- **004_extract_integra_cnp_readonly.sql** — Exploración integra interna
- **005_extract_wizdom_targeted_min.sql** — Extracción WIZDOM mínima
- **006_extract_sifcnp_targeted_min.sql** — Extracción SIFCNP mínima
- **RH_JUSTIFICACIONES_ENC.sql** — Extracción SIFCNP histórica
- **RH_JUSTIFICACIONES_DET.sql** — Extracción SIFCNP histórica

### Scripts Incrementales Consolidados
- **007_integra_local_bridge.sql** — SUPERSEDED por 001 (esquemas stg/bridge/ext no necesarios)
- **008_add_comentario_resolucion.sql** — SUPERSEDED por 001 (integrado en tabla Justificacion)
- **009_admin_hierarchy_delegation_audit_foundation.sql** — SUPERSEDED por 001 (todas las tablas incluidas)
- **010_wizdom_empleado_normalization_staging.sql** — SUPERSEDED por 002 (vistas finales reemplazan staging)

### Original (Reemplazado por 001)
- **001_init_integra_cnp.sql** — SUPERSEDED por 001_integra_marcas_base_inicial.sql

## Razón de la Consolidación

### Antes (Disperso)
- 10 scripts SQL esparcidos
- Tablas, esquemas y convenciones inconsistentes
- Dependencias ocultas entre scripts
- Difícil mantenimiento y auditoría

### Ahora (Consolidado)
- 2 scripts canónicos, claros, idempotentes
- Nomenclatura unificada según Convenciones_Nomeclatura_BD.md
- Dependencia explícita: 002 requiere 001
- Mantenimiento centralizado, versionado

## Cómo Usar el Nuevo Setup

### Instalación completa de cero
```sql
-- Paso 1: Base inicial (obligatorio)
EXECUTE [docs/db/001_integra_marcas_base_inicial.sql]

-- Paso 2: Objetos e integración (requiere paso 1)
EXECUTE [docs/db/002_integra_marcas_objetos.sql]
```

### Para ambientes existentes
- Si la base ya existe, ambos scripts son **idempotentes** (IF NOT EXISTS).
- Se puede reejecutar 002 sin afectar datos.
- Para migrar desde setup antiguo: ejecutar ambos scripts en orden.

## Referencia: Cambios de Nomenclatura

| Antes | Ahora (Canónico) |
|-------|------------------|
| dbo.Roles | Configuracion.Rol |
| dbo.Estados | Configuracion.EstadoJustificacion |
| dbo.Cat_TiposJustificacion | Configuracion.TipoJustificacion |
| dbo.Usuarios | RecursosHumanos.Usuario |
| dbo.Estructuras_Organizacionales | RecursosHumanos.EstructuraOrganizacional |
| dbo.Justificaciones_Encabezado | Operacion.Justificacion |
| dbo.Justificaciones_Detalle | Operacion.JustificacionDetalle |
| dbo.Jerarquias_Aprobacion | Operacion.JerarquiaAprobacion |
| dbo.Delegaciones_Aprobacion | Operacion.DelegacionAprobacion |
| dbo.Auditoria_Eventos | Auditoria.EventoAuditoria |
| dbo.ApiErrorLog | Auditoria.ErrorApi |
| dbo.Cat_EstadosRegistro | Configuracion.EstadoRegistro |
| dbo.Cat_TiposEventoAuditoria | Configuracion.TipoEventoAuditoria |
| dbo.Cat_ResultadosAuditoria | Configuracion.ResultadoAuditoria |

## Vistas de Integración (Nuevas en 002)

Reemplazan enfoque anterior de staging + bridge:

- **Integracion.v_EmpleadoWizdom** — Solo lectura desde WIZDOM.dbo.empleado
- **Integracion.v_OrganigramaWizdom** — Solo lectura desde WIZDOM.dbo.organigrama
- **Integracion.v_JustificacionEncabezadoSifcnp** — Solo lectura desde SIFCNP histórico
- **Integracion.v_JustificacionDetalleSifcnp** — Solo lectura desde SIFCNP histórico

## Para Consultar Histórico Obsoleto

Si necesita revisar un script obsoleto por razones de auditoría histórica:
- Los archivos se conservan en docs/db/
- Todos llevan prefijo numérico indicando fase original
- Se recomienda usar como referencia solamente, no ejecutar contra ambientes vigentes
