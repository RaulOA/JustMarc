# Justificacion de Marca

Guia practica para levantar el proyecto localmente (frontend + backend + base de datos) en Windows.

## 1) Resumen del proyecto

Aplicacion para gestionar justificaciones de marca con:
- Frontend estatico (HTML/CSS/JS) para login MVP, dashboard y flujo por rol.
- Backend .NET 8 (API REST) con arquitectura por capas.
- Base de datos SQL Server con scripts versionados en docs/db.

## 2) Arquitectura

### Frontend
- Archivos en la raiz del workspace:
  - index.html
  - dashboard.html
  - app.js
  - style.css
- No usa bundler ni package manager.
- Consume la API por defecto en http://localhost:5093.

### Backend
- Solucion: backend/IntegradorMarcas.slnx
- API: backend/src/IntegradorMarcas.Api
- Capas:
  - Application: reglas de negocio y casos de uso.
  - Domain: entidades y contratos de dominio.
  - Infrastructure: acceso a datos, repositorios y SQL.
- Tests: backend/tests/IntegradorMarcas.Tests

## 3) Prerrequisitos

Instalar:
- Windows 10/11
- .NET SDK 8.x
- SQL Server (Express o Developer)
- Opcional para servir frontend por HTTP:
  - Python 3.x, o
  - Node.js

Verificacion rapida:

~~~powershell
dotnet --version
python --version
node --version
~~~

## 4) Configuracion de base de datos

Conexion esperada en desarrollo (backend/src/IntegradorMarcas.Api/appsettings.Development.json):

~~~json
"ConnectionStrings": {
  "IntegraCnp": "Server=localhost\\SQLEXPRESS;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;"
}
~~~

Si tu instancia SQL no es localhost\\SQLEXPRESS, ajusta la cadena de conexion.

### Orden recomendado de scripts (desarrollo local)

Ejecutar en este orden:
1. docs/db/001_integra_marcas_base_inicial.sql
2. docs/db/002_integra_marcas_objetos.sql
3. docs/db/004_seed_esquema_correcto.sql
4. docs/db/005_fix_errorapi_schema.sql

Notas:
- No usar docs/db/003_integra_marcas_seed_demo.sql en el flujo principal actual (usa objetos legacy dbo que no coinciden con el esquema principal).
- docs/db/fix_fn_aprobadores.sql solo aplica si ya existe dbo.fn_AprobadoresVigentesPorSolicitante y necesitas corregirla.
- El script 002 crea vistas a WIZDOM/SIFCNP. Si no existen esas bases en local, esa parte puede fallar.

## 5) Backend: restaurar, compilar y ejecutar

Desde la raiz del workspace:

~~~powershell
dotnet restore backend/IntegradorMarcas.slnx
dotnet build backend/IntegradorMarcas.slnx
~~~

Ejecutar API:

~~~powershell
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj
~~~

URLs esperadas en Development:
- http://localhost:5093
- https://localhost:7129

Endpoints utiles:
- GET /health
- GET /swagger (si Swagger:Enabled=true)

## 6) Frontend: ejecutar

### Opcion A: abrir archivo directamente
Abrir index.html en el navegador.

### Opcion B (recomendada): servidor HTTP local
Con Python:

~~~powershell
python -m http.server 5500
~~~

Con Node.js:

~~~powershell
npx http-server -p 5500
~~~

Abrir:
- http://localhost:5500/index.html

## 7) Uso practico de Swagger

Prerrequisitos:
- API levantada en Development.
- `Swagger:Enabled=true` en la configuracion activa (en Development ya viene activo).

Levantar API:

~~~powershell
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj
~~~

URLs utiles:
- http://localhost:5093/swagger
- https://localhost:7129/swagger
- http://localhost:5093/swagger/index.html
- https://localhost:7129/swagger/index.html
- http://localhost:5093/swagger/v1/swagger.json
- https://localhost:7129/swagger/v1/swagger.json

Uso de headers en solicitudes de negocio (Try it out):
- X-User-Id: entero positivo
- X-User-Role: ROL_FUNC | ROL_JEFE | ROL_RRHH | ROL_ADMIN

Ejemplos por rol:
- Funcionario: X-User-Id=100, X-User-Role=ROL_FUNC
- Jefatura: X-User-Id=200, X-User-Role=ROL_JEFE
- RRHH: X-User-Id=300, X-User-Role=ROL_RRHH
- Admin: X-User-Id=400, X-User-Role=ROL_ADMIN

Flujo rapido de validacion:
1. Abrir Swagger.
2. Probar GET /health (sin headers de usuario).
3. Probar GET /api/justificaciones/mias con rol funcionario.
4. Probar GET /api/jefatura/justificaciones/pendientes con rol jefatura.
5. Probar GET /api/rrhh/justificaciones con rol RRHH.

