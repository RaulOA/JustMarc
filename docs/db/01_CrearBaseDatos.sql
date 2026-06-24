/* ============================================================================
   INTEGRA_CNP — Script 1 de 3: CREACION DE BASE DE DATOS Y CONFIGURACION INICIAL
   ----------------------------------------------------------------------------
   Sistema:     Justificacion de Marca (INTEGRA_CNP) — CNP / FANAL
   Responsable: Equipo de Base de Datos
   Fecha:       2026-06-24
   Motor:       SQL Server 2022

   PROPOSITO
     Crear la base de datos y los esquemas funcionales. No crea tablas ni datos:
     ese es el alcance de los scripts 02 (estructura) y 03 (datos semilla).

   ORDEN DE EJECUCION (obligatorio)
     1) 01_CrearBaseDatos.sql      <-- este archivo
     2) 02_EstructuraCompleta.sql
     3) 03_DatosSemilla.sql

   CODIFICACION
     Ejecutar con UTF-8 para preservar acentos en literales:
       - SSMS / Azure Data Studio: abrir el archivo tal cual.
       - sqlcmd:  sqlcmd -S <servidor> -d master -i 01_CrearBaseDatos.sql -f 65001

   IDEMPOTENCIA
     Todo el script puede re-ejecutarse sin error: cada objeto se crea solo si
     no existe (IF DB_ID / IF NOT EXISTS).

   CONVENCIONES (ver docs/db/Convenciones_Nomeclatura_BD.md)
     - PascalCase, espanol, sin abreviaciones.
     - Esquemas explicitos por area funcional. 'dbo' se reserva a objetos de
       sistema y a objetos legados de compatibilidad SIFCNP (ver Script 02).
   ============================================================================ */

SET NOCOUNT ON;
SET XACT_ABORT ON;
GO

/* ============================================================================
   1. BASE DE DATOS
   ----------------------------------------------------------------------------
   La cadena de conexion de la aplicacion usa la clave 'IntegraCnp' y se inyecta
   por variable de entorno (ConnectionStrings__IntegraCnp); NO se versiona.
   La base SIEMPRE se llama INTEGRA_CNP.
   ============================================================================ */

IF DB_ID('INTEGRA_CNP') IS NULL
BEGIN
    PRINT 'Creando base de datos INTEGRA_CNP...';
    CREATE DATABASE INTEGRA_CNP;
END
ELSE
BEGIN
    PRINT 'La base de datos INTEGRA_CNP ya existe. Se omite la creacion.';
END;
GO

USE INTEGRA_CNP;
GO

/* ============================================================================
   2. ESQUEMAS FUNCIONALES
   ----------------------------------------------------------------------------
   Configuracion    : catalogos y parametros del sistema.
   RecursosHumanos  : personal y estructura organizacional.
   Operacion        : justificaciones, jerarquia y delegacion de aprobacion.
   Auditoria        : trazabilidad de eventos, errores y acciones admin.
   Integracion      : vistas de solo lectura sobre BD externas (WIZDOM/SIFCNP).
   ============================================================================ */

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Configuracion')
    EXEC('CREATE SCHEMA Configuracion');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'RecursosHumanos')
    EXEC('CREATE SCHEMA RecursosHumanos');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Operacion')
    EXEC('CREATE SCHEMA Operacion');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Auditoria')
    EXEC('CREATE SCHEMA Auditoria');
GO

IF NOT EXISTS (SELECT 1 FROM sys.schemas WHERE name = 'Integracion')
    EXEC('CREATE SCHEMA Integracion');
GO

PRINT 'Script 01 completado: base de datos y esquemas listos.';
GO
