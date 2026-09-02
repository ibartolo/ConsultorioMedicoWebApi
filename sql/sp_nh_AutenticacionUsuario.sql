/*
    =====================================================================
    SP NUEVO: sp_nh_AutenticacionUsuario
    =====================================================================
    Migración WinForms -> WebApi. Reemplaza la autenticación que la app
    WinForms hacía por NHibernate (CreateCriteria):

        session.CreateCriteria(Usuario)
              .Add(Restrictions.Eq("UserName", userName))
              .Add(Restrictions.Eq("Password", password))
              .List()

    FIDELIDAD con la aplicación actual (requisito explícito):
      - Compara UserName + Password en TEXTO PLANO.
      - NO valida Estatus (un usuario inactivo con password correcto entraría,
        igual que en la app actual).
      - Devuelve la fila completa de Usuario.

    PROTECCIÓN: solo ejecuta CREATE si el SP no existe. NO modifica ni elimina
    ningún objeto existente de la base de datos productiva.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_AutenticacionUsuario', 'P') IS NOT NULL
BEGIN
    PRINT 'El SP dbo.sp_nh_AutenticacionUsuario ya existe. No se recrea (protección de no-sobrescritura).';
END
ELSE
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_AutenticacionUsuario]
    (
        @UserName   NVARCHAR(50),
        @Password   NVARCHAR(50)
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT TOP 1
            Id,
            UserName,
            Password,
            Estatus,
            CompaniaID
        FROM dbo.Usuario
        WHERE UserName = @UserName AND Password = @Password
        ORDER BY Id ASC;
    END
    ');
    PRINT 'SP dbo.sp_nh_AutenticacionUsuario creado correctamente.';
END
GO
