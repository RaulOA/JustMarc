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

### Estado funcional actual y brechas
- Backend base implementado en C# (.NET 8) con endpoints operativos para flujos de Funcionario, Jefatura y RRHH.
- Conexion activa a `INTEGRA_CNP` con repositorios Dapper y validaciones de negocio.
- Integracion frontend-backend funcional para crear, listar, consultar detalle y resolver boletas.

Pendiente para evolucion de producto:
- Autenticacion corporativa con dominio CNP-FANAL.
- Integracion productiva con vistas de WIZDOM y lectura historica SIFCNP.
- Dashboard administrativo con auditoria paginada, exportacion y gestion de jerarquias/delegaciones.

---

## 4. Stack Tecnológico

| Capa | Tecnología |
|---|---|
| **Frontend** | HTML5, CSS3, JavaScript (dashboard funcional integrado a API) |
| **Backend** | C# (.NET 8) con arquitectura por capas y endpoints operativos |
| **Base de datos principal** | Microsoft SQL Server 2019 R14 (`INTEGRA_CNP`) |
| **BD lectura ERP** | WIZDOM (fuente canonica: `dbo.empleado`, solo lectura) |
| **BD histórica** | SIFCNP (solo lectura, consulta histórica) |
| **Navegadores soportados** | Chrome, Edge, Firefox |
| **Arquitectura** | Backend en capas (Api/Application/Domain/Infrastructure) + frontend estatico |

---

## 5. Roles del Sistema

| Rol | Código | Descripción |
|---|---|---|
| **Funcionario** | `ROL_FUNC` | Crea boletas de justificación, consulta su propio historial y da seguimiento al estado de sus solicitudes. |
| **Jefatura / Aprobador** | `ROL_JEFE` | Resuelve solicitudes según la jerarquía de aprobación activa o una delegación vigente. Su alcance ya no depende únicamente de la jefatura directa importada. |
| **Recursos Humanos** | `ROL_RRHH` | Consulta **todas** las justificaciones para control operativo y planilla. Acceso de solo lectura, sin privilegios administrativos ni acceso a auditoría. |
| **Administrador** | `ROL_ADMIN` | Administra jerarquías de aprobación, delegados o subaprobadores, catálogos y estructuras organizacionales de soporte. Tiene acceso exclusivo al dashboard y reportes de auditoría. |

> ⚠️ El acceso estará basado estrictamente en roles. La autenticación se integrará con el dominio CNP-FANAL. El rol Administrador es un actor formal del sistema y su alcance queda restringido a funciones administrativas, trazabilidad y configuración.

---

## 6. Requerimientos Funcionales

### RF-01 — Gestión de Usuarios
- Poblar y sincronizar tabla de usuarios desde `WIZDOM.dbo.empleado` (solo lectura) como fuente primaria.
- Incluir funcionarios de CNP/FANAL mediante mapeo canonico de compania en bridge (`1/001 -> CNP`, `2/002 -> FANAL`).
- Datos sincronizados: cédula, nombre completo, correo institucional, jefatura directa, unidad organizacional, compañía y rol.
- Reglas obligatorias de normalizacion de origen:
  - `numero_identificacion` se preserva como texto (sin conversion numerica).
  - fechas centinela `00:00.0` se transforman a `NULL`.
  - placeholders (`NULL`, `N/T`, `N/A`, `.`, `-`, `--`) se tratan como `NULL` logico por campo.
- La jefatura directa proveniente de WIZDOM se conservará como dato base de referencia, pero no como única regla de aprobación.

### RF-02 — Creación de Boleta de Justificación
- Un `ROL_FUNC` puede crear una boleta con:
  - **Encabezado:** motivo general.
  - **Detalle (líneas):** uno o más conceptos con tipo, fecha de marca y observación específica (opcional).
- Estado inicial: **`Pendiente Jefatura`**.
- Debe permitir agregar y eliminar líneas de detalle antes de guardar.

