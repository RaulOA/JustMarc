/* ============================================================
   SIFCNP — Lógica de Aplicación (MVP PRP)
   ============================================================ */

const STORAGE_KEYS = {
  session: 'sjm_session',
  activeTab: 'sjm_activeTab',
  apiBaseUrl: 'sjm_api_base_url'
};

const ESTADOS = {
  PENDIENTE: 'Pendiente Jefatura',
  APROBADO: 'Aprobado',
  RECHAZADO: 'Rechazado'
};

const COMPANY_BY_PREFIX = {
  cnp: 'CNP',
  fanal: 'FANAL'
};

let draftDetails = [];
const jefaturaDetailCache = new Map();

const API_CONFIG = {
  defaultBaseUrl: 'http://localhost:5093',
  timeoutMs: 12000
};

const MOCK_USER_DIRECTORY = {
  'funcionario.ana': { userId: 10, role: 'ROL_FUNC' },
  'jefe.maria': { userId: 20, role: 'ROL_JEFE' },
  'rrhh.carlos': { userId: 30, role: 'ROL_RRHH' }
};

const JUSTIFICACION_TIPO_IDS = {
  'Marca Tardía': 1,
  'Omisión Marca de Entrada': 2,
  'Omisión Marca de Salida': 3,
  'Marca antes Hora de Salida': 4,
  Ausencia: 5
};

function inferRole(username) {
  const normalized = username.toLowerCase();
  if (normalized.includes('rrhh')) return 'ROL_RRHH';
  if (normalized.includes('jefe')) return 'ROL_JEFE';
  return 'ROL_FUNC';
}

function inferCompany(username) {
  const normalized = username.toLowerCase();
  if (normalized.startsWith('fanal') || normalized.includes('.fanal')) return COMPANY_BY_PREFIX.fanal;
  return COMPANY_BY_PREFIX.cnp;
}

function getSession() {
  const raw = sessionStorage.getItem(STORAGE_KEYS.session);
  if (!raw) return null;
  try {
    return JSON.parse(raw);
  } catch {
    return null;
  }
}

function setSession(session) {
  sessionStorage.setItem(STORAGE_KEYS.session, JSON.stringify(session));
}

function getApiBaseUrl() {
  const fromWindow = typeof window !== 'undefined' ? window.SJM_API_BASE_URL : '';
  const fromSession = sessionStorage.getItem(STORAGE_KEYS.apiBaseUrl) || '';
  const selected = String(fromWindow || fromSession || API_CONFIG.defaultBaseUrl).trim();
  return selected.replace(/\/$/, '');
}

function buildApiUrl(path) {
  return `${getApiBaseUrl()}${path}`;
}

function resolveUserIdentity(session) {
  if (!session?.username) return null;

  const normalized = session.username.toLowerCase();
  const directoryIdentity = MOCK_USER_DIRECTORY[normalized];
  if (directoryIdentity) {
    return {
      userId: directoryIdentity.userId,
      role: directoryIdentity.role
    };
  }

  return {
    userId: 10,
    role: session.role || inferRole(session.username)
  };
}

function buildApiHeaders(session, withJsonBody = false) {
  const identity = resolveUserIdentity(session);
  if (!identity?.userId || !identity?.role) {
    throw new Error('No fue posible resolver identidad para llamar la API.');
  }

  const headers = {
    'X-User-Id': String(identity.userId),
    'X-User-Role': identity.role
  };

  if (withJsonBody) {
    headers['Content-Type'] = 'application/json';
  }

  return headers;
}

async function parseApiError(response) {
  let parsed;

  try {
    parsed = await response.clone().json();
  } catch {
    parsed = null;
  }

  const message = parsed?.detail
    || parsed?.title
    || parsed?.message
    || `Error HTTP ${response.status}`;

  const error = new Error(message);
  error.status = response.status;
  error.payload = parsed;
  return error;
}

