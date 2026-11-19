/*
    =====================================================================
    MIGRACIÓN SaaS #4 — RECUPERACIÓN DE CONTRASEÑA (Email + tokens)
    =====================================================================
    Proyecto : ConsultorioMedico (SaaS por consultorio)
    Cambio   : password-recovery
    Objetivo : 1) Añadir Usuario.Email (nullable) con backfill desde
                  DatosInstitucion.Email y un índice único filtrado.
               2) Crear la tabla RecuperacionPasswordToken.
               3) Actualizar los SPs de Usuario para leer/guardar Email.
               4) Crear los SPs del flujo de recuperación de contraseña.

    Estrategia (idempotente):
      - Guards IF COL_LENGTH / IF OBJECT_ID / sys.indexes / sys.foreign_keys.
      - Sin DROP de tablas ni SPs. Solo CREATE OR ALTER para los SPs.
      - Backfill SOLO del menor Id por consultorio (CompaniaID) activo:
          admin  (Id=1, CompaniaID=1) -> dratania@gmail.com.mx
          BACI   (Id=5, CompaniaID=2) -> bartolocastro@gmail.com
          ivanbartolo queda NULL (no es el menor Id de su consultorio).

    ⚠  IMPORTANTE:
      - SOLO para la BD del clon develop/main (db_9c7990_consultoriodesitst).
      - NO ejecutar sobre DB_9C7990_Consultorio ni ramas -dermatologia.
      - Ejecutar DESPUÉS de migraciones_saas_01_esquema.sql y
        migraciones_saas_02_procedimientos.sql.
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
   4.1  Usuario.Email NVARCHAR(250) NULL
   ===================================================================== */
IF COL_LENGTH('dbo.Usuario', 'Email') IS NULL
BEGIN
    ALTER TABLE dbo.Usuario ADD Email NVARCHAR(250) NULL;
    PRINT 'dbo.Usuario: columna Email agregada.';
END
ELSE
    PRINT 'dbo.Usuario: columna Email ya existe.';
GO

/* =====================================================================
   4.2  Backfill Email del menor Id por consultorio (activo)
        Solo se rellena el usuario activo con el menor Id de cada
        CompaniaID y únicamente si DatosInstitucion.Email no es NULL.
        Idempotente: tras el primer run el menor Id ya tiene Email.
   ===================================================================== */
UPDATE u
   SET u.Email = di.Email
  FROM dbo.Usuario u
 INNER JOIN dbo.DatosInstitucion di ON di.Id = u.CompaniaID
 WHERE u.Email IS NULL
   AND u.Estatus = 1
   AND di.Email IS NOT NULL
   AND u.Id = (
           SELECT MIN(u2.Id)
             FROM dbo.Usuario u2
            WHERE u2.CompaniaID = u.CompaniaID
              AND u2.Estatus = 1
       );
GO

/* =====================================================================
   4.3  Índice único filtrado UX_Usuario_Email
   ===================================================================== */
IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_Usuario_Email' AND object_id = OBJECT_ID('dbo.Usuario'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_Usuario_Email ON dbo.Usuario (Email) WHERE Email IS NOT NULL;
    PRINT 'UX_Usuario_Email creado (filtrado).';
END
ELSE
    PRINT 'UX_Usuario_Email ya existe.';
GO

/* =====================================================================
   4.4  Tabla RecuperacionPasswordToken
   ===================================================================== */
IF OBJECT_ID('dbo.RecuperacionPasswordToken', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.RecuperacionPasswordToken
    (
        Id              INT IDENTITY(1,1) NOT NULL,
        UsuarioId       INT NOT NULL,
        Token           UNIQUEIDENTIFIER NOT NULL,
        FechaCreacion   DATETIME NOT NULL,
        FechaExpiracion DATETIME NOT NULL,
        Usado           BIT NOT NULL,
        FechaUso        DATETIME NULL,
        CONSTRAINT PK_RecuperacionPasswordToken PRIMARY KEY (Id),
        CONSTRAINT FK_RecuperacionPasswordToken_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario (Id)
    );
    PRINT 'dbo.RecuperacionPasswordToken creada.';
END
ELSE
    PRINT 'dbo.RecuperacionPasswordToken ya existe.';
GO

-- FK por si la tabla ya existía sin la FK (robustez / idempotencia).
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID('dbo.RecuperacionPasswordToken')
      AND pc.name = 'UsuarioId'
      AND fk.referenced_object_id = OBJECT_ID('dbo.Usuario')
)
BEGIN
    ALTER TABLE dbo.RecuperacionPasswordToken ADD CONSTRAINT FK_RecuperacionPasswordToken_Usuario FOREIGN KEY (UsuarioId) REFERENCES dbo.Usuario (Id);
    PRINT 'FK_RecuperacionPasswordToken_Usuario creada.';
END
ELSE
    PRINT 'FK RecuperacionPasswordToken.UsuarioId -> Usuario(Id) ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'UX_RecuperacionPasswordToken_Token' AND object_id = OBJECT_ID('dbo.RecuperacionPasswordToken'))
BEGIN
    CREATE UNIQUE NONCLUSTERED INDEX UX_RecuperacionPasswordToken_Token ON dbo.RecuperacionPasswordToken (Token);
    PRINT 'UX_RecuperacionPasswordToken_Token creado.';