### RF-03 — Aprobación / Rechazo por Jerarquía Configurada
- Un usuario con permisos de aprobación puede ver **únicamente** las justificaciones que le correspondan según la jerarquía de aprobación activa y las delegaciones vigentes.
- Puede **Aprobar** (estado → `Aprobado`) o **Rechazar** (estado → `Rechazado`).
- La resolución debe registrar ID del aprobador efectivo, rol con el que actúa, fecha de la acción y resultado.
- El flujo no debe quedar bloqueado cuando exista una delegación válida habilitada por Administración.

### RF-04 — Consulta por Recursos Humanos
- `ROL_RRHH` consulta **todas** las justificaciones sin restricción de usuario.
- Filtros por: funcionario, estado, fecha, compañía.
- Solo consulta; RRHH no aprueba, no rechaza y no administra jerarquías, delegaciones ni auditoría.

### RF-05 — Consulta de Historial por Funcionario
- `ROL_FUNC` consulta el historial y estado de sus propias boletas.
- Ve: fecha de creación, estado, detalles, aprobador efectivo y fecha de resolución.

### RF-06 — Consulta de Registros Históricos (SIFCNP)
- Interfaz de **solo lectura** para registros del sistema antiguo SIFCNP.
- Sin crear, editar ni eliminar datos históricos.

### RF-07 — Auditoría Persistente del Sistema
- El sistema debe registrar en base de datos las acciones operativas y administrativas relevantes.
- Cada pista de auditoría debe almacenar al menos: fecha y hora, identificador de usuario, nombre de usuario, rol, tipo de evento, descripción normalizada o catalogada, resultado del evento y referencia funcional cuando aplique.
- La consulta de auditoría es exclusiva del `ROL_ADMIN`.
- Debe existir un dashboard administrativo con visualización paginada, filtros por fecha, usuario, rol, tipo de evento y resultado, así como descarga de reportes sobre el conjunto filtrado.

### RF-08 — Configuración de Jerarquía de Aprobaciones
- El sistema debe permitir definir y mantener una jerarquía de aprobación configurable dentro de `INTEGRA_CNP`.
- Debe soportar relaciones **verticales** y **horizontales** entre unidades, responsables o niveles aprobadores.
- Un aprobador superior podrá visualizar y resolver solicitudes de unidades o subunidades bajo su alcance configurado.
- La configuración de jerarquías solo podrá ser administrada por `ROL_ADMIN`.

### RF-09 — Gestión de Delegados y Subaprobadores
- El sistema debe permitir registrar delegados o subaprobadores para cubrir ausencias, recargos u otros escenarios operativos.
- Los delegados deben heredar el mismo alcance de aprobación del delegante durante la vigencia configurada.
- El alta, baja, habilitación y deshabilitación de delegaciones solo podrá realizarla `ROL_ADMIN`.
- La gestión de delegados es administrativa y no forma parte del flujo ordinario de aprobación del usuario final.

### RF-10 — Catálogos y Estructuras Organizacionales de Soporte
- El sistema debe contar con catálogos y estructuras propias para representar jerarquías, niveles, delegaciones, eventos de auditoría, resultados de auditoría y estados administrativos de entidades configurables.
- Deben existir estados de **activo/inactivo** para las entidades de configuración relevantes.
- La lógica de jerarquía configurable, delegación y auditoría persistente **no existe hoy en WIZDOM, SIFCNP ni SICNP**; debe ser creada y administrada por el sistema en `INTEGRA_CNP`.

---

## 7. Requerimientos No Funcionales

