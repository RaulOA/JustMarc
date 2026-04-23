# Frontend: Modulos y Flujos

## 1. Proposito
Documentar la estructura funcional del frontend actual en HTML/CSS/JS, sus modulos de app.js, eventos UI y flujo por rol.

## 2. Alcance
- index.html (login)
- dashboard.html (tabs y paneles)
- app.js (logica de sesion, API, render y acciones)
- style.css (sistema de estilos)

## 3. Fuente de verdad
- index.html
- dashboard.html
- app.js
- style.css

## 4. Modulos funcionales en app.js

### 4.1 Sesion e identidad
Funciones:
- getSession, setSession, handleLogin, requireAuth, handleLogout.
- inferRole e inferCompany.
- resolveUserIdentity + MOCK_USER_DIRECTORY.

Comportamiento:
- Guarda sesion en sessionStorage con clave sjm_session.
- Infere rol por nombre de usuario cuando no hay mapeo directo.
- Identity final para API se envia en headers X-User-Id y X-User-Role.

### 4.2 Configuracion API
Funciones:
- getApiBaseUrl, buildApiUrl, buildApiHeaders, apiFetch, parseApiError.

Comportamiento:
- Base URL por defecto: http://localhost:5093.
- Timeout de request: 12000 ms.
- Maneja errores de red, timeout y HTTP con mensajes amigables.
- En errores de red/servidor muestra toast y correlationId cuando existe.

### 4.3 UI y navegacion por rol
Funciones:
- configureRoleUI, switchTab, initDashboardPage.

Comportamiento:
- Tabs visibles por rol:
  - ROL_FUNC: panel-funcionario + panel-sifcnp.
  - ROL_JEFE: panel-jefatura + panel-sifcnp.
  - ROL_RRHH: panel-rrhh + panel-sifcnp.
- Persistencia de tab activa con sjm_activeTab.

### 4.4 Funcionario (creacion e historial)
Funciones:
- addDetailLine, removeDetailLine, renderDraftDetails.
- registerJustification.
- renderFuncionarioHistory.

Comportamiento:
- Exige motivo general y al menos una linea de detalle.
- Mapea tipos de justificacion de UI a IDs de API.
- Consume:
  - POST /api/justificaciones.
  - GET /api/justificaciones/mias.

### 4.5 Jefatura (pendientes, detalle y resolucion)
Funciones:
- renderJefaturaRequests.
- toggleDetail.
- approveRequest.

Comportamiento:
- Consume:
  - GET /api/jefatura/justificaciones/pendientes.
  - GET /api/jefatura/justificaciones/{id}.
  - PATCH /api/jefatura/justificaciones/{id}/resolver.
- Usa cache local de detalle con Map (jefaturaDetailCache).

### 4.6 RRHH (consulta global)
Funciones:
- buildRrhhQueryString.
- renderRRHHTable.
- applyRRHHFilter, resetRRHHFilter.

Comportamiento:
- Consume GET /api/rrhh/justificaciones con filtros.
- Incluye filtros por funcionario, estado, compania y rango de fechas.

### 4.7 Utilidades
Funciones:
- toast/showNotice.
- formatDate/formatDateTime.
- escapeHtml.
- renderStatusBadge.
- sifcnpSearch.
- downloadReport (simulado en UI).

## 5. Eventos UI principales
- Login:
  - Click boton Ingresar -> handleLogin.
  - Enter en teclado -> handleLogin.
- Navegacion:
  - Click en cada tab -> switchTab(tabId).
- Funcionario:
  - Agregar linea -> addDetailLine.
  - Eliminar linea -> removeDetailLine(index).
  - Registrar boleta -> registerJustification.
- Jefatura:
  - Aprobar/Rechazar -> approveRequest(id, accion).
  - Ver detalle -> toggleDetail.
- RRHH:
  - Aplicar filtros -> applyRRHHFilter.
  - Limpiar filtros -> resetRRHHFilter.

## 6. Flujo por rol (resumen)

### 6.1 Funcionario
1. Login simulado.
2. Completa motivo + lineas.
3. Envia boleta a API.
4. Consulta historial propio.

### 6.2 Jefatura
1. Login como jefatura.
2. Visualiza pendientes de subordinados.
3. Consulta detalle completo.
4. Resuelve aprobando o rechazando.

### 6.3 RRHH
1. Login como RRHH.
2. Consulta global de boletas.
3. Filtra por estado, funcionario, compania y fechas.

## 7. Casos de error o limites
- Si faltan headers o rol invalido en backend: error 401/403 mostrado en toast.
- Si API no responde: timeout 408 cliente con mensaje de conexion.
- Reporte en RRHH es simulado (downloadReport no consume backend).
- Consulta historica SIFCNP en frontend es visual y no consume endpoint dedicado.

## 8. Checklist de validacion
- Funciones listadas existen en app.js.
- Eventos descritos existen en index.html/dashboard.html.
- Endpoints usados coinciden con API real.
- Restricciones por rol coinciden con configureRoleUI y backend.

## 9. Historial de cambios
- 2026-04-23: Documento creado y validado contra frontend actual.
