# Manual de Usuario - Rol Funcionario

## 1. Objetivo
Guiar al funcionario en el registro y consulta de boletas de justificacion de marcas dentro del sistema.

## 2. Alcance
Este manual cubre:
- Ingreso al sistema.
- Creacion de boletas con lineas de detalle.
- Consulta de historial propio.

No cubre:
- Resolucion de boletas (rol Jefatura).
- Consulta global de todas las boletas (rol RRHH).

## 3. Prerequisitos
- Tener acceso al frontend (index.html y dashboard.html).
- Contar con usuario de prueba o institucional activo.
- Si se usa ambiente local, API encendida y base de datos operativa.

## 4. Acceso al sistema
1. Abra index.html.
2. Ingrese usuario y contrasena.
3. Presione Ingresar.
4. Verifique que en la barra superior aparezca Rol: FUNC.

Nota:
- En ambiente de prueba, ejemplos de usuario: funcionario.ana.

## 5. Flujo principal: crear una justificacion
### 5.1 Registrar encabezado
1. Ir a Panel Funcionario.
2. Completar Motivo General.
3. Motivo General es obligatorio y maximo 500 caracteres.

### 5.2 Agregar lineas de detalle
1. Seleccionar Tipo de Justificacion.
2. Elegir Fecha de Marca.
3. (Opcional) escribir Observacion del Detalle (maximo 250).
4. Presionar Agregar Linea.
5. Repetir para cada marca requerida.

Regla obligatoria:
- Debe existir al menos una linea de detalle antes de registrar.

### 5.3 Registrar boleta
1. Verifique lineas agregadas en la tabla de detalle.
2. Presione Registrar Justificacion.
3. Espere notificacion de exito.
4. Resultado esperado: boleta creada en estado Pendiente Jefatura.

## 6. Flujo secundario: consultar historial propio
1. En el mismo panel, revise Mi Historial de Justificaciones.
2. Verifique columnas: ID, motivo, cantidad de conceptos, fecha, estado y fecha de resolucion.
3. Si no hay datos, el sistema mostrara No hay boletas registradas.

## 7. Reglas de negocio aplicables al rol
- Solo Funcionario puede crear boletas propias.
- Solo Funcionario puede consultar su historial propio.
- Una boleta nueva siempre inicia en Pendiente Jefatura.
- TipoJustificacionID debe existir en catalogo.

## 8. Mensajes de error comunes y que hacer
| Caso | Mensaje comun | Que hacer |
|---|---|---|
| Motivo vacio | El motivo general es obligatorio. | Complete el campo Motivo General. |
| Sin detalles | Debe agregar al menos una linea de detalle. | Agregue por lo menos una linea valida. |
| Tipo no seleccionado | Cada detalle requiere tipo... | Seleccione tipo antes de agregar. |
| Fecha faltante | Cada detalle requiere ... fecha | Ingrese fecha de marca. |
| Error backend validacion | No se pudo registrar la boleta: ... | Revise el texto del error y corrija datos. |
| Sesion de rol incorrecta | Solo el rol Funcionario puede registrar boletas. | Ingrese con un usuario de rol Funcionario. |
| API no disponible | No fue posible conectar con la API... | Verifique backend encendido y URL base. |

## 9. Errores frecuentes
### 9.1 No se registra la boleta
Causa:
- Falta de datos obligatorios o error de validacion API.
Accion:
- Revise motivo, tipo, fecha, longitudes maximas y vuelva a intentar.

### 9.2 Historial vacio despues de registrar
Causa:
- Falla temporal de consulta o rol incorrecto.
Accion:
- Refresque la pagina y confirme que ingreso con rol Funcionario.

### 9.3 Mensaje de credenciales invalidas
Causa:
- Usuario/contrasena no cumple reglas minimas de login en MVP.
Accion:
- Verifique datos (usuario >= 3 caracteres, contrasena >= 4).

## 10. Buenas practicas
- Describir el motivo general de forma concreta y verificable.
- Agregar observaciones por linea solo cuando aporten contexto.
- Revisar cuidadosamente fechas antes de registrar.
- Evitar duplicar boletas para el mismo evento.
- Dar seguimiento al estado en historial (Pendiente/Aprobado/Rechazado).
