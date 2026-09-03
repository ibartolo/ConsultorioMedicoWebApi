/*
    =====================================================================
    SPs NUEVOS para el módulo Cita (migración WinForms -> WebApi)
    =====================================================================
    - sp_nh_GetAllCitas : listado de citas activas (con nombre de paciente y tratamiento).
    - sp_nh_GetCitaById : detalle de cita.
    - sp_nh_SaveCita    : upsert (reemplaza el SaveOrUpdate de NHibernate).
    Solo CREATE, sin modificar/eliminar objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetAllCitas', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetAllCitas]
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            c.Id, c.Estatus, c.FechaCita, c.PacienteID, c.Comentarios, c.TratamientoID, c.Estado,
            p.Nombre + '' '' + ISNULL(p.ApellidoPaterno, '''') AS PacienteNombre,
            t.Nombre AS TratamientoNombre
        FROM dbo.Cita c
            LEFT JOIN dbo.Paciente p ON c.PacienteID = p.Id
            LEFT JOIN dbo.Tratamiento t ON c.TratamientoID = t.Id
        WHERE c.Estatus = 1
        ORDER BY c.FechaCita DESC;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetAllCitas creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetAllCitas ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_GetCitaById', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetCitaById]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Estatus, FechaCita, PacienteID, Comentarios, TratamientoID, Estado
        FROM dbo.Cita
        WHERE Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetCitaById creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetCitaById ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_SaveCita', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_SaveCita]
    (
        @Id             INT,
        @Estatus        BIT,
        @FechaCita      DATETIME = NULL,
        @PacienteID     INT,
        @Comentarios    NVARCHAR(500) = NULL,
        @TratamientoID  INT = NULL,
        @Estado         INT = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM dbo.Cita WHERE Id = @Id)
        BEGIN
            UPDATE dbo.Cita
            SET Estatus = @Estatus,
                FechaCita = @FechaCita,
                PacienteID = @PacienteID,
                Comentarios = @Comentarios,
                TratamientoID = @TratamientoID,
                Estado = @Estado
            WHERE Id = @Id;

            SELECT @Id;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Cita
                (Estatus, FechaCita, PacienteID, Comentarios, TratamientoID, Estado)
            VALUES
                (@Estatus, @FechaCita, @PacienteID, @Comentarios, @TratamientoID, @Estado);

            SELECT SCOPE_IDENTITY();
        END
    END
    ');
    PRINT 'SP dbo.sp_nh_SaveCita creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_SaveCita ya existe. No se recrea.';
END
GO
