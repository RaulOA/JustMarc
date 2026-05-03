# Especificación de Desarrollo y Ejecución Local

Fecha: 2026-05-02
Workspace: Justificacion de Marca

## 1) Alcance y hallazgos clave

Esta guía describe cómo desarrollar y ejecutar el sistema en este workspace:
- Backend .NET 8 (API + capas Application/Domain/Infrastructure + tests).
- Base de datos SQL Server para INTEGRA_CNP.
- Frontend estático HTML/CSS/JS (sin bundler ni package manager).

Hallazgos importantes:
- La solución backend usa `backend/IntegradorMarcas.slnx` (no `.sln`).
- El API corre por defecto en `http://localhost:5093` en Development.
- El frontend consume la API en `http://localhost:5093` por defecto.
- Hay dos líneas de scripts SQL (modelo nuevo por esquemas y artefactos legacy `dbo`) que pueden causar inconsistencias si se mezclan sin cuidado.
- Actualmente el proyecto de pruebas no compila por un error en `ErrorLogIntegrationTests.cs` (`cleanupSql` no definido).

## 2) Prerrequisitos

## 2.1 Software

- Windows 10/11.
- .NET SDK 8.x (`dotnet --version`).
- SQL Server (Express/Developer) con acceso local.
- Opcional para servir frontend por HTTP:
  - Python 3.x, o
  - Node.js (para usar `npx http-server` / similar).

## 2.2 Conectividad base de datos

Conexión usada por defecto en Development (`backend/src/IntegradorMarcas.Api/appsettings.Development.json`):

```json
"ConnectionStrings": {
  "IntegraCnp": "Server=localhost\\SQLEXPRESS;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;"
}
```

Si su instancia no es `localhost\\SQLEXPRESS`, debe ajustar esa cadena o usar variable de entorno equivalente.

## 3) Setup de Base de Datos (orden recomendado)

## 3.1 Orden recomendado para entorno local de desarrollo

Ejecutar en este orden:

1. `docs/db/001_integra_marcas_base_inicial.sql`
2. `docs/db/002_integra_marcas_objetos.sql`
3. `docs/db/004_seed_esquema_correcto.sql`
4. `docs/db/005_fix_errorapi_schema.sql`

Notas:
- `001` crea BD, esquemas y tablas base (`Configuracion`, `RecursosHumanos`, `Operacion`, `Auditoria`, `Integracion`) con seeds iniciales.
- `002` crea función de aprobadores y vistas de integración externa (WIZDOM/SIFCNP).
- `004` inserta datos demo alineados al esquema moderno (no `dbo` legacy).
- `005` alinea `Auditoria.ErrorApi` con el contrato del repositorio C# (columnas como `HttpMethod`, `StatusCode`, `RolUsuario`, etc.).

## 3.2 Scripts que requieren cuidado

- `docs/db/003_integra_marcas_seed_demo.sql`:
  - Usa tablas `dbo.*` legacy (`dbo.Usuarios`, `dbo.Justificaciones_Encabezado`, etc.).
  - No es compatible con el modelo principal actual en `RecursosHumanos.*` y `Operacion.*`.
  - Recomendación: no ejecutarlo en el flujo principal actual.

- `docs/db/fix_fn_aprobadores.sql`:
  - Hace `ALTER FUNCTION dbo.fn_AprobadoresVigentesPorSolicitante`.
  - Úselo solo si ya existe esa función en `dbo` y necesita corregirla.

## 3.3 Dependencias externas de integración

`002_integra_marcas_objetos.sql` crea vistas hacia:
- `[WIZDOM].[dbo].[empleado]`
- `[WIZDOM].[dbo].[organigrama]`
- `[SIFCNP].[dbo].[RH_JUSTIFICACIONES_ENC]`
- `[SIFCNP].[dbo].[RH_JUSTIFICACIONES_DET]`

Si esas bases no existen en su SQL Server local, la creación de vistas puede fallar. En entorno de desarrollo mínimo:
- Opción A: disponer esas BD/fuentes.
- Opción B: omitir temporalmente la parte de vistas de integración externa y enfocarse en flujo local core.

## 4) Comandos de Backend

Desde la raíz del workspace:

## 4.1 Restaurar y compilar

```powershell
dotnet restore backend/IntegradorMarcas.slnx
dotnet build backend/IntegradorMarcas.slnx
```

Resultado observado actualmente:
- API, Domain, Application, Infrastructure compilan.
- El proyecto de tests falla (detalle en Troubleshooting).

## 4.2 Ejecutar API

```powershell
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj
```

URLs de launch profile (Development):
- HTTP: `http://localhost:5093`
- HTTPS: `https://localhost:7129`

Endpoints útiles:
- `GET /health`
- `GET /swagger` (si `Swagger:Enabled = true`, en Development viene habilitado).

## 4.3 Identidad por headers (requerida en API)

El backend espera headers:
- `X-User-Id`
- `X-User-Role`

Sin esos headers (o inválidos), devuelve error 401 desde `HeaderUserContext`.

Roles usados por negocio:
- `ROL_FUNC`
- `ROL_JEFE`
- `ROL_RRHH`
- `ROL_ADMIN`

## 5) Comandos de Pruebas

## 5.1 Ejecutar todas las pruebas

```powershell
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj
```

Estado actual observado:
- Falla de compilación en `backend/tests/IntegradorMarcas.Tests/ErrorLogIntegrationTests.cs` por variable `cleanupSql` no definida.

## 5.2 Ejecutar solo integración (cuando compile)

```powershell
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "Category=Integration"
```

## 5.3 Cobertura

