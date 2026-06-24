# Gestión de credenciales de conexión a la base de datos

> Documento para el **Manual Técnico** del sistema *Justificación de Marca* (INTEGRA_CNP).
> Estado: vigente · Última actualización: 2026-06-24

## 1. Resumen y decisión

La cadena de conexión a SQL Server **no se almacena en ningún archivo ni código del repositorio**. Se inyecta en tiempo de ejecución mediante una **variable de entorno de usuario de Windows**:

```
ConnectionStrings__IntegraCnp
```

.NET mapea automáticamente esa variable (con doble guion bajo `__`) a la clave de configuración `ConnectionStrings:IntegraCnp` que la aplicación ya consume. Por lo tanto **la contraseña y el usuario de base de datos viven únicamente en la cuenta de Windows del operador/servidor**, fuera del proyecto.

- **Desarrollo:** variable de entorno de **usuario** de Windows en el equipo de cada desarrollador.
- **Producción (IIS):** variable de entorno a nivel del *Application Pool*/sitio o, idealmente, un gestor de secretos (ver §7).

## 2. Por qué NO se guardan las credenciales dentro del proyecto

Aunque hoy el proyecto no usa un controlador de versiones, **está previsto incorporarlo próximamente (GitHub / Azure DevOps)**. Si las credenciales estuvieran dentro de un archivo del proyecto (p. ej. `appsettings.json`), se convertirían en un riesgo real:

1. **Quedarían en el historial del repositorio para siempre.** Aunque luego se borren, permanecen en el historial de Git y pueden recuperarse. Un secreto que entra al control de versiones debe considerarse **comprometido** y rotarse.
2. **Se propagan sin control.** El código se clona, se bifurca (*fork*), se respalda y se comparte; cada copia lleva el secreto y escapa a los controles de acceso.
3. **Riesgo de exposición pública.** Un repositorio que pasa de privado a público —o un respaldo mal protegido— expondría la contraseña de `sa` y, con ella, toda la base `INTEGRA_CNP`.
4. **Mezcla configuración con código.** Las credenciales son *configuración por entorno* (dev, prod), no parte del código. Acoplarlas obliga a tocar el código para cambiar de servidor o rotar contraseñas.

Esto corresponde a la debilidad catalogada **CWE-798: *Use of Hard-coded Credentials*** y a las recomendaciones de OWASP y de Microsoft citadas en §8.

## 3. Por qué variables de entorno de usuario de Windows

Las variables de entorno son un mecanismo **reconocido y soportado de forma nativa** para mantener los secretos fuera del código:

- **Fuera del árbol del proyecto.** No están en ningún archivo versionable, así que **no pueden subirse al repositorio por accidente** (principio *Config* de The Twelve-Factor App).
- **Soporte nativo de .NET.** El proveedor de configuración de variables de entorno de ASP.NET Core las lee automáticamente y **sobrescriben** los valores de `appsettings.json`/`appsettings.{Entorno}.json`. No requiere código adicional.
- **Ámbito de usuario.** Una variable de **usuario** (no de sistema) solo es visible para esa cuenta de Windows, acotando quién puede leerla.
- **Separación por entorno.** Cada equipo/servidor define su propia variable; los secretos de producción nunca tocan los equipos de desarrollo.

> **Limitación honesta:** las variables de entorno se guardan como **texto plano** y son legibles por procesos de esa cuenta o por quien comprometa la máquina. Son un mecanismo adecuado para *mantener los secretos fuera del repositorio*, **no** un cifrado. Para mayor protección en producción, escalar a un gestor de secretos (ver §7). Microsoft documenta esta misma advertencia.

## 4. Cómo funciona en este proyecto

- La aplicación resuelve la cadena con `IConfiguration.GetConnectionString("IntegraCnp")` en `SqlConnectionFactory` (`backend/src/IntegradorMarcas.Infrastructure/Data/SqlConnectionFactory.cs`).
- `GetConnectionString("IntegraCnp")` lee la clave `ConnectionStrings:IntegraCnp`. La variable de entorno `ConnectionStrings__IntegraCnp` se mapea a esa clave (el `__` equivale a `:`).
- **Orden de precedencia** (gana el último): `appsettings.json` → `appsettings.{Entorno}.json` → *User Secrets* (solo dev) → **variables de entorno** → línea de comandos. Por eso la variable de entorno sobrescribe cualquier valor de los archivos.
- `appsettings.Development.json` **ya no contiene** la cadena `IntegraCnp` (solo quedan los marcadores vacíos `WizdomReadOnly`/`SifcnpReadOnly`).
- En arranque, `Program.cs` valida la cadena:
  - **No-Development:** si falta, **aborta el arranque** (fail-fast).
  - **Development:** si falta, **muestra una advertencia** y continúa (permite trabajar en frontend/`/health` sin BD); las funciones que usan BD fallarán hasta configurarla.

## 5. Cómo configurar la variable (Windows, ámbito de usuario)

> La contraseña **la escribe el operador**; nunca se pega en archivos del proyecto ni se comparte por canales no seguros.

En **PowerShell**, reemplazando `TU_CONTRASEÑA` por la contraseña real:

```powershell
[Environment]::SetEnvironmentVariable(
  "ConnectionStrings__IntegraCnp",
  "Server=192.168.36.83\DESARROLLO,49403;Database=INTEGRA_CNP;User ID=sa;Password=TU_CONTRASEÑA;Encrypt=True;TrustServerCertificate=True;Application Name=IntegradorMarcas.Api",
  "User")
```

Verificar que quedó registrada (en una terminal **nueva**):