| ID | Categoría | Descripción |
|---|---|---|
| RNF-01 | **Seguridad** | Control de acceso basado en roles incluyendo `ROL_ADMIN`. Segregación estricta entre operaciones administrativas, operativas y de consulta. |
| RNF-02 | **Rendimiento** | Consultas eficientes. Vistas de WIZDOM con impacto mínimo. Considerar sincronización batch y paginación obligatoria para auditoría y consultas administrativas. |
| RNF-03 | **Compatibilidad** | Chrome, Edge y Firefox modernos. Sin instalación local. |
| RNF-04 | **Usabilidad** | Interfaz intuitiva. Curva de aprendizaje mínima. Inspirada en el flujo del SIFCNP para usuarios operativos y con dashboard diferenciado para Administración. |
| RNF-05 | **Trazabilidad** | Toda acción crítica y administrativa debe dejar evidencia persistente, consultable y exportable. |
| RNF-06 | **Continuidad Operativa** | Las delegaciones vigentes deben evitar bloqueos del flujo de aprobación por ausencias o cambios temporales. |
| RNF-07 | **Configurabilidad** | La jerarquía de aprobación y las delegaciones deben ajustarse sin depender de cambios en fuentes externas ni despliegues de código. |

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
| **Acceso a Datos** | CRUD en `INTEGRA_CNP`, lecturas a vistas de WIZDOM y SIFCNP, y persistencia de estructuras internas de aprobación, delegación y auditoría. |

> La estructura organizacional importada de WIZDOM funciona como insumo de referencia. La matriz efectiva de aprobación, las delegaciones y la auditoría persistente residen en `INTEGRA_CNP`.

---

## 9. Modelo de Base de Datos

> **Base de datos:** `INTEGRA_CNP` | **Motor:** SQL Server 2022 | **Normalización:** 3FN  
> 
> **Setup canónico en dos scripts:**
> - `docs/db/001_integra_marcas_base_inicial.sql` — Base inicial con esquemas, tablas, catálogos y datos semilla (ejecutar primero)
> - `docs/db/002_integra_marcas_objetos.sql` — Función de aprobadores y 4 vistas de integración de solo lectura (ejecutar segundo)
>
> Ver detalle de consolidación en: `docs/db/ARCHIVOS_OBSOLETOS.md`
> Ver convenciones de nomenclatura en: `docs/db/Convenciones_Nomeclatura_BD.md`

### 9.1 Diagrama de Relaciones

```
Roles (1) ──────────────────────< Usuarios (N)
Cat_EstadosRegistro (1) ────────< Estructuras_Organizacionales (N)
Cat_EstadosRegistro (1) ────────< Jerarquias_Aprobacion (N)
Cat_EstadosRegistro (1) ────────< Delegaciones_Aprobacion (N)
Cat_TiposEventoAuditoria (1) ───< Auditoria_Eventos (N) >── Cat_ResultadosAuditoria (1)
                                      │
                    ┌─────────────────┴──────────────────┐
                    │ (UsuarioID — solicita)              │ (AprobadorID — resuelve)
                    ▼                                     ▼
         Justificaciones_Encabezado ◄── Estados (1..N)
                    │
                    └────< Justificaciones_Detalle >────── Cat_TiposJustificacion
```

### 9.2 Tablas Catálogo

#### Tabla: `Roles`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `RolID` | int | PK | NO | ID único del rol |
| `NombreRol` | varchar(50) | — | NO | Funcionario / Jefatura / RRHH / Administrador |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

#### Tabla: `Estados`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `EstadoID` | int | PK | NO | ID único del estado del flujo |
| `Descripcion` | varchar(100) | — | NO | Pendiente Jefatura / Aprobado / Rechazado |
| `Proceso` | varchar(50) | — | NO | `Marcas` |

#### Tabla: `Cat_TiposJustificacion`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `TipoJustificacionID` | int | PK | NO | ID único del tipo |
| `Descripcion` | varchar(100) | — | NO | Tardía / Omisión Entrada / Omisión Salida / Antes Hora Salida / Ausencia |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

#### Tabla: `Cat_EstadosRegistro`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `EstadoRegistroID` | int | PK | NO | ID del estado administrativo |
| `Descripcion` | varchar(50) | — | NO | Activo / Inactivo |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

#### Tabla: `Cat_TiposEventoAuditoria`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `TipoEventoAuditoriaID` | int | PK | NO | ID del evento auditable |
| `Descripcion` | varchar(100) | — | NO | Inicio de sesión, creación de boleta, aprobación, rechazo, alta de delegación, cambio de jerarquía, etc. |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

