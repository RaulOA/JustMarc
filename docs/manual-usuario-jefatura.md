# Manual de Usuario - Rol Jefatura

## 1. Objetivo
Guiar a la jefatura en la revision y resolucion de boletas de subordinados directos.

## 2. Alcance
Este manual cubre:
- Consulta de bandeja de pendientes.
- Revision de detalle de boleta.
- Resolucion (aprobar o rechazar).

No cubre:
- Creacion de boletas (rol Funcionario).
- Consulta global RRHH de toda la organizacion.

## 3. Prerequisitos
- Acceso a dashboard.html con rol de jefatura.
- API en funcionamiento.
- Relacion de subordinacion correctamente registrada en base de datos.

## 4. Acceso y vista de pendientes
1. Ingrese al sistema con usuario de jefatura (ejemplo de prueba: jefe.maria).
2. Abra Panel Jefatura.
3. Revise contador de pendientes y tabla de solicitudes.

## 5. Flujo principal: revisar y resolver boletas
### 5.1 Revisar bandeja
1. Identifique la fila por funcionario, motivo, tipo principal y fecha.
2. Use Ver detalle para ampliar informacion de la boleta.

### 5.2 Revisar detalle completo
1. Presione Ver detalle.
2. El sistema consulta el detalle completo por boleta.
3. Revise lineas, tipo de justificacion y fechas asociadas.

### 5.3 Aprobar boleta
1. Presione Aprobar en la fila correspondiente.
2. Confirme notificacion de exito.
3. Resultado esperado:
   - La boleta sale de pendientes.
   - Su estado pasa a Aprobado.

### 5.4 Rechazar boleta
1. Presione Rechazar.
2. Confirme notificacion.
3. Resultado esperado:
   - La boleta sale de pendientes.
   - Su estado pasa a Rechazado.

Nota operativa:
- En la UI actual, la accion de resolver envia comentario vacio por defecto.

## 6. Reglas de negocio aplicables al rol
- Solo Jefatura puede ver pendientes y detalle de aprobacion.
- Solo Jefatura puede resolver boletas.
- Jefatura solo puede actuar sobre subordinados directos.
- Solo boletas en estado Pendiente Jefatura pueden resolverse (RN-04).
- Una boleta ya resuelta no puede modificarse.

## 7. Mensajes de error comunes y que hacer
| Caso | Mensaje comun | Que hacer |
|---|---|---|
| Rol incorrecto | Solo jefatura puede ver pendientes/resolver | Ingresar con rol Jefatura. |
| Boleta no existe | No existe la boleta indicada. | Verificar ID y refrescar bandeja. |
| No subordinado | no pertenece a subordinado directo | Escalar al area administradora de datos de usuarios/jefaturas. |
| Ya resuelta | RN-04: la boleta ya fue resuelta... | No reintentar, actualizar bandeja. |
| Concurrencia | ya cambio de estado | Refrescar pendientes y continuar con otras boletas. |
| Error de carga detalle | No se pudo cargar el detalle... | Reintentar; si persiste, reportar con correlationId. |

## 8. Errores frecuentes
### 8.1 Pendientes en cero cuando se esperaba carga
Causa:
- No hay subordinados directos configurados o todas las boletas ya se resolvieron.
Accion:
- Validar jerarquia en Usuarios.JefaturaID y estado de boletas.

### 8.2 Error 403 en detalle o resolucion
Causa:
- Usuario no autorizado para esa boleta especifica.
Accion:
- Confirmar que la boleta corresponde a subordinado directo.

### 8.3 Error 409 al resolver
Causa:
- Otra accion resolvio la boleta primero.
Accion:
- No insistir sobre la misma boleta; actualizar bandeja.

## 9. Buenas practicas
- Revisar detalle completo antes de resolver.
- Resolver con criterio uniforme y trazable.
- Evitar dejar boletas pendientes por periodos extensos.
- Reportar incidencias incluyendo ID de boleta y correlationId si existe.
- Coordinar con RRHH cuando se detecten patrones de inconsistencia.
