# PRP — Sistema Web "Integrador Vehículos"
> **Product Requirements Plan** | Consejo Nacional de Producción (CNP) & FANAL  
> Versión: 1.0 | Basado en: Análisis y Diseño de SI v1.1 (24/10/2025)  
> Organización: Unidad de Tecnologías de la Información (UTI)  
> **Fase:** 2 — Continuación del plan de modernización (después de Marcas)  
> Última actualización: Abril 2026

---

## 📋 Tabla de Contenidos

1. [Resumen del Producto](#1-resumen-del-producto)
2. [Contexto y Dependencias](#2-contexto-y-dependencias)
3. [Stack Tecnológico](#3-stack-tecnológico)
4. [Roles del Sistema](#4-roles-del-sistema)
5. [Requerimientos Funcionales](#5-requerimientos-funcionales)
6. [Requerimientos No Funcionales](#6-requerimientos-no-funcionales)
7. [Arquitectura del Software](#7-arquitectura-del-software)
8. [Modelo de Base de Datos](#8-modelo-de-base-de-datos)
9. [Lógica de Negocio y Flujo](#9-lógica-de-negocio-y-flujo)
10. [Integraciones Externas](#10-integraciones-externas)
11. [Checklist de Progreso](#11-checklist-de-progreso)
12. [Notas Clave para IA Asistente](#12-notas-clave-para-ia-asistente)

---

## 1. Resumen del Producto

| Campo | Detalle |
|---|---|
| **Nombre del sistema** | Integrador Vehículos / Sistema de Solicitud de Vehículos |
| **Tipo** | Aplicación Web (desarrollo interno) |
| **Institución** | Consejo Nacional de Producción (CNP) y FANAL |
| **Fase** | **Fase 2** del plan de modernización de sistemas internos |
| **Prerrequisito** | Fase 1 (Integrador Marcas) estabilizada y en producción |
| **Reemplaza a** | Módulo de Solicitud de Vehículos del sistema SICNP (escritorio obsoleto) |
| **Base de datos** | `INTEGRA_CNP` — SQL Server 2019 R14 (compartida con Fase 1) |
| **Metodología** | Prototipado iterativo (continuación de Fase 1) |
| **Elaborado por** | Lic. Luis Diego Vega Soto, M.Ed. |
| **Revisado por** | MGTI. Sindy Mayorga Matarrita, M.Ed. |
| **Técnico programador** | Sr. Raúl Ortega Acuña |
| **DBA** | Rudy Antonio Arias Rodríguez |
| **Analista Programador** | Manuel Montealegre Bejarano |
| **Versión doc.** | 1.1 — 24/10/2025 |

---

## 2. Contexto y Dependencias

### Problema actual
El proceso de **Solicitud de Vehículos Oficiales** se gestiona a través del sistema de escritorio SICNP, el cual presenta:
- Incompatibilidad con Windows 11 y SO modernos.
- Interrupción crítica del servicio para funcionarios.
- Sin soporte para el flujo de doble aprobación de forma centralizada.

### Solución propuesta
Sistema web que digitaliza y centraliza el flujo completo de solicitud de vehículos: desde la creación por parte del funcionario, pasando por la aprobación de jefatura, hasta la autorización y gestión logística de Servicios Institucionales (asignación de vehículo, registro de kilometraje).

### ⚠️ Dependencia crítica con Fase 1
Este módulo **comparte la base de datos `INTEGRA_CNP`** y reutiliza las tablas ya creadas en la Fase 1:

| Tabla reutilizada | Origen | Descripción |
|---|---|---|
| `Usuarios` | Fase 1 — Marcas | Todos los funcionarios sincronizados desde WIZDOM |
| `Roles` | Fase 1 — Marcas | Catálogo de roles del sistema |
| `Estados` | Fase 1 — Marcas | Se extiende con los nuevos estados de Vehículos |

> 🚨 **No recrear estas tablas.** Solo agregar los nuevos estados de Vehículos en la tabla `Estados` existente y crear las tablas nuevas: `Cat_Vehiculos`, `SolicitudesVehiculos_Encabezado`, `SolicitudesVehiculos_Acompanantes`.

---

## 3. Stack Tecnológico

| Capa | Tecnología |
|---|---|
| **Frontend** | HTML5, CSS3, JavaScript |
| **Backend** | C# (.NET) |
| **Base de datos** | Microsoft SQL Server 2019 R14 — `INTEGRA_CNP` (existente desde Fase 1) |
| **BD lectura ERP** | WIZDOM (vistas de solo lectura — ya integrado en Fase 1) |
| **BD histórica** | SIFCNP (solo lectura — ya integrado en Fase 1) |
| **Navegadores** | Chrome, Edge, Firefox |
| **Arquitectura** | Monolítica 3 capas (misma que Fase 1) |

---

## 4. Roles del Sistema

| Rol | Código | Descripción en este módulo |
|---|---|---|
| **Funcionario** | `ROL_FUNC` | Crea solicitudes de vehículo y consulta el estado de las propias. |
| **Jefatura** | `ROL_JEFE` | Aprueba o rechaza las solicitudes de los funcionarios a su cargo (1ª aprobación). |
| **Servicios Institucionales** | `ROL_SERV` | Autoriza o rechaza la solicitud aprobada por jefatura. Asigna vehículo, registra kilometraje de salida y reingreso (2ª aprobación). |

> 💡 Los roles `ROL_FUNC` y `ROL_JEFE` ya existen en la tabla `Roles` de la Fase 1. El rol `ROL_SERV` (Servicios Institucionales) es **nuevo** y debe agregarse como dato semilla.

---

## 5. Requerimientos Funcionales

### RF-01 — Creación de Solicitud de Vehículo
- Un funcionario con `ROL_FUNC` puede crear una nueva solicitud con los siguientes datos:
  - **Destino:** lugar o lugares de la gira (varchar 250).
  - **Motivo:** objetivo de la salida (varchar 500).
  - **Fecha y hora de salida** (`FechaSalida`).
  - **Fecha y hora de regreso estimado** (`FechaRegreso`).
  - **¿Requiere chofer?** (campo `NecesitaChofer`: Sí / No).
  - **Funcionarios acompañantes:** lista de 0 o más funcionarios registrados en el sistema.
- Estado inicial: **`Pendiente Jefatura`**.
- El sistema notifica automáticamente a la jefatura directa del solicitante.

---

### RF-02 — Aprobación de Jefatura (1ª aprobación)
- Un usuario con `ROL_JEFE` puede ver las solicitudes de los funcionarios **directamente bajo su cargo**.
- Puede:
  - **Aprobar:** estado cambia a `Pendiente Autorización`. Se notifica a Servicios Institucionales.
  - **Rechazar:** estado cambia a `Rechazado`. Flujo termina.
- Registra: `AprobadorJefeID` y `FechaAprobacionJefe`.

---

### RF-03 — Autorización de Servicios Institucionales (2ª aprobación)
- Un usuario con `ROL_SERV` puede ver las solicitudes en estado `Pendiente Autorización`.
- Puede:
  - **Autorizar:** asigna un `VehiculoID` del catálogo de flotilla, registra `KilometrajeSalida`. Estado cambia a `Vehículo Asignado`.
  - **Rechazar:** estado cambia a `No Autorizado`. Flujo termina.
- Registra: `AutorizadorID` y `FechaAutorizacion`.

---

### RF-04 — Registro de Reingreso
- Un usuario con `ROL_SERV` puede registrar el **regreso del vehículo** al final de la gira.
- Ingresa: `KilometrajeRegreso`.
- Estado cambia a **`Finalizado`**.
- Solo aplica a solicitudes en estado `Vehículo Asignado`.

---

### RF-05 — Consulta de Estado de Solicitud
- Un `ROL_FUNC` puede consultar el historial y estado actual de **sus propias** solicitudes.
- Un `ROL_JEFE` puede consultar el estado de las solicitudes de **sus subordinados directos**.
- Ambos ven: datos de la gira, estado, quién aprobó/autorizó, vehículo asignado, kilometrajes.

---

### RF-06 — Consulta de Registros Históricos (SIFCNP)
- Interfaz de **solo lectura** para consultar solicitudes de vehículos del sistema SIFCNP.
- Sin posibilidad de crear, editar ni eliminar.
- Filtros básicos: funcionario, fecha, estado.

---

## 6. Requerimientos No Funcionales

| ID | Categoría | Descripción |
|---|---|---|
| RNF-01 | **Seguridad** | Control de acceso por roles (Funcionario, Jefatura, Servicios Institucionales). Autenticación integrada con Active Directory o usuarios de dominio CNP. |
| RNF-02 | **Rendimiento** | Consultas eficientes a `INTEGRA_CNP`. No impactar rendimiento del ERP WIZDOM. |
| RNF-03 | **Compatibilidad** | Chrome, Edge y Firefox modernos. Sin instalación local. |
| RNF-04 | **Usabilidad** | Interfaz intuitiva. Flujo inspirado en la pantalla del SIFCNP para facilitar adopción. |

---

## 7. Arquitectura del Software

```
┌──────────────────────────────────────────────────────────┐
│           CAPA DE PRESENTACIÓN (Frontend)                │
│             HTML5 + CSS3 + JavaScript                    │
│  Panel Funcionario / Panel Jefatura /                    │
│  Panel Servicios Institucionales / Panel SIFCNP          │
└───────────────────────────┬──────────────────────────────┘
                            │
┌───────────────────────────▼──────────────────────────────┐
│         CAPA DE LÓGICA DE NEGOCIO (Backend C# .NET)      │
│  - Flujo de doble aprobación (Jefatura → Serv. Inst.)    │
│  - Control de acceso por roles                           │
│  - Notificaciones automáticas por cambio de estado       │
│  - Validación de kilometraje (Regreso > Salida)          │
│  - Gestión del catálogo de vehículos                     │
└──────┬─────────────────────┬──────────────────┬──────────┘
       │                     │                  │
┌──────▼──────┐   ┌──────────▼──────┐  ┌────────▼──────┐
│ INTEGRA_CNP │   │    WIZDOM       │  │    SIFCNP     │
│ SQL Srv2019 │   │ (Solo Lectura)  │  │ (Solo Lectura)│
│ Tablas F1+  │   │ Vistas usuarios │  │  Históricos   │
│ Tablas F2   │   │                 │  │  Vehículos    │
└─────────────┘   └─────────────────┘  └───────────────┘
```

---

## 8. Modelo de Base de Datos

> **Base de datos:** `INTEGRA_CNP` (existente) | **Motor:** SQL Server 2019 R14  
> **Estrategia:** Agregar nuevas tablas al modelo ya existente de la Fase 1.

---

### 8.1 Tablas nuevas en Fase 2

```
TABLAS REUTILIZADAS (Fase 1 — NO recrear):
  Usuarios, Roles, Estados

TABLAS NUEVAS (Fase 2 — crear):
  Cat_Vehiculos
  SolicitudesVehiculos_Encabezado
  SolicitudesVehiculos_Acompanantes
```

---

### 8.2 Diagrama de Relaciones

```
Roles (1) ──────────────────────────────< Usuarios (N)
                                              │
              ┌───────────────────────────────┼──────────────────────────────┐
              │ (SolicitanteID)               │ (AprobadorJefeID)            │ (AutorizadorID)
              ▼                               ▼                              ▼
    SolicitudesVehiculos_Encabezado ◄──── Estados
              │                    ◄──── Cat_Vehiculos (VehiculoID)
              │
              └──────< SolicitudesVehiculos_Acompanantes >── Usuarios (FuncionarioID)
```

---

### 8.3 Tabla: `Cat_Vehiculos` *(nueva)*

| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `VehiculoID` | int | PK | NO | Identificador único del vehículo |
| `Placa` | varchar(10) | — | NO | Número de placa (único) |
| `Marca` | varchar(50) | — | NO | Marca del vehículo (ej. Toyota) |
| `Modelo` | varchar(50) | — | NO | Modelo del vehículo (ej. Hilux) |
| `Periodo_Vehiculo` | int | — | NO | Año del vehículo |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |

---

### 8.4 Tabla: `SolicitudesVehiculos_Encabezado` *(nueva)*

| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `SolicitudID` | int | PK | NO | Identificador único de la solicitud |
| `SolicitanteID` | int | FK→Usuarios | NO | Funcionario que crea la solicitud |
| `Destino` | varchar(250) | — | NO | Lugar(es) de destino de la gira |
| `Motivo` | varchar(500) | — | NO | Objetivo / motivo de la salida |
| `FechaSalida` | datetime | — | NO | Fecha y hora de salida |
| `FechaRegreso` | datetime | — | NO | Fecha y hora de regreso estimado |
| `NecesitaChofer` | bit | — | NO | `1` = Requiere chofer, `0` = No requiere |
| `EstadoID` | int | FK→Estados | NO | Estado actual de la solicitud |
| `AprobadorJefeID` | int | FK→Usuarios | SÍ | Jefatura que aprueba/rechaza (NULL hasta que actúe) |
| `FechaAprobacionJefe` | datetime | — | SÍ | Fecha de acción de jefatura |
| `AutorizadorID` | int | FK→Usuarios | SÍ | Encargado S.I. que autoriza/rechaza (NULL hasta que actúe) |
| `FechaAutorizacion` | datetime | — | SÍ | Fecha de acción de Servicios Institucionales |
| `VehiculoID` | int | FK→Cat_Vehiculos | SÍ | Vehículo asignado (NULL hasta autorización) |
| `KilometrajeSalida` | decimal(10,2) | — | SÍ | Km del vehículo al salir (NULL hasta autorización) |
| `KilometrajeRegreso` | decimal(10,2) | — | SÍ | Km del vehículo al regresar (NULL hasta reingreso) |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |

---

### 8.5 Tabla: `SolicitudesVehiculos_Acompanantes` *(nueva)*

| Campo | Tipo | PK/FK | Nulo | Descripción |
|---|---|---|---|---|
| `AcompananteID` | int | PK | NO | Identificador único del registro |
| `SolicitudID` | int | FK→SolicitudesVehiculos_Encabezado | NO | Solicitud a la que pertenece |
| `FuncionarioID` | int | FK→Usuarios | NO | Funcionario que acompaña la gira |
| `Usr_Registro` | varchar(50) | — | NO | Usuario que creó el registro |
| `Fec_Registro` | datetime | — | NO | Fecha de creación del registro |

---

### 8.6 Estados nuevos para Vehículos

> Agregar los siguientes registros a la tabla `Estados` existente (proceso = `'Vehiculos'`):

```sql
INSERT INTO Estados (EstadoID, Descripcion, Proceso) VALUES
(4, 'Pendiente Jefatura',      'Vehiculos'),
(5, 'Pendiente Autorización',  'Vehiculos'),
(6, 'Vehículo Asignado',       'Vehiculos'),
(7, 'Rechazado',               'Vehiculos'),
(8, 'No Autorizado',           'Vehiculos'),
(9, 'Finalizado',              'Vehiculos');
```

> ⚠️ Los IDs 1-3 ya están ocupados por los estados de Marcas. Verificar el último ID usado antes de insertar.

---

### 8.7 Rol nuevo para Vehículos

```sql
INSERT INTO Roles (RolID, NombreRol, Usr_Registro, Fec_Registro) VALUES
(4, 'Servicios Institucionales', 'SYSTEM', GETDATE());
```

---

### 8.8 Script DDL — Tablas nuevas

```sql
USE INTEGRA_CNP;
GO

-- Catálogo de vehículos de la flotilla
CREATE TABLE Cat_Vehiculos (
    VehiculoID       INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    Placa            VARCHAR(10)  NOT NULL UNIQUE,
    Marca            VARCHAR(50)  NOT NULL,
    Modelo           VARCHAR(50)  NOT NULL,
    Periodo_Vehiculo INT          NOT NULL,
    Usr_Registro     VARCHAR(50)  NOT NULL,
    Fec_Registro     DATETIME     NOT NULL DEFAULT GETDATE()
);

-- Tabla maestra de solicitudes
CREATE TABLE SolicitudesVehiculos_Encabezado (
    SolicitudID          INT            NOT NULL IDENTITY(1,1) PRIMARY KEY,
    SolicitanteID        INT            NOT NULL REFERENCES Usuarios(UsuarioID),
    Destino              VARCHAR(250)   NOT NULL,
    Motivo               VARCHAR(500)   NOT NULL,
    FechaSalida          DATETIME       NOT NULL,
    FechaRegreso         DATETIME       NOT NULL,
    NecesitaChofer       BIT            NOT NULL DEFAULT 0,
    EstadoID             INT            NOT NULL REFERENCES Estados(EstadoID),
    AprobadorJefeID      INT            NULL     REFERENCES Usuarios(UsuarioID),
    FechaAprobacionJefe  DATETIME       NULL,
    AutorizadorID        INT            NULL     REFERENCES Usuarios(UsuarioID),
    FechaAutorizacion    DATETIME       NULL,
    VehiculoID           INT            NULL     REFERENCES Cat_Vehiculos(VehiculoID),
    KilometrajeSalida    DECIMAL(10,2)  NULL,
    KilometrajeRegreso   DECIMAL(10,2)  NULL,
    Usr_Registro         VARCHAR(50)    NOT NULL,
    Fec_Registro         DATETIME       NOT NULL DEFAULT GETDATE()
);

-- Tabla hija de acompañantes
CREATE TABLE SolicitudesVehiculos_Acompanantes (
    AcompananteID  INT          NOT NULL IDENTITY(1,1) PRIMARY KEY,
    SolicitudID    INT          NOT NULL REFERENCES SolicitudesVehiculos_Encabezado(SolicitudID),
    FuncionarioID  INT          NOT NULL REFERENCES Usuarios(UsuarioID),
    Usr_Registro   VARCHAR(50)  NOT NULL,
    Fec_Registro   DATETIME     NOT NULL DEFAULT GETDATE()
);

-- Índices recomendados
CREATE INDEX IX_SolVeh_SolicitanteID ON SolicitudesVehiculos_Encabezado(SolicitanteID);
CREATE INDEX IX_SolVeh_EstadoID      ON SolicitudesVehiculos_Encabezado(EstadoID);
CREATE INDEX IX_SolVeh_VehiculoID    ON SolicitudesVehiculos_Encabezado(VehiculoID);
CREATE INDEX IX_SolVehAc_SolicitudID ON SolicitudesVehiculos_Acompanantes(SolicitudID);
```

---

## 9. Lógica de Negocio y Flujo

### 9.1 Flujo completo de una Solicitud de Vehículo

```
[FUNCIONARIO — ROL_FUNC]
      │
      ▼
  Crea solicitud (Destino, Motivo, Fechas, Chofer, Acompañantes)
      │
      ▼
  Estado: "Pendiente Jefatura" (EstadoID = 4)
      │
      │  ← Sistema notifica automáticamente a la jefatura directa
      ▼
[JEFATURA — ROL_JEFE] — solo ve solicitudes de sus subordinados directos
      │
      ├── Aprueba ──► Estado: "Pendiente Autorización" (5)
      │                │  ← Sistema notifica a Servicios Institucionales
      │                ▼
      │         [SERVICIOS INSTITUCIONALES — ROL_SERV]
      │                │
      │                ├── Autoriza ──► Asigna VehiculoID + KilometrajeSalida
      │                │               Estado: "Vehículo Asignado" (6)
      │                │                   │
      │                │                   │  (al finalizar la gira)
      │                │                   ▼
      │                │         Registra KilometrajeRegreso
      │                │         Estado: "Finalizado" (9)
      │                │
      │                └── Rechaza ──► Estado: "No Autorizado" (8)
      │                               Flujo termina.
      │
      └── Rechaza ──► Estado: "Rechazado" (7)
                      Flujo termina.
```

### 9.2 Reglas de Negocio

| # | Regla |
|---|---|
| RN-01 | El estado inicial de toda solicitud nueva es siempre `Pendiente Jefatura` (EstadoID = 4). |
| RN-02 | Solo `ROL_JEFE` puede hacer la 1ª aprobación/rechazo. |
| RN-03 | Solo `ROL_SERV` puede hacer la 2ª autorización/rechazo y el registro de reingreso. |
| RN-04 | Una solicitud `Rechazado` o `No Autorizado` **no puede modificarse**. Flujo terminado. |
| RN-05 | `ROL_SERV` solo puede autorizar solicitudes en estado `Pendiente Autorización` (ya aprobadas por jefatura). |
| RN-06 | `KilometrajeRegreso` debe ser **mayor o igual** a `KilometrajeSalida`. Validar antes de guardar. |
| RN-07 | El registro de reingreso solo aplica a solicitudes en estado `Vehículo Asignado`. |
| RN-08 | Un `ROL_FUNC` solo puede ver **sus propias** solicitudes. |
| RN-09 | Un `ROL_JEFE` solo puede ver solicitudes de funcionarios con `JefaturaID = jefaturaLogueada`. |
| RN-10 | `FechaRegreso` debe ser **posterior** a `FechaSalida`. Validar en frontend y backend. |
| RN-11 | La misma persona no puede aparecer como `SolicitanteID` y `FuncionarioID` en `Acompanantes` de la misma solicitud. |
| RN-12 | Los datos históricos de SIFCNP son de **solo lectura absoluta** — nunca escribir en esa BD. |

---

## 10. Integraciones Externas

### 10.1 WIZDOM (ERP — Solo Lectura) ✅ Ya integrado en Fase 1
- Reutilizar la integración y la tabla `Usuarios` ya sincronizada.
- No requiere trabajo adicional salvo validar que `ROL_SERV` esté correctamente asignado.

### 10.2 SIFCNP (Sistema histórico — Solo Lectura)
- Nueva consulta de históricos de **solicitudes de vehículos** (distinto módulo al de marcas).
- Solo `SELECT` — nunca `INSERT/UPDATE/DELETE`.
- La interfaz es independiente del módulo histórico de marcas.

---

## 11. Checklist de Progreso

> Marcar con `[x]` al completar. Este módulo inicia **después** de que Fase 1 (Marcas) esté en producción.

### ✅ Prerequisito
- [ ] Fase 1 (Integrador Marcas) estabilizada y en producción
- [ ] BD `INTEGRA_CNP` con tablas de Fase 1 validadas (`Usuarios`, `Roles`, `Estados`)

### 🗄️ Base de Datos — Extensión de INTEGRA_CNP
- [ ] Insertar nuevo rol `Servicios Institucionales` en tabla `Roles`
- [ ] Insertar nuevos estados de Vehículos en tabla `Estados` (IDs 4-9 o continuación)
- [ ] Crear tabla `Cat_Vehiculos` + poblar con flotilla actual
- [ ] Crear tabla `SolicitudesVehiculos_Encabezado` con todas las FKs
- [ ] Crear tabla `SolicitudesVehiculos_Acompanantes` con FKs
- [ ] Crear índices de rendimiento
- [ ] Validar integridad referencial completa con tablas de Fase 1
- [ ] Ejecutar script en ambiente de desarrollo

### 🔐 Autenticación y Roles
- [ ] Agregar `ROL_SERV` (Servicios Institucionales) al control de acceso del sistema
- [ ] Validar que el panel de Vehículos respete la visibilidad por rol
- [ ] Asignar rol `Servicios Institucionales` a los usuarios correspondientes

### 🚗 Módulo: Catálogo de Vehículos
- [ ] Pantalla de gestión del catálogo (listar, agregar, editar vehículos)
- [ ] Validar placa única al registrar vehículo
- [ ] Solo `ROL_SERV` o Administrador puede gestionar el catálogo

### 📝 RF-01 — Creación de Solicitud
- [ ] Formulario de solicitud (Destino, Motivo, Fechas/Horas, Chofer)
- [ ] Validar `FechaRegreso > FechaSalida` (RN-10)
- [ ] Grilla de acompañantes: buscar y agregar funcionarios
- [ ] Validar que el solicitante no se agregue a sí mismo como acompañante (RN-11)
- [ ] Guardar con estado inicial `Pendiente Jefatura`
- [ ] Notificación automática a jefatura directa al crear

### ✅ RF-02 — Aprobación de Jefatura
- [ ] Vista Jefatura: listado de solicitudes de sus subordinados
- [ ] Filtro por estado (Pendientes, Aprobadas, Rechazadas)
- [ ] Ver detalle completo de la solicitud + acompañantes
- [ ] Acción Aprobar → estado `Pendiente Autorización` + registrar AprobadorJefeID + Fecha
- [ ] Acción Rechazar → estado `Rechazado` + registrar AprobadorJefeID + Fecha
- [ ] Notificación automática a Servicios Institucionales al aprobar

### 🔑 RF-03 — Autorización de Servicios Institucionales
- [ ] Vista S.I.: listado de solicitudes en estado `Pendiente Autorización`
- [ ] Ver detalle completo de la solicitud
- [ ] Acción Autorizar: selector de vehículo disponible del catálogo
- [ ] Acción Autorizar: campo `KilometrajeSalida`
- [ ] Acción Autorizar → estado `Vehículo Asignado` + registrar AutorizadorID + VehiculoID + KmSalida + Fecha
- [ ] Acción Rechazar → estado `No Autorizado` + registrar AutorizadorID + Fecha

### 🏁 RF-04 — Registro de Reingreso
- [ ] Vista S.I.: listado de solicitudes en estado `Vehículo Asignado`
- [ ] Formulario de reingreso: campo `KilometrajeRegreso`
- [ ] Validar `KilometrajeRegreso >= KilometrajeSalida` (RN-06)
- [ ] Guardar → estado `Finalizado`

### 🔎 RF-05 — Consulta de Estado
- [ ] Vista Funcionario: historial de sus propias solicitudes con estado actual
- [ ] Vista Jefatura: historial de solicitudes de sus subordinados
- [ ] Ver detalle completo de cada solicitud (datos, aprobadores, vehículo, kilómetros)
- [ ] Indicador visual de estado por color/ícono

### 📂 RF-06 — Consulta Histórica SIFCNP (Vehículos)
- [ ] Pantalla de consulta de registros históricos de vehículos
- [ ] Filtros: funcionario, fecha, estado
- [ ] Vista de solo lectura (sin acciones de edición)

### 🧪 Pruebas y Calidad
- [ ] Pruebas del flujo completo: Creación → Aprobación → Autorización → Reingreso
- [ ] Pruebas de flujos negativos: Rechazo en jefatura / Rechazo en S.I.
- [ ] Pruebas de validación de kilometraje (RN-06)
- [ ] Pruebas de validación de fechas (RN-10)
- [ ] Pruebas de roles: cada rol accede solo a lo permitido
- [ ] Pruebas de integración con SIFCNP histórico
- [ ] Pruebas en Chrome, Edge y Firefox
- [ ] Validación con funcionarios piloto
- [ ] Validación con jefaturas piloto
- [ ] Validación con encargado de Servicios Institucionales

### 🚀 Despliegue
- [ ] Ambiente de desarrollo configurado para Fase 2
- [ ] Scripts DDL ejecutados en QA (sobre BD existente de Fase 1)
- [ ] Pruebas en ambiente QA validadas
- [ ] Scripts DDL ejecutados en producción
- [ ] **Puesta en producción Fase 2**
- [ ] Manual de usuario para Servicios Institucionales
- [ ] Capacitación a usuarios piloto (especialmente ROL_SERV)

---

## 12. Notas Clave para IA Asistente

> Contexto esencial para cualquier LLM que asista en el desarrollo de este módulo.

### Diferencia clave con Fase 1 (Marcas)
Este módulo tiene un flujo de **doble aprobación**, lo que significa dos actores distintos deben actuar secuencialmente antes de que la solicitud avance:
1. **Jefatura** (1ª aprobación) → cambia estado de `Pendiente Jefatura` a `Pendiente Autorización`.
2. **Servicios Institucionales** (2ª aprobación) → cambia estado de `Pendiente Autorización` a `Vehículo Asignado`.

### Múltiples FKs a Usuarios en la misma tabla
`SolicitudesVehiculos_Encabezado` tiene **tres** referencias a `Usuarios`:
```sql
SolicitanteID   → quién crea la solicitud
AprobadorJefeID → quién la aprueba (jefatura)
AutorizadorID   → quién la autoriza (Servicios Institucionales)
```
Al hacer JOINs, usar aliases para distinguirlos:
```sql
SELECT
    u1.NombreCompleto AS Solicitante,
    u2.NombreCompleto AS Jefe,
    u3.NombreCompleto AS Autorizador,
    v.Placa           AS Vehiculo
FROM SolicitudesVehiculos_Encabezado s
    JOIN Usuarios u1 ON s.SolicitanteID   = u1.UsuarioID
    LEFT JOIN Usuarios u2 ON s.AprobadorJefeID = u2.UsuarioID
    LEFT JOIN Usuarios u3 ON s.AutorizadorID   = u3.UsuarioID
    LEFT JOIN Cat_Vehiculos v ON s.VehiculoID  = v.VehiculoID
WHERE s.SolicitudID = @SolicitudID;
```

### Filtros por rol (queries base)
```sql
-- Funcionario: solo sus solicitudes
WHERE SolicitanteID = @usuarioLogueado

-- Jefatura: solicitudes de sus subordinados directos
WHERE SolicitanteID IN (
    SELECT UsuarioID FROM Usuarios WHERE JefaturaID = @usuarioLogueado
)

-- Servicios Institucionales: solicitudes en estado Pendiente Autorización
WHERE EstadoID = 5  -- 'Pendiente Autorización'

-- Servicios Institucionales para reingreso: estado Vehículo Asignado
WHERE EstadoID = 6  -- 'Vehículo Asignado'
```

### Reglas críticas de validación
```
FechaRegreso > FechaSalida              → validar en frontend Y backend
KilometrajeRegreso >= KilometrajeSalida → validar antes de guardar reingreso
SolicitanteID ≠ FuncionarioID en Acompanantes → validar al agregar acompañante
```

### Estados en orden de flujo
```
4 = Pendiente Jefatura     ← Estado inicial
5 = Pendiente Autorización ← Después de aprobación de jefatura
6 = Vehículo Asignado      ← Después de autorización de S.I.
7 = Rechazado              ← Si jefatura rechaza (flujo termina)
8 = No Autorizado          ← Si S.I. rechaza (flujo termina)
9 = Finalizado             ← Después de registro de reingreso
```

### Tablas compartidas con Fase 1 — NO tocar estructura
- `Usuarios` — solo lectura/inserción de datos, no modificar columnas.
- `Roles` — solo agregar el nuevo rol `Servicios Institucionales`.
- `Estados` — solo agregar los nuevos estados de Vehículos.

---

*Basado en: Análisis y Diseño de SI "Integrador Vehículos" v1.1 — CNP / UTI — Octubre 2025*
