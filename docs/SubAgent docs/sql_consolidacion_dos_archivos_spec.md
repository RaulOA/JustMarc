# Spec de consolidacion SQL a dos archivos

Fecha: 2026-04-23

## 1. Objetivo

Consolidar todos los scripts SQL del proyecto en solo dos archivos finales:

1. `docs/db/001_integra_marcas_base_inicial.sql`
2. `docs/db/002_integra_marcas_objetos.sql`

La consolidacion debe:

- Mantener unicamente los objetos estrictamente necesarios para el sistema realmente implementado hoy.
- Eliminar staging, bridge y scripts exploratorios separados.
- Crear exactamente 4 vistas de solo lectura sobre las 4 tablas externas necesarias.
- Corregir nomenclatura para cumplir la convencion de `docs/db/Convenciones_Nomeclatura_BD.md`.
- Dejar lista la base para sincronizacion desde WIZDOM y consulta historica desde SIFCNP.

## 2. Hallazgos que gobiernan la propuesta

### 2.1 Objetos internos realmente usados por el sistema actual

El backend expuesto hoy consume objetos de estas areas:

- Flujo operativo de boletas:
  - `Justificaciones_Encabezado`
  - `Justificaciones_Detalle`
  - `Estados`
  - `Cat_TiposJustificacion`
  - `Usuarios`
- Logging tecnico:
  - `ApiErrorLog`
- Administracion ya expuesta por API:
  - `Cat_EstadosRegistro`
  - `Estructuras_Organizacionales`
  - `Jerarquias_Aprobacion`
  - `Delegaciones_Aprobacion`
  - `Cat_TiposEventoAuditoria`
  - `Cat_ResultadosAuditoria`
  - `Auditoria_Eventos`
  - `fn_AprobadoresVigentesPorSolicitante`
- Catalogo de roles:
  - `Roles`

Conclusión: los objetos de administracion y auditoria ya no son "futuros"; forman parte de la base minima vigente porque el backend los registra en DI y expone endpoints administrativos.

### 2.2 Fuentes externas estrictamente necesarias

Las 4 tablas externas necesarias para el estado actual y la evolucion inmediata del sistema son:

1. `WIZDOM.dbo.empleado`
2. `WIZDOM.dbo.organigrama`
3. `SIFCNP.dbo.RH_JUSTIFICACIONES_ENC`
4. `SIFCNP.dbo.RH_JUSTIFICACIONES_DET`

Justificacion:

- `WIZDOM.dbo.empleado` es la fuente canonica de funcionarios.
- `WIZDOM.dbo.organigrama` es necesaria para unidad, jerarquia base y carga de estructura organizacional.
- `SIFCNP.dbo.RH_JUSTIFICACIONES_ENC` y `SIFCNP.dbo.RH_JUSTIFICACIONES_DET` son el minimo historico util para RF-06.
- `RH_TI_TipoMarca`, `RH_TI_EstadoMarcas` y `V_TI_RH_JUST_MARCA` no son estrictamente necesarias hoy porque el backend actual no consume consulta historica real; si se requiere descripcion catalogada en una fase posterior, se incorporan despues sin romper esta consolidacion.

### 2.3 Problemas de la estructura actual

- `001`, `007`, `008`, `009`, `010` se reparten la base real en multiples scripts incrementales.
- `002`, `003`, `005`, `006`, `RH_JUSTIFICACIONES_ENC.sql` y `RH_JUSTIFICACIONES_DET.sql` son artefactos de exploracion/extraccion, no parte del setup final.
- Los nombres actuales incumplen la convencion obligatoria:
  - uso de `dbo` para dominio funcional
  - uso de underscores en tablas
  - uso de sufijos `ID`
  - columnas `Usr_Registro`, `Fec_Registro`, etc.
  - constraints e indices con nombres heterogeneos
- La capa `stg` / `bridge` / `ext` agrega complejidad que hoy no es consumida por el backend productivo.

## 3. Propuesta exacta de dos archivos finales

## 3.1 Archivo 1: `docs/db/001_integra_marcas_base_inicial.sql`

Responsabilidad: crear la base funcional minima completa e idempotente.

Debe incluir:

- Creacion de base de datos `INTEGRA_CNP`.
- Creacion de esquemas:
  - `Configuracion`
  - `RecursosHumanos`
  - `Operacion`
  - `Auditoria`
  - `Integracion`
