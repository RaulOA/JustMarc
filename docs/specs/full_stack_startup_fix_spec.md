# Full Stack Startup Fix Spec

## Objetivo

Analizar problemas de arranque full-stack del workspace, enfocando backend, frontend estático, tareas de VS Code, puertos, configuración y puntos de integración API/frontend.

## Resumen Ejecutivo

- El backend compila y arranca correctamente en local sobre http://localhost:5093.
- El frontend estático también arranca correctamente sobre http://localhost:8000 usando el task actual, siempre que Python esté disponible en PATH.
- Los bloqueos principales no están en la compilación ni en el boot base de procesos, sino en la orquestación de VS Code y en dependencias runtime posteriores al arranque.
- El mayor problema de debug es que un perfil intenta iniciar la API dos veces y además no garantiza que el frontend esté servido.
- El mayor problema de validación es que el task de tests falla por quoting en PowerShell aunque las pruebas realmente pasen.
- El mayor problema funcional después del arranque es la dependencia dura de SQL Server local con esquema y objetos específicos; sin eso, el health funciona, pero endpoints de negocio fallarán.

## Evidencia Verificada

### Backend

- `build-api` compila correctamente toda la solución relacionada con la API.
- `run-api` inicia correctamente y deja la API escuchando en http://localhost:5093.
- La verificación de puerto confirmó conectividad local satisfactoria en 5093.

### Frontend

- `serve-frontend` inicia correctamente y sirve contenido en el puerto 8000.
- En este entorno existe `python.exe` en PATH, por lo que el task actual sí puede levantar el servidor estático.

### Tests

- Las pruebas xUnit ejecutadas pasan: 2 de 2 correctas.
- Sin embargo, el task `test` termina con código 1 porque PowerShell interpreta mal `--logger=console;verbosity=detailed`.

## Hallazgos Priorizados

### 1. Perfil de debug que intenta levantar la API dos veces

Severidad: alta

Archivo afectado: `.vscode/launch.json`

El perfil `Frontend + API` es de tipo `coreclr` y lanza directamente `IntegradorMarcas.Api.dll`, pero además usa `preLaunchTask: build-and-run-api`. Ese task ya ejecuta `run-api`, por lo que el prelaunch arranca una instancia de la API y luego el depurador intenta arrancar otra sobre el mismo puerto 5093.

Impacto esperado:

- Colisión de puerto 5093.
- Comportamiento inconsistente al depurar.
- Sensación de que el perfil “a veces funciona” dependiendo de si la tarea previa quedó viva o no.

Dirección de corrección:

- Ese perfil no debe usar `run-api` como preLaunchTask si el propio launch ya ejecuta el DLL.
- Debe usar solo build/preparación, o convertirse en un verdadero compound que lance frontend y API sin duplicar el proceso backend.

### 2. Perfil de debug abre el frontend sin garantizar que exista servidor en 8000

Severidad: alta

Archivo afectado: `.vscode/launch.json`

El mismo perfil `Frontend + API` abre `http://localhost:8000/index.html?api=%s`, pero su preLaunchTask `build-and-run-api` no incluye `serve-frontend`.

Impacto esperado:

- Falla de apertura del frontend cuando 8000 no está sirviendo nada.
- Experiencia de debug rota en ambientes limpios.

Dirección de corrección:

- Reemplazar ese preLaunchTask por uno que sí prepare frontend, o eliminar ese perfil y centralizar el flujo en el perfil que ya usa `prepare-full-stack-debug`.

### 3. La URL de API que VS Code intenta pasar por querystring no es consumida por el frontend

Severidad: media-alta

Archivos afectados: `.vscode/launch.json`, `app.js`

Los perfiles usan `uriFormat` con `http://localhost:8000/index.html?api=%s`, pero el frontend no lee `location.search` ni el parámetro `api`. La resolución de base URL en `app.js` solo considera:

- `window.SJM_API_BASE_URL`
- `sessionStorage`
- el default `http://localhost:5093`

Impacto esperado:

- El mecanismo de override de URL de API desde VS Code es actualmente inerte.
- Si en el futuro la API cambia de puerto o se usa https, el frontend seguirá apuntando a 5093 salvo intervención manual.

Dirección de corrección:

- Implementar lectura del query param `api` en `app.js`, o dejar de inyectarlo en `launch.json` y usar un mecanismo explícito soportado por el frontend.

### 4. El task `test` reporta fallo falso por quoting incorrecto

Severidad: media

Archivo afectado: `.vscode/tasks.json`

El task usa:

`dotnet test ... --logger=console;verbosity=detailed`

En PowerShell, el `;` corta la instrucción, por lo que:

- las pruebas sí se ejecutan y pasan
- después PowerShell intenta ejecutar `verbosity=detailed` como comando separado
- el task finaliza con exit code 1

Impacto esperado:

- Falso negativo en CI manual desde VS Code.
- Diagnóstico engañoso durante troubleshooting.

Dirección de corrección:

- Citar el argumento completo del logger o expresarlo de forma que PowerShell no separe la sentencia.

### 5. Dependencia runtime dura de SQL Server local y esquema específico

Severidad: media-alta

