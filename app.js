/* ============================================================
   SIFCNP — Lógica de Aplicación (MVP PRP)
   ============================================================ */

/* ── Sistema de Toasts (notificaciones no invasivas) ─────── */
(function initToasts() {
  if (document.getElementById('toast-container')) return;
  const container = document.createElement('div');
  container.id = 'toast-container';
  document.body.appendChild(container);
})();

/**
 * Muestra una notificación tipo toast no invasiva.
 * @param {'success'|'error'|'warning'|'info'} type
 * @param {string} message  Mensaje principal visible al usuario.
 * @param {object} [opts]
 * @param {string}  [opts.title]         Título opcional (default según tipo).
 * @param {number}  [opts.duration]      ms antes de auto-cerrar (default 5000, 0=permanente).
 * @param {string}  [opts.correlationId] ID de correlación del servidor para soporte técnico.
 */
function toast(type, message, opts = {}) {
  const container = document.getElementById('toast-container');
  if (!container) return;

  const duration = opts.duration ?? 5000;
  const defaultTitles = {
    success: 'Operación exitosa',
    error:   'Ha ocurrido un error',
    warning: 'Advertencia',
    info:    'Información'
  };
  const title = opts.title || defaultTitles[type] || '';

  const icons = {
    success: '<svg width="18" height="18" viewBox="0 0 16 16" fill="currentColor"><path d="M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z"/></svg>',
    error:   '<svg width="18" height="18" viewBox="0 0 16 16" fill="currentColor"><path d="M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zm0 11a1 1 0 1 1 0-2 1 1 0 0 1 0 2zm.75-4.25a.75.75 0 0 1-1.5 0v-3a.75.75 0 0 1 1.5 0v3z"/></svg>',
    warning: '<svg width="18" height="18" viewBox="0 0 16 16" fill="currentColor"><path d="M7.002 11a1 1 0 1 1 2 0 1 1 0 0 1-2 0zm.93-5.22a.75.75 0 0 0-1.46 0l-.5 4.5a.75.75 0 0 0 .745.845h1.03a.75.75 0 0 0 .745-.845l-.56-4.5zM8 2.5a5.5 5.5 0 1 0 0 11 5.5 5.5 0 0 0 0-11z"/></svg>',
    info:    '<svg width="18" height="18" viewBox="0 0 16 16" fill="currentColor"><path d="M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zm.75 4.25a.75.75 0 0 0-1.5 0v4.5a.75.75 0 0 0 1.5 0v-4.5zm-.75-2a1 1 0 1 1 0 2 1 1 0 0 1 0-2z"/></svg>'
  };

  const el = document.createElement('div');
  el.className = `toast toast-${type}`;
  el.setAttribute('role', 'alert');
  el.setAttribute('aria-live', type === 'error' ? 'assertive' : 'polite');

  const corrHtml = opts.correlationId
    ? `<div class="toast-corr">Ref: ${opts.correlationId}</div>`
    : '';

  const progressHtml = duration > 0
    ? `<div class="toast-progress" style="animation-duration:${duration}ms;"></div>`
    : '';

  el.innerHTML = `
    <span class="toast-icon">${icons[type] || icons.info}</span>
    <div class="toast-body">
      <div class="toast-title">${title}</div>
      <div class="toast-msg">${message}</div>
      ${corrHtml}
    </div>
    <button class="toast-close" aria-label="Cerrar notificación">
      <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor"><path d="M4.646 4.646a.5.5 0 0 1 .708 0L8 7.293l2.646-2.647a.5.5 0 0 1 .708.708L8.707 8l2.647 2.646a.5.5 0 0 1-.708.708L8 8.707l-2.646 2.647a.5.5 0 0 1-.708-.708L7.293 8 4.646 5.354a.5.5 0 0 1 0-.708z"/></svg>
    </button>
    ${progressHtml}
  `;

  container.appendChild(el);

  const dismiss = () => {
    el.classList.add('toast-exit');
    el.addEventListener('animationend', () => el.remove(), { once: true });
  };

  el.querySelector('.toast-close').addEventListener('click', dismiss);

  if (duration > 0) {
    setTimeout(dismiss, duration);
  }
}


const STORAGE_KEYS = {
  session: 'sjm_session',
  activeTab: 'sjm_activeTab',
  apiBaseUrl: 'sjm_api_base_url',
  lastActivity: 'sjm_lastActivity'
};

