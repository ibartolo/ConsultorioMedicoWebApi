/*
    =====================================================================
    MIGRACIÓN SaaS #2 — PROCEDIMIENTOS ALMACENADOS (multi-tenant)
    =====================================================================
    Proyecto : ConsultorioMedico (SaaS por consultorio)
    Cambio   : saas-registro-aislamiento
    Objetivo : Añadir aislamiento por consultorio a los SPs existentes
               (+@ConsultorioID / +@CompaniaID) y crear los SPs nuevos
               de TipoConsulta y del rehash de contraseña.

    Estrategia:
      - SQL Server 2022 (ProductMajorVersion = 16) -> se usa
        CREATE OR ALTER PROCEDURE (idempotente, sin DROP).
      - Los SPs existentes se redefinen preservando su comportamiento
        actual y añadiendo ÚNICAMENTE el filtro/inserción por tenant.
      - sp_nh_AutenticacionUsuario se redefine: filtra solo por UserName,
        devuelve Password y Estatus, y se elimina la comparación de
        password en SQL (la verificación pasa a C# con PBKDF2).
      - El parámetro de tenant (@ConsultorioID / @CompaniaID) es OBLIGATORIO
        (sin default): lo inyecta el servicio desde el claim, nunca el cliente.

    ⚠  IMPORTANTE:
      - SOLO para la BD del clon develop/main (db_9c7990_consultoriodesitst).
      - Ejecutar DESPUÉS de migraciones_saas_01_esquema.sql.
      - Re-ejecutable sin error. Sin DROP de tablas ni SPs.
    =====================================================================
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

/* =====================================================================
   2.1  CITAS
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetAllCitas]
(
    @ConsultorioID INT
)
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
      AND c.ConsultorioID = @ConsultorioID
      AND c.FechaCita >= DATEADD(year, -1, GETDATE())
    ORDER BY c.FechaCita DESC;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetCitaById]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Estatus, FechaCita, PacienteID, Comentarios, TratamientoID, Estado
    FROM dbo.Cita
    WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SaveCita]
(
    @Id             INT,
    @Estatus        BIT,
    @FechaCita      DATETIME = NULL,
    @PacienteID     INT,
    @Comentarios    NVARCHAR(500) = NULL,
    @TratamientoID  INT = NULL,
    @Estado         INT = NULL,
    @ConsultorioID  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Paciente WHERE Id = @PacienteID AND ConsultorioID = @ConsultorioID)
    BEGIN
        SELECT CAST(NULL AS INT);
        RETURN;
    END

    IF @TratamientoID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Tratamiento WHERE Id = @TratamientoID AND ConsultorioID = @ConsultorioID)
    BEGIN
        SELECT CAST(NULL AS INT);
        RETURN;
    END

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Cita
            (Estatus, FechaCita, PacienteID, Comentarios, TratamientoID, Estado, ConsultorioID)
        VALUES
            (@Estatus, @FechaCita, @PacienteID, @Comentarios, @TratamientoID, @Estado, @ConsultorioID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Cita
        SET Estatus = @Estatus,
            FechaCita = @FechaCita,
            PacienteID = @PacienteID,
            Comentarios = @Comentarios,
            TratamientoID = @TratamientoID,
            Estado = @Estado
        WHERE Id = @Id AND ConsultorioID = @ConsultorioID;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(NULL AS INT);
            RETURN;
        END

        SELECT @Id;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_DeleteCita]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    -- Baja lógica acotada al consultorio (nunca borrado físico).
    UPDATE dbo.Cita SET Estatus = 0 WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetAllCitaByPacienteID]
(
    @IdPaciente int,
    @ConsultorioID int
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT C.[Id], C.[FechaCita], C.[Comentarios], T.[Nombre],
           C.[Estatus], C.[PacienteID], C.[TratamientoID], C.[Estado]
    FROM Cita C
        INNER JOIN Paciente P ON P.Id = C.PacienteID
        LEFT JOIN Tratamiento T ON T.Id = C.TratamientoID
    WHERE C.PacienteID = @IdPaciente AND C.Estatus = 1 AND P.Estatus = 1
      AND C.ConsultorioID = @ConsultorioID
    ORDER BY C.[FechaCita] DESC;
END
GO

/* =====================================================================
   2.2  CONSULTAS
   ===================================================================== */
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
        p.Nombre + ' ' + ISNULL(p.ApellidoPaterno, '') AS PacienteNombre,
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

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetConsultaById]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Estatus, FechaConsulta, PacienteID, TratamientoID, CitaID,
           Descripcion, TipoConsultaID, DescripcionReceta, Comentarios, FechaProximaConsulta
    FROM dbo.Consulta
    WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SaveConsulta]
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
    @FechaProximaConsulta   DATETIME = NULL,
    @ConsultorioID          INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Paciente WHERE Id = @PacienteID AND ConsultorioID = @ConsultorioID)
    BEGIN
        SELECT CAST(NULL AS INT);
        RETURN;
    END

    IF @TratamientoID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Tratamiento WHERE Id = @TratamientoID AND ConsultorioID = @ConsultorioID)
    BEGIN
        SELECT CAST(NULL AS INT);
        RETURN;
    END

    IF @CitaID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Cita WHERE Id = @CitaID AND ConsultorioID = @ConsultorioID)
    BEGIN
        SELECT CAST(NULL AS INT);
        RETURN;
    END

    IF @TipoConsultaID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.TipoConsulta WHERE Id = @TipoConsultaID AND ConsultorioID = @ConsultorioID)
    BEGIN
        SELECT CAST(NULL AS INT);
        RETURN;
    END

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Consulta
            (Estatus, FechaConsulta, PacienteID, TratamientoID, CitaID, Descripcion, TipoConsultaID, DescripcionReceta, Comentarios, FechaProximaConsulta, ConsultorioID)
        VALUES
            (@Estatus, @FechaConsulta, @PacienteID, @TratamientoID, @CitaID, @Descripcion, @TipoConsultaID, @DescripcionReceta, @Comentarios, @FechaProximaConsulta, @ConsultorioID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
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
        WHERE Id = @Id AND ConsultorioID = @ConsultorioID;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(NULL AS INT);
            RETURN;
        END

        SELECT @Id;
    END
END
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
           CO.DescripcionReceta
    FROM Consulta CO
        INNER JOIN Paciente P ON P.Id = CO.PacienteID
        LEFT JOIN TipoConsulta TC ON TC.Id = CO.TipoConsultaID
    WHERE CO.PacienteID = @IdPaciente AND CO.Estatus = 1
      AND CO.ConsultorioID = @ConsultorioID
    ORDER BY CO.FechaConsulta DESC;
END
GO

/* =====================================================================
   2.3  PACIENTES
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetPacienteById]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id, Nombre, ApellidoPaterno, ApellidoMaterno, Email, FechaAlta,
        TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento
    FROM dbo.Paciente
    WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SavePaciente]
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
    @FechaNacimiento    DATETIME = NULL,
    @ConsultorioID      INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Paciente
            (Nombre, ApellidoPaterno, ApellidoMaterno, Email, FechaAlta, TelefonoCel, Comentarios, Estatus, Edad, Sexo, FechaNacimiento, ConsultorioID)
        VALUES
            (@Nombre, @ApellidoPaterno, @ApellidoMaterno, @Email, @FechaAlta, @TelefonoCel, @Comentarios, @Estatus, @Edad, @Sexo, @FechaNacimiento, @ConsultorioID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
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
        WHERE Id = @Id AND ConsultorioID = @ConsultorioID;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(NULL AS INT);
            RETURN;
        END

        SELECT @Id;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_DeletePaciente]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    -- Baja lógica acotada al consultorio.
    UPDATE dbo.Paciente SET Estatus = 0 WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetAllPacienteByEstatus]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.[Id], P.[Nombre], P.[ApellidoPaterno], P.[ApellidoMaterno],
           P.[Nombre] + ' ' + P.ApellidoPaterno + ' ' + P.ApellidoMaterno AS NombreCompleto,
           ((cast(datediff(dd, P.FechaNacimiento, GETDATE()) / 365.25 as int))) as Edad,
           P.[Sexo], P.[Email], P.[FechaAlta], P.[TelefonoCel],
           P.Estatus, P.Comentarios, P.FechaNacimiento
    FROM Paciente P
    WHERE P.Estatus = 1
      AND P.ConsultorioID = @ConsultorioID
    ORDER BY P.Id DESC;
END
GO

/* =====================================================================
   2.4  USUARIOS
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetAllUsuarios]
(
    @CompaniaID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserName, Estatus, CompaniaID
    FROM dbo.Usuario
    WHERE CompaniaID = @CompaniaID
    ORDER BY UserName;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetUsuarioById]
(
    @Id INT,
    @CompaniaID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserName, Estatus, CompaniaID
    FROM dbo.Usuario
    WHERE Id = @Id AND CompaniaID = @CompaniaID;
END
GO

-- Sin cambio de firma (el servicio asigna CompaniaID del claim y hashea).
-- Se amplía @Password a NVARCHAR(250) para alojar el hash PBKDF2.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SaveUsuario]
(
    @Id         INT,
    @UserName   NVARCHAR(50) = NULL,
    @Password   NVARCHAR(250) = NULL,
    @Estatus    BIT,
    @CompaniaID INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Usuario (UserName, Password, Estatus, CompaniaID)
        VALUES (@UserName, @Password, @Estatus, @CompaniaID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Usuario
        SET UserName = @UserName,
            Password = ISNULL(@Password, Password),
            Estatus = @Estatus
        WHERE Id = @Id AND CompaniaID = @CompaniaID;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(NULL AS INT);
            RETURN;
        END

        SELECT @Id;
    END
END
GO

/* =====================================================================
   2.5  DATOS FISCALES (+@ConsultorioID = DatosInstitucionID)
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetDatosFiscales]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DF.Id, DF.RazonSocial, DF.RFC, DF.Telefono1, DF.Telefono2, DF.Email,
           DF.Calle, DF.NoInterior, DF.NoExterior, DF.Colonia, DF.Deleacion,
           DF.Municipio, DF.Estado, DF.CP, DF.Pais, DF.DatosInstitucionID,
           DI.Nombre AS DatosInstitucionNombre
    FROM dbo.DatosFiscales DF
        LEFT JOIN dbo.DatosInstitucion DI ON DI.Id = DF.DatosInstitucionID
    WHERE DF.DatosInstitucionID = @ConsultorioID
    ORDER BY DF.RazonSocial;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetDatosFiscalById]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT DF.Id, DF.RazonSocial, DF.RFC, DF.Telefono1, DF.Telefono2, DF.Email,
           DF.Calle, DF.NoInterior, DF.NoExterior, DF.Colonia, DF.Deleacion,
           DF.Municipio, DF.Estado, DF.CP, DF.Pais, DF.DatosInstitucionID,
           DI.Nombre AS DatosInstitucionNombre
    FROM dbo.DatosFiscales DF
        LEFT JOIN dbo.DatosInstitucion DI ON DI.Id = DF.DatosInstitucionID
    WHERE DF.Id = @Id AND DF.DatosInstitucionID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SaveDatosFiscal]
(
    @Id                 INT,
    @RazonSocial        NVARCHAR(100) = NULL,
    @RFC                NVARCHAR(100) = NULL,
    @Telefono1          NVARCHAR(50)  = NULL,
    @Telefono2          NVARCHAR(50)  = NULL,
    @Email              NVARCHAR(100) = NULL,
    @Calle              NVARCHAR(50)  = NULL,
    @NoInterior         NVARCHAR(10)  = NULL,
    @NoExterior         NVARCHAR(10)  = NULL,
    @Colonia            NVARCHAR(50)  = NULL,
    @Deleacion          NVARCHAR(50)  = NULL,
    @Municipio          NVARCHAR(50)  = NULL,
    @Estado             NVARCHAR(50)  = NULL,
    @CP                 INT           = NULL,
    @Pais               NVARCHAR(50)  = NULL,
    @ConsultorioID      INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.DatosFiscales
        (RazonSocial, RFC, Telefono1, Telefono2, Email, Calle, NoInterior, NoExterior,
         Colonia, Deleacion, Municipio, Estado, CP, Pais, DatosInstitucionID)
        VALUES
        (@RazonSocial, @RFC, @Telefono1, @Telefono2, @Email, @Calle, @NoInterior, @NoExterior,
         @Colonia, @Deleacion, @Municipio, @Estado, @CP, @Pais, @ConsultorioID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.DatosFiscales
           SET RazonSocial = @RazonSocial,
               RFC = @RFC,
               Telefono1 = @Telefono1,
               Telefono2 = @Telefono2,
               Email = @Email,
               Calle = @Calle,
               NoInterior = @NoInterior,
               NoExterior = @NoExterior,
               Colonia = @Colonia,
               Deleacion = @Deleacion,
               Municipio = @Municipio,
               Estado = @Estado,
               CP = @CP,
               Pais = @Pais
         WHERE Id = @Id AND DatosInstitucionID = @ConsultorioID;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(NULL AS INT);
            RETURN;
        END

        SELECT @Id;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_DeleteDatosFiscal]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    DELETE FROM dbo.DatosFiscales
    WHERE Id = @Id AND DatosInstitucionID = @ConsultorioID;

    SELECT @@ROWCOUNT;
END
GO

/* =====================================================================
   2.6  DATOS INSTITUCIÓN (se resuelve por @Id = tenant)
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[GetDatosInstitucion]
(
    @Id INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM DatosInstitucion
    WHERE Id = @Id;
END
GO

-- Resuelve por @Id del tenant (el servicio fija @Id desde el claim).
-- La ruta de inserción devuelve el nuevo Id (SCOPE_IDENTITY).
CREATE OR ALTER PROCEDURE [dbo].[SaveOrUpdateDatosInstitucion]
(
    @Id int,
    @Nombre nvarchar(100) = NULL,
    @NoTrabajadores int = NULL,
    @Telefono1 nvarchar(50) = NULL,
    @Telefono2 nvarchar(50) = NULL,
    @Email nvarchar(100) = NULL,
    @Calle nvarchar(50) = NULL,
    @NoInterior nvarchar(10) = NULL,
    @NoExterior nvarchar(10) = NULL,
    @Colonia nvarchar(50) = NULL,
    @Deleacion nvarchar(50) = NULL,
    @Municipio nvarchar(50) = NULL,
    @Estado nvarchar(50) = NULL,
    @CP int = NULL,
    @Pais nvarchar(50) = NULL,
    @MedicoNombre nvarchar(100) = NULL,
    @MedicoCedula nvarchar(50) = NULL,
    @MedicoEspecialidad nvarchar(100) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF EXISTS(SELECT * FROM DatosInstitucion WHERE Id = @Id)
    BEGIN
        UPDATE DatosInstitucion
           SET Nombre = @Nombre,
               NoTrabajadores = @NoTrabajadores,
               Telefono1 = @Telefono1,
               Telefono2 = @Telefono2,
               Email = @Email,
               Calle = @Calle,
               NoInterior = @NoInterior,
               NoExterior = @NoExterior,
               Colonia = @Colonia,
               Deleacion = @Deleacion,
               Municipio = @Municipio,
               Estado = @Estado,
               CP = @CP,
               Pais = @Pais,
               MedicoNombre = @MedicoNombre,
               MedicoCedula = @MedicoCedula,
               MedicoEspecialidad = @MedicoEspecialidad
         WHERE Id = @Id;

        SELECT @Id;
    END
    ELSE
    BEGIN
        INSERT INTO DatosInstitucion
        (Nombre, NoTrabajadores, Telefono1, Telefono2, Email, Calle, NoInterior, NoExterior,
         Colonia, Deleacion, Municipio, Estado, CP, Pais,
         MedicoNombre, MedicoCedula, MedicoEspecialidad)
        VALUES
        (@Nombre, @NoTrabajadores, @Telefono1, @Telefono2, @Email, @Calle, @NoInterior, @NoExterior,
         @Colonia, @Deleacion, @Municipio, @Estado, @CP, @Pais,
         @MedicoNombre, @MedicoCedula, @MedicoEspecialidad);

        SELECT SCOPE_IDENTITY();
    END
END
GO

/* =====================================================================
   2.7  ANEXOS
        NOTA: el listado por paciente es SQL inline en DbWrapper.Anexo.cs
        (código C#, se ajusta en un batch posterior — no es un SP).
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetAnexoById]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Estatus, Fecha, Comentarios, PacienteID, Nombre, Ruta, ConsultaID
    FROM dbo.Anexo
    WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SaveAnexo]
(
    @Estatus        BIT,
    @Fecha          DATETIME = NULL,
    @Comentarios    NVARCHAR(1500) = NULL,
    @PacienteID     INT,
    @Nombre         NVARCHAR(150) = NULL,
    @Ruta           NVARCHAR(200) = NULL,
    @ConsultaID     INT = NULL,
    @ConsultorioID  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.Paciente WHERE Id = @PacienteID AND ConsultorioID = @ConsultorioID)
    BEGIN
        SELECT CAST(NULL AS INT);
        RETURN;
    END

    IF @ConsultaID IS NOT NULL
       AND NOT EXISTS (SELECT 1 FROM dbo.Consulta WHERE Id = @ConsultaID AND ConsultorioID = @ConsultorioID)
    BEGIN
        SELECT CAST(NULL AS INT);
        RETURN;
    END

    INSERT INTO dbo.Anexo (Estatus, Fecha, Comentarios, PacienteID, Nombre, Ruta, ConsultaID, ConsultorioID)
    VALUES (@Estatus, @Fecha, @Comentarios, @PacienteID, @Nombre, @Ruta, @ConsultaID, @ConsultorioID);

    SELECT SCOPE_IDENTITY();
END
GO

/* =====================================================================
   2.8  PAQUETES, TRATAMIENTOS, TIPO CONSULTA, TRATAMIENTOS POR PACIENTE
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetPaqueteById]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Descripcion, FechaCreacion, Estatus, Costo
    FROM dbo.Paquete
    WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SavePaquete]
(
    @Id             INT,
    @Nombre         NVARCHAR(100) = NULL,
    @Descripcion    NVARCHAR(500) = NULL,
    @FechaCreacion  DATETIME = NULL,
    @Estatus        BIT = NULL,
    @Costo          INT = NULL,
    @ConsultorioID  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Paquete (Nombre, Descripcion, FechaCreacion, Estatus, Costo, ConsultorioID)
        VALUES (@Nombre, @Descripcion, @FechaCreacion, @Estatus, @Costo, @ConsultorioID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Paquete
        SET Nombre = @Nombre, Descripcion = @Descripcion, FechaCreacion = @FechaCreacion,
            Estatus = @Estatus, Costo = @Costo
        WHERE Id = @Id AND ConsultorioID = @ConsultorioID;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(NULL AS INT);
            RETURN;
        END

        SELECT @Id;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetAllPaqueteDT]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.Id, P.Nombre, P.Descripcion, P.FechaCreacion, P.Estatus, P.Costo
    FROM Paquete AS P
    WHERE P.Estatus = 1
      AND P.ConsultorioID = @ConsultorioID
    ORDER BY P.Id DESC;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetTratamientoById]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Duracion, Descripcion, Estatus
    FROM dbo.Tratamiento
    WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SaveTratamiento]
(
    @Id             INT,
    @Nombre         VARCHAR(100) = NULL,
    @Duracion       INT = NULL,
    @Descripcion    VARCHAR(500) = NULL,
    @Estatus        BIT = NULL,
    @ConsultorioID  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Tratamiento (Nombre, Duracion, Descripcion, Estatus, ConsultorioID)
        VALUES (@Nombre, @Duracion, @Descripcion, @Estatus, @ConsultorioID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Tratamiento
        SET Nombre = @Nombre, Duracion = @Duracion, Descripcion = @Descripcion, Estatus = @Estatus
        WHERE Id = @Id AND ConsultorioID = @ConsultorioID;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(NULL AS INT);
            RETURN;
        END

        SELECT @Id;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetAllTratamiento]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM Tratamiento
    WHERE Estatus = 1
      AND ConsultorioID = @ConsultorioID
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[GetAllTipoConsulta]
(
    @ConsultorioID INT,
    @SoloActivos  BIT = 1
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT *
    FROM TipoConsulta
    WHERE (@SoloActivos = 0 OR Estatus = 1)
      AND ConsultorioID = @ConsultorioID
    ORDER BY Nombre;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetTratamientosPorPaciente]
(
    @IdPaciente INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT c.Id, c.FechaCita AS Fecha, t.Nombre AS TratamientoNombre,
           'Cita' AS Origen, c.Comentarios
    FROM dbo.Cita c
        LEFT JOIN dbo.Tratamiento t ON c.TratamientoID = t.Id
            AND (t.Id IS NULL OR t.ConsultorioID = @ConsultorioID)
    WHERE c.PacienteID = @IdPaciente AND c.Estatus = 1
      AND c.ConsultorioID = @ConsultorioID
    ORDER BY c.FechaCita DESC;
END
GO

/* =====================================================================
   2.9  RECETA + AUTENTICACIÓN
   ===================================================================== */
-- Conserva @institucionId (el servicio lo fija desde el claim) y valida
-- que la consulta pertenezca al mismo consultorio.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetRecetaByConsulta]
(
    @consultaID INT,
    @institucionId INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT P.Nombre + ' ' + P.ApellidoPaterno + ' ' + ISNULL(P.ApellidoMaterno, '') AS Paciente,
           P.Edad,
           P.Sexo,
           C.FechaConsulta,
           ISNULL(C.Descripcion, '') AS Consulta,
           ISNULL(C.DescripcionReceta, '') AS Medicamento,
           ISNULL(T.Nombre, '') AS TipoConsulta,
           C.FechaProximaConsulta AS ProximaConsulta,
           DI.Nombre AS InstitucionNombre,
           DI.Calle AS InstitucionCalle,
           DI.NoInterior AS InstitucionNoInterior,
           DI.NoExterior AS InstitucionNoExterior,
           DI.Colonia AS InstitucionColonia,
           DI.Deleacion AS InstitucionDeleacion,
           DI.Municipio AS InstitucionMunicipio,
           DI.Estado AS InstitucionEstado,
           DI.CP AS InstitucionCP,
           DI.Pais AS InstitucionPais,
           DI.Telefono1 AS InstitucionTelefono1,
           DI.Telefono2 AS InstitucionTelefono2,
           DI.Email AS InstitucionEmail,
           DI.LogoRuta,
           DI.MedicoNombre,
           DI.MedicoCedula,
           DI.MedicoEspecialidad
    FROM Paciente AS P
        INNER JOIN Consulta AS C ON P.Id = C.PacienteID
        LEFT JOIN TipoConsulta AS T ON C.TipoConsultaID = T.Id
        LEFT JOIN DatosInstitucion AS DI ON DI.Id = @institucionId
    WHERE C.Id = @consultaID
      AND C.ConsultorioID = @institucionId;
END
GO

-- Redefinido: filtra SOLO por UserName (sin filtrar Estatus), devuelve
-- Password y Estatus; la verificación de contraseña se hace en C# (PBKDF2).
-- Se elimina el parámetro @Password (la comparación ya no ocurre en SQL).
-- ATENCIÓN: el código C# (Phase 4) debe llamar este SP solo con @UserName.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_AutenticacionUsuario]
(
    @UserName NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        Id,
        UserName,
        Password,
        Estatus,
        CompaniaID
    FROM dbo.Usuario
    WHERE UserName = @UserName
    ORDER BY Id ASC;
END
GO

/* =====================================================================
   2.10  SPs NUEVOS
   ===================================================================== */
-- Lectura de paquetes del paciente (filtrado por consultorio).
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetPaquetesPorPaciente]
(
    @PacienteId INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT PP.Id, PP.PacienteID, PP.PaqueteID, P.Nombre AS PaqueteNombre
    FROM dbo.PacientePaquete PP
        INNER JOIN dbo.Paquete P ON P.Id = PP.PaqueteID
    WHERE PP.PacienteID = @PacienteId AND P.Estatus = 1
      AND PP.ConsultorioID = @ConsultorioID;
END
GO

-- Citas-tratamientos de un paquete del paciente (filtrado por consultorio).
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetCitasPorPacientePaquete]
(
    @PacientePaqueteId INT,
    @ConsultorioID INT
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
      AND PP.ConsultorioID = @ConsultorioID
      AND C.ConsultorioID = @ConsultorioID
      AND CP.ConsultorioID = @ConsultorioID
    ORDER BY C.FechaCita DESC;
END
GO

-- CRUD TipoConsulta por consultorio (nuevos).
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetTipoConsultaById]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, Nombre, Duracion, Descripcion, Estatus
    FROM dbo.TipoConsulta
    WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SaveTipoConsulta]
