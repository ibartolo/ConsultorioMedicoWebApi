/*
    =====================================================================
    MIGRACIÓN SaaS #5 — LICENCIAS Y PAGOS
    =====================================================================
    Proyecto : ConsultorioMedico (SaaS por consultorio)
    Cambio   : licencias-pagos
    Objetivo : 1) Añadir columnas de licencia a DatosInstitucion.
               2) Backfill de licencia anual para los consultorios 1 y 2.
               3) Crear la tabla ConsultorioPago (análoga a EmpresaPago,
                  sin RequiereFactura).
               4) Backfill de pagos Confirmados para los consultorios 1 y 2.
               5) Crear los SPs de licencia/pagos.
               6) Redefinir sp_nh_AutenticacionUsuario (LEFT JOIN + Email +
                  campos de licencia).

    Estrategia (idempotente):
      - Guards IF COL_LENGTH / IF OBJECT_ID / sys.indexes / sys.foreign_keys.
      - Sin DROP de tablas ni SPs. Solo CREATE OR ALTER para los SPs.
      - Backfill idempotente: licencia por FechaVigenciaFin IS NULL; pagos por
        NombreArchivo = 'Licencia anual piloto'.

    ⚠  IMPORTANTE:
      - SOLO para la BD del clon develop/main (db_9c7990_consultoriodesitst).
      - NO ejecutar sobre DB_9C7990_Consultorio ni ramas -dermatologia.
      - Ejecutar DESPUÉS de migraciones_saas_01_esquema.sql,
        migraciones_saas_02_procedimientos.sql, migraciones_saas_03_archivos.sql
        y migraciones_saas_04_password_reset.sql.
      - Re-ejecutable sin error. Sin DROP de tablas ni SPs.

    ⚠  QUOTED_IDENTIFIER ON (obligatorio): DatosInstitucion tiene índices
       filtrados; cualquier UPDATE dentro de un SP creado con QUOTED_IDENTIFIER
       OFF falla. Por eso este batch abre con SET QUOTED_IDENTIFIER ON y todos
       los SPs se crean/alteran dentro de este batch.
    =====================================================================
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

/* =====================================================================
   5.1  Columnas de licencia en DatosInstitucion
   ===================================================================== */
IF COL_LENGTH('dbo.DatosInstitucion', 'Estatus') IS NULL
BEGIN
    ALTER TABLE dbo.DatosInstitucion ADD Estatus BIT NOT NULL CONSTRAINT DF_DatosInstitucion_Estatus DEFAULT 1;
    PRINT 'dbo.DatosInstitucion: columna Estatus agregada.';
END
ELSE
    PRINT 'dbo.DatosInstitucion: columna Estatus ya existe.';
GO

IF COL_LENGTH('dbo.DatosInstitucion', 'FechaVigenciaInicio') IS NULL
BEGIN
    ALTER TABLE dbo.DatosInstitucion ADD FechaVigenciaInicio DATETIME NULL;
    PRINT 'dbo.DatosInstitucion: columna FechaVigenciaInicio agregada.';
END
ELSE
    PRINT 'dbo.DatosInstitucion: columna FechaVigenciaInicio ya existe.';
GO

IF COL_LENGTH('dbo.DatosInstitucion', 'FechaVigenciaFin') IS NULL
BEGIN
    ALTER TABLE dbo.DatosInstitucion ADD FechaVigenciaFin DATETIME NULL;
    PRINT 'dbo.DatosInstitucion: columna FechaVigenciaFin agregada.';
END
ELSE
    PRINT 'dbo.DatosInstitucion: columna FechaVigenciaFin ya existe.';
GO

IF COL_LENGTH('dbo.DatosInstitucion', 'EsPeriodoPrueba') IS NULL
BEGIN
    ALTER TABLE dbo.DatosInstitucion ADD EsPeriodoPrueba BIT NOT NULL CONSTRAINT DF_DatosInstitucion_EsPeriodoPrueba DEFAULT 0;
    PRINT 'dbo.DatosInstitucion: columna EsPeriodoPrueba agregada.';
