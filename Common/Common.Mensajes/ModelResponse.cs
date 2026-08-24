namespace DESI.ConsultorioMedico.Common.Mensajes;

/// <summary>
/// Sobre público estándar para todas las respuestas de la API.
/// </summary>
/// <typeparam name="T">Tipo del dato transportado en <see cref="Response"/>.</typeparam>
public class ModelResponse<T>
{
    /// <summary>
    /// Indica si la operación se completó con éxito.
    /// </summary>
    public bool IsSuccess { get; set; }

    /// <summary>
    /// Mensaje descriptivo del resultado (en español).
    /// </summary>
    public string Message { get; set; } = string.Empty;

    /// <summary>
    /// Dato transportado en la respuesta.
    /// </summary>
    public T? Response { get; set; }

    /// <summary>
    /// Construye una respuesta exitosa.
    /// </summary>
    public static ModelResponse<T> Success(T response, string? message = null)
        => new() { IsSuccess = true, Message = message ?? string.Empty, Response = response };

    /// <summary>
    /// Construye una respuesta de error.
    /// </summary>
    public static ModelResponse<T> Failure(string? message = null)
        => new() { IsSuccess = false, Message = message ?? string.Empty, Response = default };
}
