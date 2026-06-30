# Design - delegaciones-subaprobadores-reglas-completas (F-004)

## Punto de partida (base existente — NO reinventar)

F-004 extiende, no reemplaza. La base verificada en código es:

- **Tabla** `Operacion.DelegacionAprobacion` (`docs/db/02_EstructuraCompleta.sql`, ~L295):
  `DelegacionAprobacionId, DeleganteUsuarioId, DelegadoUsuarioId, JerarquiaAprobacionId (NULL=todas),
  Motivo, EstadoRegistroId, VigenciaDesde, VigenciaHasta, CreadoPor, FechaHoraCreacion`.
  **Carece de `ModificadoPor` / `FechaHoraModificacion`** (auditoría de modificación, ver Decisión D5).
- **TVF** `dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioId, @FechaRef)`
  (`02_EstructuraCompleta.sql`, ~L517): une `JerarquiasActivas` y `DelegacionesActivas`. La delegación
  solo cuenta si su delegante es aprobador por jerarquía del solicitante y está vigente
  (`VigenciaDesde <= FechaRef <= VigenciaHasta`, estado=1). Devuelve `AprobadorUsuarioId, Origen,
  DeleganteUsuarioId`. **Ya implementa R2, R3, R5** a nivel SQL; F-004 los blinda con tests y los
  reutiliza.
- **Prioridad `Origen='Delegacion'`** (F-003): en `JustificacionesSql.cs` las consultas
  `GetResolverValidation`, `GetAprobacionScopeValidation`, `GetCurrentApproverBySolicitante` ordenan
  `CASE WHEN fa.Origen='Delegacion' THEN 0 ELSE 1 END`.
- **Servicio** `AdminAprobacionesService` (`Application/Services`): CRUD + toggle de jerarquías y
  delegaciones, con guard `EnsureAdmin` (R7 ya cubierto a nivel rol), validaciones de vigencia
  (R21 ya cubierto) y auditoría doble (resumen + detalle; R22 parcialmente cubierto).
- **Resolución** `JustificacionService.ResolverAsync` / `GetDetalleJefaturaAsync`: validan alcance vía
  `GetResolverValidation` / `GetAprobacionScopeValidation`, que ya excluyen `je.UsuarioID =
  @AprobadorUsuarioID` (**R9 ya cubierto** para auto-aprobación).
- **Frontend** `app.js`: `loadAdminDelegaciones`, `saveAdminDelegacion` (POST),
  `deleteAdminDelegacion` que llama `DELETE /api/admin/aprobaciones/delegaciones/{id}` —
  **endpoint inexistente hoy** (misalignment que R19 cierra).

## Enfoque

Tres frentes, todos sobre la base anterior:

1. **Reglas de negocio en la capa de servicio** (Application), no en SQL ad-hoc, salvo la lógica de
   alcance que ya vive en la TVF y se reutiliza.
2. **Alineación backend↔UI del borrado** (R19/R20): nuevo endpoint `DELETE` + método de servicio +
   SQL.
3. **Nuevas vistas del delegado** (R11/R12 función; R16–R18 registro de solo lectura): endpoints de
   consulta para `ROL_JEFE` que actúa como delegado.

### Frente A — Restricciones de aprobación del delegado (R8, R9, R10, R13)

`R9` ya está cubierto por `je.UsuarioID <> @AprobadorUsuarioID`. Faltan:

- **R8 (no aprobar al titular):** en `ResolverAsync`, tras obtener `GetResolverValidation`, si
  `validation.ScopeSource == "Delegacion"` y `validation.DeleganteUsuarioId == je.UsuarioID`
  (solicitante), rechazar 403. La validación ya devuelve `ScopeSource` y `DeleganteUsuarioId`; basta
  exponer el `SolicitanteUsuarioId` en `ResolverValidationDto` o comparar en SQL.
- **R10 (fuera de rango del titular):** la TVF ya restringe el alcance del delegado al del titular
  (la delegación solo aparece si el delegante es aprobador por jerarquía del solicitante). Por tanto,
  cuando el solicitante cae fuera del rango del titular, la fila `Delegacion` **no existe** en la TVF
  y `IsInApprovalScope=false` → 403. F-004 lo **verifica con test** y documenta que es la TVF quien lo
  garantiza (no se duplica lógica).
