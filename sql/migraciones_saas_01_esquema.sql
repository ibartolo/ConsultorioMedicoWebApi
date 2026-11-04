/*
    =====================================================================
    MIGRACIÓN SaaS #1 — ESQUEMA MULTI-TENANT (idempotente)
    =====================================================================
    Proyecto : ConsultorioMedico (SaaS por consultorio)
    Cambio   : saas-registro-aislamiento
    Objetivo : Añadir aislamiento por consultorio (ConsultorioID) a las
               11 tablas de dominio, corregir FKs de CitaPaquete, ampliar
               Usuario.Password y crear índices únicos.

    ⚠  IMPORTANTE:
       - SOLO para la BD del clon develop/main (db_9c7990_consultoriodesitst).
       - NO ejecutar sobre DB_9C7990_Consultorio ni ramas -dermatologia.
       - Script RE-EJECUTABLE: usa guards (COL_LENGTH / sys.foreign_keys /
         sys.columns / sys.indexes) y NO hace DROP de tablas ni SPs.
       - El único DROP es de las FKs INCORRECTAS de CitaPaquete
         (FK_CitaPaquete_Cita y FK_CitaPaquete_PacientePaquete).
       - ConsultorioID NO lleva DEFAULT: lo asigna siempre el servicio
         desde el claim (empresaId). Pais se asigna por servicio.
    =====================================================================
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

/* =====================================================================
   1.1  Añadir columna ConsultorioID INT NULL a las 11 tablas de dominio
   ===================================================================== */
