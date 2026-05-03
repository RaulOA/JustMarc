# Spec: Rediseño Panel de Administración

**Archivo:** `docs/SubAgent docs/admin_panel_redesign_spec.md`  
**Fecha:** 2026-05-03  
**Estado:** Listo para implementación

---

## 1. Resumen del Estado Actual

### 1.1 HTML (`dashboard.html` líneas 574–793)

- `<section id="panel-admin">` contiene **4 cards apiladas verticalmente**:
  1. Dependencias (filtro + tabla + formulario de edición siempre visible)
  2. Asignaciones de Personal (filtro + tabla + formulario de edición siempre visible)
  3. Jerarquías de Aprobación (tabla sin filtro + formulario de edición siempre visible)
  4. Delegaciones (tabla sin filtro + formulario de edición siempre visible)
- El formulario de edición de cada sección está siempre expuesto, incluso si ningún registro fue seleccionado.
- Jerarquías y Delegaciones **no tienen buscador**, solo botón "Actualizar".

### 1.2 JavaScript (`app.js`)

| Función | Comportamiento actual |
|---|---|
| `initAdminPanelIfNeeded()` | Llama a los 4 `loadAdmin*` simultáneamente al activar el tab — **4 llamadas API en paralelo sin ninguna acción del usuario** |
| `loadAdminDependencias()` | Acepta filtros `search` + `estadoRegistroId` |
| `loadAdminUsuarios()` | Acepta filtros `search` + `rolId` + `unidadId` |
| `loadAdminJerarquias()` | Sin parámetros de filtro |
| `loadAdminDelegaciones()` | Sin parámetros de filtro |
| `renderAdmin*Rows()` | Renderizan tabla completa (sin paginación) |
| `selectAdmin*(index)` | Poblan formulario de edición estático |
| `saveAdmin*()` | PATCH/POST a la API |

### 1.3 CSS (`style.css`)

No existen estilos específicos del panel admin. Utiliza clases genéricas:
`.card`, `.card-header`, `.card-body`, `.filter-bar`, `.table-wrapper`, `.btn`, `.badge`, `.form-group`, `.alert`.

**Variables de color disponibles:**
- Azul institucional: `--blue-800` (header), `--blue-600` (primario), `--blue-50` (hover)
- Grises: `--grey-50` (bg), `--grey-100/200` (bordes)
- Estados: `--success`, `--danger`, `--warning`, `--info` con sus variantes `-bg`

---

## 2. Estructura HTML Propuesta

### 2.1 Esquema general del panel rediseñado

```
#panel-admin
  .panel-header
    h2 + descripción
  #admin-notice (toast/alert)
  
  .admin-tabs-nav                        ← tabs internas nuevas
    .admin-tab[data-atab="dep"]          ← Dependencias
    .admin-tab[data-atab="usr"]          ← Usuarios
    .admin-tab[data-atab="jer"]          ← Jerarquías
    .admin-tab[data-atab="del"]          ← Delegaciones
  
  .admin-tab-panel[data-atab="dep"]      ← Solo uno activo a la vez
    .filter-bar (buscadores)
    .admin-results-area
      .table-wrapper (oculto inicialmente)
      .admin-pagination
      .admin-edit-drawer (oculto, expande al seleccionar fila)
  
  ... (repetir para usr, jer, del)
```

### 2.2 HTML completo del panel rediseñado

Reemplaza el contenido de `<section id="panel-admin" class="tab-panel hidden">` con:

