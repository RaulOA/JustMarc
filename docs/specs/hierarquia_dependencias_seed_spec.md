# Spec: Jerarquia de Dependencias y Seed de Aprobadores

## Objetivo
Definir como modelar e insertar una nueva jerarquia organizacional en la BD actual, con datos demo realistas (cedula, nombre, correo), cumpliendo reglas de aprobacion y validando impacto en API/frontend.

Jerarquia solicitada:
- Gerencia > Jefatura de UTI, Jefatura de Subgerencia
- Subgerencia > Jefatura de Programas Especiales
- UTI > Unidad de Tecnologias de Informacion
- DAF > Direccion Administrativa Financiera > Recursos Humanos, Contabilidad, Proveeduria

Reglas:
- Si una dependencia tiene dependencias hijas, la jefatura subordinada la aprueba la jefatura superior.
- Cada dependencia: 5 funcionarios + 1 jefe.

## Hallazgos Tecnicos Relevantes

### 1) Esquema funcional real de BD
Los scripts base modernos crean tablas en esquemas funcionales:
- RecursosHumanos.Usuario
- RecursosHumanos.EstructuraOrganizacional
- Operacion.JerarquiaAprobacion
- Operacion.DelegacionAprobacion
- Operacion.Justificacion
- Operacion.JustificacionDetalle

Referencias:
- docs/db/001_integra_marcas_base_inicial.sql
- docs/db/004_seed_esquema_correcto.sql

### 2) Como resuelve hoy el aprobador
La logica de aprobacion en runtime depende de la funcion:
- dbo.fn_AprobadoresVigentesPorSolicitante(@SolicitanteUsuarioID, GETDATE())

Y se usa en:
- listado pendientes de jefatura
- validacion de alcance para resolver
- endpoint de aprobador actual

Referencias:
- backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs
- docs/db/fix_fn_aprobadores.sql

La funcion trabaja asi:
1. Busca la estructura del solicitante por coincidencia:
   - RecursosHumanos.EstructuraOrganizacional.CodigoOrigen = CAST(RecursosHumanos.Usuario.UnidadId AS varchar)
2. Toma jerarquias activas por esa estructura (Operacion.JerarquiaAprobacion).
3. Suma delegaciones activas (Operacion.DelegacionAprobacion).
4. En consultas de aprobador actual se prioriza Delegacion sobre Jerarquia.

### 3) Dependencia legacy dbo que debe verificarse
El backend SQL actual referencia objetos dbo legacy:
- dbo.fn_AprobadoresVigentesPorSolicitante
- dbo.Estructuras_Organizacionales

No existe en scripts actuales una creacion explicita de dbo.Estructuras_Organizacionales, y fix_fn_aprobadores.sql hace ALTER (no CREATE) de dbo.fn_AprobadoresVigentesPorSolicitante.

Implicacion:
- Antes de cualquier seed nuevo, validar compatibilidad de objetos dbo necesarios en el ambiente.

### 4) Seeds existentes y usuarios demo actuales
Seed funcional vigente para demo:
- docs/db/004_seed_esquema_correcto.sql

Incluye usuarios:
- JEFE-DEMO (RolId=2)
- ANA-DEMO (RolId=1)
- LUIS-DEMO (RolId=1)
- RRHH-DEMO (RolId=3)

Frontend (mock identidad) tiene mapeo fijo por username:
- funcionario.ana -> userId 4
- jefe.maria -> userId 3
- rrhh.carlos -> userId 6

Referencia:
- app.js

## Tablas/Columnas Exactas a Actualizar

### A) RecursosHumanos.EstructuraOrganizacional
Columnas a poblar:
- Nombre
- CodigoOrigen (string numerico, debe empatar con Usuario.UnidadId)
- EstructuraPadreId
- EstadoRegistroId (1 activo)
- VigenciaDesde
- VigenciaHasta (NULL)
- CreadoPor
- FechaHoraCreacion

### B) RecursosHumanos.Usuario
Columnas a poblar:
- Cedula
- NombreCompleto
- CorreoElectronico
- JefaturaId
- UnidadId
- RolId (1 funcionario, 2 jefatura)
- Compania
- CreadoPor
- FechaHoraCreacion

### C) Operacion.JerarquiaAprobacion
Columnas a poblar:
- AprobadorUsuarioId
- EstructuraOrganizacionalId
- NivelAprobacion (1)
- TipoRelacion ('Vertical')
- EstadoRegistroId (1)
- VigenciaDesde
- VigenciaHasta (NULL)
- CreadoPor
- FechaHoraCreacion

### D) Opcional: Operacion.DelegacionAprobacion
Solo si se requiere contingencia temporal para escalamiento sin cambiar estructura.

## Restriccion Importante del Modelo Actual
La resolucion de aprobador es por UnidadId (estructura), no por tipo de usuario dentro de la misma dependencia.

Consecuencia:
- En una misma UnidadId, jefe y funcionarios comparten el mismo conjunto de aprobadores vigentes.

