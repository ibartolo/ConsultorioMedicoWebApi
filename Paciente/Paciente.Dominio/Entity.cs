namespace DESI.ConsultorioMedico.Paciente.Dominio;

/// <summary>
/// Clase base para las entidades del dominio, con identificador de solo lectura.
/// </summary>
/// <typeparam name="T">Tipo del identificador.</typeparam>
public abstract class Entity<T>
{
    /// <summary>
    /// Identificador de la entidad (solo lectura; asignable internamente por la propia entidad).
    /// </summary>
    public T Id { get; protected set; } = default!;
}