- Creacion de tablas de dominio y administracion.
- Constraints, defaults e indices con nombres explicitos y alineados a la convencion.
- Datos semilla minimos.
- Columnas que hoy estaban en `008` y `009`, integradas desde el inicio.

### Tablas que deben quedar en el archivo 1

#### Esquema `Configuracion`

- `Configuracion.Rol`
- `Configuracion.EstadoJustificacion`
- `Configuracion.TipoJustificacion`
- `Configuracion.EstadoRegistro`
- `Configuracion.TipoEventoAuditoria`
- `Configuracion.ResultadoAuditoria`

#### Esquema `RecursosHumanos`

- `RecursosHumanos.Usuario`
- `RecursosHumanos.EstructuraOrganizacional`

#### Esquema `Operacion`

- `Operacion.Justificacion`
- `Operacion.JustificacionDetalle`
- `Operacion.JerarquiaAprobacion`
- `Operacion.DelegacionAprobacion`

#### Esquema `Auditoria`

- `Auditoria.EventoAuditoria`
- `Auditoria.ErrorApi`

### Observaciones obligatorias para el archivo 1

- `ComentarioResolucion` ya no va en migracion separada; nace en `Operacion.Justificacion`.
- `RolResolucion` ya no va en migracion separada; nace en `Operacion.Justificacion`.
- `Cedula` debe mantenerse como texto amplio. Recomendado: `VARCHAR(64)`.
- La carga de usuarios debe preservar identificaciones alfanumericas y notacion cientifica como texto literal.
- Debe mantenerse `Compania` canonica en el dominio interno con `CNP` y `FANAL`.

## 3.2 Archivo 2: `docs/db/002_integra_marcas_objetos.sql`

Responsabilidad: crear objetos dependientes, logica de aprobacion e integracion externa de solo lectura.

Debe incluir:

- Funcion de alcance de aprobadores.
- Exactamente 4 vistas de solo lectura sobre tablas externas.
- Procedimientos o bloques idempotentes opcionales de sincronizacion desde WIZDOM hacia tablas internas, si el equipo decide automatizar la carga en la misma consolidacion.

### Objeto funcional obligatorio

- `Operacion.fn_AprobadoresVigentesPorSolicitante`

### 4 vistas de solo lectura obligatorias

1. `Integracion.v_EmpleadoWizdom`
2. `Integracion.v_OrganigramaWizdom`
3. `Integracion.v_JustificacionEncabezadoSifcnp`
4. `Integracion.v_JustificacionDetalleSifcnp`

Estas cuatro vistas reemplazan el enfoque de `stg` y `bridge` para la consolidacion actual.

## 4. Tablas y vistas necesarias

## 4.1 Tablas internas necesarias para el sistema actual

- `Configuracion.Rol`
- `Configuracion.EstadoJustificacion`
- `Configuracion.TipoJustificacion`
- `Configuracion.EstadoRegistro`
- `Configuracion.TipoEventoAuditoria`
- `Configuracion.ResultadoAuditoria`
- `RecursosHumanos.Usuario`
- `RecursosHumanos.EstructuraOrganizacional`
- `Operacion.Justificacion`
- `Operacion.JustificacionDetalle`
- `Operacion.JerarquiaAprobacion`
- `Operacion.DelegacionAprobacion`
- `Auditoria.EventoAuditoria`
- `Auditoria.ErrorApi`

## 4.2 Tablas externas estrictamente necesarias

- `WIZDOM.dbo.empleado`
- `WIZDOM.dbo.organigrama`
- `SIFCNP.dbo.RH_JUSTIFICACIONES_ENC`
- `SIFCNP.dbo.RH_JUSTIFICACIONES_DET`

## 4.3 Vistas externas finales

- `Integracion.v_EmpleadoWizdom`
- `Integracion.v_OrganigramaWizdom`
- `Integracion.v_JustificacionEncabezadoSifcnp`
- `Integracion.v_JustificacionDetalleSifcnp`

## 5. Propuesta exacta de renombre por convencion

## 5.1 Esquemas

- `dbo` de negocio se reemplaza por esquemas funcionales.
- `stg`, `bridge`, `ext` se eliminan en esta consolidacion.

## 5.2 Tablas actuales -> tablas finales

