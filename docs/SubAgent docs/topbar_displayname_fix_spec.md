# Spec: Topbar Display Name Fix

## Objetivo

Evitar que el topbar muestre usernames tecnicos (ej. admin.sofia) en el campo de usuario.
Mostrar nombre completo cuando este disponible y hacer fallback seguro a username.

## Hallazgos

## 1) Ruta actual de render (frontend)

- El login guarda sesion local con campos basicos: isAuth, username, role, company, apiBaseUrl.
  - Archivo: app.js (handleLogin)
- En dashboard, configureRoleUI escribe directamente session.username en el nodo current-user.
  - Archivo: app.js (configureRoleUI)
- El HTML del topbar espera texto en current-user.
  - Archivo: dashboard.html (id="current-user")

Causa raiz: current-user se alimenta de username sin capa de "display name".

## 2) Fuentes de datos disponibles

### Sesion

- Disponible hoy: username, role, company, apiBaseUrl.
- No existe displayName/nombreCompleto en el objeto de sesion.

### API

- /api/session/status solo devuelve validez, userId, role y serverTime.
- No hay endpoint dedicado para perfil del usuario autenticado con nombre completo.
- Existen endpoints que devuelven nombres completos en otros contextos (justificaciones/admin), pero:
  - No son fuente confiable para "mi nombre" en todos los roles.
  - Algunos dependen de permisos o de que existan registros de negocio.

### BD

- La tabla RecursosHumanos.Usuario contiene NombreCompleto y esta relacionada por UsuarioId.
  - Definicion de esquema en docs/db/001_integra_marcas_base_inicial.sql
- Queries existentes ya consumen u.NombreCompleto para listados de negocio/admin.

## 3) Restricciones y riesgo

- No conviene derivar nombre desde endpoints de negocio (historico, pendientes, etc.) porque puede faltar data o romper por permisos.
- No conviene depender de mapeos hardcodeados en frontend para nombres reales.
- El fallback a username debe mantenerse para continuidad operativa si no hay nombre disponible.

## Propuesta de fix minimo y seguro

## A) Backend minimo: endpoint de perfil de sesion (read-only)

Agregar endpoint nuevo:
- GET /api/session/profile

Respuesta sugerida:
- userId
- role
- username (opcional para trazabilidad)
- nombreCompleto (nullable)
- cedula (opcional)

Reglas:
- Resolver identidad desde headers actuales (X-User-Id, X-User-Role).
- Consultar RecursosHumanos.Usuario por UsuarioId.
- Si no existe fila o NombreCompleto vacio, retornar 200 con nombreCompleto null (sin error funcional).
- Sin side effects.

Motivo de seguridad:
- Reusa mecanismo de identidad actual.
- No expone listados masivos ni endpoints admin.
- Mantiene alcance del usuario autenticado.

## B) Frontend minimo: displayName con fallback

Cambios en app.js:

1. Extender sesion con campo opcional displayName.
2. Crear helper:
   - getSessionDisplayName(session)
   - Prioridad: session.displayName (trim, no vacio) -> session.username -> "Usuario"
3. En configureRoleUI:
   - Reemplazar asignacion directa de current-user = session.username
   - Usar current-user = getSessionDisplayName(session)
4. En initDashboardPage:
   - Ejecutar hydrateSessionDisplayName() al cargar.
   - Esta funcion llama GET /api/session/profile.
   - Si llega nombreCompleto valido:
     - actualiza session.displayName
     - persiste con setSession
     - refresca current-user
   - Si falla la API o no hay nombre:
     - no interrumpe flujo
     - mantener username como fallback

## C) Compatibilidad

- No rompe login actual ni resolucion de headers.
- No cambia permisos de negocio existentes.
- Solo agrega un endpoint read-only y un enriquecimiento no bloqueante del topbar.

## Criterios de aceptacion

1. Topbar muestra nombre completo cuando /api/session/profile retorna nombreCompleto valido.
2. Si nombreCompleto es null/vacio o la llamada falla, topbar muestra username.
3. Dashboard no se bloquea por esta llamada.
4. Sin regresiones en endpoints existentes.

## Verificacion recomendada

- Caso A: usuario con NombreCompleto en BD -> topbar muestra nombre completo.
- Caso B: usuario sin NombreCompleto -> topbar muestra username.
- Caso C: API profile no disponible/timeout -> topbar sigue mostrando username.
- Caso D: cambio de usuario entre sesiones -> displayName se actualiza correctamente.
