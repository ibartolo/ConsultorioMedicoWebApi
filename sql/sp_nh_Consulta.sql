/*
    =====================================================================
    SPs NUEVOS para el módulo Consulta (migración WinForms -> WebApi)
    =====================================================================
    - sp_nh_GetAllConsultas : listado de consultas activas (con paciente y tipo de consulta).
    - sp_nh_GetConsultaById : detalle de consulta.
    - sp_nh_SaveConsulta    : upsert (reemplaza el SaveOrUpdate de NHibernate).
    Solo CREATE, sin modificar/eliminar objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetAllConsultas', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetAllConsultas]
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            c.Id, c.Estatus, c.FechaConsulta, c.PacienteID, c.TratamientoID, c.CitaID,
            c.Descripcion, c.TipoConsultaID, c.DescripcionReceta, c.Comentarios, c.FechaProximaConsulta,
            p.Nombre + '' '' + ISNULL(p.ApellidoPaterno, '''') AS PacienteNombre,
            tc.Nombre AS TipoConsultaNombre
        FROM dbo.Consulta c
            LEFT JOIN dbo.Paciente p ON c.PacienteID = p.Id
            LEFT JOIN dbo.TipoConsulta tc ON c.TipoConsultaID = tc.Id
        WHERE c.Estatus = 1
        ORDER BY c.FechaConsulta DESC;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetAllConsultas creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetAllConsultas ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_GetConsultaById', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetConsultaById]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;
        SELECT Id, Estatus, FechaConsulta, PacienteID, TratamientoID, CitaID,
               Descripcion, TipoConsultaID, DescripcionReceta, Comentarios, FechaProximaConsulta
        FROM dbo.Consulta
        WHERE Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetConsultaById creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetConsultaById ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_SaveConsulta', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_SaveConsulta]
    (
        @Id                     INT,
        @Estatus                BIT,
        @FechaConsulta          DATETIME = NULL,
        @PacienteID             INT,
        @TratamientoID          INT = NULL,
        @CitaID                 INT = NULL,
        @Descripcion            NVARCHAR(500) = NULL,
        @TipoConsultaID         INT = NULL,
        @DescripcionReceta      NVARCHAR(1000) = NULL,
        @Comentarios            NVARCHAR(500) = NULL,
        @FechaProximaConsulta   DATETIME = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM dbo.Consulta WHERE Id = @Id)
        BEGIN
            UPDATE dbo.Consulta
            SET Estatus = @Estatus,
                FechaConsulta = @FechaConsulta,
                PacienteID = @PacienteID,
                TratamientoID = @TratamientoID,
                CitaID = @CitaID,
                Descripcion = @Descripcion,
                TipoConsultaID = @TipoConsultaID,
                DescripcionReceta = @DescripcionReceta,
                Comentarios = @Comentarios,
                FechaProximaConsulta = @FechaProximaConsulta
            WHERE Id = @Id;

            SELECT @Id;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Consulta
                (Estatus, FechaConsulta, PacienteID, TratamientoID, CitaID, Descripcion, TipoConsultaID, DescripcionReceta, Comentarios, FechaProximaConsulta)
            VALUES
                (@Estatus, @FechaConsulta, @PacienteID, @TratamientoID, @CitaID, @Descripcion, @TipoConsultaID, @DescripcionReceta, @Comentarios, @FechaProximaConsulta);

            SELECT SCOPE_IDENTITY();
        END
    END
    ');
    PRINT 'SP dbo.sp_nh_SaveConsulta creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_SaveConsulta ya existe. No se recrea.';
END
GO
