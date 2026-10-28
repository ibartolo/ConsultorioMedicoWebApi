/*
    =====================================================================
    SPs NUEVOS para el módulo Paquetes del Expediente (solo lectura)
    =====================================================================
    - sp_nh_GetPaquetesPorPaciente     : paquetes adquiridos por el paciente
      (PacientePaquete JOIN Paquete, solo estatus activo).
    - sp_nh_GetCitasPorPacientePaquete : citas-tratamientos de un paquete
      (CitaPaquete JOIN Cita LEFT JOIN Tratamiento).
    Solo CREATE (guard + EXEC), sin modificar/eliminar objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetPaquetesPorPaciente', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetPaquetesPorPaciente]
    (
        @PacienteId INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT PP.Id, PP.PacienteID, PP.PaqueteID, P.Nombre AS PaqueteNombre
        FROM dbo.PacientePaquete PP
            INNER JOIN dbo.Paquete P ON P.Id = PP.PaqueteID
        WHERE PP.PacienteID = @PacienteId AND P.Estatus = 1;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetPaquetesPorPaciente creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetPaquetesPorPaciente ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_GetCitasPorPacientePaquete', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetCitasPorPacientePaquete]
    (
        @PacientePaqueteId INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT C.Id, C.FechaCita, T.Nombre AS TratamientoNombre, C.Comentarios,
               CP.Id AS CitaPaqueteID, CP.PacientePaqueteID
        FROM dbo.CitaPaquete CP
            INNER JOIN dbo.Cita C ON C.Id = CP.CitaID
            INNER JOIN dbo.PacientePaquete PP ON PP.Id = CP.PacientePaqueteID
            LEFT JOIN dbo.Tratamiento T ON T.Id = C.TratamientoID
        WHERE C.Estatus = 1 AND PP.Id = @PacientePaqueteId
        ORDER BY C.FechaCita DESC;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetCitasPorPacientePaquete creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetCitasPorPacientePaquete ya existe. No se recrea.';
END
GO