- **R13 (expiración con pendientes):** la TVF filtra por `VigenciaHasta`. Una justificación que quedó
  pendiente y dirigida al delegado deja de estar en su alcance al pasar `VigenciaHasta` →
  `ResolverAsync` devuelve 403. F-004 lo verifica con test parametrizado por fecha.

### Frente B — Anti-sub-delegación (R6)

En `AdminAprobacionesService.EnsureReferencesForDelegacionAsync`, antes de crear/actualizar, consultar
si el `DeleganteUsuarioId` propuesto es a su vez **delegado activo y vigente** de otra delegación. Si
lo es → 409. Nuevo método de repositorio `ExistsDelegacionActivaComoDelegadoAsync(int usuarioId,
DateTime fechaRef, int? delegacionIdExcluida, CancellationToken)` + SQL en `AdminAprobacionesSql`.

### Frente C — Borrado alineado (R19, R20)

- `AdminAprobacionesController`: nuevo `[HttpDelete("delegaciones/{delegacionAprobacionId:int}")]`.
- `IAdminAprobacionesService.DeleteDelegacionAsync(user, id, correlationId, ct)` + impl con
  `EnsureAdmin`, lectura previa para auditoría, borrado, auditoría doble.
- `IAdminAprobacionesRepository.DeleteDelegacionAsync(int id, CancellationToken)` + SQL
  `DeleteDelegacion`.
- Ver Decisión D1: borrado físico vs. lógico.

### Frente D — Vistas del delegado (R11, R12, R16, R17, R18)

- **Función (R11/R12):** `GET /api/delegaciones/mi-funcion` (rol `ROL_JEFE`). Devuelve, por delegación
  activa/vigente recibida: titular (nombre), vigencia, y alcance de estructuras
  (de `JerarquiaAprobacion` del titular o de la jerarquía referenciada por la delegación).
- **Registro solo lectura (R16/R17/R18):** `GET /api/delegaciones/mi-registro` (rol `ROL_JEFE`).
  Lista justificaciones cuyo `AprobadorId = delegado` resueltas dentro del período de delegación
  (R16/R17). No existe endpoint de escritura para este registro (R18 se verifica por ausencia de
  ruta de mutación + test que confirma que el registro es de solo consulta).

### Frente E — Soberanía del titular (R14, R15)

- **R14:** las consultas de jefatura del titular ya listan por alcance (`fn_Aprobadores...`). Verificar
  que una justificación ya resuelta por el delegado siga visible al titular (incluir estados resueltos
  en `ListHistoricoAsync` con `aprobadorUsuarioIdScope`). Test de visibilidad.
- **R15:** permitir re-resolución por el titular. Hoy `ResolverAsync` exige
  `EstadoId == PendienteJefatura` (RN-04). Ver Decisión D2: cómo habilitar la modificación del titular
  sin romper RN-04 para el resto.

## Archivos y firmas

### Backend — Application

- `Application/Interfaces/IAdminAprobacionesService.cs`
  `+ Task DeleteDelegacionAsync(UserContextInfo user, int delegacionAprobacionId, string? correlationId, CancellationToken ct);`
- `Application/Interfaces/IAdminAprobacionesRepository.cs`
  `+ Task<int> DeleteDelegacionAsync(int delegacionAprobacionId, CancellationToken ct);`
  `+ Task<bool> ExistsDelegacionActivaComoDelegadoAsync(int usuarioId, DateTime fechaRef, int? delegacionIdExcluida, CancellationToken ct);`
- `Application/Services/AdminAprobacionesService.cs`
  - `DeleteDelegacionAsync(...)` (R19, R20).
  - En `EnsureReferencesForDelegacionAsync(...)`: agregar guard anti-sub-delegación (R6).
- `Application/Interfaces/IDelegacionConsultaService.cs` (**nuevo**)
  `Task<IReadOnlyList<DelegacionFuncionDto>> GetMiFuncionAsync(UserContextInfo user, CancellationToken ct);`
  `Task<IReadOnlyList<DelegacionRegistroDto>> GetMiRegistroAsync(UserContextInfo user, FiltroJustificacionesDto filtros, CancellationToken ct);`
