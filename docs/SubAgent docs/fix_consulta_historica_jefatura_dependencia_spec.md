# Spec: Fix Consulta Historica para Jefatura (Dependencia sin registros propios)

## Objetivo

Ajustar el alcance de Consulta Historica para rol Jefatura de forma que:
- Jefatura vea justificaciones historicas de todos sus subordinados/dependencia.
- Jefatura no vea sus propios registros en este panel.
- Historial propio de jefatura se mantenga en Panel Funcionario.

## Hallazgos de Implementacion Actual

## 1) Endpoint historico usado por la UI
- Endpoint: GET /api/justificaciones/historico.
- Controlador: backend/src/IntegradorMarcas.Api/Controllers/JustificacionesController.cs
- La accion ListHistorico delega a JustificacionService.ListHistoricoAsync con filtros de funcionario/estado/compania/fecha.

## 2) Regla de alcance en servicio
- Servicio: backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs
- Implementacion actual de ListHistoricoAsync:
  - ROL_FUNC: fuerza scope propio (usuarioIdScope = user.UserId) e ignora filtro funcionario.
  - ROL_JEFE y ROL_RRHH: usa usuarioIdScope = null, por lo que NO restringe por usuario.
- Resultado: para ROL_JEFE el historico queda global, no acotado a dependencia.

## 3) Query/repositorio historico
- Repositorio: backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs
- SQL: backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs (ListHistorico)
- Condicion principal actual:
  - (@UsuarioID IS NULL OR u.UsuarioID = @UsuarioID)
- Como para ROL_JEFE hoy @UsuarioID va null, la consulta incluye todos los funcionarios del sistema.
- Tampoco existe exclusion explicita de registros propios para jefatura en ListHistorico.

## 4) Caller frontend de la vista historica
- Frontend: app.js, funcion renderSifcnpHistorico.
- UI: dashboard.html, panel-sifcnp.
- El frontend invoca siempre /api/justificaciones/historico.
- Para ROL_FUNC no envia parametro funcionario.
- Para ROL_JEFE si permite/enviar funcionario (texto libre), pero el control de seguridad real depende del backend.

## 5) Estado respecto al requerimiento

Requerimiento objetivo:
- Jefatura: ver todos subordinados/dependencia.
- Jefatura: excluir propios.

Comportamiento actual:
- Jefatura: ve global (mas que su dependencia).
- Jefatura: puede ver sus propios registros en historico.

Conclusion:
- No cumple requerimiento. Falta scope por dependencia y exclusion de propios para ROL_JEFE.

## Cambios Exactos Requeridos

## A) Capa Application: tipar el alcance de historico por rol

Archivo:
- backend/src/IntegradorMarcas.Application/Interfaces/IJustificacionRepository.cs
- backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs

Cambios:
1. Cambiar firma de ListHistoricoAsync en repositorio para soportar modo de alcance jefatura:
   - Opcion recomendada:
     - ListHistoricoAsync(int? usuarioId, int? aprobadorUsuarioId, bool excluirPropiosEnScopeAprobador, FiltroRrhhJustificacionesDto filtros, CancellationToken ct)
2. Actualizar servicio JustificacionService.ListHistoricoAsync:
   - ROL_FUNC:
     - usuarioId = user.UserId
     - aprobadorUsuarioId = null
     - excluirPropiosEnScopeAprobador = false
     - funcionario = null
   - ROL_RRHH:
     - usuarioId = null
     - aprobadorUsuarioId = null
     - excluirPropiosEnScopeAprobador = false
     - funcionario = filtros.Funcionario
   - ROL_JEFE:
     - usuarioId = null
     - aprobadorUsuarioId = user.UserId
     - excluirPropiosEnScopeAprobador = true
     - funcionario = filtros.Funcionario (opcional, como filtro adicional dentro del alcance)

Nota:
- Mantener validaciones de fecha/compania/texto ya existentes.

## B) Capa Infrastructure SQL: aplicar scope de dependencia para jefatura

Archivo:
- backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs (const ListHistorico)

