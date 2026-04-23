# Justificacion de Marca

## Backend API (.NET 8)

La solución backend se encuentra en `backend/` y expone endpoints para RF-02/RF-03:
- `POST /api/justificaciones`
- `GET /api/justificaciones/mias`
- `GET /api/jefatura/justificaciones/pendientes`
- `PATCH /api/jefatura/justificaciones/{justificacionId}/resolver`
- `GET /health`

### Ejecutar

1. Aplicar el script SQL en SQL Server: `docs/db/001_init_integra_cnp.sql`.
2. Ajustar la cadena `ConnectionStrings:IntegraCnp` en `backend/src/IntegradorMarcas.Api/appsettings.Development.json`.
3. Restaurar y compilar:
	- `dotnet restore backend/IntegradorMarcas.slnx`
	- `dotnet build backend/IntegradorMarcas.slnx`
4. Ejecutar API:
	- `dotnet run --project backend/src/IntegradorMarcas.Api`

### Headers mock de identidad (MVP)

Enviar en cada request:
- `X-User-Id`: identificador de usuario (int)
- `X-User-Role`: `ROL_FUNC` o `ROL_JEFE`

