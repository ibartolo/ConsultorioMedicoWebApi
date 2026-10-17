/*
    =====================================================================
    SPs NUEVOS para el módulo Paciente (migración WinForms -> WebApi)
    =====================================================================
    Regla: NHibernate muere -> todo por SPs.
      - Si la WinForms usaba un SP existente -> se reutiliza.
      - Si usaba NHibernate -> SP nuevo con prefijo sp_nh_ + nombre del método.

    Listado  -> reutiliza el SP EXISTENTE dbo.GetAllPacienteByEstatus (ya lo usaba la WinForms).
    Detalle  -> sp_nh_GetPacienteById  (reemplaza PacienteBL.GetPacienteById, NHibernate Get<T>).
    Guardar  -> sp_nh_SavePaciente     (reemplaza PacienteBL.SavePaciente, NHibernate SaveOrUpdate).
    Eliminar -> sp_nh_DeletePaciente   (baja lógica: UPDATE Estatus = 0).

    FIDELIDAD: sp_nh_SavePaciente NO escribe las columnas de auditoría (CreatedBY/DT, UpdatedBY/DT),
    igual que lo hacía NHibernate en la app actual (quedan NULL).

    PROTECCIÓN: solo CREATE si no existe. NO modifica ni elimina objetos existentes.
    =====================================================================
*/

IF OBJECT_ID('dbo.sp_nh_GetPacienteById', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_GetPacienteById]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        SELECT
            Id, Nombre, ApellidoPaterno, ApellidoMaterno, Email, FechaAlta,
            TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento
        FROM dbo.Paciente
        WHERE Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_GetPacienteById creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_GetPacienteById ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_SavePaciente', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_SavePaciente]
    (
        @Id                 INT,
        @Nombre             NVARCHAR(50) = NULL,
        @ApellidoPaterno    NVARCHAR(50) = NULL,
        @ApellidoMaterno    NVARCHAR(50) = NULL,
        @Email              NVARCHAR(50) = NULL,
        @FechaAlta          DATETIME = NULL,
        @TelefonoCel        NVARCHAR(100) = NULL,
        @Comentarios        NVARCHAR(1000) = NULL,
        @Estatus            BIT = NULL,
        @Edad               INT = NULL,
        @Sexo               NVARCHAR(15) = NULL,
        @FechaNacimiento    DATETIME = NULL
    )
    AS
    BEGIN
        SET NOCOUNT ON;

        IF EXISTS (SELECT 1 FROM dbo.Paciente WHERE Id = @Id)
        BEGIN
            UPDATE dbo.Paciente
            SET Nombre = @Nombre,
                ApellidoPaterno = @ApellidoPaterno,
                ApellidoMaterno = @ApellidoMaterno,
                Email = @Email,
                FechaAlta = @FechaAlta,
                TelefonoCel = @TelefonoCel,
                Comentarios = @Comentarios,
                Estatus = @Estatus,
                Edad = @Edad,
                Sexo = @Sexo,
                FechaNacimiento = @FechaNacimiento
            WHERE Id = @Id;

            SELECT @Id;
        END
        ELSE
        BEGIN
            INSERT INTO dbo.Paciente
                (Nombre, ApellidoPaterno, ApellidoMaterno, Email, FechaAlta, TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento)
            VALUES
                (@Nombre, @ApellidoPaterno, @ApellidoMaterno, @Email, @FechaAlta, @TelefonoCel, @Comentarios, @Estatus, @Edad, @Sexo, @FechaNacimiento);

            SELECT SCOPE_IDENTITY();
        END
    END
    ');
    PRINT 'SP dbo.sp_nh_SavePaciente creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_SavePaciente ya existe. No se recrea.';
END
GO

IF OBJECT_ID('dbo.sp_nh_DeletePaciente', 'P') IS NULL
BEGIN
    EXEC('
    CREATE PROCEDURE [dbo].[sp_nh_DeletePaciente]
    (
        @Id INT
    )
    AS
    BEGIN
        SET NOCOUNT ON;
        UPDATE dbo.Paciente SET Estatus = 0 WHERE Id = @Id;
    END
    ');
    PRINT 'SP dbo.sp_nh_DeletePaciente creado correctamente.';
END
ELSE
BEGIN
    PRINT 'El SP dbo.sp_nh_DeletePaciente ya existe. No se recrea.';
END
GO
