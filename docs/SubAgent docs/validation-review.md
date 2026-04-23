# Validation Review - Input/Output Coverage

Fecha: 2026-04-23

## Executive Summary
- La validacion de entrada principal existe en capa Application (campos requeridos, longitudes, rango de fechas en varios flujos, compania y accion), pero no es uniforme en todos los endpoints.
- El riesgo mas importante es una discrepancia funcional: se acepta comentario de resolucion, pero no se persiste ni se usa en SQL.
- En creacion de detalles se valida que TipoJustificacionID sea > 0, pero no se valida existencia en catalogo; hoy se delega el rechazo al FK de BD.
- Hay duplicidad de funciones en frontend (parseApiError y showNotice) con sombreado de implementaciones, lo que puede generar comportamiento inconsistente y deuda tecnica.
- El endpoint de boletas propias no valida explicitamente rango de fechas invertido, a diferencia de Jefatura y RRHH.
- El shaping de salida RRHH usa MIN(tj.Descripcion) para tipo principal, lo que puede producir resultados no representativos cuando hay multiples tipos.
- En SQL base (001) hay buenas restricciones de nullabilidad, longitudes y FK; en staging bridge (007) predomina nullabilidad abierta por diseno de ingestion.
- El frontend escapa HTML en la mayoria de renders tabulares y usa textContent en detalle dinamico, reduciendo riesgo XSS en UI.

## Top Findings (ordered by severity)

### F-01 - Comentario de resolucion aceptado pero descartado
- Severidad: Alta
- Evidencia:
  - backend/src/IntegradorMarcas.Api/Contracts/Requests/ResolverJustificacionRequest.cs:6
  - backend/src/IntegradorMarcas.Application/DTOs/ResolverJustificacionDto.cs:6
  - backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs:202
  - backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs:214
  - backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs:198
- Riesgo:
  - Se recibe Comentario desde API pero no se almacena ni audita, creando perdida silenciosa de datos y posible incumplimiento de trazabilidad.
- Fix concreto:
  - Agregar columna ComentarioResolucion (p.ej. VARCHAR(500) NULL) en Justificaciones_Encabezado o tabla de auditoria.
  - Extender ResolverPendiente para persistir comentario.
  - Validar longitud y nullabilidad (p.ej. <= 500) en JustificacionValidator.

### F-02 - Validacion de tipo de justificacion incompleta (solo > 0)
- Severidad: Alta
- Evidencia:
  - backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs:28
  - backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs:44
  - docs/db/001_init_integra_cnp.sql:108
- Riesgo:
  - Un TipoJustificacionID inexistente puede llegar hasta SQL y fallar por FK, retornando error tecnico (500/409 segun manejo) en vez de un 400 de validacion semantica.
- Fix concreto:
  - Validar existencia de TipoJustificacionID contra Cat_TiposJustificacion antes de insertar (repositorio/servicio).
  - Responder AppException 400 con mensaje de dominio cuando el id no exista.

### F-03 - Endpoint de boletas propias no valida rango invertido
- Severidad: Media
- Evidencia:
  - backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs:56
  - backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs:31
  - backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs:38
  - backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs:48
- Riesgo:
  - Si desde > hasta, se obtiene vacio silencioso y no error de validacion, comportamiento inconsistente respecto a Jefatura/RRHH.
- Fix concreto:
  - Invocar ValidateRangoFechas(filtros.Desde, filtros.Hasta) tambien en ListMineAsync.

### F-04 - TipoPrincipal en RRHH es no deterministico/semantica debil
- Severidad: Media
- Evidencia:
  - backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs:113
- Riesgo:
  - MIN(tj.Descripcion) selecciona alfabeticamente, no el tipo mas reciente ni el mas representativo; puede degradar calidad de salida y reporteria.
- Fix concreto:
  - Definir regla explicita (p.ej. primer detalle por fecha, tipo mayoritario, o concatenacion de tipos) y reflejarla en SQL.

### F-05 - Duplicidad y sombreado de funciones en frontend
- Severidad: Media
- Evidencia:
  - app.js:196
  - app.js:215
  - app.js:816
  - app.js:831
  - app.js:824
- Riesgo:
  - Existe deuda tecnica y riesgo de regresion: una version usa innerHTML con msg y la otra redirige a toast; cambios futuros pueden reactivar rutas menos seguras.
- Fix concreto:
  - Dejar una sola implementacion por funcion.
  - Evitar inyeccion en UI usando textContent o sanitizacion estricta si se requiere HTML.

### F-06 - Frontend sin limites declarativos de longitud/patron en campos clave
- Severidad: Media
- Evidencia:
  - index.html:213
  - index.html:218
  - dashboard.html:287
  - dashboard.html:314
  - dashboard.html:448
