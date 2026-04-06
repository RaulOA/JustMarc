/* ============================================================
   SIFCNP — Lógica de Aplicación
   ============================================================ */

/* ── Credenciales (quemadas en código) ───────────────────── */
const CREDENTIALS = { username: 'admin', password: '1234' };

/* ── Login ───────────────────────────────────────────────── */
function handleLogin() {
  const username = document.getElementById('username').value.trim();
  const password = document.getElementById('password').value;
  const errorDiv = document.getElementById('loginError');

  if (username === CREDENTIALS.username && password === CREDENTIALS.password) {
    // Store session flag
    sessionStorage.setItem('sjm_auth', 'true');
    sessionStorage.setItem('sjm_user', username);
    window.location.href = 'dashboard.html';
  } else {
    if (errorDiv) {
      errorDiv.style.display = 'flex';
      // shake effect
      const loginCard = document.querySelector('.login-card');
      if (loginCard) {
        loginCard.style.animation = 'shake .35s ease';
        setTimeout(() => loginCard.style.animation = '', 350);
      }
    }
  }
}

/* ── Auth Guard (call on dashboard pages) ────────────────── */
function requireAuth() {
  if (!sessionStorage.getItem('sjm_auth')) {
    window.location.href = 'index.html';
  }
}

/* ── Logout ──────────────────────────────────────────────── */
function handleLogout() {
  sessionStorage.removeItem('sjm_auth');
  sessionStorage.removeItem('sjm_user');
  window.location.href = 'index.html';
}

/* ── Tab Navigation ──────────────────────────────────────── */
function switchTab(tabId) {
  // Hide all panels
  document.querySelectorAll('.tab-panel').forEach(p => p.classList.add('hidden'));
  // Deactivate all nav tabs
  document.querySelectorAll('.nav-tab').forEach(t => t.classList.remove('active'));

  // Show selected panel
  const panel = document.getElementById(tabId);
  if (panel) panel.classList.remove('hidden');

  // Activate selected nav tab
  const tab = document.querySelector(`.nav-tab[data-tab="${tabId}"]`);
  if (tab) tab.classList.add('active');

  // Persist active tab in session
  sessionStorage.setItem('sjm_activeTab', tabId);
}

/* ── Restore last active tab ─────────────────────────────── */
function restoreTab() {
  const saved = sessionStorage.getItem('sjm_activeTab') || 'panel-funcionario';
  switchTab(saved);
}

/* ── Register Justification (Panel Funcionario) ──────────── */
function registerJustification() {
  const motivo      = document.getElementById('f-motivo')?.value?.trim();
  const tipo        = document.getElementById('f-tipo')?.value;
  const fecha       = document.getElementById('f-fecha')?.value;
  const observacion = document.getElementById('f-observacion')?.value?.trim();

  if (!motivo || !tipo || !fecha) {
    showNotice('f-notice', 'error', 'Por favor complete los campos obligatorios: Motivo, Tipo y Fecha.');
    return;
  }

  // Add row to personal history table
  const tbody = document.getElementById('funcionario-history-body');
  if (tbody) {
    const newId = `JM-${String(tbody.rows.length + 100 + 1).padStart(4, '0')}`;
    const tr = document.createElement('tr');
    tr.innerHTML = `
      <td class="text-mono text-sm">${newId}</td>
      <td>${escapeHtml(motivo)}</td>
      <td>${escapeHtml(tipo)}</td>
      <td>${formatDate(fecha)}</td>
      <td><span class="badge badge-pending">Pendiente</span></td>
    `;
    tbody.insertBefore(tr, tbody.firstChild);
  }

  showNotice('f-notice', 'success', 'Justificación registrada exitosamente. Estado: Pendiente de revisión.');
  clearForm('funcionario-form');
}

/* ── Approve / Reject (Panel Jefatura) ───────────────────── */
function approveRequest(btn, action) {
  const row = btn.closest('tr');
  if (!row) return;
  const statusCell = row.querySelector('.row-status');
  if (!statusCell) return;

  if (action === 'approve') {
    statusCell.innerHTML = '<span class="badge badge-approved">Aprobado</span>';
    row.querySelectorAll('.btn').forEach(b => b.disabled = true);
    showNotice('j-notice', 'success', 'Solicitud aprobada correctamente.');
  } else {
    statusCell.innerHTML = '<span class="badge badge-rejected">Rechazado</span>';
    row.querySelectorAll('.btn').forEach(b => b.disabled = true);
    showNotice('j-notice', 'error', 'Solicitud rechazada.');
  }
}

