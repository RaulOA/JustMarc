# docs/conventions.md — Convenciones del proyecto

> Derivadas del código presente. No incluyas reglas de lenguajes que el proyecto no usa.

## C# (.NET 8)

- File-scoped namespaces (`namespace X;`). `Nullable` e `ImplicitUsings` habilitados. Target `net8.0`.
- Clases concretas `sealed` por defecto (controllers, services, repositories, entities, DTOs).
- Naming: DTOs/Entities en PascalCase con sufijo `Id` (`JustificacionId`). Los **Contracts** usan sufijo
  `ID` (`JustificacionID`); los controllers mapean `Id`↔`ID` a mano. Params SQL `@PascalCase`
  (`@UsuarioID`).
- Todo I/O es `async Task` con `CancellationToken` hilado controller→service→repository→Dapper.
  Colecciones como `IReadOnlyList`/`IReadOnlyCollection`.
- SQL nuevo: raw string literals (`"""..."""`); el viejo usa verbatim (`@"..."`).
- Doc-comments `///` en acciones públicas de controllers.

## Frontend (`app.js`)

- Vanilla JS, un solo script global, sin módulos/imports. Interacción con backend solo vía `apiFetch` +
  `buildApiHeaders`.
- Todo valor interpolado en `innerHTML` pasa por `escapeHtml()`. Fechas via `formatDate`/`formatDateTime`
  (dd/mm/yyyy), `—` para vacíos.
- Claves de `sessionStorage` con prefijo `sjm_`.

## Base de datos (OBLIGATORIO — ver `docs/db/Convenciones_Nomeclatura_BD.md`)

- PascalCase en TODO objeto. Español, descriptivo, sin abreviaciones.
- Tablas en singular (`Usuario`), formato `Esquema.NombreTabla`. Esquemas: `Configuracion`,
  `RecursosHumanos`, `Operacion`, `Auditoria`, `Integracion`.
- PK = `[Tabla]Id`; FK = mismo nombre que la PK referenciada.
- Booleanos `Es`/`Tiene`; fechas `Fecha`/`FechaHora`; códigos externos `Codigo`.
- Auditoría obligatoria en entidades de negocio: `CreadoPor`, `FechaHoraCreacion`, `ModificadoPor`,
  `FechaHoraModificacion`.
- Vistas `v_PascalCase`; SP `usp_EntidadAccion` (prohibido `sp_`). Constraints/índices con nombre
  explícito. Scripts idempotentes.
- Desviaciones deliberadas (NO corregir): `Auditoria.ErrorApi` con columnas en inglés; vista legada
  `dbo.V_JUSTIFICACIONES_DETALLE` en MAYÚSCULAS_SNAKE.

## Idioma

- Español en dominio, UI, comentarios y specs. Cuidar **UTF-8** para evitar mojibake.
