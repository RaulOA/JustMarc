# Justificacion de Marca

Guia corta de onboarding para levantar y probar el proyecto en pocos minutos.

## Que es esta app

Aplicacion para gestionar justificaciones de marca.

- Frontend estatico: login y dashboard por rol.
- Backend .NET 8: API REST con autenticacion por headers.
- SQL Server: scripts en docs/db para esquema y datos base.

## Puertos en Uso

| Componente | Puerto | Contexto | Nota |
|---|---|---|---|
| API (.NET) | 5093 | Desarrollo local | Default, configurable en launchSettings.json. Usado en start-full-stack |
| Frontend (Python HTTP) | 8000 | Desarrollo local | Servido por `serve-frontend` task. Usado en start-full-stack |
| IIS Local | 8080 | Validacion local pre-produccion | **NO se usa en start-full-stack**. Solo para validar en entorno IIS sin publicar |
| SQL Server | 1433 | Produccion/Infraestructura | Puerto estandar, puede variar |

## Mapa rapido de arquitectura

- Frontend: index.html, dashboard.html, app.js, style.css.
- API: backend/src/IntegradorMarcas.Api.
- Application: backend/src/IntegradorMarcas.Application.
- Domain: backend/src/IntegradorMarcas.Domain.
- Infrastructure: backend/src/IntegradorMarcas.Infrastructure.
- Tests: backend/tests/IntegradorMarcas.Tests.
- SQL docs/scripts: docs/db.

## Prerrequisitos

- Windows 10/11.
- .NET SDK 8.
- SQL Server (Express o Developer).
- Python 3 (opcional, para servir frontend por tarea).
- VS Code con extension REST Client (humao.rest-client) para archivos .http.

Verificacion rapida:

~~~powershell
dotnet --version
python --version
~~~

## Checklist de primer arranque (Flujo Recomendado: start-full-stack)

1. Abrir esta carpeta en VS Code.
2. Terminal > Run Task > start-full-stack.
3. Esperar mensaje en consola: **Application started. Press Ctrl+C to shut down.**
4. Verificar API health: http://localhost:5093/health.
5. Abrir Swagger: http://localhost:5093/swagger.
6. Opcional UI: verificar http://localhost:8000/index.html (ya levantado por start-full-stack).
7. Opcional API quick test: abrir archivo .http y ejecutar GET /health.

## Flujo Completo de Levantamiento

### Opcion A: Usar Tasks (Recomendado)

1. Terminal > Run Task > start-full-stack.
2. Esperar: **Application started. Press Ctrl+C to shut down.**
3. API: http://localhost:5093/health
4. Frontend: http://localhost:8000/index.html

Esta opcion ejecuta restore, build-api, run-api y serve-frontend en secuencia.

### Opcion B: Fallback por Terminal Manual

Si Tasks no funcionan, abre 3 terminales separadas:

~~~powershell
# Terminal 1: Restaurar y compilar
dotnet restore backend/
dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug

# Terminal 2: Ejecutar API
# Espera: "Application started. Press Ctrl+C to shut down."
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --no-build

# Terminal 3: Servir frontend (opcional)
python -m http.server 8000 --directory .
~~~

## Detener Todo

### Metodo 1: Desde VS Code (Si usaste start-full-stack)

1. En la terminal activa: presionar **Ctrl+C** una o mas veces.
2. Esperar cierre de ambas terminales (API y Frontend).

### Metodo 2: Task + Comando Manual

1. Terminal > Run Task > stop-api-on-5093 (libera puerto 5093 para API).
2. Para detener Frontend en puerto 8000 (no hay task dedicada), ejecuta en PowerShell:

~~~powershell
Get-NetTCPConnection -LocalPort 8000 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
~~~

### Metodo 3: Comando Manual para Liberar Todos los Puertos

Si necesitas liberar puertos manualmente en una sola operacion:

~~~powershell
# Liberar puerto 5093 (API)
Get-NetTCPConnection -LocalPort 5093 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }

# Liberar puerto 8000 (Frontend)
Get-NetTCPConnection -LocalPort 8000 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
~~~

### Opcion C: Debug Completo (IDE)

1. Run and Debug > Full Stack Debug (API + Frontend).
2. Presiona F5.
3. VS Code pausara en breakpoints.

## Usar Swagger rapido

1. Levantar la API.
2. Abrir http://localhost:5093/swagger.
3. Ejecutar GET /health.
4. Para endpoints de negocio, enviar headers:
   - X-User-Id: 6
   - X-User-Role: ROL_RRHH
5. Si responde 401, revisar ambos headers.

Nota: Swagger esta activo en Development con Swagger:Enabled=true.

## Usar REST Client (.http)

