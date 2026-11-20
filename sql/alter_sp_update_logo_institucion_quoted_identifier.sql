/*
    Corrige sp_nh_UpdateLogoInstitucion.
    El SP había quedado creado con QUOTED_IDENTIFIER OFF; al actualizar
    DatosInstitucion (que tiene índices filtrados) SQL Server rechaza el UPDATE.
    Se vuelve a crear/alterar con QUOTED_IDENTIFIER ON.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_UpdateLogoInstitucion]
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
GO