async function apiFetch(path, options = {}, session = getSession()) {
  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), API_CONFIG.timeoutMs);

  try {
    const response = await fetch(buildApiUrl(path), {
      ...options,
      headers: {
        ...(options.headers || {})
      },
      signal: controller.signal
    });

    if (!response.ok) {
      throw await parseApiError(response);
    }

    if (response.status === 204) {
      return null;
    }

    const contentType = response.headers.get('content-type') || '';
    if (contentType.includes('application/json')) {
      return response.json();
    }

    return null;
  } catch (error) {
    if (error?.name === 'AbortError') {
      const timeoutError = new Error('La API tardó demasiado en responder. Intente de nuevo.');
      timeoutError.status = 408;
      throw timeoutError;
    }

    if (error instanceof TypeError) {
      const networkError = new Error('No fue posible conectar con la API. Verifique backend, URL y CORS.');
      networkError.status = 0;
      throw networkError;
    }

    throw error;
  } finally {
    clearTimeout(timer);
  }
}

function mapEstadoDescripcion(estadoId, estadoDescripcion) {
  const desc = String(estadoDescripcion || '').toLowerCase();
  if (estadoId === 2 || desc.includes('aprobad')) return ESTADOS.APROBADO;
  if (estadoId === 3 || desc.includes('rechazad')) return ESTADOS.RECHAZADO;
  return ESTADOS.PENDIENTE;
}

function presentBoletaId(justificacionId) {
  const numeric = Number(justificacionId);
  if (!Number.isFinite(numeric)) return 'JM-0000';
  return `JM-${String(numeric).padStart(4, '0')}`;
}

function normalizeApiResumen(item) {
  return {
    id: item?.justificacionID ?? 0,
    idPresentacion: presentBoletaId(item?.justificacionID),
    motivoGeneral: item?.motivoGeneral || 'Sin motivo disponible',
    estado: mapEstadoDescripcion(item?.estadoID, item?.estadoDescripcion),
    fechaCreacion: item?.fechaCreacion || null,
    fechaResolucion: item?.fechaAprobacion || null,
    cantidadDetalles: Number(item?.cantidadDetalles || 0),
    funcionarioNombre: item?.funcionarioNombre || 'Cargando...',
    funcionarioCedula: item?.funcionarioCedula || '',
    compania: item?.compania || '',
    tipoPrincipal: item?.tipoPrincipal || 'Sin detalle',
    observacionDetalle: item?.observacionDetalle || 'Cargando detalle...',
    aprobadorLabel: item?.aprobadorID ? `ID ${item.aprobadorID}` : null
  };
}

function summarizeDetailLines(lineas) {
  if (!Array.isArray(lineas) || lineas.length === 0) {
    return 'Sin líneas de detalle.';
  }

  return lineas
    .map(l => {
      const tipo = l?.tipoJustificacionDescripcion || 'Tipo no disponible';
      const fecha = formatDate(l?.fechaMarca);
      const obs = l?.observacionDetalle ? ` - ${l.observacionDetalle}` : '';
      return `${tipo} (${fecha})${obs}`;
    })
    .join(' | ');
}

function getRrhhEstadoIdFromUi(value) {
  if (value === 'Pendiente') return 1;
  if (value === 'Aprobado') return 2;
  if (value === 'Rechazado') return 3;
  return null;
}

function buildRrhhQueryString() {
  const funcionario = (document.getElementById('rrhh-fn')?.value || '').trim();
  const estadoLabel = document.getElementById('rrhh-estado')?.value || '';
  const compania = (document.getElementById('rrhh-company')?.value || '').trim();
  const fechaDesde = (document.getElementById('rrhh-desde')?.value || '').trim();
  const fechaHasta = (document.getElementById('rrhh-hasta')?.value || '').trim();
  const estadoId = getRrhhEstadoIdFromUi(estadoLabel);

  const query = new URLSearchParams();
  if (funcionario) query.set('funcionario', funcionario);
  if (estadoId) query.set('estadoId', String(estadoId));
  if (compania) query.set('compania', compania);
  if (fechaDesde) query.set('fechaDesde', fechaDesde);
  if (fechaHasta) query.set('fechaHasta', fechaHasta);

  const qs = query.toString();
  return qs ? `?${qs}` : '';
}

