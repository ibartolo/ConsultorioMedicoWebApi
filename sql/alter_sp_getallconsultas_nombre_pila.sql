/*
    Ajusta sp_nh_GetAllConsultas para que PacienteNombre sea solo el nombre
    del paciente (sin apellidos). El calendario de consultas usa ese campo.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetAllConsultas]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id, c.Estatus, c.FechaConsulta, c.PacienteID, c.TratamientoID, c.CitaID,
        c.Descripcion, c.TipoConsultaID, c.DescripcionReceta, c.Comentarios, c.FechaProximaConsulta,
        p.Nombre AS PacienteNombre,
        tc.Nombre AS TipoConsultaNombre
    FROM dbo.Consulta c
        LEFT JOIN dbo.Paciente p ON c.PacienteID = p.Id
        LEFT JOIN dbo.TipoConsulta tc ON c.TipoConsultaID = tc.Id
    WHERE c.Estatus = 1
      AND c.ConsultorioID = @ConsultorioID
      AND c.FechaConsulta >= DATEADD(year, -1, GETDATE())
    ORDER BY c.FechaConsulta DESC;
END
GO