- Riesgo:
  - El usuario puede ingresar payloads grandes o formatos pobres hasta llegar al backend; UX de error tarda y aumenta ruido operativo.
- Fix concreto:
  - Agregar atributos maxlength, minlength, pattern, required donde aplique.
  - Reflejar en frontend los mismos bounds de backend (500/250/150).

### F-07 - Shaping frontend oculta problemas de calidad de datos
- Severidad: Baja
- Evidencia:
  - app.js:301
  - app.js:308
  - app.js:311
  - app.js:312
- Riesgo:
  - Defaults como id=0, Cargando..., Sin detalle pueden enmascarar contratos rotos o payloads incompletos, dificultando deteccion temprana.
- Fix concreto:
  - Diferenciar fallback visual de error de contrato.
  - Registrar warning cuando faltan campos obligatorios (telemetria/consola controlada).

### F-08 - CORS demasiado permisivo para API local
- Severidad: Baja
- Evidencia:
  - backend/src/IntegradorMarcas.Api/Program.cs:30
  - backend/src/IntegradorMarcas.Api/Program.cs:31
  - backend/src/IntegradorMarcas.Api/Program.cs:32
- Riesgo:
  - Aumenta superficie de consumo no esperado y dificulta asegurar supuestos de origen durante pruebas/despliegues.
- Fix concreto:
  - Restringir origins por entorno (configuracion), manteniendo flexibilidad solo en desarrollo controlado.

### F-09 - Doble registro de UseExceptionHandler
- Severidad: Baja
- Evidencia:
  - backend/src/IntegradorMarcas.Api/Program.cs:44
  - backend/src/IntegradorMarcas.Api/Program.cs:60
- Riesgo:
  - Configuracion duplicada puede confundir mantenimiento y evolucion del contrato de errores.
- Fix concreto:
  - Consolidar en un solo middleware de excepciones con payload estandarizado (incluyendo correlationId).

### F-10 - Staging bridge con nullabilidad amplia sin checks de dominio
- Severidad: Baja
- Evidencia:
  - docs/db/007_integra_local_bridge.sql:74
  - docs/db/007_integra_local_bridge.sql:125
  - docs/db/007_integra_local_bridge.sql:149
  - docs/db/007_integra_local_bridge.sql:186
- Riesgo:
  - Si no hay capa posterior de normalizacion estricta, pueden propagarse datos incompletos hacia vistas canonicas/consumo.
- Fix concreto:
  - Mantener staging flexible, pero introducir reglas de calidad en capa canonica (checks de claves requeridas, deduplicacion, estados validos, rechazos controlados).

## Positive Findings
- Validacion de autenticacion por cabeceras robusta (ID entero > 0 y rol obligatorio).
  - backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs:26
  - backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs:27
  - backend/src/IntegradorMarcas.Api/Security/HeaderUserContext.cs:35
- Validaciones de negocio y bounds en Application para create/accion/compania/texto.
  - backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs:18
  - backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs:40
  - backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs:50
  - backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs:73
  - backend/src/IntegradorMarcas.Application/Validation/JustificacionValidator.cs:81
- Control de autorizacion por rol en servicio para todas las operaciones sensibles.
  - backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs:22
  - backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs:45
  - backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs:56
  - backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs:97
- Acceso a datos parametrizado con Dapper (reduce riesgo de SQL injection).
  - backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs:29
  - backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs:71
  - backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs:206
- Base SQL principal con nullabilidad y FK bien definidas para core transaccional.
  - docs/db/001_init_integra_cnp.sql:83
  - docs/db/001_init_integra_cnp.sql:103
  - docs/db/001_init_integra_cnp.sql:108
  - docs/db/001_init_integra_cnp.sql:70
- Frontend aplica escapeHtml en render de tablas y usa textContent en detalle dinamico.
  - app.js:504
  - app.js:625
  - app.js:763
  - app.js:730

## Coverage Notes
- Data formats: coberturas parciales (fechas, compania, accion) con huecos en tipo de catalogo y consistencia de tipo principal.
- Data types: tipado fuerte en C#/SQL, pero faltan validaciones semanticas tempranas para algunos enteros de query/body.
- Nullability: fuerte en tablas core (001) y mas laxa en staging bridge (007) por naturaleza de ingestion.
- Bounds: buenos limites en backend para motivo/observacion/funcionario; frontend no refleja todos los limites con atributos HTML.
- Output shaping: mapeos existentes, pero hay defaults que ocultan errores y una regla SQL de TipoPrincipal que puede distorsionar salida.