IF COL_LENGTH('dbo.Paciente', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.Paciente ADD ConsultorioID INT NULL;
    PRINT 'dbo.Paciente: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.Paciente: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.Cita', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.Cita ADD ConsultorioID INT NULL;
    PRINT 'dbo.Cita: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.Cita: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.Consulta', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.Consulta ADD ConsultorioID INT NULL;
    PRINT 'dbo.Consulta: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.Consulta: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.Anexo', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.Anexo ADD ConsultorioID INT NULL;
    PRINT 'dbo.Anexo: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.Anexo: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.PacientePaquete', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.PacientePaquete ADD ConsultorioID INT NULL;
    PRINT 'dbo.PacientePaquete: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.PacientePaquete: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.PagoPaquete', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.PagoPaquete ADD ConsultorioID INT NULL;
    PRINT 'dbo.PagoPaquete: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.PagoPaquete: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.CitaPaquete', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.CitaPaquete ADD ConsultorioID INT NULL;
    PRINT 'dbo.CitaPaquete: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.CitaPaquete: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.Tratamiento', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.Tratamiento ADD ConsultorioID INT NULL;
    PRINT 'dbo.Tratamiento: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.Tratamiento: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.TratamientoPaquete', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.TratamientoPaquete ADD ConsultorioID INT NULL;
    PRINT 'dbo.TratamientoPaquete: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.TratamientoPaquete: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.Paquete', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.Paquete ADD ConsultorioID INT NULL;
    PRINT 'dbo.Paquete: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.Paquete: columna ConsultorioID ya existe.';
GO

IF COL_LENGTH('dbo.TipoConsulta', 'ConsultorioID') IS NULL
BEGIN
    ALTER TABLE dbo.TipoConsulta ADD ConsultorioID INT NULL;
    PRINT 'dbo.TipoConsulta: columna ConsultorioID agregada.';
END
ELSE
    PRINT 'dbo.TipoConsulta: columna ConsultorioID ya existe.';
GO

/* =====================================================================
   1.2  Backfill: ConsultorioID = 1 donde sea NULL (11 tablas)
        DatosInstitucion.Id = 1 ya existe ("Dra. Tania Guadalupe Rios Bartolo").
   ===================================================================== */
UPDATE dbo.Paciente            SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.Cita                SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.Consulta            SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.Anexo               SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.PacientePaquete     SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.PagoPaquete         SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.CitaPaquete         SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.Tratamiento         SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.TratamientoPaquete  SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.Paquete             SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
UPDATE dbo.TipoConsulta        SET ConsultorioID = 1 WHERE ConsultorioID IS NULL;
PRINT 'Backfill ConsultorioID = 1 aplicado (11 tablas).';
GO

/* =====================================================================
   1.3  FK por tabla: FK_<Tabla>_ConsultorioID -> DatosInstitucion(Id)
   ===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Paciente_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.Paciente'))
BEGIN
    ALTER TABLE dbo.Paciente ADD CONSTRAINT FK_Paciente_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_Paciente_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_Paciente_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Cita_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.Cita'))
BEGIN
    ALTER TABLE dbo.Cita ADD CONSTRAINT FK_Cita_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_Cita_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_Cita_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Consulta_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.Consulta'))
BEGIN
    ALTER TABLE dbo.Consulta ADD CONSTRAINT FK_Consulta_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_Consulta_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_Consulta_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Anexo_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.Anexo'))
BEGIN
    ALTER TABLE dbo.Anexo ADD CONSTRAINT FK_Anexo_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_Anexo_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_Anexo_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PacientePaquete_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.PacientePaquete'))
BEGIN
    ALTER TABLE dbo.PacientePaquete ADD CONSTRAINT FK_PacientePaquete_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_PacientePaquete_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_PacientePaquete_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_PagoPaquete_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.PagoPaquete'))
BEGIN
    ALTER TABLE dbo.PagoPaquete ADD CONSTRAINT FK_PagoPaquete_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_PagoPaquete_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_PagoPaquete_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CitaPaquete_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.CitaPaquete'))
BEGIN
    ALTER TABLE dbo.CitaPaquete ADD CONSTRAINT FK_CitaPaquete_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_CitaPaquete_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_CitaPaquete_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Tratamiento_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.Tratamiento'))
BEGIN
    ALTER TABLE dbo.Tratamiento ADD CONSTRAINT FK_Tratamiento_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_Tratamiento_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_Tratamiento_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TratamientoPaquete_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.TratamientoPaquete'))
BEGIN
    ALTER TABLE dbo.TratamientoPaquete ADD CONSTRAINT FK_TratamientoPaquete_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_TratamientoPaquete_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_TratamientoPaquete_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_Paquete_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.Paquete'))
BEGIN
    ALTER TABLE dbo.Paquete ADD CONSTRAINT FK_Paquete_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_Paquete_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_Paquete_ConsultorioID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_TipoConsulta_ConsultorioID' AND parent_object_id = OBJECT_ID('dbo.TipoConsulta'))
BEGIN
    ALTER TABLE dbo.TipoConsulta ADD CONSTRAINT FK_TipoConsulta_ConsultorioID FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_TipoConsulta_ConsultorioID creada.';
END
ELSE
    PRINT 'FK_TipoConsulta_ConsultorioID ya existe.';
GO

/* =====================================================================
   1.4  ConsultorioID -> NOT NULL (las 11 tablas, vía is_nullable = 1)
   ===================================================================== */
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Paciente') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.Paciente ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Cita') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.Cita ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Consulta') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.Consulta ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Anexo') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.Anexo ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PacientePaquete') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.PacientePaquete ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.PagoPaquete') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.PagoPaquete ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.CitaPaquete') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.CitaPaquete ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Tratamiento') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.Tratamiento ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TratamientoPaquete') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.TratamientoPaquete ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Paquete') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.Paquete ALTER COLUMN ConsultorioID INT NOT NULL;
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.TipoConsulta') AND name = 'ConsultorioID' AND is_nullable = 1)
    ALTER TABLE dbo.TipoConsulta ALTER COLUMN ConsultorioID INT NOT NULL;
PRINT 'ConsultorioID definido como NOT NULL en las 11 tablas.';
GO

/* =====================================================================
   1.5  Usuario.Password -> NVARCHAR(250)  (PBKDF2 necesita ~82 caracteres)
   ===================================================================== */
IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuario') AND name = 'Password' AND max_length < 500)
BEGIN
    ALTER TABLE dbo.Usuario ALTER COLUMN Password NVARCHAR(250) NULL;
    PRINT 'dbo.Usuario.Password ampliado a NVARCHAR(250).';