/* ── Toggle detail row (Jefatura) ────────────────────────── */
function toggleDetail(btn) {
  const row = btn.closest('tr');
  const detailRow = row?.nextElementSibling;
  if (!detailRow || !detailRow.classList.contains('detail-row')) return;

  const isHidden = detailRow.classList.contains('hidden');
  detailRow.classList.toggle('hidden', !isHidden);
  btn.textContent = isHidden ? 'Ocultar ▲' : 'Ver detalle ▼';
}

/* ── RRHH Filter ─────────────────────────────────────────── */
function applyRRHHFilter() {
  const fnFilter    = document.getElementById('rrhh-fn')?.value?.toLowerCase().trim();
  const stFilter    = document.getElementById('rrhh-estado')?.value;
  const tbody       = document.getElementById('rrhh-tbody');
  if (!tbody) return;

  Array.from(tbody.rows).forEach(row => {
    const fn     = row.cells[0]?.textContent.toLowerCase() || '';
    const estado = row.cells[4]?.textContent.trim() || '';

    const fnMatch = !fnFilter || fn.includes(fnFilter);
    const stMatch = !stFilter || estado.includes(stFilter);
    row.style.display = fnMatch && stMatch ? '' : 'none';
  });
}

function resetRRHHFilter() {
  ['rrhh-fn', 'rrhh-estado', 'rrhh-desde', 'rrhh-hasta'].forEach(id => {
    const el = document.getElementById(id);
    if (el) el.value = '';
  });
  const tbody = document.getElementById('rrhh-tbody');
  if (tbody) Array.from(tbody.rows).forEach(r => r.style.display = '');
}

/* ── SIFCNP Search ───────────────────────────────────────── */
function sifcnpSearch() {
  const query = document.getElementById('sifcnp-query')?.value?.toLowerCase().trim();
  const tbody = document.getElementById('sifcnp-tbody');
  if (!tbody) return;

  Array.from(tbody.rows).forEach(row => {
    const fn = row.cells[0]?.textContent.toLowerCase() || '';
    row.style.display = !query || fn.includes(query) ? '' : 'none';
  });
}

/* ── Mock Download ───────────────────────────────────────── */
function downloadReport() {
  showNotice('rrhh-notice', 'success', 'Reporte generado y descargado: Reporte_Justificaciones_' + today() + '.xlsx');
}

/* ── Helpers ─────────────────────────────────────────────── */
function showNotice(targetId, type, msg) {
  const el = document.getElementById(targetId);
  if (!el) return;
  el.className = `alert alert-${type === 'error' ? 'error' : 'success'}`;
  el.innerHTML = `<svg width="16" height="16" viewBox="0 0 16 16" fill="currentColor"><path d="${type === 'error' ? 'M8 1a7 7 0 1 0 0 14A7 7 0 0 0 8 1zm0 11a1 1 0 1 1 0-2 1 1 0 0 1 0 2zm.75-4.25a.75.75 0 0 1-1.5 0v-3a.75.75 0 0 1 1.5 0v3z' : 'M13.854 3.646a.5.5 0 0 1 0 .708l-7 7a.5.5 0 0 1-.708 0l-3.5-3.5a.5.5 0 1 1 .708-.708L6.5 10.293l6.646-6.647a.5.5 0 0 1 .708 0z'}"/></svg>${msg}`;
  el.style.display = 'flex';
  setTimeout(() => { if (el) el.style.display = 'none'; }, 5000);
}

function clearForm(formId) {
  const form = document.getElementById(formId);
  if (!form) return;
  form.querySelectorAll('input, textarea, select').forEach(el => el.value = '');
}

function formatDate(iso) {
  if (!iso) return '—';
  const [y, m, d] = iso.split('-');
  return `${d}/${m}/${y}`;
}

function today() {
  return new Date().toISOString().slice(0, 10).replace(/-/g, '');
}

function escapeHtml(str) {
  return str.replace(/&/g,'&amp;').replace(/</g,'&lt;').replace(/>/g,'&gt;').replace(/"/g,'&quot;');
}
