/*
    =====================================================================
    ALTERs para limitar Citas y Consultas al ÚLTIMO AÑO (calendar).
    =====================================================================
    Modifica los SPs sp_nh_GetAllCitas y sp_nh_GetAllConsultas para que
    solo devuelvan registros con fecha >= hace 1 año (incluye futuros).
    =====================================================================
*/

ALTER PROCEDURE [dbo].[sp_nh_GetAllCitas]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id, c.Estatus, c.FechaCita, c.PacienteID, c.Comentarios, c.TratamientoID, c.Estado,
        p.Nombre + ' ' + ISNULL(p.ApellidoPaterno, '') AS PacienteNombre,
        t.Nombre AS TratamientoNombre
    FROM dbo.Cita c
        LEFT JOIN dbo.Paciente p ON c.PacienteID = p.Id
        LEFT JOIN dbo.Tratamiento t ON c.TratamientoID = t.Id
    WHERE c.Estatus = 1
      AND c.FechaCita >= DATEADD(year, -1, GETDATE())
    ORDER BY c.FechaCita DESC;
END
GO

ALTER PROCEDURE [dbo].[sp_nh_GetAllConsultas]
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        c.Id, c.Estatus, c.FechaConsulta, c.PacienteID, c.TratamientoID, c.CitaID,
        c.Descripcion, c.TipoConsultaID, c.DescripcionReceta, c.Comentarios, c.FechaProximaConsulta,
        p.Nombre + ' ' + ISNULL(p.ApellidoPaterno, '') AS PacienteNombre,
        tc.Nombre AS TipoConsultaNombre
    FROM dbo.Consulta c
        LEFT JOIN dbo.Paciente p ON c.PacienteID = p.Id
        LEFT JOIN dbo.TipoConsulta tc ON c.TipoConsultaID = tc.Id
    WHERE c.Estatus = 1
      AND c.FechaConsulta >= DATEADD(year, -1, GETDATE())
    ORDER BY c.FechaConsulta DESC;
END
GO
