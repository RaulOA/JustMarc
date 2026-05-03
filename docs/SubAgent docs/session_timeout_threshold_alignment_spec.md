# Session Timeout Threshold Alignment Spec

## Hallazgo

La lógica actual no garantiza el contrato exacto "mostrar el modal solo cuando resten 30 segundos".

En [app.js](app.js):

- `SESSION_CONFIG.WARNING_TIME_MS` está definido como `4.5 * 60 * 1000`, pero luego se usa como si representara la ventana final al calcular `warningThresholdMs = INACTIVITY_TIMEOUT_MS - WARNING_TIME_MS`.
- `startSessionExpiryTimer()` revisa el umbral con `setInterval(..., 30 * 1000)`.
- `resetIdleTimer()` actualiza `lastActivity`, pero no reinicia ni reancla ese intervalo principal.

Resultado:

- si la última actividad ocurre fuera de fase con el intervalo de 30 segundos, la condición `timeRemaining <= warningThresholdMs` se evalúa tarde;
- el modal puede aparecer con 29, 18 o 7 segundos restantes, no exactamente con 30.

Ejemplo:

- tick principal en `t=0,30,60,...`;
- actividad del usuario en `t=15`;
- el warning real debería dispararse en `t=285`;
- el siguiente tick ocurre en `t=300`;
- el modal aparece con 15 segundos restantes.

## Causa exacta

El problema no es la aritmética `5:00 - 4:30 = 0:30`, sino mezclar:

- un umbral expresado implícitamente;
- un polling fijo de 30 segundos;
- un `lastActivity` que cambia en cualquier momento sin reiniciar la programación del warning.

## Edición mínima requerida

Mantener `startFinalCountdown()` con intervalo de 1 segundo, pero reemplazar el polling de `startSessionExpiryTimer()` por una programación absoluta del warning basada en `lastActivity`.

### 1. Aclarar la constante de warning en `SESSION_CONFIG`

En vez de una constante que representa `4:30`, definir explícitamente la ventana final:

```js
WARNING_THRESHOLD_MS: 30 * 1000
```

Y usar esa constante directamente en las comparaciones del countdown final.

### 2. Reemplazar el intervalo principal por un `setTimeout`

En `startSessionExpiryTimer()`:

- limpiar timers existentes;
- calcular `timeRemaining = getTimeUntilTimeout()`;
- calcular `warningDelayMs = Math.max(0, timeRemaining - SESSION_CONFIG.WARNING_THRESHOLD_MS)`;
- programar un único `setTimeout` para abrir el modal exactamente cuando se alcance la marca de 30 segundos;
- al disparar ese timeout, mostrar el modal y arrancar `startFinalCountdown()`.

Eso reancla el warning al `lastActivity` real, no al reloj del intervalo.

### 3. Reiniciar la programación del warning cada vez que se resetea la sesión antes del modal

En `resetIdleTimer()`:

- después de `setLastActivity()`, volver a ejecutar `startSessionExpiryTimer()`.

Con eso, cada actividad previa al warning mueve también la hora programada del modal.

### 4. Ajustar `startFinalCountdown()` para usar la nueva constante explícita

Cambiar la condición a:

```js
if (timeRemaining > 0 && timeRemaining <= SESSION_CONFIG.WARNING_THRESHOLD_MS && !sessionWarningVisible) {
  sessionWarningVisible = true;
  showSessionWarningModal();
}
```

## Alcance mínimo

Solo requiere cambios funcionales en [app.js](app.js).

No requiere cambios en HTML, CSS, backend ni almacenamiento.

## Criterio de aceptación

- Sin actividad, el modal aparece al cumplirse exactamente el inicio de los últimos 30 segundos.
- Con actividad en cualquier momento antes del modal, el warning se reprograma y sigue apareciendo exactamente 30 segundos antes del vencimiento.
- El countdown final sigue actualizando cada segundo hasta logout o confirmación.