Si faltan/son invalidos los headers, la API responde 401.

## 8) Pruebas creadas

Proyecto:
- backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj

Pruebas actuales:
- Archivo: backend/tests/IntegradorMarcas.Tests/UnitTest1.cs
  - Nombre: UnitTest1.Test1
  - Tipo: unitaria (placeholder)
  - Proposito: base minima del proyecto de pruebas.
- Archivo: backend/tests/IntegradorMarcas.Tests/ErrorLogIntegrationTests.cs
  - Nombre: ErrorLogIntegrationTests.LogAsync_DebeInsertarRegistroEnAuditoriaErrorApi
  - Tipo: integracion (Trait Category=Integration)
  - Proposito: validar que ErrorLogRepository.LogAsync inserta en Auditoria.ErrorApi.

Comandos (copiar/pegar):

~~~powershell
# Ejecutar todas las pruebas
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj -v minimal

# Ejecutar solo integracion
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "Category=Integration"

# Ejecutar una prueba/clase especifica
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --filter "FullyQualifiedName~ErrorLogIntegrationTests"

# Ejecutar con cobertura
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj --collect:"XPlat Code Coverage"
~~~

Incidencia conocida actual:
- Error de compilacion CS0103 en backend/tests/IntegradorMarcas.Tests/ErrorLogIntegrationTests.cs.
- Causa: se referencia cleanupSql y no esta definido.
- Impacto: cualquier comando dotnet test falla antes de ejecutar los casos hasta corregir esa referencia.

## 9) Troubleshooting

### 401 en API
Revisar:
- Que envies X-User-Id con entero positivo.
- Que envies X-User-Role valido.

### 403 en endpoints
Revisar rol vs endpoint:
- Acciones de jefatura requieren ROL_JEFE.
- Listado global RRHH requiere ROL_RRHH.

### Error frontend: no conecta con API
Revisar:
- API levantada en http://localhost:5093.
- URL base de frontend alineada al puerto real.
- Si cambiaste puerto, actualizar app.js o variable global/sessionStorage que usa el frontend.

### Error SQL por objetos faltantes
Si aparecen errores con objetos dbo legacy (funciones/tablas), validar compatibilidad de scripts ejecutados y evitar mezclar seeds legacy con el esquema moderno sin estrategia.

### Error de logging en Auditoria.ErrorApi
Si no ejecutaste docs/db/005_fix_errorapi_schema.sql, pueden fallar inserciones de logs por desalineacion de columnas.

## 10) Comandos utiles (copiar/pegar)

~~~powershell
# Restaurar y compilar backend
dotnet restore backend/IntegradorMarcas.slnx
dotnet build backend/IntegradorMarcas.slnx

# Ejecutar API
dotnet run --project backend/src/IntegradorMarcas.Api/IntegradorMarcas.Api.csproj

# Ejecutar tests
dotnet test backend/tests/IntegradorMarcas.Tests/IntegradorMarcas.Tests.csproj -v minimal

# Servir frontend con Python
python -m http.server 5500

# Servir frontend con Node
npx http-server -p 5500
~~~

## 11) Flujo rapido recomendado

1. Ejecutar SQL en orden: 001 -> 002 -> 004 -> 005.
2. Compilar backend.
3. Levantar API.
4. Levantar frontend en puerto 5500.
5. Probar login MVP por rol y llamadas a API con headers requeridos.


## 12) Comportamiento de Sesión: Timeout por Inactividad (5 minutos)

### Descripción general
El sistema implementa un cierre automático de sesión después de 5 minutos de inactividad del usuario. Esto incluye:
- **Monitoreo de actividad**: El frontend rastrea toda actividad del usuario (movimiento del ratón, clics, toques, desplazamientos, pulsaciones de teclado).
- **Temporizador automático**: La sesión se cierra automáticamente si no hay actividad durante 5 minutos.
- **Advertencia preventiva**: Se muestra un modal de advertencia a los 4:30 minutos indicando que la sesión expirará en 30 segundos.
- **Validación del lado del servidor**: El endpoint `/api/session/status` permite validar si la sesión es válida.

### Comportamiento del usuario

**Escenario 1: Usuario activo**
1. Usuario inicia sesión exitosamente.
2. Si el usuario interactúa (escribe, hace clic, mueve el ratón), el temporizador se reinicia.
3. La sesión permanece activa mientras haya actividad dentro de la ventana de 5 minutos.