```html
<section id="panel-admin" class="tab-panel hidden">
  <div class="panel-header">
    <div class="panel-title">
      <h2>Panel de Administración</h2>
      <p>Gestione dependencias, asignaciones, jerarquías y delegaciones</p>
    </div>
  </div>

  <div id="admin-notice" class="alert" style="display:none"></div>

  <!-- ── Tabs internas ── -->
  <nav class="admin-tabs-nav" role="tablist">
    <button class="admin-tab active" data-atab="dep" role="tab"
            aria-selected="true" onclick="switchAdminTab('dep')">
      Dependencias
    </button>
    <button class="admin-tab" data-atab="usr" role="tab"
            aria-selected="false" onclick="switchAdminTab('usr')">
      Usuarios
    </button>
    <button class="admin-tab" data-atab="jer" role="tab"
            aria-selected="false" onclick="switchAdminTab('jer')">
      Jerarquías
    </button>
    <button class="admin-tab" data-atab="del" role="tab"
            aria-selected="false" onclick="switchAdminTab('del')">
      Delegaciones
    </button>
  </nav>

  <!-- ══ Sub-panel: Dependencias ══════════════════════════ -->
  <div class="admin-tab-panel" data-atab="dep" role="tabpanel">
    <div class="card">
      <div class="card-header">
        <h3>Dependencias</h3>
        <button class="btn btn-outline btn-sm" type="button"
                onclick="clearAdminSelection('dep')">Nueva</button>
      </div>
      <div class="card-body">
        <!-- Filtros -->
        <div class="filter-bar">
          <div class="form-group">
            <label for="admin-dep-search">Buscar</label>
            <input type="text" id="admin-dep-search" placeholder="Nombre o código"
                   onkeydown="if(event.key==='Enter') loadAdminDependencias()" />
          </div>
          <div class="form-group">
            <label for="admin-dep-estado">Estado</label>
            <select id="admin-dep-estado">
              <option value="">Todos</option>
              <option value="1">Activo</option>
              <option value="2">Inactivo</option>
            </select>
          </div>
          <button class="btn btn-primary" type="button" onclick="loadAdminDependencias()">
            Buscar
          </button>
        </div>

        <!-- Resultados (ocultos hasta búsqueda) -->
        <div id="dep-results-area" class="hidden">
          <div class="table-wrapper" style="margin-bottom:8px;">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nombre</th>
                  <th>Padre ID</th>
                  <th>Estado</th>
                  <th></th>
                </tr>
              </thead>
              <tbody id="admin-dep-tbody"></tbody>
            </table>
          </div>
          <div class="admin-pagination" id="dep-pagination"></div>
        </div>

        <!-- Cajón de edición (oculto hasta seleccionar fila) -->
        <div class="admin-edit-drawer hidden" id="dep-drawer">
          <div class="admin-drawer-header">
            <span id="dep-drawer-title">Nueva Dependencia</span>
            <button class="btn btn-sm btn-secondary" type="button"
                    onclick="closeAdminDrawer('dep')">Cerrar</button>
          </div>
          <div class="admin-drawer-body">
            <input type="hidden" id="admin-dep-id" />
            <div class="admin-drawer-grid">
              <div class="form-group">
                <label for="admin-dep-nombre">Nombre</label>
                <input type="text" id="admin-dep-nombre" maxlength="150" />
              </div>
              <div class="form-group">
                <label for="admin-dep-padre">Padre ID</label>
                <input type="text" id="admin-dep-padre" placeholder="Vacío para raíz" />
              </div>
              <div class="form-group">
                <label for="admin-dep-edit-estado">Estado</label>
                <select id="admin-dep-edit-estado">
                  <option value="1">Activo</option>
                  <option value="2">Inactivo</option>
                </select>
              </div>
            </div>
            <div class="admin-drawer-actions">
              <button class="btn btn-primary" type="button"
                      onclick="saveAdminDependencia()">Guardar</button>
              <button class="btn btn-secondary" type="button"
                      onclick="closeAdminDrawer('dep')">Cancelar</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- ══ Sub-panel: Usuarios ══════════════════════════════ -->
  <div class="admin-tab-panel hidden" data-atab="usr" role="tabpanel">
    <div class="card">
      <div class="card-header">
        <h3>Asignaciones de Personal</h3>
      </div>
      <div class="card-body">
        <div class="filter-bar">
          <div class="form-group">
            <label for="admin-user-search">Buscar</label>
            <input type="text" id="admin-user-search"
                   placeholder="Nombre, cédula o correo"
                   onkeydown="if(event.key==='Enter') loadAdminUsuarios()" />
          </div>
          <div class="form-group">
            <label for="admin-user-rol">Rol ID</label>
            <input type="text" id="admin-user-rol" placeholder="Ej: 2" />
          </div>
          <div class="form-group">
            <label for="admin-user-unidad">Unidad ID</label>
            <input type="text" id="admin-user-unidad" placeholder="Ej: 5" />
          </div>
          <button class="btn btn-primary" type="button" onclick="loadAdminUsuarios()">
            Buscar
          </button>
        </div>

        <div id="usr-results-area" class="hidden">
          <div class="table-wrapper" style="margin-bottom:8px;">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Nombre</th>
                  <th>Rol</th>
                  <th>Unidad</th>
                  <th>Jefatura</th>
                  <th>Activo</th>
                  <th></th>
                </tr>
              </thead>
              <tbody id="admin-user-tbody"></tbody>
            </table>
          </div>
          <div class="admin-pagination" id="usr-pagination"></div>
        </div>

        <div class="admin-edit-drawer hidden" id="usr-drawer">
          <div class="admin-drawer-header">
            <span id="usr-drawer-title">Editar Usuario</span>
            <button class="btn btn-sm btn-secondary" type="button"
                    onclick="closeAdminDrawer('usr')">Cerrar</button>
          </div>
          <div class="admin-drawer-body">
            <input type="hidden" id="admin-edit-user-id" />
            <div class="admin-drawer-grid">
              <div class="form-group">
                <label for="admin-edit-rol">Rol ID</label>
                <input type="text" id="admin-edit-rol" />
              </div>
              <div class="form-group">
                <label for="admin-edit-unidad">Unidad ID</label>
                <input type="text" id="admin-edit-unidad" />
              </div>
              <div class="form-group">
                <label for="admin-edit-jefatura">Jefatura ID</label>
                <input type="text" id="admin-edit-jefatura" placeholder="Vacío para null" />
              </div>
              <div class="form-group">
                <label for="admin-edit-activo">Activo</label>
                <select id="admin-edit-activo">
                  <option value="true">Sí</option>
                  <option value="false">No</option>
                </select>
              </div>
            </div>
            <div class="admin-drawer-actions">
              <button class="btn btn-primary" type="button"
                      onclick="saveAdminUsuarioAsignacion()">Guardar Asignación</button>
              <button class="btn btn-outline" type="button"
                      onclick="saveAdminUsuarioEstado()">Guardar Estado</button>
              <button class="btn btn-secondary" type="button"
                      onclick="closeAdminDrawer('usr')">Cancelar</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- ══ Sub-panel: Jerarquías ════════════════════════════ -->
  <div class="admin-tab-panel hidden" data-atab="jer" role="tabpanel">
    <div class="card">
      <div class="card-header">
        <h3>Jerarquías de Aprobación</h3>
        <button class="btn btn-outline btn-sm" type="button"
                onclick="clearAdminSelection('jer')">Nueva</button>
      </div>
      <div class="card-body">
        <div class="filter-bar">
          <div class="form-group">
            <label for="admin-jer-aprobador">Aprobador ID</label>
            <input type="text" id="admin-jer-aprobador" placeholder="Ej: 12"
                   onkeydown="if(event.key==='Enter') loadAdminJerarquias()" />
          </div>
          <div class="form-group">
            <label for="admin-jer-estructura">Estructura ID</label>
            <input type="text" id="admin-jer-estructura" placeholder="Ej: 3" />
          </div>
          <div class="form-group">
            <label for="admin-jer-estado">Estado</label>
            <select id="admin-jer-estado">
              <option value="">Todos</option>
              <option value="1">Activo</option>
              <option value="2">Inactivo</option>
            </select>
          </div>
          <button class="btn btn-primary" type="button" onclick="loadAdminJerarquias()">
            Buscar
          </button>
        </div>

        <div id="jer-results-area" class="hidden">
          <div class="table-wrapper" style="margin-bottom:8px;">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Aprobador</th>
                  <th>Estructura</th>
                  <th>Nivel</th>
                  <th>Relación</th>
                  <th>Estado</th>
                  <th></th>
                </tr>
              </thead>
              <tbody id="admin-jerarquia-tbody"></tbody>
            </table>
          </div>
          <div class="admin-pagination" id="jer-pagination"></div>
        </div>

        <div class="admin-edit-drawer hidden" id="jer-drawer">
          <div class="admin-drawer-header">
            <span id="jer-drawer-title">Nueva Jerarquía</span>
            <button class="btn btn-sm btn-secondary" type="button"
                    onclick="closeAdminDrawer('jer')">Cerrar</button>
          </div>
          <div class="admin-drawer-body">
            <input type="hidden" id="admin-jerarquia-id" />
            <div class="admin-drawer-grid">
              <div class="form-group">
                <label for="admin-jerarquia-aprobador">Aprobador ID</label>
                <input type="text" id="admin-jerarquia-aprobador" />
              </div>
              <div class="form-group">
                <label for="admin-jerarquia-estructura">Estructura ID</label>
                <input type="text" id="admin-jerarquia-estructura" />
              </div>
              <div class="form-group">
                <label for="admin-jerarquia-nivel">Nivel</label>
                <input type="text" id="admin-jerarquia-nivel" />
              </div>
              <div class="form-group">
                <label for="admin-jerarquia-relacion">Relación</label>
                <select id="admin-jerarquia-relacion">
                  <option value="Vertical">Vertical</option>
                  <option value="Horizontal">Horizontal</option>
                </select>
              </div>
              <div class="form-group">
                <label for="admin-jerarquia-estado">Estado</label>
                <select id="admin-jerarquia-estado">
                  <option value="1">Activo</option>
                  <option value="2">Inactivo</option>
                </select>
              </div>
              <div class="form-group">
                <label for="admin-jerarquia-desde">Vigencia Desde</label>
                <input type="date" id="admin-jerarquia-desde" />
              </div>
              <div class="form-group">
                <label for="admin-jerarquia-hasta">Vigencia Hasta</label>
                <input type="date" id="admin-jerarquia-hasta" />
              </div>
            </div>
            <div class="admin-drawer-actions">
              <button class="btn btn-primary" type="button"
                      onclick="saveAdminJerarquia()">Guardar</button>
              <button class="btn btn-secondary" type="button"
                      onclick="closeAdminDrawer('jer')">Cancelar</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>

  <!-- ══ Sub-panel: Delegaciones ══════════════════════════ -->
  <div class="admin-tab-panel hidden" data-atab="del" role="tabpanel">
    <div class="card">
      <div class="card-header">
        <h3>Delegaciones</h3>
        <button class="btn btn-outline btn-sm" type="button"
                onclick="clearAdminSelection('del')">Nueva</button>
      </div>
      <div class="card-body">
        <div class="filter-bar">
          <div class="form-group">
            <label for="admin-del-delegante">Delegante ID</label>
            <input type="text" id="admin-del-delegante" placeholder="Ej: 7"
                   onkeydown="if(event.key==='Enter') loadAdminDelegaciones()" />
          </div>
          <div class="form-group">
            <label for="admin-del-delegado">Delegado ID</label>
            <input type="text" id="admin-del-delegado" placeholder="Ej: 12" />
          </div>
          <div class="form-group">
            <label for="admin-del-estado">Estado</label>
            <select id="admin-del-estado">
              <option value="">Todos</option>
              <option value="1">Activo</option>
              <option value="2">Inactivo</option>
            </select>
          </div>
          <button class="btn btn-primary" type="button" onclick="loadAdminDelegaciones()">
            Buscar
          </button>
        </div>

        <div id="del-results-area" class="hidden">
          <div class="table-wrapper" style="margin-bottom:8px;">
            <table>
              <thead>
                <tr>
                  <th>ID</th>
                  <th>Delegante</th>
                  <th>Delegado</th>
                  <th>Jerarquía</th>
                  <th>Estado</th>
                  <th></th>
                </tr>
              </thead>
              <tbody id="admin-delegacion-tbody"></tbody>
            </table>
          </div>
          <div class="admin-pagination" id="del-pagination"></div>
        </div>

        <div class="admin-edit-drawer hidden" id="del-drawer">
          <div class="admin-drawer-header">
            <span id="del-drawer-title">Nueva Delegación</span>
            <button class="btn btn-sm btn-secondary" type="button"
                    onclick="closeAdminDrawer('del')">Cerrar</button>
          </div>
          <div class="admin-drawer-body">
            <input type="hidden" id="admin-delegacion-id" />
            <div class="admin-drawer-grid">
              <div class="form-group">
                <label for="admin-delegacion-delegante">Delegante ID</label>
                <input type="text" id="admin-delegacion-delegante" />
              </div>
              <div class="form-group">
                <label for="admin-delegacion-delegado">Delegado ID</label>
                <input type="text" id="admin-delegacion-delegado" />
              </div>
              <div class="form-group">
                <label for="admin-delegacion-jerarquia">Jerarquía ID</label>
                <input type="text" id="admin-delegacion-jerarquia" placeholder="Opcional" />
              </div>
              <div class="form-group">
                <label for="admin-delegacion-motivo">Motivo</label>
                <input type="text" id="admin-delegacion-motivo" maxlength="250" />
              </div>
              <div class="form-group">
                <label for="admin-delegacion-estado">Estado</label>
                <select id="admin-delegacion-estado">
                  <option value="1">Activo</option>
                  <option value="2">Inactivo</option>
                </select>
              </div>
              <div class="form-group">
                <label for="admin-delegacion-desde">Vigencia Desde</label>
                <input type="date" id="admin-delegacion-desde" />
              </div>
              <div class="form-group">
                <label for="admin-delegacion-hasta">Vigencia Hasta</label>
                <input type="date" id="admin-delegacion-hasta" />
              </div>
            </div>
            <div class="admin-drawer-actions">
              <button class="btn btn-primary" type="button"
                      onclick="saveAdminDelegacion()">Guardar</button>
              <button class="btn btn-secondary" type="button"
                      onclick="closeAdminDrawer('del')">Cancelar</button>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</section>
```

