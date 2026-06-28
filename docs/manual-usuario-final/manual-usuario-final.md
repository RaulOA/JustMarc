# Manual de Usuario Final — SIFCNP

**Sistema de Justificación de Marcas**

| Dato | Valor |
|---|---|
| **Producto** | SIFCNP — Sistema de Justificación de Marcas |
| **Versión del manual** | 1.0.0 |
| **Fecha** | 27 de junio de 2026 |
| **Público objetivo** | Funcionarios, jefaturas y personal de Recursos Humanos (RRHH) del CNP y FANAL |
| **Organización emisora** | _TODO: confirmar unidad responsable (p. ej. Unidad de Tecnologías de Información del CNP)_ |
| **Idioma** | Español (Costa Rica) |

> **¿Para qué sirve este manual?** Para que cualquier persona del CNP o FANAL aprenda, paso a paso, a usar SIFCNP por primera vez: cómo ingresar, cómo llenar una boleta de justificación de marca, cómo aprobarla o rechazarla, y cómo consultar el historial. Está escrito en lenguaje sencillo, sin términos técnicos.

---

## Tabla de contenidos

1. [Qué es SIFCNP](#1-qué-es-sifcnp)
2. [Conceptos básicos](#2-conceptos-básicos)
3. [Antes de empezar (requisitos)](#3-antes-de-empezar-requisitos)
4. [Cómo ingresar y salir del sistema](#4-cómo-ingresar-y-salir-del-sistema)
5. [Tareas del Funcionario](#5-tareas-del-funcionario)
6. [Tareas de la Jefatura](#6-tareas-de-la-jefatura)
7. [Tareas de Recursos Humanos (RRHH)](#7-tareas-de-recursos-humanos-rrhh)
8. [Información de referencia](#8-información-de-referencia)
9. [Solución de problemas y mensajes](#9-solución-de-problemas-y-mensajes)
10. [Limitaciones conocidas](#10-limitaciones-conocidas)
11. [Accesibilidad y ayuda](#11-accesibilidad-y-ayuda)
12. [Glosario](#12-glosario)
13. [Trazabilidad con las normas](#13-trazabilidad-con-las-normas)
14. [Fuentes](#14-fuentes)

---

## 1. Qué es SIFCNP

SIFCNP es el sistema en línea para **justificar marcas de asistencia** en el CNP y FANAL. Una "marca" es el registro de entrada o salida de la jornada. Cuando una marca falta, llega tarde o tiene algún problema, se llena una **boleta de justificación** para explicar lo ocurrido.

El sistema acompaña todo el recorrido de esa boleta:

1. El **funcionario** llena la boleta y la envía.
2. La **jefatura** la revisa y la **aprueba** o la **rechaza**.
3. **Recursos Humanos** puede consultar todas las boletas para dar seguimiento.

Cada persona ve solo lo que le corresponde según su rol.

> 📷 _Captura pendiente:_ `capturas/01-pantalla-ingreso.png` — Pantalla de ingreso de SIFCNP. **(TODO: generar con Playwright)**

---

## 2. Conceptos básicos

Antes de usar el sistema, conviene tener claros estos términos:

| Término | Qué significa |
|---|---|
| **Marca** | Registro de entrada o salida de la jornada laboral. |
| **Boleta de justificación** | Solicitud que explica una o varias marcas con problema. El sistema le pone un código tipo **JM-1234**. |
| **Motivo general** | Explicación principal de la boleta (por qué la presentás). |
| **Línea de detalle** | Cada marca específica que estás justificando, con su tipo y su fecha. Una boleta puede tener varias líneas. |
| **Estado** | Situación de la boleta: **Pendiente Jefatura**, **Aprobado** o **Rechazado**. |
| **Aprobador actual** | La jefatura que en este momento resolvería tu boleta. |
| **Compañía** | La institución a la que pertenece la persona: **CNP** o **FANAL**. |

---

## 3. Antes de empezar (requisitos)

Para usar SIFCNP necesitás:

- Una **computadora con conexión a la red institucional**.
- Un **navegador de internet actualizado** (por ejemplo, Microsoft Edge, Google Chrome o Mozilla Firefox en sus versiones recientes). _TODO: confirmar la lista oficial de navegadores soportados._
- Tu **cuenta institucional** (la misma de tu correo con dominio `@cnp.go.cr`).
- La **dirección del sistema**, que te indica la institución (por ejemplo, la dirección interna donde está publicado SIFCNP).

> **🔐 Identidad institucional.** El control de quién entra y con qué permisos se maneja con tu **cuenta institucional de Microsoft 365** (la misma de tu correo `@cnp.go.cr`). El sistema **no guarda ni administra contraseñas**: eso queda del lado de tu cuenta institucional.
>
> _Nota:_ la integración con Microsoft 365 es el modelo de identidad previsto para SIFCNP. Consultá con la Unidad de TI el estado de activación en tu entorno. _(TODO: confirmar fecha/estado de activación de Microsoft 365.)_

---

## 4. Cómo ingresar y salir del sistema

### 4.1 Ingresar

1. Abrí el navegador y entrá a la **dirección del sistema** que te dio la institución.
2. En el campo **Usuario**, escribí tu usuario institucional.
3. En el campo **Contraseña**, escribí tu contraseña institucional.
4. Hacé clic en **Ingresar** (o presioná la tecla **Enter**).

Si el usuario o la contraseña no cumplen el mínimo requerido, el sistema muestra el aviso **"Credenciales no válidas. Verifique usuario y contraseña."** y la tarjeta de ingreso se mueve para indicar el error. Revisá lo que escribiste e intentá de nuevo.

Al ingresar correctamente, llegás al **panel principal**. Arriba a la derecha vas a ver tu **nombre**, tu **rol** y, si corresponde, tu **Aprobador actual**.

> 📷 _Captura pendiente:_ `capturas/01-pantalla-ingreso.png` — Campos Usuario, Contraseña y botón Ingresar. **(TODO)**

### 4.2 Salir

Hacé clic en **Cerrar sesión**, arriba a la derecha. El sistema te devuelve a la pantalla de ingreso.

### 4.3 Aviso de sesión por inactividad

Por seguridad, si dejás el sistema **sin actividad durante 5 minutos**, aparece una ventana de aviso: **"Sesión por Expirar — Está inactivo. Su sesión se cerrará pronto."**, con una cuenta regresiva.

- Para seguir trabajando, hacé clic en **Permanecer Conectado**.
- Para salir de una vez, hacé clic en **Cerrar Sesión**.

Si no respondés a tiempo, el sistema cierra la sesión solo y muestra: **"Sesión expirada por inactividad (5 minutos). Por favor, inicie sesión nuevamente."**

> 📷 _Captura pendiente:_ `capturas/10-aviso-sesion.png` — Ventana de aviso de sesión por expirar. **(TODO)**

---

## 5. Tareas del Funcionario

Como **funcionario**, podés: crear boletas de justificación, ver tu propio historial y consultar el histórico de SIFCNP. Tenés dos secciones disponibles: **Panel Funcionario** y **Consulta Histórica (SIFCNP)**.

### 5.1 Crear una boleta de justificación

**Objetivo:** registrar una o varias marcas con problema para que tu jefatura las revise.

1. Entrá a la sección **Panel Funcionario**.
2. En **Motivo General**, escribí la explicación principal de la boleta. Es **obligatorio** y admite hasta **500 caracteres**.
3. Agregá una **línea de detalle** por cada marca que querés justificar:
   1. En **Tipo de Justificación**, elegí una de las opciones (ver la lista en la [sección 8](#8-información-de-referencia)).
   2. En **Fecha de Marca**, seleccioná el día de la marca.
   3. En **Observación del Detalle**, escribí un comentario si querés (es opcional, hasta **250 caracteres**).
   4. Hacé clic en **Agregar Línea**. La línea aparece en la tabla de abajo y el contador aumenta.
   - Podés repetir este paso para agregar **varias líneas**. Si te equivocaste en una, podés quitarla de la tabla.
4. Cuando tengas el motivo y **al menos una línea**, hacé clic en **Registrar Justificación**.

Al guardarse, el sistema muestra **"Boleta registrada en estado Pendiente Jefatura."** y limpia el formulario. La boleta queda esperando que tu jefatura la resuelva.

> **Tené en cuenta:** una boleta necesita el **motivo general** y **al menos una línea de detalle**. Cada línea necesita su **tipo** y su **fecha**. Si falta algo, el sistema te lo avisa (ver [sección 9](#9-solución-de-problemas-y-mensajes)).

> 📷 _Captura pendiente:_ `capturas/02-panel-funcionario-formulario.png` — Formulario "Nueva Justificación". **(TODO)**
> 📷 _Captura pendiente:_ `capturas/03-agregar-linea-detalle.png` — Tabla de líneas de detalle agregadas. **(TODO)**

### 5.2 Ver mi historial de justificaciones

1. En el **Panel Funcionario**, bajá hasta **Mi Historial de Justificaciones**.
2. Vas a ver tus boletas con su motivo, estado y fecha.
3. Para ver el detalle de una boleta (sus líneas), hacé clic para **expandirla**.
4. Si tenés muchas boletas, usá **Cargar 10 más** para mostrar más registros.

Cuando una boleta ya fue **Aprobada** o **Rechazada**, el historial puede mostrar el **comentario de resolución** de la jefatura (si lo hubo).

> 📷 _Captura pendiente:_ `capturas/04-mi-historial.png` — Tabla "Mi Historial de Justificaciones". **(TODO)**

### 5.3 Consulta Histórica (SIFCNP)

Esta sección muestra el histórico de marcas justificadas.

1. Entrá a **Consulta Histórica (SIFCNP)**.
2. Elegí un rango de fechas con **Fecha Desde** y **Fecha Hasta**.
3. Usá la **búsqueda** para filtrar lo que se muestra en pantalla.
4. Para guardar el resultado, hacé clic en **Descargar Reporte**: se genera un archivo que podés abrir en Excel.

> **Nota para funcionarios:** en esta sección **solo vas a ver tus propios registros**. El sistema muestra el aviso "Mostrando solo sus registros históricos." y oculta el campo para buscar por otra persona.

> 📷 _Captura pendiente:_ `capturas/09-consulta-historica-sifcnp.png` — Consulta Histórica con filtros de fecha. **(TODO)**

---

## 6. Tareas de la Jefatura

Como **jefatura**, además de **crear tus propias boletas** (igual que un funcionario, ver [sección 5](#5-tareas-del-funcionario)), tu tarea principal es **revisar y resolver** las boletas de las personas a tu cargo. Tenés disponibles: **Panel Funcionario**, **Panel Jefatura** y **Consulta Histórica (SIFCNP)**.

### 6.1 Ver las solicitudes pendientes

1. Entrá a la sección **Panel Jefatura**.
2. Vas a ver la tabla **Solicitudes Pendientes** y, arriba, un contador con cuántas hay (por ejemplo, "3 pendientes").
3. Podés **ordenar** la tabla haciendo clic en los títulos de las columnas (funcionario, motivo, tipo, fecha, estado).
4. Si hay muchas, la lista se divide en **páginas** (15 por página).

> 📷 _Captura pendiente:_ `capturas/05-panel-jefatura-pendientes.png` — Tabla "Solicitudes Pendientes". **(TODO)**

### 6.2 Ver el detalle de una boleta

Antes de decidir, conviene revisar el detalle:

1. En la fila de la boleta, hacé clic en **Ver detalle ▼**.
2. Se despliega la información: quién la presentó, su unidad, el tipo principal y el resumen de las líneas.

> 📷 _Captura pendiente:_ `capturas/07-detalle-boleta.png` — Detalle desplegado de una boleta. **(TODO)**

### 6.3 Aprobar o rechazar una boleta

**Objetivo:** resolver la boleta para que el funcionario sepa el resultado.

1. En la fila de la boleta, hacé clic en **Aprobar** o en **Rechazar**.
2. El sistema confirma con un aviso, por ejemplo: **"Boleta JM-1234 aprobada."** o **"Boleta JM-1234 rechazada."**
3. La boleta sale de la lista de pendientes y queda con su nuevo estado.

> **Tené en cuenta:** una boleta solo se puede resolver **una vez**. Si ya fue aprobada o rechazada, no se puede volver a cambiar.

> 📷 _Captura pendiente:_ `capturas/06-aprobar-rechazar.png` — Botones Aprobar y Rechazar en una fila. **(TODO)**

### 6.4 Descargar el reporte de pendientes

En el **Panel Jefatura**, hacé clic en **Descargar Reporte** para guardar la lista de boletas pendientes en un archivo que podés abrir en Excel.

---

## 7. Tareas de Recursos Humanos (RRHH)

Como personal de **RRHH**, tu función es **consultar y dar seguimiento** a todas las boletas de la institución. Tenés disponibles: **Panel Funcionario** (solo lectura del formulario), **Panel RRHH** y **Consulta Histórica (SIFCNP)**.

### 7.1 Consultar todas las boletas

1. Entrá a la sección **Panel RRHH**.
2. Arriba vas a ver cuatro tarjetas de resumen (Total, Pendientes, Aprobadas, Rechazadas). _Ver la nota en [Limitaciones conocidas](#10-limitaciones-conocidas)._
3. Usá la barra de **filtros** para acotar la búsqueda:
   - **Funcionario** (nombre o parte del nombre)
   - **Estado** (Pendiente, Aprobado o Rechazado)
   - **Compañía** (CNP o FANAL)
   - **Desde** y **Hasta** (rango de fechas)
4. Hacé clic en **Aplicar** para ver el resultado, o en **Limpiar** para borrar los filtros.

La tabla muestra: funcionario, compañía, motivo, tipo, fecha, estado y resolución.

> 📷 _Captura pendiente:_ `capturas/08-panel-rrhh-filtros.png` — Panel RRHH con la barra de filtros. **(TODO)**

### 7.2 Consulta Histórica (SIFCNP)

Funciona igual que para el funcionario (ver [sección 5.3](#53-consulta-histórica-sifcnp)), pero RRHH **sí puede buscar por funcionario**: escribí el nombre en el campo correspondiente, elegí el rango de fechas y descargá el reporte.

---

## 8. Información de referencia

### 8.1 Tipos de justificación

Al crear una línea de detalle, elegís uno de estos tipos:

| Tipo | Cuándo se usa |
|---|---|
| **Marca Tardía** | Llegaste y marcaste después de la hora. |
| **Omisión Marca de Entrada** | No quedó registrada tu marca de entrada. |
| **Omisión Marca de Salida** | No quedó registrada tu marca de salida. |
| **Marca antes Hora de Salida** | Marcaste la salida antes de la hora correspondiente. |
| **Ausencia** | No te presentaste a la jornada. |

### 8.2 Estados de una boleta

| Estado | Qué significa |
|---|---|
| **Pendiente Jefatura** | Recién creada; espera la decisión de la jefatura. |
| **Aprobado** | La jefatura aceptó la justificación. |
| **Rechazado** | La jefatura no aceptó la justificación. |

### 8.3 Campos de la boleta

| Campo | ¿Obligatorio? | Límite |
|---|---|---|
| Motivo General | Sí | 500 caracteres |
| Tipo de Justificación (por línea) | Sí | — |
| Fecha de Marca (por línea) | Sí | — |
| Observación del Detalle (por línea) | No | 250 caracteres |

---

## 9. Solución de problemas y mensajes

Esta tabla reúne los avisos más comunes, qué significan y qué hacer.

| Mensaje en pantalla | Qué significa | Qué hacer |
|---|---|---|
| "Credenciales no válidas. Verifique usuario y contraseña." | El usuario o la contraseña no cumplen el mínimo. | Revisá lo que escribiste y volvé a intentar. |
| "El motivo general es obligatorio." | Intentaste registrar sin escribir el motivo. | Escribí el motivo general antes de registrar. |
| "Debe agregar al menos una línea de detalle." | La boleta no tiene ninguna línea. | Agregá al menos una línea (tipo + fecha). |
| "Cada detalle requiere tipo de justificación y fecha de marca." | Intentaste agregar una línea sin tipo o sin fecha. | Completá el tipo y la fecha de esa línea. |
| "Boleta registrada en estado Pendiente Jefatura." | La boleta se guardó correctamente. | No hay que hacer nada; queda esperando a la jefatura. |
| "Boleta JM-#### aprobada." / "Boleta JM-#### rechazada." | La jefatura resolvió la boleta. | Confirmación; el estado ya cambió. |
| "No hay solicitudes pendientes." | No tenés boletas por revisar (jefatura). | Nada por hacer. |
| "No hay boletas registradas." | Todavía no tenés boletas en tu historial. | Creá una boleta si lo necesitás. |
| "Sesión expirada por inactividad (5 minutos). Por favor, inicie sesión nuevamente." | Pasaron 5 minutos sin actividad. | Volvé a ingresar. |
| "La API tardó demasiado en responder. Intente de nuevo." | El sistema no respondió a tiempo. | Esperá un momento y reintentá; si sigue, avisá a soporte. |
| "No fue posible conectar con la API. Verifique backend, URL y CORS." | No hay conexión con el servicio del sistema. | Revisá tu conexión; si persiste, avisá a la Unidad de TI. |

> **¿A quién acudir?** Si un problema persiste, comunicate con la mesa de ayuda institucional. _TODO: completar el contacto de soporte (correo / teléfono / unidad)._

---

## 10. Limitaciones conocidas

Para que sepas qué esperar en esta versión 1.0.0:

- **Tarjetas de resumen de RRHH:** los números de las cuatro tarjetas (Total, Pendientes, Aprobadas, Rechazadas) muestran **valores de ejemplo fijos** y **no reflejan todavía los datos reales**. Para cifras reales, usá la tabla y los filtros.
- **"Descargar Reporte" del Panel RRHH:** por ahora solo muestra un aviso con el nombre del archivo, pero **no genera el archivo**. Si necesitás descargar datos reales, usá **Descargar Reporte** desde la **Consulta Histórica (SIFCNP)** o desde el **Panel Jefatura**, que sí generan archivo.
- **Comentario al aprobar o rechazar:** en esta versión la jefatura resuelve la boleta **sin un campo para escribir un comentario**; la boleta se aprueba o rechaza sin nota adicional.
- **Ingreso provisional:** mientras se activa la cuenta institucional Microsoft 365, la validación de la contraseña en la pantalla de ingreso es **provisional**. La identidad definitiva será tu cuenta institucional `@cnp.go.cr`.

---

## 11. Accesibilidad y ayuda

SIFCNP busca ser fácil de usar para todas las personas. Para eso:

- La **forma de pedir ayuda** y de cerrar sesión está siempre en el **mismo lugar** (arriba a la derecha), en todas las pantallas.
- Las secciones tienen **títulos claros** y las tablas usan encabezados, para que sea fácil ubicarte.
- El sistema está en **español**.
- Si usás teclado, podés **ingresar con la tecla Enter** desde la pantalla de inicio.

> Si tenés alguna dificultad de accesibilidad (por ejemplo, contraste o tamaño de texto), avisá a la mesa de ayuda. _TODO: confirmar canal de soporte de accesibilidad._

---

## 12. Glosario

| Término | Definición |
|---|---|
| **Aprobador actual** | Jefatura que en este momento resolvería tu boleta. |
| **Boleta de justificación** | Solicitud que explica una o varias marcas con problema. Se identifica con un código tipo JM-1234. |
| **CNP** | Consejo Nacional de Producción. |
| **Compañía** | Institución de la persona: CNP o FANAL. |
| **Consulta Histórica (SIFCNP)** | Sección para consultar el histórico de marcas justificadas. |
| **Estado** | Situación de la boleta: Pendiente Jefatura, Aprobado o Rechazado. |
| **FANAL** | Fábrica Nacional de Licores. |
| **Jefatura** | Rol que aprueba o rechaza boletas. |
| **Línea de detalle** | Cada marca específica justificada dentro de una boleta (tipo + fecha + observación). |
| **Marca** | Registro de entrada o salida de la jornada. |
| **Motivo general** | Explicación principal de la boleta. |
| **RRHH** | Recursos Humanos. |
| **Rol** | Conjunto de permisos según tu función: Funcionario, Jefatura, RRHH o Administrador. |
| **SIFCNP** | Sistema de Justificación de Marcas. |

---

## 13. Trazabilidad con las normas

Este manual sigue la estructura prescrita por **ISO/IEC/IEEE 26514:2022** (cláusula 8, estructura de la información para usuarios) y los principios de **IEC/IEEE 82079-1:2019** (cláusulas 5 y 7), con criterios de accesibilidad de **WCAG 2.2**.

| Sección del manual | Cláusula de la norma | Qué cumple |
|---|---|---|
| Encabezado (producto, versión, público) | 82079-1 §7.2 (Identifiers) | Identificación de la información de uso, del producto y del público |
| 1. Qué es SIFCNP | 26514 §8.5 (Conceptual information) | Información conceptual y de alcance |
| 2. Conceptos básicos | 26514 §8.5; 82079-1 §7.5–7.6 | Terminología y conceptos |
| 3. Antes de empezar | 82079-1 §7.8 / §7.10.4 | Descripción y requisitos previos al uso |
| 4. Ingresar y salir | 26514 §8.6 (Instructional information) | Procedimientos paso a paso |
| 5–7. Tareas por rol | 26514 §8.6; 82079-1 §8.3.4 | Información instruccional orientada a tareas |
| 8. Información de referencia | 26514 §8.7 (Reference information) | Referencia de tipos, estados y campos |
| 9. Solución de problemas y mensajes | 26514 §8.9 y §8.10 | Troubleshooting y mensajes de error (causa/acción) |
| 10. Limitaciones conocidas | 82079-1 §5.3.1 (Completeness) | Completitud e información honesta del estado |
| 11. Accesibilidad y ayuda | WCAG 2.2 §3.2.6, §2.4.6, §3.1.1 | Ayuda consistente, encabezados, idioma |
| 12. Glosario | 26514 §8.11 (Glossary of terms) | Glosario |
| Tabla de contenidos | 26514 §9.10.5 | Navegación |

---

## 14. Fuentes

Normas y especificaciones consultadas (estructura y secciones obligatorias, a partir de índices y guías públicas; el texto íntegro de las normas ISO/IEEE es de pago):

**ISO/IEC/IEEE 26514:2022 — Design and development of information for users**
- https://www.iso.org/standard/77451.html
- https://standards.ieee.org/ieee/26514/7467/
- https://quality.arc42.org/standards/iso-26514
- https://www.kothes.com/en/blog/the-new-iso/iec/ieee-265142022-01

**IEC/IEEE 82079-1:2019 — Preparation of information for use (instructions for use)**
- https://www.iso.org/standard/71620.html
- https://webstore.iec.ch/en/publication/29075
- https://ieeexplore.ieee.org/document/8715838
- https://instrktiv.com/en/82079/

**WCAG 2.2 — Web Content Accessibility Guidelines**
- https://www.w3.org/TR/WCAG22/
- https://www.w3.org/WAI/standards-guidelines/wcag/
- https://www.w3.org/WAI/WCAG22/Understanding/consistent-help.html

---

_Documento generado a partir del código real de SIFCNP (INTEGRA_CNP). Las funcionalidades descritas corresponden a lo implementado; los puntos marcados como **TODO** quedan pendientes de aporte del equipo. Ver también `docs/PROMPT-GENERACION-MANUALES.md` para los criterios de elaboración._
