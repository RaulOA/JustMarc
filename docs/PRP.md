# PRP — Sistema Web "Integrador Marcas"
> **Product Requirements Plan** | Consejo Nacional de Producción (CNP) & FANAL  
> Versión: 1.1 | Basado en: Análisis y Diseño de SI v1.1 (16/10/2025)  
> Organización: Unidad de Tecnologías de la Información (UTI)  
> Última actualización: Abril 2026

---

## 📋 Tabla de Contenidos

1. [Resumen del Producto](#1-resumen-del-producto)
2. [Contexto y Problema](#2-contexto-y-problema)
3. [Estado Actual del Prototipo](#3-estado-actual-del-prototipo)
4. [Stack Tecnológico](#4-stack-tecnológico)
5. [Roles del Sistema](#5-roles-del-sistema)
6. [Requerimientos Funcionales](#6-requerimientos-funcionales)
7. [Requerimientos No Funcionales](#7-requerimientos-no-funcionales)
8. [Arquitectura del Software](#8-arquitectura-del-software)
9. [Modelo de Base de Datos](#9-modelo-de-base-de-datos)
10. [Lógica de Negocio y Flujos](#10-lógica-de-negocio-y-flujos)
11. [Integraciones Externas](#11-integraciones-externas)
12. [Estructura del Proyecto](#12-estructura-del-proyecto)
13. [Checklist de Progreso](#13-checklist-de-progreso)

---

## 1. Resumen del Producto

| Campo | Detalle |
|---|---|
| **Nombre del sistema** | Integrador Marcas / Sistema de Justificación de Marcas |
| **Tipo** | Aplicación Web (desarrollo interno) |
| **Institución** | Consejo Nacional de Producción (CNP) y FANAL |
| **Reemplaza a** | Sistema de escritorio SICNP (obsoleto, incompatible con Windows 11) |
| **Base de datos nueva** | `INTEGRA_CNP` en SQL Server 2019 R14 |
| **Prototipo base** | 19 de diciembre de 2025 ✅ |
| **Entrega final** | 31 de marzo de 2026 |
| **Metodología** | Prototipado iterativo |
| **Elaborado por** | Lic. Luis Diego Vega Soto, M.Ed. |
| **Técnico programador** | Sr. Raúl Ortega Acuña |
| **DBA** | Rudy Antonio Arias Rodríguez |
| **Analista Programador** | Manuel Montealegre Bejarano |
| **Analista de Sistemas** | Luis Diego Vega Soto |

---

## 2. Contexto y Problema

### Problema actual
El proceso de **Justificación de Marcas** (registro de tardías, omisiones, ausencias, etc.) se gestiona mediante el sistema de escritorio **SICNP**, que presenta:

- Incompatibilidad con **Windows 11** y SO modernos.
- Interrupción crítica del servicio para funcionarios.
- Imposibilidad de escalar o mantener la aplicación de escritorio.

### Solución propuesta
Sistema web moderno, centralizado y basado en roles que reemplace la funcionalidad de justificación de marcas del SICNP, garantizando continuidad operativa para todos los funcionarios de CNP y FANAL.

---

## 3. Estado Actual del Prototipo

> El prototipo visual frontend está **completado** con las siguientes pantallas implementadas en HTML/CSS/JS puro.

| Pantalla | Archivo | Estado |
|---|---|---|
| Login | `index.html` | ✅ Completo |
| Dashboard + Navegación por tabs | `dashboard.html` | ✅ Completo |
| Panel Funcionario (formulario + historial) | `dashboard.html` | ✅ Completo |
| Panel Jefatura (listado + aprobar/rechazar) | `dashboard.html` | ✅ Completo |
| Panel RRHH (filtros + estadísticas + tabla global) | `dashboard.html` | ✅ Completo |
| Consulta Histórica SIFCNP | `dashboard.html` | ✅ Completo |
| Estilos globales | `style.css` | ✅ Completo |
| Lógica JS del prototipo | `app.js` | ✅ Completo |

### Credenciales del prototipo
- **Usuario:** `admin`
- **Contraseña:** `1234`

### Lo que falta (Backend + BD)
- Conexión real a base de datos `INTEGRA_CNP` (SQL Server 2019)
- Backend en C# (.NET) con lógica de negocio real
- Autenticación con dominio CNP-FANAL
- Integración con vistas de WIZDOM (ERP)
- Integración de solo lectura con SIFCNP

---

## 4. Stack Tecnológico

| Capa | Tecnología |
|---|---|
| **Frontend** | HTML5, CSS3, JavaScript (actualmente prototipo estático) |
| **Backend** | C# (.NET) — *pendiente de desarrollo* |
| **Base de datos principal** | Microsoft SQL Server 2019 R14 (`INTEGRA_CNP`) |
| **BD lectura ERP** | WIZDOM (vistas de solo lectura, proveedor OPTEC) |
| **BD histórica** | SIFCNP (solo lectura, consulta histórica) |
| **Navegadores soportados** | Chrome, Edge, Firefox |
| **Arquitectura** | Monolítica 3 capas |

---

## 5. Roles del Sistema

| Rol | Código | Descripción |
|---|---|---|
| **Funcionario** | `ROL_FUNC` | Crea boletas de justificación y consulta su propio historial. |
| **Jefatura** | `ROL_JEFE` | Ve justificaciones de sus subordinados directos y aprueba o rechaza. |
| **Recursos Humanos** | `ROL_RRHH` | Consulta **todas** las justificaciones. Solo lectura. Control de planilla. |

> ⚠️ El acceso estará basado estrictamente en roles. La autenticación se integrará con el dominio CNP-FANAL. Cada rol tiene visibilidad y acciones restringidas.

---

## 6. Requerimientos Funcionales

### RF-01 — Gestión de Usuarios
- Poblar y sincronizar tabla de usuarios desde **vistas de solo lectura** del ERP WIZDOM.
- Incluir funcionarios de **CNP (código 001)** y **FANAL (código 002)**.
- Datos sincronizados: cédula, nombre completo, correo institucional, jefatura directa, unidad organizacional, compañía y rol.

### RF-02 — Creación de Boleta de Justificación
- Un `ROL_FUNC` puede crear una boleta con:
  - **Encabezado:** motivo general.
  - **Detalle (líneas):** uno o más conceptos con tipo, fecha de marca y observación específica (opcional).
- Estado inicial: **`Pendiente Jefatura`**.
- Debe permitir agregar y eliminar líneas de detalle antes de guardar.

### RF-03 — Aprobación / Rechazo por Jefatura
- `ROL_JEFE` ve **únicamente** justificaciones de su jerarquía directa.
- Puede **Aprobar** (estado → `Aprobado`) o **Rechazar** (estado → `Rechazado`).
- Registra ID del aprobador y fecha de la acción.

### RF-04 — Consulta por Recursos Humanos
- `ROL_RRHH` consulta **todas** las justificaciones sin restricción de usuario.
- Filtros por: funcionario, estado, fecha, compañía.
- Solo consulta — RH no aprueba ni rechaza.

### RF-05 — Consulta de Historial por Funcionario
- `ROL_FUNC` consulta el historial y estado de sus propias boletas.
- Ve: fecha de creación, estado, detalles, aprobador y fecha de resolución.

### RF-06 — Consulta de Registros Históricos (SIFCNP)
- Interfaz de **solo lectura** para registros del sistema antiguo SIFCNP.
- Sin crear, editar ni eliminar datos históricos.

---

## 7. Requerimientos No Funcionales

| ID | Categoría | Descripción |
|---|---|---|
| RNF-01 | **Seguridad** | Control de acceso basado en roles. Autenticación con dominio CNP-FANAL. |
| RNF-02 | **Rendimiento** | Consultas eficientes. Vistas de WIZDOM con impacto mínimo. Considerar sincronización batch. |
| RNF-03 | **Compatibilidad** | Chrome, Edge y Firefox modernos. Sin instalación local. |
| RNF-04 | **Usabilidad** | Interfaz intuitiva. Curva de aprendizaje mínima. Inspirada en el flujo del SIFCNP. |

---

## 8. Arquitectura del Software

```
┌─────────────────────────────────────────────────────┐
│            CAPA DE PRESENTACIÓN (Frontend)          │
│              HTML5 + CSS3 + JavaScript              │
│         (Prototipo actual: archivos estáticos)      │
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

---

## 9. Modelo de Base de Datos

> **Base de datos:** `INTEGRA_CNP` | **Motor:** SQL Server 2019 R14 | **Normalización:** 3FN  
> Ver script completo en: `docs/db/INTEGRA_CNP_DDL.sql`

### 9.1 Diagrama de Relaciones

```
Roles (1) ──────────────────────< Usuarios (N)
                                      │
                    ┌─────────────────┴──────────────────┐
                    │ (UsuarioID — solicita)              │ (AprobadorID — aprueba)
                    ▼                                     ▼
         Justificaciones_Encabezado ◄── Estados (1..N)
                    │
                    └────< Justificaciones_Detalle >────── Cat_TiposJustificacion
```

### 9.2 Tabla: `Roles`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `RolID` | int | PK | NO | ID único del rol |
| `NombreRol` | varchar(50) | — | NO | Funcionario / Jefatura / RRHH |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

### 9.3 Tabla: `Estados`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `EstadoID` | int | PK | NO | ID único del estado |
| `Descripcion` | varchar(100) | — | NO | Pendiente Jefatura / Aprobado / Rechazado |
| `Proceso` | varchar(50) | — | NO | `Marcas` |

### 9.4 Tabla: `Cat_TiposJustificacion`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `TipoJustificacionID` | int | PK | NO | ID único del tipo |
| `Descripcion` | varchar(100) | — | NO | Tardía / Omisión Entrada / Omisión Salida / Antes Hora Salida / Ausencia |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

### 9.5 Tabla: `Usuarios`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `UsuarioID` | int | PK | NO | ID único del usuario |
| `Cedula` | varchar(20) | — | NO | Cédula del funcionario |
| `NombreCompleto` | varchar(150) | — | NO | Nombre completo |
| `Correo` | varchar(100) | — | NO | Correo institucional |
| `JefaturaID` | int | FK→Usuarios | SÍ | ID de la jefatura directa (auto-referencia) |
| `UnidadID` | int | — | NO | ID de la unidad organizacional |
| `RolID` | int | FK→Roles | NO | Rol del sistema |
| `Compania` | varchar(10) | — | NO | `CNP` o `FANAL` |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |
| `Usr_Modifica` | varchar(50) | — | SÍ | Último usuario que modificó |
| `Fec_Modifica` | datetime | — | SÍ | Fecha de modificación |

> 💡 `JefaturaID` es FK auto-referenciada. NULL para los niveles más altos de jerarquía.

### 9.6 Tabla: `Justificaciones_Encabezado`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `JustificacionID` | int | PK | NO | ID único de la justificación |
| `UsuarioID` | int | FK→Usuarios | NO | Funcionario solicitante |
| `MotivoGeneral` | varchar(500) | — | NO | Observación general |
| `EstadoID` | int | FK→Estados | NO | Estado actual (1=Pendiente, 2=Aprobado, 3=Rechazado) |
| `FechaCreacion` | datetime | — | NO | Fecha y hora de creación |
| `AprobadorID` | int | FK→Usuarios | SÍ | Jefatura que procesa (NULL hasta resolución) |
| `FechaAprobacion` | datetime | — | SÍ | Fecha de aprobación/rechazo |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

### 9.7 Tabla: `Justificaciones_Detalle`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `DetalleID` | int | PK | NO | ID único del detalle |
| `JustificacionID` | int | FK→Justificaciones_Encabezado | NO | Encabezado padre |
| `TipoJustificacionID` | int | FK→Cat_TiposJustificacion | NO | Concepto a justificar |
| `FechaMarca` | date | — | NO | Fecha de la marca |
| `ObservacionDetalle` | varchar(250) | — | SÍ | Observación específica (opcional) |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

---

## 10. Lógica de Negocio y Flujos

### 10.1 Flujo de una Justificación

```
[FUNCIONARIO]
      │
      ▼
  Crea boleta (Encabezado + 1..N Detalles)
      │
      ▼
  Estado inicial: "Pendiente Jefatura" (EstadoID = 1)
      │
      │  Sistema notifica automáticamente a la jefatura directa
      ▼
[JEFATURA] — solo ve subordinados directos (JefaturaID = jefaturaLogueada)
      │
      ├── Aprueba ──► Estado: "Aprobado" (2) + registra AprobadorID + FechaAprobacion
      │
      └── Rechaza ──► Estado: "Rechazado" (3) + registra AprobadorID + FechaAprobacion
                              │
                              ▼
                    [RRHH] consulta todas las justificaciones
                    Especial atención: Aprobadas → aplicación en planilla
```

### 10.2 Reglas de Negocio

| # | Regla |
|---|---|
| RN-01 | Una boleta **debe tener al menos una línea de detalle** para poder guardarse. |
| RN-02 | El estado inicial es siempre `Pendiente Jefatura` (EstadoID = 1). |
| RN-03 | Solo `ROL_JEFE` puede cambiar el estado de una boleta. |
| RN-04 | Una boleta `Aprobado` o `Rechazado` **no puede modificarse**. |
| RN-05 | Un funcionario solo ve **sus propias** justificaciones. |
| RN-06 | Jefatura solo ve justificaciones donde `Usuarios.JefaturaID = jefaturaActual.UsuarioID`. |
| RN-07 | RRHH ve **todas** las justificaciones sin excepción. |
| RN-08 | Los datos de SIFCNP son de **solo lectura absoluta**. |

---

## 11. Integraciones Externas

### 11.1 WIZDOM (ERP — Solo Lectura)
- **Propósito:** Fuente de verdad para usuarios, estructura y jefaturas.
- **Método:** Consulta a vistas de BD expuestas por WIZDOM. Sin escritura.
- **Datos:** Cédula, nombre, correo, jefatura, unidad, compañía (CNP=001 / FANAL=002).
- **Recomendación:** Sincronización batch periódica (no tiempo real) para no impactar el ERP.

### 11.2 SIFCNP (Sistema histórico — Solo Lectura)
- **Propósito:** Consulta de registros anteriores a la migración.
- **Restricción:** `SELECT` únicamente — jamás `INSERT`, `UPDATE`, `DELETE`.
- **Interfaz:** Módulo separado con filtros por funcionario, fecha y tipo.

---

## 12. Estructura del Proyecto

```
Justificacion de Marca/
├── index.html                  ← Login (✅ Prototipo completo)
├── dashboard.html              ← Dashboard + todos los paneles (✅ Prototipo completo)
├── style.css                   ← Estilos globales (✅ Completo)
├── app.js                      ← Lógica JS del prototipo (✅ Completo)
│
├── docs/
│   ├── PRP.md                  ← Este documento
│   └── db/
│       └── INTEGRA_CNP_DDL.sql ← Script DDL completo de la BD
│
├── MARCAS_Análisis-Y-Diseño_SI_vr_1-1_Firmado 1 1.pdf  ← Documento fuente
├── Inicial.md                  ← Instrucciones originales del prototipo
├── README.md                   ← Guía de inicio rápido
├── Justificacion de Marca.code-workspace
├── .gitignore
├── .gitattributes
└── LICENSE
```

> **Próxima fase:** Crear estructura del proyecto .NET (Backend) con carpetas `Controllers/`, `Models/`, `Services/`, `Data/`.

---

## 13. Checklist de Progreso

> Marcar con `[x]` al completar cada ítem.

### 🎨 Prototipo Frontend (Fase 1)
- [x] Login funcional con validación básica (`index.html`)
- [x] Dashboard con navegación por pestañas (`dashboard.html`)
- [x] Panel Funcionario — formulario de creación
- [x] Panel Funcionario — tabla de historial personal
- [x] Panel Jefatura — tabla de solicitudes pendientes
- [x] Panel Jefatura — Aprobar / Rechazar con cambio de estado
- [x] Panel Jefatura — Vista de detalle expandible
- [x] Panel RRHH — estadísticas de resumen
- [x] Panel RRHH — filtros y tabla global
- [x] Panel SIFCNP — búsqueda y tabla de históricos
- [x] Estilos globales institucionales (`style.css`)
- [x] Lógica JS de navegación y formularios (`app.js`)

### 🗄️ Base de Datos — INTEGRA_CNP
- [ ] Crear base de datos `INTEGRA_CNP` en SQL Server 2019
- [ ] Crear tabla `Roles` + datos semilla
- [ ] Crear tabla `Estados` + datos semilla
- [ ] Crear tabla `Cat_TiposJustificacion` + datos semilla
- [ ] Crear tabla `Usuarios` con FK a `Roles`
- [ ] Crear tabla `Justificaciones_Encabezado` con FKs
- [ ] Crear tabla `Justificaciones_Detalle` con FKs
- [ ] Validar integridad referencial
- [ ] Crear índices de rendimiento (`UsuarioID`, `EstadoID`, `JustificacionID`, `JefaturaID`)
- [ ] Ejecutar script DDL en ambiente de desarrollo

### 🔌 Integraciones
- [ ] Documentar vistas disponibles en WIZDOM
- [ ] Desarrollar capa de acceso a vistas de WIZDOM (solo lectura)
- [ ] Implementar proceso de sincronización batch Usuarios ← WIZDOM
- [ ] Validar y documentar acceso de lectura a BD SIFCNP
- [ ] Desarrollar capa de consulta histórica a SIFCNP

### 🔐 Backend — Autenticación y Roles
- [ ] Definir mecanismo de autenticación (dominio CNP-FANAL)
- [ ] Implementar autenticación de usuarios
- [ ] Implementar middleware de control de acceso por rol
- [ ] Validar que cada rol accede solo a sus rutas/vistas permitidas

### ⚙️ Backend — API / Lógica de Negocio (C# .NET)
- [ ] Configurar proyecto .NET (estructura de carpetas)
- [ ] Configurar conexión a `INTEGRA_CNP`
- [ ] **RF-01:** Endpoint sincronización usuarios desde WIZDOM
- [ ] **RF-02:** Endpoint crear boleta (encabezado + detalles en una transacción)
- [ ] **RF-02:** Validar mínimo 1 detalle por boleta (RN-01)
- [ ] **RF-02:** Notificación automática a jefatura al crear boleta
- [ ] **RF-03:** Endpoint aprobar boleta
- [ ] **RF-03:** Endpoint rechazar boleta
- [ ] **RF-03:** Bloquear modificación de boletas ya procesadas (RN-04)
- [ ] **RF-04:** Endpoint consulta global RRHH con filtros
- [ ] **RF-05:** Endpoint historial por funcionario
- [ ] **RF-06:** Endpoint consulta histórica SIFCNP (solo lectura)

### 🖥️ Frontend — Integración con Backend
- [ ] Conectar login con autenticación real (quitar credenciales quemadas)
- [ ] Conectar Panel Funcionario con API de creación
- [ ] Conectar Panel Jefatura con API de aprobación/rechazo
- [ ] Conectar Panel RRHH con API de consulta global
- [ ] Conectar Panel SIFCNP con API de históricos
- [ ] Mostrar nombre real del usuario logueado en topbar
- [ ] Mostrar solo las pestañas correspondientes al rol del usuario
- [ ] Manejo de errores y mensajes del servidor en UI

### 🧪 Pruebas y Calidad
- [ ] Pruebas unitarias — flujo de estados (Pendiente → Aprobado/Rechazado)
- [ ] Pruebas de roles — verificar que cada rol solo accede a lo permitido
- [ ] Pruebas de integración con WIZDOM
- [ ] Pruebas de integración con SIFCNP
- [ ] Pruebas en Chrome, Edge y Firefox
- [ ] Validación con usuarios de RRHH
- [ ] Validación con jefaturas piloto
- [ ] Validación con funcionarios piloto

### 🚀 Despliegue
- [ ] Configurar ambiente de desarrollo
- [ ] Configurar ambiente de pruebas / QA
- [x] **Prototipo base entregado** → 19 de diciembre de 2025
- [ ] Ajustes post-retroalimentación del prototipo
- [ ] Configurar ambiente de producción
- [ ] **Entrega final en producción** → 31 de marzo de 2026
- [ ] Manual básico de usuario
- [ ] Capacitación a usuarios piloto

---

## 📎 Notas Clave para IA Asistente

> Esta sección es contexto esencial para cualquier LLM que asista en el desarrollo.

### Arquitectura de datos
- **Patrón Encabezado-Detalle:** `Justificaciones_Encabezado` es el maestro; `Justificaciones_Detalle` son sus líneas hijas. Siempre tratarlas en la **misma transacción** al crear o eliminar.
- **Auto-referencia en Usuarios:** `JefaturaID` apunta a otro `UsuarioID` en la misma tabla.
  - Subordinados de una jefatura: `WHERE JefaturaID = @UsuarioActualID`

### Filtros por rol (queries base)
```sql
-- Funcionario: solo sus boletas
WHERE UsuarioID = @usuarioLogueado

-- Jefatura: boletas de sus subordinados directos
WHERE UsuarioID IN (
    SELECT UsuarioID FROM Usuarios WHERE JefaturaID = @usuarioLogueado
)

-- RRHH: todas las boletas (sin filtro de usuario)
-- (sin WHERE de usuario)
```

### Reglas críticas
- **WIZDOM y SIFCNP son de solo lectura ABSOLUTA.** Nunca generar `INSERT/UPDATE/DELETE` sobre esas bases.
- **Estados son IDs fijos:**
  - `1` = Pendiente Jefatura
  - `2` = Aprobado
  - `3` = Rechazado
- **Compañías:** `CNP` = código `001`, `FANAL` = código `002`.
- **Auditoría:** Todas las tablas tienen `Usr_Registro` y `Fec_Registro`. Las principales también `Usr_Modifica` y `Fec_Modifica`.
- Una boleta con EstadoID = 2 o 3 **no se puede modificar** (RN-04).

### Estado del frontend
El prototipo usa **datos mockeados** (quemados en HTML). Al integrar el backend:
1. Reemplazar datos hardcoded con llamadas a la API.
2. Reemplazar las credenciales `admin/1234` con autenticación real.
3. Mostrar pestañas del dashboard según el `RolID` del usuario autenticado.

---

*Basado en: Análisis y Diseño de SI "Integrador Marcas" v1.1 — CNP / UTI — Octubre 2025*
