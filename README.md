# Justificación de Marca

Guía práctica para levantar, usar y probar el proyecto en pocos minutos.

## ¿Qué es esta app?

Aplicación para gestionar **justificaciones de marca** (las marcas de entrada/salida del personal). Cada quien según su rol:

- **Funcionario:** registra una justificación y consulta su historial.
- **Jefatura:** revisa, aprueba o rechaza las solicitudes de su gente.
- **RRHH:** consulta todas las justificaciones y descarga reportes.
- **Administrador:** gestiona dependencias, jerarquías y delegaciones de aprobación.

Tiene tres partes:

- **Frontend** (lo que se ve): páginas estáticas de login y panel por rol.
- **API** (la lógica): servicio en .NET 8 que atiende las peticiones.
- **Base de datos**: SQL Server, con los scripts en `docs/db`.

## Lo que necesitas instalado

- Windows 10/11.
- **.NET SDK 8** (la app está hecha en .NET 8).
- **SQL Server** (Express o Developer sirven).
- **Python 3** (opcional, solo para servir el frontend con una tarea).
- **VS Code** con la extensión **REST Client** (`humao.rest-client`) si quieres probar la API desde archivos `.http`.

Para confirmar que tienes lo básico:

~~~powershell
dotnet --version
python --version
~~~

## Arranque rápido (recomendado)

1. Abre esta carpeta en VS Code.
2. Menú **Terminal > Run Task > `start-full-stack`**. Esto compila, levanta la API y sirve el frontend, en orden.
3. Espera en la consola el mensaje: **Application started. Press Ctrl+C to shut down.**
4. Listo. Abre:
   - App: http://localhost:8000/index.html
   - API (estado): http://localhost:5093/health
   - Documentación de la API (Swagger): http://localhost:5093/swagger

¿No te funcionan las tareas de VS Code? Usa el arranque manual más abajo.

### Arranque manual (alternativa)

Abre 3 terminales:

~~~powershell
# 1) Compilar
dotnet build backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --configuration Debug

# 2) Levantar la API (espera: "Application started. Press Ctrl+C to shut down.")
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj --no-build

# 3) Servir el frontend
python -m http.server 8000 --directory .
~~~

## Cómo entrar a la app

Es un **ambiente de prueba**: no hay contraseñas reales. Escribe cualquier contraseña de **4 caracteres o más**; el rol se decide por el nombre de usuario.

Usuarios de ejemplo (los mismos que aparecen en la pantalla de login):

| Rol | Usuarios de ejemplo |
|---|---|
| Funcionario | `funcionario.ana`, `funcionario.luis` |
| Jefatura | `jefe.maria`, `jefe.ricardo` |
| RRHH | `rrhh.carlos`, `rrhh.sandra` |
| Administrador | `admin.sofia`, `admin.demo` |

Cada usuario ve solo las pestañas que le corresponden. El Administrador entra a su propio panel de gestión.

## Detener todo

1. En la terminal activa, presiona **Ctrl + C** (una o varias veces) hasta que se cierre.
2. Si algún puerto quedó ocupado, libéralo:

~~~powershell
# Liberar la API (puerto 5093) — también disponible como Run Task > stop-api-on-5093
Get-NetTCPConnection -LocalPort 5093 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }

# Liberar el frontend (puerto 8000)
Get-NetTCPConnection -LocalPort 8000 -State Listen -ErrorAction SilentlyContinue | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
~~~

## Puertos en uso

| Componente | Puerto | Para qué |
|---|---|---|
| API (.NET) | 5093 | Desarrollo local. Es el que usa el frontend por defecto. |
| Frontend | 8000 | Desarrollo local (lo levanta la tarea `serve-frontend`). |
| IIS local | 8080 | Solo para validar en un entorno tipo producción. **No** forma parte del arranque normal. |
| SQL Server | 1433 | Base de datos. Puede variar según la infraestructura. |

## Preparar la base de datos (primera vez)

1. Configura la **cadena de conexión por variable de entorno** (no se guarda en archivos del proyecto). En PowerShell, con tu contraseña real:

   ~~~powershell
   [Environment]::SetEnvironmentVariable("ConnectionStrings__IntegraCnp", "Server=TU_SERVIDOR;Database=INTEGRA_CNP;User ID=TU_USUARIO;Password=TU_CONTRASEÑA;Encrypt=True;TrustServerCertificate=True;Application Name=IntegradorMarcas.Api", "User")
   ~~~

   Reinicia VS Code/terminal para que tome la variable. Detalle y justificación en [docs/seguridad/gestion_credenciales_conexion_bd.md](docs/seguridad/gestion_credenciales_conexion_bd.md).
2. Ejecuta los **3 scripts** de `docs/db` **en este orden** (con SSMS o `sqlcmd`, en UTF-8):

   | Orden | Script | Qué hace |
   |---|---|---|
   | 1 | `01_CrearBaseDatos.sql` | Crea la base `INTEGRA_CNP` y los 5 esquemas. |
   | 2 | `02_EstructuraCompleta.sql` | Toda la estructura: tablas, índices, función, SP y vistas. *(ver nota)* |
   | 3 | `03_DatosSemilla.sql` | Catálogos (obligatorio) + datos de ejemplo (dev) + remediación opcional. |

   En **producción** ejecuta de `03_DatosSemilla.sql` **solo la Sección A (catálogos)**; las
   secciones B/C/D crean datos de prueba y correcciones que no van a producción.

