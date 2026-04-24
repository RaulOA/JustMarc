---

# Estructura y Gestión de Datos — Convenciones y Nomenclatura

Este apartado establece las normas de nomenclatura y convenciones para todos los objetos de base de datos en SQL Server 2022. Su objetivo es garantizar legibilidad, consistencia y mantenibilidad entre todos los miembros del equipo.

---

## 1. Reglas Generales

**[OBLIGATORIO]** Aplican a todos los objetos sin excepción:

- Se usa **PascalCase** en todos los objetos: tablas, columnas, vistas, procedimientos, parámetros y variables.
- Se prohíben espacios, caracteres especiales y números como primer carácter en cualquier nombre.
- Se prohíben palabras reservadas de T-SQL como nombres de objetos (`Date`, `Name`, `Value`, `Status`, `Type`, `Table`, `Key`).
- Todos los nombres deben estar en **español**, ser descriptivos y sin abreviaciones innecesarias.
- **[OBLIGATORIO]** Nunca se deben dejar que SQL Server genere nombres de objetos automáticamente (constraints, índices, claves). Un nombre autogenerado como `PK__Pedido__3213E83F5A8B1B2C` no es portable entre ambientes y dificulta el mantenimiento.

---

## 2. Esquemas

**[OBLIGATORIO]** Todo objeto debe pertenecer a un esquema explícito. El esquema `dbo` queda reservado exclusivamente para objetos de sistema.

El nombre del esquema debe representar el área funcional del negocio, en PascalCase y sin prefijos.

| Esquema | Propósito |
|---|---|
| `Ventas` | Transacciones comerciales |
| `RecursosHumanos` | Información del personal |
| `Seguridad` | Permisos y accesos |
| `Auditoria` | Trazabilidad y bitácora de cambios |
| `Configuracion` | Parámetros y catálogos del sistema |
| `Produccion` | Operaciones productivas |

---

## 3. Tablas

**[OBLIGATORIO]**

- Formato: `Esquema.NombreTabla`
- El nombre debe estar en **singular** — la tabla define el tipo de entidad, no una colección (`Pedido`, no `Pedidos`).
- Se prohíben prefijos como `tbl_`, `GP_`, `T_`, `TB_` o cualquier otro prefijo decorativo. Este tipo de notación húngara inversa nunca fue un estándar para SQL y entra en conflicto con las convenciones de nomenclatura de SQL Server.
- Para tablas de relación (muchos a muchos), usar el sustantivo que describe la relación cuando existe. Cuando no existe un término natural, concatenar ambos nombres sin separador: `PedidoProducto`.

| Esquema | Tabla | Descripción |
|---|---|---|
| `Ventas` | `Pedido` | Registro de transacciones comerciales |
| `Ventas` | `DetallePedido` | Líneas de detalle de cada pedido |
| `Produccion` | `TipoProduccion` | Catálogo de tipos de producción |
| `RecursosHumanos` | `Empleado` | Personal activo e inactivo |
| `Configuracion` | `Parametro` | Parámetros generales del sistema |

---

## 4. Columnas

**[OBLIGATORIO]**

- PascalCase, sin prefijos de tipo (`IntClienteId` ✗, `VcNombre` ✗) ni prefijos de campo (`fld_`, `col_` ✗).
- El nombre debe describir el dato, no el tipo.
- Una columna nunca debe llamarse igual que su tabla.

Reglas específicas por tipo semántico:

| Tipo semántico | Convención | Ejemplo |
|---|---|---|
| Clave primaria | `[NombreTabla]Id` | `PedidoId`, `EmpleadoId` |
| Clave foránea | Mismo nombre que la PK de la tabla referenciada | `ClienteId`, `ProductoId` |
| Fecha | Iniciar o terminar con `Fecha` | `FechaRegistro`, `FechaBaja` |
| Fecha y hora | Iniciar o terminar con `FechaHora` | `FechaHoraCreacion` |
| Booleano | Iniciar con `Es` o `Tiene`, siempre afirmativo | `EsActivo`, `TieneDescuento` |
| Monto / precio | Terminar con el concepto monetario | `PrecioUnitario`, `MontoTotal` |
| Descripción larga | Terminar con `Descripcion` o `Detalle` | `DescripcionProducto` |
| Código externo | Iniciar con `Codigo` | `CodigoPostal`, `CodigoProducto` |
| Hash / valor protegido | Sufijo `Hash` | `ContrasenaHash` |

