using Microsoft.Data.SqlClient;

namespace DESI.ConsultorioMedico.Common.Data;

/// <summary>
/// Extensiones de lectura segura sobre <see cref="SqlDataReader"/> (tolerantes a <see cref="DBNull"/>).
/// </summary>
public static class SqlDataReaderExtensions
{
    /// <summary>
    /// Lee un valor de texto; retorna <c>null</c> si la columna es <see cref="DBNull"/>.
    /// </summary>
    public static string? GetStringSafe(this SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetString(ordinal);
    }

    /// <summary>
    /// Lee un entero; retorna <c>0</c> si la columna es <see cref="DBNull"/>.
    /// </summary>
    public static int GetInt32Safe(this SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? 0 : reader.GetInt32(ordinal);
    }

    /// <summary>
    /// Lee un entero anulable; retorna <c>null</c> si la columna es <see cref="DBNull"/>.
    /// </summary>
    public static int? GetInt32NullableSafe(this SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetInt32(ordinal);
    }

    /// <summary>
    /// Lee una fecha anulable; retorna <c>null</c> si la columna es <see cref="DBNull"/>.
    /// </summary>
    public static DateTime? GetDateTimeSafe(this SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return reader.IsDBNull(ordinal) ? null : reader.GetDateTime(ordinal);
    }

    /// <summary>
    /// Lee un booleano; retorna <c>false</c> si la columna es <see cref="DBNull"/>.
    /// </summary>
    public static bool GetBoolSafe(this SqlDataReader reader, string columnName)
    {
        var ordinal = reader.GetOrdinal(columnName);
        return !reader.IsDBNull(ordinal) && reader.GetBoolean(ordinal);
    }
}
