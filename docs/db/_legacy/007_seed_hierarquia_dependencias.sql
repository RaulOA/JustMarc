-- Seed de jerarquia de dependencias y personal demo en esquema funcional.
-- Objetivo: 12 dependencias, 1 jefe + 5 funcionarios por dependencia,
-- y regla de aprobacion vertical (jefatura superior aprueba jefatura subordinada).

USE INTEGRA_CNP;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRAN;

    DECLARE @Now DATETIME2 = SYSUTCDATETIME();

    -- Prechecks de objetos requeridos
    IF OBJECT_ID('RecursosHumanos.EstructuraOrganizacional', 'U') IS NULL
        THROW 50001, 'Falta tabla RecursosHumanos.EstructuraOrganizacional.', 1;

    IF OBJECT_ID('RecursosHumanos.Usuario', 'U') IS NULL
        THROW 50002, 'Falta tabla RecursosHumanos.Usuario.', 1;

    IF OBJECT_ID('Operacion.JerarquiaAprobacion', 'U') IS NULL
        THROW 50003, 'Falta tabla Operacion.JerarquiaAprobacion.', 1;

    IF OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante', 'IF') IS NULL
       AND OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante', 'TF') IS NULL
       AND OBJECT_ID('dbo.fn_AprobadoresVigentesPorSolicitante', 'FN') IS NULL
        THROW 50004, 'Falta funcion dbo.fn_AprobadoresVigentesPorSolicitante.', 1;

    DECLARE @Dependencias TABLE
    (
        Orden INT NOT NULL,
        Codigo INT NOT NULL PRIMARY KEY,
        Nombre VARCHAR(150) NOT NULL,
        CodigoPadre INT NULL,
        JefeCedula VARCHAR(64) NOT NULL,
        JefeNombre VARCHAR(150) NOT NULL,
        JefeCorreo VARCHAR(100) NOT NULL
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
        CodigoEstructura INT NOT NULL PRIMARY KEY,
        NombreEstructura VARCHAR(150) NOT NULL,
        CodigoPadreEstructura INT NULL,
        CodigoDependencia INT NOT NULL,
        TipoNodo VARCHAR(16) NOT NULL
    );

    -- Nodo operativo (todos) y nodo de jefatura (solo dependencias no raiz).
    INSERT INTO @EstructurasEsperadas (CodigoEstructura, NombreEstructura, CodigoPadreEstructura, CodigoDependencia, TipoNodo)
    SELECT
        d.Codigo,
        d.Nombre,
        d.CodigoPadre,
        d.Codigo,
        'OPERATIVA'
    FROM @Dependencias d;

    INSERT INTO @EstructurasEsperadas (CodigoEstructura, NombreEstructura, CodigoPadreEstructura, CodigoDependencia, TipoNodo)
    SELECT
        d.Codigo + @OffsetCodigoJefatura,
        CONCAT(d.Nombre, ' - Jefatura'),
        d.Codigo,
        d.Codigo,
        'JEFATURA'
    FROM @Dependencias d
    WHERE d.CodigoPadre IS NOT NULL;

    -- 1) Upsert de estructuras por CodigoOrigen
    UPDATE eo
    SET eo.Nombre = eesp.NombreEstructura,
        eo.EstadoRegistroId = 1,
        eo.VigenciaDesde = COALESCE(eo.VigenciaDesde, DATEADD(DAY, -30, @Now)),
        eo.VigenciaHasta = NULL
    FROM RecursosHumanos.EstructuraOrganizacional eo
    INNER JOIN @EstructurasEsperadas eesp
        ON eo.CodigoOrigen = CAST(eesp.CodigoEstructura AS VARCHAR(50));

    INSERT INTO RecursosHumanos.EstructuraOrganizacional
    (
        Nombre,
        CodigoOrigen,
        EstructuraPadreId,
        EstadoRegistroId,
        VigenciaDesde,
        VigenciaHasta,
        CreadoPor,
        FechaHoraCreacion
    )
    SELECT
        eesp.NombreEstructura,
        CAST(eesp.CodigoEstructura AS VARCHAR(50)),
        NULL,
        1,
        DATEADD(DAY, -30, @Now),
        NULL,
        'seed_hierarquia_dependencias',
        @Now
    FROM @EstructurasEsperadas eesp
    WHERE NOT EXISTS (
        SELECT 1
        FROM RecursosHumanos.EstructuraOrganizacional eo
        WHERE eo.CodigoOrigen = CAST(eesp.CodigoEstructura AS VARCHAR(50))
    );

    -- 2) Resolver jerarquia padre/hijo
    UPDATE child
    SET child.EstructuraPadreId = parent.EstructuraOrganizacionalId,
        child.EstadoRegistroId = 1,
        child.VigenciaHasta = NULL
    FROM RecursosHumanos.EstructuraOrganizacional child
    INNER JOIN @EstructurasEsperadas eesp
        ON child.CodigoOrigen = CAST(eesp.CodigoEstructura AS VARCHAR(50))
    INNER JOIN RecursosHumanos.EstructuraOrganizacional parent
        ON parent.CodigoOrigen = CAST(eesp.CodigoPadreEstructura AS VARCHAR(50))
    WHERE eesp.CodigoPadreEstructura IS NOT NULL;

    UPDATE rootNode
    SET rootNode.EstructuraPadreId = NULL,
        rootNode.EstadoRegistroId = 1,
        rootNode.VigenciaHasta = NULL
    FROM RecursosHumanos.EstructuraOrganizacional rootNode
    INNER JOIN @EstructurasEsperadas eesp
        ON rootNode.CodigoOrigen = CAST(eesp.CodigoEstructura AS VARCHAR(50))
    WHERE eesp.CodigoPadreEstructura IS NULL;

    -- 3) Upsert de jefaturas (1 por dependencia)
    UPDATE u
    SET u.NombreCompleto = d.JefeNombre,
        u.CorreoElectronico = d.JefeCorreo,
        u.UnidadId = CASE WHEN d.CodigoPadre IS NULL THEN d.Codigo ELSE d.Codigo + @OffsetCodigoJefatura END,
        u.RolId = 2,
        u.Compania = 'CNP',
        u.EsActivo = 1
    FROM RecursosHumanos.Usuario u
    INNER JOIN @Dependencias d
        ON d.JefeCedula = u.Cedula;

    INSERT INTO RecursosHumanos.Usuario
    (
        Cedula,
        NombreCompleto,
        CorreoElectronico,
        JefaturaId,
        UnidadId,
        RolId,
        Compania,
        CreadoPor,
        FechaHoraCreacion
    )
    SELECT
        d.JefeCedula,
        d.JefeNombre,
        d.JefeCorreo,
        NULL,
        CASE WHEN d.CodigoPadre IS NULL THEN d.Codigo ELSE d.Codigo + @OffsetCodigoJefatura END,
        2,
        'CNP',
        'seed_hierarquia_dependencias',
        @Now
    FROM @Dependencias d
    WHERE NOT EXISTS (
        SELECT 1
        FROM RecursosHumanos.Usuario u
        WHERE u.Cedula = d.JefeCedula
    );

    -- 4) Generar y upsert de 5 funcionarios por dependencia
    DECLARE @NombrePool TABLE
    (
        PoolId INT NOT NULL PRIMARY KEY,
        NombreCompleto VARCHAR(150) NOT NULL,
        CorreoBase VARCHAR(80) NOT NULL
    );

    INSERT INTO @NombrePool (PoolId, NombreCompleto, CorreoBase)
    VALUES
        (1, 'Andrea Rojas Mena', 'andrea.rojas'),
        (2, 'Jose Pablo Villalta Ruiz', 'jose.villalta'),
        (3, 'Monica Cespedes Salas', 'monica.cespedes'),
        (4, 'Diego Badilla Murillo', 'diego.badilla'),
        (5, 'Gabriela Nunez Coto', 'gabriela.nunez'),
        (6, 'Javier Montero Castro', 'javier.montero'),
        (7, 'Natalia Carvajal Arias', 'natalia.carvajal'),
        (8, 'Cristian Calderon Soto', 'cristian.calderon'),
        (9, 'Mariana Obando Brenes', 'mariana.obando'),
        (10, 'Luis Diego Chaves Campos', 'luis.chaves');

    DECLARE @Funcionarios TABLE
    (
        Cedula VARCHAR(64) NOT NULL PRIMARY KEY,
        NombreCompleto VARCHAR(150) NOT NULL,
        CorreoElectronico VARCHAR(100) NOT NULL,
        UnidadId INT NOT NULL,
        JefeCedula VARCHAR(64) NOT NULL
    );

    ;WITH N AS
    (
        SELECT 1 AS Num
        UNION ALL SELECT 2
        UNION ALL SELECT 3
        UNION ALL SELECT 4
        UNION ALL SELECT 5
    )
    INSERT INTO @Funcionarios (Cedula, NombreCompleto, CorreoElectronico, UnidadId, JefeCedula)
    SELECT
        CONCAT('2-', d.Codigo, '-', RIGHT(CONCAT('0000', N.Num), 4)) AS Cedula,
        p.NombreCompleto,
        CONCAT(p.CorreoBase, '.', d.Codigo, '.', RIGHT(CONCAT('00', N.Num), 2), '@cnp.local') AS CorreoElectronico,
        d.Codigo AS UnidadId,
        d.JefeCedula
    FROM @Dependencias d
    CROSS JOIN N
    INNER JOIN @NombrePool p
        ON p.PoolId = ((d.Orden + N.Num - 2) % 10) + 1;

    UPDATE u
    SET u.NombreCompleto = f.NombreCompleto,
        u.CorreoElectronico = f.CorreoElectronico,
        u.UnidadId = f.UnidadId,
        u.RolId = 1,
        u.Compania = 'CNP',
        u.EsActivo = 1
    FROM RecursosHumanos.Usuario u
    INNER JOIN @Funcionarios f
        ON f.Cedula = u.Cedula;

    INSERT INTO RecursosHumanos.Usuario
    (
        Cedula,
        NombreCompleto,
        CorreoElectronico,
        JefaturaId,
        UnidadId,
        RolId,
        Compania,
        CreadoPor,
        FechaHoraCreacion
    )
    SELECT
        f.Cedula,
        f.NombreCompleto,
        f.CorreoElectronico,
        NULL,
        f.UnidadId,
        1,
        'CNP',
        'seed_hierarquia_dependencias',
        @Now
    FROM @Funcionarios f
    WHERE NOT EXISTS (
        SELECT 1
        FROM RecursosHumanos.Usuario u
        WHERE u.Cedula = f.Cedula
    );

    -- 5) Setear jefatura de funcionarios = jefe de su dependencia
    UPDATE fu
    SET fu.JefaturaId = jefe.UsuarioId
    FROM RecursosHumanos.Usuario fu
    INNER JOIN @Funcionarios f
        ON fu.Cedula = f.Cedula
    INNER JOIN RecursosHumanos.Usuario jefe
        ON jefe.Cedula = f.JefeCedula
    WHERE ISNULL(fu.JefaturaId, 0) <> jefe.UsuarioId;

    -- 6) Setear jefatura de jefaturas subordinadas = jefe de dependencia padre
    UPDATE jefeActual
    SET jefeActual.JefaturaId = jefeSuperior.UsuarioId
    FROM RecursosHumanos.Usuario jefeActual
    INNER JOIN @Dependencias d
        ON d.JefeCedula = jefeActual.Cedula
    INNER JOIN @Dependencias dp
        ON dp.Codigo = d.CodigoPadre
    INNER JOIN RecursosHumanos.Usuario jefeSuperior
        ON jefeSuperior.Cedula = dp.JefeCedula
    WHERE ISNULL(jefeActual.JefaturaId, 0) <> jefeSuperior.UsuarioId;

    -- Nodos raiz sin jefatura superior
    UPDATE jefeRaiz
    SET jefeRaiz.JefaturaId = NULL
    FROM RecursosHumanos.Usuario jefeRaiz
    INNER JOIN @Dependencias d
        ON d.JefeCedula = jefeRaiz.Cedula
    WHERE d.CodigoPadre IS NULL
      AND jefeRaiz.JefaturaId IS NOT NULL;

    -- 7) Upsert jerarquia de aprobacion:
    --    - Nodo operativo: aprueba jefe propio.
    --    - Nodo jefatura (no raiz): aprueba jefe de dependencia padre.
    DECLARE @JerarquiaEsperada TABLE
    (
        EstructuraOrganizacionalId INT NOT NULL PRIMARY KEY,
        AprobadorUsuarioId INT NOT NULL
    );

    INSERT INTO @JerarquiaEsperada (EstructuraOrganizacionalId, AprobadorUsuarioId)
    SELECT
        e.EstructuraOrganizacionalId,
        jefePropio.UsuarioId AS AprobadorUsuarioId
    FROM @Dependencias d
    INNER JOIN RecursosHumanos.EstructuraOrganizacional e
        ON e.CodigoOrigen = CAST(d.Codigo AS VARCHAR(50))
    INNER JOIN RecursosHumanos.Usuario jefePropio
        ON jefePropio.Cedula = d.JefeCedula;

    INSERT INTO @JerarquiaEsperada (EstructuraOrganizacionalId, AprobadorUsuarioId)
    SELECT
        eJef.EstructuraOrganizacionalId,
        jefePadre.UsuarioId AS AprobadorUsuarioId
    FROM @Dependencias d
    INNER JOIN @Dependencias dp
        ON dp.Codigo = d.CodigoPadre
    INNER JOIN RecursosHumanos.EstructuraOrganizacional eJef
        ON eJef.CodigoOrigen = CAST(d.Codigo + @OffsetCodigoJefatura AS VARCHAR(50))
    INNER JOIN RecursosHumanos.Usuario jefePadre
        ON jefePadre.Cedula = dp.JefeCedula;

    MERGE Operacion.JerarquiaAprobacion AS tgt
    USING @JerarquiaEsperada AS src
      ON tgt.EstructuraOrganizacionalId = src.EstructuraOrganizacionalId
     AND tgt.NivelAprobacion = 1
     AND tgt.TipoRelacion = 'Vertical'
     AND tgt.EstadoRegistroId = 1
     AND tgt.CreadoPor = 'seed_hierarquia_dependencias'
    WHEN MATCHED THEN
        UPDATE SET
            tgt.AprobadorUsuarioId = src.AprobadorUsuarioId,
            tgt.VigenciaDesde = COALESCE(tgt.VigenciaDesde, DATEADD(DAY, -30, @Now)),
            tgt.VigenciaHasta = NULL
    WHEN NOT MATCHED THEN
        INSERT
        (
            AprobadorUsuarioId,
            EstructuraOrganizacionalId,
            NivelAprobacion,
            TipoRelacion,
            EstadoRegistroId,
            VigenciaDesde,
            VigenciaHasta,
            CreadoPor,
            FechaHoraCreacion
        )
        VALUES
        (
            src.AprobadorUsuarioId,
            src.EstructuraOrganizacionalId,
            1,
            'Vertical',
            1,
            DATEADD(DAY, -30, @Now),
            NULL,
            'seed_hierarquia_dependencias',
            @Now
        );

    COMMIT TRAN;

    -- Validaciones rapidas
    SELECT
        COUNT(*) AS EstructurasSembradas
    FROM RecursosHumanos.EstructuraOrganizacional eo
    INNER JOIN @Dependencias d
        ON eo.CodigoOrigen = CAST(d.Codigo AS VARCHAR(50));

    SELECT
        SUM(CASE WHEN u.RolId = 2 THEN 1 ELSE 0 END) AS JefesSembrados,
        SUM(CASE WHEN u.RolId = 1 THEN 1 ELSE 0 END) AS FuncionariosSembrados
    FROM RecursosHumanos.Usuario u
    WHERE u.Cedula IN (SELECT JefeCedula FROM @Dependencias)
       OR u.Cedula IN (SELECT Cedula FROM @Funcionarios);

    SELECT
        COUNT(*) AS JerarquiasActivasSeed
    FROM Operacion.JerarquiaAprobacion ja
    INNER JOIN @JerarquiaEsperada je
        ON je.EstructuraOrganizacionalId = ja.EstructuraOrganizacionalId
       AND je.AprobadorUsuarioId = ja.AprobadorUsuarioId
    WHERE ja.EstadoRegistroId = 1
      AND ja.NivelAprobacion = 1
      AND ja.TipoRelacion = 'Vertical';

    SELECT
        d.Codigo,
        d.Nombre,
        d.CodigoPadre,
        p.Nombre AS DependenciaPadre,
        jefe.NombreCompleto AS JefeDependencia,
        jefeSuperior.NombreCompleto AS JefeDependenciaPadre
    FROM @Dependencias d
    LEFT JOIN @Dependencias p
        ON p.Codigo = d.CodigoPadre
    LEFT JOIN RecursosHumanos.Usuario jefe
        ON jefe.Cedula = d.JefeCedula
    LEFT JOIN RecursosHumanos.Usuario jefeSuperior
        ON jefeSuperior.Cedula = p.JefeCedula
    ORDER BY d.Orden;

    -- Casos sugeridos para validar la funcion de aprobadores por rama
    DECLARE @UsuarioFuncUti INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '2-5130-0001');
    DECLARE @UsuarioJefeUti INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '1-5130-0001');
    DECLARE @UsuarioFuncRh INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '2-5220-0001');
    DECLARE @UsuarioJefeRh INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = '1-5220-0001');

    SELECT 'FUNC_UTI' AS Caso, fa.*
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(@UsuarioFuncUti, GETDATE()) fa
    UNION ALL
    SELECT 'JEFE_UTI' AS Caso, fa.*
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(@UsuarioJefeUti, GETDATE()) fa
    UNION ALL
    SELECT 'FUNC_RH' AS Caso, fa.*
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(@UsuarioFuncRh, GETDATE()) fa
    UNION ALL
    SELECT 'JEFE_RH' AS Caso, fa.*
    FROM dbo.fn_AprobadoresVigentesPorSolicitante(@UsuarioJefeRh, GETDATE()) fa;

END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRAN;

    THROW;
END CATCH;