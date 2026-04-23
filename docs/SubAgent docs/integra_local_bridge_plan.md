# INTEGRA_CNP Local Bridge Plan

## 1. Objetivo y alcance
Adaptar la arquitectura para que el backend de Marcas se conecte unicamente a INTEGRA_CNP, y que INTEGRA_CNP concentre:
- Tablas transaccionales locales (ya existentes).
- Tablas de staging para datos extraidos de WIZDOM y SIFCNP.
- Vistas canonicas de puente para consumo interno.

Base del analisis:
- PRP y arquitectura objetivo en [PRP_Justificacion_Marcas.md](PRP_Justificacion_Marcas.md)
- DDL inicial en [docs/db/001_init_integra_cnp.sql](docs/db/001_init_integra_cnp.sql)
- Scripts de descubrimiento/extraccion en [docs/db/002_extract_wizdom_readonly.sql](docs/db/002_extract_wizdom_readonly.sql), [docs/db/003_extract_sifcnp_readonly.sql](docs/db/003_extract_sifcnp_readonly.sql), [docs/db/005_extract_wizdom_targeted_min.sql](docs/db/005_extract_wizdom_targeted_min.sql), [docs/db/006_extract_sifcnp_targeted_min.sql](docs/db/006_extract_sifcnp_targeted_min.sql)
- Supuestos de backend en [backend/src/IntegradorMarcas.Infrastructure/Data/SqlConnectionFactory.cs](backend/src/IntegradorMarcas.Infrastructure/Data/SqlConnectionFactory.cs), [backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs](backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs), [backend/src/IntegradorMarcas.Api/appsettings.json](backend/src/IntegradorMarcas.Api/appsettings.json)

## 2. Hallazgos concisos
1. El backend actual usa solo la conexion IntegraCnp en tiempo de ejecucion.
2. No hay consultas activas en codigo a WIZDOM ni a SIFCNP; los connection strings existen pero no se usan en repositorios.
3. El modelo transaccional actual en INTEGRA_CNP (Usuarios, Justificaciones_Encabezado, Justificaciones_Detalle, catalogos) soporta los endpoints implementados.
4. Los scripts de WIZDOM y SIFCNP existentes son de descubrimiento/extraccion read-only, no crean objetos puente dentro de INTEGRA_CNP.

Conclusion: la estrategia de puente local puede implementarse de forma aditiva, sin romper endpoints actuales.

## 3. Objetos requeridos en INTEGRA_CNP

### 3.1 Esquemas nuevos
Crear ahora:
- stg
- bridge
- ext

### 3.2 Tablas de staging (crear ahora)
WIZDOM (usuarios y jerarquia):
- stg.Wizdom_EmpleadoRaw
- stg.Wizdom_RelacionEmpleadoRaw

SIFCNP (historico RF-06):
- stg.Sifcnp_JustificacionesEncRaw
- stg.Sifcnp_JustificacionesDetRaw
- stg.Sifcnp_TipoMarcaRaw
- stg.Sifcnp_EstadoMarcasRaw

Control de cargas:
- stg.BridgeCargaLote

Notas de columnas minimas sugeridas para raw:
- Identificadores de fuente (SourceSystem, SourceObject, SourceRowKey)
- Campos funcionales minimos detectados en scripts targeted:
  - WIZDOM: compania, codigo_empleado, numero_identificacion, nombre/apellidos, correo, codigo_jefe, codigo_nodo_organigrama, estado_empleado, fecha_ingreso, fecha_egreso
  - SIFCNP ENC: num_justificacion, cod_solicitante, cod_estado, fec_confeccion, fec_autorizacion, fec_aprobacion, des_justificacion, des_motivo, des_observaciones
  - SIFCNP DET: num_justificacion, cod_linea, ind_concepto, fec_justificacion, ind_rebajar_planilla
  - Catalogos: RH_TI_TipoMarca (IND_CONCEPTO, DES_TIPO_CONCEPTO), RH_TI_EstadoMarcas (COD_ESTADO, DES_ESTADO)
- Metadatos de ingest: FechaCargaUtc, HashFila, LoteID

### 3.3 Vistas canonicas de puente (crear ahora)
Usuarios canonicos desde staging WIZDOM:
- bridge.vw_UsuariosCanonico

Historico RF-06 desde staging SIFCNP:
- bridge.vw_SifcnpHistoricoEncabezadoCanonico
- bridge.vw_SifcnpHistoricoDetalleCanonico
- bridge.vw_SifcnpHistoricoCompletoCanonico

Catalogos canonicos historicos:
- bridge.vw_SifcnpEstadosCanonico
- bridge.vw_SifcnpTiposCanonico

Proposito:
- Estandarizar nombres/campos sin depender de linked servers ni tres-part names.
- Permitir evolucion de ingest sin tocar consultas consumidoras.

### 3.4 Synonyms placeholders opcionales (crear placeholders ahora, synonym real despues)
Para dejar contratos de nombres listos sin acceso externo:
- ext.Src_WIZDOM_optec1empleado
- ext.Src_WIZDOM_relaciones_empleado
- ext.Src_SIFCNP_RH_JUSTIFICACIONES_ENC
- ext.Src_SIFCNP_RH_JUSTIFICACIONES_DET
- ext.Src_SIFCNP_RH_TI_TipoMarca
- ext.Src_SIFCNP_RH_TI_EstadoMarcas
- ext.Src_SIFCNP_V_TI_RH_JUST_MARCA

