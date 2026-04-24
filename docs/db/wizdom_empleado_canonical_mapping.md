# WIZDOM Empleado - Canonical Field Map y Reglas de Normalizacion

Fecha: 2026-04-23

## 1) Fuente canonica
- Tabla fuente primaria: `WIZDOM.dbo.empleado`.
- Clave tecnica de origen: `(compania, codigo_empleado)`.
- Clave de fila para staging: `SourceRowKey = compania + '|' + codigo_empleado`.

## 2) Principios de seguridad de datos
- Nunca convertir `numero_identificacion` a numerico.
- Conservar valores `raw` para trazabilidad (`CompaniaRaw`, `CorreoRaw`, `FechaIngresoRaw`, `FechaEgresoRaw`).
- Convertir placeholders a `NULL` logico por campo.
- Mantener mapeo de compania en capa canonica, no en API.

## 3) Placeholder policy (null-like)
Valores tratados como `NULL` segun campo:
- vacio
- `NULL`
- `N/T`
- `N/A`
- `.`
- `-`
- `--`

Regla especial para fechas:
- `00:00.0` => `NULL`.

## 4) Mapeo canonico (WIZDOM -> staging/bridge)

| WIZDOM.dbo.empleado | Canonico | Tipo sugerido | Regla de normalizacion |
|---|---|---|---|
| compania | CompaniaRaw / CompaniaCanonica | VARCHAR(10) | `1/001/CNP => CNP`, `2/002/FANAL => FANAL`, otro => `NULL` + flag |
| codigo_empleado | CodigoEmpleado | VARCHAR(50) | trim, sin conversion numerica |
| numero_identificacion | NumeroIdentificacion | VARCHAR(50-64) | trim, preservar literal exacto |
| nombre | Nombre | VARCHAR(100) | trim, null-like => `NULL` |
| primer_apellido | Apellido1 | VARCHAR(100) | trim, null-like => `NULL` |
| segundo_apellido | Apellido2 | VARCHAR(100) | trim, null-like => `NULL` |
| correo_electronico_principal | CorreoRaw / Correo | VARCHAR(150) | lower + trim, null-like => `NULL` |
| correo_electronico_alternativo | CorreoAlternativoRaw | VARCHAR(150) | fallback para correo principal |
| codigo_jefe | CodigoJefe | VARCHAR(50) | trim, null-like => `NULL` |
| codigo_nodo_organigrama | CodigoNodoOrganigrama | VARCHAR(50) | trim, null-like => `NULL` |
| estado_empleado | EstadoEmpleado | VARCHAR(30) | upper + trim |
| fecha_ingreso | FechaIngresoRaw / FechaIngreso | VARCHAR(50) + DATE | `00:00.0 => NULL`; parse seguro con `TRY_CONVERT` |
| fecha_egreso | FechaEgresoRaw / FechaEgreso | VARCHAR(50) + DATE | `00:00.0 => NULL`; parse seguro con `TRY_CONVERT` |

## 5) Mapeo bridge -> dbo.Usuarios

| dbo.Usuarios | Origen bridge.vw_UsuariosCanonico | Regla |
|---|---|---|
| Cedula | Cedula | texto, sin coercion numerica |
| NombreCompleto | NombreCompleto | usar nombre completo raw o concatenacion limpia |
| Correo | Correo | principal; fallback alternativo |
| Compania | Compania | solo `CNP`/`FANAL` |
| JefaturaID | CodigoJefe (resolucion posterior) | resolver en segunda fase por codigo empleado |

## 6) Contrato operativo de calidad
- Filas con `CompaniaCanonica IS NULL` se consideran no cargables a `dbo.Usuarios`.
- `NumeroIdentificacion IS NULL` se marca con `IDENTIFICACION_VACIA`.
- Cualquier excepcion de parseo debe quedar como `NULL` y no abortar lote.

## 7) Artefactos relacionados
- `docs/db/007_integra_local_bridge.sql`
- `docs/db/010_wizdom_empleado_normalization_staging.sql`
- `docs/db/005_extract_wizdom_targeted_min.sql`
