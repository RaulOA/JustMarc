# Manual de Administrador — SIFCNP

**Sistema de Justificación de Marcas**

| Dato | Valor |
|---|---|
| **Producto** | SIFCNP — Sistema de Justificación de Marcas |
| **Versión del manual** | 1.0.0 |
| **Fecha** | 27 de junio de 2026 |
| **Público objetivo** | Personas con rol **Administrador** del sistema (ROL_ADMIN) del CNP y FANAL |
| **Organización emisora** | Unidad de Tecnologías de Información (UTI) — Consejo Nacional de Producción (CNP) |
| **Idioma** | Español (Costa Rica) |

> **¿Para qué sirve este manual?** Para que la persona administradora aprenda a **configurar y mantener** SIFCNP: organizar las dependencias, asignar roles a los usuarios, definir quién aprueba las boletas (jerarquías y delegaciones), revisar la auditoría del sistema, y entender el respaldo y el mantenimiento. Está escrito en lenguaje claro; cuando aparece un término técnico, se explica en una línea.

---

## Tabla de contenidos

1. [Qué administra usted en SIFCNP](#1-qué-administra-usted-en-sifcnp)
2. [Conceptos básicos de administración](#2-conceptos-básicos-de-administración)
3. [Antes de empezar (requisitos)](#3-antes-de-empezar-requisitos)
4. [Ingresar al Panel de Administración](#4-ingresar-al-panel-de-administración)
5. [Gestión de dependencias](#5-gestión-de-dependencias)
6. [Gestión de usuarios y roles](#6-gestión-de-usuarios-y-roles)
7. [Jerarquías de aprobación](#7-jerarquías-de-aprobación)
8. [Delegaciones de aprobación](#8-delegaciones-de-aprobación)
9. [Registros: monitoreo y auditoría](#9-registros-monitoreo-y-auditoría)
10. [Configuración del sistema](#10-configuración-del-sistema)
11. [Respaldos](#11-respaldos)
12. [Mantenimiento](#12-mantenimiento)
13. [Solución de problemas y mensajes](#13-solución-de-problemas-y-mensajes)
14. [Seguridad y limitaciones conocidas](#14-seguridad-y-limitaciones-conocidas)
15. [Glosario](#15-glosario)
16. [Trazabilidad con las normas](#16-trazabilidad-con-las-normas)
17. [Fuentes](#17-fuentes)

---

## 1. Qué administra usted en SIFCNP

Como **administrador**, usted no llena ni aprueba boletas: su tarea es **mantener el sistema en orden** para que el flujo de justificaciones funcione. Desde el **Panel de Administración** puede:

- Organizar las **dependencias** (la estructura de la institución).
- Asignar a cada **usuario** su rol, su unidad y su jefatura, y activarlo o desactivarlo.
- Definir las **jerarquías de aprobación**: quién aprueba a quién.
- Registrar **delegaciones**: cuando una jefatura cede temporalmente la aprobación a otra persona.
- Revisar los **registros** de auditoría: errores del sistema y eventos importantes.

Todo lo que usted cambia aquí **afecta directamente** quién puede ver y aprobar las boletas.

![Pantalla de ingreso de SIFCNP con el usuario administrador escrito en el campo Usuario.](capturas/01-ingreso-admin.png)

*Figura 1. Ingreso al Panel de Administración.*

---

## 2. Conceptos básicos de administración

| Término | Qué significa |
|---|---|
| **Dependencia / Estructura organizacional** | Cada unidad de la institución (gerencia, jefatura, departamento). Se ordenan en forma de árbol: una dependencia puede tener una **dependencia padre**. |
| **Usuario** | Persona que usa el sistema. Tiene un **rol**, una **unidad** y, opcionalmente, una **jefatura** asignada. |
| **Rol** | Permite o limita lo que la persona puede hacer: Funcionario, Jefatura, RRHH o Administrador. |
| **Jerarquía de aprobación** | Regla que define qué aprobador resuelve las boletas de una dependencia. Puede ser **Vertical** (jefe directo) u **Horizontal** (mismo nivel) y tiene un **nivel**. |
| **Delegación de aprobación** | Permiso temporal para que otra persona (el **delegado**) apruebe en nombre de quien delega (el **delegante**), durante un período. Si hay una delegación vigente, **tiene prioridad** sobre la jerarquía. |
| **Estado de registro** | Indica si un dato está **Activo** o **Inactivo**. Desactivar es una forma de "dar de baja" sin borrar. |
| **Auditoría** | Bitácora de lo que ocurre en el sistema: eventos del negocio (por ejemplo, una boleta aprobada) y errores técnicos. |
| **Vigencia (Desde / Hasta)** | Período en que una jerarquía o delegación está en uso. |

---

## 3. Antes de empezar (requisitos)

Para administrar SIFCNP necesita:

- Una **cuenta institucional con rol Administrador** asignado en el sistema.
- Un **navegador de internet actualizado**: **Microsoft Edge, Google Chrome o Mozilla Firefox** en una versión reciente (Chrome o Edge 90 o más nuevo, Firefox 90 o más nuevo), o **Safari 14 o más nuevo** en equipos Apple. **No funciona en Internet Explorer ni en versiones de Microsoft Edge anteriores a 2020.** _Supuesto: el sistema no declara una lista oficial de navegadores; este conjunto se derivó de las funciones modernas que usa el propio código (el piso técnico real es Chrome/Edge 85, Firefox 77 y Safari 13.1)._
- La **dirección del sistema** (donde está publicado SIFCNP).

> **Gobernanza del rol Administrador.** El rol de Administrador **no es de autoasignación**: lo define la **coordinación o jefatura de la Unidad de Tecnologías de Información (UTI)** según disponga. Si necesita este rol, solicítelo por los canales oficiales de la UTI.

> **🔐 Identidad institucional.** El acceso y los permisos se manejan con la **cuenta institucional de Microsoft 365** (dominio `@cnp.go.cr`). El sistema **no administra contraseñas**: la identidad de cada persona se delega a su cuenta institucional. Este es el modelo de identidad previsto; confirme con la Unidad de TI el estado de activación. _(TODO: estado de activación de Microsoft 365.)_

---

## 4. Ingresar al Panel de Administración

1. Ingrese al sistema con su cuenta institucional (ver el [Manual de Usuario Final](../manual-usuario-final/manual-usuario-final.md), sección "Cómo ingresar").
2. Al ser administrador, verá únicamente la sección **Panel Admin**.
3. Dentro del Panel Admin hay **cinco apartados**: **Dependencias**, **Usuarios**, **Jerarquías**, **Delegaciones** y **Registros**.

> **Importante:** cada apartado **no carga datos automáticamente**. Debe aplicar un filtro y hacer clic en **Buscar** para ver la información. Los resultados se muestran en páginas de 15 registros.

![Panel de Administración con los cinco apartados (Dependencias, Usuarios, Jerarquías, Delegaciones, Registros) y la tabla de dependencias.](capturas/02-panel-admin-dependencias.png)

*Figura 2. Panel Admin con sus cinco apartados.*

---

## 5. Gestión de dependencias

**Objetivo:** mantener al día la estructura de la institución.

### 5.1 Consultar dependencias

1. Entre al apartado **Dependencias**.
2. Si lo desea, escriba un texto de búsqueda o elija un estado.
3. Haga clic en **Buscar**.
4. La tabla muestra: **ID**, **Nombre**, **Padre** y **Estado** de cada dependencia.

### 5.2 Editar una dependencia

1. En la fila de la dependencia, haga clic en **Editar**.
2. Modifique el **Nombre** y, si corresponde, el **ID de la dependencia padre**.
3. Haga clic en **Guardar**.

> **Reglas:** el nombre es obligatorio (hasta 150 caracteres) y una dependencia **no puede ser su propia padre** ni formar un ciclo (por ejemplo, A depende de B y B depende de A). El sistema lo valida y avisa si hay error.

![Formulario de edición de una dependencia con el nombre y el identificador de la dependencia padre.](capturas/03-editar-dependencia.png)

*Figura 3. Edición de una dependencia.*

---

## 6. Gestión de usuarios y roles

**Objetivo:** asegurar que cada persona tenga el rol, la unidad y la jefatura correctos.

### 6.1 Consultar usuarios

1. Entre al apartado **Usuarios**.
2. Puede filtrar por rol, unidad, jefatura, estado (activo) o por texto.
3. Haga clic en **Buscar**.
4. La tabla muestra: **ID**, **Nombre**, **Rol**, **Unidad** y si está **Activo**.

### 6.2 Cambiar el rol, la unidad o la jefatura de un usuario

1. En la fila del usuario, haga clic en **Editar asignación**.
2. Ajuste los valores:
   - **ID de Rol** — 1 = Funcionario, 2 = Jefatura, 3 = RRHH, 4 = Administrador.
   - **ID de Unidad** — la dependencia a la que pertenece.
   - **ID de Jefatura** — el usuario que es su jefe directo.
3. Haga clic en **Guardar**.

> **Reglas:** un usuario **no puede ser su propia jefatura**, la jefatura indicada debe existir y estar activa, y no se permiten ciclos directos (A jefe de B y B jefe de A). El sistema valida y avisa.

### 6.3 Activar o desactivar un usuario

En la fila del usuario, use la opción para **activar** o **desactivar**. Un usuario desactivado deja de operar en el sistema, pero su historial se conserva.

![Apartado Usuarios con la tabla de usuarios (ID, Nombre, Rol, Unidad, Activo) y el botón Editar.](capturas/04-panel-usuarios.png)

*Figura 4. Tabla de usuarios.*

![Formulario de edición de un usuario con los campos Rol, Unidad, Jefatura y Activo.](capturas/05-editar-usuario-asignacion.png)

*Figura 5. Edición de rol, unidad y jefatura.*

> **Nota sobre la identidad Microsoft 365.** En el modelo previsto, la identidad de cada persona (quién es y con qué cuenta entra) proviene de **Microsoft 365**. La asignación de **rol, unidad y jefatura** dentro de SIFCNP se seguirá administrando desde este apartado.

---

## 7. Jerarquías de aprobación

Las **jerarquías** definen **quién aprueba las boletas** de cada dependencia. Es la base del flujo de aprobación.

### 7.1 Consultar jerarquías

1. Entre al apartado **Jerarquías** y haga clic en **Buscar**.
2. La tabla muestra: **ID del Aprobador**, **ID de la Estructura**, **Nivel**, **Relación** (Vertical u Horizontal) y **Estado**.

### 7.2 Crear una jerarquía

1. Haga clic en **Crear** (o **Nueva jerarquía**).
2. Complete los campos:
   - **ID del Aprobador** — el usuario que aprobará.
   - **ID de la Estructura** — la dependencia sobre la que aprueba.
   - **Nivel de Aprobación** — número mayor a cero.
   - **Tipo de Relación** — **Vertical** u **Horizontal**.
   - **Vigencia Desde** y **Vigencia Hasta** (la fecha de fin puede quedar vacía).
3. Haga clic en **Guardar**.

> **Reglas:** el aprobador y la estructura deben existir; el nivel debe ser mayor a cero; la relación solo admite "Vertical" u "Horizontal"; la fecha de fin no puede ser anterior a la de inicio.

### 7.3 Eliminar una jerarquía

En la fila, haga clic en **Eliminar**. El sistema pide confirmación: **"¿Eliminar esta jerarquía?"**. Confirme para continuar.

![Apartado Jerarquías con la lista de jerarquías de aprobación.](capturas/06-jerarquias.png)

*Figura 6. Lista de jerarquías.*

![Formulario de creación de una jerarquía de aprobación (aprobador, estructura, nivel, relación y vigencia).](capturas/07-crear-jerarquia.png)

*Figura 7. Formulario de creación de jerarquía.*

---

## 8. Delegaciones de aprobación

Una **delegación** permite que una jefatura ceda temporalmente la aprobación a otra persona (por ejemplo, durante vacaciones).

### 8.1 Consultar delegaciones

1. Entre al apartado **Delegaciones** y haga clic en **Buscar**.
2. La tabla muestra: **ID del Delegante**, **ID del Delegado**, **Desde**, **Hasta** y **Estado**.

### 8.2 Crear una delegación

1. Haga clic en **Crear** (o **Nueva delegación**).
2. Complete los campos:
   - **ID del Delegante** — quién cede la aprobación.
   - **ID del Delegado** — quién recibe la aprobación.
   - **Vigencia Desde** y **Vigencia Hasta**.
3. Haga clic en **Guardar**.

> **Reglas:** el delegante y el delegado deben existir y **ser personas distintas** (no se permite delegarse a uno mismo); la fecha de fin no puede ser anterior a la de inicio.

> **Prioridad:** si una delegación está **vigente**, **manda sobre la jerarquía**. Es decir, durante ese período el delegado aprueba en lugar del aprobador habitual.

### 8.3 Eliminar una delegación

En la fila, haga clic en **Eliminar**. El sistema pide confirmación: **"¿Eliminar esta delegación?"**. Confirme para continuar.

![Apartado Delegaciones con la lista de delegaciones de aprobación.](capturas/08-delegaciones.png)

*Figura 8. Lista de delegaciones.*

![Formulario de creación de una delegación (delegante, delegado y vigencia).](capturas/09-crear-delegacion.png)

*Figura 9. Formulario de creación de delegación.*

---

## 9. Registros: monitoreo y auditoría

El apartado **Registros** reúne, en una sola vista de **solo lectura**, dos tipos de información:

- **Errores** del sistema (problemas técnicos registrados automáticamente).
- **Eventos** de auditoría (acciones del negocio, como crear o resolver una boleta, o cambios hechos por administradores).

### 9.1 Consultar registros

1. Entre al apartado **Registros**.
2. Use los filtros:
   - **Tipo** — Error o Evento.
   - **Texto** — para buscar dentro del mensaje, usuario, categoría, etc.
   - **Desde** y **Hasta** — rango de fechas.
   - **Ordenar por** (fecha, tipo, mensaje, usuario o estado) y **dirección** (ascendente o descendente).
3. Haga clic en **Buscar**.
4. La tabla muestra: **Fecha**, **Tipo**, **Categoría**, **Mensaje**, **Usuario**, **Estado**, **Referencia** y **Origen**.

### 9.2 Descargar los registros

Haga clic en **Descargar** para guardar los registros mostrados en un archivo que puede abrir en Excel.

> **Qué se audita.** El sistema guarda, entre otros, estos eventos: creación de boleta, aprobación, rechazo, alta y cambios de jerarquías y delegaciones, y cambios en la asignación o el estado de los usuarios. Los cambios hechos por administradores se guardan con una "foto" de los valores **antes y después**.

![Apartado Registros con los filtros y la tabla de monitoreo y auditoría (eventos y errores del sistema).](capturas/10-registros-auditoria.png)

*Figura 10. Vista de Registros con filtros.*

> **Acceso a la información de auditoría.** Las personas o instancias que requieran datos de auditoría pero **no tengan rol Administrador** (por ejemplo, auditoría interna o control interno) **no acceden directamente al sistema**: solicitan la información de auditoría a la **Unidad de Tecnologías de Información (UTI)** mediante los canales oficiales.

---

## 10. Configuración del sistema

Esta sección resume la configuración que normalmente coordina con la **Unidad de TI**.

- **Conexión a la base de datos.** El sistema se conecta a la base de datos institucional **INTEGRA_CNP**. La dirección y las credenciales de esa conexión **no se guardan dentro de la aplicación**: se entregan al sistema mediante una **variable del entorno** de la copia en funcionamiento (`ConnectionStrings__IntegraCnp`). Si esa configuración falta en producción, el sistema **no arranca** (es una protección a propósito).
- **Identidad de las personas.** Se delega a **Microsoft 365** (cuentas `@cnp.go.cr`), como se indicó antes.
- **Dirección del sistema.** La copia en funcionamiento se publica en una dirección interna definida por TI. El sistema ofrece una **página de estado** en la ruta `/health`, que responde "ok" cuando está operativo.

> _TODO: confirmar con TI la dirección de producción, la página de estado y las personas responsables de la configuración._

---

## 11. Respaldos

El respaldo de la información es **responsabilidad de la institución** (Unidad de TI / administración de base de datos), no se realiza desde la aplicación.

- Lo que se respalda es la **base de datos INTEGRA_CNP**, que contiene boletas, usuarios, jerarquías, delegaciones y auditoría.
- Se recomienda una rutina de respaldo periódica y verificada, según la política institucional.

> _TODO: documentar la política real de respaldos: frecuencia, herramienta, responsable, ubicación de las copias y procedimiento de restauración._

---

## 12. Mantenimiento

- **Actualizaciones (publicación).** Las nuevas versiones de SIFCNP las prepara el equipo de desarrollo y las **publica** la Unidad de TI en la copia en funcionamiento. El administrador funcional no realiza la publicación.
- **Verificar que el sistema esté operativo.** Puede confirmarse abriendo la **página de estado** (`/health`): si responde "ok", el sistema está en línea.
- **Datos de catálogo.** Los catálogos base (roles, estados, tipos de justificación) se cargan al instalar el sistema. No requieren mantenimiento diario.

> _TODO: confirmar el procedimiento institucional de publicación de versiones y la ventana de mantenimiento._

---

## 13. Solución de problemas y mensajes

| Mensaje en pantalla | Qué significa | Qué hacer |
|---|---|---|
| "Funcionalidad disponible solo para ROL_ADMIN." | La acción requiere rol Administrador. | Verifique que su usuario tenga el rol Administrador. |
| "Debe ingresar un número entero mayor a cero." | Escribió un valor no válido en un campo de ID. | Use un número entero mayor a cero. |
| "El nombre es requerido." | Intentó guardar una dependencia sin nombre. | Escriba el nombre antes de guardar. |
| "Complete todos los campos obligatorios." | Falta algún dato al crear jerarquía o delegación. | Complete todos los campos marcados como obligatorios. |
| "¿Eliminar esta jerarquía?" / "¿Eliminar esta delegación?" | Confirmación antes de borrar. | Acepte para eliminar o cancele para conservar. |
| "Error al cargar dependencias / usuarios / jerarquías / delegaciones." | No se pudo traer la información. | Revise la conexión del sistema; reintente. Si persiste, avise a TI. |
| "Sin resultados para los filtros seleccionados." | No hay registros que cumplan el filtro. | Cambie el filtro y vuelva a buscar. |
| "No hay registros para descargar." | La vista actual no tiene datos. | Realice una búsqueda con resultados antes de descargar. |
| "No se pudo guardar / crear / eliminar …" | La operación falló. | Revise los datos y reintente; el mensaje incluye el detalle del error. |

> **¿A quién acudir?** Ante errores que se repiten, comuníquese con la **Unidad de Tecnologías de Información (UTI) del Consejo Nacional de Producción (CNP)**: abra un ticket en la **mesa de ayuda institucional**, escriba al correo **uti@cnp.go.cr**, o use los **canales internos de Microsoft Teams**.

---

## 14. Seguridad y limitaciones conocidas

- **Identidad delegada a Microsoft 365.** La identidad definitiva será la cuenta institucional `@cnp.go.cr`. Mientras se activa, el ingreso usa una validación provisional; coordine con TI el calendario de activación.
- **Acciones de administrador auditadas.** Todo cambio hecho en el Panel Admin queda registrado en la auditoría con los valores anteriores y nuevos. Use esta bitácora para dar seguimiento.
- **Efecto de las jerarquías y delegaciones.** Recuerde que un cambio aquí **redirige de inmediato** quién aprueba las boletas. Revise la vigencia antes de guardar.
- **Apertura de conexiones (CORS).** En la versión actual, el sistema acepta solicitudes desde cualquier origen. _Recomendación para TI: restringir esto en la publicación expuesta a la red._
- **Tarjetas y descarga de RRHH.** En el panel de RRHH, las tarjetas de resumen muestran valores de ejemplo y la opción "Descargar Reporte" aún no genera archivo (ver Manual de Usuario Final). No es un problema de administración, pero conviene conocerlo.

---

## 15. Glosario

| Término | Definición |
|---|---|
| **Auditoría** | Bitácora de eventos y errores del sistema. |
| **CNP** | Consejo Nacional de Producción. |
| **Delegación de aprobación** | Permiso temporal para que otra persona apruebe en nombre de quien delega. |
| **Dependencia / Estructura organizacional** | Unidad de la institución, ordenada en árbol (con dependencia padre). |
| **Estado de registro** | Activo o Inactivo. |
| **FANAL** | Fábrica Nacional de Licores. |
| **Jerarquía de aprobación** | Regla que define el aprobador de una dependencia (Vertical/Horizontal, con nivel). |
| **Microsoft 365 / Entra** | Servicio institucional de cuentas que provee la identidad de las personas. |
| **Página de estado (`/health`)** | Dirección que indica si el sistema está operativo. |
| **Rol** | Permisos según la función: Funcionario (1), Jefatura (2), RRHH (3), Administrador (4). |
| **Vigencia** | Período (Desde/Hasta) en que aplica una jerarquía o delegación. |

---

## 16. Trazabilidad con las normas

Este manual sigue la estructura prescrita por **ISO/IEC/IEEE 26514:2022** (cláusula 8) y los principios de **IEC/IEEE 82079-1:2019** (cláusulas 5 y 7).

| Sección del manual | Cláusula de la norma | Qué cumple |
|---|---|---|
| Encabezado (producto, versión, público) | 82079-1 §7.2 (Identifiers) | Identificación del producto y del público |
| 1. Qué administra usted | 26514 §8.5 (Conceptual information) | Información conceptual y alcance del rol |
| 2. Conceptos básicos | 26514 §8.5; 82079-1 §7.5 | Terminología de administración |
| 3. Antes de empezar | 82079-1 §7.10.4 (Installation/setup) | Requisitos previos |
| 4. Ingresar al Panel Admin | 26514 §8.6 (Instructional information) | Procedimiento de acceso |
| 5–9. Tareas de administración | 26514 §8.6; 82079-1 §8.3.4 | Información instruccional orientada a tareas |
| 10. Configuración del sistema | 82079-1 §7.10.6; §7.14 (Information security) | Configuración y seguridad de la información |
| 11. Respaldos | 82079-1 §7.10.11 (Maintenance) | Mantenimiento preventivo (respaldo) |
| 12. Mantenimiento | 82079-1 §7.10.10–7.10.11 | Mantenimiento y actualización |
| 13. Solución de problemas y mensajes | 26514 §8.9 y §8.10 | Troubleshooting y mensajes |
| 14. Seguridad y limitaciones | 82079-1 §5.3.1 (Completeness); §7.11 | Completitud e información de seguridad |
| 15. Glosario | 26514 §8.11 (Glossary) | Glosario |

---

## 17. Fuentes

**ISO/IEC/IEEE 26514:2022 — Design and development of information for users**
- https://www.iso.org/standard/77451.html
- https://standards.ieee.org/ieee/26514/7467/
- https://quality.arc42.org/standards/iso-26514

**IEC/IEEE 82079-1:2019 — Preparation of information for use (instructions for use)**
- https://www.iso.org/standard/71620.html
- https://webstore.iec.ch/en/publication/29075
- https://ieeexplore.ieee.org/document/8715838

**WCAG 2.2 — Web Content Accessibility Guidelines** (accesibilidad de la ayuda en línea)
- https://www.w3.org/TR/WCAG22/

---

_Documento generado a partir del código real de SIFCNP (INTEGRA_CNP). Los puntos marcados como **TODO** quedan pendientes de aporte del equipo. Ver también `docs/PROMPT-GENERACION-MANUALES.md`._