- `Application/Services/DelegacionConsultaService.cs` (**nuevo**) — guard `EsJefatura`.
- `Application/Services/JustificacionService.cs`
  - `ResolverAsync`: guard R8 (delegado vs. titular). R9/R10/R13 ya cubiertos por TVF/SQL → tests.
  - R15: re-resolución del titular (según Decisión D2).
- `Application/DTOs/` (**nuevos**): `DelegacionFuncionDto`, `DelegacionRegistroDto`.
- `Application/Interfaces/IDelegacionConsultaRepository.cs` + impl en Infrastructure (**nuevos**) o
  reutilizar `IAdminAprobacionesRepository` / `IJustificacionRepository` (ver Decisión D3).

### Backend — Infrastructure

- `Infrastructure/Queries/AdminAprobacionesSql.cs`
  `+ DeleteDelegacion`, `+ ExistsDelegacionActivaComoDelegado`.
- `Infrastructure/Queries/DelegacionConsultaSql.cs` (**nuevo**): `MiFuncion`, `MiRegistro`.
- `Infrastructure/Repositories/AdminAprobacionesRepository.cs`: impl de los nuevos métodos.
- `Infrastructure/Repositories/DelegacionConsultaRepository.cs` (**nuevo**).
- DI en `Program.cs` / `DependencyInjection`: registrar los servicios/repositorios nuevos.

### Backend — Api

- `Api/Controllers/AdminAprobacionesController.cs`
  `+ [HttpDelete("delegaciones/{delegacionAprobacionId:int}")] DeleteDelegacion(...)` (R19).
- `Api/Controllers/DelegacionesController.cs` (**nuevo**, ruta `api/delegaciones`):
  `GET mi-funcion` (R11/R12), `GET mi-registro` (R16–R18).
- `Api/Contracts/Responses/`: `DelegacionFuncionResponse`, `DelegacionRegistroResponse` (**nuevos**).

### Base de datos

- `docs/db/02_EstructuraCompleta.sql`: según Decisión D5, agregar `ModificadoPor` /
  `FechaHoraModificacion` a `Operacion.DelegacionAprobacion` (idempotente, `IF COL_LENGTH ... IS
  NULL`). Si se descarta, dejar nota.
- Reutilizar `dbo.fn_AprobadoresVigentesPorSolicitante` tal cual (R2/R3/R5/R10/R13). Solo tocar la TVF
  si una decisión lo exige.

### Frontend

- `app.js`: `deleteAdminDelegacion` ya espera `DELETE` → queda alineado al crear R19. Agregar campos
  faltantes en el formulario admin (Motivo, JerarquiaAprobacionId) para consistencia (R7/UI).
  Vistas del delegado (mi-funcion / mi-registro) en el dashboard de jefatura: nuevas funciones
  `loadMiFuncionDelegacion`, `loadMiRegistroDelegado` usando `apiFetch` + `escapeHtml` + `formatDate`.

## Errores/excepciones

- Todas las violaciones lanzan `AppException` con `StatusCode` (403 reglas de permiso; 409
  sub-delegación; 400 vigencia/datos; 404 no existe). Nunca excepción cruda.
- Auditoría best-effort vía `IAuditEventRepository` + `IAdminActionAuditRepository` (mismo patrón que
  el servicio actual).

## Decisiones abiertas para la compuerta humana

### D1 — Borrado de delegación: físico vs. lógico
- **A. Borrado físico (`DELETE FROM`).** Pro: simple; coincide con lo que el frontend ya espera
  ("Eliminar"). Contra: pierde el histórico de la delegación; tensa RNF-05 (trazabilidad).
- **B. Borrado lógico (estado→Inactivo).** Pro: conserva histórico; ya existe `ToggleDelegacionEstado`.
  Contra: el botón "Eliminar" del frontend dejaría de "eliminar" de verdad (semántica confusa); R19
  habla de un `DELETE` que el frontend invoca.
- **Recomendación del autor:** A con auditoría previa (se serializan los valores anteriores en
  `AdminAccionAuditoria` antes de borrar), preservando trazabilidad sin duplicar el dato.

### D2 — Re-resolución del titular sobre lo resuelto por su delegado (R15)
- **A. Excepción a RN-04 acotada al titular.** Permitir que `ResolverAsync` modifique una justificación
  ya resuelta solo si el actor es el **titular/jefatura por jerarquía** y el resolutor previo fue un
  **delegado suyo**. Pro: cumple regla 8 literal. Contra: introduce una ruta de modificación sobre
  estados "finales"; hay que auditar fuerte.
