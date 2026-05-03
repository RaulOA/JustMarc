# Especificación: README para Producción IIS sin Herramientas de Desarrollo

Fecha: 2026-05-03
Archivo objetivo: README.md
Nuevo enfoque: despliegue en Windows Server 2022 (IIS) con servidor de producción sin VS Code ni .NET SDK.

## 1) Objetivo

Adaptar la sección de despliegue de README.md para un escenario real de Producción:
- Build y publish se ejecutan fuera del servidor productivo.
- El servidor IIS solo recibe artefactos Release ya publicados.
- En Producción no se usan comandos `dotnet restore/build/run/publish`.

## 2) Hallazgos del README actual

1. La sección de IIS local indica `dotnet publish` en la misma máquina donde corre IIS.
2. El documento está orientado a onboarding/desarrollo (VS Code, tasks, Swagger dev) y no separa claramente responsabilidades por entorno.
3. Falta una guía explícita de "qué se hace en máquina de build" vs "qué se hace en servidor IIS" para pase productivo.

## 3) Cambio propuesto en README

Agregar/reemplazar una sección titulada:
- `Despliegue a Producción en IIS (Windows Server 2022, sin herramientas de desarrollo)`

La sección debe dividirse en dos bloques obligatorios:
1. `Acciones en máquina de Build/Publish`
2. `Acciones en servidor IIS de Producción`

## 4) Contenido exacto a documentar

## 4.1 Acciones en máquina de Build/Publish (con SDK)

Precondiciones:
- Windows con .NET SDK 8 instalado.
- Código fuente actualizado.

Comandos exactos (desde raíz del repo):

```powershell
dotnet restore backend/
dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj -c Release
dotnet publish backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj -c Release -o .\artifacts\IntegradorMarcas.Api
```

Resultado esperado:
- Carpeta de salida `./artifacts/IntegradorMarcas.Api` con `web.config`, `IntegradorMarcas.Api.dll` y dependencias.
- Este directorio es el único que se copia al servidor IIS.

Empaquetado/transferencia sugerida:
- Comprimir `./artifacts/IntegradorMarcas.Api` (zip) y transferir por canal corporativo.

## 4.2 Acciones en servidor IIS de Producción (sin SDK)

Regla clave:
- No instalar VS Code ni .NET SDK.
- Solo runtime requerido para IIS: ASP.NET Core Hosting Bundle (x64) compatible con .NET 8.

Preparación del servidor (una sola vez):
1. Habilitar rol IIS en Windows Server 2022.
2. Instalar ASP.NET Core Hosting Bundle .NET 8.
3. Reiniciar IIS (`iisreset`) después de instalar el Hosting Bundle.

Estructura de despliegue:
1. Crear carpeta de sitio, por ejemplo `C:\inetpub\IntegradorMarcas.Api`.
2. Copiar allí el contenido publicado desde la máquina de build (sin recompilar en servidor).

Configuración IIS:
1. Crear Application Pool:
   - Nombre: `IntegradorMarcasApiPool`
   - `.NET CLR`: `No Managed Code`
   - Pipeline: `Integrated`
2. Crear sitio IIS:
   - Nombre: `IntegradorMarcas.Api`
   - Physical path: `C:\inetpub\IntegradorMarcas.Api`
   - Binding: `http` o `https` según infraestructura (puerto definido por Ops).
3. Asignar el sitio al pool `IntegradorMarcasApiPool`.

Variables de entorno en IIS (sitio o app pool):
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__IntegraCnp=<cadena SQL producción>`

Permisos:
- Otorgar lectura/ejecución sobre carpeta del sitio al identity del app pool (`IIS AppPool\IntegradorMarcasApiPool`).

Validación post-despliegue:
1. `GET /health` responde 200.
2. Endpoint de negocio responde con headers requeridos (`X-User-Id`, `X-User-Role`).
3. Revisar Event Viewer ante fallas de arranque/conexión SQL.

## 5) Texto de advertencia obligatoria para README

Incluir un bloque de advertencia visible:

- `En el servidor IIS de Producción NO se ejecutan comandos dotnet de SDK (restore/build/publish/run).`
- `Todo build/publish se realiza en una máquina de CI o build separada.`
- `El servidor productivo solo aloja artefactos Release publicados.`

## 6) Ajustes de contenido existentes

1. Mantener la sección actual de "Producción local con IIS" como referencia de laboratorio, renombrándola a `Validación local de IIS (no productivo)`.
2. Evitar mezclar pasos de VS Code/REST Client con despliegue productivo.
3. Enlazar desde el checklist de pre-check de BD hacia la nueva sección de despliegue productivo en IIS.

## 7) Criterios de aceptación de documentación

1. README separa claramente acciones de Build/Publish vs acciones de IIS Productivo.
2. El flujo productivo no exige VS Code ni .NET SDK en servidor.
3. Los comandos `dotnet` aparecen solo en el bloque de Build/Publish.
4. Se documenta explícitamente que el despliegue es Release.
5. Se incluyen variables de entorno productivas y validación mínima (`/health`).
