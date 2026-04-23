# Bloque 5 - Scripts de Extraccion BD (WIZDOM, SIFCNP, INTEGRA_CNP)

## 1. Objetivo
Definir exactamente que informacion de base de datos se debe solicitar y extraer para continuar el desarrollo sin acceso directo a credenciales de produccion.

Este documento cubre:
- Metadatos requeridos por base.
- Datos de muestra requeridos por base.
- Set minimo y set extendido de extraccion.
- Columnas de salida esperadas por paquete.
- Plan de consultas compatible con SQL Server (solo lectura).

## 2. Contexto funcional que debe soportar la extraccion
Requisitos del sistema ya implementados o en curso:
- RF-01: sincronizacion de usuarios desde WIZDOM hacia Usuarios en INTEGRA_CNP.
- RF-06: consulta historica de justificaciones desde SIFCNP (solo lectura).
- RF-02/RF-03/RF-04/RF-05: operacion transaccional y consultas en INTEGRA_CNP.

Campos criticos observados en backend actual:
- Filtros RRHH: funcionario, estado, compania, fechaDesde, fechaHasta.
- Vista jefatura: relacion subordinado por JefaturaID.
- Estado y tipos de justificacion por catalogos.

## 3. Entregables esperados del equipo DBA/infra
Como no hay credenciales de produccion en el equipo de desarrollo, se solicita entrega offline de:
- Scripts .sql ejecutados por DBA (solo SELECT).
- Resultados en CSV UTF-8 (un archivo por consulta).
- Diccionario de datos (CSV o XLSX) por base con tipo, nullability, PK/FK y descripcion.
- Nota de corte de datos: fecha/hora de extraccion y ambiente fuente.

Formato de archivo recomendado:
- 01_metadata_<base>.csv
- 02_sample_min_<base>_<objeto>.csv
- 03_sample_ext_<base>_<objeto>.csv

## 4. WIZDOM (solo lectura)

### 4.1 Metadatos requeridos
Necesarios para RF-01 (sincronizacion de usuarios):
- Lista de vistas/tablas candidatas de funcionarios, jefaturas y estructura organizacional.
- Definicion de columnas (nombre, tipo SQL, longitud, nullable).
- Claves y unicidad disponibles (si existe llave natural o surrogate key).
- Regla de compania (CNP=001, FANAL=002) en la fuente.
- Campo de estado de funcionario (activo/inactivo) si existe.

### 4.2 Datos de muestra requeridos
- Registros reales anonimizables o seudonimizados de funcionarios CNP y FANAL.
- Deben incluir casos con y sin jefatura definida.
- Deben incluir casos de correo nulo/invalido para validar limpieza.

### 4.3 Set minimo de extraccion WIZDOM
Meta: habilitar mapeo y primer proceso de carga de Usuarios.

Columnas minimas esperadas (alias de salida objetivo):
- SourceUserKey
- Cedula
- NombreCompleto
- Correo
- SourceJefaturaKey
- JefaturaCedula
- UnidadID
- UnidadNombre
- CompaniaCodigo
- CompaniaNombre
- Activo
- FechaActualizacion

Volumen minimo sugerido:
- TOP (500) de funcionarios activos.
- Debe incluir ambos codigos de compania (001, 002).

### 4.4 Set extendido de extraccion WIZDOM
Meta: robustecer reglas de negocio, auditoria y futuras integraciones.

Columnas extendidas sugeridas:
- PuestoID, PuestoNombre
- DepartamentoID, DepartamentoNombre
- CentroCostoID, CentroCostoNombre
- TipoNombramiento
- FechaIngreso
- FechaSalida
- JefeInmediatoNombre
- CorreoJefatura
- TelefonoExtension
- EstadoLaboral

Volumen extendido sugerido:
- TOP (5000) o universo completo filtrado por compania CNP/FANAL.

## 5. SIFCNP (solo lectura)

