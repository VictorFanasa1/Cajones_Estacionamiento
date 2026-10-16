/*
Script: 2026-01-add-interno-externo.sql
Objetivo: Agregar columnas bInterno y bExterno a la tabla cajonesEstacionamientos con valor por defecto 0,
          y actualizar el procedimiento sp_InsertEstacionamientoVisitantes para aceptar dichos parámetros.

Nota:
- Ejecutar en la base de datos "Estacionamiento".
- Si el procedimiento almacenado difiere, ajustar el INSERT de acuerdo con su definición actual.
*/

USE Estacionamiento;
GO

/* 1) Agregar columnas si no existen */
IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.objects o ON o.object_id = c.object_id AND o.type = 'U'
    WHERE o.name = 'cajonesEstacionamientos' AND c.name = 'bInterno')
BEGIN
    ALTER TABLE dbo.cajonesEstacionamientos
      ADD bInterno bit NOT NULL CONSTRAINT DF_cajonesEst_bInterno DEFAULT (0);
END
GO

IF NOT EXISTS (
    SELECT 1 FROM sys.columns c
    JOIN sys.objects o ON o.object_id = c.object_id AND o.type = 'U'
    WHERE o.name = 'cajonesEstacionamientos' AND c.name = 'bExterno')
BEGIN
    ALTER TABLE dbo.cajonesEstacionamientos
      ADD bExterno bit NOT NULL CONSTRAINT DF_cajonesEst_bExterno DEFAULT (0);
END
GO

/* Asegurar 0 en datos existentes */
UPDATE dbo.cajonesEstacionamientos
   SET bInterno = ISNULL(bInterno, 0),
       bExterno = ISNULL(bExterno, 0)
WHERE 1 = 1;
GO

/* 2) (Re)crear el SP de inserción para aceptar los nuevos parámetros */
IF OBJECT_ID('dbo.sp_InsertEstacionamientoVisitantes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_InsertEstacionamientoVisitantes;
GO

CREATE PROCEDURE dbo.sp_InsertEstacionamientoVisitantes
    @iCajon           int,
    @iLugar           int,
    @iUsuario         int,
    @sObservaciones   varchar(1000) = NULL,
    @dtFechaVisita    datetime,
    @horaVisita       varchar(50),
    @horaSalida       varchar(50),
    @sPlaca           varchar(50) = NULL,
    @sModelo          varchar(50) = NULL,
    @sColor           varchar(50) = NULL,
    @bInterno         bit = 0,
    @bExterno         bit = 0,
    @uiRegistroCajones int OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.cajonesEstacionamientos (
        iCajon, iLugar, iUsuario, sObservaciones, dtFechaAsignacion,
        dtFechaVisita, horaVisita, horaSalida, sPlaca, sModelo, sColor,
        bInterno, bExterno, bActivo
    )
    VALUES (
        @iCajon, @iLugar, @iUsuario, @sObservaciones, GETDATE(),
        @dtFechaVisita, @horaVisita, @horaSalida, @sPlaca, @sModelo, @sColor,
        @bInterno, @bExterno, 1
    );

    SET @uiRegistroCajones = SCOPE_IDENTITY();
END
GO