---

## 3. Cambios en JavaScript (`app.js`)

### 3.1 Estado de módulo — variables de paginación

Agregar junto a las variables globales `adminDependencias`, `adminUsuarios`, etc.:

```js
// Admin panel state
const adminPageSize = 15;
const adminPage = { dep: 1, usr: 1, jer: 1, del: 1 };
```

### 3.2 Nueva función: `switchAdminTab(atab)`

Reemplaza/complementa el manejo de tabs internas (no toca `switchTab` existente):

```js
function switchAdminTab(atab) {
  document.querySelectorAll('.admin-tab').forEach(btn => {
    btn.classList.toggle('active', btn.dataset.atab === atab);
    btn.setAttribute('aria-selected', btn.dataset.atab === atab ? 'true' : 'false');
  });
  document.querySelectorAll('.admin-tab-panel').forEach(panel => {
    panel.classList.toggle('hidden', panel.dataset.atab !== atab);
  });
  sessionStorage.setItem('adminActiveTab', atab);
}
```

### 3.3 Nueva función: `closeAdminDrawer(section)` / `openAdminDrawer(section, title)`

```js
function openAdminDrawer(section, title) {
  const drawer = document.getElementById(`${section}-drawer`);
  const titleEl = document.getElementById(`${section}-drawer-title`);
  if (titleEl) titleEl.textContent = title;
  if (drawer) {
    drawer.classList.remove('hidden');
    drawer.scrollIntoView({ behavior: 'smooth', block: 'nearest' });
  }
}

function closeAdminDrawer(section) {
  const drawer = document.getElementById(`${section}-drawer`);
  if (drawer) drawer.classList.add('hidden');
}
```

