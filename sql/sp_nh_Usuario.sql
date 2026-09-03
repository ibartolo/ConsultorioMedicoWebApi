/*
    =====================================================================
    SPs NUEVOS para el módulo Usuario (migración WinForms -> WebApi)
    =====================================================================
    - sp_nh_GetAllUsuarios : listado de usuarios.
    - sp_nh_GetUsuarioById : detalle de usuario.
    - sp_nh_SaveUsuario    : upsert (reemplaza el SaveOrUpdate de NHibernate).
    Solo CREATE, sin modificar/eliminar objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetAllUsuarios', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetAllUsuarios]
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, UserName, Password, Estatus, CompaniaID
        FROM dbo.Usuario
        ORDER BY UserName;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetAllUsuarios creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetAllUsuarios ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_GetUsuarioById', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetUsuarioById]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, UserName, Password, Estatus, CompaniaID
        FROM dbo.Usuario
        WHERE Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetUsuarioById creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetUsuarioById ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_SaveUsuario', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_SaveUsuario]
    (
        @Id         INT,
        @UserName   NVARCHAR(50) = NULL,
        @Password   NVARCHAR(50) = NULL,
        @Estatus    BIT,
        @CompaniaID INT = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM dbo.Usuario WHERE Id = @Id)
        BEGIN
            UPDATE dbo.Usuario
            SET UserName = @UserName, Password = @Password, Estatus = @Estatus, CompaniaID = @CompaniaID
            WHERE Id = @Id;

            SELECT @Id;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Usuario (UserName, Password, Estatus, CompaniaID)
            VALUES (@UserName, @Password, @Estatus, @CompaniaID);

            SELECT SCOPE_IDENTITY();
        END
    END
    ');
    PRINT 'SP dbo.sp_nh_SaveUsuario creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_SaveUsuario ya existe. No se recrea.';
END
GO
