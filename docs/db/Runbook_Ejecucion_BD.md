# Runbook — Ejecución de los scripts de BD (INTEGRA_CNP)

Procedimiento rápido y repetible para crear/alinear la base con los 3 scripts
consolidados de `docs/db/`. Pensado para que en futuras instrucciones la ejecución
sea **un solo comando**.

## 0. Conexión (ya establecida)

La conexión NO se versiona: se toma de la variable de entorno de usuario
`ConnectionStrings__IntegraCnp` (la misma que usa el backend). El runner y los
comandos de abajo la leen de ahí; la contraseña nunca se imprime.

> **Entorno detectado (2026-06-24):** servidor `CNPOCSRBD-V02-3\DESARROLLO`
> (`192.168.36.83\DESARROLLO,49403`), **SQL Server 2019** (15.0.2135.5), BD
> `INTEGRA_CNP` existente pero **sin el esquema de aplicación** (solo contenía la
> vista `dbo.VW_RH_JUSTIFICACIONES`).
>
> **Estado tras esta sesión:** se ejecutaron **01 + 02** (estructura creada y
> verificada: 15 tablas, función, 4 vistas `Integracion.*`, shim). **03 NO se
> ejecutó** ⇒ los catálogos están **vacíos**. Para que el backend funcione, corre al
> menos la **Sección A** de `03_DatosSemilla.sql`.

## 1. Flujo rápido (recomendado) — runner

```powershell
# Diagnóstico sin cambios (identidad del servidor + inventario actual)
pwsh -File docs/db/Ejecutar_Scripts.ps1 -Modo PreFlight

# Solo estructura (01 + 02): seguro para cualquier entorno
pwsh -File docs/db/Ejecutar_Scripts.ps1 -Modo Estructura

# Completo (01 + 02 + 03, incluye demo + remediación): SOLO dev/desarrollo
pwsh -File docs/db/Ejecutar_Scripts.ps1 -Modo Completo

# Solo verificación
pwsh -File docs/db/Ejecutar_Scripts.ps1 -Modo Verificar
```

El runner: lee cada `.sql` como **UTF-8 explícito**, lo envía con `Invoke-Sqlcmd`,
corta ante el primer error, imprime los `PRINT` y al final corre la verificación.

## 2. Flujo manual (sin runner)

```powershell
$cs = $env:ConnectionStrings__IntegraCnp
foreach ($s in '01_CrearBaseDatos.sql','02_EstructuraCompleta.sql','03_DatosSemilla.sql') {
    $sql = [System.IO.File]::ReadAllText("docs/db/$s", [System.Text.Encoding]::UTF8)
    Write-Host "== $s =="
    Invoke-Sqlcmd -ConnectionString $cs -Query $sql -QueryTimeout 180 -Verbose -ErrorAction Stop
}
```

> **Importante (encoding):** NO usar `-InputFile`. `Invoke-Sqlcmd` puede leer el
> archivo con codepage ANSI y convertir `í`→`Ã­` (mojibake) en los catálogos. Leer
> siempre el archivo como UTF-8 y pasarlo por `-Query`.

## 3. Producción

En producción ejecutar `01` y `02` completos, pero de `03_DatosSemilla.sql`
**solo la Sección A (catálogos)**. Las secciones B/C (datos demo) y D (remediación
de mojibake) son para dev. Copiar la Sección A a un archivo aparte o ejecutarla por
selección en SSMS.

## 4. Verificación (consulta directa)

```sql
-- Esquemas + objetos esperados
SELECT s.name AS Esquema, o.type_desc AS Tipo, COUNT(*) AS Cant
FROM sys.objects o JOIN sys.schemas s ON s.schema_id=o.schema_id
WHERE o.is_ms_shipped=0 AND o.type IN ('U','V','FN','IF','TF','P')
GROUP BY s.name,o.type_desc ORDER BY s.name,o.type_desc;

-- Alineaciones clave
SELECT
  CASE WHEN OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante') IS NULL THEN 'FALTA' ELSE 'OK' END AS DboFn,
  CASE WHEN OBJECT_ID('Operacion.fn_AprobadoresVigentesPorSolicitante') IS NULL THEN 'OK' ELSE 'AUN EXISTE' END AS OperacionFnObsoleta;

-- Columnas finales de ErrorApi (contrato C#)
SELECT name FROM sys.columns WHERE object_id=OBJECT_ID('Auditoria.ErrorApi') ORDER BY column_id;

-- Prueba de humo de la función (requiere demo de la Sección C)
DECLARE @u INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula='2-5130-0001');
SELECT * FROM dbo.fn_AprobadoresVigentesPorSolicitante(@u, GETDATE());
```

**Objetos esperados tras 01+02:** 6 catálogos + 2 RRHH + 4 Operación + 3 Auditoría
(15 tablas), 16 índices, `dbo.fn_AprobadoresVigentesPorSolicitante`, 4 vistas
`Integracion.*` (best-effort, realineadas al esquema real de WIZDOM/SIFCNP) y shim
`dbo.Estructuras_Organizacionales`. (El SP de sincronización legado fue retirado;
ver `Observaciones_Consolidacion_SQL.md` §9.)

## 5. Notas

- Idempotente: re-ejecutar no duplica ni falla.
- WIZDOM/SIFCNP ausentes ⇒ las vistas externas y la vista legada `dbo.RH_*` se
  **omiten** con `PRINT` (no abortan).
- Detalle de la consolidación y análisis 3FN: `docs/db/Observaciones_Consolidacion_SQL.md`.
