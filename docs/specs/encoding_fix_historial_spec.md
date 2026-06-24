# Encoding Fix Spec - Historial Detalle (Mojibake)

## Objetivo

Investigar por que en el detalle del historial aparecen textos con mojibake (ejemplo: OmisiÃ³n), identificar causa(s) probable(s), y proponer una correccion segura de bajo riesgo.

## Hallazgos Relevantes

1. El frontend no realiza recodificacion manual del texto recibido.
- En app.js, los flujos de historial/detalle consumen JSON con fetch + response.json().
- Los campos afectados se renderizan como texto normal (escapeHtml/textContent), sin TextDecoder custom ni conversiones latin1/utf8.
- Rutas principales:
  - GET /api/justificaciones/mias -> renderFuncionarioHistory + normalizeApiResumen.
  - GET /api/justificaciones/{id}/lineas -> toggleFuncionarioHistoryDetail.
  - GET /api/jefatura/justificaciones/{id} -> toggleDetail (usa lineas y tipoJustificacionDescripcion).

2. El backend tampoco muestra transformaciones de encoding en la capa API/application.
- Controllers mapean strings DTO -> response sin conversion adicional.
- Program.cs usa pipeline normal ASP.NET Core y serializacion JSON por defecto.

3. Los strings con acentos en el repositorio fuente estan correctos (no hay OmisiÃ³n en codigo/scripts actuales).
- Se observan literales correctos como Omision/Omision con tilde segun archivo.
- No se encontraron cadenas mojibake en el workspace.

4. El flujo de detalle depende de Descripcion de TipoJustificacion desde SQL.
- JustificacionesSql.ListMineLineas y GetDetalleJefaturaLineas proyectan tj.Descripcion AS TipoJustificacionDescripcion.
- En UI, ese valor se muestra directamente en historial detalle.

5. En scripts de base de datos inicial, multiples columnas textuales son VARCHAR (no NVARCHAR).
- Ejemplos: Configuracion.TipoJustificacion.Descripcion VARCHAR(100), Operacion.JustificacionDetalle.ObservacionDetalle VARCHAR(250), Operacion.Justificacion.MotivoGeneral VARCHAR(500).

## Analisis de Causa Raiz Probable

Causa mas probable: dato ya corrupto en persistencia (data-at-rest), no un bug de render.

Razon:
- El patron OmisiÃ³n es tipico de bytes UTF-8 interpretados como Windows-1252/Latin-1 en alguna carga o migracion previa.
- Si el valor ya quedo guardado como texto mojibake en SQL, API y frontend lo propagan tal cual.
- Como el flujo historial/detalle lee Descripcion de catalogos y observaciones desde BD, el defecto aparece exactamente donde se consumen esos campos.

Factores que aumentan riesgo de recurrencia:
- Uso extendido de VARCHAR para datos con acentos.
- Integraciones/migraciones externas que pueden insertar texto sin contrato explicito de Unicode.

## Opciones de Correccion

### Opcion A: Parche en frontend (re-decoding heuristico)

Descripcion:
- Detectar patrones tipo Ã, Â y aplicar recodificacion en cliente antes de renderizar.

Pros:
- Rapido de aplicar.

Contras:
- Alto riesgo de falsos positivos y doble conversion.
- No corrige origen de datos ni otros consumidores (API, reportes, otros clientes).
- Puede ocultar datos inconsistentes.

Riesgo:
- Medio/alto. No recomendado como solucion principal.

### Opcion B: Parche en query/API (normalizacion en salida)

Descripcion:
- Intentar corregir texto al vuelo desde SQL o backend solo para endpoints afectados.

Pros:
- Mitiga impacto visual sin tocar esquema completo inicialmente.

Contras:
- Logica compleja y fragil para todos los casos.
- Mantiene BD potencialmente corrupta.
- Aumenta deuda tecnica y comportamiento no uniforme.

Riesgo:
- Medio. Solo recomendable como mitigacion temporal controlada.

### Opcion C: Correccion en fuente de verdad (BD + hardening de escritura)

Descripcion:
- Corregir registros mojibake existentes con script puntual.
- Endurecer esquema/texto para Unicode (NVARCHAR) en columnas de negocio/catalogo sensibles.
- Mantener frontend/backend sin hacks de recodificacion.

Pros:
- Solucion estructural y consistente.
- Evita recurrencia en todos los consumidores.
- Menor deuda tecnica a mediano plazo.

Contras:
- Requiere script de migracion y validacion de datos.

Riesgo:
- Bajo/medio si se hace por fases y con rollback.
- Recomendado.

## Implementacion Preferida (Bajo Riesgo)

Aplicar Opcion C en dos fases.

### Fase 1 - Contencion y observabilidad (sin romper contratos)

1. Auditoria de datos mojibake en tablas clave:
- Configuracion.TipoJustificacion.Descripcion
- Operacion.Justificacion.MotivoGeneral
- Operacion.JustificacionDetalle.ObservacionDetalle
- Operacion.Justificacion.ComentarioResolucion

2. Script de deteccion:
- Buscar patrones comunes: Ã, Â, �, secuencias sospechosas.
- Generar reporte (id, valor actual, propuesta de valor corregido).

3. Script de correccion controlada:
- Ejecutar UPDATE solo sobre filas confirmadas.
- Respaldar antes/despues (tabla temporal o backup logico).
- Probar en ambiente no productivo.

### Fase 2 - Prevencion de recurrencia

1. Migrar columnas de texto de VARCHAR a NVARCHAR en esquema propio de la app:
- Configuracion.TipoJustificacion.Descripcion -> NVARCHAR(100)
- Operacion.Justificacion.MotivoGeneral -> NVARCHAR(500)
- Operacion.Justificacion.ComentarioResolucion -> NVARCHAR(500)
- Operacion.JustificacionDetalle.ObservacionDetalle -> NVARCHAR(250)
- Opcionalmente: CreadoPor/ModificadoPor pueden permanecer VARCHAR si son tecnicos ASCII.

2. Revisar scripts semilla/integracion:
- Mantener archivos .sql en UTF-8.
- Para literales con acentos, preferir prefijo N'...' cuando aplique en inserciones directas.

3. No introducir recodificacion heuristica en frontend.
- El UI debe seguir mostrando texto tal cual recibe de API.

## Validacion Recomendada

1. Verificacion API directa:
- Probar GET /api/justificaciones/{id}/lineas y confirmar TipoJustificacionDescripcion con acentos correctos.
- Probar GET /api/justificaciones/mias y validar observaciones.

2. Verificacion UI:
- En dashboard historial, expandir detalle y confirmar texto correcto (Omisión, etc.).

3. No regresion:
- Ejecutar pruebas backend existentes (incluyendo historial y detalle de jefatura).
- Revisar que filtros/ordenamientos no cambian por conversion a NVARCHAR.

## Impacto en Archivos del Repositorio (si se implementa)

- docs/db/001_integra_marcas_base_inicial.sql (migrar tipos base a NVARCHAR en DDL)
- Nuevo script SQL incremental en docs/db para ALTER COLUMN y correccion de datos mojibake
- Sin cambios necesarios en app.js para solucion estructural
- Sin cambios necesarios en controllers/services para serializacion

## Decicion Recomendada

Preferir Opcion C (correccion en BD + migracion a Unicode) por ser la alternativa mas segura a mediano plazo y con menor riesgo funcional en frontend/backend.

Aplicar Opcion B solo como parche temporal si existe urgencia visual inmediata y mientras se ejecuta la correccion estructural.
