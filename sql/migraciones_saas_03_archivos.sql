/*
    =====================================================================
    MIGRACIÓN SaaS #3 — ARCHIVOS (rutas por consultorio)
    =====================================================================
    Proyecto : ConsultorioMedico (SaaS por consultorio)
    Cambio   : saas-registro-aislamiento
    Objetivo : Actualizar las rutas existentes de logos y anexos para que
               queden bajo /Attachment/1/... (consultorio 1) antes de mover
               los archivos físicos (paso manual/apply, NO SQL).

    Estrategia (idempotente):
      - DatosInstitucion.LogoRuta:
          /Attachment/institucion/<guid>  ->  /Attachment/1/institucion/<guid>
      - Anexo.Ruta:
          /Attachment/<...>               ->  /Attachment/1/<...>
          (solo si aún NO está bajo /Attachment/1/)

    Re-ejecutable sin error: tras el primer run ya no hay filas que cumplan
    el WHERE (quedan bajo /Attachment/1/), por lo que no duplica el prefijo.

    ⚠  IMPORTANTE:
      - SOLO para la BD del clon develop/main (db_9c7990_consultoriodesitst).
      - Ejecutar DESPUÉS de migraciones_saas_01_esquema.sql y
        migraciones_saas_02_procedimientos.sql.
      - El MOVIMIENTO FÍSICO de archivos (Attachment/institucion ->
        Attachment/1/institucion y carpetas numéricas huérfanas ->
        Attachment/1/<num>) se realiza aparte, NO en este script.
      - Clon Anexo vacío (0 filas); el logo de la institución existe.
    =====================================================================
*/

SET ANSI_NULLS ON;
GO
SET QUOTED_IDENTIFIER ON;
GO
SET NOCOUNT ON;
GO

/* =====================================================================
   3.1  Logo institucional -> /Attachment/1/institucion/
   ===================================================================== */
UPDATE dbo.DatosInstitucion
   SET LogoRuta = REPLACE(LogoRuta, '/Attachment/institucion/', '/Attachment/1/institucion/')
 WHERE LogoRuta LIKE '/Attachment/institucion/%';
GO

/* =====================================================================
   3.2  Anexos -> /Attachment/1/... (solo si aún no están bajo el tenant)
   ===================================================================== */
UPDATE dbo.Anexo
   SET Ruta = REPLACE(Ruta, '/Attachment/', '/Attachment/1/')
 WHERE Ruta LIKE '/Attachment/%'
   AND Ruta NOT LIKE '/Attachment/1/%';
GO

PRINT 'migraciones_saas_03_archivos.sql aplicado (rutas actualizadas a /Attachment/1/).';
GO
