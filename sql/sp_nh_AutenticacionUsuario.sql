/*
    =====================================================================
    DEPRECADO - NO EJECUTAR
    =====================================================================
    Este archivo definía la versión LEGACY de sp_nh_AutenticacionUsuario:
        - Firma: @UserName + @Password (texto plano).
        - Comparaba la contraseña en SQL.

    QUEDÓ REEMPLAZADO por la redefinición del SP en:
        sql/migraciones_saas_02_procedimientos.sql

    La nueva firma es:
        - Solo @UserName (sin @Password).
        - Devuelve Password + Estatus para verificar en C# (PBKDF2).
        - No compara la contraseña en SQL.

    Este archivo NO crea ni altera el SP: solo imprime este aviso.
    NO EJECUTAR. Usar en su lugar migraciones_saas_02_procedimientos.sql.
    =====================================================================
*/

PRINT 'DEPRECADO: sp_nh_AutenticacionUsuario ahora se define en sql/migraciones_saas_02_procedimientos.sql (firma @UserName, PBKDF2 en C#). NO ejecute este archivo legacy.';
GO
