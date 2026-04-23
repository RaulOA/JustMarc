# Env Split Dev vs Prod (Backend API)

## Objetivo
Separar configuracion de Desarrollo y Produccion para evitar credenciales en codigo/fuentes y reducir riesgo de configuraciones inseguras en Produccion.

## Hallazgos actuales
- `backend/src/IntegradorMarcas.Api/appsettings.json` y `backend/src/IntegradorMarcas.Api/appsettings.Development.json` tienen las mismas `ConnectionStrings` (duplicadas).
- La API usa `ConnectionStrings:IntegraCnp` en `backend/src/IntegradorMarcas.Infrastructure/Data/SqlConnectionFactory.cs`.
- `Security:UseMockIdentity` esta en `true` en ambos archivos de settings.
- `Swagger:Enabled` esta en `true` en ambos archivos de settings.
- `launchSettings.json` fuerza `ASPNETCORE_ENVIRONMENT=Development` solo para ejecucion local.

## Cambios propuestos (exactos)

### 1) `backend/src/IntegradorMarcas.Api/appsettings.json`
Dejar solo defaults no sensibles (base para todos los entornos). Quitar bloque `ConnectionStrings`.

Propuesta de contenido:

```json
{
  "Security": {
    "UseMockIdentity": false,
    "HeaderUserId": "X-User-Id",
    "HeaderRole": "X-User-Role"
  },
  "Swagger": {
    "Enabled": false
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### 2) `backend/src/IntegradorMarcas.Api/appsettings.Development.json`
Mantener valores locales de desarrollo, incluyendo `ConnectionStrings` y overrides de seguridad/usabilidad local.

Propuesta de contenido:

```json
{
  "ConnectionStrings": {
    "IntegraCnp": "Server=localhost;Database=INTEGRA_CNP;Trusted_Connection=True;TrustServerCertificate=True;",
    "WizdomReadOnly": "Server=localhost;Database=WIZDOM;Trusted_Connection=True;TrustServerCertificate=True;",
    "SifcnpReadOnly": "Server=localhost;Database=SIFCNP;Trusted_Connection=True;TrustServerCertificate=True;"
  },
  "Security": {
    "UseMockIdentity": true,
    "HeaderUserId": "X-User-Id",
    "HeaderRole": "X-User-Role"
  },
  "Swagger": {
    "Enabled": true
  }
}
```

### 3) Nuevo archivo `backend/src/IntegradorMarcas.Api/appsettings.Production.json`
No guardar secretos en este archivo. Solo overrides seguros para Produccion.

Propuesta de contenido:

```json
{
  "Security": {
    "UseMockIdentity": false
  },
  "Swagger": {
    "Enabled": false
  }
}
```

### 4) `backend/src/IntegradorMarcas.Api/Program.cs`
Agregar validacion de arranque para Produccion: fallar rapido si falta `ConnectionStrings:IntegraCnp`.

Snippet propuesto (despues de crear `builder`):

```csharp
if (!builder.Environment.IsDevelopment())
{
    var integraCnp = builder.Configuration.GetConnectionString("IntegraCnp");
    if (string.IsNullOrWhiteSpace(integraCnp))
    {
        throw new InvalidOperationException(
            "ConnectionStrings:IntegraCnp no esta configurada para entorno no-Development.");
    }
}
```

### 5) `README.md` (seccion de configuracion)
Documentar que en Produccion las cadenas se inyectan por variables de entorno o secreto del host.

Variables recomendadas:
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__IntegraCnp=<cadena prod>`
- `ConnectionStrings__WizdomReadOnly=<cadena prod readonly>`
- `ConnectionStrings__SifcnpReadOnly=<cadena prod readonly>`

## Comportamiento esperado en runtime
- En Development:
  - Se carga `appsettings.json` + `appsettings.Development.json`.
  - Se usan cadenas locales del archivo Development.
  - `Security:UseMockIdentity=true` y `Swagger:Enabled=true`.
- En Production:
  - Se carga `appsettings.json` + `appsettings.Production.json` + variables de entorno.
  - Si falta `ConnectionStrings:IntegraCnp`, la app falla al iniciar (fail-fast).
  - `UseMockIdentity=false` y `Swagger` deshabilitado por defecto.

## Notas de seguridad
- No commitear credenciales reales en `appsettings*.json`.
- Preferir Secret Manager (local) y variables de entorno/secret store (Prod).
- Si se requiere `WizdomReadOnly` o `SifcnpReadOnly` en futuro, mantenerlas tambien fuera de repositorio para Prod.