Archivo listo para usar:

- backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.http

Flujo minimo:

1. Instalar extension humao.rest-client (si falta).
2. Levantar la API.
3. Abrir el archivo .http.
4. Seleccionar entorno local en REST Client.
5. Ejecutar GET /health.
6. Ejecutar GET /api/rrhh/justificaciones.

Variables ya configuradas en .vscode/settings.json:

- apiBaseUrl: http://localhost:5093
- userId: 6
- userRole: ROL_RRHH

## Setup minimo de base de datos

1. Revisar conexion en backend/src/IntegradorMarcas.Api/appsettings.Development.json:
   - ConnectionStrings:IntegraCnp (default: localhost\\SQLEXPRESS, DB INTEGRA_CNP).
2. Ejecutar scripts en este orden:
   - docs/db/001_integra_marcas_base_inicial.sql
   - docs/db/002_integra_marcas_objetos.sql
   - docs/db/004_seed_esquema_correcto.sql
   - docs/db/005_fix_errorapi_schema.sql
   - docs/db/006_fix_mojibake_historial_textos.sql (si ves textos como OmisiÃ³n)
3. Si no tienes bases externas WIZDOM/SIFCNP, la parte de vistas del script 002 puede fallar en local.

## Pre-check de BD para pase a Produccion (BD ya creada)

Contexto: la base de datos de Produccion ya existe en un servidor dedicado de BD (distinto al servidor IIS de la app).

Antes de generar Release y publicar, validar este checklist corto:

### 1) Datos de conexion confirmados con DBA

Tener confirmados estos valores:

- Servidor/instancia SQL de Produccion.
- Nombre de la base (por ejemplo INTEGRA_CNP).
- Tipo de autenticacion (SQL o integrada).
- Usuario tecnico de la app y su password (si aplica).
- Puerto SQL habilitado (normalmente 1433 o el definido por infraestructura).

### 2) Permisos del usuario tecnico

Validar que el usuario de la app tenga permisos reales sobre los objetos que usa la API.

Minimo esperado:

- SELECT/INSERT/UPDATE sobre tablas del esquema funcional.
- Acceso al esquema Auditoria y tabla de errores (ErrorApi).
- Permiso de ejecucion sobre SP o funciones requeridas (si existen).

### 3) Conectividad desde el servidor de app (IIS) hacia el servidor SQL

Desde el servidor donde corre IIS, validar red hacia SQL:

~~~powershell
Test-NetConnection <SQL_HOST_O_IP> -Port <SQL_PORT>
~~~

Si falla, no continuar con el pase: revisar firewall/rutas con infraestructura.

### 4) Configuracion Production en la app

Revisar que la cadena de conexion de Produccion este correcta en uno de estos lugares:

- backend/src/IntegradorMarcas.Api/appsettings.Production.json
- Variable de entorno en IIS (recomendado): ConnectionStrings__IntegraCnp

Recomendacion: en Produccion usar variable de entorno en IIS para no dejar credenciales en archivo.

### 5) Validacion final antes del Release

1. Confirmar ASPNETCORE_ENVIRONMENT=Production en IIS.
2. Confirmar que la app apunta al SQL de Produccion (no localhost/SQLEXPRESS).
3. Hacer smoke test de API:
   - GET /health
   - Un endpoint de negocio con headers (X-User-Id y X-User-Role)
4. Revisar logs iniciales de arranque y errores de SQL en Event Viewer.

Si este checklist esta OK, recien ahi generar Release y hacer pase a Produccion.

Para el pase de aplicacion en IIS productivo (sin VS Code ni SDK en servidor), seguir: **Despliegue a Produccion en IIS (Windows Server 2022, sin herramientas de desarrollo)**.

## Despliegue a Produccion en IIS (Windows Server 2022, sin herramientas de desarrollo)

Supuesto explicito: el servidor IIS de Produccion **no** tiene VS Code ni .NET SDK.

> [!WARNING]
> En el servidor IIS de Produccion NO se ejecutan comandos dotnet de SDK (restore/build/publish/run).
> Todo build/publish se realiza en una maquina de CI o build separada.
> El servidor productivo solo aloja artefactos Release publicados.

### 1) Acciones en maquina de Build/Publish

Precondiciones:

1. Windows con .NET SDK 8 instalado.
2. Codigo fuente actualizado.

Comandos (desde la raiz del repo):

~~~powershell
dotnet restore backend/
dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj -c Release
dotnet publish backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj -c Release -o .\artifacts\IntegradorMarcas.Api
~~~

Resultado esperado:

1. Carpeta .\artifacts\IntegradorMarcas.Api con IntegradorMarcas.Api.dll, web.config y dependencias.
2. Esa carpeta es el unico artefacto que se copia al servidor IIS.