END
ELSE
    PRINT 'dbo.DatosInstitucion: columna EsPeriodoPrueba ya existe.';
GO

IF COL_LENGTH('dbo.DatosInstitucion', 'Plan') IS NULL
BEGIN
    ALTER TABLE dbo.DatosInstitucion ADD [Plan] NVARCHAR(20) NOT NULL CONSTRAINT DF_DatosInstitucion_Plan DEFAULT 'Basico';
    PRINT 'dbo.DatosInstitucion: columna Plan agregada.';
END
ELSE
    PRINT 'dbo.DatosInstitucion: columna Plan ya existe.';
GO

/* =====================================================================
   5.2  Backfill de licencia anual para los consultorios 1 y 2
        Idempotente: solo filas que aún no tienen vigencia.
   ===================================================================== */
UPDATE dbo.DatosInstitucion
   SET FechaVigenciaInicio = GETDATE(),
       FechaVigenciaFin = DATEADD(YEAR, 1, GETDATE()),
       EsPeriodoPrueba = 0,
       [Plan] = 'Basico',
       Estatus = 1
 WHERE Id IN (1, 2)
   AND FechaVigenciaFin IS NULL;
GO

/* =====================================================================
   5.3  Tabla ConsultorioPago
   ===================================================================== */
IF OBJECT_ID('dbo.ConsultorioPago', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ConsultorioPago
    (
        Id                INT IDENTITY(1,1) NOT NULL,
        ConsultorioID     INT NOT NULL,
        NombreArchivo     NVARCHAR(255) NULL,
        ArchivoRuta       NVARCHAR(500) NULL,
        Monto             DECIMAL(10,2) NOT NULL,
        EstatusPagoId     TINYINT NOT NULL CONSTRAINT DF_ConsultorioPago_EstatusPagoId DEFAULT 1,
        FechaCarga        DATETIME NOT NULL,
        SubidoPor         NVARCHAR(25) NOT NULL,
        FechaConfirmacion DATETIME NULL,
        ConfirmadoPor     NVARCHAR(25) NULL,
        Notas             NVARCHAR(500) NULL,
        Estatus           BIT NOT NULL CONSTRAINT DF_ConsultorioPago_Estatus DEFAULT 1,
        CONSTRAINT PK_ConsultorioPago PRIMARY KEY (Id)
    );
    PRINT 'dbo.ConsultorioPago creada.';
END
ELSE
    PRINT 'dbo.ConsultorioPago ya existe.';
GO

-- FK por si la tabla ya existía sin la FK (robustez / idempotencia).
IF NOT EXISTS (
    SELECT 1
    FROM sys.foreign_keys fk
    JOIN sys.foreign_key_columns fkc ON fk.object_id = fkc.constraint_object_id
    JOIN sys.columns pc ON pc.object_id = fkc.parent_object_id AND pc.column_id = fkc.parent_column_id
    WHERE fk.parent_object_id = OBJECT_ID('dbo.ConsultorioPago')
      AND pc.name = 'ConsultorioID'
      AND fk.referenced_object_id = OBJECT_ID('dbo.DatosInstitucion')
)
BEGIN
    ALTER TABLE dbo.ConsultorioPago ADD CONSTRAINT FK_ConsultorioPago_DatosInstitucion FOREIGN KEY (ConsultorioID) REFERENCES dbo.DatosInstitucion (Id);
    PRINT 'FK_ConsultorioPago_DatosInstitucion creada.';
END
ELSE
    PRINT 'FK ConsultorioPago.ConsultorioID -> DatosInstitucion(Id) ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes WHERE name = 'IX_ConsultorioPago_ConsultorioID' AND object_id = OBJECT_ID('dbo.ConsultorioPago'))
BEGIN
    CREATE NONCLUSTERED INDEX IX_ConsultorioPago_ConsultorioID ON dbo.ConsultorioPago (ConsultorioID);
    PRINT 'IX_ConsultorioPago_ConsultorioID creado.';
END
ELSE
    PRINT 'IX_ConsultorioPago_ConsultorioID ya existe.';
GO

/* =====================================================================
   5.4  Backfill de pagos Confirmados para los consultorios 1 y 2
        Idempotente: solo si no existe el pago piloto del consultorio.
   ===================================================================== */
IF NOT EXISTS (SELECT 1 FROM dbo.ConsultorioPago WHERE ConsultorioID = 1 AND NombreArchivo = 'Licencia anual piloto')
BEGIN
    INSERT INTO dbo.ConsultorioPago
        (ConsultorioID, NombreArchivo, ArchivoRuta, Monto, EstatusPagoId, FechaCarga, SubidoPor, FechaConfirmacion, ConfirmadoPor, Notas, Estatus)
    VALUES
        (1, 'Licencia anual piloto', NULL, 1200.00, 2, GETDATE(), 'system', GETDATE(), 'system', 'Licencia anual piloto', 1);
    PRINT 'Pago piloto confirmado para consultorio 1.';
END
ELSE
    PRINT 'Pago piloto del consultorio 1 ya existe.';
GO

IF NOT EXISTS (SELECT 1 FROM dbo.ConsultorioPago WHERE ConsultorioID = 2 AND NombreArchivo = 'Licencia anual piloto')
BEGIN
    INSERT INTO dbo.ConsultorioPago
        (ConsultorioID, NombreArchivo, ArchivoRuta, Monto, EstatusPagoId, FechaCarga, SubidoPor, FechaConfirmacion, ConfirmadoPor, Notas, Estatus)
    VALUES
        (2, 'Licencia anual piloto', NULL, 1200.00, 2, GETDATE(), 'system', GETDATE(), 'system', 'Licencia anual piloto', 1);
    PRINT 'Pago piloto confirmado para consultorio 2.';
END
ELSE
    PRINT 'Pago piloto del consultorio 2 ya existe.';
GO

/* =====================================================================
   5.5  SPs DE LICENCIA Y PAGOS
   ===================================================================== */

-- Licencia del tenant (una fila).
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetLicencia]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id AS ConsultorioID,
           Nombre,
           Estatus,
           FechaVigenciaInicio,
           FechaVigenciaFin,
           EsPeriodoPrueba,
           [Plan]
    FROM dbo.DatosInstitucion
    WHERE Id = @ConsultorioID;