#### Tabla: `Cat_ResultadosAuditoria`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `ResultadoAuditoriaID` | int | PK | NO | ID del resultado |
| `Descripcion` | varchar(50) | — | NO | Éxito / Fallo / Denegado / Reintentado |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

### 9.3 Tablas Base

#### Tabla: `Usuarios`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `UsuarioID` | int | PK | NO | ID único del usuario |
| `Cedula` | varchar(20) | — | NO | Cédula del funcionario |
| `NombreCompleto` | varchar(150) | — | NO | Nombre completo |
| `Correo` | varchar(100) | — | NO | Correo institucional |
| `JefaturaID` | int | FK→Usuarios | SÍ | Jefatura directa importada como referencia |
| `UnidadID` | int | — | NO | ID de la unidad organizacional de origen |
| `RolID` | int | FK→Roles | NO | Rol del sistema |
| `Compania` | varchar(10) | — | NO | `CNP` o `FANAL` |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |
| `Usr_Modifica` | varchar(50) | — | SÍ | Último usuario que modificó |
| `Fec_Modifica` | datetime | — | SÍ | Fecha de modificación |

> 💡 `JefaturaID` se mantiene como FK auto-referenciada para referencia operativa, pero la autorización efectiva del flujo se resuelve en tablas propias de jerarquía y delegación.

#### Tabla: `Estructuras_Organizacionales`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `EstructuraOrganizacionalID` | int | PK | NO | ID del nodo o unidad configurable |
| `Nombre` | varchar(150) | — | NO | Nombre de la estructura |
| `CodigoOrigen` | varchar(50) | — | SÍ | Código externo si existe |
| `EstructuraPadreID` | int | FK→Estructuras_Organizacionales | SÍ | Nodo padre para relaciones verticales |
| `EstadoRegistroID` | int | FK→Cat_EstadosRegistro | NO | Activo / Inactivo |
| `VigenciaDesde` | datetime | — | SÍ | Inicio de vigencia |
| `VigenciaHasta` | datetime | — | SÍ | Fin de vigencia |

### 9.4 Tablas de Configuración y Control

#### Tabla: `Jerarquias_Aprobacion`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `JerarquiaAprobacionID` | int | PK | NO | ID de la regla jerárquica |
| `AprobadorUsuarioID` | int | FK→Usuarios | NO | Usuario aprobador titular |
| `EstructuraOrganizacionalID` | int | FK→Estructuras_Organizacionales | NO | Alcance configurado |
| `NivelAprobacion` | int | — | NO | Nivel dentro del modelo jerárquico |
| `TipoRelacion` | varchar(20) | — | NO | Horizontal / Vertical |
| `EstadoRegistroID` | int | FK→Cat_EstadosRegistro | NO | Activo / Inactivo |
| `VigenciaDesde` | datetime | — | NO | Inicio de vigencia |
| `VigenciaHasta` | datetime | — | SÍ | Fin de vigencia |

#### Tabla: `Delegaciones_Aprobacion`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `DelegacionAprobacionID` | int | PK | NO | ID de la delegación |
| `DeleganteUsuarioID` | int | FK→Usuarios | NO | Jefatura o aprobador titular |
| `DelegadoUsuarioID` | int | FK→Usuarios | NO | Delegado o subaprobador |
| `JerarquiaAprobacionID` | int | FK→Jerarquias_Aprobacion | SÍ | Regla específica asociada |
| `Motivo` | varchar(250) | — | SÍ | Razón de la delegación |
| `EstadoRegistroID` | int | FK→Cat_EstadosRegistro | NO | Activo / Inactivo |
| `VigenciaDesde` | datetime | — | NO | Inicio de vigencia |
| `VigenciaHasta` | datetime | — | SÍ | Fin de vigencia |
| `Usr_Registro` | varchar(50) | — | NO | Administrador que registró |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

### 9.5 Tablas de Proceso y Trazabilidad

