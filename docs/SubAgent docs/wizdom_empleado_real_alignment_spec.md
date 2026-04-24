# WIZDOM Empleado Real Alignment Spec

Fecha: 2026-04-23
Alcance: Alineacion entre estructura real de `WIZDOM.dbo.empleado` (CSV reales adjuntos) y artefactos actuales del repositorio (backend, scripts SQL en `docs/db`, DTOs/queries y documentacion tecnica).

## Fuentes analizadas
- `docs/db/WIZDOM_dbo_empleado_top_1000.csv`
- `docs/db/WIZDOM_dbo_empleado_data.csv`
- `docs/db/[WIZDOM].[dbo].[empleado].txt`
- `docs/db/002_extract_wizdom_readonly.sql`
- `docs/db/005_extract_wizdom_targeted_min.sql`
- `docs/db/007_integra_local_bridge.sql`
- `docs/db/001_init_integra_cnp.sql`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/UsuarioResumenDto.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/RrhhJustificacionResumenDto.cs`
- `backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs`
- Documentos tecnicos: `docs/PRP.md`, `docs/arquitectura-codigo-actual.md`, `docs/manual-tecnico.md`, `docs/flujos-datos-end-to-end.md`, `docs/Guia_Implementacion_Dev_Prod.md`

## 1) Brechas detectadas

### 1.1 Brecha de tipado y formato en identificaciones (alta)
Hallazgo real (CSV):
- `numero_identificacion` contiene valores no uniformes: numericos, notacion cientifica (`1.55802E+11`), alfanumericos (`0116060765A`) y variantes con ceros a la izquierda.
- Perfil rapido sobre `WIZDOM_dbo_empleado_data.csv`:
  - total filas: 1002
  - notacion cientifica en `numero_identificacion`: 13
  - alfanumerico en `numero_identificacion`: 17

Brecha actual:
- El modelo de `Usuarios.Cedula` en `001_init_integra_cnp.sql` es `VARCHAR(20)`, que puede truncar valores y no explicita reglas de preservacion de formato original.
- No existe normalizacion formal para evitar perdida por conversion numerica en ETL/carga.

Impacto:
- Riesgo de colisiones, truncamiento o perdida de ceros a la izquierda.
- Filtros por cedula (`ListRrhhGlobal` en `JustificacionesSql.cs`) con resultados incorrectos si la carga normaliza mal.

### 1.2 Brecha de fechas centinela y parseo de fecha (alta)
Hallazgo real (CSV):
- Uso extensivo de `00:00.0` como valor centinela/no fecha.
- Perfil rapido:
  - `fecha_ingreso = 00:00.0`: 1000/1002
  - `fecha_egreso = 00:00.0`: 685/1002
  - `fecha_nacimiento = 00:00.0`: 987/1002
- Existen ademas fechas con formato textual `dd/MM/yyyy HH:mm` (ej. en `tstamp`).

Brecha actual:
- `stg.Wizdom_EmpleadoRaw` en `007_integra_local_bridge.sql` define `FechaIngreso`/`FechaEgreso` como `DATE` directamente, sin estrategia explicita de parseo y centinelas.
- No hay capa SQL de normalizacion declarada para convertir `00:00.0` a `NULL` y preservar fecha original cruda.

Impacto:
- Fallos de carga por conversion.
- Datos de antiguedad/estado laboral inconsistentes.

### 1.3 Brecha de compania entre fuente real y dominio actual (alta)
Hallazgo real (CSV):
- `compania` real en WIZDOM usa codigos (ej. `1`, `2`) y aparecen filas vacias.

Brecha actual:
- `dbo.Usuarios.Compania` restringida por `CHECK (Compania IN ('CNP', 'FANAL'))` en `001_init_integra_cnp.sql`.
- `JustificacionValidator.ValidateCompania` solo acepta `CNP`/`FANAL`.
- No hay mapeo explicito `WIZDOM.compania -> dominio interno` documentado en scripts.

Impacto:
- Imposibilidad de cargar usuarios si no existe transformacion previa.
- Riesgo de rechazar consultas RRHH por compania aunque la fuente sea valida en WIZDOM.

### 1.4 Brecha de fuente objetivo de extraccion (media-alta)
Hallazgo real:
- Los CSV adjuntos representan claramente `WIZDOM.dbo.empleado` (estructura amplia de 100+ columnas).

Brecha actual:
- `005_extract_wizdom_targeted_min.sql` prioriza vistas candidatas (`optec1empleado`, `RH_FUNCIONARIOS`, etc.) y no fija `dbo.empleado` como fuente canonica obligatoria.
- `007_integra_local_bridge.sql` usa `SourceObject` default `optec1empleado`.

Impacto:
- Desalineacion entre fuente real productiva y fuente asumida por puente.
- Posibles faltantes de campos o diferencias semanticas.

### 1.5 Brecha de robustez ante valores sucios (media)
Hallazgo real:
- Alta presencia de placeholders: `NULL` textual, `N/T`, `N/A`, `.`, `-`, `--`, vacios.
- Perfil rapido:
  - `correo_electronico_principal` null-like: 57
  - `correo_electronico_alternativo` null-like: 577
  - `numero_telefono_principal` null-like: 258
  - `codigo_jefe` null-like: 408
- Variantes ortograficas: `jefe_funiconal` en origen; acentos y espacios extra en nombres.

Brecha actual:
- No existe contrato tecnico de normalizacion por campo en scripts de bridge ni en documentacion tecnica principal.

Impacto:
- Baja calidad de datos en `Usuarios` y errores de relacion de jefaturas.

### 1.6 Brecha de documentacion tecnica vs datos reales (media)
Brecha actual:
- `PRP.md` y documentos tecnicos declaran sincronizacion desde vistas WIZDOM y dominio limpio (`CNP/FANAL`, cedula corta), pero no documentan:
  - Identificaciones en notacion cientifica/alfanumericas.
  - Fechas centinela `00:00.0`.
  - Regla formal de limpieza y trazabilidad de valores raw.

Impacto:
- Implementaciones futuras inconsistentes entre equipos (DBA/backend/QA).

## 2) Campos criticos a usar en el sistema actual

Minimo funcional recomendado para poblar `dbo.Usuarios` y resolver flujos RF-01/RF-03/RF-04:

- Identidad primaria de usuario:
  - `codigo_empleado` (como llave tecnica de sincronizacion externa)
  - `numero_identificacion` (como `Cedula` de negocio, sin coercion numerica)
- Nombre:
  - `nombre`, `primer_apellido`, `segundo_apellido`
  - fallback: concatenacion robusta si alguno viene vacio
- Contacto:
  - `correo_electronico_principal` (fallback a alternativo)
- Jerarquia:
  - `codigo_jefe`
  - `codigo_nodo_organigrama`
- Segmentacion:
  - `compania`
  - `estado_empleado` (activo/inactivo para inclusion operativa)
- Fechas relevantes:
  - `fecha_ingreso`
  - `fecha_egreso`

Campos recomendados para trazabilidad/staging (no exponer directo en API):
- `codigo_tipo_identificacion`, `numero_pasaporte`, `tstamp`, `SourceRowKey`, `HashFila`, `FechaCargaUtc`.

## 3) Mapeo recomendado de nombres/tipos/normalizacion

## 3.1 Regla transversal
- No convertir identificaciones a numerico en ninguna etapa.
- Tratar valores null-like (`NULL`, `N/T`, `N/A`, `.`, `-`, `--`, vacio) como `NULL` logico por campo.
- Mantener columna raw original en staging para auditoria (`*_Raw` o `ValorOriginal`).

## 3.2 Mapeo WIZDOM -> Staging canonico (bridge)

| WIZDOM real | Staging sugerido | Tipo sugerido | Normalizacion |
|---|---|---|---|
| `compania` | `CompaniaRaw` + `CompaniaCanonica` | `VARCHAR(10)` | `TRIM`; mapear `1->CNP`, `2->FANAL`; otros/blank -> `NULL` + flag de calidad |
| `codigo_empleado` | `CodigoEmpleado` | `VARCHAR(50)` | `TRIM`; conservar como string |
| `numero_identificacion` | `NumeroIdentificacion` | `VARCHAR(50)` (ideal 64) | `TRIM`; preservar literal (incluye E+, letras, ceros a la izquierda) |
| `nombre` | `Nombre` | `VARCHAR(100)` | `TRIM`; null-like->NULL |
| `primer_apellido` | `Apellido1` | `VARCHAR(100)` | `TRIM`; null-like->NULL |
| `segundo_apellido` | `Apellido2` | `VARCHAR(100)` | `TRIM`; null-like->NULL |
| `correo_electronico_principal` | `Correo` | `VARCHAR(150)` | lower+trim; null-like->NULL |
| `correo_electronico_alternativo` | `CorreoAlternativo` | `VARCHAR(150)` | fallback si principal null |
| `codigo_jefe` | `CodigoJefe` | `VARCHAR(50)` | `TRIM`; null-like->NULL |
| `codigo_nodo_organigrama` | `CodigoNodoOrganigrama` | `VARCHAR(50)` | `TRIM`; null-like->NULL |
| `estado_empleado` | `EstadoEmpleado` | `VARCHAR(30)` | upper+trim; mapear catalogo (`A`,`I`,`T`, etc.) |
| `fecha_ingreso` | `FechaIngreso` | `DATE` + `FechaIngresoRaw` | `00:00.0`->NULL; parseo seguro `dd/MM/yyyy HH:mm` cuando aplique |
| `fecha_egreso` | `FechaEgreso` | `DATE` + `FechaEgresoRaw` | `00:00.0`->NULL; parseo seguro |

## 3.3 Mapeo Staging -> dbo.Usuarios

| Destino `dbo.Usuarios` | Regla recomendada |
|---|---|
| `Cedula` | desde `NumeroIdentificacion` normalizada como texto; ampliar longitud para evitar truncamiento |
| `NombreCompleto` | `COALESCE(NombreCompletoRaw, concat limpio de nombre+apellidos)` |
| `Correo` | principal valido o alternativo; si ambos null usar placeholder controlado o excluir fila segun regla negocio |
| `JefaturaID` | resolver en segunda fase por `CodigoJefe -> UsuarioID` (post-upsert) |
| `UnidadID` | derivar de `CodigoNodoOrganigrama` con tabla/mapper; si no resoluble usar valor sentinela controlado |
| `Compania` | desde `CompaniaCanonica` (`CNP`,`FANAL`) |
| `RolID` | default funcional (ej. 1) salvo reglas de negocio superiores |

## 3.4 Reglas especificas pedidas
- Fechas `00:00.0`: convertir a `NULL` siempre.
- `NULL` textual y placeholders: convertir a `NULL` por campo.
- Identificaciones:
  - preservar literal original;
  - no castear a float/int;
  - no eliminar ceros a la izquierda;
  - no truncar por longitud insuficiente.

## 4) Cambios concretos de archivos a actualizar y prioridad

## P0 (critico, antes de nuevas cargas)
1. `docs/db/007_integra_local_bridge.sql`
- Cambiar fuente canonica de `SourceObject` de `optec1empleado` a `empleado` (o parametrizable con default `empleado`).
- Agregar columnas raw para fecha/compania/correo y flags de calidad.
- Ajustar `bridge.vw_UsuariosCanonico` para:
  - mapear `CompaniaRaw` numerica a `CNP/FANAL`;
  - transformar null-like;
  - exponer `CedulaNormalizada` textual segura.

2. `docs/db/001_init_integra_cnp.sql`
- Revisar `Cedula VARCHAR(20)` -> ampliar (recomendado `VARCHAR(50)` o `VARCHAR(64)`).
- Mantener `Compania` canonica `CNP/FANAL`, pero documentar que requiere transformacion previa desde WIZDOM.

3. Nuevo script recomendado: `docs/db/010_sync_usuarios_from_wizdom_bridge.sql`
- Implementar `MERGE`/upsert idempotente `bridge.vw_UsuariosCanonico -> dbo.Usuarios`.
- Fase 1: insertar/actualizar usuarios sin `JefaturaID`.
- Fase 2: resolver `JefaturaID` por `CodigoJefe`.
- Registrar excepciones de calidad (filas descartadas) en tabla de auditoria de carga.

## P1 (alto, robustez backend y contratos)
4. `backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs`
- Confirmar que validacion de `Compania` solo se aplique a contratos API internos (CNP/FANAL) y no a datos raw de ingesta.
- Opcional: aceptar alias controlados si se decide exponer filtros con codigos.

5. `backend/src/IntegradorMarcas.Application/DTOs/UsuarioResumenDto.cs`
6. `backend/src/IntegradorMarcas.Api/Contracts/Responses/UsuarioResumenResponse.cs`
7. `backend/src/IntegradorMarcas.Application/DTOs/RrhhJustificacionResumenDto.cs`
- Mantener `Cedula`/`FuncionarioCedula` como `string` (ya correcto).
- Agregar notas de contrato para no asumir numerico.

8. `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- Revisar filtros por cedula (`LIKE`) para soportar valores con letras/simbolos sin normalizaciones destructivas.
- Opcional: aplicar `LTRIM/RTRIM` en columnas de usuario si la carga no limpia completamente.