### 5.1 Metadatos requeridos
Necesarios para RF-06 (consulta historica):
- Objetos (tablas/vistas) que contienen boletas historicas y detalle historico.
- Diccionario de columnas de encabezado y detalle.
- Catalogos historicos de tipo de justificacion y estado.
- Campo de fecha operativa historica (creacion, marca, resolucion).

### 5.2 Datos de muestra requeridos
- Boletas historicas con al menos 3 estados distintos.
- Boletas de CNP y FANAL si aplica historicamente.
- Registros con multiples lineas por boleta para validar encabezado-detalle.

### 5.3 Set minimo de extraccion SIFCNP
Meta: habilitar grilla historica con filtros por funcionario, fecha y tipo.

Columnas minimas esperadas (salida normalizada):
- HistJustificacionID
- FuncionarioCedula
- FuncionarioNombre
- TipoJustificacion
- FechaMarca
- MotivoGeneral
- EstadoHistorico
- FechaResolucion
- JefaturaNombre
- CompaniaCodigo
- SourceRowVersion (si existe)

Volumen minimo sugerido:
- TOP (1000) ordenado por fecha descendente.

### 5.4 Set extendido de extraccion SIFCNP
Meta: soporte de trazabilidad y conciliacion historica.

Columnas extendidas sugeridas:
- HistDetalleID
- ObservacionDetalle
- UsuarioRegistro
- FechaRegistro
- UsuarioModifica
- FechaModifica
- CodigoUnidad
- NombreUnidad
- CodigoPuesto
- ComentarioResolucion
- MotivoRechazo

Volumen extendido sugerido:
- TOP (10000) o ventana de 24 meses.

## 6. INTEGRA_CNP (base objetivo SQL Server 2019)

### 6.1 Metadatos requeridos
Necesarios para validar despliegue y pruebas del backend actual:
- Estructura de tablas: Roles, Estados, Cat_TiposJustificacion, Usuarios, Justificaciones_Encabezado, Justificaciones_Detalle.
- PK, FK, checks e indices vigentes.
- Identity seed/increment de tablas identity.

### 6.2 Datos de muestra requeridos
- Catalogos completos (Roles, Estados, Cat_TiposJustificacion).
- Usuarios de prueba con jerarquia valida (jefatura-subordinados).
- Boletas en estado 1, 2 y 3.
- Boletas con 1 y multiples detalles.

### 6.3 Set minimo de extraccion INTEGRA_CNP
Meta: ejecutar pruebas de endpoints implementados sin dependencia externa.

Columnas minimas esperadas por tabla:

Roles:
- RolID, NombreRol

Estados:
- EstadoID, Descripcion, Proceso

Cat_TiposJustificacion:
- TipoJustificacionID, Descripcion

Usuarios:
- UsuarioID, Cedula, NombreCompleto, Correo, JefaturaID, UnidadID, RolID, Compania

Justificaciones_Encabezado:
- JustificacionID, UsuarioID, MotivoGeneral, EstadoID, FechaCreacion, AprobadorID, FechaAprobacion

Justificaciones_Detalle:
- DetalleID, JustificacionID, TipoJustificacionID, FechaMarca, ObservacionDetalle

### 6.4 Set extendido de extraccion INTEGRA_CNP
Meta: auditoria funcional y pruebas de regresion.

Columnas extendidas sugeridas:
- Usr_Registro, Fec_Registro, Usr_Modifica, Fec_Modifica (donde aplique)
- Metadata de indices y constraints de cada tabla
- Conteos por estado, compania y rango de fecha

## 7. Columnas de salida esperadas (dataset canonico por modulo)

### 7.1 Dataset canonico de Usuario (sincronizacion WIZDOM -> INTEGRA_CNP)
- SourceSystem
- SourceUserKey
- Cedula
- NombreCompleto
- Correo
- SourceJefaturaKey
- JefaturaCedula
- UnidadID
- UnidadNombre
- CompaniaCodigo
- CompaniaNombre
- Activo
- FechaActualizacion