**Escenario 2: Usuario inactivo (sin interacción)**
1. Usuario inicia sesión y no hace clic, ni escribe, ni mueve el ratón durante 5 minutos.
2. A los 4:30 minutos: aparece un modal de advertencia con un contador regresivo mostrando "30 segundos".
3. El usuario puede:
   - **Hacer clic en "Permanecer Conectado"**: El modal se cierra, el temporizador se reinicia y el usuario continúa trabajando.
   - **Hacer clic en "Cerrar Sesión"**: La sesión se cierra inmediatamente.
   - **No hacer clic**: A los 5:00 minutos exactos, la sesión se cierra automáticamente y el usuario es redirigido a la pantalla de login.

**Escenario 3: Usuario redirigido automáticamente**
- Si la sesión se cierra por timeout o si el usuario intenta hacer una solicitud a la API después de que la sesión haya expirado:
  - Se muestra un toast/notificación: "Sesión expirada por inactividad (5 minutos). Por favor, inicie sesión nuevamente."
  - El usuario es redirigido a la página de inicio de sesión (index.html).

### Implementación técnica

**Frontend (JavaScript en app.js)**
- Funciones de monitoreo:
  - `initIdleMonitor()`: Inicia el rastreo de eventos de actividad del usuario.
  - `resetIdleTimer()`: Reinicia el temporizador de inactividad cuando hay actividad.
  - `isSessionExpired()`: Verifica si la sesión ha expirado.
- Funciones de temporizador:
  - `startSessionExpiryTimer()`: Inicia el temporizador principal (verifica cada 30 segundos).
  - `startFinalCountdown()`: Inicia el temporizador final (verifica cada segundo cuando quedan <30s).
- Modal de advertencia:
  - `showSessionWarningModal()`: Muestra el modal de advertencia.
  - `updateWarningCountdown()`: Actualiza el contador de segundos en el modal.
  - `handleStayLoggedIn()`: Reinicia la sesión cuando el usuario hace clic en "Permanecer Conectado".
  - `handleLogoutNow()`: Cierra la sesión inmediatamente.

**Backend (C# en SessionController.cs)**
- `GET /api/session/status`: Valida que los headers de autenticación (X-User-Id, X-User-Role) sean válidos.
  - Responde con 200 OK si la sesión es válida.
  - Responde con 401 Unauthorized si falta algún header o son inválidos.
- `POST /api/session/logout`: Endpoint para cerrar la sesión (puede ampliarse en el futuro para invalidación de tokens).

### Configuración

**Tiempos configurables** (en app.js, líneas ~115-120):
```javascript
const SESSION_CONFIG = {
  INACTIVITY_TIMEOUT_MS: 5 * 60 * 1000,  // 5 minutos (300,000 ms)
  WARNING_TIME_MS: 4.5 * 60 * 1000,      // 4:30 (270,000 ms)
  CHECK_INTERVAL_MS: 30 * 1000,          // Verificar cada 30 segundos
  FINAL_CHECK_INTERVAL_MS: 1 * 1000      // Verificar cada 1 segundo en los últimos 30s
};
```

### Eventos monitoreados

El sistema rastrea los siguientes eventos de usuario para considerar actividad:
- `mousemove`: Movimiento del ratón
- `keypress`: Pulsación de teclado
- `click`: Clic del ratón
- `touchstart`: Toque en pantalla (dispositivos móviles)
- `scroll`: Desplazamiento de página

**Nota**: El movimiento del ratón sobre ciertos elementos (ej: tooltips, modales) puede no reiniciar el temporizador dependiendo de la lógica del navegador.

### Almacenamiento local

La información de sesión se almacena en `sessionStorage`:
- `sjm_session`: Objeto JSON con `isAuth`, `username`, `role`, `company`, `apiBaseUrl`.
- `sjm_lastActivity`: Timestamp ISO de la última actividad del usuario (se actualiza constantemente).

Al cerrar sesión (manual o por timeout), se limpian ambas claves.

### Verificación en el navegador

Para verificar el comportamiento:

1. **Abrir la consola del navegador** (F12).
2. **Iniciar sesión** en el dashboard.
3. **Sin hacer nada**: esperar 4:30 minutos.
4. **Resultado esperado**: Aparece un modal con "Sesión por Expirar" mostrando un contador regresivo de 30 segundos.
5. **Opción A**: Haga clic en "Permanecer Conectado" → modal se cierra y sesión continúa.
6. **Opción B**: Espere hasta que se agoten los 30 segundos → sesión se cierra automáticamente y es redirigido a login.

### Futuras mejoras

- Integración de tokens JWT con expiración sincronizada en servidor.
- Endpoint opcional `/api/session/refresh` para extender sesiones sin logout.
- Logging de eventos de timeout en auditoría para análisis de patrones de uso.
- Configuración de timeouts diferentes por rol de usuario.
