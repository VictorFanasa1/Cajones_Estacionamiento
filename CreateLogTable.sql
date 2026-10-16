-- 1. Crear la tabla principal de Log
CREATE TABLE [dbo].[logEstacionamiento](
	[uiLogEstacionamiento] [int] IDENTITY(1,1) NOT NULL,
	[uiUser] [int] NULL,           -- Número de empleado o ID de usuario
	[iRol] [int] NULL,            -- ID del Rol
	[iDireccion] [int] NULL,      -- ID de la Dirección (catLugares)
	[iCajon] [int] NULL,          -- ID del Cajón (catNumeroLugares)
	[iLog] [int] NOT NULL,        -- ID de la Acción
	[dtFechaAccion] [datetime] NOT NULL DEFAULT (getdate()),
 CONSTRAINT [PK_logEstacionamiento] PRIMARY KEY CLUSTERED ([uiLogEstacionamiento] ASC)
);

-- 2. (Opcional) Crear tabla de catálogo para descripciones de acciones
CREATE TABLE [dbo].[catLog](
	[iLog] [int] NOT NULL,
	[sDescripcion] [varchar](100) NOT NULL,
 CONSTRAINT [PK_catLog] PRIMARY KEY CLUSTERED ([iLog] ASC)
);

-- Insertar los tipos de acciones actuales
INSERT INTO [dbo].[catLog] ([iLog], [sDescripcion]) VALUES (1, 'Generación de Solicitud');
INSERT INTO [dbo].[catLog] ([iLog], [sDescripcion]) VALUES (2, 'Asignación de Solicitud');
INSERT INTO [dbo].[catLog] ([iLog], [sDescripcion]) VALUES (3, 'Rechazo de Solicitud');
INSERT INTO [dbo].[catLog] ([iLog], [sDescripcion]) VALUES (4, 'Asignación Empleado');
INSERT INTO [dbo].[catLog] ([iLog], [sDescripcion]) VALUES (5, 'Liberación de Cajón');
INSERT INTO [dbo].[catLog] ([iLog], [sDescripcion]) VALUES (6, 'Edición de Lugar');
INSERT INTO [dbo].[catLog] ([iLog], [sDescripcion]) VALUES (7, 'Edición Hora de Salida');
