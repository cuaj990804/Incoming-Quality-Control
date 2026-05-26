-- Crear tabla ACCEPTED_PIECES para volantes sin defectos
IF NOT EXISTS (SELECT * FROM sys.objects WHERE object_id = OBJECT_ID(N'[dbo].[ACCEPTED_PIECES]') AND type in (N'U'))
BEGIN
    CREATE TABLE [ACCEPTED_PIECES] (
        [ID] int NOT NULL IDENTITY(1,1),
        [PROGRAM] varchar(50) NOT NULL,
        [BASE] varchar(50) NOT NULL,
        [HEATER] varchar(50) NOT NULL,
        [NTC] varchar(50) NOT NULL,
        [INSIDE] varchar(50) NOT NULL,
        [OUTSIDE] varchar(50) NOT NULL,
        [GUARD] varchar(50) NOT NULL,
        [ACCEPTED_DATE] datetime NULL DEFAULT (getdate()),
        [MOLDING] varchar(50) NULL,
        [GAP] varchar(50) NULL,
        [CONNECTOR] varchar(50) NULL,
        CONSTRAINT [PK__ACCEPTED__3214EC27XXXXXXXX] PRIMARY KEY ([ID])
    );

    PRINT 'Tabla ACCEPTED_PIECES creada exitosamente.';
END
ELSE
BEGIN
    PRINT 'La tabla ACCEPTED_PIECES ya existe.';
END
GO