- `dbo.Roles` -> `Configuracion.Rol`
- `dbo.Estados` -> `Configuracion.EstadoJustificacion`
- `dbo.Cat_TiposJustificacion` -> `Configuracion.TipoJustificacion`
- `dbo.Usuarios` -> `RecursosHumanos.Usuario`
- `dbo.Justificaciones_Encabezado` -> `Operacion.Justificacion`
- `dbo.Justificaciones_Detalle` -> `Operacion.JustificacionDetalle`
- `dbo.Cat_EstadosRegistro` -> `Configuracion.EstadoRegistro`
- `dbo.Cat_TiposEventoAuditoria` -> `Configuracion.TipoEventoAuditoria`
- `dbo.Cat_ResultadosAuditoria` -> `Configuracion.ResultadoAuditoria`
- `dbo.Estructuras_Organizacionales` -> `RecursosHumanos.EstructuraOrganizacional`
- `dbo.Jerarquias_Aprobacion` -> `Operacion.JerarquiaAprobacion`
- `dbo.Delegaciones_Aprobacion` -> `Operacion.DelegacionAprobacion`
- `dbo.Auditoria_Eventos` -> `Auditoria.EventoAuditoria`
- `dbo.ApiErrorLog` -> `Auditoria.ErrorApi`

## 5.3 Vistas actuales -> vistas finales

- `bridge.vw_UsuariosCanonico` se elimina; su reemplazo funcional es `Integracion.v_EmpleadoWizdom`.
- `bridge.vw_SifcnpEstadosCanonico` se elimina.
- `bridge.vw_SifcnpTiposCanonico` se elimina.
- `bridge.vw_SifcnpHistoricoEncabezadoCanonico` se reemplaza por `Integracion.v_JustificacionEncabezadoSifcnp`.
- `bridge.vw_SifcnpHistoricoDetalleCanonico` se reemplaza por `Integracion.v_JustificacionDetalleSifcnp`.
- `bridge.vw_SifcnpHistoricoCompletoCanonico` no se conserva en la consolidacion minima.
- `bridge.vw_WizdomEmpleadoSourceReal` y `bridge.vw_WizdomEmpleadoNormalizationPreview` se eliminan; su responsabilidad se absorbe en `Integracion.v_EmpleadoWizdom`.

## 5.4 Columnas que deben renombrarse obligatoriamente

### Regla general

- `ID` pasa a `Id`.
- `Usr_Registro` pasa a `CreadoPor`.
- `Fec_Registro` pasa a `FechaHoraCreacion`.
- `Usr_Modifica` pasa a `ModificadoPor`.
- `Fec_Modifica` pasa a `FechaHoraModificacion`.

### Ejemplos concretos

- `RolID` -> `RolId`
- `EstadoID` -> `EstadoJustificacionId` o `EstadoRegistroId` segun contexto
- `TipoJustificacionID` -> `TipoJustificacionId`
- `UsuarioID` -> `UsuarioId`
- `JefaturaID` -> `JefaturaUsuarioId`
- `AprobadorID` -> `AprobadorUsuarioId`
- `DetalleID` -> `JustificacionDetalleId`
- `ErrorLogID` -> `ErrorApiId`
- `Fec_Registro` -> `FechaHoraCreacion`
- `Fec_Modifica` -> `FechaHoraModificacion`
- `FechaCreacion` -> `FechaHoraCreacion`
- `FechaAprobacion` -> `FechaHoraAprobacion`
- `FechaEvento` -> `FechaHoraEvento`
- `RolCodigo` se conserva porque describe codigo funcional del rol

### Constraints e indices

Todos los nombres deben ser explicitos y sin autogeneracion. Ejemplos:

- PK `Operacion.Justificacion`: `Justificacion_JustificacionId`
- FK `Operacion.Justificacion` -> `RecursosHumanos.Usuario` solicitante: `Justificacion_UsuarioSolicitante`
- FK `Operacion.Justificacion` -> `RecursosHumanos.Usuario` aprobador: `Justificacion_UsuarioAprobador`
- FK `Operacion.JustificacionDetalle` -> `Operacion.Justificacion`: `JustificacionDetalle_Justificacion`
- Default `Configuracion.Rol.FechaHoraCreacion`: `Rol_FechaHoraCreacion_Default`
- Indice `Operacion.Justificacion(UsuarioId)`: `Justificacion_UsuarioId`

## 6. Definicion exacta de las 4 vistas externas

## 6.1 `Integracion.v_EmpleadoWizdom`

Base:

- `WIZDOM.dbo.empleado`

