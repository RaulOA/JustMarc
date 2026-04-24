# Runbook Local SQL + Seed + Run (Windows)

Fecha: 2026-04-23  
Repositorio: Justificacion de Marca  
Objetivo: levantar ambiente local completo (SQL Server + API + frontend) y validar flujo Funcionario -> Jefatura -> RRHH.

## 1) Prerrequisitos verificados

Ejecutar en PowerShell:

```powershell
dotnet --version
sqlcmd -?
```

Esperado:
- .NET SDK 8.x instalado.
- SQLCMD disponible.
- Instancia SQL local: localhost\SQLEXPRESS.

## 2) Preparar base local con scripts canonicos

Desde la raiz real del repo:

```powershell
Set-Location "C:\Users\User\Desktop\Justificacion de Marca"

sqlcmd -S "localhost\SQLEXPRESS" -E -b -d master -i ".\docs\db\001_integra_marcas_base_inicial.sql"
sqlcmd -S "localhost\SQLEXPRESS" -E -b -d INTEGRA_CNP -i ".\docs\db\002_integra_marcas_objetos.sql"
```

Notas:
- `-E` usa autenticacion integrada de Windows.
- `-b` hace fallar el comando si hay error SQL.
- El orden 001 -> 002 es obligatorio.

Verificacion rapida de objetos:

```powershell
sqlcmd -S "localhost\SQLEXPRESS" -E -d INTEGRA_CNP -Q "SELECT OBJECT_ID('Operacion.fn_AprobadoresVigentesPorSolicitante','FN') AS FnId;"
```

## 3) Bloque SQL de semilla minima (idempotente)

Ejecutar en SSMS o SQLCMD sobre `INTEGRA_CNP`:

```sql
USE INTEGRA_CNP;
SET XACT_ABORT ON;

BEGIN TRAN;

-- 1) Estructura base para UnidadId = 120
IF NOT EXISTS (
    SELECT 1
    FROM RecursosHumanos.EstructuraOrganizacional
    WHERE CodigoOrigen = '120'
)
BEGIN
    INSERT INTO RecursosHumanos.EstructuraOrganizacional
    (
        Nombre, CodigoOrigen, EstructuraPadreId, EstadoRegistroId,
        VigenciaDesde, VigenciaHasta, CreadoPor
    )
    VALUES
    (
        'Unidad Demo CNP', '120', NULL, 1,
        DATEADD(DAY, -30, SYSUTCDATETIME()), NULL, 'seed_local'
    );
END;

DECLARE @EstructuraId INT = (
    SELECT TOP 1 EstructuraOrganizacionalId
    FROM RecursosHumanos.EstructuraOrganizacional
    WHERE CodigoOrigen = '120'
    ORDER BY EstructuraOrganizacionalId DESC
);

-- 2) Usuarios por rol
IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'FUNC-0001')
BEGIN
    INSERT INTO RecursosHumanos.Usuario
    (
        Cedula, NombreCompleto, CorreoElectronico, JefaturaId,
        UnidadId, RolId, Compania, EsActivo, CreadoPor
    )
    VALUES ('FUNC-0001', 'Funcionario Demo', 'funcionario.demo@cnp.local', NULL, 120, 1, 'CNP', 1, 'seed_local');
END;

IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'JEFE-0001')
BEGIN
    INSERT INTO RecursosHumanos.Usuario
    (
        Cedula, NombreCompleto, CorreoElectronico, JefaturaId,
        UnidadId, RolId, Compania, EsActivo, CreadoPor
    )
    VALUES ('JEFE-0001', 'Jefatura Demo', 'jefatura.demo@cnp.local', NULL, 120, 2, 'CNP', 1, 'seed_local');
END;

IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'RRHH-0001')
BEGIN
    INSERT INTO RecursosHumanos.Usuario
    (
        Cedula, NombreCompleto, CorreoElectronico, JefaturaId,
        UnidadId, RolId, Compania, EsActivo, CreadoPor
    )
    VALUES ('RRHH-0001', 'RRHH Demo', 'rrhh.demo@cnp.local', NULL, 120, 3, 'CNP', 1, 'seed_local');
END;

DECLARE @FuncionarioId INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = 'FUNC-0001');
DECLARE @JefeId INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = 'JEFE-0001');

-- 3) Relacion funcionario -> jefatura
UPDATE RecursosHumanos.Usuario
SET JefaturaId = @JefeId,
    ModificadoPor = 'seed_local',
    FechaHoraModificacion = SYSUTCDATETIME()
WHERE UsuarioId = @FuncionarioId
  AND (JefaturaId IS NULL OR JefaturaId <> @JefeId);

-- 4) Jerarquia vigente para aprobacion
IF NOT EXISTS (
    SELECT 1
    FROM Operacion.JerarquiaAprobacion
    WHERE AprobadorUsuarioId = @JefeId
      AND EstructuraOrganizacionalId = @EstructuraId
      AND EstadoRegistroId = 1
)
BEGIN
    INSERT INTO Operacion.JerarquiaAprobacion
    (
        AprobadorUsuarioId, EstructuraOrganizacionalId, NivelAprobacion,
        TipoRelacion, EstadoRegistroId, VigenciaDesde, VigenciaHasta, CreadoPor
    )
    VALUES
    (
        @JefeId, @EstructuraId, 1,
        'Vertical', 1, DATEADD(DAY, -30, SYSUTCDATETIME()), NULL, 'seed_local'
    );
END;

-- 5) Justificacion inicial pendiente (opcional pero recomendada)
IF NOT EXISTS (
    SELECT 1
    FROM Operacion.Justificacion
    WHERE UsuarioId = @FuncionarioId
      AND MotivoGeneral = 'Seed local pendiente'
)
BEGIN
    INSERT INTO Operacion.Justificacion
    (
        UsuarioId, MotivoGeneral, EstadoJustificacionId, FechaCreacion,
        CreadoPor, FechaHoraCreacion
    )
    VALUES
    (
        @FuncionarioId, 'Seed local pendiente', 1, SYSUTCDATETIME(),
        'seed_local', SYSUTCDATETIME()
    );

    DECLARE @JustificacionId INT = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO Operacion.JustificacionDetalle
    (
        JustificacionId, TipoJustificacionId, FechaMarca,
        ObservacionDetalle, CreadoPor, FechaHoraCreacion
    )
    VALUES
    (
        @JustificacionId, 1, CAST(GETDATE() AS date),
        'Detalle seed local', 'seed_local', SYSUTCDATETIME()
    );
END;

COMMIT TRAN;
```

