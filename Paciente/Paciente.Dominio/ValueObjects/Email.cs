using System.Text.RegularExpressions;

namespace DESI.ConsultorioMedico.Paciente.Dominio.ValueObjects;

/// <summary>
/// Value Object que encapsula un correo electrónico y valida su formato y longitud.
/// </summary>
public sealed class Email
{
    private static readonly Regex PatronEmail = new(@"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.Compiled);

    public string? Value { get; }

    public Email(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            Value = value;
            return;
        }

        if (value.Length > 50)
            throw new ArgumentException("El email no puede exceder 50 caracteres.", nameof(value));

        if (!PatronEmail.IsMatch(value))
            throw new ArgumentException("El email no tiene un formato válido.", nameof(value));

        Value = value;
    }

    public override string ToString() => Value ?? string.Empty;
}
