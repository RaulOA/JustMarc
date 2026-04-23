# Bloque 3 - Integracion Frontend-Backend API

## 1. Objetivo
Definir la integracion del frontend actual (index.html + dashboard.html + app.js) con el backend .NET ya implementado, reemplazando el flujo de boletas en localStorage por llamadas HTTP, manteniendo el comportamiento visual y funcional actual.

## 2. Hallazgos del estado actual

### 2.1 Frontend actual
- Login guarda sesion en sessionStorage (llave sjm_session) con:
  - username
  - role (inferido por texto del usuario)
  - company (inferida por prefijo cnp/fanal)
- Las boletas se guardan en localStorage (llave sjm_boletas).
- Funcionalidad actual basada en localStorage:
  - Registrar boleta (funcionario)
  - Listar historial propio
  - Listar pendientes para jefatura
  - Resolver (aprobar/rechazar)
  - Tabla global RRHH
- El panel SIFCNP es actualmente estatico (tabla hardcodeada en dashboard.html).

### 2.2 Backend API implementado
Endpoints existentes:
- POST /api/justificaciones
- GET /api/justificaciones/mias
- GET /api/jefatura/justificaciones/pendientes
- PATCH /api/jefatura/justificaciones/{justificacionId}/resolver
- GET /health

Autenticacion mock actual:
- Header requerido de usuario: X-User-Id (int > 0)
- Header requerido de rol: X-User-Role (string)

Valores esperados por reglas de negocio:
- Funcionario: ROL_FUNC (tambien acepta FUNCIONARIO o 1)
- Jefatura: ROL_JEFE (tambien acepta JEFATURA o 2)
- RRHH no tiene endpoints dedicados implementados en esta fase.

## 3. Contrato exacto de endpoints implementados

## 3.1 Crear justificacion
- Metodo y ruta: POST /api/justificaciones
- Headers requeridos:
  - Content-Type: application/json
  - X-User-Id: int
  - X-User-Role: ROL_FUNC
- Body request:

```json
{
  "motivoGeneral": "Texto hasta 500 chars",
  "detalles": [
    {
      "tipoJustificacionID": 1,
      "fechaMarca": "2026-04-22T00:00:00",
      "observacionDetalle": "Opcional hasta 250 chars"
    }
  ]
}
```

- Respuesta 201 Created:

```json
{
  "justificacionID": 123,
  "estadoID": 1,
  "estadoDescripcion": "Pendiente Jefatura"
}
```

- Errores esperables:
  - 400: validaciones de payload (RN-01, campos invalidos)
  - 401: falta/invalido X-User-Id o X-User-Role
  - 403: rol no autorizado (no funcionario)
  - 500: error interno

## 3.2 Listar mis justificaciones
- Metodo y ruta: GET /api/justificaciones/mias?estadoId=&desde=&hasta=
- Headers requeridos:
  - X-User-Id: int
  - X-User-Role: ROL_FUNC
- Query params opcionales:
  - estadoId: int
  - desde: DateTime
  - hasta: DateTime

- Respuesta 200 OK:

```json
[
  {
    "justificacionID": 123,
    "motivoGeneral": "Consulta medica",
    "estadoID": 1,
    "estadoDescripcion": "Pendiente Jefatura",
    "fechaCreacion": "2026-04-22T16:21:43.123",
    "cantidadDetalles": 2,
    "aprobadorID": null,
    "fechaAprobacion": null
  }
]
```

- Errores esperables:
  - 401: headers invalidos
  - 403: rol no autorizado
  - 500: error interno

## 3.3 Listar pendientes de jefatura
- Metodo y ruta: GET /api/jefatura/justificaciones/pendientes?desde=&hasta=
- Headers requeridos:
  - X-User-Id: int
  - X-User-Role: ROL_JEFE
- Query params opcionales:
  - desde: DateTime
  - hasta: DateTime

- Respuesta 200 OK:

```json
[
  {
    "justificacionID": 123,
    "motivoGeneral": "Consulta medica",
    "estadoID": 1,
    "estadoDescripcion": "Pendiente Jefatura",
    "fechaCreacion": "2026-04-22T16:21:43.123",
    "cantidadDetalles": 2,
    "aprobadorID": null,
    "fechaAprobacion": null
  }
]
```

- Errores esperables:
  - 401: headers invalidos
  - 403: rol no autorizado
  - 500: error interno

