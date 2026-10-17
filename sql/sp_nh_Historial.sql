/*
    =====================================================================
    SPs NUEVOS para el módulo Historial (migración WinForms -> WebApi)
    =====================================================================
    - sp_nh_GetTratamientosPorPaciente : historial de tratamientos del paciente
      derivado SOLO de Cita (con nombre del tratamiento y origen 'Cita').
      Coherente con la app WinForms original (dgvCita -> GetAllCitaByPacienteID).
    Patrón idempotente DROP+CREATE para re-aplicar cambios en BD.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetTratamientosPorPaciente', 'P') IS NOT NULL
BEGIN
    DROP PROCEDURE dbo.sp_nh_GetTratamientosPorPaciente;
END
GO

CREATE PROCEDURE [dbo].[sp_nh_GetTratamientosPorPaciente]
(
    @IdPaciente INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Id, c.FechaCita AS Fecha, t.Nombre AS TratamientoNombre,
           'Cita' AS Origen, c.Comentarios
    FROM dbo.Cita c
        LEFT JOIN dbo.Tratamiento t ON c.TratamientoID = t.Id
    WHERE c.PacienteID = @IdPaciente AND c.Estatus = 1
    ORDER BY c.FechaCita DESC;
END
GO
