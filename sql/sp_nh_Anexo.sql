/*
    =====================================================================
    SPs NUEVOS para el módulo Anexo (migración WinForms -> WebApi)
    =====================================================================
    - sp_nh_SaveAnexo    : inserta un anexo (reemplaza el Save de NHibernate).
    - sp_nh_GetAnexoById : obtiene un anexo por Id (para descarga).
    El listado por paciente reutiliza el SP existente GetAllAnexoByPacienteID.
    Solo CREATE, sin modificar/eliminar objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_SaveAnexo', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_SaveAnexo]
    (
        @Estatus        BIT,
        @Fecha          DATETIME = NULL,
        @Comentarios    NVARCHAR(1000) = NULL,
        @PacienteID     INT,
        @Nombre         NVARCHAR(50) = NULL,
        @Ruta           NVARCHAR(200) = NULL,
        @ConsultaID     INT = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        INSERT INTO dbo.Anexo (Estatus, Fecha, Comentarios, PacienteID, Nombre, Ruta, ConsultaID)
        VALUES (@Estatus, @Fecha, @Comentarios, @PacienteID, @Nombre, @Ruta, @ConsultaID);

        SELECT SCOPE_IDENTITY();
    END
    ');
    PRINT 'SP dbo.sp_nh_SaveAnexo creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_SaveAnexo ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_GetAnexoById', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetAnexoById]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Estatus, Fecha, Comentarios, PacienteID, Nombre, Ruta, ConsultaID
        FROM dbo.Anexo
        WHERE Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetAnexoById creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetAnexoById ya existe. No se recrea.';
END
GO
