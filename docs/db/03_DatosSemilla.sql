/* ============================================================================
   INTEGRA_CNP — Script 3 de 3: DATOS SEMILLA
   ----------------------------------------------------------------------------
   Sistema:  Justificacion de Marca (INTEGRA_CNP) — CNP / FANAL
   Fecha:    2026-06-24
   Motor:    SQL Server 2022
   Requiere: 01_CrearBaseDatos.sql y 02_EstructuraCompleta.sql ejecutados antes.

   CONTENIDO
     SECCION A — Catalogos del sistema     [OBLIGATORIO en todos los entornos]
     SECCION B — Demo minimo (unidad 120)  [SOLO dev/demo]
     SECCION C — Jerarquia de dependencias [SOLO dev/demo]
     SECCION D — Remediacion de mojibake    [OPCIONAL, idempotente]

   En PRODUCCION ejecutar UNICAMENTE la Seccion A. Las secciones B/C generan
   usuarios y boletas de prueba; la D corrige textos ya almacenados.

   IDEMPOTENCIA
     Catalogos via MERGE / IF NOT EXISTS; demos via upsert (UPDATE + INSERT NOT
     EXISTS). Re-ejecutable sin duplicar.

   CODIFICACION
     Ejecutar en UTF-8 (SSMS o sqlcmd -f 65001) para preservar acentos.
   ============================================================================ */

IF DB_ID('INTEGRA_CNP') IS NULL
BEGIN
    RAISERROR('La base INTEGRA_CNP no existe. Ejecute 01 y 02 primero.', 16, 1);
    SET NOEXEC ON;
END;
GO

USE INTEGRA_CNP;
GO

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ############################################################################
   SECCION A — CATALOGOS DEL SISTEMA   [OBLIGATORIO]
   ############################################################################ */

/* Roles (IDs fijos consumidos por el backend: 1=Func, 2=Jefe, 3=RRHH, 4=Admin). */
MERGE INTO Configuracion.Rol AS tgt
USING (
    SELECT 1 AS RolId, 'Funcionario'   AS Descripcion
    UNION ALL SELECT 2, 'Jefatura'
    UNION ALL SELECT 3, 'RRHH'
    UNION ALL SELECT 4, 'Administrador'
) AS src
ON tgt.RolId = src.RolId
WHEN NOT MATCHED THEN
    INSERT (RolId, Descripcion, CreadoPor) VALUES (src.RolId, src.Descripcion, 'seed');
GO

/* Estados de la justificacion (1=Pendiente Jefatura, 2=Aprobada, 3=Rechazada). */
MERGE INTO Configuracion.EstadoJustificacion AS tgt
USING (
    SELECT 1 AS EstadoJustificacionId, 'Pendiente Jefatura' AS Descripcion
    UNION ALL SELECT 2, 'Aprobada'
    UNION ALL SELECT 3, 'Rechazada'
) AS src
ON tgt.EstadoJustificacionId = src.EstadoJustificacionId
WHEN NOT MATCHED THEN
    INSERT (EstadoJustificacionId, Descripcion, CreadoPor) VALUES (src.EstadoJustificacionId, src.Descripcion, 'seed');
GO

/* Tipos de marca (catalogo extensible con IDENTITY: sembrar solo si esta vacio). */
IF NOT EXISTS (SELECT 1 FROM Configuracion.TipoJustificacion)
BEGIN
    INSERT INTO Configuracion.TipoJustificacion (Descripcion, CreadoPor)
    VALUES
        ('Marca Tardía',                'seed'),
        ('Omisión Marca de Entrada',    'seed'),
        ('Omisión Marca de Salida',     'seed'),
        ('Marca antes Hora de Salida',  'seed'),
        ('Ausencia',                    'seed');
END;
GO