> **Sobre booleanos:** Los nombres de campos booleanos deben ser afirmativos. Un campo llamado `NoEstaActivo` o `Inactivo` obliga al desarrollador a razonar en negativo, lo que introduce errores de lógica. Siempre usar la forma positiva: `EsActivo = 0` significa inactivo, lo cual es claro e inequívoco.

> **Sobre la clave primaria:** Nombrar la clave primaria simplemente `Id` es problemático cuando se consultan varias tablas simultáneamente, ya que obliga a renombrar la columna en cada resultado y puede enmascarar errores en los `JOIN`. El formato `[NombreTabla]Id` elimina esta ambigüedad.

> **Sobre claves foráneas:** La columna de clave foránea debe tener exactamente el mismo nombre que la clave primaria de la tabla que referencia. Esto hace que las relaciones sean autoexplicativas: `Pedido.ClienteId` referencia a `Cliente.ClienteId`.

Ejemplos correctos:

| Tabla | Columna | Descripción |
|---|---|---|
| `Pedido` | `PedidoId` | Clave primaria del pedido |
| `Pedido` | `ClienteId` | Referencia a `Cliente.ClienteId` |
| `Pedido` | `FechaRegistro` | Fecha de creación del pedido |
| `Pedido` | `EstaAnulado` | Indica si el pedido fue anulado |
| `Cliente` | `ClienteId` | Clave primaria del cliente |
| `Cliente` | `NombreCompleto` | Nombre completo del cliente |
| `Cliente` | `FechaAlta` | Fecha de registro del cliente |
| `Cliente` | `EsActivo` | Estado activo/inactivo del cliente |
| `Producto` | `ProductoId` | Clave primaria del producto |
| `Producto` | `CodigoProducto` | Código externo del producto |
| `Producto` | `PrecioUnitario` | Precio por unidad |
| `Usuario` | `UsuarioId` | Clave primaria del usuario |
| `Usuario` | `CorreoElectronico` | Correo de la cuenta |
| `Usuario` | `ContrasenaHash` | Contraseña almacenada como hash |
| `Usuario` | `FechaHoraUltimoAcceso` | Último inicio de sesión |

---

## 5. Columnas de Auditoría

**[OBLIGATORIO]** Toda tabla de entidades de negocio debe incluir las siguientes columnas de auditoría con nombres estandarizados en todo el sistema:

| Columna | Tipo | Descripción |
|---|---|---|
| `CreadoPor` | `NVARCHAR(100)` | Usuario que creó el registro |
| `FechaHoraCreacion` | `DATETIME2` | Fecha y hora de creación |
| `ModificadoPor` | `NVARCHAR(100)` | Usuario del último cambio |
| `FechaHoraModificacion` | `DATETIME2` | Fecha y hora de la última modificación |

**[OBLIGATORIO]** Toda tabla debe soportar baja lógica. El nombre del campo debe reflejar el estado real de la entidad, no un genérico universal:

| Contexto | Campo correcto | Campo incorrecto |
|---|---|---|
| Entidades con ciclo activo/inactivo (`Empleado`, `Usuario`, `Cliente`) | `EsActivo` | `Eliminado` |
| Transacciones cancelables (`Pedido`, `Factura`) | `EstaAnulado` | `EsActivo` |
| Registros con vigencia temporal (`Contrato`, `Parametro`) | `EstaVigente` | `EsActivo` |

---

## 6. Vistas

**[OBLIGATORIO]**

- Formato: `v_NombreDescriptivo`
- El nombre debe describir qué datos expone, no cómo los obtiene.
- PascalCase después del prefijo `v_`.

| Vista | Descripción |
|---|---|
| `v_DetalleTransaccion` | Detalle de transacciones del sistema |
| `v_ResumenVentas` | Resumen de ventas por período |
| `v_UsuarioActivo` | Usuarios con sesión activa reciente |
| `v_ReporteMensual` | Consolidado mensual de operaciones |
| `v_ValidacionError` | Registros con errores en validaciones |

---

## 7. Secuencias

**[FLEXIBLE]** Formato: `SEQ_NombreContexto`

Ejemplos:
- `SEQ_Factura`
- `SEQ_Transaccion`
- `SEQ_OrdenProduccion`

---

## 8. Constraints e Índices

**[OBLIGATORIO]** Nunca dejar que SQL Server genere nombres automáticamente para ningún constraint o índice.

### 8.1 Clave Primaria

Formato: `[NombreTabla]_[NombreColumna]`

