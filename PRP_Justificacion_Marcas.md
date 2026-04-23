# PRP — Sistema Web "Integrador Marcas"
> **Product Requirements Plan** | Consejo Nacional de Producción (CNP) & FANAL  
> Versión: 1.0 | Basado en: Análisis y Diseño de SI v1.1 (16/10/2025)  
> Organización: Unidad de Tecnologías de la Información (UTI)

---

## 📋 Tabla de Contenidos

1. [Resumen del Producto](#1-resumen-del-producto)
2. [Contexto y Problema](#2-contexto-y-problema)
3. [Stack Tecnológico](#3-stack-tecnológico)
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
| **Jefatura** | `ROL_JEFE` | Puede ver las justificaciones de sus subordinados directos y aprobar o rechazar. |
| **Recursos Humanos** | `ROL_RRHH` | Puede consultar **todas** las justificaciones de todos los funcionarios. Acceso de solo lectura para control administrativo y planilla. |

> ⚠️ **Nota de seguridad:** El acceso al sistema estará basado estrictamente en roles. La autenticación se integrará con los usuarios del dominio CNP-FANAL. Cada rol tiene visibilidad y acciones restringidas según su nivel.

---

## 5. Requerimientos Funcionales

### RF-01 — Gestión de Usuarios
- El sistema debe **poblar y sincronizar** su tabla de usuarios a partir de **vistas de solo lectura** de la base de datos del ERP WIZDOM.
- Debe incluir funcionarios de **CNP (código 001)** y **FANAL (código 002)**.
- Los datos sincronizados incluyen: cédula, nombre completo, correo institucional, jefatura directa, unidad organizacional, compañía y rol.

---

### RF-02 — Creación de Boleta de Justificación
- Un funcionario con rol `ROL_FUNC` puede **crear una nueva boleta** de justificación de marca.
- La boleta tiene dos partes:
  - **Encabezado:** motivo general de la solicitud.
  - **Detalle (líneas):** uno o más conceptos, cada uno con:
    - Tipo de justificación (Tardía, Omisión Marca de Entrada, Omisión Marca de Salida, Marca antes Hora de Salida, Ausencia, etc.)
    - Fecha de la marca a justificar.
    - Observación específica (opcional).
- El estado inicial de toda boleta creada es **`Pendiente Jefatura`**.
- El sistema debe permitir **agregar y eliminar líneas de detalle** antes de guardar.

---

### RF-03 — Aprobación / Rechazo por Jefatura
- Un usuario con rol `ROL_JEFE` puede ver **únicamente** las justificaciones de los funcionarios asignados a su jefatura.
- La jefatura puede:
  - **Aprobar** la solicitud completa → estado cambia a `Aprobado`.
  - **Rechazar** la solicitud completa → estado cambia a `Rechazado`.
- La acción de aprobación/rechazo debe registrar: ID del aprobador y fecha de la acción.

---

### RF-04 — Consulta por Recursos Humanos
- Un usuario con rol `ROL_RRHH` puede **consultar todas las justificaciones** de todos los funcionarios sin restricción.
- Debe poder filtrar por: funcionario, estado, fecha, compañía (CNP/FANAL).
- El acceso es de **solo consulta** — RH no aprueba ni rechaza.
- Especial interés en las justificaciones con estado `Aprobado` para aplicación en planilla.

---

### RF-05 — Consulta de Historial por Funcionario
- Un usuario con rol `ROL_FUNC` puede **consultar el historial y estado** de sus propias justificaciones.
- Debe poder ver: fecha de creación, estado actual, detalles de las líneas y, si fue procesada, el nombre del aprobador y fecha de resolución.

---

### RF-06 — Consulta de Registros Históricos (SIFCNP)
- El sistema debe proveer una **interfaz de solo lectura** para consultar registros del sistema antiguo SIFCNP.
- Esta interfaz no permite crear, editar ni eliminar datos históricos.
- Su propósito es la **consulta de antecedentes** de justificaciones pasadas.

---

## 6. Requerimientos No Funcionales

| ID | Categoría | Descripción |
|---|---|---|
| RNF-01 | **Seguridad** | Acceso basado en roles (`ROL_FUNC`, `ROL_JEFE`, `ROL_RRHH`). Autenticación integrada con usuarios de dominio CNP-FANAL. Cada rol restringe visibilidad y acciones disponibles. |
| RNF-02 | **Rendimiento** | Las consultas a la BD deben ser eficientes. Las vistas de WIZDOM deben consultarse con impacto mínimo para no degradar el ERP. Se recomienda evitar consultas en tiempo real masivas; considerar sincronización por lotes. |
| RNF-03 | **Compatibilidad** | Compatible con Chrome, Edge y Firefox en sus versiones modernas. No requiere instalación local. |
| RNF-04 | **Usabilidad** | Interfaz intuitiva, con curva de aprendizaje mínima. Inspirada en el flujo de la interfaz anterior del SIFCNP para facilitar la adopción. |

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
│   - Flujo de aprobación/rechazo por rol             │
│   - Control de acceso por roles                     │
│   - Notificación a jefatura (automática)            │
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
| **Presentación** | Renderizado de UI, formularios, listados, navegación por roles. |
| **Lógica de Negocio** | Validaciones, flujos de estado, control de permisos, notificaciones. |
| **Acceso a Datos** | CRUD en `INTEGRA_CNP`, lecturas a vistas de WIZDOM y SIFCNP. |

---

## 8. Modelo de Base de Datos

> **Base de datos:** `INTEGRA_CNP` | **Motor:** SQL Server 2019 R14 | **Normalización:** 3FN

---

### 8.1 Diagrama de Relaciones (texto)

```
Roles (1) ──────< Usuarios (N)
                     │
                     ├──── Justificaciones_Encabezado.UsuarioID
                     └──── Justificaciones_Encabezado.AprobadorID

Estados (1) ────< Justificaciones_Encabezado (N)

Justificaciones_Encabezado (1) ──────< Justificaciones_Detalle (N)

Cat_TiposJustificacion (1) ──────< Justificaciones_Detalle (N)
```

---

### 8.2 Tablas Catálogo

#### Tabla: `Roles`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `RolID` | int | PK | NO | Identificador único del rol |
| `NombreRol` | varchar(50) | — | NO | Nombre del rol: Funcionario, Jefatura, RRHH |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |

**Datos semilla:**
```sql
INSERT INTO Roles (RolID, NombreRol) VALUES
(1, 'Funcionario'),
(2, 'Jefatura'),
(3, 'RRHH');
```

---

#### Tabla: `Estados`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `EstadoID` | int | PK | NO | Identificador único del estado |
| `Descripcion` | varchar(100) | — | NO | Descripción: Pendiente Jefatura, Aprobado, Rechazado |
| `Proceso` | varchar(50) | — | NO | Proceso al que aplica (valor: `Marcas`) |

**Datos semilla:**
```sql
INSERT INTO Estados (EstadoID, Descripcion, Proceso) VALUES
(1, 'Pendiente Jefatura', 'Marcas'),
(2, 'Aprobado',           'Marcas'),
(3, 'Rechazado',          'Marcas');
```

---

#### Tabla: `Cat_TiposJustificacion`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `TipoJustificacionID` | int | PK | NO | Identificador único del tipo |
| `Descripcion` | varchar(100) | — | NO | Tardía, Omisión Marca de Entrada, Omisión Marca de Salida, Marca antes Hora de Salida, Ausencia, etc. |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |

**Datos semilla (basados en SIFCNP):**
```sql
INSERT INTO Cat_TiposJustificacion (TipoJustificacionID, Descripcion) VALUES
(1, 'Marca Tardía'),
(2, 'Omisión Marca de Entrada'),
(3, 'Omisión Marca de Salida'),
(4, 'Marca antes Hora de Salida'),
(5, 'Ausencia');
```

---

### 8.3 Tabla Base

#### Tabla: `Usuarios`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `UsuarioID` | int | PK | NO | Identificador único del usuario |
| `Cedula` | varchar(20) | — | NO | Cédula del funcionario |
| `NombreCompleto` | varchar(150) | — | NO | Nombre completo |
| `Correo` | varchar(100) | — | NO | Correo electrónico institucional |
| `JefaturaID` | int | — | SÍ | ID del usuario que es su jefatura directa (auto-referencia) |
| `UnidadID` | int | — | NO | ID de la unidad organizacional (viene de WIZDOM) |
| `RolID` | int | FK → Roles | NO | Rol asignado en el sistema |
| `Compania` | varchar(10) | — | NO | Compañía: `CNP` o `FANAL` |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |
| `Usr_Modifica` | varchar(50) | — | SÍ | Último usuario que modificó |
| `Fec_Modifica` | datetime | — | SÍ | Fecha de última modificación |

> 💡 `JefaturaID` es una FK auto-referenciada hacia `Usuarios(UsuarioID)` — puede ser NULL para los niveles más altos de jerarquía.

---

### 8.4 Tablas de Proceso (Transaccionales)

#### Tabla: `Justificaciones_Encabezado`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `JustificacionID` | int | PK | NO | Identificador único de la justificación |
| `UsuarioID` | int | FK → Usuarios | NO | ID del funcionario solicitante |
| `MotivoGeneral` | varchar(500) | — | NO | Observación general de la solicitud |
| `EstadoID` | int | FK → Estados | NO | Estado actual: 1=Pendiente, 2=Aprobado, 3=Rechazado |
| `FechaCreacion` | datetime | — | NO | Fecha y hora de creación de la solicitud |
| `AprobadorID` | int | FK → Usuarios | SÍ | ID de la jefatura que aprueba/rechaza (NULL hasta que se procese) |
| `FechaAprobacion` | datetime | — | SÍ | Fecha de aprobación o rechazo (NULL hasta que se procese) |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |

---

#### Tabla: `Justificaciones_Detalle`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `DetalleID` | int | PK | NO | Identificador único del detalle |
| `JustificacionID` | int | FK → Justificaciones_Encabezado | NO | Encabezado al que pertenece esta línea |
| `TipoJustificacionID` | int | FK → Cat_TiposJustificacion | NO | Concepto a justificar |
| `FechaMarca` | date | — | NO | Fecha de la marca a justificar |
| `ObservacionDetalle` | varchar(250) | — | SÍ | Observación específica para este detalle (opcional) |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |

---

### 8.5 Script DDL resumido

```sql
-- Base de datos
CREATE DATABASE INTEGRA_CNP;
GO
USE INTEGRA_CNP;
GO

-- Catálogos
CREATE TABLE Roles (
    RolID        INT PRIMARY KEY,
    NombreRol    VARCHAR(50) NOT NULL,
    Usr_Registro VARCHAR(50) NOT NULL,
    Fec_Registro DATETIME    NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Estados (
    EstadoID     INT PRIMARY KEY,
    Descripcion  VARCHAR(100) NOT NULL,
    Proceso      VARCHAR(50)  NOT NULL
);

CREATE TABLE Cat_TiposJustificacion (
    TipoJustificacionID INT PRIMARY KEY IDENTITY(1,1),
    Descripcion         VARCHAR(100) NOT NULL,
    Usr_Registro        VARCHAR(50)  NOT NULL,
    Fec_Registro        DATETIME     NOT NULL DEFAULT GETDATE()
);

-- Tabla base
CREATE TABLE Usuarios (
    UsuarioID       INT PRIMARY KEY IDENTITY(1,1),
    Cedula          VARCHAR(20)  NOT NULL,
    NombreCompleto  VARCHAR(150) NOT NULL,
    Correo          VARCHAR(100) NOT NULL,
    JefaturaID      INT          NULL REFERENCES Usuarios(UsuarioID),
    UnidadID        INT          NOT NULL,
    RolID           INT          NOT NULL REFERENCES Roles(RolID),
    Compania        VARCHAR(10)  NOT NULL,
    Usr_Registro    VARCHAR(50)  NOT NULL,
    Fec_Registro    DATETIME     NOT NULL DEFAULT GETDATE(),
    Usr_Modifica    VARCHAR(50)  NULL,
    Fec_Modifica    DATETIME     NULL
);

-- Tablas transaccionales
CREATE TABLE Justificaciones_Encabezado (
    JustificacionID  INT PRIMARY KEY IDENTITY(1,1),
    UsuarioID        INT           NOT NULL REFERENCES Usuarios(UsuarioID),
    MotivoGeneral    VARCHAR(500)  NOT NULL,
    EstadoID         INT           NOT NULL REFERENCES Estados(EstadoID),
    FechaCreacion    DATETIME      NOT NULL DEFAULT GETDATE(),
    AprobadorID      INT           NULL     REFERENCES Usuarios(UsuarioID),
    FechaAprobacion  DATETIME      NULL,
    Usr_Registro     VARCHAR(50)   NOT NULL,
    Fec_Registro     DATETIME      NOT NULL DEFAULT GETDATE()
);

CREATE TABLE Justificaciones_Detalle (
    DetalleID            INT PRIMARY KEY IDENTITY(1,1),
    JustificacionID      INT          NOT NULL REFERENCES Justificaciones_Encabezado(JustificacionID),
    TipoJustificacionID  INT          NOT NULL REFERENCES Cat_TiposJustificacion(TipoJustificacionID),
    FechaMarca           DATE         NOT NULL,
    ObservacionDetalle   VARCHAR(250) NULL,
    Usr_Registro         VARCHAR(50)  NOT NULL,
    Fec_Registro         DATETIME     NOT NULL DEFAULT GETDATE()
);
```

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
      │  Sistema notifica automáticamente a la jefatura directa
      ▼
[JEFATURA]
      │
      ├── Aprueba ──→ Estado: "Aprobado"  + registra AprobadorID + FechaAprobacion
      │
      └── Rechaza ──→ Estado: "Rechazado" + registra AprobadorID + FechaAprobacion
                              │
                              ▼
                    [RRHH consulta justificaciones Aprobadas]
                    → Uso para control de planilla
```

### 9.2 Reglas de negocio

| # | Regla |
|---|---|
| RN-01 | Una boleta debe tener al menos **una línea de detalle** para poder ser guardada. |
| RN-02 | El estado inicial de toda boleta nueva es siempre `Pendiente Jefatura` (EstadoID = 1). |
| RN-03 | Solo el **rol Jefatura** puede cambiar el estado de una boleta. |
| RN-04 | Una boleta en estado `Aprobado` o `Rechazado` **no puede ser modificada**. |
| RN-05 | Un funcionario solo puede ver **sus propias** justificaciones. |
| RN-06 | Una jefatura solo puede ver las justificaciones de los **funcionarios bajo su cargo** (donde `Usuarios.JefaturaID = jefaturaActual.UsuarioID`). |
| RN-07 | El rol RRHH puede ver **todas** las justificaciones sin excepción. |
| RN-08 | Los datos de la interfaz histórica (SIFCNP) son de **solo lectura**. |

---

## 10. Integraciones Externas

### 10.1 WIZDOM (ERP — Solo Lectura)
- **Propósito:** Fuente de verdad para usuarios, estructura organizacional y jefaturas.
- **Método:** Consulta a **vistas de BD** expuestas por WIZDOM. No se escriben datos en WIZDOM.
- **Datos obtenidos:** Cédula, nombre, correo, jefatura directa, unidad, compañía (CNP=001 / FANAL=002).
- **Consideración de rendimiento:** Las consultas deben ser ligeras para no impactar el ERP. Se recomienda sincronización periódica (batch) en lugar de consultas en tiempo real por cada login.

### 10.2 SIFCNP (Sistema histórico — Solo Lectura)
- **Propósito:** Consulta de registros históricos de justificaciones anteriores a la migración.
- **Método:** Acceso de solo lectura a la BD del sistema antiguo.
- **Restricción:** No se permite ninguna operación de escritura (INSERT, UPDATE, DELETE).
- **Interfaz:** Módulo separado de consulta con filtros básicos (funcionario, fecha, tipo).

---

## 11. Checklist de Progreso de Desarrollo

> Usar esta sección para llevar el control del avance del proyecto. Marcar con `[x]` al completar.

### 🗄️ Base de Datos — INTEGRA_CNP

- [ ] Crear base de datos `INTEGRA_CNP` en SQL Server 2019
- [ ] Crear tabla `Roles` + insertar datos semilla
- [ ] Crear tabla `Estados` + insertar datos semilla
- [ ] Crear tabla `Cat_TiposJustificacion` + insertar datos semilla
- [ ] Crear tabla `Usuarios` con FK a `Roles`
- [ ] Crear tabla `Justificaciones_Encabezado` con FKs a `Usuarios` y `Estados`
- [ ] Crear tabla `Justificaciones_Detalle` con FKs a `Justificaciones_Encabezado` y `Cat_TiposJustificacion`
- [ ] Validar integridad referencial entre todas las tablas
- [ ] Crear índices de rendimiento (mínimo en `UsuarioID`, `EstadoID`, `JustificacionID`)

### 🔌 Integraciones

- [ ] Identificar y documentar las vistas disponibles en WIZDOM
- [ ] Desarrollar capa de acceso a vistas de WIZDOM (solo lectura)
- [ ] Desarrollar proceso de sincronización de usuarios desde WIZDOM a `Usuarios`
- [ ] Validar acceso de solo lectura a BD SIFCNP
- [ ] Desarrollar capa de consulta histórica a SIFCNP

### 🔐 Autenticación y Roles

- [ ] Definir mecanismo de autenticación (dominio CNP-FANAL)
- [ ] Implementar autenticación de usuarios
- [ ] Implementar control de acceso basado en roles (Funcionario / Jefatura / RRHH)
- [ ] Validar que cada rol solo accede a sus vistas y acciones permitidas

### 🖥️ Módulo: Gestión de Usuarios (RF-01)

- [ ] Pantalla de administración de usuarios (sincronizados)
- [ ] Proceso automático/manual de sincronización desde WIZDOM
- [ ] Asignación de rol por usuario

### 📝 Módulo: Creación de Justificación (RF-02)

- [ ] Formulario de encabezado (motivo general)
- [ ] Grilla de detalle: agregar líneas de detalle
- [ ] Grilla de detalle: eliminar líneas de detalle
- [ ] Selector de tipo de justificación (desde `Cat_TiposJustificacion`)
- [ ] Selector de fecha de marca
- [ ] Campo de observación por detalle (opcional)
- [ ] Validación: mínimo 1 detalle antes de guardar (RN-01)
- [ ] Guardar boleta con estado inicial `Pendiente Jefatura`
- [ ] Notificación automática a jefatura directa al crear boleta

### ✅ Módulo: Aprobación / Rechazo (RF-03)

- [ ] Vista de jefatura: listado de justificaciones de subordinados
- [ ] Filtro por estado (Pendientes, Aprobadas, Rechazadas)
- [ ] Acción: Aprobar boleta → actualizar estado + registrar AprobadorID + FechaAprobacion
- [ ] Acción: Rechazar boleta → actualizar estado + registrar AprobadorID + FechaAprobacion
- [ ] Bloquear modificación de boletas ya procesadas (RN-04)

### 🔎 Módulo: Consulta RRHH (RF-04)

- [ ] Vista de RRHH: listado de todas las justificaciones
- [ ] Filtros: por funcionario, estado, fecha de creación, compañía (CNP/FANAL)
- [ ] Vista de detalle de cada boleta
- [ ] (Opcional) Exportación a Excel/CSV para uso en planilla

### 👤 Módulo: Historial del Funcionario (RF-05)

- [ ] Vista del funcionario: historial de sus propias boletas
- [ ] Detalle de cada boleta: líneas, estado, aprobador, fecha resolución
- [ ] Indicador visual de estado (pendiente / aprobado / rechazado)

### 📂 Módulo: Consulta Histórica SIFCNP (RF-06)

- [ ] Pantalla de consulta de registros históricos
- [ ] Filtros básicos: funcionario, fecha, tipo de justificación
- [ ] Vista de solo lectura (sin botones de edición)

### 🧪 Pruebas y Calidad

- [ ] Pruebas unitarias de lógica de negocio (flujo de estados)
- [ ] Pruebas de roles y permisos (cada rol solo ve lo que debe)
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
- **Auto-referencia en Usuarios:** El campo `JefaturaID` apunta a otro `UsuarioID` en la misma tabla. Para obtener los subordinados de una jefatura: `WHERE JefaturaID = @UsuarioActualID`.
- **Control de visibilidad por rol:**
  - Funcionario: `WHERE UsuarioID = @usuarioLogueado`
  - Jefatura: `WHERE UsuarioID IN (SELECT UsuarioID FROM Usuarios WHERE JefaturaID = @usuarioLogueado)`
  - RRHH: sin filtro de usuario.
- **Las tablas de WIZDOM y SIFCNP son de solo lectura.** Nunca generar código con INSERT/UPDATE/DELETE sobre esas bases.
- **Estados son IDs fijos:** 1=Pendiente Jefatura, 2=Aprobado, 3=Rechazado. Usar constantes en el código.
- **Compañías:** CNP = código `001`, FANAL = código `002`. En la tabla `Usuarios`, el campo `Compania` almacena `'CNP'` o `'FANAL'`.
- **Campos de auditoría:** Todas las tablas tienen `Usr_Registro` y `Fec_Registro`. Las tablas principales también tienen `Usr_Modifica` y `Fec_Modifica`.

---

*Documento generado con base en el Análisis y Diseño de SI "Integrador Marcas" v1.1 — CNP / UTI — Octubre 2025*