END
ELSE
    PRINT 'UX_RecuperacionPasswordToken_Token ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_RecuperacionPasswordToken_UsuarioId' AND object_id = OBJECT_ID('dbo.RecuperacionPasswordToken'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_RecuperacionPasswordToken_UsuarioId ON dbo.RecuperacionPasswordToken (UsuarioId);
    PRINT 'IX_RecuperacionPasswordToken_UsuarioId creado.';
END
ELSE
    PRINT 'IX_RecuperacionPasswordToken_UsuarioId ya existe.';
GO

/* =====================================================================
   4.5  SPs DE USUARIO (se añade Email)
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetAllUsuarios]
(
    @CompaniaID INT
)
AS
BEGIN
    SET NOCOUNT ON;
    SELECT Id, UserName, Email, Estatus, CompaniaID
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
    SELECT Id, UserName, Email, Estatus, CompaniaID
    FROM dbo.Usuario
    WHERE Id = @Id AND CompaniaID = @CompaniaID;
END
GO

-- Añade @Email opcional. La contraseña se conserva con ISNULL(@Password, Password)
-- y el correo con ISNULL(@Email, Email) en edición. El filtro de tenant se mantiene.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SaveUsuario]
(
    @Id         INT,
    @UserName   NVARCHAR(50) = NULL,
    @Password   NVARCHAR(250) = NULL,
    @Email      NVARCHAR(250) = NULL,
    @Estatus    BIT,
    @CompaniaID INT = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF @Id = 0
    BEGIN
        INSERT INTO dbo.Usuario (UserName, Password, Email, Estatus, CompaniaID)
        VALUES (@UserName, @Password, @Email, @Estatus, @CompaniaID);

        SELECT SCOPE_IDENTITY();
    END
    ELSE
    BEGIN
        UPDATE dbo.Usuario
        SET UserName = @UserName,
            Password = ISNULL(@Password, Password),
            Email = ISNULL(@Email, Email),
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

-- Busca un usuario activo por Email (para el flujo de recuperación).
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetUsuarioByEmail]
(
    @Email NVARCHAR(250)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 Id, UserName, Email, Estatus, CompaniaID
    FROM dbo.Usuario
    WHERE Email = @Email
      AND Estatus = 1
    ORDER BY Id ASC;
END
GO

/* =====================================================================
   4.6  SPs DE RECUPERACIÓN DE CONTRASEÑA
   ===================================================================== */
-- Invalida los tokens no usados del usuario y crea uno nuevo. Devuelve el Id.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_CrearTokenRecuperacion]
(
    @UsuarioId INT,
    @Token     UNIQUEIDENTIFIER,
    @Minutos   INT = 10
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.RecuperacionPasswordToken
       SET Usado = 1,
           FechaUso = GETDATE()
     WHERE UsuarioId = @UsuarioId
       AND Usado = 0;

    -- La expiración se calcula con la hora de la BD, no con la hora del servidor web.
    INSERT INTO dbo.RecuperacionPasswordToken
        (UsuarioId, Token, FechaCreacion, FechaExpiracion, Usado, FechaUso)
    VALUES
        (@UsuarioId, @Token, GETDATE(), DATEADD(MINUTE, @Minutos, GETDATE()), 0, NULL);

    SELECT SCOPE_IDENTITY();
END
GO

-- Devuelve el token + datos del usuario asociado.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetTokenRecuperacion]
(
    @Token UNIQUEIDENTIFIER
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        t.Id AS TokenId,
        t.UsuarioId,
        u.UserName,
        u.Email,
        t.FechaCreacion,
        t.FechaExpiracion,
        t.Usado,
        t.FechaUso,
        CASE WHEN t.Usado = 0 AND t.FechaExpiracion > GETDATE()
             THEN CAST(1 AS BIT) ELSE CAST(0 AS BIT) END AS Vigente
    FROM dbo.RecuperacionPasswordToken t
        INNER JOIN dbo.Usuario u ON u.Id = t.UsuarioId
    WHERE t.Token = @Token
    ORDER BY t.Id DESC;
END
GO

-- Valida el token (no usado y no expirado) y actualiza la contraseña de forma
-- atómica. Devuelve 1 si se restableció, 0 en caso contrario.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_RestablecerPasswordConToken]
(
    @Token    UNIQUEIDENTIFIER,
    @Password NVARCHAR(250)
)
AS
BEGIN
    SET NOCOUNT ON;

    DECLARE @ids TABLE (UsuarioId INT);

    -- Marca el token como usado de forma atómica y captura el usuario.
    -- No abre/cierra transacción: funciona igual dentro o fuera de una transacción externa.
    UPDATE dbo.RecuperacionPasswordToken
       SET Usado = 1,
           FechaUso = GETDATE()
    OUTPUT inserted.UsuarioId INTO @ids
     WHERE Token = @Token
       AND Usado = 0
       AND FechaExpiracion > GETDATE();

    IF NOT EXISTS (SELECT 1 FROM @ids)
    BEGIN
        SELECT 0;
        RETURN;
    END

    UPDATE dbo.Usuario
       SET Password = @Password
     WHERE Id = (SELECT TOP 1 UsuarioId FROM @ids);

    SELECT 1;
END
GO

PRINT '====================================================';
PRINT 'Migración de recuperación de contraseña SaaS #4 finalizada.';
PRINT '====================================================';
GO