function mapDetailToApi(detail) {
  const tipoJustificacionID = JUSTIFICACION_TIPO_IDS[detail.tipo];

  if (!tipoJustificacionID) {
    throw new Error(`Tipo de justificación no mapeado: ${detail.tipo}`);
  }

  return {
    tipoJustificacionID,
    fechaMarca: `${detail.fecha}T00:00:00`,
    observacionDetalle: detail.observacion || null
  };
}

function handleLogin() {
  const usernameInput = document.getElementById('username');
  const passwordInput = document.getElementById('password');
  const errorDiv = document.getElementById('loginError');

  if (!usernameInput || !passwordInput) return;

  const username = usernameInput.value.trim();
  const password = passwordInput.value;

  if (username.length < 3 || password.length < 4) {
    if (errorDiv) {
      errorDiv.textContent = 'Credenciales no válidas. Verifique usuario y contraseña.';
      errorDiv.style.display = 'flex';
    }
    shakeLoginCard();
    return;
  }

  const session = {
    isAuth: true,
    username,
    role: inferRole(username),
    company: inferCompany(username),
    apiBaseUrl: getApiBaseUrl()
  };

  setSession(session);
  window.location.href = 'dashboard.html';
}

function shakeLoginCard() {
  const loginCard = document.querySelector('.login-card');
  if (!loginCard) return;
  loginCard.style.animation = 'none';
  void loginCard.offsetWidth;
  loginCard.style.animation = 'shake .35s ease';
}

function requireAuth() {
  const session = getSession();
  if (!session?.isAuth) {
    window.location.href = 'index.html';
  }
}

function handleLogout() {
  sessionStorage.removeItem(STORAGE_KEYS.session);
  sessionStorage.removeItem(STORAGE_KEYS.activeTab);
  window.location.href = 'index.html';
}

function configureRoleUI() {
  const session = getSession();
  if (!session) return;

  const userEl = document.getElementById('current-user');
  const roleEl = document.getElementById('current-role');
  if (userEl) userEl.textContent = session.username;
  if (roleEl) roleEl.textContent = session.role.replace('ROL_', '');

  const allowedByRole = {
    ROL_FUNC: ['panel-funcionario', 'panel-sifcnp'],
    ROL_JEFE: ['panel-jefatura', 'panel-sifcnp'],
    ROL_RRHH: ['panel-rrhh', 'panel-sifcnp']
  };

  const allowedTabs = allowedByRole[session.role] || ['panel-sifcnp'];
  document.querySelectorAll('.nav-tab').forEach(tab => {
    const tabId = tab.getAttribute('data-tab');
    tab.classList.toggle('hidden', !allowedTabs.includes(tabId));
  });

  const savedTab = sessionStorage.getItem(STORAGE_KEYS.activeTab);
  const fallback = allowedTabs[0];
  const target = savedTab && allowedTabs.includes(savedTab) ? savedTab : fallback;
  switchTab(target);
}

function switchTab(tabId) {
  document.querySelectorAll('.tab-panel').forEach(panel => panel.classList.add('hidden'));
  document.querySelectorAll('.nav-tab').forEach(tab => tab.classList.remove('active'));

  const panel = document.getElementById(tabId);
  if (panel) panel.classList.remove('hidden');

  const tab = document.querySelector(`.nav-tab[data-tab="${tabId}"]`);
  if (tab) tab.classList.add('active');

  sessionStorage.setItem(STORAGE_KEYS.activeTab, tabId);
}