## 3.4 Resolver boleta
- Metodo y ruta: PATCH /api/jefatura/justificaciones/{justificacionId}/resolver
- Headers requeridos:
  - Content-Type: application/json
  - X-User-Id: int
  - X-User-Role: ROL_JEFE
- Body request:

```json
{
  "accion": "APROBAR",
  "comentario": "Opcional"
}
```

- Respuesta exitosa: 204 No Content

- Errores esperables:
  - 400: accion no valida (solo APROBAR o RECHAZAR)
  - 401: headers invalidos
  - 403: no autorizado o boleta fuera de subordinacion
  - 404: boleta no existe
  - 409: boleta ya resuelta (RN-04)
  - 500: error interno

## 4. Mapeo de login frontend a headers mock

## 4.1 Rol
Regla ya existente en frontend:
- username contiene rrhh -> ROL_RRHH
- username contiene jefe -> ROL_JEFE
- otro caso -> ROL_FUNC

Para enviar al backend:
- Si role = ROL_FUNC -> X-User-Role = ROL_FUNC
- Si role = ROL_JEFE -> X-User-Role = ROL_JEFE
- Si role = ROL_RRHH -> no hay endpoints RRHH dedicados (ver seccion de brechas)

## 4.2 Usuario (id numerico requerido)
Backend exige X-User-Id numerico que exista en la tabla Usuarios.

Requerimiento de integracion:
- Definir una tabla de mapeo local en frontend para ambiente mock, por ejemplo:

```js
const MOCK_USER_DIRECTORY = {
  "funcionario.ana": { userId: 10, role: "ROL_FUNC" },
  "jefe.maria": { userId: 20, role: "ROL_JEFE" },
  "rrhh.carlos": { userId: 30, role: "ROL_RRHH" }
};
```

Condicion obligatoria:
- Los userId deben existir realmente en SQL Server (dbo.Usuarios). Si no existen, POST /api/justificaciones fallara por integridad referencial.

## 5. Mapeos de datos frontend-api necesarios

## 5.1 Estado
Mapeo recomendado para mantener badges actuales:
- estadoID = 1 o estadoDescripcion contiene Pendiente -> etiqueta Pendiente
- estadoID = 2 o estadoDescripcion contiene Aprobado -> etiqueta Aprobado
- estadoID = 3 o estadoDescripcion contiene Rechazado -> etiqueta Rechazado

## 5.2 Tipo de justificacion (select frontend -> TipoJustificacionID)
Catalogo semilla actual en SQL:
- 1 = Marca Tardia
- 2 = Omision Marca de Entrada
- 3 = Omision Marca de Salida
- 4 = Marca antes Hora de Salida
- 5 = Ausencia

Mapeo requerido en frontend:
- Al crear cada detalle, transformar texto seleccionado a tipoJustificacionID numerico.

## 5.3 Identificador visible
- Hoy frontend muestra JM-0101 estilo string.
- API devuelve justificacionID numerico.

Para preservar UI actual:
- Mostrar prefijo visual JM- + padStart(4) solo en presentacion.
- Guardar/usar siempre el justificacionID numerico para llamadas PATCH.

## 6. Cambios requeridos en frontend para reemplazar localStorage boletas

