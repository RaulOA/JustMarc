# Guía de Implementación — Entornos Development y Production
**Sistema:** Integrador Marcas / Justificación de Marcas  
**Institución:** CNP & FANAL — Unidad de Tecnologías de la Información  
**Versión del doc:** 1.0 | Fecha: Abril 2026  
**Técnico de referencia:** Raúl Ortega Acuña  

---

## Tabla de Contenidos

1. [Prerrequisitos](#1-prerrequisitos)
2. [Orden de Setup de Base de Datos](#2-orden-de-setup-de-base-de-datos)
3. [Configuración Backend por Entorno](#3-configuración-backend-por-entorno)
4. [Comandos Run / Build](#4-comandos-run--build)
5. [Publish — Publicación a Producción](#5-publish--publicación-a-producción)
6. [Verificación Post-Despliegue](#6-verificación-post-despliegue)
7. [Rollback Básico](#7-rollback-básico)
8. [Troubleshooting Común](#8-troubleshooting-común)

---

## 1. Prerrequisitos

### Software base (ambos entornos)

| Componente | Versión mínima | Notas |
|---|---|---|
| .NET SDK | 8.0.x | Solo necesario en máquina de build; en servidor de prod basta el Runtime |
| .NET ASP.NET Core Runtime | 8.0.x | Requerido en el servidor de producción |
| SQL Server | 2019 (14.x) | Instancia de destino `INTEGRA_CNP` |
| SQL Server Management Studio | 19+ | Para ejecutar scripts de BD |

### Solo para Development

| Componente | Versión | Notas |
|---|---|---|
| .NET SDK 8 | completo | Incluye CLI dotnet |
| SQL Server Express | 2019+ | Instancia local `localhost\SQLEXPRESS` |
| Visual Studio Code | reciente | Con extensión C# Dev Kit |
| Git | cualquier | Para clonar el repo |

### Acceso a bases de datos externas (lectura)

| BD | Propósito | Script de extracción |
|---|---|---|
| `WIZDOM` | Datos de marcas físicas | `002_extract_wizdom_readonly.sql` / `005_extract_wizdom_targeted_min.sql` |
| `SIFCNP` | Histórico de justificaciones | `003_extract_sifcnp_readonly.sql` / `006_extract_sifcnp_targeted_min.sql` |

> **Nota:** En desarrollo local, las cadenas `WizdomReadOnly` y `SifcnpReadOnly` pueden apuntar a instancias locales de prueba o quedar vacías si no se integra el puente local.

---

## 2. Orden de Setup de Base de Datos

Ejecutar los scripts **en este orden exacto** en SQL Server Management Studio, conectado al servidor objetivo:

### 2.1 Entorno Development (local)

```
Paso 1: docs/db/001_init_integra_cnp.sql
Paso 2: docs/db/007_integra_local_bridge.sql
```

- El script `001` crea la BD `INTEGRA_CNP` si no existe, y define todas las tablas del dominio: `Roles`, `Estados`, `Cat_TiposJustificacion`, `Usuarios`, `Justificaciones_Encabezado`, `Justificaciones_Detalle`.
- El script `007` (Local Bridge) requiere que `INTEGRA_CNP` ya exista; falla rápido con un error descriptivo si no. Crea los esquemas `stg`, `bridge`, `ext` y estructuras de staging para WIZDOM y SIFCNP.

### 2.2 Entorno Production

```
Paso 1: docs/db/001_init_integra_cnp.sql
Paso 2: docs/db/004_extract_integra_cnp_readonly.sql  ← (usuario readonly para reportes, si aplica)
Paso 3: docs/db/007_integra_local_bridge.sql
```

> Scripts `002`, `003`, `005`, `006` son para extractar datos desde WIZDOM/SIFCNP hacia staging. Ejecutarlos solo si se va a activar el puente de datos o se requiere poblar `stg.*` en producción.

### 2.3 Validación post-script

```sql
USE INTEGRA_CNP;
SELECT TABLE_SCHEMA, TABLE_NAME FROM INFORMATION_SCHEMA.TABLES ORDER BY TABLE_SCHEMA, TABLE_NAME;
```

Debe listar tablas en esquemas `dbo`, `stg`, `bridge`, `ext`.

---

## 3. Configuración Backend por Entorno

### 3.1 Archivos de configuración

| Archivo | Rol |
|---|---|
| `appsettings.json` | Defaults base seguros para todos los entornos. Sin credenciales. |
| `appsettings.Development.json` | Overrides locales: connection strings, mock identity, Swagger habilitado. |
| `appsettings.Production.json` | Fuerza flags seguras: `UseMockIdentity=false`, `Swagger=false`. Sin credenciales. |

### 3.2 Valores clave por entorno

| Clave | Development | Production |
|---|---|---|
| `Security:UseMockIdentity` | `true` — cabeceras X-User-Id / X-User-Role enviadas manualmente | `false` — cabeceras provistas por el proxy/gateway real |
| `Swagger:Enabled` | `true` — UI en `/swagger` | `false` — desactivado por seguridad |
| `ConnectionStrings:IntegraCnp` | Definida en `appsettings.Development.json` (Windows Auth local) | **Variable de entorno del servidor** (no en archivo) |
| `ConnectionStrings:WizdomReadOnly` | Definida en `appsettings.Development.json` | Variable de entorno del servidor |
| `ConnectionStrings:SifcnpReadOnly` | Definida en `appsettings.Development.json` | Variable de entorno del servidor |

### 3.3 Development — appsettings.Development.json actual

```json
{
  "ConnectionStrings": {
    "IntegraCnp": "Server=localhost\\SQLEXPRESS;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;",
    "WizdomReadOnly": "Server=localhost\\SQLEXPRESS;Database=WIZDOM;Trusted_Connection=True;TrustServerCertificate=True;",
    "SifcnpReadOnly": "Server=localhost\\SQLEXPRESS;Database=SIFCNP;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Security": {
    "UseMockIdentity": true
  },
  "Swagger": {
    "Enabled": true
  }
}
```

### 3.4 Production — Variables de entorno obligatorias en el servidor

```
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__IntegraCnp=Server=<servidor_prod>;Database=INTEGRA_CNP;User Id=<usuario>;Password=<clave>;TrustServerCertificate=True;
ConnectionStrings__WizdomReadOnly=Server=<servidor_wizdom>;Database=WIZDOM;User Id=<usuario_ro>;Password=<clave_ro>;TrustServerCertificate=True;
ConnectionStrings__SifcnpReadOnly=Server=<servidor_sifcnp>;Database=SIFCNP;User Id=<usuario_ro>;Password=<clave_ro>;TrustServerCertificate=True;
```

> **Importante:** El separador en variables de entorno es `__` (doble guión bajo), no `:`.  
> **La API falla al iniciar** si `ConnectionStrings:IntegraCnp` no está presente en entornos no-Development (fail-fast explícito en `Program.cs`).  
> **No colocar credenciales** en `appsettings.Production.json` ni subirlas al repositorio.

---

## 4. Comandos Run / Build

Todos los comandos se ejecutan desde la raíz del workspace (`c:\...\Justificacion de Marca\`) salvo que se indique lo contrario.

### 4.1 Restaurar dependencias

```powershell
dotnet restore backend/IntegradorMarcas.slnx
```

### 4.2 Compilar (verificar que no hay errores)

```powershell
dotnet build backend/IntegradorMarcas.slnx
```

> Si la terminal ya está posicionada en `backend/`, omitir el prefijo `backend/`:
> ```powershell
> dotnet build IntegradorMarcas.slnx
> ```

### 4.3 Ejecutar en Development

```powershell
dotnet run --project backend/src/IntegradorMarcas.Api
```

La API inicia en:
- HTTP: `http://localhost:5093`
- HTTPS: `https://localhost:7129`
- Swagger UI: `http://localhost:5093/swagger`
- Health check: `http://localhost:5093/health`

### 4.4 Headers de identidad mock (solo Development)

Incluir en cada request HTTP:

```
X-User-Id: 1
X-User-Role: ROL_FUNC   (o ROL_JEFE para probar jefatura)
```

---

## 5. Publish — Publicación a Producción

### 5.1 Generar artefacto de publicación

```powershell
dotnet publish backend/src/IntegradorMarcas.Api `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output ./publish/IntegradorMarcas
```

- `--self-contained false`: requiere .NET 8 Runtime instalado en el servidor.
- `--self-contained true`: incluye el runtime en el artefacto (mayor tamaño, sin dependencia en el servidor).
- `--runtime win-x64`: ajustar si el servidor es Linux (`linux-x64`).

### 5.2 Transferir artefacto al servidor

Copiar la carpeta `./publish/IntegradorMarcas/` al servidor de producción por el medio habitual (SMB, SCP, pipeline CI/CD, etc.).

### 5.3 Configurar variables de entorno en el servidor

En Windows Server (PowerShell, como Administrador):

```powershell
[System.Environment]::SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Production", "Machine")
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__IntegraCnp", "<cadena_prod>", "Machine")
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__WizdomReadOnly", "<cadena_ro>", "Machine")
[System.Environment]::SetEnvironmentVariable("ConnectionStrings__SifcnpReadOnly", "<cadena_ro>", "Machine")
```

> Reiniciar el proceso/servicio después de cambiar variables de entorno a nivel `Machine`.

### 5.4 Ejecutar la API en producción

**Opción A — Ejecutar directamente (prueba rápida):**

```powershell
cd C:\inetpub\IntegradorMarcas   # ruta donde se copió el artefacto
.\IntegradorMarcas.Api.exe
```

**Opción B — Como servicio Windows con IIS (recomendado):**

1. Instalar IIS + módulo ASP.NET Core Hosting Bundle en el servidor.
2. Crear sitio IIS apuntando a la carpeta publicada.
3. Asegurarse de que el pool de aplicaciones use `No Managed Code`.
4. El archivo `web.config` generado automáticamente por `dotnet publish` configura el handler de ASP.NET Core.

**Opción C — Como servicio Windows con `sc` / NSSM:**

```powershell
nssm install IntegradorMarcas "C:\inetpub\IntegradorMarcas\IntegradorMarcas.Api.exe"
nssm start IntegradorMarcas
```

---

## 6. Verificación Post-Despliegue

### 6.1 Health check

```
GET http://<servidor>:<puerto>/health
```

Respuesta esperada (`200 OK`):

```json
{ "status": "ok", "utc": "2026-04-23T..." }
```

### 6.2 Endpoints funcionales mínimos

| Endpoint | Método | Propósito | Header requerido (Prod) |
|---|---|---|---|
| `/health` | GET | Liveness check | Ninguno |
| `/api/justificaciones` | POST | Crear boleta | X-User-Id, X-User-Role: ROL_FUNC |
| `/api/justificaciones/mias` | GET | Historial propio | X-User-Id, X-User-Role: ROL_FUNC |
| `/api/jefatura/justificaciones/pendientes` | GET | Pendientes jefatura | X-User-Id, X-User-Role: ROL_JEFE |
| `/api/jefatura/justificaciones/{id}/resolver` | PATCH | Aprobar/rechazar | X-User-Id, X-User-Role: ROL_JEFE |

### 6.3 Verificar Swagger desactivado en Prod

```
GET http://<servidor>:<puerto>/swagger
```

Debe retornar `404` — confirma que Swagger está desactivado.

### 6.4 Verificar conectividad BD

Si el health check responde pero los endpoints de datos fallan con `500`, revisar:

```powershell
# Probar conectividad SQL desde el servidor de producción
sqlcmd -S <servidor_bd> -d INTEGRA_CNP -Q "SELECT TOP 1 EstadoID FROM dbo.Estados"
```

---

## 7. Rollback Básico

### 7.1 Rollback de la API

1. Detener el servicio/proceso actual.
2. Restaurar la carpeta del artefacto anterior (mantener siempre el backup de la versión previa).
3. Reiniciar el servicio.

```powershell
# Detener servicio (si se usó NSSM)
nssm stop IntegradorMarcas

# Restaurar artefacto anterior
Remove-Item -Recurse "C:\inetpub\IntegradorMarcas"
Copy-Item -Recurse "C:\inetpub\IntegradorMarcas_backup_YYYYMMDD" "C:\inetpub\IntegradorMarcas"

nssm start IntegradorMarcas
```

### 7.2 Rollback de Base de Datos

Los scripts SQL actuales (`001`, `007`) son **idempotentes** (usan `IF NOT EXISTS` / `IF OBJECT_ID IS NULL`), por lo que re-ejecutarlos no causa daño. Sin embargo, **no incluyen scripts de reversión (DROP)**. Para rollback de BD:

- Usar backup tomado antes del despliegue.
- Restaurar con SSMS: `Restore Database > From Device`.

> **Práctica recomendada:** Tomar backup de `INTEGRA_CNP` inmediatamente antes de cada despliegue que incluya cambios de esquema.

---

## 8. Troubleshooting Común

### Error: `ConnectionStrings:IntegraCnp no esta configurada para entorno no-Development`

**Causa:** La variable de entorno `ConnectionStrings__IntegraCnp` no está definida en el servidor o `ASPNETCORE_ENVIRONMENT` no es `Development`.  
**Solución:** Verificar que las variables de entorno estén definidas a nivel `Machine` y que el proceso fue reiniciado tras el cambio.

---

### Error: `MSB1009` al ejecutar dotnet build/run

**Causa:** La terminal ya está posicionada dentro de `backend/` y el comando incluye el prefijo `backend/`, resultando en una ruta duplicada.  
**Solución:**  
```powershell
# Desde la raíz del workspace:
dotnet build backend/IntegradorMarcas.slnx

# Desde backend/:
dotnet build IntegradorMarcas.slnx
```

---

### Error `403 Forbidden` en endpoints de jefatura o RRHH

**Causa:** El header `X-User-Role` no coincide con el rol requerido por el endpoint, o `UseMockIdentity=false` en Development sin proxy que inyecte headers.  
**Solución:**  
- En Development: confirmar que `appsettings.Development.json` tiene `"UseMockIdentity": true` y que el request incluye los headers correctos.
- En Production: confirmar que el gateway/proxy upstream inyecta `X-User-Id` y `X-User-Role` en cada request autenticado.

---

### Error de conexión SQL: `Login failed` o `Cannot open database`

**Causa posible 1:** La cadena de conexión usa Windows Auth (`Trusted_Connection=True`) pero el proceso de la API corre bajo una cuenta sin permisos en SQL Server.  
**Solución:** Usar SQL Auth (usuario/contraseña) en la cadena de conexión de producción, o asignar permisos al usuario del servicio.

**Causa posible 2:** El certificado SSL del servidor SQL no es confiable.  
**Solución:** Agregar `TrustServerCertificate=True` a la cadena de conexión (ya incluido en los ejemplos del README).

---

### La BD `INTEGRA_CNP` no tiene las tablas del dominio

**Causa:** El script `001_init_integra_cnp.sql` no fue ejecutado o falló silenciosamente.  
**Solución:** Re-ejecutar `001_init_integra_cnp.sql` completo — es idempotente, no destruye datos existentes. Verificar con:

```sql
USE INTEGRA_CNP;
SELECT COUNT(*) FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_SCHEMA = 'dbo';
-- Resultado esperado: >= 5 tablas (Roles, Estados, Cat_TiposJustificacion, Usuarios, Justificaciones_Encabezado, Justificaciones_Detalle)
```

---

### El script `007_integra_local_bridge.sql` falla con RAISERROR

**Causa:** `INTEGRA_CNP` no existe porque `001_init_integra_cnp.sql` no fue ejecutado primero.  
**Solución:** Respetar el orden de ejecución: `001` antes que `007`.

---

### Frontend no conecta con la API (CORS)

**Causa:** El origen del frontend no está en la lista permitida. En el código actual (`Program.cs`), la política `LocalFrontend` usa `.SetIsOriginAllowed(_ => true)` (todos los orígenes permitidos).  
**Acción en Producción:** Antes de ir a producción real, restringir la política CORS al dominio específico del frontend:

```csharp
policy.WithOrigins("https://marcas.cnp.go.cr").AllowAnyHeader().AllowAnyMethod();
```

---

*Documento generado para uso interno de la UTI — CNP/FANAL. Abril 2026.*