### 3.4 Nueva función: `clearAdminSelection(section)`

Limpia el formulario del drawer para crear un registro nuevo:

```js
function clearAdminSelection(section) {
  const fieldMaps = {
    dep: ['admin-dep-id', 'admin-dep-nombre', 'admin-dep-padre'],
    usr: ['admin-edit-user-id', 'admin-edit-rol', 'admin-edit-unidad', 'admin-edit-jefatura'],
    jer: ['admin-jerarquia-id', 'admin-jerarquia-aprobador', 'admin-jerarquia-estructura',
          'admin-jerarquia-nivel', 'admin-jerarquia-desde', 'admin-jerarquia-hasta'],
    del: ['admin-delegacion-id', 'admin-delegacion-delegante', 'admin-delegacion-delegado',
          'admin-delegacion-jerarquia', 'admin-delegacion-motivo',
          'admin-delegacion-desde', 'admin-delegacion-hasta']
  };
  (fieldMaps[section] || []).forEach(id => {
    const el = document.getElementById(id);
    if (el) el.value = '';
  });
  openAdminDrawer(section, 'Nuevo registro');
}
```

### 3.5 Función de paginación: `renderAdminPagination(section, total)`

```js
function renderAdminPagination(section, total) {
  const container = document.getElementById(`${section}-pagination`);
  if (!container) return;
  const pages = Math.ceil(total / adminPageSize);
  if (pages <= 1) { container.innerHTML = ''; return; }

  const current = adminPage[section];
  let html = '<div class="admin-pager">';
  html += `<button class="btn btn-sm btn-secondary" ${current === 1 ? 'disabled' : ''}
    onclick="goAdminPage('${section}', ${current - 1})">‹</button>`;
  html += `<span class="admin-pager-info">Página ${current} de ${pages}</span>`;
  html += `<button class="btn btn-sm btn-secondary" ${current === pages ? 'disabled' : ''}
    onclick="goAdminPage('${section}', ${current + 1})">›</button>`;
  html += '</div>';
  container.innerHTML = html;
}

function goAdminPage(section, page) {
  adminPage[section] = page;
  const renderFns = {
    dep: renderAdminDependenciasRows,
    usr: renderAdminUsuariosRows,
    jer: renderAdminJerarquiasRows,
    del: renderAdminDelegacionesRows
  };
  renderFns[section]?.();
}
```