Validacion rapida del seed:

```sql
SELECT TOP 10 UsuarioId, Cedula, NombreCompleto, RolId, JefaturaId, UnidadId
FROM RecursosHumanos.Usuario
ORDER BY UsuarioId;

SELECT TOP 10 JustificacionId, UsuarioId, EstadoJustificacionId, FechaCreacion
FROM Operacion.Justificacion
ORDER BY JustificacionId DESC;
```

## 4) Arranque backend y frontend

### 4.1 Backend (.NET 8)

Comandos exactos en PowerShell (desde raiz):

```powershell
Set-Location "C:\Users\User\Desktop\Justificacion de Marca"

dotnet restore .\backend\IntegradorMarcas.slnx
dotnet build .\backend\IntegradorMarcas.slnx
dotnet run --project .\backend\src\IntegradorMarcas.Api
```

URLs esperadas:
- http://localhost:5093
- https://localhost:7129
- http://localhost:5093/swagger
- http://localhost:5093/health

Configuracion Development alineada al repo:
- Archivo: `backend/src/IntegradorMarcas.Api/appsettings.Development.json`
- `ConnectionStrings:IntegraCnp = Server=localhost\\SQLEXPRESS;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;`
- `Security:UseMockIdentity = true`

### 4.2 Frontend estatico

En otra terminal PowerShell:

```powershell
Set-Location "C:\Users\User\Desktop\Justificacion de Marca"
python -m http.server 5500
```

Abrir en navegador:
- http://localhost:5500/index.html

Datos alineados al frontend actual (`app.js`):
- API base por defecto: `http://localhost:5093`
- Usuarios demo: `funcionario.ana`, `jefe.maria`, `rrhh.carlos`
- Headers enviados al backend: `X-User-Id`, `X-User-Role`

## 5) Secuencia de validacion (backend + frontend + smoke tests)

## Paso 0: disponibilidad tecnica

```powershell
Invoke-RestMethod -Uri "http://localhost:5093/health" -Method Get
```

