/*
    Actualiza GetAllConsultaByPacienteID para devolver también Comentarios
    y FechaProximaConsulta, necesarios para editar una consulta desde Expediente
    sin borrar esos campos.
*/
SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO

CREATE OR ALTER PROCEDURE [dbo].[GetAllConsultaByPacienteID]
(
    @IdPaciente int,
    @ConsultorioID int
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT CO.Id,
           CO.Estatus,
           CO.FechaConsulta,
           CO.Descripcion,
           CO.PacienteID,
           CO.TratamientoID,
           CO.CitaID,
           CO.TipoConsultaID,
           TC.Nombre,
           CO.DescripcionReceta,
           CO.Comentarios,
           CO.FechaProximaConsulta
    FROM Consulta CO
        INNER JOIN Paciente P ON P.Id = CO.PacienteID
        LEFT JOIN TipoConsulta TC ON TC.Id = CO.TipoConsultaID
    WHERE CO.PacienteID = @IdPaciente AND CO.Estatus = 1
      AND CO.ConsultorioID = @ConsultorioID
    ORDER BY CO.FechaConsulta DESC;
END
GO
