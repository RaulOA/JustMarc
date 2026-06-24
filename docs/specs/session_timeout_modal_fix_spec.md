# Session Timeout Warning Modal Fix Spec

## Objetivo

Corregir el flujo actual del timeout por inactividad para que cumpla exactamente estas reglas:

1. El modal de advertencia aparece solo cuando faltan 30 segundos.
2. Una vez visible, la actividad del usuario no debe resetear el temporizador ni cerrar el modal.
3. El reset solo ocurre al hacer clic en "Permanecer conectado".
4. La actividad del usuario puede resetear el timeout solo antes de que aparezca la advertencia.

## Estado actual

### app.js

- `SESSION_CONFIG.INACTIVITY_TIMEOUT_MS` es 5 minutos y `FINAL_CHECK_INTERVAL_MS` es 1 segundo.
- `initIdleMonitor()` registra `mousemove`, `keypress`, `click`, `touchstart` y `scroll` para llamar siempre a `resetIdleTimer()`.
- `resetIdleTimer()` actualmente:
  - actualiza `sjm_lastActivity` con `setLastActivity()`;
  - oculta el modal con `hideSessionWarningModal()`.
- `startSessionExpiryTimer()` usa un intervalo de 30 segundos y, cuando quedan 30 segundos o menos, corta ese intervalo y llama a `startFinalCountdown()`.
- `startFinalCountdown()` corre cada segundo, actualiza el contador y vuelve a mostrar el modal si está oculto y todavía queda cualquier tiempo positivo.
- `handleStayLoggedIn()` oculta el modal, llama `resetIdleTimer()` y luego vuelve a ejecutar `startSessionExpiryTimer()`.

### dashboard.html

- El modal existe y tiene los controles correctos:
  - botón `Permanecer Conectado` -> `handleStayLoggedIn()`;
  - botón `Cerrar Sesión` -> `handleLogoutNow()`.

## Causa del problema

Hay dos errores de lógica que juntos producen el comportamiento observado.

### 1. La actividad normal sigue reseteando la sesión cuando el modal ya está visible

Ubicación: `resetIdleTimer()` en `app.js`.

- Cualquier movimiento de cursor, scroll, click o tecla sigue ejecutando `setLastActivity()`.
- Eso viola la regla requerida: después de mostrar la advertencia, la sesión ya no debe extenderse automáticamente.
- Además, `resetIdleTimer()` oculta el modal aunque no se haya presionado `Permanecer conectado`.

Resultado:

- mover el cursor mientras el modal está activo cambia la hora de última actividad;
- el modal desaparece sin confirmación explícita;
- la expiración deja de reflejar el estado de advertencia real.

### 2. El countdown final vuelve a mostrar el modal para cualquier tiempo restante positivo

Ubicación: `startFinalCountdown()` en `app.js`.

Condición actual:

```js
if (timeRemaining > 0 && !document.getElementById('session-warning-modal')?.classList.contains('active')) {
  showSessionWarningModal();
}
```

Problema:

- si una actividad cualquiera oculta el modal y además resetea `lastActivity`, `timeRemaining` vuelve a estar cerca de 5 minutos;
- el `finalInterval` sigue vivo;
- como la condición solo exige `timeRemaining > 0`, el modal reaparece aunque ya no falten 30 segundos.

Resultado:

- el modal se vuelve a abrir o parece "reiniciarse" por movimiento de cursor;
- el contador puede reaparecer con un valor incorrecto para una advertencia final.

### 3. Al confirmar "Permanecer conectado" quedan timers previos activos

Ubicación: `handleStayLoggedIn()` y timers creados en `startSessionExpiryTimer()` / `startFinalCountdown()`.

- `handleStayLoggedIn()` inicia un nuevo ciclo con `startSessionExpiryTimer()`.
- Pero el `finalInterval` activo no se almacena ni se limpia antes de reiniciar.

Riesgo:

- múltiples intervalos concurrentes;
- reapertura inesperada del modal;
- logout duplicado o checks en paralelo.

## Cambio mínimo y seguro propuesto

No rediseñar el sistema. Mantener `sessionStorage`, el modal existente y la estructura general, pero introducir estado explícito para distinguir:

- fase normal: actividad sí resetea;
- fase de advertencia: actividad no resetea;
- confirmación explícita: sí resetea y reinicia timers limpiamente.

## Cambios concretos

### 1. Agregar estado global del timeout

En `app.js`, agregar variables de control a nivel módulo, por ejemplo:

```js
let sessionWarningVisible = false;
let sessionMainIntervalId = null;
let sessionFinalIntervalId = null;
let sessionLogoutTriggered = false;
```

Propósito:

- saber si la advertencia ya entró en fase bloqueante;
- evitar intervalos duplicados;
- centralizar cleanup al reiniciar o cerrar sesión.

### 2. Hacer que `resetIdleTimer()` ignore actividad cuando el warning está visible

Cambiar `resetIdleTimer()` para que:

