using System.Data;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace DESI.ConsultorioMedico.Common.Data;

/// <summary>
/// Clase base para repositorios ADO.NET (sin ORM). Expone métodos para ejecutar
/// procedimientos almacenados sobre una conexión creada por operación.
/// </summary>
public abstract class BaseRepository
{
    private readonly IConfiguration _configuration;

    /// <summary>
    /// Inicializa el repositorio con la configuración de la aplicación.
    /// </summary>
    protected BaseRepository(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    /// <summary>
    /// Tiempo de espera (en segundos) de los comandos. Configurable.
    /// </summary>
    protected int CommandTimeoutSeconds { get; set; } = 30;

    /// <summary>
    /// Obtiene una nueva conexión a la base de datos (una por operación).
    /// </summary>
    protected SqlConnection GetConnection() => ConnectionHelper.CreateConnection(_configuration);

    /// <summary>
    /// Ejecuta un procedimiento almacenado y mapea cada fila a un objeto.
    /// </summary>
    protected async Task<List<T>> ExecuteReaderAsync<T>(
        string storedProcedure,
        CommandType commandType,
        Func<SqlDataReader, T> mapper,
        params SqlParameter[] parameters)
    {
        var result = new List<T>();

        await using var connection = GetConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(storedProcedure, connection)
        {
            CommandType = commandType,
            CommandTimeout = CommandTimeoutSeconds
        };

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        await using var reader = await command.ExecuteReaderAsync();

        while (await reader.ReadAsync())
        {
            result.Add(mapper(reader));
        }

        return result;
    }

    /// <summary>
    /// Ejecuta un procedimiento almacenado y lee el valor de retorno (RETURN) como identificador.
    /// </summary>
    protected async Task<int> ExecuteNonQueryAsync(string storedProcedure, params SqlParameter[] parameters)
    {
        await using var connection = GetConnection();
        await connection.OpenAsync();

        await using var command = new SqlCommand(storedProcedure, connection)
        {
            CommandType = CommandType.StoredProcedure,
            CommandTimeout = CommandTimeoutSeconds
        };

        if (parameters.Length > 0)
        {
            command.Parameters.AddRange(parameters);
        }

        var returnParameter = new SqlParameter("@ReturnValue", SqlDbType.Int)
        {
            Direction = ParameterDirection.ReturnValue
        };
        command.Parameters.Add(returnParameter);

        await command.ExecuteNonQueryAsync();

        return returnParameter.Value is int value ? value : 0;
    }
}
