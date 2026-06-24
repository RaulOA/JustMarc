# Spec: Fix de unidadNombre en aprobador actual

## Contexto

Endpoint afectado: `GET /api/justificaciones/aprobador-actual`.

Campo afectado en respuesta: `aprobador.unidadNombre`.

## Consulta SQL exacta usada hoy

Archivo backend: `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`.

Bloque actual relevante:

```sql
SELECT
    @SolicitanteUsuarioID AS SolicitanteUsuarioID,
    scopeData.ScopeSource AS Origen,
    scopeData.DeleganteUsuarioId AS DeleganteUsuarioID,
    delegante.NombreCompleto AS DeleganteNombre,
    aprobador.UsuarioID AS AprobadorUsuarioID,
    aprobador.NombreCompleto AS AprobadorNombreCompleto,
    aprobador.Cedula AS AprobadorCedula,
    aprobador.CorreoElectronico AS AprobadorCorreo,
    aprobador.Compania AS AprobadorCompania,
    aprobador.UnidadId AS AprobadorUnidadID,
    eo.Nombre AS AprobadorUnidadNombre,
    aprobador.JefaturaId AS AprobadorJefaturaID
FROM (SELECT 1 AS Seed) seed
OUTER APPLY (
    SELECT TOP 1
        fa.AprobadorUsuarioId,
        fa.Origen AS ScopeSource,
        fa.DeleganteUsuarioId
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioID, GETDATE()) fa
    ORDER BY CASE WHEN fa.Origen = 'Delegacion' THEN 0 ELSE 1 END, fa.AprobadorUsuarioId
) scopeData
LEFT JOIN RecursosHumanos.Usuario aprobador ON aprobador.UsuarioID = scopeData.AprobadorUsuarioId
LEFT JOIN dbo.Estructuras_Organizacionales eo ON eo.EstructuraOrganizacionalID = aprobador.UnidadID
LEFT JOIN RecursosHumanos.Usuario delegante ON delegante.UsuarioID = scopeData.DeleganteUsuarioId;
```

## Hallazgos

1. La funcion de alcance (`dbo.fn_AprobadoresVigentesPorSolicitante`) resuelve estructura por:
   - `RecursosHumanos.Usuario.UnidadId`
   - contra `RecursosHumanos.EstructuraOrganizacional.CodigoOrigen` (no contra `EstructuraOrganizacionalId`).

2. El seed de jerarquia corregido (`docs/db/007_seed_hierarquia_dependencias.sql`) inserta y actualiza nodos en `RecursosHumanos.EstructuraOrganizacional`.

3. En BD actual, `dbo.Estructuras_Organizacionales` existe pero no tiene datos.
   - Resultado: el `LEFT JOIN dbo.Estructuras_Organizacionales ...` retorna `NULL` para `eo.Nombre`.

4. Por eso `AprobadorUnidadNombre` llega `NULL`, y en C# se transforma a `string.Empty`:
   - `JustificacionRepository.GetCurrentApproverAsync` asigna `UnidadNombre = data.AprobadorUnidadNombre ?? string.Empty`.

## Causa raiz

Desalineacion entre el origen de verdad de estructuras (tabla funcional `RecursosHumanos.EstructuraOrganizacional`) y el join legado del endpoint (`dbo.Estructuras_Organizacionales`).

## Fix minimo recomendado

Cambiar solo el join de unidad en `GetCurrentApproverBySolicitante` para usar la tabla/esquema correcto y la misma llave semantica del modelo actual (`CodigoOrigen`):

```sql
LEFT JOIN RecursosHumanos.EstructuraOrganizacional eo
    ON eo.CodigoOrigen = CAST(aprobador.UnidadID AS VARCHAR(50))
```

Sin cambios en:
- firma del endpoint
- DTOs/contratos
- funcion `fn_AprobadoresVigentesPorSolicitante`
- logica de prioridad delegacion vs jerarquia

## Impacto esperado

- `aprobador.unidadNombre` volvera a poblarse para topbar.
- Se mantiene comportamiento existente de seleccion de aprobador actual.

## Verificacion sugerida (post-fix)

1. Llamar `GET /api/justificaciones/aprobador-actual` con usuario cuyo aprobador tenga `UnidadId` de codigos semilla (`15110`, `15120`, `15130`, etc.).
2. Confirmar que `aprobador.unidadNombre` coincide con `RecursosHumanos.EstructuraOrganizacional.Nombre` para `CodigoOrigen = CAST(aprobador.unidadID AS varchar(50))`.
3. Confirmar que `aprobador.nombreCompleto` sigue llegando y no cambia `origen`.