- `Pedido_PedidoId`
- `Empleado_EmpleadoId`

### 8.2 Clave Foránea

Formato: `[TablaOrigen]_[TablaDestino]`

En casos donde hay múltiples relaciones hacia la misma tabla, agregar el contexto de la relación:

- `Pedido_Cliente` — relación estándar
- `Pedido_DireccionEnvio` — cuando hay múltiples FK hacia la misma tabla de dirección
- `Pedido_DireccionFacturacion`

### 8.3 Índices Secundarios

Formato: `[NombreTabla]_[Columna1]_[Columna2]`

- `Cliente_Apellido`
- `Pedido_FechaRegistro`
- `Empleado_DepartamentoId`

### 8.4 Constraints de Valor por Defecto

Formato: `[NombreTabla]_[NombreColumna]_Default`

- `Pedido_FechaRegistro_Default`
- `Empleado_EsActivo_Default`

### 8.5 Constraints de Verificación (CHECK)

Formato: `[NombreTabla]_[NombreColumna]_[Descripcion]`

- `Producto_PrecioUnitario_Minimo`
- `Empleado_Edad_Rango`

### 8.6 Constraints de Unicidad

Formato: `[NombreTabla]_[NombreColumna]_Unico`

- `Usuario_CorreoElectronico_Unico`
- `Producto_CodigoProducto_Unico`

---

## 9. Procedimientos Almacenados

**[OBLIGATORIO]**

- Formato: `usp_EntidadAccion`
- Se prohíbe el prefijo `sp_`. SQL Server busca primero en `master` cualquier objeto con ese prefijo, lo que genera overhead innecesario y riesgo de colisión con procedimientos del sistema.
- El nombre debe ordenarse por entidad primero y la acción como sufijo, facilitando la agrupación visual en el explorador de objetos: `ProductoObtener`, `PedidoRegistrar`.
- Las acciones válidas son: `Obtener`, `Registrar`, `Actualizar`, `Eliminar`, `Validar`, `Generar`, `Procesar`.

| Correcto | Incorrecto | Razón |
|---|---|---|
| `usp_PedidoRegistrar` | `sp_insertarpedido` | Prefijo prohibido, sin capitalización |
| `usp_EmpleadoObtenerPorDepartamento` | `usp_consulta1` | Sin descripción funcional |
| `usp_ReporteMensualGenerar` | `usp_repmens` | Abreviación innecesaria |
| `usp_StockProductoValidar` | `sp_validacion` | Prefijo prohibido, nombre ambiguo |

**[OBLIGATORIO]** Todo procedimiento debe incluir este encabezado:

```sql
-- =============================================
-- Autor:         [Nombre]
-- Fecha:         [YYYY-MM-DD]
-- Descripción:   [Propósito del procedimiento]
-- Modificaciones:
--   [YYYY-MM-DD] [Autor] [Descripción del cambio]
-- =============================================
```

---

## 10. Parámetros y Variables T-SQL

**[OBLIGATORIO]**

- Todo parámetro y variable inicia con `@`.
- Se prohíbe el uso de `@@` como prefijo de variables propias, ya que está reservado para variables globales del sistema de SQL Server.
- PascalCase después del `@`: `@ClienteId`, `@FechaInicio`, `@NombreCompleto`.
- El nombre del parámetro debe ser idéntico al nombre de la columna que representa, diferenciado únicamente por el símbolo `@`. Si la columna es `ClienteId`, el parámetro es `@ClienteId`.

| Correcto | Incorrecto |
|---|---|
| `@ClienteId` | `@@clienteid` |
| `@FechaInicio` | `@fecha1` |
| `@NombreCompleto` | `@nom` |

---

## 11. Roles

**[OBLIGATORIO]** Los roles deben integrarse con el Directorio Activo de Windows (o Microsoft Entra ID en entornos en la nube) y seguir el principio de mínimo privilegio.

Formato: `ROL_NombreFuncional`

| Rol | Permisos |
|---|---|
| `ROL_Administrador` | Control total sobre la base de datos |
| `ROL_Lectura` | `SELECT` en esquemas autorizados |
| `ROL_Operaciones` | Ejecución de procedimientos almacenados específicos |
| `ROL_ETL` | Escritura limitada a procesos de carga |
| `ROL_Auditoria` | Solo lectura sobre el esquema `Auditoria` |

**[OBLIGATORIO]** Se prohíbe usar la cuenta `sa` o cualquier cuenta con privilegios de administrador para operaciones de aplicación.

---