Para cumplir estrictamente la regla de "jefatura subordinada aprobada por jefatura superior" sin romper aprobacion local de funcionarios, la opcion mas segura es modelar nodos de jefatura como dependencias separadas (como ya sugiere el requerimiento: Jefatura de UTI, Jefatura de Subgerencia, etc.).

## Modelo Propuesto para la Jerarquia Solicitada

## 1) Arbol de estructuras
Se propone este arbol (12 dependencias):
1. Gerencia
2. Jefatura de UTI (hija de Gerencia)
3. UTI (hija de Jefatura de UTI)
4. Unidad de Tecnologias de Informacion (hija de UTI)
5. Jefatura de Subgerencia (hija de Gerencia)
6. Subgerencia (hija de Jefatura de Subgerencia)
7. Jefatura de Programas Especiales (hija de Subgerencia)
8. DAF
9. Direccion Administrativa Financiera (hija de DAF)
10. Recursos Humanos (hija de Direccion Administrativa Financiera)
11. Contabilidad (hija de Direccion Administrativa Financiera)
12. Proveeduria (hija de Direccion Administrativa Financiera)

## 2) CodigoOrigen/UnidadId sugeridos
Usar codigos nuevos para no colisionar con demo actual (120):
- 5100 Gerencia
- 5110 Jefatura de UTI
- 5120 UTI
- 5130 Unidad de Tecnologias de Informacion
- 5140 Jefatura de Subgerencia
- 5150 Subgerencia
- 5160 Jefatura de Programas Especiales
- 5200 DAF
- 5210 Direccion Administrativa Financiera
- 5220 Recursos Humanos
- 5230 Contabilidad
- 5240 Proveeduria

## 3) Regla de aprobacion implementable
Para cada dependencia D:
- Insertar 1 jefe (RolId=2) en UnidadId=D.
- Insertar 5 funcionarios (RolId=1) en UnidadId=D con JefaturaId=Jefe de D.
- JerarquiaAprobacion de D apunta al jefe de la dependencia padre de D.

Notas:
- Para nodos raiz (Gerencia, DAF), dejar jerarquia con su propio jefe o definir super-jefatura explicita.
- Si se deja autojefatura en raiz, el jefe raiz no podra autoaprobarse (regla de negocio evita autoaprobacion), pero no afecta aprobacion de subalternos.

## Dataset Demo Propuesto (Nombres Ficticios Realistas)
Convencion:
- Cedulas ficticias formato local: x-xxxx-xxxx (string).
- Correos: nombre.apellido@cnp.local
- Compania: CNP

## Jefes (12)
1. 1-5100-0001, Laura Cascante Rojas, laura.cascante@cnp.local, Gerencia
2. 1-5110-0001, Mauricio Solano Vega, mauricio.solano@cnp.local, Jefatura de UTI
3. 1-5120-0001, Adriana Fallas Monge, adriana.fallas@cnp.local, UTI
4. 1-5130-0001, Esteban Matarrita Campos, esteban.matarrita@cnp.local, Unidad de Tecnologias de Informacion
5. 1-5140-0001, Daniela Quesada Brenes, daniela.quesada@cnp.local, Jefatura de Subgerencia
6. 1-5150-0001, Ricardo Segura Jimenez, ricardo.segura@cnp.local, Subgerencia
7. 1-5160-0001, Fernanda Urena Solis, fernanda.urena@cnp.local, Jefatura de Programas Especiales
8. 1-5200-0001, Oscar Alpizar Rojas, oscar.alpizar@cnp.local, DAF
9. 1-5210-0001, Melissa Chinchilla Pineda, melissa.chinchilla@cnp.local, Direccion Administrativa Financiera
10. 1-5220-0001, Karla Araya Vargas, karla.araya@cnp.local, Recursos Humanos
11. 1-5230-0001, Joaquin Cordero Mendez, joaquin.cordero@cnp.local, Contabilidad
12. 1-5240-0001, Pablo Villalobos Mora, pablo.villalobos@cnp.local, Proveeduria

## Funcionarios por dependencia (5 cada una)
Se recomienda generar 60 funcionarios con una tabla temporal semilla (VALUES) o CTE, usando cedulas 1-XXXX-00NN segun dependencia.

Ejemplo para cada dependencia D (NN = 01..05):
- 1-DDDD-0001, 1-DDDD-0002, 1-DDDD-0003, 1-DDDD-0004, 1-DDDD-0005
- Nombres sugeridos pool realista:
  - Andrea Rojas Mena
  - Jose Pablo Villalta Ruiz
  - Monica Cespedes Salas
  - Diego Badilla Murillo
  - Gabriela Nuñez Coto
  - Javier Montero Castro
  - Natalia Carvajal Arias
  - Cristian Calderon Soto
  - Mariana Obando Brenes
  - Luis Diego Chaves Campos