END
GO

-- Historial de pagos del tenant (soft-delete activo).
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetPagosLicencia]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, ConsultorioID, NombreArchivo, ArchivoRuta, Monto, EstatusPagoId,
           FechaCarga, SubidoPor, FechaConfirmacion, ConfirmadoPor, Notas, Estatus
    FROM dbo.ConsultorioPago
    WHERE ConsultorioID = @ConsultorioID AND Estatus = 1
    ORDER BY FechaCarga DESC, Id DESC;
END
GO

-- Alta comprobante: inserta Pendiente + extensión provisional al fin del día
-- SI venció (no reduce vigencia futura). Devuelve el Id del pago.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GuardarPagoLicencia]
(
    @ConsultorioID INT,
    @NombreArchivo NVARCHAR(255),
    @ArchivoRuta   NVARCHAR(500),
    @Monto         DECIMAL(10,2),
    @Usuario       NVARCHAR(25)
)
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.ConsultorioPago
        (ConsultorioID, NombreArchivo, ArchivoRuta, Monto, EstatusPagoId, FechaCarga, SubidoPor, Estatus)
    VALUES
        (@ConsultorioID, @NombreArchivo, @ArchivoRuta, @Monto, 1, GETDATE(), @Usuario, 1);

    DECLARE @PagoId INT = SCOPE_IDENTITY();

    -- Extensión provisional al fin del día solo si la licencia venció (o no tiene vigencia).
    UPDATE dbo.DatosInstitucion
       SET FechaVigenciaFin = DATEADD(SECOND, -1, DATEADD(DAY, 1, CAST(GETDATE() AS DATE)))
     WHERE Id = @ConsultorioID
       AND (FechaVigenciaFin IS NULL OR FechaVigenciaFin < GETDATE());

    SELECT @PagoId;