## 6.1 Cambios de arquitectura en app.js
Agregar:
- BASE_URL de API (ejemplo http://localhost:5093)
- Cliente HTTP comun (apiFetch) con:
  - headers estandar
  - parse de ProblemDetails
  - manejo de timeout

Eliminar dependencia de:
- getBoletas
- saveBoletas

Mantener en sessionStorage:
- sjm_session
- sjm_activeTab

## 6.2 Flujo Funcionario
- registerJustification:
  - construir payload con motivoGeneral + detalles mapeados a TipoJustificacionID
  - POST /api/justificaciones
  - en exito limpiar formulario y refrescar historial por API
- renderFuncionarioHistory:
  - GET /api/justificaciones/mias
  - renderizar columnas actuales con fechaCreacion, cantidadDetalles, estado

## 6.3 Flujo Jefatura
- renderJefaturaRequests:
  - GET /api/jefatura/justificaciones/pendientes
  - poblar tabla pendiente
- approveRequest:
  - PATCH /api/jefatura/justificaciones/{id}/resolver con accion APROBAR o RECHAZAR
  - luego refrescar tabla pendientes

## 6.4 Flujo RRHH
- No existe endpoint RRHH implementado.
- Para preservar comportamiento actual inmediatamente:
  - Opcion A (recomendada): deshabilitar acciones RRHH con aviso funcional hasta exponer endpoint.
  - Opcion B (transitoria): mantener tabla RRHH con datos derivados de endpoints existentes segun rol, sabiendo que no es vista global real.

## 6.5 Flujo SIFCNP
- No existe endpoint historico implementado en backend actual.
- Mantener temporalmente la tabla estatica existente hasta implementar API de consulta historica.

## 7. Brechas de contrato para preservar UI al 100%

Brecha 1: tabla jefatura actual muestra Funcionario y Tipo de primera linea.
- API pendiente actual NO devuelve nombre de funcionario ni detalle tipo.
- Impacto: no se puede conservar exactamente la misma grilla solo con endpoints actuales.
- Recomendacion: extender contrato de pendientes (o crear endpoint detalle) con:
  - funcionarioNombre
  - funcionarioId
  - primerTipoJustificacionDescripcion (o lista detalles)

Brecha 2: detalle expandido jefatura muestra lista completa de detalles.
- API actual no expone endpoint de detalle por justificacion.
- Recomendacion: agregar GET /api/jefatura/justificaciones/{id} con encabezado + detalles.

Brecha 3: RRHH global.
- No hay endpoint para vista global/filtros/reporte.

## 8. Estrategia de manejo de errores frontend

## 8.1 Regla general
Toda llamada API debe manejar:
- Error de red (sin respuesta): mensaje de conectividad.
- HTTP != 2xx: parsear ProblemDetails.title y mostrarlo en alertas existentes.
- Exito: mantener mensajes actuales de confirmacion.

## 8.2 Tabla de mensajes sugeridos por codigo
- 400: Datos invalidos. Revise campos requeridos.
- 401: Sesion invalida para API. Inicie sesion nuevamente.
- 403: No tiene permisos para esta accion.
- 404: Registro no encontrado.
- 409: El estado de la boleta cambio y no puede actualizarse.
- 500: Error interno del servidor.

## 8.3 Comportamiento UI recomendado
- Deshabilitar botones de accion mientras request esta en curso.
- En error, reactivar botones y conservar datos del formulario.
- En resolver jefatura, refrescar lista aun cuando falle por 409 para reflejar estado real.

## 9. Consideraciones tecnicas de integracion

## 9.1 CORS
Actualmente Program.cs no configura CORS. Si frontend se sirve desde origen distinto (por ejemplo Live Server), el navegador bloqueara requests.

Cambio minimo backend sugerido:
- Registrar politica CORS para origen del frontend.
- Aplicar app.UseCors antes de MapControllers.

## 9.2 Fechas
- Frontend usa input date (YYYY-MM-DD).
- Backend acepta DateTime; enviar como YYYY-MM-DDT00:00:00 para evitar ambiguedad.

## 9.3 Compania
- Campo company existe en sesion frontend y en tabla RRHH UI, pero no forma parte de contratos API actuales de justificaciones.
- Mantener company como dato de presentacion hasta tener endpoint RRHH con compania.

## 10. Plan de implementacion sugerido (frontend)
1. Crear modulo API comun en app.js: base URL, buildHeaders, apiFetch.
2. Ajustar login para resolver userId mock y guardar apiUserId en sesion.
3. Reemplazar registerJustification por POST API.
4. Reemplazar renderFuncionarioHistory por GET mias.
5. Reemplazar renderJefaturaRequests y approveRequest por GET/PATCH API.
6. Mantener RRHH y SIFCNP en modo transitorio con aviso de alcance MVP.
7. Probar manualmente por rol:
   - funcionario crea y lista propio historial
   - jefatura lista pendientes y resuelve
   - manejo de 401/403/409

## 11. Criterio de aceptacion de integracion bloque 3
- No se usa localStorage para persistir boletas nuevas.
- Funcionario crea boletas via API y las ve en historial via API.
- Jefatura resuelve boletas via API con refresco de estado en pantalla.
- Headers mock se envian correctamente desde sesion.
- Errores de API se muestran en alertas UI existentes sin romper navegacion por pestanas.
- Las brechas no cubiertas por endpoints actuales quedan explicitamente controladas en UI (mensaje/estado transitorio).