### 3.6 Modificar `renderAdmin*Rows()` — slice por página + mostrar área de resultados

**Patrón común para las 4 funciones render:**

```js
// Ejemplo para renderAdminDependenciasRows():
function renderAdminDependenciasRows() {
  const tbody = document.getElementById('admin-dep-tbody');
  const resultsArea = document.getElementById('dep-results-area');
  if (!tbody || !resultsArea) return;

  if (!Array.isArray(adminDependencias) || adminDependencias.length === 0) {
    tbody.innerHTML = '<tr><td colspan="5" class="text-muted">Sin resultados.</td></tr>';
    renderAdminPagination('dep', 0);
    resultsArea.classList.remove('hidden');
    return;
  }

  const start = (adminPage.dep - 1) * adminPageSize;
  const pageData = adminDependencias.slice(start, start + adminPageSize);

  tbody.innerHTML = pageData.map((item, idx) => `
    <tr>
      <td>${item.estructuraOrganizacionalID}</td>
      <td>${escapeHtml(item.nombre || '—')}</td>
      <td>${item.estructuraPadreID ?? '—'}</td>
      <td>${getEstadoRegistroLabel(item.estadoRegistroID)}</td>
      <td><button class="btn btn-sm btn-secondary" type="button"
          onclick="selectAdminDependencia(${start + idx})">Editar</button></td>
    </tr>
  `).join('');

  renderAdminPagination('dep', adminDependencias.length);
  resultsArea.classList.remove('hidden');
}
```

