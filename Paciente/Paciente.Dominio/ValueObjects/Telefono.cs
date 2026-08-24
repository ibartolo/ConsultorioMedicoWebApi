namespace DESI.ConsultorioMedico.Paciente.Dominio.ValueObjects;

/// <summary>
/// Value Object que encapsula un teléfono y valida su longitud máxima.
/// </summary>
public sealed class Telefono
{
    public string? Value { get; }

    public Telefono(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Value = value;
            return;
        }

        if (value.Length > 100)
            throw new ArgumentException("El teléfono no puede exceder 100 caracteres.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value ?? string.Empty;
}
