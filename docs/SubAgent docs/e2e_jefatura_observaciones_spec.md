# Spec E2E — Observaciones Perfil Jefatura

**Fecha:** 2026-04-23  
**Alcance:** Frontend (`dashboard.html`, `app.js`) + Backend (C# .NET 8)  
**Rol objetivo:** `ROL_JEFE`

---

## 1. Etiqueta de rol — "JEFE" vs "Jefatura"

### Hallazgo
`configureRoleUI()` en [app.js](../../app.js#L412):

```js
if (roleEl) roleEl.textContent = session.role.replace('ROL_', '');
```

`ROL_JEFE` → muestra **"JEFE"** en el topbar (`id="current-role"`).  
Las etiquetas estructurales del panel (nav-tab, `<h2>`) dicen correctamente "Jefatura".

### Cambio requerido — `app.js`
| Ubicación | Actual | Debe decir |
|---|---|---|
| `configureRoleUI()` L~412 | `.replace('ROL_', '')` | Mapear con diccionario de etiquetas legibles |

**Plan exacto:**
```js
// Reemplazar la línea:
if (roleEl) roleEl.textContent = session.role.replace('ROL_', '');
// Por:
const ROLE_LABELS = {
  ROL_FUNC: 'Funcionario',
  ROL_JEFE: 'Jefatura',
  ROL_RRHH: 'Recursos Humanos'
};
if (roleEl) roleEl.textContent = ROLE_LABELS[session.role] ?? session.role.replace('ROL_', '');
```

### Criterio de aceptación
- El topbar del usuario `jefe.maria` muestra "Jefatura", no "JEFE".
- Otros roles muestran "Funcionario" y "Recursos Humanos".

---

## 2. Datos semilla de solicitudes pendientes

### Hallazgo
`renderJefaturaRequests()` ([app.js](../../app.js#L578)) consume **en vivo** el endpoint  
`GET /api/jefatura/justificaciones/pendientes`.  
No hay hardcoded seed en el panel de pendientes de jefatura.

Si la base de datos está vacía (instalación limpia), el tbody muestra:  
`"No hay solicitudes pendientes."` — comportamiento correcto.

### Cambio requerido
- **No se modifica código de producción.**
- Añadir datos semilla SQL en [`docs/db/001_integra_marcas_base_inicial.sql`](../db/001_integra_marcas_base_inicial.sql) o en un nuevo script `003_integra_marcas_seed_demo.sql` con al menos 2 justificaciones en estado `Pendiente Jefatura` asignadas al `UsuarioId = 20` (jefe.maria) para demo/dev.

**Estructura mínima de seed:**
```sql
-- Usuario funcionario (subordinado de jefe.maria UsuarioId=20)
INSERT INTO RecursosHumanos.Usuario (Cedula, NombreCompleto, CorreoElectronico,
    JefaturaId, UnidadId, RolId, Compania, CreadoPor)
VALUES ('1-0001-0010', 'Ana Funcionaria Demo', 'ana@cnp.go.cr', 20, 1, 1, 'CNP', 'seed');

-- Boleta pendiente
INSERT INTO Operacion.Justificacion (UsuarioId, MotivoGeneral, EstadoJustificacionId, CreadoPor)
SELECT UsuarioId, 'Tardanza por bloqueo vial', 1, 'seed'
FROM RecursosHumanos.Usuario WHERE Cedula = '1-0001-0010';
```

### Criterio de aceptación
- Con el script aplicado, el panel de jefatura muestra ≥ 2 filas al iniciar sesión como `jefe.maria`.
- Sin el script, el panel muestra el mensaje vacío (no error).

---

## 3. Segunda pestaña de jefatura — panel-sifcnp y filtros

### Hallazgo
La segunda pestaña accesible para `ROL_JEFE` es **"Consulta Histórica (SIFCNP)"** (`panel-sifcnp`).

**Filtros presentes en la UI** ([dashboard.html](../../dashboard.html#L510)):
| ID elemento | Tipo | Estado actual |
|---|---|---|
| `sifcnp-query` | `<input type="text">` Funcionario | Funcional (filtra client-side) |
| `sifcnp-desde` | `<input type="date">` Fecha inicio | **Presente pero SIN efecto** — no está wired en `sifcnpSearch()` |
| `sifcnp-hasta` | `<input type="date">` Fecha fin | **Presente pero SIN efecto** — no está wired en `sifcnpSearch()` |

Función actual ([app.js](../../app.js#L776)):
```js
function sifcnpSearch() {
  const query = (document.getElementById('sifcnp-query')?.value || '').toLowerCase().trim();
  // sifcnp-desde y sifcnp-hasta NO se leen ni aplican
  Array.from(tbody.rows).forEach(row => { /* filtra solo por nombre */ });
}
```

### Cambio requerido — `app.js`
Extender `sifcnpSearch()` para aplicar filtro de fechas sobre las filas:

```js
function sifcnpSearch() {
  const query = (document.getElementById('sifcnp-query')?.value || '').toLowerCase().trim();
  const desde = document.getElementById('sifcnp-desde')?.value || '';
  const hasta = document.getElementById('sifcnp-hasta')?.value || '';
  const tbody = document.getElementById('sifcnp-tbody');
  if (!tbody) return;

  Array.from(tbody.rows).forEach(row => {
    const fn = row.cells[0]?.textContent.toLowerCase() || '';
    const rawFecha = row.cells[3]?.textContent || ''; // formato DD/MM/YYYY
    // Convertir DD/MM/YYYY → YYYY-MM-DD para comparación
    const parts = rawFecha.split('/');
    const isoFecha = parts.length === 3 ? `${parts[2]}-${parts[1]}-${parts[0]}` : '';
    const matchNombre = !query || fn.includes(query);
    const matchDesde = !desde || isoFecha >= desde;
    const matchHasta = !hasta || isoFecha <= hasta;
    row.style.display = matchNombre && matchDesde && matchHasta ? '' : 'none';
  });
}
```

### Criterio de aceptación
- Filtrar por fecha inicio excluye filas anteriores a la fecha seleccionada.
- Filtrar por fecha fin excluye filas posteriores.
- Ambos filtros pueden combinarse con el filtro de nombre.
- Sin valores, se muestran todas las filas.

---

## 4. Tabla históricos — ¿hardcoded o API/BD?

### Hallazgo
Los datos del `<tbody id="sifcnp-tbody">` en [dashboard.html](../../dashboard.html#L547) son **10 filas HTML estáticas hardcodeadas**, no provienen de ningún endpoint ni base de datos.

El botón "Buscar" llama a `sifcnpSearch()` que filtra esas filas fijas client-side.

No existe endpoint dedicado para consulta histórica SIFCNP en el backend actual.

### Cambio requerido (dos fases)

**Fase 1 — Corrección inmediata (sin backend nuevo):**
- Mover los datos hardcodeados a un array JavaScript constante `SIFCNP_MOCK_DATA` en `app.js`.
- Renderizarlos dinámicamente en `sifcnpSearch()` para mantener el filtrado funcional.
- Vaciar el `<tbody>` en el HTML (dejar solo el tag).

**Fase 2 — Integración real (requiere backend):**
- Crear endpoint `GET /api/sifcnp/historicos?query=&desde=&hasta=` con rol `ROL_JEFE` (o todos los roles).
- En `sifcnpSearch()`, reemplazar el filtrado local por un `apiFetch` al nuevo endpoint.
- El endpoint debe consultar `Operacion.Justificacion` + `RecursosHumanos.Usuario` con filtros de parámetros.

**Archivos afectados Fase 1:**
| Archivo | Acción |
|---|---|
| `dashboard.html` | Vaciar `<tbody id="sifcnp-tbody">` |
| `app.js` | Añadir `SIFCNP_MOCK_DATA[]` y renderizado dinámico en `sifcnpSearch()` |

### Criterio de aceptación
- Fase 1: el comportamiento visual es idéntico al actual pero los datos viven en JS, no en HTML.
- Fase 2: al buscar, la tabla refleja registros reales de la BD.

---

## 5. Descarga de reporte — jefatura

### Hallazgo
El botón "Descargar Reporte" **solo existe en el Panel RRHH** ([dashboard.html](../../dashboard.html#L410)):
```html
<button class="btn btn-outline" onclick="downloadReport()">Descargar Reporte</button>
```

La función `downloadReport()` ([app.js](../../app.js#L787)) es un **stub** que solo muestra un toast:
```js
function downloadReport() {
  showNotice('rrhh-notice', 'success', `Reporte generado: Reporte_Justificaciones_${today()}.xlsx`);
}
```
No genera ningún archivo real. No existe equivalente para jefatura.

### Cambio requerido

**Paso 1 — Añadir botón en panel jefatura (`dashboard.html`):**
```html
<!-- En el panel-header de panel-jefatura, junto al badge de pendientes -->
<button class="btn btn-outline" onclick="downloadJefaturaReport()" id="jefatura-download-btn" style="display:none">
  <svg width="14" height="14" viewBox="0 0 16 16" fill="currentColor">
    <path d="M.5 9.9a.5.5 0 0 1 .5.5v2.5a1 1 0 0 0 1 1h12a1 1 0 0 0 1-1v-2.5a.5.5 0 0 1 1 0v2.5a2 2 0 0 1-2 2H2a2 2 0 0 1-2-2v-2.5a.5.5 0 0 1 .5-.5z"/>
    <path d="M7.646 11.854a.5.5 0 0 0 .708 0l3-3a.5.5 0 0 0-.708-.708L8.5 10.293V1.5a.5.5 0 0 0-1 0v8.793L5.354 8.146a.5.5 0 1 0-.708.708l3 3z"/>
  </svg>
  Descargar Reporte
</button>
```

**Paso 2 — Función en `app.js`:**
```js
function downloadJefaturaReport() {
  // Fase 1: stub con nombre de archivo contextual
  showNotice('j-notice', 'success', `Reporte generado: Reporte_Jefatura_${today()}.xlsx`);
  // Fase 2 real: construir CSV de las filas actuales del tbody o llamar endpoint
}
```

**Paso 3 — Mostrar el botón solo cuando hay datos:**
En `renderJefaturaRequests()`, después de renderizar filas:
```js
const downloadBtn = document.getElementById('jefatura-download-btn');
if (downloadBtn) downloadBtn.style.display = pending.length > 0 ? '' : 'none';
```

### Criterio de aceptación
- El botón aparece en el panel jefatura solo cuando existen filas cargadas.
- Al hacer clic, muestra un toast con nombre de archivo contextual.
- (Fase 2) genera un CSV/XLSX descargable con las filas actuales.

---

## 6. Paginación

### Hallazgo
**No existe ningún control de paginación** en el frontend actual.

- `renderJefaturaRequests()` renderiza **todas las filas** del response sin límite.
- `renderRRHHTable()` ídem.
- `sifcnpSearch()` opera sobre filas hardcodeadas sin paginación.
- El PRP menciona paginación solo para auditoría admin (RF-07, no implementado).

### Cambio requerido

**Prioridad para MVP jefatura:** paginación del panel de pendientes.

**Plan — `app.js` + `dashboard.html`:**

1. Añadir constante `const JEFATURA_PAGE_SIZE = 15;` y variable de estado `let jefaturaCurrentPage = 1;`.
2. En `renderJefaturaRequests()`, después de obtener `pending[]`:
   ```js
   const totalPages = Math.ceil(pending.length / JEFATURA_PAGE_SIZE);
   const paginated = pending.slice(
     (jefaturaCurrentPage - 1) * JEFATURA_PAGE_SIZE,
     jefaturaCurrentPage * JEFATURA_PAGE_SIZE
   );
   // renderizar `paginated` en vez de `pending`
   renderJefaturaPagination(totalPages);
   ```
3. Añadir función `renderJefaturaPagination(totalPages)` que inyecta controles Prev/Página/Next en un contenedor `<div id="jefatura-pagination">` debajo de la tabla.
4. En `dashboard.html`, añadir `<div id="jefatura-pagination" class="pagination-bar"></div>` después del `</div>` de cierre del card.

**Nota:** Si los pendientes siempre serán < 20 en el período de adopción, puede diferirse y solo implementar cuando la demo real tenga más de 15 registros.

### Criterio de aceptación
- Con más de 15 registros pendientes, se muestran solo los primeros 15 con controles Prev/Next.
- La navegación no recarga datos del API (paginación client-side sobre el array ya obtenido).
- Con ≤ 15 registros, los controles de paginación no se muestran.

---

## 7. Dependencia / Centro de Costo para perfil jefatura

### Hallazgo

**Backend:**
- `UsuarioResumenResponse.cs`: campos `UnidadID` (int), `JefaturaID` (int nullable) — solo IDs, sin nombre.
- `UsuarioResumenDto.cs`: igual — `UnidadId`, `JefaturaId` — sin `UnidadNombre`.
- SQL en `JustificacionesSql.cs` L170: `u.UnidadId AS SolicitanteUnidadID` — join solo al ID, no al nombre de la unidad.
- La tabla `RecursosHumanos.EstructuraOrganizacional` tiene campo `Nombre` y `CodigoOrigen` pero no se une en ninguna query de jefatura.

**Frontend:**
- El panel expandible de detalle (`detail-inner`) muestra: ID Boleta, Funcionario, Motivo, Líneas, Tipo, Detalle completo.
- **No hay campo de Dependencia ni Centro de Costo.**
- `UnidadID` recibido en respuesta no se renderiza.

### Cambio requerido — 3 capas

#### 7a. Backend — SQL (`JustificacionesSql.cs`)
Añadir JOIN a `EstructuraOrganizacional` y alias `UnidadNombre`:
```sql
LEFT JOIN RecursosHumanos.EstructuraOrganizacional eo
    ON eo.EstructuraOrganizacionalId = u.UnidadId
-- Agregar en SELECT:
    eo.Nombre AS SolicitanteUnidadNombre,
```

#### 7b. Backend — DTO y Response

**`UsuarioResumenDto.cs`** — añadir:
```csharp
public string UnidadNombre { get; set; } = string.Empty;
```

**`UsuarioResumenResponse.cs`** — añadir:
```csharp
public string UnidadNombre { get; set; } = string.Empty;
```

**`JustificacionRepository.cs`** — mapear campo nuevo:
```csharp
UnidadNombre = encabezado.SolicitanteUnidadNombre ?? string.Empty,
```

**`JefaturaController.cs`** — incluir en el mapping del `Solicitante`:
```csharp
UnidadNombre = result.Solicitante.UnidadNombre,
```

#### 7c. Frontend — `app.js` + `dashboard.html`

En la fila de detalle expandida (`detail-inner`), añadir un campo:
```js
// En el template string del detail-row:
<div class="detail-field"><label>Dependencia / Unidad</label><p>${escapeHtml(data?.solicitante?.unidadNombre || 'No disponible')}</p></div>
```

La normalización de `solicitante` proviene del campo `unidadNombre` (camelCase del JSON).

**Archivos afectados:**
| Archivo | Cambio |
|---|---|
| `backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs` | JOIN + alias `SolicitanteUnidadNombre` |
| `backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs` | Mapear `UnidadNombre` en `UsuarioResumenDto` |
| `backend/src/IntegradorMarcas.Application/DTOs/UsuarioResumenDto.cs` | Añadir `UnidadNombre` |
| `backend/src/IntegradorMarcas.Api/Contracts/Responses/UsuarioResumenResponse.cs` | Añadir `UnidadNombre` |
| `backend/src/IntegradorMarcas.Api/Controllers/JefaturaController.cs` | Pasar `UnidadNombre` en mapping |
| `app.js` | Renderizar `unidadNombre` en panel de detalle jefatura |

### Criterio de aceptación
- Al expandir "Ver detalle" en una boleta jefatura, aparece el campo "Dependencia / Unidad" con el nombre legible de la unidad del solicitante.
- Si la unidad no existe en `EstructuraOrganizacional`, el campo muestra "No disponible".
- El cambio no rompe la response existente (campo nuevo, no reemplaza).

---

## Resumen de cambios por archivo

| Archivo | Items | Prioridad |
|---|---|---|
| `app.js` | #1 (label rol), #3 (filtros fecha sifcnp), #4 Fase1 (datos JS), #5 (fn download jefatura), #6 (paginación), #7c (renderizar unidad) | Alta |
| `dashboard.html` | #5 (botón reporte jefatura), #6 (div paginación) | Alta |
| `backend/.../UsuarioResumenDto.cs` | #7b (añadir UnidadNombre) | Alta |
| `backend/.../UsuarioResumenResponse.cs` | #7b (añadir UnidadNombre) | Alta |
| `backend/.../JustificacionesSql.cs` | #7a (JOIN unidad) | Alta |
| `backend/.../JustificacionRepository.cs` | #7b (mapear UnidadNombre) | Alta |
| `backend/.../JefaturaController.cs` | #7b (pasar UnidadNombre) | Alta |
| `docs/db/003_integra_marcas_seed_demo.sql` (nuevo) | #2 (seed demo) | Media |

---

## Orden de implementación recomendado

1. **#1** — Label rol (1 línea en app.js, sin riesgo)
2. **#3** — Filtros fecha sifcnp (extensión función existente)
3. **#4 Fase 1** — Mover hardcoded a JS array
4. **#5** — Botón + función reporte jefatura
5. **#7** — Dependencia/unidad (backend primero, luego frontend)
6. **#6** — Paginación (puede diferirse si no hay volumen)
7. **#2** — Script seed demo (independiente, puede ir en paralelo)
