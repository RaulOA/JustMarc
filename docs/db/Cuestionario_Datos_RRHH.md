# Cuestionario de datos para RRHH (dueño del sistema) — INTEGRA_CNP

**Objetivo:** recolectar la información mínima para poblar la base y dejar el sistema
operativo. Pensado para llevárselo al usuario experto de RRHH y llenarlo en una sesión.

**Fecha:** 2026-06-26
**Estado de la BD:** estructura creada (01+02). Faltan catálogos y datos maestros.

---

## 0. Lo que NO hay que preguntar (se carga solo)

5 de los 6 catálogos del sistema tienen valores **fijos definidos por la aplicación**
(backend y frontend). **No requieren decisión de RRHH**; se cargan tal cual con la
Sección A de `03_DatosSemilla.sql`:

| Catálogo | Valores fijos | Origen |
|---|---|---|
| `Rol` | 1 Funcionario, 2 Jefatura, 3 RRHH, 4 Administrador | backend `RolesSistema.cs` |
| `EstadoJustificacion` | 1 Pendiente Jefatura, 2 Aprobada, 3 Rechazada | backend |
| `EstadoRegistro` | 1 Activo, 2 Inactivo | backend |
| `TipoEventoAuditoria` | 1–11 (eventos de auditoría) | backend |
| `ResultadoAuditoria` | 1 Éxito, 2 Fallo, 3 Denegado | backend |

> El **único catálogo** con aporte de RRHH es **`TipoJustificacion`** (ver Bloque 1).

---

## 1. Tipos de justificación  → tabla `Configuracion.TipoJustificacion`

Hoy la aplicación trae **5 tipos fijos** (el frontend los mapea por ID, `app.js`):

| ID | Descripción |
|---|---|
| 1 | Marca Tardía |
| 2 | Omisión Marca de Entrada |
| 3 | Omisión Marca de Salida |
| 4 | Marca antes Hora de Salida |
| 5 | Ausencia |

**Preguntas:**
1. ¿Estos 5 tipos son los correctos y completos para CNP/FANAL?  ☐ Sí  ☐ No
2. Si faltan, ¿cuáles? (nombre exacto): ___________________________________
3. ¿Sobra alguno o cambia de nombre? ___________________________________

> ⚠️ Agregar/cambiar tipos NO es solo BD: el frontend los tiene por ID. Cualquier
> cambio requiere ajustar también `app.js` (coordinación con desarrollo).

---

## 2. Estructura organizacional  → tabla `RecursosHumanos.EstructuraOrganizacional`

Son las **unidades/dependencias**. WIZDOM ya tiene el organigrama
(`WIZDOM.dbo.organigrama`).

**Preguntas:**
1. ¿El alcance es **toda** la organización o un **piloto** de unidades específicas?
   ☐ Todo   ☐ Piloto → ¿cuáles unidades? ____________________________
2. ¿Usamos el **código de nodo de WIZDOM** como código de unidad (`CodigoOrigen`)?
   ☐ Sí (recomendado)   ☐ No → ¿qué código usamos? __________________
3. ¿La jerarquía padre-hijo de unidades se toma de WIZDOM tal cual?  ☐ Sí  ☐ No
4. ¿Hay unidades **inactivas/cerradas** que se deben excluir? ____________________

---

## 3. Empleados  → tabla `RecursosHumanos.Usuario`

WIZDOM ya tiene a todos los empleados (`WIZDOM.dbo.empleado`) y existe un mapeo
definido (`wizdom_empleado_canonical_mapping.md`).

**Pregunta clave (la que más acelera todo):**
1. ¿Cargamos los empleados **automáticamente desde WIZDOM** en vez de a mano?
   ☐ Sí (recomendado)   ☐ No, los entregamos manualmente

Si **manual**, necesitamos por empleado: `Cédula | Nombre completo | Correo | Unidad | Compañía (CNP/FANAL)`.

**Mapeo de identidad (para integrar desde WIZDOM):**
2. ¿La **cédula** del sistema = `numero_identificacion` de WIZDOM?  ☐ Sí  ☐ No: ______
3. ¿La **unidad** del empleado = `codigo_nodo_organigrama` de WIZDOM?  ☐ Sí  ☐ No: ______
4. ¿Qué **estados de empleado** de WIZDOM cuentan como activos? (p. ej. "ACTIVO"): __________
5. ¿Se incluyen ambas compañías (**CNP y FANAL**) o solo una? ____________________

---

## 4. Roles de la aplicación  (campo `RolId` de cada empleado)

WIZDOM **no** sabe quién es "jefatura/RRHH/admin" en ESTA app. RRHH debe definirlo.

**Preguntas:**
1. ¿Quiénes son **Jefaturas** (rol 2) que aprueban boletas? → lista de cédulas/nombres
   o regla (p. ej. "el jefe de cada nodo del organigrama"): ____________________
2. ¿Quiénes de **RRHH** (rol 3) usarán el sistema (consulta global)? ____________________
3. ¿Quién(es) será(n) **Administrador** (rol 4) del sistema? ____________________
4. El resto se asume **Funcionario** (rol 1). ¿Correcto?  ☐ Sí  ☐ No

---

## 5. Reglas de aprobación  → tabla `Operacion.JerarquiaAprobacion`

Define **quién aprueba** las boletas de cada unidad. Es la lógica central del flujo.

**Preguntas:**
1. Regla por defecto: ¿la boleta de un funcionario la aprueba **la jefatura de su unidad**?
   ☐ Sí   ☐ No → ¿cuál es la regla? ____________________
2. ¿La jefatura de una unidad la aprueba **la jefatura superior** (escalamiento vertical)?
   ☐ Sí  ☐ No
3. ¿Existen aprobaciones **horizontales** o por más de un nivel?  ☐ No  ☐ Sí: __________
4. ¿Hay unidades sin jefatura propia que dependan de otra para aprobar? ____________________

---

## 6. Delegaciones (opcional)  → tabla `Operacion.DelegacionAprobacion`

Para cuando una jefatura delega temporalmente (vacaciones, incapacidad).

**Preguntas:**
1. ¿Necesitan delegaciones desde el arranque?  ☐ No (se configuran luego)  ☐ Sí
2. Si sí: `Quién delega | A quién | Desde | Hasta | Motivo`: ____________________

---

## 7. Prioridad (qué bloquea el arranque)

| Dato | ¿Bloquea go-live? | Fuente más rápida |
|---|---|---|
| Catálogos del sistema (Bloque 0) | **Sí** | Automático (Sección A) — sin RRHH |
| `TipoJustificacion` (Bloque 1) | **Sí** (ya hay 5 por defecto) | Confirmar con RRHH |
| Estructura organizacional (Bloque 2) | **Sí** | WIZDOM |
| Empleados (Bloque 3) | **Sí** | WIZDOM |
| Asignación de roles (Bloque 4) | **Sí** | Solo RRHH |
| Jerarquía de aprobación (Bloque 5) | **Sí** | RRHH (derivable de WIZDOM) |
| Delegaciones (Bloque 6) | No | RRHH, después |

---

## 8. Resumen de tablas a poblar

| Tabla | Cómo se llena |
|---|---|
| `Configuracion.*` (6 catálogos) | Script (Sección A) — automático |
| `RecursosHumanos.EstructuraOrganizacional` | Desde WIZDOM organigrama (o manual) |
| `RecursosHumanos.Usuario` | Desde WIZDOM empleado (o manual) + roles de RRHH |
| `Operacion.JerarquiaAprobacion` | Reglas de aprobación de RRHH |
| `Operacion.DelegacionAprobacion` | RRHH, opcional |