## P2 (documentacion y gobierno de datos)
9. `docs/PRP.md`
- Actualizar RF-01 con reglas de normalizacion real: compania codificada, cedula textual no numerica, fechas centinela.

10. `docs/manual-tecnico.md`
11. `docs/arquitectura-codigo-actual.md`
12. `docs/flujos-datos-end-to-end.md`
13. `docs/Guia_Implementacion_Dev_Prod.md`
- Incluir seccion de "Data Quality Contract WIZDOM empleado" con:
  - diccionario de mapeo,
  - reglas null-like,
  - manejo de `00:00.0`,
  - validaciones de carga.

14. `docs/db/005_extract_wizdom_targeted_min.sql`
- Actualizar para que `dbo.empleado` sea objetivo prioritario explicito cuando se quiera alinear con fuente real productiva.

## 5) Riesgos y validaciones

## 5.1 Riesgos
- Riesgo de truncamiento de cedulas por longitud actual (`VARCHAR(20)`).
- Riesgo de perdida semantica por coercion numerica de identificaciones (E+, alfanumerico).
- Riesgo de ruptura de FK de jefatura si `codigo_jefe` viene null-like o no mapeable.
- Riesgo de datos inconsistentes de compania por falta de transformacion numerica->canonica.
- Riesgo de errores de carga por fechas centinela no parseables.
- Riesgo de divergencia documental si no se actualiza contrato tecnico.