```powershell
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --collect:"XPlat Code Coverage"
```

## 6) Opciones para ejecutar Frontend

El frontend es estático (archivos raíz del workspace):
- `index.html`
- `dashboard.html`
- `app.js`
- `style.css`

## 6.1 Opción rápida (sin servidor)

- Abrir `index.html` directamente en navegador.
- Funciona para navegación básica porque son archivos estáticos.

## 6.2 Opción recomendada (servidor HTTP local)

Con Python:

```powershell
python -m http.server 5500
```

Abrir:
- `http://localhost:5500/index.html`

Con Node.js (si está instalado):

```powershell
npx http-server -p 5500
```

## 6.3 Usuarios de prueba para UI

La UI simula sesión y rol por nombre de usuario. Ejemplos:
- `funcionario.ana`  -> `ROL_FUNC`
- `jefe.maria` -> `ROL_JEFE`
- `rrhh.carlos` -> `ROL_RRHH`

Contraseña: la validación es mínima de longitud (MVP), no contra backend real.

## 7) Configuración de Entorno

## 7.1 API (`appsettings.*`)

- `appsettings.Development.json`:
  - `ConnectionStrings:IntegraCnp` definida por defecto.
  - `Swagger:Enabled = true`.
  - `Security:UseMockIdentity = true`.

- `appsettings.json` y `appsettings.Production.json`:
  - No traen `ConnectionStrings:IntegraCnp` por defecto.
  - En no-Development, `Program.cs` obliga esa conexión al arranque (si falta, el API no inicia).

## 7.2 Frontend -> API base URL

En `app.js`:
- Default: `http://localhost:5093`.
- También lee `window.SJM_API_BASE_URL` o `sessionStorage['sjm_api_base_url']` para override.

Si cambia el puerto de API, ajuste una de esas opciones para evitar errores de conexión.

## 8) Troubleshooting

## 8.1 Falla de build en tests (actual)

Síntoma:
- `CS0103: El nombre 'cleanupSql' no existe en el contexto actual`.

Causa:
- En `ErrorLogIntegrationTests.cs` la constante `cleanupSql` está comentada y luego se usa.

Impacto:
- `dotnet build` de la solución completa falla por el proyecto de tests.
- `dotnet test` también falla en compilación.

## 8.2 Error 401 en API

Síntoma:
- Respuestas 401 en endpoints.

Verificar:
- Enviar `X-User-Id` entero positivo.
- Enviar `X-User-Role` con rol válido.

## 8.3 Error 403 en paneles/acciones

Síntoma:
- Endpoint responde 403 aunque hay sesión frontend.

Causa común:
- Rol del usuario no corresponde al endpoint.

Ejemplos:
- Solo `ROL_JEFE` puede resolver (`PATCH /api/jefatura/justificaciones/{id}/resolver`).
- Solo `ROL_RRHH` puede listar global (`GET /api/rrhh/justificaciones`).

## 8.4 Error SQL por objetos faltantes de función/tabla legacy

Síntoma típico:
- Errores relacionados con `dbo.fn_AprobadoresVigentesPorSolicitante` o `dbo.Estructuras_Organizacionales`.

Contexto:
- El SQL de infraestructura usa referencias `dbo.*` en algunas consultas.
- Los scripts base modernos crean mayormente objetos en esquemas `Operacion`/`RecursosHumanos`.

Acción sugerida:
- Validar existencia de objetos esperados por consultas de backend.
- Evitar mezclar scripts legacy `dbo` con scripts modernos sin estrategia de compatibilidad.

## 8.5 Error de conexión frontend -> backend

Síntoma:
- Mensaje en UI: no fue posible conectar con la API.

Checklist:
- API ejecutándose en `http://localhost:5093`.
- URL base del frontend alineada al puerto real.
- CORS permitido (en `Program.cs` se permite origen abierto para entorno local).

## 8.6 Error de logging en `Auditoria.ErrorApi`

Si no se ejecuta `005_fix_errorapi_schema.sql`, pueden fallar inserts de log por columnas desalineadas entre tabla y código C#.

## 9) Flujo recomendado de arranque rápido (local)

1. Ejecutar scripts SQL recomendados (`001` -> `002` -> `004` -> `005`).
2. Compilar backend (`dotnet build backend/IntegradorMarcas.slnx`).
3. Levantar API (`dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj`).
4. Levantar frontend estático (`python -m http.server 5500`) y abrir `index.html`.
5. Iniciar sesión con usuario demo por rol y validar llamadas al API.

## 10) Referencias de archivos analizados

- `backend/IntegradorMarcas.slnx`
- `backend/src/IntegradorMarcas.Api/Program.cs`
- `backend/src/IntegradorMarcas.Api/appsettings.Development.json`
- `backend/src/IntegradorMarcas.Api/appsettings.json`
- `backend/src/IntegradorMarcas.Api/appsettings.Production.json`
- `backend/src/IntegradorMarcas.Api/Properties/launchSettings.json`
- `backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/ErrorLogRepository.cs`
- `backend/tests/IntegradorMarcas.Tests/ErrorLogIntegrationTests.cs`
- `docs/db/001_integra_marcas_base_inicial.sql`
- `docs/db/002_integra_marcas_objetos.sql`
- `docs/db/003_integra_marcas_seed_demo.sql`
- `docs/db/004_seed_esquema_correcto.sql`
- `docs/db/005_fix_errorapi_schema.sql`
- `docs/db/fix_fn_aprobadores.sql`
- `docs/PRP_Justificacion_Marcas.md`
- `index.html`
- `dashboard.html`
- `app.js`
- `style.css`
