/*
    =====================================================================
    Aumenta la longitud de los campos de dbo.Anexo y del SP de guardado.
    - Nombre      : NVARCHAR(50)   -> NVARCHAR(150)
    - Comentarios : NVARCHAR(1000) -> NVARCHAR(1500)
    =====================================================================
*/

ALTER TABLE dbo.Anexo ALTER COLUMN Nombre NVARCHAR(150) NULL;
ALTER TABLE dbo.Anexo ALTER COLUMN Comentarios NVARCHAR(1500) NULL;
GO

ALTER PROCEDURE [dbo].[sp_nh_SaveAnexo]
(
    @Estatus        BIT,
    @Fecha          DATETIME = NULL,
    @Comentarios    NVARCHAR(1500) = NULL,
    @PacienteID     INT,
    @Nombre         NVARCHAR(150) = NULL,
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
GO
