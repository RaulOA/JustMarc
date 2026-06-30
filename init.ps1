#requires -Version 5.1
<#
  init.ps1 - Verificacion del arnes (Harness Engineering)
  Forma estandar / comandos de ESTE proyecto (ver bloque CONFIGURACION abajo y guia-del-arnes.md).
  Fail-fast: lo barato primero; corta al primer [FAIL]. Exit 0 = verde.
#>

# ======================= CONFIGURACION (de este proyecto) =======================
# Comandos reales del proyecto. Vacio = no aplica (se omite).
$TestCmd      = 'dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "Category!=Integration" --nologo'
$LintCmd      = ''   # no hay linter configurado en este repo
$TypecheckCmd = ''   # .NET no tiene typecheck aparte; lo cubre Build
$BuildCmd     = 'dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug --nologo'

$RequiredTools      = @('dotnet')
$RetentionThreshold = 800   # lineas; semaforo de rotacion de progress/*

$HarnessFiles = @(
  'CLAUDE.md','AGENTS.md','feature_list.json','CHECKPOINTS.md','init.ps1',
  'docs/specs.md','docs/architecture.md','docs/conventions.md','docs/verification.md',
  'docs/constitution.md','docs/workflow.md',
  '.claude/agents/leader.md','.claude/agents/spec_author.md',
  '.claude/agents/implementer.md','.claude/agents/reviewer.md',
  'progress/current.md','progress/history.md','progress/reports.md','progress/backlog.md'
)
# ===============================================================================

$ErrorActionPreference = 'Stop'
Set-Location (Split-Path -Parent $MyInvocation.MyCommand.Path)

# Preferir un SDK .NET local de usuario (~/.dotnet) si existe. Necesario en entornos
# donde Program Files solo tiene el runtime (sin SDK) y no hay admin para tocar el PATH de maquina.
# Inofensivo donde el SDK ya esta en Program Files (guardado por Test-Path).
$UserDotnet = Join-Path $env:USERPROFILE '.dotnet'
if (Test-Path (Join-Path $UserDotnet 'dotnet.exe')) {
  $env:PATH = "$UserDotnet;$env:PATH"
  $env:DOTNET_ROOT = $UserDotnet
}

$script:Failed = $false
function Ok($m)   { Write-Host "[OK]   $m" -ForegroundColor Green }
function Warn($m) { Write-Host "[WARN] $m" -ForegroundColor Yellow }
function Fail($m) { Write-Host "[FAIL] $m" -ForegroundColor Red; $script:Failed = $true }
function Stop-IfFailed {
  if ($script:Failed) { Write-Host ""; Write-Host "Resultado: ROJO (fail-fast)" -ForegroundColor Red; exit 1 }
}

Write-Host "=== init.ps1 - verificacion del arnes ===" -ForegroundColor Cyan

# (1) Toolchain disponible
Write-Host "`n[1/6] Toolchain"
foreach ($t in $RequiredTools) {
  if (Get-Command $t -ErrorAction SilentlyContinue) { Ok "$t disponible" } else { Fail "$t no esta en PATH" }
}
Stop-IfFailed

# (2) Archivos base del arnes
Write-Host "`n[2/6] Archivos base del arnes"
foreach ($f in $HarnessFiles) { if (-not (Test-Path $f)) { Fail "falta $f" } }
if (-not $script:Failed) { Ok "todos los archivos base presentes ($($HarnessFiles.Count))" }
Stop-IfFailed

# (3) Invariantes del tablero
Write-Host "`n[3/6] Invariantes del tablero (feature_list.json)"
try { $board = Get-Content 'feature_list.json' -Raw | ConvertFrom-Json }
catch { Fail "feature_list.json no parsea: $($_.Exception.Message)"; Stop-IfFailed }

$valid = $board.rules.valid_status
if (-not $valid) { $valid = @('pending','spec_ready','in_progress','done','blocked') }
$inProgress = @()
foreach ($ft in $board.features) {
  if ($valid -notcontains $ft.status) { Fail "feature '$($ft.name)' con estado invalido: '$($ft.status)'" }
  if ($ft.status -eq 'in_progress') { $inProgress += $ft }
  if (($ft.sdd -eq $true) -and ($ft.status -ne 'pending')) {
    foreach ($spec in @('requirements.md','design.md','tasks.md')) {
      $p = "specs/$($ft.name)/$spec"
      if (-not (Test-Path $p)) { Fail "feature sdd '$($ft.name)' ($($ft.status)) sin $p" }
    }
  }
}
if ($inProgress.Count -gt 1) { Fail "mas de una feature in_progress ($($inProgress.Count)); regla one_feature_at_a_time" }
foreach ($ft in $inProgress) {
  if ((-not $ft.approved_by) -or (-not $ft.approved_at)) { Fail "feature in_progress '$($ft.name)' sin approved_by/approved_at (puerta humana)" }
}
if (-not $script:Failed) { Ok "tablero coherente (in_progress=$($inProgress.Count))" }
Stop-IfFailed

# (4) Typecheck / Lint / Build / Test (no vacios) - lo caro al final
Write-Host "`n[4/6] Verificacion de codigo"
function Invoke-Step($name, $cmd) {
  if ([string]::IsNullOrWhiteSpace($cmd)) { Write-Host "  - ${name}: (vacio, omitido)" -ForegroundColor DarkGray; return }
  Write-Host "  - $name -> $cmd" -ForegroundColor DarkGray
  Invoke-Expression $cmd
  if ($LASTEXITCODE -ne 0) { Fail "$name fallo (exit $LASTEXITCODE)" } else { Ok "$name verde" }
  Stop-IfFailed
}
Invoke-Step 'Typecheck' $TypecheckCmd
Invoke-Step 'Lint'      $LintCmd
Invoke-Step 'Build'     $BuildCmd
Invoke-Step 'Test'      $TestCmd

# (5) Retencion de progress/*
Write-Host "`n[5/6] Retencion de progress/"
Get-ChildItem 'progress' -Filter *.md -ErrorAction SilentlyContinue | ForEach-Object {
  $lines = (Get-Content $_.FullName | Measure-Object -Line).Lines
  if ($lines -gt $RetentionThreshold) { Warn "$($_.Name): $lines lineas (> $RetentionThreshold), considera rotar con /cerrar" }
  else { Ok "$($_.Name) ($lines lineas)" }
}

# (6) Resumen
Write-Host "`n[6/6] Resumen"
if ($script:Failed) { Write-Host "Resultado: ROJO" -ForegroundColor Red; exit 1 }
Write-Host "Resultado: VERDE" -ForegroundColor Green
exit 0