### 7.2 Dataset canonico de Boleta Historica (SIFCNP)
- SourceSystem
- HistJustificacionID
- HistDetalleID
- FuncionarioCedula
- FuncionarioNombre
- TipoJustificacion
- FechaMarca
- MotivoGeneral
- ObservacionDetalle
- EstadoHistorico
- FechaResolucion
- JefaturaNombre
- CompaniaCodigo

### 7.3 Dataset canonico de Boleta Operativa (INTEGRA_CNP)
- JustificacionID
- MotivoGeneral
- EstadoID
- EstadoDescripcion
- FechaCreacion
- CantidadDetalles
- AprobadorID
- FechaAprobacion
- FuncionarioID
- FuncionarioNombre
- FuncionarioCedula
- Compania
- JefaturaID
- JefaturaNombre
- TipoPrincipal

## 8. Plan SQL Server compatible (solo lectura)

## 8.1 Fase A - Descubrimiento de objetos y metadatos
Ejecutar por cada base (WIZDOM, SIFCNP, INTEGRA_CNP):

```sql
SELECT
    TABLE_SCHEMA,
    TABLE_NAME,
    TABLE_TYPE
FROM INFORMATION_SCHEMA.TABLES
ORDER BY TABLE_SCHEMA, TABLE_NAME;
```

```sql
SELECT
    c.TABLE_SCHEMA,
    c.TABLE_NAME,
    c.COLUMN_NAME,
    c.DATA_TYPE,
    c.CHARACTER_MAXIMUM_LENGTH,
    c.IS_NULLABLE,
    c.ORDINAL_POSITION
FROM INFORMATION_SCHEMA.COLUMNS c
ORDER BY c.TABLE_SCHEMA, c.TABLE_NAME, c.ORDINAL_POSITION;
```

```sql
SELECT
    tc.TABLE_SCHEMA,
    tc.TABLE_NAME,
    tc.CONSTRAINT_NAME,
    tc.CONSTRAINT_TYPE
FROM INFORMATION_SCHEMA.TABLE_CONSTRAINTS tc
ORDER BY tc.TABLE_SCHEMA, tc.TABLE_NAME;
```

## 8.2 Fase B - Identificacion de objetos candidatos WIZDOM/SIFCNP

```sql
SELECT
    s.name AS SchemaName,
    o.name AS ObjectName,
    o.type_desc AS ObjectType
FROM sys.objects o
INNER JOIN sys.schemas s ON s.schema_id = o.schema_id
WHERE o.type IN ('U','V')
  AND (
        o.name LIKE '%func%' OR
        o.name LIKE '%emplead%' OR
        o.name LIKE '%jef%' OR
        o.name LIKE '%unidad%' OR
        o.name LIKE '%marca%' OR
        o.name LIKE '%just%'
      )
ORDER BY s.name, o.name;
```

## 8.3 Fase C - Extraccion minima (plantillas)
Reemplazar [schema].[objeto] por el objeto validado por DBA.

WIZDOM (usuarios):
```sql
SELECT TOP (500)
    CAST(NULL AS VARCHAR(20)) AS SourceUserKey,
    CAST(NULL AS VARCHAR(20)) AS Cedula,
    CAST(NULL AS VARCHAR(150)) AS NombreCompleto,
    CAST(NULL AS VARCHAR(100)) AS Correo,
    CAST(NULL AS VARCHAR(20)) AS SourceJefaturaKey,
    CAST(NULL AS VARCHAR(20)) AS JefaturaCedula,
    CAST(NULL AS VARCHAR(20)) AS UnidadID,
    CAST(NULL AS VARCHAR(150)) AS UnidadNombre,
    CAST(NULL AS VARCHAR(10)) AS CompaniaCodigo,
    CAST(NULL AS VARCHAR(50)) AS CompaniaNombre,
    CAST(NULL AS BIT) AS Activo,
    CAST(NULL AS DATETIME) AS FechaActualizacion
FROM [schema].[objeto]
ORDER BY FechaActualizacion DESC;
```