END
ELSE
    PRINT 'dbo.Usuario.Password ya tiene la longitud adecuada.';
GO

/* =====================================================================
   1.6  Usuario.CompaniaID: backfill NULL->1, luego NOT NULL, luego FK
       (solo ivanbartolo tiene CompaniaID NULL; admin ya tiene 1).
   ===================================================================== */
UPDATE dbo.Usuario SET CompaniaID = 1 WHERE CompaniaID IS NULL;
PRINT 'Usuario.CompaniaID: backfill NULL -> 1 aplicado.';
GO

IF EXISTS (SELECT 1 FROM sys.columns WHERE object_id = OBJECT_ID('dbo.Usuario') AND name = 'CompaniaID' AND is_nullable = 1)
BEGIN
    ALTER TABLE dbo.Usuario ALTER COLUMN CompaniaID INT NOT NULL;
    PRINT 'dbo.Usuario.CompaniaID definido como NOT NULL.';
END
ELSE
    PRINT 'dbo.Usuario.CompaniaID ya es NOT NULL.';
GO

-- FK por si no existe (ya existe FK_Usuario_DatosInstitucion en el clon).
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
    JOIN sys.columns rc ON rc.object_id = fkc.referenced_object_id AND rc.column_id = fkc.referenced_column_id
    WHERE fk.parent_object_id = OBJECT_ID('dbo.Usuario')
      AND pc.name = 'CompaniaID'
      AND fk.referenced_object_id = OBJECT_ID('dbo.DatosInstitucion')
      AND rc.name = 'Id'
)
BEGIN
    ALTER TABLE dbo.Usuario ADD CONSTRAINT FK_Usuario_CompaniaID FOREIGN KEY (CompaniaID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_Usuario_CompaniaID creada.';
END
ELSE
    PRINT 'FK Usuario.CompaniaID -> DatosInstitucion(Id) ya existe.';
GO

/* =====================================================================
   1.7  Corregir FKs de CitaPaquete (tabla vacía -> corrección segura)
        Hoy: FK_CitaPaquete_Cita              CitaID           -> Paciente.Id  (MAL)
             FK_CitaPaquete_PacientePaquete   PacientePaqueteID -> Paquete.Id  (MAL)
        Debe: CitaID -> Cita.Id ; PacientePaqueteID -> PacientePaquete.Id
   ===================================================================== */

-- Quitar FK incorrecta: CitaID -> Paciente
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CitaPaquete_Cita' AND parent_object_id = OBJECT_ID('dbo.CitaPaquete') AND referenced_object_id = OBJECT_ID('dbo.Paciente'))
BEGIN
    ALTER TABLE dbo.CitaPaquete DROP CONSTRAINT FK_CitaPaquete_Cita;
    PRINT 'FK_CitaPaquete_Cita (incorrecta, hacia Paciente) eliminada.';
END
ELSE
    PRINT 'FK_CitaPaquete_Cita ya no referencia Paciente (nada que corregir).';
GO

-- Quitar FK incorrecta: PacientePaqueteID -> Paquete
IF EXISTS (SELECT 1 FROM sys.foreign_keys WHERE name = 'FK_CitaPaquete_PacientePaquete' AND parent_object_id = OBJECT_ID('dbo.CitaPaquete') AND referenced_object_id = OBJECT_ID('dbo.Paquete'))
BEGIN
    ALTER TABLE dbo.CitaPaquete DROP CONSTRAINT FK_CitaPaquete_PacientePaquete;
    PRINT 'FK_CitaPaquete_PacientePaquete (incorrecta, hacia Paquete) eliminada.';
END
ELSE
    PRINT 'FK_CitaPaquete_PacientePaquete ya no referencia Paquete (nada que corregir).';
GO

-- Añadir FK correcta: CitaID -> Cita(Id)
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID('dbo.CitaPaquete')
      AND pc.name = 'CitaID'
      AND fk.referenced_object_id = OBJECT_ID('dbo.Cita')
)
BEGIN
    ALTER TABLE dbo.CitaPaquete ADD CONSTRAINT FK_CitaPaquete_Cita FOREIGN KEY (CitaID) REFERENCES dbo.Cita (Id);
    PRINT 'FK_CitaPaquete_Cita (CitaID -> Cita.Id) creada correctamente.';