function addDetailLine() {
  const tipoEl = document.getElementById('f-d-tipo');
  const fechaEl = document.getElementById('f-d-fecha');
  const obsEl = document.getElementById('f-d-observacion');

  const tipo = tipoEl?.value || '';
  const fecha = fechaEl?.value || '';
  const observacion = obsEl?.value?.trim() || '';

  if (!tipo || !fecha) {
    showNotice('f-notice', 'error', 'Cada detalle requiere tipo de justificación y fecha de marca.');
    return;
  }

  draftDetails.push({ tipo, fecha, observacion });
  renderDraftDetails();

  if (tipoEl) tipoEl.value = '';
  if (fechaEl) fechaEl.value = '';
  if (obsEl) obsEl.value = '';
}

function removeDetailLine(index) {
  draftDetails = draftDetails.filter((_, i) => i !== index);
  renderDraftDetails();
}

function renderDraftDetails() {
  const tbody = document.getElementById('f-detail-body');
  const total = document.getElementById('f-detail-count');
  if (!tbody || !total) return;

  total.textContent = String(draftDetails.length);
  if (draftDetails.length === 0) {
    tbody.innerHTML = '<tr><td colspan="4" class="text-muted">No hay líneas agregadas.</td></tr>';
    return;
  }

  tbody.innerHTML = draftDetails.map((d, i) => `
    <tr>
      <td>${escapeHtml(d.tipo)}</td>
      <td>${formatDate(d.fecha)}</td>
      <td>${escapeHtml(d.observacion || '—')}</td>
      <td><button class="btn btn-sm btn-danger" type="button" onclick="removeDetailLine(${i})">Eliminar</button></td>
    </tr>
  `).join('');
}

async function registerJustification() {
  const session = getSession();
  const motivo = document.getElementById('f-motivo')?.value?.trim() || '';

  if (!session || session.role !== 'ROL_FUNC') {
    showNotice('f-notice', 'error', 'Solo el rol Funcionario puede registrar boletas.');
    return;
  }

  if (!motivo) {
    showNotice('f-notice', 'error', 'El motivo general es obligatorio.');
    return;
  }

  if (draftDetails.length === 0) {
    showNotice('f-notice', 'error', 'Debe agregar al menos una línea de detalle.');
    return;
  }

  const payload = {
    motivoGeneral: motivo,
    detalles: draftDetails.map(mapDetailToApi)
  };

  try {
    await apiFetch('/api/justificaciones', {
      method: 'POST',
      headers: buildApiHeaders(session, true),
      body: JSON.stringify(payload)
    }, session);

    draftDetails = [];
    renderDraftDetails();

    const motivoEl = document.getElementById('f-motivo');
    if (motivoEl) motivoEl.value = '';

    showNotice('f-notice', 'success', 'Boleta registrada en estado Pendiente Jefatura.');
    await renderFuncionarioHistory();
    await renderJefaturaRequests();
    await renderRRHHTable();
  } catch (error) {
    showNotice('f-notice', 'error', `No se pudo registrar la boleta: ${error.message}`);
  }
}

async function renderFuncionarioHistory() {
  const session = getSession();
  const tbody = document.getElementById('funcionario-history-body');
  if (!tbody || !session) return;

  if (session.role !== 'ROL_FUNC') {
    tbody.innerHTML = '<tr><td colspan="6" class="text-muted">Vista disponible solo para rol Funcionario.</td></tr>';
    return;
  }

  try {
    const response = await apiFetch('/api/justificaciones/mias', {
      method: 'GET',
      headers: buildApiHeaders(session)
    }, session);

    const mine = Array.isArray(response) ? response.map(normalizeApiResumen) : [];
    if (mine.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" class="text-muted">No hay boletas registradas.</td></tr>';
      return;
    }

    tbody.innerHTML = mine.map(b => `
      <tr>
        <td class="text-mono text-sm">${b.idPresentacion}</td>
        <td>${escapeHtml(b.motivoGeneral)}</td>
        <td>${b.cantidadDetalles}</td>
        <td>${formatDateTime(b.fechaCreacion)}</td>
        <td>${renderStatusBadge(b.estado)}</td>
        <td>${b.fechaResolucion ? formatDateTime(b.fechaResolucion) : '—'}</td>
      </tr>
    `).join('');
  } catch (error) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-muted">No hay boletas registradas.</td></tr>';
    showNotice('f-notice', 'error', `No se pudo cargar el historial: ${error.message}`);
  }
}

