# Manual Tecnico del Sistema

## 1. Proposito
Servir como guia tecnica operacional de alto nivel del sistema Integrador Marcas, con enlaces a la documentacion especializada del codigo.

## 2. Alcance
Este manual resume:
- Arquitectura actual y componentes.
- Operacion local y configuracion por entorno.
- Modelo de seguridad y manejo de errores.
- Estado de pruebas y calidad.

El detalle tecnico profundo se mantiene en documentos especializados para evitar duplicidad.

## 3. Fuente de verdad
- backend/src/**
- backend/tests/**
- app.js, index.html, dashboard.html
- docs/db/*.sql

## 4. Arquitectura operativa

```text
Frontend estatico (HTML/CSS/JS)
    -> API .NET 8 (controllers + services + validators)
    -> Repositorios Dapper + SQL centralizado
    -> SQL Server INTEGRA_CNP
```

Resumen por capa:
- Presentacion: index.html, dashboard.html, style.css, app.js.
- API: rutas HTTP, contratos, DI, middleware de excepciones, CORS, health.
- Application: reglas de negocio, autorizacion por rol, validaciones.
- Infrastructure: queries SQL y acceso a datos.

Referencia detallada:
- [arquitectura-codigo-actual.md](arquitectura-codigo-actual.md)

## 5. Endpoints y modulos
Endpoints activos:
- POST /api/justificaciones
- GET /api/justificaciones/mias
- GET /api/jefatura/justificaciones/pendientes
- GET /api/jefatura/justificaciones/{justificacionId}
- PATCH /api/jefatura/justificaciones/{justificacionId}/resolver
- GET /api/rrhh/justificaciones
- GET /health

Referencias:
- [api-endpoints-reference.md](api-endpoints-reference.md)
- [frontend-modulos-y-flujos.md](frontend-modulos-y-flujos.md)
- [flujos-datos-end-to-end.md](flujos-datos-end-to-end.md)

## 6. Configuracion y ejecucion local

### 6.1 Prerrequisitos
- .NET SDK 8.x
- SQL Server 2019+
- Navegador moderno

### 6.2 Scripts SQL minimos
1. docs/db/001_init_integra_cnp.sql
2. docs/db/007_integra_local_bridge.sql
3. docs/db/008_add_comentario_resolucion.sql (si aplica)

### 6.3 Comandos
```powershell
dotnet restore backend/IntegradorMarcas.slnx
dotnet build backend/IntegradorMarcas.slnx
dotnet run --project backend/src/IntegradorMarcas.Api
```

Health:
- http://localhost:5093/health

Referencia de despliegue Dev/Prod:
- [Guia_Implementacion_Dev_Prod.md](Guia_Implementacion_Dev_Prod.md)

## 7. Seguridad y autorizacion
- Identidad HTTP basada en headers:
  - X-User-Id
  - X-User-Role
- Roles soportados: ROL_FUNC, ROL_JEFE, ROL_RRHH.
- Autorizacion por caso de uso en JustificacionService.
- CORS local configurado para permitir origenes/metodos/headers.

## 8. Manejo de errores y trazabilidad
- Formato de error: ProblemDetails (RFC7807).
- Codigos frecuentes: 400, 401, 403, 404, 409, 499, 500.
- Se emite X-Correlation-Id en errores gestionados por el handler principal.
- ErrorLogRepository persiste excepciones en dbo.ApiErrorLog sin propagar errores de logging.

## 9. Pruebas y calidad
- Proyecto: backend/tests/IntegradorMarcas.Tests.
- Framework: xUnit.
- Estado actual: existe prueba placeholder y cobertura baja.

Estrategia y backlog:
- [pruebas-estrategia-y-cobertura.md](pruebas-estrategia-y-cobertura.md)

## 10. Convenciones
Estilo y reglas de mantenimiento documental/codigo:
- [convenciones-codigo-y-documentacion.md](convenciones-codigo-y-documentacion.md)

## 11. Checklist de validacion operativa
- API inicia sin errores y /health responde 200.
- Endpoints funcionan con headers validos por rol.
- Errores devuelven ProblemDetails.
- Documentacion especializada enlazada desde este manual.

## 12. Historial de cambios
- 2026-04-23: Manual refactorizado como guia de alto nivel y portal de referencias tecnicas.