const SESSION_CONFIG = {
  INACTIVITY_TIMEOUT_MS: 5 * 60 * 1000,  // 5 minutes
  WARNING_THRESHOLD_MS: 30 * 1000,        // Show warning in the final 30 seconds
  FINAL_CHECK_INTERVAL_MS: 1 * 1000       // Check every 1 second in final 30 seconds
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
const JEFATURA_PAGE_SIZE = 15;
const FUNCIONARIO_HISTORY_PAGE_SIZE = 10;
let jefaturaCurrentPage = 1;
let jefaturaAllPending = [];
let jefaturaSortField = 'fechaCreacion';
let jefaturaSortDirection = 'desc';
let sifcnpCurrentResults = [];
let funcionarioHistoryAll = [];
let funcionarioHistoryVisibleCount = FUNCIONARIO_HISTORY_PAGE_SIZE;
const funcionarioHistoryDetailCache = new Map();
let adminDepData = [];
let adminUsrData = [];
let adminJerData = [];
let adminDelData = [];
let adminDepPage = 1;
let adminUsrPage = 1;
let adminJerPage = 1;
let adminDelPage = 1;
let adminMonData = [];
let adminMonPage = 1;
const ADMIN_PAGE_SIZE = 15;

const API_CONFIG = {
  defaultBaseUrl: 'http://localhost:5093',
  timeoutMs: 12000
};

let sessionWarningVisible = false;
let sessionMainIntervalId = null;
let sessionFinalIntervalId = null;
let sessionLogoutTriggered = false;

const MOCK_USER_DIRECTORY = {
  'funcionario.ana': { userId: 4, role: 'ROL_FUNC' },
  'funcionario.luis': { userId: 5, role: 'ROL_FUNC' },
  'jefe.maria': { userId: 3, role: 'ROL_JEFE' },
  'jefe.ricardo': { userId: 3, role: 'ROL_JEFE' },
  'rrhh.carlos': { userId: 6, role: 'ROL_RRHH' },
  'rrhh.sandra': { userId: 6, role: 'ROL_RRHH' },
  'admin.demo': { userId: 1, role: 'ROL_ADMIN' },
  'admin.sofia': { userId: 1, role: 'ROL_ADMIN' }
};

const JUSTIFICACION_TIPO_IDS = {
  'Marca Tardía': 1,
  'Omisión Marca de Entrada': 2,
  'Omisión Marca de Salida': 3,
  'Marca antes Hora de Salida': 4,
  Ausencia: 5
};

function inferRole(username) {
  const normalized = String(username || '').trim().toLowerCase();
  const directoryIdentity = MOCK_USER_DIRECTORY[normalized];
  if (directoryIdentity?.role) return directoryIdentity.role;

  if (normalized.includes('admin')) return 'ROL_ADMIN';
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
  // Initialize last activity timestamp when session is set
  if (session?.isAuth) {
    setLastActivity();
  }
}

function getSessionDisplayName(session) {
  const dn = session?.displayName?.trim();
  if (dn) return dn;
  return session?.username || 'Usuario';
}

async function hydrateSessionDisplayName() {
  const session = getSession();
  if (!session?.isAuth) return;
  try {
    const data = await apiFetch('/api/session/profile', { method: 'GET', headers: buildApiHeaders(session) }, session);
    const nombre = data?.nombreCompleto?.trim();
    if (nombre) {
      session.displayName = nombre;
      setSession(session);
      const userEl = document.getElementById('current-user');
      if (userEl) userEl.textContent = nombre;
    }
  } catch {
    // fallo silencioso; el topbar ya muestra username como fallback
  }
}

/**
 * Get the timestamp (ISO string) of the last user activity.
 * If not set, assumes just logged in, so return current time.
 */
function getLastActivity() {
  const stored = sessionStorage.getItem(STORAGE_KEYS.lastActivity);
  if (stored) {
    try {
      return new Date(stored);
    } catch {
      return new Date();
    }
  }
  return new Date();
}

/**
 * Update the last activity timestamp to now.
 */
function setLastActivity() {
  sessionStorage.setItem(STORAGE_KEYS.lastActivity, new Date().toISOString());
}

/**
 * Check if the session has expired due to inactivity.
 */
function isSessionExpired() {
  const session = getSession();
  if (!session?.isAuth) return true;

  const lastActivity = getLastActivity();
  const now = new Date();
  const inactivityMs = now - lastActivity;

  return inactivityMs > SESSION_CONFIG.INACTIVITY_TIMEOUT_MS;
}

/**
 * Get milliseconds remaining until session timeout.
 */
function getTimeUntilTimeout() {
  const lastActivity = getLastActivity();
  const now = new Date();
  const inactivityMs = now - lastActivity;
  const timeRemaining = SESSION_CONFIG.INACTIVITY_TIMEOUT_MS - inactivityMs;
  return Math.max(0, Math.round(timeRemaining));
}

function getApiBaseUrlFromQuery() {
  if (typeof window === 'undefined') return '';

  try {
    const apiBaseUrl = new URLSearchParams(window.location.search).get('api') || '';
    const normalized = apiBaseUrl.trim().replace(/\/$/, '');
    return /^https?:\/\//i.test(normalized) ? normalized : '';
  } catch {
    return '';
  }
}

function syncApiBaseUrlOverride() {
  const fromQuery = getApiBaseUrlFromQuery();
  if (!fromQuery) return;

  sessionStorage.setItem(STORAGE_KEYS.apiBaseUrl, fromQuery);
  if (typeof window !== 'undefined') {
    window.SJM_API_BASE_URL = fromQuery;
  }
}

function getApiBaseUrl() {
  const fromQuery = getApiBaseUrlFromQuery();
  const fromWindow = typeof window !== 'undefined' ? window.SJM_API_BASE_URL : '';
  const fromSession = sessionStorage.getItem(STORAGE_KEYS.apiBaseUrl) || '';
  const selected = String(fromQuery || fromWindow || fromSession || API_CONFIG.defaultBaseUrl).trim();
  return selected.replace(/\/$/, '');
}

function buildApiUrl(path) {
  return `${getApiBaseUrl()}${path}`;
}

function resolveUserIdentity(session) {
  if (!session?.username) return null;

  const normalized = String(session.username).trim().toLowerCase();
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

  const correlationId = parsed?.correlationId
    || response.headers.get('X-Correlation-Id')
    || null;

  const error = new Error(message);
  error.status = response.status;
  error.payload = parsed;
  error.correlationId = correlationId;
  return error;
}


async function apiFetch(path, options = {}, session = getSession()) {
  // Check if session has expired before making the request
  if (isSessionExpired()) {
    handleLogout('Sesión expirada por inactividad. Por favor, inicie sesión nuevamente.');
    throw new Error('Session expired');
  }

  const controller = new AbortController();
  const timer = setTimeout(() => controller.abort(), API_CONFIG.timeoutMs);

  try {
    const response = await fetch(buildApiUrl(path), {
      ...options,
      headers: { ...(options.headers || {}) },
      signal: controller.signal
    });

    if (!response.ok) {
      throw await parseApiError(response);
    }

    if (response.status === 204) return null;

    const contentType = response.headers.get('content-type') || '';
    if (contentType.includes('application/json')) return response.json();

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
    // Toast automático para errores de servidor y red (con correlationId para trazabilidad)
    if (error?.status >= 500 || error?.status === 0 || error?.status === 408) {
      toast('error', error.message, {
        title: error?.status >= 500 ? 'Error del servidor' : 'Error de conexión',
        correlationId: error.correlationId ?? undefined,
        duration: 8000
      });
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
    comentarioResolucion: item?.comentarioResolucion || null,
    estado: mapEstadoDescripcion(item?.estadoID, item?.estadoDescripcion),
    fechaCreacion: item?.fechaCreacion || null,
    fechaResolucion: item?.fechaAprobacion || null,
    cantidadDetalles: Number(item?.cantidadDetalles || 0),
    funcionarioNombre: item?.funcionarioNombre || 'Cargando...',
    funcionarioCedula: item?.funcionarioCedula || '',
    compania: item?.compania || '',
    tipoPrincipal: item?.tipoPrincipal || 'Sin detalle',
    observacionDetalle: item?.observacionDetalle || '—',
    aprobadorLabel: item?.aprobadorID ? `ID ${item.aprobadorID}` : null
  };
}

// Mitigacion temporal acotada al detalle de historial del funcionario.
function normalizeMojibakeTemporaryForHistoryDetail(value) {
  if (typeof value !== 'string' || !/[ÃÂ�]/.test(value)) {
    return value;
  }

  let normalized = value;
  const replacements = [
    ['Ã¡', 'á'], ['Ã©', 'é'], ['Ã­', 'í'], ['Ã³', 'ó'], ['Ãº', 'ú'],
    ['Ã', 'Á'], ['Ã‰', 'É'], ['Ã', 'Í'], ['Ã“', 'Ó'], ['Ãš', 'Ú'],
    ['Ã±', 'ñ'], ['Ã‘', 'Ñ'], ['Ã¼', 'ü'], ['Ãœ', 'Ü'],
    ['Â¿', '¿'], ['Â¡', '¡'], ['Â°', '°'], ['Â', '']
  ];

  replacements.forEach(([wrong, right]) => {
    normalized = normalized.replaceAll(wrong, right);
  });

  return normalized.replaceAll('�', '');
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

function handleLogout(reason) {
  if (sessionLogoutTriggered) return;

  sessionLogoutTriggered = true;
  clearSessionTimeoutIntervals();
  sessionWarningVisible = false;
  hideSessionWarningModal();

  sessionStorage.removeItem(STORAGE_KEYS.session);
  sessionStorage.removeItem(STORAGE_KEYS.activeTab);
  sessionStorage.removeItem(STORAGE_KEYS.lastActivity);

  if (reason) {
    toast('warning', reason, {
      title: 'Sesión finalizada',
      duration: 6000
    });
    // Give a moment for the toast to display before navigating
    setTimeout(() => {
      window.location.href = 'index.html';
    }, 500);
  } else {
    window.location.href = 'index.html';
  }
}

/* ── Session Inactivity Timeout (5 minutes) ─────────────────── */

/**
 * Initialize idle activity monitoring.
 * Listens for user activity and resets the inactivity timer.
 */
function initIdleMonitor() {
  const events = ['mousemove', 'keypress', 'click', 'touchstart', 'scroll'];

  events.forEach(event => {
    document.addEventListener(event, resetIdleTimer, { passive: true });
  });
}

function clearSessionTimeoutIntervals() {
  if (sessionMainIntervalId) {
    clearTimeout(sessionMainIntervalId);
    sessionMainIntervalId = null;
  }

  if (sessionFinalIntervalId) {
    clearInterval(sessionFinalIntervalId);
    sessionFinalIntervalId = null;
  }
}

/**
 * Reset the idle timer by updating last activity timestamp.
 */
function resetIdleTimer() {
  if (!getSession()?.isAuth || sessionWarningVisible) return;

  setLastActivity();
  startSessionExpiryTimer();
}

/**
 * Check session validity on page load.
 * If session has expired, logout with a message.
 */
function checkSessionValidityOnLoad() {
  const session = getSession();
  if (!session?.isAuth) return;

  if (isSessionExpired()) {
    handleLogout('Sesión expirada por inactividad (5 minutos). Por favor, inicie sesión nuevamente.');
  }
}

/**
 * Start the session expiry timer.
 * Schedules the warning exactly when 30 seconds remain.
 */
function startSessionExpiryTimer() {
  clearSessionTimeoutIntervals();
  sessionWarningVisible = false;
  sessionLogoutTriggered = false;
  hideSessionWarningModal();

  if (!getSession()?.isAuth) return;

  const timeRemaining = getTimeUntilTimeout();

  if (timeRemaining <= 0) {
    handleLogout('Sesión expirada por inactividad (5 minutos). Por favor, inicie sesión nuevamente.');
    return;
  }

  const warningDelayMs = Math.max(0, timeRemaining - SESSION_CONFIG.WARNING_THRESHOLD_MS);

  sessionMainIntervalId = setTimeout(() => {
    sessionMainIntervalId = null;

    if (!getSession()?.isAuth) {
      clearSessionTimeoutIntervals();
      return;
    }

    const currentTimeRemaining = getTimeUntilTimeout();

    if (currentTimeRemaining <= 0) {
      handleLogout('Sesión expirada por inactividad (5 minutos). Por favor, inicie sesión nuevamente.');
      return;
    }

    if (currentTimeRemaining <= SESSION_CONFIG.WARNING_THRESHOLD_MS) {
      sessionWarningVisible = true;
      showSessionWarningModal();
      startFinalCountdown();
    }
  }, warningDelayMs);
}

/**
 * Start the final countdown timer (checks every second).
 * Called when less than 30 seconds remain.
 */
function startFinalCountdown() {
  if (sessionFinalIntervalId) {
    clearInterval(sessionFinalIntervalId);
  }

  sessionFinalIntervalId = setInterval(() => {
    if (!getSession()?.isAuth) {
      clearSessionTimeoutIntervals();
      return;
    }

    const timeRemaining = getTimeUntilTimeout();

    updateWarningCountdown(timeRemaining);

    if (timeRemaining > 0 && timeRemaining <= SESSION_CONFIG.WARNING_THRESHOLD_MS && !sessionWarningVisible) {
      sessionWarningVisible = true;
      showSessionWarningModal();
    }

    if (timeRemaining <= 0) {
      handleLogout('Sesión expirada por inactividad (5 minutos). Por favor, inicie sesión nuevamente.');
    }
  }, SESSION_CONFIG.FINAL_CHECK_INTERVAL_MS);
}

/**
 * Show the session warning modal.
 */
function showSessionWarningModal() {
  const modal = document.getElementById('session-warning-modal');
  if (!modal) return;

  sessionWarningVisible = true;
  modal.classList.add('active');
  updateWarningCountdown(getTimeUntilTimeout());

  // Focus on the "Stay Logged In" button for accessibility
  const stayBtn = modal.querySelector('.session-warning-btn-stay');
  if (stayBtn) stayBtn.focus();
}

/**
 * Hide the session warning modal.
 */
function hideSessionWarningModal() {
  const modal = document.getElementById('session-warning-modal');
  if (modal) modal.classList.remove('active');
}

function resetSessionTimeoutFromWarningConfirmation() {
  if (!getSession()?.isAuth) return;

  clearSessionTimeoutIntervals();
  sessionWarningVisible = false;
  sessionLogoutTriggered = false;
  setLastActivity();
  hideSessionWarningModal();
  startSessionExpiryTimer();
}

/**
 * Update the countdown display in the warning modal.
 */
function updateWarningCountdown(msRemaining) {
  const secondsRemaining = Math.ceil(msRemaining / 1000);
  const countdownEl = document.getElementById('session-warning-countdown');
  if (countdownEl) {
    countdownEl.textContent = secondsRemaining;
  }
}

/**
 * Handle "Stay Logged In" button click in warning modal.
 */
function handleStayLoggedIn() {
  resetSessionTimeoutFromWarningConfirmation();
}

/**
 * Handle "Logout Now" button click in warning modal.
 */
function handleLogoutNow() {
  handleLogout();
}

function configureRoleUI() {
  const session = getSession();
  if (!session) return;

  const userEl = document.getElementById('current-user');
  const roleEl = document.getElementById('current-role');
  if (userEl) userEl.textContent = getSessionDisplayName(session);
  const ROLE_LABELS = { ROL_FUNC: 'Funcionario', ROL_JEFE: 'Jefatura', ROL_RRHH: 'Recursos Humanos', ROL_ADMIN: 'Administrador' };
  if (roleEl) roleEl.textContent = ROLE_LABELS[session.role] ?? session.role.replace('ROL_', '');

  const allowedByRole = {
    ROL_FUNC: ['panel-funcionario', 'panel-sifcnp'],
    ROL_JEFE: ['panel-funcionario', 'panel-jefatura', 'panel-sifcnp'],
    ROL_RRHH: ['panel-funcionario', 'panel-rrhh', 'panel-sifcnp'],
    ROL_ADMIN: ['panel-admin']
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
  configureSifcnpScopeUI(session);
}

async function renderCurrentApproverTopbar() {
  const approverEl = document.getElementById('current-approver');
  if (!approverEl) return;

  const session = getSession();
  if (!session) {
    approverEl.textContent = 'Aprobador actual: No disponible';
    return;
  }

  if (session.role !== 'ROL_FUNC' && session.role !== 'ROL_JEFE' && session.role !== 'ROL_RRHH') {
    approverEl.textContent = 'Aprobador actual: No aplica';
    return;
  }

  approverEl.textContent = 'Aprobador actual: Cargando...';

  try {
    const response = await apiFetch('/api/justificaciones/aprobador-actual', {
      method: 'GET',
      headers: buildApiHeaders(session)
    }, session);

    const aprobador = response?.aprobador;
    if (!aprobador?.nombreCompleto) {
      approverEl.textContent = 'Aprobador actual: No definido';
      return;
    }

    if (aprobador.unidadNombre) {
      approverEl.textContent = `Aprobador actual: ${aprobador.nombreCompleto} - ${aprobador.unidadNombre}`;
      return;
    }

    approverEl.textContent = `Aprobador actual: ${aprobador.nombreCompleto}`;
  } catch {
    approverEl.textContent = 'Aprobador actual: No disponible';
  }
}

function configureSifcnpScopeUI(session) {
  const funcionarioGroup = document.getElementById('sifcnp-funcionario-group');
  const scopeNote = document.getElementById('sifcnp-scope-note');
  if (!funcionarioGroup || !scopeNote) return;

  const isFuncionario = session?.role === 'ROL_FUNC';
  funcionarioGroup.classList.toggle('hidden', isFuncionario);
  scopeNote.classList.toggle('hidden', !isFuncionario);

  if (isFuncionario) {
    scopeNote.textContent = 'Mostrando solo sus registros historicos.';
  }
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

  if (!session || (session.role !== 'ROL_FUNC' && session.role !== 'ROL_JEFE')) {
    showNotice('f-notice', 'error', 'Solo Funcionario o Jefatura pueden registrar boletas.');
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

  if (session.role !== 'ROL_FUNC' && session.role !== 'ROL_JEFE' && session.role !== 'ROL_RRHH') {
    tbody.innerHTML = '<tr><td colspan="4" class="text-muted">Vista disponible para Funcionario, Jefatura y RRHH.</td></tr>';
    funcionarioHistoryAll = [];
    funcionarioHistoryVisibleCount = FUNCIONARIO_HISTORY_PAGE_SIZE;
    renderFuncionarioHistoryFooter();
    return;
  }

  try {
    const response = await apiFetch('/api/justificaciones/mias', {
      method: 'GET',
      headers: buildApiHeaders(session)
    }, session);

    funcionarioHistoryAll = Array.isArray(response) ? response.map(normalizeApiResumen) : [];
    funcionarioHistoryVisibleCount = FUNCIONARIO_HISTORY_PAGE_SIZE;
    renderFuncionarioHistoryPage();
  } catch (error) {
    tbody.innerHTML = '<tr><td colspan="4" class="text-muted">No hay boletas registradas.</td></tr>';
    funcionarioHistoryAll = [];
    funcionarioHistoryVisibleCount = FUNCIONARIO_HISTORY_PAGE_SIZE;
    renderFuncionarioHistoryFooter();
    showNotice('f-notice', 'error', `No se pudo cargar el historial: ${error.message}`);
  }
}

function renderFuncionarioHistoryPage() {
  const tbody = document.getElementById('funcionario-history-body');
  if (!tbody) return;

  if (funcionarioHistoryAll.length === 0) {
    tbody.innerHTML = '<tr><td colspan="4" class="text-muted">No hay boletas registradas.</td></tr>';
    renderFuncionarioHistoryFooter();
    return;
  }

  const visibleItems = funcionarioHistoryAll.slice(0, funcionarioHistoryVisibleCount);

  tbody.innerHTML = visibleItems.map(b => `
    <tr>
      <td class="text-mono text-sm">${b.idPresentacion}</td>
      <td>${formatDateTime(b.fechaCreacion)}</td>
      <td>${renderStatusBadge(b.estado)}</td>
      <td>
        <button class="btn btn-sm btn-secondary" type="button" onclick="toggleFuncionarioHistoryDetail(this, ${b.id})">Ver detalle ▼</button>
      </td>
    </tr>
    <tr class="detail-row hidden" data-func-hist-id="${b.id}">
      <td colspan="4">
        <div class="detail-inner">
          <div class="detail-field"><label>Motivo general</label><p>${escapeHtml(b.motivoGeneral)}</p></div>
          ${renderComentarioResolucionDetalle(b)}
          <div class="detail-field" style="grid-column: 1 / -1;">
            <label>Detalle por líneas</label>
            <div class="funcionario-history-lines text-sm text-muted" data-func-hist-lines="${b.id}">Cargando detalle...</div>
          </div>
        </div>
      </td>
    </tr>
  `).join('');

  renderFuncionarioHistoryFooter();
}

function renderComentarioResolucionDetalle(item) {
  const isResuelto = item.estado === ESTADOS.APROBADO || item.estado === ESTADOS.RECHAZADO;
  if (!isResuelto) {
    return '';
  }

  if (!item.comentarioResolucion) {
    return '<div class="detail-field" style="grid-column: 1 / -1;"><label>Comentario de resolución</label><p>—</p></div>';
  }

  return `<div class="detail-field" style="grid-column: 1 / -1;"><label>Comentario de resolución</label><p>${escapeHtml(item.comentarioResolucion)}</p></div>`;
}

async function toggleFuncionarioHistoryDetail(btn, justificacionId) {
  const row = btn.closest('tr');
  const detailRow = row?.nextElementSibling;
  if (!detailRow || !detailRow.classList.contains('detail-row')) return;

  const isHidden = detailRow.classList.contains('hidden');
  detailRow.classList.toggle('hidden', !isHidden);
  btn.textContent = isHidden ? 'Ocultar ▲' : 'Ver detalle ▼';

  if (!isHidden) {
    return;
  }

  const container = detailRow.querySelector(`[data-func-hist-lines="${justificacionId}"]`);
  if (!container) {
    return;
  }

  if (funcionarioHistoryDetailCache.has(justificacionId)) {
    container.innerHTML = funcionarioHistoryDetailCache.get(justificacionId);
    return;
  }

  try {
    const session = getSession();
    if (!session) return;

    const response = await apiFetch(`/api/justificaciones/${justificacionId}/lineas`, {
      method: 'GET',
      headers: buildApiHeaders(session)
    }, session);

    const lineas = Array.isArray(response) ? response : [];
    const html = lineas.length > 0
      ? lineas.map((linea, idx) => {
          const tipoRaw = linea.tipoJustificacionDescripcion || 'Sin tipo';
          const fecha = formatDate(linea.fechaMarca);
          const observacionRaw = linea.observacionDetalle || '—';
          const tipo = escapeHtml(normalizeMojibakeTemporaryForHistoryDetail(tipoRaw));
          const observacion = escapeHtml(normalizeMojibakeTemporaryForHistoryDetail(observacionRaw));
          return `<div style="margin-bottom:8px;"><strong>Línea ${idx + 1}:</strong> Tipo: ${tipo} | Fecha de marca: ${fecha} | Observación: ${observacion}</div>`;
        }).join('')
      : '<div>No hay líneas de detalle registradas.</div>';

    funcionarioHistoryDetailCache.set(justificacionId, html);
    container.innerHTML = html;
  } catch (error) {
    container.innerHTML = '<div>No fue posible cargar las líneas de detalle.</div>';
  }
}

function renderFuncionarioHistoryFooter() {
  const summaryEl = document.getElementById('funcionario-history-summary');
  const loadMoreBtn = document.getElementById('funcionario-history-load-more');
  if (!summaryEl || !loadMoreBtn) return;

  const total = funcionarioHistoryAll.length;
  const showing = Math.min(funcionarioHistoryVisibleCount, total);
  summaryEl.textContent = `Mostrando ${showing} de ${total}`;

  const hasMore = showing < total;
  loadMoreBtn.style.display = hasMore ? '' : 'none';
}

function loadMoreFuncionarioHistory() {
  funcionarioHistoryVisibleCount += FUNCIONARIO_HISTORY_PAGE_SIZE;
  renderFuncionarioHistoryPage();
}

async function renderJefaturaRequests() {
  const session = getSession();
  const tbody = document.getElementById('jefatura-tbody');
  const countEl = document.getElementById('jefatura-pending-count');
  if (!tbody || !session) return;

  if (session.role !== 'ROL_JEFE') {
    if (countEl) countEl.textContent = '0 pendientes';
    tbody.innerHTML = '<tr><td colspan="6" class="text-muted">Vista disponible solo para rol Jefatura.</td></tr>';
    renderJefaturaPagination(0);
    renderJefaturaSortUI();
    return;
  }

  try {
    const response = await apiFetch('/api/jefatura/justificaciones/pendientes', {
      method: 'GET',
      headers: buildApiHeaders(session)
    }, session);

    jefaturaAllPending = Array.isArray(response) ? response.map(normalizeApiResumen) : [];
    if (countEl) countEl.textContent = `${jefaturaAllPending.length} pendientes`;
    jefaturaCurrentPage = 1;

    if (jefaturaAllPending.length === 0) {
      tbody.innerHTML = '<tr><td colspan="6" class="text-muted">No hay solicitudes pendientes.</td></tr>';
      const downloadBtn = document.getElementById('jefatura-download-btn');
      if (downloadBtn) downloadBtn.style.display = 'none';
      renderJefaturaPagination(0);
      renderJefaturaSortUI();
      return;
    }

    renderJefaturaPageView();
  } catch (error) {
    if (countEl) countEl.textContent = '0 pendientes';
    tbody.innerHTML = '<tr><td colspan="6" class="text-muted">No hay solicitudes pendientes.</td></tr>';
    renderJefaturaPagination(0);
    renderJefaturaSortUI();
    showNotice('j-notice', 'error', `No se pudieron cargar pendientes: ${error.message}`);
  }
}

function normalizeSortText(value) {
  return String(value ?? '').trim();
}

function normalizeSortDate(value) {
  const time = new Date(value).getTime();
  return Number.isNaN(time) ? -Infinity : time;
}

function getSortedJefaturaPending() {
  const data = Array.isArray(jefaturaAllPending) ? [...jefaturaAllPending] : [];

  if (!jefaturaSortField) {
    return data;
  }

  const direction = jefaturaSortDirection === 'asc' ? 1 : -1;

  data.sort((a, b) => {
    let result = 0;

    if (jefaturaSortField === 'fechaCreacion') {
      result = normalizeSortDate(a?.fechaCreacion) - normalizeSortDate(b?.fechaCreacion);
    } else {
      const left = normalizeSortText(a?.[jefaturaSortField]);
      const right = normalizeSortText(b?.[jefaturaSortField]);
      result = left.localeCompare(right, 'es', { sensitivity: 'base' });
    }

    if (result === 0) {
      const leftId = Number(a?.id || 0);
      const rightId = Number(b?.id || 0);
      result = leftId - rightId;
    }

    return result * direction;
  });

  return data;
}

function setJefaturaSort(field) {
  if (!field) return;

  if (jefaturaSortField === field) {
    jefaturaSortDirection = jefaturaSortDirection === 'asc' ? 'desc' : 'asc';
  } else {
    jefaturaSortField = field;
    jefaturaSortDirection = 'asc';
  }

  jefaturaCurrentPage = 1;
  renderJefaturaPageView();
}

function renderJefaturaSortUI() {
  const headers = document.querySelectorAll('#panel-jefatura th[data-sort-field]');
  headers.forEach((header) => {
    const field = header.getAttribute('data-sort-field') || '';
    const indicator = header.querySelector('.jefatura-sort-indicator');
    const isActive = field === jefaturaSortField;

    if (isActive && jefaturaSortDirection === 'asc') {
      header.setAttribute('aria-sort', 'ascending');
      if (indicator) indicator.textContent = '▲';
      return;
    }

    if (isActive && jefaturaSortDirection === 'desc') {
      header.setAttribute('aria-sort', 'descending');
      if (indicator) indicator.textContent = '▼';
      return;
    }

    header.setAttribute('aria-sort', 'none');
    if (indicator) indicator.textContent = '↕';
  });
}

function renderJefaturaPageView() {
  const tbody = document.getElementById('jefatura-tbody');
  const downloadBtn = document.getElementById('jefatura-download-btn');
  if (!tbody) return;

  const sorted = getSortedJefaturaPending();
  const totalPages = Math.ceil(sorted.length / JEFATURA_PAGE_SIZE);

  if (totalPages > 0 && jefaturaCurrentPage > totalPages) {
    jefaturaCurrentPage = totalPages;
  }
  if (jefaturaCurrentPage < 1) {
    jefaturaCurrentPage = 1;
  }

  const paginated = sorted.slice(
    (jefaturaCurrentPage - 1) * JEFATURA_PAGE_SIZE,
    jefaturaCurrentPage * JEFATURA_PAGE_SIZE
  );

  if (downloadBtn) downloadBtn.style.display = jefaturaAllPending.length > 0 ? '' : 'none';

  if (sorted.length === 0) {
    tbody.innerHTML = '<tr><td colspan="6" class="text-muted">No hay solicitudes pendientes.</td></tr>';
    renderJefaturaPagination(0);
    renderJefaturaSortUI();
    return;
  }

  tbody.innerHTML = paginated.map((b) => `
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
          <div class="detail-field"><label>Dependencia / Unidad</label><p class="detail-unidad">—</p></div>
          <div class="detail-field" style="grid-column: 1 / -1;"><label>Detalle completo</label><p class="detail-completo">${escapeHtml(b.observacionDetalle)}</p></div>
        </div>
      </td>
    </tr>
  `).join('');

  renderJefaturaPagination(totalPages);
  renderJefaturaSortUI();
}

function renderJefaturaPagination(totalPages) {
  const container = document.getElementById('jefatura-pagination');
  if (!container) return;

  if (totalPages <= 0) {
    container.innerHTML = `
      <button class="btn btn-sm btn-secondary" disabled>&#8249; Anterior</button>
      <span style="padding:0 12px;font-size:.9rem;">Página 0 de 0 - Sin resultados</span>
      <button class="btn btn-sm btn-secondary" disabled>Siguiente &#8250;</button>
    `;
    return;
  }

  const isFirstPage = jefaturaCurrentPage === 1;
  const isLastPage = jefaturaCurrentPage === totalPages;

  container.innerHTML = `
    <button class="btn btn-sm btn-secondary" onclick="jefaturaGoToPage(${jefaturaCurrentPage - 1})" ${isFirstPage ? 'disabled' : ''}>&#8249; Anterior</button>
    <span style="padding:0 12px;font-size:.9rem;">Página ${jefaturaCurrentPage} de ${totalPages}</span>
    <button class="btn btn-sm btn-secondary" onclick="jefaturaGoToPage(${jefaturaCurrentPage + 1})" ${isLastPage ? 'disabled' : ''}>Siguiente &#8250;</button>
  `;
}

function jefaturaGoToPage(page) {
  const totalPages = Math.ceil(jefaturaAllPending.length / JEFATURA_PAGE_SIZE);
  if (page < 1 || page > totalPages) return;
  jefaturaCurrentPage = page;
  renderJefaturaPageView();
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
      'success',
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

    const unidadEl = detailRow.querySelector('.detail-unidad');
    if (funcionarioEl) funcionarioEl.textContent = solicitanteNombre;
    if (tipoEl) tipoEl.textContent = tipoPrincipal;
    if (detalleEl) detalleEl.textContent = summarizeDetailLines(lineas);
    if (unidadEl) unidadEl.textContent = data?.solicitante?.unidadNombre || 'No disponible';
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

function renderSifcnpRows(rows) {
  const tbody = document.getElementById('sifcnp-tbody');
  if (!tbody) return;

  tbody.innerHTML = rows.length > 0
    ? rows.map(row => `
      <tr>
        <td>${escapeHtml(row.nombre)}</td>
        <td class="text-mono text-sm">${escapeHtml(row.cedula)}</td>
        <td>${escapeHtml(row.concepto)}</td>
        <td>${escapeHtml(row.fecha)}</td>
        <td>${escapeHtml(row.observacion)}</td>
        <td>${renderStatusBadge(row.estado)}</td>
      </tr>
    `).join('')
    : '<tr><td colspan="6" class="text-muted">No se encontraron registros.</td></tr>';
}

async function renderSifcnpHistorico() {
  const session = getSession();
  if (!session) return;

  const query = (document.getElementById('sifcnp-query')?.value || '').trim();
  const desde = document.getElementById('sifcnp-desde')?.value || '';
  const hasta = document.getElementById('sifcnp-hasta')?.value || '';
  const tbody = document.getElementById('sifcnp-tbody');
  if (!tbody) return;

  const params = new URLSearchParams();
  if (query && session.role !== 'ROL_FUNC') params.set('funcionario', query);
  if (desde) params.set('fechaDesde', desde);
  if (hasta) params.set('fechaHasta', hasta);

  const endpoint = `/api/justificaciones/historico${params.toString() ? `?${params.toString()}` : ''}`;

  try {
    const response = await apiFetch(endpoint, {
      method: 'GET',
      headers: buildApiHeaders(session)
    }, session);

    const rows = Array.isArray(response)
      ? response.map(item => ({
        nombre: item?.funcionarioNombre || 'No disponible',
        cedula: item?.funcionarioCedula || '—',
        concepto: item?.tipoPrincipal || 'Sin detalle',
        fecha: formatDate(item?.fechaCreacion),
        observacion: item?.motivoGeneral || '—',
        estado: mapEstadoDescripcion(item?.estadoID, item?.estadoDescripcion)
      }))
      : [];

    sifcnpCurrentResults = rows;
    renderSifcnpRows(rows);
  } catch (error) {
    sifcnpCurrentResults = [];
    renderSifcnpRows([]);
    showNotice('sifcnp-notice', 'error', `No se pudo cargar el historico: ${error.message}`);
  }
}

function sifcnpSearch() {
  renderSifcnpHistorico();
}

function downloadReport() {
  showNotice('rrhh-notice', 'success', `Reporte generado: Reporte_Justificaciones_${today()}.xlsx`);
}

function downloadJefaturaReport() {
  if (jefaturaAllPending.length === 0) {
    showNotice('j-notice', 'warning', 'No hay datos pendientes para descargar.');
    return;
  }
  const headers = ['Funcionario', 'Motivo', 'Tipo', 'Fecha', 'Estado'];
  const rows = jefaturaAllPending.map(b => [
    b.funcionarioNombre,
    b.motivoGeneral,
    b.tipoPrincipal,
    formatDateTime(b.fechaCreacion),
    b.estado
  ]);
  const csvContent = [headers, ...rows]
    .map(r => r.map(cell => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(','))
    .join('\r\n');
  const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `Reporte_Jefatura_${today()}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

function downloadSifcnpReport() {
  if (!Array.isArray(sifcnpCurrentResults) || sifcnpCurrentResults.length === 0) {
    showNotice('sifcnp-notice', 'warning', 'No hay registros para descargar en la vista actual.');
    return;
  }

  const headers = ['Funcionario', 'Cedula', 'Concepto', 'Fecha', 'Observacion', 'Estado'];
  const rows = sifcnpCurrentResults.map(row => [
    row.nombre,
    row.cedula,
    row.concepto,
    row.fecha,
    row.observacion,
    row.estado
  ]);

  const csvContent = [headers, ...rows]
    .map(r => r.map(cell => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(','))
    .join('\r\n');

  const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `Reporte_Consulta_SIFCNP_${today()}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

function requireAdminSession() {
  const session = getSession();
  if (!session || session.role !== 'ROL_ADMIN') {
    throw new Error('Funcionalidad disponible solo para ROL_ADMIN.');
  }
  return session;
}

function parseNullableInt(rawValue) {
  const raw = String(rawValue ?? '').trim();
  if (!raw) return null;
  const value = Number(raw);
  if (!Number.isInteger(value) || value <= 0) {
    throw new Error('Debe ingresar un número entero mayor a cero.');
  }
  return value;
}

function normalizeDateForApi(rawValue) {
  const raw = String(rawValue || '').trim();
  return raw ? `${raw}T00:00:00` : null;
}

function normalizeDateForInput(rawValue) {
  if (!rawValue) return '';
  return String(rawValue).slice(0, 10);
}

function getEstadoRegistroLabel(estadoId) {
  return Number(estadoId) === 1 ? 'Activo' : 'Inactivo';
}

// ===== ADMIN PANEL =====

function initAdminPanelIfNeeded() {
  // Solo activa el tab, sin cargar datos automáticamente
}

function switchAdminTab(tab) {
  document.querySelectorAll('.admin-tab-btn').forEach(btn => {
    btn.classList.toggle('active', btn.dataset.adminTab === tab);
  });
  document.querySelectorAll('.admin-sub-panel').forEach(panel => {
    panel.style.display = 'none';
  });
  const target = document.getElementById('admin-sub-' + tab);
  if (target) target.style.display = '';
}

async function loadAdminDependencias() {
  const search = (document.getElementById('admin-dep-search')?.value || '').toLowerCase();
  const resultsEl = document.getElementById('admin-dep-results');
  resultsEl.innerHTML = '<p class="admin-loading">Cargando...</p>';
  try {
    const session = requireAdminSession();
    const data = await apiFetch('/api/admin/organizacion/dependencias', { method: 'GET', headers: buildApiHeaders(session) }, session);
    adminDepData = (Array.isArray(data) ? data : []).filter(d =>
      !search || d.nombre?.toLowerCase().includes(search) || d.codigo?.toLowerCase().includes(search)
    );
    adminDepPage = 1;
    renderAdminDepPage();
  } catch (e) {
    resultsEl.innerHTML = '<p class="admin-error">Error al cargar dependencias.</p>';
  }
}

function renderAdminDepPage() {
  const resultsEl = document.getElementById('admin-dep-results');
  const total = adminDepData.length;
  if (total === 0) { resultsEl.innerHTML = '<p class="admin-empty-hint">Sin resultados.</p>'; return; }
  const start = (adminDepPage - 1) * ADMIN_PAGE_SIZE;
  const page = adminDepData.slice(start, start + ADMIN_PAGE_SIZE);
  let html = `<table class="admin-table"><thead><tr><th>ID</th><th>Nombre</th><th>Padre</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>`;
  page.forEach(d => {
    const id = d.estructuraOrganizacionalID ?? d.dependenciaId ?? d.id ?? '';
    html += `<tr>
      <td>${escapeHtml(String(id))}</td>
      <td>${escapeHtml(d.nombre ?? '')}</td>
      <td>${escapeHtml(String(d.estructuraPadreID ?? d.padreId ?? '—'))}</td>
      <td>${escapeHtml(getEstadoRegistroLabel(d.estadoRegistroID ?? 1))}</td>
      <td><button class="btn-sm" onclick="openAdminDepEdit(${id})">Editar</button></td>
    </tr>`;
  });
  html += `</tbody></table>`;
  html += renderAdminPagination(adminDepPage, total, 'Dep');
  resultsEl.innerHTML = html;
}

function openAdminDepEdit(id) {
  const dep = adminDepData.find(d => (d.estructuraOrganizacionalID ?? d.dependenciaId ?? d.id) === id);
  if (!dep) return;
  const drawer = document.getElementById('admin-dep-drawer');
  drawer.style.display = '';
  drawer.innerHTML = `
    <div class="admin-drawer-inner">
      <h4>Editar Dependencia #${escapeHtml(String(id))}</h4>
      <label>Nombre<input id="edit-dep-nombre" class="admin-input" value="${escapeHtml(dep.nombre ?? '')}" /></label>
      <label>Padre ID<input id="edit-dep-padre" class="admin-input" value="${escapeHtml(String(dep.estructuraPadreID ?? dep.padreId ?? ''))}" /></label>
      <div class="admin-drawer-actions">
        <button class="btn btn-primary" onclick="saveAdminDep(${id})">Guardar</button>
        <button class="btn btn-secondary" onclick="closeAdminDrawer('dep')">Cancelar</button>
      </div>
    </div>`;
}

async function saveAdminDep(id) {
  try {
    const session = requireAdminSession();
    const nombre = (document.getElementById('edit-dep-nombre')?.value || '').trim();
    const estructuraPadreID = parseNullableInt(document.getElementById('edit-dep-padre')?.value);
    if (!nombre) { alert('El nombre es requerido.'); return; }
    await apiFetch(`/api/admin/organizacion/dependencias/${id}`, {
      method: 'PATCH',
      headers: buildApiHeaders(session, true),
      body: JSON.stringify({ nombre, estructuraPadreID })
    }, session);
    closeAdminDrawer('dep');
    await loadAdminDependencias();
  } catch (e) {
    showNotice('admin-notice', 'error', `No se pudo guardar dependencia: ${e.message}`);
  }
}

async function loadAdminUsuarios() {
  const search = (document.getElementById('admin-usr-search')?.value || '').toLowerCase();
  const resultsEl = document.getElementById('admin-usr-results');
  resultsEl.innerHTML = '<p class="admin-loading">Cargando...</p>';
  try {
    const session = requireAdminSession();
    const data = await apiFetch('/api/admin/organizacion/usuarios', { method: 'GET', headers: buildApiHeaders(session) }, session);
    adminUsrData = (Array.isArray(data) ? data : []).filter(u =>
      !search ||
      u.nombreCompleto?.toLowerCase().includes(search) ||
      u.username?.toLowerCase().includes(search) ||
      u.dependencia?.toLowerCase().includes(search) ||
      u.rolDescripcion?.toLowerCase().includes(search)
    );
    adminUsrPage = 1;
    renderAdminUsrPage();
  } catch (e) {
    resultsEl.innerHTML = '<p class="admin-error">Error al cargar usuarios.</p>';
  }
}

function renderAdminUsrPage() {
  const resultsEl = document.getElementById('admin-usr-results');
  const total = adminUsrData.length;
  if (total === 0) { resultsEl.innerHTML = '<p class="admin-empty-hint">Sin resultados.</p>'; return; }
  const start = (adminUsrPage - 1) * ADMIN_PAGE_SIZE;
  const page = adminUsrData.slice(start, start + ADMIN_PAGE_SIZE);
  let html = `<table class="admin-table"><thead><tr><th>ID</th><th>Nombre</th><th>Rol</th><th>Unidad</th><th>Activo</th><th>Acciones</th></tr></thead><tbody>`;
  page.forEach(u => {
    const id = u.usuarioID ?? u.empleadoId ?? u.id ?? '';
    html += `<tr>
      <td>${escapeHtml(String(id))}</td>
      <td>${escapeHtml(u.nombreCompleto ?? '')}</td>
      <td><span class="admin-badge">${escapeHtml(u.rolDescripcion ?? u.rol ?? '')}</span></td>
      <td>${escapeHtml(String(u.unidadID ?? u.dependencia ?? ''))}</td>
      <td>${u.esActivo ? 'Sí' : 'No'}</td>
      <td><button class="btn-sm" onclick="openAdminUsrEdit(${id})">Editar</button></td>
    </tr>`;
  });
  html += `</tbody></table>`;
  html += renderAdminPagination(adminUsrPage, total, 'Usr');
  resultsEl.innerHTML = html;
}

function openAdminUsrEdit(id) {
  const usr = adminUsrData.find(u => (u.usuarioID ?? u.empleadoId ?? u.id) === id);
  if (!usr) return;
  const drawer = document.getElementById('admin-usr-drawer');
  drawer.style.display = '';
  const rol = usr.rolID ?? usr.rol ?? '';
  drawer.innerHTML = `
    <div class="admin-drawer-inner">
      <h4>Editar Usuario: ${escapeHtml(usr.nombreCompleto ?? String(id))}</h4>
      <label>Rol ID
        <input id="edit-usr-rolid" class="admin-input" value="${escapeHtml(String(rol))}" placeholder="ID de rol" />
      </label>
      <label>Unidad ID
        <input id="edit-usr-unidad" class="admin-input" value="${escapeHtml(String(usr.unidadID ?? ''))}" placeholder="ID de unidad" />
      </label>
      <label>Jefatura ID
        <input id="edit-usr-jefatura" class="admin-input" value="${escapeHtml(String(usr.jefaturaID ?? ''))}" placeholder="Vacío para null" />
      </label>
      <label>Activo
        <select id="edit-usr-activo" class="admin-input">
          <option value="true" ${usr.esActivo ? 'selected' : ''}>Sí</option>
          <option value="false" ${!usr.esActivo ? 'selected' : ''}>No</option>
        </select>
      </label>
      <div class="admin-drawer-actions">
        <button class="btn btn-primary" onclick="saveAdminUsrAsignacion(${id})">Guardar Asignación</button>
        <button class="btn btn-secondary" onclick="closeAdminDrawer('usr')">Cancelar</button>
      </div>
    </div>`;
}

async function saveAdminUsrAsignacion(id) {
  try {
    const session = requireAdminSession();
    const payload = {
      rolID: parseNullableInt(document.getElementById('edit-usr-rolid')?.value),
      unidadID: parseNullableInt(document.getElementById('edit-usr-unidad')?.value),
      jefaturaID: parseNullableInt(document.getElementById('edit-usr-jefatura')?.value),
      esActivo: document.getElementById('edit-usr-activo')?.value === 'true'
    };
    await apiFetch(`/api/admin/organizacion/usuarios/${id}/asignacion`, {
      method: 'PATCH',
      headers: buildApiHeaders(session, true),
      body: JSON.stringify(payload)
    }, session);
    closeAdminDrawer('usr');
    await loadAdminUsuarios();
  } catch (e) {
    showNotice('admin-notice', 'error', `No se pudo guardar usuario: ${e.message}`);
  }
}

async function loadAdminJerarquias() {
  const search = (document.getElementById('admin-jer-search')?.value || '').toLowerCase();
  const resultsEl = document.getElementById('admin-jer-results');
  resultsEl.innerHTML = '<p class="admin-loading">Cargando...</p>';
  try {
    const session = requireAdminSession();
    const data = await apiFetch('/api/admin/aprobaciones/jerarquias', { method: 'GET', headers: buildApiHeaders(session) }, session);
    adminJerData = (Array.isArray(data) ? data : []).filter(j =>
      !search ||
      String(j.estructuraOrganizacionalID ?? '').includes(search) ||
      String(j.aprobadorUsuarioID ?? '').includes(search) ||
      j.tipoRelacion?.toLowerCase().includes(search)
    );
    adminJerPage = 1;
    renderAdminJerPage();
  } catch (e) {
    resultsEl.innerHTML = '<p class="admin-error">Error al cargar jerarquías.</p>';
  }
}

function renderAdminJerPage() {
  const resultsEl = document.getElementById('admin-jer-results');
  const total = adminJerData.length;
  if (total === 0) { resultsEl.innerHTML = '<p class="admin-empty-hint">Sin resultados.</p>'; return; }
  const start = (adminJerPage - 1) * ADMIN_PAGE_SIZE;
  const page = adminJerData.slice(start, start + ADMIN_PAGE_SIZE);
  let html = `<table class="admin-table"><thead><tr><th>ID</th><th>Aprobador ID</th><th>Estructura ID</th><th>Nivel</th><th>Relación</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>`;
  page.forEach(j => {
    const id = j.jerarquiaAprobacionID ?? j.jerarquiaId ?? j.id ?? '';
    const activo = (j.estadoRegistroID ?? 1) === 1;
    html += `<tr>
      <td>${escapeHtml(String(id))}</td>
      <td>${escapeHtml(String(j.aprobadorUsuarioID ?? ''))}</td>
      <td>${escapeHtml(String(j.estructuraOrganizacionalID ?? ''))}</td>
      <td>${escapeHtml(String(j.nivelAprobacion ?? ''))}</td>
      <td>${escapeHtml(j.tipoRelacion ?? '—')}</td>
      <td><span class="admin-badge ${activo ? 'active' : 'inactive'}">${activo ? 'Activo' : 'Inactivo'}</span></td>
      <td><button class="btn-sm btn-danger" onclick="deleteAdminJerarquia(${id})">Eliminar</button></td>
    </tr>`;
  });
  html += `</tbody></table>`;
  html += renderAdminPagination(adminJerPage, total, 'Jer');
  resultsEl.innerHTML = html;
}

async function deleteAdminJerarquia(id) {
  if (!confirm('¿Eliminar esta jerarquía?')) return;
  try {
    const session = requireAdminSession();
    await apiFetch(`/api/admin/aprobaciones/jerarquias/${id}`, { method: 'DELETE', headers: buildApiHeaders(session) }, session);
    await loadAdminJerarquias();
  } catch (e) {
    showNotice('admin-notice', 'error', `No se pudo eliminar jerarquía: ${e.message}`);
  }
}

function openAdminJerarquiaForm() {
  const drawer = document.getElementById('admin-jer-drawer');
  drawer.style.display = '';
  drawer.innerHTML = `
    <div class="admin-drawer-inner">
      <h4>Nueva Jerarquía</h4>
      <label>Aprobador ID (usuario)<input id="new-jer-apr" type="number" class="admin-input" placeholder="ID de usuario aprobador" /></label>
      <label>Estructura ID<input id="new-jer-est" type="number" class="admin-input" placeholder="ID de estructura organizacional" /></label>
      <label>Nivel<input id="new-jer-nivel" type="number" class="admin-input" value="1" /></label>
      <label>Tipo Relación
        <select id="new-jer-rel" class="admin-input">
          <option value="Vertical">Vertical</option>
          <option value="Horizontal">Horizontal</option>
        </select>
      </label>
      <label>Vigencia Desde<input id="new-jer-desde" type="date" class="admin-input" /></label>
      <label>Vigencia Hasta<input id="new-jer-hasta" type="date" class="admin-input" /></label>
      <div class="admin-drawer-actions">
        <button class="btn btn-primary" onclick="saveAdminJerarquia()">Crear</button>
        <button class="btn btn-secondary" onclick="closeAdminDrawer('jer')">Cancelar</button>
      </div>
    </div>`;
}

async function saveAdminJerarquia() {
  const aprobadorUsuarioID = parseNullableInt(document.getElementById('new-jer-apr')?.value);
  const estructuraOrganizacionalID = parseNullableInt(document.getElementById('new-jer-est')?.value);
  const nivelAprobacion = parseNullableInt(document.getElementById('new-jer-nivel')?.value) || 1;
  const tipoRelacion = document.getElementById('new-jer-rel')?.value || 'Vertical';
  const vigenciaDesde = normalizeDateForApi(document.getElementById('new-jer-desde')?.value);
  const vigenciaHasta = normalizeDateForApi(document.getElementById('new-jer-hasta')?.value);
  if (!aprobadorUsuarioID || !estructuraOrganizacionalID || !vigenciaDesde) {
    alert('Complete todos los campos obligatorios.');
    return;
  }
  try {
    const session = requireAdminSession();
    await apiFetch('/api/admin/aprobaciones/jerarquias', {
      method: 'POST',
      headers: buildApiHeaders(session, true),
      body: JSON.stringify({ aprobadorUsuarioID, estructuraOrganizacionalID, nivelAprobacion, tipoRelacion, vigenciaDesde, vigenciaHasta })
    }, session);
    closeAdminDrawer('jer');
    await loadAdminJerarquias();
  } catch (e) {
    showNotice('admin-notice', 'error', `No se pudo crear jerarquía: ${e.message}`);
  }
}

async function loadAdminDelegaciones() {
  const search = (document.getElementById('admin-del-search')?.value || '').toLowerCase();
  const resultsEl = document.getElementById('admin-del-results');
  resultsEl.innerHTML = '<p class="admin-loading">Cargando...</p>';
  try {
    const session = requireAdminSession();
    const data = await apiFetch('/api/admin/aprobaciones/delegaciones', { method: 'GET', headers: buildApiHeaders(session) }, session);
    adminDelData = (Array.isArray(data) ? data : []).filter(d =>
      !search ||
      String(d.deleganteUsuarioID ?? '').includes(search) ||
      String(d.delegadoUsuarioID ?? '').includes(search)
    );
    adminDelPage = 1;
    renderAdminDelPage();
  } catch (e) {
    resultsEl.innerHTML = '<p class="admin-error">Error al cargar delegaciones.</p>';
  }
}

function renderAdminDelPage() {
  const resultsEl = document.getElementById('admin-del-results');
  const total = adminDelData.length;
  if (total === 0) { resultsEl.innerHTML = '<p class="admin-empty-hint">Sin resultados.</p>'; return; }
  const start = (adminDelPage - 1) * ADMIN_PAGE_SIZE;
  const page = adminDelData.slice(start, start + ADMIN_PAGE_SIZE);
  let html = `<table class="admin-table"><thead><tr><th>ID</th><th>Delegante ID</th><th>Delegado ID</th><th>Desde</th><th>Hasta</th><th>Estado</th><th>Acciones</th></tr></thead><tbody>`;
  page.forEach(d => {
    const id = d.delegacionAprobacionID ?? d.delegacionId ?? d.id ?? '';
    const activo = (d.estadoRegistroID ?? 1) === 1;
    html += `<tr>
      <td>${escapeHtml(String(id))}</td>
      <td>${escapeHtml(String(d.deleganteUsuarioID ?? ''))}</td>
      <td>${escapeHtml(String(d.delegadoUsuarioID ?? ''))}</td>
      <td>${d.vigenciaDesde ? escapeHtml(String(d.vigenciaDesde).substring(0, 10)) : '—'}</td>
      <td>${d.vigenciaHasta ? escapeHtml(String(d.vigenciaHasta).substring(0, 10)) : '—'}</td>
      <td><span class="admin-badge ${activo ? 'active' : 'inactive'}">${activo ? 'Activo' : 'Inactivo'}</span></td>
      <td><button class="btn-sm btn-danger" onclick="deleteAdminDelegacion(${id})">Eliminar</button></td>
    </tr>`;
  });
  html += `</tbody></table>`;
  html += renderAdminPagination(adminDelPage, total, 'Del');
  resultsEl.innerHTML = html;
}

async function deleteAdminDelegacion(id) {
  if (!confirm('¿Eliminar esta delegación?')) return;
  try {
    const session = requireAdminSession();
    await apiFetch(`/api/admin/aprobaciones/delegaciones/${id}`, { method: 'DELETE', headers: buildApiHeaders(session) }, session);
    await loadAdminDelegaciones();
  } catch (e) {
    showNotice('admin-notice', 'error', `No se pudo eliminar delegación: ${e.message}`);
  }
}

function openAdminDelegacionForm() {
  const drawer = document.getElementById('admin-del-drawer');
  drawer.style.display = '';
  drawer.innerHTML = `
    <div class="admin-drawer-inner">
      <h4>Nueva Delegación</h4>
      <label>Delegante ID<input id="new-del-dlg" type="number" class="admin-input" placeholder="ID usuario que delega" /></label>
      <label>Delegado ID<input id="new-del-dld" type="number" class="admin-input" placeholder="ID usuario que recibe" /></label>
      <label>Vigencia Desde<input id="new-del-desde" type="date" class="admin-input" /></label>
      <label>Vigencia Hasta<input id="new-del-hasta" type="date" class="admin-input" /></label>
      <div class="admin-drawer-actions">
        <button class="btn btn-primary" onclick="saveAdminDelegacion()">Crear</button>
        <button class="btn btn-secondary" onclick="closeAdminDrawer('del')">Cancelar</button>
      </div>
    </div>`;
}

async function saveAdminDelegacion() {
  const deleganteUsuarioID = parseNullableInt(document.getElementById('new-del-dlg')?.value);
  const delegadoUsuarioID = parseNullableInt(document.getElementById('new-del-dld')?.value);
  const vigenciaDesde = normalizeDateForApi(document.getElementById('new-del-desde')?.value);
  const vigenciaHasta = normalizeDateForApi(document.getElementById('new-del-hasta')?.value);
  if (!deleganteUsuarioID || !delegadoUsuarioID || !vigenciaDesde) {
    alert('Complete todos los campos obligatorios.');
    return;
  }
  try {
    const session = requireAdminSession();
    await apiFetch('/api/admin/aprobaciones/delegaciones', {
      method: 'POST',
      headers: buildApiHeaders(session, true),
      body: JSON.stringify({ deleganteUsuarioID, delegadoUsuarioID, vigenciaDesde, vigenciaHasta })
    }, session);
    closeAdminDrawer('del');
    await loadAdminDelegaciones();
  } catch (e) {
    showNotice('admin-notice', 'error', `No se pudo crear delegación: ${e.message}`);
  }
}

function closeAdminDrawer(key) {
  const drawer = document.getElementById(`admin-${key}-drawer`);
  if (drawer) drawer.style.display = 'none';
}

async function loadAdminMonitoring() {
  const resultsEl = document.getElementById('admin-mon-results');
  if (!resultsEl) return;

  const tipo = document.getElementById('admin-mon-tipo')?.value || '';
  const search = (document.getElementById('admin-mon-search')?.value || '').trim();
  const desde = document.getElementById('admin-mon-desde')?.value || '';
  const hasta = document.getElementById('admin-mon-hasta')?.value || '';
  const sortBy = document.getElementById('admin-mon-sortby')?.value || 'fecha';
  const sortDir = document.getElementById('admin-mon-sortdir')?.value || 'desc';

  resultsEl.innerHTML = '<p class="admin-loading">Cargando registros...</p>';

  try {
    const session = requireAdminSession();
    const qs = new URLSearchParams();
    if (tipo) qs.set('tipo', tipo);
    if (search) qs.set('search', search);
    if (desde) qs.set('desde', `${desde}T00:00:00`);
    if (hasta) qs.set('hasta', `${hasta}T00:00:00`);
    qs.set('sortBy', sortBy);
    qs.set('sortDir', sortDir);

    const data = await apiFetch(`/api/admin/monitoring/registros?${qs.toString()}`, {
      method: 'GET',
      headers: buildApiHeaders(session)
    }, session);

    adminMonData = Array.isArray(data) ? data : [];
    adminMonPage = 1;
    renderAdminMonitoringPage();
  } catch (e) {
    resultsEl.innerHTML = '<p class="admin-error">Error al cargar registros de monitoreo.</p>';
  }
}

function renderAdminMonitoringPage() {
  const resultsEl = document.getElementById('admin-mon-results');
  if (!resultsEl) return;

  const total = adminMonData.length;
  if (total === 0) {
    resultsEl.innerHTML = '<p class="admin-empty-hint">Sin resultados para los filtros seleccionados.</p>';
    return;
  }

  const start = (adminMonPage - 1) * ADMIN_PAGE_SIZE;
  const page = adminMonData.slice(start, start + ADMIN_PAGE_SIZE);

  let html = `<table class="admin-table"><thead><tr><th>Fecha</th><th>Tipo</th><th>Categoría</th><th>Mensaje</th><th>Usuario</th><th>Estado</th><th>Referencia</th><th>Origen</th></tr></thead><tbody>`;
  page.forEach(item => {
    const isError = (item.tipo || '').toUpperCase() === 'ERROR';
    html += `<tr>
      <td>${escapeHtml(formatDateTime(item.fecha))}</td>
      <td><span class="admin-badge ${isError ? 'inactive' : 'active'}">${escapeHtml(item.tipo || '')}</span></td>
      <td>${escapeHtml(item.categoria || '—')}</td>
      <td title="${escapeHtml(item.detalle || '')}">${escapeHtml(item.mensaje || '—')}</td>
      <td>${escapeHtml(item.usuario || '—')}</td>
      <td>${escapeHtml(item.estado || '—')}</td>
      <td>${escapeHtml(item.referencia || '—')}</td>
      <td>${escapeHtml(item.origen || '—')}</td>
    </tr>`;
  });
  html += '</tbody></table>';
  html += renderAdminPagination(adminMonPage, total, 'Mon');
  resultsEl.innerHTML = html;
}

function downloadAdminMonitoringReport() {
  if (!Array.isArray(adminMonData) || adminMonData.length === 0) {
    showNotice('admin-notice', 'warning', 'No hay registros para descargar.');
    return;
  }

  const headers = ['Fecha', 'Tipo', 'Categoria', 'Mensaje', 'Usuario', 'Estado', 'Referencia', 'Origen', 'Detalle'];
  const rows = adminMonData.map(x => [
    formatDateTime(x.fecha),
    x.tipo,
    x.categoria,
    x.mensaje,
    x.usuario,
    x.estado,
    x.referencia,
    x.origen,
    x.detalle
  ]);

  const csvContent = [headers, ...rows]
    .map(r => r.map(cell => `"${String(cell ?? '').replace(/"/g, '""')}"`).join(','))
    .join('\r\n');

  const blob = new Blob(['\uFEFF' + csvContent], { type: 'text/csv;charset=utf-8;' });
  const url = URL.createObjectURL(blob);
  const a = document.createElement('a');
  a.href = url;
  a.download = `Reporte_Monitoreo_Admin_${today()}.csv`;
  document.body.appendChild(a);
  a.click();
  document.body.removeChild(a);
  URL.revokeObjectURL(url);
}

function renderAdminPagination(currentPage, total, key) {
  const totalPages = Math.ceil(total / ADMIN_PAGE_SIZE);
  if (totalPages <= 1) return '';
  const prev = currentPage > 1 ? `<button class="btn-sm" onclick="adminChangePage('${key}', ${currentPage - 1})">‹ Anterior</button>` : '';
  const next = currentPage < totalPages ? `<button class="btn-sm" onclick="adminChangePage('${key}', ${currentPage + 1})">Siguiente ›</button>` : '';
  return `<div class="admin-pagination">${prev}<span>${currentPage} / ${totalPages}</span>${next}</div>`;
}

function adminChangePage(key, page) {
  if (key === 'Dep') { adminDepPage = page; renderAdminDepPage(); }
  else if (key === 'Usr') { adminUsrPage = page; renderAdminUsrPage(); }
  else if (key === 'Jer') { adminJerPage = page; renderAdminJerPage(); }
  else if (key === 'Del') { adminDelPage = page; renderAdminDelPage(); }
  else if (key === 'Mon') { adminMonPage = page; renderAdminMonitoringPage(); }
}

function renderStatusBadge(estado) {
  if (estado === ESTADOS.APROBADO) return '<span class="badge badge-approved">Aprobado</span>';
  if (estado === ESTADOS.RECHAZADO) return '<span class="badge badge-rejected">Rechazado</span>';
  return '<span class="badge badge-pending">Pendiente</span>';
}

/** showNotice redirige al sistema de toasts global. */
function showNotice(targetId, type, msg) {
  const toastType = type === 'error' ? 'error' : type === 'warning' ? 'warning' : 'success';
  toast(toastType, msg);
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
  const session = getSession();
  checkSessionValidityOnLoad();
  configureRoleUI();
  hydrateSessionDisplayName();
  renderCurrentApproverTopbar();
  renderDraftDetails();
  renderFuncionarioHistory();
  renderJefaturaRequests();
  renderRRHHTable();
  if (session?.role !== 'ROL_ADMIN') {
    renderSifcnpHistorico();
  }
  initAdminPanelIfNeeded();
}

document.addEventListener('DOMContentLoaded', () => {
  syncApiBaseUrlOverride();

  if (document.getElementById('username')) {
    initLoginPage();
  }

  if (document.querySelector('.topbar')) {
    initDashboardPage();
    // Initialize session timeout monitoring on all authenticated pages
    initIdleMonitor();
    startSessionExpiryTimer();
  }
});