Aplicar el mismo patrón a `renderAdminUsuariosRows`, `renderAdminJerarquiasRows`, `renderAdminDelegacionesRows` usando sus respectivos `section` keys: `usr`, `jer`, `del`.

### 3.7 Modificar `selectAdmin*()` — abrir drawer en lugar de poblar form estático

```js
// Ejemplo para selectAdminDependencia:
function selectAdminDependencia(index) {
  const row = adminDependencias[index];
  if (!row) return;
  document.getElementById('admin-dep-id').value = row.estructuraOrganizacionalID;
  document.getElementById('admin-dep-nombre').value = row.nombre || '';
  document.getElementById('admin-dep-padre').value = row.estructuraPadreID ?? '';
  document.getElementById('admin-dep-edit-estado').value = String(row.estadoRegistroID || 1);
  openAdminDrawer('dep', `Editando: ${escapeHtml(row.nombre || String(row.estructuraOrganizacionalID))}`);
}
```

Misma lógica para `selectAdminUsuario` → `openAdminDrawer('usr', ...)`,  
`selectAdminJerarquia` → `openAdminDrawer('jer', ...)`,  
`selectAdminDelegacion` → `openAdminDrawer('del', ...)`.

### 3.8 Modificar `saveAdmin*()` — cerrar drawer tras guardar exitoso

En cada función `saveAdmin*`, después de `showNotice(...)` de éxito, agregar:
```js
closeAdminDrawer('dep');   // o 'usr', 'jer', 'del' según corresponda
```

### 3.9 Modificar `loadAdminJerarquias()` — agregar filtros

```js
async function loadAdminJerarquias() {
  try {
    const session = requireAdminSession();
    const aprobadorId = (document.getElementById('admin-jer-aprobador')?.value || '').trim();
    const estructuraId = (document.getElementById('admin-jer-estructura')?.value || '').trim();
    const estado = document.getElementById('admin-jer-estado')?.value || '';
    const query = new URLSearchParams();
    if (aprobadorId) query.set('aprobadorId', aprobadorId);
    if (estructuraId) query.set('estructuraId', estructuraId);
    if (estado) query.set('estadoRegistroId', estado);

    adminPage.jer = 1;
    const response = await apiFetch(`/api/admin/aprobaciones/jerarquias${query.toString() ? `?${query}` : ''}`, {
      method: 'GET', headers: buildApiHeaders(session)
    }, session);

    adminJerarquias = Array.isArray(response) ? response : [];
    renderAdminJerarquiasRows();
  } catch (error) {
    adminJerarquias = [];
    renderAdminJerarquiasRows();
    showNotice('admin-notice', 'error', `No se pudieron cargar jerarquías: ${error.message}`);
  }
}
```

### 3.10 Modificar `loadAdminDelegaciones()` — agregar filtros