- **B. Endpoint dedicado** `PATCH /justificaciones/{id}/revisar-titular`. Pro: no toca la semántica de
  `ResolverAsync`; separa el caso especial. Contra: superficie nueva.
- **C. Solo lectura para el titular (sin re-resolver).** Cumple "ver" pero **no** "modificar" de la
  regla 8. Contra: incumple la regla tal como está escrita.
- **Recomendación:** B (endpoint dedicado y auditado), por aislamiento del riesgo.

### D3 — Capa de datos de las vistas del delegado
- **A. Nuevo `IDelegacionConsultaRepository`.** Pro: cohesión, no infla interfaces existentes.
- **B. Extender `IAdminAprobacionesRepository` / `IJustificacionRepository`.** Pro: menos archivos.
  Contra: mezcla responsabilidades admin con vistas de jefatura.
- **Recomendación:** A.

### D4 — Alcance del registro de solo lectura (R17): ¿por fecha de resolución o por vigencia?
- **A. Filtrar por `FechaAprobacion` dentro de `[VigenciaDesde, VigenciaHasta]`.** Pro: refleja "lo que
  efectivamente tramitó en su período". Contra: deja fuera lo delegado pero no tramitado.
- **B. Filtrar por solapamiento de la vigencia con la vida de la justificación.** Pro: muestra todo lo
  que "le tocó". Contra: incluye no tramitadas (puede ser deseable como evidencia).
- **Recomendación:** A (la regla 9 dice "las justificaciones que se les haya delegado en los tiempos
  del permiso"; A es la lectura literal sobre lo tramitado). Pendiente de confirmación del humano.

### D5 — Columnas de auditoría de modificación en `DelegacionAprobacion`
La tabla no tiene `ModificadoPor`/`FechaHoraModificacion` (las convenciones las exigen; herencia
previa también señalada para `JerarquiaAprobacion` en `findings.md`).
- **A. Agregarlas ahora** (idempotente) y poblarlas en update/toggle/delete. Pro: cumple convención y
  RNF-05. Contra: amplía el alcance de F-004 hacia BD.
- **B. Dejarlo fuera** y enrutar a backlog. Pro: mantiene F-004 enfocada. Contra: persiste la deuda.
- **Recomendación:** A si el humano acepta el toque de esquema; si no, B con nota en backlog.

## Decisiones resueltas (compuerta humana — 2026-06-29)

El humano aceptó las recomendaciones del autor. Quedan fijadas así para la implementación:

- **D1 → A:** borrado **físico** (`DELETE FROM`) con auditoría previa (serializar valores anteriores en
  `AdminAccionAuditoria` antes de borrar).
- **D2 → B:** **endpoint dedicado** y auditado para la re-resolución del titular
  (`PATCH /justificaciones/{id}/revisar-titular`); no se toca la semántica de `ResolverAsync`.
- **D3 → A:** **nuevo** `IDelegacionConsultaRepository` (no se infla `IAdminAprobacionesRepository`).
- **D4 → A:** registro de solo lectura filtrado por `FechaAprobacion` dentro de
  `[VigenciaDesde, VigenciaHasta]` (lo efectivamente tramitado en el período).
- **D5 → A:** **agregar** `ModificadoPor` / `FechaHoraModificacion` a `Operacion.DelegacionAprobacion`
  (migración idempotente) y poblarlas en update/toggle/delete. Cierra la deuda para esta tabla; la de
  `JerarquiaAprobacion` (en `findings.md`) sigue como ítem aparte.

## Alternativas descartadas

- **Mover toda la lógica de reglas a SQL/TVF nuevas** — por qué no: rompe la arquitectura (la
  autorización vive como guard clause en Application, no en BD); duplicaría reglas y dificultaría el
  test unitario sin BD (gate `Category!=Integration`).
- **Crear una tabla nueva de "sub-aprobadores" aparte de `DelegacionAprobacion`** — por qué no: F-004
  explícitamente extiende la tabla existente; una segunda tabla duplicaría el modelo y la TVF.
- **Resolver R10 con una consulta de rango jerárquico nueva** — por qué no: la TVF ya garantiza que el
  alcance del delegado es el del titular; agregar otra consulta sería redundante y arriesgaría
  divergencia.
