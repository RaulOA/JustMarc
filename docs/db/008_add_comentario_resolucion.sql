/* ============================================================
   INTEGRA_CNP - Migracion incremental
   Agrega ComentarioResolucion a Justificaciones_Encabezado
   ============================================================ */

USE INTEGRA_CNP;
GO

IF COL_LENGTH('dbo.Justificaciones_Encabezado', 'ComentarioResolucion') IS NULL
BEGIN
    ALTER TABLE dbo.Justificaciones_Encabezado
        ADD ComentarioResolucion VARCHAR(500) NULL;
END;
GO
