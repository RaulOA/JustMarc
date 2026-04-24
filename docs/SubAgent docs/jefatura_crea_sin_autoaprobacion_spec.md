# Spec: Jefatura crea justificación sin auto-visualización/auto-aprobación

## Objetivo
Permitir que usuarios con rol `ROL_JEFE` puedan crear boletas de justificación, pero **sin poder ver ni resolver sus propias boletas**. Las boletas creadas por jefatura deben quedar visibles y resolubles únicamente por su superior jerárquico vigente o por delegados vigentes del superior.

## Contexto actual (hallazgos)

### Frontend
- En `app.js`, `registerJustification()` bloquea explícitamente si el rol no es `ROL_FUNC`.
- En `app.js`, `configureRoleUI()` habilita tabs por rol con esta matriz:
  - `ROL_FUNC`: `panel-funcionario`, `panel-sifcnp`
  - `ROL_JEFE`: `panel-jefatura`, `panel-sifcnp`
  - `ROL_RRHH`: `panel-rrhh`, `panel-sifcnp`
- En `dashboard.html`, el formulario de creación está únicamente dentro de `panel-funcionario`.
- `renderFuncionarioHistory()` también está restringido a `ROL_FUNC` (esto actualmente evita que jefatura vea su historial propio desde esa vista).

### Backend
- `JustificacionesController` (`POST /api/justificaciones`) delega en `JustificacionService.CreateAsync(...)`.
- `JustificacionService.CreateAsync(...)` hoy solo permite `RolesSistema.EsFuncionario(user.Role)`.
- Flujos de jefatura (`pendientes`, `detalle`, `resolver`) usan alcance de aprobación:
  - `ListPendientesJefaturaAsync` -> `ListPendientesJefatura` SQL.
  - `GetDetalleJefaturaAsync` -> validación `GetAprobacionScopeValidation` + query detalle.
  - `ResolverAsync` -> validación `GetResolverValidation` + `ResolverPendiente`.
- El alcance se basa en `fn_AprobadoresVigentesPorSolicitante(je.UsuarioID, GETDATE())`, que devuelve aprobadores por:
  - Jerarquía vigente (`Origen='Jerarquia'`).
  - Delegación vigente del jerarca (`Origen='Delegacion'`, con `DeleganteUsuarioId`).

## 1) Cambios frontend para que `ROL_JEFE` pueda crear justificación

### 1.1 Habilitación de panel de creación para jefatura
**Archivo:** `app.js` (`configureRoleUI`)
- Cambiar `allowedByRole.ROL_JEFE` para incluir `panel-funcionario` además de `panel-jefatura` y `panel-sifcnp`.
- Resultado esperado: jefatura ve tab de creación, pero conserva tab de pendientes para aprobar terceros.

Propuesta:
- Antes: `ROL_JEFE: ['panel-jefatura', 'panel-sifcnp']`
- Después: `ROL_JEFE: ['panel-funcionario', 'panel-jefatura', 'panel-sifcnp']`

### 1.2 Permitir envío de boleta por `ROL_JEFE`
**Archivo:** `app.js` (`registerJustification`)
- Sustituir validación rígida:
  - Antes: solo `ROL_FUNC`.
  - Después: permitir `ROL_FUNC` **o** `ROL_JEFE`.
- Mensaje de error actualizado: “Solo Funcionario o Jefatura pueden registrar boletas.”

### 1.3 Mantener restricción de historial personal
**Archivo:** `app.js` (`renderFuncionarioHistory`)
- Mantener la restricción a `ROL_FUNC` para no abrir una vista “mis boletas” a jefatura.
- Opcional UX: para `ROL_JEFE`, mostrar texto explícito en la tabla: “Jefatura puede crear boletas, pero no visualizarlas en su propio historial.”

### 1.4 Etiqueta de UI del panel de creación (opcional recomendado)
**Archivo:** `dashboard.html`
- Ajustar copy en `panel-funcionario` para un texto neutral:
  - Ejemplo título: “Registro de Justificación”.
  - Subtexto: “Disponible para Funcionario y Jefatura según reglas de aprobación.”
- No se requiere mover markup; basta con habilitar tab para jefatura y mantener mismas acciones.

## 2) Cambios backend para impedir auto-visualización y auto-aprobación (pending/detail/resolver)

Aunque el alcance actual depende de `fn_AprobadoresVigentesPorSolicitante`, se recomienda reforzar explícitamente la regla de negocio de no auto-gestión para evitar regresiones por cambios de datos jerárquicos.

