# Manual de Usuario - Rol RRHH

## 1. Objetivo
Guiar al rol RRHH en la consulta global de justificaciones y uso correcto de filtros operativos.

## 2. Alcance
Este manual cubre:
- Consulta global de boletas.
- Aplicacion y limpieza de filtros.
- Interpretacion de resultados para seguimiento.

No cubre:
- Resolucion de boletas (propia de Jefatura).
- Registro de boletas (propio de Funcionario).

## 3. Prerequisitos
- Acceso a dashboard.html con rol RRHH.
- API habilitada y con datos disponibles.
- Conocimiento basico de criterios de busqueda (funcionario, estado, compania, fechas).

## 4. Acceso al panel RRHH
1. Ingrese con usuario RRHH (ejemplo de prueba: rrhh.carlos).
2. Abra Panel RRHH.
3. Verifique que se muestre la tabla global de boletas.

## 5. Flujo principal: consulta global
1. Ingrese criterios de filtro segun necesidad.
2. Presione Aplicar filtros.
3. Revise resultados en la tabla.
4. Si requiere nueva consulta, ajuste filtros y vuelva a aplicar.
5. Para volver al estado base, use Limpiar filtros.

## 6. Uso de filtros
### 6.1 Funcionario
- Acepta nombre o cedula (busqueda parcial).
- Longitud maxima: 150 caracteres.

### 6.2 Estado
- Pendiente, Aprobado o Rechazado.
- El frontend mapea el valor seleccionado a estadoId para la API.

### 6.3 Compania
- Valores validos: CNP o FANAL.
- Cualquier otro valor genera validacion en backend.

### 6.4 Fecha desde / fecha hasta
- Filtran por fecha de creacion de la boleta.
- Regla: fechaDesde no puede ser mayor que fechaHasta.

## 7. Reglas de negocio aplicables al rol
- Solo RRHH puede consultar boletas globales.
- RRHH no puede aprobar ni rechazar desde su panel.
- Los filtros respetan validaciones de rango y compania permitida.

## 8. Mensajes de error comunes y que hacer
| Caso | Mensaje comun | Que hacer |
|---|---|---|
| Rol incorrecto | Solo RRHH puede consultar boletas globales. | Ingresar con rol RRHH. |
| Compania invalida | Compania invalida. Valores permitidos: CNP o FANAL. | Corregir compania a CNP/FANAL. |
| Rango invalido | Desde no puede ser mayor que Hasta. | Ajustar fechas en orden correcto. |
| Texto muy largo | texto de busqueda no puede exceder 150 | Reducir longitud del filtro funcionario. |
| Sin datos | No hay registros disponibles. | Verificar filtros o ampliar rango de fechas. |
| Falla de API | No se pudo cargar informacion RRHH... | Revisar conexion backend y repetir consulta. |

## 9. Casos de consulta frecuentes
- Ver boletas pendientes de una compania especifica.
- Buscar historial de un funcionario por nombre o cedula.
- Auditar volumen de aprobadas y rechazadas por rango de fechas.

## 10. Errores frecuentes
### 10.1 Tabla vacia con filtros muy restrictivos
Causa:
- Combinacion de filtros sin coincidencias.
Accion:
- Limpiar filtros y aplicar criterios progresivos.

### 10.2 Error 400 por parametros
Causa:
- Compania invalida o rango de fecha incorrecto.
Accion:
- Corregir parametros segun reglas del manual.

### 10.3 Error de conectividad
Causa:
- API detenida o URL base no disponible.
Accion:
- Verificar estado del backend y soporte tecnico.

## 11. Buenas practicas
- Aplicar primero filtros amplios y luego refinar.
- Mantener consistencia en criterios de periodos (semanal/mensual).
- Validar compania y estado antes de ejecutar consultas criticas.
- Documentar incidencias con mensaje exacto y correlationId cuando exista.
- Coordinar con Jefaturas cuando se detecten atrasos de resolucion.