async function renderJefaturaRequests() {
  const session = getSession();
  const tbody = document.getElementById('jefatura-tbody');
  const countEl = document.getElementById('jefatura-pending-count');
  if (!tbody || !session) return;

  if (session.role !== 'ROL_JEFE') {
    if (countEl) countEl.textContent = '0 pendientes';
    tbody.innerHTML = '<tr><td colspan="6" class="text-muted">Vista disponible solo para rol Jefatura.</td></tr>';
    return;
  }

  try {
    const response = await apiFetch('/api/jefatura/justificaciones/pendientes', {
      method: 'GET',
      headers: buildApiHeaders(session)
    }, session);

    const pending = Array.isArray(response) ? response.map(normalizeApiResumen) : [];
    if (countEl) countEl.textContent = `${pending.length} pendientes`;

    if (pending.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" class="text-muted">No hay solicitudes pendientes.</td></tr>';
      return;
    }

    tbody.innerHTML = pending.map((b) => `
      <tr>
        <td><strong>${escapeHtml(b.funcionarioNombre)}</strong></td>
        <td>${escapeHtml(b.motivoGeneral)}</td>
        <td>${escapeHtml(b.tipoPrincipal)}</td>
        <td>${formatDateTime(b.fechaCreacion)}</td>
        <td class="row-status">${renderStatusBadge(b.estado)}</td>
        <td>
          <div class="flex gap-8">
            <button class="btn btn-sm btn-success" type="button" onclick="approveRequest(${b.id},'approve')">Aprobar</button>
            <button class="btn btn-sm btn-danger" type="button" onclick="approveRequest(${b.id},'reject')">Rechazar</button>
            <button class="btn btn-sm btn-secondary" type="button" onclick="toggleDetail(this, ${b.id})">Ver detalle ▼</button>
          </div>
        </td>
      </tr>
      <tr class="detail-row hidden" data-boleta-id="${b.id}">
        <td colspan="6">
          <div class="detail-inner">
            <div class="detail-field"><label>ID Boleta</label><p class="detail-id">${b.idPresentacion}</p></div>
            <div class="detail-field"><label>Funcionario</label><p class="detail-funcionario">${escapeHtml(b.funcionarioNombre)}</p></div>
            <div class="detail-field"><label>Motivo general</label><p class="detail-motivo">${escapeHtml(b.motivoGeneral)}</p></div>
            <div class="detail-field"><label>Líneas</label><p>${b.cantidadDetalles}</p></div>
            <div class="detail-field"><label>Tipo principal</label><p class="detail-tipo">${escapeHtml(b.tipoPrincipal)}</p></div>
            <div class="detail-field" style="grid-column: 1 / -1;"><label>Detalle completo</label><p class="detail-completo">${escapeHtml(b.observacionDetalle)}</p></div>
          </div>
        </td>
      </tr>
    `).join('');
  } catch (error) {
    if (countEl) countEl.textContent = '0 pendientes';
    tbody.innerHTML = '<tr><td colspan="6" class="text-muted">No hay solicitudes pendientes.</td></tr>';
    showNotice('j-notice', 'error', `No se pudieron cargar pendientes: ${error.message}`);
  }
}

