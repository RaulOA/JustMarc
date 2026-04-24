# SQL Local + Seed + Run Spec

Fecha: 2026-04-23
Repositorio: Justificacion de Marca
Objetivo: levantar entorno local en Windows (SQL Server + backend .NET + frontend estatico), con datos minimos para validar flujos de Funcionario, Jefatura y RRHH.

## 1) Prerrequisitos exactos

### Software obligatorio
- Windows 10/11.
- .NET SDK 8.0.x.
- SQL Server 2019+ (recomendado SQL Server Express local: localhost\\SQLEXPRESS).
- SQL Server Management Studio (SSMS) 19+ o SQLCMD.
- Git (para clonar/actualizar repo).
- Navegador moderno (Edge/Chrome/Firefox).

### Verificacion rapida en PowerShell
```powershell
# .NET
 dotnet --version

# SQLCMD (opcional pero recomendado)
 sqlcmd -?
```

### Configuracion SQL local recomendada
- Instancia local: localhost\\SQLEXPRESS.
- Modo de autenticacion: Windows Authentication (para Development local).
- Certificado: usar TrustServerCertificate=True en la cadena de conexion local.

### Archivos de referencia usados
- README.md
- docs/Guia_Implementacion_Dev_Prod.md
- docs/manual-tecnico.md
- docs/db/001_integra_marcas_base_inicial.sql
- docs/db/002_integra_marcas_objetos.sql
- docs/db/ARCHIVOS_OBSOLETOS.md
- backend/src/IntegradorMarcas.Api/appsettings*.json
- backend/src/IntegradorMarcas.Api/Program.cs
- backend/src/IntegradorMarcas.Api/Properties/launchSettings.json
- backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs

## 2) Pasos para crear/restaurar base local

## Ruta A (recomendada): crear base desde scripts canonicos

1. Abrir SSMS y conectarse a localhost\\SQLEXPRESS.
2. Ejecutar en orden:
   - docs/db/001_integra_marcas_base_inicial.sql
   - docs/db/002_integra_marcas_objetos.sql
3. Verificar objetos principales:
```sql
USE INTEGRA_CNP;
SELECT SCHEMA_NAME(schema_id) AS Esquema, name AS Objeto, type_desc
FROM sys.objects
WHERE type IN ('U','V','FN','P')
ORDER BY SCHEMA_NAME(schema_id), name;
```
4. Confirmar que existe la funcion:
```sql
SELECT OBJECT_ID('Operacion.fn_AprobadoresVigentesPorSolicitante', 'FN') AS FnId;
```

## Ruta B (opcional): restaurar desde backup .bak
Nota: el repositorio no incluye .bak. Si Infra entrega uno:

```sql
-- 1) inspeccionar nombres logicos
RESTORE FILELISTONLY FROM DISK = 'C:\\Backups\\INTEGRA_CNP.bak';

-- 2) restaurar (ajustar nombres logicos y rutas)
RESTORE DATABASE INTEGRA_CNP
FROM DISK = 'C:\\Backups\\INTEGRA_CNP.bak'
WITH MOVE 'INTEGRA_CNP' TO 'C:\\Program Files\\Microsoft SQL Server\\MSSQL16.SQLEXPRESS\\MSSQL\\DATA\\INTEGRA_CNP.mdf',
     MOVE 'INTEGRA_CNP_log' TO 'C:\\Program Files\\Microsoft SQL Server\\MSSQL16.SQLEXPRESS\\MSSQL\\DATA\\INTEGRA_CNP_log.ldf',
     REPLACE, RECOVERY;
```

Despues de restaurar, ejecutar 002_integra_marcas_objetos.sql para alinear funcion y vistas de integracion.

## 3) Orden correcto de scripts SQL

Orden canonicamente valido (segun Guia_Implementacion_Dev_Prod.md y ARCHIVOS_OBSOLETOS.md):
1. docs/db/001_integra_marcas_base_inicial.sql
2. docs/db/002_integra_marcas_objetos.sql

No usar para setup nuevo:
- 001_init_integra_cnp.sql
- 007_integra_local_bridge.sql
- 008_add_comentario_resolucion.sql
- 009_admin_hierarchy_delegation_audit_foundation.sql
- 010_wizdom_empleado_normalization_staging.sql
Todos estan declarados como superseded en docs/db/ARCHIVOS_OBSOLETOS.md.