/* Estado de registro para baja logica (1=Activo, 2=Inactivo). */
MERGE INTO Configuracion.EstadoRegistro AS tgt
USING (
    SELECT 1 AS EstadoRegistroId, 'Activo' AS Descripcion
    UNION ALL SELECT 2, 'Inactivo'
) AS src
ON tgt.EstadoRegistroId = src.EstadoRegistroId
WHEN NOT MATCHED THEN
    INSERT (EstadoRegistroId, Descripcion, CreadoPor) VALUES (src.EstadoRegistroId, src.Descripcion, 'seed');
GO

/* Tipos de evento de auditoria (1..7 funcionales + 8..11 acciones admin). */
MERGE INTO Configuracion.TipoEventoAuditoria AS tgt
USING (
    SELECT 1  AS TipoEventoAuditoriaId, 'CreacionJustificacion'                          AS Descripcion
    UNION ALL SELECT 2,  'ResolucionJustificacionAprobada'
    UNION ALL SELECT 3,  'ResolucionJustificacionRechazada'
    UNION ALL SELECT 4,  'AltaJerarquia'
    UNION ALL SELECT 5,  'CambioEstadoJerarquia'
    UNION ALL SELECT 6,  'AltaDelegacion'
    UNION ALL SELECT 7,  'CambioEstadoDelegacion'
    UNION ALL SELECT 8,  'ActualizacionJerarquia'
    UNION ALL SELECT 9,  'ActualizacionDelegacion'
    UNION ALL SELECT 10, 'ActualizacionAsignacionUsuarioODependencia'
    UNION ALL SELECT 11, 'CambioEstadoUsuario'
) AS src
ON tgt.TipoEventoAuditoriaId = src.TipoEventoAuditoriaId
WHEN NOT MATCHED THEN
    INSERT (TipoEventoAuditoriaId, Descripcion, CreadoPor) VALUES (src.TipoEventoAuditoriaId, src.Descripcion, 'seed');
GO

/* Resultados de auditoria (1=Exito, 2=Fallo, 3=Denegado). */
MERGE INTO Configuracion.ResultadoAuditoria AS tgt
USING (
    SELECT 1 AS ResultadoAuditoriaId, 'Exito' AS Descripcion
    UNION ALL SELECT 2, 'Fallo'
    UNION ALL SELECT 3, 'Denegado'
) AS src
ON tgt.ResultadoAuditoriaId = src.ResultadoAuditoriaId
WHEN NOT MATCHED THEN
    INSERT (ResultadoAuditoriaId, Descripcion, CreadoPor) VALUES (src.ResultadoAuditoriaId, src.Descripcion, 'seed');
GO

PRINT 'Seccion A completada: catalogos sembrados.';
GO

/* ############################################################################
   SECCION B — DEMO MINIMO (unidad 120)   [SOLO dev/demo]
   Jefe + 2 funcionarios + RRHH + 1 jerarquia vertical + 2 boletas pendientes.
   Sirve para validar rapido el flujo de jefatura sin la jerarquia completa.
   ############################################################################ */

BEGIN TRAN;

IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.EstructuraOrganizacional WHERE CodigoOrigen = '120')
    INSERT INTO RecursosHumanos.EstructuraOrganizacional
        (Nombre, CodigoOrigen, EstructuraPadreId, EstadoRegistroId, VigenciaDesde, VigenciaHasta, CreadoPor, FechaHoraCreacion)
    VALUES
        ('Unidad Demo CNP', '120', NULL, 1, DATEADD(DAY,-30,SYSUTCDATETIME()), NULL, 'seed_demo', SYSUTCDATETIME());

DECLARE @EstrID INT = (SELECT TOP 1 EstructuraOrganizacionalId FROM RecursosHumanos.EstructuraOrganizacional
                       WHERE CodigoOrigen = '120' ORDER BY EstructuraOrganizacionalId DESC);

IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'JEFE-DEMO')
    INSERT INTO RecursosHumanos.Usuario (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    VALUES ('JEFE-DEMO', 'jefe.maria', 'jefe.maria@cnp.local', NULL, 120, 2, 'CNP', 'seed_demo', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'ANA-DEMO')
    INSERT INTO RecursosHumanos.Usuario (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    VALUES ('ANA-DEMO', 'Ana Funcionaria Demo', 'ana.demo@cnp.local', NULL, 120, 1, 'CNP', 'seed_demo', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'LUIS-DEMO')
    INSERT INTO RecursosHumanos.Usuario (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    VALUES ('LUIS-DEMO', 'Luis Tecnico Demo', 'luis.demo@cnp.local', NULL, 120, 1, 'CNP', 'seed_demo', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'RRHH-DEMO')
    INSERT INTO RecursosHumanos.Usuario (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    VALUES ('RRHH-DEMO', 'rrhh.carlos', 'rrhh.carlos@cnp.local', NULL, 120, 3, 'CNP', 'seed_demo', SYSUTCDATETIME());

DECLARE @JefeID INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = 'JEFE-DEMO');
DECLARE @AnaID  INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = 'ANA-DEMO');
DECLARE @LuisID INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = 'LUIS-DEMO');

UPDATE RecursosHumanos.Usuario SET JefaturaId = @JefeID
WHERE Cedula IN ('ANA-DEMO','LUIS-DEMO') AND (JefaturaId IS NULL OR JefaturaId <> @JefeID);

IF NOT EXISTS (SELECT 1 FROM Operacion.JerarquiaAprobacion
               WHERE AprobadorUsuarioId = @JefeID AND EstructuraOrganizacionalId = @EstrID AND EstadoRegistroId = 1)
    INSERT INTO Operacion.JerarquiaAprobacion
        (AprobadorUsuarioId, EstructuraOrganizacionalId, NivelAprobacion, TipoRelacion, EstadoRegistroId, VigenciaDesde, VigenciaHasta, CreadoPor, FechaHoraCreacion)
    VALUES
        (@JefeID, @EstrID, 1, 'Vertical', 1, DATEADD(DAY,-30,SYSUTCDATETIME()), NULL, 'seed_demo', SYSUTCDATETIME());

IF NOT EXISTS (SELECT 1 FROM Operacion.Justificacion
               WHERE UsuarioId = @AnaID AND MotivoGeneral = 'Tardanza por bloqueo vial ruta 27' AND CreadoPor = 'seed_demo')
BEGIN
    DECLARE @J1 INT;
    INSERT INTO Operacion.Justificacion (UsuarioId, MotivoGeneral, EstadoJustificacionId, CreadoPor, FechaHoraCreacion, FechaCreacion)
    VALUES (@AnaID, 'Tardanza por bloqueo vial ruta 27', 1, 'seed_demo', SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @J1 = CAST(SCOPE_IDENTITY() AS INT);
    INSERT INTO Operacion.JustificacionDetalle (JustificacionId, TipoJustificacionId, FechaMarca, ObservacionDetalle, CreadoPor, FechaHoraCreacion)
    VALUES (@J1, 1, CAST(DATEADD(DAY,-1,GETDATE()) AS date), 'Ingreso 08:22, accidente autopista', 'seed_demo', SYSUTCDATETIME());
END;

IF NOT EXISTS (SELECT 1 FROM Operacion.Justificacion
               WHERE UsuarioId = @LuisID AND MotivoGeneral = 'Omision de marca por visita tecnica' AND CreadoPor = 'seed_demo')
BEGIN
    DECLARE @J2 INT;
    INSERT INTO Operacion.Justificacion (UsuarioId, MotivoGeneral, EstadoJustificacionId, CreadoPor, FechaHoraCreacion, FechaCreacion)
    VALUES (@LuisID, 'Omision de marca por visita tecnica', 1, 'seed_demo', SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @J2 = CAST(SCOPE_IDENTITY() AS INT);
    INSERT INTO Operacion.JustificacionDetalle (JustificacionId, TipoJustificacionId, FechaMarca, ObservacionDetalle, CreadoPor, FechaHoraCreacion)
    VALUES (@J2, 3, CAST(DATEADD(DAY,-2,GETDATE()) AS date), 'No registro salida al retornar de visita externa', 'seed_demo', SYSUTCDATETIME());
END;

COMMIT TRAN;
PRINT 'Seccion B completada: demo minimo (unidad 120).';
GO

/* ############################################################################
   SECCION C — JERARQUIA DE DEPENDENCIAS   [SOLO dev/demo]
   12 dependencias, 1 jefe + 5 funcionarios por dependencia, y jerarquia vertical
   (la jefatura superior aprueba a la jefatura subordinada).
   ############################################################################ */

BEGIN TRY
    BEGIN TRAN;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    IF OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante', 'IF') IS NULL
       AND OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante', 'TF') IS NULL
       AND OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante', 'FN') IS NULL
        THROW 50004, 'Falta funcion dbo.fn_AprobadoresVigentesPorSolicitante (ejecute 02).', 1;

    DECLARE @Dependencias TABLE
    (
        Orden INT NOT NULL, Codigo INT NOT NULL PRIMARY KEY, Nombre VARCHAR(150) NOT NULL,
        CodigoPadre INT NULL, JefeCedula VARCHAR(64) NOT NULL, JefeNombre VARCHAR(150) NOT NULL, JefeCorreo VARCHAR(100) NOT NULL
    );

    INSERT INTO @Dependencias (Orden, Codigo, Nombre, CodigoPadre, JefeCedula, JefeNombre, JefeCorreo)
    VALUES
        (1, 5100, 'Gerencia', NULL, '1-5100-0001', 'Laura Cascante Rojas', 'laura.cascante@cnp.local'),
        (2, 5110, 'Jefatura de UTI', 5100, '1-5110-0001', 'Mauricio Solano Vega', 'mauricio.solano@cnp.local'),
        (3, 5120, 'UTI', 5110, '1-5120-0001', 'Adriana Fallas Monge', 'adriana.fallas@cnp.local'),
        (4, 5130, 'Unidad de Tecnologias de Informacion', 5120, '1-5130-0001', 'Esteban Matarrita Campos', 'esteban.matarrita@cnp.local'),
        (5, 5140, 'Jefatura de Subgerencia', 5100, '1-5140-0001', 'Daniela Quesada Brenes', 'daniela.quesada@cnp.local'),
        (6, 5150, 'Subgerencia', 5140, '1-5150-0001', 'Ricardo Segura Jimenez', 'ricardo.segura@cnp.local'),
        (7, 5160, 'Jefatura de Programas Especiales', 5150, '1-5160-0001', 'Fernanda Urena Solis', 'fernanda.urena@cnp.local'),
        (8, 5200, 'DAF', NULL, '1-5200-0001', 'Oscar Alpizar Rojas', 'oscar.alpizar@cnp.local'),
        (9, 5210, 'Direccion Administrativa Financiera', 5200, '1-5210-0001', 'Melissa Chinchilla Pineda', 'melissa.chinchilla@cnp.local'),
        (10, 5220, 'Recursos Humanos', 5210, '1-5220-0001', 'Karla Araya Vargas', 'karla.araya@cnp.local'),
        (11, 5230, 'Contabilidad', 5210, '1-5230-0001', 'Joaquin Cordero Mendez', 'joaquin.cordero@cnp.local'),
        (12, 5240, 'Proveeduria', 5210, '1-5240-0001', 'Pablo Villalobos Mora', 'pablo.villalobos@cnp.local');

    DECLARE @OffsetCodigoJefatura INT = 10000;

    DECLARE @EstructurasEsperadas TABLE
    (
        CodigoEstructura INT NOT NULL PRIMARY KEY, NombreEstructura VARCHAR(150) NOT NULL,
        CodigoPadreEstructura INT NULL, CodigoDependencia INT NOT NULL, TipoNodo VARCHAR(16) NOT NULL
    );

    -- Nodo operativo (todas) + nodo de jefatura (solo dependencias no raiz).
    INSERT INTO @EstructurasEsperadas (CodigoEstructura, NombreEstructura, CodigoPadreEstructura, CodigoDependencia, TipoNodo)
    SELECT d.Codigo, d.Nombre, d.CodigoPadre, d.Codigo, 'OPERATIVA' FROM @Dependencias d;

    INSERT INTO @EstructurasEsperadas (CodigoEstructura, NombreEstructura, CodigoPadreEstructura, CodigoDependencia, TipoNodo)
    SELECT d.Codigo + @OffsetCodigoJefatura, CONCAT(d.Nombre, ' - Jefatura'), d.Codigo, d.Codigo, 'JEFATURA'
    FROM @Dependencias d WHERE d.CodigoPadre IS NOT NULL;

    -- 1) Upsert de estructuras por CodigoOrigen
    UPDATE eo
    SET eo.Nombre = eesp.NombreEstructura, eo.EstadoRegistroId = 1,
        eo.VigenciaDesde = COALESCE(eo.VigenciaDesde, DATEADD(DAY, -30, @Now)), eo.VigenciaHasta = NULL
    FROM RecursosHumanos.EstructuraOrganizacional eo
    INNER JOIN @EstructurasEsperadas eesp ON eo.CodigoOrigen = CAST(eesp.CodigoEstructura AS VARCHAR(50));

    INSERT INTO RecursosHumanos.EstructuraOrganizacional
        (Nombre, CodigoOrigen, EstructuraPadreId, EstadoRegistroId, VigenciaDesde, VigenciaHasta, CreadoPor, FechaHoraCreacion)
    SELECT eesp.NombreEstructura, CAST(eesp.CodigoEstructura AS VARCHAR(50)), NULL, 1, DATEADD(DAY, -30, @Now), NULL, 'seed_hierarquia_dependencias', @Now
    FROM @EstructurasEsperadas eesp
    WHERE NOT EXISTS (SELECT 1 FROM RecursosHumanos.EstructuraOrganizacional eo
                      WHERE eo.CodigoOrigen = CAST(eesp.CodigoEstructura AS VARCHAR(50)));

    -- 2) Resolver jerarquia padre/hijo de estructuras
    UPDATE child
    SET child.EstructuraPadreId = parent.EstructuraOrganizacionalId, child.EstadoRegistroId = 1, child.VigenciaHasta = NULL
    FROM RecursosHumanos.EstructuraOrganizacional child
    INNER JOIN @EstructurasEsperadas eesp ON child.CodigoOrigen = CAST(eesp.CodigoEstructura AS VARCHAR(50))
    INNER JOIN RecursosHumanos.EstructuraOrganizacional parent ON parent.CodigoOrigen = CAST(eesp.CodigoPadreEstructura AS VARCHAR(50))
    WHERE eesp.CodigoPadreEstructura IS NOT NULL;

    UPDATE rootNode
    SET rootNode.EstructuraPadreId = NULL, rootNode.EstadoRegistroId = 1, rootNode.VigenciaHasta = NULL
    FROM RecursosHumanos.EstructuraOrganizacional rootNode
    INNER JOIN @EstructurasEsperadas eesp ON rootNode.CodigoOrigen = CAST(eesp.CodigoEstructura AS VARCHAR(50))
    WHERE eesp.CodigoPadreEstructura IS NULL;

    -- 3) Upsert de jefaturas (1 por dependencia)
    UPDATE u
    SET u.NombreCompleto = d.JefeNombre, u.CorreoElectronico = d.JefeCorreo,
        u.UnidadId = CASE WHEN d.CodigoPadre IS NULL THEN d.Codigo ELSE d.Codigo + @OffsetCodigoJefatura END,
        u.RolId = 2, u.Compania = 'CNP', u.EsActivo = 1
    FROM RecursosHumanos.Usuario u INNER JOIN @Dependencias d ON d.JefeCedula = u.Cedula;

    INSERT INTO RecursosHumanos.Usuario (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    SELECT d.JefeCedula, d.JefeNombre, d.JefeCorreo, NULL,
           CASE WHEN d.CodigoPadre IS NULL THEN d.Codigo ELSE d.Codigo + @OffsetCodigoJefatura END,
           2, 'CNP', 'seed_hierarquia_dependencias', @Now
    FROM @Dependencias d
    WHERE NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario u WHERE u.Cedula = d.JefeCedula);

    -- 4) Generar y upsert de 5 funcionarios por dependencia
    DECLARE @NombrePool TABLE (PoolId INT NOT NULL PRIMARY KEY, NombreCompleto VARCHAR(150) NOT NULL, CorreoBase VARCHAR(80) NOT NULL);
    INSERT INTO @NombrePool (PoolId, NombreCompleto, CorreoBase)
    VALUES
        (1, 'Andrea Rojas Mena', 'andrea.rojas'), (2, 'Jose Pablo Villalta Ruiz', 'jose.villalta'),
        (3, 'Monica Cespedes Salas', 'monica.cespedes'), (4, 'Diego Badilla Murillo', 'diego.badilla'),
        (5, 'Gabriela Nunez Coto', 'gabriela.nunez'), (6, 'Javier Montero Castro', 'javier.montero'),
        (7, 'Natalia Carvajal Arias', 'natalia.carvajal'), (8, 'Cristian Calderon Soto', 'cristian.calderon'),
        (9, 'Mariana Obando Brenes', 'mariana.obando'), (10, 'Luis Diego Chaves Campos', 'luis.chaves');

    DECLARE @Funcionarios TABLE
    (
        Cedula VARCHAR(64) NOT NULL PRIMARY KEY, NombreCompleto VARCHAR(150) NOT NULL,
        CorreoElectronico VARCHAR(100) NOT NULL, UnidadId INT NOT NULL, JefeCedula VARCHAR(64) NOT NULL
    );

    ;WITH N AS (SELECT 1 AS Num UNION ALL SELECT 2 UNION ALL SELECT 3 UNION ALL SELECT 4 UNION ALL SELECT 5)
    INSERT INTO @Funcionarios (Cedula, NombreCompleto, CorreoElectronico, UnidadId, JefeCedula)
    SELECT
        CONCAT('2-', d.Codigo, '-', RIGHT(CONCAT('0000', N.Num), 4)),
        p.NombreCompleto,
        CONCAT(p.CorreoBase, '.', d.Codigo, '.', RIGHT(CONCAT('00', N.Num), 2), '@cnp.local'),
        d.Codigo, d.JefeCedula
    FROM @Dependencias d
    CROSS JOIN N
    INNER JOIN @NombrePool p ON p.PoolId = ((d.Orden + N.Num - 2) % 10) + 1;

    UPDATE u
    SET u.NombreCompleto = f.NombreCompleto, u.CorreoElectronico = f.CorreoElectronico,
        u.UnidadId = f.UnidadId, u.RolId = 1, u.Compania = 'CNP', u.EsActivo = 1
    FROM RecursosHumanos.Usuario u INNER JOIN @Funcionarios f ON f.Cedula = u.Cedula;

    INSERT INTO RecursosHumanos.Usuario (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    SELECT f.Cedula, f.NombreCompleto, f.CorreoElectronico, NULL, f.UnidadId, 1, 'CNP', 'seed_hierarquia_dependencias', @Now
    FROM @Funcionarios f
    WHERE NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario u WHERE u.Cedula = f.Cedula);

    -- 5) Jefatura de funcionarios = jefe de su dependencia
    UPDATE fu
    SET fu.JefaturaId = jefe.UsuarioId
    FROM RecursosHumanos.Usuario fu
    INNER JOIN @Funcionarios f ON fu.Cedula = f.Cedula
    INNER JOIN RecursosHumanos.Usuario jefe ON jefe.Cedula = f.JefeCedula
    WHERE ISNULL(fu.JefaturaId, 0) <> jefe.UsuarioId;

    -- 6) Jefatura de jefaturas subordinadas = jefe de la dependencia padre
    UPDATE jefeActual
    SET jefeActual.JefaturaId = jefeSuperior.UsuarioId
    FROM RecursosHumanos.Usuario jefeActual
    INNER JOIN @Dependencias d ON d.JefeCedula = jefeActual.Cedula
    INNER JOIN @Dependencias dp ON dp.Codigo = d.CodigoPadre
    INNER JOIN RecursosHumanos.Usuario jefeSuperior ON jefeSuperior.Cedula = dp.JefeCedula
    WHERE ISNULL(jefeActual.JefaturaId, 0) <> jefeSuperior.UsuarioId;

    UPDATE jefeRaiz
    SET jefeRaiz.JefaturaId = NULL
    FROM RecursosHumanos.Usuario jefeRaiz
    INNER JOIN @Dependencias d ON d.JefeCedula = jefeRaiz.Cedula
    WHERE d.CodigoPadre IS NULL AND jefeRaiz.JefaturaId IS NOT NULL;

    -- 7) Upsert jerarquia de aprobacion (operativo -> jefe propio; jefatura -> jefe padre)
    DECLARE @JerarquiaEsperada TABLE (EstructuraOrganizacionalId INT NOT NULL PRIMARY KEY, AprobadorUsuarioId INT NOT NULL);

    INSERT INTO @JerarquiaEsperada (EstructuraOrganizacionalId, AprobadorUsuarioId)
    SELECT e.EstructuraOrganizacionalId, jefePropio.UsuarioId
    FROM @Dependencias d
    INNER JOIN RecursosHumanos.EstructuraOrganizacional e ON e.CodigoOrigen = CAST(d.Codigo AS VARCHAR(50))
    INNER JOIN RecursosHumanos.Usuario jefePropio ON jefePropio.Cedula = d.JefeCedula;

    INSERT INTO @JerarquiaEsperada (EstructuraOrganizacionalId, AprobadorUsuarioId)
    SELECT eJef.EstructuraOrganizacionalId, jefePadre.UsuarioId
    FROM @Dependencias d
    INNER JOIN @Dependencias dp ON dp.Codigo = d.CodigoPadre
    INNER JOIN RecursosHumanos.EstructuraOrganizacional eJef ON eJef.CodigoOrigen = CAST(d.Codigo + @OffsetCodigoJefatura AS VARCHAR(50))
    INNER JOIN RecursosHumanos.Usuario jefePadre ON jefePadre.Cedula = dp.JefeCedula;

    MERGE Operacion.JerarquiaAprobacion AS tgt
    USING @JerarquiaEsperada AS src
      ON tgt.EstructuraOrganizacionalId = src.EstructuraOrganizacionalId
     AND tgt.NivelAprobacion = 1 AND tgt.TipoRelacion = 'Vertical'
     AND tgt.EstadoRegistroId = 1 AND tgt.CreadoPor = 'seed_hierarquia_dependencias'
    WHEN MATCHED THEN
        UPDATE SET tgt.AprobadorUsuarioId = src.AprobadorUsuarioId,
                   tgt.VigenciaDesde = COALESCE(tgt.VigenciaDesde, DATEADD(DAY, -30, @Now)),
                   tgt.VigenciaHasta = NULL
    WHEN NOT MATCHED THEN
        INSERT (AprobadorUsuarioId, EstructuraOrganizacionalId, NivelAprobacion, TipoRelacion, EstadoRegistroId, VigenciaDesde, VigenciaHasta, CreadoPor, FechaHoraCreacion)
        VALUES (src.AprobadorUsuarioId, src.EstructuraOrganizacionalId, 1, 'Vertical', 1, DATEADD(DAY, -30, @Now), NULL, 'seed_hierarquia_dependencias', @Now);

    COMMIT TRAN;
    PRINT 'Seccion C completada: jerarquia de 12 dependencias.';
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0 ROLLBACK TRAN;
    THROW;
