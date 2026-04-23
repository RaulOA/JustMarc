# Justificacion de Marca

## Backend API (.NET 8)

La solución backend se encuentra en `backend/` y expone endpoints para RF-02/RF-03:
- `POST /api/justificaciones`
- `GET /api/justificaciones/mias`
- `GET /api/jefatura/justificaciones/pendientes`
- `PATCH /api/jefatura/justificaciones/{justificacionId}/resolver`
- `GET /health`

### Ejecutar

1. Aplicar scripts SQL en este orden en SQL Server:
	- `docs/db/001_init_integra_cnp.sql`
	- `docs/db/007_integra_local_bridge.sql`
2. La API de Marcas se conecta unicamente a `INTEGRA_CNP` (connection string `IntegraCnp`).
3. Ajustar la cadena `ConnectionStrings:IntegraCnp` en `backend/src/IntegradorMarcas.Api/appsettings.Development.json`.
4. Restaurar y compilar:
	- `dotnet restore backend/IntegradorMarcas.slnx`
	- `dotnet build backend/IntegradorMarcas.slnx`
5. Ejecutar API:
	- `dotnet run --project backend/src/IntegradorMarcas.Api`

### Headers mock de identidad (MVP)

Enviar en cada request:
- `X-User-Id`: identificador de usuario (int)
- `X-User-Role`: `ROL_FUNC` o `ROL_JEFE`

### Configuracion Dev vs Prod

- `appsettings.json`: contiene defaults seguros y no sensibles.
- `appsettings.Development.json`: contiene configuracion local de desarrollo (SQL Express local, `UseMockIdentity=true`, `Swagger:Enabled=true`).
- `appsettings.Production.json`: fuerza flags seguras para produccion (`UseMockIdentity=false`, `Swagger:Enabled=false`) y no incluye secretos.

En produccion, las cadenas de conexion deben inyectarse por variables de entorno (o secret store del host):
- `ASPNETCORE_ENVIRONMENT=Production`
- `ConnectionStrings__IntegraCnp=<cadena de conexion productiva requerida>`
- `ConnectionStrings__WizdomReadOnly=<cadena de solo lectura productiva>`
- `ConnectionStrings__SifcnpReadOnly=<cadena de solo lectura productiva>`

La API valida al iniciar que `ConnectionStrings:IntegraCnp` exista en entornos no Development y falla rapido si falta.

## Guía de Implementación (Dev → Prod)

Para instrucciones completas de setup, configuración de entornos, publicación y troubleshooting, consultar:

**[docs/Guia_Implementacion_Dev_Prod.md](docs/Guia_Implementacion_Dev_Prod.md)**

Incluye: prerrequisitos, arquitectura, orden de scripts BD, runbooks de desarrollo y publicación, variables de entorno, checklist de verificación, rollback y matriz de troubleshooting.