Campos minimos a exponer:

- `CompaniaCodigoOrigen`
- `Compania`
- `CodigoEmpleado`
- `Cedula`
- `Nombre`
- `PrimerApellido`
- `SegundoApellido`
- `NombreCompleto`
- `CorreoElectronico`
- `CodigoJefe`
- `CodigoNodoOrganigrama`
- `EstadoEmpleado`
- `FechaIngreso`
- `FechaEgreso`
- `FechaHoraOrigen`

Reglas:

- `numero_identificacion` siempre como texto.
- `00:00.0` a `NULL` en fechas.
- `1/001/CNP -> CNP`; `2/002/FANAL -> FANAL`.
- placeholders (`NULL`, `N/T`, `N/A`, `.`, `-`, `--`) a `NULL`.

## 6.2 `Integracion.v_OrganigramaWizdom`

Base:

- `WIZDOM.dbo.organigrama`

Campos minimos a exponer:

- `CompaniaCodigoOrigen`
- `Compania`
- `CodigoNodoOrganigrama`
- `NombreNodoOrganigrama`
- `CodigoNodoOrganigramaPadre`
- `CodigoJefe`
- `Nivel`
- `EstadoNodo`
- `FechaHoraOrigen`

## 6.3 `Integracion.v_JustificacionEncabezadoSifcnp`

Base:

- `SIFCNP.dbo.RH_JUSTIFICACIONES_ENC`

Campos minimos a exponer:

- `NumeroJustificacion`
- `CodigoSolicitante`
- `CodigoEstado`
- `FechaHoraConfeccion`
- `FechaHoraAutorizacion`
- `FechaHoraAprobacion`
- `DescripcionJustificacion`
- `DescripcionMotivo`
- `DescripcionObservacion`

## 6.4 `Integracion.v_JustificacionDetalleSifcnp`

Base:

- `SIFCNP.dbo.RH_JUSTIFICACIONES_DET`

Campos minimos a exponer:

- `NumeroJustificacion`
- `CodigoLinea`
- `CodigoConcepto`
- `FechaJustificacion`
- `EsRebajaPlanilla`

## 7. Joins cross-database y politica obligatoria de COLLATE

## 7.1 Regla obligatoria

Todo `JOIN`, `UNION`, comparacion, filtro o concatenacion entre columnas de texto provenientes de:

- `INTEGRA_CNP` y `WIZDOM`
- `INTEGRA_CNP` y `SIFCNP`
- dos bases externas con colaciones distintas

Debe usar `COLLATE DATABASE_DEFAULT` en ambos lados de la comparacion.

No se debe asumir que las colaciones de servidor, instancia o base coinciden.

## 7.2 Casos exactos donde debe aplicarse

1. Cruce de empleados con organigrama en WIZDOM:

```sql
ON e.compania COLLATE DATABASE_DEFAULT = o.compania COLLATE DATABASE_DEFAULT
AND e.codigo_nodo_organigrama COLLATE DATABASE_DEFAULT = o.codigo_nodo_organigrama COLLATE DATABASE_DEFAULT
```

2. Cruce de funcionario con jefatura por codigo de empleado:

```sql
ON e.compania COLLATE DATABASE_DEFAULT = j.compania COLLATE DATABASE_DEFAULT
AND e.codigo_jefe COLLATE DATABASE_DEFAULT = j.codigo_empleado COLLATE DATABASE_DEFAULT
```

3. Cruce de encabezado con detalle historico en SIFCNP:

```sql
ON enc.num_justificacion COLLATE DATABASE_DEFAULT = det.num_justificacion COLLATE DATABASE_DEFAULT
```

4. Futuras sincronizaciones de vistas externas a tablas internas:

```sql
ON destino.Cedula COLLATE DATABASE_DEFAULT = origen.Cedula COLLATE DATABASE_DEFAULT
```

## 7.3 Regla adicional de normalizacion

En vistas externas, toda columna textual expuesta debe salir ya convertida a la colacion local mediante:

```sql
CAST(columna AS VARCHAR(n)) COLLATE DATABASE_DEFAULT
```

cuando la columna de origen participe en comparaciones o pueda ser reutilizada por objetos locales.

## 8. Archivos existentes que quedan obsoletos y reemplazo

### Se reemplazan completamente

- `docs/db/001_init_integra_cnp.sql`
  - reemplazo: `docs/db/001_integra_marcas_base_inicial.sql`