#### Tabla: `Justificaciones_Encabezado`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `JustificacionID` | int | PK | NO | ID único de la justificación |
| `UsuarioID` | int | FK→Usuarios | NO | Funcionario solicitante |
| `MotivoGeneral` | varchar(500) | — | NO | Observación general |
| `EstadoID` | int | FK→Estados | NO | Estado actual del flujo |
| `FechaCreacion` | datetime | — | NO | Fecha y hora de creación |
| `AprobadorID` | int | FK→Usuarios | SÍ | Aprobador efectivo que resolvió |
| `FechaAprobacion` | datetime | — | SÍ | Fecha de aprobación o rechazo |
| `RolResolucion` | varchar(20) | — | SÍ | Rol con el que se procesó la resolución |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

#### Tabla: `Justificaciones_Detalle`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `DetalleID` | int | PK | NO | ID único del detalle |
| `JustificacionID` | int | FK→Justificaciones_Encabezado | NO | Encabezado padre |
| `TipoJustificacionID` | int | FK→Cat_TiposJustificacion | NO | Concepto a justificar |
| `FechaMarca` | date | — | NO | Fecha de la marca |
| `ObservacionDetalle` | varchar(250) | — | SÍ | Observación específica (opcional) |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó |
| `Fec_Registro` | datetime | — | NO | Fecha de creación |

#### Tabla: `Auditoria_Eventos`
| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `AuditoriaEventoID` | bigint | PK | NO | ID del evento auditado |
| `FechaEvento` | datetime | — | NO | Fecha y hora del evento |
| `UsuarioID` | int | FK→Usuarios | SÍ | Usuario que ejecutó la acción |
| `NombreUsuario` | varchar(150) | — | NO | Nombre del usuario al momento del evento |
| `RolCodigo` | varchar(20) | — | NO | Rol con el que actuó |
| `TipoEventoAuditoriaID` | int | FK→Cat_TiposEventoAuditoria | NO | Tipo de evento |
| `DescripcionEvento` | varchar(500) | — | NO | Descripción normalizada o catalogada |
| `ResultadoAuditoriaID` | int | FK→Cat_ResultadosAuditoria | NO | Resultado del evento |
| `ReferenciaFuncional` | varchar(100) | — | SÍ | ID de boleta, delegación, jerarquía u otra referencia |
| `PayloadResumen` | varchar(1000) | — | SÍ | Resumen técnico no sensible del evento |

### 9.6 Consideraciones del Modelo

- `INTEGRA_CNP` almacenará sus propias estructuras de aprobación y delegación, independientes de las fuentes externas.
- Los catálogos y estados administrativos permitirán absorber cambios organizacionales sin alterar la lógica base por código.
- La auditoría funcional y administrativa será persistente, filtrable y consultable solo por `ROL_ADMIN`.

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
      │  Sistema determina ruta según jerarquía activa en INTEGRA_CNP
      ▼
[APROBADOR EFECTIVO]
      │
      ├── Si existe delegación vigente, actúa el delegado con el mismo alcance
      │
      ├── Aprueba ──► Estado: "Aprobado" (2) + registra AprobadorID + FechaAprobacion + RolResolucion
      │
      └── Rechaza ──► Estado: "Rechazado" (3) + registra AprobadorID + FechaAprobacion + RolResolucion
                              │
                              ▼
                    [RRHH] consulta todas las justificaciones
                    [ADMIN] administra jerarquías, delegaciones y auditoría