Observacion:
- Para evitar duplicidad de cedula/correo, consolidar lista final en una CTE UsersSeed con clave natural Cedula y correo unico.

## Script Nuevo Recomendado
Crear:
- docs/db/007_seed_hierarquia_dependencias.sql

No modificar 003_integra_marcas_seed_demo.sql (legacy dbo).
Tomar 004_seed_esquema_correcto.sql como base de estilo idempotente.

Estructura recomendada del script 007:
1. SET XACT_ABORT ON; BEGIN TRAN;
2. Precheck de objetos requeridos (tablas funcionales + dbo.fn_AprobadoresVigentesPorSolicitante).
3. UPSERT de 12 estructuras (por CodigoOrigen).
4. Resolucion de EstructuraPadreId en segunda pasada.
5. UPSERT de 12 jefes.
6. UPSERT de 60 funcionarios (5 por dependencia).
7. UPDATE de JefaturaId para funcionarios (jefe de su dependencia).
8. UPDATE de JefaturaId para jefes subordinados (jefe de dependencia padre).
9. UPSERT de Operacion.JerarquiaAprobacion (vigente y activa).
10. COMMIT;
11. Bloque de SELECTs de validacion.

## Validaciones SQL Obligatorias

### 1) Conteos esperados
- 12 estructuras nuevas.
- 12 jefes nuevos.
- 60 funcionarios nuevos.
- 12 jerarquias activas nuevas.

### 2) Integridad de jerarquia
- Toda estructura no raiz debe tener EstructuraPadreId valido.
- Todo funcionario debe tener JefaturaId no nulo.

### 3) Prueba de resolucion de aprobador
Para al menos 4 casos (uno por rama):
- Usuario funcionario en UTI
- Usuario jefe en UTI
- Usuario funcionario en Recursos Humanos
- Usuario jefe en Recursos Humanos

Ejecutar:
- SELECT * FROM dbo.fn_AprobadoresVigentesPorSolicitante(@UsuarioId, GETDATE())

Validar que el aprobador esperado este presente.

## Dependencias API/Frontend a Verificar

## Backend
Endpoints criticos:
- GET /api/justificaciones/aprobador-actual
- GET /api/jefatura/justificaciones/pendientes
- PATCH /api/jefatura/justificaciones/{id}/resolver
- GET /api/justificaciones/historico

Todos dependen indirectamente de dbo.fn_AprobadoresVigentesPorSolicitante.

## Frontend
Dependencias visibles:
- Topbar "Aprobador actual" en app.js (renderCurrentApproverTopbar)
- Bandeja jefatura en app.js (renderJefaturaRequests)

Riesgo de prueba:
- app.js usa MOCK_USER_DIRECTORY con pocos userId fijos.

Recomendacion para QA controlado:
- Agregar temporalmente 2-3 usuarios seed nuevos al MOCK_USER_DIRECTORY para navegar por UI con identidad deterministica.
- Alternativa sin tocar frontend: validar con curl usando headers X-User-Id/X-User-Role.

## Plan de Verificacion End-to-End (Seguro)
1. Ejecutar script 007 en ambiente local dev.
2. Probar SQL function con 4 usuarios muestra.
3. Levantar API y consultar /api/justificaciones/aprobador-actual por cada usuario.
4. Crear boleta con usuario funcionario de una dependencia hija.
5. Entrar como jefe aprobador esperado y confirmar aparece en pendientes.
6. Resolver boleta y validar trazabilidad en historico.
7. Repetir para una rama DAF.

## Riesgos y Mitigaciones
1. Riesgo: objetos dbo legacy faltantes.
- Mitigacion: precheck al inicio de 007; abortar con RAISERROR si faltan.

2. Riesgo: colision de datos demo previos.
- Mitigacion: usar Cedula/Correo/CodigoOrigen nuevos y UPSERT idempotente.

3. Riesgo: jefes sin aprobador en nodos raiz.
- Mitigacion: documentar excepcion o definir super-jefatura explicita.

4. Riesgo: inconsistencia de nombres con acentos.
- Mitigacion: guardar en codificacion UTF-8 y reutilizar script 006 si aparece mojibake.

## Archivos Exactos Implicados
- docs/db/001_integra_marcas_base_inicial.sql
- docs/db/004_seed_esquema_correcto.sql
- docs/db/fix_fn_aprobadores.sql
- backend/src/IntegradorMarcas.Infrastructure/Queries/JustificacionesSql.cs
- backend/src/IntegradorMarcas.Infrastructure/Repositories/JustificacionRepository.cs
- app.js
- Nuevo propuesto: docs/db/007_seed_hierarquia_dependencias.sql

## Resultado Esperado
Con este modelado y seed:
- Se persiste la jerarquia solicitada con datos demo consistentes.
- Cada dependencia queda con 5 funcionarios + 1 jefe.
- El flujo de resolucion de aprobador se mantiene alineado a la logica actual de API.
- La verificacion funcional puede hacerse por SQL, API y frontend de forma controlada e idempotente.