SIFCNP (historico):
```sql
SELECT TOP (1000)
    CAST(NULL AS VARCHAR(30)) AS HistJustificacionID,
    CAST(NULL AS VARCHAR(30)) AS HistDetalleID,
    CAST(NULL AS VARCHAR(20)) AS FuncionarioCedula,
    CAST(NULL AS VARCHAR(150)) AS FuncionarioNombre,
    CAST(NULL AS VARCHAR(100)) AS TipoJustificacion,
    CAST(NULL AS DATE) AS FechaMarca,
    CAST(NULL AS VARCHAR(500)) AS MotivoGeneral,
    CAST(NULL AS VARCHAR(250)) AS ObservacionDetalle,
    CAST(NULL AS VARCHAR(100)) AS EstadoHistorico,
    CAST(NULL AS DATETIME) AS FechaResolucion,
    CAST(NULL AS VARCHAR(150)) AS JefaturaNombre,
    CAST(NULL AS VARCHAR(10)) AS CompaniaCodigo
FROM [schema].[objeto]
ORDER BY FechaMarca DESC;
```

INTEGRA_CNP (operativo RRHH):
```sql
SELECT TOP (1000)
    je.JustificacionID,
    je.MotivoGeneral,
    je.EstadoID,
    e.Descripcion AS EstadoDescripcion,
    je.FechaCreacion,
    COUNT(jd.DetalleID) AS CantidadDetalles,
    je.AprobadorID,
    je.FechaAprobacion,
    u.UsuarioID AS FuncionarioID,
    u.NombreCompleto AS FuncionarioNombre,
    u.Cedula AS FuncionarioCedula,
    u.Compania,
    u.JefaturaID,
    j.NombreCompleto AS JefaturaNombre,
    MIN(tj.Descripcion) AS TipoPrincipal
FROM dbo.Justificaciones_Encabezado je
INNER JOIN dbo.Estados e ON e.EstadoID = je.EstadoID
INNER JOIN dbo.Usuarios u ON u.UsuarioID = je.UsuarioID
LEFT JOIN dbo.Usuarios j ON j.UsuarioID = u.JefaturaID
LEFT JOIN dbo.Justificaciones_Detalle jd ON jd.JustificacionID = je.JustificacionID
LEFT JOIN dbo.Cat_TiposJustificacion tj ON tj.TipoJustificacionID = jd.TipoJustificacionID
GROUP BY
    je.JustificacionID,
    je.MotivoGeneral,
    je.EstadoID,
    e.Descripcion,
    je.FechaCreacion,
    je.AprobadorID,
    je.FechaAprobacion,
    u.UsuarioID,
    u.NombreCompleto,
    u.Cedula,
    u.Compania,
    u.JefaturaID,
    j.NombreCompleto
ORDER BY je.FechaCreacion DESC, je.JustificacionID DESC;
```

## 8.4 Fase D - Validaciones post-extraccion
Validaciones minimas a reportar por DBA:
- Conteo total por dataset y por compania (001/002 o CNP/FANAL).
- Conteo de nulos en Cedula, NombreCompleto, Correo, Jefatura.
- Conteo de boletas por estado.
- Muestra de 20 registros con jefatura no nula y 20 con jefatura nula.

## 9. Criterios de aceptacion del bloque
Se considera completo cuando se entregan:
- Metadatos completos de las 3 bases.
- Extraccion minima completa de WIZDOM, SIFCNP e INTEGRA_CNP.
- Al menos una extraccion extendida (WIZDOM o SIFCNP) con volumen ampliado.
- Evidencia de validaciones post-extraccion.

Con esto, el equipo puede continuar desarrollo de sincronizacion, consulta historica y pruebas de integracion sin credenciales de produccion en entorno dev.
