# Verificación de roles y control de acceso

Fecha de revisión: 2026-04-23

## 1) Roles detectados

### Base de datos
- Funcionario (RolID=1): definido en [docs/db/001_init_integra_cnp.sql](docs/db/001_init_integra_cnp.sql#L119)
- Jefatura (RolID=2): definido en [docs/db/001_init_integra_cnp.sql](docs/db/001_init_integra_cnp.sql#L120)
- RRHH (RolID=3): definido en [docs/db/001_init_integra_cnp.sql](docs/db/001_init_integra_cnp.sql#L121)
- Tabla y FK de roles en usuarios: [docs/db/001_init_integra_cnp.sql](docs/db/001_init_integra_cnp.sql#L20), [docs/db/001_init_integra_cnp.sql](docs/db/001_init_integra_cnp.sql#L68)

### Backend
- Roles canónicos: ROL_FUNC, ROL_JEFE, ROL_RRHH en [backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs](backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs#L5)
- Alias aceptados por backend:
  - Funcionario: FUNCIONARIO o "1" en [backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs](backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs#L12)
  - Jefatura: JEFATURA o "2" en [backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs](backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs#L18)
  - RRHH: RRHH o "3" en [backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs](backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs#L24)

### Frontend
- Roles usados en sesión/UI: ROL_FUNC, ROL_JEFE, ROL_RRHH en [app.js](app.js#L109), [app.js](app.js#L110), [app.js](app.js#L111)
- Inferencia de rol por username en [app.js](app.js#L122)

## 2) Matriz funcionalidad -> rol permitido/prohibido

| Funcionalidad / Endpoint | Funcionario | Jefatura | RRHH | Evidencia |
|---|---|---|---|---|
| Crear boleta (POST /api/justificaciones) | Permitido | Prohibido | Prohibido | Endpoint: [backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs](backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs#L23). Regla: [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L20) |
| Ver mis boletas (GET /api/justificaciones/mias) | Permitido | Prohibido | Prohibido | Endpoint: [backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs](backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs#L53). Regla: [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L44) |
| Ver pendientes de jefatura (GET /api/jefatura/justificaciones/pendientes) | Prohibido | Permitido | Prohibido | Endpoint: [backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs](backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs#L22). Regla: [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L55) |
| Ver detalle de boleta de jefatura (GET /api/jefatura/justificaciones/{id}) | Prohibido | Permitido (solo subordinados) | Prohibido | Endpoint: [backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs](backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs#L49). Regla subordinado: [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L91) |
| Resolver boleta (PATCH /api/jefatura/justificaciones/{id}/resolver) | Prohibido | Permitido (solo subordinados y estado pendiente) | Prohibido | Endpoint: [backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs](backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs#L104). Reglas: [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L107), [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L120), [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L125) |
| Consulta global RRHH (GET /api/rrhh/justificaciones) | Prohibido | Prohibido | Permitido | Endpoint: [backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs](backend/src/IntegradorMarcas.Api/Controllers/RrhhController.cs#L9). Regla: [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L66) |
| UI Panel Funcionario | Permitido | Oculto | Oculto | Tabs por rol: [app.js](app.js#L414) |
| UI Panel Jefatura | Oculto | Permitido | Oculto | Tabs por rol: [app.js](app.js#L414) |
| UI Panel RRHH | Oculto | Oculto | Permitido | Tabs por rol: [app.js](app.js#L414) |
| UI Consulta Histórica SIFCNP | Permitido | Permitido | Permitido | Tabs por rol: [app.js](app.js#L414) |

## 3) Verificación de controles de acceso

### Backend
- No se encontró autenticación/autorización framework (sin AddAuthentication/UseAuthentication/UseAuthorization ni atributos Authorize).
- El backend obtiene identidad y rol desde headers del request en [backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs](backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs#L22) y [backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs](backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs#L41).
- Las restricciones de rol están implementadas principalmente en la capa de servicio en [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L20).
- Las reglas de pertenencia de subordinado y estado pendiente para resolver sí están bien validadas en servicio y reforzadas en SQL: [backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs](backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs#L120), [backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs](backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs#L227).

### Frontend
- El gating de UI es por visibilidad de pestañas y verificaciones en JavaScript de session.role, por ejemplo [app.js](app.js#L405), [app.js](app.js#L497), [app.js](app.js#L583), [app.js](app.js#L727).
- El rol puede inferirse por nombre de usuario en [app.js](app.js#L122), y los headers enviados a API se construyen en cliente en [app.js](app.js#L178).

## 4) Hallazgos

### Alta
1. Suplantación de rol/usuario por confianza en headers cliente
- Impacto: Un actor puede enviar X-User-Id y X-User-Role arbitrarios y ejecutar funcionalidades de otro rol/usuario.
- Evidencia:
  - Backend confía en headers: [backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs](backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs#L22), [backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs](backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs#L41)
  - Frontend genera esos headers localmente: [app.js](app.js#L178)
  - No hay middleware de autenticación/autorización en pipeline: [backend/src/IntegradorMarcas.Api/Program.cs](backend/src/IntegradorMarcas.Api/Program.cs#L128)

2. CORS totalmente abierto combinado con auth por headers
- Impacto: Superficie de abuso amplia para llamadas cross-origin a la API con headers personalizados.
- Evidencia: [backend/src/IntegradorMarcas.Api/Program.cs](backend/src/IntegradorMarcas.Api/Program.cs#L30), [backend/src/IntegradorMarcas.Api/Program.cs](backend/src/IntegradorMarcas.Api/Program.cs#L31), [backend/src/IntegradorMarcas.Api/Program.cs](backend/src/IntegradorMarcas.Api/Program.cs#L32)

### Media
1. Discrepancia semántica de roles entre capas
- Impacto: Inconsistencia y potenciales errores de integración (BD usa nombres y enteros; backend acepta múltiples alias; frontend usa solo códigos ROL_*).
- Evidencia: [docs/db/001_init_integra_cnp.sql](docs/db/001_init_integra_cnp.sql#L119), [backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs](backend/src/IntegradorMarcas.Domain/Constants/RolesSistema.cs#L12), [app.js](app.js#L109)

2. Modo de identificación no confiable en frontend
- Impacto: Para usuarios no mapeados, se usa userId=10 por defecto y rol inferido por username; genera llamadas inconsistentes y favorece intentos de elevación.
- Evidencia: [app.js](app.js#L122), [app.js](app.js#L169)

### Baja
1. Configuración Security.UseMockIdentity no aplicada explícitamente
- Impacto: Riesgo de confusión operativa; la bandera existe en configuración, pero no se observa lógica condicional de uso.
- Evidencia: [backend/src/IntegradorMarcas.Api/appsettings.Development.json](backend/src/IntegradorMarcas.Api/appsettings.Development.json#L8), [backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs](backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs#L22)

2. Doble registro de UseExceptionHandler
- Impacto: Complejidad innecesaria y ambigüedad de comportamiento en manejo de errores.
- Evidencia: [backend/src/IntegradorMarcas.Api/Program.cs](backend/src/IntegradorMarcas.Api/Program.cs#L44), [backend/src/IntegradorMarcas.Api/Program.cs](backend/src/IntegradorMarcas.Api/Program.cs#L60)

## 5) Conclusión

Estado general: Incorrecto.

Aunque la lógica de negocio por rol en servicios está bien planteada (incluyendo validación de subordinación y estado para jefatura), el modelo de seguridad de acceso es débil porque depende de headers controlados por cliente y no de autenticación/autorización robusta en backend.