```

### 10.2 Reglas de Negocio

| # | Regla |
|---|---|
| RN-01 | Una boleta **debe tener al menos una línea de detalle** para poder guardarse. |
| RN-02 | El estado inicial es siempre `Pendiente Jefatura` (EstadoID = 1). |
| RN-03 | Solo usuarios con autorización activa de aprobación, ya sea por jerarquía configurada o delegación vigente, pueden cambiar el estado de una boleta. |
| RN-04 | Una boleta `Aprobado` o `Rechazado` **no puede modificarse**. |
| RN-05 | Un funcionario solo ve **sus propias** justificaciones. |
| RN-06 | La visibilidad de jefaturas se resuelve con la matriz de `Jerarquias_Aprobacion` y `Delegaciones_Aprobacion`, no únicamente con `Usuarios.JefaturaID`. |
| RN-07 | RRHH ve **todas** las justificaciones sin excepción, pero sin acceso a operaciones administrativas ni auditoría. |
| RN-08 | Los datos de SIFCNP son de **solo lectura absoluta**. |
| RN-09 | Toda resolución de boleta y toda operación administrativa genera un evento en `Auditoria_Eventos`. |
| RN-10 | Las delegaciones o subaprobaciones solo pueden ser creadas, modificadas o desactivadas por `ROL_ADMIN`. |
| RN-11 | La auditoría persistente solo puede ser consultada por `ROL_ADMIN`. |
| RN-12 | La jerarquía configurable, las delegaciones y la auditoría persistente no existen hoy en las fuentes actuales; deben crearse y mantenerse dentro de `INTEGRA_CNP`. |

---

## 11. Integraciones Externas

### 11.1 WIZDOM (ERP — Solo Lectura)
- **Propósito:** Fuente de verdad para usuarios, estructura de origen y jefaturas directas.
- **Método:** Consulta a vistas de BD expuestas por WIZDOM. Sin escritura.
- **Datos:** Cédula, nombre, correo, jefatura, unidad, compañía (CNP=001 / FANAL=002).
- **Límite funcional:** La información de jefatura directa funciona como referencia operativa, pero **no** constituye por sí sola la lógica final de aprobaciones jerárquicas ni de delegaciones.
- **Recomendación:** Sincronización batch periódica (no tiempo real) para no impactar el ERP.

### 11.2 SIFCNP (Sistema histórico — Solo Lectura)
- **Propósito:** Consulta de registros anteriores a la migración.
- **Restricción:** `SELECT` únicamente; jamás `INSERT`, `UPDATE` o `DELETE`.
- **Interfaz:** Módulo separado con filtros por funcionario, fecha y tipo.
- **Límite funcional:** SIFCNP no participa en la jerarquía de aprobación, delegaciones ni auditoría operativa del nuevo sistema.

> La lógica de aprobaciones jerárquicas, delegaciones y auditoría persistente no existe hoy en WIZDOM ni en SIFCNP. Esa capacidad debe ser creada por el sistema y persistida en `INTEGRA_CNP`.

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
│       ├── 001_integra_marcas_base_inicial.sql ← Base inicial (ejecutar primero)
│       ├── 002_integra_marcas_objetos.sql ← Objetos e integración (ejecutar segundo)
│       ├── ARCHIVOS_OBSOLETOS.md ← Referencia histórica de scripts consolidados
│       ├── Convenciones_Nomeclatura_BD.md ← Estándares de nomenclatura
│       ├── flujos-datos-end-to-end.md ← Flujos de datos
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
- [ ] Crear tabla `Cat_EstadosRegistro` + datos semilla Activo/Inactivo
- [ ] Crear tabla `Cat_TiposEventoAuditoria` + datos semilla
- [ ] Crear tabla `Cat_ResultadosAuditoria` + datos semilla
- [ ] Crear tabla `Usuarios` con FK a `Roles`
- [ ] Crear tabla `Estructuras_Organizacionales`
- [ ] Crear tabla `Jerarquias_Aprobacion`
- [ ] Crear tabla `Delegaciones_Aprobacion`
- [ ] Crear tabla `Justificaciones_Encabezado` con FKs
- [ ] Crear tabla `Justificaciones_Detalle` con FKs
- [ ] Crear tabla `Auditoria_Eventos`
- [ ] Validar integridad referencial
- [ ] Crear índices de rendimiento (`UsuarioID`, `EstadoID`, `JustificacionID`, `JefaturaID`, `FechaEvento`, `TipoEventoAuditoriaID`, `EstadoRegistroID`)
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
- [ ] Sembrar y validar `ROL_ADMIN`
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
- [ ] **RF-07:** Endpoint consulta paginada de auditoría con filtros y exportación
- [ ] **RF-08:** Endpoint administración de jerarquías de aprobación
- [ ] **RF-09:** Endpoint administración de delegados y subaprobadores
- [ ] **RF-10:** Endpoint administración de catálogos y estructuras organizacionales

### 🖥️ Frontend — Integración con Backend
- [ ] Conectar login con autenticación real (quitar credenciales quemadas)
- [ ] Conectar Panel Funcionario con API de creación
- [ ] Conectar Panel Jefatura con API de aprobación/rechazo
- [ ] Conectar Panel RRHH con API de consulta global
- [ ] Conectar Panel SIFCNP con API de históricos
- [ ] Incorporar dashboard administrativo con auditoría paginada, filtros y descarga de reportes
- [ ] Incorporar pantallas de mantenimiento de jerarquías, delegaciones y catálogos
- [ ] Mostrar nombre real del usuario logueado en topbar
- [ ] Mostrar solo las pestañas correspondientes al rol del usuario
- [ ] Manejo de errores y mensajes del servidor en UI

### 🧪 Pruebas y Calidad
- [ ] Pruebas unitarias — flujo de estados (Pendiente → Aprobado/Rechazado)
- [ ] Pruebas de roles — verificar que cada rol solo accede a lo permitido
- [ ] Pruebas de autorización por jerarquía configurada
- [ ] Pruebas de delegación y subaprobación vigente
- [ ] Pruebas de restricción de auditoría exclusiva para Administrador
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
- **Auto-referencia en Usuarios:** `JefaturaID` apunta a otro `UsuarioID` en la misma tabla, pero se usa como referencia. La autorización efectiva se resuelve mediante `Jerarquias_Aprobacion` y `Delegaciones_Aprobacion`.

### Filtros por rol (queries base)
```sql
-- Funcionario: solo sus boletas
WHERE UsuarioID = @usuarioLogueado