```js
async function loadAdminDelegaciones() {
  try {
    const session = requireAdminSession();
    const deleganteId = (document.getElementById('admin-del-delegante')?.value || '').trim();
    const delegadoId  = (document.getElementById('admin-del-delegado')?.value  || '').trim();
    const estado      = document.getElementById('admin-del-estado')?.value || '';
    const query = new URLSearchParams();
    if (deleganteId) query.set('deleganteId', deleganteId);
    if (delegadoId)  query.set('delegadoId', delegadoId);
    if (estado)      query.set('estadoRegistroId', estado);

    adminPage.del = 1;
    const response = await apiFetch(`/api/admin/aprobaciones/delegaciones${query.toString() ? `?${query}` : ''}`, {
      method: 'GET', headers: buildApiHeaders(session)
    }, session);

    adminDelegaciones = Array.isArray(response) ? response : [];
    renderAdminDelegacionesRows();
  } catch (error) {
    adminDelegaciones = [];
    renderAdminDelegacionesRows();
    showNotice('admin-notice', 'error', `No se pudieron cargar delegaciones: ${error.message}`);
  }
}
```

### 3.11 Modificar `initAdminPanelIfNeeded()` — eliminar carga automática

```js
async function initAdminPanelIfNeeded() {
  const session = getSession();
  if (!session || session.role !== 'ROL_ADMIN') return;
  // Restaurar última sub-tab activa, si existe
  const lastTab = sessionStorage.getItem('adminActiveTab') || 'dep';
  switchAdminTab(lastTab);
  // NO cargar datos automáticamente — el usuario presiona "Buscar"
}
```

### 3.12 Modificar `loadAdminDependencias()` y `loadAdminUsuarios()` — resetear página

Al inicio de cada `loadAdmin*`, agregar:
```js
adminPage.dep = 1; // o usr, jer, del según sección
```

---

## 4. Cambios en CSS (`style.css`)

Agregar al final del archivo, antes del último comentario de sección o al final:

```css
/* ══ Admin Panel — Tabs Internas ════════════════════════════ */

.admin-tabs-nav {
  display: flex;
  gap: 0;
  border-bottom: 2px solid var(--grey-200);
  margin-bottom: 20px;
  overflow-x: auto;
}

.admin-tab {
  padding: 10px 22px;
  font-family: var(--font-main);
  font-size: .875rem;
  font-weight: 500;
  color: var(--grey-600);
  background: transparent;
  border: none;
  border-bottom: 2px solid transparent;
  margin-bottom: -2px;
  cursor: pointer;
  transition: color var(--transition), border-color var(--transition);
  white-space: nowrap;
}

.admin-tab:hover {
  color: var(--blue-600);
}

.admin-tab.active {
  color: var(--blue-700);
  border-bottom-color: var(--blue-600);
  font-weight: 600;
}

/* ══ Admin Panel — Cajón de Edición ════════════════════════ */

.admin-edit-drawer {
  margin-top: 20px;
  border: 1.5px solid var(--blue-300);
  border-radius: var(--radius-md);
  background: var(--blue-50);
  overflow: hidden;
  animation: drawer-in 180ms ease;
}

@keyframes drawer-in {
  from { opacity: 0; transform: translateY(-6px); }
  to   { opacity: 1; transform: translateY(0); }
}

.admin-drawer-header {
  display: flex;
  align-items: center;
  justify-content: space-between;
  padding: 10px 16px;
  background: var(--blue-100);
  border-bottom: 1px solid var(--blue-300);
  font-size: .875rem;
  font-weight: 600;
  color: var(--blue-800);
}

.admin-drawer-body {
  padding: 16px 20px;
}

.admin-drawer-grid {
  display: grid;
  grid-template-columns: repeat(auto-fill, minmax(200px, 1fr));
  gap: 12px 16px;
  margin-bottom: 16px;
}

.admin-drawer-grid .form-group {
  margin-bottom: 0;
}

.admin-drawer-actions {
  display: flex;
  gap: 10px;
  flex-wrap: wrap;
  border-top: 1px solid var(--blue-200);
  padding-top: 14px;
}

/* ══ Admin Panel — Paginación ══════════════════════════════ */

.admin-pager {
  display: flex;
  align-items: center;
  gap: 10px;
  justify-content: flex-end;
  padding: 8px 4px;
}

.admin-pager-info {
  font-size: .82rem;
  color: var(--grey-600);
}

/* ══ Admin Panel — Estado vacío ════════════════════════════ */

.admin-tab-panel { /* sin estilos extra — hereda del layout */ }
```

---

## 5. Resumen de cambios por archivo

### `dashboard.html`