END
GO

-- Confirmación MANUAL (no expuesta en WebApi). Valida tenant del pago;
-- +1 año desde max(hoy, vigencia actual).
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_ConfirmarPagoLicencia]
(
    @PagoId        INT,
    @ConsultorioID INT,
    @Usuario       NVARCHAR(25),
    @Notas         NVARCHAR(500) = NULL
)
AS
BEGIN
    SET NOCOUNT ON;

    IF NOT EXISTS (SELECT 1 FROM dbo.ConsultorioPago WHERE Id = @PagoId AND ConsultorioID = @ConsultorioID AND Estatus = 1)
    BEGIN
        SELECT -1;
        RETURN;
    END

    UPDATE dbo.ConsultorioPago
       SET EstatusPagoId = 2,
           FechaConfirmacion = GETDATE(),
           ConfirmadoPor = @Usuario,
           Notas = ISNULL(@Notas, Notas)
     WHERE Id = @PagoId;

    UPDATE dbo.DatosInstitucion
       SET FechaVigenciaFin = DATEADD(YEAR, 1, CASE WHEN FechaVigenciaFin > GETDATE() THEN FechaVigenciaFin ELSE GETDATE() END),
           EsPeriodoPrueba = 0
     WHERE Id = @ConsultorioID;

    SELECT 1;
END
GO

-- Trial 60 días (solo en Registro). Sin @Usuario: DatosInstitucion no tiene auditoría.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_SetLicenciaInicial]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.DatosInstitucion
       SET Estatus = 1,
           FechaVigenciaInicio = GETDATE(),
           FechaVigenciaFin = DATEADD(DAY, 60, GETDATE()),
           EsPeriodoPrueba = 1,
           [Plan] = 'Basico'
     WHERE Id = @ConsultorioID;

    SELECT @@ROWCOUNT;
END
GO

-- Primer usuario (menor Id, activo) del tenant para el correo.
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetPrimerUsuarioConsultorio]
(
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1 Id, UserName, Email
    FROM dbo.Usuario
    WHERE CompaniaID = @ConsultorioID AND Estatus = 1
    ORDER BY Id ASC;
END
GO

-- Pago por Id validando tenant (descarga/autorización).
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_GetPagoLicenciaById]
(
    @PagoId        INT,
    @ConsultorioID INT
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT Id, ConsultorioID, NombreArchivo, ArchivoRuta, Monto, EstatusPagoId,
           FechaCarga, SubidoPor, FechaConfirmacion, ConfirmadoPor, Notas, Estatus
    FROM dbo.ConsultorioPago
    WHERE Id = @PagoId AND ConsultorioID = @ConsultorioID AND Estatus = 1;
END
GO

/* =====================================================================
   5.6  Redefinir sp_nh_AutenticacionUsuario
        Preserva el contrato auth (solo @UserName) y agrega Email + campos
        de licencia (LEFT JOIN DatosInstitucion; di.Estatus AS LicenciaEstatus
        para no chocar con Usuario.Estatus).
   ===================================================================== */
CREATE OR ALTER PROCEDURE [dbo].[sp_nh_AutenticacionUsuario]
(
    @UserName NVARCHAR(50)
)
AS
BEGIN
    SET NOCOUNT ON;

    SELECT TOP 1
        u.Id,
        u.UserName,
        u.Password,
        u.Email,
        u.Estatus,
        u.CompaniaID,
        di.FechaVigenciaFin,
        di.EsPeriodoPrueba,
        di.[Plan],
        di.Estatus AS LicenciaEstatus
    FROM dbo.Usuario u
        LEFT JOIN dbo.DatosInstitucion di ON di.Id = u.CompaniaID
    WHERE u.UserName = @UserName
    ORDER BY u.Id ASC;
END
GO

PRINT '====================================================';
PRINT 'Migración de licencias y pagos SaaS #5 finalizada.';
PRINT '====================================================';
GO