async function approveRequest(boletaId, action) {
  const session = getSession();

  if (!session || session.role !== 'ROL_JEFE') {
    showNotice('j-notice', 'error', 'Solo el rol Jefatura puede resolver boletas.');
    return;
  }

  const requestBody = {
    accion: action === 'approve' ? 'APROBAR' : 'RECHAZAR',
    comentario: ''
  };

  try {
    await apiFetch(`/api/jefatura/justificaciones/${boletaId}/resolver`, {
      method: 'PATCH',
      headers: buildApiHeaders(session, true),
      body: JSON.stringify(requestBody)
    }, session);

    showNotice(
      'j-notice',
      action === 'approve' ? 'success' : 'error',
      action === 'approve'
        ? `Boleta ${presentBoletaId(boletaId)} aprobada.`
        : `Boleta ${presentBoletaId(boletaId)} rechazada.`
    );

    await renderJefaturaRequests();
    await renderRRHHTable();
    await renderFuncionarioHistory();
  } catch (error) {
    showNotice('j-notice', 'error', `No se pudo resolver la boleta: ${error.message}`);
  }
}

async function toggleDetail(btn, boletaId) {
  const row = btn.closest('tr');
  const detailRow = row?.nextElementSibling;
  if (!detailRow || !detailRow.classList.contains('detail-row')) return;

  const isHidden = detailRow.classList.contains('hidden');
  detailRow.classList.toggle('hidden', !isHidden);
  btn.textContent = isHidden ? 'Ocultar ▲' : 'Ver detalle ▼';

  if (!isHidden) {
    return;
  }

  const session = getSession();
  if (!session || session.role !== 'ROL_JEFE') {
    return;
  }

  const funcionarioEl = detailRow.querySelector('.detail-funcionario');
  const tipoEl = detailRow.querySelector('.detail-tipo');
  const detalleEl = detailRow.querySelector('.detail-completo');

  try {
    let data = jefaturaDetailCache.get(boletaId);
    if (!data) {
      if (detalleEl) detalleEl.textContent = 'Cargando detalle...';
      data = await apiFetch(`/api/jefatura/justificaciones/${boletaId}`, {
        method: 'GET',
        headers: buildApiHeaders(session)
      }, session);
      jefaturaDetailCache.set(boletaId, data);
    }

    const lineas = Array.isArray(data?.detalles) ? data.detalles : [];
    const solicitanteNombre = data?.solicitante?.nombreCompleto || 'No disponible';
    const tipoPrincipal = lineas[0]?.tipoJustificacionDescripcion || 'Sin líneas de detalle';

    if (funcionarioEl) funcionarioEl.textContent = solicitanteNombre;
    if (tipoEl) tipoEl.textContent = tipoPrincipal;
    if (detalleEl) detalleEl.textContent = summarizeDetailLines(lineas);
  } catch (error) {
    if (detalleEl) detalleEl.textContent = 'No fue posible cargar el detalle de la boleta.';
    showNotice('j-notice', 'error', `No se pudo cargar el detalle: ${error.message}`);
  }
}

async function renderRRHHTable() {
  const tbody = document.getElementById('rrhh-tbody');
  if (!tbody) return;

  const session = getSession();
  if (!session) return;

  if (session.role === 'ROL_RRHH') {
    const endpoint = `/api/rrhh/justificaciones${buildRrhhQueryString()}`;

    try {
      const response = await apiFetch(endpoint, {
        method: 'GET',
        headers: buildApiHeaders(session)
      }, session);

      const boletas = Array.isArray(response) ? response.map(normalizeApiResumen) : [];
      if (boletas.length === 0) {
        tbody.innerHTML = '<tr><td colspan="7" class="text-muted">No hay registros disponibles.</td></tr>';
        return;
      }

      tbody.innerHTML = boletas.map(b => `
        <tr>
          <td>${escapeHtml(b.funcionarioNombre)}</td>
          <td>${escapeHtml(b.compania || '—')}</td>
          <td>${escapeHtml(b.motivoGeneral)}</td>
          <td>${escapeHtml(b.tipoPrincipal)}</td>
          <td>${formatDateTime(b.fechaCreacion)}</td>
          <td>${renderStatusBadge(b.estado)}</td>
          <td>${b.aprobadorLabel && b.fechaResolucion ? `${escapeHtml(b.aprobadorLabel)} (${formatDateTime(b.fechaResolucion)})` : '—'}</td>
        </tr>
      `).join('');
    } catch (error) {
      tbody.innerHTML = '<tr><td colspan="7" class="text-muted">No hay registros disponibles.</td></tr>';
      showNotice('rrhh-notice', 'error', `No se pudo cargar información RRHH: ${error.message}`);
    }

    return;
  }

  tbody.innerHTML = '<tr><td colspan="7" class="text-muted">Vista disponible solo para rol RRHH.</td></tr>';
}