| Cambio | Detalle |
|---|---|
| Reemplazar contenido de `#panel-admin` | Por la estructura con `.admin-tabs-nav` + 4 `.admin-tab-panel` |
| Eliminar 4 cards apiladas | Sustituidas por sub-panels con tabs |
| Eliminar formularios de edición estáticos | Reemplazados por `.admin-edit-drawer` ocultos |
| Agregar buscadores a Jerarquías y Delegaciones | Nuevos inputs: `admin-jer-aprobador`, `admin-jer-estructura`, etc. |

### `app.js`

| Cambio | Tipo |
|---|---|
| `switchAdminTab(atab)` | Nueva función |
| `openAdminDrawer(section, title)` | Nueva función |
| `closeAdminDrawer(section)` | Nueva función |
| `clearAdminSelection(section)` | Nueva función |
| `renderAdminPagination(section, total)` | Nueva función |
| `goAdminPage(section, page)` | Nueva función |
| `renderAdminDependenciasRows()` | Modificar: slice por página + mostrar `dep-results-area` |
| `renderAdminUsuariosRows()` | Modificar: slice por página + mostrar `usr-results-area` |
| `renderAdminJerarquiasRows()` | Modificar: slice por página + mostrar `jer-results-area` |
| `renderAdminDelegacionesRows()` | Modificar: slice por página + mostrar `del-results-area` |
| `selectAdminDependencia()` | Modificar: llamar `openAdminDrawer('dep', ...)` |
| `selectAdminUsuario()` | Modificar: llamar `openAdminDrawer('usr', ...)` |
| `selectAdminJerarquia()` | Modificar: llamar `openAdminDrawer('jer', ...)` |
| `selectAdminDelegacion()` | Modificar: llamar `openAdminDrawer('del', ...)` |
| `saveAdminDependencia()` | Modificar: `closeAdminDrawer('dep')` tras éxito |
| `saveAdminUsuarioAsignacion()` | Modificar: `closeAdminDrawer('usr')` tras éxito |
| `saveAdminUsuarioEstado()` | Modificar: `closeAdminDrawer('usr')` tras éxito |
| `saveAdminJerarquia()` | Modificar: `closeAdminDrawer('jer')` tras éxito |
| `saveAdminDelegacion()` | Modificar: `closeAdminDrawer('del')` tras éxito |
| `loadAdminJerarquias()` | Modificar: agregar filtros + reset página |
| `loadAdminDelegaciones()` | Modificar: agregar filtros + reset página |
| `loadAdminDependencias()` | Modificar: reset `adminPage.dep = 1` |
| `loadAdminUsuarios()` | Modificar: reset `adminPage.usr = 1` |
| `initAdminPanelIfNeeded()` | Modificar: eliminar 4 llamadas API, solo `switchAdminTab` |
| Variables globales | Agregar `adminPageSize = 15`, `adminPage = {dep:1, usr:1, jer:1, del:1}` |

### `style.css`

| Clase nueva | Propósito |
|---|---|
| `.admin-tabs-nav` | Contenedor de tabs internas |
| `.admin-tab` / `.admin-tab.active` | Botones de sub-tab |
| `.admin-edit-drawer` | Cajón de edición animado |
| `.admin-drawer-header` / `.admin-drawer-body` | Partes del drawer |
| `.admin-drawer-grid` | Grid responsive para campos del formulario |
| `.admin-drawer-actions` | Fila de botones de acción |
| `.admin-pager` / `.admin-pager-info` | Paginación compacta |

---

## 6. Notas de implementación

1. **IDs de formulario sin cambios**: todos los `id` de inputs existentes se conservan (`admin-dep-id`, `admin-jerarquia-aprobador`, etc.) para no romper las funciones `saveAdmin*`.
2. **Nuevos IDs de filtros en Jerarquías/Delegaciones**: `admin-jer-aprobador`, `admin-jer-estructura`, `admin-jer-estado`, `admin-del-delegante`, `admin-del-delegado`, `admin-del-estado` — estos son nuevos y sólo se leen en las versiones modificadas de `loadAdminJerarquias()` / `loadAdminDelegaciones()`.
3. **Sin cambios en la API**: todos los endpoints PATCH/POST/GET permanecen igual.
4. **Compatibilidad con `switchTab` existente**: `switchAdminTab` es independiente y no interfiere con el sistema de tabs principal del dashboard.
5. **El área de resultados** (`dep-results-area`, etc.) inicia en `class="hidden"` y solo se muestra tras la primera búsqueda exitosa, evitando tablas vacías al cargar el panel.