END CATCH;
GO

/* ############################################################################
   SECCION D — REMEDIACION DE MOJIBAKE   [OPCIONAL, idempotente]
   Corrige textos UTF-8-como-Latin1 ('Ã³'->'ó', etc.) ya almacenados. En una BD
   sembrada limpia es un no-op (los WHERE no encuentran patrones). Ejecutar solo
   si se detecta mojibake en datos historicos.
   ############################################################################ */

-- Correcciones puntuales de catalogo
UPDATE Configuracion.TipoJustificacion SET Descripcion = 'Omisión' WHERE Descripcion = 'OmisiÃ³n';
UPDATE Configuracion.TipoJustificacion SET Descripcion = 'Comisión' WHERE Descripcion = 'ComisiÃ³n';
UPDATE Configuracion.TipoJustificacion SET Descripcion = 'Reunión'  WHERE Descripcion = 'ReuniÃ³n';
GO

-- Normalizacion de textos operativos (idempotente)
UPDATE Operacion.Justificacion
SET MotivoGeneral = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(MotivoGeneral,
  'Ã¡','á'),'Ã©','é'),'Ã­','í'),'Ã³','ó'),'Ãº','ú'),
  'Ã','Á'),'Ã‰','É'),'Ã','Í'),'Ã“','Ó'),'Ãš','Ú'),
  'Ã±','ñ'),'Ã‘','Ñ'),'Ã¼','ü'),'Ãœ','Ü'),
  'Â¿','¿'),'Â¡','¡'),'Â°','°'),'Â',''),'�',''), CHAR(194),''))