### 2.1 Permitir creación por jefatura
**Archivo:** `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`
- En `CreateAsync(...)`, cambiar validación de rol:
  - Antes: solo `EsFuncionario`.
  - Después: `EsFuncionario || EsJefatura`.
- Mensaje sugerido: “Solo funcionario o jefatura pueden crear boletas.”

### 2.2 Bloqueo explícito en listado pendientes jefatura
**Archivo:** `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- En `ListPendientesJefatura`, agregar condición:
  - `AND je.UsuarioID <> @AprobadorUsuarioID`
- Impacto: aun si por configuración errónea alguien se vuelve su propio aprobador vigente, no verá su propia boleta en pendientes.

### 2.3 Bloqueo explícito en detalle jefatura
**Archivo:** `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- En `GetDetalleJefaturaEncabezado`, agregar:
  - `AND je.UsuarioID <> @AprobadorUsuarioID`

**Archivo relacionado:** `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- En `GetAprobacionScopeValidation`, forzar `IsInApprovalScope = 0` cuando `je.UsuarioID = @AprobadorUsuarioID`.
- Sugerencia técnica: en el `CASE` de `IsInApprovalScope`, añadir predicado `je.UsuarioID <> @AprobadorUsuarioID`.

### 2.4 Bloqueo explícito en resolver
**Archivos:**
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`

Cambios:
- `GetResolverValidation`: mismo endurecimiento que `GetAprobacionScopeValidation` para que auto-solicitudes no entren en scope.
- `ResolverPendiente`: agregar `AND je.UsuarioID <> @AprobadorUsuarioID` en `WHERE`.
- `ResolverAsync` (servicio): opcional recomendada una guard clause temprana si se expone `SolicitanteUsuarioId` en validación, para retornar 403 con mensaje funcional claro.

### 2.5 Mensajería y códigos HTTP
Mantener respuestas:
- `403` para boleta fuera de alcance o auto-intento.
- `404` para boleta inexistente.
- `409` para boleta ya resuelta.

Mensaje sugerido para auto-intento:
- “No puede visualizar ni resolver boletas creadas por su propio usuario.”

## 3) Garantía de visibilidad/aprobación por superior o delegados vigentes

### 3.1 Base actual correcta
El diseño actual ya usa `fn_AprobadoresVigentesPorSolicitante(solicitante, fecha)` en:
- `ListPendientesJefatura`
- `GetDetalleJefaturaEncabezado`
- `GetResolverValidation` / `GetAprobacionScopeValidation`
- `ResolverPendiente`

Esto garantiza que el aprobador autorizado sea:
- El aprobador por jerarquía vigente del solicitante.
- O delegado vigente del aprobador jerárquico (cuando aplica), con trazabilidad por `ScopeSource` y `DeleganteUsuarioId`.

### 3.2 Endurecimiento para requisito nuevo
Para cumplir “solo superior o delegado si superior no está”, se recomienda:
- Conservar la función como fuente única de alcance.
- Aplicar el filtro explícito anti auto-aprobación (`je.UsuarioID <> @AprobadorUsuarioID`) en todas las consultas de jefatura.
- Mantener la prioridad de delegación ya presente en validaciones (`ORDER BY` favorece `Delegacion` en scopeData) para trazabilidad de auditoría.

### 3.3 Dependencia de datos maestros
La garantía operativa depende de vigencias y estados correctos en:
- `Operacion.JerarquiaAprobacion`
- `Operacion.DelegacionAprobacion`
- `RecursosHumanos.EstructuraOrganizacional`

Recomendación operativa:
- Ejecutar/controlar script de fix de función (`docs/db/fix_fn_aprobadores.sql`) en ambientes donde exista desalineación de esquema/owner (`dbo` vs `Operacion`) o vigencias nulas.

## 4) Riesgos de regresión y pruebas de aceptación concretas

## Riesgos
1. Regresión de autorización de creación:
- Al habilitar jefatura para crear, podría abrirse creación para roles no previstos si la condición queda demasiado amplia.

2. Regresión de flujo jefatura de terceros:
- Filtro anti auto podría bloquear incorrectamente boletas de subordinados si se usa parámetro equivocado.

3. Desalineación SQL Function owner/schema:
- En algunos scripts aparece `dbo.fn_AprobadoresVigentesPorSolicitante`; en backend se invoca `dbo.fn_...` también.
- Si en una base la función está solo en `Operacion`, puede fallar alcance.