function applyRRHHFilter() {
  renderRRHHTable();
}

function resetRRHHFilter() {
  ['rrhh-fn', 'rrhh-estado', 'rrhh-company', 'rrhh-desde', 'rrhh-hasta'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.value = '';
  });
  renderRRHHTable();
}

function sifcnpSearch() {
  const query = (document.getElementById('sifcnp-query')?.value || '').toLowerCase().trim();
  const tbody = document.getElementById('sifcnp-tbody');
  if (!tbody) return;

  Array.from(tbody.rows).forEach(row => {
    const fn = row.cells[0]?.textContent.toLowerCase() || '';
    row.style.display = !query || fn.includes(query) ? '' : 'none';
  });
}

function downloadReport() {
  showNotice('rrhh-notice', 'success', `Reporte generado: Reporte_Justificaciones_${today()}.xlsx`);
}

function renderStatusBadge(estado) {
  if (estado === ESTADOS.APROBADO) return '<span class="badge badge-approved">Aprobado</span>';
  if (estado === ESTADOS.RECHAZADO) return '<span class="badge badge-rejected">Rechazado</span>';
  return '<span class="badge badge-pending">Pendiente</span>';
}

function showNotice(targetId, type, msg) {
  const el = document.getElementById(targetId);
  if (!el) return;
  const icon = type === 'error'
    ? 'M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zm0 11a1 1 0 1 1 0-2 1 1 0 0 1 0 2zm.75-4.25a.75.75 0 0 1-1.5 0v-3a.75.75 0 0 1 1.5 0v3z'
    : 'M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z';

  el.className = `alert alert-${type === 'error' ? 'error' : 'success'}`;
  el.innerHTML = `<svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor"><path d="${icon}"/></svg>${msg}`;
  el.style.display = 'flex';
  setTimeout(() => {
    if (el) el.style.display = 'none';
  }, 5000);
}

function formatDate(isoDate) {
  if (!isoDate) return '—';
  const date = new Date(isoDate);
  if (Number.isNaN(date.getTime())) return '—';
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  return `${day}/${month}/${year}`;
}

function formatDateTime(isoDate) {
  if (!isoDate) return '—';
  const date = new Date(isoDate);
  if (Number.isNaN(date.getTime())) return '—';
  const day = String(date.getDate()).padStart(2, '0');
  const month = String(date.getMonth() + 1).padStart(2, '0');
  const year = date.getFullYear();
  const hour = String(date.getHours()).padStart(2, '0');
  const minute = String(date.getMinutes()).padStart(2, '0');
  return `${day}/${month}/${year} ${hour}:${minute}`;
}

function today() {
  return new Date().toISOString().slice(0, 10).replace(/-/g, '');
}

function escapeHtml(value) {
  return String(value)
    .replace(/&/g, '&amp;')
    .replace(/</g, '&lt;')
    .replace(/>/g, '&gt;')
    .replace(/"/g, '&quot;')
    .replace(/'/g, '&#39;');
}

function initLoginPage() {
  document.addEventListener('keydown', event => {
    if (event.key === 'Enter') {
      handleLogin();
    }
  });
}

function initDashboardPage() {
  requireAuth();
  configureRoleUI();
  renderDraftDetails();
  renderFuncionarioHistory();
  renderJefaturaRequests();
  renderRRHHTable();
}

document.addEventListener('DOMContentLoaded', () => {
  if (document.getElementById('username')) {
    initLoginPage();
  }

  if (document.querySelector('.topbar')) {
    initDashboardPage();
  }
});
