/*
    =====================================================================
    Cambios para el primer entregable de receta / configuración
    =====================================================================
    - DatosInstitucion: LogoRuta + datos del médico (un solo médico).
    - SaveOrUpdateDatosInstitucion: guarda los datos del médico.
    - sp_nh_UpdateLogoInstitucion: actualiza solo el logo.
    - sp_nh_GetRecetaByConsulta: receta con paciente, institución y médico.
    =====================================================================
*/

IF COL_LENGTH('dbo.DatosInstitucion', 'LogoRuta') IS NULL
    ALTER TABLE dbo.DatosInstitucion ADD LogoRuta NVARCHAR(200) NULL;
IF COL_LENGTH('dbo.DatosInstitucion', 'MedicoNombre') IS NULL
    ALTER TABLE dbo.DatosInstitucion ADD MedicoNombre NVARCHAR(100) NULL;
IF COL_LENGTH('dbo.DatosInstitucion', 'MedicoCedula') IS NULL
    ALTER TABLE dbo.DatosInstitucion ADD MedicoCedula NVARCHAR(50) NULL;
IF COL_LENGTH('dbo.DatosInstitucion', 'MedicoEspecialidad') IS NULL
    ALTER TABLE dbo.DatosInstitucion ADD MedicoEspecialidad NVARCHAR(100) NULL;
GO

ALTER PROCEDURE [dbo].[SaveOrUpdateDatosInstitucion]
(
    @Id int,
    @Nombre nvarchar(100) = NULL,
    @NoTrabajadores int = NULL,
    @Telefono1 nvarchar(50) = NULL,
    @Telefono2 nvarchar(50) = NULL,
    @Email nvarchar(100) = NULL,
    @Calle nvarchar(50) = NULL,
    @NoInterior nvarchar(10) = NULL,
    @NoExterior nvarchar(10) = NULL,
    @Colonia nvarchar(50) = NULL,
    @Deleacion nvarchar(50) = NULL,
    @Municipio nvarchar(50) = NULL,
    @Estado nvarchar(50) = NULL,
    @CP int = NULL,
    @Pais nvarchar(50) = NULL,
    @MedicoNombre nvarchar(100) = NULL,
    @MedicoCedula nvarchar(50) = NULL,
    @MedicoEspecialidad nvarchar(100) = NULL
)
AS
BEGIN
    IF EXISTS(SELECT * FROM DatosInstitucion WHERE Id = @Id)
    BEGIN
        UPDATE DatosInstitucion
           SET Nombre = @Nombre,
               NoTrabajadores = @NoTrabajadores,
               Telefono1 = @Telefono1,
               Telefono2 = @Telefono2,
               Email = @Email,
               Calle = @Calle,
               NoInterior = @NoInterior,
               NoExterior = @NoExterior,
               Colonia = @Colonia,
               Deleacion = @Deleacion,
               Municipio = @Municipio,
               Estado = @Estado,
               CP = @CP,
               Pais = @Pais,
               MedicoNombre = @MedicoNombre,
               MedicoCedula = @MedicoCedula,
               MedicoEspecialidad = @MedicoEspecialidad
         WHERE Id = @Id;

        SELECT @Id;
    END
    ELSE
    BEGIN
        INSERT INTO DatosInstitucion
        (Nombre, NoTrabajadores, Telefono1, Telefono2, Email, Calle, NoInterior, NoExterior,
         Colonia, Deleacion, Municipio, Estado, CP, Pais,
         MedicoNombre, MedicoCedula, MedicoEspecialidad)
        VALUES
        (@Nombre, @NoTrabajadores, @Telefono1, @Telefono2, @Email, @Calle, @NoInterior, @NoExterior,
         @Colonia, @Deleacion, @Municipio, @Estado, @CP, @Pais,
         @MedicoNombre, @MedicoCedula, @MedicoEspecialidad);

        SELECT @@IDENTITY;
    END
END
GO

IF OBJECT_ID('dbo.sp_nh_UpdateLogoInstitucion', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_UpdateLogoInstitucion]
    (
        @Id INT,
        @LogoRuta NVARCHAR(200) = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        UPDATE dbo.DatosInstitucion
           SET LogoRuta = @LogoRuta
         WHERE Id = @Id;

        SELECT @@ROWCOUNT;
    END
    ');
    PRINT 'SP dbo.sp_nh_UpdateLogoInstitucion creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_UpdateLogoInstitucion ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_GetRecetaByConsulta', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetRecetaByConsulta]
    (
        @consultaID INT,
        @institucionId INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT P.Nombre + '' '' + P.ApellidoPaterno + '' '' + ISNULL(P.ApellidoMaterno, '''') AS Paciente,
               P.Edad,
               P.Sexo,
               C.FechaConsulta,
               ISNULL(C.Descripcion, '''') AS Consulta,
               ISNULL(C.DescripcionReceta, '''') AS Medicamento,
               ISNULL(T.Nombre, '''') AS TipoConsulta,
               C.FechaProximaConsulta AS ProximaConsulta,
               DI.Nombre AS InstitucionNombre,
               DI.Calle AS InstitucionCalle,
               DI.NoInterior AS InstitucionNoInterior,
               DI.NoExterior AS InstitucionNoExterior,
               DI.Colonia AS InstitucionColonia,
               DI.Deleacion AS InstitucionDeleacion,
               DI.Municipio AS InstitucionMunicipio,
               DI.Estado AS InstitucionEstado,
               DI.CP AS InstitucionCP,
               DI.Pais AS InstitucionPais,
               DI.Telefono1 AS InstitucionTelefono1,
               DI.Telefono2 AS InstitucionTelefono2,
               DI.Email AS InstitucionEmail,
               DI.LogoRuta,
               DI.MedicoNombre,
               DI.MedicoCedula,
               DI.MedicoEspecialidad
        FROM Paciente AS P
            INNER JOIN Consulta AS C ON P.Id = C.PacienteID
            LEFT JOIN TipoConsulta AS T ON C.TipoConsultaID = T.Id
            LEFT JOIN DatosInstitucion AS DI ON DI.Id = @institucionId
        WHERE C.Id = @consultaID;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetRecetaByConsulta creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetRecetaByConsulta ya existe. No se recrea.';
END
GO