Archivos afectados: `backend/src/IntegradorMarcas.Api/appsettings.Development.json`, `backend/src/IntegradorMarcas.Infrastructure/Data/SqlConnectionFactory.cs`, `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`, repositorios de infraestructura

La API arranca, pero los endpoints de negocio dependen de:

- SQL Server en `localhost\\SQLEXPRESS`
- base `INTEGRA_CNP`
- esquemas y tablas como `Operacion`, `Configuracion`, `RecursosHumanos`, `Auditoria`
- función `dbo.fn_AprobadoresVigentesPorSolicitante`

Impacto esperado:

- `/health` responde bien aun sin BD operativa.
- llamadas del dashboard a `/api/justificaciones`, `/api/jefatura/...` y `/api/rrhh/...` fallarán si la BD no existe o no tiene scripts aplicados.

Dirección de corrección:

- No es necesariamente un cambio de código inmediato; sí debe tratarse como prerrequisito de arranque funcional completo.
- Conviene documentar mejor la diferencia entre “API arriba” y “flujo de negocio operativo”.

### 6. Producción no puede arrancar sin cadena de conexión externa

Severidad: media

Archivos afectados: `backend/src/IntegradorMarcas.Api/Program.cs`, `backend/src/IntegradorMarcas.Api/appsettings.Production.json`

En no-Development, `Program.cs` exige `ConnectionStrings:IntegraCnp`. Sin embargo, `appsettings.Production.json` no la define.

Impacto esperado:

- En Production la API falla en arranque si no se inyecta por variable de entorno o configuración externa.

Dirección de corrección:

- Esto parece intencional y correcto desde seguridad/operación, pero debe estar documentado con claridad como requisito obligatorio de despliegue.

### 7. La autenticación del frontend es local/mock y no representa un login real de backend

Severidad: baja-media

Archivos afectados: `app.js`, `backend/src/IntegradorMarcas.Api/Controllers/SessionController.cs`

El login del frontend crea sesión local sin llamar a la API. Eso no bloquea startup, pero sí puede generar confusión al validar “integración completa”, porque el flujo de autenticación no comprueba backend ni headers hasta el primer consumo de endpoints de negocio.

Impacto esperado:

- Aparente inicio exitoso del frontend aunque la API esté caída.
- Los errores reales aparecen después, al cargar paneles o ejecutar acciones.

Dirección de corrección:

- Opcionalmente validar `/api/session/status` al entrar al dashboard o antes del primer render de datos.

## Archivos Que Probablemente Requieren Cambios

### Cambios prioritarios

- `.vscode/launch.json`
- `.vscode/tasks.json`
- `app.js`

### Cambios recomendados pero secundarios

- `README.md`
- `backend/src/IntegradorMarcas.Api/appsettings.Development.json`
- `backend/src/IntegradorMarcas.Api/Program.cs`

## Propuesta de Ajuste

### Ajuste 1: limpiar flujo de debug full-stack

Objetivo:

- evitar doble arranque de API
- garantizar frontend servido antes de abrir navegador

Opciones razonables:

- Convertir `Frontend + API` en un perfil que solo buildée y luego lance el DLL, mientras `serve-frontend` corre en un preLaunchTask apropiado.
- O eliminar `Frontend + API` y usar un compound real que combine `Debug API (.NET)` con `Frontend Browser`.

### Ajuste 2: alinear mecanismo de base URL

Objetivo:

- que el frontend use de verdad la URL que VS Code intenta pasar

Opciones razonables:

- Leer `?api=` en `app.js` y persistirlo en sesión.
- O dejar de pasar querystring y usar siempre 5093 como contrato fijo local.

### Ajuste 3: corregir tasks de validación

Objetivo:

- que `test` no falle falsamente

Acción:

- corregir quoting del argumento `--logger=console;verbosity=detailed`

### Ajuste 4: endurecer documentación de prerrequisitos funcionales

Objetivo:

- evitar confundir “arrancó el proceso” con “funciona la app completa”

Acción:

- documentar que el frontend depende de API en 5093 y que los endpoints de negocio dependen de SQL Server + scripts de `docs/db`.

## Comandos y Tasks Recomendados Para Validación

### Build y smoke test mínimo

- Task: `build-api`
- Task: `run-api`
- Task: `serve-frontend`
- Abrir: `http://localhost:8000/index.html`
- Verificar: `http://localhost:5093/health`

### Validación integrada desde VS Code

- Task: `stop-api-on-5093`
- Task: `prepare-full-stack-debug`
- Launch: `Debug API (.NET)`

### Validación de pruebas

- Task: `test` después de corregir el quoting
- Alternativa por terminal: `dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --verbosity normal --logger "console;verbosity=detailed"`

### Validación funcional con BD disponible

- Aplicar scripts de `docs/db`
- Navegar desde login a dashboard
- Probar endpoints/acciones de:
  - Funcionario
  - Jefatura
  - RRHH

## Resultado Esperado Tras Los Fixes

- Un único flujo confiable para levantar full-stack desde VS Code.
- Sin dobles procesos de API ni colisiones de puerto.
- Sin falsos fallos en el task de tests.
- Con mecanismo consistente para resolver la URL base de la API.
- Con distinción clara entre arranque técnico y operación funcional dependiente de base de datos.
