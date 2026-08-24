namespace DESI.ConsultorioMedico.Common.Mensajes;

/// <summary>
/// Resultado interno de una operación (no expuesto al cliente).
/// </summary>
public class OperationResult
{
    /// <summary>
    /// Indica si la operación se completó con éxito.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    public static OperationResult Success(string? message = null)
        => new() { IsSuccess = true, Message = message ?? string.Empty };

    public static OperationResult Failure(string? message = null)
        => new() { IsSuccess = false, Message = message ?? string.Empty };
}

/// <summary>
/// Resultado interno de una operación con dato de retorno (no expuesto al cliente).
/// </summary>
/// <typeparam name="T">Tipo del dato transportado en <see cref="Data"/>.</typeparam>
public class OperationResult<T>
{
    /// <summary>
    /// Indica si la operación se completó con éxito.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado.
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Dato transportado en el resultado.
    /// </summary>
    public T Data { get; set; } = default!;

    public static OperationResult<T> Success(T data, string? message = null)
        => new() { IsSuccess = true, Message = message ?? string.Empty, Data = data };

    public static OperationResult<T> Failure(string? message = null)
        => new() { IsSuccess = false, Message = message ?? string.Empty, Data = default! };
}