## 5.2 Validaciones recomendadas (checklist operativo)
1. Validacion de perfil previo a carga (staging):
- % de `numero_identificacion` con E+, alfanumerico, longitud maxima.
- % de `fecha_*` con `00:00.0`.
- % de `compania` fuera de {1,2}.

2. Validacion de transformacion:
- 100% de `CompaniaCanonica` en {CNP,FANAL} para filas cargables.
- 0 conversiones numericas de identificacion.
- Fechas centinela convertidas a `NULL` sin excepciones de parseo.

3. Validacion post-upsert `dbo.Usuarios`:
- Sin truncamientos en `Cedula`.
- `JefaturaID` resoluble esperado vs no resoluble reportado.
- Duplicados por cedula/codigo_empleado monitoreados.

4. Validacion de negocio backend:
- Filtro RRHH por `funcionario` encuentra usuarios por cedula alfanumerica.
- Endpoints actuales no regresan error por compania tras sincronizacion canonica.

5. Validacion de regresion:
- Smoke test completo de endpoints:
  - POST `/api/justificaciones`
  - GET `/api/justificaciones/mias`
  - GET `/api/jefatura/justificaciones/pendientes`
  - GET `/api/jefatura/justificaciones/{id}`
  - PATCH `/api/jefatura/justificaciones/{id}/resolver`
  - GET `/api/rrhh/justificaciones`

## Cierre ejecutivo
El mayor desalineamiento no esta en DTOs de API (que ya usan `string` para cedula), sino en la frontera de datos WIZDOM->bridge->`dbo.Usuarios`: tipado insuficiente de identificacion, falta de contrato de limpieza (`00:00.0`, NULLs textuales), y ausencia de mapeo formal de compania/carga de usuarios. La correccion debe iniciar en SQL de bridge y script de sincronizacion de usuarios (P0), luego consolidar contratos en backend/documentacion (P1/P2).