Recomendacion tecnica:
- Crear un script de placeholders con bloques IF DB_ID(...) IS NOT NULL para crear synonyms reales solo cuando exista conectividad.
- Mientras tanto, mantener estos nombres reservados en documentacion y en vistas basadas en stg.

## 4. Cambios minimos a scripts existentes

## 4.1 Estrategia recomendada
Aplicar dos cambios:
1. Editar [docs/db/001_init_integra_cnp.sql](docs/db/001_init_integra_cnp.sql) con cambios minimos no disruptivos.
2. Agregar un unico script nuevo [docs/db/007_integra_local_bridge.sql](docs/db/007_integra_local_bridge.sql) con todos los objetos de puente.

## 4.2 Edicion minima en 001_init_integra_cnp.sql
Agregar al final, sin tocar tablas transaccionales existentes:
- Creacion idempotente de esquemas stg, bridge, ext.
- Comentario de referencia de migracion: ejecutar 007_integra_local_bridge.sql.

No modificar:
- Definiciones actuales de Roles, Estados, Cat_TiposJustificacion, Usuarios, Justificaciones_Encabezado, Justificaciones_Detalle.
- Seeds actuales.

## 4.3 Nuevo script 007_integra_local_bridge.sql
Debe contener en orden:
1. Preconditions (USE INTEGRA_CNP, SET XACT_ABORT ON).
2. CREATE TABLE idempotente para staging y control de lote.
3. Indices minimos en campos de filtro/joins de staging.
4. CREATE OR ALTER VIEW para vistas canonicas bridge.
5. Seccion opcional de synonyms condicionada por DB_ID.

Beneficio:
- Mantiene limpio el bootstrap inicial (001) y encapsula la capa bridge en una sola migracion.

## 5. Compatibilidad con endpoints backend actuales

Compatibilidad actual: alta, sin cambios obligatorios de codigo para los endpoints existentes.

Motivos:
1. Las consultas de [backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs](backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs) usan exclusivamente tablas dbo transaccionales ya existentes.
2. La fabrica de conexiones [backend/src/IntegradorMarcas.Infrastructure/Data/SqlConnectionFactory.cs](backend/src/IntegradorMarcas.Infrastructure/Data/SqlConnectionFactory.cs) utiliza solo IntegraCnp.
3. Los controladores implementados operan sobre flujo operativo (RF-02/RF-03/RF-04/RF-05), no sobre RF-06 historico externo directo.

Endpoints no impactados por esta migracion:
- POST /api/justificaciones
- GET /api/justificaciones/mias
- GET /api/jefatura/justificaciones/pendientes
- GET /api/jefatura/justificaciones/{justificacionId}
- PATCH /api/jefatura/justificaciones/{justificacionId}/resolver
- GET /api/rrhh/justificaciones

Consideraciones para futura sincronizacion de Usuarios:
- Respetar FK y check actuales en dbo.Usuarios (RolID existente, Compania en CNP/FANAL, JefaturaID referencial).
- Implementar upsert en dos fases para jefaturas (primero usuarios base, luego resolver JefaturaID).

## 6. Nombres exactos de objetos SQL a crear ahora

Crear ahora (sin acceso a DB externa):

Esquemas:
- stg
- bridge
- ext

Tablas:
- stg.BridgeCargaLote
- stg.Wizdom_EmpleadoRaw
- stg.Wizdom_RelacionEmpleadoRaw
- stg.Sifcnp_JustificacionesEncRaw
- stg.Sifcnp_JustificacionesDetRaw
- stg.Sifcnp_TipoMarcaRaw
- stg.Sifcnp_EstadoMarcasRaw

Vistas canonicas:
- bridge.vw_UsuariosCanonico
- bridge.vw_SifcnpEstadosCanonico
- bridge.vw_SifcnpTiposCanonico
- bridge.vw_SifcnpHistoricoEncabezadoCanonico
- bridge.vw_SifcnpHistoricoDetalleCanonico
- bridge.vw_SifcnpHistoricoCompletoCanonico

Objetos placeholder documentados (synonyms opcionales, creacion condicional):
- ext.Src_WIZDOM_optec1empleado
- ext.Src_WIZDOM_relaciones_empleado
- ext.Src_SIFCNP_RH_JUSTIFICACIONES_ENC
- ext.Src_SIFCNP_RH_JUSTIFICACIONES_DET
- ext.Src_SIFCNP_RH_TI_TipoMarca
- ext.Src_SIFCNP_RH_TI_EstadoMarcas
- ext.Src_SIFCNP_V_TI_RH_JUST_MARCA

## 7. Plan de ejecucion sugerido
1. Actualizar 001 con creacion de esquemas y referencia a 007.
2. Crear y ejecutar 007 con tablas staging + vistas bridge.
3. Cargar stg mediante procesos ETL/CSV import desde extracciones read-only existentes.
4. Validar vistas bridge con consultas de humo.
5. Implementar endpoints RF-06 contra bridge.vw_SifcnpHistoricoCompletoCanonico cuando se priorice ese bloque.

## 8. Riesgos y mitigaciones
Riesgo: mapeo incompleto de llaves entre fuentes legacy y Usuarios locales.
Mitigacion: conservar SourceRowKey y campos fuente originales en staging para trazabilidad.

Riesgo: diferencias de codigos de estado entre SIFCNP y Estados locales.
Mitigacion: resolver equivalencias en bridge.vw_SifcnpEstadosCanonico con CASE controlado y tabla de mapeo futura si se requiere.

Riesgo: degradacion por crecimiento de staging.
Mitigacion: particionar por fecha de carga mas adelante; iniciar con indices por claves de negocio y FechaCargaUtc.