(
    @Id             INT,
    @Nombre         NVARCHAR(50) = NULL,
    @Duracion       INT = NULL,
    @Descripcion    NVARCHAR(500) = NULL,
    @Estatus        BIT = NULL,
    @ConsultorioID  INT
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.TipoConsulta (Nombre, Duracion, Descripcion, Estatus, ConsultorioID)
        VALUES (@Nombre, @Duracion, @Descripcion, @Estatus, @ConsultorioID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.TipoConsulta
        SET Nombre = @Nombre, Duracion = @Duracion, Descripcion = @Descripcion, Estatus = @Estatus
        WHERE Id = @Id AND ConsultorioID = @ConsultorioID;

        IF @@ROWCOUNT = 0
        BEGIN
            SELECT CAST(NULL AS INT);
            RETURN;
        END

        SELECT @Id;
    END
END
GO

CREATE OR ALTER PROCEDURE [dbo].[sp_nh_DeleteTipoConsulta]
(
    @Id INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    -- Baja lógica (Estatus = 0), acotada al consultorio.
    UPDATE dbo.TipoConsulta SET Estatus = 0 WHERE Id = @Id AND ConsultorioID = @ConsultorioID;
END
GO

-- Rehash transparente: actualiza solo la contraseña (PBKDF2) de un usuario.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_UpdateUsuarioPassword]
(
    @Id INT,
    @Password NVARCHAR(250)
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Usuario SET Password = @Password WHERE Id = @Id;

    SELECT @@ROWCOUNT;
END
GO

PRINT '====================================================';
PRINT 'Migración de procedimientos SaaS #2 finalizada.';
PRINT '====================================================';
GO
