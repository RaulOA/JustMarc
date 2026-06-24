# Scripts SQL legados (archivados)

Estos 10 scripts fueron **consolidados** el 2026-06-24 en los tres scripts canónicos
de `docs/db/`:

- `01_CrearBaseDatos.sql`
- `02_EstructuraCompleta.sql`
- `03_DatosSemilla.sql`

Se conservan aquí solo como referencia histórica. **No ejecutar.** El orden y la
lógica vigentes están en los tres scripts consolidados.

## Mapa de consolidación

| Script legado | Destino | Notas |
|---|---|---|
| `001_integra_marcas_base_inicial.sql` | 01 (BD + esquemas) + 02 (tablas, índices, catálogos→03) | Dividido por responsabilidad |
| `002_integra_marcas_objetos.sql` | 02 (función, SP, vistas Integración) | Función movida a `dbo`; SP corregido (`LIMIT`→`TOP`); vistas con guarda `DB_ID` |
| `003_integra_marcas_seed_demo.sql` | **ELIMINADO** | Obsoleto: usaba esquema viejo `dbo.Usuarios` / `dbo.Justificaciones_*` inexistente |
| `004_seed_esquema_correcto.sql` | 03 (Sección B) | Demo mínimo unidad 120 |
| `005_fix_errorapi_schema.sql` | 02 (definición final de `Auditoria.ErrorApi` + bloque de alineación idempotente) | Renombres ya incorporados al `CREATE TABLE` |
| `006_fix_mojibake_historial_textos.sql` | 03 (Sección D, opcional) | Remediación idempotente |
| `007_seed_hierarquia_dependencias.sql` | 03 (Sección C) | Jerarquía de 12 dependencias |
| `008_admin_audit_and_alignment.sql` | 02 (tabla `AdminAccionAuditoria` + columnas) + 03 (tipos evento 8–11) | |
| `fix_fn_aprobadores.sql` | 02 (Sección F) | Versión vigente de la función (`dbo`), reemplaza la de 002 |
| `historico_justificaciones.sql` | 02 (Sección I, con guarda) | Vista legada `dbo.V_JUSTIFICACIONES_DETALLE` |

Ver el detalle completo en `docs/db/Observaciones_Consolidacion_SQL.md`.
