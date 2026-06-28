<#
.SYNOPSIS
    Ejecuta los scripts consolidados de INTEGRA_CNP contra la BD configurada,
    de forma idempotente, secuencial y con verificacion.

.DESCRIPTION
    Usa la cadena de conexion de la variable de entorno ConnectionStrings__IntegraCnp
    (la misma que consume el backend). Lee cada .sql como UTF-8 EXPLICITO y lo envia
    como -Query para evitar que Invoke-Sqlcmd reinterprete los acentos con el codepage
    ANSI (causa clasica de mojibake). Corta ante el primer error.

.PARAMETER Modo
    PreFlight   : solo diagnostico (identidad del servidor + inventario actual).
    Estructura  : ejecuta 01 + 02 (crea BD/esquemas + estructura completa).
    Completo    : ejecuta 01 + 02 + 03 (incluye demo y remediacion). SOLO dev.
    Verificar   : solo corre la verificacion post-ejecucion.

.EXAMPLE
    pwsh -File docs/db/Ejecutar_Scripts.ps1 -Modo PreFlight
    pwsh -File docs/db/Ejecutar_Scripts.ps1 -Modo Estructura
    pwsh -File docs/db/Ejecutar_Scripts.ps1 -Modo Completo

.NOTES
    Requiere el modulo SqlServer (Invoke-Sqlcmd). En PRODUCCION no usar 'Completo':
    de 03 ejecutar manualmente solo la Seccion A (catalogos).
#>
[CmdletBinding()]
param(
    [ValidateSet('PreFlight','Estructura','Completo','Verificar')]
    [string]$Modo = 'PreFlight'
)

$ErrorActionPreference = 'Stop'
$dir = Split-Path -Parent $MyInvocation.MyCommand.Path

