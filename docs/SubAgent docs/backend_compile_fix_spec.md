# Backend Compile Fix Spec

## Objetivo
Corregir errores de compilacion `CS1002 (Se esperaba ;)` que bloquean `dotnet run` en `IntegradorMarcas.Api` con cambios minimos y sin alterar logica SQL.

## Evidencia de fallo
Comando ejecutado:

```powershell
Set-Location "C:\Users\User\Desktop\Justificacion de Marca"
dotnet run --project .\backend\src\IntegradorMarcas.Api
```

Resultado: 17 errores `CS1002`, concentrados en:
- `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`

## Causa raiz
En ambos archivos, multiples constantes `public const string ... = @"..."` cierran el literal SQL con comillas dobles, pero **sin `;` de C#** al final de la asignacion.

Patron incorrecto:

```csharp
public const string X = @"...
..."   // falta ;
```

Patron correcto:

```csharp
public const string X = @"...
...";
```

## Fix minimo propuesto
Agregar solo el `;` faltante al final de cada constante afectada.

### 1) `backend/src/IntegradorMarcas.Infrastructure/Queries/AdminAprobacionesSql.cs`
Agregar `;` en estas lineas de cierre:
- 19 (`ListJerarquias`)
- 44 (`CreateJerarquia`)
- 57 (`GetJerarquiaById`)
- 62 (`ToggleJerarquiaEstado`)
- 86 (`ListDelegaciones`)
- 111 (`CreateDelegacion`)
- 124 (`GetDelegacionById`)

No requiere cambios en `ToggleDelegacionEstado` (ya termina en `;`).

### 2) `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
Agregar `;` en estas lineas de cierre:
- 20 (`InsertEncabezado`)
- 38 (`InsertDetalle`)
- 68 (`ListMine`)
- 102 (`ListPendientesJefatura`)
- 153 (`ListRrhhGlobal`)
- 189 (`GetDetalleJefaturaEncabezado`)
- 201 (`GetDetalleJefaturaLineas`)
- 219 (`GetResolverValidation`)
- 237 (`GetAprobacionScopeValidation`)
- 243 (`GetExistingTipoJustificacionIds`)

No requiere cambios en `ResolverPendiente` (ya termina en `;`).

## Patch de referencia (conceptual)

```diff
- ORDER BY JerarquiaAprobacionId DESC;"
+ ORDER BY JerarquiaAprobacionId DESC;";

- SELECT CAST(SCOPE_IDENTITY() AS INT)"
+ SELECT CAST(SCOPE_IDENTITY() AS INT)";
```

Aplicar el mismo ajuste a todos los cierres listados arriba.

## Riesgo y alcance
- Alcance: sintaxis C# unicamente.
- Riesgo funcional: bajo; no cambia SQL ni nombres de parametros.
- Impacto esperado: elimina errores `CS1002` actuales y permite avanzar a la siguiente fase de compilacion.

## Verificacion posterior
1. Ejecutar:

```powershell
dotnet run --project .\backend\src\IntegradorMarcas.Api
```

2. Si aparecen nuevos errores, tratarlos como fase 2 (ya no deberian ser estos `CS1002` en los dos archivos objetivo).