> **Nota:** algunas vistas y el SP del script `02` se conectan a bases externas (`WIZDOM` y `SIFCNP`). Si no las tienes en tu equipo local, esos objetos se **omiten automáticamente** (guardas `DB_ID`/`OBJECT_ID`) y el resto se crea igual. Esas bases externas son **solo de consulta**: la app nunca escribe en ellas. Los scripts anteriores quedaron archivados en `docs/db/_legacy/` (ver `docs/db/Observaciones_Consolidacion_SQL.md`).

## Probar la API

### Con Swagger (desde el navegador)

1. Levanta la API.
2. Abre http://localhost:5093/swagger.
3. Prueba `GET /health`: debe responder OK.
4. Para los endpoints de negocio, agrega estas dos cabeceras (identifican al usuario):
   - `X-User-Id: 6`
   - `X-User-Role: ROL_RRHH`
5. Si recibes **401**, casi siempre es porque falta alguna de las dos cabeceras.

### Con REST Client (archivos .http)

1. Instala la extensión `humao.rest-client`.
2. Abre `backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.http`.
3. Selecciona el entorno **local** (abajo a la derecha en VS Code).
4. Ejecuta `GET /health` y luego `GET /api/rrhh/justificaciones`.

Las cabeceras de usuario ya vienen configuradas en `.vscode/settings.json` (`X-User-Id: 6`, `X-User-Role: ROL_RRHH`).

## Pruebas automáticas

~~~powershell
# Todas las pruebas
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj

# Solo las rápidas, sin la prueba que requiere base de datos real
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "Category!=Integration"
~~~

## Si algo falla (problemas comunes)

1. **La API no levanta** → confirma que tienes .NET 8 y vuelve a compilar (`build-api`).
2. **Error 401 en la API** → faltan las cabeceras `X-User-Id` y `X-User-Role`.
3. **Swagger no abre** → primero revisa http://localhost:5093/health; si responde, abre `/swagger`.
4. **REST Client no reemplaza las variables** → asegúrate de tener seleccionado el entorno **local**.
5. **El frontend no conecta con la API** → confirma que la API esté en `:5093` y el frontend en `:8000`.

## Llevar a producción (IIS)

La idea clave: **se compila en una máquina de desarrollo o de build, y al servidor solo se copia el resultado ya compilado.** El servidor de producción no necesita herramientas de desarrollo.

1. **En la máquina de build** (con .NET SDK 8), genera el paquete:

   ~~~powershell
   dotnet publish backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj -c Release -o .\artifacts\IntegradorMarcas.Api
   ~~~

2. **En el servidor IIS** (una sola vez):
   - Instala el **ASP.NET Core Hosting Bundle** para .NET 8 y ejecuta `iisreset`.
   - Crea un Application Pool con **No Managed Code**.
   - Crea el sitio apuntando a la carpeta donde copiarás los archivos publicados.

3. **Configura el entorno** en el sitio o el pool:
   - `ASPNETCORE_ENVIRONMENT = Production`
   - `ConnectionStrings__IntegraCnp = <cadena de conexión a SQL de producción>`

   > Recomendado: usar variable de entorno para la cadena de conexión, así no quedan credenciales en archivos.

4. **Copia** el contenido de `artifacts\IntegradorMarcas.Api` a la carpeta del sitio y reinícialo.

5. **Verifica**: `GET /health` debe responder, y un endpoint de negocio debe funcionar enviando las cabeceras `X-User-Id` y `X-User-Role`.

**Antes del pase**, confirma con quien administra la base de datos: servidor y nombre de la base, tipo de autenticación, usuario/permisos de la app y conectividad de red hacia SQL (`Test-NetConnection <host_sql> -Port <puerto>`).

Para validar todo esto en local sin tocar producción, puedes publicar la API en un IIS local en el puerto **8080** siguiendo los mismos pasos.

## Estructura del proyecto

~~~
.
├── index.html, dashboard.html, app.js, style.css   # Frontend (lo que ve el usuario)
├── backend/
│   ├── src/IntegradorMarcas.Api               # La API (puntos de entrada)
│   ├── src/IntegradorMarcas.Application        # Reglas de negocio
│   ├── src/IntegradorMarcas.Domain             # Conceptos del dominio
│   ├── src/IntegradorMarcas.Infrastructure     # Acceso a base de datos
│   └── tests/IntegradorMarcas.Tests            # Pruebas automáticas
├── docs/
│   ├── db/                                      # Scripts de base de datos
│   └── specs/                                   # Especificaciones de cambios
└── CLAUDE.md                                    # Guía técnica para asistencia con IA
~~~

## Trabajo con Claude Code

Este proyecto se asiste con **Claude Code**. La guía técnica (arquitectura, comandos, convenciones) vive en `CLAUDE.md`.

- Para cambios pequeños, basta con pedir el cambio directamente.
- Para funcionalidades grandes o poco claras, usa el comando **`/spec`**: primero genera una especificación en `docs/specs/` para revisarla y luego la implementa.
