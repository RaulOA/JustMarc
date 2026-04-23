# Bloque 2 - Backend API MVP (.NET + SQL Server)

## 1. Objetivo de este bloque
Diseñar el backend MVP para continuar el frontend existente, priorizando:
- RF-02: Creación de boleta (encabezado + detalles en transacción)
- RF-03: Flujo de aprobación/rechazo por jefatura

Persistencia objetivo: SQL Server 2019, base INTEGRA_CNP, alineada con el script actual docs/db/001_init_integra_cnp.sql.

## 2. Hallazgos relevantes del workspace
- Frontend MVP completo en HTML/CSS/JS con flujo mock en localStorage (app.js):
  - Crear boleta con múltiples líneas
  - Historial propio funcionario
  - Pendientes para jefatura
  - Aprobar/rechazar
  - Vista RRHH (tabla global)
- PRP define reglas RN-01 a RN-04 y estados fijos:
  - 1 = Pendiente Jefatura
  - 2 = Aprobado
  - 3 = Rechazado
- Modelo de datos ya iniciado en SQL con tablas:
  - Roles, Estados, Cat_TiposJustificacion
  - Usuarios
  - Justificaciones_Encabezado
  - Justificaciones_Detalle

## 3. Stack y enfoque técnico propuesto
- Runtime: .NET 8 (ASP.NET Core Web API)
- Data access: Dapper (simple y directo para MVP)
- SQL Client: Microsoft.Data.SqlClient
- Documentación API: Swagger/OpenAPI
- Auth (MVP): cabeceras simuladas para usuario/rol (hasta integrar dominio)
- Arquitectura: monolítica 3 capas liviana
  - API (Controllers)
  - Aplicación (Services + validaciones)
  - Infraestructura (Repositories + SQL)

## 4. Estructura de carpetas propuesta
```text
backend/
  src/
    IntegradorMarcas.Api/
      Controllers/
        JustificacionesController.cs
        JefaturaController.cs
      Contracts/
        Requests/
          CreateJustificacionRequest.cs
          JustificacionDetalleRequest.cs
          ResolverJustificacionRequest.cs
        Responses/
          JustificacionResumenResponse.cs
          JustificacionDetalleResponse.cs
          JustificacionCompletaResponse.cs
      Program.cs
      appsettings.json
      appsettings.Development.json
    IntegradorMarcas.Application/
      Interfaces/
        IJustificacionService.cs
      Services/
        JustificacionService.cs
      Validation/
        JustificacionValidator.cs
    IntegradorMarcas.Domain/
      Entities/
        Usuario.cs
        Rol.cs
        Estado.cs
        TipoJustificacion.cs
        JustificacionEncabezado.cs
        JustificacionDetalle.cs
      Constants/
        EstadoIds.cs
        RolesSistema.cs
    IntegradorMarcas.Infrastructure/
      Data/
        SqlConnectionFactory.cs
      Repositories/
        Interfaces/
          IJustificacionRepository.cs
        JustificacionRepository.cs
      Queries/
        JustificacionesSql.cs
  tests/
    IntegradorMarcas.Tests/
      JustificacionServiceTests.cs
```

## 5. Modelos de datos (alineados a INTEGRA_CNP)

### 5.1 Tabla Roles
Entidad: Rol
- RolID (int, PK)
- NombreRol (varchar(50))
- Usr_Registro (varchar(50))
- Fec_Registro (datetime)

### 5.2 Tabla Estados
Entidad: Estado
- EstadoID (int, PK)
- Descripcion (varchar(100))
- Proceso (varchar(50))

### 5.3 Tabla Cat_TiposJustificacion
Entidad: TipoJustificacion
- TipoJustificacionID (int, PK, identity)
- Descripcion (varchar(100))
- Usr_Registro (varchar(50))
- Fec_Registro (datetime)

### 5.4 Tabla Usuarios
Entidad: Usuario
- UsuarioID (int, PK, identity)
- Cedula (varchar(20))
- NombreCompleto (varchar(150))
- Correo (varchar(100))
- JefaturaID (int, FK a Usuarios, nullable)
- UnidadID (int)
- RolID (int, FK a Roles)
- Compania (varchar(10), check CNP/FANAL)
- Usr_Registro (varchar(50))
- Fec_Registro (datetime)
- Usr_Modifica (varchar(50), nullable)
- Fec_Modifica (datetime, nullable)

