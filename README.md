# Integrador Marcas

Repositorio del sistema de Justificacion de Marcas para CNP/FANAL.

## Estado real del repositorio
- Frontend web estatico (HTML/CSS/JS): login simulado, dashboard por roles y consumo de API.
- Backend .NET 8 por capas (Api, Application, Domain, Infrastructure).
- Persistencia SQL Server via Dapper y SQL centralizado.
- Flujos implementados: Funcionario, Jefatura y RRHH.

## Arquitectura resumida
```text
Frontend (index.html, dashboard.html, app.js)
	 -> HTTP + headers X-User-Id / X-User-Role
API .NET 8 (controllers + validacion + excepciones)
	 -> Application (servicios, reglas, validadores)
Infrastructure (repositorios Dapper + SQL)
	 -> SQL Server (INTEGRA_CNP)
```

## Endpoints implementados
- POST /api/justificaciones
- GET /api/justificaciones/mias
- GET /api/jefatura/justificaciones/pendientes
- GET /api/jefatura/justificaciones/{justificacionId}
- PATCH /api/jefatura/justificaciones/{justificacionId}/resolver
- GET /api/rrhh/justificaciones
- GET /health

## Seguridad actual (MVP)
- Identidad por headers obligatorios:
  - X-User-Id (int > 0)
  - X-User-Role (ROL_FUNC, ROL_JEFE, ROL_RRHH)
- Autorizacion por rol en capa de servicio.
- Manejo de errores con ProblemDetails y correlationId.

## Quickstart local
1. Ejecutar scripts SQL base:
	- docs/db/001_init_integra_cnp.sql
	- docs/db/007_integra_local_bridge.sql
	- docs/db/008_add_comentario_resolucion.sql (si aplica)
2. Configurar backend/src/IntegradorMarcas.Api/appsettings.Development.json:
	- ConnectionStrings:IntegraCnp
3. Restaurar y compilar:
	- dotnet restore backend/IntegradorMarcas.slnx
	- dotnet build backend/IntegradorMarcas.slnx
4. Ejecutar API:
	- dotnet run --project backend/src/IntegradorMarcas.Api
5. Probar salud:
	- http://localhost:5093/health
6. Abrir frontend:
	- index.html

## Configuracion por entorno
- appsettings.json: base comun.
- appsettings.Development.json: desarrollo local (Swagger habilitado).
- appsettings.Production.json: produccion segura (Swagger deshabilitado).

En entorno no Development, la API exige ConnectionStrings:IntegraCnp y falla rapido si no existe.

## Pruebas
- Proyecto: backend/tests/IntegradorMarcas.Tests
- Framework: xUnit
- Estado actual: cobertura minima (placeholder inicial)
- Comando:
  - dotnet test backend/IntegradorMarcas.slnx

## Documentacion tecnica
Portal maestro:
- docs/README-manuales.md

Documentos especializados:
- docs/arquitectura-codigo-actual.md
- docs/api-endpoints-reference.md
- docs/frontend-modulos-y-flujos.md
- docs/flujos-datos-end-to-end.md
- docs/convenciones-codigo-y-documentacion.md
- docs/pruebas-estrategia-y-cobertura.md
- docs/manual-tecnico.md
- docs/Guia_Implementacion_Dev_Prod.md

## Convenciones clave
- Backend por capas con separacion de responsabilidades.
- SQL parametrizado y centralizado.
- Errores de negocio via AppException + ProblemDetails.
- Documentacion en espanol y alineada al codigo ejecutable.

