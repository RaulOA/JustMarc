# PostToolUse (Edit|Write): chequeo BARATO.
# Este repo no tiene lint ni typecheck, asi que el unico chequeo estatico barato es recompilar
# el backend, y solo cuando se edito un .cs. Nunca bloquea (siempre exit 0): es informativo.
try {
  $raw  = [Console]::In.ReadToEnd()
  $file = ($raw | ConvertFrom-Json).tool_input.file_path
} catch { exit 0 }

if ((-not $file) -or ($file -notmatch '\.cs$')) { exit 0 }

Write-Host "[harness] .cs editado -> dotnet build (chequeo barato)"
$out = & dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug --nologo -v minimal 2>&1
if ($LASTEXITCODE -ne 0) {
  Write-Host "[WARN] build con errores:"
  ($out | Select-String -Pattern ': error' | Select-Object -First 8) | ForEach-Object { Write-Host "  $_" }
} else {
  Write-Host "[OK] build limpio"
}
exit 0
