# Especificacion de implementacion: Jefatura (sorting, paginacion visible y reporte dual)

## Objetivo
Corregir tres observaciones en el frontend de Jefatura/Consulta:
1. La tabla de Solicitudes Pendientes no permite orden asc/desc por columna.
2. La paginacion no se percibe cuando hay pocos datos.
3. El boton de reporte existe en Solicitudes, pero debe estar tambien en Consulta Historica (y mantenerse en ambas).

## Diagnostico actual

### Hallazgos en dashboard.html
- En la tabla de Jefatura ([dashboard.html](dashboard.html)), los encabezados son texto plano (sin `onclick`, sin `button`, sin `aria-sort`) en las columnas Funcionario, Motivo, Tipo, Fecha y Estado.
- Existe contenedor de paginacion [dashboard.html](dashboard.html) con id `jefatura-pagination`, pero no hay etiqueta de estado cuando solo hay 1 pagina.
- El boton `Descargar Reporte` de Jefatura esta en el header de Panel Jefatura con id `jefatura-download-btn`.
- En Panel SIFCNP (Consulta Historica) no existe boton de descarga de reporte.

### Hallazgos en app.js
- Se mantiene el estado de lista en `jefaturaAllPending`, pagina en `jefaturaCurrentPage` y tamano de pagina `JEFATURA_PAGE_SIZE`.
- La funcion `renderJefaturaPageView()` solo pagina y renderiza; no aplica ordenamiento.
- `renderJefaturaPagination(totalPages)` oculta todo (`container.innerHTML = ''`) cuando `totalPages <= 1`; por eso no hay indicacion visible de paginacion con pocos datos.
- `downloadJefaturaReport()` exporta todas las filas de `jefaturaAllPending` a CSV.
- No hay funcion de reporte para Consulta Historica (`panel-sifcnp`).

## Cambios propuestos por archivo

## 1) dashboard.html

### 1.1 Encabezados ordenables en Jefatura
- Convertir las celdas `th` ordenables de la tabla de Solicitudes Pendientes en controles accesibles (recomendado: `button` dentro del `th`) para:
  - Funcionario
  - Motivo
  - Tipo
  - Fecha
  - Estado
- Mantener Acciones sin ordenamiento.
- Agregar atributos para accesibilidad y estado visual:
  - `aria-sort` en `th` (o gestion equivalente semantica).
  - Indicador visual de direccion (ejemplo: `^`/`v` o icono).

### 1.2 Paginacion siempre visible (incluso 0 o 1 pagina)
- Mantener el contenedor `#jefatura-pagination` siempre renderizado con un texto de estado minimo.
- Estado minimo sugerido:
  - 0 registros: `Pagina 0 de 0 - Sin resultados`.
  - 1 pagina: `Pagina 1 de 1` + controles deshabilitados.

### 1.3 Boton de reporte en Consulta Historica
- En el header de `panel-sifcnp`, agregar un boton `Descargar Reporte` adicional.
- Conservar el boton actual de Jefatura (no mover ni eliminar).
- El nuevo boton debe invocar una nueva funcion JS dedicada (ver cambios en `app.js`).

## 2) app.js

### 2.1 Estado de ordenamiento para Jefatura
- Agregar estado global de orden:
  - `jefaturaSortField` (nullable).
  - `jefaturaSortDirection` (`asc` | `desc`).
- Valor inicial recomendado:
  - Campo `fechaCreacion` en `desc` (mas reciente primero), o neutro si se prefiere no imponer orden inicial.

### 2.2 Handler de ordenamiento
- Crear funcion `setJefaturaSort(field)` con comportamiento:
  - Si `field` es distinto al actual: establecer `asc`.
  - Si `field` es igual al actual: alternar `asc` <-> `desc`.
  - Reiniciar a pagina 1 al cambiar orden.
  - Re-render con `renderJefaturaPageView()`.

### 2.3 Aplicar sorting antes de paginar
- En `renderJefaturaPageView()`:
  - Derivar una copia ordenada de `jefaturaAllPending` (no mutar el arreglo fuente si se desea mantener consistencia).
  - Aplicar comparadores por tipo de dato:
    - texto: `localeCompare('es', { sensitivity: 'base' })`
    - fecha: `new Date(...).getTime()`
  - Luego aplicar `slice` para pagina actual.

### 2.4 Indicadores de sort en encabezados
- Crear `renderJefaturaSortUI()` para:
  - Actualizar `aria-sort`.
  - Actualizar icono/flecha de columna activa.
  - Limpiar estado visual de columnas no activas.
- Invocar desde `renderJefaturaPageView()` tras cada render.

### 2.5 Paginacion visible aun con pocos datos
- Ajustar `renderJefaturaPagination(totalPages)`:
  - Eliminar el caso que deja `innerHTML = ''` cuando `totalPages <= 1`.
  - Renderizar siempre bloque de navegacion + resumen.
  - Deshabilitar botones cuando no aplique.

### 2.6 Reporte dual (Jefatura + Consulta)
- Mantener `downloadJefaturaReport()` para Solicitudes Pendientes (sin cambios funcionales mayores).
- Agregar nueva funcion para Consulta Historica, por ejemplo `downloadSifcnpReport()`:
  - Fuente de datos: resultado vigente en `#sifcnp-tbody` o arreglo filtrado en memoria (recomendado mantener arreglo de resultados filtrados para no leer DOM).
  - Formato: CSV UTF-8 con BOM, consistente con Jefatura.
  - Nombre sugerido: `Reporte_Consulta_SIFCNP_YYYYMMDD.csv`.
- Conectar el nuevo boton de `panel-sifcnp` a esta funcion.

## Criterios de aceptacion

1. Orden por columna
- Al hacer click en encabezados de Jefatura (Funcionario, Motivo, Tipo, Fecha, Estado), la tabla ordena ascendente/descendente alternando en cada click.
- La columna activa muestra indicador visual y estado accesible (`aria-sort`).
- La columna Acciones no cambia ni participa del sort.

2. Paginacion visible con pocos datos
- Con 0 registros pendientes: se muestra estado de paginacion visible (no vacio) y navegacion deshabilitada.
- Con 1..`JEFATURA_PAGE_SIZE` registros: se muestra `Pagina 1 de 1` y botones deshabilitados.
- Con mas de `JEFATURA_PAGE_SIZE`: paginacion funcional con Anterior/Siguiente.

3. Boton de reporte en ambas pestanas
- Existe boton Descargar Reporte en Panel Jefatura (Solicitudes Pendientes).
- Existe boton Descargar Reporte en Panel SIFCNP (Consulta Historica).
- Ambos botones funcionan de forma independiente y generan archivo CSV.

4. No regresiones
- Aprobar/Rechazar y Ver detalle siguen funcionando sin cambios de comportamiento.
- El conteo de pendientes (`#jefatura-pending-count`) se mantiene correcto.
- Los roles y visibilidad por rol no se alteran.

## Notas de implementacion
- Mantener nombres de funciones y estilo actual del proyecto para minimizar impacto.
- Preferir una sola fuente de verdad para resultados de Consulta Historica (arreglo en memoria) para exportar exactamente lo filtrado.
- Incluir prueba manual rapida por escenarios: 0, 1, 2 paginas; y orden por cada columna.
