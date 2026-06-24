# PRP — Sistema Web "Integrador Marcas"
> **Product Requirements Plan** | Consejo Nacional de Producción (CNP) & FANAL  
> Versión: 1.1 | Basado en: Análisis y Diseño de SI v1.1 (16/10/2025)  
> Organización: Unidad de Tecnologías de la Información (UTI)  
> Última actualización: Junio 2026

---

## 📋 Tabla de Contenidos

1. [Resumen del Producto](#1-resumen-del-producto)
1.1 [Estado actual de implementación](#11-estado-actual-de-implementacion)
2. [Contexto y Problema](#2-contexto-y-problema)
3. [Stack Tecnológico](#3-stack-tecnologico)
4. [Roles del Sistema](#4-roles-del-sistema)
5. [Requerimientos Funcionales](#5-requerimientos-funcionales)
6. [Requerimientos No Funcionales](#6-requerimientos-no-funcionales)
7. [Arquitectura del Software](#7-arquitectura-del-software)
8. [Modelo de Base de Datos](#8-modelo-de-base-de-datos)
9. [Lógica de Negocio y Flujos](#9-lógica-de-negocio-y-flujos)
10. [Integraciones Externas](#10-integraciones-externas)
11. [Checklist de Progreso de Desarrollo](#11-checklist-de-progreso-de-desarrollo)

---

## 1. Resumen del Producto

| Campo | Detalle |
|---|---|
| **Nombre del sistema** | Integrador Marcas / Sistema de Justificación de Marcas |
| **Tipo** | Aplicación Web (desarrollo interno) |
| **Institución** | Consejo Nacional de Producción (CNP) y FANAL |
| **Reemplaza a** | Sistema de escritorio SICNP (obsoleto, incompatible con Windows 11) |
| **Base de datos nueva** | `INTEGRA_CNP` en SQL Server 2019 R14 |
| **Prototipo base** | 19 de diciembre de 2025 |
| **Entrega final** | 31 de marzo de 2026 |
| **Metodología** | Prototipado iterativo |
| **Elaborado por** | Lic. Luis Diego Vega Soto, M.Ed. |
| **Técnico programador** | Sr. Raúl Ortega Acuña |
| **DBA** | Rudy Antonio Arias Rodríguez |
| **Última actualización de implementación** | Junio 2026 |

---

## 1.1 Estado actual de implementación

A junio de 2026, el proyecto cuenta con una implementación funcional de los flujos principales en frontend y backend. Lo ya disponible incluye:

- API REST con autenticación basada en headers `X-User-Id` y `X-User-Role`.
- Frontend estático con login y paneles por rol para `ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH` y `ROL_ADMIN`.
- Creación de justificaciones con cabecera + líneas, validación de mínimo 1 detalle y estado inicial `Pendiente Jefatura`.
- Historial de justificaciones propias, listado de pendientes de jefatura, y resolución de boletas (aprobar/rechazar).
- Consulta RRHH con filtros y descarga CSV en la interfaz.
- Consulta histórica tipo SIFCNP de solo lectura con filtros por funcionario y rango de fechas.
- Panel administrativo básico con endpoints y UI para dependencias, jerarquías, delegaciones y monitoreo de eventos/errores.
- Esquema de base de datos `INTEGRA_CNP` con scripts de creación, catálogos y seed de datos principales.

> Nota: la sincronización automática desde WIZDOM no está implementada aún; la tabla de usuarios se gestiona en el esquema local de `RecursosHumanos`.

---

## 2. Contexto y Problema

### Problema actual
El proceso de **Justificación de Marcas** (registro de tardías, omisiones, ausencias, etc.) se gestiona actualmente mediante el sistema de escritorio **SICNP**, el cual presenta:

- Incompatibilidad con **Windows 11** y sistemas operativos modernos.
- Interrupción crítica del servicio para un número creciente de funcionarios.
- Imposibilidad de escalar o mantener la aplicación de escritorio.

### Solución propuesta
Desarrollar un **sistema web moderno, centralizado y basado en roles** que replique y mejore la funcionalidad de justificación de marcas del antiguo SICNP, garantizando continuidad operativa para todos los funcionarios de CNP y FANAL.

---

## 3. Stack Tecnológico

| Capa | Tecnología |
|---|---|
| **Frontend** | HTML5, CSS3, JavaScript |
| **Backend** | C# (.NET) |
| **Base de datos principal** | Microsoft SQL Server 2019 R14 (`INTEGRA_CNP`) |
| **BD lectura ERP** | WIZDOM (vistas de solo lectura, proveedor OPTEC) |
| **BD histórica** | SIFCNP (solo lectura, consulta histórica) |
| **Navegadores soportados** | Chrome, Edge, Firefox |
| **Arquitectura** | Monolítica 3 capas |

---

## 4. Roles del Sistema

| Rol | Código | Descripción |
|---|---|---|
| **Funcionario** | `ROL_FUNC` | Puede crear boletas de justificación y consultar el historial de las propias. |
| **Jefatura / Aprobador** | `ROL_JEFE` | Puede ver y resolver las justificaciones que le correspondan según la jerarquía de aprobación activa o una delegación vigente. |
| **Recursos Humanos** | `ROL_RRHH` | Puede consultar **todas** las justificaciones de todos los funcionarios. Acceso de solo lectura para control administrativo y planilla, sin privilegios administrativos ni auditoría. |
| **Administrador** | `ROL_ADMIN` | Puede administrar jerarquías de aprobación, delegados o subaprobadores, catálogos, estructuras organizacionales y consultar en exclusividad la auditoría del sistema. |

> ⚠️ **Nota de seguridad:** El acceso al sistema estará basado estrictamente en roles. La autenticación se integrará con los usuarios del dominio CNP-FANAL. El rol Administrador se formaliza como actor del sistema con privilegios acotados al ámbito administrativo y de trazabilidad.

---

## 5. Requerimientos Funcionales

### RF-01 — Gestión de Usuarios
- El sistema debe **poblar y sincronizar** su tabla de usuarios a partir de **vistas de solo lectura** de la base de datos del ERP WIZDOM.
- Debe incluir funcionarios de **CNP (código 001)** y **FANAL (código 002)**.
- Los datos sincronizados incluyen: cédula, nombre completo, correo institucional, jefatura directa, unidad organizacional, compañía y rol.
- La jefatura directa importada se conservará como dato de referencia, pero no como regla única de aprobación.

---

### RF-02 — Creación de Boleta de Justificación
- Un funcionario con rol `ROL_FUNC` puede **crear una nueva boleta** de justificación de marca.
- La boleta tiene dos partes:
  - **Encabezado:** motivo general de la solicitud.
  - **Detalle (líneas):** uno o más conceptos, cada uno con:
    - Tipo de justificación.
    - Fecha de la marca a justificar.
    - Observación específica (opcional).
- El estado inicial de toda boleta creada es **`Pendiente Jefatura`**.
- El sistema debe permitir **agregar y eliminar líneas de detalle** antes de guardar.

---

### RF-03 — Aprobación / Rechazo por Jerarquía Configurada
- Un usuario con rol `ROL_JEFE` puede ver y resolver únicamente las justificaciones que le correspondan según la jerarquía de aprobación activa y las delegaciones vigentes.
- La jefatura o aprobador puede:
  - **Aprobar** la solicitud completa → estado cambia a `Aprobado`.
  - **Rechazar** la solicitud completa → estado cambia a `Rechazado`.
- La acción de aprobación o rechazo debe registrar: aprobador efectivo, rol con el que actúa, fecha y resultado.

---

### RF-04 — Consulta por Recursos Humanos
- Un usuario con rol `ROL_RRHH` puede **consultar todas las justificaciones** de todos los funcionarios sin restricción.
- Debe poder filtrar por: funcionario, estado, fecha y compañía (CNP/FANAL).
- El acceso es de **solo consulta**; RRHH no aprueba, no rechaza y no administra jerarquías, delegaciones ni auditoría.
- Especial interés en las justificaciones con estado `Aprobado` para aplicación en planilla.

---

### RF-05 — Consulta de Historial por Funcionario
- Un usuario con rol `ROL_FUNC` puede **consultar el historial y estado** de sus propias justificaciones.
- Debe poder ver: fecha de creación, estado actual, detalles de las líneas y, si fue procesada, el nombre del aprobador efectivo y fecha de resolución.

---

### RF-06 — Consulta de Registros Históricos (SIFCNP)
- El sistema debe proveer una **interfaz de solo lectura** para consultar registros del sistema antiguo SIFCNP.
- Esta interfaz no permite crear, editar ni eliminar datos históricos.
- Su propósito es la **consulta de antecedentes** de justificaciones pasadas.

---

### RF-07 — Auditoría Persistente del Sistema
- El sistema debe almacenar en base de datos una bitácora persistente de acciones operativas y administrativas relevantes.
- Cada evento debe guardar al menos: fecha y hora, identificador del usuario, nombre del usuario, rol, tipo de evento, descripción normalizada o catalogada, resultado y referencia funcional cuando aplique.
- La consulta de auditoría será exclusiva del `ROL_ADMIN`.
- Debe existir un dashboard administrativo con visualización paginada, filtros y descarga de reportes.

---

### RF-08 — Configuración de Jerarquía de Aprobaciones
- El sistema debe permitir definir una jerarquía de aprobación configurable dentro de `INTEGRA_CNP`.
- Debe soportar relaciones **verticales** y **horizontales** entre unidades, responsables y niveles aprobadores.
- La configuración de jerarquías solo podrá ser administrada por `ROL_ADMIN`.

---

### RF-09 — Gestión de Delegados y Subaprobadores
- El sistema debe permitir registrar delegados o subaprobadores para cubrir ausencias y otros escenarios operativos.
- Los delegados tendrán el mismo alcance de aprobación del delegante durante la vigencia configurada.
- El alta, baja, habilitación y deshabilitación de delegaciones solo podrá realizarla `ROL_ADMIN`.

---

### RF-10 — Catálogos y Estructuras Organizacionales de Soporte
- El sistema debe mantener catálogos y estructuras internas para representar jerarquías, niveles, delegaciones, eventos de auditoría, resultados y estados administrativos activo/inactivo.
- Esta lógica de jerarquía configurable, delegación y auditoría persistente **no existe hoy en las fuentes actuales**; debe ser creada por el sistema.

---

## 6. Requerimientos No Funcionales

| ID | Categoría | Descripción |
|---|---|---|
| RNF-01 | **Seguridad** | Acceso basado en roles (`ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH`, `ROL_ADMIN`). Segregación estricta entre operación, consulta y administración. |
| RNF-02 | **Rendimiento** | Las consultas a la BD deben ser eficientes. Las vistas de WIZDOM deben consultarse con impacto mínimo. La auditoría y los paneles administrativos deben usar paginación. |
| RNF-03 | **Compatibilidad** | Compatible con Chrome, Edge y Firefox en sus versiones modernas. No requiere instalación local. |
| RNF-04 | **Usabilidad** | Interfaz intuitiva, con curva de aprendizaje mínima. Inspirada en el flujo anterior para usuarios operativos y con dashboard diferenciado para Administración. |
| RNF-05 | **Trazabilidad** | Toda acción crítica y administrativa debe dejar evidencia persistente, consultable y exportable. |
| RNF-06 | **Continuidad Operativa** | Las delegaciones vigentes deben evitar bloqueos del flujo de aprobación por ausencias o recargos. |
| RNF-07 | **Configurabilidad** | La jerarquía de aprobación y los delegados deben poder ajustarse sin depender de cambios en WIZDOM o SIFCNP. |

---

## 7. Arquitectura del Software

```
┌─────────────────────────────────────────────────────┐
│            CAPA DE PRESENTACIÓN (Frontend)          │
│              HTML5 + CSS3 + JavaScript              │
└────────────────────────┬────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────┐
│          CAPA DE LÓGICA DE NEGOCIO (Backend)        │
│                    C# (.NET)                        │
│   - Flujo de creación de justificaciones            │
│   - Motor de aprobación jerárquica y delegaciones   │
│   - Módulo administrativo y dashboard de auditoría  │
│   - Control de acceso por roles y auditoría         │
│   - Notificaciones y trazabilidad persistente       │
└──────┬──────────────────┬──────────────────┬────────┘
       │                  │                  │
┌──────▼──────┐  ┌────────▼──────┐  ┌────────▼──────┐
│ INTEGRA_CNP │  │    WIZDOM     │  │    SIFCNP     │
│ SQL Srv2019 │  │ (Solo Lectura)│  │ (Solo Lectura)│
│  (R/W)      │  │  Vistas ERP   │  │  Históricos   │
└─────────────┘  └───────────────┘  └───────────────┘
```

### Descripción de capas

| Capa | Responsabilidad |
|---|---|
| **Presentación** | Renderizado de UI, formularios, listados, navegación por roles y dashboard administrativo. |
| **Lógica de Negocio** | Validaciones, flujos de estado, motor de aprobación jerárquica, control de permisos, delegaciones, notificaciones y auditoría. |
| **Acceso a Datos** | CRUD en `INTEGRA_CNP`, lecturas a vistas de WIZDOM y SIFCNP, y persistencia de estructuras internas de aprobación y trazabilidad. |

---

## 8. Modelo de Base de Datos

> **Base de datos:** `INTEGRA_CNP` | **Motor:** SQL Server 2019 R14 | **Normalización:** 3FN

---

### 8.1 Diagrama de Relaciones (texto)

```
Roles (1) ──────< Usuarios (N)
Cat_EstadosRegistro (1) ──────< Estructuras_Organizacionales (N)
Cat_EstadosRegistro (1) ──────< Jerarquias_Aprobacion (N)
Cat_EstadosRegistro (1) ──────< Delegaciones_Aprobacion (N)
Cat_TiposEventoAuditoria (1) ─< Auditoria_Eventos (N) >─ Cat_ResultadosAuditoria (1)

Estados (1) ────< Justificaciones_Encabezado (N)
Justificaciones_Encabezado (1) ──────< Justificaciones_Detalle (N)
Cat_TiposJustificacion (1) ──────< Justificaciones_Detalle (N)
```

---

### 8.2 Tablas Catálogo y Configuración

#### Tabla: `Roles`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `RolID` | int | PK | NO | Identificador único del rol |
| `NombreRol` | varchar(50) | — | NO | Funcionario, Jefatura, RRHH, Administrador |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |

**Datos semilla:**
```sql
INSERT INTO Roles (RolID, NombreRol) VALUES
(1, 'Funcionario'),
(2, 'Jefatura'),
(3, 'RRHH'),
(4, 'Administrador');
```

#### Tabla: `Cat_EstadosRegistro`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `EstadoRegistroID` | int | PK | NO | Estado administrativo |
| `Descripcion` | varchar(50) | — | NO | Activo / Inactivo |

#### Tabla: `Cat_TiposEventoAuditoria`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `TipoEventoAuditoriaID` | int | PK | NO | Tipo de evento auditable |
| `Descripcion` | varchar(100) | — | NO | Creación de boleta, aprobación, rechazo, alta de delegación, cambio de jerarquía, etc. |

#### Tabla: `Cat_ResultadosAuditoria`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `ResultadoAuditoriaID` | int | PK | NO | Resultado del evento |
| `Descripcion` | varchar(50) | — | NO | Éxito / Fallo / Denegado / Reintentado |

#### Tabla: `Jerarquias_Aprobacion`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `JerarquiaAprobacionID` | int | PK | NO | Regla jerárquica configurable |
| `AprobadorUsuarioID` | int | FK → Usuarios | NO | Usuario aprobador titular |
| `EstructuraOrganizacionalID` | int | — | NO | Alcance configurado |
| `NivelAprobacion` | int | — | NO | Nivel aprobador |
| `TipoRelacion` | varchar(20) | — | NO | Horizontal / Vertical |
| `EstadoRegistroID` | int | FK → Cat_EstadosRegistro | NO | Activo / Inactivo |

#### Tabla: `Delegaciones_Aprobacion`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `DelegacionAprobacionID` | int | PK | NO | Delegación o subaprobación |
| `DeleganteUsuarioID` | int | FK → Usuarios | NO | Aprobador titular |
| `DelegadoUsuarioID` | int | FK → Usuarios | NO | Delegado o subaprobador |
| `EstadoRegistroID` | int | FK → Cat_EstadosRegistro | NO | Activo / Inactivo |
| `VigenciaDesde` | datetime | — | NO | Inicio de vigencia |
| `VigenciaHasta` | datetime | — | SÍ | Fin de vigencia |

---

### 8.3 Tablas Operativas

#### Tabla: `Usuarios`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `UsuarioID` | int | PK | NO | Identificador único del usuario |
| `Cedula` | varchar(20) | — | NO | Cédula del funcionario |
| `NombreCompleto` | varchar(150) | — | NO | Nombre completo |
| `Correo` | varchar(100) | — | NO | Correo electrónico institucional |
| `JefaturaID` | int | — | SÍ | Jefatura directa importada como referencia |
| `UnidadID` | int | — | NO | ID de la unidad organizacional |
| `RolID` | int | FK → Roles | NO | Rol asignado en el sistema |
| `Compania` | varchar(10) | — | NO | Compañía: `CNP` o `FANAL` |

#### Tabla: `Justificaciones_Encabezado`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `JustificacionID` | int | PK | NO | Identificador único de la justificación |
| `UsuarioID` | int | FK → Usuarios | NO | ID del funcionario solicitante |
| `MotivoGeneral` | varchar(500) | — | NO | Observación general de la solicitud |
| `EstadoID` | int | FK → Estados | NO | Estado actual del flujo |
| `AprobadorID` | int | FK → Usuarios | SÍ | Aprobador efectivo |
| `FechaAprobacion` | datetime | — | SÍ | Fecha de aprobación o rechazo |
| `RolResolucion` | varchar(20) | — | SÍ | Rol con el que se procesó |

#### Tabla: `Auditoria_Eventos`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `AuditoriaEventoID` | bigint | PK | NO | Identificador del evento |
| `FechaEvento` | datetime | — | NO | Fecha y hora |
| `UsuarioID` | int | FK → Usuarios | SÍ | Usuario que ejecutó la acción |
| `NombreUsuario` | varchar(150) | — | NO | Nombre del usuario |
| `RolCodigo` | varchar(20) | — | NO | Rol con el que actuó |
| `TipoEventoAuditoriaID` | int | FK → Cat_TiposEventoAuditoria | NO | Tipo de evento |
| `ResultadoAuditoriaID` | int | FK → Cat_ResultadosAuditoria | NO | Resultado |
| `ReferenciaFuncional` | varchar(100) | — | SÍ | Referencia a boleta, jerarquía o delegación |

> La jerarquía configurable, delegaciones y auditoría persistente deben residir en `INTEGRA_CNP`; no existen hoy en WIZDOM ni SIFCNP.

---

## 9. Lógica de Negocio y Flujos

### 9.1 Flujo principal de una Justificación

```
[FUNCIONARIO]
      │
      ▼
  Crea boleta (Encabezado + 1..N Detalles)
      │
      ▼
  Estado: "Pendiente Jefatura"  ←── Estado inicial
      │
      │  Sistema determina el alcance según jerarquía activa en INTEGRA_CNP
      ▼
[APROBADOR EFECTIVO]
      │
      ├── Si existe delegación vigente, actúa el delegado con el mismo alcance
      │
      ├── Aprueba ──→ Estado: "Aprobado"  + registra AprobadorID + FechaAprobacion + RolResolucion
      │
      └── Rechaza ──→ Estado: "Rechazado" + registra AprobadorID + FechaAprobacion + RolResolucion
                              │
                              ▼
                    [RRHH] consulta justificaciones aprobadas
                    [ADMIN] administra jerarquías, delegaciones y auditoría
```

### 9.2 Reglas de negocio

| # | Regla |
|---|---|
| RN-01 | Una boleta debe tener al menos **una línea de detalle** para poder ser guardada. |
| RN-02 | El estado inicial de toda boleta nueva es siempre `Pendiente Jefatura` (EstadoID = 1). |
| RN-03 | Solo usuarios con autorización activa de aprobación, ya sea por jerarquía configurada o delegación vigente, pueden cambiar el estado de una boleta. |
| RN-04 | Una boleta en estado `Aprobado` o `Rechazado` **no puede ser modificada**. |
| RN-05 | Un funcionario solo puede ver **sus propias** justificaciones. |
| RN-06 | La visibilidad de jefatura se resuelve con tablas internas de jerarquía y delegación, no solo con `JefaturaID`. |
| RN-07 | El rol RRHH puede ver **todas** las justificaciones sin excepción, pero no la auditoría. |
| RN-08 | Los datos de la interfaz histórica (SIFCNP) son de **solo lectura**. |
| RN-09 | Toda acción administrativa o resolución de boleta genera una pista de auditoría persistente. |
| RN-10 | Las delegaciones o subaprobaciones solo pueden ser administradas por `ROL_ADMIN`. |
| RN-11 | La auditoría persistente es de acceso exclusivo para `ROL_ADMIN`. |

---

## 10. Integraciones Externas

### 10.1 WIZDOM (ERP — Solo Lectura)
- **Propósito:** Fuente de verdad para usuarios, estructura organizacional de origen y jefaturas directas.
- **Método:** Consulta a **vistas de BD** expuestas por WIZDOM. No se escriben datos en WIZDOM.
- **Datos obtenidos:** Cédula, nombre, correo, jefatura directa, unidad, compañía (CNP=001 / FANAL=002).
- **Límite funcional:** La lógica final de aprobaciones jerárquicas y delegaciones no existe en WIZDOM; debe ser construida por el sistema.

### 10.2 SIFCNP (Sistema histórico — Solo Lectura)
- **Propósito:** Consulta de registros históricos de justificaciones anteriores a la migración.
- **Método:** Acceso de solo lectura a la BD del sistema antiguo.
- **Restricción:** No se permite ninguna operación de escritura.
- **Interfaz:** Módulo separado de consulta con filtros básicos.
- **Límite funcional:** SIFCNP no participa en jerarquías, delegaciones ni auditoría operativa del nuevo sistema.

---

## 11. Checklist de Progreso de Desarrollo

> Usar esta sección para llevar el control del avance del proyecto. Marcar con `[x]` al completar.

### 🗄️ Base de Datos — INTEGRA_CNP

- [x] Crear base de datos `INTEGRA_CNP` en SQL Server 2019
- [x] Crear tabla `Roles` + insertar datos semilla
- [x] Crear tabla `Estados` + insertar datos semilla
- [x] Crear tabla `Cat_TiposJustificacion` + insertar datos semilla
- [x] Crear tabla `Cat_EstadosRegistro` + insertar datos semilla
- [x] Crear tabla `Cat_TiposEventoAuditoria` + insertar datos semilla
- [x] Crear tabla `Cat_ResultadosAuditoria` + insertar datos semilla
- [x] Crear tabla `Usuarios` con FK a `Roles`
- [x] Crear tabla `Jerarquias_Aprobacion`
- [x] Crear tabla `Delegaciones_Aprobacion`
- [x] Crear tabla `Justificaciones_Encabezado` con FKs a `Usuarios` y `Estados`
- [x] Crear tabla `Justificaciones_Detalle` con FKs a `Justificaciones_Encabezado` y `Cat_TiposJustificacion`
- [x] Crear tabla `Auditoria_Eventos`
- [x] Validar integridad referencial entre todas las tablas
- [x] Crear índices de rendimiento para aprobaciones, delegaciones y auditoría

### 🔌 Integraciones

- [ ] Identificar y documentar las vistas disponibles en WIZDOM
- [ ] Desarrollar capa de acceso a vistas de WIZDOM (solo lectura)
- [ ] Desarrollar proceso de sincronización de usuarios desde WIZDOM a `Usuarios`
- [x] Validar acceso de solo lectura a BD SIFCNP
- [x] Desarrollar capa de consulta histórica a SIFCNP

### 🔐 Autenticación y Roles

- [x] Definir mecanismo de autenticación (dominio CNP-FANAL)
- [x] Implementar autenticación de usuarios
- [x] Implementar control de acceso basado en roles (Funcionario / Jefatura / RRHH / Administrador)
- [x] Validar que cada rol solo accede a sus vistas y acciones permitidas

### 🖥️ Módulo: Gestión de Usuarios (RF-01)

- [x] Pantalla de administración de usuarios (sincronizados)
- [ ] Proceso automático/manual de sincronización desde WIZDOM
- [x] Asignación de rol por usuario

### 📝 Módulo: Creación de Justificación (RF-02)

- [x] Formulario de encabezado (motivo general)
- [x] Grilla de detalle: agregar líneas de detalle
- [x] Grilla de detalle: eliminar líneas de detalle
- [x] Selector de tipo de justificación (desde `Cat_TiposJustificacion`)
- [x] Selector de fecha de marca
- [x] Campo de observación por detalle (opcional)
- [x] Validación: mínimo 1 detalle antes de guardar (RN-01)
- [x] Guardar boleta con estado inicial `Pendiente Jefatura`
- [ ] Notificación automática a jefatura directa al crear boleta

### ✅ Módulo: Aprobación / Rechazo (RF-03)

- [x] Vista de jefatura: listado de justificaciones según jerarquía activa o delegación vigente
- [ ] Filtro por estado (Pendientes, Aprobadas, Rechazadas)
- [x] Acción: Aprobar boleta → actualizar estado + registrar AprobadorID + FechaAprobacion
- [x] Acción: Rechazar boleta → actualizar estado + registrar AprobadorID + FechaAprobacion
- [x] Bloquear modificación de boletas ya procesadas (RN-04)

### 🛠️ Módulo: Administración (RF-07 a RF-10)

- [x] Dashboard administrativo de auditoría con paginación, filtros y descarga de reportes
- [x] Pantalla de mantenimiento de jerarquías de aprobación
- [x] Pantalla de mantenimiento de delegados o subaprobadores
- [x] Pantalla de mantenimiento de catálogos y estructuras organizacionales
- [x] Restricción de acceso exclusiva para `ROL_ADMIN`

### 🔎 Módulo: Consulta RRHH (RF-04)

- [x] Vista de RRHH: listado de todas las justificaciones
- [x] Filtros: por funcionario, estado, fecha de creación, compañía (CNP/FANAL)
- [x] Vista de detalle de cada boleta
- [x] (Opcional) Exportación a Excel/CSV para uso en planilla

### 👤 Módulo: Historial del Funcionario (RF-05)

- [x] Vista del funcionario: historial de sus propias boletas
- [x] Detalle de cada boleta: líneas, estado, aprobador, fecha resolución
- [x] Indicador visual de estado (pendiente / aprobado / rechazado)

### 📂 Módulo: Consulta Histórica SIFCNP (RF-06)

- [x] Pantalla de consulta de registros históricos
- [x] Filtros básicos: funcionario, fecha, tipo de justificación
- [x] Vista de solo lectura (sin botones de edición)

### 🧪 Pruebas y Calidad

- [ ] Pruebas unitarias de lógica de negocio (flujo de estados)
- [ ] Pruebas de roles y permisos (cada rol solo ve lo que debe)
- [ ] Pruebas de jerarquía configurable horizontal y vertical
- [ ] Pruebas de delegación y subaprobación vigente
- [ ] Pruebas de auditoría persistente y restricción exclusiva para Administrador
- [ ] Pruebas de integración con WIZDOM
- [ ] Pruebas de integración con SIFCNP
- [ ] Pruebas de compatibilidad en Chrome, Edge y Firefox
- [ ] Validación con usuarios de RRHH
- [ ] Validación con jefaturas piloto
- [ ] Validación con funcionarios piloto

### 🚀 Despliegue

- [ ] Configurar ambiente de desarrollo
- [ ] Configurar ambiente de pruebas/QA
- [ ] **Prototipo base entregado** → 19 de diciembre de 2025
- [ ] Ajustes basados en retroalimentación del prototipo base
- [ ] Configurar ambiente de producción
- [ ] **Entrega final en producción** → 31 de marzo de 2026
- [ ] Documentación de usuario final (manual básico)
- [ ] Capacitación a usuarios piloto

---

## 📎 Notas para el Asistente de IA

> Esta sección proporciona contexto clave para cualquier modelo de IA que asista en el desarrollo.

- **Patrón de BD:** Encabezado-Detalle. `Justificaciones_Encabezado` es el maestro; `Justificaciones_Detalle` son sus líneas hijas. Siempre tratar ambas en la misma transacción al crear o eliminar.
- **Auto-referencia en Usuarios:** El campo `JefaturaID` apunta a otro `UsuarioID` en la misma tabla, pero se conserva como referencia. La autorización real se resuelve con `Jerarquias_Aprobacion` y `Delegaciones_Aprobacion`.
- **Control de visibilidad por rol:**
- Funcionario: `WHERE UsuarioID = @usuarioLogueado`
- Jefatura: alcance resuelto por la jerarquía activa o la delegación vigente.
- RRHH: sin filtro de usuario.
- Administrador: acceso a auditoría, jerarquías, delegaciones y catálogos.
- **Las tablas de WIZDOM y SIFCNP son de solo lectura.** Nunca generar código con INSERT/UPDATE/DELETE sobre esas bases.
- **Estados son IDs fijos:** 1=Pendiente Jefatura, 2=Aprobado, 3=Rechazado. Usar constantes en el código.
- **Compañías:** CNP = código `001`, FANAL = código `002`. En la tabla `Usuarios`, el campo `Compania` almacena `'CNP'` o `'FANAL'`.
- **Campos de auditoría:** Todas las tablas tienen `Usr_Registro` y `Fec_Registro`. Las tablas principales también tienen `Usr_Modifica` y `Fec_Modifica`.
- **Pistas de auditoría persistentes:** Registrar como mínimo fecha/hora, usuario, nombre, rol, tipo de evento, resultado y referencia funcional.
- **Capacidad nueva:** La jerarquía configurable, las delegaciones y la auditoría persistente no existen hoy en las fuentes actuales y deben implementarse en `INTEGRA_CNP`.

---

*Documento generado con base en el Análisis y Diseño de SI "Integrador Marcas" v1.1 — CNP / UTI — Octubre 2025*
