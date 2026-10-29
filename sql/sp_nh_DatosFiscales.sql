/*
    =====================================================================
    SPs NUEVOS para Datos Fiscales
    =====================================================================
    - sp_nh_GetDatosFiscales      : lista todos los registros fiscales
                                    (join con DatosInstitucion para el nombre).
    - sp_nh_GetDatosFiscalById    : obtiene un registro fiscal por Id.
    - sp_nh_SaveDatosFiscal       : inserta o actualiza un registro fiscal.
    - sp_nh_DeleteDatosFiscal     : elimina un registro fiscal.
    Solo CREATE (guard + EXEC), sin modificar/eliminar objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetDatosFiscales', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetDatosFiscales]
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT DF.Id, DF.RazonSocial, DF.RFC, DF.Telefono1, DF.Telefono2, DF.Email,
               DF.Calle, DF.NoInterior, DF.NoExterior, DF.Colonia, DF.Deleacion,
               DF.Municipio, DF.Estado, DF.CP, DF.Pais, DF.DatosInstitucionID,
               DI.Nombre AS DatosInstitucionNombre
        FROM dbo.DatosFiscales DF
            LEFT JOIN dbo.DatosInstitucion DI ON DI.Id = DF.DatosInstitucionID
        ORDER BY DF.RazonSocial;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetDatosFiscales creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetDatosFiscales ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_GetDatosFiscalById', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetDatosFiscalById]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT DF.Id, DF.RazonSocial, DF.RFC, DF.Telefono1, DF.Telefono2, DF.Email,
               DF.Calle, DF.NoInterior, DF.NoExterior, DF.Colonia, DF.Deleacion,
               DF.Municipio, DF.Estado, DF.CP, DF.Pais, DF.DatosInstitucionID,
               DI.Nombre AS DatosInstitucionNombre
        FROM dbo.DatosFiscales DF
            LEFT JOIN dbo.DatosInstitucion DI ON DI.Id = DF.DatosInstitucionID
        WHERE DF.Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetDatosFiscalById creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetDatosFiscalById ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_SaveDatosFiscal', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_SaveDatosFiscal]
    (
        @Id                 INT,
        @RazonSocial        NVARCHAR(100) = NULL,
        @RFC                NVARCHAR(100) = NULL,
        @Telefono1          NVARCHAR(50)  = NULL,
        @Telefono2          NVARCHAR(50)  = NULL,
        @Email              NVARCHAR(100) = NULL,
        @Calle              NVARCHAR(50)  = NULL,
        @NoInterior         NVARCHAR(10)  = NULL,
        @NoExterior         NVARCHAR(10)  = NULL,
        @Colonia            NVARCHAR(50)  = NULL,
        @Deleacion          NVARCHAR(50)  = NULL,
        @Municipio          NVARCHAR(50)  = NULL,
        @Estado             NVARCHAR(50)  = NULL,
        @CP                 INT           = NULL,
        @Pais               NVARCHAR(50)  = NULL,
        @DatosInstitucionID INT           = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM dbo.DatosFiscales WHERE Id = @Id)
        BEGIN
            UPDATE dbo.DatosFiscales
               SET RazonSocial = @RazonSocial,
                   RFC = @RFC,
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
                   DatosInstitucionID = @DatosInstitucionID
             WHERE Id = @Id;

            SELECT @Id;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.DatosFiscales
            (RazonSocial, RFC, Telefono1, Telefono2, Email, Calle, NoInterior, NoExterior,
             Colonia, Deleacion, Municipio, Estado, CP, Pais, DatosInstitucionID)
            VALUES
            (@RazonSocial, @RFC, @Telefono1, @Telefono2, @Email, @Calle, @NoInterior, @NoExterior,
             @Colonia, @Deleacion, @Municipio, @Estado, @CP, @Pais, @DatosInstitucionID);

            SELECT SCOPE_IDENTITY();
        END
    END
    ');
    PRINT 'SP dbo.sp_nh_SaveDatosFiscal creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_SaveDatosFiscal ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_DeleteDatosFiscal', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_DeleteDatosFiscal]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        DELETE FROM dbo.DatosFiscales WHERE Id = @Id;
        SELECT @@ROWCOUNT;
    END
    ');
    PRINT 'SP dbo.sp_nh_DeleteDatosFiscal creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_DeleteDatosFiscal ya existe. No se recrea.';
END
GO
