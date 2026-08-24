namespace DESI.ConsultorioMedico.Paciente.Dominio.ValueObjects;

/// <summary>
/// Value Object que encapsula el sexo. Solo admite "Masculino" o "Femenino" (no es enum).
/// </summary>
public sealed class Sexo
{
    public const string Masculino = "Masculino";
    public const string Femenino = "Femenino";

    public string? Value { get; }

    public Sexo(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Value = value;
            return;
        }

        if (value.Length > 15)
            throw new ArgumentException("El sexo no puede exceder 15 caracteres.", nameof(value));

        if (value != Masculino && value != Femenino)
            throw new ArgumentException("El sexo debe ser 'Masculino' o 'Femenino'.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value ?? string.Empty;
}