## 4) Estrategia de datos semilla minima para flujos clave

Contexto:
- El script 001 crea catalogos base, pero no deja un set minimo de usuarios/estructura para validar todo el ciclo por roles.
- Para probar Funcionario -> Jefatura -> RRHH se requiere: 3 usuarios, 1 estructura, 1 jerarquia vigente y al menos 1 justificacion pendiente.

Ejecutar este seed minimo en INTEGRA_CNP:

```sql
USE INTEGRA_CNP;
SET XACT_ABORT ON;

-- 1) Estructura organizacional base (codigo origen debe calzar con UnidadId del funcionario)
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

-- 3) Asignar jefatura del funcionario
UPDATE RecursosHumanos.Usuario
SET JefaturaId = @JefeId,
    ModificadoPor = 'seed_local',
    FechaHoraModificacion = SYSUTCDATETIME()
WHERE UsuarioId = @FuncionarioId
  AND (JefaturaId IS NULL OR JefaturaId <> @JefeId);

-- 4) Jerarquia vigente para que fn_AprobadoresVigentesPorSolicitante encuentre al jefe
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

-- 5) (Opcional) crear una justificacion pendiente de forma manual para smoke inicial
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

## 5) Comandos para correr backend y frontend

## Backend (.NET 8)
Desde la raiz del repo:

```powershell
dotnet restore backend/IntegradorMarcas.slnx
dotnet build backend/IntegradorMarcas.slnx
dotnet run --project backend/src/IntegradorMarcas.Api
```

URLs esperadas (launchSettings):
- http://localhost:5093
- https://localhost:7129
- http://localhost:5093/swagger
- http://localhost:5093/health

Config local esperada en backend/src/IntegradorMarcas.Api/appsettings.Development.json:
```json
{
  "ConnectionStrings": {
    "IntegraCnp": "Server=localhost\\SQLEXPRESS;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;",
    "WizdomReadOnly": "",
    "SifcnpReadOnly": ""
  },
  "Security": {
    "UseMockIdentity": true
  },
  "Swagger": {
    "Enabled": true
  }
}
```

## Frontend (estatico)
Opcion minima:
- Abrir index.html directamente en navegador.

Opcion recomendada (evitar restricciones locales del browser):
```powershell
# desde la raiz del repo
python -m http.server 5500
# luego abrir http://localhost:5500/index.html
```

Notas frontend:
- app.js usa base por defecto: http://localhost:5093.
- Usuarios demo para login: funcionario.ana, jefe.maria, rrhh.carlos.
- Headers enviados por frontend: X-User-Id, X-User-Role.

## 6) Pruebas de humo recomendadas

## Smoke 0: disponibilidad
1. GET http://localhost:5093/health -> 200 con { status: "ok" }.
2. Abrir http://localhost:5093/swagger.

## Smoke 1: crear boleta (Funcionario)
Request:
```bash
curl -X POST "http://localhost:5093/api/justificaciones" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 10" \
  -H "X-User-Role: ROL_FUNC" \
  -d '{
    "motivoGeneral": "Atraso por incidente vial",
    "detalles": [
      {
        "tipoJustificacionID": 1,
        "fechaMarca": "2026-04-23T00:00:00",
        "observacionDetalle": "Ingreso 08:22"
      }
    ]
  }'
```
Esperado: 201 Created.

## Smoke 2: pendientes jefatura
```bash
curl "http://localhost:5093/api/jefatura/justificaciones/pendientes" \
  -H "X-User-Id: 20" \
  -H "X-User-Role: ROL_JEFE"
```
Esperado: 200 con al menos una boleta pendiente (si se corrio seed).

## Smoke 3: resolver boleta
```bash
curl -X PATCH "http://localhost:5093/api/jefatura/justificaciones/{id}/resolver" \
  -H "Content-Type: application/json" \
  -H "X-User-Id: 20" \
  -H "X-User-Role: ROL_JEFE" \
  -d '{"accion":"APROBAR","comentario":"Validado"}'
```
Esperado: 204 No Content.

## Smoke 4: consulta global RRHH
```bash
curl "http://localhost:5093/api/rrhh/justificaciones?compania=CNP&estadoId=1" \
  -H "X-User-Id: 30" \
  -H "X-User-Role: ROL_RRHH"