### 5.5 Tabla Justificaciones_Encabezado
Entidad: JustificacionEncabezado
- JustificacionID (int, PK, identity)
- UsuarioID (int, FK a Usuarios)
- MotivoGeneral (varchar(500))
- EstadoID (int, FK a Estados)
- FechaCreacion (datetime)
- AprobadorID (int, FK a Usuarios, nullable)
- FechaAprobacion (datetime, nullable)
- Usr_Registro (varchar(50))
- Fec_Registro (datetime)

### 5.6 Tabla Justificaciones_Detalle
Entidad: JustificacionDetalle
- DetalleID (int, PK, identity)
- JustificacionID (int, FK a Justificaciones_Encabezado)
- TipoJustificacionID (int, FK a Cat_TiposJustificacion)
- FechaMarca (date)
- ObservacionDetalle (varchar(250), nullable)
- Usr_Registro (varchar(50))
- Fec_Registro (datetime)

## 6. Endpoints API (MVP RF-02 y RF-03 primero)

### 6.1 Crear justificación (encabezado + detalle transaccional)
POST /api/justificaciones
- Rol esperado: ROL_FUNC
- Operación:
  - Valida request
  - Inserta encabezado con EstadoID=1
  - Inserta N detalles
  - Commit transacción
- Respuesta: 201 Created con ID generado y estado inicial

### 6.2 Listar mis justificaciones
GET /api/justificaciones/mias?estadoId=&desde=&hasta=
- Rol esperado: ROL_FUNC
- Filtro base por UsuarioID del autenticado
- Incluye estado, fecha creación, cantidad detalles, aprobador/fecha resolución si existe

### 6.3 Listar pendientes para jefatura
GET /api/jefatura/justificaciones/pendientes?desde=&hasta=
- Rol esperado: ROL_JEFE
- Filtro base: subordinados directos (Usuarios.JefaturaID = usuario logueado)
- Solo EstadoID=1

### 6.4 Aprobar/Rechazar boleta
PATCH /api/jefatura/justificaciones/{justificacionId}/resolver
Body: accion = APROBAR | RECHAZAR, comentario opcional
- Rol esperado: ROL_JEFE
- Cambia estado a 2 o 3
- Registra AprobadorID y FechaAprobacion
- Debe validar que la boleta esté en pendiente y pertenezca a subordinado

## 7. Reglas de validación (RN-01 a RN-04)

### RN-01
Una boleta debe tener al menos una línea de detalle.
- Validar en CreateJustificacionRequest: Detalles.Count >= 1
- Si falla: 400 Bad Request

### RN-02
Estado inicial siempre Pendiente Jefatura (EstadoID=1).
- No aceptar EstadoID en request de creación
- Asignar en backend de forma forzada

### RN-03
Solo ROL_JEFE puede cambiar estado.
- Proteger endpoint resolver por rol
- Validar además relación jerárquica del solicitante con la jefatura

### RN-04
Boleta Aprobada/Rechazada no puede modificarse.
- Resolver solo si EstadoID actual = 1
- Cualquier intento sobre EstadoID 2/3 retorna 409 Conflict

## 8. DTOs propuestos

### 8.1 CreateJustificacionRequest
- MotivoGeneral (string, required, max 500)
- Detalles (array required, min 1)

### 8.2 JustificacionDetalleRequest
- TipoJustificacionID (int, required)
- FechaMarca (date, required)
- ObservacionDetalle (string?, max 250)

### 8.3 ResolverJustificacionRequest
- Accion (string, required: APROBAR|RECHAZAR)
- Comentario (string?, opcional para trazabilidad futura)

### 8.4 JustificacionResumenResponse
- JustificacionID (int)
- MotivoGeneral (string)
- EstadoID (int)
- EstadoDescripcion (string)
- FechaCreacion (datetime)
- CantidadDetalles (int)
- AprobadorID (int?)
- FechaAprobacion (datetime?)

### 8.5 JustificacionCompletaResponse
- Encabezado (JustificacionResumenResponse)
- Detalles (array JustificacionDetalleResponse)

### 8.6 JustificacionDetalleResponse
- DetalleID (int)
- TipoJustificacionID (int)
- TipoJustificacionDescripcion (string)
- FechaMarca (date)
- ObservacionDetalle (string?)

## 9. Split mínimo Repository/Service

### Service (reglas de negocio)
IJustificacionService / JustificacionService
- CreateAsync(usuarioId, request)
- ListMineAsync(usuarioId, filtros)
- ListPendientesJefaturaAsync(jefaturaId, filtros)
- ResolverAsync(jefaturaId, justificacionId, accion)