Esperado: `status = ok`.

## Paso 1: crear boleta (Funcionario)

```powershell
$headersFunc = @{
  "X-User-Id"   = "10"
  "X-User-Role" = "ROL_FUNC"
  "Content-Type" = "application/json"
}

$bodyCrear = @'
{
  "motivoGeneral": "Atraso por incidente vial",
  "detalles": [
    {
      "tipoJustificacionID": 1,
      "fechaMarca": "2026-04-23T00:00:00",
      "observacionDetalle": "Ingreso 08:22"
    }
  ]
}
'@

$creada = Invoke-RestMethod -Uri "http://localhost:5093/api/justificaciones" -Method Post -Headers $headersFunc -Body $bodyCrear
$justificacionId = $creada.justificacionID
$justificacionId
```

Esperado: ID numerico creado.

## Paso 2: pendientes de jefatura

```powershell
$headersJefe = @{
  "X-User-Id"   = "20"
  "X-User-Role" = "ROL_JEFE"
}

$pendientes = Invoke-RestMethod -Uri "http://localhost:5093/api/jefatura/justificaciones/pendientes" -Method Get -Headers $headersJefe
$pendientes | Select-Object -First 5 justificacionID, estadoDescripcion, motivoGeneral
```

Esperado: aparece la boleta creada en estado pendiente.

## Paso 3: resolver boleta (Jefatura)

```powershell
$bodyResolver = @'
{
  "accion": "APROBAR",
  "comentario": "Validado por jefatura en smoke local"
}
'@

Invoke-RestMethod -Uri "http://localhost:5093/api/jefatura/justificaciones/$justificacionId/resolver" -Method Patch -Headers (@{ "X-User-Id"="20"; "X-User-Role"="ROL_JEFE"; "Content-Type"="application/json" }) -Body $bodyResolver
```

Esperado: respuesta 204 (sin contenido).

## Paso 4: consulta RRHH

```powershell
$headersRrhh = @{
  "X-User-Id"   = "30"
  "X-User-Role" = "ROL_RRHH"
}

$rrhh = Invoke-RestMethod -Uri "http://localhost:5093/api/rrhh/justificaciones?compania=CNP" -Method Get -Headers $headersRrhh
$rrhh | Select-Object -First 5 justificacionID, estadoDescripcion, funcionarioNombre, compania
```

Esperado: incluye registros de CNP y la boleta recien procesada.

## Paso 5: smoke de pruebas automatizadas backend

En una terminal aparte (con API detenida si hay bloqueo de archivos):

```powershell
Set-Location "C:\Users\User\Desktop\Justificacion de Marca"
dotnet test .\backend\IntegradorMarcas.slnx
```

Esperado: ejecucion de pruebas completada sin errores de compilacion.

## 6) Checklist final de exito

- [ ] `sqlcmd` ejecuta 001 y 002 sin errores (`-b` no corta por fallo).
- [ ] Existe `Operacion.fn_AprobadoresVigentesPorSolicitante` en `INTEGRA_CNP`.
- [ ] Seed minimo inserta/actualiza usuarios `FUNC-0001`, `JEFE-0001`, `RRHH-0001` sin duplicar.
- [ ] `GET /health` responde 200 con `status = ok`.
- [ ] Backend inicia en `http://localhost:5093` y Swagger carga en `/swagger`.
- [ ] Frontend abre en `http://localhost:5500/index.html` y conecta con API local.
- [ ] Se crea boleta con `ROL_FUNC`, se ve en pendientes de jefatura, y se resuelve con `ROL_JEFE`.
- [ ] `GET /api/rrhh/justificaciones` responde correctamente con `ROL_RRHH`.
- [ ] `dotnet test` ejecuta correctamente en `backend/IntegradorMarcas.slnx`.

## 7) Troubleshooting rapido

- Si `sqlcmd` no existe: instalar SQLCMD o usar SSMS para ejecutar scripts.
- Si falla conexion SQL: validar instancia `localhost\SQLEXPRESS` y cadena en `appsettings.Development.json`.
- Si frontend no conecta: confirmar API viva en `http://localhost:5093` y revisar consola del navegador.
- Si smoke de jefatura no muestra pendientes: re-ejecutar bloque seed y verificar `JerarquiaAprobacion` vigente para Unidad `120`.