-- Jefatura / aprobador: boletas según jerarquía activa o delegación vigente
WHERE EXISTS (
    SELECT 1
    FROM Jerarquias_Aprobacion JA
    WHERE JA.AprobadorUsuarioID = @usuarioLogueado
)
OR EXISTS (
    SELECT 1
    FROM Delegaciones_Aprobacion DA
    WHERE DA.DelegadoUsuarioID = @usuarioLogueado
      AND GETDATE() BETWEEN DA.VigenciaDesde AND ISNULL(DA.VigenciaHasta, GETDATE())
)

-- RRHH: todas las boletas (sin filtro de usuario)
-- (sin WHERE de usuario)

-- Administrador: auditoría paginada y consultas administrativas
ORDER BY FechaEvento DESC
```

### Reglas críticas
- **WIZDOM y SIFCNP son de solo lectura ABSOLUTA.** Nunca generar `INSERT/UPDATE/DELETE` sobre esas bases.
- **Estados son IDs fijos:**
  - `1` = Pendiente Jefatura
  - `2` = Aprobado
  - `3` = Rechazado
- **Compañías:** `CNP` = código `001`, `FANAL` = código `002`.
- **Auditoría:** Todas las tablas tienen `Usr_Registro` y `Fec_Registro`. Las principales también `Usr_Modifica` y `Fec_Modifica`.
- **Pistas de auditoría persistentes:** Registrar fecha/hora, usuario, nombre, rol, tipo de evento, resultado y referencia funcional en `Auditoria_Eventos`.
- **Acceso a auditoría:** Exclusivo de `ROL_ADMIN` mediante dashboard paginado, filtros y descarga de reportes.
- Una boleta con EstadoID = 2 o 3 **no se puede modificar** (RN-04).
- La jerarquía configurable, las delegaciones y la auditoría persistente deben construirse en `INTEGRA_CNP`; no existen hoy en las fuentes actuales.

### Estado del frontend
El prototipo usa **datos mockeados** (quemados en HTML). Al integrar el backend:
1. Reemplazar datos hardcoded con llamadas a la API.
2. Reemplazar las credenciales `admin/1234` con autenticación real.
3. Mostrar pestañas del dashboard según el `RolID` del usuario autenticado.

---

*Basado en: Análisis y Diseño de SI "Integrador Marcas" v1.1 — CNP / UTI — Octubre 2025*
