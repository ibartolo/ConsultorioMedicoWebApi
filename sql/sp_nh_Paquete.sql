/*
    =====================================================================
    SPs NUEVOS para el módulo Paquete (migración WinForms -> WebApi)
    =====================================================================
    - sp_nh_GetPaqueteById : detalle de paquete.
    - sp_nh_SavePaquete    : upsert (reemplaza el SaveOrUpdate de NHibernate).
    Solo CREATE, sin modificar/eliminar objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetPaqueteById', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetPaqueteById]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Nombre, Descripcion, FechaCreacion, Estatus, Costo
        FROM dbo.Paquete
        WHERE Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetPaqueteById creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetPaqueteById ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_SavePaquete', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_SavePaquete]
    (
        @Id             INT,
        @Nombre         NVARCHAR(100) = NULL,
        @Descripcion    NVARCHAR(500) = NULL,
        @FechaCreacion  DATETIME = NULL,
        @Estatus        BIT = NULL,
        @Costo          INT = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM dbo.Paquete WHERE Id = @Id)
        BEGIN
            UPDATE dbo.Paquete
            SET Nombre = @Nombre, Descripcion = @Descripcion, FechaCreacion = @FechaCreacion,
                Estatus = @Estatus, Costo = @Costo
            WHERE Id = @Id;

            SELECT @Id;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Paquete (Nombre, Descripcion, FechaCreacion, Estatus, Costo)
            VALUES (@Nombre, @Descripcion, @FechaCreacion, @Estatus, @Costo);

            SELECT SCOPE_IDENTITY();
        END
    END
    ');
    PRINT 'SP dbo.sp_nh_SavePaquete creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_SavePaquete ya existe. No se recrea.';
END
GO