### 2) Acciones en servidor IIS de Produccion

Preparacion del servidor (una sola vez):

1. Habilitar rol IIS en Windows Server 2022.
2. Instalar ASP.NET Core Hosting Bundle x64 compatible con .NET 8.
3. Ejecutar iisreset despues de instalar el Hosting Bundle.

Despliegue:

1. Crear carpeta del sitio, por ejemplo C:\inetpub\IntegradorMarcas.Api.
2. Copiar el contenido publicado desde .\artifacts\IntegradorMarcas.Api (sin recompilar en servidor).

Configuracion IIS:

1. Crear Application Pool:
   - Nombre: IntegradorMarcasApiPool
   - .NET CLR: No Managed Code
   - Pipeline: Integrated
2. Crear sitio IIS:
   - Nombre: IntegradorMarcas.Api
   - Physical path: C:\inetpub\IntegradorMarcas.Api
   - Binding: http o https segun infraestructura
3. Asignar el sitio al pool IntegradorMarcasApiPool.

Variables de entorno (sitio o app pool):

1. ASPNETCORE_ENVIRONMENT=Production
2. ConnectionStrings__IntegraCnp=<cadena SQL produccion>

Permisos:

1. Dar lectura/ejecucion en la carpeta del sitio al identity del pool: IIS AppPool\IntegradorMarcasApiPool.

Validacion minima post-despliegue:

1. GET /health responde 200.
2. Un endpoint de negocio responde con headers requeridos (X-User-Id, X-User-Role).

Do/Don't rapido:

- Do: publicar en Release en maquina de build y copiar artefactos ya publicados.
- Do: definir ASPNETCORE_ENVIRONMENT=Production y cadena de conexion por variable de entorno.
- Don't: instalar SDK o usar dotnet restore/build/publish/run en el servidor IIS.
- Don't: editar codigo en el servidor productivo.

Logs y diagnostico:

1. Event Viewer > Windows Logs > Application.
2. stdout de ASP.NET Core si web.config tiene habilitado stdoutLogEnabled.

## Validacion local de IIS (no productivo)

Objetivo: publicar solo la API en IIS local para validar un entorno tipo produccion.

### 1) Requisitos del servidor local

1. IIS habilitado en Windows.
2. ASP.NET Core Hosting Bundle instalado (misma version mayor de .NET que la app: .NET 8).
3. Puerto libre para el sitio (ejemplo: 8080).

### 2) Publicar la API

Desde la raiz del proyecto:

~~~powershell
dotnet publish backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj -c Release -o C:\inetpub\IntegradorMarcas.Api
~~~

Esto genera los archivos listos para IIS en C:\inetpub\IntegradorMarcas.Api.

### 3) Configurar IIS

1. Abrir Administrador de IIS.
2. Crear un Application Pool nuevo:
   - Name: IntegradorMarcasApiPool
   - .NET CLR version: No Managed Code
   - Managed pipeline mode: Integrated
3. Crear un sitio nuevo:
   - Site name: IntegradorMarcas.Api
   - Physical path: C:\inetpub\IntegradorMarcas.Api
   - Binding: http, puerto 8080 (o el que uses)
4. Asignar el sitio al pool IntegradorMarcasApiPool.

### 4) Variables de entorno en IIS (modo Production)

En el sitio de IIS, agregar variable de entorno:

- ASPNETCORE_ENVIRONMENT = Production

Nota: la app ya tiene appsettings.Production.json.

### 5) Permisos de carpeta

Dar permisos de lectura/ejecucion sobre C:\inetpub\IntegradorMarcas.Api al usuario del Application Pool (IIS AppPool\IntegradorMarcasApiPool).

### 6) Verificacion rapida

1. Reiniciar el sitio en IIS.
2. Abrir:
   - http://localhost:8080/health
3. Si responde OK, el despliegue quedo funcionando.

### 7) Si no levanta en IIS

1. Revisar Event Viewer (Application).
2. Revisar stdout logs de ASP.NET Core si estan habilitados en web.config.
3. Confirmar Hosting Bundle instalado.
4. Confirmar que el Application Pool usa No Managed Code.

## Top 5 troubleshooting

1. API no levanta
   - Correr restore y build-api; confirmar SDK .NET 8.

2. 401 en endpoints
   - Enviar X-User-Id y X-User-Role validos.

3. Swagger no abre
   - Validar primero http://localhost:5093/health y luego /swagger.

4. REST Client no resuelve variables
   - Seleccionar entorno local y revisar .vscode/settings.json.

5. Frontend no conecta a API
   - Confirmar API en :5093 y frontend en :8000; revisar URL base en app.js.