4. UX confusa en panel de creación:
- Jefatura crea boleta pero no la ve en historial propio; requiere texto explícito para evitar tickets.

5. Cobertura de pruebas insuficiente (actualmente mínima):
- Sin tests automatizados, cambios de autorización pueden romperse en futuras iteraciones.

## Pruebas de aceptación

### A. Creación por jefatura
1. Login como `ROL_JEFE`.
2. Verificar que aparece tab/panel de registro de justificación.
3. Crear boleta válida con 1+ detalle.
4. Esperado: `201 Created`, estado inicial “Pendiente Jefatura”.

### B. No auto-visualización en pendientes
1. Con usuario `U_JEFE` crear boleta propia.
2. Consultar `GET /api/jefatura/justificaciones/pendientes` como `U_JEFE`.
3. Esperado: la boleta creada por `U_JEFE` **no** aparece.

### C. No auto-visualización en detalle
1. Intentar `GET /api/jefatura/justificaciones/{idPropio}` como `U_JEFE`.
2. Esperado: `403` (o `404` según política), nunca detalle exitoso.

### D. No auto-aprobación
1. Intentar `PATCH /api/jefatura/justificaciones/{idPropio}/resolver` como `U_JEFE`.
2. Esperado: `403`; estado en BD permanece `Pendiente Jefatura`.

### E. Superior directo sí puede ver/aprobar
1. Definir jerarquía vigente donde `U_SUPERIOR` es aprobador de estructura de `U_JEFE`.
2. Crear boleta con `U_JEFE`.
3. `U_SUPERIOR` lista pendientes, abre detalle y aprueba.
4. Esperado: visible, resoluble, estado final `Aprobado`, `AprobadorId=U_SUPERIOR`.

### F. Delegado vigente del superior sí puede ver/aprobar
1. Crear delegación activa `U_SUPERIOR -> U_DELEGADO` (vigente en fecha actual).
2. Crear boleta con `U_JEFE`.
3. `U_DELEGADO` lista pendientes, abre detalle y resuelve.
4. Esperado: permitido; en auditoría/validación `ScopeSource='Delegacion'` y `DeleganteUsuarioId=U_SUPERIOR`.

### G. Delegación vencida o inactiva no otorga alcance
1. Vencer o inactivar delegación.
2. Repetir consulta/resolución con `U_DELEGADO`.
3. Esperado: `403` por fuera de alcance.

## 5) Lista de archivos a modificar

### Frontend
- `app.js`
  - `configureRoleUI`
  - `registerJustification`
  - (opcional UX) `renderFuncionarioHistory` mensaje contextual para jefatura
- `dashboard.html`
  - (opcional UX) textos de encabezado del panel de creación

### Backend API/Application
- `backend/src/IntegradorMarcas.Application/Services/JustificacionService.cs`
  - `CreateAsync` (habilitar `ROL_JEFE`)
  - (opcional) mensaje específico auto-intento en detalle/resolver

### Backend Infrastructure SQL
- `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs`
  - `ListPendientesJefatura` (filtro anti auto)
  - `GetDetalleJefaturaEncabezado` (filtro anti auto)
  - `GetAprobacionScopeValidation` (anti auto en `IsInApprovalScope`)
  - `GetResolverValidation` (anti auto en `IsInApprovalScope`)
  - `ResolverPendiente` (filtro anti auto)

### Pruebas (nuevas)
- `backend/tests/IntegradorMarcas.Tests/...` (crear suite nueva)
  - tests de autorización de creación
  - tests de scope anti auto en pendientes/detalle/resolver
  - tests de flujo superior/delegado vigente

### SQL/operación (si aplica por ambiente)
- `docs/db/fix_fn_aprobadores.sql` (alineación de función en ambientes con desajuste)

## Notas de implementación recomendadas
- Mantener la función de aprobadores vigentes como única fuente de verdad del alcance.
- El filtro explícito `je.UsuarioID <> @AprobadorUsuarioID` debe repetirse en **todas** las rutas de lectura/resolución de jefatura para defensa en profundidad.
- No habilitar endpoint nuevo; reutilizar `POST /api/justificaciones` con autorización ampliada en servicio.
- Mantener historial “mías” solo para funcionario para cumplir “jefatura no podrá visualizarla” en UI estándar.
