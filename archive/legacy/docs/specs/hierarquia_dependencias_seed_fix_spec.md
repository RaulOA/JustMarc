# Spec: Correccion Seed Jerarquia Dependencias (Aprobador por Rol)

## Objetivo
Corregir el modelo de datos semilla para que, bajo el backend actual (sin cambios en C# ni en la funcion de aprobadores), se cumpla esta regla:

- Jefatura subordinada: aprobada por jefatura superior.
- Funcionario regular de la misma dependencia: aprobado por la jefatura de su propia dependencia.

## Hallazgos Clave

1. La resolucion de aprobadores vigente es estructural, no por rol.
   - `dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioID, @FechaRef)` obtiene la estructura via `Usuario.UnidadId` -> `EstructuraOrganizacional.CodigoOrigen`.
   - Luego toma `Operacion.JerarquiaAprobacion` para esa estructura.
   - No usa `Usuario.RolId` ni `Usuario.JefaturaId` para elegir aprobador.

2. El seed actual `docs/db/007_seed_hierarquia_dependencias.sql` crea 1 estructura por dependencia y asigna en esa misma `UnidadId` tanto al jefe como a sus funcionarios.
   - Paso 3: jefe en `UnidadId = Codigo`.
   - Paso 4: funcionarios en `UnidadId = Codigo`.
   - Paso 7: jerarquia de la estructura `Codigo` apunta al jefe del padre.

3. Resultado observado (correctamente explicado por el modelo actual):
   - Si jefe y funcionarios comparten `UnidadId`, ambos comparten aprobador vigente.
   - Por eso FUNC_5130 y JEFE_5130, y FUNC_5220 y JEFE_5220, terminan resolviendo al aprobador superior.

## Causa Raiz
La semilla mezcla en una sola estructura dos poblaciones con regla de aprobacion distinta (jefatura vs funcionarios), pero la funcion de aprobador solo distingue por estructura (`UnidadId`).

## Opciones Evaluadas

1. Cambiar funcion/backend para decidir por rol (`RolId`) y/o `JefaturaId`.
   - Pros: no requiere duplicar estructuras.
   - Contras: cambia logica de negocio en runtime, mayor riesgo y alcance (SQL + backend + pruebas).
   - Veredicto: no minimo bajo el modelo actual.

2. Mantener funcion y separar estructuras de jefatura vs operativas en seed/modelo de datos.
   - Pros: compatible con backend actual; corrige la ambiguedad en origen; impacto acotado a seed/modelado.
   - Contras: agrega nodos de estructura adicionales y convencion de codigos.
   - Veredicto: mejor ajuste minimo.

## Correccion Minima Recomendada

Adoptar doble estructura por dependencia no raiz:

- Estructura operativa de dependencia (para funcionarios).
- Estructura de jefatura de esa dependencia (solo para el jefe de la dependencia).

Reglas de asignacion:

1. Funcionarios de dependencia D:
   - `Usuario.UnidadId = D_OP`.
   - `Usuario.JefaturaId = UsuarioId del jefe de D`.
   - `JerarquiaAprobacion(D_OP) = jefe de D`.

2. Jefe de dependencia D (subordinada):
   - `Usuario.UnidadId = D_JEF`.
   - `Usuario.JefaturaId = jefe de dependencia padre` (como metadato de linea de mando).
   - `JerarquiaAprobacion(D_JEF) = jefe de dependencia padre`.

3. Dependencias raiz:
   - Mantener criterio actual (autorreferencia o nulo segun politicas actuales), sin afectar la correccion del problema reportado.

## Impacto de Datos (Minimo)

1. No se modifica contrato API ni consultas C#.
2. Se modifica exclusivamente el seed/modelo de estructuras para dejar de compartir `UnidadId` entre jefe y funcionarios cuando la regla de aprobacion difiere.
3. Se mantiene `Operacion.JerarquiaAprobacion` como fuente de verdad para aprobador vigente.

## Ajustes Especificos en 007

1. Reemplazar la tabla temporal unica `@Dependencias` por una definicion de estructuras derivadas:
   - Nodo operativo por dependencia (`CodigoOp`).
   - Nodo jefatura por dependencia no raiz (`CodigoJef`).

2. Upsert de `RecursosHumanos.EstructuraOrganizacional` para ambos tipos de nodo.

3. Asignacion de usuarios:
   - Jefe D -> `UnidadId = CodigoJef` (o `CodigoOp` solo en raiz si se decide simplificar).
   - Funcionarios D -> `UnidadId = CodigoOp`.

4. Construccion de `@JerarquiaEsperada` en dos renglones por dependencia no raiz:
   - `CodigoOp` aprueba jefe propio.
   - `CodigoJef` aprueba jefe padre.

5. Validaciones de salida:
   - `FUNC_5130` debe resolver a jefe 5130.
   - `JEFE_5130` debe resolver a jefe de dependencia padre.
   - `FUNC_5220` debe resolver a jefe 5220.
   - `JEFE_5220` debe resolver a jefe de dependencia padre.

## Convencion Sugerida de Codigos
Para evitar colision y mantener trazabilidad:

- Operativa: codigo original (ej. 5130).
- Jefatura: codigo derivado numerico estable (ej. 15130 o 51301), documentado en el script.

Requisito: seguir usando valores numericos porque `Usuario.UnidadId` es `INT` y la funcion compara contra `CodigoOrigen` convertido desde ese entero.

## SQL de Verificacion (Post-fix)

1. Ver aprobador actual por casos:

```sql
SELECT 'FUNC_5130' AS Caso, *
FROM dbo.fn_AprobadoresVigentesPorSolicitante((SELECT UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '2-5130-0001'), GETDATE());

SELECT 'JEFE_5130' AS Caso, *
FROM dbo.fn_AprobadoresVigentesPorSolicitante((SELECT UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '1-5130-0001'), GETDATE());

SELECT 'FUNC_5220' AS Caso, *
FROM dbo.fn_AprobadoresVigentesPorSolicitante((SELECT UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '2-5220-0001'), GETDATE());

SELECT 'JEFE_5220' AS Caso, *
FROM dbo.fn_AprobadoresVigentesPorSolicitante((SELECT UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '1-5220-0001'), GETDATE());
```

2. Criterio de aceptacion:
   - Ningun `FUNC_xxxx` debe resolver al jefe superior si existe jefe propio activo de su dependencia.
   - Todo `JEFE_xxxx` subordinado debe resolver al jefe de la dependencia padre.

## Decision
La correccion minima y de menor riesgo, bajo el backend actual, es de modelo de datos/seed: separar estructuras de jefatura y operativas para evitar que jefe y funcionarios compartan la misma clave de resolucion (`UnidadId`).