END
ELSE
    PRINT 'FK CitaPaquete.CitaID -> Cita.Id ya existe.';
GO

-- Añadir FK correcta: PacientePaqueteID -> PacientePaquete(Id)
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID('dbo.CitaPaquete')
      AND pc.name = 'PacientePaqueteID'
      AND fk.referenced_object_id = OBJECT_ID('dbo.PacientePaquete')
)
BEGIN
    ALTER TABLE dbo.CitaPaquete ADD CONSTRAINT FK_CitaPaquete_PacientePaquete FOREIGN KEY (PacientePaqueteID) REFERENCES dbo.PacientePaquete (Id);
    PRINT 'FK_CitaPaquete_PacientePaquete (PacientePaqueteID -> PacientePaquete.Id) creada correctamente.';
END
ELSE
    PRINT 'FK CitaPaquete.PacientePaqueteID -> PacientePaquete.Id ya existe.';
GO

/* =====================================================================
   1.8  Índices únicos (guard IF NOT EXISTS sys.indexes)
        Los campos opcionales usan índices filtrados (WHERE col IS NOT NULL).
        NO se crea índice único sobre DatosFiscales.Email.
   ===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Usuario_UserName' AND object_id = OBJECT_ID('dbo.Usuario'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_Usuario_UserName ON dbo.Usuario (UserName) WHERE UserName IS NOT NULL;
    PRINT 'UX_Usuario_UserName creado (filtrado).';
END
ELSE
    PRINT 'UX_Usuario_UserName ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DatosFiscales_RFC' AND object_id = OBJECT_ID('dbo.DatosFiscales'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_DatosFiscales_RFC ON dbo.DatosFiscales (RFC) WHERE RFC IS NOT NULL;
    PRINT 'UX_DatosFiscales_RFC creado (filtrado).';
END
ELSE
    PRINT 'UX_DatosFiscales_RFC ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DatosFiscales_RazonSocial' AND object_id = OBJECT_ID('dbo.DatosFiscales'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_DatosFiscales_RazonSocial ON dbo.DatosFiscales (RazonSocial) WHERE RazonSocial IS NOT NULL;
    PRINT 'UX_DatosFiscales_RazonSocial creado (filtrado).';
END
ELSE
    PRINT 'UX_DatosFiscales_RazonSocial ya existe.';
GO

-- 1:1 fiscal por consultorio
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DatosFiscales_DatosInstitucionID' AND object_id = OBJECT_ID('dbo.DatosFiscales'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_DatosFiscales_DatosInstitucionID ON dbo.DatosFiscales (DatosInstitucionID) WHERE DatosInstitucionID IS NOT NULL;
    PRINT 'UX_DatosFiscales_DatosInstitucionID creado (1:1, filtrado).';
END
ELSE
    PRINT 'UX_DatosFiscales_DatosInstitucionID ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DatosInstitucion_Nombre' AND object_id = OBJECT_ID('dbo.DatosInstitucion'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_DatosInstitucion_Nombre ON dbo.DatosInstitucion (Nombre) WHERE Nombre IS NOT NULL;
    PRINT 'UX_DatosInstitucion_Nombre creado (filtrado).';
END
ELSE
    PRINT 'UX_DatosInstitucion_Nombre ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_DatosInstitucion_Email' AND object_id = OBJECT_ID('dbo.DatosInstitucion'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_DatosInstitucion_Email ON dbo.DatosInstitucion (Email) WHERE Email IS NOT NULL;
    PRINT 'UX_DatosInstitucion_Email creado (filtrado).';
END
ELSE
    PRINT 'UX_DatosInstitucion_Email ya existe.';
GO

PRINT '============================================';
PRINT 'Migración de esquema SaaS #1 finalizada.';
PRINT '============================================';
GO