```
Esperado: 200 con lista filtrada.

## Smoke 5: control de seguridad
- Repetir cualquier endpoint sin headers -> esperado 401.
- Probar endpoint RRHH con rol funcionario -> esperado 403.

## 7) Problemas comunes en Windows y como resolverlos

1. Error de conexion SQL (A network-related or instance-specific error)
- Causa tipica: instancia incorrecta o servicio apagado.
- Acciones:
  - Abrir SQL Server Configuration Manager.
  - Verificar servicio SQL Server (SQLEXPRESS) en Running.
  - Confirmar instancia en connection string: localhost\\SQLEXPRESS.

2. Login failed for user / Trusted_Connection falla
- Causa: autenticacion no alineada con entorno.
- Acciones:
  - En Development local: usar Trusted_Connection=True.
  - Si usa SQL Login: cambiar a User Id/Password y habilitar Mixed Mode.

3. API inicia pero POST/PATCH fallan con "Invalid column name 'Usr_Registro'"
- Causa: desalineacion entre queries de backend (usa Usr_Registro) y esquema SQL consolidado (usa CreadoPor/ModificadoPor).
- Confirmacion:
```sql
SELECT name FROM sys.columns WHERE object_id = OBJECT_ID('Operacion.Justificacion');
```
- Mitigacion rapida local (solo entorno de pruebas): agregar columnas de compatibilidad si faltan.
```sql
USE INTEGRA_CNP;

IF COL_LENGTH('Operacion.Justificacion', 'Usr_Registro') IS NULL
    ALTER TABLE Operacion.Justificacion ADD Usr_Registro VARCHAR(100) NULL;
IF COL_LENGTH('Operacion.JustificacionDetalle', 'Usr_Registro') IS NULL
    ALTER TABLE Operacion.JustificacionDetalle ADD Usr_Registro VARCHAR(100) NULL;
IF COL_LENGTH('Operacion.JerarquiaAprobacion', 'Usr_Registro') IS NULL
    ALTER TABLE Operacion.JerarquiaAprobacion ADD Usr_Registro VARCHAR(100) NULL;
IF COL_LENGTH('Operacion.DelegacionAprobacion', 'Usr_Registro') IS NULL
    ALTER TABLE Operacion.DelegacionAprobacion ADD Usr_Registro VARCHAR(100) NULL;
```
- Recomendacion estructural: alinear definitivamente SQL del backend con las columnas canonicamente definidas en 001.

4. Swagger no abre
- Causa: se ejecuta en Production o Swagger disabled.
- Acciones:
  - Revisar ASPNETCORE_ENVIRONMENT=Development.
  - Verificar Swagger.Enabled=true en appsettings.Development.json.

5. CORS o error de red desde frontend
- Causa: API apagada o URL base incorrecta.
- Acciones:
  - Confirmar backend en http://localhost:5093/health.
  - Verificar base URL en app.js (defaultBaseUrl) o session storage.

6. Puerto 5093 en uso
- Causa: otro proceso ocupando el puerto.
- Acciones:
```powershell
netstat -ano | findstr :5093
# identificar PID y liberar proceso, o cambiar applicationUrl en launchSettings.json
```

7. Certificado HTTPS local invalido
- Causa: certificado dev de .NET no confiado.
- Acciones:
```powershell
dotnet dev-certs https --clean
dotnet dev-certs https --trust
```

8. Error al ejecutar vistas de integracion WIZDOM/SIFCNP
- Causa: BD externas no existen en local.
- Nota:
  - Las vistas de 002 referencian [WIZDOM] y [SIFCNP].
  - Para pruebas MVP de flujos core no se requieren esas fuentes.
  - Si SQL rechaza por permisos/ausencia, crear DBs stub o adaptar temporalmente 002 en entorno local.

---

## Resumen operativo minimo
1. Ejecutar 001 y 002 en orden.
2. Aplicar seed minimo de usuarios/estructura/jerarquia.
3. Configurar appsettings.Development con localhost\\SQLEXPRESS.
4. Ejecutar backend con dotnet run.
5. Abrir index.html (o servir estatico por python -m http.server).
6. Validar smoke tests (health, crear, pendientes, resolver, RRHH).
