-- Seed en tablas del esquema real (RecursosHumanos.*, Operacion.*)
-- Idempotente. Solo para dev/demo.
SET XACT_ABORT ON;
BEGIN TRAN;

-- 1) Estructura organizacional en esquema correcto
IF NOT EXISTS (
    SELECT 1 FROM RecursosHumanos.EstructuraOrganizacional WHERE CodigoOrigen = '120'
)
BEGIN
    INSERT INTO RecursosHumanos.EstructuraOrganizacional
    (Nombre, CodigoOrigen, EstructuraPadreId, EstadoRegistroId, VigenciaDesde, VigenciaHasta, CreadoPor, FechaHoraCreacion)
    VALUES
    ('Unidad Demo CNP', '120', NULL, 1, DATEADD(DAY,-30,SYSUTCDATETIME()), NULL, 'seed_demo', SYSUTCDATETIME());
END;

DECLARE @EstrID INT = (
    SELECT TOP 1 EstructuraOrganizacionalId
    FROM RecursosHumanos.EstructuraOrganizacional
    WHERE CodigoOrigen = '120'
    ORDER BY EstructuraOrganizacionalId DESC
);

-- 2) Jefatura demo
IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'JEFE-DEMO')
BEGIN
    INSERT INTO RecursosHumanos.Usuario
    (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    VALUES
    ('JEFE-DEMO', 'jefe.maria', 'jefe.maria@cnp.local', NULL, 120, 2, 'CNP', 'seed_demo', SYSUTCDATETIME());
END;

-- 3) Funcionario Ana
IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'ANA-DEMO')
BEGIN
    INSERT INTO RecursosHumanos.Usuario
    (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    VALUES
    ('ANA-DEMO', 'Ana Funcionaria Demo', 'ana.demo@cnp.local', NULL, 120, 1, 'CNP', 'seed_demo', SYSUTCDATETIME());
END;

-- 4) Funcionario Luis
IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'LUIS-DEMO')
BEGIN
    INSERT INTO RecursosHumanos.Usuario
    (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    VALUES
    ('LUIS-DEMO', 'Luis Tecnico Demo', 'luis.demo@cnp.local', NULL, 120, 1, 'CNP', 'seed_demo', SYSUTCDATETIME());
END;

-- 5) RRHH demo
IF NOT EXISTS (SELECT 1 FROM RecursosHumanos.Usuario WHERE Cedula = 'RRHH-DEMO')
BEGIN
    INSERT INTO RecursosHumanos.Usuario
    (Cedula, NombreCompleto, CorreoElectronico, JefaturaId, UnidadId, RolId, Compania, CreadoPor, FechaHoraCreacion)
    VALUES
    ('RRHH-DEMO', 'rrhh.carlos', 'rrhh.carlos@cnp.local', NULL, 120, 3, 'CNP', 'seed_demo', SYSUTCDATETIME());
END;

DECLARE @JefeID  INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = 'JEFE-DEMO');
DECLARE @AnaID   INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = 'ANA-DEMO');
DECLARE @LuisID  INT = (SELECT TOP 1 UsuarioId FROM RecursosHumanos.Usuario WHERE Cedula = 'LUIS-DEMO');

-- Asignar JefaturaId
UPDATE RecursosHumanos.Usuario SET JefaturaId = @JefeID WHERE Cedula IN ('ANA-DEMO','LUIS-DEMO') AND (JefaturaId IS NULL OR JefaturaId <> @JefeID);

-- 5) Jerarquía de aprobación en esquema correcto
IF NOT EXISTS (
    SELECT 1 FROM Operacion.JerarquiaAprobacion
    WHERE AprobadorUsuarioId = @JefeID AND EstructuraOrganizacionalId = @EstrID AND EstadoRegistroId = 1
)
BEGIN
    INSERT INTO Operacion.JerarquiaAprobacion
    (AprobadorUsuarioId, EstructuraOrganizacionalId, NivelAprobacion, TipoRelacion, EstadoRegistroId, VigenciaDesde, VigenciaHasta, CreadoPor, FechaHoraCreacion)
    VALUES
    (@JefeID, @EstrID, 1, 'Vertical', 1, DATEADD(DAY,-30,SYSUTCDATETIME()), NULL, 'seed_demo', SYSUTCDATETIME());
END;

-- 6) Boleta pendiente para Ana
IF NOT EXISTS (
    SELECT 1 FROM Operacion.Justificacion
    WHERE UsuarioId = @AnaID AND MotivoGeneral = 'Tardanza por bloqueo vial ruta 27' AND CreadoPor = 'seed_demo'
)
BEGIN
    DECLARE @J1 INT;
    INSERT INTO Operacion.Justificacion
    (UsuarioId, MotivoGeneral, EstadoJustificacionId, CreadoPor, FechaHoraCreacion, FechaCreacion)
    VALUES
    (@AnaID, 'Tardanza por bloqueo vial ruta 27', 1, 'seed_demo', SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @J1 = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO Operacion.JustificacionDetalle
    (JustificacionId, TipoJustificacionId, FechaMarca, ObservacionDetalle, CreadoPor, FechaHoraCreacion)
    VALUES
    (@J1, 1, CAST(DATEADD(DAY,-1,GETDATE()) AS date), 'Ingreso 08:22, accidente autopista', 'seed_demo', SYSUTCDATETIME());
END;

-- 7) Boleta pendiente para Luis
IF NOT EXISTS (
    SELECT 1 FROM Operacion.Justificacion
    WHERE UsuarioId = @LuisID AND MotivoGeneral = 'Omision de marca por visita tecnica' AND CreadoPor = 'seed_demo'
)
BEGIN
    DECLARE @J2 INT;
    INSERT INTO Operacion.Justificacion
    (UsuarioId, MotivoGeneral, EstadoJustificacionId, CreadoPor, FechaHoraCreacion, FechaCreacion)
    VALUES
    (@LuisID, 'Omision de marca por visita tecnica', 1, 'seed_demo', SYSUTCDATETIME(), SYSUTCDATETIME());
    SET @J2 = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO Operacion.JustificacionDetalle
    (JustificacionId, TipoJustificacionId, FechaMarca, ObservacionDetalle, CreadoPor, FechaHoraCreacion)
    VALUES
    (@J2, 3, CAST(DATEADD(DAY,-2,GETDATE()) AS date), 'No registro salida al retornar de visita externa', 'seed_demo', SYSUTCDATETIME());
END;

COMMIT TRAN;

-- Validación
SELECT u.UsuarioId, u.Cedula, u.NombreCompleto, u.RolId, u.UnidadId,
       j.JustificacionId, j.MotivoGeneral, j.EstadoJustificacionId
FROM RecursosHumanos.Usuario u
LEFT JOIN Operacion.Justificacion j ON j.UsuarioId = u.UsuarioId
WHERE u.Cedula IN ('JEFE-DEMO','ANA-DEMO','LUIS-DEMO','RRHH-DEMO')
ORDER BY u.Cedula, j.JustificacionId;