```powershell
[Environment]::GetEnvironmentVariable("ConnectionStrings__IntegraCnp", "User")
```

> **Importante:** los procesos ya abiertos (VS Code, terminales, la API) **no ven** la variable hasta reiniciarse. Cierra y vuelve a abrir VS Code/PowerShell después de definirla.

### 5.1 Cadena recomendada para la aplicación

La cadena que entrega SSMS está pensada para esa herramienta; para la API conviene ajustarla:

| Parámetro | SSMS | Recomendado para la API | Motivo |
|---|---|---|---|
| `Database` | (ausente) | `INTEGRA_CNP` | La API debe fijar la base de trabajo. *(confirmar el nombre exacto en el servidor de desarrollo)* |
| `Application Name` | `SQL Server Management Studio` | `IntegradorMarcas.Api` | Permite al DBA identificar las conexiones de la app. |
| `Pooling` | `False` | (omitir → `True`) | El *pooling* mejora el rendimiento de una API web. |
| `Command Timeout` | `0` (infinito) | (omitir → 30 s por defecto) | Evita que una consulta colgada bloquee indefinidamente. |
| `Persist Security Info` | `True` | (omitir → `False`) | Buena práctica: no retener la contraseña en memoria tras conectar. |
| `Encrypt` / `TrustServerCertificate` | `True` / `True` | `True` / `True` (dev) | En producción, preferir certificado válido y `TrustServerCertificate=False`. |

### 5.2 Recomendación de seguridad: no usar `sa`

`sa` es la cuenta de administrador total del motor. Para la aplicación se recomienda un **inicio de sesión SQL dedicado y de mínimo privilegio** (p. ej. `svc_integramarcas`) con permisos solo sobre los objetos que usa la API (esquemas `Configuracion`, `RecursosHumanos`, `Operacion`, `Auditoria` e `Integracion`). Esto limita el impacto si la credencial se ve comprometida. *(Aplicar al menos en producción.)*

## 6. Verificación funcional

1. Definir la variable (§5) y reiniciar la terminal/VS Code.
2. Compilar y levantar la API.
3. `GET http://localhost:5093/health` debe responder `200`.
4. Un endpoint con datos (con cabeceras `X-User-Id` y `X-User-Role`), p. ej. `GET /api/rrhh/justificaciones`, debe devolver datos en lugar de error de conexión.

## 7. Producción y evolución

- **IIS:** definir `ConnectionStrings__IntegraCnp` como variable de entorno del *Application Pool*/sitio (o del sistema), con un **login de mínimo privilegio**, no `sa`.
- **Gestor de secretos (recomendado a futuro):** para una protección superior (cifrado, rotación, auditoría de acceso), migrar a **Azure Key Vault** mediante su proveedor de configuración para ASP.NET Core. Las variables de entorno siguen siendo válidas como puente.
- **Rotación:** si una contraseña llegara a quedar en un archivo, un log o el historial de Git, considerarla comprometida y **rotarla**.

> **Alternativa para desarrollo:** Microsoft también recomienda la herramienta **Secret Manager (User Secrets)** de .NET, que guarda los secretos en un archivo JSON en el perfil del usuario, fuera del árbol del proyecto. Se eligió variables de entorno por simplicidad operativa y porque el mismo mecanismo sirve en IIS; User Secrets queda como opción equivalente para dev.

## 8. Fuentes (respaldo de la práctica)

**Microsoft Learn (primera fuente):**
- [Safe storage of app secrets in development in ASP.NET Core](https://learn.microsoft.com/aspnet/core/security/app-secrets) — «Never store passwords or other sensitive data in source code or configuration files»; sección *Work with environment variables*.
- [Configuration in ASP.NET Core](https://learn.microsoft.com/aspnet/core/fundamentals/configuration/) — convención `__`, precedencia de las variables de entorno sobre `appsettings`, y guía de seguridad/secretos.
- [Store application secrets safely during development (.NET microservices)](https://learn.microsoft.com/dotnet/architecture/microservices/secure-net-microservices-web-applications/developer-app-secrets-storage) — «best practice to not include secrets in source code… not to store secrets in source control».
- [Configuration providers in .NET](https://learn.microsoft.com/dotnet/core/extensions/configuration-providers) — proveedor de variables de entorno.
- [Azure Key Vault configuration provider (ASP.NET Core)](https://learn.microsoft.com/aspnet/core/security/key-vault-configuration) — gestor de secretos para producción.
- [about_Environment_Variables (PowerShell)](https://learn.microsoft.com/powershell/module/microsoft.powershell.core/about/about_environment_variables) y [setx (Windows)](https://learn.microsoft.com/windows-server/administration/windows-commands/setx) — manejo de variables de entorno en Windows.

**Estándares y guías independientes:**
- [OWASP Secrets Management Cheat Sheet](https://cheatsheetseries.owasp.org/cheatsheets/Secrets_Management_Cheat_Sheet.html) — los secretos no deben estar en el código ni entrar al control de versiones; deben inyectarse en tiempo de ejecución.
- [OWASP DevSecOps Guideline — Secrets Management](https://owasp.org/www-project-devsecops-guideline/latest/01a-Secrets-Management).
- [CWE-798: Use of Hard-coded Credentials (MITRE)](https://cwe.mitre.org/data/definitions/798.html) — catálogo de la debilidad de credenciales incrustadas.
- [The Twelve-Factor App — Config](https://12factor.net/config) — guardar la configuración (incl. credenciales) en variables de entorno, no en archivos que puedan subirse al repo.
