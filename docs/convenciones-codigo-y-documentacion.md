# Convenciones de Codigo y Documentacion

## 1. Proposito
Definir convenciones vigentes de codigo y de documentacion tecnica para mantener consistencia entre frontend, backend y manuales.

## 2. Alcance
- Convenciones observadas en el codigo actual del repositorio.
- Estandar documental para los manuales en docs/.

## 3. Fuente de verdad
- backend/src/**
- app.js, index.html, dashboard.html, style.css
- docs/*.md

## 4. Convenciones de codigo backend

### 4.1 Estructura y capas
- Api: controllers, contracts, seguridad y bootstrap (Program.cs).
- Application: interfaces, DTOs, validacion y servicios.
- Domain: constantes y elementos de dominio base.
- Infrastructure: repositorios y SQL.

### 4.2 Nomenclatura
- Clases y metodos: PascalCase.
- Interfaces: prefijo I (ejemplo: IJustificacionService).
- DTOs: sufijo Dto.
- Requests/Responses: sufijo Request/Response.
- Constantes de rol: ROL_FUNC, ROL_JEFE, ROL_RRHH.

### 4.3 Errores
- Errores de negocio/validacion: AppException con status explicito.
- Errores inesperados: handler global con ProblemDetails.
- Siempre que aplique, incluir correlationId para trazabilidad.

### 4.4 SQL
- SQL centralizado en clase estatica JustificacionesSql.
- Parametrizacion con Dapper (evitar concatenacion insegura).
- Control de concurrencia en resolucion via condicion en UPDATE.

## 5. Convenciones de codigo frontend

### 5.1 Estilo general
- JavaScript vanilla en un solo modulo app.js.
- Funciones en camelCase.
- Constantes en MAYUSCULAS con guion bajo o en objetos semanticos.

### 5.2 Estado y almacenamiento
- sessionStorage para sesion, tab activa y apiBaseUrl.
- Cache de detalle jefatura en memoria con Map.

### 5.3 Integracion API
- Fetch centralizado en apiFetch.
- Headers de identidad obligatorios en buildApiHeaders.
- Parseo de error centralizado en parseApiError.

## 6. Convenciones de endpoints y contratos
- Versionado actual implicito (sin prefijo /v1).
- JSON camelCase en payload HTTP.
- Rutas por contexto funcional:
  - /api/justificaciones
  - /api/jefatura/justificaciones
  - /api/rrhh/justificaciones

## 7. Estandar de documentacion tecnica

### 7.1 Estructura minima recomendada
Cada documento tecnico debe incluir:
1. Proposito
2. Alcance
3. Fuente de verdad
4. Contenido principal (tablas/diagramas)
5. Casos de error o limites
6. Checklist de validacion
7. Historial de cambios

### 7.2 Reglas de contenido
- Redactar en espanol tecnico, concreto y verificable.
- Evitar duplicar detalle de API en multiples manuales; usar referencias cruzadas.
- Mantener comandos ejecutables y rutas reales del repo.
- Registrar fecha de actualizacion en historial.

## 8. Checklist para PR (documentacion)
- Si cambia endpoint o contrato, actualizar docs/api-endpoints-reference.md.
- Si cambia flujo de rol, actualizar docs/flujos-datos-end-to-end.md.
- Si cambia arquitectura o dependencias, actualizar docs/arquitectura-codigo-actual.md.
- Si cambia operacion/arranque, actualizar README.md y docs/manual-tecnico.md.
- Si cambia estrategia de pruebas, actualizar docs/pruebas-estrategia-y-cobertura.md.

## 9. Checklist de validacion
- Convenciones listadas coinciden con codigo real.
- Estructura documental aplica en manuales nuevos.
- No hay duplicacion excesiva entre documentos.

## 10. Historial de cambios
- 2026-04-23: Documento creado con convenciones vigentes del repositorio.