# --- Cadena de conexion (nunca se imprime) ---------------------------------
$cs = $env:ConnectionStrings__IntegraCnp
if ([string]::IsNullOrWhiteSpace($cs)) {
    throw "Falta la variable de entorno ConnectionStrings__IntegraCnp."
}
$csMaster = $cs -replace 'Database=INTEGRA_CNP', 'Database=master' `
                -replace 'Initial Catalog=INTEGRA_CNP', 'Initial Catalog=master'

function Invoke-SqlFile([string]$nombre) {
    $path = Join-Path $dir $nombre
    if (-not (Test-Path $path)) { throw "No existe $path" }
    $sql = [System.IO.File]::ReadAllText($path, [System.Text.Encoding]::UTF8)
    Write-Host "################ EJECUTANDO $nombre ################" -ForegroundColor Cyan
    $msgs = Invoke-Sqlcmd -ConnectionString $cs -Query $sql -QueryTimeout 180 -Verbose 4>&1
    $msgs | ForEach-Object { Write-Host ("  " + $_.ToString()) }
    Write-Host ">>> OK: $nombre" -ForegroundColor Green
}

function Show-PreFlight() {
    Write-Host "=== PRE-FLIGHT ===" -ForegroundColor Cyan
    $info = Invoke-Sqlcmd -ConnectionString $csMaster -QueryTimeout 30 -Query @'
SET NOCOUNT ON;
SELECT @@SERVERNAME AS ServerName,
       CAST(SERVERPROPERTY('ProductVersion') AS varchar(40)) AS Version,
       CASE WHEN DB_ID('INTEGRA_CNP') IS NULL THEN 'NO' ELSE 'SI' END AS IntegraCnpExiste,
       CASE WHEN DB_ID('WIZDOM') IS NULL THEN 'NO' ELSE 'SI' END AS Wizdom,
       CASE WHEN DB_ID('SIFCNP') IS NULL THEN 'NO' ELSE 'SI' END AS Sifcnp;
'@
    $info | Format-List
    if ($info.IntegraCnpExiste -eq 'SI') {
        Invoke-Sqlcmd -ConnectionString $cs -QueryTimeout 30 -Query @'
SET NOCOUNT ON;
SELECT s.name AS Esquema, o.type_desc AS Tipo, COUNT(*) AS Cant
FROM sys.objects o JOIN sys.schemas s ON s.schema_id=o.schema_id
WHERE o.is_ms_shipped=0 AND o.type IN ('U','V','FN','IF','TF','P')
GROUP BY s.name,o.type_desc ORDER BY s.name,o.type_desc;
'@ | Format-Table -AutoSize
    }
}

function Show-Verificacion() {
    Write-Host "=== VERIFICACION POST-EJECUCION ===" -ForegroundColor Cyan
    $ds = Invoke-Sqlcmd -ConnectionString $cs -QueryTimeout 60 -OutputAs DataSet -Query @'
SET NOCOUNT ON;
SELECT s.name AS Esquema, o.type_desc AS Tipo, COUNT(*) AS Cant
FROM sys.objects o JOIN sys.schemas s ON s.schema_id=o.schema_id
WHERE o.is_ms_shipped=0 AND o.type IN ('U','V','FN','IF','TF','P')
GROUP BY s.name,o.type_desc ORDER BY s.name,o.type_desc;

SELECT
  CASE WHEN OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante') IS NULL THEN 'FALTA' ELSE 'OK' END AS DboFn,
  CASE WHEN OBJECT_ID('Operacion.fn_AprobadoresVigentesPorSolicitante') IS NULL THEN 'OK (dropeada)' ELSE 'AUN EXISTE' END AS OperacionFnObsoleta,
  CASE WHEN OBJECT_ID('Operacion.usp_SincronizarJustificacionesDesdeHistorico') IS NULL THEN 'FALTA' ELSE 'OK' END AS SpSync,
  ISNULL((SELECT type_desc FROM sys.objects WHERE object_id=OBJECT_ID('dbo.Estructuras_Organizacionales')),'NO') AS EstrOrg;

SELECT STRING_AGG(name, ', ') WITHIN GROUP (ORDER BY column_id) AS ErrorApiCols
FROM sys.columns WHERE object_id=OBJECT_ID('Auditoria.ErrorApi');

SELECT 'Rol' AS Catalogo, COUNT(*) AS Filas FROM Configuracion.Rol
UNION ALL SELECT 'EstadoJustificacion', COUNT(*) FROM Configuracion.EstadoJustificacion
UNION ALL SELECT 'TipoJustificacion', COUNT(*) FROM Configuracion.TipoJustificacion
UNION ALL SELECT 'EstadoRegistro', COUNT(*) FROM Configuracion.EstadoRegistro
UNION ALL SELECT 'TipoEventoAuditoria', COUNT(*) FROM Configuracion.TipoEventoAuditoria
UNION ALL SELECT 'ResultadoAuditoria', COUNT(*) FROM Configuracion.ResultadoAuditoria
UNION ALL SELECT 'Usuario', COUNT(*) FROM RecursosHumanos.Usuario
UNION ALL SELECT 'EstructuraOrganizacional', COUNT(*) FROM RecursosHumanos.EstructuraOrganizacional
UNION ALL SELECT 'JerarquiaAprobacion', COUNT(*) FROM Operacion.JerarquiaAprobacion;
'@
    Write-Host "-- Inventario por esquema/tipo --"; $ds.Tables[0] | Format-Table -AutoSize
    Write-Host "-- Objetos clave --";               $ds.Tables[1] | Format-List
    Write-Host "-- Columnas Auditoria.ErrorApi --"; $ds.Tables[2] | Format-List
    Write-Host "-- Conteos --";                     $ds.Tables[3] | Format-Table -AutoSize

    # Prueba de humo de la funcion de aprobadores (si hay datos demo)
    Invoke-Sqlcmd -ConnectionString $cs -QueryTimeout 30 -Query @'
SET NOCOUNT ON;
DECLARE @u INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '2-5130-0001');
IF @u IS NOT NULL
    SELECT 'fn(FUNC_UTI)' AS Prueba, AprobadorUsuarioId, Origen
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(@u, GETDATE());
'@ | Format-Table -AutoSize
}

switch ($Modo) {
    'PreFlight'  { Show-PreFlight }
    'Estructura' { Invoke-SqlFile '01_CrearBaseDatos.sql'; Invoke-SqlFile '02_EstructuraCompleta.sql'; Show-Verificacion }
    'Completo'   { Invoke-SqlFile '01_CrearBaseDatos.sql'; Invoke-SqlFile '02_EstructuraCompleta.sql'; Invoke-SqlFile '03_DatosSemilla.sql'; Show-Verificacion }
    'Verificar'  { Show-Verificacion }
}
Write-Host "FIN ($Modo)." -ForegroundColor Green
