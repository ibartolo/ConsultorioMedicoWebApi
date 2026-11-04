/*
    =====================================================================
    RESET DE DATOS - CONSULTORIO MÉDICO
    =====================================================================
    ADVERTENCIA: ESTE SCRIPT BORRA DATOS. HAZ UN RESPALDO ANTES.

    Borra:
      - Pacientes, citas, consultas y anexos.
      - Paquetes del paciente, citas de paquete y pagos de paquete.
      - Catálogos de tratamientos, paquetes, tipos de consulta y
        tratamientos de paquete.

    Reinicia los Id (IDENTITY) de las tablas vaciadas.

    CONSERVA:
      - dbo.Usuario  -> solicitado.
      - dbo.DatosInstitucion -> porque Usuario.CompaniaID tiene FK a esta tabla.
      - dbo.sysdiagrams -> tabla de sistema, no es información de la app.

    OPCIONAL:
      - dbo.DatosFiscales: por defecto se conserva. Si quieres borrarlo,
        descomenta la línea de DELETE y su DBCC CHECKIDENT.

    NOTA:
      Este script NO borra los archivos físicos de la carpeta Attachment/
      de la WebApi. Si quieres limpiarlos, hazlo por separado.
    =====================================================================
*/

SET NOCOUNT ON;
SET XACT_ABORT ON;

BEGIN TRY
    BEGIN TRANSACTION;

    /* ============================================================= */
    /* 1. Datos transaccionales / clínicos                           */
    /* ============================================================= */
    DELETE FROM dbo.Anexo;
    DELETE FROM dbo.CitaPaquete;
    DELETE FROM dbo.PagoPaquete;
    DELETE FROM dbo.Consulta;
    DELETE FROM dbo.Cita;
    DELETE FROM dbo.PacientePaquete;
    DELETE FROM dbo.Paciente;

    /* ============================================================= */
    /* 2. Catálogos                                                  */
    /* ============================================================= */
    DELETE FROM dbo.TratamientoPaquete;
    DELETE FROM dbo.Paquete;
    DELETE FROM dbo.Tratamiento;
    DELETE FROM dbo.TipoConsulta;

    /* ============================================================= */
    /* 3. Opcional: datos fiscales                                   */
    /*    Descomenta estas dos líneas si también quieres borrarlos.  */
    /* ============================================================= */
    -- DELETE FROM dbo.DatosFiscales;

    COMMIT TRANSACTION;
END TRY
BEGIN CATCH
    IF @@TRANCOUNT > 0
        ROLLBACK TRANSACTION;

    DECLARE @ErrorMessage NVARCHAR(4000) = ERROR_MESSAGE();
    RAISERROR(@ErrorMessage, 16, 1);
END CATCH;

/* ============================================================= */
/* 4. Reiniciar identidades de las tablas vaciadas               */
/* ============================================================= */
DBCC CHECKIDENT ('dbo.Anexo', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.CitaPaquete', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.PagoPaquete', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Consulta', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Cita', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.PacientePaquete', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Paciente', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.TratamientoPaquete', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Paquete', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.Tratamiento', RESEED, 0) WITH NO_INFOMSGS;
DBCC CHECKIDENT ('dbo.TipoConsulta', RESEED, 0) WITH NO_INFOMSGS;

/* Si borraste DatosFiscales, descomenta: */
-- DBCC CHECKIDENT ('dbo.DatosFiscales', RESEED, 0) WITH NO_INFOMSGS;

/* ============================================================= */
/* 5. Verificación                                               */
/* ============================================================= */
SELECT 'Anexo' AS Tabla, COUNT(*) AS Filas FROM dbo.Anexo
UNION ALL SELECT 'CitaPaquete', COUNT(*) FROM dbo.CitaPaquete
UNION ALL SELECT 'PagoPaquete', COUNT(*) FROM dbo.PagoPaquete
UNION ALL SELECT 'Consulta', COUNT(*) FROM dbo.Consulta
UNION ALL SELECT 'Cita', COUNT(*) FROM dbo.Cita
UNION ALL SELECT 'PacientePaquete', COUNT(*) FROM dbo.PacientePaquete
UNION ALL SELECT 'Paciente', COUNT(*) FROM dbo.Paciente
UNION ALL SELECT 'TratamientoPaquete', COUNT(*) FROM dbo.TratamientoPaquete
UNION ALL SELECT 'Paquete', COUNT(*) FROM dbo.Paquete
UNION ALL SELECT 'Tratamiento', COUNT(*) FROM dbo.Tratamiento
UNION ALL SELECT 'TipoConsulta', COUNT(*) FROM dbo.TipoConsulta
UNION ALL SELECT 'DatosFiscales', COUNT(*) FROM dbo.DatosFiscales
UNION ALL SELECT 'Usuario (intacto)', COUNT(*) FROM dbo.Usuario
UNION ALL SELECT 'DatosInstitucion (intacto)', COUNT(*) FROM dbo.DatosInstitucion;
