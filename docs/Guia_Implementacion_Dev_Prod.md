# Guía de Implementación — Entornos Development y Production

**Sistema:** Integrador Marcas / Justificación de Marcas  
**Institución:** CNP & FANAL — Unidad de Tecnologías de la Información  
**Versión:** 1.0 | Abril 2026  
**Técnico de referencia:** Raúl Ortega Acuña

> Documento orientado a runbooks de despliegue y operacion por entorno.
> Para detalle de arquitectura, API, frontend, flujos, convenciones y pruebas, ver:
> - [arquitectura-codigo-actual.md](arquitectura-codigo-actual.md)
> - [api-endpoints-reference.md](api-endpoints-reference.md)
> - [frontend-modulos-y-flujos.md](frontend-modulos-y-flujos.md)
> - [flujos-datos-end-to-end.md](flujos-datos-end-to-end.md)
> - [convenciones-codigo-y-documentacion.md](convenciones-codigo-y-documentacion.md)
> - [pruebas-estrategia-y-cobertura.md](pruebas-estrategia-y-cobertura.md)

---

## Tabla de Contenidos

1. [Prerrequisitos](#1-prerrequisitos)
2. [Resumen de Arquitectura](#2-resumen-de-arquitectura)
3. [Orden de Setup de Base de Datos](#3-orden-de-setup-de-base-de-datos)
4. [Runbook de Desarrollo](#4-runbook-de-desarrollo)
5. [Runbook de Publicación a Producción](#5-runbook-de-publicación-a-producción)
6. [Variables de Entorno](#6-variables-de-entorno)
7. [Checklist de Verificación Post-Despliegue](#7-checklist-de-verificación-post-despliegue)
8. [Rollback Básico](#8-rollback-básico)
9. [Troubleshooting](#9-troubleshooting)

---

## 1. Prerrequisitos

### Software requerido en ambos entornos

| Componente | Versión mínima | Notas |
|---|---|---|
| .NET SDK | 8.0.x | Solo en máquina de build; en servidor de prod basta el Runtime |
| ASP.NET Core Runtime | 8.0.x | Requerido en el servidor de producción |
| SQL Server | 2019 (15.x) | Instancia destino: `INTEGRA_CNP` |
| SQL Server Management Studio | 19+ | Para ejecutar scripts de BD |

### Solo para Development local

| Componente | Versión | Notas |
|---|---|---|
| .NET SDK 8 | completo | Incluye CLI `dotnet` |
| SQL Server Express | 2019+ | Instancia local `localhost\SQLEXPRESS` |
| Visual Studio Code | reciente | Con extensión C# Dev Kit |
| Git | cualquier | Para clonar el repositorio |

### Acceso a bases de datos externas (solo lectura)

| Base de datos | Propósito | Scripts asociados |
|---|---|---|
| `WIZDOM` | Marcas físicas de empleados | `docs/db/002_extract_wizdom_readonly.sql`, `005_extract_wizdom_targeted_min.sql` |
| `SIFCNP` | Histórico de justificaciones | `docs/db/003_extract_sifcnp_readonly.sql`, `006_extract_sifcnp_targeted_min.sql` |

> En desarrollo local, las cadenas `WizdomReadOnly` y `SifcnpReadOnly` pueden apuntar a instancias locales de prueba o dejarse vacías si no se activa el puente de datos.

---

## 2. Resumen de Arquitectura

> **Topología importante:** la API y la base de datos **no residen en la misma máquina**. Son VMs independientes en el servidor. La comunicación entre ellas es siempre mediante cadena de conexión TCP con host/IP, puerto y credenciales explícitas. No se usa Windows Authentication entre VMs a menos que el dominio Active Directory lo permita explícitamente.

```
[PC del usuario — navegador]
        |  HTTPS
        v
[VM de Aplicación — servidor de releases]
  IntegradorMarcas.Api (.NET 8)
  IIS / NSSM
        |  TCP/IP  →  Server=<IP_VM_BD>,1433
        v
[VM de Base de Datos — servidor SQL]
  SQL Server 2019+
  Base: INTEGRA_CNP
   ├── dbo.*  (dominio: Justificaciones, Usuarios, Roles, Estados)
   ├── stg.*  (staging datos externos)
   ├── bridge.* (vistas puente locales)
   └── ext.*  (placeholders, activar cuando se conecte a externas)
        ^  lectura futura (cadena de conexión separada)
        |
[VM / Servidor WIZDOM]      [VM / Servidor SIFCNP]
  Solo lectura                Solo lectura
```

**Regla de oro:** La app solo conoce la dirección de la VM de BD. Nunca se conecta directamente a WIZDOM ni SIFCNP desde la app; esas fuentes alimentarán vistas/tablas dentro de INTEGRA_CNP.

### Proyectos de la solución (`backend/IntegradorMarcas.slnx`)

| Proyecto | Rol |
|---|---|
| `IntegradorMarcas.Api` | Punto de entrada HTTP, controllers, configuración |
| `IntegradorMarcas.Application` | Servicios de aplicación, DTOs, validaciones |
| `IntegradorMarcas.Domain` | Entidades, constantes de dominio |
| `IntegradorMarcas.Infrastructure` | Repositorios, acceso a datos, queries |
| `IntegradorMarcas.Tests` | Pruebas unitarias |

---

## 3. Orden de Setup de Base de Datos

Ejecutar los scripts **en este orden exacto** en SSMS, conectado al servidor objetivo:

### 3.1 Entorno Development (local)

```
Paso 1: docs/db/001_init_integra_cnp.sql
Paso 2: docs/db/007_integra_local_bridge.sql
```

**¿Qué hace cada script?**

- `001_init_integra_cnp.sql` — Crea la base de datos `INTEGRA_CNP` si no existe y define todas las tablas del dominio: `Roles`, `Estados`, `Cat_TiposJustificacion`, `Usuarios`, `Justificaciones_Encabezado`, `Justificaciones_Detalle`. Incluye datos semilla básicos. Es **idempotente** (usa `IF NOT EXISTS`).
- `007_integra_local_bridge.sql` — Verifica que `INTEGRA_CNP` exista (falla rápido con error descriptivo si no). Crea los esquemas `stg`, `bridge`, `ext` y las estructuras de staging para WIZDOM y SIFCNP.

### 3.2 Entorno Production

```
Paso 1: docs/db/001_init_integra_cnp.sql
Paso 2: docs/db/004_extract_integra_cnp_readonly.sql   ← usuario readonly para reportes (si aplica)
Paso 3: docs/db/007_integra_local_bridge.sql
```

> Los scripts `002`, `003`, `005`, `006` son para extraer datos desde WIZDOM/SIFCNP hacia staging. Ejecutarlos únicamente si se va a activar el puente de datos o poblar `stg.*` en producción.

### 3.3 Validación post-script

```sql
USE INTEGRA_CNP;
SELECT TABLE_SCHEMA, TABLE_NAME
FROM INFORMATION_SCHEMA.TABLES
ORDER BY TABLE_SCHEMA, TABLE_NAME;
```

Debe listar tablas en esquemas `dbo`, `stg`, `bridge` y `ext`.

---

## 4. Runbook de Desarrollo

Todos los comandos se ejecutan desde la **raíz del workspace** salvo indicación contraria.

### 4.1 Clonar y restaurar dependencias

```powershell
git clone <url-del-repositorio>
cd "Justificacion de Marca"
dotnet restore backend/IntegradorMarcas.slnx
```

### 4.2 Configurar cadenas de conexión locales

Editar `backend/src/IntegradorMarcas.Api/appsettings.Development.json`:

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

> **Por qué `Trusted_Connection=True` solo en Development:**  
> En tu MV local, la app y el SQL Server Express corren en la **misma máquina**. Windows Authentication (`Trusted_Connection`) usa el usuario de Windows del proceso, lo que funciona localmente sin contraseña.  
> En producción, la app y la BD son **VMs separadas**. `Trusted_Connection` fallará a menos que ambas estén en el mismo dominio Active Directory con la cuenta de servicio configurada. Lo habitual es usar SQL Authentication (usuario + contraseña) con cadena de conexión apuntando a la IP o hostname de la VM de BD.
>
> `WizdomReadOnly` y `SifcnpReadOnly` se dejan vacíos en desarrollo porque el puente aún no está activo.

### 4.3 Compilar

```powershell
dotnet build backend/IntegradorMarcas.slnx
```

> Si la terminal ya está posicionada en `backend/`, omitir el prefijo `backend/`:
> ```powershell
> dotnet build IntegradorMarcas.slnx
> ```

### 4.4 Ejecutar la API en Development

```powershell
dotnet run --project backend/src/IntegradorMarcas.Api
```

La API inicia en:

| URL | Propósito |
|---|---|
| `http://localhost:5093` | HTTP |
| `https://localhost:7129` | HTTPS |
| `http://localhost:5093/swagger` | Swagger UI |
| `http://localhost:5093/health` | Health check |

### 4.5 Headers de identidad mock (solo Development)

Incluir en cada request HTTP mientras `UseMockIdentity=true`:

```
X-User-Id: 1
X-User-Role: ROL_FUNC
```

Usar `ROL_JEFE` para probar los endpoints de jefatura, `ROL_RRHH` para los de RRHH.

### 4.6 Ejecutar pruebas unitarias

```powershell
dotnet test backend/IntegradorMarcas.slnx
```

---

## 5. Runbook de Publicación a Producción

### 5.1 Generar artefacto de publicación

```powershell
dotnet publish backend/src/IntegradorMarcas.Api `
    --configuration Release `
    --runtime win-x64 `
    --self-contained false `
    --output ./publish/IntegradorMarcas
```

| Parámetro | Descripción |
|---|---|
| `--self-contained false` | Requiere .NET 8 Runtime instalado en el servidor (artefacto más pequeño) |
| `--self-contained true` | Incluye el runtime en el artefacto (sin dependencia en el servidor, mayor tamaño) |
| `--runtime win-x64` | Ajustar a `linux-x64` si el servidor destino es Linux |

### 5.2 Transferir artefacto al servidor

Copiar la carpeta `./publish/IntegradorMarcas/` al servidor de producción (SMB, SCP, pipeline CI/CD, etc.).

### 5.3 Configurar variables de entorno en el servidor de aplicaciones (VM de releases)

> La VM de aplicaciones **no tiene SQL Server**. Se conecta a la VM de BD mediante TCP/IP usando el hostname o IP de esa VM. Nunca use `localhost` ni `Trusted_Connection=True` en producción si son VMs separadas.

Ejecutar en la **VM de releases** como **Administrador** en PowerShell:

```powershell
# Entorno ASP.NET Core
[System.Environment]::SetEnvironmentVariable(
    "ASPNETCORE_ENVIRONMENT", "Production", "Machine")

# Cadena hacia la VM de BD (reemplazar IP_VM_BD, usuario y contraseña)
[System.Environment]::SetEnvironmentVariable(
    "ConnectionStrings__IntegraCnp",
    "Server=<IP_VM_BD>,1433;Database=INTEGRA_CNP;User Id=<usuario_svc>;Password=<clave>;TrustServerCertificate=True;",
    "Machine")

# Solo activar cuando el puente a fuentes externas esté listo
[System.Environment]::SetEnvironmentVariable(
    "ConnectionStrings__WizdomReadOnly",
    "Server=<IP_VM_WIZDOM>,1433;Database=WIZDOM;User Id=<usuario_ro>;Password=<clave_ro>;TrustServerCertificate=True;",
    "Machine")

[System.Environment]::SetEnvironmentVariable(
    "ConnectionStrings__SifcnpReadOnly",
    "Server=<IP_VM_SIFCNP>,1433;Database=SIFCNP;User Id=<usuario_ro>;Password=<clave_ro>;TrustServerCertificate=True;",
    "Machine")
```

> **Reiniciar** el proceso o servicio después de cambiar variables a nivel `Machine`.

#### Parámetros de cadena de conexión explicados

| Parámetro | Valor de ejemplo | Descripción |
|---|---|---|
| `Server` | `192.168.10.15,1433` | IP (o hostname) de la VM de BD y puerto SQL |
| `Database` | `INTEGRA_CNP` | Nombre de la base de datos destino |
| `User Id` | `svc_marcas` | Usuario SQL dedicado al servicio, con permisos mínimos |
| `Password` | `...` | Contraseña del usuario SQL. **Nunca en el repo.** |
| `TrustServerCertificate` | `True` | Evita error de SSL si el certificado no es de CA pública |
| `Trusted_Connection` | **Omitir en Prod** | Solo para máquina local. En VMs separadas usar `User Id`/`Password` |

### 5.4 Iniciar la API en producción

**Opción A — Prueba directa (no recomendada para producción estable):**

```powershell
cd C:\inetpub\IntegradorMarcas
.\IntegradorMarcas.Api.exe
```

**Opción B — IIS + ASP.NET Core Hosting Bundle (recomendada):**

1. Instalar [ASP.NET Core Hosting Bundle](https://dotnet.microsoft.com/download) en el servidor.
2. Crear sitio IIS apuntando a la carpeta publicada.
3. Configurar el pool de aplicaciones en **No Managed Code**.
4. El `web.config` generado por `dotnet publish` configura automáticamente el handler de ASP.NET Core.

**Opción C — Servicio Windows con NSSM:**

```powershell
nssm install IntegradorMarcas "C:\inetpub\IntegradorMarcas\IntegradorMarcas.Api.exe"
nssm start IntegradorMarcas
```

---

## 6. Variables de Entorno

### 6.1 Tabla de configuración por entorno

| Clave | Development | Production |
|---|---|---|
| `ASPNETCORE_ENVIRONMENT` | `Development` | `Production` |
| `Security:UseMockIdentity` | `true` (en `appsettings.Development.json`) | `false` (en `appsettings.Production.json`) |
| `Swagger:Enabled` | `true` (en `appsettings.Development.json`) | `false` (en `appsettings.Production.json`) |
| `ConnectionStrings:IntegraCnp` | En `appsettings.Development.json` | **Variable de entorno del servidor** |
| `ConnectionStrings:WizdomReadOnly` | En `appsettings.Development.json` | Variable de entorno del servidor |
| `ConnectionStrings:SifcnpReadOnly` | En `appsettings.Development.json` | Variable de entorno del servidor |

### 6.2 Archivos de configuración del backend

| Archivo | Rol |
|---|---|
| `appsettings.json` | Defaults base para todos los entornos. Sin credenciales. |
| `appsettings.Development.json` | Overrides locales: connection strings, mock identity, Swagger habilitado. |
| `appsettings.Production.json` | Fuerza flags seguras. Sin credenciales ni secretos. |

### 6.3 Ejemplo de variables de entorno para Production

> Reemplazar `192.168.10.X` con la IP real de cada VM. Si el SQL Server usa el puerto por defecto (1433), puede omitirse.

```
ASPNETCORE_ENVIRONMENT=Production

# VM de BD principal (única requerida para arrancar)
ConnectionStrings__IntegraCnp=Server=192.168.10.15,1433;Database=INTEGRA_CNP;User Id=svc_marcas;Password=<clave>;TrustServerCertificate=True;

# VMs de fuentes externas (opcionales hasta activar el puente)
ConnectionStrings__WizdomReadOnly=Server=192.168.10.20,1433;Database=WIZDOM;User Id=svc_marcas_ro;Password=<clave_ro>;TrustServerCertificate=True;
ConnectionStrings__SifcnpReadOnly=Server=192.168.10.25,1433;Database=SIFCNP;User Id=svc_marcas_ro;Password=<clave_ro>;TrustServerCertificate=True;
```

> **Importante:**
> - El separador en variables de entorno es `__` (doble guión bajo), no `:`.
> - **Nunca** usar `Trusted_Connection=True` ni `Integrated Security=SSPI` en estas cadenas a menos que el equipo de infraestructura confirme que ambas VMs están en dominio AD con la cuenta de servicio delegada.
> - `ConnectionStrings__IntegraCnp` es **obligatoria**. La API no arranca sin ella en Production.
> - Las otras dos son opcionales hasta que se active el puente de datos.  
> **La API falla al iniciar** si `ConnectionStrings:IntegraCnp` no está presente en entornos no-Development (fail-fast en `Program.cs`).  
> **No colocar credenciales** en `appsettings.Production.json` ni en el repositorio.

---

## 7. Checklist de Verificación Post-Despliegue

### 7.1 Health check

```
GET http://<servidor>:<puerto>/health
```

Respuesta esperada (`200 OK`):

```json
{ "status": "ok", "utc": "2026-04-23T..." }
```

### 7.2 Endpoints mínimos funcionales

| Endpoint | Método | Validación | Header requerido (Prod) |
|---|---|---|---|
| `/health` | GET | `200 OK`, body `{ "status": "ok" }` | — |
| `/api/justificaciones` | POST | `201 Created` con cuerpo válido | `X-User-Id`, `X-User-Role: ROL_FUNC` |
| `/api/justificaciones/mias` | GET | `200 OK`, array (puede ser vacío) | `X-User-Id`, `X-User-Role: ROL_FUNC` |
| `/api/jefatura/justificaciones/pendientes` | GET | `200 OK`, array | `X-User-Id`, `X-User-Role: ROL_JEFE` |
| `/api/jefatura/justificaciones/{id}/resolver` | PATCH | `200 OK` | `X-User-Id`, `X-User-Role: ROL_JEFE` |

### 7.3 Verificar Swagger desactivado en Prod

```
GET http://<servidor>:<puerto>/swagger
```

Debe retornar `404`. Si retorna `200`, Swagger está activo — revisar que `ASPNETCORE_ENVIRONMENT=Production` está correctamente configurado.

### 7.4 Verificar conectividad BD desde la VM de aplicaciones

Desde la **VM de releases** (no desde la VM de BD), probar que la conexión TCP llega:

```powershell
# Prueba de puerto TCP (reemplazar IP y puerto)
Test-NetConnection -ComputerName 192.168.10.15 -Port 1433
# Esperado: TcpTestSucceeded : True

# Si sqlcmd está disponible en la VM de aplicaciones:
sqlcmd -S 192.168.10.15,1433 -d INTEGRA_CNP -U svc_marcas -P <clave> `
       -Q "SELECT TOP 1 EstadoID FROM dbo.Estados"
# Debe retornar al menos una fila.
```

> Si `TcpTestSucceeded` es `False`, el firewall de la VM de BD o del servidor bloquea el puerto 1433. El equipo de infraestructura debe abrir la regla entre ambas VMs.

### 7.5 Checklist rápido

- [ ] `GET /health` responde `200`
- [ ] Swagger retorna `404`
- [ ] `POST /api/justificaciones` crea registro en BD
- [ ] Variables de entorno definidas a nivel `Machine`
- [ ] Backup de `INTEGRA_CNP` tomado antes del despliegue
- [ ] Logs del servicio sin errores de conexión SQL

---

## 8. Rollback Básico

### 8.1 Rollback de la API

1. Detener el servicio/proceso.
2. Restaurar la carpeta del artefacto anterior (siempre mantener backup de la versión previa).
3. Reiniciar el servicio.

```powershell
# Detener servicio (si se usó NSSM)
nssm stop IntegradorMarcas

# Restaurar artefacto anterior
Remove-Item -Recurse "C:\inetpub\IntegradorMarcas"
Copy-Item -Recurse "C:\inetpub\IntegradorMarcas_backup_YYYYMMDD" "C:\inetpub\IntegradorMarcas"

nssm start IntegradorMarcas
```

### 8.2 Rollback de Base de Datos

Los scripts `001` y `007` son **idempotentes** — re-ejecutarlos no destruye datos. Sin embargo, **no incluyen scripts de reversión (DROP)**. Para rollback de BD:

1. Usar el backup tomado antes del despliegue.
2. Restaurar en SSMS: `Restaurar base de datos > Desde dispositivo`.

> **Práctica obligatoria:** Tomar backup de `INTEGRA_CNP` inmediatamente antes de cada despliegue que incluya cambios de esquema.

---

## 9. Troubleshooting

### `ConnectionStrings:IntegraCnp no está configurada para entorno no-Development`

**Causa:** La variable de entorno `ConnectionStrings__IntegraCnp` no está definida en el servidor o `ASPNETCORE_ENVIRONMENT` no está seteado.  
**Solución:** Verificar las variables a nivel `Machine` y reiniciar el proceso tras el cambio.

---

### Error `MSB1009` al ejecutar `dotnet build` o `dotnet run`

**Causa:** La terminal está posicionada en `backend/` y el comando incluye el prefijo `backend/`, generando una ruta duplicada.  
**Solución:**

```powershell
# Desde la raíz del workspace:
dotnet build backend/IntegradorMarcas.slnx

# Desde backend/:
dotnet build IntegradorMarcas.slnx
```

---

### Error `403 Forbidden` en endpoints de jefatura o RRHH

**Causa:** El header `X-User-Role` no coincide con el rol requerido, o `UseMockIdentity=false` en Development.  
**Solución (Development):** Confirmar que `appsettings.Development.json` tiene `"UseMockIdentity": true` y que el request incluye los headers correctos.  
**Solución (Production):** Confirmar que el gateway/proxy upstream inyecta `X-User-Id` y `X-User-Role` en cada request autenticado.

---

### Error SQL: `Login failed` o `Cannot open database`

**Causa 1 — VMs separadas con Windows Auth:**  
Si la cadena usa `Trusted_Connection=True` y las VMs no están en el mismo dominio AD, el login falla.  
**Solución:** Cambiar a SQL Auth en la cadena de producción:
```
User Id=svc_marcas;Password=<clave>;
```
Eliminar `Trusted_Connection=True` o `Integrated Security=SSPI` de esa cadena.

**Causa 2 — Nombre del servidor incorrecto:**  
Usaste `localhost` o `Server=.` en la VM de aplicaciones. Desde otra VM, `localhost` apunta a sí misma, no a la VM de BD.  
**Solución:** Usar IP o hostname real de la VM de BD: `Server=192.168.10.15,1433`.

**Causa 3 — Puerto bloqueado por firewall:**  
`Test-NetConnection -ComputerName <IP_BD> -Port 1433` retorna `False`.  
**Solución:** Solicitar a infraestructura apertura del puerto 1433 entre las VMs.

**Causa 4 — Certificado SSL del servidor SQL no confiable:**  
**Solución:** Agregar `TrustServerCertificate=True` a la cadena de conexión.

---

### La BD `INTEGRA_CNP` no tiene tablas del dominio

**Causa:** El script `001_init_integra_cnp.sql` no fue ejecutado o falló.  
**Solución:** Re-ejecutar `docs/db/001_init_integra_cnp.sql` (es idempotente). Verificar con:

```sql
USE INTEGRA_CNP;
SELECT COUNT(*) AS total_tablas
FROM INFORMATION_SCHEMA.TABLES
WHERE TABLE_SCHEMA = 'dbo';
-- Esperado: >= 6 (Roles, Estados, Cat_TiposJustificacion, Usuarios, Justificaciones_Encabezado, Justificaciones_Detalle)
```

---

### El script `007_integra_local_bridge.sql` falla con `RAISERROR`

**Causa:** `INTEGRA_CNP` no existe porque `001_init_integra_cnp.sql` no fue ejecutado primero.  
**Solución:** Respetar el orden: ejecutar `001` antes que `007`.

---

### Frontend no conecta con la API (error CORS)

**Causa:** El origen del frontend no está en la lista permitida.  
**Estado actual:** La política `LocalFrontend` en `Program.cs` permite todos los orígenes (`.SetIsOriginAllowed(_ => true)`).  
**Acción antes de ir a producción real:** Restringir al dominio del frontend:

```csharp
policy.WithOrigins("https://marcas.cnp.go.cr").AllowAnyHeader().AllowAnyMethod();
```

---

*Documento de uso interno — UTI, CNP/FANAL. Abril 2026.*