WHERE MotivoGeneral LIKE '%Ã%' OR MotivoGeneral LIKE '%Â%';

UPDATE Operacion.Justificacion
SET ComentarioResolucion = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ComentarioResolucion,
  'Ã¡','á'),'Ã©','é'),'Ã­','í'),'Ã³','ó'),'Ãº','ú'),
  'Ã','Á'),'Ã‰','É'),'Ã','Í'),'Ã“','Ó'),'Ãš','Ú'),
  'Ã±','ñ'),'Ã‘','Ñ'),'Ã¼','ü'),'Ãœ','Ü'),
  'Â¿','¿'),'Â¡','¡'),'Â°','°'),'Â',''),'�',''), CHAR(194),''))
WHERE ComentarioResolucion LIKE '%Ã%' OR ComentarioResolucion LIKE '%Â%';

UPDATE Operacion.JustificacionDetalle
SET ObservacionDetalle = REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(REPLACE(ObservacionDetalle,
  'Ã¡','á'),'Ã©','é'),'Ã­','í'),'Ã³','ó'),'Ãº','ú'),
  'Ã','Á'),'Ã‰','É'),'Ã','Í'),'Ã“','Ó'),'Ãš','Ú'),
  'Ã±','ñ'),'Ã‘','Ñ'),'Ã¼','ü'),'Ãœ','Ü'),
  'Â¿','¿'),'Â¡','¡'),'Â°','°'),'Â',''),'�',''), CHAR(194),''))
WHERE ObservacionDetalle LIKE '%Ã%' OR ObservacionDetalle LIKE '%Â%';
GO

SET NOEXEC OFF;
GO
PRINT 'Script 03 completado: datos semilla aplicados.';
GO
