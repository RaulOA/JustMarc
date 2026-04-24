-- ============================================================
-- Script: 003_integra_marcas_seed_demo.sql
-- Propósito: Datos semilla mínimos para demo/dev del flujo de jefatura.
-- Esquema real: dbo.Usuarios, dbo.Justificaciones_Encabezado, dbo.Justificaciones_Detalle
-- Idempotente: usa IF NOT EXISTS antes de cada INSERT.
-- Aplicar sólo en entornos de desarrollo / demo.
-- ============================================================

USE INTEGRA_CNP;
SET XACT_ABORT ON;
BEGIN TRAN;

-- Obtener ID de jefatura demo ya existente (JEFE-0001, UsuarioID=2)
DECLARE @JefeID INT = (SELECT TOP 1 UsuarioID FROM dbo.Usuarios WHERE Cedula = 'JEFE-0001');

-- ── 1. Funcionario subordinado de jefe.maria ────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Cedula = 'ANA-DEMO-01')
BEGIN
    INSERT INTO dbo.Usuarios
        (Cedula, NombreCompleto, Correo, JefaturaID, UnidadID, RolID, Compania, Usr_Registro, Fec_Registro)
    VALUES
        ('ANA-DEMO-01', 'Ana Funcionaria Demo', 'ana.demo@cnp.local', @JefeID, 120, 1, 'CNP', 'seed_demo', GETDATE());
END;

-- ── 2. Segundo funcionario subordinado ──────────────────────────────────
IF NOT EXISTS (SELECT 1 FROM dbo.Usuarios WHERE Cedula = 'LUIS-DEMO-01')
BEGIN
    INSERT INTO dbo.Usuarios
        (Cedula, NombreCompleto, Correo, JefaturaID, UnidadID, RolID, Compania, Usr_Registro, Fec_Registro)
    VALUES
        ('LUIS-DEMO-01', 'Luis Técnico Demo', 'luis.demo@cnp.local', @JefeID, 120, 1, 'CNP', 'seed_demo', GETDATE());
END;

DECLARE @AnaID  INT = (SELECT TOP 1 UsuarioID FROM dbo.Usuarios WHERE Cedula = 'ANA-DEMO-01');
DECLARE @LuisID INT = (SELECT TOP 1 UsuarioID FROM dbo.Usuarios WHERE Cedula = 'LUIS-DEMO-01');

-- ── 3. Boleta #1 — Pendiente Jefatura para Ana ──────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM dbo.Justificaciones_Encabezado
    WHERE UsuarioID = @AnaID AND MotivoGeneral = 'Tardanza por bloqueo vial ruta 27' AND Usr_Registro = 'seed_demo'
)
BEGIN
    DECLARE @Jus1ID INT;
    INSERT INTO dbo.Justificaciones_Encabezado
        (UsuarioID, MotivoGeneral, EstadoID, FechaCreacion, Usr_Registro, Fec_Registro)
    VALUES
        (@AnaID, 'Tardanza por bloqueo vial ruta 27', 1, GETDATE(), 'seed_demo', GETDATE());
    SET @Jus1ID = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.Justificaciones_Detalle
        (JustificacionID, TipoJustificacionID, FechaMarca, ObservacionDetalle, Usr_Registro, Fec_Registro)
    VALUES
        (@Jus1ID, 1, CAST(DATEADD(DAY,-1,GETDATE()) AS date), 'Ingreso 08:22 por accidente en autopista', 'seed_demo', GETDATE());
END;

-- ── 4. Boleta #2 — Pendiente Jefatura para Luis ─────────────────────────
IF NOT EXISTS (
    SELECT 1 FROM dbo.Justificaciones_Encabezado
    WHERE UsuarioID = @LuisID AND MotivoGeneral = 'Omisión de marca por visita técnica fuera de sede' AND Usr_Registro = 'seed_demo'
)
BEGIN
    DECLARE @Jus2ID INT;
    INSERT INTO dbo.Justificaciones_Encabezado
        (UsuarioID, MotivoGeneral, EstadoID, FechaCreacion, Usr_Registro, Fec_Registro)
    VALUES
        (@LuisID, 'Omisión de marca por visita técnica fuera de sede', 1, GETDATE(), 'seed_demo', GETDATE());
    SET @Jus2ID = CAST(SCOPE_IDENTITY() AS INT);

    INSERT INTO dbo.Justificaciones_Detalle
        (JustificacionID, TipoJustificacionID, FechaMarca, ObservacionDetalle, Usr_Registro, Fec_Registro)
    VALUES
        (@Jus2ID, 3, CAST(DATEADD(DAY,-2,GETDATE()) AS date), 'No registró salida al retornar de visita externa', 'seed_demo', GETDATE());
END;

COMMIT TRAN;

-- Validación final
SELECT u.UsuarioID, u.Cedula, u.NombreCompleto, u.JefaturaID,
       j.JustificacionID, j.MotivoGeneral, j.EstadoID
FROM dbo.Usuarios u
INNER JOIN dbo.Justificaciones_Encabezado j ON j.UsuarioID = u.UsuarioID
WHERE u.Cedula IN ('ANA-DEMO-01','LUIS-DEMO-01')
ORDER BY j.JustificacionID;