Responsabilidades:
- Validaciones RN-01..RN-04
- Orquestación transaccional con repositorio
- Reglas de autorización funcional (propietario/subordinado)

### Repository (persistencia)
IJustificacionRepository / JustificacionRepository
- BeginTransaction + CreateHeader + CreateDetails + Commit/Rollback
- Query por funcionario
- Query pendientes por jefatura
- Update resolución (estado/aprobador/fecha)

Responsabilidades:
- SQL parametrizado
- Mapeo de filas a entidades/DTO de salida
- Sin reglas de negocio complejas

## 10. Diseño SQL de operaciones críticas

### 10.1 Transacción de creación (RF-02)
1) INSERT Justificaciones_Encabezado
- UsuarioID
- MotivoGeneral
- EstadoID = 1
- Usr_Registro

2) Obtener JustificacionID (SCOPE_IDENTITY)

3) INSERT N filas en Justificaciones_Detalle
- JustificacionID
- TipoJustificacionID
- FechaMarca
- ObservacionDetalle
- Usr_Registro

4) COMMIT

### 10.2 Resolución por jefatura (RF-03)
UPDATE Justificaciones_Encabezado
SET
- EstadoID = 2 o 3
- AprobadorID = @jefaturaId
- FechaAprobacion = GETDATE()
WHERE
- JustificacionID = @id
- EstadoID = 1
- UsuarioID IN (SELECT UsuarioID FROM Usuarios WHERE JefaturaID = @jefaturaId)

Si affected rows = 0:
- no existe, o no pertenece a subordinado, o ya no está pendiente.

## 11. Claves de configuración
appsettings.json
- ConnectionStrings:
  - IntegraCnp (SQL principal lectura/escritura)
  - WizdomReadOnly (futuro RF-01)
  - SifcnpReadOnly (futuro RF-06)
- Security:
  - UseMockIdentity (bool, MVP true)
  - HeaderUserId (string, ej X-User-Id)
  - HeaderRole (string, ej X-User-Role)
- Logging:
  - LogLevel.Default
  - LogLevel.Microsoft.AspNetCore
- Swagger:
  - Enabled (bool)

## 12. Ejecución paso a paso (scaffold MVP)
1. Crear solución y proyectos:
- dotnet new sln -n IntegradorMarcas
- dotnet new webapi -n IntegradorMarcas.Api
- dotnet new classlib -n IntegradorMarcas.Application
- dotnet new classlib -n IntegradorMarcas.Domain
- dotnet new classlib -n IntegradorMarcas.Infrastructure
- dotnet new xunit -n IntegradorMarcas.Tests

2. Agregar referencias entre proyectos:
- Api -> Application, Infrastructure
- Application -> Domain
- Infrastructure -> Domain, Application
- Tests -> Application, Domain

3. Instalar paquetes:
- Dapper
- Microsoft.Data.SqlClient
- Swashbuckle.AspNetCore

4. Configurar DI en Program.cs:
- SqlConnectionFactory
- Repositories
- Services

5. Ejecutar script SQL:
- docs/db/001_init_integra_cnp.sql en SQL Server 2019

6. Implementar endpoints de RF-02/RF-03 y listar mías/pendientes.

7. Probar en Swagger:
- Crear boleta con 1..N detalles
- Consultar mías
- Consultar pendientes jefatura
- Aprobar/Rechazar

8. Integrar frontend actual (app.js) reemplazando localStorage por llamadas HTTP.

## 13. Criterios de aceptación del bloque
- Crear boleta guarda encabezado + detalles en una sola transacción
- Estado inicial forzado a Pendiente Jefatura
- Jefatura solo puede resolver boletas de subordinados directos
- No se permite resolver dos veces la misma boleta
- Funcionario ve únicamente sus boletas
- Pendientes de jefatura devuelven solo estado pendiente

## 14. Riesgos y mitigaciones inmediatas
- Riesgo: identidad aún mock.
  - Mitigación: encapsular contexto usuario en IUserContext para luego conectar AD sin romper controladores.
- Riesgo: divergencia entre catálogos frontend y BD.
  - Mitigación: endpoint GET /api/catalogos/tipos-justificacion para poblar select dinámico.
- Riesgo: concurrencia al resolver.
  - Mitigación: update condicionado por EstadoID=1 + control de filas afectadas.