- `docs/db/007_integra_local_bridge.sql`
  - reemplazo: `docs/db/002_integra_marcas_objetos.sql`
- `docs/db/008_add_comentario_resolucion.sql`
  - reemplazo: integrado dentro de `001_integra_marcas_base_inicial.sql`
- `docs/db/009_admin_hierarchy_delegation_audit_foundation.sql`
  - reemplazo: integrado dentro de `001_integra_marcas_base_inicial.sql` y `002_integra_marcas_objetos.sql`
- `docs/db/010_wizdom_empleado_normalization_staging.sql`
  - reemplazo: `Integracion.v_EmpleadoWizdom` dentro de `002_integra_marcas_objetos.sql`

### Quedan obsoletos como artefactos exploratorios

- `docs/db/002_extract_wizdom_readonly.sql`
- `docs/db/003_extract_sifcnp_readonly.sql`
- `docs/db/004_extract_integra_cnp_readonly.sql`
- `docs/db/005_extract_wizdom_targeted_min.sql`
- `docs/db/006_extract_sifcnp_targeted_min.sql`
- `docs/db/RH_JUSTIFICACIONES_ENC.sql`
- `docs/db/RH_JUSTIFICACIONES_DET.sql`

Razon:

- no forman parte del setup final
- sirven solo para descubrimiento/perfilado o exportacion puntual
- su responsabilidad queda absorbida por las 4 vistas permanentes de `Integracion`

### Se conservan solo como referencia documental

- `docs/db/[WIZDOM].[dbo].[empleado].txt`
- `docs/db/[WIZDOM].[dbo].[organigrama].txt`
- `docs/db/wizdom_empleado_canonical_mapping.md`
- CSV y reportes adjuntos

## 9. Orden final de ejecucion

1. Ejecutar `docs/db/001_integra_marcas_base_inicial.sql`
2. Ejecutar `docs/db/002_integra_marcas_objetos.sql`

No debe existir un tercer script funcional para setup base.

## 10. Impacto tecnico obligatorio fuera del SQL

Si se implementa esta consolidacion con renombre real de objetos, el backend debe actualizar sus consultas para apuntar a los nuevos nombres y esquemas.

Impactos directos:

- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AuditoriaSql.cs`
- documentacion tecnica de despliegue y arquitectura

## 11. Decision final recomendada

La consolidacion debe adoptar estas decisiones exactas:

- Reducir el setup a 2 scripts definitivos.
- Eliminar `stg`, `bridge` y `ext` del paquete final.
- Mantener solo 14 tablas internas y 1 funcion porque son las estrictamente necesarias para el sistema realmente implementado hoy.
- Crear exactamente 4 vistas de solo lectura sobre las 4 tablas externas minimas:
  - `WIZDOM.dbo.empleado`
  - `WIZDOM.dbo.organigrama`
  - `SIFCNP.dbo.RH_JUSTIFICACIONES_ENC`
  - `SIFCNP.dbo.RH_JUSTIFICACIONES_DET`
- Aplicar `COLLATE DATABASE_DEFAULT` de forma obligatoria en cualquier cruce textual cross-database.
- Renombrar esquemas, tablas, vistas, constraints y columnas al estandar de la convencion adjunta, sin mantener nombres legacy en los scripts nuevos.

## 12. Resumen ejecutivo

La base actual esta fragmentada entre un script inicial, migraciones incrementales y varios scripts de exploracion. El codigo vigente demuestra que la base minima real no es solo el MVP de boletas; tambien incluye administracion de jerarquias, delegaciones, auditoria funcional y logging tecnico. Por tanto, la consolidacion correcta no es "dejar menos tablas" sino dejar solo las tablas que el backend realmente usa hoy, y sacar del setup final todo lo que era staging, bridge y perfilado.

La propuesta exacta es cerrar el proyecto en dos archivos finales: uno de base inicial con 14 tablas internas ya normalizadas por convencion y otro de objetos con la funcion de aprobadores y cuatro vistas externas de solo lectura. Las cuatro tablas externas estrictamente necesarias son `WIZDOM.dbo.empleado`, `WIZDOM.dbo.organigrama`, `SIFCNP.dbo.RH_JUSTIFICACIONES_ENC` y `SIFCNP.dbo.RH_JUSTIFICACIONES_DET`. Todo cruce textual entre bases debe usar `COLLATE DATABASE_DEFAULT` de forma explicita.
