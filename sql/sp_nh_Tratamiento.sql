/*
    =====================================================================
    SPs NUEVOS para el módulo Tratamiento (migración WinForms -> WebApi)
    =====================================================================
    - sp_nh_GetTratamientoById : detalle de tratamiento.
    - sp_nh_SaveTratamiento    : upsert (reemplaza el SaveOrUpdate de NHibernate).
    Solo CREATE, sin modificar/eliminar objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetTratamientoById', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetTratamientoById]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Nombre, Duracion, Descripcion, Estatus
        FROM dbo.Tratamiento
        WHERE Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetTratamientoById creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetTratamientoById ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_SaveTratamiento', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_SaveTratamiento]
    (
        @Id         INT,
        @Nombre     VARCHAR(100) = NULL,
        @Duracion   INT = NULL,
        @Descripcion VARCHAR(500) = NULL,
        @Estatus    BIT = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM dbo.Tratamiento WHERE Id = @Id)
        BEGIN
            UPDATE dbo.Tratamiento
            SET Nombre = @Nombre, Duracion = @Duracion, Descripcion = @Descripcion, Estatus = @Estatus
            WHERE Id = @Id;

            SELECT @Id;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Tratamiento (Nombre, Duracion, Descripcion, Estatus)
            VALUES (@Nombre, @Duracion, @Descripcion, @Estatus);

            SELECT SCOPE_IDENTITY();
        END
    END
    ');
    PRINT 'SP dbo.sp_nh_SaveTratamiento creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_SaveTratamiento ya existe. No se recrea.';
END
GO