- si no hay sesión autenticada, no haga nada;
- si `sessionWarningVisible === true`, no actualice `lastActivity` y no oculte el modal;
- solo en fase previa al warning haga `setLastActivity()`.

Regla resultante:

- antes del warning, la actividad extiende la sesión;
- después del warning, la actividad normal no cambia nada.

### 3. Separar "ocultar modal" de "resetear sesión"

No usar `resetIdleTimer()` como side effect del botón de permanencia.

Crear una función explícita, por ejemplo `resetSessionTimeoutFromWarningConfirmation()`, que:

- limpie timers activos;
- ponga `sessionWarningVisible = false`;
- actualice `lastActivity`;
- oculte el modal;
- reinicie `sessionLogoutTriggered` si aplica;
- arranque un único `startSessionExpiryTimer()` nuevo.

Esto evita que el reset dependa de eventos genéricos del usuario.

### 4. Corregir `startSessionExpiryTimer()` para mantener una sola instancia

Antes de iniciar un nuevo intervalo principal:

- limpiar `sessionMainIntervalId` y `sessionFinalIntervalId` si existen;
- reiniciar flags solo cuando corresponde al comienzo de un nuevo ciclo.

Además:

- cuando el tiempo restante llegue a 30 segundos o menos, mostrar el modal una sola vez;
- marcar `sessionWarningVisible = true` antes de transferir el control al countdown final.

### 5. Corregir `startFinalCountdown()` para operar solo dentro de la ventana final de 30 segundos

Actualizar la lógica para que:

- solo muestre o mantenga el modal si `timeRemaining > 0 && timeRemaining <= 30 * 1000`;
- si el warning ya está visible, solo actualice el contador;
- no vuelva a abrir el modal fuera de esa ventana.

Esto elimina el re-open incorrecto tras cualquier reset o estado desincronizado.

### 6. Hacer que `showSessionWarningModal()` y `hideSessionWarningModal()` sincronicen estado

`showSessionWarningModal()`:

- establecer `sessionWarningVisible = true`;
- asegurar que el countdown inicial se renderice con segundos restantes reales.

`hideSessionWarningModal()`:

- remover la clase `active`;
- no resetear `lastActivity` por sí sola;
- poner `sessionWarningVisible = false` solo cuando el cierre sea intencional y controlado.

Nota:

- Si se prefiere evitar ambigüedad, el flag puede no tocarse en `hideSessionWarningModal()` y manejarse solo desde funciones de transición de estado. Eso es más seguro que dejar el modal visual y el estado lógico desacoplados.

### 7. Actualizar `handleStayLoggedIn()` para que sea el único punto de reset permitido

Reemplazar la lógica actual por un flujo explícito:

```js
function handleStayLoggedIn() {
  resetSessionTimeoutFromWarningConfirmation();
}
```

Sin llamar `resetIdleTimer()` directamente.

### 8. Limpiar timers en `handleLogout()`

Antes de limpiar `sessionStorage` y navegar:

- cancelar `sessionMainIntervalId` y `sessionFinalIntervalId`;
- resetear flags del warning;
- ocultar modal si sigue visible.

Esto evita callbacks tardíos sobre una sesión ya cerrada.

## Criterio de cambio mínimo

No es necesario:

- cambiar el HTML del modal;
- cambiar CSS;
- introducir backend ni nuevas llaves persistidas;
- rediseñar la detección de actividad.

Sí es necesario:

- bloquear el reset automático durante la fase de advertencia;
- limpiar timers correctamente;
- restringir la reapertura del modal a la ventana final real de 30 segundos.

## Archivos a cambiar

- `app.js`

No se requieren cambios funcionales en `dashboard.html` para esta corrección.

## Aceptación exacta

### Regla 1

- La sesión inicia.
- El usuario no hace actividad.
- El modal no aparece antes de los últimos 30 segundos.
- El modal aparece cuando quedan 30 segundos o menos.

### Regla 2

- Con el modal visible, mover el cursor, hacer scroll, presionar teclas o hacer click fuera de `Permanecer conectado` no cambia el countdown.
- El modal no se oculta.

### Regla 3

- Al hacer clic en `Permanecer conectado`, el modal se oculta, `lastActivity` se actualiza y el ciclo vuelve a empezar desde 5 minutos.

### Regla 4

- Antes de que aparezca el modal, cualquier actividad monitoreada sigue extendiendo la sesión normalmente.

## Riesgos a vigilar durante implementación

- No dejar intervalos huérfanos al rearmar el timeout.
- No disparar `handleLogout()` más de una vez.
- No permitir que clicks dentro del modal activen listeners globales y cambien estado fuera del flujo explícito.

## Resumen operativo

La reaparición/reset del modal en movimiento de cursor ocurre porque la actividad global sigue llamando `resetIdleTimer()` cuando el warning ya está activo, y el countdown final vuelve a mostrar el modal con una condición demasiado amplia (`timeRemaining > 0`). La corrección mínima consiste en introducir estado explícito de warning, bloquear resets automáticos durante esa fase, limpiar intervalos al confirmar permanencia y permitir reset solo desde `handleStayLoggedIn()`.