# Funcionario Historico Scope Fix Spec

## Objetivo

Corregir el bug donde un usuario con rol funcionario (ejemplo: "funcionaria ana") ve justificaciones de multiples personas en Consulta Historica, cuando solo debe ver sus propios registros.

## Resumen Ejecutivo

- El comportamiento reportado se explica por un desacople frontend-backend en RF-06:
  - El panel de Consulta Historica en frontend usa un dataset mock global y no aplica scope por usuario autenticado.
  - No existe endpoint backend dedicado para consulta historica SIFCNP con reglas de alcance por rol.
- En contraste, el historial operativo (RF-05) si esta protegido: `GET /api/justificaciones/mias` aplica filtro por `UsuarioID`.
- Resultado: para rol `ROL_FUNC`, la pantalla historica muestra registros de terceros por diseno actual del UI (no por un WHERE roto en SQL productivo de ese flujo, porque ese flujo aun no esta conectado a backend).

## Causa Raiz (Root Cause)

1. Frontend de Consulta Historica sin control de alcance
- En el dashboard, la pestana SIFCNP es visible para todos los roles.
- La tabla se alimenta con `SIFCNP_MOCK_DATA` (registros de varias personas) y `renderSifcnpMockData()` en inicializacion de dashboard.
- `sifcnpSearch()` filtra por texto/fechas, pero no por identidad de sesion ni rol.

2. Falta de endpoint de backend para RF-06 con autorizacion y scope
- No hay controlador/servicio/repositorio para "consulta historica" en backend.
- Tampoco hay DTOs/queries de aplicacion para resolver historico con reglas:
  - `ROL_FUNC`: solo propios.
  - `ROL_RRHH`: global.
  - `ROL_JEFE`: segun decision funcional (global, solo aprobables o solo propios historicos).

3. La capa de seguridad existente protege RF-05 y RF-04, pero no RF-06
- RF-05 (historial propio) si usa SQL con `WHERE je.UsuarioID = @UsuarioID`.
- RF-04 (RRHH global) usa endpoint y query separados.
- RF-06 quedo como UI mock, por eso se salta los controles de backend.

## Evidencia Tecnica Relevante

### Frontend
- `app.js`
  - `configureRoleUI()` habilita `panel-sifcnp` para todos los roles.
  - `SIFCNP_MOCK_DATA` contiene registros de multiples funcionarios.
  - `renderSifcnpMockData()` carga todos los registros.
  - `sifcnpSearch()` no aplica filtro por usuario autenticado.
  - `initDashboardPage()` llama `renderSifcnpMockData()` al iniciar.
- `dashboard.html`
  - Panel "Consulta Historica - SIFCNP" con filtro libre por funcionario y tabla global.

### Backend actual (si protegido en otros flujos)
- `backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs`
  - `GET /api/justificaciones/mias` usa contexto autenticado.
- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`
  - `ListMineAsync`: solo `ROL_FUNC`.
  - `ListRrhhAsync`: solo `ROL_RRHH`.
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
  - `ListMine` con `WHERE je.UsuarioID = @UsuarioID`.

### SQL historico disponible para integrar RF-06
- `docs/db/002_integra_marcas_objetos.sql`
  - Vista `Integracion.v_JustificacionEncabezadoSifcnp` (incluye `CedulaFuncionario`).
  - Vista `Integracion.v_JustificacionDetalleSifcnp`.

## Especificacion de Correccion

## 1) Backend: crear flujo historico con scope server-side obligatorio

Agregar endpoint dedicado y nunca depender de filtro UI para seguridad.

Contrato recomendado:
- `GET /api/justificaciones/historico`
- Query params:
  - `funcionario` (opcional; solo permitido para RRHH/Jefatura si negocio lo autoriza)
  - `fechaDesde` (opcional)
  - `fechaHasta` (opcional)

Reglas de alcance (minimo):
- `ROL_FUNC`: ignorar cualquier `funcionario` recibido y forzar filtro por identidad del usuario autenticado.
- `ROL_RRHH`: permitir consulta global con filtros.
- `ROL_JEFE`: definir explicitamente con negocio. Recomendado corto plazo: denegar (403) hasta definir alcance historico de jefatura para evitar sobreexposicion.

Mapeo identidad para historico:
- Resolver cedula del usuario autenticado (`RecursosHumanos.Usuario.Cedula`) y filtrar historico por esa cedula.
- Evitar confiar en nombre libre ingresado por UI como llave de seguridad.

## 2) Application/Infrastructure: DTO + servicio + query historica

Agregar DTOs especificos para historico (request/response) y query con filtro condicional por rol.

Lineamiento SQL:
- Fuente: `Integracion.v_JustificacionEncabezadoSifcnp` + join opcional a detalle.
- Para `ROL_FUNC`: `WHERE h.CedulaFuncionario = @CedulaFuncionarioAutenticado`.
- Para `ROL_RRHH`: filtro opcional por nombre/cedula, fechas, estado.
- Agregar ORDER BY por fecha descendente y paginacion (recomendado) para evitar cargas grandes.

## 3) Frontend: reemplazar mock por API y endurecer UI por rol

Cambios funcionales:
- Eliminar dependencia de `SIFCNP_MOCK_DATA` para datos productivos.
- Reemplazar `renderSifcnpMockData()` por `renderSifcnpHistorico()` que llame backend.
- En `ROL_FUNC`:
  - ocultar/disabled input de "Funcionario" en panel SIFCNP (porque el scope es implicito al usuario).
  - mostrar mensaje "Mostrando solo sus registros historicos".
- Mantener que el backend sea la fuente final de seguridad aunque la UI oculte campos.

## 4) Pruebas: agregar cobertura de no-regresion de seguridad

Backend:
- Test de autorizacion para endpoint historico por rol.
- Test de alcance:
  - usuario `ROL_FUNC` A no puede obtener registros de B, incluso enviando `funcionario=B`.
- Test RRHH global con filtros.

Frontend:
- Smoke test de panel SIFCNP por rol (funcionario sin filtro editable de funcionario).

## Archivos Exactos Probablemente Necesitando Cambios

Frontend:
- `app.js`
- `dashboard.html`

Backend API/Application/Infrastructure (nuevos o extendidos):
- `backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs` (o nuevo `HistoricoController.cs`)
- `backend/src/IntegradorMarcas.Application/Interfaces/IJustificacionService.cs`
- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`
- `backend/src/IntegradorMarcas.Application/Interfaces/IJustificacionRepository.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs`
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- `backend/src/IntegradorMarcas.Application/DTOs/` (nuevos DTOs de historico)
- `backend/src/IntegradorMarcas.Api/Contracts/Requests/` (si se modelan filtros tipados)
- `backend/src/IntegradorMarcas.Api/Contracts/Responses/` (response de historico)

Pruebas:
- `backend/tests/IntegradorMarcas.Tests/` (nuevos tests de scope historico)

Opcional documental/SQL:
- `docs/PRP_Justificacion_Marcas.md` (aclarar alcance por rol en RF-06 si esta ambiguo)
- `docs/db/` (si se formaliza script SQL de consulta historica o SP auxiliar)

## Pasos Recomendados de Validacion (Prueba de la Correccion)

1. Validacion manual funcional (roles)
- Login como `funcionaria ana` (ROL_FUNC).
- Abrir Consulta Historica.
- Confirmar que no existe filtro editable por funcionario (o que su valor no altera alcance).
- Ejecutar busqueda en rango amplio.
- Verificar que todos los resultados pertenecen solo a su cedula/usuario.

2. Prueba de seguridad por manipulacion de request
- Desde DevTools o REST client, invocar `GET /api/justificaciones/historico?funcionario=<otro>` con headers de Ana.
- Resultado esperado: el backend ignora o bloquea filtro de tercero, y solo retorna registros de Ana.

3. Prueba cruzada RRHH
- Login como `ROL_RRHH`.
- Ejecutar filtros globales por funcionario/compania/fechas.
- Verificar que RRHH si puede consultar multiples funcionarios.

4. Prueba de no-regresion RF-05
- `GET /api/justificaciones/mias` para funcionario sigue retornando solo propias.
- Confirmar que cambios de historico no afectan historial operativo existente.

5. Prueba automatizada
- Agregar tests de integración para endpoint historico:
  - `ROL_FUNC` own-scope enforced.
  - `ROL_FUNC` cannot escalate scope by query tampering.
  - `ROL_RRHH` global scope allowed.
- Ejecutar suite de tests y evidenciar verde.

## Criterio de Aceptacion de Fix

- Un `ROL_FUNC` nunca puede ver historicos de otro funcionario en RF-06, ni por UI ni alterando querystring manualmente.
- El scope se garantiza en backend (no solo en frontend).
- RRHH mantiene visibilidad global segun RF-04.
- Sin regresiones en RF-05 (historial propio operativo).