Agregar parametros SQL:
- @AprobadorUsuarioID INT = NULL
- @ExcluirPropiosEnScopeAprobador BIT = 0

Agregar condicion de alcance jefatura (ademas de filtros existentes):

- Cuando @AprobadorUsuarioID IS NOT NULL:
  - Solo incluir registros cuyo solicitante este en alcance de aprobacion vigente del aprobador:
    - EXISTS (SELECT 1 FROM dbo.fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE()) fa WHERE fa.AprobadorUsuarioId = @AprobadorUsuarioID)
  - Y excluir propios cuando corresponda:
    - (@ExcluirPropiosEnScopeAprobador = 0 OR je.UsuarioID <> @AprobadorUsuarioID)

Comportamiento resultante:
- ROL_JEFE obtiene historico de su dependencia/subordinados segun alcance de aprobacion vigente (jerarquia/delegacion), sin incluir sus boletas.
- ROL_FUNC mantiene solo propio.
- ROL_RRHH mantiene global.

## C) Capa Repository: mapear nuevos parametros

Archivo:
- backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs

Cambios:
1. Actualizar firma y llamada a Dapper en ListHistoricoAsync para enviar:
   - UsuarioID
   - AprobadorUsuarioID
   - ExcluirPropiosEnScopeAprobador
   - filtros existentes
2. No cambiar el shape de salida del DTO RrhhJustificacionResumenDto.

## D) Frontend: mantener UX alineada al alcance real

Archivos:
- app.js
- dashboard.html

Cambios funcionales:
1. Mensaje de alcance para jefatura en configureSifcnpScopeUI:
   - Mostrar nota explicita para ROL_JEFE: "Mostrando registros historicos de su dependencia. Sus registros propios se consultan en Panel Funcionario."
2. Filtro funcionario en panel SIFCNP:
   - Puede mantenerse visible para jefatura como filtro adicional, pero debe entenderse que solo filtra dentro de su alcance.
3. No confiar en UI para seguridad.
   - El backend debe imponer el scope siempre.

## E) Pruebas requeridas

Archivos:
- backend/tests/IntegradorMarcas.Tests/JustificacionServiceHistoricoTests.cs
- (si existe suite de integracion API) agregar pruebas de endpoint historico.

Agregar/ajustar tests unitarios del servicio:
1. ListHistoricoAsync_RolJefatura_AplicaScopeAprobadorYExcluyePropios
   - Verificar que el servicio envía aprobadorUsuarioId = user.UserId.
   - Verificar que envía excluirPropiosEnScopeAprobador = true.
2. Mantener tests actuales:
   - ROL_FUNC fuerza scope propio e ignora funcionario.
   - ROL_RRHH mantiene filtro funcionario sin scope usuario.

Agregar tests de integracion (recomendado):
1. Jefatura no ve boletas fuera de su dependencia.
2. Jefatura no ve boletas propias en /api/justificaciones/historico.
3. RRHH ve global.
4. Funcionario ve solo propio.

## Riesgos y Consideraciones

1. Scope de dependencia depende de la funcion dbo.fn_AprobadoresVigentesPorSolicitante y de la vigencia de jerarquias/delegaciones.
2. Usar GETDATE() en SQL mantiene semantica igual a panel de pendientes/resolucion de jefatura.
3. Si negocio requiere dependencia estructural historica (no vigente) para fechas pasadas, se necesitara una regla distinta (fuera de este fix).

## Criterios de Aceptacion

1. Usuario ROL_JEFE en Consulta Historica solo ve boletas de personas dentro de su alcance de aprobacion.
2. Usuario ROL_JEFE no ve boletas donde je.UsuarioID = su propio usuario.
3. Usuario ROL_FUNC sigue viendo solo sus boletas.
4. Usuario ROL_RRHH mantiene visibilidad global con filtros.
5. Sin regresiones en endpoints de jefatura pendientes/detalle/resolucion.

## Archivo de salida

- docs/SubAgent docs/fix_consulta_historica_jefatura_dependencia_spec.md
